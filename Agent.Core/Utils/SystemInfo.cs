using System;
using System.CodeDom;
using System.Globalization;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Net;
using System.Management;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Agent.Core.Utils
{
    public static class SystemInfo
    {

        #region Get verious system information.
        
        public static string Code
        {
            get { return "windows"; }
        }
        
        public static string FullyQualifiedDomainName
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

        /// <summary>
        /// Gets the info required by the server for the CPU.
        /// </summary>
        /// <returns>Returns JObject with the CPU info.</returns>
        public static JObject GetCpuInfo()
        {
            var cpuInfo = new JObject();
            try
            {
                cpuInfo["cpu_id"] = ProcessorDetails("DeviceID");
                cpuInfo["name"] = ProcessorDetails("Name");
                cpuInfo["bit_type"] = ProcessorDetails("DataWidth");
                cpuInfo["speed_mhz"] = ProcessorDetails("MaxClockSpeed");
                cpuInfo["cores"] = ProcessorDetails("NumberOfCores");
                //get check for l2 and l3 cache, and get the highest level
                if (string.IsNullOrEmpty(ProcessorDetails("L3CacheSize")))
                    cpuInfo["cache_kb"] = ProcessorDetails("L3CacheSize");
                else 
                    cpuInfo["cache_kb"] = ProcessorDetails("L2CacheSize");
            }
            catch (Exception)
            {

                throw;
            }

            return cpuInfo;
        }

        /// <summary>
        /// Gets the required video(display) information for the server.
        /// </summary>
        /// <returns>Returns a JObject with the required information.</returns>
        public static JObject GetVideoInfo()
        {
            var videoInfo = new JObject();

            try
            {
                videoInfo["name"] = VideoDetails("Caption");
                videoInfo["speed_mhz"] = VideoDetails("AdapterDACType");
                videoInfo["ram_kb"] = VideoDetails("AdapterRAM");
            }
            catch (Exception)
            {
                
                throw;
            }

            return videoInfo;
        }

        public static JObject GetNetwork()
        {
            var networkInfo = new JObject();
            var controller = new JObject();
            try
            {
                foreach (var net in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (net.OperationalStatus == OperationalStatus.Up &&
                        (net.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                         || net.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        //Obtain IP for the particular Interface
                        foreach (var ip in net.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) continue;
                            controller.Add("ip_address", ip.Address.ToString());
                            controller.Add("mac", net.GetPhysicalAddress().ToString());
                            controller.Add("name", net.Name);
                            break;
                        }

                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return controller;
        }

        /// <summary>
        /// Gets the HD information requeried by the server.
        /// </summary>
        /// <returns>Returns JObect with the HD info.</returns>
        public static JObject GetHardDrive()
        {
            var harddriveinfo = new JObject();

            try
            {
                var drivesarray = Environment.GetLogicalDrives();

                foreach (var drive in drivesarray)
                {
                    var di = new System.IO.DriveInfo(drive);
                    if (!di.IsReady) continue;
                    if ((di.DriveType != System.IO.DriveType.Fixed) && (di.DriveType != System.IO.DriveType.Removable))
                        continue;
                    long tmpHdSize;
                    long tmpHdFree;

                    var name = ((di.VolumeLabel == null) || (di.VolumeLabel.Equals(String.Empty))) ? di.Name : di.VolumeLabel;
                    name = name.Replace("\\", "");
                    var Size = di.TotalSize.ToString(CultureInfo.InvariantCulture);
                    var InterfaceType = di.DriveFormat;
                    var FreeSpace = di.TotalFreeSpace.ToString(CultureInfo.InvariantCulture);

                    var converted = Int64.TryParse(Size, out tmpHdSize);
                    converted = Int64.TryParse(FreeSpace, out tmpHdFree);


                    if (converted)
                    {
                        tmpHdSize = tmpHdSize / 1024; //Convert to KB from Bytes
                        tmpHdFree = tmpHdFree / 1024;
                    }

                    harddriveinfo["name"] = name;
                    harddriveinfo["file_system"] = InterfaceType;
                    harddriveinfo["file_kb"] = tmpHdSize.ToString(CultureInfo.InvariantCulture);
                    harddriveinfo["free_size_kb"] = tmpHdFree.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                
                throw;
            }

            return harddriveinfo;
        }

        /// <summary>
        /// Gets the total amount of RAM memory on the system.
        /// </summary>
        /// <returns>Returns a long variable with total amount of ram on the system.</returns>
        public static long GetMemory()
        {
            string allMemory = PhysicalMemoryDetails("Capacity");
            string[] MemBank = allMemory.Split('|');
            long memory = 0;

            foreach (string bank in MemBank)
            {
                if (!string.IsNullOrEmpty(bank))
                {
                    try
                    {
                        long x = long.Parse(bank);
                        memory = memory + x;
                    }
                    catch (Exception)
                    {
                        
                    }
                }
            }
            memory = (memory/1024);

            return memory;
        }


#endregion

        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms684139(v=vs.85).aspx
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        /// <summary>
        /// Determins if system is 64/32bit.
        /// </summary>
        /// <returns>True for 64bit operating system.</returns>
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
        
        /// <summary>
        /// Converts the systems last up time format into a yyyymmddhhmmss format.
        /// </summary>
        /// <param name="time">System last up time.</param>
        /// <returns>DateTime variable structured yyyymmddhhmmss.</returns>
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


        /// <summary>
        /// Generate the system uptime.
        /// </summary>
        /// <returns>Long, seconds of uptime.</returns>
        public static long Uptime()
        {
            long uptime = 0;

            try
            {
                var boot = SystemDetails("LastBootUpTime");
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
        /// 
        /// AdminPasswordStatus - 3
        ///AutomaticManagedPagefile - False
        ///AutomaticResetBootOption - True
        ///AutomaticResetCapability - True
        ///BootOptionOnLimit -
        ///BootOptionOnWatchDog -
        ///BootROMSupported - True
        ///BootupState - Normal boot
        ///Caption - Name-PC
        ///ChassisBootupState - 3
        ///CreationClassName - Win32_ComputerSystem
        ///CurrentTimeZone - -240
        ///DaylightInEffect - True
        ///Description - AT/AT COMPATIBLE
        ///DNSHostName - Name-PC
        ///Domain - WORKGROUP
        ///DomainRole - 0
        ///EnableDaylightSavingsTime - True
        ///FrontPanelResetStatus - 3
        ///HypervisorPresent - False
        ///InfraredSupported - False
        ///InitialLoadInfo -
        ///InstallDate -
        ///KeyboardPasswordStatus - 3
        ///LastLoadInfo -
        ///Manufacturer - EVGA INTERNATIONAL CO.,LTD
        ///Model - E679 1.1.1
        ///Name - Name-PC
        ///NameFormat -
        ///NetworkServerModeEnabled - True
        ///NumberOfLogicalProcessors - 8
        ///NumberOfProcessors - 1
        ///OEMLogoBitmap -
        ///OEMStringArray - System.String[]
        ///PartOfDomain - False
        ///PauseAfterReset - -1
        ///PCSystemType - 1
        ///PCSystemTypeEx - 1
        ///PowerManagementCapabilities -
        ///PowerManagementSupported -
        ///PowerOnPasswordStatus - 3
        ///PowerState - 0
        ///PowerSupplyState - 3
        ///PrimaryOwnerContact -
        ///PrimaryOwnerName - email@gmail.com
        ///ResetCapability - 1
        ///ResetCount - -1
        ///ResetLimit - -1
        ///Roles - System.String[]
        ///Status - OK
        ///SupportContactDescription -
        ///SystemStartupDelay -
        ///SystemStartupOptions -
        ///SystemStartupSetting -
        ///SystemType - x64-based PC
        ///ThermalState - 3
        ///TotalPhysicalMemory - 17145245696
        ///UserName - Name-PC\DOODS-PC\UserName
        ///WakeUpType - 6
        ///Workgroup - WORKGROUP
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
                Logger.Log("Error retriving Motherboard info details, parameter-{0}.", LogLevel.Error, info);
            }

            return propertie;
        }

       /// <summary>
       /// Gets the System info details aka "win32_operatingsystem".
       /// </summary>
       /// <param name="info">
        ///Operating System Details.
        /// 
        /// BootDevice - \Device\HarddiskVolume3
        /// BuildNumber - 9600
        /// BuildType - Multiprocessor Free
        /// Caption - Microsoft Windows 8.1 Pro
        /// CodeSet - 1252
        /// CountryCode - 1
        /// CreationClassName - Win32_OperatingSystem
        /// CSCreationClassName - Win32_ComputerSystem
        /// CSDVersion - Service Pack
        /// CSName - Name-PC
        /// CurrentTimeZone - -240
        /// DataExecutionPrevention_32BitApplications - True
        /// DataExecutionPrevention_Available - True
        /// DataExecutionPrevention_Drivers - True
        /// DataExecutionPrevention_SupportPolicy - 2
        /// Debug - False
        /// Description -
        /// Distributed - False
        /// EncryptionLevel - 256
        /// ForegroundApplicationBoost - 2
        /// FreePhysicalMemory - 6216788
        /// FreeSpaceInPagingFiles - 238576
        /// FreeVirtualMemory - 4432440
        /// InstallDate - 20131112234328.000000-300
        /// LargeSystemCache -
        /// LastBootUpTime - 20140502123847.491494-240
        /// LocalDateTime - 20140502170907.553000-240
        /// Locale - 0409
        /// Manufacturer - Microsoft Corporation
        /// MaxNumberOfProcesses - 4294967295
        /// MaxProcessMemorySize - 137438953344
        /// MUILanguages - System.String[]
        /// Name - Microsoft Windows 8.1 Pro|C:\WINDOWS|\Device\Harddisk1\Partition2
        /// NumberOfLicensedUsers -
        /// NumberOfProcesses - 115
        /// NumberOfUsers - 2
        /// OperatingSystemSKU - 48
        /// Organization -
        /// OSArchitecture - 64-bit
        /// OSLanguage - 1033
        /// OSProductSuite - 256
        /// OSType - 18
        /// OtherTypeDescription -
        /// PAEEnabled -
        /// PlusProductID -
        /// PlusVersionNumber -
        /// PortableOperatingSystem - False
        /// Primary - True
        /// ProductType - 1
        /// RegisteredUser - email@gmail.com
        /// SerialNumber - 12653-22520-01254-AAOEM
        /// ServicePackMajorVersion - 0
        /// ServicePackMinorVersion - 0
        /// SizeStoredInPagingFiles - 1081344
        /// Status - OK
        /// SuiteMask - 272
        /// SystemDevice - \Device\HarddiskVolume4
        /// SystemDirectory - C:\WINDOWS\system32
        /// SystemDrive - C:
        /// TotalSwapSpaceSize -
        /// TotalVirtualMemorySize - 17824748
        /// TotalVisibleMemorySize - 16743404
        /// Version - 6.3.9600
        /// WindowsDirectory - C:\WINDOWS
       /// </param>
       /// <returns>String with the value.</returns>
        public static string SystemDetails(string info)
        {
            string propertie = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_operatingsystem");
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
                Logger.Log("Error retriving System info details, parameter-{0}.", LogLevel.Error, info);
            }

            return propertie;
        }
 
        /// <summary>
        /// Gets the value for the desired value aka "win32_bios".
        /// </summary>
        /// <param name="info">
        ///Bios Details.
        /// 
        /// BiosCharacteristics - System.UInt16[]
        /// BIOSVersion - System.String[]
        /// BuildNumber -
        /// Caption - BIOS Date: 03/01/13 16:41:21 Ver: 04.06.05
        /// CodeSet -
        /// CurrentLanguage - en|US|iso8859-1
        /// Description - BIOS Date: 03/01/13 16:41:21 Ver: 04.06.05
        /// IdentificationCode -
        /// InstallableLanguages - 1
        /// InstallDate -
        /// LanguageEdition -
        /// ListOfLanguages - System.String[]
        /// Manufacturer - American Megatrends Inc.
        /// Name - BIOS Date: 03/01/13 16:41:21 Ver: 04.06.05
        /// OtherTargetOS -
        /// PrimaryBIOS - True
        /// ReleaseDate - 20130301000000.000000+000
        /// SerialNumber - To Be Filled By O.E.M.
        /// SMBIOSBIOSVersion - 4.6.5
        /// SMBIOSMajorVersion - 2
        /// SMBIOSMinorVersion - 7
        /// SMBIOSPresent - True
        /// SoftwareElementID - BIOS Date: 03/01/13 16:41:21 Ver: 04.06.05
        /// SoftwareElementState - 3
        /// Status - OK
        /// TargetOperatingSystem - 0
        /// Version - ALASKA - 1072009
        /// </param>
        /// <returns>String with the value from the specified parameter.</returns>
        public static string BiosDetails(string info)
        {
            string propertie = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_bios");
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
                Logger.Log("Error retriving Bios info details, parameter-{0}.", LogLevel.Error, info);
            }

            return propertie;
        }

        /// <summary>
        /// Gets the processor details aka "win32_processor".
        /// </summary>
        /// <param name="info">
        ///Processor Details.
        ///
        /// AddressWidth - 64
        /// Architecture - 9
        /// Availability - 3
        /// Caption - Intel64 Family 6 Model 42 Stepping 7
        /// ConfigManagerErrorCode -
        /// ConfigManagerUserConfig -
        /// CpuStatus - 1
        /// CreationClassName - Win32_Processor
        /// CurrentClockSpeed - 3501
        /// CurrentVoltage -
        /// DataWidth - 64
        /// Description - Intel64 Family 6 Model 42 Stepping 7
        /// DeviceID - CPU0
        /// ErrorCleared -
        /// ErrorDescription -
        /// ExtClock - 100
        /// Family - 198
        /// InstallDate -
        /// L2CacheSize - 1024
        /// L2CacheSpeed -
        /// L3CacheSize - 8192
        /// L3CacheSpeed - 0
        /// LastErrorCode -
        /// Level - 6
        /// LoadPercentage -
        /// Manufacturer - GenuineIntel
        /// MaxClockSpeed - 3501
        /// Name - Intel(R) Core(TM) i7-2700K CPU @ 3.50GHz
        /// NumberOfCores - 4
        /// NumberOfLogicalProcessors - 8
        /// OtherFamilyDescription -
        /// PNPDeviceID -
        /// PowerManagementCapabilities -
        /// PowerManagementSupported - False
        /// ProcessorId - BFEBFBFF000206A7
        /// ProcessorType - 3
        /// Revision - 10759
        /// Role - CPU
        /// SecondLevelAddressTranslationExtensions - True
        /// SocketDesignation - SOCKET 0
        /// Status - OK
        /// StatusInfo - 3
        /// Stepping -
        /// SystemCreationClassName - Win32_ComputerSystem
        /// SystemName - Name-PC
        /// UniqueId -
        /// UpgradeMethod - 36
        /// Version -
        /// VirtualizationFirmwareEnabled - True
        /// VMMonitorModeExtensions - True
        /// VoltageCaps - 3
        /// 
        /// </param>
        /// <returns>String with value from specified parameter.</returns>
        public static string ProcessorDetails(string info)
        {
            string propertie = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_processor");
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
                Logger.Log("Error retriving Processor info details, parameter-{0}.", LogLevel.Error, info);
            }
        

            return propertie;
        }

        /// <summary>
        /// Gets the video detaisl as per the win32_videocontroller.
        /// </summary>
        /// <param name="info">
        /// Video Controller properties.
        /// 
        /// AcceleratorCapabilities -
        /// AdapterCompatibility - Advanced Micro Devices, Inc.
        /// AdapterDACType - Internal DAC(400MHz)
        /// AdapterRAM - 3221225472
        /// Availability - 3
        /// CapabilityDescriptions -
        /// Caption - AMD Radeon HD 7900 Series
        /// ColorTableEntries -
        /// ConfigManagerErrorCode - 0
        /// ConfigManagerUserConfig - False
        /// CreationClassName - Win32_VideoController
        /// CurrentBitsPerPixel - 32
        /// CurrentHorizontalResolution - 1920
        /// CurrentNumberOfColors - 4294967296
        /// CurrentNumberOfColumns - 0
        /// CurrentNumberOfRows - 0
        /// CurrentRefreshRate - 60
        /// CurrentScanMode - 4
        /// CurrentVerticalResolution - 1080
        /// Description - AMD Radeon HD 7900 Series
        /// DeviceID - VideoController1
        /// DeviceSpecificPens -
        /// DitherType - 0
        /// DriverDate - 20140417000000.000000-000
        /// DriverVersion - 14.100.0.0
        /// ErrorCleared -
        /// ErrorDescription -
        /// ICMIntent -
        /// ICMMethod -
        /// InfFilename - oem73.inf
        /// InfSection - ati2mtag_R575
        /// InstallDate -
        /// InstalledDisplayDrivers - aticfx64.dll,aticfx64.dll,aticfx64.dll,aticfx32,x32,aticfx32,atiumd64.dll,atidxx64.dll,atidxx64.dll,atiumdag,atidxx32,atidxtiumdva,atiumd6a.cap,atitmm64.dll
        /// LastErrorCode -
        /// MaxMemorySupported -
        /// MaxNumberControlled -
        /// MaxRefreshRate - 60
        /// MinRefreshRate - 60
        /// Monochrome - False
        /// Name - AMD Radeon HD 7900 Series
        /// NumberOfColorPlanes -
        /// NumberOfVideoPages -
        /// PNPDeviceID - PCI\VEN_1002&DEV_6798&SUBSYS_044C1043&REV_00\4&3CF2B21&0&000
        /// PowerManagementCapabilities -
        /// PowerManagementSupported -
        /// ProtocolSupported -
        /// ReservedSystemPaletteEntries -
        /// SpecificationVersion -
        /// Status - OK
        /// StatusInfo -
        /// SystemCreationClassName - Win32_ComputerSystem
        /// SystemName - Name-PC
        /// SystemPaletteEntries -
        /// TimeOfLastReset -
        /// VideoArchitecture - 5
        /// VideoMemoryType - 2
        /// VideoMode -
        /// VideoModeDescription - 1920 x 1080 x 4294967296 colors
        /// VideoProcessor - AMD Radeon Graphics Processor (0x6798)
        /// </param>
        /// <returns>Return a string with value from the asked properie.</returns>
        public static string VideoDetails(string info)
        {
            string propertie = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
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
                Logger.Log("Error retriving Video info details, parameter-{0}.", LogLevel.Error, info);
            }

            return propertie;
        }

        /// <summary>
        /// Retrieves disk drives(HD) info ont he computer, as per "win32_diskdrive".
        /// It will generate a string with all the info avalable for each disk.
        /// Each info requested will be separed by "|".
        /// </summary>
        /// <param name="info">
        /// Disk Drive Details.
        /// Per Disk
        /// 
        /// Availability -
        /// BytesPerSector - 512
        /// Capabilities - System.UInt16[]
        /// CapabilityDescriptions - System.String[]
        /// Caption - Samsung SSD 840 PRO Series
        /// CompressionMethod -
        /// ConfigManagerErrorCode - 0
        /// ConfigManagerUserConfig - False
        /// CreationClassName - Win32_DiskDrive
        /// DefaultBlockSize -
        /// Description - Disk drive
        /// DeviceID - \\.\PHYSICALDRIVE3
        /// ErrorCleared -
        /// ErrorDescription -
        /// ErrorMethodology -
        /// FirmwareRevision - DXM05B0Q
        /// Index - 3
        /// InstallDate -
        /// InterfaceType - IDE
        /// LastErrorCode -
        /// Manufacturer - (Standard disk drives)
        /// MaxBlockSize -
        /// MaxMediaSize -
        /// MediaLoaded - True
        /// MediaType - Fixed hard disk media
        /// MinBlockSize -
        /// Model - Samsung SSD 840 PRO Series
        /// Name - \\.\PHYSICALDRIVE3
        /// NeedsCleaning -
        /// NumberOfMediaSupported -
        /// Partitions - 1
        /// PNPDeviceID - SCSI\DISK&VEN_SAMSUNG&PROD_SSD_840_PRO_SERI\7&ECD9645&0&0000 
        /// PowerManagementCapabilities -
        /// PowerManagementSupported -
        /// SCSIBus - 0
        /// SCSILogicalUnit - 0
        /// SCSIPort - 3
        /// SCSITargetId - 0
        /// SectorsPerTrack - 63
        /// SerialNumber - S1ATNEAD528912Z
        /// Signature - 1440165922
        /// Size - 256052966400
        /// Status - OK
        /// StatusInfo -
        /// SystemCreationClassName - Win32_ComputerSystem
        /// SystemName - Name-PC
        /// TotalCylinders - 31130
        /// TotalHeads - 255
        /// TotalSectors - 500103450
        /// TotalTracks - 7938150
        /// TracksPerCylinder - 255
        /// </param>
        /// <param name="size">Set to true if need to add size per disk to the string.</param>
        /// <returns>
        /// String with each disk info requested, seperated by "|".
        /// If size set to "true", string will include disk size per disk.
        ///</returns>
        public static string DiskDriveDetails(string info, bool size = false)
        {
            StringBuilder buildstring = new StringBuilder();
            string properties = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_diskdrive");
                ManagementObjectCollection osDetaislCollection = osDetails.Get();
                
                foreach (ManagementObject mo in osDetaislCollection)
                {
                    foreach (var pro in mo.Properties)
                    {
                        if (pro.Name == info)
                        {
                            buildstring.Append(pro.Value.ToString()).Append("|");
                            //if request for size if send true, it will get the size for available HD
                            if (size)
                            {
                                if (pro.Name == "Size")
                                    buildstring.Append(pro.Value.ToString()).Append("|");
                            }
                        }
                    }
                }
                properties = buildstring.ToString();
            }
            catch
            {
                Logger.Log("Error while gathering Disk Drive Details., parameter-{0}", LogLevel.Error, info);
            }
            
            return properties;
        }

        /// <summary>
        /// Retrives Physical Memory Details(RAM).
        /// It will generate a string with all info from all available bank.
        /// Per stick of ram(bank), installed on computer.
        /// String detail info will be seperated by "|".
        /// </summary>
        /// <param name="info">
        /// Physical Memory Details(RAM).
        /// Per bank installed.
        /// 
        /// BankLabel - BANK 0
        /// Capacity - 4294967296
        /// Caption - Physical Memory
        /// CreationClassName - Win32_PhysicalMemory
        /// DataWidth - 64
        /// Description - Physical Memory
        /// DeviceLocator - ChannelA-DIMM0
        /// FormFactor - 8
        /// HotSwappable -
        /// InstallDate -
        /// InterleaveDataDepth - 2
        /// InterleavePosition - 1
        /// Manufacturer - 04CD
        /// MemoryType - 0
        /// Model -
        /// Name - Physical Memory
        /// OtherIdentifyingInfo -
        /// PartNumber - F3-17000CL9-4GBXM
        /// PositionInRow -
        /// PoweredOn -
        /// Removable -
        /// Replaceable -
        /// SerialNumber - 00000000
        /// SKU -
        /// Speed - 2133
        /// Status -
        /// Tag - Physical Memory 0
        /// TotalWidth - 64
        /// TypeDetail - 128
        /// Version -
        /// </param>
        /// <param name="size">Set to true if need to add bank size per bank to string.</param>
        /// <returns>
        /// String with required details.
        /// If size == true, bank size will be include in the string.
        /// String details will be seperated by "|".
        /// </returns>
        public static string PhysicalMemoryDetails(string info, bool size = false)
        {

            StringBuilder buildstring = new StringBuilder();
            string properties = null;
            try
            {
                ManagementObjectSearcher osDetails = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                ManagementObjectCollection osDetaislCollection = osDetails.Get();

                foreach (ManagementObject mo in osDetaislCollection)
                {
                    foreach (var pro in mo.Properties)
                    {
                        if (pro.Name == info)
                        {
                            buildstring.Append(pro.Value.ToString()).Append("|");
                            //if request for size if send true, it will get the size for each bank.
                            if (size)
                            {
                                if (pro.Name == "Capacity")
                                    buildstring.Append(pro.Value.ToString()).Append("|");
                            }
                        }
                    }
                }
                properties = buildstring.ToString();
            }
            catch
            {
                Logger.Log("Error while gathering Physical Memory Details, parameter-{0}.", LogLevel.Error, info);
            }

            return properties;  

        }

    }
}
