using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Xml.XPath;

namespace SpringCard.LibCs.Windows
{
    public sealed partial class Drivers
    {
        private const string SPRINGCARD = "SPRINGCARD";

        private static class NativeMethods
        {
            [DllImport("SetupAPI.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool SetupGetInfDriverStoreLocation(
                [MarshalAs(UnmanagedType.LPTStr)] string FileName,
                Int32 AlternatePlatformInfo,
                Int32 LocalName,
                StringBuilder ReturnBuffer,
                Int32 ReturnBufferSize,
                out Int32 RequiredSize);
        }

        public class DriverInfo
        {
            public string InfAlias;
            public string InfFileName;
            public string InfSubDirectory;

            public string CatalogFile;

            public string DriverVer;
            public string Class;
            public string ClassGuid;
            public string Provider;           

            public string Name;
            public string Version;
            public string Date;
        }

        private static string GetInfFileName(string InfName)
        {
            try
            {
                NativeMethods.SetupGetInfDriverStoreLocation(InfName, 0, 0, null, 0, out int RequiredSize);
                StringBuilder ReturnBuffer = new StringBuilder(RequiredSize);
                NativeMethods.SetupGetInfDriverStoreLocation(InfName, 0, 0, ReturnBuffer, ReturnBuffer.Capacity, out RequiredSize);
                return ReturnBuffer.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static string TranslateInfString(IniFile inifile, string Text)
        {
            if (Text.StartsWith("%") && Text.EndsWith("%"))
            {
                Text = Text.Replace("%", "");
                Text = inifile.ReadString("Strings", Text, Text);
                if (Text.StartsWith("\"") && Text.EndsWith("\""))
                    Text = Text.Replace("\"", "");
            }
            return Text;
        }

        private static DriverInfo ReadInfFile(string InfFile)
        {
            DriverInfo result = new DriverInfo();
            IniFile inifile = IniFile.OpenReadWrite(InfFile);

            result.InfFileName = InfFile;
            result.CatalogFile = inifile.ReadString("Version", "CatalogFile");
            result.Provider = TranslateInfString(inifile, inifile.ReadString("Version", "Provider"));
            result.DriverVer = inifile.ReadString("Version", "DriverVer");
            result.ClassGuid = inifile.ReadString("Version", "ClassGuid");
            result.Class = TranslateInfString(inifile, inifile.ReadString("Version", "Class"));

            result.InfSubDirectory = Path.GetFileName(Path.GetDirectoryName(InfFile));
            result.Name = Path.GetFileNameWithoutExtension(result.CatalogFile);

            string[] e = result.DriverVer.Split(',');
            if (e.Length == 2)
            {
                e[0] = e[0].Trim();
                e[1] = e[1].Trim();

                string[] d = e[0].Split('/');
                if (d.Length == 3)
                {
                    result.Date = String.Format("{0}/{1}/{2}", d[1], d[0], d[2]);
                }
                else
                {
                    result.Date = e[0];
                }

                result.Version = e[1];
            }
            else
            {
                result.Date = result.DriverVer;
                result.Version = result.DriverVer;
            }

            return result;
        }

        private static string GetInfAlias(string InfString)
        {
            try
            {
                string result = InfString;

                string[] e;
                e = result.Split(';');
                if (e.Length > 1)
                    result = e[0];
                e = result.Split(',');
                if (e.Length > 1)
                    result = e[0];

                if (result.StartsWith("@"))
                    result = result.Substring(1);

                return result;
            }
            catch
            {
                return null;
            }
        }

        public static List<DriverInfo> EnumDrivers()
        {
            Dictionary<string,string> InfFiles = new Dictionary<string, string>();

            /* Enumerate USB devices */
            /* --------------------- */

            try
            {
                RegistryKey keyUsbEnum = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\Usb");
                foreach (string strUsbVidPid in keyUsbEnum.GetSubKeyNames())
                {
                    try
                    {
                        RegistryKey keyUsbVidPid = keyUsbEnum.OpenSubKey(strUsbVidPid);
                        foreach (string strDevice in keyUsbVidPid.GetSubKeyNames())
                        {
                            try
                            {
                                RegistryKey keyDevice = keyUsbVidPid.OpenSubKey(strDevice);

                                string deviceDesc = (string)keyDevice.GetValue("DeviceDesc");
                                string deviceMfg = (string)keyDevice.GetValue("Mfg");

                                if (deviceDesc.ToUpper().Contains(SPRINGCARD) || deviceMfg.ToUpper().Contains(SPRINGCARD))
                                {
                                    string deviceInfAlias = GetInfAlias(deviceMfg);
                                    if (!string.IsNullOrEmpty(deviceInfAlias))
                                    {
                                        string deviceInfFileName = GetInfFileName(deviceInfAlias);
                                        if (!string.IsNullOrEmpty(deviceInfFileName))
                                        {
                                            if (!InfFiles.ContainsKey(deviceInfAlias))
                                                InfFiles[deviceInfAlias] = deviceInfFileName;
                                        }
                                    }
                                }

                                keyDevice.Close();
                            }
                            catch { }
                        }
                        keyUsbVidPid.Close();
                    }
                    catch { }
                }
                keyUsbEnum.Close();
            }
            catch { }

            /* Enumerate SmartCard readers */
            /* --------------------------- */

            try
            {
                RegistryKey keySmartCardReaderEnum = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\ROOT\SMARTCARDREADER");
                foreach (string strSmartCardReader in keySmartCardReaderEnum.GetSubKeyNames())
                {
                    try
                    {
                        RegistryKey keySmartCardReader = keySmartCardReaderEnum.OpenSubKey(strSmartCardReader);

                        string deviceDesc = (string)keySmartCardReader.GetValue("DeviceDesc");
                        string deviceMfg = (string)keySmartCardReader.GetValue("Mfg");

                        if (deviceDesc.ToUpper().Contains(SPRINGCARD) || deviceMfg.ToUpper().Contains(SPRINGCARD))
                        {
                            string deviceInfAlias = GetInfAlias(deviceMfg);
                            if (!string.IsNullOrEmpty(deviceInfAlias))
                            {
                                string deviceInfFileName = GetInfFileName(deviceInfAlias);
                                if (!string.IsNullOrEmpty(deviceInfFileName))
                                {
                                    if (!InfFiles.ContainsKey(deviceInfAlias))
                                        InfFiles[deviceInfAlias] = deviceInfFileName;
                                }
                            }
                        }

                        keySmartCardReader.Close();
                    }
                    catch { }
                }
                keySmartCardReaderEnum.Close();
            }
            catch { }

            List<DriverInfo> result = new List<DriverInfo>();

            foreach (KeyValuePair<string,string> InfFile in InfFiles)
            {
                try
                {
                    DriverInfo info = ReadInfFile(InfFile.Value);
                    info.InfAlias = InfFile.Key;
                    result.Add(info);
                }
                catch { }
            }

            return result;
        }


    }
}