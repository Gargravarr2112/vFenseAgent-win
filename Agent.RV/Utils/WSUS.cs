using Microsoft.Win32;

namespace Agent.RV.Utils
{

    /// <summary>
    /// Class checks if WSUS is enable on the system.
    /// Check various properties and registry keys on the system that determine different status of WSUS.
    /// Info available at: https://github.com/toppatch/vFenseAgent-win/wiki/Registry-keys-for-configuring-Automatic-Updates-&-WSUS
    /// </summary>
    public static class WSUS
    {
        //Registry key where WSUS properties are resgitered.
        private const string WinUpdate = @"Software\Policies\Microsoft\Windows\WindowsUpdate";
        private const string InternetCom = @"SYSTEM\Internet\Communication Management\Internet Communication";
        private const string AutoUpdate = @"Software\Policies\Microsoft\Windows\WindowsUpdate\AU";
        
        //WSUS Server URL
        private static string _wsusServer = "";

        /// <summary>
        /// Gets the WSUS server link.
        /// </summary>
        public static string GetServerWSUS
        {
            get
            {
                return _wsusServer;
            }
        }

        /// <summary>
        /// Check is WSUS is enable on the system.
        /// Check using:
        /// HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WindowsUpdate
        /// summary:
        /// Entry name					Data type	Values
        /// 
        /// DisableWindowsUpdateAccess	Reg_DWORD	1 = Disables access to Windows Update.
        /// 										0 = Enables access to Windows Update
        /// 										
        /// WUServer					Reg_SZ		HTTP(S) URL of the WSUS server that is
        /// 										used by Automatic Updates and API 
        /// 										callers (by default). This policy is 
        /// 										paired with WUStatusServer, and both 
        /// 										keys must be set to the same value to be valid.
        /// 										
        /// WUStatusServer				Reg_SZ		The HTTP(S) URL of the server to which 
        /// 										reporting information is sent for client 
        /// 										computers that use the WSUS server that is 
        /// 										configured by the WUServer key. This policy 
        /// 										is paired with WUServer, and both keys must 
        /// 										be set to the same value to be valid.
        /// </summary>
        /// <returns>Returns bool for WSUS enable or disable.</returns>
        public static bool IsWSUSEnabled()
        {
            bool WSUSKeyON = false;

            //"Software\Policies\Microsoft\Windows\WindowsUpdate"
            var rkwinupdate = Registry.LocalMachine.OpenSubKey(WinUpdate);
            //Software\Policies\Microsoft\Windows\WindowsUpdate\AU
            var rkWSUS = Registry.LocalMachine.OpenSubKey(AutoUpdate);


            //First check 'HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WindowsUpdate\AU'
            //For: UseWUServer - Without this set to 1 (On) the other keys (WUServer and WUStatusServer) will be ignored.
            try
            {
                if (rkWSUS != null)
                {
                    var wuServer = rkWSUS.GetValue("UseWUServer");
                    if (wuServer != null)
                    {
                        var useWuServer = int.Parse(string.Format("{0}", wuServer));
                        rkWSUS.Close();
                        if (useWuServer == 1)
                            WSUSKeyON = true;
                        else
                            WSUSKeyON = false;
                    }
                }
            }
            catch
            {
                WSUSKeyON = false;
            }


            //If the above key is found and set to "1" then proceed.
            try
            {
                if (!WSUSKeyON) return false;
                if (rkwinupdate == null) return false;

                var wuServer = (string)rkwinupdate.GetValue("WUServer");
                var wuStatusServer = (string)rkwinupdate.GetValue("WUStatusServer");

                if (wuServer == wuStatusServer)
                {
                    _wsusServer = wuServer;
                    rkwinupdate.Close();
                    return true;
                }
                rkwinupdate.Close();
                return false;
            }
            catch
            {
                if (rkwinupdate != null) rkwinupdate.Close();
                return false;
            }
        }

        /// <summary>
        /// Check is the access to the Windows Updates are allowed.
        /// Using the registry key values in:
        /// HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WindowsUpdate
        /// 
        /// Entry name					Data type	Values
        /// 
        /// DisableWindowsUpdateAccess	Reg_DWORD	1 = Disables access to Windows Update.
        /// 										0 = Enables access to Windows Update
        /// 										
        /// WUServer					Reg_SZ		HTTP(S) URL of the WSUS server that is
        /// 										used by Automatic Updates and API 
        /// 										callers (by default). This policy is 
        /// 										paired with WUStatusServer, and both 
        /// 										keys must be set to the same value to be valid.
        /// 										
        /// WUStatusServer				Reg_SZ		The HTTP(S) URL of the server to which 
        /// 										reporting information is sent for client 
        /// 										computers that use the WSUS server that is 
        /// 										configured by the WUServer key. This policy 
        /// 										is paired with WUServer, and both keys must 
        /// 										be set to the same value to be valid.
        /// </summary>
        /// <returns>Returns bool for access to the Windows Update access</returns>
        public static bool IsWindowsUpdateAccessDisabled()
        {
            //"Software\Policies\Microsoft\Windows\WindowsUpdate"
            var rkwinupdate = Registry.LocalMachine.OpenSubKey(WinUpdate);

            try
            {
                if (rkwinupdate != null)
                {
                    var winupdate = rkwinupdate.GetValue("DisableWindowsUpdateAccess");
                    if (winupdate != null)
                    {
                        var updateAccess = int.Parse(string.Format("{0}", winupdate));
                        rkwinupdate.Close();
                        return updateAccess != 0;
                    }
                }
            }
            catch
            {
                if (rkwinupdate != null) rkwinupdate.Close();
                return false;
            }
            if (rkwinupdate != null) rkwinupdate.Close();
            return false;
        }
        
        /// <summary>
        /// Check if the internet communication to Windows Updates is disable.
        /// If enabled, blocks access to "http://windowsupdate.microsoft.com".
        /// Using registry in the registry key location:
        /// HKEY_LOCAL_MACHINE\SYSTEM\Internet Communication Management\Internet Communication
        /// example:
        /// 
        /// Entry name					Data type	Corresponding Group 	Values
        /// 										Policy Setting			
        /// 
        /// DisableWindowsUpdateAccess	Reg_DWORD	Turn off access to		1 = Enabled. All Windows Update 
        /// 										all Windows Update		features are removed. This includes 
        /// 										features				blocking access to the Windows Update 
        /// 																website at http://windowsupdate.microsoft.com, 
        /// 																from the Windows Update hyperlink on the Start 
        /// 																menu, and also on the Tools menu in Internet 
        /// 																Explorer. Windows automatic updating is also 
        /// 																disabled; you will neither be notified about 
        /// 																nor will you receive critical updates from 
        /// 																Windows Update. This setting also prevents 
        /// 																Device Manager from automatically installing 
        /// 																driver updates from the Windows Update website.		
        /// 																
        /// 																0 = Disabled or not configured. Users will be 
        /// 																able to access the Windows Update website and 
        /// 																enable automatic updating to receive notifications 
        /// 																and critical updates from Windows Update..
        /// </summary>
        /// <returns>Returns bool.</returns>
        public static bool IsInternetCommWinUpdateAccessDisabled()
        {
            //"SYSTEM\Internet\Communication Management\Internet Communication"
            var rkInterCom = Registry.LocalMachine.OpenSubKey(InternetCom);

            try
            {
                if (rkInterCom != null)
                {
                    var winupdate = rkInterCom.GetValue("DisableWindowsUpdateAccess");
                    if (winupdate != null)
                    {
                        var updateAccess = int.Parse(string.Format("{0}", winupdate));
                        rkInterCom.Close();

                        if (updateAccess == 0) //0 = Not configured, 
                            return false;

                        if (updateAccess == 1) //1 = update access disabled
                            return true;
                    }
                }
            }
            catch
            {
                if (rkInterCom != null) rkInterCom.Close();
                return false;
            }
            if (rkInterCom != null) rkInterCom.Close();

            return false;
        }

        /// <summary>
        /// Checks the Registry keys for Automatic Update configuration options.
        /// HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WindowsUpdate\AU
        /// 
        /// Entry name		Data type	Values
        /// 
        /// AUOptions		Reg_DWORD	2 = Notify before download.
        /// 							3 = Automatically download 
        /// 							    and notify of installation.
        /// 							4 = Automatically download 
        /// 							    and schedule installation. 
        /// 							    Only valid if values exist 
        /// 							    for ScheduledInstallDay 
        /// 							    and ScheduledInstallTime.
        /// 							5 = Automatic Updates is 
        /// 							    required and users can configure it.
        /// 
        /// NoAutoUpdate	Reg_DWORD	0 = Enable Automatic Updates.
        /// 							1 = Disable Automatic Updates.
        /// 							
        /// UseWUServer		Reg_DWORD	1 = The computer gets its updates from a WSUS server.
        /// 							0 = The computer gets its updates from Microsoft Update.
        /// 							The WUServer value is not respected unless this key is set.
        /// </summary>
        /// <returns>
        /// Returns the option setup on the system.
        /// Using Enum AutomaticUpdateStatus.
        /// </returns>
        public static AutomaticUpdateStatus GetAutomaticUpdatesOptions()
        {
            //Software\Policies\Microsoft\Windows\WindowsUpdate\AU
            var rkautoupdate = Registry.LocalMachine.OpenSubKey(AutoUpdate);

            try
            {
                if (rkautoupdate != null)
                {
                    var autoUpdate = rkautoupdate.GetValue("AUOptions");
                    if (autoUpdate != null)
                    {
                        var auOptions = int.Parse(string.Format("{0}", autoUpdate));
                        rkautoupdate.Close();

                        switch (auOptions)
                        {
                            case 0:
                                return AutomaticUpdateStatus.Error;
                            case 2:
                                return AutomaticUpdateStatus.NotifyBeforeDownload;
                            case 3:
                                return AutomaticUpdateStatus.AutomaticDownloadAndNotifyOfInstall;
                            case 4:
                                return AutomaticUpdateStatus.AutomaticDownloadAndScheduleInstall;
                            case 5:
                                return AutomaticUpdateStatus.AutomaticUpdatesIsRequiredAndUsersCanConfigureIt;

                        }

                    }
                }
            }
            catch
            {
                if (rkautoupdate != null) rkautoupdate.Close();
                return AutomaticUpdateStatus.Error;
            }
            if (rkautoupdate != null) rkautoupdate.Close();
            return AutomaticUpdateStatus.Error;
        }

        /// <summary>
        /// Check if the WSUS Automatic update are enabled.
        /// Gets registry Value "NoAutoUpdate".
        /// HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\WindowsUpdate\AU
        /// key values:
        /// 
        /// Entry name		Data type	Values
        /// 
        /// AUOptions		Reg_DWORD	2 = Notify before download.
        /// 							3 = Automatically download 
        /// 							    and notify of installation.
        /// 							4 = Automatically download 
        /// 							    and schedule installation. 
        /// 							    Only valid if values exist 
        /// 							    for ScheduledInstallDay 
        /// 							    and ScheduledInstallTime.
        /// 							5 = Automatic Updates is 
        /// 							    required and users can configure it.
        /// 
        /// NoAutoUpdate	Reg_DWORD	0 = Enable Automatic Updates.
        /// 							1 = Disable Automatic Updates.
        /// 							
        /// UseWUServer		Reg_DWORD	1 = The computer gets its updates from a WSUS server.
        /// 							0 = The computer gets its updates from Microsoft Update.
        /// 							The WUServer value is not respected unless this key is set.
        /// </summary>
        /// <returns></returns>
        public static bool IsAutomaticUpdatesEnabled()
        {
            //Software\Policies\Microsoft\Windows\WindowsUpdate\AU
            var rkautoupdate = Registry.LocalMachine.OpenSubKey(AutoUpdate);

            try
            {
                if (rkautoupdate != null) //check the registry key exsist
                {
                    var autoUpdate = rkautoupdate.GetValue("NoAutoUpdate");
                    if (autoUpdate == null) return false;

                    var noAutoUpdate = int.Parse(string.Format("{0}", autoUpdate));
                    if (noAutoUpdate == 0) //0 - Automatic Updates are Enabled
                        return true;
                    if (noAutoUpdate == 1) //1 - Automatic Updates are disabled
                        return false;
                }
            }
            catch
            {
                if (rkautoupdate != null) rkautoupdate.Close();
                return false;
            }
            finally
            {
                if (rkautoupdate != null) rkautoupdate.Close();
            }

            return false;
        }

        /// <summary>
        /// Enum to use with the Automatic update options.
        /// </summary>
        public enum AutomaticUpdateStatus
        {
            Error = 0,
            NotifyBeforeDownload = 2,
            AutomaticDownloadAndNotifyOfInstall = 3,
            AutomaticDownloadAndScheduleInstall = 4,
            AutomaticUpdatesIsRequiredAndUsersCanConfigureIt = 5
        }
    }
}
