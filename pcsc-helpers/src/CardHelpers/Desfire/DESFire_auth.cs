/*
 * Created by SharpDevelop.
 * User: Jerome Izaac
 * Date: 08/09/2017
 * Time: 10:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using System;


namespace SpringCard.PCSC.CardHelpers
{
    /// <summary>
    /// Description of DESFire_auth.
    /// </summary>
    public partial class Desfire
    {
        public bool ActivateBenchAuth = false;
        void CleanupInitVector()
        {
            for (int i = 0; i < init_vector.Length; i++)
                init_vector[i] = 0;
        }


        /**f* DesfireAPI/Authenticate
         *
         * NAME
         *   Authenticate
         *
         * DESCRIPTION
         *   Perform authentication using the specified DES or 3DES key on the currently
         *   selected DESFIRE application.
         *   This is the legacy function, available even on DESFIRE EV0.
         *   The generated session key is afterwards used for non-ISO ciphering or macing.
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SWORD SPROX_Desfire_Authenticate(BYTE bKeyNumber,
         *                                    const BYTE pbAccessKey[16]);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SWORD SPROXx_Desfire_Authenticate(SPROX_INSTANCE rInst,
         *                                     BYTE bKeyNumber,
         *                                     const BYTE pbAccessKey[16]);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_Authenticate(SCARDHANDLE hCard,
         *                                   BYTE bKeyNumber,
         *                                   const BYTE pbAccessKey[16]);
         *
         * INPUTS
         *   BYTE bKeyNumber             : number of the key (KeyNo)
         *   const BYTE pbAccessKey[16]  : 16-byte Access Key (DES/3DES2K keys)
         *
         * RETURNS
         *   DF_OPERATION_OK    : authentication succeed
         *   Other code if internal or communication error has occured. 
         *
         * NOTES
         *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
         *   * If the 2nd half of the key string is equal to the 1st half, the key is
         *   handled as a single DES key by the DESFIRE card.
         *   * If the 2nd half of the key string is NOT equal to the 1st half, the key
         *   is handled as a 3DES key.
         *
         * SEE ALSO
         *   AuthenticateIso24
         *   AuthenticateIso
         *   AuthenticateAes
         *   ChangeKeySettings
         *   GetKeySettings
         *   ChangeKey
         *   GetKeyVersion
         *
         **/
        public long Authenticate(byte bKeyNumber, byte[] pbAccessKey)
        {
            UInt32 t;
            long status;
            byte[] abRndB = new byte[8];
            byte[] abRndA = new byte[8];

            logger.trace("Authenticate, key number={0:X02}, key value={1}", bKeyNumber, BinConvert.ToHex(pbAccessKey));

            if (pbAccessKey == null)
                return DFCARD_LIB_CALL_ERROR;

            if (pbAccessKey.Length != 16)
                return DFCARD_LIB_CALL_ERROR;

            /* Each new Authenticate must invalidate the current authentication status. */
            CleanupAuthentication();

            /* Check whether a TripleDES key was passed (both key halves are different). */
            bool areEqual = true;
            for (int i = 0; i < 8; i++)
                if (pbAccessKey[i] != pbAccessKey[8 + i])
                    areEqual = false;

            if (!areEqual)
            {
                /* If the two key halves are not identical, we are doing TripleDES. */
                /* We have to remember that TripleDES is in effect, because the manner of building
                   the session key is different in this case. */
                SetKey(pbAccessKey);
                session_type = KEY_LEGACY_3DES;
                logger.debug("\tKey is 3DES");
            }
            else
            {
                SetKey(pbAccessKey);
                session_type = KEY_LEGACY_DES;
                logger.debug("\tKey is simple DES");
            }

            /* Create the command string consisting of the command byte and the parameter byte. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_AUTHENTICATE;
            xfer_buffer[xfer_length++] = bKeyNumber;

            /* Send the command string to the PICC and get its response (1st frame exchange).
               The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
            status = Command(0, WANTS_ADDITIONAL_FRAME);
            if (status != DF_OPERATION_OK)
                return status;

            /* Check the number of bytes received, we expect 9 bytes. */
            if (xfer_length != 9)
            {
                /* Error: block with inappropriate number of bytes received from the card. */
                return DFCARD_WRONG_LENGTH;
            }

            /* OK, we received the 8 bytes Ek( RndB ) from the PICC.
               Decipher Ek( RndB ) to get RndB in ctx->xfer_buffer.
               Note that the status code has already been extracted from the queue. */
            t = 8;
            byte[] tmp = new byte[8];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, 8);
            logger.debug("\tEk(RndB)={0}", BinConvert.ToHex(tmp));

            /*
            {
              Console.Write("To decipher: " );
              for (int k=0; k< tmp.Length; k++)
                Console.Write(String.Format("{0:x02}", tmp[k]));
              Console.Write("\n");
            }
            */

            CipherRecv(ref tmp, ref t);
            /*
            {
              Console.Write("Deciphered: " );
              for (int k=0; k< tmp.Length; k++)
                Console.Write(String.Format("{0:x02}", tmp[k]));
              Console.Write("\n");
            }
            */
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);


            /* Store this RndB (is needed later on for generating the session key). */
            Array.ConstrainedCopy(xfer_buffer, 1, abRndB, 0, 8);
            logger.debug("\tRndB={0}", BinConvert.ToHex(abRndB));

            /* Now the PCD has to generate RndA. */
            //GetRandomBytes(SPROX_PARAM_P  abRndA, 8);
            abRndA = random.Get(8);
            logger.debug("\tRndA={0}", BinConvert.ToHex(abRndA));

            /* Start the second frame with a status byte indicating to the PICC that the Authenticate
               command is continued. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_ADDITIONAL_FRAME;

            /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
               after the status byte. */
            Array.ConstrainedCopy(abRndA, 0, xfer_buffer, (int)xfer_length, 8);
            xfer_length += 8;

            Array.ConstrainedCopy(abRndB, 1, xfer_buffer, (int)xfer_length, 7);
            xfer_length += 7;
            xfer_buffer[xfer_length++] = abRndB[0]; /* first byte move to last byte */

            logger.debug("\tRndA || RndB'={0}", BinConvert.ToHex(xfer_buffer, 1, 16));

            /* Apply the DES send operation to the 16 argument bytes before sending the second frame
               to the PICC ( do not include the status byte in the DES operation ). */
            t = 16;
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
            CipherSend(ref tmp, ref t, t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

            logger.debug("\tEk(RndA || RndB')={0}", BinConvert.ToHex(xfer_buffer, 1, 16));

            /* Send the 2nd frame to the PICC and get its response. */
            status = Command(0, WANTS_OPERATION_OK);
            if (status != DF_OPERATION_OK)
                return status;

            /* We should now have Ek( RndA' ) in our buffer.
               RndA' was made from RndA by the PICC by rotating the string one byte to the left.
               Decipher Ek( RndA' ) to get RndA' in ctx->xfer_buffer. */
            t = 8;
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);

            logger.debug("\tEk(RndA')={0}", BinConvert.ToHex(tmp));

            CipherRecv(ref tmp, ref t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

            logger.debug("\tRndA'={0}", BinConvert.ToHex(tmp));

            /* Now we have RndA' in our buffer.
               We have to check whether it matches our local copy of RndA.
               If one of the subsequent comparisons fails, we do not trust the PICC and therefore
               abort the authentication procedure ( no session key is generated ). */

            /* First compare the bytes 1 to 7 of RndA with the first 7 bytes in the queue. */
            for (int i = 0; i < 7; i++)
                if (xfer_buffer[INF + 1 + i] != abRndA[1 + i])
                    return DFCARD_WRONG_KEY;

            /* Then compare the leftmost byte of RndA with the last byte in the queue. */
            if (xfer_buffer[INF + 8] != abRndA[0])
                return DFCARD_WRONG_KEY;

            /* The actual authentication has succeeded.
               Finally we have to generate the session key from both random numbers RndA and RndB.
               The first half of the session key is the concatenation of RndA[0-3] + RndB[0-3]. */
            /* If the original key passed through pbAccessKey is a TripleDES key, the session
               key must also be a TripleDES key. */

            if (session_type == KEY_LEGACY_DES)
            {
                /*
                memcpy(ctx->session_key +  0, abRndA + 0, 4);
                memcpy(ctx->session_key +  4, abRndB + 0, 4);
                memcpy(ctx->session_key +  8, ctx->session_key, 8);
                memcpy(ctx->session_key + 16, ctx->session_key, 8);  

                Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, NULL, NULL);
                */

                /*session_key*/
                SesAuthENCKey = new byte[16];
                Array.ConstrainedCopy(abRndA, 0, /*session_key*/SesAuthENCKey, 0, 4);
                Array.ConstrainedCopy(abRndB, 0, /*session_key*/SesAuthENCKey, 4, 4);
                Array.ConstrainedCopy(/*session_key*/SesAuthENCKey, 0, /*session_key*/SesAuthENCKey, 8, 8);
                logger.trace("Session key={0}", BinConvert.ToHex(SesAuthENCKey));
                SetKey(/*session_key*/SesAuthENCKey);
            }
            else
            if (session_type == KEY_LEGACY_3DES)
            {
                /* For TripleDES generate the second part of the session key.
                   This is the concatenation of RndA[4-7] + RndB[4-7]. */
                /*
                memcpy(ctx->session_key +  0, abRndA + 0, 4);
                memcpy(ctx->session_key +  4, abRndB + 0, 4);
                memcpy(ctx->session_key +  8, abRndA + 4, 4);
                memcpy(ctx->session_key + 12, abRndB + 4, 4);
                memcpy(ctx->session_key + 16, ctx->session_key, 8);

                Desfire_InitCrypto3Des(SPROX_PARAM_P  ctx->session_key, ctx->session_key+8, NULL);
                */

                /*session_key*/
                SesAuthENCKey = new byte[16];
                Array.ConstrainedCopy(abRndA, 0, /*session_key*/SesAuthENCKey, 0, 4);
                Array.ConstrainedCopy(abRndB, 0, /*session_key*/SesAuthENCKey, 4, 4);
                Array.ConstrainedCopy(abRndA, 4, /*session_key*/SesAuthENCKey, 8, 4);
                Array.ConstrainedCopy(abRndB, 4, /*session_key*/SesAuthENCKey, 12, 4);
                logger.trace("Session key={0}", BinConvert.ToHex(SesAuthENCKey));
                SetKey(/*session_key*/SesAuthENCKey);
            }


            /* Authenticate succeeded, therefore we remember the number of the key which was used
               to obtain the current authentication status. */
            session_key_id = bKeyNumber;

            /* Reset the init vector */
            CleanupInitVector();

            /* Success. */
            return DF_OPERATION_OK;
        }


        /**f* DesfireAPI/AuthenticateIso
         *
         * NAME
         *   AuthenticateIso
         *
         * DESCRIPTION
         *   Perform authentication using the specified 3DES key on the currently
         *   selected DESFIRE application.
         *   The generated session key is afterwards used for ISO ciphering or CMACing.
         *   This function is not available on DESFIRE EV0 cards.
         *
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_AuthenticateIso(byte bKeyNumber,
         *                                       const byte pbAccessKey[16]);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_AuthenticateIso(SPROX_INSTANCE rInst,
         *                                        byte bKeyNumber,
         *                                        const byte pbAccessKey[16]);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_AuthenticateIso(SCARDHANDLE hCard,
         *                                      byte bKeyNumber,
         *                                      const byte pbAccessKey[16]);
         *
         * INPUTS
         *   byte bKeyNumber             : number of the key (KeyNo)
         *   const byte pbAccessKey[16]  : 16-byte Access Key (DES/3DES2K keys)
         *
         * RETURNS
         *   DF_OPERATION_OK    : authentication succeed
         *   Other code if internal or communication error has occured. 
         *
         * NOTES
         *   Both DES and 3DES keys are stored in strings consisting of 16 bytes :
         *   - If the 2nd half of the key string is equal to the 1st half, the 
         *   64-bit key is handled as a single DES key by the DESFIRE card
         *   (well, actually there are only 56 significant bits).
         *   - If the 2nd half of the key string is NOT equal to the 1st half, the
         *   key is a 128 bit 3DES key
         *   (well, actually there are only 112 significant bits).
         *
         * SEE ALSO
         *   Authenticate
         *   AuthenticateIso24
         *   AuthenticateAes
         *   ChangeKeySettings
         *   GetKeySettings
         *   ChangeKey
         *   GetKeyVersion
         *
         **/
        public long AuthenticateIso(byte bKeyNumber, byte[] pbAccessKey)
        {
            byte[] bKeyBuffer24 = new byte[24];
            if (pbAccessKey == null)
                return DFCARD_LIB_CALL_ERROR;

            if (pbAccessKey.Length != 16)
                return DFCARD_LIB_CALL_ERROR;

            Array.ConstrainedCopy(pbAccessKey, 0, bKeyBuffer24, 0, 16);
            Array.ConstrainedCopy(pbAccessKey, 0, bKeyBuffer24, 16, 8);

            return AuthenticateIso24(bKeyNumber, bKeyBuffer24);
        }

        /**f* DesfireAPI/AuthenticateIso24
         *
         * NAME
         *   AuthenticateIso24
         *
         * DESCRIPTION
         *   Perform authentication using the specified 3DES key on the currently
         *   selected DESFIRE application.
         *   The generated session key is afterwards used for ISO ciphering or CMACing.
         *   This function is not available on DESFIRE EV0 cards.
         *
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_AuthenticateIso24(byte bKeyNumber,
         *                                         const byte pbAccessKey[24]);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_AuthenticateIso24(SPROX_INSTANCE rInst,
         *                                          byte bKeyNumber,
         *                                          const byte pbAccessKey[24]);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_AuthenticateIso24(SCARDHANDLE hCard,
         *                                        byte bKeyNumber,
         *                                        const byte pbAccessKey[24]);
         *
         * INPUTS
         *   byte bKeyNumber             : number of the key (KeyNo)
         *   const byte pbAccessKey[24]  : 24-byte Access Key (DES/3DES2K/3DES3K keys)
         *
         * RETURNS
         *   DF_OPERATION_OK    : authentication succeed
         *   Other code if internal or communication error has occured. 
         *
         * NOTES
         *   Both DES and 3DES keys are stored in strings consisting of 24 bytes :
         *   - If the 2nd third of the key string is equal to the 1st third, the 
         *   64-bit key is handled as a single DES key by the DESFIRE card
         *   (well, actually there are only 56 significant bits).
         *   - If the 2nd third of the key string is NOT equal to the 1st third AND
         *   the 3rd third is equal to the 1st third, the key is a 128 bit 3DES key
         *   (well, actually there are only 112 significant bits).
         *   - Overwise, the key is a 192 bit 3DES key "3DES3K mode" (well, actually
         *   (well, actually there are only 168 significant bits).
         *
         * SEE ALSO
         *   Authenticate
         *   AuthenticateIso
         *   AuthenticateAes
         *   ChangeKeySettings
         *   GetKeySettings
         *   ChangeKey
         *   GetKeyVersion
         *
         **/
        public long AuthenticateIso24(byte bKeyNumber, byte[] pbAccessKey)
        {
            byte rnd_size = 0;
            long status;
            UInt32 t;
            byte[] abRndB = new byte[16];
            byte[] abRndA = new byte[16];

            if (pbAccessKey == null)
                return DFCARD_LIB_CALL_ERROR;

            /* Each new Authenticate must invalidate the current authentication status. */
            CleanupAuthentication();

            /* Create the command string consisting of the command byte and the parameter byte. */
            xfer_buffer[INF + 0] = DF_AUTHENTICATE_ISO;
            xfer_buffer[INF + 1] = bKeyNumber;
            xfer_length = 2;

            /* Send the command string to the PICC and get its response (1st frame exchange).
               The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
            status = Command(0, WANTS_ADDITIONAL_FRAME);
            if (status != DF_OPERATION_OK)
                return status;

            /* Check the number of bytes received, we expect 9 or 17 bytes. */
            if (xfer_length == 9)
            {
                /* This is a 3DES2K (or a DES) */
                rnd_size = 8;
                bool are_equal = true;
                for (int i = 0; i < 8; i++)
                    if (pbAccessKey[i] != pbAccessKey[i + 8])
                        are_equal = false;

                if (are_equal)
                {
                    session_type = KEY_ISO_DES;
                    SetKey(pbAccessKey);
                    //InitCrypto3Des(pbAccessKey, pbAccessKey+8, null);
                }
                else
                {
                    session_type = KEY_ISO_3DES2K;
                    SetKey(pbAccessKey);
                    //InitCrypto3Des(pbAccessKey, null, null);
                }

            }
            else
            if (xfer_length == 17)
            {
                /* This is a 3DES3K */
                rnd_size = 16;
                session_type = KEY_ISO_3DES3K;
                SetKey(pbAccessKey);
                //InitCrypto3Des(pbAccessKey, pbAccessKey+8, pbAccessKey+16);
            }
            else
            {
                /* Error: block with inappropriate number of bytes received from the card. */
                return DFCARD_WRONG_LENGTH;
            }

            /* OK, we received the cryptogram Ek( RndB ) from the PICC.
               Decipher Ek( RndB ) to get RndB in xfer_buffer.
               Note that the status code has already been extracted from the queue. */
            t = rnd_size;
            byte[] tmp = new byte[rnd_size];
            Array.ConstrainedCopy(xfer_buffer, INF + 1, tmp, 0, rnd_size);
            CipherRecv(ref tmp, ref t);

            /* Store this RndB (is needed later on for generating the session key). */
            Array.ConstrainedCopy(tmp, 0, abRndB, 0, rnd_size);
            /*
            {
              Console.WriteLine("abRndB: ");
              for (int k=0; k<rnd_size; k++)
                Console.Write(String.Format("{0:x02}", abRndB[k]));
              Console.Write("\n");
            }
            */

            /* Now the PCD has to generate RndA. */
            //GetRandombytes(SPROX_PARAM_P  abRndA, rnd_size);
            abRndA = random.Get(rnd_size); /* MBA: 3K3DES use 16 bytes long random */

            /*
            {
              Console.WriteLine("abRndA: ");
              for (int k=0; k<abRndA.Length; k++)
                Console.Write(String.Format("{0:x02}", abRndA[k]));
              Console.Write("\n");
            }
            */

            /* Start the second frame with a status byte indicating to the PICC that the Authenticate
               command is continued. */
            xfer_buffer[0] = DF_ADDITIONAL_FRAME;

            /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
              after the status byte. */
            Array.ConstrainedCopy(abRndA, 0, xfer_buffer, 1, rnd_size);
            Array.ConstrainedCopy(abRndB, 1, xfer_buffer, 1 + rnd_size, rnd_size - 1);

            xfer_buffer[1 + 2 * rnd_size - 1] = abRndB[0]; /* first byte move to last byte */
            xfer_length = (uint)(1 + 2 * rnd_size);

            /*
            {
              Console.WriteLine("xfer_buffer: ");
              for (int k=0; k<xfer_length; k++)
                Console.Write(String.Format("{0:x02}", xfer_buffer[k]));
              Console.Write("\n");
            }      
            */

            /* Apply the DES send operation to the argument bytes before sending the second frame
               to the PICC ( do not include the status byte in the DES operation ). */
            t = (uint)(2 * rnd_size);
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
            CipherSend(ref tmp, ref t, t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

            /* Send the 2nd frame to the PICC and get its response. */
            status = Command(0, WANTS_OPERATION_OK);
            if (status != DF_OPERATION_OK)
                return status;

            /* We should now have Ek( RndA' ) in our buffer.
               RndA' was made from RndA by the PICC by rotating the string one byte to the left.
               Decipher Ek( RndA' ) to get RndA' in xfer_buffer. */
            t = rnd_size;
            tmp = new byte[rnd_size];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
            CipherRecv(ref tmp, ref t);

            /*
            {
              Console.WriteLine("tmp: ");
              for (int k=0; k<t; k++)
                Console.Write(String.Format("{0:x02}", tmp[k]));
              Console.Write("\n");
            }
            */
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);



            /* Now we have RndA' in our buffer.
               We have to check whether it matches our local copy of RndA.
               If one of the subsequent comparisons fails, we do not trust the PICC and therefore
               abort the authentication procedure ( no session key is generated ). */

            /* First compare the bytes 1 to bRndLen-1 of RndA with the first bRndLen-1 bytes in the queue. */
            /*
            {
              Console.WriteLine("xfer_buffer: ");
              for (int k=0; k<t+1; k++)
                Console.Write(String.Format("{0:x02}", xfer_buffer[k]));
              Console.Write("\n");
            }
            */

            for (byte i = 1; i < rnd_size - 1; i++)
                if (xfer_buffer[i] != abRndA[i])
                    return DFCARD_WRONG_KEY;

            /* Then compare the leftmost byte of RndA with the last byte in the queue. */
            if (xfer_buffer[1 + rnd_size - 1] != abRndA[0])
                return DFCARD_WRONG_KEY;

            /* The actual authentication has succeeded.
               Finally we have to generate the session key from both random numbers RndA and RndB. */
            if (session_type == KEY_ISO_DES)
            {

                /*
                memcpy(session_key +  0, abRndA +  0, 4);
                memcpy(session_key +  4, abRndB +  0, 4);
                memcpy(session_key +  8, session_key, 8);
                memcpy(session_key + 16, session_key, 8);
                */

                /*session_key*/
                SesAuthENCKey = new byte[16];
                Array.ConstrainedCopy(abRndA, 0, /*session_key*/SesAuthENCKey, 0, 4);
                Array.ConstrainedCopy(abRndB, 0, /*session_key*/SesAuthENCKey, 4, 4);
                Array.ConstrainedCopy(/*session_key*/SesAuthENCKey, 0, /*session_key*/SesAuthENCKey, 8, 8);
                //Array.ConstrainedCopy(session_key, 0, session_key, 16, 8);

                //Console.WriteLine("KEY_ISO_DES");

                SetKey(/*session_key*/SesAuthENCKey);

                //InitCrypto3Des(SPROX_PARAM_P  session_key, null, null);

            }
            else
            if (session_type == KEY_ISO_3DES2K)
            {
                /*
                memcpy(session_key +  0, abRndA +  0, 4);
                memcpy(session_key +  4, abRndB +  0, 4);
                memcpy(session_key +  8, abRndA +  4, 4);
                memcpy(session_key + 12, abRndB +  4, 4);
                memcpy(session_key + 16, session_key, 8);
                */


                /*session_key*/
                SesAuthENCKey = new byte[16];
                Array.ConstrainedCopy(abRndA, 0, /*session_key*/SesAuthENCKey, 0, 4);
                Array.ConstrainedCopy(abRndB, 0, /*session_key*/SesAuthENCKey, 4, 4);
                Array.ConstrainedCopy(abRndA, 4, /*session_key*/SesAuthENCKey, 8, 4);
                Array.ConstrainedCopy(abRndB, 4, /*session_key*/SesAuthENCKey, 12, 4);
                //Array.ConstrainedCopy(session_key, 0, session_key, 16, 8); 

                //byte[] test_cmac_session_key = { 0x4c, 0xf1, 0x51, 0x34, 0xa2, 0x85, 0x0d, 0xd5, 0x8a, 0x3d, 0x10, 0xba, 0x80, 0x57, 0x0d, 0x38 } ;


                //Console.WriteLine("KEY_ISO_3DES2K");

                SetKey(/*session_key*/SesAuthENCKey);

                //InitCrypto3Des(session_key, session_key+8, null);

            }
            else
            if (session_type == KEY_ISO_3DES3K)
            {
                /*
                memcpy(session_key +  0, abRndA +  0, 4);
                memcpy(session_key +  4, abRndB +  0, 4);
                memcpy(session_key +  8, abRndA +  6, 4);
                memcpy(session_key + 12, abRndB +  6, 4);
                memcpy(session_key + 16, abRndA + 12, 4);
                memcpy(session_key + 20, abRndB + 12, 4);
                */

                Array.ConstrainedCopy(abRndA, 0, /*session_key*/SesAuthENCKey, 0, 4);
                Array.ConstrainedCopy(abRndB, 0, /*session_key*/SesAuthENCKey, 4, 4);
                Array.ConstrainedCopy(abRndA, 6, /*session_key*/SesAuthENCKey, 8, 4);
                Array.ConstrainedCopy(abRndB, 6, /*session_key*/SesAuthENCKey, 12, 4);
                Array.ConstrainedCopy(abRndA, 12, /*session_key*/SesAuthENCKey, 16, 4);
                Array.ConstrainedCopy(abRndB, 12, /*session_key*/SesAuthENCKey, 20, 4);

                //Console.WriteLine("KEY_ISO_3DES3K");

                SetKey(/*session_key*/SesAuthENCKey);

                //Desfire_InitCrypto3Des(session_key, session_key+8, session_key+16);
            }

            /* Authenticate succeeded, therefore we remember the number of the key which was used
               to obtain the current authentication status. */
            session_key_id = bKeyNumber;

            /* Reset the init vector */
            CleanupInitVector();

            /* Initialize the CMAC calculator */
            InitCmac();

            /* Success. */
            return DF_OPERATION_OK;

        }

        public class BenchResult
        {
            public UInt16 BlockExchanged;
            public UInt32 Duration;

            public BenchResult()
            {
                BlockExchanged = 0;
                Duration = 0;
            }
        }

        public BenchResult[] BenchAuthCmds = new BenchResult[2];

        private long retieveStats(out BenchResult result)
        {
            result = new BenchResult();
            if (!ActivateBenchAuth)
            {
                return SCARD_E_CARD_UNSUPPORTED;
            }

            byte[] send_buffer = new byte[] { 0xFF, 0xFB, 0x21, 0x00 };
            ushort recv_length = 0;
            Logger.Debug("<< " + BinConvert.ToHex(send_buffer));
            byte[] recv_buffer = transmitter.Transmit(send_buffer);
            if (recv_buffer != null)
            {
                Logger.Debug(">> " + BinConvert.ToHex(recv_buffer));
                recv_length = (ushort)recv_buffer.Length;
            }
            else
            {
                Logger.Debug(">> ERROR");
                return -1;
            }

            UInt16 sw;
            if (recv_length < 2)
                return DFCARD_PCSC_BAD_RESP_LEN;

            sw = (ushort)((recv_buffer[recv_length - 2] * 0x0100) | recv_buffer[recv_length - 1]);

            /* SW must be 9000 */
            switch (sw)
            {
                case 0x9000:
                    break;
                case 0x6F01:
                case 0x6F3D:
                case 0x6F51:
                case 0x6F52:
                    return SCARD_W_REMOVED_CARD;
                case 0x6F02:
                case 0x6F3E:
                    return SCARD_E_COMM_DATA_LOST;
                case 0x6F47:
                    return SCARD_E_CARD_UNSUPPORTED;
                default: return DFCARD_PCSC_BAD_RESP_SW;
            }
            if (recv_length != 8)
                return DFCARD_WRONG_LENGTH;

            byte[] blockExchanged = new byte[2];
            Array.Copy(recv_buffer, 0, blockExchanged, 0, 2);
            result.BlockExchanged = BinUtils.ToWord(blockExchanged);
            byte[] duration = new byte[4];
            Array.Copy(recv_buffer, 2, duration, 0, 4);
            result.Duration = BinUtils.ToDword(duration);

            return DF_OPERATION_OK;
        }

        /**f* DesfireAPI/AuthenticateAes
         *
         * NAME
         *   AuthenticateAes
         *
         * DESCRIPTION
         *   Perform authentication using the specified AES key on the currently
         *   selected DESFIRE application.
         *   This function is not available on DESFIRE EV0 cards.
         *   The generated session key is afterwards used for ISO ciphering or CMACing.
         *
         * SYNOPSIS
         *
         *   [[sprox_desfire.dll]]
         *   SUInt16 SPROX_Desfire_AuthenticateAes(byte bKeyNumber,
         *                                       const byte pbAccessKey[16]);
         *
         *   [[sprox_desfire_ex.dll]]
         *   SUInt16 SPROXx_Desfire_AuthenticateAes(SPROX_INSTANCE rInst,
         *                                        byte bKeyNumber,
         *                                        const byte pbAccessKey[16]);
         *
         *   [[pcsc_desfire.dll]]
         *   LONG  SCardDesfire_AuthenticateAes(SCARDHANDLE hCard,
         *                                      byte bKeyNumber,
         *                                      const byte pbAccessKey[16]);
         *
         * INPUTS
         *   byte bKeyNumber             : number of the key (KeyNo)
         *   const byte pbAccessKey[16]  : 16-byte Access Key (AES)
         *
         * RETURNS
         *   DF_OPERATION_OK    : authentication succeed
         *   Other code if internal or communication error has occured. 
         *
         * NOTES
         *   AES keys are always 128-bit long.
         *
         * SEE ALSO
         *   Authenticate
         *   AuthenticateIso24
         *   AuthenticateIso
         *   ChangeKeySettings
         *   GetKeySettings
         *   ChangeKey
         *   GetKeyVersion
         *
         **/
        public long AuthenticateAes(byte bKeyNumber, byte[] pbAccessKey)
        {
            long status;
            UInt32 t;
            byte[] abRndB = new byte[16];
            byte[] abRndA = new byte[16];

            if (pbAccessKey == null)
                return DFCARD_LIB_CALL_ERROR;

            logger.trace("AuthenticateAes, key number={0:X02}, key value={1}", bKeyNumber, BinConvert.ToHex(pbAccessKey));

            /* Each new Authenticate must invalidate the current authentication status. */
            CleanupAuthentication();

            /* Initialize the cipher unit with the authentication key */
            session_type = KEY_ISO_AES;
            SetKey(pbAccessKey);//InitCryptoAes(pbAccessKey);

            /* Create the command string consisting of the command byte and the parameter byte. */
            xfer_buffer[INF + 0] = DF_AUTHENTICATE_AES;
            xfer_buffer[INF + 1] = bKeyNumber;
            xfer_length = 2;

            /* Send the command string to the PICC and get its response (1st frame exchange).
               The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
            status = Command(0, WANTS_ADDITIONAL_FRAME);
            if (status != DF_OPERATION_OK)
                return status;

            retieveStats(out BenchAuthCmds[0]);

            /* Check the number of bytes received, we expect 17 bytes. */
            if (xfer_length != 17)
            {
                /* Error: block with inappropriate number of bytes received from the card. */
                return DFCARD_WRONG_LENGTH;
            }


            /* OK, we received the cryptogram Ek( RndB ) from the PICC.
               Decipher Ek( RndB ) to get RndB in xfer_buffer.
               Note that the status code has already been extracted from the queue. */
            t = 16;
            byte[] tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, INF + 1, tmp, 0, (int)t);
            logger.debug("\tEk(RndB)={0}", BinConvert.ToHex(tmp));
            CipherRecv(ref tmp, ref t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, INF + 1, (int)t);

            /* Store this RndB (is needed later on for generating the session key). */
            Array.ConstrainedCopy(xfer_buffer, INF + 1, abRndB, 0, 16);
            logger.debug("\tRndB={0}", BinConvert.ToHex(abRndB));

            /* Now the PCD has to generate RndA. */
            //GetRandombytes(SPROX_PARAM_P  abRndA, 16);
            abRndA = random.Get(16);
            logger.debug("\tRndA={0}", BinConvert.ToHex(abRndA));

            /* Start the second frame with a status byte indicating to the PICC that the Authenticate
               command is continued. */
            xfer_buffer[INF + 0] = DF_ADDITIONAL_FRAME;

            /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
               after the status byte. */
            Array.ConstrainedCopy(abRndA, 0, xfer_buffer, INF + 1, 16);
            Array.ConstrainedCopy(abRndB, 1, xfer_buffer, INF + 1 + 16, 15);
            xfer_buffer[INF + 1 + 31] = abRndB[0]; /* first byte move to last byte */
            xfer_length = 1 + 32;

            /* Apply the DES send operation to the argument bytes before sending the second frame
               to the PICC ( do not include the status byte in the DES operation ). */
            t = 32;
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
            logger.debug("\tRndA||RndB'={0}", BinConvert.ToHex(tmp));
            CipherSend(ref tmp, ref t, t);
            logger.debug("\tEk(RndA||RndB')={0}", BinConvert.ToHex(tmp));
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

            /* Send the 2nd frame to the PICC and get its response. */
            status = Command(0, WANTS_OPERATION_OK);
            if (status != DF_OPERATION_OK)
                return status;

            retieveStats(out BenchAuthCmds[1]);

            /* We should now have Ek( RndA' ) in our buffer.
               RndA' was made from RndA by the PICC by rotating the string one byte to the left.
               Decipher Ek( RndA' ) to get RndA' in xfer_buffer. */
            t = 16;
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
            logger.debug("\tEk(RndA')={0}", BinConvert.ToHex(tmp));
            CipherRecv(ref tmp, ref t);
            logger.debug("\tRndA'={0}", BinConvert.ToHex(tmp));
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

            /* Now we have RndA' in our buffer.
               We have to check whether it matches our local copy of RndA.
               If one of the subsequent comparisons fails, we do not trust the PICC and therefore
               abort the authentication procedure ( no session key is generated ). */

            /* First compare the bytes 1 to bRndLen-1 of RndA with the first bRndLen-1 bytes in the queue. */
            for (int k = 0; k < 15; k++)
                if (xfer_buffer[INF + 1 + k] != abRndA[1 + k])
                    return DFCARD_WRONG_KEY;


            /* Then compare the leftmost byte of RndA with the last byte in the queue. */
            if (xfer_buffer[INF + 1 + 15] != abRndA[0])
                return DFCARD_WRONG_KEY;


            /* The actual authentication has succeeded.
               Finally we have to generate the session key from both random numbers RndA and RndB. */
            /*
            memcpy(session_key +  0, abRndA +  0, 4);
            memcpy(session_key +  4, abRndB +  0, 4);
            memcpy(session_key +  8, abRndA + 12, 4);
            memcpy(session_key + 12, abRndB + 12, 4);
            memset(session_key + 16, 0, 8);
            */
            /*session_key*/
            SesAuthENCKey = new byte[16];
            Array.ConstrainedCopy(abRndA, 0, /*session_key*/SesAuthENCKey, 0, 4);
            Array.ConstrainedCopy(abRndB, 0, /*session_key*/SesAuthENCKey, 4, 4);
            Array.ConstrainedCopy(abRndA, 12, /*session_key*/SesAuthENCKey, 8, 4);
            Array.ConstrainedCopy(abRndB, 12, /*session_key*/SesAuthENCKey, 12, 4);

            logger.trace("SessionEncKey={0}", BinConvert.ToHex(SesAuthENCKey));

            /* Initialize the cipher unit with the session key */
            session_type = KEY_ISO_AES;
            SetKey(/*session_key*/SesAuthENCKey);
            //Desfire_InitCryptoAes(SPROX_PARAM_P  session_key);

            /* Authenticate succeeded, therefore we remember the number of the key which was used
               to obtain the current authentication status. */
            session_key_id = bKeyNumber;

            /* Reset the init vector */
            CleanupInitVector();

            /* Initialize the CMAC calculator */
            InitCmac();

            /* Success. */
            return DF_OPERATION_OK;

        }

        void CleanupAuthentication()
        {
            session_key_id = -1;
            session_type = KEY_EMPTY;
            init_vector = new byte[16];
            for (int i = 0; i < init_vector.Length; i++)
                init_vector[i] = 0;


            secure_mode = SecureMode.EV0;

        }
        /// <summary>
        /// IV for CmdData = E(KSesAuthENC; 0xA5 || 0x5A || TI || CmdCtr || 0x0000000000000000)
        /// EBC mode of NIST SP800-38A
        /// </summary>
        /// <param name="cipher">header is different between cipher and decipher</param>
        /// 
        void RefreshVectorEv2(bool cipher = true)
        {
            byte[] plain_text = new byte[16];

            for (int i = 0; i < init_vector.Length; i++)
                init_vector[i] = 0;

            if (cipher == true)
            {
                plain_text[0] = 0xA5;
                plain_text[1] = 0x5A;
            }
            else
            {
                plain_text[0] = 0x5A;
                plain_text[1] = 0xA5;
            }

            for (int i = 2; i < 6; i++)
                plain_text[i] = TransactionIdentifier[i - 2];

            /* CmdCtr LSB first */
            plain_text[6] = (byte)(CmdCtr & 0x00FF);
            plain_text[7] = (byte)((CmdCtr >> 8) & 0x00FF);
#if _VERBOSE
      Console.WriteLine("InitVectorEv2 Use CmdCtr {0} ", CmdCtr);
#endif


            for (int i = 8; i < 16; i++)
                plain_text[i] = 0x00;


#if _VERBOSE
      Console.WriteLine("=================================================================");
      Console.WriteLine("InitVectorEv2 Plain " + BinConvert.ToHex(plain_text, 16));
#endif

            byte[] IV = DesfireCrypto.AES_Encrypt_ECB(plain_text, SesAuthENCKey, init_vector);

            for (int i = 0; i < IV.Length; i++)
                init_vector[i] = IV[i];

#if _VERBOSE
      Console.WriteLine("InitVectorEv2 Key " + BinConvert.ToHex(SesAuthENCKey, SesAuthENCKey.Length));
      Console.WriteLine("InitVectorEv2 Cipher " + BinConvert.ToHex(init_vector, IV.Length));

#endif


        }
        /// <summary>
        /// Decipher received data, CBC mode of NIST SP800-38A
        /// </summary>
        /// <param name="cipher_key">key to use for deciphering</param>
        /// <param name="data">deciphered data</param>
        /// <param name="length">length of deciphered data</param>
        void DeCipherSP80038A(byte[] cipher_key, ref byte[] data, ref UInt32 length, bool random_iv = false)
        {
            byte[] buffer;
            byte[] dummy_iv = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            UInt16 block_count;
            UInt32 i, j;      

            if (data == null)
                return;

            if ((length % 16) != 0)
                return;

            buffer = new byte[16];
            block_count = (UInt16)(length / 16);


            if (random_iv == false)
            {
                for (i = 0; i < init_vector.Length; i++)
                    init_vector[i] = 0;
            }
            else
            {
                /* refresh IV to take in account CmdCtr */
                RefreshVectorEv2(false);
                //init_vector = new byte[16] { 0xC9, 0x13, 0xB4, 0x53, 0x11, 0xA4, 0x99, 0x59, 0xDF, 0x94, 0x60, 0x12, 0x6D, 0xFD, 0x52, 0x7F };
            }

#if _VERBOSE
      Console.WriteLine("DeCipherSP80038A " + BinConvert.ToHex(data, length));
#endif

            for (i = 0; i < block_count; i++)
            {
                //1.
                Array.ConstrainedCopy(data, (int)(16 * i), buffer, 0, 16);   // B  <- P

                //2.
                byte[] deciphered = DesfireCrypto.AES_Decrypt(buffer, cipher_key, dummy_iv);
                Array.ConstrainedCopy(deciphered, 0, data, (int)(16 * i), 16);

                //3. 
                for (j = 0; j < 16; j++)
                    data[16 * i + j] ^= (byte)(init_vector[j]);  // P  <- P XOR IV           

                //4. 
                Array.ConstrainedCopy(buffer, 0, init_vector, 0, 16);// IV <- B 
            }

#if _VERBOSE
      Console.WriteLine("DeCipherSP80038A " + BinConvert.ToHex(data, length));
#endif
        }



        /// <summary>
        /// CBC mode of NIST SP800-38A
        /// </summary>
        /// <param name="data">data to cipher</param>
        /// <param name="length"></param>
        /// <param name="max_length"></param>
        void CipherSP80038A(byte[] input, int offset, uint length, uint max_length, ref byte[] output, int offset_out, ref uint output_length)
        {
            uint actual_length;
            uint block_size;
            uint block_count;
            uint i, j;
            byte[] dummy_iv = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };


            if (input == null)
                return;

            if ((length - offset) <= 0)
                return;

            byte[] raw_data = new byte[length - offset];

            Array.ConstrainedCopy(input, offset, raw_data, 0, raw_data.Length);

#if _VERBOSE
      Console.WriteLine("CipherSP80038A input  " + BinConvert.ToHex(input, offset, (int) length));
      Console.WriteLine("CipherSP80038A raw_data " + BinConvert.ToHex(raw_data, raw_data.Length));
#endif
            /* refresh IV to take in account CmdCtr */
            RefreshVectorEv2();

            actual_length = (uint)raw_data.Length;


            /* Step 1 : padding */
            /* adding always 0x80 followed, if required, by zero bytes until a string with a length of a multiple of
             * 16 byte is obtained.There are no exceptions. Note that if the plain data is a multiple of 16 
             * bytes already, an additional padding block is added.
             */
            block_size = 16;

            if ((actual_length % block_size) == 0)
            {
                actual_length++;
            }

            while ((actual_length % block_size) != 0)
            {
                /*if (actual_length >= max_length)
                  return;*/
                actual_length++;
            }

            byte[] data = new byte[actual_length];

            Array.ConstrainedCopy(raw_data, 0, data, 0, raw_data.Length);
            data[raw_data.Length] = 0x80;

#if _VERBOSE
      Console.WriteLine("CipherSP80038A padding  " + BinConvert.ToHex(data, data.Length));
#endif

            block_count = (actual_length / block_size);


            /* Keep last IV */
            for (i = 0; i < block_count; i++)
            {

                /* P  <- P XOR IV for first */
                /* P  <- P XOR P(n-1) for other */
                for (j = 0; j < 16; j++)
                    data[16 * i + j] ^= (byte)(init_vector[j]);

                //2.
                byte[] tmp = new byte[block_size];
                Array.ConstrainedCopy(data, (int)(16 * i), tmp, 0, (int)block_size);

                //3. P  <- IV  
                init_vector = DesfireCrypto.AES_Encrypt(tmp, SesAuthENCKey, dummy_iv);
                Array.ConstrainedCopy(init_vector, 0, data, (int)(16 * i), 16);

            }
            length = actual_length;

            if (length >= max_length)
                return;

            Array.ConstrainedCopy(data, 0, output, offset_out, (int)length);
            output_length = (uint)(length + offset_out);

#if _VERBOSE
      Console.WriteLine("CipherSP80038A output  " + BinConvert.ToHex(output, output_length));
#endif
        }
        /// <summary>
        /// NSIST Special Publication 800-38B
        /// </summary>
        /// <param name="cipher_key">cipher key</param>
        /// <param name="cipher_IV">init vector used to cipher</param>
        /// <param name="xor_last_block">last block used for last xor</param>
        /// <param name="data">data to cipher</param>
        /// <param name="length"></param>
        /// <param name="max_length"></param>
        void CipherSP80038B(byte[] cipher_key, byte[] cipher_IV, byte[] xor_last_block, ref byte[] data, ref UInt32 length, UInt32 max_length)
        {
            UInt32 actual_length;
            UInt32 block_size;
            UInt32 block_count;
            UInt32 i, j;
            /* dummy and init vector used */
            byte[] dummy_iv = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};


            if (data == null)
                return;

            if (cipher_IV == null)
                return;

            actual_length = length;

            /* Step 1 : padding */
            /* 
             * during the authentication itself(Cmd.AuthenticateEV2First and Cmd.AuthenticateEV2NonFirst),
             * where no padding is applied at all.
             */
            block_size = 16;
            while ((actual_length % block_size) != 0)
            {
                if (actual_length >= max_length)
                    return;
                data[actual_length++] = 0x00;
            }

            block_count = (actual_length / block_size);

            /* Keep last IV */
            for (i = 0; i < block_count; i++)
            {

#if _VERBOSE
    Console.WriteLine(string.Format("IN Block {0} {1} ", i, BinConvert.ToHex(data, (int)(16 * i), 16)));
    Console.WriteLine(string.Format("CIPHER Block {0} {1} ", i, BinConvert.ToHex(cipher_IV, 0, 16)));
#endif
                /* P  <- P XOR IV for first */
                /* P  <- P XOR P(n-1) for other */
                for (j = 0; j < 16; j++)
                    data[16 * i + j] ^= (byte)(cipher_IV[j]);

                //2.
                byte[] tmp = new byte[block_size];
                Array.ConstrainedCopy(data, (int)(16 * i), tmp, 0, (int)block_size);

#if _VERBOSE
    Console.WriteLine(string.Format("XORED Block {0} {1} ", i, BinConvert.ToHex(tmp, 16)));
#endif

                if ((xor_last_block != null) && (i == (block_count - 1)))
                {
                    for (j = 0; j < 16; j++)
                        tmp[j] ^= (byte)(xor_last_block[j]);
#if _VERBOSE
    Console.WriteLine("SUBMAC " + BinConvert.ToHex(tmp, 16));
#endif
                }

                //3. P  <- IV  
                cipher_IV = DesfireCrypto.AES_Encrypt(tmp, cipher_key,  dummy_iv);
                Array.ConstrainedCopy(cipher_IV, 0, data, (int)(16 * i), 16);

#if _VERBOSE
    Console.WriteLine(string.Format("OUT Block {0} {1} ", i, BinConvert.ToHex(cipher_IV, 16)));
#endif
            }

            length = actual_length;

#if _VERBOSE
    Console.WriteLine(string.Format("Cipher result {0} {1} ", i, BinConvert.ToHex(data, length)));
#endif
        }


        /**f* DesfireAPI/AuthenticateEv2First
        *
        * NAME
        *   AuthenticateEv2First
        *
        * DESCRIPTION
        *   Perform authentication using the specified AES key on the currently
        *   selected DESFIRE application.
        *   This function is not available on DESFIRE EV0 cards.
        *   The generated session key is afterwards used for ISO ciphering or CMACing.
        *
        * SYNOPSIS
        *
        *   [[sprox_desfire.dll]]
        *   SUInt16 SPROX_Desfire_AuthenticateEv2First(byte bKeyNumber,
        *                                       const byte pbAccessKey[16],
        *                                      byte LenCaps,
        *                                      byte[] PCDCaps2);
        *
        *   [[sprox_desfire_ex.dll]]
        *   SUInt16 SPROXx_Desfire_AuthenticateEv2First(SPROX_INSTANCE rInst,
        *                                        byte bKeyNumber,
        *                                        const byte pbAccessKey[16]),
        *                                      byte LenCaps,
        *                                      byte[] PCDCaps2);
        *                                      
        *   [[pcsc_desfire.dll]]
        *   LONG  SCardDesfire_AuthenticateEv2First(SCARDHANDLE hCard,
        *                                      byte bKeyNumber,
        *                                      const byte pbAccessKey[16],
        *                                      byte LenCaps,
        *                                      byte[] PCDCaps2);
        *
        * INPUTS
        *   byte bKeyNumber             : number of the key (KeyNo)
        *   const byte pbAccessKey[16]  : 16-byte Access Key (AES)
        *
        * RETURNS
        *   DF_OPERATION_OK    : authentication succeed
        *   Other code if internal or communication error has occured. 
        *
        * NOTES
        *   AES keys are always 128-bit long.
        *
        * SEE ALSO
        *   Authenticate
        *   AuthenticateIso24
        *   AuthenticateIso
        *   ChangeKeySettings
        *   GetKeySettings
        *   ChangeKey
        *   GetKeyVersion
        *
        **/
        public long AuthenticateEv2First(byte bKeyNumber, byte[] pbAccessKey, byte LenCaps, byte[] PCDCaps2)
        {
            long status;
            UInt32 t;
            byte[] abRndB = new byte[16];
            byte[] abRndA = new byte[16];
            byte[] IV = new byte[16];

            if (pbAccessKey == null)
                return DFCARD_LIB_CALL_ERROR;

            /* Each new Authenticate must invalidate the current authentication status. */
            /*
             * For the encryption during authentication(both Cmd.AuthenticateEV2First and
             * Cmd.AuthenticateEV2NonFirst), the IV will be 128 bits of 0.
             */
            CleanupAuthentication();

            /* Initialize the cipher unit with the authentication key */
            session_type = KEY_ISO_AES;
            SetKey(pbAccessKey);

            /* Create the command string consisting of the command byte and the parameter byte. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_AUTHENTICATE_EV2_FIRST;
            xfer_buffer[xfer_length++] = bKeyNumber;
            xfer_buffer[xfer_length++] = LenCaps;

            if (LenCaps != 0)
            {
                Array.ConstrainedCopy(PCDCaps2, 0, xfer_buffer, (int)xfer_length, LenCaps);
                xfer_length += LenCaps;
            }


            /* Send the command string to the PICC and get its response (1st frame exchange).
               The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
            status = Command(0, WANTS_ADDITIONAL_FRAME, false);
            if (status != DF_OPERATION_OK)
                return status;

            /* Check the number of bytes received, we expect 17 bytes. */
            if (xfer_length != 17)
            {
                /* Error: block with inappropriate number of bytes received from the card. */
                return DFCARD_WRONG_LENGTH;
            }

            /* OK, we received the cryptogram E(Kx, RndB) from the PICC.
               Decipher E(Kx, RndB) to get RndB in xfer_buffer.
               Note that the status code has already been extracted from the queue. */

            t = 16;
            byte[] tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, INF + 1, tmp, 0, (int)t);
            DeCipherSP80038A(SesAuthENCKey, ref tmp, ref t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, INF + 1, (int)t);

            /* Store this RndB (is needed later on for generating the session key). */
            Array.ConstrainedCopy(xfer_buffer, INF + 1, abRndB, 0, 16);

#if _VERBOSE
      Console.WriteLine("abRndB " + BinConvert.ToHex(abRndB, 0, 16));
#endif

            /* Now the PCD has to generate RndA. */
            abRndA = random.Get(16);

#if _VERBOSE
      Console.WriteLine("abRndA " + BinConvert.ToHex(abRndA, 0, 16));
#endif

            /* Start the second frame with a status byte indicating to the PICC that the Authenticate
               command is continued. E(Kx, RndA || RndB’)*/
            xfer_buffer[INF + 0] = DF_ADDITIONAL_FRAME;

            /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
               after the status byte.  */
            Array.ConstrainedCopy(abRndA, 0, xfer_buffer, INF + 1, 16);
            Array.ConstrainedCopy(abRndB, 1, xfer_buffer, INF + 1 + 16, 15);
            xfer_buffer[INF + 1 + 31] = abRndB[0]; /* first byte move to last byte */
            xfer_length = 1 + 32;

            //Console.WriteLine("Xfer(clear) > " + BinConvert.ToHex(xfer_buffer, 33));

            /* Apply the DES send operation to the argument bytes before sending the second frame
               to the PICC ( do not include the status byte in the DES operation ). */
            t = 32;
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);

            CipherSP80038B(SesAuthENCKey, IV, null, ref tmp, ref t, t);

            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

            /* Send the 2nd frame to the PICC and get its response. */
            status = Command(0, WANTS_OPERATION_OK, false);
            if (status != DF_OPERATION_OK)
                return status;

            /* We should now have E(Kx, TI || RndA’ || PDcap2 || PCDcap2) in our buffer.
               RndA' was made from RndA by the PICC by rotating the string one byte to the left.
               Decipher E(Kx, TI || RndA’ || PDcap2 || PCDcap2) to get RndA' in xfer_buffer. */
            /* TI : 4 bytes,  RndA’ 16 bytes, PDcap2 6 bytes, PCDcap2 6 bytes) */

            /* copie RndA' */
            t = 32;
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
            DeCipherSP80038A(SesAuthENCKey, ref tmp, ref t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

#if _VERBOSE
      Console.WriteLine("TI " + BinConvert.ToHex(xfer_buffer, 1, 4));
#endif
            /* Now we have TI || RndA’ || PDcap2 || PCDcap2 in our buffer.
               We have to check whether it matches our local copy of RndA.
               If one of the subsequent comparisons fails, we do not trust the PICC and therefore
               abort the authentication procedure ( no session key is generated ). */

            TransactionIdentifier = new byte[4];
            Array.ConstrainedCopy(xfer_buffer, INF + 1, TransactionIdentifier, 0, 4);

            /* init command counter like the PICC, LSB first */
            CmdCtr = 0x0000;
#if _VERBOSE
      Console.WriteLine("Init CmdCtr {0}", CmdCtr);
#endif

            /* Then compare the bytes 1 to bRndLen-1 of RndA with the first bRndLen-1 bytes in the queue. */
            for (int k = 0; k < 15; k++)
                if (xfer_buffer[INF + 5 + k] != abRndA[1 + k])
                    return DFCARD_WRONG_KEY;


            /* Then compare the leftmost byte of RndA with the last byte in the queue. */
            if (xfer_buffer[INF + 1 + 19] != abRndA[0])
                return DFCARD_WRONG_KEY;

            PDCaps2 = new byte[6];
            Array.ConstrainedCopy(xfer_buffer, INF + 20, PDCaps2, 0, 6);

            PCDCaps2 = new byte[6];
            Array.ConstrainedCopy(xfer_buffer, INF + 26, PCDCaps2, 0, 6);

            /* Desfire Ev2 */
            //SV1 = 0xA5 || 0x5A || 0x00 || 0x01 || 0x00 || 0x80 || RndA[15..14] || (RndA[13..8] ⊕ RndB[15..10])|| RndB[9..0] || RndA[7..0]
            byte[] SV1 = new byte[32];
            byte[] SV2 = new byte[32];
            byte[] rnda = new byte[6];
            byte[] rndb = new byte[6];


#if _VERBOSE
      Console.WriteLine("Key " + BinConvert.ToHex(pbAccessKey, 0, 16));
      Console.WriteLine("abRndA " + BinConvert.ToHex(abRndA, 0, 16));
      Console.WriteLine("abRndB " + BinConvert.ToHex(abRndB, 0, 16));
#endif

            Array.ConstrainedCopy(abRndA, 2, rnda, 0, 6);
            Array.ConstrainedCopy(abRndB, 0, rndb, 0, 6);

            for (int j = 0; j < 6; j++)
                rndb[j] ^= rnda[j];

            /* SV1 = 0xA5 || 0x5A || 0x00 || 0x01 || 0x00 || 0x80 || RndA[15..14] || (RndA[13..8] ⊕ RndB[15..10])|| RndB[9..0] || RndA[7..0] */
            SV1[0] = 0xA5; SV1[1] = 0x5A; SV1[2] = 0x00; SV1[3] = 0x01; SV1[4] = 0x00; SV1[5] = 0x80;

            /*RndA[15..14]*/
            Array.ConstrainedCopy(abRndA, 0, SV1, 6, 2);
            /*(RndA[13..8] ⊕ RndB[15..10])*/
            Array.ConstrainedCopy(rndb, 0, SV1, 8, 6);
            /* RndB[9..0] */
            Array.ConstrainedCopy(abRndB, 6, SV1, 14, 10);
            /*RndA[7..0] */
            Array.ConstrainedCopy(abRndA, 8, SV1, 24, 8);

            /* SV2 = 0x5A || 0xA5 || 0x00 || 0x01 || 0x00 || 0x80 || RndA[15..14] || (RndA[13..8] ⊕ RndB[15..10])|| RndB[9..0] || RndA[7..0] */
            SV2[0] = 0x5A; SV2[1] = 0xA5; SV2[2] = 0x00; SV2[3] = 0x01; SV2[4] = 0x00; SV2[5] = 0x80;
            Array.ConstrainedCopy(abRndA, 0, SV2, 6, 2);
            Array.ConstrainedCopy(rndb, 0, SV2, 8, 6);
            Array.ConstrainedCopy(abRndB, 6, SV2, 14, 10);
            Array.ConstrainedCopy(abRndA, 8, SV2, 24, 8);

#if _VERBOSE
      Console.WriteLine("SV1 " + BinConvert.ToHex(SV1, 0, 32));
      Console.WriteLine("SV2 " + BinConvert.ToHex(SV2, 0, 32));

#endif

            /* Initialize the cipher unit with the session key */
            session_type = KEY_ISO_AES;

            // 5. Calculate CMAC
            byte[] CMAC_IV = new byte[16];
            byte[] CMAC = new byte[16];
            byte[] ENC = new byte[16];

            CalculateCMACEV2(pbAccessKey, CMAC_IV, SV2, ref CMAC);
            CalculateCMACEV2(pbAccessKey, CMAC_IV, SV1, ref ENC);

            SetSesAuthMACKey(CMAC);
            SetSesAuthENCKey(ENC);
#if _VERBOSE
      Console.WriteLine("SessionMacKey " + BinConvert.ToHex(CMAC, 16));
      Console.WriteLine("SessionEnkKey " + BinConvert.ToHex(ENC, 16));
      Console.WriteLine("=================================================================");
#endif

            secure_mode = SecureMode.EV2;

            /* Authenticate succeeded, therefore we remember the number of the key which was used
               to obtain the current authentication status. */
            session_key_id = bKeyNumber;

            /* Reset the init vector */
            CleanupInitVector();

            /* Initialize the CMAC calculator */
            InitCmacEv2();

            /* Success. */
            return DF_OPERATION_OK;

        }
        /**f* DesfireAPI/AuthenticateEv2NonFirst
        *
        * NAME
        *   AuthenticateEv2First
        *
        * DESCRIPTION
        *   Perform authentication using the specified AES key on the currently
        *   selected DESFIRE application.
        *   This function is not available on DESFIRE EV0 cards.
        *   The generated session key is afterwards used for ISO ciphering or CMACing.
        *
        * SYNOPSIS
        *
        *   [[sprox_desfire.dll]]
        *   SUInt16 SPROX_Desfire_AuthenticateEv2NonFirst(byte bKeyNumber,
        *                                       const byte pbAccessKey[16]);
        *
        *   [[sprox_desfire_ex.dll]]
        *   SUInt16 SPROXx_Desfire_AuthenticateEv2NonFirst(SPROX_INSTANCE rInst,
        *                                        byte bKeyNumber,
        *                                        const byte pbAccessKey[16]);
        *
        *   [[pcsc_desfire.dll]]
        *   LONG  SCardDesfire_AuthenticateEv2NonFirst(SCARDHANDLE hCard,
        *                                      byte bKeyNumber,
        *                                      const byte pbAccessKey[16]);
        *
        * INPUTS
        *   byte bKeyNumber             : number of the key (KeyNo)
        *   const byte pbAccessKey[16]  : 16-byte Access Key (AES)
        *
        * RETURNS
        *   DF_OPERATION_OK    : authentication succeed
        *   Other code if internal or communication error has occured. 
        *
        * NOTES
        *   AES keys are always 128-bit long.
        *
        * SEE ALSO
        *   Authenticate
        *   AuthenticateIso24
        *   AuthenticateIso
        *   ChangeKeySettings
        *   GetKeySettings
        *   ChangeKey
        *   GetKeyVersion
        *
        **/
        public long AuthenticateEv2NonFirst(byte bKeyNumber, byte[] pbAccessKey)
        {
            long status;
            UInt32 t;
            byte[] abRndB = new byte[16];
            byte[] abRndA = new byte[16];
            byte[] IV = new byte[16];

            if (pbAccessKey == null)
                return DFCARD_LIB_CALL_ERROR;

#if _VERBOSE
      Console.WriteLine("AuthenticateEv2NonFirst IN {0}", CmdCtr);
#endif

            /* Each new Authenticate must invalidate the current authentication status. */
            CleanupAuthentication();

            /* Initialize the cipher unit with the authentication key */
            session_type = KEY_ISO_AES;
            SetKey(pbAccessKey);

            /* Create the command string consisting of the command byte and the parameter byte. */
            xfer_length = 0;
            xfer_buffer[xfer_length++] = DF_AUTHENTICATHE_EV2_NONFIRST;
            xfer_buffer[xfer_length++] = bKeyNumber;

            /* Send the command string to the PICC and get its response (1st frame exchange).
               The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
            /*do not affect the CmdCtr(neither increasing, nor resetting)*/
            status = Command(0, WANTS_ADDITIONAL_FRAME, false);
#if _VERBOSE
      Console.WriteLine("AuthenticateEv2NonFirst {0}", CmdCtr);
#endif
            if (status != DF_OPERATION_OK)
                return status;

            /* Check the number of bytes received, we expect 17 bytes. */
            if (xfer_length != 17)
            {
                /* Error: block with inappropriate number of bytes received from the card. */
                return DFCARD_WRONG_LENGTH;
            }

            /* OK, we received the cryptogram E(Kx, RndB) from the PICC.
               Decipher E(Kx, RndB) to get RndB in xfer_buffer.
               Note that the status code has already been extracted from the queue. */

            t = 16;
            byte[] tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, INF + 1, tmp, 0, (int)t);
            DeCipherSP80038A(SesAuthENCKey, ref tmp, ref t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, INF + 1, (int)t);

            /* Store this RndB (is needed later on for generating the session key). */
            Array.ConstrainedCopy(xfer_buffer, INF + 1, abRndB, 0, 16);

            /* Now the PCD has to generate RndA. */
            abRndA = random.Get(16);

            /* Start the second frame with a status byte indicating to the PICC that the Authenticate
               command is continued. E(Kx, RndA || RndB’)*/
            xfer_buffer[INF + 0] = DF_ADDITIONAL_FRAME;

            /* Append RndA and RndB' ( RndB' is generated by rotating RndB one byte to the left )
               after the status byte.  */
            Array.ConstrainedCopy(abRndA, 0, xfer_buffer, INF + 1, 16);
            Array.ConstrainedCopy(abRndB, 1, xfer_buffer, INF + 1 + 16, 15);
            xfer_buffer[INF + 1 + 31] = abRndB[0]; /* first byte move to last byte */
            xfer_length = 1 + 32;

            /* Apply the DES send operation to the argument bytes before sending the second frame
               to the PICC ( do not include the status byte in the DES operation ). */
            t = 32;
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
            CipherSP80038B(SesAuthENCKey, IV, null, ref tmp, ref t, t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

            /* Send the 2nd frame to the PICC and get its response. */
            status = Command(0, WANTS_OPERATION_OK, false);
            if (status != DF_OPERATION_OK)
                return status;

            /* We should now have E(Kx, RndA’) in our buffer.
               RndA' was made from RndA by the PICC by rotating the string one byte to the left.
               Decipher E(Kx,RndA’) to get RndA' in xfer_buffer. */
            t = 16;
            tmp = new byte[t];
            Array.ConstrainedCopy(xfer_buffer, 1, tmp, 0, (int)t);
            DeCipherSP80038A(SesAuthENCKey, ref tmp, ref t);
            Array.ConstrainedCopy(tmp, 0, xfer_buffer, 1, (int)t);

            /* Now we have RndA’in our buffer.
               We have to check whether it matches our local copy of RndA.
               If one of the subsequent comparisons fails, we do not trust the PICC and therefore
               abort the authentication procedure ( no session key is generated ). */

            /* Then compare the bytes 1 to bRndLen-1 of RndA with the first bRndLen-1 bytes in the queue. */
            for (int k = 0; k < 15; k++)
                if (xfer_buffer[INF + 1 + k] != abRndA[1 + k])
                    return DFCARD_WRONG_KEY;


            /* Then compare the leftmost byte of RndA with the last byte in the queue. */
            if (xfer_buffer[INF + 1 + 15] != abRndA[0])
                return DFCARD_WRONG_KEY;


            /* Desfire Ev2 */
            //SV1 = 0xA5 || 0x5A || 0x00 || 0x01 || 0x00 || 0x80 || RndA[15..14] || (RndA[13..8] ⊕ RndB[15..10])|| RndB[9..0] || RndA[7..0]
            byte[] SV1 = new byte[32];
            byte[] SV2 = new byte[32];
            byte[] rnda = new byte[6];
            byte[] rndb = new byte[6];

            Array.ConstrainedCopy(abRndA, 2, rnda, 0, 6);
            Array.ConstrainedCopy(abRndB, 0, rndb, 0, 6);

            for (int j = 0; j < 6; j++)
                rndb[j] ^= rnda[j];
            /* SV1 = 0xA5 || 0x5A || 0x00 || 0x01 || 0x00 || 0x80 || RndA[15..14] || (RndA[13..8] ⊕ RndB[15..10])|| RndB[9..0] || RndA[7..0] */
            SV1[0] = 0xA5; SV1[1] = 0x5A; SV1[2] = 0x00; SV1[3] = 0x01; SV1[4] = 0x00; SV1[5] = 0x80;
            Array.ConstrainedCopy(abRndA, 0, SV1, 6, 2);
            Array.ConstrainedCopy(rndb, 0, SV1, 8, 6);
            Array.ConstrainedCopy(abRndB, 6, SV1, 14, 10);
            Array.ConstrainedCopy(abRndA, 8, SV1, 24, 8);

            /* SV2 = 0x5A || 0xA5 || 0x00 || 0x01 || 0x00 || 0x80 || RndA[15..14] || (RndA[13..8] ⊕ RndB[15..10])|| RndB[9..0] || RndA[7..0] */
            SV2[0] = 0x5A; SV2[1] = 0xA5; SV2[2] = 0x00; SV2[3] = 0x01; SV2[4] = 0x00; SV2[5] = 0x80;
            Array.ConstrainedCopy(abRndA, 0, SV2, 6, 2);
            Array.ConstrainedCopy(rndb, 0, SV2, 8, 6);
            Array.ConstrainedCopy(abRndB, 6, SV2, 14, 10);
            Array.ConstrainedCopy(abRndA, 8, SV2, 24, 8);

            /* Initialize the cipher unit with the session key */
            session_type = KEY_ISO_AES;

            // 5. Calculate CMAC
            byte[] CMAC_IV = new byte[16];
            byte[] CMAC = new byte[16];
            byte[] ENC = new byte[16];

            CalculateCMACEV2(pbAccessKey, CMAC_IV, SV2, ref CMAC);
            CalculateCMACEV2(pbAccessKey, CMAC_IV, SV1, ref ENC);

            SetSesAuthMACKey(CMAC);
            SetSesAuthENCKey(ENC);
#if _VERBOSE
      Console.WriteLine("Mac session " + BinConvert.ToHex(CMAC, 16));
      Console.WriteLine("End session " + BinConvert.ToHex(ENC, 16));
#endif

            /* Iniliaze init vector for encryption */
            secure_mode = SecureMode.EV2;

            /* Authenticate succeeded, therefore we remember the number of the key which was used
               to obtain the current authentication status. */
            session_key_id = bKeyNumber;

            /* Reset the init vector */
            CleanupInitVector();

            /* Initialize the CMAC calculator */
            InitCmacEv2();



#if _VERBOSE
      Console.WriteLine("AuthenticateEv2NonFirst OUT {0}", CmdCtr);
#endif

            /* Success. */
            return DF_OPERATION_OK;

        }

        public void test_nsist()
        {
            byte[] mac = new byte[16];


            //byte[] test= new byte[] { 0x5E, 0x3F, 0x0F, 0x4A, 0xC5, 0x26, 0x3E, 0x5A, 0x68, 0xEB, 0xAD, 0xDB, 0x1B, 0x58, 0xB7, 0xBE, 0xED, 0xB7, 0x5E, 0x4A, 0xFF, 0x2D, 0x24, 0x95, 0xE1, 0x12, 0xCA, 0xC7, 0xA9, 0x81, 0xFA, 0x8D };
            byte[] test = new byte[] { 0xC6, 0xFA, 0x96, 0x1F, 0x44, 0xA2, 0xC7, 0x9F, 0x10, 0x46, 0xE2, 0x2B, 0x81, 0x11, 0x3D, 0xAF, 0x27, 0xF0, 0x6B, 0x5E, 0x71, 0xBC, 0x39, 0x95, 0x6B, 0xF4, 0xF5, 0x2D, 0x40, 0xF8, 0x68, 0xBB };
            byte[] appBKeyMaster0_16 = new byte[16] { 0x41, 0x70, 0x70, 0x2E, 0x42, 0x20, 0x4D, 0x61, 0x73, 0x74, 0x65, 0x72, 0x20, 0x4B, 0x65, 0x79 };
            byte[] ivv = new byte[16];
            uint tt = 32;
            CipherSP80038B(appBKeyMaster0_16, ivv, null, ref test, ref tt, tt);

#if _VERBOSE
      Console.WriteLine("=========================================================");
      Console.WriteLine("====================== TEST 1 ===========================");
#endif

            /* nsist example 1 */
            byte[] key1 = new byte[] { 0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6, 0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C };
            byte[] M1 = new byte[16];
            SetSesAuthMACKey(key1);

            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(M1, 0, false, ref mac);
#if _VERBOSE
      Console.WriteLine("=========================================================");
      Console.WriteLine("==================== TEST 2 ==========================");
#endif

            /* nsist example 2 */
            byte[] key2 = new byte[] { 0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6, 0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C };
            byte[] M2 = new byte[16] { 0x6B, 0xC1, 0xBE, 0xE2, 0x2E, 0x40, 0x9F, 0x96, 0xE9, 0x3D, 0x7E, 0x11, 0x73, 0x93, 0x17, 0x2A };
            SetSesAuthMACKey(key2);

            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(M2, (uint)M2.Length, false, ref mac);
#if _VERBOSE
      Console.WriteLine("=========================================================");
      Console.WriteLine("==================== TEST 3 ==============================");
#endif

            /* nsist example 3 */
            byte[] key3 = new byte[] { 0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6, 0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C };
            byte[] M3 = new byte[20] { 0x6B, 0xC1, 0xBE, 0xE2, 0x2E, 0x40, 0x9F, 0x96, 0xE9, 0x3D, 0x7E, 0x11, 0x73, 0x93, 0x17, 0x2A, 0xAE, 0x2D, 0x8A, 0x57 };
            SetSesAuthMACKey(key3);

            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(M3, (uint)M3.Length, false, ref mac);

#if _VERBOSE
      Console.WriteLine("=========================================================");
      Console.WriteLine("===================== TEST 4 =============================");
#endif

            /* nsist example 1 */
            byte[] key4 = new byte[] { 0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6, 0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C };
            byte[] M4 = new byte[64] {
        0x6B, 0xC1, 0xBE, 0xE2, 0x2E, 0x40, 0x9F, 0x96, 0xE9, 0x3D, 0x7E, 0x11, 0x73, 0x93, 0x17, 0x2A,
        0xAE, 0x2D, 0x8A, 0x57, 0x1E, 0x03, 0xAC, 0x9C, 0x9E, 0xB7, 0x6F, 0xAC, 0x45, 0xAF, 0x8E, 0x51,
        0x30, 0xC8, 0x1C, 0x46, 0xA3, 0x5C, 0xE4, 0x11, 0xE5, 0xFB, 0xC1, 0x19, 0x1A, 0x0A, 0x52, 0xEF,
        0xF6, 0x9F, 0x24, 0x45, 0xDF, 0x4F, 0x9B, 0x17, 0xAD, 0x2B, 0x41, 0x7B, 0xE6, 0x6C, 0x37, 0x10 };
            SetSesAuthMACKey(key4);

            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(M4, (uint)M4.Length, false, ref mac);

#if _VERBOSE
      Console.WriteLine("=========================================================");
      Console.WriteLine("===================== TEST 5 ============================");
#endif

            byte[] key = new byte[16] { 0x18, 0x5D, 0x0E, 0x3C, 0xEA, 0x4C, 0x0C, 0x32, 0xDF, 0xAD, 0x84, 0xB3, 0x41, 0x4A, 0x50, 0x54 };
            byte[] M = new byte[] { 0xBD, 0x02, 0x00, 0xB0, 0x4D, 0x6C, 0x11, 0x02, 0x00, 0x00, 0x00, 0x15, 0x00, 0x00 };
            byte[] M0 = new byte[] { 0x3D, 0x01, 0x00, 0xB0, 0x4D, 0x6C, 0x11, 0x02, 0x00, 0x00, 0x00, 0x15, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
        0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15 };


            SetSesAuthMACKey(key);

            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(M, (uint)M.Length, false, ref mac);

#if _VERBOSE
      Console.WriteLine("=========================================================");
      Console.WriteLine("=================== TEST 6 ==============================");
#endif

            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(M0, (uint)M0.Length, false, ref mac);



#if _VERBOSE
      Console.WriteLine("=========================================================");
      Console.WriteLine("=================== TEST 7 ===============================");
#endif

            byte[] key5 = new byte[16];
            byte[] M5 = new byte[] { 0xA5, 0x5A, 0x00, 0x01, 0x00, 0x80, 0x87, 0x6D, 0xE9, 0x7B, 0x7F, 0x8D, 0xCF,
        0x2B, 0x2B, 0xA7, 0x7D, 0x10, 0xB2, 0x80, 0x25, 0xF2, 0x24, 0xE6, 0xAF, 0xBF, 0x56, 0x48, 0x34, 0xF9, 0x8F, 0x1E };

            byte[] M6 = new byte[] { 0x5A, 0xA5, 0x00, 0x01, 0x00, 0x80, 0x87, 0x6D, 0xE9, 0x7B, 0x7F, 0x8D, 0xCF,
        0x2B, 0x2B, 0xA7, 0x7D, 0x10, 0xB2, 0x80, 0x25, 0xF2, 0x24, 0xE6, 0xAF, 0xBF, 0x56, 0x48, 0x34, 0xF9, 0x8F, 0x1E };

            byte[] IV = new byte[16];


            CalculateCMACEV2(key5, IV, M5, ref mac);
            CalculateCMACEV2(key5, IV, M6, ref mac);

            SetSesAuthMACKey(key5);
            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(M5, (uint)M5.Length, false, ref mac);

            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(M6, (uint)M6.Length, false, ref mac);


#if _VERBOSE
      Console.WriteLine("=========================================================");
      Console.WriteLine("==================== TEST 8 ==============================");
#endif

            byte[] plain_text = new byte[16];
            byte[] SesAuthENCKey = new byte[16] { 0x03, 0x0A, 0x8C, 0x76, 0xBB, 0xC9, 0x54, 0x51, 0x3A, 0x52, 0xB2, 0xAA, 0xD1, 0x61, 0xD1, 0x5B };
            plain_text = new byte[16] { 0xA5, 0x5A, 0xB0, 0x4D, 0x6C, 0x11, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            Console.WriteLine("InitVectorEv2 Before AES_Encrypt_ECB " + BinConvert.ToHex(plain_text, 16));
            byte[] init = new byte[16] { 0x03, 0x0A, 0x8C, 0x76, 0xBB, 0xC9, 0x54, 0x51, 0x3A, 0x52, 0xB2, 0xAA, 0xD1, 0x61, 0xD1, 0x5B };
            byte[] result = DesfireCrypto.AES_Encrypt_ECB(plain_text, SesAuthENCKey, init);
            Console.WriteLine("InitVectorEv2 After AES_Encrypt_ECB= " + BinConvert.ToHex(result, result.Length));

            TransactionIdentifier = new byte[4];
            SetSesAuthENCKey(SesAuthENCKey);
            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            uint length = 0;
            uint t = 16;
            byte[] tmp = new byte[48];


            Console.WriteLine("A CipherSP80038A Plain " + BinConvert.ToHex(plain_text, 16));
            CipherSP80038A(plain_text, 0, 16, 48, ref tmp, 0, ref t);
            Console.WriteLine("B CipherSP80038A Cipher " + BinConvert.ToHex(tmp, t));
            DeCipherSP80038A(SesAuthENCKey, ref plain_text, ref length, false);
            Console.WriteLine("C CipherSP80038A DeCipher " + BinConvert.ToHex(plain_text, 16));



            plain_text = new byte[16] { 0x3B, 0x7A, 0x63, 0xF3, 0x1B, 0xB6, 0x08, 0x84, 0xCC, 0xC0, 0x99, 0xE8, 0x36, 0xFB, 0x32, 0xAA };
            SesAuthENCKey = new byte[16] { 0xD2, 0xDB, 0x39, 0x03, 0x5F, 0xB4, 0x60, 0xA3, 0x1A, 0x46, 0xD9, 0xD1, 0x26, 0x71, 0xEB, 0x9A };

            length = 16;
            DeCipherSP80038A(SesAuthENCKey, ref plain_text, ref length, false);


        }
        /// <summary>
        /// Just for when using desfire with SAM. Set session type as authenticate is made from SAM.
        /// Lazy solution to avoid reviewing all library for session_type variable.
        /// </summary>
        /// <param name="type"></param>
        public void SetSamSessionType(SessionType type, ICardApduTransmitter sam)
        {
            sam_channel = sam;
            switch ( type)
            { 
                case SessionType.KEY_LEGACY_DES:
                    session_type = KEY_LEGACY_DES;
                    break;
                case SessionType.KEY_LEGACY_3DES:
                    session_type = KEY_LEGACY_3DES;
                    break;
                case SessionType.KEY_ISO_DES:
                    session_type = KEY_ISO_DES;
                    break;
                case SessionType.KEY_ISO_3DES2K:
                    session_type = KEY_ISO_3DES2K;
                    break;
                case SessionType.KEY_ISO_3DES3K:
                    session_type = KEY_ISO_3DES3K;
                    break;
                case SessionType.KEY_ISO_AES:
                    session_type = KEY_ISO_AES;
                    break;
                case SessionType.KEY_ISO_MODE:
                    session_type = KEY_ISO_MODE;
                    break;
                default:
                    session_type = 0x00;
                    break;
            }            
        }
    }
}
