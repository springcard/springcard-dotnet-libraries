using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;

namespace SpringCard.LibCs.Windows
{
	public static class USB
	{
        private static Logger logger = new Logger(typeof(USB).FullName);

        public const ushort SpringCard_VendorIDw = 0x1C34;
        public const string WmiWhereUsb = @"PNPDeviceID LIKE '%USB%VID%&PID%'";

        private static Dictionary<string, string> GetDevicesTree()
        {
            List<string> parents = new List<string>();
            Dictionary<string, string> result = new Dictionary<string, string>();

            try
            {
                ManagementObjectCollection collection = null;

                string selectString = @"SELECT * From Win32_USBControllerDevice";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectString))
                    collection = searcher.Get();

                if (collection != null)
                {
                    foreach (ManagementObject mo in collection)
                    {
                        string parentId = null;
                        string deviceId = (string)mo.GetPropertyValue("Dependent");

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
            }
            catch { }

            return result;
        }

        public static List<WMI.DeviceInfo> EnumDevices()
        {
            List<WMI.DeviceInfo> result = new List<WMI.DeviceInfo>();

            Dictionary<string, string> deviceTree = GetDevicesTree();

            try
            {
                ManagementObjectCollection collection = null;

                string selectString = @"SELECT * From Win32_PnPEntity WHERE PNPDeviceID LIKE '%USB%VID%&PID%'";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectString))
                    collection = searcher.Get();

                if (collection != null)
                {
                    foreach (ManagementObject mo in collection)
                    {
                        string deviceId = (string)mo.GetPropertyValue("PNPDeviceID");

                        if (!deviceTree.ContainsKey(deviceId))
                            continue;

                        string parentDeviceId = deviceTree[deviceId];

                        WMI.DeviceInfo di = new WMI.DeviceInfo(mo, parentDeviceId);

                        result.Add(di);
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
            if (vid == SpringCard_VendorIDw)
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

        public static List<WMI.DeviceInfo> EnumDevices(EnumDeviceFilterE filter)
        {
            return WMI.EnumDevices(WMI.WmiDeviceTable, WmiWhereUsb, delegate (WMI.DeviceInfo device) {
                bool match = false;

                if ((filter & EnumDeviceFilterE.SpringCard) != 0)
                {
                    if (device.wVendorID == SpringCard_VendorIDw)
                        match = true;
                }
                if ((filter & EnumDeviceFilterE.FTDISerial) != 0)
                {
                    if (device.Service.ToUpper() == "FTDIBUS")
                        match = true;
                }
                if ((filter & EnumDeviceFilterE.AtmelDFU) != 0)
                {
                    if (device.Type == "Atmel DFU")
                        match = true;
                }
                if ((filter & EnumDeviceFilterE.SpringCardHID) != 0)
                {
                    if (device.Type.Contains("RFID Scanner"))
                        match = true;
                }
                if ((filter & EnumDeviceFilterE.SpringCardSerial) != 0)
                {
                    if (device.wVendorID == SpringCard_VendorIDw)
                        if (device.Service.ToLower() == "usbser")
                            match = true;
                }
                if ((filter & EnumDeviceFilterE.CSB6Family) != 0)
                {
                    if (device.wVendorID == SpringCard_VendorIDw)
                        if ((device.wProductID & 0xF000) == 0x7000)
                            match = true;
                }
                if ((filter & EnumDeviceFilterE.H512Family) != 0)
                {
                    if (device.wVendorID == SpringCard_VendorIDw)
                        if ((device.wProductID & 0xF000) == 0xA000)
                            match = true;
                }
                if ((filter & EnumDeviceFilterE.H663Family) != 0)
                {
                    if (device.wVendorID == SpringCard_VendorIDw)
                        if ((device.wProductID & 0xF000) == 0x9000)
                            match = true;
                }

                return match;
            });
        }

        public static List<WMI.DeviceInfo> EnumDevices_RFIDScanners()
		{
            return EnumDevices(EnumDeviceFilterE.SpringCardHID);
		}
		
		public static List<WMI.DeviceInfo> EnumDevices_DFU()
		{
            return EnumDevices(EnumDeviceFilterE.AtmelDFU);
		}

		public static List<WMI.DeviceInfo> EnumDevices_H_Group(bool IncludeDFU = false, bool IncludeCSB6 = false)
		{
            EnumDeviceFilterE filter = EnumDeviceFilterE.H663Family | EnumDeviceFilterE.H512Family;
            if (IncludeDFU)
                filter |= EnumDeviceFilterE.AtmelDFU;
            if (IncludeCSB6)
                filter |= EnumDeviceFilterE.CSB6Family;
            return EnumDevices(filter);
		}

        public static List<WMI.DeviceInfo> EnumDevices_Serial()
        {
            return EnumDevices(EnumDeviceFilterE.FTDISerial | EnumDeviceFilterE.SpringCardSerial);
        }

    }
}
