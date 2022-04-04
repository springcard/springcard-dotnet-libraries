using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using SpringCard.LibCs;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage.Streams;
using System.Text;
using System.Threading;

namespace SpringCard.Bluetooth
{
    public partial class BLE
    {
        private static HashtagChris.DotNetBlueZ.Adapter bluez_adapter = null;


        public static class AsyncHelper
        {
            private static readonly TaskFactory _taskFactory = new
                TaskFactory(CancellationToken.None,
                            TaskCreationOptions.None,
                            TaskContinuationOptions.None,
                            TaskScheduler.Default);

            public static TResult RunSync<TResult>(Func<Task<TResult>> func)
                => _taskFactory
                    .StartNew(func)
                    .Unwrap()
                    .GetAwaiter()
                    .GetResult();

            public static void RunSync(Func<Task> func)
                => _taskFactory
                    .StartNew(func)
                    .Unwrap()
                    .GetAwaiter()
                    .GetResult();
        }


        internal class LinuxDeviceInfo : DeviceInfo
        {

            public void Update(HashtagChris.DotNetBlueZ.Device1Properties deviceProperties)
            {
                if (!string.IsNullOrEmpty(deviceProperties.Name))
                {
                    this.Name = deviceProperties.Name;
                }

                if (deviceProperties.Paired)
                {
                    this.DeviceStatus = BleDeviceStatus.Connected;
                    this.BondingStatus = BleBondingStatus.Bonded;
                }
                else if (deviceProperties.Connected)
                {
                    this.DeviceStatus = BleDeviceStatus.Connected;
                    this.BondingStatus = BleBondingStatus.NotBonded;
                }
#if false       
                /* Log all device's properties */
                foreach (var info in deviceProperties.GetType().GetProperties())
                {
                    try
                    {
                        var value = info.GetValue(deviceProperties) ?? "(null)";
                        Logger.Debug("\t" + info.Name + "= " + value.ToString());
                    }
                    catch
                    {
                        Logger.Warning("Error while displaying object's info");
                    }
                }
#endif 
                if (!string.IsNullOrEmpty(deviceProperties.Name))
                {
                    this.Name = deviceProperties.Name;
                }


                if ((deviceProperties.ServiceData != null) && (deviceProperties.ServiceData.Count >= 1))
                {
                    Logger.Debug("ServiceData:");
                    foreach (var data in deviceProperties.ServiceData)
                    {
                        Logger.Debug(data.Key + " " + data.Value);
                    }
                   // this.PrimaryServiceUuid = new BluetoothUuid(deviceProperties.ServiceData[0].ToString()) ;
                }

#if false 
                if ((deviceProperties.ManufacturerData != null) && (deviceProperties.ManufacturerData.Count >= 1))
                {
                    Logger.Debug("ManufacturerData:");
                    foreach (var data in deviceProperties.ManufacturerData)
                    {
                        Logger.Debug(data.Key+" "+data.Value);
                    }

                }
#endif 
                if ((deviceProperties.UUIDs != null) && (deviceProperties.UUIDs.Length >= 1))
                {
                    this.PrimaryServicesUuid = new List<BluetoothUuid>();
                    Logger.Debug("UUIDs:");
                    foreach (string uuid in deviceProperties.UUIDs)
                    {
                        Logger.Debug(uuid);
                        this.PrimaryServicesUuid.Add(new BluetoothUuid(uuid));
                    }                    
                }

                this.rssiValue = deviceProperties.RSSI;
                this.rssiValid = true;
            }

            public LinuxDeviceInfo(BluetoothAddress Address, HashtagChris.DotNetBlueZ.Device1Properties deviceProperties) : base(Address)
            {
                this.DeviceStatus = BleDeviceStatus.Advertising;
                Update(deviceProperties);
            }



        }

#region Device

        

        public class LinuxDevice : Device
        {
            internal LinuxDevice(BluetoothAddress Address)
            {
                this.Address = Address;
            }

            private class GattServiceCached
            {
                public IGattService1 service;
                public GattService1Properties properties;
                public GattCharacteristicCached[] characteristics;
                public GattServiceCached(IGattService1 _service, GattService1Properties _properties, GattCharacteristicCached[] _characteristics)
                {
                    service = _service;
                    properties = _properties;
                    characteristics = _characteristics;
                }
            }

            private class GattCharacteristicCached
            {
                public GattCharacteristic characteristic;
                public GattCharacteristic1Properties properties;
                public GattCharacteristicCached(GattCharacteristic _characteristic, GattCharacteristic1Properties _properties)
                {
                    characteristic = _characteristic;
                    properties = _properties;

                }
            }

            private class BleDeviceData
            {
                public List<GattServiceCached> services;
                public List<GattCharacteristicCached> characteristics;
                public List<GattCharacteristicCached> eventedCharacteristics;
            }

            private static Dictionary<string, BleDeviceData> deviceDataCache = new Dictionary<string, BleDeviceData>();

            private BleDeviceData deviceData;

            private HashtagChris.DotNetBlueZ.Device linuxBLEDevice;


            private GattCharacteristicCached FindCharacteristic(BluetoothUuid characteristicUuid)
            {
                if (deviceData.characteristics == null)
                    return null;
                foreach (GattCharacteristicCached characteristic in deviceData.characteristics)
                {
                    BluetoothUuid u = new BluetoothUuid(characteristic.properties.UUID);
                    if (u.Equals(characteristicUuid))
                    {
                        return characteristic;
                    }
                }
                return null;
            }

            private void HandleCommunicationException(Exception e)
            {
                Logger.Trace("Exception {0} ({1}/{2:X08})", e.Message, e.HResult, e.HResult);
            }

            private void defaultPairingRequestHandler(DeviceInformationCustomPairing CP, DevicePairingRequestedEventArgs DPR)
            {
                //so we get here for custom pairing request.
                //this is the magic place where your pin goes.
                //my device actually does not require a pin but
                //Linux requires at least a "0".  So this solved 
                //it.  This does not pull up the Linux UI either.
                DPR.Accept();
            }

            private async Task<bool> ConnectAsync(BluetoothAddress deviceAddress, bool bondingRequired)
            {
                TimeSpan timeout = TimeSpan.FromSeconds(10);
                string strAddress = deviceAddress.ToString();


                linuxBLEDevice = await bluez_adapter.GetDeviceAsync(strAddress);

                if (linuxBLEDevice == null)
                {
                    Console.WriteLine($"Bluetooth peripheral with address '{deviceAddress}' not found. Use `bluetoothctl` or Bluetooth Manager to scan and possibly pair first.");
                    return false;
                }

                deviceData = null;
                if (deviceDataCache.ContainsKey(strAddress))
                {
                    // TODO : JDA check code avec SAL
                    deviceData = deviceDataCache[strAddress];
                    deviceDataCache.Remove(strAddress);
                }
                if (deviceData == null)
                    deviceData = new BleDeviceData();

                linuxBLEDevice.Disconnected += OnDeviceDisconnected;

                Logger.Debug("Connecting to device " + strAddress);
                await linuxBLEDevice.ConnectAsync();
                await linuxBLEDevice.WaitForPropertyValueAsync("Connected", value: true, timeout);
                await linuxBLEDevice.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);
                Logger.Debug("ServicesResolved ");


                await ReadGattAsync(false, false);
                DumpGatt();

                deviceDataCache[strAddress] = deviceData;

                Logger.Debug("Connected!");
                return true;
            }

            protected override bool Connect(bool bondingRequired)
            {

                bool result = AsyncHelper.RunSync(() => ConnectAsync(Address, bondingRequired));

                return result;
            }


            public override void Disconnect()
            {
                if (linuxBLEDevice != null)
                {
                    Logger.Debug("Disconnecting...");

                    if (deviceData.eventedCharacteristics != null)
                    {
                        foreach (GattCharacteristicCached characteristic in deviceData.eventedCharacteristics)
                        {
                            characteristic.characteristic.Value -= OnCharacteristicNotification;
                        }
                    }

                    linuxBLEDevice.DisconnectAsync();



                   // linuxBLEDevice.Disconnected -= OnDeviceDisconnected;
                }
                else
                {
                    Logger.Debug("(Already disconnected)");
                }

                Connected = false;
            }


            private async Task OnDeviceDisconnected(HashtagChris.DotNetBlueZ.Device sender, BlueZEventArgs eventArgs)
            {
                Logger.Trace("BLE Device {0}: disconnected", Address.ToString());
                linuxBLEDevice.Dispose();
                linuxBLEDevice = null;
                ReportDisconnect();
            }


            private async Task OnCharacteristicNotification(GattCharacteristic sender, GattCharacteristicValueEventArgs eventArgs)
            {
                string uuid_str = await sender.GetUUIDAsync();
                BluetoothUuid uuid = new BluetoothUuid(uuid_str);

                Logger.Debug("BLE Notification on {0}", uuid.ToString());

                byte[] buffer = new byte[eventArgs.Value.Length];
                eventArgs.Value.CopyTo(buffer, 0);

                Logger.Debug(">" + BinConvert.ToHex(buffer));

                ReportNotification(uuid, buffer);
            }



            private async Task<bool> EnableCharacteristicEventsAsync(BluetoothUuid characteristicUuid)
            {
                Logger.Debug("Enable notification on characteristic " + characteristicUuid.ToString());
                var services = await linuxBLEDevice.GetAllAsync();
                foreach (var serviceUuid in services.UUIDs)
                {
                    var service = await linuxBLEDevice.GetServiceAsync(serviceUuid);
                    GattCharacteristic characteristic = await service.GetCharacteristicAsync(characteristicUuid.ToString());

                    if (characteristic == null)
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " not found in service " + serviceUuid);
                        continue;
                    }

                    GattCharacteristic1Properties properties = await characteristic.GetAllAsync();
                    if (!properties.Flags.Contains("indicate") && !properties.Flags.Contains("notify"))
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " has no indication/notification");
                        return false;
                    }

                    Logger.Debug("Enabling events for " + characteristicUuid.ToString());

                    characteristic.Value += OnCharacteristicNotification;


                    if (deviceData.eventedCharacteristics == null)
                         deviceData.eventedCharacteristics = new List<GattCharacteristicCached>();
                     deviceData.eventedCharacteristics.Add(new GattCharacteristicCached(characteristic, properties));

                    return true;

                }

                Logger.Trace("Characteristic " + characteristicUuid.ToString() + " not found in GATT ");
                return false;
            }


            /* Defined on Windows but not on Linux, so we define it here */
            private enum GattCommunicationStatus
            {
                Success = 0,
                Unreachable = 1,
                ProtocolError = 2,
                AccessDenied = 3
            }
            private void HandleCommunicationError(GattCommunicationStatus status)
              {
                  Logger.Trace("Error: {0}", status.ToString());
                  if (linuxBLEDevice != null)
                  {
                      Disconnect();
                  }
              }

            public override bool EnableCharacteristicEvents(BluetoothUuid characteristicUuid)
            {
                return AsyncHelper.RunSync(() => EnableCharacteristicEventsAsync(characteristicUuid));
            }

            private async Task<byte[]> ReadCharacteristicAsync(BluetoothUuid characteristicUuid)
            {
                try
                {
                    Logger.Debug("BLE Read({0})", characteristicUuid.ToString());

                    GattCharacteristicCached characteristic = FindCharacteristic(characteristicUuid);

                    if (characteristic == null)
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " not found");
                        return null;
                    }

                    GattCharacteristic1Properties properties = characteristic.properties;

                    if (!properties.Flags.Contains("read"))
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " is not readable");
                        return null;
                    }
                    TimeSpan timeout = new TimeSpan(0, 0, 2); // 2 sec
                    byte[] result = await characteristic.characteristic.ReadValueAsync(timeout);

                    if ((result == null) )
                    {
                        Logger.Trace("Read from characteristic " + characteristicUuid.ToString() + " failed");
    
                        HandleCommunicationError(GattCommunicationStatus.Unreachable);
                        return null;
                    }

                    if (result.Length == 0)
                        return new byte[0];

                    byte[] buffer = result.ToArray();
                    Logger.Debug(">" + BinConvert.ToHex(buffer));

                    return buffer;
                }
                catch (Exception e)
                {
                    Logger.Trace("Failed to read " + characteristicUuid.ToString());
                    Logger.Trace(string.Format("(Error {0}/{1:X08}: {2})", e.HResult, e.HResult, e.Message));
                    return null;
                }
            }

            public override byte[] ReadCharacteristic(BluetoothUuid characteristicUuid)
            {
                return ReadCharacteristicAsync(characteristicUuid).Result;
            }


            public override byte[] ReadCharacteristic(BluetoothUuid characteristicUuid, bool allowCache)
            {
                /* Cache cannot be used with Bluez on Linux */
                return ReadCharacteristicAsync(characteristicUuid).Result;
            }

            private async Task<bool> ReadGattAsync(bool allowCache, bool ignoreAccessErrors)
            {
                Logger.Debug("Retrieving the list of services from the GATT (cache={0}, ignore access errors={1})...", allowCache, ignoreAccessErrors);

                deviceData.services = new List<GattServiceCached>();
                deviceData.characteristics = new List<GattCharacteristicCached>();

                var services =  await (linuxBLEDevice.GetServicesAsync());

                foreach (var service in services)
                {
                    var serviceProperties = await (service.GetAllAsync());
                    var characteristics = await (service.GetCharacteristicsAsync());
                    var characteristicsFinal = new List<GattCharacteristicCached>();

                    for (int i=0; i< characteristics.Count; i++)
                    {
                        var characteristicProperties = await (characteristics[i].GetAllAsync());
                        GattCharacteristic realCharacteristic = await service.GetCharacteristicAsync(characteristicProperties.UUID);
                        var charFinale = new GattCharacteristicCached(realCharacteristic, characteristicProperties);
                        characteristicsFinal.Add(charFinale);
                        deviceData.characteristics.Add(charFinale);
                    }

                    deviceData.services.Add(new GattServiceCached(service, serviceProperties, characteristicsFinal.ToArray()));
                }


                return true;
            }

            private async Task<bool> WriteCharacteristicAsync(BluetoothUuid characteristicUuid, byte[] buffer)
            {
                try
                {
                    Logger.Debug("BLE Write({0})", characteristicUuid.ToString());

                    GattCharacteristicCached characteristic = FindCharacteristic(characteristicUuid);

                    if (characteristic == null)
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " not found");
                        return false;
                    }

                    GattCharacteristic1Properties properties = characteristic.properties;

                    if (!properties.Flags.Contains("write"))
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " is not writable");
                        return false;
                    }

                    TimeSpan timeout = new TimeSpan(0, 0, 2); // 2 sec

                    Logger.Debug("<" + BinConvert.ToHex(buffer));

                    var options = new Dictionary<string, object>();
                    options.Add("type", "reliable");

                    var readTask = characteristic.characteristic.WriteValueAsync(buffer, options);
                    var timeoutTask = Task.Delay(timeout);

                    await Task.WhenAny(new Task[] { readTask, timeoutTask });
                    if (!readTask.IsCompleted)
                    {
                        Logger.Trace("Write to characteristic " + characteristicUuid.ToString() + " failed");

                        HandleCommunicationError(GattCommunicationStatus.Unreachable);
                        return false;
                    }

                     return true;
                }
                catch (Exception e)
                {
                    Logger.Trace("Failed to write " + characteristicUuid.ToString());
                    HandleCommunicationException(e);
                    Logger.Trace(string.Format("(Error {0}/{1:X08}: {2})", e.HResult, e.HResult, e.Message));
                    return false;
                }
            }

            public override bool WriteCharacteristic(BluetoothUuid characteristicUuid, byte[] value)
            {
                return AsyncHelper.RunSync(() => WriteCharacteristicAsync(characteristicUuid, value));
            }

            public bool ReadGatt(bool allowCache, bool ignoreAccessErrors)
            {
                if (linuxBLEDevice == null)
                    return false;

                if (!AsyncHelper.RunSync(() => ReadGattAsync(allowCache, ignoreAccessErrors)))
                    return false;
                    
                DumpGatt();
                return true;
            }

            private void DumpGatt()
            {
                Logger.Debug("GATT tree:");
                foreach (GattServiceCached service in deviceData.services)
                {

                    Logger.Debug("+-- " + service.properties.UUID.ToString());

                    foreach (GattCharacteristicCached characteristic in deviceData.characteristics)
                    {

                        if (service.characteristics.Contains(characteristic))
                        {
                            BluetoothUuid characteristicUuid = new BluetoothUuid(characteristic.properties.UUID);
                            Logger.Debug($"  +-- {characteristicUuid.ToString()} ({String.Join(" ", characteristic.properties.Flags)})");
                        }
                    }
                }
#if false
                Logger.Debug("GATT characteristics:");
                foreach (GattCharacteristicCached characteristic in deviceData.characteristics)
                {
                    Logger.Debug("+-- " + characteristic.properties.UUID.ToString());
                }
#endif
            }

            public override BluetoothUuid[] GetGattServices()
            {
                if (deviceData.services == null)
                    return null;

                BluetoothUuid[] result = new BluetoothUuid[deviceData.services.Count];
                for (int i = 0; i < result.Length; i++)
                    result[i] = new BluetoothUuid(deviceData.services[i].properties.UUID);

                return result;
            }

            public override BluetoothUuid[] GetGattCharacteristics(BluetoothUuid serviceUuid)
            {
                if (deviceData.characteristics == null)
                    return null;

                foreach (var service in deviceData.services)
                {
                    if (service.properties.UUID.ToUpper() == serviceUuid.ToString().ToUpper())
                    {
                        BluetoothUuid[] result = new BluetoothUuid[service.characteristics.Length];
                        int c = 0;
                        foreach (var characteristic in service.characteristics)
                        {
                            BluetoothUuid characteristicUuid = new BluetoothUuid(characteristic.properties.UUID);
                            result[c++] = characteristicUuid;
                        }
                        return result;
                    }
                }

                return null;
            }
        }

#endregion

        public sealed class LinuxAdapter : Adapter
        {
            private static volatile LinuxAdapter instance;
            private static object locker = new Object();



            public static LinuxAdapter Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (locker)
                        {
                            if (instance == null)
                                instance = new LinuxAdapter();
                        }
                    }
                    return instance;
                }
            }

            private LinuxAdapter()
            {

            }

            ~LinuxAdapter()
            {
                Close();
            }


            public override bool DeleteAllBondings()
            {
                Logger.Error("Windows BLE API doesn't offer the Delete All Bondings function");
                return false;
            }


            public override bool ClearSystemCache()
            {
#if false
                /* Clear device by removing dir and file as root */
                Logger.Warning("Warning, on Linux system, you need to be use sudo or be root to execute this command");
                var output = @"rm /var/lib/bluetooth/DC\:A6\:32\:51\:5B\:24/cache/90\:FD\:9F\:6E\:33\:6F".Bash();
                if(output != String.Empty)
                {
                    Logger.Error("Clearing Device from system's cache failed, error:\n" + output);
                    return false;
                }

                output = @"rm -r /var/lib/bluetooth/DC\:A6\:32\:51\:5B\:24/90\:FD\:9F\:6E\:33\:6F".Bash();
                if (output != String.Empty)
                {
                    Logger.Error("Clearing Device from system's cache failed, error:\n" + output);
                    return false;
                }
#endif
                /* Deletes all devices with D-Bus interface */
                var devices = AsyncHelper.RunSync(() => bluez_adapter.GetDevicesAsync());
                foreach (var device in devices)
                {                    
                    AsyncHelper.RunSync(() => bluez_adapter.RemoveDeviceAsync(device.ObjectPath));
                }

                return false;
            }

            public override Device CreateDevice(BluetoothAddress Address)
            {
                return new LinuxDevice(Address);
            }


            // public const string AllSelectorString = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";

            // public enum SelectorFilter { All, Unpaired, Paired };


#region Override, global

            private bool isScanning;


            private new LinuxDeviceInfo GetDeviceInfo(BluetoothAddress address)
            {
                if (scanResults.ContainsKey(address.ToString()))
                    return (LinuxDeviceInfo)scanResults[address.ToString()];
                return null;
            }


            private async Task OnAdvertisementReceived(HashtagChris.DotNetBlueZ.Adapter sender, DeviceFoundEventArgs eventArgs)
            {
               try
                {
                    Device1Properties deviceProperties = await eventArgs.Device.GetAllAsync();
                    lock (locker)
                    {
                        var bt_addr = deviceProperties.Address.ToString();
                        BluetoothAddress address = new BluetoothAddress(bt_addr);
                        LinuxDeviceInfo deviceInfo = GetDeviceInfo(address);

                        if (deviceInfo == null)
                        {
                            Logger.Trace("BLE Scan <- {0}", address.ToString());
                            deviceInfo = new LinuxDeviceInfo(address, deviceProperties);
                        }
                        else
                        {
                            deviceInfo.Update(deviceProperties);
                        }

                        SetDeviceInfo(deviceInfo);
                    }
                }
                catch(Exception e)
                {
                    Logger.Error($"{e.Message}");
                }
            }

            public override bool Open()
            {
                bluez_adapter = AsyncHelper.RunSync(() => BlueZManager.GetAdaptersAsync()).FirstOrDefault();
                
                var adapterPath = bluez_adapter.ObjectPath.ToString();
                var adapterName = adapterPath.Substring(adapterPath.LastIndexOf("/") + 1);
                Logger.Debug($"Using Bluetooth adapter {adapterName}");


                // Print out the devices we already know about.

               /* var devices = AsyncHelper.RunSync(() => bluez_adapter.GetDevicesAsync());

                foreach (var device in devices)
                {
                    var bt_addr = AsyncHelper.RunSync(() => device.GetAddressAsync());
                    BluetoothAddress address = new BluetoothAddress(bt_addr);
                    LinuxDeviceInfo deviceInfo = GetDeviceInfo(address);

                    if (deviceInfo == null)
                    {
                        Logger.Trace("BLE known <- {0}", address.ToString());
                        deviceInfo = new LinuxDeviceInfo(address, device);
                    }
                    else
                    {
                        deviceInfo.Update(device);
                    }

                    SetDeviceInfo(deviceInfo);
                }
                Logger.Debug($"{devices.Count} device(s) found ahead of scan.");*/

                return true;
            }

            public override void Close()
            {
                StopScan();
            }


            public override bool StartScan()
            {
                if (bluez_adapter == null)
                {
                    Logger.Error("BLE:BlueZ adapter is null");
                    return false;
                }

                ClearSystemCache();

                Logger.Debug("BLE:Start scan");

                bluez_adapter.DeviceFound += OnAdvertisementReceived;
                bluez_adapter.StartDiscoveryAsync();
                               
                // No  OnDeviceRmoved callback in bluez stack?? bluez_adapter.DeviceRemoved += OnAdvertisementReceived;

                Logger.Debug("BLE:Scan started");
                return true;
            }

            public override void StopScan()
            {
                if (bluez_adapter == null)
                {
                    Logger.Error("BLE:BlueZ adapter is null");
                    return;
                }

                bluez_adapter.DeviceFound -= OnAdvertisementReceived;

                Logger.Debug("BLE:Stop scan");
                bluez_adapter.StopDiscoveryAsync();
               
                Logger.Debug("BLE:Scan stopped");
            }

#endregion
        }

    }
}
