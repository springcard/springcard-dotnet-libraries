using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class Desfire
    {
        const byte SW_AUTHENT = 0x01;
        const byte SW_SELECT_DF = 0x02;
        const byte SW_WRITE = 0x04;

        static bool Comparebytes(byte[] a1, uint a1_offset, byte[] a2, uint a2_offset, uint size)
        {
            for (int i = 0; i < size; ++i)
            {
                if (a1[a1_offset + i] != a2[a2_offset + i])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="INS"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="Lc"></param>
        /// <param name="data_in"></param>
        /// <param name="Le"></param>
        /// <param name="data_out"></param>
        /// <param name="data_out_len"></param>
        /// <param name="SW"></param>
        /// <returns></returns>
        long IsoApdu(byte INS, byte P1, byte P2, byte Lc, byte[] data_in, byte Le, ref byte[] data_out, ref UInt16 data_out_len, ref UInt16 SW)
        {

            long status = 0;
            byte[] send_buffer;
            UInt16 send_length = 0;

            byte[] recv_buffer = new byte[280];
            int recv_length = recv_buffer.Length;
            UInt16 i;
            UInt16 _SW;


            if (data_out_len != 0)
                send_buffer = new byte[Lc + 6];
            else
                send_buffer = new byte[Lc + 5];

            if (((Lc == 0) && (data_in != null)) || ((Lc != 0) && (data_in == null)))
                return DFCARD_LIB_CALL_ERROR;

            send_buffer[send_length++] = 0;
            send_buffer[send_length++] = INS;
            send_buffer[send_length++] = P1;
            send_buffer[send_length++] = P2;
            send_buffer[send_length++] = Lc;
            if (data_in != null)
            {                
                for (i = 0; i < Lc; i++)
                    send_buffer[send_length++] = data_in[i];
            }
            if (data_out_len != 0)
            {
                send_buffer[send_length++] = Le;
            }

            Logger.Debug(">> " + BinConvert.ToHex(send_buffer, send_length));
#if _VERBOSE
            Console.WriteLine(">> " + BinConvert.ToHex(send_buffer, send_length));
#endif

            recv_buffer = transmitter.Transmit(send_buffer);

            if (recv_buffer != null)
            {
                Logger.Debug(">> " + BinConvert.ToHex(recv_buffer));
#if _VERBOSE
                Console.WriteLine(">> " + BinConvert.ToHex(recv_buffer));
#endif
                recv_length = (ushort)recv_buffer.Length;
            }
            else
            {
                Logger.Debug(">> ERROR");
                return -1;
            }

            if (recv_length < 2)
                return DFCARD_PCSC_BAD_RESP_LEN;

            recv_length -= 2;

            //if (data_out_len != null)
            data_out_len = (UInt16)recv_length;

            if (data_out != null)
            {
                if ((Le == 0) && (recv_length > 256))
                {
                    recv_length = 256;
                    status = DFCARD_OVERFLOW;
                }
                else
                  if ((Le != 0) && (recv_length > Le))
                {
                    recv_length = Le;
                    status = DFCARD_OVERFLOW;
                }

                //memcpy(data_out, recv_buffer, recv_length);
                Array.ConstrainedCopy(recv_buffer, 0, data_out, 0, recv_length);
            }

            _SW = recv_buffer[recv_length + 0];
            _SW <<= 8;
            _SW |= recv_buffer[recv_length + 1];

            //if (SW is not null)
            SW = _SW;

            if (_SW != 0x9000)
                status = DFCARD_PCSC_BAD_RESP_SW;

            return status;
        }

        long IsoApduExt(byte CLS, byte INS, byte P1, byte P2, byte Lc, byte[] data_in, byte Le, byte[] data_out, ref UInt16 data_out_len, ref UInt16 SW)
        {
            long status = 0;
            byte[] send_buffer;
            UInt16 send_length = 0;
            byte[] recv_buffer = null;// new byte[280];

            if (data_out_len != 0)
                send_buffer = new byte[Lc + 6];
            else
                send_buffer = new byte[Lc + 5];

#if !_USE_PCSC
            int recv_length = 0;// (UInt16)recv_buffer.Length;
#else
      UInt32 recv_length = sizeof(recv_buffer);
#endif
            UInt16 i;
            UInt16 _SW = 0x9000;

            if (((Lc == 0) && (data_in != null)) || ((Lc != 0) && (data_in == null)))
                return DFCARD_LIB_CALL_ERROR;

            send_buffer[send_length++] = CLS;
            send_buffer[send_length++] = INS;
            send_buffer[send_length++] = P1;
            send_buffer[send_length++] = P2;
            send_buffer[send_length++] = Lc;
            if (data_in != null)
            {                
                for (i = 0; i < Lc; i++)
                    send_buffer[send_length++] = data_in[i];
            }
            if (data_out != null)
            {
                send_buffer[send_length++] = Le;
            }

            recv_buffer = transmitter.Transmit(send_buffer);
            recv_length = recv_buffer.Length;
            if (recv_length < 2)
                return DFCARD_PCSC_BAD_RESP_LEN;

            /* Wrapping of the native MIFARE Plus EV1 APDU Format */
            if (CLS == 0x90)
            {
                _SW = recv_buffer[recv_length - 2];
                _SW <<= 8;
                _SW |= recv_buffer[recv_length - 1];
            }

            recv_length -= 2;

            //if (data_out_len != null)
            data_out_len = (UInt16)recv_length;

            if (data_out != null)
            {
                if ((Le == 0) && (recv_length > 256))
                {
                    recv_length = 256;
                    status = DFCARD_OVERFLOW;
                }
                else if ((Le != 0) && (recv_length > Le))
                {
                    recv_length = Le;
                    status = DFCARD_OVERFLOW;
                }
                /*if (CLS != 0x90)
                    //memcpy(data_out, &recv_buffer[2], recv_length);
                    Array.ConstrainedCopy(recv_buffer, 2, data_out, 0, recv_length);
                else
                    //memcpy(data_out, &recv_buffer[0], recv_length);*/
                    Array.ConstrainedCopy(recv_buffer, 0, data_out, 0, recv_length);
            }

            if (CLS != 0x90)
            {
                _SW = recv_buffer[recv_length + 0];
                _SW <<= 8;
                _SW |= recv_buffer[recv_length + 1];
            }

            //if (SW != null)
            SW = _SW;


            if (CLS == 0x90)
            {
                if (_SW != 0x9190)
                    status = DFCARD_PCSC_BAD_RESP_SW;
            }
            else
            {
                if (_SW != 0x9000)
                    status = DFCARD_PCSC_BAD_RESP_SW;
            }

            return status;
        }


        static long TranslateSW(UInt16 SW, byte param)
        {
            long status;

            switch (SW)
            {
                case 0x9000:
                    status = DF_OPERATION_OK;
                    break;

                case 0x9190:
                    status = DF_OPERATION_OK;
                    break;

                case 0x6282: /* End of file reached before reading Le bytes */
                    status = DFCARD_ERROR - DF_BOUNDARY_ERROR;
                    break;

                case 0x6581: /* Memory failure */
                    status = DFCARD_ERROR - DF_EEPROM_ERROR;
                    break;

                case 0x6700: /* Wrong length */
                    status = DFCARD_ERROR - DF_LENGTH_ERROR;
                    break;

                case 0x6982: /* File access not allowed */
                    status = DFCARD_ERROR - DF_AUTHENTICATION_ERROR;
                    break;

                case 0x6985:
                    if ((param & SW_WRITE) != 0x00)
                    {
                        /* Access condition not satisfied */
                        status = DFCARD_ERROR - DF_PERMISSION_DENIED;
                    }
                    else
                    {
                        /* File empty */
                        status = DFCARD_ERROR - DF_BOUNDARY_ERROR;
                    }
                    break;

                case 0x6A82:
                    if ((param & SW_SELECT_DF) != 0x00)
                    {
                        /* DF not found */
                        status = DFCARD_ERROR - DF_APPLICATION_NOT_FOUND;
                    }
                    else
                    {
                        /* EF not found */
                        status = DFCARD_ERROR - DF_FILE_NOT_FOUND;
                    }
                    break;

                case 0x6A86: /* Wrong parameter P1 and/or P2 */
                    status = DFCARD_ERROR - DF_PARAMETER_ERROR;
                    break;

                case 0x6A87: /* Lc inconsistent with P1/P2 */
                    status = DFCARD_ERROR - DF_LENGTH_ERROR;
                    break;

                case 0x6B00: /* Wrong parameter P1 and/or P2 */
                    status = DFCARD_ERROR - DF_PARAMETER_ERROR;
                    break;

                case 0x6C00:
                    if ((param & SW_AUTHENT) != 0x00)
                    {
                        /* Wrong Le */
                        status = DFCARD_ERROR - DF_PARAMETER_ERROR;
                    }
                    else
                    {
                        /* File not found */
                        status = DFCARD_ERROR - DF_FILE_NOT_FOUND;
                    }
                    break;

                case 0x6D00: /* Instruction not supported */
                    status = DFCARD_ERROR - DF_ILLEGAL_COMMAND_CODE;
                    break;

                case 0x6E00: /* Wrong CLA */
                    status = DFCARD_ERROR - DF_ILLEGAL_COMMAND_CODE;
                    break;


                default:
                    status = DFCARD_PCSC_BAD_RESP_SW;
                    break;
            }

            return status;
        }

        /**f* DesfireAPI/IsoSelectApplet
         *
         * NAME
         *   IsoSelectApplet
         *
         * DESCRIPTION
         *   Send the ISO 7816-4 SELECT FILE command with the DESFIRE applet name as parameter
         *   (P2 = 0x04, DataIn = 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x00 )
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoSelectApplet(ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoSelectApplet(SPROX_INSTANCE rInst,
         *                                        ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoSelectApplet(SCARDHANDLE hCard,
         *                                      ref UInt16SW);
         *
         * INPUTS
         *   ref UInt16SW            : optional pointer to retrieve the Status UInt16 in
         *                         case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK   : success
         *   Other code if internal or communication error has occured.
         *
         * SIDE EFFECT
         *   Wrapping mode is implicitly defined to DF_isoWrapping_CARD
         *
         **/
        long IsoSelectApplet(ref UInt16? SW)
        {
            byte[] DesfireAID = new byte[] { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x00 };
            long status;
            UInt16 SW_local = 0;
            UInt16 data_out_local = 0;
            byte[] nullbyte = new byte[16];

            CleanupAuthentication();
            isoWrapping = DF_ISO_WRAPPING_CARD;

            status = IsoApdu(0xA4, 0x04, 0x00, (byte)DesfireAID.Length, DesfireAID, 0, ref nullbyte, ref data_out_local, ref SW_local);

            if (SW != null)
            {
                SW = SW_local;
            }
            return status;
        }

        /**f* DesfireAPI/IsoSelectDF
         *
         * NAME
         *   IsoSelectDF
         *
         * DESCRIPTION
         *   Implementation of ISO 7816-4 SELECT FILE command using a Directory File ID (P2=0x02)
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoSelectDF(UInt16 wFileID,
         *                                   byte abFci[],
         *                                   UInt16 wMaxFciLength,
         *                                   ref UInt16wGotFciLength,
         *                                   ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoSelectDF(SPROX_INSTANCE rInst,
         *                                    UInt16 wFileID,
         *                                    byte abFci[],
         *                                    UInt16 wMaxFciLength,
         *                                    ref UInt16wGotFciLength,
         *                                    ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoSelectDF(SCARDHANDLE hCard,
         *                                  UInt16 wFileID,
         *                                  byte abFci[],
         *                                  UInt16 wMaxFciLength,
         *                                  ref UInt16wGotFciLength,
         *                                  ref UInt16SW);
         *
         * INPUTS
         *   UInt16 wFileID        : the identifier of the DF
         *   byte abFci[]        : buffer to receive the FCI of the DF (if some)
         *   UInt16 wMaxFciLength  : maximum length of FCI
         *   ref UInt16wGotFciLength : actual length of FCI
         *   ref UInt16SW            : optional pointer to retrieve the Status UInt16 in
         *                         case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK   : success
         *   Other code if internal or communication error has occured.
         *
         * NOTES
         *   The abFci and wGotFciLength parameter could be set to null if no FCI is expected
         *   or if the caller doesn't care of the FCI.
         *   This function is also relevant for the root application (Master File -> wFileID = 0x3F00)
         *
         * SEE ALSO
         *   IsoSelectDFName
         *   IsoSelectEF
         *
         **/
        long IsoSelectDF(UInt16 wFileID, byte[] abFci, UInt16 wFciMaxLength, ref UInt16 wFciGotLength, ref UInt16 SW)
        {
            long status;
            byte[] data = new byte[2];
            UInt16 SW_local = 0;
            UInt16 data_out_local = 0;
            byte[] nullbyte = new byte[16];

            /*if (SW == null)
              SW = &_SW;*/

            /* Change application = forget authentication state */
            CleanupAuthentication();

            data[0] = (byte)(wFileID >> 8);
            data[1] = (byte)wFileID;

            /* Do the select */
            if (abFci == null)
            {
                status = IsoApdu(0xA4, 0x00, 0x0C, 2, data, 0, ref nullbyte, ref data_out_local, ref SW_local);
            }
            else
            {
                byte Le;

                Le = (wFciMaxLength > 255) ? (byte)0 : (byte)wFciMaxLength;

                status = IsoApdu(0xA4, 0x00, 0x00, 2, data, Le, ref abFci, ref wFciGotLength, ref SW_local);
            }

            /* Translate the Status UInt16 into a Desfire error code */
            if (status == DFCARD_PCSC_BAD_RESP_SW)
                status = TranslateSW(SW_local, SW_SELECT_DF);

            //if (SW != null)
            {
                SW = SW_local;
            }
            return status;
        }

        /**f* DesfireAPI/IsoSelectDFName
         *
         * NAME
         *   IsoSelectDFName
         *
         * DESCRIPTION
         *   Implementation of ISO 7816-4 SELECT FILE command using a Directory Name (P2=0x04)
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoSelectDFName(byte abDFName[],
         *                                       byte bDFNameLength,
         *                                       byte abFci[],
         *                                       UInt16 wMaxFciLength,
         *                                       ref UInt16wGotFciLength,
         *                                       ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoSelectDFName(SPROX_INSTANCE rInst,
         *                                       byte abDFName[],
         *                                       byte bDFNameLength,
         *                                       byte abFci[],
         *                                       UInt16 wMaxFciLength,
         *                                       ref UInt16wGotFciLength,
         *                                       ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoSelectDFName(SCARDHANDLE hCard,
         *                                      byte abDFName[],
         *                                      byte bDFNameLength,
         *                                      byte abFci[],
         *                                      UInt16 wMaxFciLength,
         *                                      ref UInt16wGotFciLength,
         *                                      ref UInt16SW);
         *
         * INPUTS
         *   byte abDFName : the name of the DF
         *   byte bDFNameLength  : the size of the name of the DF
         *   byte abFci[]        : buffer to receive the FCI of the DF (if some)
         *   UInt16 wMaxFciLength  : maximum length of FCI
         *   ref UInt16wGotFciLength : actual length of FCI
         *   ref UInt16SW            : optional pointer to retrieve the Status UInt16 in
         *                         case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK   : success
         *   Other code if internal or communication error has occured.
         *
         * NOTES
         *   The abFci and wGotFciLength parameter could be set to null if no FCI is expected
         *   or if the caller doesn't care of the FCI.
         *
         * SEE ALSO
         *   IsoSelectDF
         *   IsoSelectEF
         *
         **/
        long IsoSelectDFName(byte[] abDFName, byte abDFNameLength, byte[] abFci, UInt16 wFciMaxLength, ref UInt16 wFciGotLength, ref UInt16? SW)
        {
            long status;
            UInt16 SW_local = 0;
            UInt16 data_out_local = 0;
            byte[] nullbyte = new byte[16];

            /* Change application = forget authentication state */
            CleanupAuthentication();

            /* Do the select */
            if (abFci == null)
            {
                status = IsoApdu(0xA4, 0x04, 0x0C, abDFNameLength, abDFName, 0, ref nullbyte, ref data_out_local, ref SW_local);
            }
            else
            {
                byte Le;

                Le = (wFciMaxLength > 255) ? (byte)0 : (byte)wFciMaxLength;
                status = IsoApdu(0xA4, 0x04, 0x00, abDFNameLength, abDFName, Le, ref abFci, ref wFciGotLength, ref SW_local);
            }

            /* Translate the Status UInt16 into a Desfire error code */
            if (status == DFCARD_PCSC_BAD_RESP_SW)
                status = TranslateSW(SW_local, SW_SELECT_DF);

            if (SW != null)
            {
                SW = SW_local;
            }

            return status;
        }

        /**f* DesfireAPI/IsoSelectEF
         *
         * NAME
         *   IsoSelectEF
         *
         * DESCRIPTION
         *   Implementation of ISO 7816-4 SELECT FILE command using a Elementary File ID (P2=0x02)
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoSelectEF(UInt16 wFileID,
         *                                   ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoSelectEF(SPROX_INSTANCE rInst,
         *                                    UInt16 wFileID,
         *                                    ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoSelectEF(SCARDHANDLE hCard,
         *                                  UInt16 wFileID,
         *                                  ref UInt16SW);
         *
         * INPUTS
         *   UInt16 wFileID      : the identifier of the EF
         *   ref UInt16SW          : optional pointer to retrieve the Status UInt16 in
         *                       case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK   : success
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   IsoSelectDF
         *
         **/
        long IsoSelectEF(UInt16 wFileID, ref UInt16? SW)
        {
            long status;
            byte[] data = new byte[2];
            UInt16 SW_local = 0;
            UInt16 data_out_local = 0;
            byte[] nullbyte = new byte[16];

            data[0] = (byte)(wFileID >> 8);
            data[1] = (byte)wFileID;

            status = IsoApdu(0xA4, 0x02, 0x0C, 2, data, 0, ref nullbyte, ref data_out_local, ref SW_local);

            /* Translate the Status UInt16 into a Desfire error code */
            if (status == DFCARD_PCSC_BAD_RESP_SW)
                status = TranslateSW(SW_local, 0);

            if (SW != null)
            {
                SW = SW_local;
            }

            return status;
        }

        /**f* DesfireAPI/IsoReadBinary
         *
         * NAME
         *   IsoReadBinary
         *
         * DESCRIPTION
         *   Implementation of ISO 7816-4 READ BINARY command in Desfire EV1 flavour
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoReadBinary(UInt16 wOffset,
         *                                     byte abData[],
         *                                     byte bWantLength,
         *                                     ref UInt16wGotLength,
         *                                     ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoReadBinary(SPROX_INSTANCE rInst,
         *                                      UInt16 wOffset,
         *                                      byte abData[],
         *                                      byte bWantLength,
         *                                      ref UInt16wGotLength,
         *                                      ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoReadBinary(SCARDHANDLE hCard,
         *                                    UInt16 wOffset,
         *                                    byte abData[],
         *                                    byte bWantLength,
         *                                    ref UInt16wGotLength,
         *                                    ref UInt16SW);
         *
         * INPUTS
         *   UInt16 wOffset      : starting position for the read operation
         *   byte abData[]     : buffer to receive the data
         *   byte bWantLength  : maximum data length to read. Set to 0 to read 256 bytes.
         *   ref UInt16wGotLength  : actual data length read
         *   ref UInt16SW          : optional pointer to retrieve the Status UInt16 in
         *                       case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK   : success
         *   Other code if internal or communication error has occured.
         *
         * NOTES
         *   After a successfull authentication, a CMAC is added to card's response.
         *   The value of bWantLength must be choosen in consequence.
         *   This command checks the value of the CMAC in card's response and removes
         *   it from the data buffer.
         *
         * SEE ALSO
         *   IsoUpdateBinary
         *   IsoReadRecord
         *
         **/
        long IsoReadBinary(UInt16 wOffset, byte[] abData, byte bWantLength, ref UInt16 wGotLength, ref UInt16? SW)
        {
            byte P1, P2;
            long status;
            UInt16 SW_local = 0;


            if (abData == null)
                return DFCARD_LIB_CALL_ERROR;

            /*if (wGotLength == null)
              return DFCARD_LIB_CALL_ERROR;*/

            if (wOffset > 32767)
                return DFCARD_LIB_CALL_ERROR;

            P1 = (byte)(wOffset >> 8);
            P2 = (byte)wOffset;

            status = IsoApdu(0xB0, P1, P2, 0, null, bWantLength, ref abData, ref wGotLength, ref SW_local);

            if ((status == DF_OPERATION_OK) && (session_type != 0x00))
            {
                /* There's a CMAC at the end of the data */
                byte l;
                byte[] buffer = new byte[280];
                byte[] cmac = new byte[8];

                if (wGotLength < 8)
                {
                    /* CMAC not included ? */
                    return DFCARD_WRONG_LENGTH;
                }

                l = (byte)(wGotLength - 8);

                //memcpy(&buffer[0], abData, l);
                Array.ConstrainedCopy(abData, 0, buffer, 0, l);
                buffer[l++] = (byte)SW;

                ComputeCmac(buffer, l, false, ref cmac);

                wGotLength -= 8;


                //if (memcmp(cmac, &abData[wGotLength], 8))
                if (Comparebytes(cmac, 0, abData, (uint)wGotLength, 8))
                {
                    /* Wrong CMAC */
                    return DFCARD_WRONG_MAC;
                }
            }

            /* Translate the Status UInt16 into a Desfire error code */
            if (status == DFCARD_PCSC_BAD_RESP_SW)
                status = TranslateSW(SW_local, 0);

            if (SW != null)
            {
                SW = SW_local;
            }

            return status;
        }

        /**f* DesfireAPI/IsoUpdateBinary
         *
         * NAME
         *   IsoUpdateBinary
         *
         * DESCRIPTION
         *   Implementation of ISO 7816-4 UPDATE BINARY command in Desfire EV1 flavour
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoUpdateBinary(UInt16 wOffset,
         *                                       byte abData[],
         *                                       byte bLength,
         *                                       ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoUpdateBinary(SPROX_INSTANCE rInst,
         *                                        UInt16 wOffset,
         *                                        byte abData[],
         *                                        byte bLength,
         *                                        ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoUpdateBinary(SCARDHANDLE hCard,
         *                                      UInt16 wOffset;
         *                                      byte abData[],
         *                                      byte bLength,
         *                                      ref UInt16SW);
         *
         * INPUTS
         *   UInt16 wOffset        : starting position for the write operation in bytes
         *   byte abData[] : buffer containing the data to write
         *   byte bLength        : size of data to be written in bytes
         *   ref UInt16SW            : optional pointer to retrieve the Status UInt16 in
         *                         case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK     : success
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   IsoReadBinary
         *
         **/
        long IsoUpdateBinary(UInt16 wOffset, byte[] abData, byte bLength, ref UInt16? SW)
        {
            byte P1, P2;
            long status;
            UInt16 SW_local = 0;
            UInt16 data_out_local = 0;
            byte[] nullbyte = new byte[16];

            if (wOffset > 32767)
                return DFCARD_LIB_CALL_ERROR;

            P1 = (byte)(wOffset >> 8);
            P2 = (byte)wOffset;

            status = IsoApdu(0xD6, P1, P2, bLength, abData, 0, ref nullbyte, ref data_out_local, ref SW_local);

            /* Translate the Status UInt16 into a Desfire error code */
            if (status == DFCARD_PCSC_BAD_RESP_SW)
                status = TranslateSW(SW_local, SW_WRITE);

            if (SW != null)
            {
                SW = SW_local;
            }

            return status;
        }

        /**f* DesfireAPI/IsoReadRecord
         *
         * NAME
         *   IsoReadRecord
         *
         * DESCRIPTION
         *   Implementation of ISO 7816-4 READ RECORD command in Desfire EV1 flavour
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoReadRecord(byte bRecNum,
         *                                     BOOL fReadAll;
         *                                     BOOL abData[],
         *                                     UInt16 wMaxLength,
         *                                     ref UInt16wGotLength,
         *                                     ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoReadRecord(SPROX_INSTANCE rInst,
         *                                      byte bRecNum,
         *                                      BOOL fReadAll;
         *                                      BOOL abData[],
         *                                      UInt16 wMaxLength,
         *                                      ref UInt16wGotLength,
         *                                      ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoReadRecord(SCARDHANDLE hCard,
         *                                    byte bRecNum,
         *                                    BOOL fReadAll;
         *                                    BOOL abData[],
         *                                    UInt16 wMaxLength,
         *                                    ref UInt16wGotLength,
         *                                    ref UInt16SW);
         *
         * INPUTS
         *   byte bRecNum      : first (or only) record to read
         *   BOOL fReadAll     : true  : read all records (starting from bRecNum)
         *                       false : read only record # bRecNum
         *   byte abData[]     : buffer to receive the data
         *   UInt16 wMaxLength   : size of the buffer
         *   ref UInt16wGotLength  : actual data length read
         *   ref UInt16SW          : optional pointer to retrieve the Status UInt16 in
         *                       case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK   : success
         *   Other code if internal or communication error has occured.
         *
         * NOTES
         *   After a successfull authentication, a CMAC is added to card's response.
         *   This command checks the value of the CMAC in card's response and removes
         *   it from the data buffer.
         *
         * SEE ALSO
         *   IsoAppendRecord
         *   IsoReadBinary
         *
         **/
        long IsoReadRecord(byte bRecNum, bool fReadAll, byte[] abData, UInt16 wMaxLength, ref UInt16 wGotLength, ref UInt16 SW)
        {
            byte P2, Le;
            long status;
            UInt16 SW_local = 0;


            /*if (wGotLength == null)
              return DFCARD_LIB_CALL_ERROR;*/

            if (abData == null)
                return DFCARD_LIB_CALL_ERROR;

            P2 = (fReadAll) ? (byte)0x05 : (byte)0x04;
            Le = (wMaxLength > 255) ? (byte)0 : (byte)wMaxLength;

            status = IsoApdu(0xB2, bRecNum, P2, 0, null, Le, ref abData, ref wGotLength, ref SW_local);

            if ((status == DF_OPERATION_OK) && (session_type != 0x00))
            {
                /* There's a CMAC at the end of the data */
                byte l;
                byte[] buffer = new byte[280];
                byte[] cmac = new byte[8];

                if (wGotLength < 8)
                {
                    /* CMAC not included ? */
                    return DFCARD_WRONG_LENGTH;
                }

                l = (byte)(wGotLength - 8);
                //memcpy(&buffer[0], abData, l);
                Array.ConstrainedCopy(abData, 0, buffer, 0, l);
                buffer[l++] = (byte)SW;

                ComputeCmac(buffer, l, false, ref cmac);

                wGotLength -= 8;

                //if (memcmp(cmac, &abData[*wGotLength], 8))
                if (Comparebytes(cmac, 0, abData, wGotLength, 8))
                {
                    /* Wrong CMAC */
                    return DFCARD_WRONG_MAC;
                }
            }

            /* Translate the Status UInt16 into a Desfire error code */
            if (status == DFCARD_PCSC_BAD_RESP_SW)
                status = TranslateSW(SW, 0);

            return status;
        }

        /**f* DesfireAPI/IsoAppendRecord
         *
         * NAME
         *   IsoAppendRecord
         *
         * DESCRIPTION
         *   Implementation of ISO 7816-4 APPEND RECORD command in Desfire EV1 flavour
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoAppendRecord(byte abData[],
         *                                       byte bLength,
         *                                       ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoAppendRecord(SPROX_INSTANCE rInst,
         *                                        byte abData[],
         *                                        byte bLength,
         *                                        ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoAppendRecord(SCARDHANDLE hCard,
         *                                      byte abData[],
         *                                      byte bLength,
         *                                      ref UInt16SW);
         *
         * INPUTS
         *   byte abData[] : buffer containing the data to write
         *   byte bLength        : size of data to be written in bytes
         *   ref UInt16SW            : optional pointer to retrieve the Status UInt16 in
         *                         case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK     : success
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   IsoReadRecord
         *
         **/
        long IsoAppendRecord(byte[] abData, byte bLength, ref UInt16? SW)
        {
            long status;
            UInt16 SW_local = 0;
            UInt16 data_out_local = 0;
            byte[] nullbyte = new byte[16];

            status = IsoApdu(0xE2, 0x00, 0x00, bLength, abData, 0, ref nullbyte, ref data_out_local, ref SW_local);

            /* Translate the Status UInt16 into a Desfire error code */
            if (status == DFCARD_PCSC_BAD_RESP_SW)
                status = TranslateSW(SW_local, SW_WRITE);

            if (SW != null)
            {
                SW = SW_local;
            }

            return status;
        }

        /**f* DesfireAPI/IsoGetChallenge
         *
         * NAME
         *   IsoGetChallenge
         *
         * DESCRIPTION
         *   Implementation of ISO 7816-4 GET CHALLENGE command in Desfire EV1 flavour
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_IsoGetChallenge(byte bKeyAlgorithm,
         *                                       byte bRndSize,
         *                                       byte abRndCard1[],
         *                                       ref UInt16SW);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_IsoGetChallenge(SPROX_INSTANCE rInst,
         *                                        byte bRndSize,
         *                                        byte abRndCard1[],
         *                                        ref UInt16SW);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_IsoGetChallenge(SCARDHANDLE hCard,
         *                                      byte bRndSize,
         *                                      byte abRndCard1[],
         *                                      ref UInt16SW);
         *
         * INPUTS
         *   byte bRndSize              : size of the challenge
         *                                (8 bytes for DES/3DES2K, 16 bytes for 3DES3K or AES)
         *   byte abRndCard1[]          : card's first challenge (not involved in session key)
         *   ref UInt16SW                   : optional pointer to retrieve the Status UInt16 in
         *                                case an error occurs
         *
         * RETURNS
         *   DF_OPERATION_OK    : success
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   IsoMutualAuthenticate
         *   IsoExternalAuthenticate
         *   IsoInternalAuthenticate
         *
         **/
        long IsoGetChallenge(byte bRndSize, byte[] abRndCard1, ref UInt16? SW)
        {
            long status;
            UInt16 SW_local = 0;
            UInt16 data_out_local = 0;

            //status = IsoApdu(0x84, 0x00, 0x00, 0, null, bRndSize, ref abRndCard1, ref null, ref SW_local);      
            status = IsoApdu(0x84, 0x00, 0x00, 0, null, bRndSize, ref abRndCard1, ref data_out_local, ref SW_local);

            /* Translate the Status UInt16 into a Desfire error code */
            if (status == DFCARD_PCSC_BAD_RESP_SW)
                status = TranslateSW(SW_local, SW_AUTHENT);

            if (SW != null)
            {
                SW = SW_local;
            }

            return status;
        }

#if _1
    static long Desfire_IsoLoadKey(byte bKeyAlgorithm, byte[] abKeyValue)
    {  

      if (abKeyValue == null)
        return DFCARD_LIB_CALL_ERROR;

      if (bKeyAlgorithm == DF_ISO_CIPHER_2KDES)
      {
        if (!memcmp(&abKeyValue[0], &abKeyValue[8], 8))
        {
          session_type = KEY_ISO_DES;
          //InitCrypto3Des(abKeyValue[0], &abKeyValue[8], &abKeyValue[0]);
          SetKey(/*session_key*/SesAuthENCKey);
        }
        else
        {
          session_type = KEY_ISO_DES;
          InitCrypto3Des( & abKeyValue[0], &abKeyValue[0], &abKeyValue[0]);
        }
      }
      else
        if (bKeyAlgorithm == DF_ISO_CIPHER_3KDES)
      {
        session_type = KEY_ISO_3DES3K;
        InitCrypto3Des( abKeyValue[0], &abKeyValue[8], &abKeyValue[16]);
      }
      else
          if (bKeyAlgorithm == DF_ISO_CIPHER_AES)
      {
        session_type = KEY_ISO_AES;
        InitCryptoAes( abKeyValue);
      }
      else
        return DFCARD_LIB_CALL_ERROR;

      CleanupInitVector();
      return DF_OPERATION_OK;
    }

/**f* DesfireAPI/IsoExternalAuthenticate
 *
 * NAME
 *   IsoExternalAuthenticate
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 EXTERNAL AUTHENTICATE command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SUInt16 SPROX_Desfire_IsoExternalAuthenticate(byte bKeyAlgorithm,
 *                                               byte bKeyReference,
 *                                               byte bRndSize,
 *                                               byte abRndCard1[],
 *                                               byte abRndHost1[],
 *                                               byte abKeyValue[],
 *                                               ref UInt16SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SUInt16 SPROXx_Desfire_IsoExternalAuthenticate(SPROX_INSTANCE rInst,
 *                                                byte bKeyAlgorithm,
 *                                                byte bKeyReference,
 *                                                byte bRndSize,
 *                                                byte abRndCard1[],
 *                                                byte abRndHost1[],
 *                                                byte abKeyValue[],
 *                                                ref UInt16SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoExternalAuthenticate(SCARDHANDLE hCard,
 *                                              byte bKeyAlgorithm,
 *                                              byte bKeyReference,
 *                                              byte bRndSize,
 *                                              byte abRndCard1[],
 *                                              byte abRndHost1[],
 *                                              byte abKeyValue[],
 *                                              ref UInt16SW);
 *
 * INPUTS
 *   byte bKeyAlgorithm         : algorithm to be used:
 *                                - 0x02 : DES or 3DES2K (16-byte key)
 *                                - 0x04 : 3DES3K (24-byte key)
 *                                - 0x09 : AES (16-byte key)
 *   byte bKeyReference         : reference to the key in the card
 *                                - 0x00 : card's master key (valid only on root application)
 *                                - 0x8n : application's key #n
 *   byte bRndSize              : size of the challenge
 *                                (8 bytes for DES/3DES2K, 16 bytes for 3DES3K or AES)
 *   byte abRndCard1[]    : card's first challenge (as returned by IsoGetChallenge - not involved in session key)
 *   byte abRndHost1[]    : host's first challenge (choosen by the caller - involved in session key)
 *   byte abKeyValue [16] : the key itself
 *   ref UInt16SW                   : optional pointer to retrieve the Status UInt16 in
 *                                case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK    : success
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   IsoMutualAuthenticate
 *   IsoGetChallenge
 *   IsoInternalAuthenticate
 *
 **/
IsoExternalAuthenticate(byte bKeyAlgorithm, byte bKeyReference, byte bRndSize, byte abRndCard1[], byte abRndHost1[], byte abKeyValue[],ref UInt16 SW)
{
  long status;
  UInt32 t;
  UInt16 _SW;
  byte buffer[32];
  

  if (SW == null)
    SW = &_SW;

      //memcpy(&buffer[0], abRndHost1, bRndSize);
      Array.ConstrainedCopy(abRndHost1, 0, buffer, 0, bRndSize);
      //memcpy(&buffer[bRndSize], abRndCard1, bRndSize);
      Array.ConstrainedCopy(abRndCard1, 0, buffer, bRndSize, bRndSize);

      if (abKeyValue != null)
  {
    status = Desfire_IsoLoadKey bKeyAlgorithm, abKeyValue);
    if (status != DF_OPERATION_OK)
      return status;
  }

  t = 2 * bRndSize;
  Desfire_CipherSend buffer, &t, sizeof(buffer));

  status = IsoApdu) 0x82, bKeyAlgorithm, bKeyReference, (byte)(2 * bRndSize), buffer, 0, null, null, SW);

  if (status == DFCARD_PCSC_BAD_RESP_SW)
    status = TranslateSW(*SW, SW_AUTHENT);

  return status;
}

/**f* DesfireAPI/IsoInternalAuthenticate
 *
 * NAME
 *   IsoInternalAuthenticate
 *
 * DESCRIPTION
 *   Implementation of ISO 7816-4 INTERNAL AUTHENTICATE command in Desfire EV1 flavour
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SUInt16 SPROX_Desfire_IsoInternalAuthenticate(byte bKeyAlgorithm,
 *                                               byte bKeyReference,
 *                                               byte bRndSize,
 *                                               byte abRndHost2[],
 *                                               byte abRndCard2[],
 *                                               byte abKeyValue[],
 *                                               ref UInt16SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SUInt16 SPROXx_Desfire_IsoInternalAuthenticate(SPROX_INSTANCE rInst,
 *                                                byte bKeyAlgorithm,
 *                                                byte bKeyReference,
 *                                                byte bRndSize,
 *                                                byte abRndHost2[],
 *                                                byte abRndCard2[],
 *                                                byte abKeyValue[],
 *                                                ref UInt16SW);
 *
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoInternalAuthenticate(SCARDHANDLE hCard,
 *                                              byte bKeyAlgorithm,
 *                                              byte bKeyReference,
 *                                              byte bRndSize,
 *                                              byte abRndHost2[],
 *                                              byte abRndCard2[],
 *                                              byte abKeyValue[],
 *                                              ref UInt16SW);
 *
 * INPUTS
 *   byte bKeyAlgorithm         : algorithm to be used:
 *                                - 0x02 : DES or 3DES2K (16-byte key)
 *                                - 0x04 : 3DES3K (24-byte key)
 *                                - 0x09 : AES (16-byte key)
 *   byte bKeyReference         : reference to the key in the card
 *                                - 0x00 : card's master key (valid only on root application)
 *                                - 0x8n : application's key #n
 *   byte bRndSize              : size of the challenge
 *                                (8 bytes for DES/3DES2K, 16 bytes for 3DES3K or AES)
 *   byte abRndHost2[]    : host's second challenge (choosen by the caller - not involved in session key)
 *   byte abRndCard2[]          : card's second challenge (choosen by the card - involved in session key)
 *   byte abKeyValue [16] : the key itself
 *   ref UInt16SW                   : optional pointer to retrieve the Status UInt16 in
 *                                case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK    : success
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   IsoMutualAuthenticate
 *   IsoGetChallenge
 *   IsoExternalAuthenticate
 *
 **/
IsoInternalAuthenticate(byte bKeyAlgorithm, byte bKeyReference, byte bRndSize, byte abRndHost2[], byte abRndCard2[], byte abKeyValue[],ref UInt16 SW)
{
  long status;
  UInt16 length;
  byte buffer[32];
  UInt32 t;
  UInt16 _SW;
  

  if (SW == null)
    SW = &_SW;

  if (abRndHost2 == null)
    return DFCARD_LIB_CALL_ERROR;

  status = IsoApdu) 0x88, bKeyAlgorithm, bKeyReference, bRndSize, abRndHost2, (byte)(2 * bRndSize), buffer, &length, SW);
  if (status != DF_OPERATION_OK)
  {
    if (status == DFCARD_PCSC_BAD_RESP_SW)
      status = TranslateSW(*SW, SW_AUTHENT);

    return status;
  }

  if (abKeyValue != null)
  {
    status = Desfire_IsoLoadKey bKeyAlgorithm, abKeyValue);
    if (status != DF_OPERATION_OK)
      return status;
  }

  t = 2 * bRndSize;
  Desfire_CipherRecv buffer, &t);

  if (memcmp(abRndHost2, &buffer[bRndSize], bRndSize))
    status = DFCARD_WRONG_KEY;

  if (abRndCard2 != null)
        //memcpy(abRndCard2, &buffer[0], bRndSize);
        Array.ConstrainedCopy(buffer, 0, abRndCard2, 0, bRndSize);

      return status;
}

/**f* DesfireAPI/IsoMutualAuthenticate
 *
 * NAME
 *   IsoMutualAuthenticate
 *
 * DESCRIPTION
 *   Perform a mutual-authentication using the Desfire ISO 7816-4 commands (IsoGetChallenge,
 *   IsoExternalAuthenticate, IsoInternalAuthenticate) using the specified key value.
 *   Depending on bKeyAlgorithm, the key is either DES/3DES2K (16 bytes), AES (16 bytes)
 *   or 3DES3K (24 bytes).
 *   The generated session key is afterwards used for ISO CMACing.
 *
 * SYNOPSIS
 *
 *   [[sprox_desfire.dll]]
 *   SUInt16 SPROX_Desfire_IsoMutualAuthenticate(byte bKeyAlgorithm,
 *                                             byte bKeyReference,
 *                                             byte abKeyValue[],
 *                                             ref UInt16SW);
 *
 *   [[sprox_desfire_ex.dll]]
 *   SUInt16 SPROXx_Desfire_IsoMutualAuthenticate(SPROX_INSTANCE rInst,
 *                                              byte bKeyAlgorithm,
 *                                              byte bKeyReference,
 *                                              byte abKeyValue[],
 *                                              ref UInt16SW);
 *   [[pcsc_desfire.dll]]
 *   LONG  SCardDesfire_IsoMutualAuthenticate(SCARDHANDLE hCard,
 *                                            byte bKeyAlgorithm,
 *                                            byte bKeyReference,
 *                                            byte abKeyValue[],
 *                                            ref UInt16SW);
 *
 * INPUTS
 *   byte bKeyAlgorithm         : algorithm to be used:
 *                                - 0x02 : DES or 3DES2K (16-byte key)
 *                                - 0x04 : 3DES3K (24-byte key)
 *                                - 0x09 : AES (16-byte key)
 *   byte bKeyReference         : reference to the key in the card
 *                                - 0x00 : card's master key (valid only on root application)
 *                                - 0x8n : application's key #n
 *   byte abKeyValue [16] : the key itself
 *   ref UInt16SW                   : optional pointer to retrieve the Status UInt16 in
 *                                case an error occurs
 *
 * RETURNS
 *   DF_OPERATION_OK    : authentication succeed
 *   Other code if internal or communication error has occured.
 *
 * SEE ALSO
 *   Authenticate
 *   AuthenticateAes
 *   AuthenticateIso24
 *   AuthenticateIso
 *   IsoGetChallenge
 *   IsoExternalAuthenticate
 *   IsoInternalAuthenticate
 *
 **/
IsoMutualAuthenticate(byte bKeyAlgorithm, byte bKeyReference, byte abKeyValue[],ref UInt16 SW)
{
  long status;
  UInt16 _SW;

  byte bRndSize;
  byte abRndCard1[16], abRndCard2[16];
  byte abRndHost1[16], abRndHost2[16];

  

  if (SW == null) SW = &_SW;

  switch (bKeyAlgorithm)
  {
    case DF_ISO_CIPHER_2KDES: bRndSize = 8; break;
    case DF_ISO_CIPHER_3KDES: bRndSize = 16; break;
    case DF_ISO_CIPHER_AES: bRndSize = 16; break;
    default: return DFCARD_LIB_CALL_ERROR;
  }

  if (abKeyValue == null)
    return DFCARD_LIB_CALL_ERROR;

  session_key_id = bKeyReference & 0x7F;

  /* Get random bytes to populate host's challenges */
  GetRandombytes abRndHost1, bRndSize);
  GetRandombytes abRndHost2, bRndSize);

  /* Get first challenge from card */
  status = IsoGetChallenge) bRndSize, abRndCard1, SW);
  if (status != DF_OPERATION_OK)
    return status;

  /* Activate the cipher engine */
  CleanupAuthentication();
  status = Desfire_IsoLoadKey bKeyAlgorithm, abKeyValue);
  if (status != DF_OPERATION_OK)
    return status;

  /* External authenticate feeds the card with first challenge from the host */
  status = IsoExternalAuthenticate) bKeyAlgorithm, bKeyReference, bRndSize, abRndCard1, abRndHost1, null, SW);
  if (status != DF_OPERATION_OK)
    return status;

  /* Internal authenticate returns second challenge from the card */
  status = IsoInternalAuthenticate) bKeyAlgorithm, bKeyReference, bRndSize, abRndHost2, abRndCard2, null, SW);
  if (status != DF_OPERATION_OK)
    return status;

  /* Compute the session key over host's first challenge and card's second challenge */
  if (bKeyAlgorithm == DF_ISO_CIPHER_2KDES)
  {
    if (!memcmp(&abKeyValue[0], &abKeyValue[8], 8))
    {
      /* Single DES key */
      memcpy(&session_enc_key[0], &abRndHost1[0], 4);
      memcpy(&session_enc_key[4], &abRndCard2[0], 4);
      memcpy(&session_enc_key[8], &session_enc_key[0], 8);
      memcpy(&session_enc_key[16], &session_enc_key[0], 8);

      Desfire_InitCrypto3Des& session_enc_key[0], null, null);

    }
    else
    {
      /* Triple DES with 2 keys */
      memcpy(&session_enc_key[0], &abRndHost1[0], 4);
      memcpy(&session_enc_key[4], &abRndCard2[0], 4);
      memcpy(&session_enc_key[8], &abRndHost1[4], 4);
      memcpy(&session_enc_key[12], &abRndCard2[4], 4);
      memcpy(&session_enc_key[16], &session_enc_key[0], 8);

      Desfire_InitCrypto3Des& session_enc_key[0], &session_enc_key[8], null);

    }
  }
  else
    if (bKeyAlgorithm == DF_ISO_CIPHER_3KDES)
  {
    /* Triple DES with 3 keys */
    memcpy(&session_enc_key[0], &abRndHost1[0], 4);
    memcpy(&session_enc_key[4], &abRndCard2[0], 4);
    memcpy(&session_enc_key[8], &abRndHost1[6], 4);
    memcpy(&session_enc_key[12], &abRndCard2[6], 4);
    memcpy(&session_enc_key[16], &abRndHost1[12], 4);
    memcpy(&session_enc_key[20], &abRndCard2[12], 4);

    Desfire_InitCrypto3Des& session_enc_key[0], &session_enc_key[8], &session_enc_key[16]);

  }
  else
      if (bKeyAlgorithm == DF_ISO_CIPHER_AES)
  {
    /* AES */
    memcpy(&session_enc_key[0], &abRndHost1[0], 4);
    memcpy(&session_enc_key[4], &abRndCard2[0], 4);
    memcpy(&session_enc_key[8], &abRndHost1[12], 4);
    memcpy(&session_enc_key[12], &abRndCard2[12], 4);

    Desfire_InitCryptoAes session_enc_key);
  }

  Desfire_CleanupInitVector();

  /* Initialize the CMAC calculator */
  Desfire_InitCmac();

  return DF_OPERATION_OK;
}
#endif
    }
}
