using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class Desfire
    {

        public const byte AppEntriesCount = 32;
        public class FileEntry
        {

            /// <summary>
            /// File type
            /// </summary>
            public enum FILE_TYPE : byte
            {
                Standard    = 0x01,
                Backup      = 0x02,
                Value       = 0x04,
                Linear      = 0x08,
                Cyclic      = 0x10,
            }

            public FILE_TYPE Type;

            /// <summary>
            /// standard, backup, file
            /// </summary>
            public byte FileNo;
            public byte[] FileIsoID;
            public byte ComSet;
            public byte[] AccessRights;
            public short sAccessRights
            {
                get
                {
                    if ((AccessRights == null) || (AccessRights.Length != 2))
                        return 0;
                    return BitConverter.ToInt16(AccessRights, 0);
                }
                set
                {
                    AccessRights = BitConverter.GetBytes(value);
                }
            }
            public byte[] FileSize;
            public Int32 iFileSize
            {
                get
                {
                    if ((FileSize == null) || (FileSize.Length != 4))
                        return 0;
                    return BitConverter.ToInt32(FileSize, 0);
                }
                set
                {
                    FileSize = BitConverter.GetBytes(value);
                }
            }
            //public byte[] data;
            /// <summary>
            /// value file
            /// </summary>
            public byte[] LowerLimit;
            public Int32 iLowerLimit
            {
                get
                {
                    if ((LowerLimit == null) || (LowerLimit.Length != 4))
                        return 0;
                    return BitConverter.ToInt32(LowerLimit, 0);
                }
                set
                {
                    LowerLimit = BitConverter.GetBytes(value);
                }
            }
            public byte[] UpperLimit;
            public Int32 iUpperLimit
            {
                get
                {
                    if ((UpperLimit == null) || (UpperLimit.Length != 4))
                        return 0;
                    return BitConverter.ToInt32(UpperLimit, 0);
                }
                set
                {
                    UpperLimit = BitConverter.GetBytes(value);
                }
            }
            public byte[] Value;
            public Int32 iValue
            {
                get
                {
                    if ((Value == null) || (Value.Length != 4))
                        return 0;
                    return BitConverter.ToInt32(Value, 0);
                }
                set
                {
                    Value = BitConverter.GetBytes(value);
                }
            }
            public byte LimitedCreditEnabled;

            /// <summary>
            /// linear, cyclic record file
            /// </summary>
            public byte[] RecordSize;
            public Int32 iRecordSize
            {
                get
                {
                    if ((RecordSize == null) || (RecordSize.Length != 4))
                        return 0;
                    return BitConverter.ToInt32(RecordSize, 0);
                }
                set
                {
                    RecordSize = BitConverter.GetBytes(value);
                }
            }
            public byte[] MaxNumOfRecords;
            public Int32 iMaxNumOfRecords
            {
                get
                {
                    if ((MaxNumOfRecords == null) || (MaxNumOfRecords.Length != 4))
                        return 0;
                    return BitConverter.ToInt32(MaxNumOfRecords, 0);
                }
                set
                {
                    MaxNumOfRecords = BitConverter.GetBytes(value);
                }
            }
            public byte[] Offset;
            public Int32 iOffset
            {
                get
                {
                    if ((Offset == null) || (Offset.Length != 3))
                        return 0;
                    return BitConverter.ToInt32(Offset, 0);
                }
                set
                {
                    Offset = BitConverter.GetBytes(value);
                }
            }
            public byte[] Length;
            public Int32 iLength
            {
                get
                {
                    if ((Length == null) || (Length.Length != 3))
                        return 0;
                    return BitConverter.ToInt32(Length, 0);
                }
                set
                {
                    Length = BitConverter.GetBytes(value);
                }
            }
            
            public byte[] Data;
        }
        public class AppEntry
        {

            public AppEntry Clone()
            {
                return (AppEntry)this.MemberwiseClone();
            }

            /// <summary>
            /// Key Settings 1
            /// </summary>
            [Flags]
            public enum APP_SETTINGS : byte
            {
                NotAlowChangeMaster = 0x01,
                FreeDirectoryListAccessWithoutMaster = 0x02,
                FreeCreateDeleteWithoutMasterKey = 0x04,
                ConfigurationChangeable = 0x08,
                ChangeKeyAccessRight = 0xF0,
            }

            public bool NotAlowChangeMaster;
            public bool FreeDirectoryListAccessWithoutMaster;
            public bool FreeCreateDeleteWithoutMasterKey;
            public bool ConfigurationChangeable;
            public byte ChangeKeyAccessRight;
            public uint Aid;

            public byte SET_KEYSETTINGS1
            {
                get
                {
                    byte result = 0;

                    if (NotAlowChangeMaster)
                        result |= (byte)APP_SETTINGS.NotAlowChangeMaster;
                    if (FreeDirectoryListAccessWithoutMaster)
                        result |= (byte)APP_SETTINGS.FreeDirectoryListAccessWithoutMaster;
                    if (FreeCreateDeleteWithoutMasterKey)
                        result |= (byte)APP_SETTINGS.FreeCreateDeleteWithoutMasterKey;
                    if (ConfigurationChangeable)
                        result |= (byte)APP_SETTINGS.ConfigurationChangeable;

                    result |= ChangeKeyAccessRight;

                    return result;
                }
                set
                {
                    NotAlowChangeMaster = ((value & (byte)APP_SETTINGS.NotAlowChangeMaster) != 0);
                    FreeDirectoryListAccessWithoutMaster = ((value & (byte)APP_SETTINGS.FreeDirectoryListAccessWithoutMaster) != 0);
                    FreeCreateDeleteWithoutMasterKey = ((value & (byte)APP_SETTINGS.FreeCreateDeleteWithoutMasterKey) != 0);
                    ConfigurationChangeable = ((value & (byte)APP_SETTINGS.ConfigurationChangeable) != 0);
                    NumberKeyPerApp = (byte)(value & 0x0F);
                    ChangeKeyAccessRight = (byte)(value & 0xF0);
                }
            }

            /// <summary>
            /// Key Settings 2
            /// </summary>
            [Flags]
            public enum ISO_IEC : byte
            {
                NoTwoByteFileId = 0x00,
                TwoByteFileId = 0x20,
            }            

            [Flags]
            public enum CRYPTO_METHOD : byte
            {
                Des = 0x00,
                K3des = 0x40,
                Aes = 0x80,
            }
            public CRYPTO_METHOD CryptoMethod;
            
            public bool FileIdentifierSupported;
            public byte NumberKeyPerApp;
            public byte SET_KEYSETTINGS2
            {
                get
                {
                    byte result = 0;

                    if (FileIdentifierSupported)
                        result |= (byte)ISO_IEC.TwoByteFileId;

                    result |= NumberKeyPerApp;
                    result |= (byte)CryptoMethod;                

                    return result;
                }
                set
                {
                    FileIdentifierSupported = ((value & (byte)ISO_IEC.TwoByteFileId) != 0);
                    NumberKeyPerApp = (byte)(value & 0x0F);
                    CryptoMethod = (CRYPTO_METHOD)(value & 0xC0);
                }
            }

            /// <summary>
            /// ISO SETTINGS
            /// </summary>
            public byte[] IsoFileID;
            public byte[] IsoFileName;           

            public AppEntry()
            {
                NotAlowChangeMaster = false;
                FreeDirectoryListAccessWithoutMaster = true;
                FreeCreateDeleteWithoutMasterKey = false;
                ConfigurationChangeable = true;
                ChangeKeyAccessRight = 0x00;
                CryptoMethod = CRYPTO_METHOD.Des;
                FileIdentifierSupported = false;
                NumberKeyPerApp = 0x0E;
            }
            /// <summary>
            /// Keys entries
            /// </summary>
            public const int AppKeyCount = 14;
            public Dictionary<Byte, byte[]> Keys = new Dictionary<Byte, byte[]>();

            /// <summary>
            /// Files entries
            /// </summary>
            public const int AppFileCount = 32;
            public Dictionary<byte, FileEntry> Files = new Dictionary<byte, FileEntry>();

            /// <summary>
            /// Used for advanced parameters crypted
            /// </summary>
            public byte Options;
            public byte[] Data;

        }
    }
}
