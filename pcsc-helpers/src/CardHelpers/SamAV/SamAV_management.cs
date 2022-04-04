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
        public bool ChangeKeyEntryAV1(byte keyIdx, byte[] keyEntryData)
        {
            Logger.Debug("ChangeKeyEntryAV1({0}, {1})", keyIdx, BinConvert.ToHex(keyEntryData));

            bool keepInitVector = false;
            byte[] initVector = new byte[16];

            byte[] data_to_encrypt = new byte[keyEntryData.Length + 4];
            int offset = 0;

            for (int i = 0; i < keyEntryData.Length; i++)
                data_to_encrypt[offset++] = keyEntryData[i];

            /* ------- calculate crc and concat	-------	*/
            if (activeAuth.AuthType == AuthTypeE.TDES_CRC16)
            {
                Logger.Debug("Calculating CRC16 and padding with 00 00 ...");
                byte[] crc = CryptoPrimitives.ComputeCrc16(keyEntryData);
                data_to_encrypt[offset++] = crc[0];
                data_to_encrypt[offset++] = crc[1];
                data_to_encrypt[offset++] = 0x00;
                data_to_encrypt[offset++] = 0x00;

            }
            else
            if ((activeAuth.AuthType == AuthTypeE.AES) || (activeAuth.AuthType == AuthTypeE.TDES_CRC32))
            {
                Logger.Debug("Calculating CRC32 ...");
                byte[] crc = CryptoPrimitives.ComputeCrc32(keyEntryData);
                data_to_encrypt[offset++] = crc[0];
                data_to_encrypt[offset++] = crc[1];
                data_to_encrypt[offset++] = crc[2];
                data_to_encrypt[offset++] = crc[3];

            }
            else
            {
                Logger.Debug("Can't calculate CRC: Authentication Key is not 3DES nor AES");
                userInteraction.Error("Authentication Key must be 3DES or AES.\nKey Entry " + String.Format("{0:x02}", keyIdx).ToUpper() + " can't be changed.");
                LastError = ResultE.InvalidParameters;
                return false;
            }

            string data = "";
            for (int i = 0; i < data_to_encrypt.Length; i++)
                data += "-" + data_to_encrypt[i];

            /*------- encrypt	------- */
            Logger.Debug("Encrypting ...");
            byte[] encrypted_data;

            if ((activeAuth.AuthType == AuthTypeE.TDES_CRC16) || (activeAuth.AuthType == AuthTypeE.TDES_CRC32))
            {
                encrypted_data = CryptoPrimitives.TripleDES_Encrypt(data_to_encrypt, session_key, initVector);
            }
            else
            if (activeAuth.AuthType == AuthTypeE.AES)
            {
                encrypted_data = CryptoPrimitives.AES_Encrypt(data_to_encrypt, session_key, initVector);
            }
            else
            {
                Logger.Debug("Can't encrypt: Authentication Key is not 3DES nor AES");
                userInteraction.Error("Authentication Key must be 3DES or AES.\nKey Entry " + String.Format("{0:x02}", keyIdx).ToUpper() + " can't be changed.");
                LastError = ResultE.InternalError;
                return false;
            }

            /*	 Keep IV	*/
            if (keepInitVector)
            {
                Logger.Debug("Keeping IV ...");

                if (activeAuth.AuthType == AuthTypeE.AES)
                {
                    /* il faut 16 octets	*/
                    Array.ConstrainedCopy(encrypted_data, encrypted_data.Length - 16, initVector, 0, initVector.Length);
                }
                else
                {
                    /*	il faut 8 octets	*/
                    Array.ConstrainedCopy(encrypted_data, encrypted_data.Length - 8, initVector, 0, initVector.Length);
                }
            }

            /*	------- Send APDU	------- 	*/
            Logger.Debug("Sending 'Change Entry' Command ...");
            CAPDU capdu = new CAPDU(CLA, (byte)INS.ChangeKeyEntry, keyIdx, 0xFF, encrypted_data);
            RAPDU rapdu = Transmit(capdu);

            if (rapdu.SW == 0x9000)
            {
                return true;
            }
            else
            {
                LastError = ResultE.UnexpectedStatusWord;
                return false;
            }
        }

        public bool ChangeKeyEntry(byte keyIdx, byte[] keyEntryData)
        {
            Logger.Debug("ChangeKeyEntry({0:X02}, {1})", keyIdx, BinConvert.ToHex(keyEntryData));

            if (keyEntryData == null)
            {
                Logger.Error("No data");
                userInteraction.Error("Key Entry " + BinConvert.ToHex(keyIdx) + " has no data.");
                LastError = ResultE.InvalidParameters;
                return false;
            }

            if (keyEntryData.Length != 61)
            {
                Logger.Error("Invalid data length");
                userInteraction.Error("Key Entry " + BinConvert.ToHex(keyIdx) + " has an invalid length.");
                LastError = ResultE.InvalidParameters;
                return false;
            }

            byte set0 = keyEntryData[55];
            CAPDU capdu;
            if (((set0 & 0x38) == 0x18) || ((set0 & 0x38) == 0x28))
            {
                /* 24-byte keys : don't update VC (in P2)	*/
                capdu = new CAPDU(CLA, (byte)INS.ChangeKeyEntry, keyIdx, 0xDF, keyEntryData);
            }
            else
            {
                capdu = new CAPDU(CLA, (byte)INS.ChangeKeyEntry, keyIdx, 0xFF, keyEntryData);
            }

            if (!Command(capdu, out RAPDU rapdu))
                return false;

            if (rapdu.SW != 0x9000)
            {
                Logger.Error("SW={0:X04}", rapdu.SW);
                userInteraction.Error(string.Format("Failed to change key entry {0:X02} (SAM error)", keyIdx));
                LastError = ResultE.ExecutionFailed;
                return false;
            }

            return true;
        }
		
	}
	
}