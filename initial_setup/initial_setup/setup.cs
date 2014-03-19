using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace initial_setup
{
    class DoPatcher
    {

        private static StringCollection files = new StringCollection();

        private static string defaultPath = Path.Combine(Environment.CurrentDirectory, @"Agent.Console\bin\Debug");

        private const string AgentCore = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Agent.Core.dll";
        private const string AgentRv = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Agent.RV.dll";
        private const string AgentRA = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Agent.RA.dll";
        private const string AgentMonitoring = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Agent.Monitoring.dll";
        private const string TpaServiceFile = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Agent.RV.Service.exe";
        private const string TpaMaintenanceFile = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Agent.RV.WatcherService.exe";
        private const string RestSharp = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/RestSharp.dll";
        private const string JsonDll = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Newtonsoft.Json.dll";
        private const string NLog = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/NLog.dll";
        private const string MicroCompress1 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Microsoft.Deployment.Compression.dll";
        private const string MicroCompress2 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/Microsoft.Deployment.Compression.Cab.dll";
        //############################################**Updates for Remote Assist**##########################################################################################
        //***openssh files***
        private const string addUser = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/addUser.cmd";
        private const string bash = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/bash.exe";
        private const string chmod = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/chmod.exe";
        private const string chown = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/chown.exe";
        private const string cygcrypt0 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygcrypt0.dll";
        private const string cygcrypto097 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygcrypto0.9.7.dll";
        private const string cygcrypto098 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygcrypto0.9.8.dll";
        private const string cygedit0 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygedit0.dll";
        private const string cyggccs1 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cyggcc_s1.dll";
        private const string cygiconv2 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygiconv2.dll";
        private const string cygintl2 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygintl2.dll";
        private const string cygintl3 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygintl3.dll";
        private const string cygintl8 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygintl8.dll";
        private const string cygminires = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygminires.dll";
        private const string cygncures10 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygncurses10.dll";
        private const string cygpath = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygpath.exe";
        private const string cygrunsrv = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygrunsrv.exe";
        private const string cygssp0 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygssp0.dll";
        private const string cygwin1 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygwin1.dll";
        private const string cygwrap0 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygwrap0.dll";
        private const string cygz = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/cygz.dll";
        private const string xfalse = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/false.exe";
        private const string last = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/last.exe";
        private const string ls = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/ls.exe";
        private const string mkdir = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/mkdir.exe";
        private const string mkgroup = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/mkgroup.exe";
        private const string mkpasswd = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/mkpasswd.c";
        private const string mkpasswdexe = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/mkpasswd.exe";
        private const string quietcmd = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/quietcmd.bat";
        private const string rm = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/rm.exe";
        private const string scp = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/scp.exe";
        private const string sftp = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/sftp.exe";
        private const string sh = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/sh.exe";
        private const string ssh = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/ssh.exe";
        private const string sshadd = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/sshadd.exe";
        private const string sshagent = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/sshagent.exe";
        private const string sshkeygen = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/sshkeygen.exe";
        private const string sshkeyscan = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/sshkeyscan.exe";
        private const string cswitch = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/switch.c";
        private const string xswitch = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/switch.exe";
        //***vnc files***
        private const string sas = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/sas.dll";
        private const string screenhooks32 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/screenhooks32.dll";
        private const string screenhooks64 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/screenhooks64.dll";
        private const string tvnserver32 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/tvnserver32.exe";
        private const string tvnserver64 = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/tvnserver64.exe";
        //***etc files***
        private const string tunnel = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/tunnel";
        private const string ptunnel = @"http://updater.toppatch.com/Packages/Products/RV_AGENTS/Windows/LatestPatch/tunnel.pub";
        public static void DoDownload()
        {
            
            if (!Directory.Exists(defaultPath))
                Directory.CreateDirectory(defaultPath);

            var uris = new List<string> { AgentCore, AgentRv, AgentRA, AgentMonitoring, RestSharp, TpaMaintenanceFile, TpaServiceFile, MicroCompress1, MicroCompress2,
                                            NLog, JsonDll, addUser, bash, chmod, chown, cygcrypt0, cygcrypto097, cygcrypto098, cygedit0, cyggccs1, cygiconv2, cygintl2, 
                                            cygintl3, cygintl8, cygminires, cygncures10, cygpath, cygrunsrv, cygssp0, cygwin1, cygwrap0, cygz, xfalse, last, ls, mkdir, 
                                            mkgroup, mkpasswd, mkpasswdexe, quietcmd, rm, scp, sftp, sh, ssh, sshadd, sshagent, sshkeygen, sshkeyscan, cswitch, xswitch,
                                            sas, screenhooks32, screenhooks64, tvnserver32, tvnserver64, tunnel, ptunnel};

            string downloadpath = Path.Combine(defaultPath, "download");

            if (!Directory.Exists(downloadpath))
                Directory.CreateDirectory(downloadpath);

            foreach (var uri in uris)
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        var stripped = uri.Split(new[] { '/' });
                        var filename = stripped[stripped.Length - 1];

                        //if (Data.ProxyObj != null)
                        //    client.Proxy = Data.ProxyObj;

                        client.DownloadFile(uri, Path.Combine(downloadpath, filename));
                    }
                }
                catch
                {
                    Console.WriteLine("failed attempting to download " + uri);
                }
            }

        }

        public static StringCollection GetFileStrings()
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
        public static StringCollection GetFileStrings(string x)
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

        public static void MoveFiles()
        {
            var installPath = Path.Combine(defaultPath, "core");
            var pluginsDir = Path.Combine(defaultPath, "plugins");
            var opensshDir = Path.Combine(defaultPath, "openssh");
            var vncDir = Path.Combine(defaultPath, "vnc");
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
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "mkpassed.exe")))
                            throw new Exception("Unable to copy Microsoft.Deployment.Compression.Cab.dll, access denied by the OS.");
                        Data.Logger("Copied to: " + opensshDir);
                        break;
                    case "quietcmd.bat":
                        Data.Logger(filename);
                        if (!Tools.CopyFile(file, Path.Combine(opensshDir, "quitecmd.bat")))
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
