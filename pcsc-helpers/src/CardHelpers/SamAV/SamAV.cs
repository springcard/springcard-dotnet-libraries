/**
 *
 * \defgroup SamAV2
 *
 * \brief SamAV2 library (.NET only, no native depedency)
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
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
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

        public static string KeyToString(byte[] keyValue)
        {
            return BinConvert.ToHex(keyValue);
        }


        IUserInteraction userInteraction = DefaultUserInteraction.Instance;
        ICardApduTransmitter samReader = null;

        public ushort _StatusWord;

        private ResultE _LastError;
        public ResultE LastError
        {
            get
            {
                ResultE value = _LastError;
                return value;
            }
            private set
            {
                //Logger.Trace("LastError<-{0}", value.ToString());
                _LastError = value;
            }
        }

        internal class AuthInfo
        {
            public AuthTypeE AuthType;
            public byte KeyIdx;
            public byte KeyVersion;

            public AuthInfo Clone()
            {
                return (AuthInfo)this.MemberwiseClone();
            }

            public AuthInfo(AuthTypeE AuthType, byte KeyIdx, byte KeyVersion)
            {
                this.AuthType = AuthType;
                this.KeyIdx = KeyIdx;
                this.KeyVersion = KeyVersion;
            }
        }

        AuthInfo activeAuth;
        SamVersionE samVersion;

        /*
        byte DefaultAuthKeyIdx;
        byte DefaultAuthKeyVersion;
        byte[] DefaultAuthKeyValue;
        */



        byte[] session_key;
        IRandomGenerator random = new DefaultRandomGenerator();

        private byte key_Report = 0x80;
        private bool onlyPublic = false;
        private byte[] rsa_Key;

        public byte[] Rsa_Key
        {
            get
            {
                return rsa_Key;
            }

            set
            {
                rsa_Key = value;
            }
        }

        public byte Key_Report
        {
            get
            {
                return key_Report;
            }

            set
            {
                key_Report = value;
            }
        }

        public bool OnlyPublic
        {
            get
            {
                return onlyPublic;
            }

            set
            {
                onlyPublic = value;
            }
        }

        public SamAV(ICardApduTransmitter samReader)
        {
            if (samReader == null)
                throw new ArgumentNullException("samReader can't be null");

            this.samReader = samReader;

            //	IV = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        }

        private bool OnCommunicationError()
        {
            Logger.Warning("No response from the SAM");
            userInteraction.Error("The communication channel with the SAM has been broken");
            LastError = ResultE.CommunicationError;
            return false;
        }


        private bool OnStatusWordError(ushort sw)
        {
            Logger.Warning("SAM SW={0:X04}", sw);
            userInteraction.Warning(string.Format("The SAM has returned an error (SW={0:X04})", sw));
            LastError = ResultE.UnexpectedStatusWord;

            _StatusWord = sw;
            return false;
        }

        private bool OnPiccStatusWordError(ushort sw)
        {
            Logger.Warning("PICC SW={0:X04}", sw);
            userInteraction.Warning(string.Format("The PICC has returned an error (SW={0:X04})", sw));
            LastError = ResultE.UnexpectedStatusWord;
            return false;
        }

        private bool OnSecurityError()
        {
            Logger.Warning("Security error");
            userInteraction.Warning(string.Format("The SAM has returned an invalid response (bad MAC or CMAC)"));
            LastError = ResultE.SecurityError;
            return false;
        }

        /*
        public void SetAuthenticationParameters(AuthTypeE KeyType, byte KeyIdx, byte KeyVersion, byte[] initVector, bool keepInitVector)
        {
            activeAuth = new AuthInfo(KeyType, KeyIdx, KeyVersion);
            currentAuth.AuthType = KeyType;
            currentAuth.KeyIdx = KeyIdx;
            currentAuth.KeyVersion = KeyVersion;
            currentAuth.InitVector = initVector;
            currentAuth.KeepInitVector = keepInitVector;
        }
        */

        public bool Program(Entries entries)
        {
            for (int i=entries.Length-1; i>=0; i--)
            {
                Entry entry = entries[i];
                if (entry.MustWrite)
                {
                    if (entry is SymmetricEntry)
                    {
                        Logger.Trace("Programming symmetric key " + BinConvert.ToHex(entry.Index) + "...");

                        if (entry.MustGenerate)
                        {
                            throw new Exception("Key generation is not implemented");
                        }

                        if (!ChangeKeyEntry(entry.Index, entry.Value))
                        {
                            switch (LastError)
                            {
                                case ResultE.Success:
                                    Logger.Trace("OK");
                                    break;
                                case ResultE.InvalidParameters:
                                    Logger.Trace("Invalid parameters");
                                    userInteraction.Error("Key entry " + BinConvert.ToHex(entry.Index) + " is corrupt");
                                    return false;
                                case ResultE.CommunicationError:
                                    Logger.Trace("Communication error");
                                    userInteraction.Error("Error while writing key entry " + BinConvert.ToHex(entry.Index));
                                    return false;
                                case ResultE.ExecutionFailed:
                                    if (activeAuth == null)
                                    {
                                        Logger.Trace("Not authenticated");
                                        userInteraction.Error("Failed to write key entry " + BinConvert.ToHex(entry.Index));
                                        return false;
                                    }
                                    if ((((SymmetricEntry)entry).ChangeKeyEntry == activeAuth.KeyIdx) &&
                                        (((SymmetricEntry)entry).ChangeKeyVersion == activeAuth.KeyVersion))
                                    {
                                        Logger.Trace("Change key failed");
                                        userInteraction.Error("Failed to write key entry " + BinConvert.ToHex(entry.Index));
                                        return false;
                                    }
                                    Logger.Trace("Execution failed, but this is possibly OK");
                                    userInteraction.Info(string.Format("Ignoring wrong SW on key entry {0:X02} since it may already have been written...", entry.Index));
                                    break;
                            }
                        }
                    }
                    else if (entry is AsymetricEntry)
                    {
                        Logger.Trace("Programming asymmetric key (PKI) " + BinConvert.ToHex((byte) (entry.Index % SymmetricKeyCount)) + "...");

                        if (!ChangePkiEntry((byte)(entry.Index % SymmetricKeyCount), entry.Value, entry.MustGenerate))
                        {
                            switch (LastError)
                            {
                                case ResultE.Success:
                                    Logger.Trace("OK");
                                    break;
                                case ResultE.InvalidParameters:
                                    Logger.Trace("Invalid parameters");
                                    userInteraction.Error("PKI entry " + BinConvert.ToHex(entry.Index) + " is corrupt");
                                    return false;
                                case ResultE.CommunicationError:
                                    Logger.Trace("Communication error");
                                    userInteraction.Error("Error while writing PKI entry " + BinConvert.ToHex(entry.Index));
                                    return false;
                                case ResultE.ExecutionFailed:
                                    Logger.Trace("Execution failed");
                                    userInteraction.Error("Failed to write PKI entry " + BinConvert.ToHex(entry.Index));
                                    return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private bool Swith_To_AV2_First_part(byte KeyIdx, byte KeyVersion, byte[] KeyValue)
        {
            /* Connect and change change Key 0 in AES Mode : key will be { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }	*/
            string newKey0Val = "";

           
            if (!AuthenticateHostAV1(AuthTypeE.TDES_CRC16, 0x00, 0x00, new byte[16]))
            {
                return false;
            }

            newKey0Val = "0000000000000000000000000000000000112233445566778899aabbccddeeffabcdef012345678990817263545e740f000000000000ff2000000102";

            if (!ChangeKeyEntry(0, BinConvert.HexToBytes(newKey0Val)))
            {
                return false;
            }

            return true;
        }

        public bool Switch_to_AV2()
        {
            Logger.Debug("------------------------------Switching from AV1 to AV2------------------------------");

            /* verifier la version : doit etre AV1	*/
            if (!SamIsAV1())
            {
                Logger.Debug("The SAM is not an AV1. Can't switch to AV2");
                userInteraction.Error("The SAM is not an AV1. It cannot be switched to AV2");
                return false;
            }

            if (!Swith_To_AV2_First_part(0, 0, new byte[16]))
            {
                userInteraction.Error("The first part of the switching process is to connect and change the root key to AES 128.\nThis operation has failed.");
                return false;
            }


            /* Second, Switch from AV1 to AV2	*/

            // Get Rnd2 from SAM
            Logger.Debug("Getting Rnd2 from SAM ...");
            byte[] apdu2 = { CLA, (byte)INS.LockUnlock, 0x03, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            CAPDU capdu = new CAPDU(apdu2);

            RAPDU rapdu;
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }

            if (rapdu.SW != 0x90AF)
            {
                Logger.Debug("First rapdu invalid");
                return false;
            }

            byte[] Rnd2 = new byte[rapdu.GetBytes().Length - 2];
            for (int i = 0; i < rapdu.GetBytes().Length - 2; i++)
                Rnd2[i] = rapdu.GetByte(i);

            // 4. CMAC_load
            Logger.Debug("Calculating CMAC_load ...");
            byte[] CMAC_load = new byte[Rnd2.Length + 1 + 3];   // +P1+MaxChainBlocs
            int offset = 0;
            for (int i = 0; i < Rnd2.Length; i++)
                CMAC_load[offset++] = Rnd2[i];

            CMAC_load[offset++] = 0x03;
            CMAC_load[offset++] = 0x00;
            CMAC_load[offset++] = 0x00;
            CMAC_load[offset++] = 0x00;

            byte[] Key = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] IV = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            // 5. Calculate CMAC
            Logger.Debug("Calculating CMAC ...");
            byte[] long_CMAC = CryptoPrimitives.CalculateCMAC(Key, IV, CMAC_load);
            byte[] CMAC = new byte[long_CMAC.Length / 2];
            int j = 0;
            for (int i = 1; i < long_CMAC.Length;)
            {
                CMAC[j++] = long_CMAC[i];
                i += 2;
            }

            //Console.Write("CMAC=");
            //for (int i = 0; i < CMAC.Length; i++)
            //    Console.Write("-" + String.Format("{0:x02}", CMAC[i]));
            //Console.Write("\n");

            // 6. Generate Random
            Logger.Debug("Generating Rnd1 ...");
            byte[] Rnd1 = random.Get(12);

            // 7. Generate CAPDU
            Logger.Debug("Generating CAPDU ...");
            byte[] capdu2 = new byte[20];
            offset = 0;

            for (int i = 0; i < CMAC.Length; i++)
                capdu2[offset++] = CMAC[i];

            for (int i = 0; i < Rnd1.Length; i++)
                capdu2[offset++] = Rnd1[i];


            // 8. SV1
            Logger.Debug("Generating CAPDU ...");
            byte[] SV1 = new byte[16];
            offset = 0;
            for (int i = 7; i < Rnd1.Length; i++)
                SV1[offset++] = Rnd1[i];

            for (int i = 7; i < Rnd2.Length; i++)
                SV1[offset++] = Rnd2[i];

            for (int i = 0; i < 5; i++)
                SV1[offset++] = (byte)(Rnd1[i] ^ Rnd2[i]);

            SV1[offset++] = 0x91;

            /*
            Console.Write("SV1=");
            for (int i = 0; i < SV1.Length; i++)
                Console.Write("-" + String.Format("{0:x02}", SV1[i]));
            Console.Write("\n");
            */


            // 9. KXE
            Logger.Debug("Calculating KXE ...");
            byte[] KXE = CryptoPrimitives.AES_Encrypt(SV1, Key, IV);

            /*
            Console.Write("KXE=");
            for (int i = 0; i < KXE.Length; i++)
                Console.Write("-" + String.Format("{0:x02}", KXE[i]));
            Console.Write("\n");
            */

            // 10. Get R-APDU
            capdu = new CAPDU(CLA, (byte)INS.LockUnlock, 0x00, 0x00, capdu2, 0x00);
            rapdu = Transmit(capdu);

            //Console.WriteLine("\nrapdu etape 10=" + rapdu.AsString());

            if (rapdu.SW != 0x90AF)
            {
                Logger.Debug("Second rapdu invalid");
                return false;
            }

            byte[] Ek_Kxe_RndB = new byte[16];
            offset = 0;
            for (int i = 8; i < rapdu.GetBytes().Length - 2; i++)
                Ek_Kxe_RndB[offset++] = rapdu.GetByte(i);

            // 11. CMAC Load in last R-APDU
            Logger.Debug("Generating CMAD Load in last R-APDU ...");
            byte[] CMAC_load_last_rapdu = new byte[16];
            offset = 0;
            for (int i = 0; i < Rnd1.Length; i++)
                CMAC_load_last_rapdu[offset++] = Rnd1[i];

            CMAC_load_last_rapdu[offset++] = 0x03;
            CMAC_load_last_rapdu[offset++] = 0x00;
            CMAC_load_last_rapdu[offset++] = 0x00;
            CMAC_load_last_rapdu[offset++] = 0x00;

            //12. CMAC calculation
            Logger.Debug("Calculating CMAC...");
            long_CMAC = CryptoPrimitives.CalculateCMAC(Key, IV, CMAC_load_last_rapdu);

            j = 0;
            for (int i = 1; i < long_CMAC.Length;)
            {
                CMAC[j++] = long_CMAC[i];
                i += 2;
            }

            /*
            Console.Write("CMAC calcule:");
            for (int i = 0; i < CMAC.Length; i++)
                Console.Write("-" + String.Format("{0:x02}", CMAC[i]));
            Console.Write("\n");
            */

            //13. RndB

            //Console.Write("test decrypt RndB:");
            //for (int i = 0; i < RndB.Length ; i++)
            //	Console.Write("-" + String.Format("{0:x02}", RndB[i]));
            //Console.Write("\n");		


            //Console.Write("Ek_Kxe_RndB:");
            //for (int i = 0; i < Ek_Kxe_RndB.Length ; i++)
            //	Console.Write("-" + String.Format("{0:x02}", Ek_Kxe_RndB[i]));
            //Console.Write("\n");	


            Logger.Debug("Decrypting to get RndB ...");
            byte[] RndB = CryptoPrimitives.AES_Decrypt(Ek_Kxe_RndB, KXE, IV);

            //Console.Write("RndB:");
            //for (int i = 0; i < RndB.Length ; i++)
            //	Console.Write("-" + String.Format("{0:x02}", RndB[i]));
            //Console.Write("\n");	


            //14. RndA
            Logger.Debug("Generating RndA ...");
            byte[] RndA = random.Get(16);

            //15. RndBpp
            Logger.Debug("Rotating to get RndBpp...");
            byte[] RndBp = CryptoPrimitives.RotateLeft(RndB);
            byte[] RndBpp = CryptoPrimitives.RotateLeft(RndBp);

            //16.RndA+RndBpp
            Logger.Debug("Concatening RndA and RndBpp ...");
            offset = 0;
            byte[] concat = new byte[RndA.Length + RndBpp.Length];
            for (int i = 0; i < RndA.Length; i++)
                concat[offset++] = RndA[i];

            for (int i = 0; i < RndBpp.Length; i++)
                concat[offset++] = RndBpp[i];

            /*
            Console.Write("concat:");
            for (int i = 0; i < concat.Length; i++)
                Console.Write("-" + String.Format("{0:x02}", concat[i]));
            Console.Write("\n");
            */


            // 17.Ek(Kxe, RndA+RndB'')
            //byte[] Ex_KXE = { 0x3f, 0xec, 0xb2, 0x19, 0x82, 0x8d, 0xfe, 0x14, 0xb2, 0x54, 0xca, 0xbf, 0xc7, 0x1c, 0x1b, 0x7f  };
            Logger.Debug("Encrypting the concatenation ...");
            byte[] Ek_KXE_concat = CryptoPrimitives.AES_Encrypt(concat, KXE, IV);

            /*
            Console.Write("n17:");
            for (int i = 0; i < Ek_KXE_concat.Length; i++)
                Console.Write("-" + String.Format("{0:x02}", Ek_KXE_concat[i]));
            Console.Write("\n");
            */


            // 18.capdu part 3
            capdu = new CAPDU(CLA, (byte)INS.LockUnlock, 0x00, 0x00, Ek_KXE_concat, 0x00);

            // 19. RndA''
            byte[] RndAp = CryptoPrimitives.RotateLeft(RndA);
            byte[] RndApp = CryptoPrimitives.RotateLeft(RndAp);


            // 20. Get Rapdu
            rapdu = Transmit(capdu);
            //Console.WriteLine("\nrapdu_final=" + rapdu.AsString());

            if ((rapdu.SW1 != 0x90) || (rapdu.SW2 != 0x00))
            {
                Logger.Debug("Last rapdu invalid");
                return false;
            }

            //21 Decrypt RAPDU
            byte[] EkRndapp = new byte[rapdu.GetBytes().Length - 2];
            byte[] Recv_Rndapp = CryptoPrimitives.AES_Decrypt(EkRndapp, KXE, IV);

            //22. Comparaison



            //Console.Write("RndApp:");
            //for (int i = 0; i < RndApp.Length ; i++)
            //	Console.Write("-" + String.Format("{0:x02}", RndApp[i]));
            //Console.Write("\n");	

            //Console.Write("Recv_Rndapp:");
            //for (int i = 0; i < Recv_Rndapp.Length ; i++)
            //	Console.Write("-" + String.Format("{0:x02}", Recv_Rndapp[i]));
            //Console.Write("\n");	


            //for (int i = 0; i< Recv_Rndapp.Length; i++)
            //{
            //	if (RndApp[i] != Recv_Rndapp[i])
            //		MessageBox.Show("AV2 KO");
            //}
            //MessageBox.Show("AV2 OK");

            Logger.Debug("---------Switching from AV1 to AV2 OK---------");
			userInteraction.Info("Switch from AV1 to AV2 completed");
            return true;
        }


        public string Get16RandomBytesAsString()
        {
            Logger.Debug("------------------------------Getting 16 Random Bytes------------------------------");

            byte[] capdu_byte = { CLA, 0x60, 0x00, 0x00, 0x00 };
            CAPDU capdu = new CAPDU(capdu_byte);

            RAPDU rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return null;
            }

            if (rapdu.GetBytes().Length != 33)
            {
                userInteraction.Error("The response from 'Get Version' command is invalid\nThe SAM may not be from NXP");
                Logger.Debug("The response from 'Get Version' command is invalid. The SAM may not be from NXP");
                return null;
            }


            //byte[] apdu = {CLA, INS.AuthenticateHost, 0x00, 0x00, 0x03, SAM_Key_Entry, SAM_Key_Version, 0x00};
            byte[] apdu = { CLA, (byte)INS.AuthenticateHost, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00 };
            string ret = "";

            /* Get 16 bytes	*/
            capdu = new CAPDU(apdu);
            rapdu = Transmit(capdu);

            ret += rapdu.AsString("").Substring(0, rapdu.AsString("").Length - 12);

            /* Get 8 last bytes	*/

            capdu = new CAPDU(apdu);
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return null;
            }

            ret += rapdu.AsString("").Substring(0, rapdu.AsString("").Length - 12);

            return ret;
        }

        public string Get6RandomBytesAsString()
        {
            Logger.Debug("------------------------------Getting 6 Random Bytes------------------------------");

            byte[] capdu_byte = { CLA, 0x60, 0x00, 0x00, 0x00 };
            CAPDU capdu = new CAPDU(capdu_byte);

            RAPDU rapdu = Transmit(capdu);     
            if (rapdu == null)
            {
                OnCommunicationError();
                return null;
            }

            if (rapdu.GetBytes().Length != 33)
            {
                userInteraction.Error("The response from 'Get Version' command is invalid\nThe SAM may not be from NXP");
                Logger.Debug("The response from 'Get Version' command is invalid. The SAM may not be from NXP");
                return null;
            }


            byte[] apdu = { CLA, (byte)INS.AuthenticateHost, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00 };
            string ret = "";

            /* Get 6 first bytes	*/
            capdu = new CAPDU(apdu);
            rapdu = Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return null;
            }

            ret = rapdu.AsString("").Substring(0, rapdu.AsString("").Length - 16);
            return ret;
        }

        public bool PKIGenerateKeyPair_AV2(byte PkiEntry, string val)
        {
            Logger.Debug("------------------------------Changing Pki Entry " + String.Format("{0:x02}", PkiEntry) + "------------------------------");

            byte[] new_key_entry_data;

            // Tests sur val	
            if (val == null)
            {
                Logger.Debug("No value to change !");
                userInteraction.Error("Key Entry " + String.Format("{0:x02}", PkiEntry).ToUpper() + " has no value !");
                return false;
            }


            //val = val.Substring(0, val.Length - 2); // Remove "RW" (random + write)

            Char charRange = '-';
            int startIndex = val.LastIndexOf(charRange);

            if (startIndex > 1)
            {
                val = val.Substring(0, startIndex - 3); // Remove "RIW" (random + write) and PKI
            }


            // The string must be first checked, and then sent to the SAM
            for (int i = 0; i < (val.Length); i++)
            {
                if (!isByte(val[i]))
                {
                    Logger.Debug("New value is not in hexadecimal format");
                    userInteraction.Error("Key Entry " + String.Format("{0:x02}", PkiEntry).ToUpper() + " is not in hexadecimal format.");
                    return false;
                }
            }
            Logger.Debug("New value: " + val);
            CardBuffer CardBufKey_entry = new CardBuffer(val);

            new_key_entry_data = CardBufKey_entry.GetBytes();

            byte[] cmde_key_entry_data = CardBufKey_entry.GetBytes(3, (int)new_key_entry_data[2]);

            CAPDU capdu;
            capdu = new CAPDU(CLA, 0x15, new_key_entry_data[0], new_key_entry_data[1], cmde_key_entry_data);
            RAPDU rapdu = Transmit(capdu);
            //Console.WriteLine("\nRAPDU du pki generate key Pair=" + rapdu.AsString());

            if (rapdu.SW == 0x9000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool PKIExportPublicKey_AV2(byte PkiEntry)
        {
            Logger.Debug("------------------------------Exporting Public Key Pki Entry " + String.Format("{0:x02}", PkiEntry) + "------------------------------");


            byte[] cmde = new byte[] { CLA, 0x18, PkiEntry, 0x00, 0x00 };
            byte[] public_tmp = new byte[2048];
            int offset = 0;
            CAPDU capdu = new CAPDU(cmde);
            RAPDU rapdu = Transmit(capdu);
            //Console.WriteLine("\nRAPDU du pki export private key=" + rapdu.AsString());

            rapdu.GetBytes().CopyTo(public_tmp, 0);
            offset += (rapdu.GetBytes().Length - 2);

            // maximum two call to get full key
            if (rapdu.SW == 0x90AF)
            {
                do
                {
                    // see page 68 ref ds164535_P5DF081 MIFARE secure access module SAM AV2_3.5
                    /*Note that for certain commands (e.g.PKI_ExportPrivateKey, PKI_ExportPublicKey, but
                    also some of the data processing commands), it is possible that the full command is sent,
                    while the response indicates that still more data will follow (by status words 90AFh). In that
                    case, the host needs to request the subsequent part(s) by repeating the command header
                    consisting of the CLA, INS, P1, P2 and Le bytes. The CLA and INS bytes are repeated
                    literally, while the other bytes are set to 00h.Exception for this is the DESFire_ReadX
                    command, where also P2 is repeated and on top chained commands have an Lc - byte and
                    datafield containing 0xAF, i.e.the command code used by DESFire for application
                    chaining (see also Sect. 11.13.4).As these command-response pairs are still considered
                    as part of the same command, in case of MAC or Full Protection, no new protection needs
                    to be applied, i.e.no MAC is added and the Cmd_Ctr is not increased.*/
                    cmde = new byte[] { CLA, 0x18, 0x00, 0x00, 0x00 };
                    capdu = new CAPDU(cmde);
                    rapdu = Transmit(capdu);
                    rapdu.GetBytes().CopyTo(public_tmp, offset);
                    offset += (rapdu.GetBytes().Length - 2);
                }
                while (rapdu.SW == 0x90AF);

                byte[] public_pki = new byte[offset];
                RawData.MemCopy(public_pki, 0, public_tmp, 0, offset);
                this.rsa_Key = public_pki;
            }
            else if ((rapdu.SW1 == 0x90) && (rapdu.SW2 == 0x00))
            {
                this.rsa_Key = rapdu.GetBytes();
            }

            if ((rapdu.SW1 == 0x90) && (rapdu.SW2 == 0x00))
            {
                return true;
            }
            else
            {
                //this.Rsa_Private_Key = null;
                return false;
            }
        }
        public bool PKIExportPrivateKey_AV2(byte PkiEntry)
        {
            Logger.Debug("------------------------------Exporting Private Key Pki Entry " + String.Format("{0:x02}", PkiEntry) + "------------------------------");


            byte[] cmde = new byte[] { CLA, 0x1F, PkiEntry, 0x00, 0x00 };
            byte[] private_tmp = new byte[2048];
            int offset = 0;
            CAPDU capdu = new CAPDU(cmde);
            RAPDU rapdu = Transmit(capdu);
            //Console.WriteLine("\nRAPDU du pki export private key=" + rapdu.AsString());

            rapdu.GetBytes().CopyTo(private_tmp, 0);
            offset += (rapdu.GetBytes().Length - 2);

            // maximum two call to get full key
            if (rapdu.SW == 0x90AF)
            {
                do
                {
                    // see page 68 ref ds164535_P5DF081 MIFARE secure access module SAM AV2_3.5
                    cmde = new byte[] { CLA, 0x1F, 0x00, 0x00, 0x00 };
                    capdu = new CAPDU(cmde);
                    rapdu = Transmit(capdu);
                    rapdu.GetBytes().CopyTo(private_tmp, offset);
                    offset += (rapdu.GetBytes().Length - 2);
                }
                while (rapdu.SW == 0x90AF);

                byte[] private_pki = new byte[offset];
                RawData.MemCopy(private_pki, 0, private_tmp, 0, offset);
                this.rsa_Key = private_pki;
            }
            else if ((rapdu.SW1 == 0x90) && (rapdu.SW2 == 0x00))
            {
                this.rsa_Key = rapdu.GetBytes();
            }

            if ((rapdu.SW1 == 0x90) && (rapdu.SW2 == 0x00))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool PKIImportKeyPair_AV2(byte PkiEntry, string val)
        {
            Logger.Debug("------------------------------Import Pki Entry " + String.Format("{0:x02}", PkiEntry) + "------------------------------");

            CAPDU capdu;
            RAPDU rapdu;

            // Tests sur val	
            if (val == null)
            {
                Logger.Debug("No value to import !");
                userInteraction.Error("Key Entry " + String.Format("{0:x02}", PkiEntry).ToUpper() + " has no value !");
                return false;
            }

            /*Char charRange = '-';
            int startIndex = val.LastIndexOf(charRange);

            if (startIndex > 1)
            {
                startIndex += 1;
                val = val.Substring(startIndex, (val.Length- (startIndex))); // Remove "RW" (random + write) and PKI
            }*/


            // The string must be first checked, and then sent to the SAM
            for (int i = 0; i < val.Length; i++)
            {
                if (!isByte(val[i]))
                {
                    Logger.Debug("New value is not in hexadecimal format");
                    userInteraction.Error("Key Entry " + String.Format("{0:x02}", PkiEntry).ToUpper() + " is not in hexadecimal format.");
                    return false;
                }
            }
            Logger.Debug("New value: " + val);
            CardBuffer CardBufKey_entry = new CardBuffer(val);
            //byte[] cmde_share_entry_data = new byte[248];

            byte[] cmde;
            byte[] data;

            if (CardBufKey_entry.Length > 248)
            {
                // write 14 header before data PKI_No to PKi_ipq
                //0000630000ff0100000400800080
                int offset = 13;
                if (PkiEntry == 0x02)
                {
                    offset = 9;
                }

                data = CardBufKey_entry.GetBytes(0, offset);

                cmde = new byte[data.Length + 1];

                //cmde[0] = 0x0E;
                cmde[0] = PkiEntry;
                data.CopyTo(cmde, 1);

                capdu = new CAPDU(CLA, 0x19, 0x00, 0xAF, cmde);
                rapdu = Transmit(capdu);

                // maximum two call to get full key
                if (rapdu.SW == 0x90AF)
                {
                    do
                    {
                        if ((CardBufKey_entry.Length - offset) > 248)
                        {
                            cmde = CardBufKey_entry.GetBytes(offset, 248);
                            capdu = new CAPDU(CLA, 0x19, 0x00, 0xAF, cmde);
                        }
                        else
                        {
                            cmde = CardBufKey_entry.GetBytes(offset, (CardBufKey_entry.Length - offset));
                            capdu = new CAPDU(CLA, 0x19, 0x00, 0x00, cmde);
                        }
                        rapdu = Transmit(capdu);
                        offset += 248;
                    }
                    while (rapdu.SW == 0x90AF);

                }
            }
            else
            {
                // rest of command
                data = CardBufKey_entry.GetBytes(0, CardBufKey_entry.Length);
                cmde = new byte[data.Length + 1];
                data.CopyTo(cmde, 1);
                cmde[0] = (byte)CardBufKey_entry.Length;

                capdu = new CAPDU(CLA, 0x19, 0x00, 0x00, cmde);
                rapdu = Transmit(capdu);
                if (rapdu == null)
                {
                    OnCommunicationError();
                    return false;
                }
            }

            if ((rapdu.SW1 == 0x90) && (rapdu.SW2 == 0x00))
            {
                return true;
            }
            return false;
        }
    }
}
