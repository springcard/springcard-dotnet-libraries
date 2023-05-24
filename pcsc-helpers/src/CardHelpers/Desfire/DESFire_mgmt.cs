/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 12/09/2017
 * Time: 14:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using System;
using System.Collections.Generic;

namespace SpringCard.PCSC.CardHelpers
{
    /// <summary>
    /// Description of DESFire_mgmt.
    /// </summary>
    public partial class Desfire
    {
        /**
         * NAME
         *   DesfireAPI :: Card management functions
         *
         * COPYRIGHT
         *   (c) 2009 SpringCard - www.springcard.com
         *
         * DESCRIPTION
         *   Implementation of management functions to personalize or format the
         *   DESFIRE card.
         *
         **/

        public long ECP2EndSession()
        {
            xfer_buffer[INF + 0] = DF_ECP2_END_SESSION;
            xfer_length = 1;

            return Command(0, WANTS_OPERATION_OK);
        }


        /**f* DesfireAPI/GetFreeMemory
         *
         * NAME
         *   GetFreeMemory
         *
         * DESCRIPTION
         *   Reads out the number of available bytes on the PICC
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_GetFreeMemory(DWORD *pdwFreeBytes);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_GetFreeMemory(SPROX_INSTANCE rInst,
         *                                      DWORD *pdwFreeBytes);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_GetFreeMemory(SCARDHANDLE hCard,
         *                                    DWORD *pdwFreeBytes);
         *
         * INPUTS
         *   DWORD *pdwFreeBytes : number of free bytes on the PICC
         *
         * RETURNS
         *   DF_OPERATION_OK : operation succeeded
         *   Other code if internal or communication error has occured.
         *
         * NOTES
         *   This command can be issued without valid authentication.
         *
         **/
        public long GetFreeMemory(ref UInt32 pdwFreeBytes)
        {
            long status;

            /* Create the info block containing the command code and the key number argument. */
            xfer_buffer[INF + 0] = DF_GET_FREE_MEMORY;
            xfer_length = 1;

            if (secure_mode == SecureMode.EV2)
            {
                /* Under active authentication, the command Cmd.GetFileSettings requires CommMode.MAC */
                status = Command(4, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }
            else
            {
                /* Communicate the info block to the card and check the operation's return status. */
                /* The returned info block must contain 4 bytes, the status code and the requested */
                /* information.                                                                    */
                status = Command(4, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }
            if (status != DF_OPERATION_OK)
                return status;

            /* Get the actual value. */
            pdwFreeBytes = 0;
            pdwFreeBytes += xfer_buffer[INF + 3];
            pdwFreeBytes <<= 8;
            pdwFreeBytes += xfer_buffer[INF + 2];
            pdwFreeBytes <<= 8;
            pdwFreeBytes += xfer_buffer[INF + 1];


            /* Success. */
            return DF_OPERATION_OK;
        }

        /**f* DesfireAPI/GetCardUID
         *
         * NAME
         *   GetCardUID
         *
         * DESCRIPTION
         *   Reads out the 7-byte serial number of the PICC
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_GetCardUID(BYTE uid[7]);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_GetCardUID(SPROX_INSTANCE rInst,
         *                                   BYTE uid[7]);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_GetCardUID(SCARDHANDLE hCard,
         *                                 BYTE uid[7]);
         *
         *
         * RETURNS
         *   DF_OPERATION_OK : operation succeeded
         *   Other code if internal or communication error has occured.
         *
         * NOTES
         *   This command must be preceded by an authentication (with any key).
         *
         **/
        public long GetCardUID(out byte[] uid)
        {
            uid = new byte[7];

            long status;

            /* Create the info block containing the command code and the key number argument. */
            xfer_buffer[INF + 0] = DF_GET_CARD_UID;
            xfer_length = 1;

            if (secure_mode == SecureMode.EV2)
            {
                /* Communicate the info block to the card and check the operation's return status. */
                /* The returned info block must contain 4 bytes, the status code and the requested */
                /* information.                                                                    */
                CipherSP80038A(xfer_buffer, 1, xfer_length, (uint)xfer_buffer.Length, ref xfer_buffer, 1, ref xfer_length);

                byte[] cipher = new byte[xfer_length];
                byte[] cmac = new byte[8];

                Array.ConstrainedCopy(xfer_buffer, 0, cipher, 0, (int)xfer_length);
                ComputeCmacEv2(cipher, xfer_length, false, ref cmac);
                Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
                xfer_length += 8;

                status = Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
                if (status != DF_OPERATION_OK)
                    return status;

                /* at first we have to decipher the recv_buffer */
                xfer_length -= 1;

                byte[] tmp = new byte[xfer_length];
                Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)xfer_length);
                DeCipherSP80038A(SesAuthENCKey, ref tmp, ref xfer_length, true);
                Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)xfer_length);

                /* Get the actual value. */
                Array.ConstrainedCopy(xfer_buffer, 1, uid, 0, 7);
            }
            else
            {

                /* Communicate the info block to the card and check the operation's return status. */
                /* The returned info block must contain 4 bytes, the status code and the requested */
                /* information.                                                                    */
                status = Command(17, COMPUTE_COMMAND_CMAC | WANTS_OPERATION_OK);
                if (status != DF_OPERATION_OK)
                    return status;

                /* at first we have to decipher the recv_buffer */
                xfer_length -= 1;

                byte[] tmp = new byte[xfer_length];
                Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)xfer_length);
                CipherRecv(ref tmp, ref xfer_length);
                Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)xfer_length);

                /* Get the actual value. */
                Array.ConstrainedCopy(xfer_buffer, 1, uid, 0, 7);
            }

            /* Success. */
            return DF_OPERATION_OK;

        }

        /**f* DesfireAPI/FormatDAM
          *
          * NAME
          *   FormatDAM
          *
          * DESCRIPTION
          *   Releases the DesFire card user memory
          *
          * SYNOPSIS
          *
          *   [[sprox_desfire.dll]]
          *   SWORD SPROX_Desfire_FormatDAM(void);
          *
          *   [[sprox_desfire_ex.dll]]
          *   SWORD SPROXx_Desfire_FormatDAM(SPROX_INSTANCE rInst);
          *
          *   [[pcsc_desfire.dll]]
          *   LONG  SCardDesfire_FormatDAM(SCARDHANDLE hCard);
          *
          * RETURNS
          *   DF_OPERATION_OK    : format succeeded
          *   Other code if internal or communication error has occured.
          *
          * NOTES
          *   Current DAM selected are deleted.
          *   This command always requires a preceding authentication with the DAM card master key.
          *
          **/
        public long FormatDAM()
        {
            /* Communicate the info block to the card and check the operation's return status. */
            return FormatPICC();
        }

        /**f* DesfireAPI/FormatPICC
          *
          * NAME
          *   FormatPICC
          *
          * DESCRIPTION
          *   Releases the DesFire card user memory
          *
          * SYNOPSIS
          *
          *   [[sprox_desfire.dll]]
          *   SWORD SPROX_Desfire_FormatPICC(void);
          *
          *   [[sprox_desfire_ex.dll]]
          *   SWORD SPROXx_Desfire_FormatPICC(SPROX_INSTANCE rInst);
          *
          *   [[pcsc_desfire.dll]]
          *   LONG  SCardDesfire_FormatPICC(SCARDHANDLE hCard);
          *
          * RETURNS
          *   DF_OPERATION_OK    : format succeeded
          *   Other code if internal or communication error has occured.
          *
          * NOTES
          *   All applications are deleted and all files within those applications  are deleted.
          *   This command always requires a preceding authentication with the DesFire card master key.
          *
          **/
        public long FormatPICC()
        {
            /* Create the info block containing the command code. */
            xfer_buffer[INF + 0] = DF_FORMAT_PICC;
            xfer_length = 1;

            if (secure_mode == SecureMode.EV2)
            {
                /* Communicate the info block to the card and check the operation's return status. */
                return Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }
            /* Communicate the info block to the card and check the operation's return status. */
            return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
        }


        /**f* DesfireAPI/CreateApplication
         *
         * NAME
         *   CreateApplication
         *
         * DESCRIPTION
         *   Create a new application on the DesFire card
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_CreateApplication(DWORD aid,
         *                                         BYTE key_setting_1,
         *                                         BYTE key_setting_2);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_CreateApplication(SPROX_INSTANCE rInst,
         *                                          DWORD aid,
         *                                          BYTE key_setting_1,
         *                                          BYTE key_setting_2);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_CreateApplication(SCARDHANDLE hCard,
         *                                        DWORD aid,
         *                                        BYTE key_setting_1,
         *                                        BYTE key_setting_2);
         *
         * INPUTS
         *   DWORD aid          : Application IDentifier
         *   BYTE key_setting_1 : Settings of the Application master key (see chapter 4.3.2 of datasheet
         *                        of mifare DesFire MF3ICD40 for more information)
         *   BYTE key_setting_2 : Number of keys that can be stored within the application for
         *                        cryptographic purposes, plus flags to specify cryptographic method and
         *                        to enable giving ISO names to the EF.
         *
         * RETURNS
         *   DF_OPERATION_OK : application created successfully
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateApplicationIso
         *   DeleteApplication
         *   GetApplicationIDs
         *   SelectApplication
         *
         **/
        public long CreateApplication(UInt32 aid, byte key_setting_1, byte key_setting_2)
        {

            /* Create the info block containing the command code and the given parameters. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_CREATE_APPLICATION;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            xfer_buffer[xfer_length++] = key_setting_1;
            xfer_buffer[xfer_length++] = key_setting_2;

            if (secure_mode == SecureMode.EV2)
            {
                /* Communicate the info block to the card and check the operation's return status. */
                return Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }

            /* Communicate the info block to the card and check the operation's return status. */
            return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
        }


        /**f* DesfireAPI/CreateApplication
        *
        * NAME
        *   CreateApplication
        *
        * DESCRIPTION
        *   Create a new application on the DesFire card
        *
        * SYNOPSIS
        *
        *   [[sprox_desfire.dll]]
        *   SWORD SPROX_Desfire_CreateApplication(DWORD aid,
        *                                         BYTE key_setting_1,
        *                                         BYTE key_setting_2,
        *                                         BYTE key_setting_3,
        *                                        BYTE aks_version,
        *                                        BYTE NoKeySets,
        *                                        BYTE MaxKeySize,
        *                                        BYTE AppKeySetSett);
        *
        *   [[sprox_desfire_ex.dll]]
        *   SWORD SPROXx_Desfire_CreateApplication(SPROX_INSTANCE rInst,
        *                                         DWORD aid,
        *                                         BYTE key_setting_1,
        *                                         BYTE key_setting_2,
        *                                         BYTE key_setting_3,
        *                                        BYTE aks_version,
        *                                        BYTE NoKeySets,
        *                                        BYTE MaxKeySize,
        *                                        BYTE AppKeySetSett);
        *
        *   [[pcsc_desfire.dll]]
        *   LONG  SCardDesfire_CreateApplication(SCARDHANDLE hCard,
        *                                        DWORD aid,
        *                                        BYTE key_setting_1,
        *                                        BYTE key_setting_2,
        *                                        BYTE key_setting_3,
        *                                        BYTE aks_version,
        *                                        BYTE NoKeySets,
        *                                        BYTE MaxKeySize,
        *                                        BYTE AppKeySetSett);
        *
        * INPUTS
        *   DWORD aid          : Application IDentifier
        *   BYTE key_setting_1 : Settings of the Application master key (see chapter 4.3.2 of datasheet
        *                        of mifare DesFire MF3ICD40 for more information)
        *   BYTE key_setting_2 : Number of keys that can be stored within the application for
        *                        cryptographic purposes, plus flags to specify cryptographic method and
        *                        to enable giving ISO names to the EF.
        *   BYTE key_setting_3 : [Optional, present if KeySett2[b4] is set] Additional optional key settings
        *   BYTE aks_version   : [Optional, present if KeySett3[b0] is set] Key Set Version of the Active Key Set
        *   BYTE NoKeySets     : [Optional, present if KeySett3[b0] is set] Number of Key Sets Minimum 2 and maximum 16 key sets
        *   BYTE MaxKeySize    : [Optional, present if KeySett3[b0] is set] Max. Key Size Has to allow for the specified crypto method in KeySett2
        *   BYTE AppKeySetSett : [Optional, present if KeySett3[b0] is set] Application Key
        *
        * RETURNS
        *   DF_OPERATION_OK : application created successfully
        *   Other code if internal or communication error has occured.
        *
        * SEE ALSO
        *   CreateApplicationIso
        *   DeleteApplication
        *   GetApplicationIDs
        *   SelectApplication
        *
        **/
        public long CreateApplication(UInt32 aid,
        byte key_setting_1,
        byte key_setting_2,
        byte key_setting_3,
        byte aks_version,
        byte NoKeySets,
        byte MaxKeySize,
        byte Aks)
        {

            /* Create the info block containing the command code and the given parameters. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_CREATE_APPLICATION;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);

            xfer_buffer[xfer_length++] = key_setting_1;
            xfer_buffer[xfer_length++] = key_setting_2;
            if ((key_setting_2 & 0x10) == 0x10)
                xfer_buffer[xfer_length++] = key_setting_3;

            if ((key_setting_3 & 0x01) == 0x01)
            {
                xfer_buffer[xfer_length++] = aks_version;
                if ((NoKeySets >= 2) && (NoKeySets <= 16))
                {
                    xfer_buffer[xfer_length++] = NoKeySets;
                    if ((MaxKeySize == 0x10) || (MaxKeySize == 18))
                    {
                        xfer_buffer[xfer_length++] = MaxKeySize;
                    }
                    else
                        return DF_PARAMETER_ERROR;

                    xfer_buffer[xfer_length++] = Aks;
                }
                else
                    return DF_PARAMETER_ERROR;
            }

            if (secure_mode == SecureMode.EV2)
            {
                return Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
            }
            /* Send the command string to the PICC and get its response (1st frame exchange).
                The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
            return Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);

        }
        /**f* DesfireAPI/CreateIsoApplication
         *
         * NAME
         *   CreateIsoApplication
         *
         * DESCRIPTION
         *   Create a new application on the DesFire card, and defines the ISO identifier
         *   and name of the application
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_CreateIsoApplication(DWORD aid,
         *                                            BYTE key_setting_1,
         *                                            BYTE key_setting_2,
         *                                            WORD iso_df_id,
         *                                            const BYTE iso_df_name[],
         *                                            BYTE iso_df_namelen);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_CreateIsoApplication(SPROX_INSTANCE rInst,
         *                                             DWORD aid,
         *                                             BYTE key_setting_1,
         *                                             BYTE key_setting_2,
         *                                             WORD iso_df_id,
         *                                             const BYTE iso_df_name[],
         *                                             BYTE iso_df_namelen);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_CreateIsoApplication(SCARDHANDLE hCard,
         *                                           DWORD aid,
         *                                           BYTE key_setting_1,
         *                                           BYTE key_setting_2,
         *                                           WORD iso_df_id,
         *                                           const BYTE iso_df_name[],
         *                                           BYTE iso_df_namelen);
         *
         * INPUTS
         *   DWORD aid                : Application IDentifier
         *   BYTE key_setting_1       : Settings of the Application master key (see chapter 4.3.2 of datasheet
         *                              of mifare DesFire MF3ICD40 for more information)
         *   BYTE key_setting_2       : Number of keys that can be stored within the application for
         *                              cryptographic purposes, plus flags to specify cryptographic method and
         *                              to enable giving ISO names to the EF.
         *   BYTE iso_df_id           : ID of the ISO DF
         *   const BYTE iso_df_name[] : name of the ISO DF
         *   BYTE iso_df_namelen      : length of iso_df_name
         *
         * RETURNS
         *   DF_OPERATION_OK : application created successfully
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateApplication
         *   DeleteApplication
         *   GetApplicationIDs
         *   SelectApplication
         *
         **/
        public long CreateIsoApplication(UInt32 aid, byte key_setting_1, byte key_setting_2, UInt16 iso_df_id, byte[] iso_df_name, byte iso_df_namelen)
        {
            xfer_length = 0;

            /* Create the info block containing the command code and the given parameters. */
            xfer_buffer[xfer_length++] = DF_CREATE_APPLICATION;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            xfer_buffer[xfer_length++] = key_setting_1;
            xfer_buffer[xfer_length++] = key_setting_2;
            xfer_buffer[xfer_length++] = (byte)(iso_df_id & 0x00FF);
            xfer_buffer[xfer_length++] = (byte)((iso_df_id >> 8) & 0x00FF); // JDA

            if (iso_df_name != null)
            {
                if (iso_df_namelen == 0)
                    iso_df_namelen = (byte)iso_df_name.Length;
                if (iso_df_namelen > 16)
                    return DFCARD_LIB_CALL_ERROR;

                Array.ConstrainedCopy(iso_df_name, 0, xfer_buffer, (int)xfer_length, iso_df_namelen);
                xfer_length += iso_df_namelen;
            }

            if (secure_mode == SecureMode.EV2)
            {
                return Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }

            /* Send the command string to the PICC and get its response (1st frame exchange).
                The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
            return Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
        }

        /**f* DesfireAPI/CreateIsoApplication
         *
         * NAME
         *   CreateIsoApplication
         *
         * DESCRIPTION
         *   Create a new application on the DesFire card, and defines the ISO identifier
         *   and name of the application
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_CreateIsoApplication(DWORD aid,
         *                                            BYTE key_setting_1,
         *                                            BYTE key_setting_2,
         *                                            WORD iso_df_id,
         *                                            const BYTE iso_df_name[],
         *                                            BYTE iso_df_namelen);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_CreateIsoApplication(SPROX_INSTANCE rInst,
         *                                             DWORD aid,
         *                                             BYTE key_setting_1,
         *                                             BYTE key_setting_2,
         *                                             WORD iso_df_id,
         *                                             const BYTE iso_df_name[],
         *                                             BYTE iso_df_namelen);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_CreateIsoApplication(SCARDHANDLE hCard,
         *                                           DWORD aid,
         *                                           BYTE key_setting_1,
         *                                           BYTE key_setting_2,
         *                                           WORD iso_df_id,
         *                                           const BYTE iso_df_name[],
         *                                           BYTE iso_df_namelen);
         *
         * INPUTS
         *   DWORD aid                : Application IDentifier
         *   BYTE key_setting_1       : Settings of the Application master key (see chapter 4.3.2 of datasheet
         *                              of mifare DesFire MF3ICD40 for more information)
         *   BYTE key_setting_2       : Number of keys that can be stored within the application for
         *                              cryptographic purposes, plus flags to specify cryptographic method and
         *                              to enable giving ISO names to the EF.
         *   BYTE iso_df_id           : ID of the ISO DF
         *   const BYTE iso_df_name[] : name of the ISO DF
         *   BYTE iso_df_namelen      : length of iso_df_name
         *
         * RETURNS
         *   DF_OPERATION_OK : application created successfully
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateApplication
         *   DeleteApplication
         *   GetApplicationIDs
         *   SelectApplication
         *
         **/
        public long CreateIsoApplication(
          UInt32 aid,
          byte key_setting_1,
          byte key_setting_2,
          byte key_setting_3,
          byte aks_version,
          byte NoKeySets,
          byte MaxKeySize,
          byte Aks,
          ushort iso_df_id,
          byte[] iso_df_name,
          int iso_df_namelen)
        {

            /* Create the info block containing the command code and the given parameters. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_CREATE_APPLICATION;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);

            xfer_buffer[xfer_length++] = key_setting_1;
            xfer_buffer[xfer_length++] = key_setting_2;
            if ((key_setting_2 & 0x10) == 0x10)
                xfer_buffer[xfer_length++] = key_setting_3;
            if ((key_setting_3 & 0x01) == 0x01)
            {
                xfer_buffer[xfer_length++] = aks_version;
                if ((NoKeySets >= 2) && (NoKeySets <= 16))
                {
                    xfer_buffer[xfer_length++] = NoKeySets;
                    if ((NoKeySets == 0x10) || (NoKeySets == 18))
                    {
                        xfer_buffer[xfer_length++] = MaxKeySize;
                    }
                    else
                        return DF_PARAMETER_ERROR;

                    xfer_buffer[xfer_length++] = Aks;
                }
                else
                    return DF_PARAMETER_ERROR;
            }

            xfer_buffer[xfer_length++] = (byte)(iso_df_id & 0x00FF);
            xfer_buffer[xfer_length++] = (byte)((iso_df_id >> 8) & 0x00FF);

            if (iso_df_name != null)
            {
                if (iso_df_namelen == 0)
                    iso_df_namelen = (byte)iso_df_name.Length;
                if (iso_df_namelen > 16)
                    return DFCARD_LIB_CALL_ERROR;

                Array.ConstrainedCopy(iso_df_name, 0, xfer_buffer, (int)xfer_length, iso_df_namelen);
                xfer_length += (uint)iso_df_namelen;
            }

            if (secure_mode == SecureMode.EV2)
            {
                return Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
            }

            /* Send the command string to the PICC and get its response (1st frame exchange).
                The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
            return Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
        }

        /**f* DesfireAPI/DeleteApplication
         *
         * NAME
         *   DeleteApplication
         *
         * DESCRIPTION
         *   Permanently deactivates an application on the DesFire card
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_DeleteApplication(DWORD aid);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_DeleteApplication(SPROX_INSTANCE rInst,
         *                                     DWORD aid);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_DeleteApplication(SCARDHANDLE hCard,
         *                                     DWORD aid);
         *
         * INPUTS
         *   DWORD aid                   : Application IDentifier
         *
         * RETURNS
         *   DF_OPERATION_OK    : application deleted successfully
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateApplication
         *   GetApplicationIDs
         *   SelectApplication
         *
         **/
        public long DeleteApplication(UInt32 aid)
        {
            long status;

            /* Create the info block containing the command code and the given parameters. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_DELETE_APPLICATION;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);

            if (secure_mode == SecureMode.EV2)
            {
                status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }
            else
            {
                /* Communicate the info block to the card and check the operation's return status. */
                status = Command(0, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | LOOSE_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }

            /* If the current application is deleted, the root application is implicitly selected */
            if ((status == DF_OPERATION_OK) && (current_aid == aid))
            {
                current_aid = 0;
                CleanupAuthentication();
            }

            return status;
        }

        /**f* DesfireAPI/GetApplicationIDs
         *
         * NAME
         *   GetApplicationIDs
         *
         * DESCRIPTION
         *   Returns the Application IDentifiers of all active applications on a DesFire card
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_GetApplicationIDs(BYTE aid_max_count,
         *                                     DWORD aid_list[],
         *                                     BYTE *aid_count);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_GetApplicationIDs(SPROX_INSTANCE rInst,
         *                                     BYTE aid_max_count,
         *                                     DWORD aid_list[],
         *                                     BYTE *aid_count);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_GetApplicationIDs(SCARDHANDLE hCard,
         *                                     BYTE aid_max_count,
         *                                     DWORD aid_list[],
         *                                     BYTE *aid_count);
         *
         * INPUTS
         *   BYTE aid_max_count          : maximum number of Application IDentifiers
         *   DWORD aid_list[]            : Application IDentifier list
         *   BYTE *aid_count             : number of Application IDentifiers on DesFire card
         *
         * RETURNS
         *   DF_OPERATION_OK    : operation succeeded
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateApplication
         *   DeleteApplication
         *   SelectApplication
         *
         **/
        public long GetApplicationIDs(byte aid_max_count, ref UInt32[] aid_list, ref byte aid_count)
        {
            byte i;
            UInt32 recv_length = 1;
            byte[] recv_buffer = new byte[256];
            long status;

            /* Set the number of applications to zero */
            aid_count = 0;

            /* create the info block containing the command code */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_GET_APPLICATION_IDS;
            for (; ; )
            {
                if ((secure_mode == SecureMode.EV2) && (session_key_id != -1))
                {
                    /* authentification CMAC is required */
                    status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
                }
                else
                {
                    status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
                }

                if (status != DF_OPERATION_OK)
                    goto done;

                if ((xfer_length - 1) > (256 - recv_length))
                {
                    Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(256 - recv_length));
                    recv_length = 256;
                }
                else
                {
                    Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(xfer_length - 1));
                    recv_length += (xfer_length - 1);
                }                

                if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
                    break;

                xfer_length = 1;
            }

            recv_buffer[0] = DF_OPERATION_OK;

            /* Check the CMAC */
            if (secure_mode == SecureMode.EV2)
            {
                status = VerifyCmacRecvEv2(recv_buffer, ref recv_length);
            }
            else
            {
                status = VerifyCmacRecv(recv_buffer, ref recv_length);
            }
            if (status != DF_OPERATION_OK)
                goto done;


            recv_length -= 1;     /* substract 1 byte for the received status */

            /* ByteCount must be in multiples of APPLICATION_ID_SIZE bytes
               we check this to proof the format integrity
               if zero bytes have been received this is ok -> means no
               applications existing */
            if ((recv_length % APPLICATION_ID_SIZE) != 0)
            {
                status = DFCARD_WRONG_LENGTH;
                goto done;
            }

            for (i = 0; i < (recv_length / APPLICATION_ID_SIZE); i++)
            {
                /* Extract AID */
                if ((i < aid_max_count) && (aid_list != null))
                {
                    UInt32 aid;

                    aid = recv_buffer[INF + 3 + APPLICATION_ID_SIZE * i];
                    aid <<= 8;
                    aid += recv_buffer[INF + 2 + APPLICATION_ID_SIZE * i];
                    aid <<= 8;
                    aid += recv_buffer[INF + 1 + APPLICATION_ID_SIZE * i];

                    aid_list[i] = aid;
                }
            }

            aid_count = i;

            if ((aid_max_count != 0) && (i >= aid_max_count))
                status = DFCARD_OVERFLOW;

            done:
            return status;
        }

        /**f* DesfireAPI/Desfire_GetIsoApplications
         *
         * NAME
         *   Desfire_GetIsoApplications
         *
         * DESCRIPTION
         *   Returns the Application IDentifiers, ISO DF IDs and ISO DF Names of all active
         *   applications on a DesFire card having an ISO DF ID / DF Name
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_GetIsoApplications(BYTE app_max_count,
         *                                          DF_ISO_APPLICATION_ST app_list[],
         *                                          BYTE *app_count);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_GetIsoApplications(SPROX_INSTANCE rInst,
         *                                           BYTE app_max_count,
         *                                           DF_ISO_APPLICATION_ST app_list[],
         *                                           BYTE *app_count);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_GetIsoApplications(SCARDHANDLE hCard,
         *                                         BYTE app_max_count,
         *                                         DF_ISO_APPLICATION_ST app_list[],
         *                                         BYTE *app_count);
         *
         * INPUTS
         *   BYTE app_max_count               : maximum number of Applications
         *   DF_ISO_APPLICATION_ST app_list[] : list of Applications
         *   BYTE *app_count                  : number of Applications on DesFire card
         *
         * RETURNS
         *   DF_OPERATION_OK    : operation succeeded
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateIsoApplication
         *   GetApplicationIDs
         *
         **/
        public long GetIsoApplications(byte app_max_count, List<DF_ISO_APPLICATION_ST> app_list, ref byte app_count)
        {
            byte i;
            UInt32 recv_length = 1;
            byte[] recv_buffer = new byte[1024];
            long status;

            /* Set the number of applications to zero */
            app_count = 0;

            /* create the info block containing the command code */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_GET_DF_NAMES;

            i = 0;
            for (; ; )
            {
                if (secure_mode == SecureMode.EV2)
                {
                    status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
                }
                else
                {
                    status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
                }


                if (status != DF_OPERATION_OK)
                    goto done;

                if ((xfer_length != 1) && (xfer_length < 6))
                {
                    status = DFCARD_WRONG_LENGTH;
                    goto done;
                }

                /* Extract application data */
                if ((app_list != null) && (i < app_max_count))
                {
                    DF_ISO_APPLICATION_ST app = new DF_ISO_APPLICATION_ST();
                    app.abIsoName = new byte[16];

                    app.dwAid = xfer_buffer[INF + 3];
                    app.dwAid <<= 8;
                    app.dwAid += xfer_buffer[INF + 2];
                    app.dwAid <<= 8;
                    app.dwAid += xfer_buffer[INF + 1];

                    app.wIsoId = xfer_buffer[INF + 5];
                    app.wIsoId <<= 8;
                    app.wIsoId += xfer_buffer[INF + 4];

                    for (app.bIsoNameLen = 0; app.bIsoNameLen < app.abIsoName.Length; app.bIsoNameLen++)
                    {
                        if ((UInt16)(INF + 6 + app.bIsoNameLen) >= xfer_length)
                            break;
                        app.abIsoName[app.bIsoNameLen] = xfer_buffer[INF + 6 + app.bIsoNameLen];
                    }
                    app_list.Add(app);

                }
                i++;

                if ((xfer_length - 1) > (1024 - recv_length))
                {
                    Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(1024 - recv_length));
                    recv_length = 1024;
                }
                else
                {
                    /* Remember the frame for later CMAC processing */
                    Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(xfer_length - 1));
                    recv_length += (xfer_length - 1);
                }                

                if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
                    break;

                xfer_length = 1;

            }

            recv_buffer[0] = DF_OPERATION_OK;

            /* Check the CMAC */
            if (secure_mode == SecureMode.EV2)
            {
                status = VerifyCmacRecvEv2(recv_buffer, ref recv_length);
            }
            else
            {
                status = VerifyCmacRecv(recv_buffer, ref recv_length);
            }
            if (status != DF_OPERATION_OK)
                goto done;

            recv_length -= 1;     /* substract 1 byte for the received status */

            app_count = i;

            if (i >= app_max_count)
                status = DFCARD_OVERFLOW;

            done:
            return status;

        }

        /**f* DesfireAPI/SelectApplication
         *
         * NAME
         *   SelectApplication
         *
         * DESCRIPTION
         *   Selects one specific application for further access
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_SelectApplication(DWORD aid);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_SelectApplication(SPROX_INSTANCE rInst,
         *                                     DWORD aid);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_SelectApplication(SCARDHANDLE hCard,
         *                                     DWORD aid);
         *
         * INPUTS
         *   DWORD aid                   : Application IDentifier
         *
         * RETURNS
         *   DF_OPERATION_OK    : application selected
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateApplication
         *   DeleteApplication
         *   GetApplicationIDs
         *
         **/
        public long SelectApplication(UInt32 aid)
        {
            long status;
            UInt32 aid_copy = aid; /* MBA: save it ! */

            /* Each SelectApplication causes a currently valid authentication state to be lost */
            CleanupAuthentication();

            /* Create the info block containing the command code and the given parameters. */
            xfer_buffer[INF + 0] = DF_SELECT_APPLICATION;
            xfer_buffer[INF + 1] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[INF + 2] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[INF + 3] = (byte)(aid & 0x000000FF);
            xfer_length = 4;

            /* Communicate the info block to the card and check the operation's return status. */
            status = Command(0, WANTS_OPERATION_OK);

            if (status == DF_OPERATION_OK)
                current_aid = aid_copy;

            return status;
        }

        /**t* DesfireAPI/DF_VERSION_INFO
         *
         * NAME
         *   DF_VERSION_INFO
         *
         * DESCRIPTION
         *   Structure for returning the information supplied by the GetVersion command.
         *
         * SOURCE
         *   typedef struct
         *   {
         *     // hardware related information
         *     BYTE    bHwVendorID;     // vendor ID (0x04 for NXP)
         *     BYTE    bHwType;         // type (0x01)
         *     BYTE    bHwSubType;      // subtype (0x01)
         *     BYTE    bHwMajorVersion; // major version number
         *     BYTE    bHwMinorVersion; // minor version number
         *     BYTE    bHwStorageSize;  // storage size (0x18 = 4096 bytes)
         *     BYTE    bHwProtocol;     // communication protocol type (0x05 meaning ISO 14443-2 and -3)
        
         *     // software related information
         *     BYTE    bSwVendorID;     // vendor ID (0x04 for NXP)
         *     BYTE    bSwType;         // type (0x01)
         *     BYTE    bSwSubType;      // subtype (0x01)
         *     BYTE    bSwMajorVersion; // major version number
         *     BYTE    bSwMinorVersion; // minor version number
         *     BYTE    bSwStorageSize;  // storage size (0x18 = 4096 bytes)
         *     BYTE    bSwProtocol;     // communication protocol type (0x05 meaning ISO 14443-3 and -4)
         *
         *     BYTE    abUid[7];        // unique serial number
         *     BYTE    abBatchNo[5];    // production batch number
         *     BYTE    bProductionCW;   // calendar week of production
         *     BYTE    bProductionYear; // year of production
         *   } DF_VERSION_INFO;
         *
         **/

        /**f* DesfireAPI/GetVersion
         *
         * NAME
         *   GetVersion
         *
         * DESCRIPTION
         *   Returns manufacturing related data of the DesFire card
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_GetVersion(DF_VERSION_INFO *pVersionInfo);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_GetVersion(SPROX_INSTANCE rInst,
         *                                     DF_VERSION_INFO *pVersionInfo);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_GetVersion(SCARDHANDLE hCard,
         *                                     DF_VERSION_INFO *pVersionInfo);
         *
         * INPUTS
         *   DF_VERSION_INFO *pVersionInfo : card's version information
         *
         * RETURNS
         *   DF_OPERATION_OK    : operation succeeded
         *   Other code if internal or communication error has occured.
         *
         **/
        public long GetVersion(out DF_VERSION_INFO VersionInfo)
        {
            VersionInfo = new DF_VERSION_INFO();
            VersionInfo.abUid = new byte[7];
            VersionInfo.abBatchNo = new byte[5];

            UInt32 recv_length = 1;
            byte[] recv_buffer = new byte[256];
            long status;

            /* create the info block containing the command code */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_GET_VERSION;

            for (; ; )
            {
                if ((secure_mode == SecureMode.EV2) && (session_key_id != -1))
                {
                    /* authentification CMAC is required */
                    status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
                }
                else
                {
                    status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);
                }
                if (status != DF_OPERATION_OK)
                    goto done;


                if ((xfer_length - 1) > (256 - recv_length))
                {
                    Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(256 - recv_length));
                    recv_length = 256;
                }
                else
                {
                    /* Remember the frame for later CMAC processing */
                    Array.ConstrainedCopy(xfer_buffer, INF + 1, recv_buffer, (int)recv_length, (int)(xfer_length - 1));
                    recv_length += (xfer_length - 1);
                }

                if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
                    break;

                xfer_length = 1;
            }

            recv_buffer[0] = DF_OPERATION_OK;

            /* Check the CMAC */
            if (secure_mode == SecureMode.EV2)
            {
                status = VerifyCmacRecvEv2(recv_buffer, ref recv_length);
            }
            else
            {
                status = VerifyCmacRecv(recv_buffer, ref recv_length);
            }

            if (status != DF_OPERATION_OK)
                goto done;

            recv_length -= 1;     /* substract 1 byte for the received status */

            if (recv_length != 28)
            {
                status = DFCARD_WRONG_LENGTH;
                goto done;
            }

            VersionInfo.bHwVendorID = recv_buffer[1];
            VersionInfo.bHwType = recv_buffer[2];
            VersionInfo.bHwSubType = recv_buffer[3];
            VersionInfo.bHwMajorVersion = recv_buffer[4];
            VersionInfo.bHwMinorVersion = recv_buffer[5];
            VersionInfo.bHwStorageSize = recv_buffer[6];
            VersionInfo.bHwProtocol = recv_buffer[7];
            VersionInfo.bSwVendorID = recv_buffer[8];
            VersionInfo.bSwType = recv_buffer[9];
            VersionInfo.bSwSubType = recv_buffer[10];
            VersionInfo.bSwMajorVersion = recv_buffer[11];
            VersionInfo.bSwMinorVersion = recv_buffer[12];
            VersionInfo.bSwStorageSize = recv_buffer[13];
            VersionInfo.bSwProtocol = recv_buffer[14];
            Array.Copy(recv_buffer, 15, VersionInfo.abUid, 0, 7);
            Array.Copy(recv_buffer, 22, VersionInfo.abBatchNo, 0, 5);
            VersionInfo.bProductionCW = recv_buffer[27];
            VersionInfo.bProductionYear = recv_buffer[28];

        done:
            return status;
        }

        /**f* DesfireAPI/SetConfiguration
         *
         * NAME
         *   SetConfiguration
         *
         * DESCRIPTION
         *   Sends the SetConfiguration command to the DESFIRE card.
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_SetConfiguration(BYTE option,
         *                                        const BYTE data[],
         *                                        BYTE length);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_SetConfiguration(SPROX_INSTANCE rInst,
         *                                         BYTE option,
         *                                         const BYTE data[],
         *                                         BYTE length);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_SetConfiguration(SCARDHANDLE hCard,
         *                                       BYTE option,
         *                                       const BYTE data[],
         *                                       BYTE length);
         *
         * INPUTS
         *
         * RETURNS
         *   DF_OPERATION_OK : operation succeeded
         *   Other code if internal or communication error has occured.
         *
         * NOTES
         *   Read DESFIRE EV1 manual, chapter 9.4.9 for details.
         *   DO NOT USE THIS COMMAND unless you're really sure you know
         *   what you're doing!!!
         *
         **/
        public long SetConfiguration(byte option, byte[] data, byte length)
        {
            xfer_length = 0;

            /* Create the info block containing the command code and the key number argument. */
            xfer_buffer[xfer_length++] = DF_SET_CONFIGURATION;
            xfer_buffer[xfer_length++] = option;

            if (data != null)
            {
                Array.ConstrainedCopy(data, 0, xfer_buffer, (int)xfer_length, length);
                xfer_length += length;
            }

            if (secure_mode == SecureMode.EV2)
            {
                CipherSP80038A(xfer_buffer, 2, xfer_length, (uint)xfer_buffer.Length, ref xfer_buffer, 2, ref xfer_length);

                byte[] temp = new byte[xfer_length];
                byte[] cmac = new byte[8];

                Array.ConstrainedCopy(xfer_buffer, 0, temp, 0, (int)xfer_length);
                ComputeCmacEv2(temp, xfer_length, false, ref cmac);

                Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
                xfer_length += 8;

                return Command(0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }
            else
            {
                /* Add the CRC */
                XferAppendCrc(2);

                /* Cipher the option, the data and the CRC */
                XferCipherSend(2);
                return Command(1, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aid"></param>
        /// <param name="DAMSlotNo"></param>
        /// <param name="damSlotVersion"></param>
        /// <param name="quotaLimit"></param>
        /// <param name="key_setting_1"></param>
        /// <param name="key_setting_2"></param>
        /// <param name="key_setting_3"></param>
        /// <param name="aks_version"></param>
        /// <param name="NoKeySets"></param>
        /// <param name="MaxKeySize"></param>
        /// <param name="Aks"></param>
        /// <param name="iso_df_id"></param>
        /// <param name="iso_df_name"></param>
        /// <param name="iso_df_namelen"></param>
        /// <param name="EncK"></param>
        /// <param name="Dammac"></param>
        /// <returns></returns>
        private long CreateDelegatedApplication_Ex(
          UInt32 aid,
          UInt16 DAMSlotNo,
          byte damSlotVersion,
          UInt16 quotaLimit,
          byte key_setting_1,
          byte key_setting_2,
          byte key_setting_3,
          byte aks_version,
          byte NoKeySets,
          byte MaxKeySize,
          byte Aks,
          ushort iso_df_id,
          byte[] iso_df_name,
          int iso_df_namelen,
          byte[] EncK,
          byte[] Dammac)
        {
            long status;


            /* Create the info block containing the command code and the given parameters. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_CREATE_DELEGATED_APPLICATION;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);
            aid >>= 8;
            xfer_buffer[xfer_length++] = (byte)(aid & 0x000000FF);

            xfer_buffer[xfer_length++] = (byte)(DAMSlotNo & 0x00FF);
            DAMSlotNo >>= 8;
            xfer_buffer[xfer_length++] = (byte)(DAMSlotNo & 0x00FF);
            xfer_buffer[xfer_length++] = damSlotVersion;
            xfer_buffer[xfer_length++] = (byte)(quotaLimit & 0x00FF);
            quotaLimit >>= 8;
            xfer_buffer[xfer_length++] = (byte)(quotaLimit & 0x00FF);

            xfer_buffer[xfer_length++] = key_setting_1;
            xfer_buffer[xfer_length++] = key_setting_2;
            if ((key_setting_2 & 0x10) == 0x10)
                xfer_buffer[xfer_length++] = key_setting_3;
            if ((key_setting_3 & 0x01) == 0x01)
            {
                xfer_buffer[xfer_length++] = aks_version;
                if ((NoKeySets >= 2) && (NoKeySets <= 16))
                {
                    xfer_buffer[xfer_length++] = NoKeySets;
                    if ((NoKeySets == 0x10) || (NoKeySets == 18))
                    {
                        xfer_buffer[xfer_length++] = MaxKeySize;
                    }
                    else
                        return DF_PARAMETER_ERROR;

                    xfer_buffer[xfer_length++] = Aks;
                }
                else
                    return DF_PARAMETER_ERROR;
            }
            if (iso_df_name != null)
            {
                xfer_buffer[xfer_length++] = (byte)(iso_df_id & 0x00FF);
                xfer_buffer[xfer_length++] = (byte)((iso_df_id >> 8) & 0x00FF);

                if (iso_df_namelen == 0)
                    iso_df_namelen = (byte)iso_df_name.Length;
                if (iso_df_namelen > 16)
                    return DFCARD_LIB_CALL_ERROR;

                Array.ConstrainedCopy(iso_df_name, 0, xfer_buffer, (int)xfer_length, iso_df_namelen);
                xfer_length += (uint)iso_df_namelen;
            }


            byte[] buffer = new byte[xfer_length + 64];
            uint buffer_length = xfer_length;
            Array.ConstrainedCopy(xfer_buffer, 0, buffer, (int)0, (int)buffer_length);

            if (secure_mode == SecureMode.EV2)
            {
                /*If secure messaging is active, the secure messaging is applied as if the command or
                response would have been sent in a single large frame. The 0xAF command and
                response codes are ignored for these calculations. If a MAC is appended to the command
                or response, this may result in an additional frame if necessary.*/
                Array.ConstrainedCopy(EncK, 0, buffer, (int)buffer_length, EncK.Length);
                buffer_length += (uint)EncK.Length;
                Array.ConstrainedCopy(Dammac, 0, buffer, (int)buffer_length, Dammac.Length);
                buffer_length += (uint)Dammac.Length;
                byte[] cmac = new byte[8];
                ComputeCmacEv2(buffer, buffer_length, false, ref cmac);

                status = Command(0, WANTS_ADDITIONAL_FRAME);
                if (status != DF_OPERATION_OK)
                    return DFCARD_ERROR;

                if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
                    return DFCARD_ERROR;

                xfer_length = 0;
                xfer_buffer[xfer_length++] = DF_ADDITIONAL_FRAME;
                Array.ConstrainedCopy(EncK, 0, xfer_buffer, (int)xfer_length, EncK.Length);
                xfer_length += (uint)EncK.Length;
                Array.ConstrainedCopy(Dammac, 0, xfer_buffer, (int)xfer_length, Dammac.Length);
                xfer_length += (uint)Dammac.Length;
                Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
                xfer_length += 8;

                return Command(0, CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }
            else
            {
                /* Send the command string to the PICC and get its response (1st frame exchange).
                   The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
                status = Command(0, COMPUTE_COMMAND_CMAC | WANTS_ADDITIONAL_FRAME | WANTS_OPERATION_OK);

                if (status != DF_OPERATION_OK)
                    return DFCARD_ERROR;

                if (xfer_buffer[INF + 0] != DF_ADDITIONAL_FRAME)
                    return DFCARD_ERROR;

                xfer_length = 0;
                xfer_buffer[xfer_length++] = DF_ADDITIONAL_FRAME;
                Array.ConstrainedCopy(EncK, 0, xfer_buffer, 1, EncK.Length);
                xfer_length += (uint)EncK.Length;
                Array.ConstrainedCopy(Dammac, 0, xfer_buffer, (int)xfer_length, Dammac.Length);
                xfer_length += (uint)Dammac.Length;

                /* Send the 2nd frame to the PICC and get its response. */
                return Command(0, WANTS_OPERATION_OK);
            }
        }

        /**f* DesfireAPI/CreateDelegatedApplication
         *
         * NAME
         *   CreateApplicationEv2
         *
         * DESCRIPTION
         *   Create a new application on the DesFire card
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_CreateDelegatedApplication(DWORD aid,
         *                                         WORD DAMSlotNo, 
         *                                         BYTE damSlotVersion,
         *                                         WORD quotaLimit, 
         *                                         BYTE key_setting_1,
         *                                         BYTE key_setting_2,
         *                                         BYTE key_setting_3,
         *                                        BYTE aks_version,
         *                                        BYTE NoKeySets,
         *                                        BYTE MaxKeySize,
         *                                        BYTE AppKeySetSett);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_CreateDelegatedApplication(SPROX_INSTANCE rInst,
         *                                         DWORD aid,
         *                                         WORD DAMSlotNo, 
         *                                         BYTE damSlotVersion,
         *                                         WORD quotaLimit, 
         *                                         BYTE key_setting_1,
         *                                         BYTE key_setting_2,
         *                                         BYTE key_setting_3,
         *                                        BYTE aks_version,
         *                                        BYTE NoKeySets,
         *                                        BYTE MaxKeySize,
         *                                        BYTE AppKeySetSett);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_CreateDelegatedApplication(SCARDHANDLE hCard,
         *                                        DWORD aid,
         *                                        WORD DAMSlotNo, 
         *                                        BYTE damSlotVersion,
         *                                        WORD quotaLimit, 
         *                                        BYTE key_setting_1,
         *                                        BYTE key_setting_2,
         *                                        BYTE key_setting_3,
         *                                        BYTE aks_version,
         *                                        BYTE NoKeySets,
         *                                        BYTE MaxKeySize,
         *                                        BYTE AppKeySetSett);
         *
         * INPUTS
         *   DWORD aid          : Application IDentifier
         *   WORD DAMSlotNo     : Slot number associated with the memory space of the application to be created.
         *   BYTE damSlotVersion: Slot version associated with the memory space of the application to be created.
         *   WORD quotaLimit    : Maximal memory consumption of the application in 32 byte blocks.
         *   BYTE key_setting_1 : Settings of the Application master key (see chapter 4.3.2 of datasheet
         *                        of mifare DesFire MF3ICD40 for more information)
         *   BYTE key_setting_2 : Number of keys that can be stored within the application for
         *                        cryptographic purposes, plus flags to specify cryptographic method and
         *                        to enable giving ISO names to the EF.
         *   BYTE key_setting_3 : [Optional, present if KeySett2[b4] is set] Additional optional key settings
         *   BYTE aks_version   : [Optional, present if KeySett3[b0] is set] Key Set Version of the Active Key Set
         *   BYTE NoKeySets     : [Optional, present if KeySett3[b0] is set] Number of Key Sets Minimum 2 and maximum 16 key sets
         *   BYTE MaxKeySize    : [Optional, present if KeySett3[b0] is set] Max. Key Size Has to allow for the specified crypto method in KeySett2
         *   BYTE AppKeySetSett : [Optional, present if KeySett3[b0] is set] Application Key
         *
         * RETURNS
         *   DF_OPERATION_OK : application created successfully
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateApplicationIso
         *   DeleteApplication
         *   GetApplicationIDs
         *   SelectApplication
         *
         **/
        public long CreateDelegatedApplication(UInt32 aid,
          UInt16 DAMSlotNo,
          byte damSlotVersion,
          UInt16 quotaLimit,
          byte key_setting_1,
          byte key_setting_2,
          byte key_setting_3,
          byte aks_version,
          byte NoKeySets,
          byte MaxKeySize,
          byte Aks,
          byte[] EncK,
          byte[] Dammac)
        {
            return CreateDelegatedApplication_Ex(
              aid,
              DAMSlotNo,
              damSlotVersion,
              quotaLimit,
              key_setting_1,
              key_setting_2,
              key_setting_3,
              aks_version,
              NoKeySets,
              MaxKeySize,
              Aks,
              0x0000,
              null,
              0,
              EncK,
              Dammac);

        }

        /**f* DesfireAPI/CreateIsoDelegatedApplication
         *
         * NAME
         *   CreateIsoApplication
         *
         * DESCRIPTION
         *   Create a new application on the DesFire card, and defines the ISO identifier
         *   and name of the application
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_CreateIsoDelegatedApplication(DWORD aid,
         *                                            BYTE key_setting_1,
         *                                            BYTE key_setting_2,
         *                                            WORD iso_df_id,
         *                                            const BYTE iso_df_name[],
         *                                            BYTE iso_df_namelen);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_CreateIsoDelegatedApplication(SPROX_INSTANCE rInst,
         *                                             DWORD aid,
         *                                             BYTE key_setting_1,
         *                                             BYTE key_setting_2,
         *                                             WORD iso_df_id,
         *                                             const BYTE iso_df_name[],
         *                                             BYTE iso_df_namelen);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_CreateIsoDelegatedApplication(SCARDHANDLE hCard,
         *                                           DWORD aid,
         *                                           BYTE key_setting_1,
         *                                           BYTE key_setting_2,
         *                                           WORD iso_df_id,
         *                                           const BYTE iso_df_name[],
         *                                           BYTE iso_df_namelen);
         *
         * INPUTS
         *   DWORD aid                : Application IDentifier
         *   BYTE key_setting_1       : Settings of the Application master key (see chapter 4.3.2 of datasheet
         *                              of mifare DesFire MF3ICD40 for more information)
         *   BYTE key_setting_2       : Number of keys that can be stored within the application for
         *                              cryptographic purposes, plus flags to specify cryptographic method and
         *                              to enable giving ISO names to the EF.
         *   BYTE iso_df_id           : ID of the ISO DF
         *   const BYTE iso_df_name[] : name of the ISO DF
         *   BYTE iso_df_namelen      : length of iso_df_name
         *
         * RETURNS
         *   DF_OPERATION_OK : application created successfully
         *   Other code if internal or communication error has occured.
         *
         * SEE ALSO
         *   CreateApplication
         *   DeleteApplication
         *   GetApplicationIDs
         *   SelectApplication
         *
         **/
        public long CreateIsoDelegatedApplication(
          UInt32 aid,
          UInt16 DAMSlotNo,
          byte damSlotVersion,
          UInt16 quotaLimit,
          byte key_setting_1,
          byte key_setting_2,
          byte key_setting_3,
          byte aks_version,
          byte NoKeySets,
          byte MaxKeySize,
          byte Aks,
          ushort iso_df_id,
          byte[] iso_df_name,
          int iso_df_namelen,
          byte[] EncK,
          byte[] Dammac)
        {
            return CreateDelegatedApplication_Ex(
              aid,
              DAMSlotNo,
              damSlotVersion,
              quotaLimit,
              key_setting_1,
              key_setting_2,
              key_setting_3,
              aks_version,
              NoKeySets,
              MaxKeySize,
              Aks,
              iso_df_id,
              iso_df_name,
              iso_df_namelen,
              EncK,
              Dammac);
        }

        /**f* DesfireAPI/Desfire_GetDelegatedInfo
         *
         * NAME
         *   GetDelegatedInfo
         *
         * DESCRIPTION
         *   Get specific information associated with delegated applications.
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_GetDelegatedInfo(WORD DAMSlotNo);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_GetDelegatedInfo(SPROX_INSTANCE rInst,
         *                                  WORD DAMSlotNo);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_GetDelegatedInfo(SCARDHANDLE hCard,
         *                                WORD DAMSlotNo]);
         *
         * INPUTS
         *   WORD DAMSlotNo        : slot id
         *
         * RETURNS
         *   DF_OPERATION_OK    : change succeeded
         *   Other code if internal or communication error has occured.
         *
         * NOTES
         *
         * SEE ALSO
         *
         **/
        public long GetDelegatedInfo(UInt16 DAMSlotNo,
          ref UInt32 aid,
          ref byte dam_slot_version,
          ref byte quota_limit,
          ref byte free_blocks)
        {
            long status;

            /* Begin the info block with the command code and the number of the key to be changed. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_GET_DELEGATED_INFO;
            xfer_buffer[xfer_length++] = (byte)(DAMSlotNo & 0x00FF);
            DAMSlotNo >>= 8;
            xfer_buffer[xfer_length++] = (byte)(DAMSlotNo & 0x00FF);

            /* EV2 secure mode */
            if (secure_mode == SecureMode.EV2)
            {
                status = Command(0, COMPUTE_COMMAND_CMAC | APPEND_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }
            else
            {
                /* Communicate the info block to the card and check the operation's return status. */
                status = Command(9, COMPUTE_COMMAND_CMAC | CHECK_RESPONSE_CMAC | WANTS_OPERATION_OK);
            }

            if (status != DF_OPERATION_OK)
                return status;

            if (xfer_buffer[INF + 0] != DF_OPERATION_OK)
                return DF_ADDITIONAL_FRAME;


            /* Dam slot version. */
            dam_slot_version = xfer_buffer[INF + 1];

            /* QuotaLimit. */
            quota_limit = 0;
            quota_limit += xfer_buffer[INF + 3];
            quota_limit <<= 8;
            quota_limit += xfer_buffer[INF + 2];

            /* FreeBlocks. */
            free_blocks = 0;
            free_blocks += xfer_buffer[INF + 5];
            free_blocks <<= 8;
            free_blocks += xfer_buffer[INF + 4];

            /* AID. */
            aid = xfer_buffer[INF + 8];
            aid <<= 8;
            aid += xfer_buffer[INF + 7];
            aid <<= 8;
            aid += xfer_buffer[INF + 6];

            return DF_OPERATION_OK;
        }
        /// <summary>
        /// EncK = EDAM(KPICCDAMENC,Random(7)||KAppDAMDefault||KeyVerAppDAMDefault)
        /// </summary>
        /// <param name="PICCDAMENCKey"></param>
        /// <param name="DAMTransport"></param>
        /// <returns></returns>
        public byte[] Calc_EncK(byte[] PICCDAMENCKey, byte[] AppDAMDefault, byte KeyVerAppDAMDefault)
        {
            byte[] IV = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] rnd = random.Get(7);

            byte[] input = new byte[AppDAMDefault.Length + 8];

            Array.Copy(rnd, 0, input, 0, 7);
            Array.Copy(AppDAMDefault, 0, input, 7, 16);
            input[input.Length - 1] = KeyVerAppDAMDefault;

#if _VERBOSE
            Console.WriteLine("ENCK in " + BinConvert.ToHex(input, input.Length));
            Console.WriteLine("ENCK in " + BinConvert.ToHex(PICCDAMENCKey, 16));
            Console.WriteLine("ENCK in " + BinConvert.ToHex(IV, 16));
#endif

            byte[] EncK = DesfireCrypto.AES_Encrypt(input, PICCDAMENCKey, IV);
            string s = "EncK ";
            for (int i = 0; i < EncK.Length; i++)
            {
                s += String.Format("{0:x02}", EncK[i]);
            }
#if _VERBOSE
            Console.WriteLine("ENCK  out" + BinConvert.ToHex(EncK, EncK.Length));
#endif
            return EncK;
        }

        /// <summary>
        /// DAMMAC = MACtDAM(KPICCDAMMAC,Cmd||AID||DAMSlotNo
        /// ||DAMSlotVersion||QuotaLimit||KeySett1||KeySett2
        /// [|| AKSVersion || NoKeySets || MaxKeySize || RollKey]
        /// [|| ISOFileID][|| ISODFName]||EncK)
        /// </summary>
        /// <param name="PICCDAMMACKey"></param>
        /// <param name="cmd"></param>
        /// <param name="aid"></param>
        /// <param name="damSlotNo"></param>
        /// <param name="damSlotVersion"></param>
        /// <param name="quotaLimit"></param>
        /// <param name="key_setting_1"></param>
        /// <param name="key_setting_2"></param>
        /// <param name="key_setting_3"></param>
        /// <param name="aks_version"></param>
        /// <param name="NoKeySets"></param>
        /// <param name="MaxKeySize"></param>
        /// <param name="AppKeySetSett"></param>
        /// <param name="ENCK"></param>
        /// <returns></returns>
        public byte[] Calc_DAMMAC(
            byte[] PICCDAMMACKey,
            byte cmd,
            UInt32 aid,
            UInt16 damSlotNo,
            byte damSlotVersion,
            UInt16 quotaLimit,
            byte key_setting_1,
            byte key_setting_2,
            byte key_setting_3,
            byte aks_version,
            byte NoKeySets,
            byte MaxKeySize,
            byte Aks,
            ushort iso_df_id,
            byte[] iso_df_name,
            byte[] ENCK)
        {

            byte[] IV = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] input;
            int input_lenght = 0;

            if ((key_setting_2 & 0x10) == 0x10)
            {
                input_lenght++;
                if ((key_setting_3 & 0x01) == 0x01)
                {
                    input_lenght++;
                    if ((NoKeySets >= 2) && (NoKeySets <= 16))
                    {
                        input_lenght++;
                        if ((NoKeySets == 0x10) || (NoKeySets == 18))
                        {
                            input_lenght++;
                        }
                        input_lenght++;
                    }
                }
            }

            if (iso_df_name != null)
                input = new byte[11 + ENCK.Length + (iso_df_name.Length + 2) + input_lenght];
            else
                input = new byte[11 + ENCK.Length + input_lenght];

            input_lenght = 0;
            input[input_lenght++] = cmd;
            input[input_lenght++] = (byte)(aid & 0x000000FF);
            input[input_lenght++] = (byte)((aid >> 8) & 0x00FF);
            input[input_lenght++] = (byte)((aid >> 16) & 0x00FF);
            input[input_lenght++] = (byte)(damSlotNo & 0x00FF);
            input[input_lenght++] = (byte)(damSlotNo >> 8);
            input[input_lenght++] = damSlotVersion;
            input[input_lenght++] = (byte)(quotaLimit & 0x00FF);
            input[input_lenght++] = (byte)(quotaLimit >> 8);
            input[input_lenght++] = key_setting_1;
            input[input_lenght++] = key_setting_2;

            if ((key_setting_2 & 0x10) == 0x10)
            {
                input[input_lenght++] = key_setting_3;
                if ((key_setting_3 & 0x01) == 0x01)
                {
                    input[input_lenght++] = aks_version;
                    if ((NoKeySets >= 2) && (NoKeySets <= 16))
                    {
                        input[input_lenght++] = NoKeySets;
                        if ((NoKeySets == 0x10) || (NoKeySets == 18))
                        {
                            input[input_lenght++] = MaxKeySize;
                        }

                        input[input_lenght++] = Aks;
                    }
                }
            }

            if (iso_df_name != null)
            {
                input[input_lenght++] = (byte)(iso_df_id & 0x00FF);
                input[input_lenght++] = (byte)(iso_df_id >> 8);

                for (int i = 0; i < iso_df_name.Length; i++)
                    input[input_lenght++] = iso_df_name[i];
            }
            /* add encK at the end */
            for (int i = 0; i < ENCK.Length; i++)
                input[input_lenght++] = ENCK[i];

#if _VERBOSE
            Console.WriteLine("DAMMAC  in " + BinConvert.ToHex(input, input.Length));
            Console.WriteLine("DAMMAC  PICCDAMMACKey " + BinConvert.ToHex(PICCDAMMACKey, PICCDAMMACKey.Length));
            Console.WriteLine("DAMMAC  IV " + BinConvert.ToHex(IV, IV.Length));
#endif

            byte[] CMAC_enormous = DesfireCrypto.CalculateCMAC(PICCDAMMACKey, IV, input);
#if _VERBOSE
            Console.WriteLine("DAMMAC  out " + BinConvert.ToHex(CMAC_enormous, CMAC_enormous.Length));
#endif

            string s = "CMAC_enormous calcul soft: ";
            for (int i = 0; i < CMAC_enormous.Length; i++)
                s += String.Format("{0:x02}", CMAC_enormous[i]);

            //Console.WriteLine(s);

            byte[] CMAC_full = new byte[16];
            Array.ConstrainedCopy(CMAC_enormous, CMAC_enormous.Length - 16, CMAC_full, 0, 16);

            s = "CMAC_full calcul soft: ";
            for (int i = 0; i < CMAC_full.Length; i++)
            {
                s += String.Format("{0:x02}", CMAC_full[i]);
            }

            byte[] CMAC = new byte[8];
            int j = 0;

            for (int i = 1; i < CMAC_full.Length;)
            {
                CMAC[j++] = CMAC_full[i];
                i += 2;
            }

            s = "CMAC calcul soft: ";
            for (int i = 0; i < CMAC.Length; i++)
            {
                s += String.Format("{0:x02}", CMAC[i]);
            }
            //Console.WriteLine(s);

            return CMAC;
        }
    }
}
