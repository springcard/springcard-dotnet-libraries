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
        public enum LockUnlockMode
        {
            Unlock = 0x00,
            Lock = 0x01,
            LockAndSetUnlockKey = 0x02,
            Activate = 0x03,
            UnlockPL = 0x04
        }

        public byte[] ComputeCMAC(byte[] data, byte[] aesKeyValue, byte[] aesInitVector)
        {
            byte[] cmac = CryptoPrimitives.CalculateCMAC(aesKeyValue, aesInitVector, data);
            byte[] result = new byte[cmac.Length / 2];
            int j = 0;
            for (int i = 1; i < cmac.Length;)
            {
                result[j++] = cmac[i];
                i += 2;
            }
            return result;
        }

        public byte[] ComputeCMAC(byte[] data, byte[] aesKeyValue)
        {
            return ComputeCMAC(data, aesKeyValue, new byte[16]);
        }

        private class LockUnlockSelfTestTransmitter : ICardApduTransmitter
        {
            void ICardApduTransmitter.Disconnect() { }
            void ICardApduTransmitter.Reconnect() { }

            RAPDU ICardApduTransmitter.Transmit(CAPDU capdu)
            {
                switch (capdu.ToString())
                {
                    case "8010000002010000":
                        return new RAPDU(BinConvert.HexToBytes("61504E3709A02F5B195BD14890AF"));
                    case "8010000014C4067B6EA12928980EDF9687F8CF98B4F9D3DC0B00":
                        return new RAPDU(BinConvert.HexToBytes("3EF2A84573D7DC76252EECA4A3A2A8CB0FAD65227138891290AF"));
                    case "80100000208376F3915E7B2E1FE924471033CE4CDE7515C0739A4A02345DA36E0068AC025400":
                        return new RAPDU(BinConvert.HexToBytes("6CFBC9E8CCC12100AC91274FD54310569000"));

                    default:
                        Logger.Fatal("APDU {0} not implemented", capdu.ToString());
                        return new RAPDU(0x6F00);
                }
            }
        }
        private bool LockUnlockSelfTestRunning = false;

        public static void LockUnlockSelfTest()
        {
            LockUnlockSelfTestTransmitter transmitter = new LockUnlockSelfTestTransmitter();
            SamAV sam = new SamAV(transmitter);
            sam.LockUnlockSelfTestRunning = true;
            if (!sam.LockUnlock(LockUnlockMode.Unlock, 0x01, 0x00, BinConvert.HexToBytes("9c9ef11637ab241942c4055c05099abc")))
                throw new Exception("LockUnlockSelfTest failed");
        }

        public bool LockUnlock(LockUnlockMode mode, byte[] data, byte[] aesKeyValue)
        {
            Logger.Debug("SAM_LockUnlock mode={0}, data={1}", mode, BinConvert.ToHex(data));

            /* 1st stage - Initiate the LockUnlock sequence */
            /* -------------------------------------------- */

            CAPDU capdu = new CAPDU(CLA, (byte)INS.LockUnlock, (byte)mode, 0x00, data, 0x00);
            if (!Command(capdu, out RAPDU rapdu))
                return false;

            if (rapdu.SW != 0x90AF)
                return OnStatusWordError(rapdu.SW);

            /* Retrieve SAM's Rnd2 */

            byte[] Rnd2 = rapdu.data.Bytes;
            Logger.Debug("\tRnd2={0}", BinConvert.ToHex(Rnd2));

            /* 2nd stage - Send a proof we know the key and host's Rnd1  */
            /* --------------------------------------------------------- */

            byte[] cmac_data;
            cmac_data = BinUtils.Concat(null, Rnd2);
            cmac_data = BinUtils.Concat(cmac_data, (byte)mode);
            if ((data != null) && (data.Length > 2))
                cmac_data = BinUtils.Concat(cmac_data, BinUtils.Copy(data, 2, data.Length - 2));
            while ((cmac_data.Length % 16) != 0)
                cmac_data = BinUtils.Concat(cmac_data, 0x00);

            Logger.Debug("\tCMAC data={0}", BinConvert.ToHex(cmac_data));

            byte[] CmacHost = ComputeCMAC(cmac_data, aesKeyValue);

            Logger.Debug("\tCMAC host={0}", BinConvert.ToHex(CmacHost));

            byte[] Rnd1 = PRNG.Generate(12);
            if (LockUnlockSelfTestRunning)
                Rnd1 = BinConvert.HexToBytes("0EDF9687F8CF98B4F9D3DC0B");
            Logger.Debug("\tRnd1={0}", BinConvert.ToHex(Rnd1));

            capdu = new CAPDU(CLA, (byte)INS.LockUnlock, 0x00, 0x00, BinUtils.Concat(CmacHost, Rnd1), 0x00);
            if (!Command(capdu, out rapdu))
                return false;

            if (rapdu.SW != 0x90AF)
                return OnStatusWordError(rapdu.SW);

            /* Retrieve SAM's CmacSam and encrypted RndA */

            byte[] CmacSam = BinUtils.Copy(rapdu.data.Bytes, 0, 8);
            byte[] EncSAM1 = BinUtils.Copy(rapdu.data.Bytes, 8, 16);

            /* 3rd stage - Verify the SAM knows the key given the CMAC computed over host's Rnd1 */
            /* --------------------------------------------------------------------------------- */

            cmac_data = BinUtils.Concat(null, Rnd1);
            cmac_data = BinUtils.Concat(cmac_data, (byte)mode);
            if ((data != null) && (data.Length > 2))
                cmac_data = BinUtils.Concat(cmac_data, BinUtils.Copy(data, 2, data.Length - 2));
            while ((cmac_data.Length % 16) != 0)
                cmac_data = BinUtils.Concat(cmac_data, 0x00);

            Logger.Debug("\tCMAC data={0}", BinConvert.ToHex(cmac_data));

            byte[] CmacSamVerif = ComputeCMAC(cmac_data, aesKeyValue);

            Logger.Debug("\tCMAC SAM={0}", BinConvert.ToHex(CmacSam));
            Logger.Debug("\t         {0}", BinConvert.ToHex(CmacSamVerif));

            if (!BinUtils.Equals(CmacSam, CmacSamVerif))
                return OnSecurityError();

            /* 4rd stage - Compute SV1 and KXE, send a proof we have the right KXE and send RndA */
            /* --------------------------------------------------------------------------------- */

            byte[] SV1 = new byte[16];
            int offset = 0;
            for (int i = 7; i < Rnd1.Length; i++)
                SV1[offset++] = Rnd1[i];
            for (int i = 7; i < Rnd2.Length; i++)
                SV1[offset++] = Rnd2[i];
            for (int i = 0; i < 5; i++)
                SV1[offset++] = (byte)(Rnd1[i] ^ Rnd2[i]);
            SV1[offset++] = 0x91;

            Logger.Debug("\tSV1={0}", BinConvert.ToHex(SV1));

            byte[] KXE = CryptoPrimitives.AES_Encrypt(SV1, aesKeyValue, new byte[16]);

            Logger.Debug("\tKXE={0}", BinConvert.ToHex(KXE));

            Logger.Debug("\tEncSAM1={0}", BinConvert.ToHex(EncSAM1));

            byte[] RndB = CryptoPrimitives.AES_Decrypt(EncSAM1, KXE, new byte[16]);

            Logger.Debug("\tRndB={0}", BinConvert.ToHex(RndB));

            byte[] RndBpp = CryptoPrimitives.RotateLeft(RndB, 2);

            byte[] RndA = PRNG.Generate(16);
            if (LockUnlockSelfTestRunning)
                RndA = BinConvert.HexToBytes("B8158B41CE4A0CF91DCF432064DF8A66");

            Logger.Debug("\tRndA={0}", BinConvert.ToHex(RndA));

            byte[] RndAppVerif = CryptoPrimitives.RotateLeft(RndA, 2);

            byte[] RndA_RndBpp = BinUtils.Concat(RndA, RndBpp);

            Logger.Debug("\tRndA || RndB''={0}", BinConvert.ToHex(RndA_RndBpp));

            byte[] EncHost = CryptoPrimitives.AES_Encrypt(RndA_RndBpp, KXE, new byte[16]);

            Logger.Debug("\tEncHost={0}", BinConvert.ToHex(EncHost));

            capdu = new CAPDU(CLA, (byte)INS.LockUnlock, 0x00, 0x00, EncHost, 0x00);
            if (!Command(capdu, out rapdu))
                return false;

            if (rapdu.SW != 0x9000)
                return OnStatusWordError(rapdu.SW);

            byte[] EncSAM2 = BinUtils.Copy(rapdu.data.Bytes, 0, 16);

            Logger.Debug("\tEncSAM2={0}", BinConvert.ToHex(EncSAM2));

            byte[] RndApp = CryptoPrimitives.AES_Decrypt(EncSAM2, KXE, new byte[16]);

            Logger.Debug("\tRndA'' SAM={0}", BinConvert.ToHex(RndApp));
            Logger.Debug("\t           {0}", BinConvert.ToHex(RndAppVerif));

            if (!BinUtils.Equals(RndApp, RndAppVerif))
                return OnSecurityError();

            return true;
        }

        public bool LockUnlock(LockUnlockMode mode, byte keyNo, byte keyVer, byte[] aesKeyValue)
        {
            switch (mode)
            {
                case LockUnlockMode.Lock:
                case LockUnlockMode.Unlock:
                case LockUnlockMode.UnlockPL:
                    break;
                default:
                    throw new ArgumentException();
            }

            return LockUnlock(mode, new byte[] { keyNo, keyVer }, aesKeyValue);
        }

        public bool Lock(byte keyNo, byte keyVer, byte[] aesKeyValue)
        {
            return LockUnlock(LockUnlockMode.Lock, keyNo, keyVer, aesKeyValue);
        }

        public bool Unlock(byte keyNo, byte keyVer, byte[] aesKeyValue)
        {
            return LockUnlock(LockUnlockMode.Unlock, keyNo, keyVer, aesKeyValue);
        }

        public bool UnlockPL(byte keyVer, byte[] aesKeyValue)
        {
            return LockUnlock(LockUnlockMode.UnlockPL, 0xF0, keyVer, aesKeyValue);
        }

        public bool Lock(byte lockKeyNo, byte lockKeyVer, byte unlockKeyNo, byte unlockKeyVer, byte[] aesKeyValue)
        {
            return LockUnlock(LockUnlockMode.LockAndSetUnlockKey, new byte[] { lockKeyNo, lockKeyVer, unlockKeyNo, unlockKeyVer }, aesKeyValue);
        }


    }
}
