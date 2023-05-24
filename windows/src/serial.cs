using System;
using System.Collections.Generic;
using System.Management;
using System.IO.Ports;

namespace SpringCard.LibCs.Windows
{
	public static class SERIAL
	{
		private static Logger logger = new Logger(typeof(SERIAL).FullName);

		public class DeviceInfo
		{
			ManagementObject device;
			
			public DeviceInfo(ManagementObject device)
			{
				this.device = device;
			}
			public string Name
			{
				get
				{
					return (string)device.GetPropertyValue("Name");
				}				
			}			
			public string PortName
			{
				get
				{
					string name = Name;
					
					string[] exploded_name = name.Split('(');
					
					foreach (string subname in exploded_name)
					{
						if (subname.StartsWith("COM") || subname.StartsWith("LPT"))
						{
							string[] exploded_subname = subname.Split(')');
							return exploded_subname[0];
						}
					}
					
					return name;
				}				
			}						
			public string DeviceId
			{
				get
				{
					return (string)device.GetPropertyValue("DeviceID");
				}				
			}						
			public string PnpDeviceId
			{
				get
				{
					return (string)device.GetPropertyValue("PNPDeviceID");
				}				
			}			
			public string[] HardwareIds
			{
				get
				{
					return (string[])device.GetPropertyValue("HardwareID");
				}				
			}			
			
		}
    
		public static List<DeviceInfo> EnumCommPorts()
		{
			List<DeviceInfo> commPorts = new List<DeviceInfo>();
			
			try
			{
				ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
				
				foreach (ManagementObject queryObj in searcher.Get())
				{
					DeviceInfo deviceObj = new DeviceInfo(queryObj);					
					if (deviceObj.PortName.StartsWith("LPT")) continue;
                    if (WinUtils.Debug)
                        logger.debug("Found a comm. port: " + deviceObj.PortName);
					commPorts.Add(deviceObj);
				}
			}
			catch (Exception e)
			{
				logger.trace("Exception while listing the comm. ports");
				logger.trace(e.Message);
			}
			
			return commPorts;
		}
		
		public static string[] GetCommPortNames()
		{
			string[] result = SerialPort.GetPortNames();
			if (result == null)
				result = new string[0];
			return result;
		}

		public static string GetDeviceCommPortName(USB.DeviceInfo device)
		{
			if (device == null)
				return null;

			string FtdiLookup = null;

			if (device.Service.ToUpper() == "FTDIBUS")
			{
				try
				{
					string[] e;
					FtdiLookup = device.PnpDeviceId;
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

			foreach (DeviceInfo commPortInfo in EnumCommPorts())
			{
				if (commPortInfo.PnpDeviceId == device.PnpDeviceId)
				{
					return commPortInfo.PortName;
				}

				if (FtdiLookup != null)
				{
					if (commPortInfo.PnpDeviceId.StartsWith(FtdiLookup))
					{
						return commPortInfo.PortName;
					}
				}
			}

			return null;
		}


		public static List<WMI.DeviceInfo> EnumDevices()
        {
            return WMI.EnumDevices("Win32_SerialPort", null, null);
        }
    }
}
