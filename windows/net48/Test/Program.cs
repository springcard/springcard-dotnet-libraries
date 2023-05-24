using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;

namespace Test
{
    static class Program
    {
        static void DumpUsbDevice(USB.DeviceInfo deviceInfo, int depth)
        {
            string s = "";
            for (int i = 0; i <= depth; i++)
                s += "\t";
            if (deviceInfo.IsHub)
                Console.WriteLine(s + string.Format("*** This is a hub ***"));
            Console.WriteLine(s + string.Format("Name: {0}", deviceInfo.Name));
            if (!string.IsNullOrEmpty(deviceInfo.FriendlyName) && (deviceInfo.FriendlyName != deviceInfo.Name))
                Console.WriteLine(s + string.Format("FriendlyName: {0}", deviceInfo.FriendlyName));
            Console.WriteLine(s + string.Format("Mode: {0}", deviceInfo.Mode));
            Console.WriteLine(s + string.Format("DeviceId: {0}", deviceInfo.DeviceId));
            Console.WriteLine(s + string.Format("HardwareId: {0}", deviceInfo.HardwareId));
            if (deviceInfo.HardwareIds != null)
            {
                Console.WriteLine(s + string.Format("HardwareIds:"));
                foreach (string t in deviceInfo.HardwareIds)
                    Console.WriteLine(s + string.Format(" - {0}", t));
            }
            if (deviceInfo.CompatibleIds != null)
            {
                Console.WriteLine(s + string.Format("CompatibleIds:"));
                foreach (string t in deviceInfo.CompatibleIds)
                    Console.WriteLine(s + string.Format(" - {0}", t));
            }
            Console.WriteLine(s + string.Format("DevicePath: {0}", deviceInfo.DevicePath));
            Console.WriteLine(s + string.Format("SerialNumber: {0}", deviceInfo.SerialNumber));
            Console.WriteLine(s + string.Format("Address: {0}", deviceInfo.Address));
            Console.WriteLine(s + string.Format("BusNumber: {0}", deviceInfo.BusNumber));
            Console.WriteLine(s + string.Format("Service: {0}", deviceInfo.Service));
            Console.WriteLine(s + string.Format("DriverKeyName: {0}", deviceInfo.DriverKeyName));
            Console.WriteLine(s + string.Format("LocationInfo: {0}", deviceInfo.LocationInfo));
            Console.WriteLine(s + string.Format("LocationPath: {0}", deviceInfo.LocationPath));
            Console.WriteLine(s + string.Format("PhysicalObjectName: {0}", deviceInfo.PhysicalObjectName));
            Console.WriteLine(s + string.Format("ClassName: {0}", deviceInfo.ClassName));
            Console.WriteLine(s + string.Format("ClassGuid: {0}", deviceInfo.ClassGuid));

            string commName = USB.GetAssociatedCommName(deviceInfo);
            if (!string.IsNullOrEmpty(commName))
                Console.WriteLine(s + string.Format("SerialPort: {0}", commName));
            string diskName = USB.GetAssociatedDiskName(deviceInfo);
            if (!string.IsNullOrEmpty(diskName))
                Console.WriteLine(s + string.Format("Disk: {0}", diskName));
        }

        static void DumpUsbTreeByPort(USB.TreeNode usbNode, int depth = 0)
        {
            string s = "";
            for (int i = 0; i < depth; i++)
                s += "\t";

            if (usbNode.Parent != null)
            {
                s += string.Format("{0:D02} : ", usbNode.ChildNumber);
            }

            if (usbNode.Description == null)
            {
                s += "[empty]";
            }
            else
            {
                s += usbNode.Description;
            }

            Console.WriteLine(s);

            if (usbNode.DeviceInfo != null)
            {
                DumpUsbDevice(usbNode.DeviceInfo, depth + 1);
            }

            if (usbNode.Children != null)
            {
                foreach (USB.TreeNode childNode in usbNode.Children)
                    DumpUsbTreeByPort(childNode, depth + 1);
            }
        }

        static void DumpUsbTreeByLocation(USB.TreeNode usbNode, int depth = 0)
        {
            string s = "";
            for (int i = 0; i < depth; i++)
                s += "\t";

            s += usbNode.FriendlyLocationPath + " ";

            if (usbNode.Description == null)
            {
                s += "[empty]";
            }
            else
            {
                s += usbNode.Description;
            }

            Console.WriteLine(s);

            if (usbNode.DeviceInfo != null)
            {
                DumpUsbDevice(usbNode.DeviceInfo, depth + 1);
            }

            if (usbNode.Children != null)
            {
                foreach (USB.TreeNode childNode in usbNode.Children)
                    DumpUsbTreeByLocation(childNode, depth + 1);
            }
        }

        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            SystemConsole.Show();
            WinUtils.Debug = true;
            Logger.ReadArgs(args);

            Console.WriteLine("Hello, world");

            if (false)
            {
                Console.WriteLine("**** TESTING SETUPAPI.EnumUsbHubs() ****");
                List<SETUPAPI.DeviceInfo> hubs = SETUPAPI.EnumUsbHubs();
                foreach (SETUPAPI.DeviceInfo hub in hubs)
                {
                    Console.WriteLine("{0} ({1})", hub.DeviceId, hub.Description);
                    Console.WriteLine("\tDriverKeyName: {0}", hub.DriverKeyName);
                    Console.WriteLine("\tDevicePath: {0}", hub.DevicePath);
                    Console.WriteLine("\tPhysicalObject: {0}", hub.PhysicalObjectName);
                    Console.WriteLine("\tLocationInfo: {0}", hub.LocationInfo);
                    Console.WriteLine("\tLocationPath: {0}", hub.LocationPath);
                    Console.WriteLine("\tBusNumber: {0}", hub.BusNumber);
                    Console.WriteLine("\tAddress: {0}", hub.Address);
                    if (hub.HardwareIds != null)
                    {
                        Console.WriteLine("\tHardwareIds:");
                        foreach (string s in hub.HardwareIds)
                            Console.WriteLine("\t\t{0}", s);
                    }
                    if (hub.CompatibleIds != null)
                    {
                        Console.WriteLine("\tCompatibleIds:");
                        foreach (string s in hub.CompatibleIds)
                            Console.WriteLine("\t\t{0}", s);
                    }
                    Console.WriteLine();
                }
            }

            if (false)
            {
                Console.WriteLine("**** TESTING USB.GetHubTree() ****");
                USB.TreeNode usbRoot = USB.GetHubTree();
                Console.WriteLine("**** DUMP BY PORT ****");
                DumpUsbTreeByPort(usbRoot);
                Console.WriteLine("**** DUMP BY LOCATION ****");
                DumpUsbTreeByLocation(usbRoot);
            }

            if (false)
            {
                Console.WriteLine("**** TESTING MULTIPROD USBHELPER ****");

                int boardCount = 5;

                if (UsbHelper.Instance.GetMultiBenchUSBSetup(out string portNameBench, out USB.DeviceInfo deviceInfoBench, boardCount, out string[] portNameBoards, out USB.DeviceInfo[] deviceInfoBoards))
                {
                    if (portNameBench != null) /* Controller */
                    {
                        Console.WriteLine("Controller is {0}", portNameBench);
                        if (deviceInfoBench != null)
                        {
                            Console.WriteLine("\tName: {0}", deviceInfoBench.Name);
                            Console.WriteLine("\tSerialNumber: {0}", deviceInfoBench.SerialNumber);
                        }

                        for (int boardIndex = 0; boardIndex < boardCount; boardIndex++)
                        {
                            if (!string.IsNullOrEmpty(portNameBoards[boardIndex]))
                            {
                                Console.WriteLine("Board #{0} is   {1}", boardIndex+1, portNameBoards[boardIndex]);
                            }
                            else
                            {
                                Console.WriteLine("Board #{0} not found", boardIndex + 1);
                            }

                            if (deviceInfoBoards[boardIndex] != null)
                            {
                                if (deviceInfoBench != null)
                                {
                                    Console.WriteLine("\t\tName: {0}", deviceInfoBench.Name);
                                    Console.WriteLine("\t\tSerialNumber: {0}", deviceInfoBench.SerialNumber);
                                }

                                string commPortName = USB.GetAssociatedCommName(deviceInfoBoards[boardIndex]);
                                if (!string.IsNullOrEmpty(commPortName))
                                {
                                    Console.WriteLine("\t\tComm. port: {0}", commPortName);
                                }
                            }
                            else
                            {
                                Console.WriteLine("\t\t{0}", "{Empty}");
                            }
                        }
                    }
                }

            }

            if (false)
            {
                Console.WriteLine("**** TESTING SETUPAPI.EnumUsbDevices() ****");
                List<SETUPAPI.DeviceInfo> devices = SETUPAPI.EnumUsbDevices();
                foreach (SETUPAPI.DeviceInfo device in devices)
                {
                    Console.WriteLine("{0} ({1})", device.DeviceId, device.Description);
                    Console.WriteLine("\tDriverKeyName: {0}", device.DriverKeyName);
                    Console.WriteLine("\tDevicePath: {0}", device.DevicePath);
                    Console.WriteLine("\tPhysicalObject: {0}", device.PhysicalObjectName);
                    Console.WriteLine("\tLocationInfo: {0}", device.LocationInfo);
                    Console.WriteLine("\tLocationPath: {0}", device.LocationPath);
                    Console.WriteLine("\tBusNumber: {0}", device.BusNumber);
                    Console.WriteLine("\tAddress: {0}", device.Address);
                    Console.WriteLine("\tHardwareIds:");
                    foreach (string s in device.HardwareIds)
                        Console.WriteLine("\t\t{0}", s);
                    Console.WriteLine("\tCompatibleIds:");
                    foreach (string s in device.CompatibleIds)
                        Console.WriteLine("\t\t{0}", s);
                    Console.WriteLine();
                }
            }


            if (false)
            {
                Console.WriteLine("**** TESTING USB.EnumHubsAndDevices() ****");
                
                List<USB.DeviceInfo> usbDevices = USB.EnumHubsAndDevices();

                SortedDictionary<string, List<USB.DeviceInfo>> usbDeviceTree = new SortedDictionary<string, List<USB.DeviceInfo>>();

                foreach (USB.DeviceInfo usbDevice in usbDevices)
                {
                    if (string.IsNullOrEmpty(usbDevice.LocationInfo))
                        continue;

                    if (usbDeviceTree.ContainsKey(usbDevice.LocationInfo))
                    {
                        usbDeviceTree[usbDevice.LocationInfo].Add(usbDevice);
                    }
                    else
                    {
                        usbDeviceTree[usbDevice.LocationInfo] = new List<USB.DeviceInfo>();
                        usbDeviceTree[usbDevice.LocationInfo].Add(usbDevice);
                    }
                }

                foreach (KeyValuePair<string, List<USB.DeviceInfo>> entry in usbDeviceTree)
                {
                    Console.WriteLine("{0}", entry.Key);
                    foreach (USB.DeviceInfo usbDevice in entry.Value)
                    {
                        Console.WriteLine("\t{0} {1} {2}", usbDevice.DeviceId, usbDevice.Service, usbDevice.DriverKeyName);
                    }
                }
            }

            if (false)
            {
                Console.WriteLine("**** TESTING USB.GetTree() ****");
                USB.TreeNode usbRoot = USB.GetTree();
                DumpUsbTreeByPort(usbRoot);
                DumpUsbTreeByLocation(usbRoot);
            }

            if (false)
            {
                Console.WriteLine("**** TESTING USB.GetBasicTree() ****");
                Dictionary<string, USB.DeviceInfo> usbTree = USB.GetBasicTree();
                foreach (KeyValuePair<string, USB.DeviceInfo> usbNode in usbTree)
                {
                    if (usbNode.Value == null)
                    {
                        Console.WriteLine("{0}: {1}", usbNode.Key, "[empty]");
                    }
                    else
                    {
                        Console.WriteLine("{0}: {1}", usbNode.Key, usbNode.Value.ToString());
                    }
                }
                Console.WriteLine("DONE!");
            }

            if (false)
                return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
