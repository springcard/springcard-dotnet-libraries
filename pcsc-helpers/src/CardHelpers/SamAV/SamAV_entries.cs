/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 17/07/2012
 * Time: 09:49
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public const byte KeyEntriesCount = 128;

        public class KeyEntry
        {
            public KeyEntry Clone()
            {
                return (KeyEntry)this.MemberwiseClone();
            }

            public byte VersionA;
            public byte VersionB;
            public byte VersionC;
            public uint DesfireAid;
            public byte DesfireKeyIdx;
            public byte ChangeKeyIdx;
            public byte ChangeKeyVersion;
            public byte CounterIdx;

            public enum KeyTypeE : byte
            {
                DesfireEV0 = 0,
                TripleDes_Iso10116_Crc16_Mac32 = 1,
                Mifare = 2,
                TripleDes3K = 3,
                Aes128 = 4,
                Aes192 = 5,
                TripleDes_Iso10116_Crc32_Mac64 = 6,
                Rfu7 = 7
            }
            public KeyTypeE KeyType;

            public enum KeyClassE : byte
            {
                Host = 0,
                PICC = 1,
                OfflineChange = 2,
                Rfu3 = 3,
                OfflineCrypto = 4,
                Rfu5 = 5,
                Rfu6 = 6,
                Rfu7 = 7
            }
            public KeyClassE KeyClass;

            public bool KeepIV;
            public bool HostAuthKey;
            public bool LockUnlockKey;
            public bool DiversifiedOnly;
            public bool DisableKeyEntry;
            public bool EnableDumpSecretKey;
            public bool EnableDumpSessionKey;
            public bool DisableWriteToPICC;
            public bool DisableDecrypt;
            public bool DisableEncrypt;
            public bool DisableVerifyMAC;
            public bool DisableGenerateMAC;
            public byte[] ValueA = new byte[16];
            public byte[] ValueB = new byte[16];
            public byte[] ValueC = new byte[16];

            public KeyEntry()
            {
                CounterIdx = 0xFF;
                SET = 0x2002;
                ExtSET = 0x00;
            }

            protected bool IsValidSecretKey()
            {
                if (ValueA == null)
                    return false;
                if (ValueB == null)
                    return false;
                if (ValueC == null)
                    return false;
                return true;
            }

            [Flags]
            public enum ESET_HI : byte
            {
                EnableDumpSessionKey = 0x01,
                KeepIV = 0x04,
            }

            public byte SET_HI
            {
                get
                {
                    byte result = 0;
                    if (EnableDumpSessionKey)
                        result |= (byte)ESET_HI.EnableDumpSessionKey;
                    if (KeepIV)
                        result |= (byte)ESET_HI.KeepIV;
                    result |= (byte)(((byte)KeyType << 3) & 0x38);
                    return result;
                }
                set
                {
                    KeyType = (KeyTypeE)((value >> 3) & 0x07);
                    KeepIV = ((value & (byte)ESET_HI.KeepIV) != 0);
                    EnableDumpSessionKey = ((value & (byte)ESET_HI.EnableDumpSessionKey) != 0);
                }
            }

            [Flags]
            public enum ESET_LO : byte
            {
                HostAuthKey = 0x01,
                DisableKeyEntry = 0x02,
                LockUnlockKey = 0x04,
                DisableWriteToPICC = 0x08,
                DisableDecrypt = 0x10,
                DisableEncrypt = 0x20,
                DisableVerifyMAC = 0x40,
                DisableGenerateMAC = 0x80,
            }

            public byte SET_LO
            {
                get
                {
                    byte result = 0;
                    if (HostAuthKey)
                        result |= (byte)ESET_LO.HostAuthKey;
                    if (DisableKeyEntry)
                        result |= (byte)ESET_LO.DisableKeyEntry;
                    if (LockUnlockKey)
                        result |= (byte)ESET_LO.LockUnlockKey;
                    if (DisableWriteToPICC)
                        result |= (byte)ESET_LO.DisableWriteToPICC;
                    if (DisableDecrypt)
                        result |= (byte)ESET_LO.DisableDecrypt;
                    if (DisableEncrypt)
                        result |= (byte)ESET_LO.DisableEncrypt;
                    if (DisableVerifyMAC)
                        result |= (byte)ESET_LO.DisableVerifyMAC;
                    if (DisableGenerateMAC)
                        result |= (byte)ESET_LO.DisableGenerateMAC;
                    return result;
                }
                set
                {
                    HostAuthKey = ((value & (byte)ESET_LO.HostAuthKey) != 0);
                    DisableKeyEntry = ((value & (byte)ESET_LO.DisableKeyEntry) != 0);
                    LockUnlockKey = ((value & (byte)ESET_LO.LockUnlockKey) != 0);
                    DisableWriteToPICC = ((value & (byte)ESET_LO.DisableWriteToPICC) != 0);
                    DisableDecrypt = ((value & (byte)ESET_LO.DisableDecrypt) != 0);
                    DisableEncrypt = ((value & (byte)ESET_LO.DisableEncrypt) != 0);
                    DisableVerifyMAC = ((value & (byte)ESET_LO.DisableVerifyMAC) != 0);
                    DisableGenerateMAC = ((value & (byte)ESET_LO.DisableGenerateMAC) != 0);
                }
            }

            public ushort SET
            {
                get
                {
                    return (ushort)((SET_HI << 8) | SET_LO);
                }
                set
                {
                    SET_HI = (byte)(value >> 8);
                    SET_LO = (byte)(value);
                }
            }

            public byte ExtSET
            {
                get
                {
                    byte result = 0;
                    result |= (byte)(((byte)KeyClass) & 0x07);
                    if (EnableDumpSecretKey)
                        result |= 0x08;
                    if (DiversifiedOnly)
                        result |= 0x10;
                    return result;
                }
                set
                {
                    KeyClass = (KeyClassE)(value & 0x07);
                    EnableDumpSecretKey = ((value & 0x08) != 0);
                    DiversifiedOnly = ((value & 0x10) != 0);
                }
            }

            public byte[] ToChangeKeyEntryAV1()
            {
                byte[] result = new byte[60];

                int offset = 0;
                Array.Copy(ValueA, 0, result, offset, 16); offset += 16;
                Array.Copy(ValueB, 0, result, offset, 16); offset += 16;
                Array.Copy(ValueC, 0, result, offset, 16); offset += 16;
                result[offset++] = (byte)(DesfireAid >> 16);
                result[offset++] = (byte)(DesfireAid >> 8);
                result[offset++] = (byte)(DesfireAid);
                result[offset++] = DesfireKeyIdx;
                result[offset++] = ChangeKeyIdx;
                result[offset++] = ChangeKeyVersion;
                result[offset++] = CounterIdx;
                result[offset++] = SET_HI;
                result[offset++] = SET_LO;
                result[offset++] = VersionA;
                result[offset++] = VersionB;
                result[offset++] = VersionC;

                return result;
            }

            [Flags]
            public enum ChangeKeyProMask : byte
            {
                ExplicitVersions = 0x01,
                UpdateSET = 0x02,
                UpdateCounter = 0x04,
                UpdateChangeKey = 0x08,
                UpdateDF = 0x10,
                UpdateKeyC = 0x20,
                UpdateKeyB = 0x40,
                UpdateKeyA = 0x80
            }

            public byte[] ToChangeKeyEntry(ChangeKeyProMask ProMask = (KeyEntry.ChangeKeyProMask) 0xFF)
            {
                int length = 0;

                if ((ProMask & ChangeKeyProMask.UpdateKeyA) != 0)
                    length += 17;
                if ((ProMask & ChangeKeyProMask.UpdateKeyB) != 0)
                    length += 17;
                if ((ProMask & ChangeKeyProMask.UpdateKeyC) != 0)
                    length += 17;
                if ((ProMask & ChangeKeyProMask.UpdateDF) != 0)
                    length += 4;
                if ((ProMask & ChangeKeyProMask.UpdateChangeKey) != 0)
                    length += 2;
                if ((ProMask & ChangeKeyProMask.UpdateCounter) != 0)
                    length += 1;
                if ((ProMask & ChangeKeyProMask.UpdateSET) != 0)
                    length += 3;

                byte[] result = new byte[length];

                int offset = 0;
                if ((ProMask & ChangeKeyProMask.UpdateKeyA) != 0)
                {
                    if (ValueA != null)
                        Array.Copy(ValueA, 0, result, offset, 16);
                    offset += 16;
                }
                if ((ProMask & ChangeKeyProMask.UpdateKeyB) != 0)
                {
                    if (ValueB != null)
                        Array.Copy(ValueB, 0, result, offset, 16);
                    offset += 16;
                }
                if ((ProMask & ChangeKeyProMask.UpdateKeyC) != 0)
                {
                    if (ValueC != null)
                        Array.Copy(ValueC, 0, result, offset, 16);
                    offset += 16;
                }
                if ((ProMask & ChangeKeyProMask.UpdateDF) != 0)
                {
                    result[offset++] = (byte)(DesfireAid >> 16);
                    result[offset++] = (byte)(DesfireAid >> 8);
                    result[offset++] = (byte)(DesfireAid);
                    result[offset++] = DesfireKeyIdx;
                }
                if ((ProMask & ChangeKeyProMask.UpdateChangeKey) != 0)
                {
                    result[offset++] = ChangeKeyIdx;
                    result[offset++] = ChangeKeyVersion;
                }
                if ((ProMask & ChangeKeyProMask.UpdateCounter) != 0)
                {
                    result[offset++] = CounterIdx;
                }
                if ((ProMask & ChangeKeyProMask.UpdateSET) != 0)
                {
                    result[offset++] = SET_HI;
                    result[offset++] = SET_LO;
                }
                if ((ProMask & ChangeKeyProMask.UpdateKeyA) != 0)
                {
                    result[offset++] = VersionA;
                }
                if ((ProMask & ChangeKeyProMask.UpdateKeyB) != 0)
                {
                    result[offset++] = VersionB;
                }
                if ((ProMask & ChangeKeyProMask.UpdateKeyC) != 0)
                {
                    result[offset++] = VersionC;
                }
                if ((ProMask & ChangeKeyProMask.UpdateSET) != 0)
                {
                    result[offset++] = ExtSET;
                }

                return result;
            }

            public byte[] ToOfflineChangeKeyEntryCryptogram(ushort OfflineCounter, byte[] OfflineKeyValue, byte KeyIdx, ChangeKeyProMask ProMask = (ChangeKeyProMask) 0xFF)
            {
                byte[] keyEntryData = ToChangeKeyEntry(ProMask);
                ProMask |= ChangeKeyProMask.ExplicitVersions;
                return CreateOfflineCryptogram(OfflineCounter, OfflineKeyValue, SamAV.INS.ChangeKeyEntry, KeyIdx, (byte) ProMask, keyEntryData);
            }


            public void FromGetKeyEntry(byte[] GetKeyEntryData, bool VersionsAhead)
            {
                int offset = 0;

                if (VersionsAhead)
                {
                    VersionA = GetKeyEntryData[offset++];
                    VersionB = GetKeyEntryData[offset++];
                    VersionC = GetKeyEntryData[offset++];
                }
                DesfireAid = 0;
                DesfireAid |= GetKeyEntryData[offset++]; DesfireAid <<= 8;
                DesfireAid |= GetKeyEntryData[offset++]; DesfireAid <<= 8;
                DesfireAid |= GetKeyEntryData[offset++];
                DesfireKeyIdx = GetKeyEntryData[offset++];
                ChangeKeyIdx = GetKeyEntryData[offset++];
                ChangeKeyVersion = GetKeyEntryData[offset++];
                CounterIdx = GetKeyEntryData[offset++];
                SET_HI = GetKeyEntryData[offset++];
                SET_LO = GetKeyEntryData[offset++];
                if (!VersionsAhead)
                {
                    VersionA = GetKeyEntryData[offset++];
                    VersionB = GetKeyEntryData[offset++];
                    VersionC = GetKeyEntryData[offset++];
                }
                ExtSET = GetKeyEntryData[offset++];
            }

            public static KeyEntry Deserialize(byte[] value)
            {
                KeyEntry result = new KeyEntry();

                if (value.Length < 60)
                    throw new ArgumentException(nameof(value));
                if (value.Length > 61)
                    throw new ArgumentException(nameof(value));

                int offset = 0;
                Array.Copy(value, offset, result.ValueA, 0, 16); offset += 16;
                Array.Copy(value, offset, result.ValueB, 0, 16); offset += 16;
                Array.Copy(value, offset, result.ValueC, 0, 16); offset += 16;
                result.DesfireAid = 0;
                for (int i = 0; i < 3; i++)
                {
                    result.DesfireAid <<= 8;
                    result.DesfireAid |= value[offset++];
                }
                result.DesfireKeyIdx = value[offset++];
                result.ChangeKeyIdx = value[offset++];
                result.ChangeKeyVersion = value[offset++];
                result.CounterIdx = value[offset++];
                result.SET_HI = value[offset++];
                result.SET_LO = value[offset++];
                result.VersionA = value[offset++];
                result.VersionB = value[offset++];
                result.VersionC = value[offset++];

                if (value.Length > offset)
                    result.ExtSET = value[offset++];

                return result;
            }

            public static KeyEntry TryDeserialize(byte[] value)
            {
                try
                {
                    return Deserialize(value);
                }
                catch (Exception e)
                {
                    Logger.Trace(e.Message);
                    return null;
                }
            }
        }

        public bool GetKeyEntry(byte keyIdx, out byte[] keyEntryData)
        {
            keyEntryData = null;

            CAPDU capdu = new CAPDU(CLA, 0x64, keyIdx, 0x00, 0x00);
            if (!Command(capdu, out RAPDU rapdu))
                return false;

            if (rapdu.SW != 0x9000)
                return OnStatusWordError(rapdu.SW);

            keyEntryData = rapdu.DataBytes;
            return true;
        }

        public bool GetKeyEntry(byte keyIdx, out KeyEntry keyEntry)
        {
            keyEntry = null;

            if (!GetKeyEntry(keyIdx, out byte[] keyEntryData))
                return false;

            keyEntry = new KeyEntry();

            int offset = 0;
            keyEntry.VersionA = keyEntryData[offset++];
            keyEntry.VersionB = keyEntryData[offset++];
            keyEntry.VersionC = keyEntryData[offset++];

            keyEntry.DesfireAid = keyEntryData[offset++]; keyEntry.DesfireAid <<= 8;
            keyEntry.DesfireAid |= keyEntryData[offset++]; keyEntry.DesfireAid <<= 8;
            keyEntry.DesfireAid |= keyEntryData[offset++];

            keyEntry.DesfireKeyIdx = keyEntryData[offset++];
            keyEntry.ChangeKeyIdx = keyEntryData[offset++];
            keyEntry.ChangeKeyVersion = keyEntryData[offset++];
            keyEntry.CounterIdx = keyEntryData[offset++];

            keyEntry.SET_HI = keyEntryData[offset++];
            keyEntry.SET_LO = keyEntryData[offset++];

            keyEntry.ExtSET = keyEntryData[offset++];

            return true;
        }

        public bool GetKeyEntrySecretKeys(byte keyIdx, ref KeyEntry keyEntry, bool ignoreErrors = false)
        {
            bool result;

            result = DumpSecretKey(keyIdx, keyEntry.VersionA, out keyEntry.ValueA);
            if (!result && !ignoreErrors)
                return false;
            if (keyEntry.VersionB != keyEntry.VersionA)
            {
                result = DumpSecretKey(keyIdx, keyEntry.VersionB, out keyEntry.ValueB);
                if (!result && !ignoreErrors)
                    return false;
            }
            if ((keyEntry.VersionC != keyEntry.VersionA) && (keyEntry.VersionC != keyEntry.VersionB))
            {
                result = DumpSecretKey(keyIdx, keyEntry.VersionC, out keyEntry.ValueC);
                if (!result && !ignoreErrors)
                    return false;
            }

            return true;
        }

        public bool DumpSecretKey(byte KeyIdx, byte KeyVersion, out byte[] KeyValue)
        {
            byte P1 = 0x00;
            byte[] in_data = new byte[2];
            in_data[0] = KeyIdx;
            in_data[1] = KeyVersion;

            /* note only plain dump is available */
            
            if (Command(INS.DumpSecretKey, P1, 0x00, in_data, true, out KeyValue, ExpectE.Success) != ResultE.Success)
            {
                KeyValue = null;
                return false;
            }

            return true;
        }

        public bool DumpSessionKey(out byte[] KeyValue)
        {
            byte P1 = 0x00;

            /* note only plain dump is available */

            if (Command(INS.DumpSessionKey, P1, 0x00, null, true, out KeyValue, ExpectE.Success) != ResultE.Success)
            {
                KeyValue = null;
                return false;
            }

            return true;
        }
        public bool GetPkiPrivateEntry(byte keyIdx, out PKIKeyEntry pkiKeyEntry)
        {
            pkiKeyEntry = null;
            
            if ( !PKIExportPrivateKey_AV2(keyIdx) )
            {
                return false; 
            }

            pkiKeyEntry = PKIKeyEntry.Deserialize(this.rsa_Key);

            return true;
        }

        public bool GetPkiPublicEntry(byte keyIdx, out PKIKeyEntry pkiKeyEntry)
        {
            pkiKeyEntry = null;

            if (!PKIExportPublicKey_AV2(keyIdx))
            {
                return false;
            }

            pkiKeyEntry = PKIKeyEntry.Deserialize(this.rsa_Key);

            return true;
        }

        public bool ChangeKeyEntryAV1(byte keyIdx, KeyEntry keyEntry)
        {
            return ChangeKeyEntryAV1(keyIdx, keyEntry.ToChangeKeyEntryAV1());
        }

        public bool ChangeKeyEntry(byte keyIdx, KeyEntry keyEntry, KeyEntry.ChangeKeyProMask ProMask = (KeyEntry.ChangeKeyProMask) 0xFF)
        {
            return ChangeKeyEntry(keyIdx, keyEntry.ToChangeKeyEntry(ProMask));
        }

        public const int SymmetricKeyCount = 128;
        public const int AsymmetricKeyCount = 3;



        public abstract class Entry
        {
            public byte Index { get; private set; }

            public byte[] Value { get; private set; }

            public bool MustWrite { get; private set; }
            public bool MustGenerate { get; private set; }

            public static Entry CreateFromIniLine(byte index, string IniValue)
            {
                if (index < SymmetricKeyCount)
                {
                    if (IniValue.Length == 124)
                    {
                        string iniValueSym = IniValue.Substring(0, 122);
                        string iniValueFlags = IniValue.Substring(122, 2);
                        byte[] valueSym = BinConvert.HexToBytes(iniValueSym);
                        if (valueSym.Length != 61)
                            return null;

                        SymmetricEntry result = new SymmetricEntry();
                        result.Index = index;
                        result.Value = valueSym;
                        if (iniValueFlags.Contains("W"))
                            result.MustWrite = true;
                        if (iniValueFlags.Contains("R"))
                            result.MustGenerate = true;

                        return result;
                    }
                }

                if ((index >= SymmetricKeyCount) && (index < SymmetricKeyCount + AsymmetricKeyCount))
                {
                    int firstIndex = IniValue.IndexOf('-');
                    int lastIndex = IniValue.LastIndexOf('-');

                    if (firstIndex < lastIndex)
                    {
                        string iniKeyEntryAsym = IniValue.Substring(0, firstIndex);
                        lastIndex += 1;
                        string iniKeyValueAsym = IniValue.Substring(lastIndex, IniValue.Length - (lastIndex));
                        string iniValueFlags = IniValue.Substring(firstIndex, lastIndex);
                        

                        AsymetricEntry result = new AsymetricEntry();
                        result.Index = index;
                        byte[] valueAsym = BinConvert.HexToBytes(iniKeyValueAsym);
                        result.Value = valueAsym;

                        if (iniValueFlags.Contains("W"))
                            result.MustWrite = true;
                        if (iniValueFlags.Contains("R"))
                            result.MustGenerate = true;

                        return result;
                    }
                }

                return null;
            }
        }

        public class Entries
        {
            List<Entry> entries = new List<Entry>();

            public int Length { get { return entries.Count; } }
            public Entry this[int index] { get { return entries[index]; } }


            public static Entries CreateFromIniFile(string FileName)
            {
                IniFile iniFile = new IniFile(FileName);

                Entries result = new Entries();

                for (int index=0; index<SymmetricKeyCount + AsymmetricKeyCount; index++)
                {
                    string IniKey = string.Format("{0}", index);
                    string IniValue = iniFile.ReadString("Configuration", IniKey, null);
                    if (IniValue != null)
                    {
                        Entry entry = Entry.CreateFromIniLine((byte) index, IniValue);
                        if (entry != null)
                            result.entries.Add(entry);
                    }
                }

                return result;
            }
        }

        public class SymmetricEntry : Entry
        {
            public byte ChangeKeyEntry
            {
                get
                {
                    return Value[51];
                }
            }

            public byte ChangeKeyVersion
            {
                get
                {
                    return Value[52];
                }
            }
        }

        public class AsymetricEntry : Entry
        {

        }


    }
}
