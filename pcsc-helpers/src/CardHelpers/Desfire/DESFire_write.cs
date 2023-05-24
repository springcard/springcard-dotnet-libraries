/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 14/09/2017
 * Time: 10:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using System;

namespace SpringCard.PCSC.CardHelpers
{
    /// <summary>
    /// Description of DESFire_write.
    /// </summary>
    public partial class Desfire
    {
        /**h* DesfireAPI/Write
         *
         * NAME
         *   DesfireAPI :: Core of the writing functions
         *
         * COPYRIGHT
         *   (c) 2009 SpringCard - www.springcard.com
         *
         * DESCRIPTION
         *   Implementation of the various DESFIRE write functions.
         *
         **/

        /**f* DesfireAPI/WriteData
         *
         * NAME
         *   WriteData
         *
         * DESCRIPTION
         *   Allows to write data from Standard Data File or Backup Data File
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_WriteData(byte file_id,
         *                                     byte comm_mode,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_WriteData(SPROX_INSTANCE rInst,
         *                                     byte file_id,
         *                                     byte comm_mode,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_WriteData(SCARDHANDLE hCard,
         *                                     byte file_id,
         *                                     byte comm_mode,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         * INPUTS
         *   byte file_id      : File IDentifier
         *   byte comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
         *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
         *                       datasheet of mifare DesFire MF3ICD40 for more information)
         *   UInt32 from_offset : starting position for the write operation in bytes
         *   UInt32 size        : size of the buffer in bytes
         *   byte data[]       : buffer to write to the card
         *
         * RETURNS
         *   DF_OPERATION_OK   : success, data has been written
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   WriteData2
         *
         **/
        public long WriteData(byte file_id, byte comm_mode, UInt32 from_offset, UInt32 size, byte[] data)
        {
            return WriteDataEx(DF_WRITE_DATA, file_id, comm_mode, from_offset, size, data);
        }

        /**f* DesfireAPI/WriteData2
         *
         * NAME
         *   WriteData2
         *
         * DESCRIPTION
         *   Allows to write data from Standard Data File or Backup Data File
         *
          * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_WriteData2(byte file_id,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_WriteData2(SPROX_INSTANCE rInst,
         *                                     byte file_id,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_WriteData2(SCARDHANDLE hCard,
         *                                     byte file_id,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         * INPUTS
         *   byte file_id      : File IDentifier
         *   UInt32 from_offset : starting position for the write operation in bytes
         *   UInt32 size        : size of the buffer in bytes
         *   byte data[]       : buffer to write to the card
         *
         * RETURNS
         *   DF_OPERATION_OK   : success, data has been written
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   WriteData
         *
         **/

        public long WriteData2(byte file_id, UInt32 from_offset, UInt32 size, byte[] data)
        {
            byte comm_mode = 0;
            UInt16 access_rights = 0;
            byte write_only_access;
            byte read_write_access;
            long status;

            // we have to receive the communications mode first
            byte file_type;
            DF_FILE_SETTINGS file_settings;
            status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out file_settings);
            if (status != DF_OPERATION_OK)
                return status;

            // Depending on the access_rights field (settings w and r/w) we have to decide whether   
            // we are able to communicate in the mode indicated by comm_mode.                        
            // If ctx->auth_key does neither match w nor r/w and one of this settings                
            // contains the value "ever" (0xE) communication has to be done in plain mode regardless 
            // of the mode indicated by comm_mode.                                                  
            write_only_access = (byte)((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
            read_write_access = (byte)((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

            if ((write_only_access != session_key_id)
             && (read_write_access != session_key_id)
             && ((write_only_access == 0x0E) || (read_write_access == 0x0E)))
            {
                comm_mode = DF_COMM_MODE_PLAIN;
            }

            // Now execute the command
            return WriteData(file_id, comm_mode, from_offset, size, data);
        }

        /**f* DesfireAPI/WriteData2
         *
         * NAME
         *   WriteData2
         *
         * DESCRIPTION
         *   Allows to write data from Standard Data File or Backup Data File
         *
          * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_WriteData2(byte file_id,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_WriteData2(SPROX_INSTANCE rInst,
         *                                     byte file_id,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_WriteData2(SCARDHANDLE hCard,
         *                                     byte file_id,
         *                                     UInt32 from_offset,
         *                                     UInt32 size,
         *                                     const byte data[]);
         *
         * INPUTS
         *   byte file_id      : File IDentifier
         *   UInt32 from_offset : starting position for the write operation in bytes
         *   UInt32 size        : size of the buffer in bytes
         *   byte data[]       : buffer to write to the card
         *
         * RETURNS
         *   DF_OPERATION_OK   : success, data has been written
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   WriteData
         *
         **/

        public long WriteDataIso(byte file_id, UInt32 from_offset, UInt32 size, byte[] data)
        {
            byte comm_mode = 0;
            UInt16 access_rights = 0;
            byte write_only_access;
            byte read_write_access;
            long status;
            UInt32 size_proceed = 0;
            UInt32 offset_proceed = from_offset;
            UInt32 max_fsc = 0;
            // we have to receive the communications mode first
            byte file_type;
            DF_FILE_SETTINGS file_settings;
            status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out file_settings);
            if (status != DF_OPERATION_OK)
                return status;

            // Depending on the access_rights field (settings w and r/w) we have to decide whether   
            // we are able to communicate in the mode indicated by comm_mode.                        
            // If ctx->auth_key does neither match w nor r/w and one of this settings                
            // contains the value "ever" (0xE) communication has to be done in plain mode regardless 
            // of the mode indicated by comm_mode.                                                  
            write_only_access = (byte)((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
            read_write_access = (byte)((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

            if ((write_only_access != session_key_id)
             && (read_write_access != session_key_id)
             && ((write_only_access == 0x0E) || (read_write_access == 0x0E)))
            {
                comm_mode = DF_COMM_MODE_PLAIN;
            }

            /* PCB(1) CID(1) CLA(1) INS(1) p1(1) p2(1) Lc(1) ... Le(1) EDC(2)*/
            /* 10 for APDU header, and 7 for Write Desfire command */
            max_fsc = (MAX_FSC - 11) - 7;
            // Now execute the command
            if (size >= max_fsc)
            {
                size_proceed = size;
                while (size_proceed > 0)
                {
                    if (size_proceed > max_fsc)
                        status = WriteDataEx(DF_WRITE_DATA_ISO, file_id, comm_mode, offset_proceed, max_fsc, data);
                    else
                        status = WriteDataEx(DF_WRITE_DATA_ISO, file_id, comm_mode, offset_proceed, size_proceed, data);

                    if (status != DF_OPERATION_OK)
                        return status;

                    if (size_proceed > max_fsc)
                    {
                        size_proceed -= max_fsc;
                        offset_proceed += max_fsc;
                    }
                    else
                    {
                        size_proceed = 0;
                    }
                }
            }
            else
                status = WriteDataEx(DF_WRITE_DATA_ISO, file_id, comm_mode, from_offset, size, data);

            return status;
        }


        /* DesfireAPI/WriteDataEx
         *
         * NAME
         *   WriteDataEx
         *
         * DESCRIPTION
         *   Allows to write data from a Standard Data File, a Backup Data File, a Cyclic File or a Linear Record File
         *
         * INPUTS
         *   byte write_command : command to send, DF_WRITE_DATA or DF_WRITE_RECORD
         *   byte file_id       : ID of the file
         *   byte comm_mode     : communication mode
         *   UInt32 from_offset  : starting position for the write operation
         *   UInt32 size         : size of the buffer
         *   byte data[]        : buffer to write to the card
         *
         * RETURNS
         *   DF_OPERATION_OK    : success, data has been written
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   WriteData
         *   WriteData2
         *   WriteRecord
         *   WriteRecord2
         *
         **/
        public long WriteDataEx(byte write_command, byte file_id, byte comm_mode, UInt32 from_offset, UInt32 size, byte[] data)
        {
            long status;
            UInt32 buffer_size, max_frame_length, length, next_length, done_length = 0;
            byte comm_flags;
            UInt32 temp;

            if (isoWrapping == DF_ISO_WRAPPING_CARD)
            {
                /* PCB(1) CID(1) CLA(1) INS(1) p1(1) p2(1) Lc(1) ... Le(1) EDC(2)*/
                max_frame_length = MAX_FSC - 11;
            }
            else
            {
                /* PCB(1) CID(1) OPCODE(1) ... EDC(2)*/
                max_frame_length = MAX_FSC - 5;
            }

            buffer_size = size + 64; // TODO : confirmer la longueur

            byte[] buffer = new byte[buffer_size];

            for (int k = 0; k < buffer_size; k++) // TODO
                buffer[k] = 0;


            length = 0;
            buffer[length++] = write_command;
            buffer[length++] = file_id;

            temp = from_offset;
            buffer[length++] = (byte)(temp & 0x000000FF); temp >>= 8;
            buffer[length++] = (byte)(temp & 0x000000FF); temp >>= 8;
            buffer[length++] = (byte)(temp & 0x000000FF);

            temp = size;
            buffer[length++] = (byte)(temp & 0x000000FF); temp >>= 8;
            buffer[length++] = (byte)(temp & 0x000000FF); temp >>= 8;
            buffer[length++] = (byte)(temp & 0x000000FF);

            if (data.Length >= size)
            {
                Array.ConstrainedCopy(data, 0, buffer, (int)length, (int)size);
                length += size;
            }
            else
            {
                Array.ConstrainedCopy(data, 0, buffer, (int)length, (int)data.Length);
                length += (uint)data.Length;
            }
#if _VERBOSE
            Console.WriteLine("CipherSP80038A 1  " + BinConvert.ToHex(buffer, length));
#endif

            /* decide upon the communications mode which cryptographic */
            /* operation is to be applied on the data                  */
            if (secure_mode == SecureMode.EV2)
            {
                if (comm_mode == DF_COMM_MODE_ENCIPHERED)
                {
                    byte[] cipher = new byte[length + 16];
                    uint cipher_length = 0;

                    CipherSP80038A(buffer, 8, length, (uint)buffer.Length, ref cipher, 0, ref cipher_length);

                    Array.ConstrainedCopy(cipher, 0, buffer, 8, (int)cipher_length);
                    length = 8 + cipher_length;
#if _VERBOSE
                    Console.WriteLine("CipherSP80038A 2  " + BinConvert.ToHex(buffer, length));
#endif
                    byte[] cmac = new byte[8];
                    ComputeCmacEv2(buffer, length, false, ref cmac);

                    Array.ConstrainedCopy(cmac, 0, buffer, (int)length, 8);
                    length += 8;
#if _VERBOSE
                    Console.WriteLine("CipherSP80038A 2  " + BinConvert.ToHex(buffer, length));
#endif
                }
                else if (comm_mode == DF_COMM_MODE_MACED)
                {
                    byte[] cmac;
                    /* append the 8 bytes CMAC (computed over the whole buffer) */
                    cmac = new byte[8];
                    ComputeCmacEv2(buffer, length, false, ref cmac);
                    Array.ConstrainedCopy(cmac, 0, buffer, (int)length, 8);
                    length += 8;
                }
                else
                {
                    /* do nothing */
                }
            }
            else
            {
                if (comm_mode == DF_COMM_MODE_ENCIPHERED)
                {
                    UInt32 pos_data = length - size;
                    UInt32 len_data = size;
                    byte[] tmp = null;
                    if ((session_type & KEY_ISO_MODE) != 0)
                    {
                        /* at first we have to append the CRC bytes, computed other the whole buffer */
                        byte[] crc = new byte[4];
                        ComputeCrc32(buffer, length, ref crc);
                        Array.ConstrainedCopy(crc, 0, buffer, (int)length, 4);
                        length += 4;
                        len_data += 4;
                    }
                    else
                    {
                        /* we compute the CRC other the data only */
                        byte[] crc = new byte[2];
                        tmp = new byte[length - pos_data];
                        Array.ConstrainedCopy(buffer, (int)pos_data, tmp, 0, (int)(length - pos_data));
                        ComputeCrc16(tmp, size, ref crc);
                        Array.ConstrainedCopy(crc, 0, buffer, (int)(pos_data + len_data), 2);
                        len_data += 2;
                        length += 2;
                    }

                    /* finally do the padding and the cipher operation on the data only */

                    tmp = new byte[buffer_size - pos_data];
                    Array.ConstrainedCopy(buffer, (int)pos_data, tmp, 0, (int)(buffer_size - pos_data));
                    CipherSend(ref tmp, ref len_data, buffer_size - pos_data);
                    Array.ConstrainedCopy(tmp, 0, buffer, (int)pos_data, (int)len_data);

                    length = pos_data + len_data;

                }
                else
              if (comm_mode == DF_COMM_MODE_MACED)
                {
                    byte[] cmac;
                    if ((session_type & KEY_ISO_MODE) != 0)
                    {
                        /* append the 8 bytes CMAC (computed over the whole buffer) */
                        cmac = new byte[8];
                        ComputeCmac(buffer, length, false, ref cmac);
                        Array.ConstrainedCopy(cmac, 0, buffer, (int)length, 8);
                        length += 8;
                    }
                    else
                    {
                        /* append the 4 bytes MAC (computed over the data only) */
                        cmac = new byte[4];
                        ComputeMac(data, size, ref cmac);
                        Array.ConstrainedCopy(cmac, 0, buffer, (int)length, 4);
                        length += 4;
                    }

                }
                else
                {
                    /* if comm_mode is neither MACed nor ciphered we leave the data as it is */
                    /* this means a plain communication                                      */
                    if ((session_type & KEY_ISO_MODE) != 0)
                    {
                        /* compute the 8 bytes CMAC, but do not send it */
                        byte[] cmac = null;
                        ComputeCmac(buffer, length, false, ref cmac);
                    }
                }
            }

            /* Now our data is ready for being sent to the PICC */
            do
            {
                /* determine the limiting factor for the number of bytes to be appended to     */
                /* the ctx->xfer_buffer                                                        */
                /* this is either the maximum frame size or the number of bytes which are left */
                if ((length - done_length) > max_frame_length)
                {
                    /* transfer the calculated number of bytes  */
                    next_length = max_frame_length;
                    /* first command , we add 1 because opcode  is defined inside the APDU header */
                    if (((isoWrapping == DF_ISO_WRAPPING_READER) || (isoWrapping == DF_ISO_WRAPPING_CARD)) &&
                          (done_length == 0))
                    {
                        next_length++;
                    }

                }
                else
                {
                    /* transfer all the bytes left  */
                    next_length = length - done_length;

                }

                /* Only the first frame has its actual size, others has to be shortened by one */
                /* to put the ADDITIONAL_FRAME header                                          */
                if ((next_length == max_frame_length) && (done_length > 0))
                    next_length--;

                if ((done_length + next_length) >= length)
                {
                    if (((secure_mode == SecureMode.EV2) && (comm_mode == DF_COMM_MODE_PLAIN)) ||
                      ((secure_mode == SecureMode.EV2) && (comm_mode == DF_COMM_MODE_PLAIN2)))
                    {
                        /* This will be the last frame */
                        comm_flags = LOOSE_RESPONSE_CMAC | WANTS_OPERATION_OK;
                    }
                    else
                    {
                        /* This will be the last frame */
                        comm_flags = CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK;
                    }
                }
                else
                {
                    /* Chaining expected */
                    comm_flags = WANTS_ADDITIONAL_FRAME;
                }

                /* Now put these bytes into the ctx->xfer_buffer */
                if (done_length == 0)
                {
                    /* First frame */
                    Array.ConstrainedCopy(buffer, 0, xfer_buffer, INF + 0, (int)next_length);
                    xfer_length = (UInt16)next_length;
                }
                else
                {
                    /* Next frame */
                    xfer_buffer[INF + 0] = DF_ADDITIONAL_FRAME;
                    Array.ConstrainedCopy(buffer, (int)done_length, xfer_buffer, INF + 1, (int)next_length);
                    xfer_length = (UInt16)(1 + next_length);
                }

                status = Command(1, comm_flags);

                if (status != DF_OPERATION_OK)
                    break;

                done_length += next_length;

            } while (done_length < length);


            /* because ( eNumOfbytesExtracted = eNbytesToWrite ) is the only way for     */
            /* leaving the loop correctly an interrupted write operation is detected via */
            /* a status code different from DF_OPERATION_OK                              */

            return status;
        }


    }
}
