using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Agent.Core;
using Agent.Core.Utils;
using Agent.RV.Uninstaller;

namespace Agent.RV.CustomApps
{
    public static class CustomAppsManager
    {

        public static RVsofResult AddAppDetailsToResults(RVsofResult results)
        {
            results.Data.Name = String.Empty;
            results.Data.Description = String.Empty;
            results.Data.Kb = String.Empty;
            results.Data.ReleaseDate = 0.0;
            results.Data.VendorSeverity = String.Empty;
            results.Data.VendorName = String.Empty;
            results.Data.VendorId = String.Empty;
            results.Data.Version = String.Empty;
            results.Data.SupportUrl = String.Empty;
            return results;
        }

        /// <summary>
        /// Gets an Operation send from the server for installing a CustomeApp.
        /// Determines what type of install it is(exe, msi, msp).
        /// And sends it to the appropriate method to prepair it for install. 
        /// </summary>
        /// <param name="customApp">Json Operatoin from the server.</param>
        /// <returns>Returns updated Operation with the results.</returns>
        public static Operations.SavedOpData InstallCustomAppsOperation(Operations.SavedOpData customApp)
        {
                try
                {
                    var appDirectory = Path.Combine(Settings.CustomAppDirectory, customApp.filedata_app_id);
                    var appFiles = Directory.GetFiles(appDirectory);
                    var file = appFiles[0];

                    var extension = Path.GetExtension(file);

                    InstallResult installResult;
                    switch (extension.ToLower())
                    {
                        case Extension.Exe:
                            Logger.Log("Installing: {0}", LogLevel.Info, customApp.filedata_app_name);
                            installResult = ExeInstall(file, customApp.filedata_app_clioptions);
                            break;

                        case Extension.Msi:
                            Logger.Log("Installing: {0}", LogLevel.Info, customApp.filedata_app_name);
                            installResult = MsiInstall(file, customApp.filedata_app_clioptions);
                            break;

                        case Extension.Msp:
                            Logger.Log("Installing: {0}", LogLevel.Info, customApp.filedata_app_name);
                            installResult = MspInstall(file, customApp.filedata_app_clioptions);
                            break;

                        default:
                            Logger.Log("{0} is not a supported file format.", LogLevel.Warning, extension);
                            throw new Exception(String.Format("{0} is not a supported file format.", extension));
                     }


                        if (!installResult.Success)
                        {
                            //Custom App Failed to Install
                            customApp.success         = false.ToString().ToLower();
                            customApp.error           = String.Format("Failed to install {0}. {1}. Exit code: {2}.", customApp.filedata_app_name, installResult.ExitCodeMessage, installResult.ExitCode);
                            customApp.reboot_required = installResult.Restart.ToString().ToLower();

                            Logger.Log("Custom App Failed to Install: {0}", LogLevel.Warning, customApp.filedata_app_name + ", Error: " + customApp.error);
                            return customApp;
                        }

                        //Custom App Installed OK
                        customApp.success         = true.ToString().ToLower();
                        customApp.error           = string.Empty;
                        customApp.reboot_required = installResult.Restart.ToString().ToLower();

                        Logger.Log("Custom App Installed Successfully: {0}", LogLevel.Info, customApp.filedata_app_name);
                        return customApp;
                }
                catch (Exception e)
                {
                    //Error when attempting to install custom app.
                    customApp.success = false.ToString().ToLower();
                    customApp.error = String.Format("Failed to install Custom App, Exception error: {0}", e.Message);

                    Logger.Log("Custom App Failed to Install: {0}", LogLevel.Info, customApp.filedata_app_name + ", Error: " + customApp.error);
                    return customApp;
                }
        }

        /// <summary>
        /// Prepairs the app to be installed from Windows exe.
        /// Requires exe path and cli(command line option).
        /// </summary>
        /// <param name="exePath">Exe path for the locatoin of the installer.</param>
        /// <param name="cliOptions">Command line option.</param>
        /// <returns>InstallResults - populated.</returns>
        private static InstallResult ExeInstall(string exePath, string cliOptions)
        {
            var processInfo = new ProcessStartInfo();
            processInfo.FileName = exePath;
            processInfo.Arguments = cliOptions;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            processInfo.RedirectStandardOutput = true;

            var result = RunProcess(processInfo);

            return result;
        }

        /// <summary>
        /// Prepairs the app to be installed from Windows MSI installer.
        /// Requires MSI path and cli(command line option).
        /// </summary>
        /// <param name="msiPath">MSI path for the location of the installer.</param>
        /// <param name="cliOptions">Command line option.</param>
        /// <returns>InstallResults - populated.</returns>
        private static InstallResult MsiInstall(string msiPath, string cliOptions)
        {
            var processInfo = new ProcessStartInfo();
            processInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "msiexec.exe");
            processInfo.Arguments = String.Format(@"/i {0} /quiet /norestart {1}", msiPath, cliOptions);
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            processInfo.RedirectStandardOutput = true;

            var result = RunProcess(processInfo);

            return result;
        }

        /// <summary>
        /// Prepairs the app to be installed from Windows MSP.
        /// Requires MSP path and cli(command line option).
        /// </summary>
        /// <param name="mspPath">String path for the locatoin of the installer.</param>
        /// <param name="cliOptions">Command Line Option.</param>
        /// <returns>InstallResults - populated.</returns>
        private static InstallResult MspInstall(string mspPath, string cliOptions)
        {
            var processInfo = new ProcessStartInfo();
            processInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "msiexec.exe");
            processInfo.Arguments = String.Format(@"/p {0} /quiet {1}", mspPath, cliOptions);
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            processInfo.RedirectStandardOutput = true;

            var result = RunProcess(processInfo);

            return result;
        }

        /// <summary>
        /// Runs the Install, for the specified options.
        /// Required info as per Windows command prompt execution methods.
        /// </summary>
        /// <param name="processInfo">Parameters to with the required info to run the install.</param>
        /// <returns></returns>
        private static InstallResult RunProcess(ProcessStartInfo processInfo)
        {
            var result = new InstallResult();

            // The following WindowsUninstaller.WindowsExitCode used below might be Windows specific. 
            // Third party apps might not use same code. Good luck!
            try
            {
                using (var process = Process.Start(processInfo))
                {
                    process.WaitForExit();

                    result.ExitCode = process.ExitCode;
                    result.ExitCodeMessage = new Win32Exception(process.ExitCode).Message;

                    switch (result.ExitCode)
                    {
                        case (int)WindowsUninstaller.WindowsExitCode.Restart:
                        case (int)WindowsUninstaller.WindowsExitCode.Reboot:
                            result.Restart = true;
                            result.Success = true;
                            break;
                        case (int)WindowsUninstaller.WindowsExitCode.Sucessful:
                            result.Success = true;
                            break;
                        default:
                            result.Success = false;
                            break;
                    }

                    var output = process.StandardOutput;
                    result.Output = output.ReadToEnd();
                }
                return result;
            }
            catch (Exception e)
            {
                Logger.Log("Custom App Installer failed to install: {0}.", LogLevel.Error, processInfo.FileName);
                Logger.LogException(e);

                result.ExitCode          = -1;
                result.ExitCodeMessage   = String.Format("Custom App Installer failed to install {0}.", processInfo.FileName);
                result.Output            = String.Empty;
                result.Restart           = false;
                result.Success           = false;
                
                return result;
            }
            
        }
    }

    public struct InstallResult
    {
        public int ExitCode;
        public string ExitCodeMessage;
        public bool Restart;
        public bool Success;
        public string Output;
    }

    public static class Extension
    {
        public const string Exe = ".exe";
        public const string Msi = ".msi";
        public const string Msp = ".msp";
    }
}
