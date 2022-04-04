using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
  public partial class Desfire
  {

      /* DesfireAPI/ModifyValue
      *
      * NAME
      *   ModifyValue
      *
      * DESCRIPTION
      *   Allows to modify a value stored in a Value File.
      *
      * INPUTS
      *   BYTE modify_command : command to send,  DF_LIMITED_CREDIT, DF_CREDIT or DF_DEBIT
      *   BYTE file_id        : ID of the file
      *   BYTE comm_mode      : communication mode
      *   LONG amount         : amount to increase decrease to the current value stored in the file. Only positive values allowed.
      *
      * RETURNS
      *   DF_OPERATION_OK     : success, data has been written
      *   Other code if internal or communication error has occured.
      *
      * SEE ALSO
      *   LimitedCredit
      *   LimitedCredit2
      *   Credit
      *   Credit2
      *   Debit
      *   Debit2
      *
      **/
    private long ModifyValue(byte modify_command, byte file_id, byte comm_mode, long amount)
    {
      byte comm_flags = CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK;

      xfer_length = 0;

      /* Create the info block containing the command code, the file ID argument and the */
      /* modification amount argument.                                                   */
      xfer_buffer[xfer_length++] = modify_command;
      xfer_buffer[xfer_length++] = file_id;

      xfer_buffer[xfer_length++] = (byte) (amount & 0x000000FF); amount >>= 8;
      xfer_buffer[xfer_length++] = (byte) (amount & 0x000000FF); amount >>= 8;
      xfer_buffer[xfer_length++] = (byte) (amount & 0x000000FF); amount >>= 8;
      xfer_buffer[xfer_length++] = (byte) (amount & 0x000000FF);

      if (comm_mode == DF_COMM_MODE_ENCIPHERED)
      {
        if (secure_mode == SecureMode.EV2)
        {
          /* 'Encrypt' the buffer E(KSesAuth, NewKey[|| KeyVer]) */
          CipherSP80038A(xfer_buffer, 2, xfer_length, (uint)xfer_buffer.Length, ref xfer_buffer, 2, ref xfer_length);

          byte[] data = new byte[xfer_length];
          byte[] cmac = new byte[8];

          Array.ConstrainedCopy(xfer_buffer, 0, data, 0, (int)xfer_length);
          ComputeCmacEv2(data, xfer_length, false, ref cmac);

          Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
          xfer_length += 8;

          return Command(0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
        }
        else
        {
          /* Append a CRC to the amount argument. */
          XferAppendCrc(2);
          /* 'Encrypt' the amount argument bytes and the CRC bytes */
          XferCipherSend(2);
        }
      }
      else if (comm_mode == DF_COMM_MODE_MACED)
      {
        if ((session_type & KEY_ISO_MODE) == KEY_ISO_MODE)
        {
          /* Compute the CMAC, both ways */
          comm_flags |= COMPUTE_COMMAND_CMAC;
          comm_flags |= APPEND_COMMAND_CMAC;
        }
        else
        {
          byte[] cmac = new byte[8];
          byte[] tmp = new byte[4];
          Array.ConstrainedCopy(xfer_buffer, INF + 2, tmp, 0, 4);
          ComputeMac(tmp, 4, ref cmac);
          Array.ConstrainedCopy(cmac, 0, xfer_buffer, INF + 6, 4);
          xfer_length += 4;
        }
      }
      else 
      {
        if ((session_type & KEY_ISO_MODE) == KEY_ISO_MODE)
        {
          /* Check the CMAC in response */
          comm_flags |= COMPUTE_COMMAND_CMAC;
          comm_flags |= LOOSE_RESPONSE_CMAC;
        }

      }
      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, comm_flags);
    }

    /**f* DesfireAPI/GetValue
    *
    * NAME
    *   GetValue
    *
    * DESCRIPTION
    *   Allows to read current stored value from Value Files
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SWORD SPROX_Desfire_GetValue(BYTE file_id,
    *                                     BYTE comm_mode,
    *                                     LONG *value);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SWORD SPROXx_Desfire_GetValue(SPROX_INSTANCE rInst,
    *                                     BYTE file_id,
    *                                     BYTE comm_mode,
    *                                     LONG *value);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_GetValue(SCARDHANDLE hCard,
    *                                     BYTE file_id,
    *                                     BYTE comm_mode,
    *                                     LONG *value);
    *
    * INPUTS
    *   BYTE file_id      : File IDentifier
    *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
    *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
    *                       datasheet of mifare DesFire MF3ICD40 for more information)
    *   LONG *value       : pointer to receive current value
    *
    * RETURNS
    *   DF_OPERATION_OK   : success, value has been read
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   GetValue2
    *
    **/
    public long GetValue(byte file_id, byte comm_mode, ref long value)
    {
      uint t;
      long temp;
      long status;

      /* Create the info block containing the command code and the file ID argument. */
      xfer_buffer[INF + 0] = DF_GET_VALUE;
      xfer_buffer[INF + 1] = file_id;
      xfer_length = 2;

      if (secure_mode == SecureMode.EV2)
      {
        if (comm_mode == DF_COMM_MODE_ENCIPHERED)
        {
          CipherSP80038A(xfer_buffer, 2, xfer_length, (uint)xfer_buffer.Length, ref xfer_buffer, 2, ref xfer_length);

          byte[] cipher = new byte[xfer_length];
          byte[] cmac = new byte[8];

          Array.ConstrainedCopy(xfer_buffer, 0, cipher, 0, (int)xfer_length);
          ComputeCmacEv2(cipher, xfer_length, false, ref cmac);
          Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
          xfer_length += 8;

          status = Command(0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
        }
        else if (comm_mode == DF_COMM_MODE_MACED)
        {
          status = Command(5, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
          if (status != DF_OPERATION_OK)
            return status;
        }
        else 
        {
          status = Command(5, WANTS_OPERATION_OK);
          if (status != DF_OPERATION_OK)
            return status;
        }
      }
      else
      {

        if (comm_mode == DF_COMM_MODE_ENCIPHERED)
        {
          /* The communication mode is DF_COMM_MODE_ENCIPHERED.                 */
          /* GetValue returns 9 bytes in DES/3DES mode and 17 bytes in AES mode */

          if ((session_type & KEY_ISO_AES) != 0)
          {
            status = Command(17, COMPUTE_COMMAND_CMAC | WANTS_OPERATION_OK);
            t = 16;
          }
          else
          {
            status = Command(9, COMPUTE_COMMAND_CMAC | WANTS_OPERATION_OK);
            t = 8;
          }
          if (status != DF_OPERATION_OK)
            return status;

          byte[] tmp = new byte[t];
          Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
          /* Decipher the received block. */
          CipherRecv(ref tmp, ref t);
          Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);
          if ((session_type & KEY_ISO_MODE) == KEY_ISO_MODE)
          {
            /* Verify the CRC */
            if (VerifyCrc32(xfer_buffer, INF, 5, true, null) != DF_OPERATION_OK)
            {
              /* abortion due to integrity error -> wrong CRC */
              return DFCARD_WRONG_CRC;
            }
          }
          else
          {
            /* Verify the CRC */
            if (VerifyCrc16(xfer_buffer, INF + 1, 4, null) != DF_OPERATION_OK)
            {
              /* abortion due to integrity error -> wrong CRC */
              return DFCARD_WRONG_CRC;
            }
            /* Check also the padding bytes for enhanced security. */
            if ((xfer_buffer[INF + 7] != 0x00) || (xfer_buffer[INF + 8] != 0x00))
            {
              /* Error: cryptogram contains incorrect padding bytes. */
              return DFCARD_WRONG_PADDING;
            }
          }
        }
        else if ((comm_mode == DF_COMM_MODE_MACED) && ((session_type & KEY_ISO_MODE) == 0))
        {
          byte[] mac32 = new byte[4];

          /* The communication mode is DF_COMM_MODE_MACED.                      */
          /* GetValue returns 9 bytes, the status byte, the four byte value and */
          /* the four byte MAC.                                                 */
          status = Command(9, WANTS_OPERATION_OK);
          if (status != DF_OPERATION_OK)
            return status;

          /* Check the received MAC. */
          byte[] tmp = new byte[9];
          Array.ConstrainedCopy(xfer_buffer, INF + 1, tmp, 0, 9);
          ComputeMac(tmp, 4, ref mac32);

          for (int k = 0; k < 4; k++)
            if (xfer_buffer[INF + 5 + k] != mac32[k])
              return DFCARD_WRONG_MAC;
        }
        else
        {
          /* GetValue returns 5 bytes, the status byte and the four byte value. */
          status = Command(5, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
          if (status != DF_OPERATION_OK)
            return status;
        }
      }

      /* Return the requested value bytes. */
      temp = xfer_buffer[INF + 4];  temp <<= 8;
      temp += xfer_buffer[INF + 3]; temp <<= 8;
      temp += xfer_buffer[INF + 2]; temp <<= 8;
      temp += xfer_buffer[INF + 1];

      //if (value != null)
        value = temp;

      /* Success. */
      return DF_OPERATION_OK;
    }

    /**f* DesfireAPI/GetValue2
    *
    * NAME
    *   GetValue2
    *
    * DESCRIPTION
    *   Allows to read current stored value from Value Files
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SWORD SPROX_Desfire_GetValue2(BYTE file_id,
    *                                     LONG *value);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SWORD SPROXx_Desfire_GetValue2(SPROX_INSTANCE rInst,
    *                                     BYTE file_id,
    *                                     LONG *value);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_GetValue2(SCARDHANDLE hCard,
    *                                     BYTE file_id,
    *                                     LONG *value);
    *
    * INPUTS
    *   BYTE file_id      : File IDentifier
    *   LONG *value       : pointer to receive current value
    *
    * RETURNS
    *   DF_OPERATION_OK   : success, value has been read
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   GetValue
    *
    **/
    public long Desfire_GetValue2(byte file_id, ref long value)
    {
      byte comm_mode;
      UInt16 access_rights;
      byte read_only_access;
      byte write_only_access;
      byte read_write_access;
      long status;

      DF_DATA_FILE_SETTINGS dfs = new DF_DATA_FILE_SETTINGS();
      DF_FILE_SETTINGS fs = new DF_FILE_SETTINGS(dfs);
      byte file_type = 0x00;

      /* we have to receive the communications mode first */
      status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out fs);
      if (status != DF_OPERATION_OK)
        return status;

      /* GetValue is controlled by the fields r, w, r/w within the access_rights.               */
      /* Depending on the access_rights field (settings r, w and r/w) we have to decide whether */
      /* we are able to communicate in the mode indicated by comm_mode.                         */
      /* If ctx->auth_key does neither match r, w nor r/w and one of this settings              */
      /* contains the value "ever" (0xE) communication has to be done in plain mode regardless  */
      /* of the mode indicated by comm_mode.                                                    */
      read_only_access  = (byte) ((access_rights & DF_READ_ONLY_ACCESS_MASK)  >> DF_READ_ONLY_ACCESS_SHIFT);
      write_only_access = (byte) ((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
      read_write_access = (byte) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

      if ((read_only_access  != session_key_id)
        && (write_only_access != session_key_id)
        && (read_write_access != session_key_id)
        && ((read_only_access == 0x0E) || (write_only_access == 0x0E) || (read_write_access == 0x0E)))
      {
        comm_mode = DF_COMM_MODE_PLAIN;
      }

      /* Now execute the command */
      return GetValue(file_id, comm_mode, ref value);
    }

    /**f* DesfireAPI/LimitedCredit
    *
    * NAME
    *   LimitedCredit
    *
    * DESCRIPTION
    *   Allows a limited increase of a value stored in a Value File without having full Read&Write permissions to the file.
    *   This feature can be enabled or disabled during value file creation.
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SWORD SPROX_Desfire_LimitedCredit(BYTE file_id,
    *                                     BYTE comm_mode,
    *                                     LONG amount);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SWORD SPROXx_Desfire_LimitedCredit(SPROX_INSTANCE rInst,
    *                                     BYTE file_id,
    *                                     BYTE comm_mode,
    *                                     LONG amount);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_LimitedCredit(SCARDHANDLE hCard,
    *                                     BYTE file_id,
    *                                     BYTE comm_mode,
    *                                     LONG amount);
    *
    * INPUTS
    *   BYTE file_id      : File IDentifier
    *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
    *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
    *                       datasheet of mifare DesFire MF3ICD40 for more information)
    *   LONG amount       : amount to increase to the current value stored in the file. Only positive values allowed.
    *
    * RETURNS
    *   DF_OPERATION_OK   : success, data has been written
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   LimitedCredit2
    *
    **/
    public long LimitedCredit(byte file_id, byte comm_mode, long amount)
    {
      return ModifyValue(DF_LIMITED_CREDIT, file_id, comm_mode, amount);
    }

  /**f* DesfireAPI/LimitedCredit2
    *
    * NAME
    *   LimitedCredit2
    *
    * DESCRIPTION
    *   Allows a limited increase of a value stored in a Value File without having full Read&Write permissions to the file.
    *   This feature can be enabled or disabled during value file creation.
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SWORD SPROX_Desfire_LimitedCredit2(BYTE file_id,
    *                                     LONG amount);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SWORD SPROXx_Desfire_LimitedCredit2(SPROX_INSTANCE rInst,
    *                                     BYTE file_id,
    *                                     LONG amount);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_LimitedCredit2(SCARDHANDLE hCard,
    *                                     BYTE file_id,
    *                                     LONG amount);
    *
    * INPUTS
    *   BYTE file_id      : File IDentifier
    *   LONG amount       : amount to increase to the current value stored in the file. Only positive values allowed.
    *
    * RETURNS
    *   DF_OPERATION_OK   : success, data has been written
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   LimitedCredit
    *
  **/
    public long LimitedCredit2(byte file_id, long amount)
    {
      byte comm_mode;
      UInt16 access_rights;
      byte write_only_access;
      byte read_write_access;
      long status;
      DF_DATA_FILE_SETTINGS dfs = new DF_DATA_FILE_SETTINGS();
      DF_FILE_SETTINGS fs = new DF_FILE_SETTINGS(dfs);
      byte file_type = 0x00;
      /* we have to receive the communications mode first */
      status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out fs);
      if (status != DF_OPERATION_OK)
        return status;

      /* LimitedCredit is controlled by the fields w and r/w within the access_rights.         */
      /* Depending on the access_rights field (settings w and r/w) we have to decide whether   */
      /* we are able to communicate in the mode indicated by comm_mode.                        */
      /* If ctx->auth_key does neither match w nor r/w and one of this settings                */
      /* contains the value "ever" (0xE) communication has to be done in plain mode regardless */
      /* of the mode indicated by comm_mode.                                                   */
      write_only_access = (byte) ((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
      read_write_access = (byte) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

      if ((write_only_access != session_key_id)
        && (read_write_access != session_key_id)
        && ((write_only_access == 0x0E) || (read_write_access == 0x0E)))
      {
        comm_mode = DF_COMM_MODE_PLAIN;
      }

      /* Now execute the command */
      return LimitedCredit(file_id, comm_mode, amount);
    }

    /**f* DesfireAPI/Credit
      *
      * NAME
      *   Credit
      *
      * DESCRIPTION
      *   Allows to increase a value stored in a Value File.
      *
      * SYNOPSIS
      *
      *   [[sprox_desfire.dll]]
      *   SWORD SPROX_Desfire_Credit(BYTE file_id,
      *                                     BYTE comm_mode,
      *                                     LONG amount);
      *
      *   [[sprox_desfire_ex.dll]]
      *   SWORD SPROXx_Desfire_Credit(SPROX_INSTANCE rInst,
      *                                     BYTE file_id,
      *                                     BYTE comm_mode,
      *                                     LONG amount);
      *
      *   [[pcsc_desfire.dll]]
      *   LONG  SCardDesfire_Credit(SCARDHANDLE hCard,
      *                                     BYTE file_id,
      *                                     BYTE comm_mode,
      *                                     LONG amount);
      *
      * INPUTS
      *   BYTE file_id      : File IDentifier
      *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
      *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
      *                       datasheet of mifare DesFire MF3ICD40 for more information)
      *   LONG amount       : amount to increase to the current value stored in the file. Only positive values allowed.
      *
      * RETURNS
      *   DF_OPERATION_OK   : success, data has been written
      *   Other code if internal or communication error has occured.
      *
      * NOTES
      *   The Credit command requires Authentication with the key specified for "Read&Write" access.
      *
      * SEE ALSO
      *   Credit2
      *
      **/
    public long Credit(byte file_id, byte comm_mode, long amount)
    {
      return ModifyValue(DF_CREDIT, file_id, comm_mode, amount);
    }

    /**f* DesfireAPI/Credit2
      *
      * NAME
      *   Credit2
      *
      * DESCRIPTION
      *   Allows to increase a value stored in a Value File.
      *
      * SYNOPSIS
      *
      *   [[sprox_desfire.dll]]
      *   SWORD SPROX_Desfire_Credit2(BYTE file_id,
      *                                     LONG amount);
      *
      *   [[sprox_desfire_ex.dll]]
      *   SWORD SPROXx_Desfire_Credit2(SPROX_INSTANCE rInst,
      *                                     BYTE file_id,
      *                                     LONG amount);
      *
      *   [[pcsc_desfire.dll]]
      *   LONG  SCardDesfire_Credit2(SCARDHANDLE hCard,
      *                                     BYTE file_id,
      *                                     LONG amount);
      *
      * INPUTS
      *   BYTE file_id      : File IDentifier
      *   LONG amount       : amount to increase to the current value stored in the file. Only positive values allowed.
      *
      * RETURNS
      *   DF_OPERATION_OK   : success, data has been written
      *   Other code if internal or communication error has occured.
      *
      * NOTES
      *   The Credit command requires Authentication with the key specified for "Read&Write" access.
      *
      * SEE ALSO
      *   Credit
      *
      **/
    public long Credit2(byte file_id, long amount)
    {
      byte comm_mode;
      UInt16 access_rights;
      byte read_write_access;
      long status;
      DF_DATA_FILE_SETTINGS dfs = new DF_DATA_FILE_SETTINGS();
      DF_FILE_SETTINGS fs = new DF_FILE_SETTINGS(dfs);
      byte file_type = 0x00;

      /* we have to receive the communications mode first */
      status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out fs);
      if (status != DF_OPERATION_OK)
        return status;

      /* Credit is controlled by the field r/w within the access_rights.                    */
      /* Depending on the AccessRights field (setting r/w) we have to decide whether        */
      /* we are able to communicate in the mode indicated by comm_mode.                     */
      /* If ctx->auth_key does not match r/w and this setting contains the value            */
      /* "ever" (0xE), communication has to be done in plain mode regardless of the mode    */
      /* indicated by comm_mode.                                                            */
      read_write_access = (byte) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

      if ((read_write_access != session_key_id)
        && (read_write_access == 0x0E))
      {
        comm_mode = DF_COMM_MODE_PLAIN;
      }

      /* Now execute the command */
      return Credit(file_id, comm_mode, amount);
    }

    /**f* DesfireAPI/Debit
      *
      * NAME
      *   Debit
      *
      * DESCRIPTION
      *   Allows to decrease a value stored in a Value File.
      *
      * SYNOPSIS
      *
      *   [[sprox_desfire.dll]]
      *   SWORD SPROX_Desfire_Debit(BYTE file_id,
      *                                     BYTE comm_mode,
      *                                     LONG amount);
      *
      *   [[sprox_desfire_ex.dll]]
      *   SWORD SPROXx_Desfire_Debit(SPROX_INSTANCE rInst,
      *                                     BYTE file_id,
      *                                     BYTE comm_mode,
      *                                     LONG amount);
      *
      *   [[pcsc_desfire.dll]]
      *   LONG  SCardDesfire_Debit(SCARDHANDLE hCard,
      *                                     BYTE file_id,
      *                                     BYTE comm_mode,
      *                                     LONG amount);
      *
      * INPUTS
      *   BYTE file_id      : File IDentifier
      *   BYTE comm_mode    : file's communication settings (DF_COMM_MODE_PLAIN, DF_COMM_MODE_MACED,
      *                       DF_COMM_MODE_PLAIN2 or DF_COMM_MODE_ENCIPHERED)(see chapter 3.2 of
      *                       datasheet of mifare DesFire MF3ICD40 for more information)
      *   LONG amount       : amount to decrease to the current value stored in the file. Only positive values allowed.
      *
      * RETURNS
      *   DF_OPERATION_OK   : success, data has been written
      *   Other code if internal or communication error has occured.
      *
      * NOTES
      *   The Credit command requires Authentication with the key specified for "Read", "Write" ord "Read&Write" access.
      *
      * SEE ALSO
      *   Debit2
      *
      **/
    public long Debit(byte file_id, byte comm_mode, long amount)
    {
      return ModifyValue(DF_DEBIT, file_id, comm_mode, amount);
    }

    /**f* DesfireAPI/Debit2
      *
      * NAME
      *   Debit2
      *
      * DESCRIPTION
      *   Allows to decrease a value stored in a Value File.
      *
      * SYNOPSIS
      *
      *   [[sprox_desfire.dll]]
      *   SWORD SPROX_Desfire_Debit2(BYTE file_id,
      *                                     LONG amount);
      *
      *   [[sprox_desfire_ex.dll]]
      *   SWORD SPROXx_Desfire_Debit2(SPROX_INSTANCE rInst,
      *                                     BYTE file_id,
      *                                     LONG amount);
      *
      *   [[pcsc_desfire.dll]]
      *   LONG  SCardDesfire_Debit2(SCARDHANDLE hCard,
      *                                     BYTE file_id,
      *                                     LONG amount);
      *
      * INPUTS
      *   BYTE file_id      : File IDentifier
      *   LONG amount       : amount to decrease to the current value stored in the file. Only positive values allowed.
      *
      * RETURNS
      *   DF_OPERATION_OK   : success, data has been written
      *   Other code if internal or communication error has occured.
      *
      * NOTES
      *   The Credit command requires Authentication with the key specified for "Read", "Write" ord "Read&Write" access.
      *
      * SEE ALSO
      *   Debit
      *
      **/
    public long Debit2(byte file_id, long amount)
    {
      byte comm_mode;
      UInt16 access_rights;
      byte read_only_access;
      byte write_only_access;
      byte read_write_access;
      long status;

      DF_DATA_FILE_SETTINGS dfs = new DF_DATA_FILE_SETTINGS();
      DF_FILE_SETTINGS fs = new DF_FILE_SETTINGS(dfs);
      byte file_type = 0x00;


      /* we have to receive the communications mode first */
      status = GetFileSettings(file_id, out file_type, out comm_mode, out access_rights, out fs);
      if (status != DF_OPERATION_OK)
        return status;

      /* Debit is controlled by the fields r, w, r/w within the access_rights.                  */
      /* Depending on the access_rights field (settings r, w and r/w) we have to decide whether */
      /* we are able to communicate in the mode indicated by comm_mode.                         */
      /* If ctx->auth_key does neither match r, w nor r/w and one of this settings              */
      /* contains the value "ever" (0xE) communication has to be done in plain mode regardless  */
      /* of the mode indicated by comm_mode.                                                    */
      read_only_access  = (byte) ((access_rights & DF_READ_ONLY_ACCESS_MASK)  >> DF_READ_ONLY_ACCESS_SHIFT);
      write_only_access = (byte) ((access_rights & DF_WRITE_ONLY_ACCESS_MASK) >> DF_WRITE_ONLY_ACCESS_SHIFT);
      read_write_access = (byte) ((access_rights & DF_READ_WRITE_ACCESS_MASK) >> DF_READ_WRITE_ACCESS_SHIFT);

      if ((read_only_access  != session_key_id)
        && (write_only_access != session_key_id)
        && (read_write_access != session_key_id)
        && ((read_only_access == 0x0E) || (write_only_access == 0x0E) || (read_write_access == 0x0E)))
      {
        comm_mode = DF_COMM_MODE_PLAIN;
      }

      /* Now execute the command */
      return Debit(file_id, comm_mode, amount);
    }
  }
}
