using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class Desfire
    {
        UInt16 measuredFDT;
        UInt16 minFDT;
        byte Option = 0x00;
        byte PPS1;
        byte[] pRndR = new byte[8];
        byte[] pRndC = new byte[8];
        byte[] payload = new byte[16];
#if _0
    byte[] vcuid = new byte[10];
    uint   vcuid_length = 0;
    uint CRDSIZ;

    byte MKSID;
    public enum PROXIMITY_CHECK_SETTINGS : byte
    {
      Proximity_Check_Not_Supported = 0x00,
      Proximity_Check_Supported = 0x08,
      Proximity_Check_Supported_Mandatory = 0x10,
      Proximity_Check_Reserved = 0x18,
    }

    PROXIMITY_CHECK_SETTINGS PCSUP;
#endif
        /*TimeSpan t0 = new DateTime();
        TimeSpan t1 = new DateTime();

        void BENCH_INIT()
        {
            t0 = TimeSpan.FromTicks(DateTime.Now.Ticks);
        }
        */

        double GET_BENCH()
        {
            //TimeSpan sincelast = TimeSpan.FromTicks(DateTime.Now.Ticks - t0.Ticks);
            return 0.0;
        }

        long ISOTest(byte[] vcEncKey, byte[] vcMacKey)
        {
            byte[] EncryptedStream = new byte[] { 
                0xC4, 0x04, 0xE2, 0xF7,
                0x61, 0xEE, 0xB7, 0xCA,
                0x77, 0xAC, 0x23, 0x91,
                0x20, 0x5A, 0x85, 0x64 };
            byte[] vcData = new byte[16];


            byte[] abRndChal = new byte[] {
                0x71, 0x65, 0x7D, 0x5A,
                0xFF, 0xE9, 0xBA, 0xEE,
                0xCE, 0xA8, 0x40, 0x12,
                0x1B, 0xA0, 0x07, 0xF4 };
            byte[] cMac = new byte[8];

            UInt32 t = 16;
            CleanupAuthentication();
            isoWrapping = DF_ISO_WRAPPING_CARD;

            SetSesAuthENCKey(vcEncKey);
            Console.WriteLine("EncryptedStream " + BinConvert.ToHex(EncryptedStream, t));

            DeCipherSP80038A(vcEncKey, ref EncryptedStream, ref t, false);

            Console.WriteLine("DecryptedStream " + BinConvert.ToHex(EncryptedStream, t));

            for (byte bIndex = 0; bIndex < 16; bIndex++)
            {
                vcData[bIndex] = (byte)(EncryptedStream[bIndex] ^ abRndChal[bIndex]);
            }
            Console.WriteLine("VCdata " + BinConvert.ToHex(vcData, t));

            byte[] buffer = new byte[32];

            Array.ConstrainedCopy(abRndChal, 0, buffer, 0, 16);
            Array.ConstrainedCopy(vcData, 0, buffer, 16, 16);
            Console.WriteLine("MAC Input Data " + BinConvert.ToHex(buffer, 32));


            SetSesAuthMACKey(vcMacKey);
            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(buffer, (uint)buffer.Length, false, ref cMac);

            Console.WriteLine("MAC " + BinConvert.ToHex(cMac, 8));

            return 0;
        }

        /// <summary>
        /// presents an IID to the PD as defined in compliance with ISO/IEC 7816-4
        /// </summary>
        /// <param name="vcEncKey">[in] key used for ciphering</param>
        /// <param name="iid">[in] vc iid</param>
        /// <param name="size_iid">[in] size of vc iid</param>
        /// <param name="abRndChal">[out] </param>
        /// <param name="vcData">[out] VC data</param>
        /// <param name="SW">[out] ISO status</param>
        /// <returns></returns>
        long ISOSelect(byte[] vcEncKey,
                        byte[] iid, UInt32 size_iid,
                        ref byte[] abRndChal,
                        ref byte[] vcData,
                        ref UInt16 SW)
        {
            long status;
            UInt32 t = 16;
            int bIndex = 0;
            byte[] pFCI = new byte[64];
            UInt16 FCI_out_size = 64;

            CleanupAuthentication();
            isoWrapping = DF_ISO_WRAPPING_CARD;

            SetSesAuthENCKey(vcEncKey);
            status = IsoApdu(DF_CMD_ISOSVC, 0x04, 0x00, (byte)size_iid, iid, 0, ref pFCI, ref FCI_out_size, ref SW);
            if (status == DF_OPERATION_OK)
            {
                if (FCI_out_size == 2)
                {
                    /* Since no MAC is returned, this indicates AuthVCMandatory flag is not set and IsoSelect is success
                       * as per the ref arch(v 15) page no 283 */
                    FCI_out_size = 0;
                }
                else if (FCI_out_size == 36)
                {
#if _VERBOSE
                    Console.WriteLine("pFCI " + BinConvert.ToHex(pFCI, FCI_out_size));
#endif
                    /* FCI Length should be 36 bytes excluding 2 bytes of status data */
                    /* As per reference architecture, Handling the two response cases:
                     * Case-1: [if TargetVC != NULL AND TargetVC.AuthVCMandatory == true] and
                     * Case-2: [if TargetVC == NULL] in a single if-case since there is no way to get 'AuthVCMandatory' and 'targetted IID Supported' flag values */

                    /* 4 Bytes Header + 16 Bytes RndChal + 16 Bytes Payload + 2 Bytes SW1||SW2 (Total 38 Bytes returned) */

                    if ((pFCI[0] == 0x6F) &&
                       (pFCI[1] == 0x22) &&
                       (pFCI[2] == 0x85) &&
                       (pFCI[3] == 0x20))
                    {
                        Array.ConstrainedCopy(pFCI, 4, abRndChal, 0, 16);
                        Array.ConstrainedCopy(pFCI, 20, vcData, 0, 16);
                        //Array.ConstrainedCopy(pFCI, 20, ivData, 0, 16);

#if _VERBOSE
                        Console.WriteLine("abRndChal " + BinConvert.ToHex(abRndChal, 16));
                        Console.WriteLine("payload Encrypted " + BinConvert.ToHex(vcData, 16));
#endif

                        DeCipherSP80038A(vcEncKey, ref vcData, ref t, false);
                        /* Decrypted VCData is not plain text. It needs to be XORed with RndChal to obtain plain VCData */
                        for (bIndex = 0; bIndex < 16; bIndex++)
                        {
                            vcData[bIndex] = (byte)(vcData[bIndex] ^ abRndChal[bIndex]);
                        }

#if _VERBOSE
                        Console.WriteLine("abRndChal " + BinConvert.ToHex(abRndChal, 16));
                        Console.WriteLine("payload Decrypted " + BinConvert.ToHex(vcData, 16));
#endif
                    }
                }
                else
                {
                    /* Case-3: [if TargetVC != NULL AND TargetVC.AuthVCMandatory == false AND (IID is DESFire application DF name)]
                           * FCI[36] bytes shall be stored in file ID 31 of the DF */
                    //memcpy(pFCI, &pRecv[0], 36);
                }

            }
            status = TranslateSW(SW, 0x00);
            return status;
        }

        /// <summary>
        /// Depending on AuthVCMandatory, the PCD authenticates and confirms its intent to communicate with a given VC.
        /// </summary>
        /// <param name="vcMacKey">[in] key used for macing</param>
        /// <param name="abRndChal"></param>
        /// <param name="vcData">IsoSelect VC data</param>
        /// <param name="SW">ISO response</param>
        /// <returns></returns>
        long IsoExternalAuthenticate(byte[] vcMacKey,
              byte[] abRndChal,
              byte[] vcData,
              ref UInt16 SW)
        {
            long status;
            byte[] cmac = new byte[8];
            UInt16 data_out_local = 0;
            byte[] nullbyte = new byte[16];
            byte[] buffer = new byte[32];

            Array.ConstrainedCopy(abRndChal, 0, buffer, 0, 16);
            Array.ConstrainedCopy(vcData, 0, buffer, 16, 16);

#if _VERBOSE
            Console.WriteLine("#### IsoExternalAuthenticate' " + BinConvert.ToHex(buffer, 32));
#endif
            SetSesAuthMACKey(vcMacKey);
            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(buffer, (uint)buffer.Length, false, ref cmac);

            secure_mode = SecureMode.NO;

            if ((isoWrapping & DF_ISO_WRAPPING_CARD) != 0)
            {
                data_out_local = 0;
                status = IsoApdu(DF_CMD_ISOEXT_AUTH, 0x00, 0x00, (byte)0x08, cmac, 0, ref nullbyte, ref data_out_local, ref SW);

                status = TranslateSW(SW, 0x00);
            }
            else
            {
                return DFCARD_CONDITION_OF_USE;
            }
            return status;
        }

        /// <summary>
        ///  PICC is prepared to perform the Proximity Check by drawing the random number RndR.
        /// </summary>
        /// <param name="Option">[out] Option field defining subsequent response</param>
        /// <param name="minFDT">[out] Published response time</param>
        /// <param name="PPS1">[out] The 4 least significant bits indicate the divisor values used to compute the data rate for the PD to PCD and PCD to PD communications.</param>
        /// <param name="SW">[out] ISO status code</param>
        /// <returns></returns>
        long PreparePC(ref byte Option, ref UInt16 minFDT, ref byte PPS1, ref UInt16 SW)
        {
            long status = 0;
            UInt16 data_size = 32;
            byte[] buffer = new byte[32];

            if (isoWrapping == DF_ISO_WRAPPING_CARD)
            {
                status = IsoApduExt(0x90, DF_CMD_PPC, 0x00, 0x00, 0x00, null, 0, buffer, ref data_size, ref SW);
                if (SW == 0x9190)
                {
                    Option = buffer[0];
                    minFDT = (UInt16)(buffer[1] << 8);
                    minFDT |= (UInt16)(buffer[2] & 0x00FF);
                    status = TranslateSW(SW, 0x00);
                }
                else
                {
                    xfer_length = 0;
                    xfer_buffer[xfer_length++] = DF_CMD_PPC;
                    /* Send the command string to the PICC and get its response (1st frame exchange).
                     * The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
                    status = Command(0, WANTS_OPERATION_OK, false);
                    if (status != DF_OPERATION_OK)
                    {
                        return status;
                    }
                    if (xfer_length > 3)
                    {
                        Option = xfer_buffer[0];
                        minFDT = (UInt16)(xfer_buffer[1] << 8);
                        minFDT |= (UInt16)(xfer_buffer[2] & 0x00FF);
                        PPS1 = xfer_buffer[3];
                    }
                }
            }
            return status;
        }

        /// <summary>
        /// ProximityCheck : The PICC answers with a prepared random number at the published response time in PreparePC. This command may be
        /// repeated up to 8 times splitting the random number for different time measurements
        /// </summary>
        /// <param name="SW">[out] ISO status code</param>
        /// <returns></returns>
        long ProximityCheck(ref UInt16 SW)
        {
            long status = 0;
            byte Lc = 0x08;
            byte i = 0;
            byte[] tmp = new byte[8];

            UInt16 data_size = 0;
            byte[] slipt = new byte[2];
            double average = 0;

            pRndR = random.Get(Lc);

            for (i = 0; i < Lc; i++)
            {
                if ((isoWrapping & DF_ISO_WRAPPING_CARD) != 0)
                {
                    slipt[0] = 0x01;
                    slipt[1] = pRndR[i];
                    data_size = 8;
                    status = IsoApduExt(0x90, DF_CMD_PC, 0x00, 0x00, 0x02, slipt, 0x01, tmp, ref data_size, ref SW);
                    pRndC[i] = tmp[0];

                    status = TranslateSW(SW, 0x00);
                    if (status != DF_OPERATION_OK)
                        return status;

                    average += GET_BENCH();
                }
                else
                {
                    xfer_length = 0;
                    xfer_buffer[xfer_length++] = DF_CMD_PC;
                    xfer_buffer[xfer_length++] = 0x08;
                    xfer_buffer[xfer_length++] = pRndR[i];

                    /* Send the command string to the PICC and get its response (1st frame exchange).
                        The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
                    status = Command(0, WANTS_OPERATION_OK, false);
                    if (status != DF_OPERATION_OK)
                    {
                        return status;
                    }
                    pRndC[i] = xfer_buffer[1];
                    average += GET_BENCH();
                }
            }

            average /= 8;
            //
            measuredFDT = (UInt16)average;
            /* Measures time from Tx-end until Rx-begin. */
            if (minFDT < measuredFDT)
            {
                Console.WriteLine("Average time for transaction : {0} us expected %d measured {1}\r\n", average, minFDT, measuredFDT);
            }
            return status;
        }

        /// <summary>
        /// The ISOSelect command presents an IID to the PD as defined in compliance with ISO/IEC 7816-4.
        /// The IID is supported by the infrastructure containing the PCD.
        /// </summary>
        /// <param name="vcMacKey"> [in] mac proximity key</param>
        /// <param name="SW">[out] ISO status code</param>
        /// <returns></returns>
        long VerifyPC(byte[] vcMacKey, ref UInt16 SW)
        {
            long status;
            byte[] buffer = new byte[32];
            UInt16 data_size = 0;
            byte[] cmac = new byte[8];

            /* MACt(KVCProximityKey , VPC || Option || pubRespTime || PPS1 || (pRndR1 || pRndC1) || .. || (pRndRn || pRndCn)) */
            buffer[data_size++] = DF_CMD_VPC;
            buffer[data_size++] = (byte)((measuredFDT & 0xFF00) >> 8);
            buffer[data_size++] = (byte)((measuredFDT & 0x00FF));
            buffer[data_size++] = PPS1;
            buffer[data_size++] = pRndR[0]; buffer[data_size++] = pRndC[0];
            buffer[data_size++] = pRndR[1]; buffer[data_size++] = pRndC[1];
            buffer[data_size++] = pRndR[2]; buffer[data_size++] = pRndC[2];
            buffer[data_size++] = pRndR[3]; buffer[data_size++] = pRndC[3];
            buffer[data_size++] = pRndR[4]; buffer[data_size++] = pRndC[4];
            buffer[data_size++] = pRndR[5]; buffer[data_size++] = pRndC[5];
            buffer[data_size++] = pRndR[6]; buffer[data_size++] = pRndC[6];
            buffer[data_size++] = pRndR[7]; buffer[data_size++] = pRndC[7];

            SetSesAuthMACKey(vcMacKey);
            InitCmacEv2();
            session_type = KEY_ISO_AES;
            secure_mode = SecureMode.EV2;
            ComputeCmacEv2(buffer, (uint)data_size, false, ref cmac);

            secure_mode = SecureMode.NO;

            if ((isoWrapping & DF_ISO_WRAPPING_CARD) != 0)
            {
                data_size = 32;
                status = IsoApduExt(0x90, DF_CMD_VPC, 0x00, 0x00, 0x08, cmac, 0x08, buffer, ref data_size, ref SW);

                status = TranslateSW(SW, 0x00);
                if (status != DF_OPERATION_OK)
                    return status;
            }
            else
            {
                xfer_length = 0;
                xfer_buffer[xfer_length++] = DF_CMD_VPC;
                Array.ConstrainedCopy(cmac, 0, xfer_buffer, (int)xfer_length, 8);
                xfer_length += 8;


                /* Send the command string to the PICC and get its response (1st frame exchange).
				    The PICC has to respond with an DF_ADDITIONAL_FRAME status byte. */
                status = Command(0, WANTS_OPERATION_OK, false);
                if (status != DF_OPERATION_OK)
                {
                    return status;
                }
            }
            return status;
        }


        /// <summary>
        /// The ISOSelect command presents an IID to the PD as defined in compliance with ISO/IEC 7816-4.
        /// The IID is supported by the infrastructure containing the PCD.
        /// </summary>
        /// <param name="vcEncKey"></param>
        /// <param name="vcMacKey"></param>
        /// <param name="VCProximityKey"></param>
        /// <param name="iid"></param>
        /// <param name="size_iid"></param>
        /// <param name="bProxCheck"></param>
        /// <param name="SW"></param>
        /// <returns></returns>
        long VCSelect(
          byte[] vcEncKey,
          byte[] vcMacKey,
          byte[] VCProximityKey,
          ref byte[] iid,
          UInt32 size_iid,
          bool bProxCheck,
          ref UInt16 SW)
        {
            long status;
            byte[] vcdata = new byte[16];
            byte[] abRndChal = new byte[16];


            CleanupAuthentication();
            ISOTest(vcEncKey, vcMacKey);

            status = ISOSelect(vcEncKey, iid, size_iid, ref abRndChal, ref vcdata,  ref SW);

            if (status == DF_OPERATION_OK)
            {
                status = IsoExternalAuthenticate(vcMacKey, abRndChal, vcdata, ref SW);
                if (status != DF_OPERATION_OK)
                    return status;
            }

            if (bProxCheck == true)
            {
                status = PreparePC(ref Option, ref minFDT, ref PPS1, ref SW);
                if (status == DF_OPERATION_OK)
                {
                    status = ProximityCheck(ref SW);
                    if (status == DF_OPERATION_OK)
                        status = VerifyPC(VCProximityKey, ref SW);
                }
            }

            return status;
        }
        public long Check_VirtualCard(bool bProximityCheck)
        {
            byte[] key_test = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            UInt16 StatusWord = 0;

            byte[] PCDCapL3 = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
            //byte pcd_cap_len = 4;
            //byte info;

            byte[] data = new byte[16] { 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA, 0x00, 0xAA };
            //UInt32 data_size = 16;
            byte[] iid = new byte[16] { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] vc_enc_key = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] vc_mac_key = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] vc_prox_key = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            /* Check that we can run a Virtual Card selection with the card */
            /* ------------------------------------------------------------ */

            return VCSelect(vc_enc_key, vc_mac_key, vc_prox_key, ref iid, 16, bProximityCheck, ref StatusWord);

        }
    }
}
