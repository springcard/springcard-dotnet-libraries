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
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.IO;
#if !NET5_0_OR_GREATER
using System.Windows.Forms;
#endif

namespace SpringCard.LibCs.Windows
{
    /**
	 * \brief Registry-based configuration
	 */
    public class RegistryCfgFile : IConfigReader, IConfigWriter
    {
        private Logger logger;
        private RegistryCfgFile(string keyString)
        {
            logger = new Logger(this, keyString);
            if (WinUtils.Debug)
                logger.debug("Object created");
        }        

        private bool writable;
        private RegistryKey registryKey;
        public string Prefix = "";

        public void Close()
        {
            if (writable)
                registryKey.Flush();
            registryKey.Close();
        }


        private RegistryValueKind getKind(string Name)
        {
            try
            {
                RegistryValueKind result = registryKey.GetValueKind(Prefix + Name);
                return result;
            }
            catch (Exception e)
            {
                if (!(e is System.IO.IOException))
                    logger.trace("GetValueKind {0} failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                return RegistryValueKind.None;
            }
        }

        /**
		 * \brief Read a string value
		 */
        public string ReadString(string Name, string Default = "")
        {
            if (registryKey != null)
            {
                RegistryValueKind k = getKind(Name);
                if (k == RegistryValueKind.String)
                {
                    try
                    {
                        string strValue = (string)registryKey.GetValue(Prefix + Name, Default);
                        if (WinUtils.Debug)
                            logger.debug("{0} is a string, value {1}", Prefix + Name, strValue);
                        return strValue;
                    }
                    catch (Exception e)
                    {
                        logger.trace("ReadString {0} failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                    }
                }
                else if (k != RegistryValueKind.None)
                {
                    logger.trace("{0} is not suitable for ReadString ({1})", Prefix + Name, k.ToString());
                }
            }
            return Default;
        }

        /**
		 * \brief Read an integer value
		 */
        public int ReadInteger(string Name, int Default = 0)
        {
            if (registryKey != null)
            {
                RegistryValueKind k = getKind(Name);
                if (k == RegistryValueKind.String)
                {
                    try
                    {
                        string strValue = (string)registryKey.GetValue(Prefix + Name, Default.ToString());
                        if (int.TryParse(strValue, out int result))
                        {
                            if (WinUtils.Debug)
                                logger.debug("{0} is a String, value {1}, returning int {2}", Prefix + Name, strValue, result);
                            return result;
                        }
                        logger.trace("{0} is a String, value {1}, not an int", Prefix + Name, strValue);
                    }
                    catch (Exception e)
                    {
                        logger.trace("ReadInteger {0} (String) failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                    }
                }
                else if (k == RegistryValueKind.DWord)
                {
                    try
                    {
                        Int32 intValue = (Int32)registryKey.GetValue(Prefix + Name, Default);
                        if (WinUtils.Debug)
                            logger.debug("{0} is a DWord, int value {1}", Prefix + Name, (int)intValue);
                        return (int)intValue;
                    }
                    catch (Exception e)
                    {
                        logger.trace("ReadInteger {0} (DWord) failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                    }
                }
                else if (k != RegistryValueKind.None)
                {
                    logger.trace("{0} is not suitable for ReadInteger ({1})", Prefix + Name, k.ToString());
                }
            }
            return Default;
        }

        /**
		 * \brief Read an unsigned integer value
		 */
        public uint ReadUnsigned(string Name, uint Default = 0)
        {
            if (registryKey != null)
            {
                RegistryValueKind k = getKind(Name);
                if (k == RegistryValueKind.String)
                {
                    try
                    {
                        string strValue = (string)registryKey.GetValue(Prefix + Name, Default.ToString());
                        if (uint.TryParse(strValue, out uint result))
                        {
                            if (WinUtils.Debug)
                                logger.debug("{0} is a String, value {1}, returning uint {2}", Prefix + Name, strValue, result);
                            return result;
                        }
                        logger.trace("{0} is a String, value {1}, not a uint", Prefix + Name, strValue);
                    }
                    catch (Exception e)
                    {
                        logger.trace("ReadUnsigned {0} (String) failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                    }
                }
                else if (k == RegistryValueKind.DWord)
                {
                    try
                    {
                        Int32 intValue = (Int32)registryKey.GetValue(Prefix + Name, Default);
                        if (WinUtils.Debug)
                            logger.debug("{0} is a DWord, uint value {1}", Prefix + Name, (uint)intValue);
                        return (uint)intValue;
                    }
                    catch (Exception e)
                    {
                        logger.trace("ReadUnsigned {0} (DWord) failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                    }
                }
                else if (k != RegistryValueKind.None)
                {
                    logger.trace("{0} is not suitable for ReadInteger ({1})", Prefix + Name, k.ToString());
                }
            }
            return Default;
        }

        /**
		 * \brief Read a boolean value
		 */
        public bool ReadBoolean(string Name, bool Default = false)
        {
            if (registryKey != null)
            {
                RegistryValueKind k = getKind(Name);
                if (k == RegistryValueKind.String)
                {
                    try
                    {
                        string strValue = (string)registryKey.GetValue(Prefix + Name, null);
                        bool result = StrUtils.ReadBoolean(strValue, out bool valid);
                        if (valid)
                        {
                            if (WinUtils.Debug)
                                logger.debug("{0} is a String, value {1}, returning bool {2}", Prefix + Name, strValue, result);
                            return result;
                        }
                        logger.trace("{0} is a String, value {1}, not a bool", Prefix + Name, strValue);
                    }
                    catch (Exception e)
                    {
                        logger.trace("ReadBoolean {0} (String) failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                    }
                }
                else if (k == RegistryValueKind.DWord)
                {
                    try
                    {
                        Int32 intValue = (Int32)registryKey.GetValue(Prefix + Name, Default ? 1 : 0);
                        bool result = ((int)intValue != 0);
                        if (WinUtils.Debug)
                            logger.debug("{0} is a DWord, value {1} -> bool value {2}", Prefix + Name, intValue, result);
                        return result;
                    }
                    catch (Exception e)
                    {
                        logger.trace("ReadBoolean {0} (DWord) failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                    }
                }
                else if (k != RegistryValueKind.None)
                {
                    logger.trace("{0} is not suitable for ReadBoolean ({1})", Prefix + Name, k.ToString());
                }
            }
            return Default;
        }

        /**
		 * \brief Remove an entry
		 */
        public bool Remove(string Name)
        {
            if (registryKey == null)
                return false;
            if (!writable)
                return false;

            try
            {
                registryKey.DeleteValue(Prefix + Name);
                return true;
            }
            catch (Exception e)
            {
                logger.trace("Remove {0} failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                return false;
            }
        }

        /**
		 * \brief Write an empty entry
		 */
        public bool WriteName(string Name)
        {
            return WriteString(Name, null);
        }

        /**
		 * \brief Write a string entry
		 */
        public bool WriteString(string Name, string Value)
        {
            if (registryKey == null)
                return false;
            if (!writable)
                return false;

            RegistryValueKind k = getKind(Name);
            if ((k != RegistryValueKind.None) && (k != RegistryValueKind.String))
                Remove(Name);

            try
            {
                registryKey.SetValue(Prefix + Name, Value, RegistryValueKind.String);
                return true;
            }
            catch (Exception e)
            {
                logger.trace("WriteString {0} failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                return false;
            }
        }

        /**
		 * \brief Write an integer entry
		 */
        public bool WriteInteger(string Name, int Value)
        {
            if (registryKey == null)
                return false;
            if (!writable)
                return false;

            RegistryValueKind k = getKind(Name);
            if ((k != RegistryValueKind.None) && (k != RegistryValueKind.DWord))
                Remove(Name);

            try
            {
                Int32 v = (Int32)Value;
                registryKey.SetValue(Prefix + Name, v, RegistryValueKind.DWord);
                return true;
            }
            catch (Exception e)
            {
                logger.trace("WriteInteger {0} failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                return false;
            }
        }

        /**
		 * \brief Write an unsigned integer entry
		 */
        public bool WriteUnsigned(string Name, uint Value)
        {
            if (registryKey == null)
                return false;
            if (!writable)
                return false;

            RegistryValueKind k = getKind(Name);
            if ((k != RegistryValueKind.None) && (k != RegistryValueKind.DWord))
                Remove(Name);

            try
            {
                Int32 v = (Int32)Value;
                registryKey.SetValue(Prefix + Name, v, RegistryValueKind.DWord);
                return true;
            }
            catch (Exception e)
            {
                logger.trace("WriteUnsigned {0} failed with exception {1} ({2})", Prefix + Name, e.Message, e.GetType().ToString());
                return false;
            }
        }

        /**
		 * \brief Write a boolean entry
		 */
        public bool WriteBoolean(string Name, bool value)
        {
            int i = value ? 1 : 0;
            return WriteInteger(Name, i);
        }


        /**
         * \brief Open branch
         */
        public enum RegistryRoot
        {
            CurrentUser,
            LocalMachine
        }

        public static RegistryCfgFile Open(RegistryKey Key, bool Writable)
        {
            if (Key == null)
            {
                Logger.Error("RegistryCfgFile.Open: Key parameter is null");
                return null;
            }
            if (string.IsNullOrEmpty(Key.Name))
            {
                Logger.Error("RegistryCfgFile.Open: Key.Name is null");
                return null;
            }
            RegistryCfgFile result = new RegistryCfgFile(Key.Name);
            result.writable = Writable;
            result.registryKey = Key;
            return result;
        }

        public static RegistryCfgFile OpenReadOnly(RegistryKey Key)
        {
            return Open(Key, false);
        }

        public static RegistryCfgFile OpenReadWrite(RegistryKey Key)
        {
            return Open(Key, true);
        }

        private static RegistryCfgFile Open(RegistryRoot Root, string FullPath, bool Writable)
        {
            string rootStr = "????";
            if (Root == RegistryRoot.CurrentUser)
                rootStr = "HKCU";
            else if (Root == RegistryRoot.LocalMachine)
                rootStr = "HKLM";

            RegistryCfgFile result = new RegistryCfgFile(Path.Combine(rootStr, FullPath));
            result.writable = Writable;
            if (Root == RegistryRoot.CurrentUser)
            {
                if (Writable)
                {
                    result.registryKey = Registry.CurrentUser.CreateSubKey(FullPath);
                }
                else
                {
                    result.registryKey = Registry.CurrentUser.OpenSubKey(FullPath, false);
                }
                if (result.registryKey == null)
                {
                    result.logger.warning(@"Failed opening HKCU\{0} ({1})", FullPath, Writable ? "read/write" : "read only");
                }
            }
            else if (Root == RegistryRoot.LocalMachine)
            {
                if (Writable)
                {
                    result.registryKey = Registry.LocalMachine.CreateSubKey(FullPath);
                }
                else
                {
                    result.registryKey = Registry.LocalMachine.OpenSubKey(FullPath, false);
                }
                if (result.registryKey == null)
                {
                    result.logger.warning(@"Failed opening HKLM\{0} ({1})", FullPath, Writable ? "read/write" : "read only");
                }
            }
            else
            {
                result.logger.error("Invalid root");
                throw new Exception("Invalid root for RegistryCfgFile");
            }

            return result;
        }

        /**
         * \brief Open a registry path in read-only mode
         */
        public static RegistryCfgFile OpenReadOnly(RegistryRoot Root, string FullPath)
        {
            return Open(Root, FullPath, false);
        }

        /**
         * \brief Open a registry path in read-write mode
         */
        public static RegistryCfgFile OpenReadWrite(RegistryRoot Root, string FullPath)
        {
            return Open(Root, FullPath, true);
        }

        /**
         * \brief Open a CurrentUser registry path in read-only mode
         */
        public static RegistryCfgFile OpenReadOnly(string FullPath)
        {
            return Open(RegistryRoot.CurrentUser, FullPath, false);
        }

        /**
         * \brief Open a CurrentUser registry path in read-write mode
         */
        public static RegistryCfgFile OpenReadWrite(string FullPath)
        {
            return Open(RegistryRoot.CurrentUser, FullPath, true);
        }

        /**
         * \brief Open a registry company / application / section path in read-only mode
         */
        public static RegistryCfgFile OpenReadOnly(RegistryRoot Root, string CompanyName, string ApplicationName, string SectionName = "")
        {
            string FullPath = @"SOFTWARE\" + CompanyName + @"\" + ApplicationName;
            if (!string.IsNullOrEmpty(SectionName))
                FullPath += @"\" + SectionName;
            return Open(Root, FullPath, false);
        }

        /**
         * \brief Open a registry company / application / section path in read-write mode
         */
        public static RegistryCfgFile OpenReadWrite(RegistryRoot Root, string CompanyName, string ApplicationName, string SectionName = "")
        {
            string FullPath = @"SOFTWARE\" + CompanyName + @"\" + ApplicationName;
            if (!string.IsNullOrEmpty(SectionName))
                FullPath += @"\" + SectionName;
            return Open(Root, FullPath, true);
        }

        /**
         * \brief Open a CurrentUser registry company / application / section in read-only mode
         */
        public static RegistryCfgFile OpenReadOnly(string CompanyName, string ApplicationName, string SectionName = "")
        {
            return OpenReadOnly(RegistryRoot.CurrentUser, CompanyName, ApplicationName, SectionName);
        }

        /**
         * \brief Open a CurrentUser registry company / application / section in write-only mode
         */
        public static RegistryCfgFile OpenReadWrite(string CompanyName, string ApplicationName, string SectionName = "")
        {
            return OpenReadWrite(RegistryRoot.CurrentUser, CompanyName, ApplicationName, SectionName);
        }

        /**
         * \brief Open a CurrentUser registry in read-only mode for the current application
         */
        public static RegistryCfgFile OpenApplicationSectionReadOnly(string SectionName = "")
        {
            return OpenReadOnly(RegistryRoot.CurrentUser, AppUtils.CompanyName, AppUtils.ApplicationName, SectionName);
        }

        /**
         * \brief Open a CurrentUser registry company / application / section in write-only mode
         */
        public static RegistryCfgFile OpenApplicationSectionReadWrite(string SectionName = "")
        {
            return OpenReadWrite(RegistryRoot.CurrentUser, AppUtils.CompanyName, AppUtils.ApplicationName, SectionName);
        }
    }
}