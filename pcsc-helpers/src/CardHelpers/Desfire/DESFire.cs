/**
 *
 * \defgroup Desfire
 *
 * \brief Desfire library (.NET only, no native depedency)
 *
 * \copyright
 *   Copyright (c) 2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Reflection;
using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;

namespace SpringCard.PCSC.CardHelpers
{
    /// <summary>
    /// Description of DESFire.
    /// </summary>
    public partial class Desfire
    {
        byte isoWrapping;

        UInt32 current_aid;

        byte session_type;

        int session_key_id;

        UInt32 xfer_length;
        byte[] xfer_buffer;

        byte[] init_vector;
        byte[] cmac_subkey_1;
        byte[] cmac_subkey_2;



        #region EV2

        public enum SecureMode : byte
        {
            EV0 = 0x00,
            EV1,
            EV2
        }

        SecureMode secure_mode;

        byte[] SesAuthMACKey;
        byte[] SesAuthENCKey;

        byte[] TransactionIdentifier;
        UInt16 CmdCtr;
        byte[] PDCaps2;
        byte[] PCDCaps2;
        #endregion //EV2

        ICardTransmitter transmitter;
        ICardApduTransmitter sam_channel = null;
        IRandomGenerator random = new DefaultRandomGenerator();
        ILogger logger = SCARD.SCardLogger;


        /**
         * \brief Library information
         **/
        public static class Library
        {
            /**
             * \brief Read Library information
             **/
            public static readonly AppModuleInfo ModuleInfo = new AppModuleInfo(Assembly.GetExecutingAssembly());
        }

        /* export current context */
        public bool ExportContext(ref byte TisoWrapping, ref UInt32 Tcurrent_aid, ref byte Tsession_type, out byte[] Tsession_key, ref int Tsession_key_id, out byte[] Tinit_vector, out byte[] Tcmac_subkey_1, out byte[] Tcmac_subkey_2)
        {
            bool rc = true;

            TisoWrapping = isoWrapping;
            Tcurrent_aid = current_aid;
            Tsession_type = session_type;
            Tsession_key_id = session_key_id;

            if (init_vector == null)
            {
                rc = false;
            }

            /* create new array copy */
            Tsession_key = SesAuthENCKey;// session_key;
            Tinit_vector = init_vector;
            Tcmac_subkey_1 = cmac_subkey_1;
            Tcmac_subkey_2 = cmac_subkey_2;

            return rc;
        }

        /* Import current context */
        public bool ImportContext(byte TisoWrapping, UInt32 Tcurrent_aid, byte Tsession_type, byte[] Tsession_key, int Tsession_key_id, byte[] Tinit_vector, byte[] Tcmac_subkey_1, byte[] Tcmac_subkey_2)
        {
            isoWrapping = TisoWrapping;
            current_aid = Tcurrent_aid;
            session_type = Tsession_type;
            if (Tsession_key != null)
            {
                /*session_key*/
                SesAuthENCKey = new byte[Tsession_key.Length];
                /*session_key*/
                SesAuthENCKey = Tsession_key;
            }

            session_key_id = Tsession_key_id;

            if (Tinit_vector != null)
            {
                init_vector = new byte[Tinit_vector.Length];
                init_vector = Tinit_vector;
            }

            if (Tcmac_subkey_1 != null)
            {
                cmac_subkey_1 = new byte[Tcmac_subkey_1.Length];
                cmac_subkey_1 = Tcmac_subkey_1;
            }

            if (Tcmac_subkey_2 != null)
            {
                cmac_subkey_2 = new byte[Tcmac_subkey_2.Length];
                cmac_subkey_2 = Tcmac_subkey_2;
            }

            return true;
        }

        public void IsoWrapping(byte mode)
        {
            isoWrapping = mode;
        }

        /**
		 * \brief Instanciate a Desfire card object over a card channel. The channel must already be connected.
		 */
        public Desfire(ICardTransmitter transmitter)
        {
            this.transmitter = transmitter;
            this.isoWrapping = DF_ISO_WRAPPING_OFF;
            xfer_buffer = new byte[64];
            init_vector = new byte[16];
        }

        /**
		 * \brief Instanciate a Desfire card object over a card channel. The channel must already be connected.
		 */
        public Desfire(ICardTransmitter transmitter, byte isoWrapping)
        {
            this.transmitter = transmitter;
            this.isoWrapping = isoWrapping;
            xfer_buffer = new byte[64];
            init_vector = new byte[16];
        }

        /**
		 * \brief Instanciate a Desfire card object over a card channel. The channel must already be connected.
		 */
        public Desfire(ICardTransmitter transmitter, DF_ISO_WRAPPING_E isoWrapping)
        {
            this.transmitter = transmitter;
            this.isoWrapping = (byte) isoWrapping;
            xfer_buffer = new byte[64];
            init_vector = new byte[16];
        }

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void SetRandomGenerator(IRandomGenerator random)
        {
            this.random = random;
        }

        /**f* DesfireAPI/GetErrorMessage
        *
        * NAME
        *   GetErrorMessage
        *
        * DESCRIPTION
        *   Retrieves the error message corresponding to the status code
        *
        * SYNOPSIS
        *
        *   [[sprox_desfire.dll]]
        *   const TCHAR* SPROX_Desfire_GetErrorMessage(SWORD status);
        *
        *   [[sprox_desfire_ex.dll]]
        *   const TCHAR* SPROXx_Desfire_GetErrorMessage(SWORD status);
        *
        *   [[pcsc_desfire.dll]]
        *   const TCHAR*  SCardDesfire_GetErrorMessage(LONG status);
        *
        * INPUTS
        *   SPROX_RC status  : value of error code
        *
        * RETURNS
        *   const TCHAR *    : error message corresponding to the status code
        *
        **/
        public string GetErrorMessage(long status)
        {
            switch (status)
            {
                case DF_OPERATION_OK:
                    return ("Function was executed without failure");

                case DFCARD_ERROR:
                    return ("Desfire : unknown error");
                case DFCARD_ERROR - DF_NO_CHANGES:
                    return ("Desfire : no changes done to backup file, no need to commit/abort");
                case DFCARD_ERROR - DF_OUT_OF_EEPROM_ERROR:
                    return ("Desfire : insufficient NV-memory to complete command");
                case DFCARD_ERROR - DF_ILLEGAL_COMMAND_CODE:
                    return ("Desfire : command code not supported");
                case DFCARD_ERROR - DF_INTEGRITY_ERROR:
                    return ("Desfire : CRC or MAC does not match, or invalid padding bytes");
                case DFCARD_ERROR - DF_NO_SUCH_KEY:
                    return ("Desfire : invalid key number specified");
                case DFCARD_ERROR - DF_LENGTH_ERROR:
                    return ("Desfire : length of command string invalid");
                case DFCARD_ERROR - DF_PERMISSION_DENIED:
                    return ("Desfire : current configuration or status does not allow the requested command");
                case DFCARD_ERROR - DF_PARAMETER_ERROR:
                    return ("Desfire : value of the parameter(s) invalid");
                case DFCARD_ERROR - DF_APPLICATION_NOT_FOUND:
                    return ("Desfire : requested application not present on the card");
                case DFCARD_ERROR - DF_APPL_INTEGRITY_ERROR:
                    return ("Desfire : unrecoverable error within application, application will be disabled");
                case DFCARD_ERROR - DF_AUTHENTICATION_CORRECT:
                    return ("Desfire : successfull authentication");
                case DFCARD_ERROR - DF_AUTHENTICATION_ERROR:
                    return ("Desfire : current authentication status does not allow the requested command");
                case DFCARD_ERROR - DF_ADDITIONAL_FRAME:
                    return ("Desfire : additionnal data frame is expected to be sent");
                case DFCARD_ERROR - DF_BOUNDARY_ERROR:
                    return ("Desfire : attempt to read or write data out of the file's or record's limits");
                case DFCARD_ERROR - DF_CARD_INTEGRITY_ERROR:
                    return ("Desfire : unrecoverable error within the card, the card will be disabled");
                case DFCARD_ERROR - DF_COMMAND_ABORTED:
                    return ("Desfire : the current command has been aborted");
                case DFCARD_ERROR - DF_CARD_DISABLED_ERROR:
                    return ("Desfire : card was disabled by an unrecoverable error");
                case DFCARD_ERROR - DF_COUNT_ERROR:
                    return ("Desfire : maximum number of 28 applications has been reached");
                case DFCARD_ERROR - DF_DUPLICATE_ERROR:
                    return ("Desfire : the specified file or application already exists");
                case DFCARD_ERROR - DF_FILE_NOT_FOUND:
                    return ("Desfire : the specified file does not exists");
                case DFCARD_ERROR - DF_FILE_INTEGRITY_ERROR:
                    return ("Desfire : unrecoverable error within file, file will be disabled");
                case DFCARD_ERROR - DF_EEPROM_ERROR:
                    return ("Desfire : could not complete NV-memory write operation, due to power loss, aborting");

                case DFCARD_LIB_CALL_ERROR:
                    return ("Desfire : invalid parameters in function call");
                case DFCARD_OUT_OF_MEMORY:
                    return ("Desfire : not enough memory");
                case DFCARD_OVERFLOW:
                    return ("Desfire : supplied buffer is too short");
                case DFCARD_WRONG_KEY:
                    return ("Desfire : card authentication denied");
                case DFCARD_WRONG_MAC:
                    return ("Desfire : wrong MAC in card's frame");
                case DFCARD_WRONG_CRC:
                    return ("Desfire : wrong CRC in card's fame");
                case DFCARD_WRONG_LENGTH:
                    return ("Desfire : length of card's frame is invalid");
                case DFCARD_WRONG_PADDING:
                    return ("Desfire : wrong padding in card's frame");
                case DFCARD_WRONG_FILE_TYPE:
                    return ("Desfire : wrong file type");
                case DFCARD_WRONG_RECORD_SIZE:
                    return ("Desfire : wrong record size");

                case DFCARD_PCSC_BAD_RESP_LEN:
                    return ("Desfire : card's response is too short");
                case DFCARD_PCSC_BAD_RESP_SW:
                    return ("Desfire : card's status word is invalid");
                default:
                    break;
            }
            return ("Not a Desfire error code");
        }
    }
}
