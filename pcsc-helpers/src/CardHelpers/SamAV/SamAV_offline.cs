using SpringCard.LibCs;
using System;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public bool TransmitOfflineCryptogram(byte[] command)
        {
            Logger.Trace("ProcessOfflineApdu");
            Logger.Trace("\tCommand={0}", BinConvert.ToHex(command));

            CAPDU capdu = new CAPDU(command);
            if (!Command(capdu, out RAPDU rapdu))
                return false;

            byte[] response = rapdu.Bytes;
            Logger.Trace("\tResponse={0}", BinConvert.ToHex(response));

            if (rapdu.SW != 0x9000)
            {
                LastError = ResultE.UnexpectedStatusWord;
                return false;
            }

            return true;
        }

        public bool ActivateOfflineKey(byte KeyIdx, byte KeyVersion, byte[] DivInput = null)
        {
            Logger.Trace("ActivateOfflineKey");
            Logger.Trace("\tKeyIdx={0:X02}", KeyIdx);
            Logger.Trace("\tKeyVersion={0:X02}", KeyVersion);

            if (DivInput == null)
            {
                DivInput = new byte[0];
            }
            else
            {
                Logger.Trace("\tDivInput={0}", BinConvert.ToHex(DivInput));
            }

            byte P1;

            byte[] Data = new byte[2 + DivInput.Length];
            Data[0] = KeyIdx;
            Data[1] = KeyVersion;

            if (DivInput.Length > 0)
            {
                Array.Copy(DivInput, 0, Data, 2, DivInput.Length);
                P1 = 0x01;
            }
            else
            {
                P1 = 0x00;
            }

            CAPDU capdu = new CAPDU(CLA, (byte)INS.ActivateOfflineKey, P1, 0x00, Data);
            if (!Command(capdu, out RAPDU rapdu))
                return false;

            if (rapdu.SW != 0x9000)
                return OnStatusWordError(rapdu.SW);

            return true;
        }


        private static byte[] CreateOfflineCryptogramEx(ushort OfflineCounter, byte[] OfflineKeyEncrypt, byte[] OfflineKeyCmac, byte CLA, byte INS, byte P1, byte P2, byte[] Data, byte[] SamUid)
        {
            byte[] input = Data;
            if (SamUid != null)
                input = BinUtils.Concat(input, SamUid);

            Logger.Debug("Plain input:                 {0}", BinConvert.ToHex(input));
            byte[] paddedInput = Padd(input);
            Logger.Debug("Padded input:                {0}", BinConvert.ToHex(paddedInput));
            byte[] cryptedInput = CryptoPrimitives.AES_Encrypt(paddedInput, OfflineKeyEncrypt, new byte[16]);
            Logger.Debug("Crypted input:               {0}", BinConvert.ToHex(cryptedInput));

            int cmacInputLength = cryptedInput.Length + 7;
            byte[] cmacInput = new byte[cmacInputLength];
            int offset = 0;
            cmacInput[offset++] = CLA;
            cmacInput[offset++] = INS;
            cmacInput[offset++] = P1;
            cmacInput[offset++] = P2;
            cmacInput[offset++] = (byte)(cryptedInput.Length + 10);
            cmacInput[offset++] = (byte)(OfflineCounter % 0x0100);
            cmacInput[offset++] = (byte)(OfflineCounter / 0x0100);
            Array.Copy(cryptedInput, 0, cmacInput, offset, cryptedInput.Length);
            Logger.Debug("APDU for CMAC: {0}", BinConvert.ToHex(cmacInput));

            CryptoPrimitives.CMAC cmac = new CryptoPrimitives.CMAC(OfflineKeyCmac);
            byte[] cmacValue = cmac.Compute8(cmacInput);
            Logger.Debug("CMAC         : {0}", BinConvert.ToHex(cmacValue));

            return BinUtils.Concat(cmacInput, cmacValue);
        }

        public static byte[] CreateOfflineCryptogram(ushort OfflineCounter, byte[] OfflineKeyValue, byte CLA, byte INS, byte P1, byte P2, byte[] Data, byte[] SamUid = null)
        {
            Logger.Trace("CreateOfflineApdu");
            if ((SamUid != null) && (SamUid.Length > 0))
                Logger.Trace("\tSamUID={0}", BinConvert.ToHex(SamUid));
            Logger.Trace("\tCounter={0:X04}", OfflineCounter);
            Logger.Trace("\tKeyValue={0}", KeyToString(OfflineKeyValue));

            byte[] SV1 = new byte[16];
            SV1[0] = (byte)(OfflineCounter % 0x0100);
            SV1[1] = (byte)(OfflineCounter / 0x0100);
            for (int i = 2; i < 16; i++)
                SV1[i] = 0x71;
            Logger.Debug("\tSV1={0}", BinConvert.ToHex(SV1));
            byte[] OfflineKeyEncrypt = CryptoPrimitives.AES_Encrypt(SV1, OfflineKeyValue, new byte[16]);
            Logger.Debug("\tKeyEncrypt={0}", KeyToString(OfflineKeyEncrypt));

            byte[] SV2 = new byte[16];
            SV2[0] = (byte)(OfflineCounter % 0x0100);
            SV2[1] = (byte)(OfflineCounter / 0x0100);
            for (int i = 2; i < 16; i++)
                SV2[i] = 0x72;
            Logger.Debug("\tSV2={0}", BinConvert.ToHex(SV2));
            byte[] OfflineKeyCmac = CryptoPrimitives.AES_Encrypt(SV2, OfflineKeyValue, new byte[16]);
            Logger.Debug("\tKeyCmac={0}", KeyToString(OfflineKeyCmac));

            return CreateOfflineCryptogramEx(OfflineCounter, OfflineKeyEncrypt, OfflineKeyCmac, CLA, INS, P1, P2, Data, SamUid);
        }

        public static byte[] CreateOfflineCryptogram(ushort OfflineCounter, byte[] OfflineKeyValue, INS INS, byte P1, byte P2, byte[] Data, byte[] SamUid = null)
        {
            return CreateOfflineCryptogram(OfflineCounter, OfflineKeyValue, CLA, (byte)INS, P1, P2, Data, SamUid);
        }

        public static byte[] CreateOfflineCryptogram(ushort OfflineCounter, byte[] OfflineKeyEncrypt, byte[] OfflineKeyCmac, byte CLA, byte INS, byte P1, byte P2, byte[] Data, byte[] SamUid = null)
        {
            Logger.Trace("CreateOfflineApdu");
            if ((SamUid != null) && (SamUid.Length > 0))
                Logger.Trace("\tSamUID={0}", BinConvert.ToHex(SamUid));
            Logger.Trace("\tCounter={0:X04}", OfflineCounter);
            Logger.Trace("\tKeyEncrypt={0}", KeyToString(OfflineKeyEncrypt));
            Logger.Trace("\tKeyCmac={0}", KeyToString(OfflineKeyCmac));

            return CreateOfflineCryptogramEx(OfflineCounter, OfflineKeyEncrypt, OfflineKeyCmac, CLA, INS, P1, P2, Data, SamUid);
        }

        public static byte[] CreateOfflineCryptogram(ushort OfflineCounter, byte[] OfflineKeyEncrypt, byte[] OfflineKeyCmac, INS INS, byte P1, byte P2, byte[] Data, byte[] SamUid = null)
        {
            return CreateOfflineCryptogram(OfflineCounter, OfflineKeyEncrypt, OfflineKeyCmac, CLA, (byte)INS, P1, P2, Data, SamUid);
        }

        public static byte[] ArmorOfflineCryptogram(byte[] SamUid, ushort OfflineCounter, byte OfflineKeyIndex, byte OfflineKeyVersion, INS INS, byte P1, byte[] DataExcerpt, byte[] OfflineCryptogram)
        {
            return ArmorOfflineCryptogram(null, null, SamUid, OfflineCounter, OfflineKeyIndex, OfflineKeyVersion, INS, P1, DataExcerpt, OfflineCryptogram);
        }

        public static byte[] ArmorOfflineCryptogram(byte[] ArmorKey, byte[] SamUid, ushort OfflineCounter, byte OfflineKeyIndex, byte OfflineKeyVersion, INS INS, byte P1, byte[] DataExcerpt, byte[] OfflineCryptogram)
        {
            return ArmorOfflineCryptogram(null, ArmorKey, SamUid, OfflineCounter, OfflineKeyIndex, OfflineKeyVersion, INS, P1, DataExcerpt, OfflineCryptogram);
        }

        public static byte[] ArmorOfflineCryptogram(byte[] ArmorSalt, byte[] ArmorKey, byte[] SamUid, ushort OfflineCounter, byte OfflineKeyIndex, byte OfflineKeyVersion, INS INS, byte P1, byte[] DataExcerpt, byte[] OfflineCryptogram)
        {
            if (DataExcerpt == null)
                DataExcerpt = new byte[0];
            if (DataExcerpt.Length > 16)
                return null;

            byte[] armorPrefix = new byte[1 + 7 + 2 + 1 + 1 + 1 + 1 + DataExcerpt.Length];

            armorPrefix[0] = (byte) (DataExcerpt.Length & 0x0F);
            Array.Copy(SamUid, 0, armorPrefix, 1, 7);
            armorPrefix[8] = (byte)(OfflineCounter / 0x0100);
            armorPrefix[9] = (byte)(OfflineCounter % 0x0100);
            armorPrefix[10] = OfflineKeyIndex;
            armorPrefix[11] = OfflineKeyVersion;
            armorPrefix[12] = (byte)INS;
            armorPrefix[13] = P1;
            if (DataExcerpt != null)
                Array.Copy(DataExcerpt, 0, armorPrefix, 14, DataExcerpt.Length);

            byte[] buffer = BinUtils.Concat(armorPrefix, OfflineCryptogram);
            return Armor(ArmorSalt, ArmorKey, buffer);
        }

        public static bool UnarmorOfflineCryptogram(byte[] ArmoredMessage, out byte[] SamUid, out ushort OfflineCounter, out byte OfflineKeyIndex, out byte OfflineKeyVersion, out INS INS, out byte P1, out byte[] DataExcerpt, out byte[] OfflineCryptogram)
        {
            return UnarmorOfflineCryptogram(ArmoredMessage, null, out SamUid, out OfflineCounter, out OfflineKeyIndex, out OfflineKeyVersion, out INS, out P1, out DataExcerpt, out OfflineCryptogram);
        }

        public static bool UnarmorOfflineCryptogram(byte[] ArmoredMessage, byte[] ArmorKey, out byte[] SamUid, out ushort OfflineCounter, out byte OfflineKeyIndex, out byte OfflineKeyVersion, out INS INS, out byte P1, out byte[] DataExcerpt, out byte[] OfflineCryptogram)
        {
            SamUid = null;
            OfflineCounter = 0;
            OfflineKeyIndex = 0;
            OfflineKeyVersion = 0;
            INS = default;
            P1 = 0;
            DataExcerpt = null;
            OfflineCryptogram = null;

            if (!Unarmor(ArmorKey, ArmoredMessage, out byte[] buffer))
                return false;

            if (buffer.Length < 14)
                return false;

            DataExcerpt = new byte[buffer[0] & 0x0F];
            SamUid = BinUtils.Copy(buffer, 1, 7);
            OfflineCounter = (ushort) ((buffer[8] * 0x0100) + buffer[9]);
            OfflineKeyIndex = buffer[10];
            OfflineKeyVersion = buffer[11];
            INS = (INS)buffer[12];
            P1 = buffer[13];

            int dataExcerptLength = buffer[0] & 0x0F;

            DataExcerpt = BinUtils.Copy(buffer, 14, dataExcerptLength);

            OfflineCryptogram = BinUtils.Copy(buffer, 14 + dataExcerptLength, buffer.Length - 14 - dataExcerptLength);

            return true;
        }
    }
}
