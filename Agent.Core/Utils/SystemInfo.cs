using System;
using System.CodeDom;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Net;
using System.Management;
using RestSharp;

namespace Agent.Core.Utils
{
    public static class SystemInfo
    {
        private static string OsInfoKey(string key)
        {
            using (var rKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            {
                return ((rKey == null) || (rKey.GetValue(key) == null)) ? String.Empty : rKey.GetValue(key).ToString();
            }
        }

        public static string Code
        {
            get { return "windows"; }
        }

        public static string Name
        {
            get 
            {
                try
                {
                    return OsInfoKey("ProductName");
                }
                catch (Exception e)
                {
                    Logger.Log("Exception: {0}", LogLevel.Error, e.Message);
                    if (e.InnerException != null)
                    {
                        Logger.Log("Inner exception: {0}", LogLevel.Error, e.InnerException.Message);
                    }
                    Logger.Log("Could not get os string.", LogLevel.Error);
                    return Settings.EmptyValue;
                }
            }
        }

        public static string ServicePack
        {
            get
            {
                try
                {
                    OperatingSystem os = Environment.OSVersion;
                    var sp = os.ServicePack;
                    return sp;
                }
                catch (Exception e)
                {
                    Logger.Log("Exception: {0}", LogLevel.Error, e.Message);
                    if (e.InnerException != null)
                    {
                        Logger.Log("Inner exception: {0}", LogLevel.Error, e.InnerException.Message);
                    }
                    Logger.Log("Could not get os string.", LogLevel.Error);
                    return Settings.EmptyValue;
                }
            }
        }

        public static string Version
        {
            get 
            {
                try
                {
                    var os = Environment.OSVersion;
                    var version = String.Format(@"{0}.{1}.{2}", os.Version.Major, os.Version.Minor, os.Version.Build);
                    return version;
                }
                catch (InvalidOperationException e)
                {
                    Logger.Log("Exception: {0}", LogLevel.Error, e.Message);
                    if (e.InnerException != null)
                    {
                        Logger.Log("Inner exception: {0}", LogLevel.Error, e.InnerException.Message);
                    }
                    Logger.Log("Could not get OS version details.", LogLevel.Error);
                    return Settings.EmptyValue;
                }
            }
        }

        public static int BitType
        {
            get 
            {
                try
                {
                    return Bits();
                }
                catch (Exception e)
                {
                    Logger.Log("Exception: {0}", LogLevel.Error, e.Message);
                    if (e.InnerException != null)
                    {
                        Logger.Log("Inner exception: {0}", LogLevel.Error, e.InnerException.Message);
                    }
                    Logger.Log("Could not get bit type.", LogLevel.Error);
                    return 0;
                }
            }
        }

        public static string ComputerName
        {
            get
            {
                try
                {
                    return Environment.MachineName;
                }
                catch(InvalidOperationException e)
                {
                    Logger.Log("Could not get Machine/Computer Name.", LogLevel.Error);
                    Logger.LogException(e);
                    return Settings.EmptyValue;
                }
            }
        }

        public static string Fqdn
        {
            get
            {
                try 
                {
                    var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                    var hostName = Dns.GetHostName();
                    string fqdn;
                    if (!hostName.Contains(domainName))
                        fqdn = hostName + "." + domainName;
                    else
                        fqdn = hostName;
                    
                    return fqdn;
                }
                catch(InvalidOperationException e)
                {
                    Logger.Log("Could not get FQDN.", LogLevel.Error);
                    Logger.LogException(e);
                    return Settings.EmptyValue;
                }
            } 
        }

        public static string Hardware
        {
            get
            {
                var specs = new HardwareSpecs();
                return specs.GetAllHardwareSpecs();
            }
        }

        public static bool IsWindows64Bit
        {
            get
            {
                // IntPtr on a 32bit Windows is size == 4
                var is64BitProcess = (IntPtr.Size == 8);

                // Have to double check. 32bit app can run on 64bit Windows.
                return is64BitProcess || InternalCheckIsWow64();
            }
        }

        private static int Bits()
        {

            var type = (IsWindows64Bit) ? "64" : "32";
            return Convert.ToInt32(type);
        }

        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms684139(v=vs.85).aspx
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        private static bool InternalCheckIsWow64()
        {
            // Windows XP and up. (XP 64bit is NT 5.2)
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (var p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    return IsWow64Process(p.Handle, out retVal) && retVal;
                }
            }
            return false;
        }

        private static string GetLastBootUptime()
        {
            string bootUpTime = null;

            try
            {
                var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj["LastBootUpTime"] == null) continue;
                    bootUpTime = queryObj["LastBootUpTime"].ToString();
                    break;
                }
            }
            catch (ManagementException e)
            {
                Logger.Log("Could not find the last boot up time.", LogLevel.Error);
                Logger.LogException(e);
            }

            return bootUpTime;
        }

        private static DateTime ConvertToDateTime(string time)
        {
            // Format: yyyymmddhhmmss
            var dateTime = new DateTime();

            try
            {
                var year = Convert.ToInt32(time.Substring(0, 4));
                var month = Convert.ToInt32(time.Substring(4, 2));
                var day = Convert.ToInt32(time.Substring(6, 2));
                var hour = Convert.ToInt32(time.Substring(8, 2));
                var minute = Convert.ToInt32(time.Substring(10, 2));
                var second = Convert.ToInt32(time.Substring(12, 2));

                dateTime = new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception e)
            {
                Logger.Log("Could not convert time to DateTime.", LogLevel.Error);
                Logger.LogException(e);
            }
            return dateTime;
        }

        public static long Uptime()
        {
            long uptime = 0;

            try
            {
                var boot = GetLastBootUptime();
                var bootTime = ConvertToDateTime(boot);
                var ts = DateTime.Now - bootTime;
                uptime = Convert.ToInt64(ts.TotalSeconds);
            }
            catch (Exception e)
            {
                Logger.Log("Could not get the system uptime.", LogLevel.Error);
                Logger.LogException(e);
            }

            return uptime;
        }

       /// <summary>
       /// Gets Motherboard info details aka "win32_computersystem".
       /// </summary>
       /// <param name="info">
        ///Motherboard Details.
        ///AdminPasswordStatus
        ///AutomaticManagedPagefile
        ///AutomaticResetBootOption
        ///AutomaticResetCapability
        ///BootOptionOnLimit
        ///BootOptionOnWatchDog
        ///BootROMSupported
        ///BootupState
        ///Caption
        ///ChassisBootupState
        ///CreationClassName
        ///CurrentTimeZone
        ///DaylightInEffect
        ///Description
        ///DNSHostName
        ///Domain
        ///DomainRole
        ///EnableDaylightSavingsTime
        ///FrontPanelResetStatus
        ///HypervisorPresent
        ///InfraredSupported
        ///InitialLoadInfo
        ///InstallDate
        ///KeyboardPasswordStatus
        ///LastLoadInfo
        ///Manufacturer
        ///Model
        ///Name
        ///NameFormat
        ///NetworkServerModeEnabled
        ///NumberOfLogicalProcessors
        ///NumberOfProcessors
        ///OEMLogoBitmap
        ///OEMStringArray
        ///PartOfDomain
        ///PauseAfterReset
        ///PCSystemType
        ///PCSystemTypeEx
        ///PowerManagementCapabilities
        ///PowerManagementSupported
        ///PowerOnPasswordStatus
        ///PowerState
        ///PowerSupplyState
        ///PrimaryOwnerContact
        ///PrimaryOwnerName
        ///ResetCapability
        ///ResetCount
        ///ResetLimit
        ///Roles
        ///Status
        ///SupportContactDescription
        ///SystemStartupDelay
        ///SystemStartupOptions
        ///SystemStartupSetting
        ///SystemType
        ///ThermalState
        ///TotalPhysicalMemory
        ///UserName
        ///WakeUpType
        ///Workgroup
       /// </param>
       /// <returns>String with value from parameter.</returns>
        public static string MotherboardDetails(string info)
        {
            string propertie = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_computersystem");
                ManagementObjectCollection osDetaislCollection = osDetails.Get();
                
                foreach (ManagementObject mo in osDetaislCollection)
                {
                    foreach (var pro in mo.Properties)
                    {
                        if (pro.Name == info)
                        {
                            propertie = pro.Value.ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
                Logger.Log("Error retriving Motherboard info details.", LogLevel.Error);
            }

            return propertie;
        }

       /// <summary>
       /// Gets the System info details aka "win32_operatingsystem".
       /// </summary>
       /// <param name="info">
        ///Operating System Details.
        ///BootDevice
        ///BuildNumber
        ///BuildType
        ///Caption
        ///CodeSet
        ///CountryCode
        ///CreationClassName
        ///CSCreationClassName
        ///CSDVersion
        ///CSName
        ///CurrentTimeZone
        ///DataExecutionPrevention_32BitApplications
        ///DataExecutionPrevention_Available
        ///DataExecutionPrevention_Drivers
        ///DataExecutionPrevention_SupportPolicy
        ///Debug
        ///Description
        ///Distributed
        ///EncryptionLevel
        ///ForegroundApplicationBoost
        ///FreePhysicalMemory
        ///FreeSpaceInPagingFiles
        ///FreeVirtualMemory
        ///InstallDate
        ///LargeSystemCache
        ///LastBootUpTime
        ///LocalDateTime
        ///Locale
        ///Manufacturer
        ///MaxNumberOfProcesses
        ///MaxProcessMemorySize
        ///MUILanguages
        ///Name
        ///NumberOfLicensedUsers
        ///NumberOfProcesses
        ///NumberOfUsers
        ///OperatingSystemSKU
        ///Organization
        ///OSArchitecture
        ///OSLanguage
        ///OSProductSuite
        ///OSType
        ///OtherTypeDescription
        ///PAEEnabled
        ///PlusProductID
        ///PlusVersionNumber
        ///PortableOperatingSystem
        ///Primary
        ///ProductType
        ///RegisteredUser
        ///SerialNumber
        ///ServicePackMajorVersion
        ///ServicePackMinorVersion
        ///SizeStoredInPagingFiles
        ///Status
        ///SuiteMask
        ///SystemDevice
        ///SystemDirectory
        ///SystemDrive
        ///TotalSwapSpaceSize
        ///TotalVirtualMemorySize
        ///TotalVisibleMemorySize
        ///Version
        ///WindowsDirectory
       /// </param>
       /// <returns>String with the value.</returns>
        public static string SystemDetails(string info)
        {
            string propertie = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_operatingsystem");
                ManagementObjectCollection osDetaislCollection = osDetails.Get();

                int x = 0;

                foreach (ManagementObject mo in osDetaislCollection)
                {
                    foreach (var pro in mo.Properties)
                    {
                        if (pro.Name == info)
                        {
                            propertie = pro.Value.ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
                Logger.Log("Error retriving System info details.", LogLevel.Error);
            }

            return propertie;
        }
 
        /// <summary>
        /// Gets the value for the desired value aka "win32_bios".
        /// </summary>
        /// <param name="info">
        ///Bios Details.
        ///BiosCharacteristics
        ///BIOSVersion
        ///BuildNumber
        ///Caption
        ///CodeSet
        ///CurrentLanguage
        ///Description
        ///IdentificationCode
        ///InstallableLanguages
        ///InstallDate
        ///LanguageEdition
        ///ListOfLanguages
        ///Manufacturer
        ///Name
        ///OtherTargetOS
        ///PrimaryBIOS
        ///ReleaseDate
        ///SerialNumber
        ///SMBIOSBIOSVersion
        ///SMBIOSMajorVersion
        ///SMBIOSMinorVersion
        ///SMBIOSPresent
        ///SoftwareElementID
        ///SoftwareElementState
        ///Status
        ///TargetOperatingSystem
        ///Version
        /// </param>
        /// <returns>String with the value from the specified parameter.</returns>
        public static string BiosDetails(string info)
        {
            string propertie = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_bios");
                ManagementObjectCollection osDetaislCollection = osDetails.Get();

                int x = 0;

                foreach (ManagementObject mo in osDetaislCollection)
                {
                    foreach (var pro in mo.Properties)
                    {
                        if (pro.Name == info)
                        {
                            propertie = pro.Value.ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
                Logger.Log("Error retriving Bios info details.", LogLevel.Error);
            }

            return propertie;
        }

        /// <summary>
        /// Gets the processor details aka "win32_processor".
        /// </summary>
        /// <param name="info">
        ///Processor Details.
        ///AddressWidth
        ///Architecture
        ///Availability
        ///Caption
        ///ConfigManagerErrorCode
        ///ConfigManagerUserConfig
        ///CpuStatus
        ///CreationClassName
        ///CurrentClockSpeed
        ///CurrentVoltage
        ///DataWidth
        ///Description
        ///DeviceID
        ///ErrorCleared
        ///ErrorDescription
        ///ExtClock
        ///Family
        ///InstallDate
        ///L2CacheSize
        ///L2CacheSpeed
        ///L3CacheSize
        ///L3CacheSpeed
        ///LastErrorCode
        ///Level
        ///LoadPercentage
        ///Manufacturer
        ///MaxClockSpeed
        ///Name
        ///NumberOfCores
        ///NumberOfLogicalProcessors
        ///OtherFamilyDescription
        ///PNPDeviceID
        ///PowerManagementCapabilities
        ///PowerManagementSupported
        ///ProcessorId
        ///ProcessorType
        ///Revision
        ///Role
        ///SecondLevelAddressTranslationExtensions
        ///SocketDesignation
        ///Status
        ///StatusInfo
        ///Stepping
        ///SystemCreationClassName
        ///SystemName
        ///UniqueId
        ///UpgradeMethod
        ///Version
        ///VirtualizationFirmwareEnabled
        ///VMMonitorModeExtensions
        ///VoltageCaps
        /// </param>
        /// <returns>String with value from specified parameter.</returns>
        public static string ProcessorDetails(string info)
        {
            string propertie = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_processor");
                ManagementObjectCollection osDetaislCollection = osDetails.Get();

                int x = 0;

                foreach (ManagementObject mo in osDetaislCollection)
                {
                    foreach (var pro in mo.Properties)
                    {
                        if (pro.Name == info)
                        {
                            propertie = pro.Value.ToString();
                            break;
                        }
                    }
                }
            }
            catch
            {
                Logger.Log("Error retriving Processor info details.", LogLevel.Error);
            }
        

            return propertie;
        }

    }
}
