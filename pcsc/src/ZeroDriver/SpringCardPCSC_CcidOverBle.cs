/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using System.IO.Ports;
using System.Threading;
using SpringCard.LibCs;
using SpringCard.Bluetooth;
using SpringCard.PCSC;
using System.Collections.Generic;

namespace SpringCard.PCSC.ZeroDriver
{
	public abstract class SCardReaderList_CcidOverBle : SCardReaderList_CcidOver
	{	
        protected BLE.Device bleDevice;
        protected bool bleUseBonding;
        protected static Thread asyncThread;
        //protected int CommunicationMtu = 64;
        protected int MAX_LONG_WRITE = 512;

        protected int firmwareVersionMajor = 0;
        protected int firmwareVersionMinor = 0;
        protected int firmwareVersionBuild = 0;

        public const string SoftwareRevisionCharacteristicUuid = "2A28-0000-1000-8000-00805F9B34FB";


        public override bool Available
		{
			get
			{
				return (deviceState == DeviceState.Active);
			}
		}
        
        private bool Connect()
        {
			Logger.Trace("Connecting to BLE device {0}...", bleDevice.Address.ToString());

			if (!bleDevice.Connect(bleUseBonding, new BLE.Device.DisconnectCallback(BleDeviceDisconnectCallback), new BLE.Device.NotificationCallback(BleDeviceNotificationCallback)))
        	{
        		Logger.Trace("Connection failed");
        		return false;
        	}

        	Logger.Trace("Connected to BLE device");
        	return true;
        }

        private void BackgroundCloser()
        {
			Logger.Debug("Disposing the BLE device...");
			ExitCcidMode();
            RemoteDisconnect();
        }

        protected override void CloseDevice()
        {
            Logger.Trace("CCID over BLE: closing...");
            asyncThread = new Thread(BackgroundCloser);
            asyncThread.Start();
            Thread.Sleep(1000);
            Logger.Trace("Releasing the BLE device");
            bleDevice.Disconnect();
        }

		protected abstract bool VerifyGattProfile();
		protected abstract bool EnterCcidMode();
		protected abstract bool ExitCcidMode();
        protected abstract bool RemoteDisconnect();
        protected abstract bool SendCcidPcToRdr(byte[] buffer);
		protected abstract void BleDeviceNotificationCallback(BLE.Device bleDevice, BluetoothUuid uuid, byte[] value);
        protected abstract bool ReadSoftwareVersion();

        protected bool ReadAndParseRevisionString()
        {
            byte[] buffer;

            Logger.Trace("Read Software Version");

            buffer = bleDevice.ReadCharacteristic(new BluetoothUuid(BLE.OrgBluetoothCharacteristicSoftwareRevisionUuid));

            if (buffer == null)
            {
                Logger.Trace("Can't read from the device!");
                return false;
            }

            String softwareRevisionString = StrUtils.ToStr_ASCII(buffer);

            Logger.Trace($"Version = {softwareRevisionString}");

            String firmwareVersion = softwareRevisionString;

            try {

                if (softwareRevisionString.Contains("-"))
                {
                    Logger.Trace("with -");
                    if (!int.TryParse(softwareRevisionString.Split('-')[0].Split('.')[0], out firmwareVersionMajor)
                        || !int.TryParse(softwareRevisionString.Split('-')[0].Split('.')[1], out firmwareVersionMinor)
                        || !int.TryParse(softwareRevisionString.Split('-')[1], out firmwareVersionBuild))
                    {
                        Logger.Trace("Fail to extract number from string");
                        return false;
                    }
                } else
                {
                    Logger.Trace("with .");
                    if (!int.TryParse(softwareRevisionString.Split('.')[0], out firmwareVersionMajor)
                        || !int.TryParse(softwareRevisionString.Split('.')[1], out firmwareVersionMinor)
                        || !int.TryParse(softwareRevisionString.Split('.')[2], out firmwareVersionBuild))
                    {
                        Logger.Trace("Fail to extract number from string");
                        return false;
                    }
                }
            }
            catch
            {
                Logger.Trace($"Exception while parsing revision string {softwareRevisionString}");
                return false;
            }

            return true;
        }

        private void BackgroundSender(Object obj)
        {
			byte[] buffer = (byte []) obj;
			if (!SendCcidPcToRdr(buffer))
			{
				Logger.Trace("Failed to send to the BLE device, closing");
                bleDevice.Disconnect();
			}
        }

        protected override bool Send(byte endpoint, byte[] buffer)
		{
            if (endpoint == CCID.EP_Bulk_PC_To_RDR)
            {
                asyncThread = new Thread(new ParameterizedThreadStart(BackgroundSender));
                asyncThread.Start(buffer);
                return true;
            }
			
			Logger.Trace("Can't send to endpoint " + BinConvert.ToHex(endpoint));
			return false;
		}		
		
		

		
		private void BleDeviceDisconnectCallback(BLE.Device bleDevice)
		{
			Logger.Trace("CCID over BLE: disconnected");
			
			deviceState = DeviceState.NotActive;
			
			if (Children != null)
            {
                for (int i=0; i<Children.Length; i++)
                {
                    if (Children[i] != null)
                    {
                        Logger.Debug("CCID over BLE: disconnected - slot {0}", i);
                        Children[i].NotifyParentLost();
                    }
                }
            }

            Logger.Debug("CCID over BLE: done with disconnected device");

            /* Call callback */
            if (OnDisconnectEx != null)
                OnDisconnectEx(this);
            if (OnDisconnect != null)
                OnDisconnect();
        }

        protected abstract bool GetSlotCount();

        protected override bool GetDescriptors()
		{
			Logger.Trace("GetDescriptors (BLE)");
			
			SlotCount = 1;
			
			byte[] buffer;
			buffer = bleDevice.ReadCharacteristic(new BluetoothUuid(SpringCard.Bluetooth.BLE.OrgBluetoothCharacteristicVendorNameUuid));
			if (buffer == null)
				return false;
			
			VendorName = StrUtils.ToStr(buffer);

			buffer = bleDevice.ReadCharacteristic(new BluetoothUuid(SpringCard.Bluetooth.BLE.OrgBluetoothCharacteristicProductNameUuid));
			if (buffer == null)
				return false;
			
			ProductName = StrUtils.ToStr(buffer);
			
			buffer =  bleDevice.ReadCharacteristic(new BluetoothUuid(SpringCard.Bluetooth.BLE.OrgBluetoothCharacteristicSerialNumberUuid));
			if (buffer == null)
				return false;
			
			SerialNumber = StrUtils.ToStr(buffer);

			return GetSlotCount();
		}

		public static bool ServiceUuidSupported(List<BluetoothUuid> primaryServicesUuid)
		{
			if (primaryServicesUuid == null || primaryServicesUuid.Count < 1)
				return false;

            Logger.Debug("Search service UUID supported");
            foreach (var serviceUuid in primaryServicesUuid)
            {
                Logger.Debug($"Service {serviceUuid}");

                switch (serviceUuid.ToString().ToUpper())
                {
                    case SCardReaderList_CcidOverBle_SoMoD600.ApplicationServiceUuid:
                        return true;

                    case SCardReaderList_CcidOverBle_SpringCore.CcidUnbondedServiceUuid:
                        return true;

                    case SCardReaderList_CcidOverBle_SpringCore.CcidBondedServiceUuid:
                        return true;

                    default:
                        break;
                }
            }
            Logger.Debug("Supported service UUID not found");
            return false;            
        }

		public static bool ServiceUuidSupported(List<string> primaryServicesUuid)
		{
            if (primaryServicesUuid == null || primaryServicesUuid.Count < 1)
                return false;

            List<BluetoothUuid> services = new List<BluetoothUuid>();
            foreach(string serviceUuid in primaryServicesUuid)
            {
                services.Add(new BluetoothUuid(serviceUuid));
            }
			return ServiceUuidSupported(services);
		}

        public static SCardReaderList_CcidOverBle Instantiate(BLE.Adapter bleAdapter, BluetoothAddress deviceAddress, List<BluetoothUuid> deviceServicesUuid)
        {
            return Instantiate(bleAdapter, deviceAddress, deviceServicesUuid, null);
        }

        public static SCardReaderList_CcidOverBle Instantiate(BLE.Adapter bleAdapter, BluetoothAddress deviceAddress, List<BluetoothUuid> deviceServicesUuid, SecureConnectionParameters secureConnectionParameters)
		{
			SCardReaderList_CcidOverBle result = null;
            foreach(BluetoothUuid uuid in deviceServicesUuid)
            {
                switch (uuid.ToString())
                {
                    case SCardReaderList_CcidOverBle_SoMoD600.ApplicationServiceUuid:
                        result = new SCardReaderList_CcidOverBle_SoMoD600(bleAdapter, deviceAddress);
                        break;

                    case SCardReaderList_CcidOverBle_SpringCore.CcidUnbondedServiceUuid:
                        result = new SCardReaderList_CcidOverBle_SpringCore(bleAdapter, deviceAddress, secureConnectionParameters, false);
                        break;

                    case SCardReaderList_CcidOverBle_SpringCore.CcidBondedServiceUuid:
                        result = new SCardReaderList_CcidOverBle_SpringCore(bleAdapter, deviceAddress, secureConnectionParameters, true);
                        break;

                    default:
                        break;
                }
            }

            if(result == null)
            {
                return null;
            }
			

			if (!result.Connect())
			{
				return null;
			}
			
			if (!result.VerifyGattProfile())
			{
				result.CloseDevice();
				return null;
			}

            if (!result.ReadSoftwareVersion())
            {
                result.CloseDevice();
                return null;
            }

            if (!result.MakeReaderList())
            {
                result.CloseDevice();
                return null;
            }

            if (!result.EnterCcidMode())
            {
                result.CloseDevice();
                return null;
            }

            if (!result.UpdateReaderNames())
            {
                result.CloseDevice();
                return null;
            }

            if (secureConnectionParameters != null)
            {
                if (!result.OpenSecureConnection())
                {
                    result.CloseDevice();
                    return null;
                }
            }

            result.StartDevice();			
			return result;
		}
		
		private class InstantiateParams
		{
			public BackgroundInstantiateCallback Callback;
			public BLE.Adapter bleAdapter;
			public BluetoothAddress deviceAddress;
            [ObsoleteAttribute("This property is obsolete. Use deviceServicesUuid instead.", true)]
            public BluetoothUuid devicePrimaryUuid;
            public List<BluetoothUuid> deviceServicesUuid;
            public SecureConnectionParameters secureConnectionParameters;
		}
		
		public static void BackgroundInstantiate(BackgroundInstantiateCallback Callback, BLE.Adapter BleAdapter, string DeviceAddress, List<string> DeviceServicesUuid)
		{
			BackgroundInstantiate(Callback, BleAdapter, DeviceAddress, DeviceServicesUuid, null);
		}

        public static void BackgroundInstantiate(BackgroundInstantiateCallback Callback, BLE.Adapter BleAdapter, string DeviceAddress, List<string> DeviceServicesUuid, SecureConnectionParameters SecureConnection)
        {
            var PrimaryServicesUuid = new List<BluetoothUuid>();

            foreach (string uuid in DeviceServicesUuid)
            {
                   PrimaryServicesUuid.Add(new BluetoothUuid(uuid));
            }
            BackgroundInstantiate(Callback, BleAdapter, new BluetoothAddress(DeviceAddress), PrimaryServicesUuid, SecureConnection);
        }

        public static void BackgroundInstantiate(BackgroundInstantiateCallback Callback, BLE.Adapter BleAdapter, BluetoothAddress DeviceAddress, List<BluetoothUuid> DeviceServicesUuid)
        {
            BackgroundInstantiate(Callback, BleAdapter, DeviceAddress, DeviceServicesUuid, null);
        }

        public static void BackgroundInstantiate(BackgroundInstantiateCallback Callback, BLE.Adapter BleAdapter, BluetoothAddress DeviceAddress, List<BluetoothUuid> DeviceServicesUuid, SecureConnectionParameters SecureConnection)
		{
			if (Callback == null)
				return;
			
			InstantiateParams p = new InstantiateParams();
			
			p.Callback = Callback;
			p.bleAdapter = BleAdapter;
			p.deviceAddress = DeviceAddress;
			p.deviceServicesUuid = DeviceServicesUuid;
            p.secureConnectionParameters = SecureConnection;

            asyncThread = new Thread(InstantiateProc);
			asyncThread.Start(p);
		}
		
		private static void InstantiateProc(object o)
		{
			InstantiateParams p = (InstantiateParams) o;
			Logger.Trace("Background instantiate");			
			SCardReaderList_CcidOverBle instance = Instantiate(p.bleAdapter, p.deviceAddress, p.deviceServicesUuid, p.secureConnectionParameters);
			Logger.Trace("Calling the callback");			
			p.Callback(instance);
		}
    }
}
