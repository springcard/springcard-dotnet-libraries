/*
 * Author : herve.t@springcard.com
 * Date: 19/09/2014
 */

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;

namespace SpringCard.LibCs.Windows
{
	/// <summary>
	/// A class used to activate / deactivate and reset USB devices connected to the computer
	/// 
	/// Exemple of use :
	///     setupapi.ActivateDevice("SpringCard Prox'N'Roll");
	///     setupapi.DeactivateDevice("SpringCard Prox'N'Roll");
	///     setupapi.ResetDevice("SpringCard Prox'N'Roll");
	///
	/// </summary>
	public static class SETUPAPI
	{
		private static Logger logger = new Logger(typeof(SETUPAPI).FullName);

		#region structures
		internal const uint DIF_PROPERTYCHANGE = 0x12;
		internal const uint DICS_ENABLE = 1;
		internal const uint DICS_DISABLE = 2;        // disable device
		internal const uint DICS_PROPCHANGE = 3;     // Reset
		internal const uint DICS_FLAG_GLOBAL = 1;    // not profile-specific

		internal const uint DICS_FLAG_CONFIGSPECIFIC = (0x00000002);

		internal const uint DIGCF_ALLCLASSES = 0x00000004;
		internal const uint DIGCF_PROFILE = 0x00000008;
		internal const uint DIGCF_PRESENT = 0x00000002;
		internal const uint DIGCF_DEFAULT = 0x00000001;
		internal const uint DIGCF_DEVICEINTERFACE = 0x00000010;


		internal const uint ERROR_INVALID_DATA = 13;
		internal const uint ERROR_NO_MORE_ITEMS = 259;
		internal const uint ERROR_ELEMENT_NOT_FOUND = 1168;

		internal const uint SPDRP_DEVICEDESC = 0x00000000; // DeviceDesc (R/W)
		internal const uint SPDRP_HARDWAREID = 0x00000001; // HardwareID (R/W)
		internal const uint SPDRP_COMPATIBLEIDS = 0x00000002; // CompatibleIDs (R/W)
		internal const uint SPDRP_UNUSED0 = 0x00000003; // unused
		internal const uint SPDRP_SERVICE = 0x00000004; // Service (R/W)
		internal const uint SPDRP_UNUSED1 = 0x00000005; // unused
		internal const uint SPDRP_UNUSED2 = 0x00000006; // unused
		internal const uint SPDRP_CLASS = 0x00000007; // Class (R--tied to ClassGUID)
		internal const uint SPDRP_CLASSGUID = 0x00000008; // ClassGUID (R/W)
		internal const uint SPDRP_DRIVER = 0x00000009; // Driver (R/W)
		internal const uint SPDRP_CONFIGFLAGS = 0x0000000A; // ConfigFlags (R/W)
		internal const uint SPDRP_MFG = 0x0000000B; // Mfg (R/W)
		internal const uint SPDRP_FRIENDLYNAME = 0x0000000C; // FriendlyName (R/W)
		internal const uint SPDRP_LOCATION_INFORMATION = 0x0000000D; // LocationInformation (R/W)
		internal const uint SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E; // PhysicalDeviceObjectName (R)
		internal const uint SPDRP_CAPABILITIES = 0x0000000F; // Capabilities (R)
		internal const uint SPDRP_UI_NUMBER = 0x00000010; // UiNumber (R)
		internal const uint SPDRP_UPPERFILTERS = 0x00000011; // UpperFilters (R/W)
		internal const uint SPDRP_LOWERFILTERS = 0x00000012; // LowerFilters (R/W)
		internal const uint SPDRP_BUSTYPEGUID = 0x00000013; // BusTypeGUID (R)
		internal const uint SPDRP_LEGACYBUSTYPE = 0x00000014; // LegacyBusType (R)
		internal const uint SPDRP_BUSNUMBER = 0x00000015; // BusNumber (R)
		internal const uint SPDRP_ENUMERATOR_NAME = 0x00000016; // Enumerator Name (R)
		internal const uint SPDRP_SECURITY = 0x00000017; // Security (R/W, binary form)
		internal const uint SPDRP_SECURITY_SDS = 0x00000018; // Security (W, SDS form)
		internal const uint SPDRP_DEVTYPE = 0x00000019; // Device Type (R/W)
		internal const uint SPDRP_EXCLUSIVE = 0x0000001A; // Device is exclusive-access (R/W)
		internal const uint SPDRP_CHARACTERISTICS = 0x0000001B; // Device Characteristics (R/W)
		internal const uint SPDRP_ADDRESS = 0x0000001C; // Device Address (R)
		internal const uint SPDRP_UI_NUMBER_DESC_FORMAT = 0X0000001D; // UiNumberDescFormat (R/W)
		internal const uint SPDRP_DEVICE_POWER_DATA = 0x0000001E; // Device Power Data (R)
		internal const uint SPDRP_REMOVAL_POLICY = 0x0000001F; // Removal Policy (R)
		internal const uint SPDRP_REMOVAL_POLICY_HW_DEFAULT = 0x00000020; // Hardware Removal Policy (R)
		internal const uint SPDRP_REMOVAL_POLICY_OVERRIDE = 0x00000021; // Removal Policy Override (RW)
		internal const uint SPDRP_INSTALL_STATE = 0x00000022; // Device Install State (R)
		internal const uint SPDRP_LOCATION_PATHS = 0x00000023; // Device Location Paths (R)
		internal const uint SPDRP_BASE_CONTAINERID = 0x00000024;  // Base ContainerID (R)

		/*
		static DEVPROPKEY DEVPKEY_Device_DeviceDesc;
		static DEVPROPKEY DEVPKEY_Device_HardwareIds;
		*/

		[StructLayout(LayoutKind.Sequential)]
		internal struct SP_CLASSINSTALL_HEADER
		{
			public UInt32 cbSize;
			public UInt32 InstallFunction;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct SP_PROPCHANGE_PARAMS
		{
			public SP_CLASSINSTALL_HEADER ClassInstallHeader;
			public UInt32 StateChange;
			public UInt32 Scope;
			public UInt32 HwProfile;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct SP_DEVINFO_DATA
		{
			public UInt32 cbSize;
			public Guid classGuid;
			public UInt32 devInst;
			public IntPtr reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct SP_DEVICE_INTERFACE_DATA
		{
			internal UInt32 cbSize;
			internal System.Guid InterfaceClassGuid;
			internal Int32 Flags;
			internal IntPtr Reserved;
		}

		const int BUFFER_SIZE = 1024;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct SP_DEVICE_INTERFACE_DETAIL_DATA
		{
			public UInt32 cbSize;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
			public string DevicePath;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct DEVPROPKEY
		{
			public Guid fmtid;
			public UInt32 pid;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct USB_ROOT_HUB_NAME
		{
			public UInt32 cbSize;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
			public string RootHubName;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct USB_HUB_DESCRIPTOR
		{
			public byte bDescriptorLength;
			public byte bDescriptorType;
			public byte bNumberOfPorts;
			public short wHubCharacteristics;
			public byte bPowerOnToPowerGood;
			public byte bHubControlCurrent;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			public byte[] bRemoveAndPowerMask;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct USB_NODE_INFORMATION
		{
			public int NodeType;
			public USB_HUB_INFORMATION HubInformation; // Yeah, I'm assuming we'll just use the first form
		}

		[StructLayout(LayoutKind.Sequential)]
		struct USB_HUB_INFORMATION
		{
			public USB_HUB_DESCRIPTOR HubDescriptor;
			public byte HubIsBusPowered;
		}

		enum USB_HUB_NODE
		{
			UsbHub,
			UsbMIParent
		}

		internal enum DeviceState
		{
			On,
			Off,
			Reset
		}

		private static bool stateChanged = false;
		private static bool canStop = false;
		#endregion

		#region DLL Imports
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern IntPtr SetupDiGetClassDevsW([In] ref Guid ClassGuid, [MarshalAs(UnmanagedType.LPWStr)] string Enumerator, IntPtr parent, UInt32 flags);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, UInt32 memberIndex, [Out] out SP_DEVINFO_DATA deviceInfoData);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiSetClassInstallParams(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData, [In] ref SP_PROPCHANGE_PARAMS classInstallParams, UInt32 ClassInstallParamsSize);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiChangeState(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiGetDevicePropertyW(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA DeviceInfoData, [In] ref DEVPROPKEY propertyKey, [Out] out UInt32 propertyType, IntPtr propertyBuffer, UInt32 propertyBufferSize, out UInt32 requiredSize, UInt32 flags);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiGetDeviceRegistryPropertyW(IntPtr DeviceInfoSet, [In] ref SP_DEVINFO_DATA DeviceInfoData, uint Property, [Out] out UInt32 PropertyRegDataType, IntPtr PropertyBuffer, uint PropertyBufferSize, [In, Out] ref UInt32 RequiredSize);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiGetDeviceRegistryPropertyW(IntPtr DeviceInfoSet, [In] ref SP_DEVINFO_DATA DeviceInfoData, uint Property, [Out] out UInt32 PropertyRegDataType, ref UInt32 PropertyBuffer, uint PropertyBufferSize, [In, Out] ref UInt32 RequiredSize);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiGetDeviceInstanceIdW(IntPtr DeviceInfoSet, [In] ref SP_DEVINFO_DATA DeviceInfoData, IntPtr DeviceInstanceIdBuffer, uint DeviceInstanceIdSize, [In, Out] ref UInt32 RequiredSize);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		internal static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, [In] ref Guid ClassGuid, uint MemberIndex, [In, Out] ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);


		#endregion

		#region Public methods
		static SETUPAPI()
		{
			/*
			DEVPKEY_Device_DeviceDesc = new DEVPROPKEY();
			DEVPKEY_Device_DeviceDesc.fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67,0xd1, 0x46, 0xa8, 0x50, 0xe0);
			DEVPKEY_Device_DeviceDesc.pid = 2;

			DEVPKEY_Device_HardwareIds = new DEVPROPKEY();
			DEVPKEY_Device_HardwareIds.fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67,0xd1, 0x46, 0xa8, 0x50, 0xe0);
			DEVPKEY_Device_HardwareIds.pid = 3;
			*/
		}

		/// <summary>
		/// Activate a device via its parts
		/// </summary>
		/// <param name="deviceName"></param>
		public static void ActivateDevice(string pid, string vid, string serialNumber)
		{
			if (string.IsNullOrEmpty(vid) || string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(serialNumber))
				return;

			ChangeDeviceState(pid, vid, serialNumber, DeviceState.On);
		}

		/// <summary>
		/// Activate a device via its parts
		/// </summary>
		/// <param name="deviceName"></param>
		public static void DeactivateDevice(string pid, string vid, string serialNumber)
		{
			if (string.IsNullOrEmpty(vid) || string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(serialNumber))
				return;

			ChangeDeviceState(pid, vid, serialNumber, DeviceState.Off);
		}

		/// <summary>
		/// Reset a device via its parts
		/// </summary>
		/// <param name="deviceName"></param>
		public static void ResetDevice(string pid, string vid, string serialNumber)
		{
			if (string.IsNullOrEmpty(vid) || string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(serialNumber))
				return;

			ChangeDeviceState(pid, vid, serialNumber, DeviceState.Reset);
		}
		#endregion


		#region Private methods
		/// <summary>
		/// Change a device (via its name) state (activate / deactivate / reset)
		/// </summary>
		/// <param name="pid"></param>
		/// <param name="vid"></param>
		/// <param name="serialNumber"></param>
		/// <param name="status"></param>
		/// <param name="listbox"></param>
		private static void ChangeDeviceState(string pid, string vid, string serialNumber, DeviceState status)
		{
			serialNumber = serialNumber.ToUpper();
			IntPtr info = IntPtr.Zero;
			Guid NullGuid = Guid.Empty;
			stateChanged = false;
			try
			{
				info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF_ALLCLASSES | DIGCF_PRESENT);
				CheckError("SetupDiGetClassDevs");

				SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
				devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

				for (uint i = 0; ; i++)
				{
					SetupDiEnumDeviceInfo(info, i, out devdata);
					if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
						CheckError("No device found matching filter.", 0xcffff);
					CheckError("SetupDiEnumDeviceInfo");
					if (canStop)
						break;

					string devicepath = GetStringPropertyForDevice(info, devdata, 1);   // 0 & 1

					if (string.IsNullOrEmpty(devicepath))
						continue;

					if (devicepath != null && IsUsbHardwareId(devicepath))
					{
						string reconstPid = "", reconstVid = "", foundedSerialNumber = "";
						ExtractPidAndVid(devicepath, out reconstPid, out reconstVid);
						foundedSerialNumber = GetSerialNumberFromWMI(reconstPid, reconstVid);

						if (serialNumber.Equals(foundedSerialNumber))
						{
							stateChanged = true;
							ChangeReaderStatus(info, devdata, status);
						}
					}
				}
			}
			finally
			{
				if (info != IntPtr.Zero)
					SetupDiDestroyDeviceInfoList(info);
			}
		}

		/// <summary>
		/// Part in charge to really change the device's state
		/// </summary>
		/// <param name="info"></param>
		/// <param name="devdata"></param>
		/// <param name="status"></param>
		private static void ChangeReaderStatus(IntPtr info, SP_DEVINFO_DATA devdata, DeviceState status)
		{
			string devicepathTemp = GetStringPropertyForDevice(info, devdata, 0);
			SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
			header.cbSize = (UInt32)Marshal.SizeOf(header);
			header.InstallFunction = DIF_PROPERTYCHANGE;

			SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS();
			propchangeparams.ClassInstallHeader = header;
			if (status == DeviceState.On || status == DeviceState.Off)
			{
				propchangeparams.StateChange = (status == DeviceState.Off) ? DICS_DISABLE : DICS_ENABLE;
				propchangeparams.Scope = DICS_FLAG_GLOBAL;
			}
			else    // reset
			{
				propchangeparams.StateChange = DICS_PROPCHANGE;
				propchangeparams.Scope = DICS_FLAG_CONFIGSPECIFIC;
			}
			propchangeparams.HwProfile = 0;

			SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));
			CheckError("SetupDiSetClassInstallParams");

			SetupDiChangeState(info, ref devdata);
			CheckError("SetupDiChangeState");
		}

		/// <summary>
		/// Check for an error
		/// </summary>
		/// <param name="message"></param>
		/// <param name="lasterror"></param>
		private static void CheckError(string message, int lasterror = -1)
		{

			int code = lasterror == -1 ? Marshal.GetLastWin32Error() : lasterror;
			if (code != 0)
			{
				if (!stateChanged)
					throw new ApplicationException(String.Format("SetupApi error {0} in {1}", code, message));
				else
					canStop = true;
			}
		}

		/// <summary>
		/// Get a device's instance id
		/// </summary>
		/// <param name="info"></param>
		/// <param name="devdata"></param>
		/// <param name="propId"></param>
		/// <returns></returns>
		internal static string GetDeviceInstanceId(IntPtr info, SP_DEVINFO_DATA devdata)
		{
			uint outsize;
			IntPtr buffer = IntPtr.Zero;
			try
			{
				uint buflen = 512;
				buffer = Marshal.AllocHGlobal((int)buflen);
				outsize = 0;
				SetupDiGetDeviceInstanceIdW(
					info,
					ref devdata,
					buffer,
					buflen,
					ref outsize);
				int errcode = Marshal.GetLastWin32Error();
				if (errcode == ERROR_INVALID_DATA) return null;
				CheckError("SetupDiGetDeviceInstanceId", errcode);
				byte[] lbuffer = new byte[outsize * Marshal.SystemDefaultCharSize];
				Marshal.Copy(buffer, lbuffer, 0, (int)outsize * Marshal.SystemDefaultCharSize);
				return Encoding.Unicode.GetString(lbuffer);
			}
			finally
			{
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal(buffer);
			}
		}

		internal static UInt32 GetDwordPropertyForDevice(IntPtr info, SP_DEVINFO_DATA devdata, uint propId)
		{
			uint proptype, outsize = 0;
			UInt32 result = 0;
			try
			{
				SetupDiGetDeviceRegistryPropertyW(
					info,
					ref devdata,
					propId,
					out proptype,
					ref result,
					4,
					ref outsize);
				int errcode = Marshal.GetLastWin32Error();
				if (errcode == ERROR_INVALID_DATA) return 0;
				CheckError("SetupDiGetDeviceProperty", errcode);
			}
            finally
            {

            }
			return result;
		}

		/// <summary>
		/// Get a device's information
		/// </summary>
		/// <param name="info"></param>
		/// <param name="devdata"></param>
		/// <param name="propId"></param>
		/// <returns></returns>
		internal static string GetStringPropertyForDevice(IntPtr info, SP_DEVINFO_DATA devdata, uint propId)
		{
			uint proptype, outsize;
			IntPtr buffer = IntPtr.Zero;
			try
			{
				uint buflen = 512;
				buffer = Marshal.AllocHGlobal((int)buflen);
				outsize = 0;
				SetupDiGetDeviceRegistryPropertyW(
					info,
					ref devdata,
					propId,
					out proptype,
					buffer,
					buflen,
					ref outsize);
				int errcode = Marshal.GetLastWin32Error();
				if (errcode == ERROR_INVALID_DATA) return null;
				CheckError("SetupDiGetDeviceProperty", errcode);
				byte[] lbuffer = new byte[outsize];
				Marshal.Copy(buffer, lbuffer, 0, (int)outsize);
				return Encoding.Unicode.GetString(lbuffer);
			}
			finally
			{
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal(buffer);
			}
		}

		internal static string[] BufferToStrings(byte[] buffer)
        {
			List<string> result = new List<string>();

			int start_offset = 0;
			for (int end_offset = 0; end_offset < buffer.Length - 1; end_offset += 2)
            {
				if ((buffer[end_offset] == 0) && (buffer[end_offset+1] == 0) && (start_offset < buffer.Length - 2))
				{
					result.Add(Encoding.Unicode.GetString(buffer, start_offset, end_offset - start_offset));
					start_offset = end_offset + 2;
				}
			}

			return result.ToArray();
        }

		/// <summary>
		/// Get a device's information
		/// </summary>
		/// <param name="info"></param>
		/// <param name="devdata"></param>
		/// <param name="propId"></param>
		/// <returns></returns>
		internal static string[] GetMultiStringPropertyForDevice(IntPtr info, SP_DEVINFO_DATA devdata, uint propId)
		{
			uint proptype, outsize;
			IntPtr buffer = IntPtr.Zero;
			try
			{
				uint buflen = 512;
				buffer = Marshal.AllocHGlobal((int)buflen);
				outsize = 0;
				SetupDiGetDeviceRegistryPropertyW(
					info,
					ref devdata,
					propId,
					out proptype,
					buffer,
					buflen,
					ref outsize);
				int errcode = Marshal.GetLastWin32Error();
				if (errcode == ERROR_INVALID_DATA) return null;
				CheckError("SetupDiGetDeviceProperty", errcode);
				byte[] lbuffer = new byte[outsize];
				Marshal.Copy(buffer, lbuffer, 0, (int)outsize);
				return BufferToStrings(lbuffer);
			}
			finally
			{
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal(buffer);
			}
		}

		/// <summary>
		/// Returns true is the string looks like an USB hardware ID on the form of "USB\VID_1C34&PID_7141..."
		/// </summary>
		/// <param name="hardwareId"></param>
		/// <returns></returns>
		private static bool IsUsbHardwareId(string hardwareId)
		{
			string normalizedString = hardwareId.ToUpper().Trim();
			if (normalizedString.StartsWith("USB") && normalizedString.Contains("VID_") && normalizedString.Contains("PID_"))
				return true;
			return false;
		}

		/// <summary>
		/// Extracts a Pid and Vid from a string
		/// </summary>
		/// <param name="hardwareId"></param>
		/// <param name="pid"></param>
		/// <param name="vid"></param>
		private static void ExtractPidAndVid(string hardwareId, out string pid, out string vid)
		{
			string normalizedString = hardwareId.ToUpper().Trim();
			vid = pid = "";
			// Vid
			if (normalizedString.StartsWith("USB") && normalizedString.Contains("VID_"))
				vid = normalizedString.Substring(normalizedString.IndexOf("VID_") + 4, 4).ToUpper();

			// Pid
			if (normalizedString.StartsWith("USB") && normalizedString.Contains("PID_"))
				pid = normalizedString.Substring(normalizedString.IndexOf("PID_") + 4, 4).ToUpper();
		}


		/// <summary>
		/// Extract an USB serial's number from a WMI DeviceId or PNPDeviceId
		/// The caller must ensure that the string smells like an hard id...
		/// </summary>
		/// <param name="hardwareId"></param>
		/// <returns></returns>
		private static string ExtractSerialNumber(string hardwareId)
		{
			if (hardwareId.IndexOf(@"\") == -1)
				return "";
			string serialNumber = "";
			try
			{
				serialNumber = hardwareId.Substring(hardwareId.LastIndexOf(@"\") + 1).Trim().ToUpper();
			}
			catch (Exception)
			{
			}
			return serialNumber;
		}

		/// <summary>
		/// Retreive an hardware's serial number from its PID and VID
		/// </summary>
		/// <param name="searchedVid"></param>
		/// <param name="searchedPid"></param>
		/// <returns></returns>
		private static string GetSerialNumberFromWMI(string searchedPid, string searchedVid)
		{
			if (string.IsNullOrEmpty(searchedVid) || string.IsNullOrEmpty(searchedPid))
				throw new ArgumentException("MissingParameters");

			try
			{
				ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * From Win32_PnPEntity WHERE (PNPDeviceID LIKE '%USB%VID%&PID%')  AND (PNPDeviceID LIKE '%VID_" + searchedVid + "%')  AND (PNPDeviceID LIKE '%PID_" + searchedPid + "%')");
				foreach (ManagementObject queryObj in searcher.Get())
				{
					if (queryObj["Status"].ToString().ToUpper().Equals("OK"))
					{
						if (IsUsbHardwareId(queryObj["DeviceID"].ToString()))
						{
							return ExtractSerialNumber(queryObj["DeviceID"].ToString());
						}
						else if (IsUsbHardwareId(queryObj["PNPDeviceID"].ToString()))
						{
							return ExtractSerialNumber(queryObj["PNPDeviceID"].ToString());
						}
					}
				}
			}
			catch (ManagementException e)
			{
				throw e;
			}
			return "";
		}

		public static readonly Guid GUID_CLASS_USB_HOST_CONTROLLER = new Guid("{3ABF6F2D-71C4-462A-8A92-1E6861E6AF27}");
		public static readonly Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid("{A5DCBF10-6530-11D2-901F-00C04FB951ED}");
		public static readonly Guid GUID_DEVINTERFACE_USB_HUB = new Guid("{F18A0E88-C30C-11D0-8815-00A0C906BED8}");

		public const int IOCTL_USB_GET_ROOT_HUB_NAME = 0x220408;
		public const int IOCTL_USB_GET_NODE_INFORMATION = 0x220408;

		public class UsbHubInfo
        {
			public string DriverKeyName;
			public bool IsRoot;
			public bool IsBusPowered;
			public int NumberOfPorts
			{
				get
                {
					return Ports.Count;
                }
				set
				{
					Ports = new Dictionary<int, DeviceInfo>();
					for (int i = 1; i <= value; i++)
						Ports[i] = null;
				}
			}
			public int RootNumber = -1;
			public int HubNumber = -1;
			public string LocationInfo;
			public Dictionary<int, DeviceInfo> Ports;
		}

		public class DeviceInfo
		{
			public string DeviceId { get; internal set; }
			public string DevicePath { get; internal set; }
			public string Description { get; internal set; }
			public string[] HardwareIds { get; internal set; }
			public string[] CompatibleIds { get; internal set; }
			public string LocationInfo { get; internal set; }
			public string LocationPath { get; internal set; }
			public string PhysicalObjectName { get; internal set; }
			public string Service { get; internal set; }
			public string DriverKeyName { get; internal set; }
			public uint Address { get; internal set; }
			public uint BusNumber { get; internal set; }
			public string ClassName { get; internal set; }
			public string ClassGuid { get; internal set; }

			internal DeviceInfo Parent;
			internal UsbHubInfo UsbHubInfo;

			private string _FriendlyLocationPath;
			public string FriendlyLocationPath
            {
                get
                {
					if (string.IsNullOrEmpty(_FriendlyLocationPath))
					{
						string location = LocationPath;

						int indexUsbroot = location.IndexOf("USBROOT(");
						int indexAcpi = location.IndexOf("ACPI(");

						if (indexUsbroot >= 0)
						{
							/* This is USB */
							if (indexAcpi > indexUsbroot)
							{
								location = location.Substring(indexUsbroot, indexAcpi - indexUsbroot);
							}
							else
							{
								location = location.Substring(indexUsbroot);
							}


							location = location.Replace("USBROOT(0)#", "");
							location = location.Replace("USB(", "");
							location = location.Replace(")", "");

							_FriendlyLocationPath = "USB";
							foreach (string piece in location.Split('#'))
							{
								if (int.TryParse(piece, out int value))
									_FriendlyLocationPath += string.Format("\\#{0:D04}", value + 1);
							}
						}
						else
						{
							_FriendlyLocationPath = location;
						}
					}
					return _FriendlyLocationPath;
				}
            }

			public override string ToString()
            {
				string result;
				result = string.Format("[{0} {1}] \"{2}\"", LocationInfo, FriendlyLocationPath, Description);
				return result;
			}

			internal bool IsChildOf(DeviceInfo parentDevice)
            {
				if (!DecodeUsbLocationPath(parentDevice.LocationPath, out string parentPathPci, out string parentPathUsbRoot, out string parentPathUsbDetails))
				{
					logger.warning("Parent location path {0} is invalid", parentDevice.LocationPath);
					return false;
				}

				if (!DecodeUsbLocationPath(this.LocationPath, out string thisPathPci, out string thisPathUsbRoot, out string thisPathUsbDetails))
				{
					logger.warning("Current location path {0} is invalid", LocationPath);
					return false;
				}

				if (parentPathPci != thisPathPci)
				{
					if (WinUtils.Debug)
						logger.debug("{0}: {1} is not child of {2}", LocationPath, thisPathPci, parentPathPci);
					return false;
				}

				if (parentPathUsbRoot != thisPathUsbRoot)
				{
					if (WinUtils.Debug)
						logger.debug("{0}: {1} is not child of {2}", LocationPath, thisPathUsbRoot, parentPathUsbRoot);
					return false;
				}

				string[] parentPieces = parentDevice.FriendlyLocationPath.Split('\\');
				string[] childPieces = FriendlyLocationPath.Split('\\');

				if (childPieces.Length != parentPieces.Length + 1)
					return false;

				for (int i=0; i<parentPieces.Length; i++)
                {
					if (string.Compare(parentPieces[i], childPieces[i]) != 0)
						return false;
                }

				if (WinUtils.Debug)
					logger.debug("\t{0} is child of {1}", FriendlyLocationPath, parentDevice.FriendlyLocationPath);

				return true;
            }

			internal bool IsDescendantOf(DeviceInfo parentDevice)
			{
				if (!DecodeUsbLocationPath(parentDevice.LocationPath, out string parentPathPci, out string parentPathUsbRoot, out string parentPathUsbDetails))
				{
					logger.warning("Parent location path {0} is invalid", parentDevice.LocationPath);
					return false;
				}

				if (!DecodeUsbLocationPath(this.LocationPath, out string thisPathPci, out string thisPathUsbRoot, out string thisPathUsbDetails))
				{
					logger.warning("Current location path {0} is invalid", LocationPath);
					return false;
				}

				if (parentPathPci != thisPathPci)
				{
					if (WinUtils.Debug)
						logger.debug("{0}: {1} is not descendant of {2}", LocationPath, thisPathPci, parentPathPci);
					return false;
				}

				if (parentPathUsbRoot != thisPathUsbRoot)
				{
					if (WinUtils.Debug)
						logger.debug("{0}: {1} is not descendant of {2}", LocationPath, thisPathUsbRoot, parentPathUsbRoot);
					return false;
				}

				string[] parentPieces = parentDevice.FriendlyLocationPath.Split('\\');
				string[] childPieces = FriendlyLocationPath.Split('\\');

				if (childPieces.Length <= parentPieces.Length)
					return false;

				for (int i = 0; i < parentPieces.Length; i++)
				{
					if (string.Compare(parentPieces[i], childPieces[i]) != 0)
						return false;
				}

				if (WinUtils.Debug)
					logger.debug("\t{0} is descendant of {1}", FriendlyLocationPath, parentDevice.FriendlyLocationPath);

				return true;
			}
		}

		internal static string GetUsbRootHubDriverKeyName(string rootControllerPath)
		{
			string result = null;

			SafeFileHandle controllerHandle = KERNEL32.CreateFile(rootControllerPath, KERNEL32.GENERIC_WRITE, KERNEL32.FILE_SHARE_WRITE, IntPtr.Zero, KERNEL32.OPEN_EXISTING, 0, 0);
			if (controllerHandle.IsInvalid)
			{
				logger.warning("Failed to open {0}", rootControllerPath);
				return null;
			}

			/* Obtain the driver key name for this host controller */
			int nBytesReturned;
			USB_ROOT_HUB_NAME HubName = new USB_ROOT_HUB_NAME();
			int nBytes = Marshal.SizeOf(HubName);
			IntPtr ptrHubName = Marshal.AllocHGlobal(nBytes);

			// get the Hub Name
			if (KERNEL32.DeviceIoControl(controllerHandle, IOCTL_USB_GET_ROOT_HUB_NAME, ptrHubName, nBytes, ptrHubName, nBytes, out nBytesReturned, IntPtr.Zero))
			{
				HubName = (USB_ROOT_HUB_NAME)Marshal.PtrToStructure(ptrHubName, typeof(USB_ROOT_HUB_NAME));
				result = HubName.RootHubName;
			}
			else
            {
				logger.warning("DeviceIoControl failed for {0}", rootControllerPath);
			}

			Marshal.FreeHGlobal(ptrHubName);

			controllerHandle.Close();

			return result;
		}

		private static UsbHubInfo GetUsbHubInfo(string driverKeyName)
        {
			UsbHubInfo result = new UsbHubInfo();

			driverKeyName = driverKeyName.Replace(@"\\?\", "");

			result.DriverKeyName = driverKeyName;

			string driverFileName = @"\\.\" + driverKeyName;
			SafeFileHandle hubHandle = KERNEL32.CreateFile(driverFileName, KERNEL32.GENERIC_WRITE, KERNEL32.FILE_SHARE_WRITE, IntPtr.Zero, KERNEL32.OPEN_EXISTING, 0, 0);
			if (hubHandle.IsInvalid)
			{
				logger.warning("Failed to open {0}", driverFileName);
				return null;
			}

			USB_NODE_INFORMATION NodeInfo = new USB_NODE_INFORMATION();
			NodeInfo.NodeType = (int)USB_HUB_NODE.UsbHub;
			int nBytes = Marshal.SizeOf(NodeInfo);
			IntPtr ptrNodeInfo = Marshal.AllocHGlobal(nBytes);
			Marshal.StructureToPtr(NodeInfo, ptrNodeInfo, true);

			// get the Hub Information
			if (KERNEL32.DeviceIoControl(hubHandle, IOCTL_USB_GET_NODE_INFORMATION, ptrNodeInfo, nBytes, ptrNodeInfo, nBytes, out nBytes, IntPtr.Zero))
			{
				NodeInfo = (USB_NODE_INFORMATION)Marshal.PtrToStructure(ptrNodeInfo, typeof(USB_NODE_INFORMATION));
				result.IsBusPowered = Convert.ToBoolean(NodeInfo.HubInformation.HubIsBusPowered);
				result.NumberOfPorts = NodeInfo.HubInformation.HubDescriptor.bNumberOfPorts;
			}
			else
			{
				logger.warning("DeviceIoControl failed for {0}", driverKeyName);
			}

			Marshal.FreeHGlobal(ptrNodeInfo);

			hubHandle.Close();

			return result;
		}

		private static bool DecodeUsbLocationPath(string locationPath, out string pathPci, out string pathUsbRoot, out string pathUsbDetails)
		{
			pathPci = "";
			pathUsbRoot = "";
			pathUsbDetails = "";

			if (string.IsNullOrEmpty(locationPath))
			{
				logger.debug("Location is empty");
				return false;
			}

			string[] pieces = locationPath.ToUpper().Split('#');

			int i;
			for (i = 0; i < pieces.Length; i++)
            {
				if (pieces[i].Contains(")"))
				{
					string[] e = pieces[i].Split(')');
					pieces[i] = e[0] + ")";
				}
			}

			i = 0;
			for ( ; i < pieces.Length; i++)
			{
				if (pieces[i].StartsWith("PCI"))
                {
					if (pathPci != "")
						pathPci += "#";
					pathPci += pieces[i];
                }
				else
                {
					break;
                }
			}

			for (; i < pieces.Length; i++)
			{
				if (pieces[i].StartsWith("USBROOT"))
				{
					if (pathUsbRoot != "")
						pathUsbRoot += "#";
					pathUsbRoot += pieces[i];
				}
				else
				{
					break;
				}
			}

			for (; i < pieces.Length; i++)
			{
				if (pieces[i].StartsWith("USB"))
				{
					if (pathUsbDetails != "")
						pathUsbDetails += "#";
					pathUsbDetails += pieces[i];
				}
				else
				{
					break;
				}
			}

			return true;
		}

		private static bool DecodeUsbLocationInfo(string locationInfo, out int hubNumber, out int portNumber)
        {
			hubNumber = 0;
			portNumber = 0;

			if (string.IsNullOrEmpty(locationInfo))
			{
				logger.debug("Location is empty");
				return false;
			}

			string[] pieces = locationInfo.ToLower().Split('.');

			if (pieces.Length != 2)
				goto wrong_format;

			string[] portPieces = pieces[0].Split('#');
			string[] hubPieces = pieces[1].Split('#');

			if ((portPieces.Length != 2) || (hubPieces.Length != 2))
				goto wrong_format;

			if (portPieces[0] != "port_")
				goto wrong_format;
			if (hubPieces[0] != "hub_")
				goto wrong_format;

			if (!int.TryParse(hubPieces[1], out hubNumber))
			{
				if (WinUtils.Debug)
					logger.debug("Hub number '{0}' in location {0} is not an int", hubPieces[1], locationInfo);
				return false;
			}
			if (!int.TryParse(portPieces[1], out portNumber))
			{
				if (WinUtils.Debug)
					logger.debug("Port number '{0}' in location {0} is not an int", portPieces[1], locationInfo);
				return false;
			}

			return true;

		wrong_format:
			logger.error("Location {0} does not have the expected format", locationInfo);
			return false;
		}

		private static bool AttachUsbHubToTree(DeviceInfo usbHub, DeviceInfo parentHub)
        {
			if (!DecodeUsbLocationInfo(usbHub.LocationInfo, out int hubNumber, out int portNumber))
			{
				logger.warning("Can't attach USB hub {0} (location info is invalid)", usbHub.ToString());
				return false;
			}

			if (usbHub.IsChildOf(parentHub))
			{
				usbHub.Parent = parentHub;
				if (parentHub.UsbHubInfo == null)
				{
					logger.error("Trying to attach USB hub {0} to {1} that is not a hub itself", usbHub.ToString(), parentHub.ToString());
					throw new Exception("USB tree is invalid");
				}
				if (parentHub.UsbHubInfo.HubNumber < 0)
				{
					parentHub.UsbHubInfo.HubNumber = hubNumber;
					if (WinUtils.Debug)
						logger.debug("\tUSB hub {0} is now known as hub {1}", parentHub.ToString(), parentHub.UsbHubInfo.HubNumber);
				}
				else if (parentHub.UsbHubInfo.HubNumber != hubNumber)
				{
					logger.error("Trying to attach USB hub {0} to the wrong parent {1} (hub {2} instead of hub {3})", usbHub.ToString(), parentHub.ToString(), parentHub.UsbHubInfo.HubNumber, hubNumber);
					throw new Exception("USB tree is invalid");
				}
				parentHub.UsbHubInfo.Ports[portNumber] = usbHub;
				usbHub.UsbHubInfo.RootNumber = usbHub.Parent.UsbHubInfo.RootNumber;

				if (WinUtils.Debug)
					logger.debug("\tAttached USB hub {0} as port {1:D04} under hub {2}", usbHub.ToString(), portNumber, parentHub.ToString());
				return true;
			}

			/* Device not directly attached to a hub, explore the children */
			if (usbHub.IsDescendantOf(parentHub))
			{
				if (parentHub.UsbHubInfo != null)
				{
					foreach (KeyValuePair<int, DeviceInfo> hubChildWalker in parentHub.UsbHubInfo.Ports)
						if (hubChildWalker.Value != null)
							if (hubChildWalker.Value.UsbHubInfo != null)
								if (AttachUsbHubToTree(usbHub, hubChildWalker.Value))
									return true;
				}
			}

			return false;
        }

		private static bool AttachUsbDeviceToTree(DeviceInfo usbDevice, List<DeviceInfo> usbTree)
		{
			if (!DecodeUsbLocationInfo(usbDevice.LocationInfo, out int hubNumber, out int portNumber))
			{
				logger.warning("Can't attach USB device {0} (location is invalid)", usbDevice.ToString());
				return false;
			}

			foreach (DeviceInfo usbTreeWalker in usbTree)
			{
				if (usbDevice.IsChildOf(usbTreeWalker))
				{
					usbDevice.Parent = usbTreeWalker;
					if (usbTreeWalker.UsbHubInfo == null)
					{
						logger.error("Trying to attach USB device {0} to {1} that is not a hub", usbDevice.ToString(), usbTreeWalker.ToString());
						throw new Exception("USB tree is invalid");
					}
					if (usbTreeWalker.UsbHubInfo.HubNumber < 0)
                    {
						usbTreeWalker.UsbHubInfo.HubNumber = hubNumber;
						if (WinUtils.Debug)
							logger.debug("\tUSB hub {0} is now known as hub {1}", usbTreeWalker.ToString(), usbTreeWalker.UsbHubInfo.HubNumber);
					}
					else if (usbTreeWalker.UsbHubInfo.HubNumber != hubNumber)
					{
						logger.error("Trying to attach USB device {0} to the wrong hub {1} (hub {2} instead of hub {3})", usbDevice.ToString(), usbTreeWalker.ToString(), usbTreeWalker.UsbHubInfo.HubNumber, hubNumber);
						throw new Exception("USB tree is invalid");
					}
					usbTreeWalker.UsbHubInfo.Ports[portNumber] = usbDevice;
					if (WinUtils.Debug)
						logger.debug("\tAttached USB device {0} as port {1:D04} under hub {2}", usbDevice.ToString(), portNumber, usbTreeWalker.ToString());
					return true;
				}
			}

			/* Device not directly attached to a hub, explore the children */
			foreach (DeviceInfo hubTreeWalker in usbTree)
			{
				if (hubTreeWalker.UsbHubInfo != null)
				{
					List<DeviceInfo> usbChildren = new List<DeviceInfo>();
					foreach (KeyValuePair<int, DeviceInfo> hubChildWalker in hubTreeWalker.UsbHubInfo.Ports)
						if (hubChildWalker.Value != null)
							if (hubChildWalker.Value.UsbHubInfo != null)
								usbChildren.Add(hubChildWalker.Value);
					if (usbChildren.Count > 0)
						if (AttachUsbDeviceToTree(usbDevice, usbChildren))
							return true;
				}
			}

			return false;
		}

		private static void DumpUsbDeviceInTree(int depth, int portIndex, DeviceInfo deviceInfo, bool showDeviceDetails)
		{
			string s = "";

			for (int i = 0; i < depth; i++)
				s += "\t";

			if (portIndex >= 0)
			{
				s += string.Format("{0:D02} : ", portIndex);
			}

			if (deviceInfo == null)
			{
				s += "[empty]";
			}
			else
			{
				s += deviceInfo.Description;
			}

			logger.trace(s);

			if (deviceInfo != null)
			{
				if (showDeviceDetails)
				{
					s = "";

					for (int i = 0; i <= depth; i++)
						s += "\t";

					s += deviceInfo.DevicePath;
					logger.trace(s);
				}

				if (deviceInfo.UsbHubInfo != null)
				{
					/* Device is a hub */
					foreach (KeyValuePair<int, DeviceInfo> entry in deviceInfo.UsbHubInfo.Ports)
					{
						DumpUsbDeviceInTree(depth + 1, entry.Key, entry.Value, showDeviceDetails);
					}
				}
			}
		}

		private static void DumpUsbTree(List<DeviceInfo> rootHubs, bool showDeviceDetails)
        {
			foreach (DeviceInfo rootHub in rootHubs)
				DumpUsbDeviceInTree(0, -1, rootHub, showDeviceDetails);
        }

		public static List<DeviceInfo> BuildUsbTree()
        {
			/* The result will be the list of root hubs (generally there's only one) */
			List<DeviceInfo> rootHubs = new List<DeviceInfo>();

			/* Get the list of root controllers */
			List<DeviceInfo> usbControllerList = EnumerateDevicesWithGuid(GUID_CLASS_USB_HOST_CONTROLLER);
			if ((usbControllerList == null) || (usbControllerList.Count == 0))
				return rootHubs;

			/* Get the list of hubs */
			List<DeviceInfo> usbHubList = EnumerateDevicesWithGuid(GUID_DEVINTERFACE_USB_HUB);
			if ((usbHubList == null) || (usbHubList.Count == 0))
				return rootHubs;

			/* Get the list of devices */
			List<DeviceInfo> usbDeviceList = EnumerateDevicesWithGuid(GUID_DEVINTERFACE_USB_DEVICE);

			/* Get root controller(s) */
			Dictionary<string, DeviceInfo> rootControllers = new Dictionary<string, DeviceInfo>();
			foreach (DeviceInfo usbController in usbControllerList)
            {
				string driverKeyName = GetUsbRootHubDriverKeyName(usbController.DevicePath);
				if (!string.IsNullOrEmpty(driverKeyName))
				{
					if (WinUtils.Debug)
						logger.debug("Found USB root controller {0}", driverKeyName);
					rootControllers[driverKeyName.ToLower()] = usbController;
				}
			}

			/* Get the details of all USB hubs */
			foreach (DeviceInfo usbHub in usbHubList)
            {
				if (!string.IsNullOrEmpty(usbHub.DevicePath))
				{
					usbHub.UsbHubInfo = GetUsbHubInfo(usbHub.DevicePath);
				}
			}

			/* Attach the USB root hubs to the USB root controllers */
			int index = 0;
			foreach (DeviceInfo usbHub in usbHubList)
			{
				if (usbHub.UsbHubInfo == null)
					continue;

				string devicePath = usbHub.DevicePath.Replace("\\\\?\\", "").ToLower();
				if (rootControllers.ContainsKey(devicePath))
				{
					DeviceInfo rootController = rootControllers[devicePath];

					if (string.IsNullOrEmpty(usbHub.LocationInfo))
						usbHub.LocationInfo = rootController.LocationInfo;

					usbHub.UsbHubInfo.IsRoot = true;
					usbHub.UsbHubInfo.RootNumber = index++;

					if (WinUtils.Debug)
						logger.debug("Adding USB root hub {0} as tree #{1} - root controller is {2}", usbHub.ToString(), usbHub.UsbHubInfo.RootNumber, rootController.ToString());
					rootHubs.Add(usbHub);
				}
			}

			/* Attach the other USB hubs to the USB root hubs */
			foreach (DeviceInfo rootHub in rootHubs)
            {
				bool something_done;
				do
				{
					something_done = false;
					foreach (DeviceInfo usbHub in usbHubList)
					{
						if (rootHubs.Contains(usbHub))
						{
							/* Don't attach the root hub to themselves */
							continue;
						}

						if (usbHub.Parent != null)
						{
							/* This one is already attached */
							continue;
						}

						if (AttachUsbHubToTree(usbHub, rootHub))
						{
							if (WinUtils.Debug)
								logger.debug("Attached USB child hub as {0} under {1} on tree #{2}", usbHub.ToString(), usbHub.Parent.ToString(), rootHub.UsbHubInfo.RootNumber);
							something_done = true;
						}

					}
				} while (something_done);
			}

			foreach (DeviceInfo usbHub in usbHubList)
			{
				if (rootHubs.Contains(usbHub))
					continue;
				if (usbHub.Parent == null)
					logger.warning("USB hub {0} has no parent", usbHub.ToString());
			}


			/* And attach every device to its parent */
			if (usbDeviceList != null)
			{
				foreach (DeviceInfo usbDevice in usbDeviceList)
				{
					if (!AttachUsbDeviceToTree(usbDevice, rootHubs))
						logger.warning("USB device {0} has no parent hub", usbDevice.ToString());
				}
			}

			if (WinUtils.Debug)
				DumpUsbTree(rootHubs, true);

			return rootHubs;
		}

		public static List<DeviceInfo> EnumUsbHubsAndDevices()
		{
			List<DeviceInfo> result = new List<DeviceInfo>();
			result.AddRange(EnumerateDevicesWithGuid(GUID_DEVINTERFACE_USB_HUB));
			result.AddRange(EnumerateDevicesWithGuid(GUID_DEVINTERFACE_USB_DEVICE));
			return result;
		}

		public static List<DeviceInfo> EnumUsbDevices()
		{
			List<DeviceInfo> result = new List<DeviceInfo>();
			result.AddRange(EnumerateDevicesWithGuid(GUID_DEVINTERFACE_USB_DEVICE));
			return result;
		}

		public static List<DeviceInfo> EnumUsbHubs()
        {
			List<DeviceInfo> result = new List<DeviceInfo>();
			result.AddRange(EnumerateDevicesWithGuid(GUID_DEVINTERFACE_USB_HUB));
			return result;
		}

		public static List<DeviceInfo> EnumAllDevices()
		{
			return EnumerateDevicesWithGuid(Guid.Empty);
		}

		public static List<DeviceInfo> EnumerateDevicesWithGuid(Guid ClassGuid)
		{
			List<DeviceInfo> result = new List<DeviceInfo>();
			IntPtr deviceList = SetupDiGetClassDevsW(ref ClassGuid, null, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
			CheckError("SetupDiGetClassDevs");

			for (uint i = 0; ; i++)
			{
				SP_DEVINFO_DATA deviceData = new SP_DEVINFO_DATA();
				deviceData.cbSize = (UInt32)Marshal.SizeOf(deviceData);

				SetupDiEnumDeviceInfo(deviceList, i, out deviceData);
				if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
					break;
				CheckError("SetupDiEnumDeviceInfo");

				SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
				deviceInterfaceData.cbSize = (UInt32)Marshal.SizeOf(deviceInterfaceData);

				SetupDiEnumDeviceInterfaces(deviceList, IntPtr.Zero, ref ClassGuid, i, ref deviceInterfaceData);
				CheckError("SetupDiEnumDeviceInterfaces");

				int deviceInterfaceDetailSize = 0;
				SetupDiGetDeviceInterfaceDetail(deviceList, ref deviceInterfaceData, IntPtr.Zero, 0, ref deviceInterfaceDetailSize, IntPtr.Zero);

				SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData = new SP_DEVICE_INTERFACE_DETAIL_DATA();
				if (IntPtr.Size == 8) // for 64 bit operating systems
					deviceInterfaceDetailData.cbSize = 8;
				else
					deviceInterfaceDetailData.cbSize = (UInt32) (4 + Marshal.SystemDefaultCharSize); // for 32 bit systems

				SetupDiGetDeviceInterfaceDetail(deviceList, ref deviceInterfaceData, ref deviceInterfaceDetailData, deviceInterfaceDetailSize, ref deviceInterfaceDetailSize, IntPtr.Zero);
				CheckError("SetupDiGetDeviceInterfaceDetail");

				DeviceInfo info = new DeviceInfo();

				info.DeviceId = GetDeviceInstanceId(deviceList, deviceData);
				info.DevicePath = deviceInterfaceDetailData.DevicePath;
				info.Address = GetDwordPropertyForDevice(deviceList, deviceData, SPDRP_ADDRESS);
				info.BusNumber = GetDwordPropertyForDevice(deviceList, deviceData, SPDRP_BUSNUMBER);
				info.Description = GetStringPropertyForDevice(deviceList, deviceData, SPDRP_DEVICEDESC);
				info.HardwareIds = GetMultiStringPropertyForDevice(deviceList, deviceData, SPDRP_HARDWAREID);
				if (info.HardwareIds == null) info.HardwareIds = new string[0];
				info.CompatibleIds = GetMultiStringPropertyForDevice(deviceList, deviceData, SPDRP_COMPATIBLEIDS);
				if (info.CompatibleIds == null) info.CompatibleIds = new string[0];
				info.ClassName = GetStringPropertyForDevice(deviceList, deviceData, SPDRP_CLASS);
				info.ClassGuid = GetStringPropertyForDevice(deviceList, deviceData, SPDRP_CLASSGUID);
				info.Service = GetStringPropertyForDevice(deviceList, deviceData, SPDRP_SERVICE);
				info.DriverKeyName = GetStringPropertyForDevice(deviceList, deviceData, SPDRP_DRIVER);
				info.LocationInfo = GetStringPropertyForDevice(deviceList, deviceData, SPDRP_LOCATION_INFORMATION);
				info.LocationPath = GetStringPropertyForDevice(deviceList, deviceData, SPDRP_LOCATION_PATHS);
				info.PhysicalObjectName = GetStringPropertyForDevice(deviceList, deviceData, SPDRP_PHYSICAL_DEVICE_OBJECT_NAME);

				result.Add(info);
			}

			SetupDiDestroyDeviceInfoList(deviceList);

			return result;
		}

#endregion

	}
}