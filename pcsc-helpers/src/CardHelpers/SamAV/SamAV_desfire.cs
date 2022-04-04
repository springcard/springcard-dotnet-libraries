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
        const int max_length_frame = 54;

        public bool DesfireAuthenticateEx(ICardApduTransmitter DesfireCard,
            byte KeyMode,
            byte DesfireKeyIdx,
            bool fDivAv2Mode,
            bool fApplicationKeyNo,
            bool fDivTwoRounds,
            byte SamKeyIdx,
            byte KeyVersion,
            byte[] pbDivInp)
        {
            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp;
            byte p1 = 0x00;


            if ((pbDivInp != null) && (pbDivInp.Length > 0))
                p1 |= 0x01;

            if (fApplicationKeyNo)
                p1 |= 0x02;

            if (fDivTwoRounds)
                p1 |= 0x08;

            if ((pbDivInp != null) && (pbDivInp.Length > 0))
            {
                if (fDivAv2Mode)
                {
                    /* 1 to 31 bytes for the AV2 key diversification methods */
                    if (pbDivInp.Length > 31)
                    {
                        return false;
                    }
                    p1 |= 0x10;
                }                    
                else
                {
                    /* The diversification input has to be of 8(DES) or 16(AES) bytes length for the AV1 compatibility key */
                    if ((pbDivInp.Length != 8) && (pbDivInp.Length != 16))
                    {
                        return false;
                    }
                }
            }

            /* Send Authenticate AES command to the Desfire card */
            /* ------------------------------------------------- */
            capdu = new CAPDU(Desfire.CLA, KeyMode, 0x00, 0x00, new byte[] { DesfireKeyIdx }, 0x00);
            Logger.Debug("Desfire<{0}", capdu.ToString());
            rapdu = DesfireCard.Transmit(capdu);
            if (rapdu == null)
            {
                _StatusWord = rapdu.SW;
                return false;
            }
            Logger.Debug("Desfire>{0}", rapdu.ToString());
            if (rapdu.SW != 0x91AF)
            {
                _StatusWord = rapdu.SW;
                return false;
            }

            /* Send SAM_AuthenticatePICC command to the SAM */
            /* -------------------------------------------- */
            temp = new byte[] { SamKeyIdx, KeyVersion };
            temp = BinUtils.Concat(temp, rapdu.DataBytes);
            if (pbDivInp == null)
            {
                /* No diversification */
                capdu = new CAPDU(CLA, (byte)INS.AuthenticatePicc, p1, 0x00, temp, 0x00);
            }
            else
            {
                /* Diversification with UID */
                temp = BinUtils.Concat(temp, pbDivInp);
                capdu = new CAPDU(CLA, (byte)INS.AuthenticatePicc, p1, 0x00, temp, 0x00);
            }
            Logger.Debug("SAM<{0}", capdu.ToString());
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                _StatusWord = rapdu.SW;
                return false;
            }
            Logger.Debug("SAM>{0}", rapdu.ToString());
            if (rapdu.SW != 0x90AF)
            {
                _StatusWord = rapdu.SW;
                return false;
            }

            /* Forward SAM's response to the card */
            /* ---------------------------------- */

            temp = rapdu.DataBytes;
            capdu = new CAPDU(Desfire.CLA, 0xAF, 0x00, 0x00, temp, 0x00);
            Logger.Debug("Desfire<{0}", capdu.ToString());
            rapdu = DesfireCard.Transmit(capdu);
            if (rapdu == null)
            {
                _StatusWord = rapdu.SW;
                return false;
            }
            Logger.Debug("Desfire>{0}", rapdu.ToString());
            if (rapdu.SW != 0x9100)
            {
                _StatusWord = rapdu.SW;
                return false;
            }

            /* Forward card's response to the SAM */
            /* ---------------------------------- */

            temp = rapdu.DataBytes;
            capdu = new CAPDU(CLA, (byte)INS.AuthenticatePicc, 0x00, 0x00, temp);
            Logger.Debug("SAM<{0}", capdu.ToString());
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                _StatusWord = rapdu.SW;
                return false;
            }
            Logger.Debug("SAM>{0}", rapdu.ToString());
            if (rapdu.SW != 0x9000)
            {
                _StatusWord = rapdu.SW;
                return false;
            }

            /* OK ! */
            return true;
        }

        public bool DesfireAuthenticate(ICardApduTransmitter DesfireCard,
            byte DesfireKeyIdx,
            bool fDivAv2Mode,
            bool fApplicationKeyNo,
            bool fDivTwoRounds,
            byte SamKeyIdx,
            byte KeyVersion,
            byte[] pbDivInp)
        {
            return DesfireAuthenticateEx(DesfireCard,
            Desfire.DF_AUTHENTICATE,
            DesfireKeyIdx,
            fDivAv2Mode,
            fApplicationKeyNo,
            fDivTwoRounds,
            SamKeyIdx,
            KeyVersion,
            pbDivInp);
        }

        public bool DesfireAuthenticateAes(ICardApduTransmitter DesfireCard,
            byte DesfireKeyIdx,
            bool fDivAv2Mode,
            bool fApplicationKeyNo,
            byte SamKeyIdx,
            byte KeyVersion,
            byte[] pbDivInp)
        {
            return DesfireAuthenticateEx(DesfireCard,
            Desfire.DF_AUTHENTICATE_AES,
            DesfireKeyIdx,
            fDivAv2Mode,
            fApplicationKeyNo,
            false,
            SamKeyIdx,
            KeyVersion,
            pbDivInp);
        }
        public bool DesfireReadUID(ICardApduTransmitter DesfireCard, out byte[] Uid)
        {
            Uid = null;

            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp;

            /* Ask the SAM to update its CMAC */
            /* ------------------------------ */

            temp = new byte[1];
            temp[0] = Desfire.DF_GET_CARD_UID;

            capdu = new CAPDU(CLA, (byte)INS.GenerateMac, 0x00, 0x00, temp, 0x00);
            Logger.Debug("SAM<{0}", capdu.ToString());
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                return false;
            }
            Logger.Debug("SAM>{0}", rapdu.ToString());
            if (rapdu.SW != 0x9000)
            {
                return false;
            }

            /* Ask the card to return its UID */
            /* ------------------------------ */

            capdu = new CAPDU(Desfire.CLA, Desfire.DF_GET_CARD_UID, 0x00, 0x00, 0x00);
            Logger.Debug("Desfire<{0}", capdu.ToString());
            rapdu = DesfireCard.Transmit(capdu);
            if (rapdu == null)
            {
                return false;
            }
            Logger.Debug("Desfire>{0}", rapdu.ToString());
            if (rapdu.SW != 0x9100)
            {
                return false;
            }

            /* We've got the ciphered UID in rapdu.DataBytes */
            /* Ask the SAM to decipher data */
            byte status = rapdu.SW2;
            byte[] cipher = new byte[rapdu.DataBytes.Length];

            Array.Copy(rapdu.DataBytes, 0, cipher, 0, rapdu.DataBytes.Length);
            return SAM_DecipherData(status, 7, cipher, false, out Uid);

        }

        public bool DesfireReadData(ICardApduTransmitter DesfireCard, byte mode, byte FileId, uint Offset, uint Length, out byte[] Data)
        {
            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp;
            byte[] cmde = null;
            uint RemainingLength = 0;
            bool first_frame = true;
            Data = null;

            if ((Length != 0) && (Length <= Offset))
                return false;

            RemainingLength = Length - Offset;

            /* Ask the SAM to update its CMAC */
            /* ------------------------------ */

            temp = new byte[8];
            temp[0] = Desfire.DF_READ_DATA;
            temp[1] = FileId;
            temp[2] = (byte)(Offset & 0x0FF);
            temp[3] = (byte)((Offset >> 8) & 0x0FF);
            temp[4] = (byte)((Offset >> 16) & 0x0FF);
            temp[5] = (byte)(Length & 0x0FF);
            temp[6] = (byte)((Length >> 8) & 0x0FF);
            temp[7] = (byte)((Length >> 16) & 0x0FF);

            capdu = new CAPDU(CLA, (byte)INS.GenerateMac, 0x00, 0x00, temp, 0x00);
            Logger.Debug("SAM<{0}", capdu.ToString());
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                return false;
            }
            Logger.Debug("SAM>{0}", rapdu.ToString());
            if (rapdu.SW != 0x9000)
            {
                return false;
            }

            cmde = new byte[7];
            Array.Copy(temp, 1, cmde, 0, 7);

            if (RemainingLength > max_length_frame)
            {
                //temp = new byte[max_length_frame];

                while (RemainingLength > max_length_frame)
                {
                    /* first frame */
                    if (first_frame == true)
                    {
                        capdu = new CAPDU(Desfire.CLA, Desfire.DF_READ_DATA, 0x00, 0x00, cmde, 0x00);
                        first_frame = false;
                    }
                    else
                    {
                        capdu = new CAPDU(Desfire.CLA, 0xAF, 0x00, 0x00, 0x00);
                    }
                    RemainingLength -= (uint)rapdu.DataBytes.Length;

                    Logger.Debug("Desfire<{0}", capdu.ToString());
                    rapdu = DesfireCard.Transmit(capdu);
                    if (rapdu == null)
                    {
                        return false;
                    }

                    Logger.Debug("Desfire>{0}", rapdu.ToString());
                    if (rapdu.SW != 0x91AF)
                    {
                        break;
                    }
                    Data =  BinUtils.Concat(Data, rapdu.DataBytes);
                }

                if (rapdu.SW != 0x9100)
                {
                    return false;
                }

                Data = BinUtils.Concat(Data, rapdu.DataBytes);

                if (mode == Desfire.DF_COMM_MODE_ENCIPHERED)
                {
                    byte status = rapdu.SW2;
                    byte[] cipher = new byte[Data.Length];

                    Array.Copy(Data, 0, cipher, 0, Data.Length);
                    return SAM_DecipherData(status, (int)Length, cipher, false, out Data);
                }
                else if (mode == Desfire.DF_COMM_MODE_MACED)
                {
                    byte status = rapdu.SW2;
                    byte[] mac = new byte[rapdu.DataBytes.Length];
                    Array.Copy(rapdu.DataBytes, 0, mac, 0, rapdu.DataBytes.Length);

                    if (SAM_VerifyMAC(status, mac, 8, true) == false)
                    {
                        return false;
                    }
                }
            }
            else
            {

                /* Ask the card to read its file */
                /* ----------------------------- */
                

                capdu = new CAPDU(Desfire.CLA, Desfire.DF_READ_DATA, 0x00, 0x00, cmde, 0x00);
                Logger.Debug("Desfire<{0}", capdu.ToString());
                rapdu = DesfireCard.Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }
                Logger.Debug("Desfire>{0}", rapdu.ToString());
                if (rapdu.SW != 0x9100)
                {
                    return false;
                }

                if (mode == Desfire.DF_COMM_MODE_ENCIPHERED)
                {
                    byte status = rapdu.SW2;
                    byte[] cipher = new byte[rapdu.DataBytes.Length];

                    Array.Copy(rapdu.DataBytes, 0, cipher, 0, rapdu.DataBytes.Length);
                    return SAM_DecipherData(status, (int)Length, cipher, false, out Data);
                }
                else if (mode == Desfire.DF_COMM_MODE_MACED)
                {
                    byte status = rapdu.SW2;
                    byte[] mac = new byte[rapdu.DataBytes.Length];
                    Array.Copy(rapdu.DataBytes, 0, mac, 0, rapdu.DataBytes.Length);

                    if (SAM_VerifyMAC(status, mac, 8, true) == false)
                    {
                        return false;
                    }
                }

                Data = rapdu.DataBytes;
            }
            return true;
        }

        public bool DesfireWriteData(ICardApduTransmitter DesfireCard, byte mode, byte FileId, uint Offset, uint Length, byte[] Data)
        {           
            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp;
            byte[] result = null;
            byte[] cmde = null;
            int RemainingLength = 0;
            

            /* out of limits */
            if (Data.Length < Length)
            {
                return false;
            }
            /* out of limits */
            if (Data.Length < (Offset+Length))
            {
                return false;
            }

            /* Ask the SAM to update its CMAC */
            /* ------------------------------ */

            temp = new byte[8+ Length];
            temp[0] = Desfire.DF_WRITE_DATA;
            temp[1] = FileId;
            temp[2] = (byte)(Offset & 0x0FF);
            temp[3] = (byte)((Offset >> 8) & 0x0FF);
            temp[4] = (byte)((Offset >> 16) & 0x0FF);
            temp[5] = (byte)(Length & 0x0FF);
            temp[6] = (byte)((Length >> 8) & 0x0FF);
            temp[7] = (byte)((Length >> 16) & 0x0FF);


            /*P2 = 08; means the encryption starts from 8th byte of the data in data field(count from 0th). */
            if (mode == Desfire.DF_COMM_MODE_ENCIPHERED)
            {
                Array.Copy(Data, Offset, temp, 8, Length);
                SAM_EncipherData(temp, 8, out result) ;
                if (result == null)
                    return false;

                cmde = new byte[result.Length + 7];

                Array.Copy(temp, 1, cmde, 0, 7);
                Array.Copy(result, 0, cmde, 7, result.Length);
            }
            else if (mode == Desfire.DF_COMM_MODE_MACED)
            {
                Array.Copy(Data, Offset, temp, 8, Length);
                SAM_GenerateMAC(temp, out result);
                
                if (result == null)
                    return false;
                cmde = new byte[result.Length + 7 + Data.Length];
                Array.Copy(temp, 1, cmde, 0, 7);
                Array.Copy(Data, Offset, cmde, 7, Length);
                Array.Copy(result, 0, cmde, 7 + Length, result.Length);
            }
            else
                return false;

            RemainingLength = cmde.Length;


            if (cmde.Length > max_length_frame)
            {
                temp = new byte[max_length_frame/* + 1*/];

                while (RemainingLength > max_length_frame)
                {
                    Array.Copy(cmde, cmde.Length - RemainingLength, temp, 0, max_length_frame/* + 1*/);
                    
                    /* first frame */
                    if (RemainingLength == cmde.Length)
                    {
                        capdu = new CAPDU(Desfire.CLA, Desfire.DF_WRITE_DATA, 0x00, 0x00, temp, 0x00);
                    }
                    else
                    {
                        capdu = new CAPDU(Desfire.CLA, 0xAF/*Desfire.DF_ADDITIONAL_FRAME*/, 0x00, 0x00, temp, 0x00);
                    }
                    RemainingLength -= (max_length_frame/* + 1*/);

                    Logger.Debug("Desfire<{0}", capdu.ToString());
                    rapdu = DesfireCard.Transmit(capdu);
                    if (rapdu == null)
                    {
                        return false;
                    }

                    Logger.Debug("Desfire>{0}", rapdu.ToString());
                    if (rapdu.SW != 0x91AF)
                    {
                        return false;
                    }
                }

                temp = new byte[RemainingLength];
                Array.Copy(cmde, cmde.Length - RemainingLength, temp, 0, RemainingLength);

                capdu = new CAPDU(Desfire.CLA, 0xAF/*Desfire.DF_ADDITIONAL_FRAME*/, 0x00, 0x00, temp, 0x00);
                Logger.Debug("Desfire<{0}", capdu.ToString());
                rapdu = DesfireCard.Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }

                Logger.Debug("Desfire>{0}", rapdu.ToString());
                if (rapdu.SW != 0x9100)
                {
                    return false;
                }

                byte status = rapdu.SW2;
                byte[] mac = new byte[rapdu.DataBytes.Length];
                Array.Copy(rapdu.DataBytes, 0, mac, 0, rapdu.DataBytes.Length);

                if (SAM_VerifyMAC(status, mac, 8, true) == false)
                {
                    return false;
                }
            }
            else
            {
                capdu = new CAPDU(Desfire.CLA, Desfire.DF_WRITE_DATA, 0x00, 0x00, cmde, 0x00);
                Logger.Debug("Desfire<{0}", capdu.ToString());
                rapdu = DesfireCard.Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }

                Logger.Debug("Desfire>{0}", rapdu.ToString());
                if (rapdu.SW != 0x9100)
                {
                    return false;
                }

                byte status = rapdu.SW2;
                byte[] mac = new byte[rapdu.DataBytes.Length];
                Array.Copy(rapdu.DataBytes, 0, mac, 0, rapdu.DataBytes.Length);

                if (SAM_VerifyMAC(status, mac, 8, true) == false)
                {
                    return false;
                }
            }


            return true;

        }

        public bool DesfireSelectApplication(UInt32 aid)
        {
            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp;


            temp = new byte[3];
            temp[0] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            temp[1] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            temp[2] = (byte)(aid & 0x000000FF);

            capdu = new CAPDU(CLA, (byte)INS.SelectApplication, 0x00, 0x00, temp);
            Logger.Debug("SAM<{0}", capdu.ToString());
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                return false;
            }
            Logger.Debug("SAM>{0}", rapdu.ToString());
            if (rapdu.SW != 0x9000)
            {
                return false;
            }

            return true;
        }


        public bool SAM_DecipherData(byte status, int ResultLength, byte[] cipher_to_check, bool fSkipStatus, out byte[] decipher_data)
        {
            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp = null;
            int capdu_data_len = 0;
            int RemainingLength = cipher_to_check.Length;
            bool first_loop = true;

            decipher_data = null;

            if (cipher_to_check.Length > 0xF0)
            {
                /* Several frames need to be sent to the SAM */
                /* ----------------------------------------- */

                while (RemainingLength > 0xF0)
                {
                    
                    /* First loop: add three bytes for length, if known */
                    if (first_loop && (ResultLength != 0))
                    {
                        temp = new byte[0xF0 + 3];

                        temp[capdu_data_len++] = (byte)(ResultLength & 0x000000FF);
                        ResultLength >>= 8;
                        temp[capdu_data_len++] = (byte)(ResultLength & 0x000000FF);
                        ResultLength >>= 8;
                        temp[capdu_data_len++] = (byte)(ResultLength & 0x000000FF);
                    }
                    else
                    {
                        temp = new byte[0xF0];
                        capdu_data_len = 0;
                    }

                    Array.Copy(cipher_to_check, (cipher_to_check.Length- RemainingLength), temp, capdu_data_len, 0xF0);
                    capdu_data_len += cipher_to_check.Length;

                    /*if (!fSkipStatus)
                    {
                        temp[capdu_data_len++] = status;
                    }*/

                    capdu = new CAPDU(CLA, (byte)INS.DecipherData, 0xAF, 0x00, temp, 0x00);
                    Logger.Debug("SAM<{0}", capdu.ToString());
                    rapdu = Transmit(capdu);
                    if (rapdu == null)
                    {
                        return false;
                    }
                    Logger.Debug("SAM>{0}", rapdu.ToString());
                    if (decipher_data == null)
                        decipher_data = rapdu.DataBytes;
                    else
                        decipher_data = BinUtils.Concat(decipher_data, rapdu.DataBytes);

                    if (rapdu.SW != 0x90AF)
                    {
                        return false;
                    }

                    if (first_loop)
                        first_loop = false;

                    RemainingLength -= 0xF0;
                }

                /* This is the last APDU  */
                if (fSkipStatus)
                {
                    temp = new byte[RemainingLength];
                }                        
                else
                {
                    temp = new byte[RemainingLength + 1];
                }
                Array.Copy(cipher_to_check, (cipher_to_check.Length - RemainingLength), temp, 0, RemainingLength);
                if (!fSkipStatus)
                {
                    temp[RemainingLength] = status;
                }

                capdu = new CAPDU(CLA, (byte)INS.DecipherData, 0x00, 0x00, temp, 0x00);
                Logger.Debug("SAM<{0}", capdu.ToString());
                rapdu = Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }
                Logger.Debug("SAM>{0}", rapdu.ToString());
                if (decipher_data == null)
                    decipher_data = rapdu.DataBytes;
                else
                    decipher_data = BinUtils.Concat(decipher_data, rapdu.DataBytes);

                if (rapdu.SW != 0x9000)
                {
                    return false;
                }
                return true;
            }
            else
            {
                if (fSkipStatus)
                    temp = new byte[cipher_to_check.Length + 3];
                else
                    temp = new byte[cipher_to_check.Length + 4];
                //temp = new byte[20];

                /* Only one frame needs to be sent to the SAM */
                /* ------------------------------------------ */
                temp[capdu_data_len++] = (byte)(ResultLength & 0x000000FF);
                ResultLength >>= 8;
                temp[capdu_data_len++] = (byte)(ResultLength & 0x000000FF);
                ResultLength >>= 8;
                temp[capdu_data_len++] = (byte)(ResultLength & 0x000000FF);

                Array.Copy(cipher_to_check, 0, temp, capdu_data_len, cipher_to_check.Length);
                capdu_data_len += cipher_to_check.Length;

                
                if (!fSkipStatus)
                {
                    temp[capdu_data_len++] = status;
                }
                
                capdu = new CAPDU(CLA, (byte)INS.DecipherData, 0x00, 0x00, temp, 0x00);
                Logger.Debug("SAM<{0}", capdu.ToString());
                rapdu = Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }
                Logger.Debug("SAM>{0}", rapdu.ToString());
                decipher_data = rapdu.DataBytes;
                if (rapdu.SW != 0x9000)
                {
                    return false;
                }

            }
            return true;
        }

        public bool SAM_EncipherData( byte[] data_to_cipher, int offset, out byte[] ciphered_data)
        {
            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp = null;
            int RemainingLength = data_to_cipher.Length;
            bool first_loop = true;
            const int max_frame_size = 0xEC;
            ciphered_data = null;

            // 0xF6 DES + CR16
            // 0xEC AES
            // 0xF5 DES + CRC32
            // 0xE5 AES + CMAC


            if (data_to_cipher.Length > max_frame_size)
            {
                /* Several frames need to be sent to the SAM */
                /* ----------------------------------------- */

                while (RemainingLength > max_frame_size)
                {

                    /* First loop: add three bytes for length, if known */
                    if (first_loop )
                    {
                        temp = new byte[max_frame_size];
                        Array.Copy(data_to_cipher, 0, temp, 0, temp.Length);
                        capdu = new CAPDU(CLA, (byte)INS.CipherData, 0xAF, (byte)offset, temp, 0x00);
                        first_loop = false;
                    }
                    else
                    {
                        temp = new byte[max_frame_size];
                        Array.Copy(data_to_cipher, (data_to_cipher.Length - RemainingLength), temp, 0, temp.Length);
                        capdu = new CAPDU(CLA, (byte)INS.CipherData, 0xAF, 0x00, temp, 0x00);
                    }

                    Logger.Debug("SAM<{0}", capdu.ToString());
                    rapdu = Transmit(capdu);
                    if (rapdu == null)
                    {
                        return false;
                    }
                    Logger.Debug("SAM>{0}", rapdu.ToString());

                    ciphered_data = BinUtils.Concat(ciphered_data, rapdu.DataBytes);
                    if (rapdu.SW != 0x90AF)
                    {
                        return false;
                    }

                    if (first_loop)
                        first_loop = false;

                    RemainingLength -= max_frame_size;
                }

                temp = new byte[RemainingLength];
                Array.Copy(data_to_cipher, (data_to_cipher.Length - RemainingLength), temp, 0, temp.Length);

                capdu = new CAPDU(CLA, (byte)INS.CipherData, 0x00, 0x00, temp, 0x00);
                Logger.Debug("SAM<{0}", capdu.ToString());
                rapdu = Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }
                Logger.Debug("SAM>{0}", rapdu.ToString());

                //ciphered_data = new byte[rapdu.DataBytes.Length];
                // Array.Copy(rapdu.DataBytes, 0, ciphered_data, 0, rapdu.DataBytes.Length);
                //ciphered_data = rapdu.DataBytes;
                ciphered_data = BinUtils.Concat(ciphered_data, rapdu.DataBytes);
                if (rapdu.SW != 0x9000)
                {
                    return false;
                }

            }
            else
            {
                capdu = new CAPDU(CLA, (byte)INS.CipherData, 0x00, (byte)offset, data_to_cipher, 0x00);
                Logger.Debug("SAM<{0}", capdu.ToString());
                rapdu = Transmit(capdu);
                if (rapdu == null)
                {
                    return false;
                }
                Logger.Debug("SAM>{0}", rapdu.ToString());

                ciphered_data = new byte[rapdu.DataBytes.Length];
                Array.Copy(rapdu.DataBytes, 0, ciphered_data, 0, rapdu.DataBytes.Length);
                //ciphered_data = rapdu.DataBytes;
                if (rapdu.SW != 0x9000)
                {
                    return false;
                }
            }
            return true;
        }

        public bool SAM_GenerateMAC(byte[] data_to_update_mac, out byte[] mac)
        {
            CAPDU capdu;
            RAPDU rapdu;

            mac = null;

            if ( data_to_update_mac == null)
            {
                return false;
            }

            /* Ask the SAM to update its CMAC */
            /* ------------------------------ */
            capdu = new CAPDU(CLA, (byte)INS.GenerateMac, 0x00, 0x00, data_to_update_mac, 0x00);
            Logger.Debug("SAM<{0}", capdu.ToString());
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                return false;
            }
            Logger.Debug("SAM>{0}", rapdu.ToString());
            if (rapdu.SW != 0x9000)
            {
                return false;
            }

            mac = new byte[rapdu.DataBytes.Length];
            Array.Copy(rapdu.DataBytes, 0, mac, 0, rapdu.DataBytes.Length);
            return true;
        }

        bool SAM_VerifyMAC(byte status, byte[] mac_to_check, int MacLength, bool fKeepStatus)
        {
            CAPDU capdu;
            RAPDU rapdu;
            byte[] temp = null;
            int capdu_data_len = 0;
            int RemainingLength = 0;

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
                    capdu = new CAPDU(CLA, (byte)INS.VerifyMac, 0xAF, 0x00, frame);
                    Logger.Debug("SAM<{0}", capdu.ToString());
                    rapdu = Transmit(capdu);
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
                capdu = new CAPDU(CLA, (byte)INS.VerifyMac, 0x00, (byte)MacLength, frame);
                Logger.Debug("SAM<{0}", capdu.ToString());
                rapdu = Transmit(capdu);
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
                capdu = new CAPDU(CLA, (byte)INS.VerifyMac, 0x00, 0x00, temp);
                Logger.Debug("SAM<{0}", capdu.ToString());
                rapdu = Transmit(capdu);
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

            return true;

        }
        private bool DesfireChangeKeyEx(ICardApduTransmitter DesfireCard,
            byte DesfireKeyType,
            byte DesfireKeyIdx,
            bool fDivAv2Mode,
            bool fNewDivEnable,
            bool fNewDivTwoRounds,
            bool fOldDivEnable,
            bool fOldDivTwoRounds,
            bool fChangeMasterCardKey,
            byte OldSamKeyIdx,
            byte OldKeyVersion,
            byte NewSamKeyIdx,
            byte NewKeyVersion,
            byte[] pbDivInp)
        {
            CAPDU capdu;
            RAPDU rapdu;
            byte KeyCompMeth = 0x00;
            byte Cfg = 0x00;
            byte[] temp = new byte[4];
            int capdu_len = 0;


            /* AV2 or AV1 mode compatibility*/
            if ((pbDivInp != null) && (pbDivInp.Length > 0))
            {
                temp = new byte[4 + pbDivInp.Length];
                if (fDivAv2Mode)
                {
                    /* 1 to 31 bytes for the AV2 key diversification methods */
                    if (pbDivInp.Length > 31)
                    {
                        return false;
                    }                    
                }
                else
                {
                    /* The diversification input has to be of 8(DES) or 16(AES) bytes length for the AV1 compatibility key */
                    if ((pbDivInp.Length != 8) && (pbDivInp.Length != 16))
                    {
                        return false;
                    }
                }
            }

            if ((pbDivInp != null) && (pbDivInp.Length > 0))
            {
                /*use of key diversification for new key:*/
                if (fNewDivEnable)
                    KeyCompMeth |= 0x02;

                /*use of key diversification for current key*/
                if (fOldDivEnable)
                    KeyCompMeth |= 0x04;
            }

            if (fDivAv2Mode)
                KeyCompMeth |= 0x20;

            /* key diversification method for new key number of round */
            if (fNewDivTwoRounds)
                KeyCompMeth |= 0x08;

            /* key diversification method for current key number of round */
            if (fOldDivTwoRounds)
                KeyCompMeth |= 0x10;

            /* when the ChangeKey key of the targeting application
             * is 0E, or the master key itself is changed)
             */
            if (DesfireKeyIdx == 0x00)
            {
                KeyCompMeth |= 0x01;
            }

            /* bit 0 to 3 desfire idx */
            Cfg = (byte)(0x0F & DesfireKeyIdx);
            /* bit 4, key master card shall be changed */
            if ((fChangeMasterCardKey) && (DesfireKeyIdx == 0x00))
                Cfg |= 0x10;

            temp[capdu_len++] = OldSamKeyIdx;
            temp[capdu_len++] = OldKeyVersion;
            temp[capdu_len++] = NewSamKeyIdx;
            temp[capdu_len++] = NewKeyVersion;

            if ((pbDivInp != null) && (pbDivInp.Length > 0))
            {
                Array.Copy(pbDivInp, 0, temp, capdu_len, pbDivInp.Length);
            }

            capdu = new CAPDU(CLA, (byte)INS.ChangeKeyPicc, KeyCompMeth, Cfg, temp, 0x00);

            Logger.Debug("SAM<{0}", capdu.ToString());
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                return false;
            }
            Logger.Debug("SAM>{0}", rapdu.ToString());

            if (rapdu.SW != 0x9000)
            {
                Logger.Debug("SAM> Card not authenticated");
                return false;
            }

            /* Send SAM Answer to DESFire Card */
            /* ------------------------------- */
           
            temp = new byte[rapdu.DataBytes.Length + 1];
            temp[0] = DesfireKeyIdx;
            if ((Cfg & 0x10) != 0x00)
                temp[0] |= DesfireKeyType;

            Array.Copy(rapdu.DataBytes, 0, temp, 1, rapdu.DataBytes.Length);

            /* Send the 2nd frame to the PICC and get its response. */
            capdu = new CAPDU(Desfire.CLA, Desfire.DF_CHANGE_KEY, 0x00, 0x00, temp, 0x00);
            Logger.Debug("Desfire<{0}", capdu.ToString());
            rapdu = DesfireCard.Transmit(capdu);
            if (rapdu == null)
            {
                return false;
            }
            Logger.Debug("Desfire>{0}", rapdu.ToString());

            /* 3. Verify MAC, if present */
            if ( (rapdu.SW == 0x9100) && (rapdu.DataBytes.Length > 1) )
            {
                byte status = rapdu.SW2;
                byte[] mac = new byte[rapdu.DataBytes.Length];
                Array.Copy(rapdu.DataBytes, 0, mac, 0, rapdu.DataBytes.Length);
                return SAM_VerifyMAC(status, mac, 8, true);
            }
            if ((rapdu.SW != 0x9000) && (rapdu.SW != 0x9100))
            {
                return false;
            }
            return true;
        }

        public bool DesfireChangeKeyAes(ICardApduTransmitter DesfireCard,
            byte DesfireKeyIdx,
            bool fDivAv2Mode,
            bool fNewDivEnable,
            bool fOldDivEnable,
            bool fChangeMasterCardKey,
            byte OldSamKeyIdx,
            byte OldKeyVersion,
            byte NewSamKeyIdx,
            byte NewKeyVersion,
            byte[] pbDivInp)
        {
            return DesfireChangeKeyEx(DesfireCard,
                Desfire.DF_APPLSETTING2_AES,
                DesfireKeyIdx,
                fDivAv2Mode,
                fNewDivEnable,
                false,
                fOldDivEnable,
                false,
                fChangeMasterCardKey,
                OldSamKeyIdx,
                OldKeyVersion,
                NewSamKeyIdx,
                NewKeyVersion,
                pbDivInp);
        }
    }
}
