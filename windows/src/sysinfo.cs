/**
 *
 * \ingroup Windows
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Collections.Generic;
#if !NET5_0_OR_GREATER
using System.Windows.Forms;
using Microsoft.VisualBasic.Devices;
#endif


namespace SpringCard.LibCs.Windows
{
	public static class SysInfo
	{
		static Dictionary<string, object> _cache = null;

		public static string OperatingSystem
		{
			get
            {
				if (_cache == null)
					_cache = GetObjects();
				return (string) _cache["OperatingSystem"];
			}
        }

		public static string Platform
		{
			get
			{
				if (_cache == null)
					_cache = GetObjects();
				return (string)_cache["Platform"];
			}
		}

		public static string OSVersion
		{
			get
			{
				if (_cache == null)
					_cache = GetObjects();
				return (string)_cache["OSVersion"];
			}
		}

		public static string OSServicePack
		{
			get
			{
				if (_cache == null)
					_cache = GetObjects();
				return (string)_cache["OSServicePack"];
			}
		}

		public static bool SystemIs64Bit
		{
			get
			{
				if (_cache == null)
					_cache = GetObjects();
				return (bool)_cache["SystemIs64Bit"];
			}
		}

		public static string ProcessorArchitecture
		{
			get
			{
				if (_cache == null)
					_cache = GetObjects();
				return (string)_cache["ProcessorArchitecture"];
			}
		}

		public static int ProcessorCount
		{
			get
			{
				if (_cache == null)
					_cache = GetObjects();
				return (int)_cache["ProcessorCount"];
			}
		}

		public static string PhysicalMemory
		{
			get
			{
				if (_cache == null)
					_cache = GetObjects();
				return (string)_cache["PhysicalMemory"];
			}
		}

		/**
		 * \brief Get the list of system information
		 */
		public static Dictionary<string, object> GetObjects()
		{
#if NET5_0_OR_GREATER
			Dictionary<string, object> result = new Dictionary<string, object>();
#else
			Uac uac = new Uac();
			ComputerInfo info = new ComputerInfo();
			
			Dictionary<string, object> result = new Dictionary<string, object>();
			
			result["OperatingSystem"] = info.OSFullName;
			result["Platform"] = info.OSPlatform;
			result["OSVersion"] = info.OSVersion;
			result["OSServicePack"] = Environment.OSVersion.ServicePack;
			result["SystemIs64Bit"] = Environment.Is64BitOperatingSystem;
			result["SystemIsDebug"] = SystemInformation.DebugOS;
			result["ProcessorArchitecture"] = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
			result["ProcessorCount"] = Environment.ProcessorCount;
			result["PhysicalMemory"] = string.Format("{0:#.###}GB", ((double) info.TotalPhysicalMemory) / 1024.0 / 1024.0 / 1024.0).Replace(',', '.');
			result["BootMode"] = SystemInformation.BootMode.ToString();
			result["PowerLineStatus"] = SystemInformation.PowerStatus.PowerLineStatus.ToString();
			result["BatteryChargeStatus"] = SystemInformation.PowerStatus.BatteryChargeStatus.ToString();
			result["BatteryLifePercent"] = string.Format("{0}%", SystemInformation.PowerStatus.BatteryLifePercent * 100.0);
			result["BatteryLifeRemaining"] = SystemInformation.PowerStatus.BatteryLifeRemaining;
			result["Culture"] = info.InstalledUICulture.ToString();
			result["ApplicationIs64Bit"] = Environment.Is64BitProcess;
			result["ApplicationIsElevated"] = uac.IsRunAsAdmin();
			result["UserIsAdmin"] = uac.IsRunAsAdmin();
			result["CommandLine"] = Environment.CommandLine;
			result["CurrentDirectory"] = Environment.CurrentDirectory;
			result["Secure"] = SystemInformation.Secure;
			result["MachineName"] = Environment.MachineName;
			result["ComputerName"] = SystemInformation.ComputerName;
			result["UserName"] = SystemInformation.UserName;
			result["UserInteractive"] = SystemInformation.UserInteractive;
			result["TerminalServer"] = SystemInformation.TerminalServerSession;
			result["ShutdownStarted"] = Environment.HasShutdownStarted;			
#endif
			return result;
		}

			private static string Format(string value)
		{
			return value;
		}

		private static string Format(int value)
		{
			return string.Format("{0}", value);
		}

		private static string Format(bool value)
		{
			return value ? "true" : "false";
		}


		/**
		 * \brief Get the list of system information
		 */
		public static Dictionary<string, string> Get()
		{
#if NET5_0_OR_GREATER
			Dictionary<string, string> result = new Dictionary<string, string>();
#else
			Uac uac = new Uac();
			ComputerInfo info = new ComputerInfo();
			
			Dictionary<string, string> result = new Dictionary<string, string>();
			
			result["OperatingSystem"] = Format(info.OSFullName);
			result["Platform"] = Format(info.OSPlatform);
			result["OSVersion"] = Format(info.OSVersion);
			result["OSServicePack"] = Format(Environment.OSVersion.ServicePack);
			result["SystemIs64Bit"] = Format(Environment.Is64BitOperatingSystem);
			result["SystemIsDebug"] = Format(SystemInformation.DebugOS);
			result["ProcessorArchitecture"] = Format(Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));
			result["ProcessorCount"] = Format(Environment.ProcessorCount);
			result["PhysicalMemory"] = string.Format("{0:#.###}GB", ((double) info.TotalPhysicalMemory) / 1024.0 / 1024.0 / 1024.0);
			result["BootMode"] = Format(SystemInformation.BootMode.ToString());
			result["PowerLineStatus"] = Format(SystemInformation.PowerStatus.PowerLineStatus.ToString());
			result["BatteryChargeStatus"] = Format(SystemInformation.PowerStatus.BatteryChargeStatus.ToString());
			result["BatteryLifePercent"] = Format(string.Format("{0}%", SystemInformation.PowerStatus.BatteryLifePercent * 100.0));
			result["BatteryLifeRemaining"] = Format(SystemInformation.PowerStatus.BatteryLifeRemaining);
			result["Culture"] = Format(info.InstalledUICulture.ToString());
			result["ApplicationIs64Bit"] = Format(Environment.Is64BitProcess);
			result["ApplicationIsElevated"] = Format(uac.IsRunAsAdmin());
			result["UserIsAdmin"] = Format(uac.IsRunAsAdmin());
			result["CommandLine"] = Format(Environment.CommandLine);
			result["CurrentDirectory"] = Format(Environment.CurrentDirectory);
			result["Secure"] = Format(SystemInformation.Secure);
			result["MachineName"] = Format(Environment.MachineName);
			result["ComputerName"] = Format(SystemInformation.ComputerName);
			result["UserName"] = Format(SystemInformation.UserName);
			result["UserInteractive"] = Format(SystemInformation.UserInteractive);
			result["TerminalServer"] = Format(SystemInformation.TerminalServerSession);
			result["ShutdownStarted"] = Format(Environment.HasShutdownStarted);			
#endif
			return result;
		}

        static Dictionary<string, string> cache = null;

		/**
		 * \brief Get a system information
		 */
		public static string Get(string Name)
        {
            if (cache == null)
                cache = Get();
            return cache[Name];
        }

	}
}
