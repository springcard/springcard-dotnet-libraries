using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows
{
	public static class USB
	{
        private static Logger logger = new Logger(typeof(USB).FullName);

        public const ushort SpringCard_VendorId = 0x1C34;
        public const string SpringCard_VendorName = "SpringCard";
        public const string WmiWhereUsb = @"PNPDeviceID LIKE '%USB%VID%&PID%'";

        private static Dictionary<string, string> GetWmiDeviceTree()
        {
            List<string> parents = new List<string>();
            Dictionary<string, string> result = new Dictionary<string, string>();

            logger.debug("USB:GetDevicesTree");

            try
            {
                ManagementObjectCollection collection = null;

                string selectString = @"SELECT * From Win32_USBControllerDevice";

                if (WinUtils.Debug)
                    logger.debug("WMI:{0}", selectString);

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectString))
                    collection = searcher.Get();

                if (collection != null)
                {
                    foreach (ManagementObject mo in collection)
                    {
                        string parentId = null;
                        string deviceId = (string)mo.GetPropertyValue("Dependent");

                        if (WinUtils.Debug)
                            logger.debug("\t{0}", deviceId);

                        string[] e;
                        e = deviceId.Split('=');
                        deviceId = e[1];
                        deviceId = deviceId.Replace("\\\\", "\\");
                        deviceId = deviceId.Replace("\"", "");

                        if (deviceId.Contains("&MI_"))
                        {
                            string parentPattern = deviceId.Substring(0, deviceId.IndexOf("&MI_")) + "\\";
                            for (int i= parents.Count-1; i>0; i--)
                            {
                                if (parents[i].StartsWith(parentPattern))
                                {
                                    parentId = parents[i];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            parents.Add(deviceId);
                        }

                        result[deviceId] = parentId;
                    }
                    collection.Dispose();
                }
                else
                {
                    logger.debug("USB:Empty");
                }
            }
            catch (Exception e)
            {
                logger.warning("USB:" + e.Message);
            }

            return result;
        }

        public class DeviceInfo : WMI.DeviceInfo
        {
            public string FriendlyName { get; private set; }
            public string Mode { get; private set; }
            public string Type { get; private set; } = "";
            public string DevicePath { get; private set; } = "";
            public string LocationInfo { get; protected set; } = "";
            public string LocationPath { get; protected set; } = "";
            public string[] HardwareIds { get; private set; }
            public string[] CompatibleIds { get; private set; }
            public string ParentPnpDeviceId { get; private set; }
            public string DriverKeyName { get; private set; }
            public string FriendlyLocationPath { get; private set; }
            public string PhysicalObjectName { get; private set; }
            public uint Address { get; private set; }
            public uint BusNumber { get; private set; }
            public string ClassName { get; private set; }
            public string ClassGuid { get; private set; }
            public bool IsHub { get; private set; }

            public string VendorId
            {
                get
                {
                    string s = PnpDeviceId.ToUpper();
                    if (s.Contains("USB") && s.Contains("VID_"))
                    {
                        s = s.Substring(s.IndexOf("VID_") + 4, 4);
                        return s;
                    }
                    s = HardwareId.ToUpper();
                    if (s.Contains("USB") && s.Contains("VID_"))
                    {
                        s = s.Substring(s.IndexOf("VID_") + 4, 4);
                        return s;
                    }
                    return "";
                }
            }

            public string ProductId
            {
                get
                {
                    string s = PnpDeviceId.ToUpper();
                    if (s.Contains("USB") && s.Contains("PID_"))
                    {
                        s = s.Substring(s.IndexOf("PID_") + 4, 4);
                        return s;
                    }
                    s = HardwareId.ToUpper();
                    if (s.Contains("USB") && s.Contains("PID_"))
                    {
                        s = s.Substring(s.IndexOf("PID_") + 4, 4);
                        return s;
                    }
                    return "";
                }
            }

            public string VersionId
            {
                get
                {
                    string s = PnpDeviceId.ToUpper();
                    if (s.Contains("USB") && s.Contains("REV_"))
                    {
                        s = s.Substring(s.IndexOf("REV_") + 4, 4);
                        return s;
                    }
                    s = HardwareId.ToUpper();
                    if (s.Contains("USB") && s.Contains("REV_"))
                    {
                        s = s.Substring(s.IndexOf("REV_") + 4, 4);
                        return s;
                    }
                    return "";
                }
            }

            public string Version
            {
                get
                {
                    string t = VersionId;
                    if (t.Length == 4)
                    {
                        t = t.Substring(0, 2) + "." + t.Substring(2, 2);
                        if (t.StartsWith("0")) t = t.Substring(1);
                    }
                    return t;
                }
            }

            public ushort wVendorId
            {
                get
                {
                    string t = VendorId;
                    if (string.IsNullOrEmpty(t))
                        return 0;
                    return BinConvert.ParseHexW(t);
                }
            }

            public ushort wProductId
            {
                get
                {
                    string t = ProductId;
                    if (string.IsNullOrEmpty(t))
                       return 0;
                    return BinConvert.ParseHexW(t);
                }
            }

            public string SerialNumber
            {
                get
                {
                    string[] s = PnpDeviceId.ToUpper().Split('\\');
                    if (s.Length > 2)
                    {
                        return s[s.Length - 1];
                    }
                    return "";
                }
            }

            public byte[] abSerialNumber
            {
                get
                {
                    byte[] result = BinConvert.TryHexToBytes(SerialNumber);
                    if (result == null)
                        result = new byte[0];
                    return result;
                }
            }

            private void RecognizeDevice()
            {
                FriendlyName = Caption;

                if (wVendorId == SpringCard_VendorId)
                {
                    switch (wProductId & 0xFF00)
                    {
                        case 0x6000:
                            /* SpringCore ? */
                            FriendlyName = "SpringCore";
                            switch (wProductId & 0x000F)
                            {
                                case 0x0000:
                                case 0x0007:
                                    Mode = "Direct";
                                    break;
                                case 0x0001:
                                case 0x0004:
                                    Mode = "VCP";
                                    break;
                                case 0x0002:
                                case 0x000A:
                                    Mode = "PC/SC";
                                    break;
                                case 0x0003:
                                    Mode = "RFID Scanner";
                                    break;
                                default:
                                    Mode = "Unknown";
                                    break;
                            }
                            break;

                        case 0x6100:
                            /* SpringCore'18 generation */
                            switch (wProductId & 0x00F0)
                            {
                                case 0x0010:
                                    FriendlyName = "E518";
                                    break;
                                case 0x0020:
                                    FriendlyName = "H518";
                                    break;
                                case 0x0030:
                                    FriendlyName = "Puck";
                                    break;
                                default:
                                    FriendlyName = "SpringCore";
                                    break;
                            }
                            switch (wProductId & 0x000F)
                            {
                                case 0x0000:
                                case 0x0007:
                                    Mode = "Direct";
                                    break;
                                case 0x0001:
                                case 0x0004:
                                    Mode = "VCP";
                                    break;
                                case 0x0002:
                                case 0x000A:
                                    Mode = "PC/SC";
                                    break;
                                case 0x0003:
                                    Mode = "RFID Scanner";
                                    break;
                                default:
                                    Mode = "Unknown";
                                    break;
                            }
                            break;

                        case 0x7000:
                            FriendlyName = "Legacy device";
                            break;

                        case 0x7100:
                            FriendlyName = "CSB6";
                            break;

                        case 0x7200:
                            FriendlyName = "CSB6/RDR";
                            break;

                        case 0x9100:
                            switch (wProductId)
                            {
                                case 0x7241:
                                    FriendlyName = "Prox'N'Roll RFID Scanner";
                                    break;
                                case 0x9241:
                                    FriendlyName = "Prox'N'Roll RFID Scanner HSP";
                                    break;
                                default:
                                    FriendlyName = "H663";
                                    break;
                            }
                            switch (wProductId & 0x0F00)
                            {
                                case 0x0000:
                                    Mode = "VCP";
                                    break;
                                case 0x0100:
                                    Mode = "PC/SC";
                                    break;
                                case 0x0200:
                                    Mode = "RFID Scanner";
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case 0x9200:
                            FriendlyName = "H663/RDR";
                            break;

                        case 0xA100:
                            FriendlyName = "H512";
                            break;

                        case 0xAB00:
                            switch ((wProductId) & 0xFFF0)
                            {
                                case 0xABC0:
                                    FriendlyName = "ABC Smartcard";
                                    Mode = "PC/SC";
                                    break;
                                case 0xABD0:
                                    FriendlyName = "Socket Mobile";
                                    break;
                            }
                            break;

                        case 0xAF00:
                            switch (wProductId & 0x00F0)
                            {
                                /* AFCare/Doctolib device */
                                case 0x00C0:
                                    Mode = "PC/SC";
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }
                else if ((wVendorId == 0x03EB) && (wProductId == 0x2FF6))
                {
                    Mode = "Atmel DFU";
                }
                else if (wVendorId == 0x0403)
                {
                    /* FTDI */
                    switch (wProductId)
                    {
                        case 0xD968:
                            FriendlyName = SpringCard_VendorName + " IUSB-XXX";
                            break;
                        case 0xD969:
                            FriendlyName = SpringCard_VendorName + " CSB4U";
                            break;
                        case 0xD96A:
                            FriendlyName = SpringCard_VendorName + " CSB4U/RDR";
                            break;
                        default:
                            FriendlyName = SpringCard_VendorName + " USB to serial interface";
                            break;
                    }
                }


                if (wVendorId == SpringCard_VendorId)
                    if (!FriendlyName.ToLower().Contains(SpringCard_VendorName.ToLower()))
                        FriendlyName = string.Format("{0} {1}", SpringCard_VendorName, FriendlyName);

                if (!string.IsNullOrEmpty(Mode))
                    if (!FriendlyName.ToLower().Contains(Mode.ToLower()))
                        FriendlyName = string.Format("{0} ({1})", FriendlyName, Mode);
            }

            public override string ToString()
            {
                return string.Format("{0} \"{1}\"", PnpDeviceId, FriendlyName);
            }

            private SETUPAPI.DeviceInfo setupapiDeviceInfo;

            public DeviceInfo(string ParentPnpDeviceId, WMI.DeviceInfo wmiDeviceInfo, SETUPAPI.DeviceInfo setupapiDeviceInfo) : base(wmiDeviceInfo.GetManagementObject())
            {
                this.ParentPnpDeviceId = ParentPnpDeviceId;
                this.setupapiDeviceInfo = setupapiDeviceInfo;

                if (setupapiDeviceInfo != null)
                {
                    DevicePath = setupapiDeviceInfo.DevicePath;
                    DriverKeyName = setupapiDeviceInfo.DriverKeyName;
                    LocationInfo = setupapiDeviceInfo.LocationInfo;
                    LocationPath = setupapiDeviceInfo.LocationPath;
                    FriendlyLocationPath = setupapiDeviceInfo.FriendlyLocationPath;
                    PhysicalObjectName = setupapiDeviceInfo.PhysicalObjectName;
                    Address = setupapiDeviceInfo.Address;
                    BusNumber = setupapiDeviceInfo.BusNumber;
                    ClassName = setupapiDeviceInfo.ClassName;
                    ClassGuid = setupapiDeviceInfo.ClassGuid;
                    HardwareIds = setupapiDeviceInfo.HardwareIds;
                    CompatibleIds = setupapiDeviceInfo.CompatibleIds;

                    if (setupapiDeviceInfo.UsbHubInfo != null)
                        IsHub = true;
                    if (!string.IsNullOrEmpty(setupapiDeviceInfo.Service) && setupapiDeviceInfo.Service.ToLower().Contains("usbhub"))
                        IsHub = true;
                }

                RecognizeDevice();
            }
        }

        public static List<DeviceInfo> EnumDevices()
        {
            return InternalEnum(false, true);
        }

        public static List<DeviceInfo> EnumHubsAndDevices()
        {
            return InternalEnum(true, true);
        }

        private static List<DeviceInfo> InternalEnum(bool hubs, bool devices)
        {
            logger.debug("USB:EnumDevices");

            List<DeviceInfo> result = new List<DeviceInfo>();

            List<SETUPAPI.DeviceInfo> setupApiDeviceList = null;
            try
            {
                if (hubs && devices)
                {
                    setupApiDeviceList = SETUPAPI.EnumUsbHubsAndDevices();
                }
                else if (hubs && !devices)
                {
                    setupApiDeviceList = SETUPAPI.EnumUsbHubs();
                }
                else if (!hubs && devices)
                {
                    setupApiDeviceList = SETUPAPI.EnumUsbDevices();
                }
            }
            catch { }

            Dictionary<string, string> wmiDeviceTree = GetWmiDeviceTree();

            try
            {
                ManagementObjectCollection collection = null;

                string selectString = @"SELECT * From Win32_PnPEntity WHERE PNPDeviceID LIKE '%USB%VID%&PID%'";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectString))
                    collection = searcher.Get();

                if (collection != null)
                {
                    foreach (ManagementObject managementObject in collection)
                    {
                        string deviceId = (string)managementObject.GetPropertyValue("PNPDeviceID");

                        if (!wmiDeviceTree.ContainsKey(deviceId))
                            continue;

                        string parentDeviceId = wmiDeviceTree[deviceId];

                        WMI.DeviceInfo wmiDeviceInfo = new WMI.DeviceInfo(managementObject);

                        SETUPAPI.DeviceInfo setupapiDeviceInfo = null;

                        if (setupApiDeviceList != null)
                        {
                            foreach (SETUPAPI.DeviceInfo setupApiDeviceWalker in setupApiDeviceList)
                            {
                                if (string.Compare(wmiDeviceInfo.DeviceId, setupApiDeviceWalker.DeviceId) == 0)
                                {
                                    setupapiDeviceInfo = setupApiDeviceWalker;
                                    break;
                                }
                            }

                            if (setupapiDeviceInfo == null)
                            {
                                foreach (SETUPAPI.DeviceInfo setupApiDeviceWalker in setupApiDeviceList)
                                {
                                    if (string.Compare(parentDeviceId, setupApiDeviceWalker.DeviceId) == 0)
                                    {
                                        setupapiDeviceInfo = setupApiDeviceWalker;
                                        break;
                                    }
                                }
                            }
                        }

                        if (wmiDeviceInfo != null)
                        {
                            if (setupapiDeviceInfo == null)
                            {
                                bool hidden = false;
                                if (wmiDeviceInfo.HardwareId.Contains("ROOT_DEVICE_ROUTER"))
                                    hidden = true;
                                if (wmiDeviceInfo.Service.Contains("USBHUB"))
                                    hidden = true;

                                if (!hidden)
                                {
                                    logger.warning("USB device {0} / WMI device {1} has not been found by SETUPAPI ({2})", deviceId, parentDeviceId, wmiDeviceInfo.Description);
                                    logger.debug("\tHardwareId: {0}", wmiDeviceInfo.HardwareId);
                                    logger.debug("\tService: {0}", wmiDeviceInfo.Service);
                                }
                            }

                            DeviceInfo usbDeviceInfo = new DeviceInfo(parentDeviceId, wmiDeviceInfo, setupapiDeviceInfo);
                            result.Add(usbDeviceInfo);
                        }
                    }
                    collection.Dispose();
                }
            }
            catch { }

            return result;
        }

        [Flags]
        public enum EnumDeviceFilterE : UInt64
        {
            SpringCard  = 0x0001000000000,
            FTDISerial  = 0x0010000000000,
            AtmelDFU    = 0x0020000000000,
            SpringCardHID = 0x0000010000000,
            SpringCardSerial = 0x0000020000000,
            CSB6Family  = 0x0000000000001,
            H512Family  = 0x0000000000002,
            H663Family  = 0x0000000000004,
        }

        public static bool IsSpringCardRFIDScanner(ushort vid, ushort pid)
        {
            if (vid == SpringCard_VendorId)
            {
                switch (pid)
                {
                    case 0x7241:
                    case 0x9241:
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }

        public static List<DeviceInfo> EnumDevices(EnumDeviceFilterE filter)
        {
            List<DeviceInfo> deviceInfoList = EnumDevices();

            List<DeviceInfo> result = new List<DeviceInfo>();

            foreach (DeviceInfo deviceInfo in deviceInfoList)
            {
                bool match = false;

                if ((filter & EnumDeviceFilterE.SpringCard) != 0)
                {
                    if (deviceInfo.wVendorId == SpringCard_VendorId)
                        match = true;
                }
                if ((filter & EnumDeviceFilterE.FTDISerial) != 0)
                {
                    if (deviceInfo.Service.ToUpper() == "FTDIBUS")
                        match = true;
                }
                if ((filter & EnumDeviceFilterE.AtmelDFU) != 0)
                {
                    if (deviceInfo.Mode == "Atmel DFU")
                        match = true;
                }
                if ((filter & EnumDeviceFilterE.SpringCardHID) != 0)
                {
                    if (deviceInfo.Mode.Contains("RFID Scanner"))
                        match = true;
                }
                if ((filter & EnumDeviceFilterE.SpringCardSerial) != 0)
                {
                    if (deviceInfo.wVendorId == SpringCard_VendorId)
                        if (deviceInfo.Service.ToLower() == "usbser")
                            match = true;
                }
                if ((filter & EnumDeviceFilterE.CSB6Family) != 0)
                {
                    if (deviceInfo.wVendorId == SpringCard_VendorId)
                        if ((deviceInfo.wProductId & 0xF000) == 0x7000)
                            match = true;
                }
                if ((filter & EnumDeviceFilterE.H512Family) != 0)
                {
                    if (deviceInfo.wVendorId == SpringCard_VendorId)
                        if ((deviceInfo.wProductId & 0xF000) == 0xA000)
                            match = true;
                }
                if ((filter & EnumDeviceFilterE.H663Family) != 0)
                {
                    if (deviceInfo.wVendorId == SpringCard_VendorId)
                        if ((deviceInfo.wProductId & 0xF000) == 0x9000)
                            match = true;
                }

                if (match)
                    result.Add(deviceInfo);
            }

            return result;
        }

        public static List<DeviceInfo> EnumDevices_RFIDScanners()
		{
            return EnumDevices(EnumDeviceFilterE.SpringCardHID);
		}
		
		public static List<DeviceInfo> EnumDevices_DFU()
		{
            return EnumDevices(EnumDeviceFilterE.AtmelDFU);
		}

		public static List<DeviceInfo> EnumDevices_H_Group(bool IncludeDFU = false, bool IncludeCSB6 = false)
		{
            EnumDeviceFilterE filter = EnumDeviceFilterE.H663Family | EnumDeviceFilterE.H512Family;
            if (IncludeDFU)
                filter |= EnumDeviceFilterE.AtmelDFU;
            if (IncludeCSB6)
                filter |= EnumDeviceFilterE.CSB6Family;
            return EnumDevices(filter);
		}

        public static List<DeviceInfo> EnumDevices_Serial()
        {
            return EnumDevices(EnumDeviceFilterE.FTDISerial | EnumDeviceFilterE.SpringCardSerial);
        }

        public static string GetAssociatedCommName(DeviceInfo deviceInfo)
        {
            return GetAssociatedCommName(SERIAL.EnumCommPorts(), deviceInfo);
        }

        public static string GetAssociatedCommName(List<SERIAL.DeviceInfo> CommPortList, DeviceInfo deviceInfo)
        {
            if (CommPortList == null)
                return null;
            if (deviceInfo == null)
                return null;

            string FtdiLookup = null;

            if (deviceInfo.Service == "FTDIBUS")
            {
                try
                {
                    string[] e;
                    FtdiLookup = deviceInfo.PnpDeviceId;
                    FtdiLookup = FtdiLookup.Replace("USB", "FTDIBUS");
                    e = FtdiLookup.Split(new char[] { '&' }, 2);
                    FtdiLookup = e[0] + "+" + e[1];
                    e = FtdiLookup.Split(new char[] { '\\' }, 3);
                    FtdiLookup = e[0] + "\\" + e[1] + "+" + e[2];
                }
                catch
                {
                    FtdiLookup = null;
                }
            }

            foreach (SERIAL.DeviceInfo commPort in CommPortList)
            {
                if (commPort.PnpDeviceId == deviceInfo.PnpDeviceId)
                {
                    if (WinUtils.Debug)
                        logger.debug("Device {0} is a comm. port: {1}", deviceInfo.DeviceId, commPort.PortName);
                    return commPort.PortName;
                }

                if (FtdiLookup != null)
                {
                    if (commPort.PnpDeviceId.StartsWith(FtdiLookup))
                    {
                        if (WinUtils.Debug)
                            logger.debug("Device {0} is a FTDI comm. port: {1}", deviceInfo.DeviceId, commPort.PortName);
                        return commPort.PortName;
                    }
                }
            }

            return null;
        }

        public static string GetAssociatedDiskName(DeviceInfo deviceInfo)
        {
            return GetAssociatedDiskName(DISK.EnumUsbDisks(), deviceInfo);
        }

        public static string GetAssociatedDiskName(List<DISK.DeviceInfo> DiskList, DeviceInfo deviceInfo)
        {
            foreach (DISK.DeviceInfo disk in DiskList)
            {
                if (!string.IsNullOrEmpty(deviceInfo.SerialNumber))
                {
                    if (!string.IsNullOrEmpty(disk.PnpDeviceId))
                    {
                        string s = disk.PnpDeviceId.ToUpper();
                        string[] e = s.Split('\\');
                        if (e.Length > 1)
                        {
                            s = e[e.Length - 1];
                            e = s.Split('&');
                            s = e[0];

                            if (deviceInfo.SerialNumber.ToUpper().Equals(s))
                            {
                                string diskName = disk.GetDiskName();
                                if (!string.IsNullOrEmpty(diskName))
                                {
                                    if (WinUtils.Debug)
                                        logger.debug("Device {0} is a disk drive: {1}", deviceInfo.DeviceId, diskName);
                                    return diskName;
                                }
                                else
                                {
                                    if (WinUtils.Debug)
                                        logger.debug("Device {0} is an unattached disk drive");
                                    return null;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public class TreeNode
        {
            public string LocationInfo { get; internal set; }
            public string FriendlyLocationPath { get; internal set; }
            public string Description { get; internal set; }
            public TreeNode Parent { get; internal set; }
            internal int HubNumber = -1;
            public int ChildNumber { get; internal set; }
            public TreeNode[] Children { get; internal set; }
            public DeviceInfo DeviceInfo { get; internal set; }
        }

        private static TreeNode ReadSetupUsbTree(SETUPAPI.DeviceInfo setupapiDeviceInfo, List<DeviceInfo> deviceList, TreeNode Parent = null, int ChildNumber = 0)
        {
            TreeNode result = new TreeNode();

            if (Parent != null)
            {
                result.Parent = Parent;
                result.ChildNumber = ChildNumber;
                Parent.Children[ChildNumber - 1] = result;
                if (string.IsNullOrEmpty(Parent.FriendlyLocationPath))
                    result.FriendlyLocationPath = Path.Combine("USB", string.Format("#{0:D04}", ChildNumber));
                else
                    result.FriendlyLocationPath = Path.Combine(Parent.FriendlyLocationPath, string.Format("#{0:D04}", ChildNumber));

                /* Create a fake location info... May be overriden later by the device's */
                if (Parent.HubNumber >= 0)
                    result.LocationInfo = string.Format("Port_#{0:D04}.Hub_#{1:D04}", ChildNumber, Parent.HubNumber);
            }

            if (setupapiDeviceInfo != null)
            {
                /* We must have a description from SetupApi */
                result.LocationInfo = setupapiDeviceInfo.LocationInfo;
                result.Description = setupapiDeviceInfo.Description;

                if (setupapiDeviceInfo.UsbHubInfo != null)
                {
                    /* This is a hub */
                    /* ------------- */

                    result.HubNumber = setupapiDeviceInfo.UsbHubInfo.HubNumber;
                    result.Children = new TreeNode[setupapiDeviceInfo.UsbHubInfo.NumberOfPorts];
                    for (int i = 1; i <= setupapiDeviceInfo.UsbHubInfo.NumberOfPorts; i++)
                    {
                        if (!setupapiDeviceInfo.UsbHubInfo.Ports.ContainsKey(i))
                        {
                            logger.error("Hub {0} does not have a child port at index {1}", setupapiDeviceInfo.ToString(), i);
                            throw new Exception("Internal error USB / SETUPAPI");
                        }
                        result.Children[i - 1] = ReadSetupUsbTree(setupapiDeviceInfo.UsbHubInfo.Ports[i], deviceList, result, i);
                    }

                    /* Find the device associated to the hub in the list provided by WMI */
                    foreach (DeviceInfo deviceInfoWalker in deviceList)
                    {
                        if (string.Compare(deviceInfoWalker.FriendlyLocationPath, setupapiDeviceInfo.FriendlyLocationPath) == 0)
                        {
                            result.DeviceInfo = deviceInfoWalker;
                            if (WinUtils.Debug)
                                logger.debug("SETUPAPI USB hub \"{0}\" at {1} {2} is {3}", setupapiDeviceInfo.Description, setupapiDeviceInfo.FriendlyLocationPath, setupapiDeviceInfo.LocationInfo, result.DeviceInfo.ToString());
                            break;
                        }
                    }

                    if (result.DeviceInfo == null)
                    {
                        if (!setupapiDeviceInfo.UsbHubInfo.IsRoot)
                            if (!string.IsNullOrEmpty(setupapiDeviceInfo.LocationInfo))
                                logger.warning("No USB device found for SETUPAPI USB hub \"{0}\" at {1} [{2}]", setupapiDeviceInfo.Description, setupapiDeviceInfo.LocationInfo, setupapiDeviceInfo.LocationPath);
                    }
                }
                else
                {
                    /* This is a device */
                    /* ---------------- */

                    /* Find the device in the list provided by WMI */
                    foreach (DeviceInfo deviceInfoWalker in deviceList)
                    {
                        if (string.Compare(deviceInfoWalker.FriendlyLocationPath, setupapiDeviceInfo.FriendlyLocationPath) == 0)
                        {
                            result.DeviceInfo = deviceInfoWalker;
                            if (WinUtils.Debug)
                                logger.debug("SETUPAPI USB device \"{0}\" at {1} {2} is {3}", setupapiDeviceInfo.Description, setupapiDeviceInfo.FriendlyLocationPath, setupapiDeviceInfo.LocationInfo, result.DeviceInfo.ToString());
                            break;
                        }
                    }

                    if (result.DeviceInfo == null)
                    {
                        logger.warning("No USB device found for SETUPAPI USB device \"{0}\" at {1} [{2}]", setupapiDeviceInfo.Description, setupapiDeviceInfo.LocationInfo, setupapiDeviceInfo.LocationPath);
                    }
                }
            }

            return result;
        }

        public static TreeNode GetHubTree()
        {
            /* Get the USB tree from the SetupApi */
            List<SETUPAPI.DeviceInfo> setupUsbTree = SETUPAPI.BuildUsbTree();

            if ((setupUsbTree == null) || (setupUsbTree.Count == 0))
                return null;

            /* Get the list of USB devices */
            List<DeviceInfo> deviceList = InternalEnum(true, false);

            TreeNode result;
            if (setupUsbTree.Count == 1)
            {
                result = ReadSetupUsbTree(setupUsbTree[0], deviceList);
            }
            else
            {
                result = new TreeNode();
                result.Children = new TreeNode[setupUsbTree.Count];
                for (int i = 1; i <= setupUsbTree.Count; i++)
                {
                    result.Children[i - 1] = ReadSetupUsbTree(setupUsbTree[i - 1], deviceList, result, i);
                }
            }
            return result;
        }

        public static TreeNode GetTree()
        {
            /* Get the USB tree from the SetupApi */
            List<SETUPAPI.DeviceInfo> setupUsbTree = SETUPAPI.BuildUsbTree();

            if ((setupUsbTree == null) || (setupUsbTree.Count == 0))
                return null;

            /* Get the list of USB devices */
            List<DeviceInfo> deviceList = InternalEnum(true, true);

            TreeNode result;
            if (setupUsbTree.Count == 1)
            {
                result = ReadSetupUsbTree(setupUsbTree[0], deviceList);
            }
            else
            {
                result = new TreeNode();
                result.FriendlyLocationPath = "USB";
                result.Description = "USB Tree";
                result.Children = new TreeNode[setupUsbTree.Count];
                for (int i = 1; i <= setupUsbTree.Count; i++)
                    result.Children[i - 1] = ReadSetupUsbTree(setupUsbTree[i - 1], deviceList, result, i);
            }
            return result;
        }

        private static void Walk(Dictionary<string, DeviceInfo> result, TreeNode node)
        {
            if (!string.IsNullOrEmpty(node.FriendlyLocationPath))
                result[node.FriendlyLocationPath] = node.DeviceInfo;
            if (node.Children != null)
                foreach (TreeNode child in node.Children)
                    Walk(result, child);
        }

        public static Dictionary<string, DeviceInfo> GetBasicTree()
        {
            TreeNode rootNode = GetTree();

            Dictionary<string, DeviceInfo> result = new Dictionary<string, DeviceInfo>();

            Walk(result, rootNode);

            return result;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct DevBroadcastDeviceinterface
        {
            internal int Size;
            internal int DeviceType;
            internal int Reserved;
            internal Guid ClassGuid;
            internal short Name;
        }

        private const int DBT_DEVICE_ARRIVAL = 0x8000; // system detected a new device        
        private const int DBT_DEVICE_REMOVE_COMPLETE = 0x8004; // device is gone      
        private const int WM_DEVICE_CHANGE = 0x0219; // device change event      
        private const int DBT_DEVICE_TYPE_DEVICE_INTERFACE = 5;

        public static IntPtr RegisterPNPNotifications(IntPtr windowHandle)
        {
            DevBroadcastDeviceinterface dbi = new DevBroadcastDeviceinterface
            {
                DeviceType = DBT_DEVICE_TYPE_DEVICE_INTERFACE,
                Reserved = 0,
                ClassGuid = SETUPAPI.GUID_DEVINTERFACE_USB_DEVICE,
                Name = 0
            };

            dbi.Size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.Size);
            Marshal.StructureToPtr(dbi, buffer, true);

            return RegisterDeviceNotification(windowHandle, buffer, 0);
        }

        public static void UnregisterPNPNotifications(IntPtr notificationHandle)
        {
            UnregisterDeviceNotification(notificationHandle);
        }

        public static bool IsMessagePNPNotification(ref Message m, out bool isInsert, out bool isRemove)
        {
            isInsert = false;
            isRemove = false;
            if (m.Msg == WM_DEVICE_CHANGE)
            {
                switch ((int)m.WParam)
                {
                    case DBT_DEVICE_REMOVE_COMPLETE:
                        isInsert = true;
                        return true;
                    case DBT_DEVICE_ARRIVAL:
                        isRemove = true;
                        return true;
                }
            }
            return false;
        }
    }
}
