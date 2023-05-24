using System;
using System.Collections.Generic;
using System.Management;
using System.IO.Ports;

namespace SpringCard.LibCs.Windows
{
	public static class DISK
	{
		private static Logger logger = new Logger(typeof(DISK).FullName);

		public class LogicalDisk : WMI.DeviceInfo
		{
			public LogicalDisk(ManagementObject managementObject) : base(managementObject)
			{

			}
		}

		public class PartitionInfo : WMI.DeviceInfo
		{
			public PartitionInfo(ManagementObject managementObject) : base(managementObject)
			{

			}

			public List<LogicalDisk> LogicalDisks = new List<LogicalDisk>();
		}

		public class DeviceInfo : WMI.DeviceInfo
		{
			public DeviceInfo(ManagementObject managementObject) : base(managementObject)
			{

			}

			public List<PartitionInfo> Partitions = new List<PartitionInfo>();

			public string GetDiskName()
            {
				foreach (PartitionInfo partition in Partitions)
                {
					foreach (LogicalDisk disk in partition.LogicalDisks)
                    {
						string name = disk.Name;
						if (!string.IsNullOrEmpty(name))
							return name;
                    }
                }
				return null;
            }
		}

		public static List<DeviceInfo> EnumUsbDisks()
        {
			List<DeviceInfo> result = new List<DeviceInfo>();

			foreach (ManagementObject managementObjectDisk in new ManagementObjectSearcher(@"SELECT * FROM Win32_DiskDrive WHERE InterfaceType LIKE 'USB%'").Get())
			{
				DeviceInfo diskInfo = new DeviceInfo(managementObjectDisk);
				if (WinUtils.Debug)
					Logger.Debug("Adding USB Disk {0}", diskInfo.Name);

				foreach (ManagementObject managementObjectPartition in new ManagementObjectSearcher(
					"ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + managementObjectDisk.Properties["DeviceID"].Value
					+ "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
				{
					PartitionInfo partitionInfo = new PartitionInfo(managementObjectPartition);
					if (WinUtils.Debug)
						Logger.Debug("\tAdding partition {0}", partitionInfo.Name);

					foreach (ManagementObject managementObjectLogicalDisk in new ManagementObjectSearcher(
								"ASSOCIATORS OF {Win32_DiskPartition.DeviceID='"
									+ managementObjectPartition["DeviceID"]
									+ "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
					{
						LogicalDisk logicalDiskInfo = new LogicalDisk(managementObjectLogicalDisk);
						if (WinUtils.Debug)
							Logger.Debug("\t\tAdding logical disk {0}", logicalDiskInfo.Name);
						partitionInfo.LogicalDisks.Add(logicalDiskInfo);
					}

					diskInfo.Partitions.Add(partitionInfo);
				}

				result.Add(diskInfo);
			}

			return result;
		}

		public static void DumpUsbDisks()
        {
			List<DeviceInfo> driveList = EnumUsbDisks();
			foreach (DeviceInfo drive in driveList)
            {
				Console.WriteLine(drive.Name);
				foreach (PartitionInfo partition in drive.Partitions)
                {
					Console.WriteLine("\t" + partition.Name);
					foreach (LogicalDisk disk in partition.LogicalDisks)
                    {
						Console.WriteLine("\t\t" + disk.Name);
					}
				}
            }
		}
	}
}
