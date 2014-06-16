using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace PatchPayload
{
    public static class Installer
    {
        //###################################################################################################################################################################
        /*
         * VERY IMPORTANT
         * THIS INFORMATION MUST BE FILLED OUT, IT DETERMINES HOW THE UPDATER WILL BEHAVE.
        */
        //###################################################################################################################################################################
        public static string OperationType = null;//Data.OperationValue.InstallAgentUpdate;
        public static string InstallerName = Data.OperationValue.UpdateInstallerName;
        public static string versionNumber = "";  //AGENT VERSION TO UPGRADE TO
        public const string versionNumberPatcher = "2.0";  //THE UPDATER VERSION NUMBER (THIS PROGRAM)
        

        static void Main(string[] args)
        {
           
            var vfiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);

            foreach (string file in vfiles)
            {
                if (file.EndsWith("version.txt"))
                {
                    using (var vfile = System.IO.File.OpenText(file))
                    {
                        string line = vfile.ReadLine();
                        versionNumber = line;
                    }
                }
            }

            if (!String.IsNullOrEmpty(versionNumber))//&& OperationType != null)
            {
                if (!Directory.Exists(Data.AgentUpdateDirectory))
                    Directory.CreateDirectory(Data.AgentUpdateDirectory);
                //else
                //{
                //    try
                //    {
                //        Directory.Delete(Data.AgentUpdateDirectory, true);
                //        Directory.CreateDirectory(Data.AgentUpdateDirectory);
                //    }
                //    catch { }
                //}

                var files = new StringCollection();


                Data.Logger("Loading up Agent Updater v" + versionNumberPatcher);

                //var configFile = Path.Combine(Tools.RetrieveInstallationPath(), "agent.config");
                //Tools.GetProxyFromConfigFile(configFile);

                //Data.Logger("Preparing to Download Content from Server.");
                //DownloadPatchContent();

                Data.Logger("Upgrading RV Agent from v" + Tools.RetrieveCurrentAgentVersion() + " to v" + versionNumber);
                var contents = Directory.GetFiles(Data.AgentUpdateDirectory);

                Data.Logger("Patch Content: ");
                foreach (var item in contents)
                {
                    var stripped = item.Split(new[] { '\\' });
                    var filename = stripped[stripped.Length - 1];
                    if (filename != "TopPatchAgent.exe")
                    {
                        files.Add(item);
                        Data.Logger("  " + item);
                    }
                }
                Data.Logger("Starting to patch Agent core files...");
                StartPatchProcess(files, versionNumber);
            }

        }
        
        private static void StartPatchProcess(StringCollection files, string newVersion)
        {
            const string tpaServiceName = "TpaService";
            const string tpaMaintenance = "TpaMaintenance";

            var rootDirectoryTp = Tools.RetrieveInstallationPath();

            Data.Logger("Retrieving Service install location: " + rootDirectoryTp);
            var tpaServiceFilePath = Path.Combine(rootDirectoryTp, "Agent.RV.Service.exe");
            var tpaMaintenanceFilePath = Path.Combine(rootDirectoryTp, "Agent.RV.WatcherService.exe");

            try
            {
                var savedOperations = Operations.LoadOpDirectory().Where(p => p.operation == OperationType).ToList();
                var operation = new Operations.SavedOpData();

                if (savedOperations.Any())
                {
                    foreach (var item in savedOperations)
                    {
                        var data = (from d in item.filedata_app_uris
                                    where d.file_name.ToLower() == InstallerName
                                    select d).FirstOrDefault();
                        if (data != null)
                            operation = item;
                        else
                            operation = null;
                    }
                }
                else
                {
                    Data.Logger("There are no update operations in Agent Operations folder. Proceeding but server will not receive results.");
                }

                Data.Logger("Uninstalling Services");
                #region Uninstalling Services
                try
                {
                    Tools.UninstallService(tpaMaintenance);
                    Tools.UninstallService(tpaServiceName);
                    Thread.Sleep(5000);
                }
                catch
                { }

                for (int s = 0; s < 5; s++)
                {
                    if (!Tools.ServiceExists(tpaMaintenance))
                    {
                        Data.Logger("Service Removed.");
                        break;
                    }
                }

                for (int s = 0; s < 5; s++)
                {
                    if (!Tools.ServiceExists(tpaServiceName))
                    {
                        Data.Logger("Service Removed.");
                        break;
                    }
                    Thread.Sleep(1000);
                }
                Data.Logger("Service Uninstall, successfull.");
                #endregion

                var pluginsDir = Tools.GetPluginDirectory();
                var opensshDir = Path.Combine(Tools.GetBinDirectory(), "openssh");
                var vncDir = Path.Combine(Tools.GetBinDirectory(), "vnc");
                var etcDir = Tools.GetEtcDirectory();
                var installPath = Tools.RetrieveInstallationPath();

                if (!Directory.Exists(opensshDir))
                    Directory.CreateDirectory(opensshDir);
                if (!Directory.Exists(vncDir))
                    Directory.CreateDirectory(vncDir);
                if (!Directory.Exists(etcDir))
                    Directory.CreateDirectory(etcDir);

                Data.Logger("PATCHING RV AGENT FILES: ");
                #region Patching Files
                foreach (var file in files)
                {
                    var stripped = file.Split(new[] { '\\' });
                    var filename = stripped[stripped.Length - 1];

                    try
                    {
                        switch (filename)
                        {
                            case "Agent.RV.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(pluginsDir, "Agent.RV.dll")))
                                    throw new Exception("Unable to copy Agent.RV.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + pluginsDir);
                                break;
                            case "Agent.RA.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(pluginsDir, "Agent.RA.dll")))
                                    throw new Exception("Unable to copy Agent.RA.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + pluginsDir);
                                break;
                            case "Agent.Monitoring.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(pluginsDir, "Agent.Monitoring.dll")))
                                    throw new Exception("Unable to copy Agent.Monitoring.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + pluginsDir);
                                break;
                            case "Agent.Core.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(installPath, "Agent.Core.dll")))
                                    throw new Exception("Unable to copy Agent.Core.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + installPath);
                                break;
                            case "RestSharp.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(installPath, "RestSharp.dll")))
                                    throw new Exception("Unable to copy RestSharp.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + installPath);
                                break;
                            case "Agent.RV.Service.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(installPath, "Agent.RV.Service.exe")))
                                    throw new Exception("Unable to copy Agent.RV.Service.exe, access denied by the OS.");
                                Data.Logger("Copied to: " + installPath);
                                break;
                            case "Agent.RV.WatcherService.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(installPath, "Agent.RV.WatcherService.exe")))
                                    throw new Exception(
                                        "Unable to copy Agent.RV.WatcherService.exe, access denied by the OS.");
                                Data.Logger("Copied to: " + installPath);
                                break;
                            case "NLog.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(installPath, "NLog.dll")))
                                    throw new Exception("Unable to copy NLog.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + installPath);
                                break;
                            case "Newtonsoft.Json.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(installPath, "Newtonsoft.Json.dll")))
                                    throw new Exception("Unable to copy Newtonsoft.Json.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + installPath);
                                break;
                            case "Microsoft.Deployment.Compression.dll":
                                Data.Logger(filename);
                                if (
                                    !Tools.CopyFile(file,
                                        Path.Combine(installPath, "Microsoft.Deployment.Compression.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + installPath);
                                break;
                            case "Microsoft.Deployment.Compression.Cab.dll":
                                Data.Logger(filename);
                                if (
                                    !Tools.CopyFile(file,
                                        Path.Combine(installPath, "Microsoft.Deployment.Compression.Cab.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + installPath);
                                break;
                            case "addUser.cmd":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "addUser.cmd")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "bash.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "bash.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "chmod.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "chmod.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "chown.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "chown.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygcrypt0.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygcrypt-0.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygcrypto0.9.7.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygcrypto-0.9.7.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygcrypto0.9.8.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygcrypto-0.9.8.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygedit0.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygedit-0.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cyggcc_s1.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cyggcc_s-1.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygiconv2.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygiconv-2.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygintl2.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygintl-2.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygintl3.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygintl-3.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygintl8.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygintl-8.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygminires.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygminires.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygncurses10.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygncurses-10.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygpath.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygpath.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygrunsrv.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygrunsrv.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygssp0.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygssp-0.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygwin1.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygwin1.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygwrap0.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygwrap-0.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "cygz.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygz.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "false.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "false.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "last.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "last.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "ls.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ls.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "mkdir.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkdir.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "mkgroup.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkgroup.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "mkpasswd.c":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkpasswd.c")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "mkpasswd.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkpasswd.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "quietcmd.bat":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "quietcmd.bat")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "rm.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "rm.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "scp.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "scp.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "sftp.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "sftp.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "sh.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "sh.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "ssh.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "sshadd.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh-add.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "sshagent.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh-agent.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "sshkeygen.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh-keygen.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "sshkeyscan.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh-keyscan.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "switch.c":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "switch.c")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "switch.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(opensshDir, "switch.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + opensshDir);
                                break;
                            case "sas.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(vncDir, "sas.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + vncDir);
                                break;
                            case "screenhooks32.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(vncDir, "screenhooks32.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + vncDir);
                                break;
                            case "screenhooks64.dll":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(vncDir, "screenhooks64.dll")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + vncDir);
                                break;
                            case "tvnserver32.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(vncDir, "tvnserver32.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + vncDir);
                                break;
                            case "tvnserver64.exe":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(vncDir, "tvnserver64.exe")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + vncDir);
                                break;
                            case "tunnel":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(etcDir, "tunnel")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + etcDir);
                                break;
                            case "tunnel.pub":
                                Data.Logger(filename);
                                if (!Tools.CopyFile(file, Path.Combine(etcDir, "tunnel.pub")))
                                    throw new Exception(
                                        "Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                                Data.Logger("Copied to: " + etcDir);
                                break;
                            default:
                                continue;
                        }
                    }
                    catch
                    {
                        Data.Logger("Error " + filename);
                    }
                }

                Data.Logger("PATCHING COMPLETE!");
                Data.Logger(" ");
                #endregion

                Tools.SetNewVersionNumber(newVersion);
                RegistryTool.SetAgentVersionNumber(newVersion);
                Data.Logger("Changed revision to: " + newVersion);

                Thread.Sleep(3000);

                if (operation != null)
                {
                    var operationDir = Tools.GetOpDirectory();
                    Data.Logger("Retriving TopPatch Operations Folder.");
                    if (!Directory.Exists(operationDir))
                        Directory.CreateDirectory(operationDir);

                    operation.success = true.ToString().ToLower();
                    operation.reboot_required = false.ToString().ToLower();
                    operation.operation_status = Operations.OperationStatus.ResultsPending;
                    Data.Logger("Updated operation status to: ResultsPending, All files were upgraded successfully. ");

                    Data.Logger("Deleting old agent operations... ");
                    int fileCount = Directory.GetFiles(operationDir, "*.*", SearchOption.AllDirectories).Length;
                    if (fileCount > 0)
                    {
                        Directory.Delete(operationDir, true);
                        Data.Logger("Deleted. ");
                        Directory.CreateDirectory(operationDir);
                    }

                    var tempRootOperationsFile = Path.Combine(Tools.GetOpDirectory(), operation.filedata_app_id + ".data");
                    Data.Logger("Saving operation information to:  " + tempRootOperationsFile);
                    Tools.WriteJsonFile(tempRootOperationsFile, operation);
                    Data.Logger("Operation Saved OK. ");

                    Data.Logger("Deleting old folders...");
                    Tools.DeleteOldFolders();
                    Data.Logger("Done.");
                }

                Data.Logger("Installing Services...");
                #region Installing Services
                ServiceTools.ServiceInstaller.InstallAndStart(tpaMaintenance, "TpaMaintenance", tpaMaintenanceFilePath);
                ServiceTools.ServiceInstaller.InstallAndStart(tpaServiceName, "TpaService", tpaServiceFilePath);

                for (int s = 0; s < 5; s++)
                {
                    if (Tools.ServiceExists(tpaMaintenance))
                    {
                        Data.Logger("TpaMaintenance Started OK.");
                        break;
                    }
                }
                Thread.Sleep(2000);
                for (int s = 0; s < 5; s++)
                {
                    if (Tools.ServiceExists(tpaServiceName))
                    {
                        Data.Logger("TpaService Started OK.");
                        break;
                    }
                }
                Thread.Sleep(1000);
                #endregion
                Data.Logger("Done.");
                Data.Logger("");
                Data.Logger("=========================================================");
                Data.Logger("");
                try
                {
                    Directory.Delete(AppDomain.CurrentDomain.BaseDirectory, true);
                }
                catch
                {
                }
            }
            catch (Exception e)
            {
                Data.Logger("  ");
                Data.Logger("EXCEPTION ERROR: " + e.Message + "  -  " + e.StackTrace);
                Data.Logger("Upate may have not completely successfully. Installing and Starting Services.");

                ServiceTools.ServiceInstaller.InstallAndStart(tpaMaintenance, "TpaMaintenance", tpaMaintenanceFilePath);
                ServiceTools.ServiceInstaller.InstallAndStart(tpaServiceName, "TpaService", tpaServiceFilePath);
            }
        }
    
    }
}
