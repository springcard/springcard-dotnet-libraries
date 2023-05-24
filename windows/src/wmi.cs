using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;

namespace SpringCard.LibCs.Windows
{
    public static class WMI
    {
        private static Logger logger = new Logger(typeof(WMI).FullName);

        public const string WmiDeviceTable = @"Win32_PnPEntity";

        public class DeviceInfo
        {
            ManagementObject managementObject; /* Right-click References on the right and manually add System.Management. Even though I included it in the using statement I still had to do this. Once I did, all worked fine. */

            public string HardwareId { get; private set; } = "";

            internal ManagementObject GetManagementObject()
            {
                return managementObject;
            }


            public DeviceInfo(ManagementObject managementObject)
            {
                this.managementObject = managementObject;

                try
                {
                    string[] s = (string[])managementObject.GetPropertyValue("HardwareID");
                    HardwareId = s[0];
                } catch { }
                if (string.IsNullOrEmpty(HardwareId))
                {
                    try
                    {
                        HardwareId = (string)managementObject.GetPropertyValue("HardwareID");
                    }
                    catch { }
                }

                if (HardwareId == null)
                {
                    HardwareId = managementObject.Path.Path;
                    HardwareId = HardwareId.Replace("\\\\", "\\");
                    string C = "DeviceID=";
                    if (HardwareId.Contains(C))
                    {
                        HardwareId = HardwareId.Substring(HardwareId.IndexOf(C) + C.Length);
                        if (HardwareId.StartsWith("\"") && HardwareId.EndsWith("\"") && (HardwareId.Length >= 2))
                            HardwareId = HardwareId.Substring(1, HardwareId.Length - 2);
                    }
                }
            }

            private string GetProperty(string name)
            {
                try
                {
                    return (string)managementObject.GetPropertyValue(name);
                }
                catch
                {
                    return "";
                }
            }

            public string DeviceId
            {
                get
                {
                    return GetProperty("DeviceID");
                }
            }

            public string PnpDeviceId
            {
                get
                {
                    return GetProperty("PNPDeviceID");
                }
            }

            public string Description
            {
                get
                {
                    return GetProperty("Description");
                }
            }

            public string Name
            {
                get
                {
                    return GetProperty("Name");
                }
            }

            public string Manufacturer
            {
                get
                {
                    return GetProperty("Manufacturer");
                }
            }

            public string Service
            {
                get
                {
                    return GetProperty("Service");
                }
            }

            public string Status
            {
                get
                {
                    return GetProperty("Status");
                }
            }

            public string Caption
            {
                get
                {
                    return GetProperty("Caption");
                }
            }

            public string GetHidInstanceId()
            {
                return HID.GetInstanceId(this);
            }
        }

        public static void Dump(ManagementObject managementObject, int depth = 0)
        {
            string prefix = "";
            for (int i = 0; i < depth; i++)
                prefix += "\t";

            foreach (PropertyData data in managementObject.Properties)
            {
                string name = data.Name;
                List<string> values = new List<string>();

                if (data.Value != null)
                {
                    if (data.Value is string)
                    {
                        values.Add(data.Value as string);
                    }
                    else if (data.Value is string[])
                    {
                        foreach (string s in data.Value as string[])
                            values.Add(s);
                    }
                    else if (data.Value is byte[])
                    {
                        values.Add(BinConvert.ToHex(data.Value as byte[]));
                    }
                    else
                    {
                        values.Add(data.Value.ToString());
                    }
                }

                if (values.Count == 0)
                {
                    // Skip
                }
                else if (values.Count == 1)
                {
                    Console.WriteLine(prefix + "{0}: {1}", name, values[0]);
                }
                else
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        Console.WriteLine(prefix + "{0}#{1}: {2}", name, i, values[i]);
                    }
                }
            }
        }

        public static List<DeviceInfo> EnumDevices()
		{
			return EnumDevices(WmiDeviceTable, null, null);
		}
		
		public delegate bool EnumDeviceFilter(DeviceInfo device);
		public static List<DeviceInfo> EnumDevices(string table, string where, EnumDeviceFilter filter)
		{
            logger.debug("WMI:EnumDevices");

            List<DeviceInfo> devices = new List<DeviceInfo>();
			
			try
			{
				ManagementObjectCollection collection = null;
				
				string selectString = string.Format(@"SELECT * FROM {0}", table);
                if (!string.IsNullOrEmpty(where))
                    selectString += string.Format(" WHERE ({0})", where);

                if (WinUtils.Debug)
                    logger.debug("WMI:{0}", selectString);

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectString))
					collection = searcher.Get();
				
				if (collection != null)
				{
					foreach (ManagementObject managementObject in collection)
					{
						try
						{						
							DeviceInfo deviceInfo = new DeviceInfo(managementObject);

                            if (filter != null)
                            {
                                if (!filter(deviceInfo))
                                {
                                    if (WinUtils.Debug)
                                        logger.debug("WMI:Discarding {0}", deviceInfo.Description);
                                    continue;
                                }
                            }

                            if (WinUtils.Debug)
                                logger.debug("WMI:Adding {0}", deviceInfo.Description);
                            devices.Add(deviceInfo);
						}
						catch (Exception e)
                        {
                            logger.warning("WMI:" + e.Message);
                        }
					}
					
					collection.Dispose();
				}
                else
                {
                    logger.debug("WMI:Empty");
                }
			}
			catch (Exception e)
            {
                logger.warning("WMI:" + e.Message);
            }
			
			return devices;			
		}
	
    }
}
