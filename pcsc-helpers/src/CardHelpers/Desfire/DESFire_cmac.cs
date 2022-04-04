/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 14:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using System;

namespace SpringCard.PCSC.CardHelpers
{
    /// <summary>
    /// Description of DESFire_core.
    /// </summary>
    public partial class Desfire
    {
        int XferCmacSend(bool append)
        {
            byte[] cmac = new byte[8];

            if ((session_type & KEY_ISO_MODE) != KEY_ISO_MODE)
                return DF_OPERATION_OK;

            /* compute cmac only when lest frame of a command */
            if (xfer_buffer[0] == DF_ADDITIONAL_FRAME)
            {
                if (xfer_length == 1)
                {
                    return DF_OPERATION_OK;
                }
                else
                {
                    return DFCARD_UNEXPECTED_CHAINING;
                }
            }
            if (secure_mode == SecureMode.EV2)
            {
                ComputeCmacEv2(xfer_buffer, xfer_length, false, ref cmac);
            }
            else
            {
                ComputeCmac(xfer_buffer, xfer_length, false, ref cmac);
            }

            /* add cmac */
            if (append)
            {
                Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
                xfer_length += 8;
            }

            return DF_OPERATION_OK;

        }

        int XferCmacRecv()
        {
            if (xfer_buffer[0] == DF_ADDITIONAL_FRAME)
                return DFCARD_UNEXPECTED_CHAINING;

            if (secure_mode == SecureMode.EV2)
            {
                return VerifyCmacRecvEv2(xfer_buffer, ref xfer_length);
            }
            else
            {
                return VerifyCmacRecv(xfer_buffer, ref xfer_length);
            }
        }


        void InitCmac()
        {
            byte bMSB;
            byte block_size, rb_xor_value;
            UInt32 t, i;

            logger.trace("InitCmac");

            rb_xor_value = (byte)((session_type == KEY_ISO_AES) ? 0x87 : 0x1B);
            block_size = (byte)((session_type == KEY_ISO_AES) ? 16 : 8);

            cmac_subkey_1 = new byte[block_size];
            cmac_subkey_2 = new byte[block_size];

            byte[] abSavedInitVktr = new byte[init_vector.Length];

            // Save the InitVector:
            Array.ConstrainedCopy(init_vector, 0, abSavedInitVktr, 0, init_vector.Length);

            // Generate the padding bytes for O-MAC by enciphering a zero block
            // with the actual session key:
            for (i = 0; i < cmac_subkey_1.Length; i++)
                cmac_subkey_1[i] = 0;

            logger.debug("\tstep0. subkey1={0}", BinConvert.ToHex(cmac_subkey_1));

            t = block_size;
            /*
            {
              Console.Write("Before CipherSend, cmac_subkey_1=");
              for (int k=0; k<t; k++)
                Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
              Console.Write("\n");
            }
            */
            CipherSend(ref cmac_subkey_1, ref t, t);

            logger.debug("\tstep1. subkey1={0}", BinConvert.ToHex(cmac_subkey_1));

            /*
            {
              Console.Write("After CipherSend, cmac_subkey_1=");
              for (int k=0; k<t; k++)
                Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
              Console.Write("\n");
            }
            */

            // If the MSBit of the generated cipher == 1 -> K1 = (cipher << 1) ^ Rb ...
            // store MSB:
            bMSB = cmac_subkey_1[0];

            // Shift the complete cipher for 1 bit ==> K1:
            byte tmp;
            for (i = 0; i < (UInt32)(block_size - 1); i++)
            {
                tmp = (byte)((cmac_subkey_1[i] << 1) & 0x00FE);
                cmac_subkey_1[i] = tmp;

                // add the carry over bit:
                cmac_subkey_1[i] |= (byte)(((cmac_subkey_1[i + 1] & 0x80) != 0) ? 0x01 : 0x00);
            }

            tmp = (byte)((cmac_subkey_1[block_size - 1] << 1) & 0x00FE);
            cmac_subkey_1[block_size - 1] = tmp;
            if ((bMSB & 0x80) != 0)
            {
                // XOR with Rb:
                cmac_subkey_1[block_size - 1] ^= rb_xor_value;
            }

            logger.debug("\tstep2. subkey1={0}", BinConvert.ToHex(cmac_subkey_1));

            /*
            {
              Console.Write("After shift, cmac_subkey_1=");
              for (int k=0; k<t; k++)
                Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
              Console.Write("\n");
            }
            */

            // store MSB:
            bMSB = cmac_subkey_1[0];

            // Shift K1 ==> K2:
            for (i = 0; i < (UInt32)(block_size - 1); i++)
            {
                cmac_subkey_2[i] = (byte)((cmac_subkey_1[i] << 1) & 0x00FE);
                cmac_subkey_2[i] |= (byte)(((cmac_subkey_1[i + 1] & 0x80) != 0) ? 0x01 : 0x00);
            }
            cmac_subkey_2[block_size - 1] = (byte)((cmac_subkey_1[block_size - 1] << 1) & 0x00FE);

            if ((bMSB & 0x80) == 0x80)
            {
                // XOR with Rb:
                cmac_subkey_2[block_size - 1] ^= rb_xor_value;
            }

            logger.debug("\tstep3. subkey2={0}", BinConvert.ToHex(cmac_subkey_2));

            /*
            {
              Console.Write("After shift, cmac_subkey_2=");
              for (int k=0; k<t; k++)
                Console.Write(String.Format("{0:x02}", cmac_subkey_2[k]));
              Console.Write("\n");
            }
            */

            // We have to restore the InitVector:
            Array.ConstrainedCopy(abSavedInitVktr, 0, init_vector, 0, init_vector.Length);

            logger.trace("CMAC SubKey1={0}", BinConvert.ToHex(cmac_subkey_1));
            logger.trace("CMAC SubKey2={0}", BinConvert.ToHex(cmac_subkey_2));
        }

        void ComputeCmac(byte[] data, UInt32 length, bool move_status, ref byte[] cmac)
        {
            UInt32 i, actual_length, block_size;

            /*{
              Console.Write("Data to CMAC over: ");
              for (int k=0; k<length; k++)
                Console.Write(String.Format("{0:x02}", data[k]));
              Console.Write("\n");
            }*/


            /* means we are working with SAM for MAC */
            if (sam_channel != null)
            {
                byte[] mac_data = new byte[length];
                Array.Copy(data, 0, mac_data, 0, length);
                SAM_GenerateMAC(mac_data, ref cmac);
                return;
            }


            /*
            {
              Console.Write("Data to CMAC over: ");
              for (int k=0; k<length; k++)
                Console.Write(String.Format("{0:x02}", data[k]));
              Console.Write("\n");
            }
            */

            if ((session_type & KEY_ISO_MODE) != KEY_ISO_MODE)
                Console.WriteLine("INVALID FUNCTION CALL 'Compute CMAC'\n");

            // Adapt the crypto mode if the sessionkey is done in CBC_Send_Decrypt:
            // enCryptoMethod = (m_SessionKeyCryptoMethod == CRM_3DES_DF4 ? CRM_3DES_ISO:m_SessionKeyCryptoMethod);

            block_size = (uint)((session_type == KEY_ISO_AES) ? 16 : 8);

            // First we enlarge eNumOfBytes to a multiple of the cipher block size for allocating
            // memory of the intermediate buffer. Zero padding will be done by the DF8Encrypt function.
            // If we are ISO-authenticated, we have to do the spetial padding for the O-MAC:
            actual_length = length;
            while ((actual_length % block_size) != 0)
                actual_length++;

            byte[] buffer = new byte[actual_length];
            for (i = 0; i < actual_length; i++)
                buffer[i] = 0;


            if (move_status)
            {
                Array.ConstrainedCopy(data, 1, buffer, 0, (int)(length - 1));
                buffer[length - 1] = data[0];
            }
            else
            {
                Array.ConstrainedCopy(data, 0, buffer, 0, (int)(length));
            }

            /*
            {
              Console.Write("Before padding, buffer: ");
              for (int k=0; k<actual_length; k++)
                Console.Write(String.Format("{0:x02}", buffer[k]));
              Console.Write("\n");
            }
            */

            /* Do the ISO padding and/or XORing */

            if ((length % block_size) != 0)
            {

                /* Block incomplete -> padding */
                buffer[length++] = 0x80;

                /*
                {
                  Console.Write("after padding, buffer: ");
                  for (int k=0; k<actual_length; k++)
                    Console.Write(String.Format("{0:x02}", buffer[k]));
                  Console.Write("\n");
                }
                */

                /* XOR the last eight bytes with CMAC_SubKey2 */
                length = actual_length - block_size;
                for (i = 0; i < block_size; i++)
                {
                    buffer[length + i] ^= (byte)(cmac_subkey_2[i]);
                }
            }
            else
            {

                /* Block complete -> no padding */

                /* XOR the last eight bytes with CMAC_SubKey1 */
                length = actual_length - block_size;
                for (i = 0; i < block_size; i++)
                {
                    buffer[length + i] ^= (byte)(cmac_subkey_1[i]);
                }
            }
            /*
            {
              Console.Write("After padding, buffer: ");
              for (int k=0; k<actual_length; k++)
                Console.Write(String.Format("{0:x02}", buffer[k]));
              Console.Write("\n");
            }
            */

            CipherSend(ref buffer, ref actual_length, actual_length);

            // Save the current init vector, which is the last cipher block of the cryptogram:
            Array.ConstrainedCopy(buffer, (int)(actual_length - block_size), init_vector, 0, (int)block_size);

            if (cmac != null)
            {
                // The mac is the first half of the init vector:
                Array.ConstrainedCopy(init_vector, 0, cmac, 0, 8);
            }

        }
        public bool SAM_GenerateMAC(byte[] data_to_update_mac, ref byte[] cmac)
        {
            CAPDU capdu;
            RAPDU rapdu;

            if (sam_channel == null)
            {
                return false;
            }
            if (data_to_update_mac == null)
            {
                return false;
            }

            /* Ask the SAM to update its CMAC */
            /* ------------------------------ */
            capdu = new CAPDU(0x80, (byte)0x7C /*INS.GenerateMac*/, 0x00, 0x00, data_to_update_mac, 0x00);
            Logger.Debug("SAM<{0}", capdu.ToString());
            rapdu = sam_channel.Transmit(capdu);
            if (rapdu == null)
            {
                return false;
            }
            Logger.Debug("SAM>{0}", rapdu.ToString());
            if (rapdu.SW != 0x9000)
            {
                return false;
            }

            //mac = new byte[rapdu.DataBytes.Length];
            if (cmac.Length < rapdu.DataBytes.Length)
                return false;

            Array.Copy(rapdu.DataBytes, 0, cmac, 0, rapdu.DataBytes.Length);
            return true;
        }

        bool SAM_VerifyMAC(byte status, byte[] mac_to_check, int MacLength, bool fKeepStatus)
        {
            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp = null;
            int capdu_data_len = 0;
            int RemainingLength = 0;


            if (sam_channel == null)
            {
                return false;
            }

            if (mac_to_check.Length > 0xFF)
            {
                /* Several frames need to be sent to the SAM */
                /* ----------------------------------------- */

                /* Construct a buffer, intercaling the status byte (00) between data and CMAC, if needed */
                /* Only one frame needs to be sent to the SAM */
                /* ------------------------------------------ */

                if (fKeepStatus)
                {
                    capdu_data_len = mac_to_check.Length + 1;
                    /* Construct a buffer, intercaling the status byte (00) between data and CMAC, if needed */
                    temp = new byte[capdu_data_len];
                    Array.Copy(mac_to_check, 0, temp, 0, (mac_to_check.Length - MacLength));
                    capdu_data_len += (mac_to_check.Length - MacLength);
                    temp[capdu_data_len++] = status;
                    Array.Copy(mac_to_check, (mac_to_check.Length - MacLength), temp, capdu_data_len, MacLength);

                    RemainingLength = mac_to_check.Length + 1;
                }
                else
                {
                    capdu_data_len = mac_to_check.Length;
                    /* Construct a buffer, intercaling the status byte (00) between data and CMAC, if needed */
                    temp = new byte[capdu_data_len];
                    Array.Copy(mac_to_check, 0, temp, 0, mac_to_check.Length);
                    RemainingLength = mac_to_check.Length;
                }
                int idx = 0;
                byte[] frame = null;
                while (RemainingLength > 255)
                {
                    frame = new byte[255];
                    Array.Copy(temp, idx * 255, frame, 0, 255);
                    capdu = new CAPDU(0x80, (byte)0x5C/*INS.VerifyMac*/, 0xAF, 0x00, frame);
                    Logger.Debug("SAM<{0}", capdu.ToString());
                    rapdu = sam_channel.Transmit(capdu);
                    if (rapdu == null)
                    {
                        return false;
                    }
                    Logger.Debug("SAM>{0}", rapdu.ToString());

                    if (rapdu.SW != 0x90AF)
                    {
                        return false;
                    }
                    RemainingLength -= 255;
                    idx++;
                }
                frame = new byte[RemainingLength];
                Array.Copy(temp, idx * 255, frame, 0, RemainingLength);
                capdu = new CAPDU(0x80, (byte)0x5C/*INS.VerifyMac*/, 0x00, (byte)MacLength, frame);
                Logger.Debug("SAM<{0}", capdu.ToString());
                rapdu = sam_channel.Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }
                Logger.Debug("SAM>{0}", rapdu.ToString());

                if (rapdu.SW != 0x9000)
                {
                    return false;
                }

            }
            else
            {
                /* Only one frame needs to be sent to the SAM */
                /* ------------------------------------------ */
                if (fKeepStatus)
                {
                    capdu_data_len = mac_to_check.Length + 1;
                    /* Construct a buffer, intercaling the status byte (00) between data and CMAC, if needed */
                    temp = new byte[capdu_data_len];
                    Array.Copy(mac_to_check, 0, temp, 0, (mac_to_check.Length - MacLength));
                    capdu_data_len = (mac_to_check.Length - MacLength);
                    temp[capdu_data_len++] = status;
                    Array.Copy(mac_to_check, (mac_to_check.Length - MacLength), temp, capdu_data_len, MacLength);
                }
                else
                {
                    temp = new byte[mac_to_check.Length];
                    Array.Copy(mac_to_check, 0, temp, 0, mac_to_check.Length);
                }
                capdu = new CAPDU(0x80, (byte)0x5C/*INS.VerifyMac*/, 0x00, 0x00, temp);
                Logger.Debug("SAM<{0}", capdu.ToString());
                rapdu = sam_channel.Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }
                Logger.Debug("SAM>{0}", rapdu.ToString());

                if (rapdu.SW != 0x9000)
                {
                    return false;
                }
            }
            if (xfer_length > 8)
                xfer_length -= 8;

            return true;

        }

        int VerifyCmacRecv(byte[] buffer, ref UInt32 length)
        {
            UInt32 l;
            byte[] cmac = new byte[8];

            if ((session_type & KEY_ISO_MODE) != KEY_ISO_MODE)
                return DF_OPERATION_OK;

            l = length;

            if (l < 9)
                return DFCARD_WRONG_LENGTH;

            l -= 8;

            /*{
              Console.Write("Verify - CMAC calculated on :");
              for (int i=0; i<l; i++)
                Console.Write(String.Format("{0:x02}", buffer[i]));
              Console.Write("\n");
            }*/

            /* means we are working with SAM for MAC */
            if (sam_channel != null)
            {
                byte[] mac_data = null;
                /* just status code inside answer */
                if (l == 1)
                {
                    /* copy cmac returned by desfire */
                    mac_data = new byte[8];
                    Array.Copy(buffer, 1, mac_data, 0, 8);
                }
                else if (l > 1)
                {
                    /* copy cmac returned by desfire */
                    mac_data = new byte[8 + (l - 1)];
                    Array.Copy(buffer, 1, mac_data, 0, 8 + (l - 1));
                }
                else
                {
                    return DFCARD_WRONG_MAC;
                }

                if (SAM_VerifyMAC(buffer[0], mac_data, (int)8, true) == true)
                {
                    length = l;
                    return DF_OPERATION_OK;
                }
                else
                    return DFCARD_WRONG_MAC;


            }

            ComputeCmac(buffer, l, true, ref cmac);


            /*{
              Console.Write("CMAC calculated :");
              for (int i=0; i<8; i++)
                Console.Write(String.Format("{0:x02}", cmac[i]));
              Console.Write("\n");
            }*/

            for (int i = 0; i < 8; i++)
                if (buffer[l + i] != cmac[i])
                    return DFCARD_WRONG_MAC;

            length = l;
            return DF_OPERATION_OK;


        }
        /// <summary>
        ///  NIST Special Publication 800-38B. Use KsessionAuthMac
        /// </summary>
        void InitCmacEv2()
        {

            byte bMSB;
            byte block_size, rb_xor_value;
            UInt32 t, i;
            byte[] IV = new byte[16];
            rb_xor_value = 0x87;
            block_size = 16;

            cmac_subkey_1 = new byte[block_size];
            cmac_subkey_2 = new byte[block_size];

            // Generate the padding bytes for O-MAC by enciphering a zero block
            // with the actual session key:
            for (i = 0; i < cmac_subkey_1.Length; i++)
                cmac_subkey_1[i] = 0;

            t = block_size;
#if _VERBOSE
      Console.WriteLine("=================================================================");
      Console.WriteLine("InitCmacEv2 L= " + BinConvert.ToHex(cmac_subkey_1, t));
#endif
            CipherSP80038B(SesAuthMACKey, IV, null, ref cmac_subkey_1, ref t, t);
#if _VERBOSE
      Console.WriteLine("InitCmacEv2 SesAuthMACKey= " + BinConvert.ToHex(SesAuthMACKey, SesAuthMACKey.Length));
#endif

            // If the MSBit of the generated cipher == 1 -> K1 = (cipher << 1) ^ Rb ...
            // store MSB:
            bMSB = cmac_subkey_1[0];

            // Shift the complete cipher for 1 bit ==> K1:
            byte tmp;
            for (i = 0; i < (UInt32)(block_size - 1); i++)
            {
                tmp = (byte)((cmac_subkey_1[i] << 1) & 0x00FE);
                cmac_subkey_1[i] = tmp;

                // add the carry over bit:
                cmac_subkey_1[i] |= (byte)(((cmac_subkey_1[i + 1] & 0x80) != 0) ? 0x01 : 0x00);
            }

            tmp = (byte)((cmac_subkey_1[block_size - 1] << 1) & 0x00FE);
            cmac_subkey_1[block_size - 1] = tmp;
            if ((bMSB & 0x80) != 0)
            {
                // XOR with Rb:
                cmac_subkey_1[block_size - 1] ^= rb_xor_value;
            }
#if _VERBOSE
      Console.WriteLine("InitCmacEv2 cmac_subkey_1= " + BinConvert.ToHex(cmac_subkey_1, t));
#endif

            // store MSB:
            bMSB = cmac_subkey_1[0];

            // Shift K1 ==> K2:
            for (i = 0; i < (UInt32)(block_size - 1); i++)
            {
                cmac_subkey_2[i] = (byte)((cmac_subkey_1[i] << 1) & 0x00FE);
                cmac_subkey_2[i] |= (byte)(((cmac_subkey_1[i + 1] & 0x80) != 0) ? 0x01 : 0x00);
            }
            cmac_subkey_2[block_size - 1] = (byte)((cmac_subkey_1[block_size - 1] << 1) & 0x00FE);

            if ((bMSB & 0x80) == 0x80)
            {
                // XOR with Rb:
                cmac_subkey_2[block_size - 1] ^= rb_xor_value;
            }
#if _VERBOSE
      Console.WriteLine("InitCmacEv2 cmac_subkey_2= " + BinConvert.ToHex(cmac_subkey_2, t));
#endif

        }

        /// <summary>
        /// MAC(KsAuthMac,Cmd||CmdCtr||TI||CmdHeader||CmdData))
        /// </summary>
        /// <param name="data">input for cmac calculation</param>
        /// <param name="length">lenght of data</param>
        /// <param name="move_status"></param>
        /// <param name="cmac"></param>
        void ComputeCmacEv2(byte[] data, UInt32 length, bool move_status, ref byte[] cmac)
        {
            UInt32 i, actual_length, block_size;
            int index_data = 7;
            byte[] IV = new byte[16];

#if _VERBOSE
      Console.WriteLine("#### ComputeCmacEv2' " + BinConvert.ToHex(data, length));
#endif

            if ((session_type & KEY_ISO_MODE) != KEY_ISO_MODE)
                Console.WriteLine("INVALID FUNCTION CALL 'Compute CMAC'\n");

            /* cryto is AES, so 16 */
            block_size = 16;

            // First we enlarge eNumOfBytes to a multiple of the cipher block size for allocating
            // memory of the intermediate buffer. Zero padding will be done by the DF8Encrypt function.
            // If we are ISO-authenticated, we have to do the spetial padding for the O-MAC:
            actual_length = length;
            if (TransactionIdentifier != null)
            {
                /* add 6 for CmdCtr and TI */
                actual_length += 6;
                while ((actual_length % block_size) != 0)
                    actual_length++;
            }
            else
            {
                if (actual_length == 0)
                    actual_length = 16;
                else
                {
                    if (actual_length == 0)
                        actual_length++;

                    while ((actual_length % block_size) != 0)
                        actual_length++;
                }
            }

            byte[] buffer = new byte[actual_length];
            for (i = 0; i < actual_length; i++)
                buffer[i] = 0;

            /* true when request verifying */
            /* true it is or response */
            if (move_status)
            {
                if (TransactionIdentifier == null)
                {
                    Array.ConstrainedCopy(data, 1, buffer, 0, (int)(length - 1));
                    buffer[length - 1] = data[0];
                }
                else
                {
                    /* RespStatus */
                    Array.ConstrainedCopy(data, 0, buffer, 0, 1);

                    /* CmdCtr */
                    buffer[1] = (byte)(CmdCtr & 0x00FF);
                    buffer[2] = (byte)((CmdCtr >> 8) & 0x00FF);

                    /* TI */
                    Array.ConstrainedCopy(TransactionIdentifier, 0, buffer, 3, 4);

                    /* RespData */
                    Array.ConstrainedCopy(data, 1, buffer, index_data, (int)(length - 1));
                }
            }
            else
            {
                if (TransactionIdentifier == null)
                {
                    Array.ConstrainedCopy(data, 0, buffer, 0, (int)(length));
                }
                else
                {
                    /* CmdHeader */
                    Array.ConstrainedCopy(data, 0, buffer, 0, 1);
                    /* CmdCtr */
                    //Array.ConstrainedCopy(CmdCtr, 0, buffer, 1, 2);
                    buffer[1] = (byte)(CmdCtr & 0x00FF);
                    buffer[2] = (byte)((CmdCtr >> 8) & 0x00FF);
                    /* TI */
                    Array.ConstrainedCopy(TransactionIdentifier, 0, buffer, 3, 4);
                    /* CmdHeader + CmdData */
                    Array.ConstrainedCopy(data, 1, buffer, index_data, (int)(length - 1));
                }
            }
#if _VERBOSE
      if (length > 0)
      {
        Console.WriteLine("ComputeCmacEv2 Before padding " +
                 BinConvert.ToHex(buffer, 0, 1) +
           " " + BinConvert.ToHex(buffer, 1, 2) +
           " " + BinConvert.ToHex(buffer, 3, 4) +
           " " + BinConvert.ToHex(buffer, (uint)index_data, length - 1));

        Console.WriteLine("ComputeCmacEv2 Before padding " + BinConvert.ToHex(buffer, length + 6));
      }
#endif
            if (TransactionIdentifier == null)
            {
                if (length == 0)
                    buffer[0] = 0x80;
                else if ((length % block_size) != 0)
                    buffer[length] = 0x80;

#if _VERBOSE
        Console.WriteLine("ComputeCmacEv2 padding padded buffer' " + BinConvert.ToHex(buffer, actual_length));
#endif
                if (((length % block_size) != 0) || (length == 0))
                {
                    CipherSP80038B(SesAuthMACKey, IV, cmac_subkey_2, ref buffer, ref actual_length, actual_length);
                }
                else
                {
                    CipherSP80038B(SesAuthMACKey, IV, cmac_subkey_1, ref buffer, ref actual_length, actual_length);
                }
            }
            /* Do the ISO padding and/or XORing */
            /* modulo has to be done on Cmd + Ctr + Ti */
            else if ((TransactionIdentifier != null) && ((((length + 6) % block_size) != 0) || (length == 0)))
            {
                /*if (TransactionIdentifier == null)
                {
                  if (length == 0)
                    buffer[0] = 0x80;
                  else
                    buffer[length] = 0x80;
                }
                else
                {*/
                /* Block incomplete -> padding */
                if (buffer.Length > (length + 6))
                {
                    buffer[length + 6] = 0x80;
                }
                //}
#if _VERBOSE
        Console.WriteLine("ComputeCmacEv2 padding padded buffer' " + BinConvert.ToHex(buffer, actual_length));
#endif
                CipherSP80038B(SesAuthMACKey, IV, cmac_subkey_2, ref buffer, ref actual_length, actual_length);
            }
            else
            {

                /* Block complete -> no padding */
#if _VERBOSE
          Console.WriteLine("ComputeCmacEv2 no padding buffer' " + BinConvert.ToHex(buffer, actual_length));
#endif
                CipherSP80038B(SesAuthMACKey, IV, cmac_subkey_1, ref buffer, ref actual_length, actual_length);
            }


#if _VERBOSE
      Console.WriteLine("ComputeCmacEv2 ciphered' " + BinConvert.ToHex(buffer, (buffer.Length - 16), 16));
#endif
            if (cmac != null)
            {
                /* cmac is present on the last 16 digits */
                /* MACt(key, message) = S14 || S12 || S10 || S8 || S6 || S4 || S2 || S0 */
                for (int z = 0; (z < 8) && (buffer.Length >= 16); z++)
                {
                    cmac[z] = buffer[(buffer.Length - 16) + (z * 2) + 1];
                }

#if _VERBOSE
        Console.WriteLine("ComputeCmacEv2 CMAC' " + BinConvert.ToHex(cmac, 8));
#endif
            }

        }

        int VerifyCmacRecvEv2(byte[] buffer, ref UInt32 length)
        {
            UInt32 l;
            byte[] cmac = new byte[8];

            if ((session_type & KEY_ISO_MODE) != KEY_ISO_MODE)
                return DF_OPERATION_OK;

            l = length;

            if (l < 9)
                return DFCARD_WRONG_LENGTH;

            l -= 8;
#if _VERBOSE
      Console.WriteLine("VerifyCmacRecvEv2 - CMAC Response: " + BinConvert.ToHex(buffer, l, 8));
#endif
            ComputeCmacEv2(buffer, l, true, ref cmac);

#if _VERBOSE
      Console.WriteLine("VerifyCmacRecvEv2 CMAC calculated : " + BinConvert.ToHex(cmac, 8));
#endif


            for (int i = 0; i < 8; i++)
            {
                if (buffer[l + i] != cmac[i])
                {
#if _VERBOSE
          Console.WriteLine("VerifyCmacRecvEv2 DFCARD_WRONG_MAC");
#endif
                    return DFCARD_WRONG_MAC;
                }
            }

#if _VERBOSE
      Console.WriteLine("VerifyCmacRecvEv2 DF_OPERATION_OK");
#endif
            length = l;
            return DF_OPERATION_OK;


        }
        /// <summary>
        /// NIST Special Publication 800-38B.
        /// Used to calculate Session Key.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <param name="input"></param>
        /// <param name="cmac"></param>
        /// <returns></returns>
        public bool CalculateCMACEV2(byte[] Key, byte[] IV, byte[] input, ref byte[] cmac)
        {
            uint i;
            uint actual_length;
            uint block_size;
            uint length = (uint)input.Length;

#if _VERBOSE
      Console.WriteLine("=================================================================");
      Console.WriteLine("CalculateCMACEV2 Key= " + BinConvert.ToHex(Key, Key.Length));
      Console.WriteLine("CalculateCMACEV2 " + BinConvert.ToHex(input, length));
#endif
            SetSesAuthMACKey(Key);
            InitCmacEv2();

            /*if ((session_type & KEY_ISO_MODE) != KEY_ISO_MODE)
              Console.WriteLine("INVALID FUNCTION CALL 'Compute CMAC'\n");*/

            /* cryto is AES, so 16 */
            block_size = 16;

            // First we enlarge eNumOfBytes to a multiple of the cipher block size for allocating
            // memory of the intermediate buffer. Zero padding will be done by the DF8Encrypt function.
            // If we are ISO-authenticated, we have to do the spetial padding for the O-MAC:
            actual_length = length;

            if (actual_length == 0)
            {
                actual_length = 16;
            }
            else
            {
                while ((actual_length % block_size) != 0)
                    actual_length++;
            }

            byte[] buffer = new byte[actual_length];
            for (i = 0; i < actual_length; i++)
                buffer[i] = 0;

            Array.ConstrainedCopy(input, 0, buffer, 0, (int)(length));

#if _VERBOSE
      if (input.Length > 0)
      {
        Console.WriteLine("CalculateCMACEV2 before padding " + BinConvert.ToHex(buffer, actual_length));
      }
#endif

            /* Do the ISO padding and/or XORing */
            if (((length % block_size) != 0) || (length == 0))
            {
                if (length == 0)
                    buffer[0] = 0x80;
                else
                    buffer[length] = 0x80;

#if _VERBOSE
        Console.WriteLine("CalculateCMACEV2 padding padded buffer " + BinConvert.ToHex(buffer, actual_length));
#endif
                CipherSP80038B(Key, IV, cmac_subkey_2, ref buffer, ref actual_length, actual_length);
            }
            else
            {
                /* Block complete -> no padding */
#if _VERBOSE
        Console.WriteLine("CalculateCMACEV2 no padding buffer " + BinConvert.ToHex(buffer, actual_length));
#endif
                CipherSP80038B(Key, IV, cmac_subkey_1, ref buffer, ref actual_length, actual_length);
            }


#if _VERBOSE
      Console.WriteLine("CalculateCMACEV2 ciphered " + BinConvert.ToHex(buffer, (buffer.Length - 16), 16));
#endif
            if ((cmac != null) && (cmac.Length >= 16))
            {
                Array.ConstrainedCopy(buffer, (buffer.Length - 16), cmac, 0, 16);
            }
            else
            {
                return false;
            }

#if _VERBOSE
      Console.WriteLine("CalculateCMACEV2 CMAC " + BinConvert.ToHex(cmac, 16));
#endif
            return true;
        }

    }
}
