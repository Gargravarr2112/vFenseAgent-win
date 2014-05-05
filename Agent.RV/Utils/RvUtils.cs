using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Agent.Core.Utils;
using Microsoft.Win32;

namespace Agent.RV.Utils
{
    public static class RvUtils
    {
        /// <summary>
        /// This controls the automatic restarting on Windows 8 if Critical system updates are installed.
        /// </summary>
        /// <param name="enable">Used to enable auto restart, default = true.</param>
        public static void Windows8AutoRestart(bool enable = true)
        {
            const string win8AutoRestartKey = "Software\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU";
            const string key = "CurrentVersion";
            string osVersion;
            bool nullKey = false;

            //GET OS VERSION
            using (RegistryKey rKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                osVersion = ((rKey == null) || (rKey.GetValue(key) == null))
                                ? String.Empty
                                : rKey.GetValue(key).ToString();
            }

            if (osVersion == "6.2") // WINDOWS 8
            {
                if (enable)
                {
                    //ENABLE WINDOWS 8 AUTOMATIC RESTART ON CRITICAL UPDATE.
                    using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(win8AutoRestartKey, true))
                    {
                        if (regKey != null)
                            regKey.SetValue("NoAutoRebootWithLoggedOnUsers",
                                            unchecked(Convert.ToInt32(0x0)),
                                            RegistryValueKind.DWord);
                        else
                            nullKey = true;
                    }
                    //RegKey was Null, create a new one.
                    if (nullKey)
                    {
                        using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey
                            (win8AutoRestartKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
                        {
                            if (regKey != null)
                                regKey.SetValue("NoAutoRebootWithLoggedOnUsers",
                                                unchecked(Convert.ToInt32(0x0)),
                                                RegistryValueKind.DWord);
                        }
                    }

                    Logger.Log("Enabled Windows 8 Auto Restart on Critical Updates.", LogLevel.Debug);
                }
                else
                {
                    //DISABLE WINDOWS 8 AUTOMATIC RESTART ON CRITICAL UPDATE.
                    using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(win8AutoRestartKey, true))
                    {
                        if (regKey != null)
                            regKey.SetValue("NoAutoRebootWithLoggedOnUsers",
                                            unchecked(Convert.ToInt32(0x1)),
                                            RegistryValueKind.DWord);
                        else
                            nullKey = true;
                    }
                    //RegKey was Null, create a new one.
                    if (nullKey)
                    {
                        using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey
                            (win8AutoRestartKey, RegistryKeyPermissionCheck.ReadWriteSubTree))
                        {
                            if (regKey != null)
                                regKey.SetValue("NoAutoRebootWithLoggedOnUsers",
                                                unchecked(Convert.ToInt32(0x1)),
                                                RegistryValueKind.DWord);
                        }
                    }

                    Logger.Log("Disabled Windows 8 Auto Restart on Critical Updates.", LogLevel.Debug);
                }
            }
        }

        /// <summary>
        /// Used to send restart command to the system.
        /// </summary>
        /// <param name="secondsToShutdown">Seconds to wait till restart, default 60 seconds.</param>
        public static void RestartSystem(int secondsToShutdown = 60)
        {
            /*
             * ProcessInfo.Arguments 
             * -r = restart 
             * -f = force applications to shutdown
             * -t = time in seconds till shutdown
             * -c = comment to warn user of shutdown.
             * */
            var processInfo = new ProcessStartInfo();
            processInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shutdown.exe");

            string comment = String.Format("In {0} seconds, this computer will be restarted on behalf of the TopPatch RV Server.", secondsToShutdown);
            processInfo.Arguments = String.Format(@"-r -f -t {0} -c ""{1}"" ", secondsToShutdown, comment);

            Process.Start(processInfo);
        }

        #region methods to generate HASH file tags.
        public static string Md5HashFile(string fn)
        {
            try
            {
                byte[] hash = MD5.Create().ComputeHash(File.ReadAllBytes(fn));
                return BitConverter.ToString(hash).Replace("-", "");
            }
            catch
            {
                return String.Empty;
            }
        }

        public static string Sha1HashFile(string fn)
        {
            try
            {
                byte[] hash = File.ReadAllBytes(fn);
                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] result = sha.ComputeHash(hash);
                return BitConverter.ToString(result).Replace("-", "");
            }
            catch
            {
                return String.Empty;
            }
        }

        public static string Sha256HashFile(string fn)
        {
            try
            {
                byte[] hash = File.ReadAllBytes(fn);
                SHA256 sha = new SHA256CryptoServiceProvider();
                byte[] result = sha.ComputeHash(hash);
                return BitConverter.ToString(result).Replace("-", "");
            }
            catch
            {
                return String.Empty;
            }
        }
        #endregion

    }
}
