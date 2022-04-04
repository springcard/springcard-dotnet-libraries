using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;
using System.Xml.XPath;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public const byte KUCEntriesCount = 16;

        public class KUCEntry
        {
            public KUCEntry Clone()
            {
                return (KUCEntry)this.MemberwiseClone();
            }

            public uint Limit = 0xFFFFFFFF;
            public byte ChangeKeyIdx = 0x00; /* Default when SAM is blank */
            public byte ChangeKeyVersion = 0x00;
            public uint Value = 0x00000000;

            public KUCEntry()
            {

            }

            public byte[] ToChangeKUCEntry()
            {
                byte[] result = new byte[6];

                Array.Copy(BinUtils.FromDword(Limit, BinUtils.Endianness.LittleEndian), 0, result, 0, 4);
                result[4] = ChangeKeyIdx;
                result[5] = ChangeKeyVersion;

                return result;
            }
        }

        public bool GetKUCEntry(byte kucIdx, out KUCEntry kucEntry)
        {
            //Logger.Trace("GetKUCEntry {0:X02}", kucIdx);

            kucEntry = null;
            
            if (Command(INS.GetKUCEntry, kucIdx, 0x00, out byte[] data, ExpectE.Success) != ResultE.Success)
                return false;

            if ((data == null) || (data.Length != 10))
            {
                LastError = ResultE.InvalidResponseData;
                return false;
            }

            kucEntry = new KUCEntry();

            kucEntry.Limit = BinUtils.ToDword(BinUtils.Copy(data, 0, 4), BinUtils.Endianness.LittleEndian);
            kucEntry.ChangeKeyIdx = data[4];
            kucEntry.ChangeKeyVersion = data[5];
            kucEntry.Value = BinUtils.ToDword(BinUtils.Copy(data, 6, 4), BinUtils.Endianness.LittleEndian);

            return true;
        }

        public bool ChangeKUCEntry(byte kucIdx, byte proMas, byte[] kucEntry)
        {
            //Logger.Trace("ChangeKUCEntry {0:X02}", kucIdx);

            if (Command(INS.ChangeKUCEntry, kucIdx, proMas, kucEntry, ExpectE.Success) != ResultE.Success)
                return false;

            return true;
        }

        public bool ChangeKUCEntry(byte kucIdx, bool updateLimit, bool updateChangeKeyIdx, bool updateChangeKeyVersion, KUCEntry kucEntry)
        {
            byte proMas = 0x00;

            if (updateLimit)
                proMas |= 0x80;
            if (updateChangeKeyIdx)
                proMas |= 0x40;
            if (updateChangeKeyVersion)
                proMas |= 0x20;

            byte[] data = new byte[6];

            Array.Copy(BinUtils.FromDword(kucEntry.Limit, BinUtils.Endianness.LittleEndian), 0, data, 0, 4);
            data[4] = kucEntry.ChangeKeyIdx;
            data[5] = kucEntry.ChangeKeyVersion;

            return ChangeKUCEntry(kucIdx, proMas, data);
        }

        public bool ChangeKUCEntry(byte kucIdx, KUCEntry kucEntry)
        {
            return ChangeKUCEntry(kucIdx, kucEntry, false);
        }

        public bool ChangeKUCEntry(byte kucIdx, KUCEntry kucEntry, bool force)
        {
            bool updateLimit = force;
            bool updateChangeKeyIdx = force;
            bool updateChangeKeyVersion = force;

            if (!force)
            {
                if (!GetKUCEntry(kucIdx, out KUCEntry oldKucEntry))
                    return false;

                if (kucEntry.Limit != oldKucEntry.Limit)
                    updateLimit = true;
                if (kucEntry.ChangeKeyIdx != oldKucEntry.ChangeKeyIdx)
                    updateChangeKeyIdx = true;
                if (kucEntry.ChangeKeyVersion != oldKucEntry.ChangeKeyVersion)
                    updateChangeKeyVersion = true;

                if (!updateLimit && !updateChangeKeyIdx && !updateChangeKeyVersion)
                {
                    Logger.Debug("No actual change in KUC entry");
                    LastError = ResultE.NothingToDo;
                    return true;
                }
            }

            return ChangeKUCEntry(kucIdx, updateLimit, updateChangeKeyIdx, updateChangeKeyVersion, kucEntry);
        }

        public bool ChangeKUCEntryLimit(byte KucIdx, ulong Limit)
        {
            byte[] data = new byte[4];
            Array.Copy(BinUtils.FromDword((uint)Limit, BinUtils.Endianness.LittleEndian), 0, data, 0, 4);
            return ChangeKUCEntry(KucIdx, 0x80, data);
        }

        public bool ChangeKUCChangeKey(byte KucIdx, byte ChangeKeyIdx, byte ChangeKeyVersion)
        {
            byte[] data = new byte[2];
            data[0] = ChangeKeyIdx;
            data[1] = ChangeKeyVersion;
            return ChangeKUCEntry(KucIdx, 0x60, data);
        }
    }
}
