using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class UsbHelper
    {
        object locker = new object();
        Dictionary<string, USB.DeviceInfo> deviceTree = null;
        DateTime deviceValidUntil = DateTime.MinValue;
        public const int Validity = 2500;

        private bool InternalRefresh()
        {
            if (deviceTree != null)
            {
                if (deviceValidUntil > DateTime.Now)
                {
                    /* Use cache */
                    return true;
                }
            }

            try
            {
                deviceTree = USB.GetBasicTree();
                deviceValidUntil = DateTime.Now.AddMilliseconds(Validity);
                return (deviceTree != null);
            }
            catch (Exception e)
            {
                Logger.Error("Error in UsbHelper:InternalRefresh");
                Logger.Error(e.Message);
                return false;
            }
        }

        public void OnUSBPNPNotification()
        {
            lock (locker)
            {
                deviceTree = null;
            }
        }

        public bool Exists(string portName)
        {
            lock (locker)
            {
                if (!InternalRefresh())
                    return false;
                return deviceTree.ContainsKey(portName);
            }
        }

        public USB.DeviceInfo FindDevice(string deviceName, int deviceIndex, out string portName)
        {
            portName = null;
            lock (locker)
            {
                if (!InternalRefresh())
                    return null;
                foreach (KeyValuePair<string, USB.DeviceInfo> entry in deviceTree)
                {
                    if (entry.Value != null)
                    {
                        if (string.Compare(deviceName, entry.Value.Description) == 0)
                        {
                            if (deviceIndex == 0)
                            {
                                portName = entry.Key;
                                return entry.Value;
                            }
                            deviceIndex--;
                        }
                    }
                }
            }
            return null;
        }

        public USB.DeviceInfo FindDevice(ushort vendorId, ushort productId, int deviceIndex, out string portName)
        {
            portName = null;
            lock (locker)
            {
                if (!InternalRefresh())
                    return null;

                int index = 0;
                foreach (KeyValuePair<string, USB.DeviceInfo> entry in deviceTree)
                {
                    if (entry.Value != null)
                    {
                        if ((entry.Value.wVendorId == vendorId) && (entry.Value.wProductId == productId))
                        {
                            if (index == deviceIndex)
                            {
                                portName = entry.Key;
                                return entry.Value;
                            }
                            index++;
                        }
                    }
                }
            }
            return null;
        }

        public USB.DeviceInfo GetDeviceAfter(string portNameBase, int indexAfter, out string portNameDevice)
        {
            portNameDevice = null;
            lock (locker)
            {
                if (!InternalRefresh())
                    return null;

                bool found = false;
                int index = 0;
                for (int i = 0; i < deviceTree.Count; i++)
                {
                    string portName = deviceTree.Keys.ElementAt(i);
                    USB.DeviceInfo deviceInfo = deviceTree.Values.ElementAt(i);
                    if (!found)
                    {
                        /* Looking for the base */
                        if (string.Compare(portName, portNameBase) == 0)
                        {
                            found = true;
                            if (index == indexAfter)
                            {
                                portNameDevice = portName;
                                return deviceInfo;
                            }
                            index++;
                        }
                    }
                    else
                    {
                        /* Counting the devices (not the hubs) */
                        if ((deviceInfo == null) || (!deviceInfo.IsHub))
                        {
                            if (index == indexAfter)
                            {
                                /* This is the one */
                                portNameDevice = portName;
                                return deviceInfo;
                            }
                            index++;
                        }
                    }
                }

                List<string> portNames = new List<string>();
                foreach (KeyValuePair<string, USB.DeviceInfo> entry in deviceTree)
                    portNames.Add(entry.Key);

                int indexBase = portNames.IndexOf(portNameBase);
                if (indexBase < 0)
                    return null;
                int indexTarget = indexBase + indexAfter;
                if ((indexTarget < 0) || (indexTarget >= portNames.Count))
                    return null;
                portNameDevice = portNames[indexTarget];
                return deviceTree[portNameDevice];
            }
        }

        public USB.DeviceInfo GetDevice(string portName)
        {
            lock (locker)
            {
                if (!InternalRefresh())
                    return null;
                if (!deviceTree.ContainsKey(portName))
                    return null;
                return deviceTree[portName];
            }
        }

        public bool Get(string portName, out bool exists, out USB.DeviceInfo deviceInfo)
        {
            exists = false;
            deviceInfo = null;
            lock (locker)
            {
                if (!InternalRefresh())
                    return false;
                if (!deviceTree.ContainsKey(portName))
                    return false;
                exists = true;
                deviceInfo = deviceTree[portName];
                return true;
            }
        }

        public bool GetMultiBenchUSBSetup(out string portNameBench, out USB.DeviceInfo deviceInfoBench, int boardCount, out string[] portNameBoards, out USB.DeviceInfo[] deviceInfoBoards)
        {
            InternalRefresh();

            foreach (KeyValuePair<string, USB.DeviceInfo> entry in deviceTree)
            {
                if (entry.Value != null)
                {
                    Console.WriteLine("{0}: {1}", entry.Key, entry.Value.ToString());
                    Console.WriteLine("\t{0}", entry.Value.LocationInfo);
                    Console.WriteLine("\t{0}", entry.Value.LocationPath);
                    Console.WriteLine("\t{0}", entry.Value.FriendlyLocationPath);
                }
                else
                    Console.WriteLine("{0}: Empty", entry.Key);
            }

            portNameBoards = new string[boardCount];
            deviceInfoBoards = new USB.DeviceInfo[boardCount];

            deviceInfoBench = FindDevice(0x0403, 0x601C, 0, out portNameBench);

            if (portNameBench == null)
                return false;

            for (int boardIndex = 0; boardIndex < boardCount; boardIndex++)
                deviceInfoBoards[boardIndex] = GetDeviceAfter(portNameBench, 1 + boardIndex, out portNameBoards[boardIndex]);

            return true;
        }

        public static UsbHelper Instance = new UsbHelper();
    }
}

