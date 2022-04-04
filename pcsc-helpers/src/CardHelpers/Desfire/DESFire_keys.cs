/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 15/09/2017
 * Time: 10:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using System;

namespace SpringCard.PCSC.CardHelpers
{
  /// <summary>
  /// Description of DESFire_keys.
  /// </summary>
  public partial class Desfire
  {
    /**h* DesfireAPI/Keys
     *
     * NAME
     *   DesfireAPI :: Key management functions
     *
     * COPYRIGHT
     *   (c) 2009 SpringCard - www.springcard.com
     *
     * DESCRIPTION
     *   Implementation of management functions to change keys or
     *   key settings withing a DESFIRE application.
     *
     **/

    /**f* DesfireAPI/ChangeKeySettings
     *
     * NAME
     *   ChangeKeySettings
     *
     * DESCRIPTION
     *   Changes the key settings of the currently selected application
     *   (or of card's master key if root application is selected)
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_ChangeKeySettings (byte key_settings);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ChangeKeySettings(SPROX_INSTANCE rInst,
     *                                          byte key_settings);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ChangeKeySettings  (SCARDHANDLE hCard,
     *                                          byte key_settings);
     *
     * INPUTS
     *   byte key_settings  : new key settings (see chapter 4.3.2 of datasheet of mifare
     *                        DesFire MF3ICD40 for more information)
     *
     * RETURNS
     *   DF_OPERATION_OK    : change succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   Authenticate
     *   GetKeySettings
     *   ChangeKey
     *   GetKeyVersion
     *
     **/
    public long ChangeKeySettings(byte key_settings)
    {
      xfer_length = 0;

      /* Create the info block containing the command code */
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY_SETTINGS;
      xfer_buffer[xfer_length++] = key_settings;


      if (secure_mode == SecureMode.EV2)
      {
        /* 'Encrypt' the buffer E(KSesAuth, NewKey[|| KeyVer]) */
        CipherSP80038A(xfer_buffer, 1, xfer_length, (uint)xfer_buffer.Length, ref xfer_buffer, 1, ref xfer_length);

        byte[] data = new byte[xfer_length];
        byte[] cmac = new byte[8];

        Array.ConstrainedCopy(xfer_buffer, 0, data, 0, (int)xfer_length);
        ComputeCmacEv2(data, xfer_length, false, ref cmac);

        Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
        xfer_length += 8;

      }
      else
      {
        /* Append the CRC value corresponding to the key_settings byte */
        /* Then 'Encrypt' the bNewKeySettings byte and its CRC bytes.  */
        XferAppendCrc(1);
        XferCipherSend(1);

        /* Communicate the info block to the card and check the operation's return status. */
        
      }
      return Command(0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);

    }

    /**f* DesfireAPI/GetKeySettings
     *
     * NAME
     *   GetKeySettings
     *
     * DESCRIPTION
     *   Gets information on the DesFire card and application master key settings. In addition it returns
     *   the maximum number of keys which can be stored within the selected application.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_GetKeySettings (byte *key_settings,
     *                                       byte *key_count);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_GetKeySettings(SPROX_INSTANCE rInst,
     *                                       byte *key_settings,
     *                                       byte *key_count);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetKeySettings  (SCARDHANDLE hCard,
     *                                       byte *key_settings,
     *                                       byte *key_count);
     *
     * INPUTS
     *   byte *key_settings          : master key settings (see chapter 4.3.2 of datasheet of mifare
     *                                 DesFire MF3ICD40 for more information)
     *   byte *key_count             : maximum number of keys
     *
     * RETURNS
     *   DF_OPERATION_OK    : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   Authenticate
     *   ChangeKeySettings
     *   ChangeKey
     *   GetKeyVersion
     *
     **/
    public long GetKeySettings(ref byte key_settings, ref byte key_count)
    {
      long status;

      /* Create the info block containing the command code */
      xfer_buffer[INF + 0] = DF_GET_KEY_SETTINGS;
      xfer_length = 1;

      /* EV2 secure mode */
      if (/*(session_key_id != -1) &&*/ (secure_mode == SecureMode.EV2))
      {
        /* size is 3 or 7 if AKS defined*/
        status = Command(0 , COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      else
      {
        /* Communicate the info block to the card and check the operation's return status. */
        /* GetKeySettings returns 3 bytes, the status and two bytes of information.        */
        status = Command(3, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      if (status != DF_OPERATION_OK)
        return status;

      /* Return the requested key settings bytes. */
      key_settings = xfer_buffer[INF + 1];
      key_count = xfer_buffer[INF + 2];

      /* Success */
      return DF_OPERATION_OK;
    }
    /**f* DesfireAPI/GetKeySettings
    *
    * NAME
    *   GetKeySettings
    *
    * DESCRIPTION
    *   Gets information on the DesFire card and application master key settings (used for AKS). In addition it returns
    *   the maximum number of keys which can be stored within the selected application.
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SUInt16 SPROX_Desfire_GetKeySettings (byte *key_settings,
    *                                       byte *key_count,
    *                                       byte *AKSVersion,
    *                                       byte *NoKeySets,
    *                                       byte *MaxKeySize,
    *                                       byte *AppKeySetSett);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SUInt16 SPROXx_Desfire_GetKeySettings(SPROX_INSTANCE rInst,
    *                                       byte *key_settings,
    *                                       byte *key_count,
    *                                       byte *AKSVersion,
    *                                       byte *NoKeySets,
    *                                       byte *MaxKeySize,
    *                                       byte *AppKeySetSett);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_GetKeySettings  (SCARDHANDLE hCard,
    *                                       byte *key_settings,
    *                                       byte *key_count,
    *                                       byte *AKSVersion,
    *                                       byte *NoKeySets,
    *                                       byte *MaxKeySize,
    *                                       byte *AppKeySetSett);
    *
    * INPUTS
    *   byte *key_settings          : master key settings (see chapter 4.3.2 of datasheet of mifare
    *                                 DesFire MF3ICD40 for more information)
    *   byte *key_count             : maximum number of keys
    *   byte *AKSVersion            : AKS version
    *   byte *NoKeySets             : maximun number Of AKS
    *   byte *MaxKeySize            : maximum size for a key
    *   byte *AppKeySetSett         : 
    *
    * RETURNS
    *   DF_OPERATION_OK    : operation succeeded
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   Authenticate
    *   ChangeKeySettings
    *   ChangeKey
    *   GetKeyVersion
    *
    **/
    public long GetKeySettings(ref byte key_settings, ref byte key_count, ref byte AKSVersion, ref byte NoKeySets, ref byte MaxKeySize, ref byte AppKeySetSett)
    {
      long status;

      /* Create the info block containing the command code */
      xfer_buffer[INF + 0] = DF_GET_KEY_SETTINGS;
      xfer_length = 1;

      /* EV2 secure mode */
      if (/*(session_key_id != -1) &&*/ (secure_mode == SecureMode.EV2))
      {
        /* size is 3 or 7 if AKS defined*/
        status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      else
      {
        /* Communicate the info block to the card and check the operation's return status. */
        /* GetKeySettings returns 3 bytes, the status and two bytes of information.        */
        status = Command(3, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      if (status != DF_OPERATION_OK)
        return status;

      /* Return the requested key settings bytes. */
      key_settings = xfer_buffer[INF + 1];
      key_count = xfer_buffer[INF + 2];
      AKSVersion = xfer_buffer[INF + 3];
      NoKeySets = xfer_buffer[INF + 4];
      MaxKeySize = xfer_buffer[INF + 5];
      AppKeySetSett = xfer_buffer[INF + 6];
      /* Success */
      return DF_OPERATION_OK;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key_set_number"></param>
    /// <param name="key_number"></param>
    /// <param name="new_key"></param>
    /// <param name="old_key"></param>
    /// <returns></returns>
    private long ChangeKey_Ex(byte key_set_number, byte key_number, byte[] new_key, byte[] old_key)
    {
      uint header_size = 2;
      byte b;
      byte[] buffer = new byte[24];

      if ((current_aid != 0x000000) && ((key_number & DF_APPLSETTING2_AES) != 0)) /* Update 27/07/2011 to check the current AID (patch submitted by Evaldas Auryla) */
        return DFCARD_LIB_CALL_ERROR;

      if ((session_type == KEY_ISO_DES) || (session_type == KEY_ISO_3DES2K) || (session_type == KEY_ISO_3DES3K) || (session_type == KEY_ISO_AES))
      {
        byte[] new_key_24 = new byte[24];
        byte[] old_key_24 = new byte[24];

        if (new_key != null)
        {
          Array.ConstrainedCopy(new_key, 0, new_key_24, 0, 16);
          Array.ConstrainedCopy(new_key, 0, new_key_24, 16, 8);
          new_key = new_key_24;
        }

        if (old_key != null)
        {
          Array.ConstrainedCopy(old_key, 0, old_key_24, 0, 16);
          Array.ConstrainedCopy(old_key, 0, old_key_24, 16, 8);
          old_key = old_key_24;
        }
        if (key_set_number == 0xFF)
          return ChangeKey24(key_number, new_key, old_key);
        else
          return ChangeKey24(key_set_number, key_number, new_key, old_key);
      }

      for (int k = 0; k < buffer.Length; k++)
        buffer[k] = 0;

      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      if (key_set_number == 0xFF)
      {
        xfer_buffer[xfer_length++] = DF_CHANGE_KEY;
        xfer_buffer[xfer_length++] = key_number;
      }
      else
      {
        xfer_buffer[xfer_length++] = DF_CHANGE_KEY_EV2;
        xfer_buffer[xfer_length++] = key_set_number;
        xfer_buffer[xfer_length++] = key_number;
      }
      header_size = xfer_length;

      byte[] crc16 = new byte[2];
      if (old_key != null)
      {
        /* If a 'previous key' has been passed, format the 24 byte cryptogram according to the     */
        /* three key procedure (new key, previous key, all encrypted with the current session key) */

        /* Take the new key into the buffer. */
        Array.ConstrainedCopy(new_key, 0, buffer, 0, 16);

        /* XOR the previous key to the new key. */
        for (b = 0; b < 16; b++)
          buffer[b] ^= old_key[b];

        /* Append a first CRC, computed over the XORed key combination. */
        ComputeCrc16(buffer, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 16, 2);

        /* Append a second CRC, computed over the new key only. */
        ComputeCrc16(new_key, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 18, 2);

      }
      else
      {
        /* If no 'previous key' has been passed, format the 24 byte cryptogram according to the */
        /* two key procedure (new key, encrypted with the current session key).                 */

        /* Take the new key into the buffer. */
        for (int k = 0; k < 16; k++)
          buffer[k] = new_key[k];

        /* Append the CRC computed over the new key. */
        ComputeCrc16(new_key, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 16, 2);

      }

      /* Append the 24 byte buffer to the command string. */
      Array.ConstrainedCopy(buffer, 0, xfer_buffer, (int)xfer_length, 24);
      xfer_length += 24;

      /* 'Encrypt' the 24 bytes */
      XferCipherSend(header_size);

      /* Forget the current authentication state if we're changing the current key */
      if ((key_number & 0x3F) == (session_key_id & 0x3F))
        CleanupAuthentication();

      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, WANTS_OPERATION_OK);
    }

    /**f* DesfireAPI/ChangeKey
    *
    * NAME
    *   ChangeKey
    *
    * DESCRIPTION
    *   Change a DES, or 3DES2K key in the selected Desfire application.
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SUInt16 SPROX_Desfire_ChangeKey(byte key_number,
    *                                 const byte new_key[16],
    *                                 const byte old_key[16]);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SUInt16 SPROXx_Desfire_ChangeKey(SPROX_INSTANCE rInst,
    *                                  byte key_number,
    *                                  const byte new_key[16],
    *                                  const byte old_key[16]);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_ChangeKey(SCARDHANDLE hCard,
    *                                byte key_number,
    *                                const byte new_key[16],
    *                                const byte old_key[16]);
    *
    * INPUTS
    *   byte key_number        : number of the key (KeyNo)
    *   const byte new_key[16] : 16-byte New Key (DES/3DES keys)
    *   const byte old_key[16] : 16-byte Old Key (DES/3DES keys)
    *
    * RETURNS
    *   DF_OPERATION_OK    : change succeeded
    *   Other code if internal or communication error has occured.
    *
    * NOTES
    *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
    *   * If the 2nd half of the key string is equal to the 1st half, the key is
    *   handled as a single DES key by the DesFire card.
    *   * If the 2nd half of the key string is NOT equal to the 1st half, the key
    *   is handled as a 3DES key.
    *
    *   After a successful change of the key used to reach the current authentication status, this
    *   authentication is invalidated, an authentication with the new key is necessary for subsequent
    *   operations.
    *
    *   If authentication has been performed before calling ChangeKey with the old key,
    *   use null instead of old_key.
    *
    * SEE ALSO
    *   ChangeKey24
    *   ChangeKeyAes
    *   Authenticate
    *   ChangeKeySettings
    *   GetKeySettings
    *   GetKeyVersion
    *
    **/
    public long ChangeKey(byte key_number, byte[] new_key, byte[] old_key)
    {
      return ChangeKey_Ex(0xFF, key_number, new_key, old_key);
#if _0
      byte b;
      byte[] buffer = new byte[24];

      if ((current_aid != 0x000000) && ((key_number & DF_APPLSETTING2_AES) != 0)) /* Update 27/07/2011 to check the current AID (patch submitted by Evaldas Auryla) */
        return DFCARD_LIB_CALL_ERROR;

      if ((session_type == KEY_ISO_DES) || (session_type == KEY_ISO_3DES2K) || (session_type == KEY_ISO_3DES3K) || (session_type == KEY_ISO_AES))
      {
        byte[] new_key_24 = new byte[24];
        byte[] old_key_24 = new byte[24];

        if (new_key != null)
        {
          Array.ConstrainedCopy(new_key, 0, new_key_24, 0, 16);
          Array.ConstrainedCopy(new_key, 0, new_key_24, 16, 8);
          new_key = new_key_24;
        }

        if (old_key != null)
        {
          Array.ConstrainedCopy(old_key, 0, old_key_24, 0, 16);
          Array.ConstrainedCopy(old_key, 0, old_key_24, 16, 8);
          old_key = old_key_24;
        }
        return ChangeKey24(key_number, new_key, old_key);
      }

      for (int k = 0; k < buffer.Length; k++)
        buffer[k] = 0;

      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY;
      xfer_buffer[xfer_length++] = key_number;

      byte[] crc16 = new byte[2];
      if (old_key != null)
      {
        /* If a 'previous key' has been passed, format the 24 byte cryptogram according to the     */
        /* three key procedure (new key, previous key, all encrypted with the current session key) */

        /* Take the new key into the buffer. */
        Array.ConstrainedCopy(new_key, 0, buffer, 0, 16);

        /* XOR the previous key to the new key. */
        for (b = 0; b < 16; b++)
          buffer[b] ^= old_key[b];

        /* Append a first CRC, computed over the XORed key combination. */
        ComputeCrc16(buffer, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 16, 2);

        /* Append a second CRC, computed over the new key only. */
        ComputeCrc16(new_key, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 18, 2);

      }
      else
      {
        /* If no 'previous key' has been passed, format the 24 byte cryptogram according to the */
        /* two key procedure (new key, encrypted with the current session key).                 */

        /* Take the new key into the buffer. */
        for (int k = 0; k < 16; k++)
          buffer[k] = new_key[k];

        /* Append the CRC computed over the new key. */
        ComputeCrc16(new_key, 16, ref crc16);
        Array.ConstrainedCopy(crc16, 0, buffer, 16, 2);

      }

      /* Append the 24 byte buffer to the command string. */
      Array.ConstrainedCopy(buffer, 0, xfer_buffer, (int)xfer_length, 24);
      xfer_length += 24;

      /* 'Encrypt' the 24 bytes */
      XferCipherSend(2);

      /* Forget the current authentication state if we're changing the current key */
      if ((key_number & 0x3F) == (session_key_id & 0x3F))
        CleanupAuthentication();

      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, WANTS_OPERATION_OK);
#endif
    }

    /**f* DesfireAPI/Desfire_ChangeKeyEv2
     *
     * NAME
     *   ChangeKey
     *
     * DESCRIPTION
     *   Change a DES, or 3DES2K key in the selected Desfire application.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_ChangeKeyEv2(byte key_number,
     *                                 const byte new_key[16],
     *                                 const byte old_key[16]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ChangeKeyEv2(SPROX_INSTANCE rInst,
     *                                  byte key_number,
     *                                  const byte new_key[16],
     *                                  const byte old_key[16]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ChangeKeyEv2(SCARDHANDLE hCard,
     *                                byte key_number,
     *                                const byte new_key[16],
     *                                const byte old_key[16]);
     *
     * INPUTS
     *   byte key_number        : number of the key (KeyNo)
     *   const byte new_key[16] : 16-byte New Key (DES/3DES keys)
     *   const byte old_key[16] : 16-byte Old Key (DES/3DES keys)
     *
     * RETURNS
     *   DF_OPERATION_OK    : change succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
     *   * If the 2nd half of the key string is equal to the 1st half, the key is
     *   handled as a single DES key by the DesFire card.
     *   * If the 2nd half of the key string is NOT equal to the 1st half, the key
     *   is handled as a 3DES key.
     *
     *   After a successful change of the key used to reach the current authentication status, this
     *   authentication is invalidated, an authentication with the new key is necessary for subsequent
     *   operations.
     *
     *   If authentication has been performed before calling ChangeKey with the old key,
     *   use null instead of old_key.
     *
     * SEE ALSO
     *   ChangeKey24
     *   ChangeKeyAes
     *   Authenticate
     *   ChangeKeySettings
     *   GetKeySettings
     *   GetKeyVersion
     *
     **/
    public long ChangeKey(byte key_set_number, byte key_number, byte[] new_key, byte[] old_key)
    {
      return ChangeKey_Ex(key_set_number, key_number, new_key, old_key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key_number"></param>
    /// <param name="new_key"></param>
    /// <param name="old_key"></param>
    /// <param name="header_size"></param>
    /// <returns></returns>
    private long ChangeKey24_Ex(byte key_number, byte[] new_key, byte[] old_key, byte key_size, uint header_size = 2)
    {
      byte b = 0x00;
      /* Take the new key into the buffer. */
      Array.ConstrainedCopy(new_key, 0, xfer_buffer, (int)xfer_length, key_size);

      xfer_length += key_size;

      /*
      {
        Console.Write("xfer_buffer: ");
        for (int k=0; k<xfer_length; k++)
          Console.Write(String.Format("{0:x02}", xfer_buffer[k]));
        Console.Write("\n");
      }
      */

      byte[] crc32 = new byte[4];
      if (old_key != null)
      {
        /* If a 'previous key' has been passed, format the 40 byte cryptogram according to the     */
        /* three key procedure (new key, previous key, all encrypted with the current session key) */

        /* XOR the previous key with the new key. */
        for (b = 0; b < key_size; b++)
          xfer_buffer[xfer_length - key_size + b] ^= old_key[b];

        /* Append a first CRC, computed over the XORed key combination and the header */
        ComputeCrc32(xfer_buffer, xfer_length, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int)xfer_length, 4);
        xfer_length += 4;

        /* Append a second CRC, computed over the new key only. */
        ComputeCrc32(new_key, key_size, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int)xfer_length, 4);

        xfer_length += 4;

      }
      else
      {
        /* If no 'previous key' has been passed, format the 32 byte cryptogram according to the */
        /* two key procedure (new key, encrypted with the current session key).                 */

        /* Append the CRC computed over the new key and the header */
        ComputeCrc32(xfer_buffer, xfer_length, ref crc32);
        Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int)xfer_length, 4);
        xfer_length += 4;
      }

      /* 'Encrypt' the buffer */
      XferCipherSend(header_size);

      /* Forget the current authentication state if we're changing the current key */
      if ((key_number & 0x3F) == (session_key_id & 0x3F))
        CleanupAuthentication();

      /* Communicate the info block to the card and check the operation's return status. */
      return Command(0, LOOSE_RESPONSE_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
    }
    /**f* DesfireAPI/ChangeKey24
     *
     * NAME
     *   ChangeKey24
     *
     * DESCRIPTION
     *   Change a 3DES3K key in the selected Desfire application.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_ChangeKey24(byte key_number,
     *                                   const byte new_key[24],
     *                                   const byte old_key[24]);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_ChangeKey24(SPROX_INSTANCE rInst,
     *                                    byte key_number,
     *                                    const byte new_key[24],
     *                                    const byte old_key[24]);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_ChangeKey24(SCARDHANDLE hCard,
     *                                  byte key_number,
     *                                  const byte new_key[24],
     *                                  const byte old_key[24]);
     *
     * INPUTS
     *   byte key_number        : number of the key (KeyNo)
     *   const byte new_key[24] : 24-byte New Key (3DES keys)
     *   const byte old_key[24] : 24-byte Old Key (3DES keys)
     *
     * RETURNS
     *   DF_OPERATION_OK    : change succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ChangeKey16
     *   ChangeKeyAes
     *   Authenticate
     *   ChangeKeySettings
     *   GetKeySettings
     *   GetKeyVersion
     *
     **/
    public long ChangeKey24(byte key_number, byte[] new_key, byte[] old_key)
    {
      byte key_size;
      //byte b;

      if ((key_number & DF_APPLSETTING2_AES) != 0)
        return DFCARD_LIB_CALL_ERROR;

      if ((key_number & DF_APPLSETTING2_3DES3K) != 0)
      {
        key_size = 24;
      }
      else
      {
        switch (session_type)
        {
          case KEY_ISO_AES:
          case KEY_ISO_DES:
          case KEY_ISO_3DES2K: key_size = 16; break;

          case KEY_ISO_3DES3K: key_size = 24; break;

          default: return DFCARD_LIB_CALL_ERROR;
        }
      }

      for (int k = 0; k < xfer_buffer.Length; k++)
        xfer_buffer[k] = 0;

      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY;
      xfer_buffer[xfer_length++] = key_number;

      return ChangeKey24_Ex( key_number, new_key, old_key, key_size, 2);      
    }
    /**f* DesfireAPI/Desfire_ChangeKey24
    *
    * NAME
    *   ChangeKey24
    *
    * DESCRIPTION
    *   Change a 3DES3K key in the selected Desfire application.
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SUInt16 SPROX_Desfire_ChangeKeyEv224(byte key_number,
    *                                   const byte new_key[24],
    *                                   const byte old_key[24]);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SUInt16 SPROXx_Desfire_ChangeKeyEv224(SPROX_INSTANCE rInst,
    *                                    byte key_number,
    *                                    const byte new_key[24],
    *                                    const byte old_key[24]);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_ChangeKeyEv224(SCARDHANDLE hCard,
    *                                  byte key_number,
    *                                  const byte new_key[24],
    *                                  const byte old_key[24]);
    *
    * INPUTS
    *   byte key_number        : number of the key (KeyNo)
    *   const byte new_key[24] : 24-byte New Key (3DES keys)
    *   const byte old_key[24] : 24-byte Old Key (3DES keys)
    *
    * RETURNS
    *   DF_OPERATION_OK    : change succeeded
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   ChangeKey16
    *   ChangeKeyAes
    *   Authenticate
    *   ChangeKeySettings
    *   GetKeySettings
    *   GetKeyVersion
    *
    **/
    public long ChangeKey24(byte key_set_number, byte key_number, byte[] new_key, byte[] old_key)
    {
      byte key_size;
      //byte b;

      if ((key_number & DF_APPLSETTING2_AES) != 0)
        return DFCARD_LIB_CALL_ERROR;

      if ((key_number & DF_APPLSETTING2_3DES3K) != 0)
      {
        key_size = 24;

      }
      else
      {
        switch (session_type)
        {
          case KEY_ISO_AES:
          case KEY_ISO_DES:
          case KEY_ISO_3DES2K: key_size = 16; break;

          case KEY_ISO_3DES3K: key_size = 24; break;

          default: return DFCARD_LIB_CALL_ERROR;
        }
      }

      for (int k = 0; k < xfer_buffer.Length; k++)
        xfer_buffer[k] = 0;

      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY_EV2;
      xfer_buffer[xfer_length++] = key_set_number;
      xfer_buffer[xfer_length++] = key_number;

      return ChangeKey24_Ex(key_number, new_key, old_key, key_size, 3);
    }

    /// <summary>
    /// ChangeKeyAes_Ex
    /// </summary>
    /// <param name="key_number"></param>
    /// <param name="key_version"></param>
    /// <param name="new_key"></param>
    /// <param name="old_key"></param>
    /// <returns></returns>
    public long ChangeKeyAes_Ex(byte key_number, byte key_version, byte[] new_key, byte[] old_key, int header_size = 2, bool bexcept = false)
    {
      byte b;

#if _VERBOSE
      Console.WriteLine("ChangeKeyAes_Ex xfer_length {0} {1}", key_number, key_version);
#endif

      /* Take the new key into the buffer. */
      Array.ConstrainedCopy(new_key, 0, xfer_buffer, (int)xfer_length, 16);
      xfer_length += 16;

      if (secure_mode == SecureMode.EV2)
      {
        /*Targeted key equal to authenticated key*/
        
        /* except KeySetNo is different from zero, 
        * always the case where targeted key is different 
        * from authenticated key needs to be applied
        */
        
        if ( ((key_number&0x3F) == session_key_id) && (bexcept == false) )
        {
          /* The key version goes here */
          xfer_buffer[xfer_length++] = key_version;

#if _VERBOSE
          Console.WriteLine("ChangeKeyAes_Ex before CipherSP80038A {0}", xfer_length);
          Console.WriteLine("ChangeKeyAes_Ex before CipherSP80038A " + BinConvert.ToHex(xfer_buffer, xfer_length));
#endif
          /*
           * Cryptogram = E(KSesAuth, NewKey[|| KeyVer]) ||
           * MACt(KSesAuthMAC, Cmd || CmdCtr || TI || KeyNo || E(KSesAuthENC,NewKey[|| KeyVer]))
           */

          /* 'Encrypt' the buffer E(KSesAuth, NewKey[|| KeyVer]) */
          CipherSP80038A(xfer_buffer, header_size, xfer_length, (uint)xfer_buffer.Length, ref xfer_buffer, header_size, ref xfer_length);

          byte[] data = new byte[xfer_length];
          byte[] cmac = new byte[8];

#if _VERBOSE
          Console.WriteLine("ChangeKeyAes_Ex after CipherSP80038A {0}", xfer_length);
          Console.WriteLine("ChangeKeyAes_Ex after CipherSP80038A " + BinConvert.ToHex(xfer_buffer, xfer_length));
#endif
          Array.ConstrainedCopy(xfer_buffer, 0, data, 0, (int)xfer_length);
          ComputeCmacEv2(data, xfer_length, false, ref cmac);

#if _VERBOSE
          Console.WriteLine("ChangeKeyAes_Ex + CMAC " + BinConvert.ToHex(cmac, 8));
#endif

          Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
          xfer_length += 8;

#if _VERBOSE
          Console.WriteLine("ChangeKeyAes_Ex end {0}", xfer_length);
          Console.WriteLine("ChangeKeyAes_Ex end " + BinConvert.ToHex(xfer_buffer, xfer_length));
#endif
        }
        /*Targeted key different from authenticated key*/
        else
        {
          /* Cryptogram = E(KSesAuthENC, (NewKey⊕OldKey)[|| KeyVer] || CRC32NK)||
           * MACt(KSesAuthMAC, Cmd || CmdCtr || TI || KeyNo || E(KSesAuthENC,
           * (NewKey⊕OldKey)[|| KeyVer] || CRC32NK))
           */
          if (old_key == null)
          {
            return DFCARD_LIB_CALL_ERROR;
          }
          /* If a 'previous key' has been passed, format the 40 byte cryptogram according to the     */
          /* three key procedure (new key, previous key, all encrypted with the current session key) */

          /* XOR the previous key with the new key. */
          for (b = 0; b < 16; b++)
            xfer_buffer[xfer_length - 16 + b] ^= old_key[b];

          /* The key version goes here */
          xfer_buffer[xfer_length++] = key_version;

#if _VERBOSE
          Console.WriteLine("ChangeKeyAes_Ex xfer_length {0}", xfer_length);
          Console.WriteLine("ChangeKeyAes_Ex xfer_buffer " + BinConvert.ToHex(xfer_buffer, xfer_length));
#endif

          /* Append a first CRC, computed over the XORed key combination and the header */
          byte[] crc32 = new byte[4];

          ComputeCrc32(new_key, 16, ref crc32);
          Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int)xfer_length, 4);
          xfer_length += 4;

#if _VERBOSE
          Console.WriteLine("ChangeKeyAes_Ex xfer_length {0}", xfer_length);
          Console.WriteLine("ChangeKeyAes_Ex xfer_buffer " + BinConvert.ToHex(xfer_buffer, xfer_length));
#endif

          /* 'Encrypt' the buffer E(KSesAuthENC, (NewKey⊕OldKey)[|| KeyVer] || CRC32NK) */
          CipherSP80038A(xfer_buffer, header_size, xfer_length, (uint)xfer_buffer.Length, ref xfer_buffer, header_size, ref xfer_length);

          byte[] data = new byte[xfer_length];
          byte[] cmac = new byte[8];

          Array.ConstrainedCopy(xfer_buffer, 0, data, 0, (int)xfer_length);
          ComputeCmacEv2(data, xfer_length, false, ref cmac);

#if _VERBOSE
          Console.WriteLine("ChangeKeyAes_Ex xfer_length {0}", xfer_length);
          Console.WriteLine("ChangeKeyAes_Ex data " + BinConvert.ToHex(data, xfer_length));
#endif

          Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
          xfer_length += 8;
#if _VERBOSE
          Console.WriteLine("ChangeKeyAes_Ex xfer_length {0}", xfer_length);
          Console.WriteLine("ChangeKeyAes_Ex xfer_buffer " + BinConvert.ToHex(xfer_buffer, xfer_length));
#endif
        }

        /* Forget the current authentication state if we're changing the current key */
        if ((key_number & 0x3F) == (session_key_id & 0x3F) && (bexcept == false))
          CleanupAuthentication();

        return Command(0, WANTS_OPERATION_OK);
      }
      else
      {
        byte[] crc32 = new byte[4];
        if (old_key != null)
        {
          /* If a 'previous key' has been passed, format the 40 byte cryptogram according to the     */
          /* three key procedure (new key, previous key, all encrypted with the current session key) */

          /* XOR the previous key with the new key. */
          for (b = 0; b < 16; b++)
            xfer_buffer[xfer_length - 16 + b] ^= old_key[b];

          /* The key version goes here */
          xfer_buffer[xfer_length++] = key_version;

          /* Append a first CRC, computed over the XORed key combination and the header */

          ComputeCrc32(xfer_buffer, xfer_length, ref crc32);
          Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int)xfer_length, 4);

          xfer_length += 4;

          /* Append a second CRC, computed over the new key only. */
          ComputeCrc32(new_key, 16, ref crc32);
          Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int)xfer_length, 4);
          xfer_length += 4;

        }
        else
        {
          /* If no 'previous key' has been passed, format the 32 byte cryptogram according to the */
          /* two key procedure (new key, encrypted with the current session key).                 */

          /* The key version goes here */
          xfer_buffer[xfer_length++] = key_version;

          /* Append the CRC computed over the new key and the header */
          ComputeCrc32(xfer_buffer, xfer_length, ref crc32);
          Array.ConstrainedCopy(crc32, 0, xfer_buffer, (int)xfer_length, 4);
          xfer_length += 4;
        }

        /* 'Encrypt' the buffer */
        XferCipherSend((uint)header_size);

        /* Forget the current authentication state if we're changing the current key */
        if ((key_number & 0x3F) == (session_key_id & 0x3F))
          CleanupAuthentication();

        /* Communicate the info block to the card and check the operation's return status. */
        return Command(0, LOOSE_RESPONSE_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
    }

    /**f* DesfireAPI/ChangeKeyAes
    *
    * NAME
    *   ChangeKeyAes
    *
    * DESCRIPTION
    *   Change an AES key in the selected Desfire application.
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SUInt16 SPROX_Desfire_ChangeKeyAes(byte key_number,
    *                                    byte key_version,
    *                                    const byte new_key[16],
    *                                    const byte old_key[16]);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SUInt16 SPROXx_Desfire_ChangeKeyAes(SPROX_INSTANCE rInst,
    *                                     byte key_number,
    *                                     byte key_version,
    *                                     const byte new_key[16],
    *                                     const byte old_key[16]);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_ChangeKeyAes(SCARDHANDLE hCard,
    *                                   byte key_number,
    *                                   byte key_version,
    *                                   const byte new_key[16],
    *                                   const byte old_key[16]);
    *
    * INPUTS
    *   byte key_number        : number of the key (KeyNo)
    *   byte key_version       : version number to be stored together with the key
    *   const byte new_key[16] : 16-byte New Key (AES key)
    *   const byte old_key[16] : 16-byte Old Key (AES key)
    *
    * RETURNS
    *   DF_OPERATION_OK    : change succeeded
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   ChangeKey
    *   ChangeKey24
    *   Authenticate
    *   ChangeKeySettings
    *   GetKeySettings
    *   GetKeyVersion
    *
    **/
    public long ChangeKeyAes(byte key_number, byte key_version, byte[] new_key, byte[] old_key)
    {

#if _VERBOSE
      Console.WriteLine("ChangeKeyAes xfer_length {0} {1}", key_number, key_version);
#endif

      for (int k = 0; k < xfer_buffer.Length; k++)
        xfer_buffer[k] = 0;

      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY;
      xfer_buffer[xfer_length++] = (byte)(key_number);

      return ChangeKeyAes_Ex(key_number, key_version, new_key, old_key, 2);
    }

    /**f* DesfireAPI/Desfire_ChangeKeyEv2Aes
    *
    * NAME
    *   ChangeKeyAes
    *
    * DESCRIPTION
    *   Change an AES key in the selected Desfire application.
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SUInt16 SPROX_Desfire_ChangeKeyEv2Aes(byte key_number,
    *                                    byte key_version,
    *                                    const byte new_key[16],
    *                                    const byte old_key[16]);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SUInt16 SPROXx_Desfire_ChangeKeyEv2Aes(SPROX_INSTANCE rInst,
    *                                     byte key_number,
    *                                     byte key_version,
    *                                     const byte new_key[16],
    *                                     const byte old_key[16]);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_ChangeKeyEv2Aes(SCARDHANDLE hCard,
    *                                   byte key_number,
    *                                   byte key_version,
    *                                   const byte new_key[16],
    *                                   const byte old_key[16]);
    *
    * INPUTS
    *   byte key_number        : number of the key (KeyNo)
    *   byte key_version       : version number to be stored together with the key
    *   const byte new_key[16] : 16-byte New Key (AES key)
    *   const byte old_key[16] : 16-byte Old Key (AES key)
    *
    * RETURNS
    *   DF_OPERATION_OK    : change succeeded
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   ChangeKey
    *   ChangeKey24
    *   Authenticate
    *   ChangeKeySettings
    *   GetKeySettings
    *   GetKeyVersion
    *
    **/
    public long ChangeKeyAes(byte key_set_number, byte key_number, byte key_version, byte[] new_key, byte[] old_key)
    {

      for (int k = 0; k < xfer_buffer.Length; k++)
        xfer_buffer[k] = 0;


      /* Begin the info block with the command code and the number of the key to be changed. */
      xfer_length = 0;
      xfer_buffer[xfer_length++] = DF_CHANGE_KEY_EV2;
      xfer_buffer[xfer_length++] = key_set_number;
      xfer_buffer[xfer_length++] = key_number;

      /* except KeySetNo is different from zero, 
        * always the case where targeted key is different 
        * from authenticated key needs to be applied
        */
      if (key_set_number != 0x00)
        return ChangeKeyAes_Ex(key_number, key_version, new_key, old_key, 3, true);
      else
        return ChangeKeyAes_Ex(key_number, key_version, new_key, old_key, 3, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bKeyNumber"></param>
    /// <param name="bKeySetNumber"></param>
    /// <param name="pbKeyVersion"></param>
    /// <returns></returns>
    private long GetKeyVersion_Ex(ushort header_size, ref byte[] pbKeyVersion)
    {
      long status;

      /* EV2 secure mode */
      if (secure_mode == SecureMode.EV2)
      {
        status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      else
      {
        status = Command(header_size, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      
      if (status != DF_OPERATION_OK)
        return status;

      if ( xfer_length > 1 )
      {
        /* Get the key version byte. */
        if (pbKeyVersion.Length >= (xfer_length - 1))
          Array.Copy(xfer_buffer, INF + 1, pbKeyVersion, 0, (xfer_length - 1));
        else
          Array.Copy(xfer_buffer, INF + 1, pbKeyVersion, 0, pbKeyVersion.Length);
      }
      else
      {
        return DF_PARAMETER_ERROR;
      }

      /* Success. */
      return DF_OPERATION_OK;
    }

    /**f* DesfireAPI/GetKeyVersion
     *
     * NAME
     *   GetKeyVersion
     *
     * DESCRIPTION
     *   Reads out the current key version of any key stored on the PICC
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_GetKeyVersion(byte bKeyNumber,
     *                                     byte *pbKeyVersion);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_GetKeyVersion(SPROX_INSTANCE rInst,
     *                                      byte bKeyNumber,
     *                                      byte *pbKeyVersion);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetKeyVersion(SCARDHANDLE hCard,
     *                                    byte bKeyNumber,
     *                                    byte *pbKeyVersion);
     *
     * INPUTS
     *   byte bKeyNumber             : number of the key (KeyNo)
     *   byte *pbKeyVersion          : current version of the specified key
     *
     * RETURNS
     *   DF_OPERATION_OK    : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   This command can be issued without valid authentication.
     *
     * SEE ALSO
     *   Authenticate
     *   GetKeySettings
     *   ChangeKeySettings
     *   ChangeKey
     *
     **/
    public long GetKeyVersion(byte bKeyNumber, ref byte pbKeyVersion)
    {
      long status;
      byte[] localKeyVersion = new byte[16];
      /* Create the info block containing the command code and the key number argument. */
      xfer_buffer[INF + 0] = DF_GET_KEY_VERSION;
      xfer_buffer[INF + 1] = bKeyNumber;
      xfer_length = 2;

      status = GetKeyVersion_Ex ( 2, ref localKeyVersion);

      if (status != DF_OPERATION_OK)
        return status;

      /* Get the key version byte. */
      pbKeyVersion = localKeyVersion[0];

      /* Success. */
      return DF_OPERATION_OK;      
    }

    /**f* DesfireAPI/GetKeyVersionEv2
     *
     * NAME
     *   GetKeyVersionEv2
     *
     * DESCRIPTION
     *   Reads out the current key version of any key stored on the PICC
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SUInt16 SPROX_Desfire_GetKeyVersionEv2(byte bKeyNumber,
     *                                      byte bKeySetNumber,
     *                                     byte *pbKeyVersion);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SUInt16 SPROXx_Desfire_GetKeyVersionEv2(SPROX_INSTANCE rInst,
     *                                      byte bKeyNumber,
     *                                      byte bKeySetNumber,
     *                                      byte *pbKeyVersion);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_GetKeyVersionEv2(SCARDHANDLE hCard,
     *                                    byte bKeyNumber,
     *                                    byte bKeySetNumber,
     *                                    byte *pbKeyVersion);
     *
     * INPUTS
     *   byte bKeyNumber             : number of the key (KeyNo)
     *   byte *pbKeyVersion          : current version of the specified key
     *
     * RETURNS
     *   DF_OPERATION_OK    : operation succeeded
     *   Other code if internal or communication error has occured.
     *
     * NOTES
     *   This command can be issued without valid authentication.
     *
     * SEE ALSO
     *   Authenticate
     *   GetKeySettings
     *   ChangeKeySettings
     *   ChangeKey
     *
     **/
    public long GetKeyVersion(byte bKeyNumber, byte bKeySetNumber, ref byte[] pbKeyVersion)
    {
      /* Create the info block containing the command code and the key number argument. */
      xfer_buffer[INF + 0] = DF_GET_KEY_VERSION;
      xfer_buffer[INF + 1] = 0x40; // (byte) (bKeyNumber | 0x40);
      xfer_buffer[INF + 2] = 0x80; // (byte) (bKeySetNumber | 0x80);
      xfer_length = 3; 

      return GetKeyVersion_Ex(3, ref pbKeyVersion);
    }

    /**f* DesfireAPI/RollKeySet
     *
     * NAME
     *   RollKeySet
     *
     * DESCRIPTION
     *   Set Key set.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_RollKeySet(BYTE key_set_number);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_RollKeySet(SPROX_INSTANCE rInst,
     *                                     BYTE key_set_number);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_RollKeySet(SCARDHANDLE hCard,
     *                                   BYTE key_set_number);
     *
     * INPUTS
     *   BYTE key_set_number     : number of the key set to init
     *
     * RETURNS
     *   DF_OPERATION_OK    : init succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ChangeKey
     *   ChangeKey24
     *   Authenticate
     *   ChangeKeySettings
     *   GetKeySettings
     *   GetKeyVersion
     *
     **/
    public long RollKeySet(byte bKeySetNumber)
    {
      long status;

      /* Create the info block containing the command code and the key number argument. */
      xfer_buffer[INF + 0] = DF_ROLL_KEY_SET;
      xfer_buffer[INF + 1] = bKeySetNumber;
      xfer_length = 2;

      if (secure_mode == SecureMode.EV2)
      {
        status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
      }
      else
      {
        status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
      }
      if (status != DF_OPERATION_OK)
        return status;

      /* As the authentication is lost, the response is sent with CommMode.Plain. */
      /*status = VerifyCmacRecvEv2(xfer_buffer, ref xfer_length);
      if (status != DF_OPERATION_OK)
        return status;*/

      /* Success. */
      return DF_OPERATION_OK;
    }

    /**f* DesfireAPI/InitializeKeySet
    *
    * NAME
    *   InitializeKeySet
    *
    * DESCRIPTION
    *   Initialize the key set in the selected Desfire application.
    *
    * SYNOPSIS
    *
    *   [[sprox_desfire.dll]]
    *   SWORD SPROX_Desfire_InitializeKeySet(BYTE key_set_number,
    *                                    BYTE key_set_type);
    *
    *   [[sprox_desfire_ex.dll]]
    *   SWORD SPROXx_Desfire_InitializeKeySet(SPROX_INSTANCE rInst,
    *                                     BYTE key_set_number,
    *                                     BYTE key_set_type);
    *
    *   [[pcsc_desfire.dll]]
    *   LONG  SCardDesfire_InitializeKeySet(SCARDHANDLE hCard,
    *                                   BYTE key_set_number,
    *                                   BYTE key_set_type);
    *
    * INPUTS
    *   BYTE key_set_number     : number of the key set to init
    *   BYTE key_set_type       : Type ok key set.
    *
    * RETURNS
    *   DF_OPERATION_OK    : init succeeded
    *   Other code if internal or communication error has occured.
    *
    * SEE ALSO
    *   ChangeKey
    *   ChangeKey24
    *   Authenticate
    *   ChangeKeySettings
    *   GetKeySettings
    *   GetKeyVersion
    *
    **/
    public long InitializeKeySet(byte bKeySetNumber, byte bKeySetType)
    {
      long status;
      /* If KeySetNo is 0x00, the command is rejected as it is forbidden to initialize the AKS. */
      if ((bKeySetNumber & 0x7F) == 0x00)
      {
        return DFCARD_LIB_CALL_ERROR;
      }
      /* only 00, 01 or 10 allowed */
      if ((bKeySetType & 0x03) == 0x03)
      {
        return DFCARD_LIB_CALL_ERROR;
      }
      /* Create the info block containing the command code and the key number argument. */
      xfer_buffer[INF + 0] = DF_INITIALIZE_KEY_SET;
      xfer_buffer[INF + 1] = bKeySetNumber;
      xfer_buffer[INF + 2] = bKeySetType;
      xfer_length = 3;

      /* Communicate the info block to the card and check the operation's return status.   */
      /* The returned info block must contain two bytes, the status code and the requested */
      /* key version byte.                                                                 */
      if (secure_mode == SecureMode.EV2)
      {
        status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      else
      {
        status = Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
        
      if (status != DF_OPERATION_OK)
        return status;

      
      /* Success. */
      return DF_OPERATION_OK;
    }

    /**f* DesfireAPI/FinalizeKeySet
     *
     * NAME
     *   InitializeKeySet
     *
     * DESCRIPTION
     *   Initialize the key set in the selected Desfire application.
     *
     * SYNOPSIS
     *
     *   [[sprox_desfire.dll]]
     *   SWORD SPROX_Desfire_FinalizeKeySet(BYTE key_set_number,
     *                                    BYTE key_set_version);
     *
     *   [[sprox_desfire_ex.dll]]
     *   SWORD SPROXx_Desfire_FinalizeKeySet(SPROX_INSTANCE rInst,
     *                                     BYTE key_set_number,
     *                                     BYTE key_set_version);
     *
     *   [[pcsc_desfire.dll]]
     *   LONG  SCardDesfire_FinalizeKeySet(SCARDHANDLE hCard,
     *                                   BYTE key_set_number,
     *                                   BYTE key_set_version);
     *
     * INPUTS
     *   BYTE key_set_number     : number of the key set to init
     *   BYTE key_set_type       : Type ok key set.
     *
     * RETURNS
     *   DF_OPERATION_OK    : init succeeded
     *   Other code if internal or communication error has occured.
     *
     * SEE ALSO
     *   ChangeKey
     *   ChangeKey24
     *   Authenticate
     *   ChangeKeySettings
     *   GetKeySettings
     *   GetKeyVersion
     *
     **/
    public long FinalizeKeySet(byte bKeySetNumber, byte bKkeySetVersion)
    {
      long status;
      /* If KeySetNo is 0x00, the command is rejected as it is forbidden to initialize the AKS. */
      if ((bKeySetNumber & 0x7F) == 0x00)
      {
        return DFCARD_LIB_CALL_ERROR;
      }
      /*The key set version of the key set targeted by Cmd.RollKeySet has
        to be strictly bigger than the key set version of the AKS. 0x00 is never possible*/
      if (bKkeySetVersion == 0x00)
      {
        return DFCARD_LIB_CALL_ERROR;
      }
      /* Create the info block containing the command code and the key number argument. */
      xfer_buffer[INF + 0] = DF_FINALIZE_KEY_SET;
      xfer_buffer[INF + 1] = bKeySetNumber;
      xfer_buffer[INF + 2] = bKkeySetVersion;
      xfer_length = 3;

      /* Communicate the info block to the card and check the operation's return status.   */
      /* The returned info block must contain two bytes, the status code and the requested */
      /* key version byte.                                                                 */
      if (secure_mode == SecureMode.EV2)
      {
        status = Command(0, APPEND_COMMAND_CMAC | COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      else
      {
        status = Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
      }
      if (status != DF_OPERATION_OK)
        return status;

      /* Success. */
      return DF_OPERATION_OK;
    }
  }

}
