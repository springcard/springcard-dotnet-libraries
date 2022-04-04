using System;
using System.Drawing;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;
using System.Security.Cryptography;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Text;
using System.Linq;
using System.IO;
using System.Security.Policy;
using System.CodeDom;
using Microsoft.Win32;

namespace SpringCard.PCSC.ReaderHelpers
{
    public class PcscBridge
    {
        private static Logger logger = new Logger(typeof(PcscBridge).FullName);
        public const string BridgeDevicesRegistryKeyName = @"SOFTWARE\SpringCard\Drivers\Devices";
        public const string BridgeRuntimeRegistryKeyName = @"SOFTWARE\SpringCard\PcscBridge\Runtime";

        public static bool IsBridgeReader(string ReaderName)
        {
            return IsBridgeReader(ReaderName, out bool dummyActive, out bool dummyConnected);
        }

        public static bool IsBridgeReader(string ReaderName, out bool IsActive, out bool IsConnected)
        {
            bool IsFound = false;
            IsActive = false;
            IsConnected = false;
#if !NET5_0_OR_GREATER
            try
            {
                RegistryKey hklmKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey bridgeDevicesKey = hklmKey.OpenSubKey(BridgeDevicesRegistryKeyName, RegistryKeyPermissionCheck.ReadSubTree);

                if (bridgeDevicesKey != null)
                {
                    foreach (string subkeyName in bridgeDevicesKey.GetSubKeyNames())
                    {
                        RegistryKey bridgeDeviceSubkey = bridgeDevicesKey.OpenSubKey(subkeyName, RegistryKeyPermissionCheck.ReadSubTree);
                        string instanceName = (string)bridgeDeviceSubkey.GetValue("Instance", "");
                        if (string.IsNullOrEmpty(instanceName))
                            continue;
                        if (!int.TryParse(instanceName, out int instanceNumber))
                            continue;
                        string deviceName = (string)bridgeDeviceSubkey.GetValue("DeviceName", "");
                        if (string.IsNullOrEmpty(deviceName))
                            continue;

                        for (int i = 0; i < 8; i++)
                        {
                            string slotField = string.Format("SlotName{0}", i);
                            if (bridgeDeviceSubkey.GetValue(slotField) == null)
                                continue;
                            string slotName = (string)bridgeDeviceSubkey.GetValue(slotField, "");
                            if (string.IsNullOrEmpty(slotName))
                                continue;

                            string possibleReaderName = string.Format("{0} {1} {2}", deviceName, slotName, instanceNumber);
                            if (possibleReaderName == ReaderName)
                            {
                                logger.debug("Reader {0} is a bridge (instance {0})", ReaderName, instanceName);
                                IsFound = true;
                                break;
                            }
                        }

                        if (IsFound)
                        {
                            string bridgeRuntimeKeyName = string.Format(@"{0}\{1}", BridgeRuntimeRegistryKeyName, instanceName);
                            RegistryKey bridgeRuntimeKey = hklmKey.OpenSubKey(bridgeRuntimeKeyName, RegistryKeyPermissionCheck.ReadSubTree);

                            if (bridgeRuntimeKey != null)
                            {
                                Int32 activeState = (Int32)bridgeRuntimeKey.GetValue("Active", (Int32)0);
                                if (activeState != 0)
                                    IsActive = true;
                                Int32 connectedState = (Int32)bridgeRuntimeKey.GetValue("Connected", (Int32)0);
                                if (connectedState != 0)
                                    IsConnected = true;
                                logger.debug("Active: {0}, Connected: {1}", activeState, connectedState);
                            }
                            else
                            {
                                logger.debug("No runtime data for bridge instance {0}", instanceName);
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.trace("Is {0} a bridge or not? Exception {1}", ReaderName, e.Message);
            }
#endif
            return IsFound;
        }
    }
}
