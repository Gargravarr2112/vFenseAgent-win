using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace initial_setup
{
    class DoPatcher
    {

        private StringCollection files = new StringCollection();

        private string mainPath = String.Empty;
        private string defaultPath = Path.Combine(Environment.CurrentDirectory, @"Agent.Console\bin\Debug");
        

        public DoPatcher()
        {
            string path = Directory.GetCurrentDirectory();
            mainPath = path;
        }

        public void DoDownload()
        {

            if (!Directory.Exists(defaultPath))
                Directory.CreateDirectory(defaultPath);
            
            string downloadpath = Path.Combine(defaultPath, "download");
            string version = Tools.GetAvailableVersion();
            string downloadlink = Tools.GetDownloadLink(version);
            byte dlspeed = 255;

            if (!Directory.Exists(downloadpath))
                Directory.CreateDirectory(downloadpath);

            WebClient webClient = new WebClient();
            string downloadpathName = downloadpath + @"\dependecies.zip";

            webClient.DownloadFile(downloadlink, downloadpathName);     //Tools.DownloadThrottle(downloadpath, downloadlink, "dependencies.zip", dlspeed);


        }

        public void DoExtract()
        {
            
        }
        public StringCollection GetFileStrings()
        {
            files.Clear();

            string downloadpath = Path.Combine(defaultPath, "download");

            var contents = Directory.GetFiles(downloadpath);

            foreach (var item in contents)
            {
                var stripped = item.Split(new[] { '\\' });
                var filename = stripped[stripped.Length - 1];

                files.Add(item);
            }

            return files;
        }

        public StringCollection GetFileStrings(string x)
        {
            files.Clear();

            string downloadpath = x;

            var contents = Directory.GetFiles(downloadpath);

            foreach (var item in contents)
            {
                var stripped = item.Split(new[] { '\\' });
                var filename = stripped[stripped.Length - 1];

                files.Add(item);
            }

            return files;
        }

        public void MoveFiles()
        {
            var binDir = Path.Combine(defaultPath, "bin");
            var installPath = Path.Combine(defaultPath, "core");
            var pluginsDir = Path.Combine(defaultPath, "plugins");
            var opensshDir = Path.Combine(binDir, "openssh");
            var vncDir = Path.Combine(binDir, "vnc");
            var etcDir = Path.Combine(defaultPath, "etc");
            var content = Path.Combine(defaultPath, "content");

            if (!Directory.Exists(installPath))
                Directory.CreateDirectory(installPath);
            if (!Directory.Exists(pluginsDir))
                Directory.CreateDirectory(pluginsDir);
            if (!Directory.Exists(opensshDir))
                Directory.CreateDirectory(opensshDir);
            if (!Directory.Exists(vncDir))
                Directory.CreateDirectory(vncDir);
            if (!Directory.Exists(etcDir))
                Directory.CreateDirectory(etcDir);
            if (!Directory.Exists(content))
                Directory.CreateDirectory(content);

            var files = GetFileStrings();

            #region moving the files
            foreach (var file in files)
            {
                var stripped = file.Split(new[] { '\\' });
                var filename = stripped[stripped.Length - 1];

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
                            throw new Exception("Unable to copy Agent.RV.WatcherService.exe, access denied by the OS.");
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
                        if (!Tools.CopyFile(file, Path.Combine(installPath, "Microsoft.Deployment.Compression.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + installPath);
                        break;
                    case "Microsoft.Deployment.Compression.Cab.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(installPath, "Microsoft.Deployment.Compression.Cab.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + installPath);
                        break;
                    case "addUser.cmd":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "addUser.cmd")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "bash.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "bash.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "chmod.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "chmod.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "chown.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "chown.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygcrypt0.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygcrypt-0.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygcrypto0.9.7.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygcrypto-0.9.7.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygcrypto0.9.8.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygcrypto-0.9.8.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygedit0.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygedit-0.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cyggcc_s1.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cyggcc_s-1.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygiconv2.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygiconv-2.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygintl2.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygintl-2.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygintl3.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygintl-3.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygintl8.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygintl-8.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygminires.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygminires.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygncurses10.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygncurses-10.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygpath.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygpath.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygrunsrv.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygrunsrv.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygssp0.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygssp-0.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygwin1.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygwin1.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygwrap0.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygwrap-0.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "cygz.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "cygz.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "false.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "false.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "last.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "last.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "ls.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ls.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "mkdir.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkdir.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "mkgroup.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkgroup.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "mkpasswd.c":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkpasswd.c")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "mkpasswd.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkpasswd.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "quietcmd.bat":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "quietcmd.bat")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "rm.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "rm.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "scp.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "scp.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "sftp.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "sftp.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "sh.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "sh.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "ssh.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "sshadd.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh-add.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "sshagent.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh-agent.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "sshkeygen.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh-keygen.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "sshkeyscan.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "ssh-keyscan.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "switch.c":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "switch.c")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "switch.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "switch.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "sas.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(vncDir, "sas.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + vncDir);
                        break;
                    case "screenhooks32.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(vncDir, "screenhooks32.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + vncDir);
                        break;
                    case "screenhooks64.dll":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(vncDir, "screenhooks64.dll")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + vncDir);
                        break;
                    case "tvnserver32.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(vncDir, "tvnserver32.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + vncDir);
                        break;
                    case "tvnserver64.exe":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(vncDir, "tvnserver64.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + vncDir);
                        break;
                    case "tunnel":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(etcDir, "tunnel")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + etcDir);
                        break;
                    case "tunnel.pub":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(etcDir, "tunnel.pub")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + etcDir);
                        break;
                    default:
                        continue;
                }
            }
            #endregion


            if (Directory.Exists(installPath))
            {
                var file = GetFileStrings(installPath);
                foreach (var x in file)
                {
                    try
                    {
                        File.Delete(x);
                    }
                    catch
                    {

                    }
                }
                Directory.Delete(installPath);

            }
            var download = Path.Combine(defaultPath, "download");

            if (Directory.Exists(download))
            {
                var file = GetFileStrings((download));
                foreach (var x in file)
                {
                    try
                    {
                        File.Delete(x);
                    }
                    catch
                    {

                    }
                }
                Directory.Delete(download);
            }

            if (Directory.Exists(pluginsDir))
            {
                var file = GetFileStrings(pluginsDir);
                foreach (var x in file)
                {
                    try
                    {
                        File.Delete(x);
                    }
                    catch
                    {

                    }
                }
            }


        }
    }
}

