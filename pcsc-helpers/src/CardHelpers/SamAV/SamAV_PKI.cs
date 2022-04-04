using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;
using SpringCard.LibCs.Crypto;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public const byte PKIKeyEntriesCount = 3;
        public const byte PKIPrivateKeyEntriesCount = 2;
        public const byte PKIPublicKeyEntriesCount = 2;

        public class PKIKeyEntry
        {
            /* PKI_SET */

            public bool PrivateKey;
            public bool EnableDumpPrivateKey;
            public bool DisableKeyEntry;
            public bool DisableEncryptDecrypt;            
            public bool DisableSignature;
            public bool EnableUpdateKeyEntries;
            public bool PrivateKeyHasCrt;

            /*key reference number of the PKI key entry*/
            public byte IPKI_KeyNo;
            /*key reference number of change entry key*/
            public byte IPKI_KeyNoCEK;
            /*key version of change entry key*/
            public byte IPKI_KeyVCEK;
            /*reference number of key usage counter*/
            public byte IPKI_RefNoKUC;
            /*the RSA key length: it shall be between 32 and 256 (bytes)*/
            public byte[] IPKI_NLen = new byte[2];
            /*the public exponent length: it shall be between 4 and 256 bytes, a 
             * multiple of 4 and shall not be greater than PKI_NLen bytes.*/
            public byte[] IPKI_eLen = new byte[2];
            

            /* the prime p length */
            public byte[] IPKI_pLen = new byte[2];
            /* the prime q length */
            public byte[] IPKI_qLen = new byte[2];
            /* the modulus N */
            public byte[] IPKI_N = null;

            /*(optional, present if P1 bit 0 is '1') the public exponent e: it must be 
             * an odd integer.Note that successful key generation is not
             * guaranteed if e is not prime.To have a low execution time, it is very
             * common to use a short prime for e, although in concern of security
             * aspects it shall not be too small. The value 65537 for PKI_e has
             * proven to be a good choice.*/
            public byte[] IPKI_e = null;

            /* the prime p */
            public byte[] IPKI_p = null;
            /* the prime q */
            public byte[] IPKI_q = null;

            /*the private exponent dP*/
            public byte[] IPKI_dP = null;
            /*the private exponent dQ*/
            public byte[] IPKI_dQ = null;
            /*the inverse p-1 mod q */
            public byte[] IPKI_ipq = null;

            public PKIKeyEntry()
            {

            }

            public PKIKeyEntry(PKIKeyEntry source)
            {
                this.SET_HI = source.SET_HI;
                this.SET_LO = source.SET_LO;
                this.IPKI_KeyNo = source.IPKI_KeyNo;
                this.IPKI_KeyNoCEK = source.IPKI_KeyNoCEK;
                this.IPKI_KeyVCEK = source.IPKI_KeyVCEK;
                this.IPKI_RefNoKUC = source.IPKI_RefNoKUC;
                Array.Copy(source.IPKI_NLen, 0, this.IPKI_NLen, 0, 2);
                Array.Copy(source.IPKI_eLen, 0, this.IPKI_eLen, 0, 2);
                Array.Copy(source.IPKI_pLen, 0, this.IPKI_pLen, 0, 2);
                Array.Copy(source.IPKI_qLen, 0, this.IPKI_qLen, 0, 2);
                if( source.IPKI_N != null)
                {
                    this.IPKI_N = new byte[source.IPKI_N.Length];
                    Array.Copy(source.IPKI_N, 0, this.IPKI_N, 0, source.IPKI_N.Length);
                }
                if (source.IPKI_e != null)
                {
                    this.IPKI_e = new byte[source.IPKI_e.Length];
                    Array.Copy(source.IPKI_e, 0, this.IPKI_e, 0, source.IPKI_e.Length);
                }
                if (source.IPKI_p != null)
                {
                    this.IPKI_p = new byte[source.IPKI_p.Length];
                    Array.Copy(source.IPKI_p, 0, this.IPKI_p, 0, source.IPKI_p.Length);
                }
                if (source.IPKI_q != null)
                {
                    this.IPKI_q = new byte[source.IPKI_q.Length];
                    Array.Copy(source.IPKI_q, 0, this.IPKI_q, 0, source.IPKI_q.Length);
                }
                if (source.IPKI_dP != null)
                {
                    this.IPKI_dP = new byte[source.IPKI_dP.Length];
                    Array.Copy(source.IPKI_dP, 0, this.IPKI_dP, 0, source.IPKI_dP.Length);
                }
                if (source.IPKI_dQ != null)
                {
                    this.IPKI_dQ = new byte[source.IPKI_dQ.Length];
                    Array.Copy(source.IPKI_dQ, 0, this.IPKI_dQ, 0, source.IPKI_dQ.Length);
                }
                if (source.IPKI_ipq != null)
                {
                    this.IPKI_ipq = new byte[source.IPKI_ipq.Length];
                    Array.Copy(source.IPKI_ipq, 0, this.IPKI_ipq, 0, source.IPKI_ipq.Length);
                }
            }

            [Flags]
            public enum ESET_LO : byte
            {
                PrivateKey = 0x01,
                EnableDumpPrivateKey = 0x02,
                DisableKeyEntry = 0x04,
                DisableEncryptDecrypt = 0x08,
                DisableSignature = 0x10,
                EnableRemoteUpdates = 0x20,
                PrivateKeyHasCrt = 0x40
            }

            public byte SET_LO
            {
                get
                {
                    byte result = 0;
                    if (PrivateKey)
                        result |= (byte)ESET_LO.PrivateKey;
                    if (EnableDumpPrivateKey)
                        result |= (byte)ESET_LO.EnableDumpPrivateKey;
                    if (DisableKeyEntry)
                        result |= (byte)ESET_LO.DisableKeyEntry;
                    if (DisableEncryptDecrypt)
                        result |= (byte)ESET_LO.DisableEncryptDecrypt;
                    if (DisableSignature)
                        result |= (byte)ESET_LO.DisableSignature;
                    if (EnableUpdateKeyEntries)
                        result |= (byte)ESET_LO.EnableRemoteUpdates;
                    if (PrivateKeyHasCrt)
                        result |= (byte)ESET_LO.PrivateKeyHasCrt;

                    result |= 0x40; /* CRT */
                    return result;
                }
                set
                {
                    PrivateKey = ((value & (byte)ESET_LO.PrivateKey) != 0);
                    EnableDumpPrivateKey = ((value & (byte)ESET_LO.EnableDumpPrivateKey) != 0);
                    DisableKeyEntry = ((value & (byte)ESET_LO.DisableKeyEntry) != 0);
                    DisableEncryptDecrypt = ((value & (byte)ESET_LO.DisableEncryptDecrypt) != 0);
                    DisableSignature = ((value & (byte)ESET_LO.DisableSignature) != 0);
                    EnableUpdateKeyEntries = ((value & (byte)ESET_LO.EnableRemoteUpdates) != 0);
                    PrivateKeyHasCrt = ((value & (byte)ESET_LO.PrivateKeyHasCrt) != 0);
                }
            }

            public byte SET_HI
            {
                get
                {
                    return 0;
                }
                set
                {

                }
            }

            public ushort SET
            {
                get
                {
                    return (ushort)((SET_HI << 8) | SET_LO);
                }
                set
                {
                    SET_HI = (byte)(value >> 8);
                    SET_LO = (byte)(value);
                }
            }
            public static PKIKeyEntry Deserialize(byte[] value)
            {
                PKIKeyEntry result = new PKIKeyEntry();

                /*if (value.Length < 60)
                    throw new ArgumentException(nameof(value));
                if (value.Length > 61)
                    throw new ArgumentException(nameof(value));*/

                int offset = 0;
//                offset += 3;
                //result.IPKI_KeyNo = value[offset++];

                result.SET_HI = value[offset++];
                result.SET_LO = value[offset++];
                result.IPKI_KeyNoCEK = value[offset++];
                result.IPKI_KeyVCEK = value[offset++];
                result.IPKI_RefNoKUC = value[offset++];

                result.IPKI_NLen[1] = value[offset++];
                result.IPKI_NLen[0] = value[offset++];
                
                result.IPKI_eLen[1] = value[offset++];
                result.IPKI_eLen[0] = value[offset++];

                result.IPKI_pLen[1] = value[offset++];
                result.IPKI_pLen[0] = value[offset++];

                result.IPKI_qLen[1] = value[offset++];
                result.IPKI_qLen[0] = value[offset++];

                int PKI_NLen = (((int)(result.IPKI_NLen[1]) << 8 ) | ((int)(result.IPKI_NLen[0])));
                int PKI_eLen = (((int)(result.IPKI_eLen[1]) << 8) | ((int)(result.IPKI_eLen[0])));
                int PKI_pLen = (((int)(result.IPKI_pLen[1]) << 8) | ((int)(result.IPKI_pLen[0])));
                int PKI_qLen = (((int)(result.IPKI_qLen[1]) << 8) | ((int)(result.IPKI_qLen[0])));

                if (value.Length >= (offset + PKI_NLen))
                {
                    result.IPKI_N = new byte[PKI_NLen];
                    Array.Copy(value, offset, result.IPKI_N, 0, PKI_NLen);
                    offset += PKI_NLen;
                    if (value.Length >= (offset + PKI_eLen))
                    {
                        result.IPKI_e = new byte[PKI_eLen];
                        Array.Copy(value, offset, result.IPKI_e, 0, PKI_eLen);
                        offset += PKI_eLen;
                        if (value.Length >= (offset + PKI_eLen))
                        {
                            result.IPKI_p = new byte[PKI_pLen];
                            Array.Copy(value, offset, result.IPKI_p, 0, PKI_pLen);
                            offset += PKI_pLen;
                            if (value.Length >= (offset + PKI_qLen))
                            {
                                result.IPKI_q = new byte[PKI_qLen];
                                Array.Copy(value, offset, result.IPKI_q, 0, PKI_qLen);
                                offset += PKI_qLen;
                                if (value.Length >= (offset + PKI_pLen))
                                {
                                    result.IPKI_dP = new byte[PKI_pLen];
                                    Array.Copy(value, offset, result.IPKI_dP, 0, PKI_pLen);
                                    offset += PKI_pLen;
                                    if (value.Length >= (offset + PKI_qLen))
                                    {
                                        result.IPKI_dQ = new byte[PKI_qLen];
                                        Array.Copy(value, offset, result.IPKI_dQ, 0, PKI_qLen);
                                        offset += PKI_qLen;
                                        if (value.Length >= (offset + PKI_qLen))
                                        {
                                            result.IPKI_ipq = new byte[PKI_qLen];
                                            Array.Copy(value, offset, result.IPKI_ipq, 0, PKI_qLen);
                                            offset += PKI_qLen;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return result;
            }
        }

        public class PKIPublicKeyEntry : PKIKeyEntry
        {
            public byte[] N;
            public byte[] E;

            public PKIPublicKeyEntry()
            {

            }

            public PKIPublicKeyEntry(PKIPublicKeyEntry source) : base(source)
            {
                this.N = source.N;
                this.E = source.E;
            }

            public PKIPublicKeyEntry(PKIKeyEntry keySettings) : base(keySettings)
            {
                this.N = keySettings.IPKI_N;
                this.E = keySettings.IPKI_e;
            }

            public PKIPublicKeyEntry(PKIKeyEntry keySettings, RSAPublicKey publicKey) : base(keySettings)
            {
                this.N = publicKey.N;
                this.E = publicKey.E;
            }

            public RSAPublicKey GetPublicKey()
            {
                RSAPublicKey result = new RSAPublicKey(this.N, this.E);
                return result;
            }

            protected bool IsValidPublicKey()
            {
                if (N == null)
                    return false;
                if (E == null)
                    return false;
                return true;
            }

            public byte[] GetImportPublicKeyCommand()
            {
                byte[] result = new byte[9 + N.Length + E.Length];
                int offset = 0;

                result[offset++] = SET_HI;
                result[offset++] = SET_LO;
                result[offset++] = IPKI_KeyNoCEK;
                result[offset++] = IPKI_KeyVCEK;
                result[offset++] = IPKI_RefNoKUC;
                result[offset++] = (byte)(N.Length / 0x0100);
                result[offset++] = (byte)(N.Length % 0x0100);
                result[offset++] = (byte)(E.Length / 0x0100);
                result[offset++] = (byte)(E.Length % 0x0100);

                int i;
                for (i = 0; i < N.Length; i++)
                    result[offset++] = N[i];
                for (i = 0; i < E.Length; i++)
                    result[offset++] = E[i];

                return result;
            }
        }

        public class PKIPrivateKeyEntry : PKIPublicKeyEntry
        {
            public byte[] P;
            public byte[] Q;
            public byte[] dP;
            public byte[] dQ;
            public byte[] qInv;

            public PKIPrivateKeyEntry()
            {

            }

            public PKIPrivateKeyEntry(PKIPublicKeyEntry source) : base(source)
            {
                this.P = source.IPKI_p;
                this.Q = source.IPKI_q;
                this.dP = source.IPKI_dP;
                this.dQ = source.IPKI_dQ;
                this.qInv = source.IPKI_ipq;
            }

            public PKIPrivateKeyEntry(PKIPrivateKeyEntry source) : base(source)
            { 
                this.P = source.P;
                this.Q = source.Q;
                this.dP = source.dP;
                this.dQ = source.dQ;
                this.qInv = source.qInv;
            }

            public PKIPrivateKeyEntry(PKIKeyEntry keySettings, RSAPrivateKey privateKey) : base(keySettings)
            {
                this.E = privateKey.E;
                this.N = privateKey.N;
                this.P = privateKey.P;
                this.Q = privateKey.Q;
                this.dP = privateKey.dP;
                this.dQ = privateKey.dQ;
                this.qInv = privateKey.iPQ;
            }

            public RSAPrivateKey GetPrivateKey()
            {
                RSAPrivateKey result = new RSAPrivateKey(this.P, this.Q, this.E);
                return result;
            }

            protected bool IsValidPrivateKey()
            {
                if (!IsValidPublicKey())
                    return false;
                if (P == null)
                    return false;
                if (Q == null)
                    return false;
                if (dP == null)
                    return false;
                if (dQ == null)
                    return false;
                if (qInv == null)
                    return false;
                return true;
            }

            public byte[] GetImportPrivateKeyCommand()
            {
                byte[] result = new byte[13 + N.Length + E.Length + 2 * P.Length + 3 * Q.Length];
                int offset = 0;

                result[offset++] = SET_HI;
                result[offset++] = SET_LO;
                result[offset++] = IPKI_KeyNoCEK;
                result[offset++] = IPKI_KeyVCEK;
                result[offset++] = IPKI_RefNoKUC;
                result[offset++] = (byte)(N.Length / 0x0100);
                result[offset++] = (byte)(N.Length % 0x0100);
                result[offset++] = (byte)(E.Length / 0x0100);
                result[offset++] = (byte)(E.Length % 0x0100);
                result[offset++] = (byte)(P.Length / 0x0100);
                result[offset++] = (byte)(P.Length % 0x0100);
                result[offset++] = (byte)(Q.Length / 0x0100);
                result[offset++] = (byte)(Q.Length % 0x0100);

                int i;
                for (i = 0; i < N.Length; i++)
                    result[offset++] = N[i];
                for (i = 0; i < E.Length; i++)
                    result[offset++] = E[i];
                for (i = 0; i < P.Length; i++)
                    result[offset++] = P[i];
                for (i = 0; i < Q.Length; i++)
                    result[offset++] = Q[i];
                for (i = 0; i < dP.Length; i++)
                    result[offset++] = dP[i];
                for (i = 0; i < dQ.Length; i++)
                    result[offset++] = dQ[i];
                for (i = 0; i < qInv.Length; i++)
                    result[offset++] = qInv[i];

                return result;
            }
        }

        public bool PKIGenerateKeyPair(byte KeyIdx, PKIKeyEntry KeyEntry, int PrivateKeyBytes = 256, int PublicExponentBytes = 4, ulong PublicExponentValue = 0x00010001)
        {
            byte[] data = new byte[10 + PublicExponentBytes];
            int offset = 0;

            KeyEntry.PrivateKey = true;
            KeyEntry.PrivateKeyHasCrt = true;

            data[offset++] = KeyIdx;
            data[offset++] = KeyEntry.SET_HI;
            data[offset++] = KeyEntry.SET_LO;
            data[offset++] = KeyEntry.IPKI_KeyNoCEK;
            data[offset++] = KeyEntry.IPKI_KeyVCEK;
            data[offset++] = KeyEntry.IPKI_RefNoKUC;
            data[offset++] = (byte)(PrivateKeyBytes / 0x0100);
            data[offset++] = (byte)(PrivateKeyBytes % 0x0100);
            data[offset++] = (byte)(PublicExponentBytes / 0x0100);
            data[offset++] = (byte)(PublicExponentBytes % 0x0100);

            ulong t = PublicExponentValue;
            for (int i=0; i<PublicExponentBytes; i++)
            {
                data[offset + PublicExponentBytes - i - 1] = (byte) (t % 0x0100);
                t /= 0x0100;
            }

            if (Command(INS.PKIGenerateKeyPair, 0x01, 0x00, data) != ResultE.Success)
                return false;
            return true;
        }

        public bool PKIExportPublicKey(byte KeyIdx, out PKIPublicKeyEntry KeyEntry)
        {
            bool first = true;
            int sizeModulus = 0, offsetModulus = 0;
            int sizeExponent = 0, offsetExponent = 0;

            KeyEntry = null;

            for (; ; )
            {
                int offset = 0;
                byte[] data;

                ResultE result;

                if (first)
                {
                    /* Provide KeyIdx in P1 */
                    result = Command(INS.PKIExportPublicKey, KeyIdx, 0x00, out data, ExpectE.SuccessOrContinue);
                }
                else
                {
                    /* Chaining - do not provide KeyIdx */
                    result = Command(INS.PKIExportPublicKey, 0x00, 0x00, out data, ExpectE.SuccessOrContinue);
                }


                if ((result != ResultE.Success) && (result != ResultE.Continue))
                    return false;

                if (first)
                {
                    first = false;

                    if ((data == null) || (data.Length < 9))
                    {
                        LastError = ResultE.InvalidResponseData;
                        return false;
                    }

                    /* Read pki_entry */
                    KeyEntry = new PKIPublicKeyEntry();
                    KeyEntry.SET_HI = data[offset++];
                    KeyEntry.SET_LO = data[offset++];
                    KeyEntry.IPKI_KeyNoCEK = data[offset++];
                    KeyEntry.IPKI_KeyVCEK = data[offset++];
                    KeyEntry.IPKI_RefNoKUC = data[offset++];

                    /* Read sizes */
                    sizeModulus = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.N = new byte[sizeModulus];
                    sizeExponent = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.E = new byte[sizeExponent];
                }
                else
                {
                    if (data == null)
                    {
                        LastError = ResultE.InvalidResponseData;
                        return false;
                    }
                }

                /* Read the data bytes */
                while (offset < data.Length)
                {
                    if (offsetModulus < sizeModulus)
                    {
                        KeyEntry.N[offsetModulus++] = data[offset++];
                    }
                    else if (offsetExponent < sizeExponent)
                    {
                        KeyEntry.E[offsetExponent++] = data[offset++];
                    }
                    else
                    {
                        LastError = ResultE.InvalidResponseData;
                        return false;
                    }
                }

                if (result == ResultE.Success)
                {
                    KeyEntry.IPKI_N = new byte[KeyEntry.N.Length];
                    KeyEntry.IPKI_e = new byte[KeyEntry.E.Length];
                    Array.Copy(KeyEntry.N, 0, KeyEntry.IPKI_N, 0, KeyEntry.N.Length);
                    Array.Copy(KeyEntry.E, 0, KeyEntry.IPKI_e, 0, KeyEntry.E.Length);
                    return true;
                }
            }
        }

        private bool PKIImportKeyEx(byte P1, byte KeyIdx, byte[] data)
        {
            data = BinUtils.Concat(KeyIdx, data);
            return PKIImportKeyEx(P1, data);
        }

        private bool PKIImportKeyEx(byte P1, byte[] data)
        {
            int offset = 0;

            while (offset < data.Length)
            {
                int length = data.Length - offset;
                if (length > 255)
                    length = 255;
                byte[] buffer = BinUtils.Copy(data, offset, length);

                if (length < 255)
                {
                    /* Last block */
                    if (Command(INS.PKIImportKey, P1, 0x00, buffer, ExpectE.Success) != ResultE.Success)
                        return false;
                }
                else
                {
                    /* Chaining */
                    if (Command(INS.PKIImportKey, P1, 0xAF, buffer, ExpectE.Continue) != ResultE.Continue)
                        return false;
                    /* Dont repeat P1 in case of chaining */
                    P1 = 0x00;
                }

                offset += length;
            }

            return true;
        }

        public bool PKIImportPublicKey(byte KeyIdx, PKIPublicKeyEntry KeyEntry)
        {
            return PKIImportKeyEx(0x00, KeyIdx, KeyEntry.GetImportPublicKeyCommand());
        }

        public bool PKIExportPrivateKey(byte KeyIdx, out PKIPrivateKeyEntry KeyEntry)
        {
            bool first = true;
            int sizeModulus = 0, offsetModulus = 0;
            int sizeExponent = 0, offsetExponent = 0;
            int sizeP = 0, offsetP = 0, offsetDP = 0;
            int sizeQ = 0, offsetQ = 0, offsetDQ = 0, offsetQInv = 0;

            KeyEntry = null;

            for (; ; )
            {
                int offset = 0;
                byte[] data;

                ResultE result;

                if (first)
                {
                    /* Provide KeyIdx in P1 */
                    result = Command(INS.PKIExportPrivateKey, KeyIdx, 0x00, out data, ExpectE.SuccessOrContinue);
                }
                else
                {
                    /* Chaining - do not provide KeyIdx */
                    result = Command(INS.PKIExportPrivateKey, 0x00, 0x00, out data, ExpectE.SuccessOrContinue);
                }

                if ((result != ResultE.Success) && (result != ResultE.Continue))
                    return false;

                if (first)
                {
                    first = false;

                    if ((data == null) || (data.Length < 9))
                    {
                        LastError = ResultE.InvalidResponseData;
                        return false;
                    }

                    /* Read pki_entry */
                    KeyEntry = new PKIPrivateKeyEntry();
                    KeyEntry.SET_HI = data[offset++];
                    KeyEntry.SET_LO = data[offset++];
                    KeyEntry.IPKI_KeyNoCEK = data[offset++];
                    KeyEntry.IPKI_KeyVCEK = data[offset++];
                    KeyEntry.IPKI_RefNoKUC = data[offset++];

                    /* Read sizes */
                    sizeModulus = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.N = new byte[sizeModulus];
                    sizeExponent = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.E = new byte[sizeExponent];
                    sizeP = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.P = new byte[sizeP];
                    KeyEntry.dP = new byte[sizeP];
                    sizeQ = (data[offset++] * 0x0100) | data[offset++];
                    KeyEntry.Q = new byte[sizeQ];
                    KeyEntry.dQ = new byte[sizeQ];
                    KeyEntry.qInv = new byte[sizeQ];
                }
                else
                {
                    if (data == null)
                    {
                        LastError = ResultE.InvalidResponseData;
                        return false;
                    }
                }

                /* Read the data bytes */
                while (offset < data.Length)
                {
                    if (offsetModulus < sizeModulus)
                    {
                        KeyEntry.N[offsetModulus++] = data[offset++];
                    }
                    else if (offsetExponent < sizeExponent)
                    {
                        KeyEntry.E[offsetExponent++] = data[offset++];
                    }
                    else if (offsetP < sizeP)
                    {
                        KeyEntry.P[offsetP++] = data[offset++];
                    }
                    else if (offsetQ < sizeQ)
                    {
                        KeyEntry.Q[offsetQ++] = data[offset++];
                    }
                    else if (offsetDP < sizeP)
                    {
                        KeyEntry.dP[offsetDP++] = data[offset++];
                    }
                    else if (offsetDQ < sizeQ)
                    {
                        KeyEntry.dQ[offsetDQ++] = data[offset++];
                    }
                    else if (offsetQInv < sizeQ)
                    {
                        KeyEntry.qInv[offsetQInv++] = data[offset++];
                    }
                    else
                    {
                        LastError = ResultE.InvalidResponseData;
                        return false;
                    }
                }

                if (result == ResultE.Success)
                {
                    KeyEntry.IPKI_N = new byte[KeyEntry.N.Length];
                    KeyEntry.IPKI_e = new byte[KeyEntry.E.Length];
                    Array.Copy(KeyEntry.N, 0, KeyEntry.IPKI_N, 0, KeyEntry.N.Length);
                    Array.Copy(KeyEntry.E, 0, KeyEntry.IPKI_e, 0, KeyEntry.E.Length);

                    KeyEntry.IPKI_p = new byte[KeyEntry.P.Length];
                    KeyEntry.IPKI_q = new byte[KeyEntry.Q.Length];
                    KeyEntry.IPKI_dP = new byte[KeyEntry.dP.Length];
                    KeyEntry.IPKI_dQ = new byte[KeyEntry.dQ.Length];
                    KeyEntry.IPKI_ipq= new byte[KeyEntry.qInv.Length];

                    Array.Copy(KeyEntry.P, 0, KeyEntry.IPKI_p, 0, KeyEntry.P.Length);
                    Array.Copy(KeyEntry.Q, 0, KeyEntry.IPKI_q, 0, KeyEntry.Q.Length);
                    Array.Copy(KeyEntry.dP, 0, KeyEntry.IPKI_dP, 0, KeyEntry.dP.Length);
                    Array.Copy(KeyEntry.dQ, 0, KeyEntry.IPKI_dQ, 0, KeyEntry.dQ.Length);
                    Array.Copy(KeyEntry.qInv, 0, KeyEntry.IPKI_ipq, 0, KeyEntry.qInv.Length);
                    return true;
                }
            }
        }

        public bool PKIImportPrivateKey(byte KeyIdx, PKIPrivateKeyEntry KeyEntry)
        {
            return PKIImportKeyEx(0x00, KeyIdx, KeyEntry.GetImportPrivateKeyCommand());
        }

        public enum EPKIHash : byte
        {
            SHA1 = 0x00,
            SHA224 = 0x01,
            SHA256 = 0x03
        }

        public bool PKIGenerateHash(EPKIHash HashMode, byte[] Message, out byte[] Digest)
        {
            Digest = null;
            byte P1 = (byte)HashMode;
            byte[] data = BinUtils.FromDword((uint) Message.Length, BinUtils.Endianness.BigEndian);
            data = BinUtils.Concat(data, Message);
            int offset = 0;

            while (offset < data.Length)
            {
                int length = data.Length - offset;
                if (length > 255)
                    length = 255;
                byte[] buffer = BinUtils.Copy(data, offset, length);

                if (length < 255)
                {
                    /* Last block */
                    if (Command(INS.PKIGenerateHash, P1, 0x00, buffer, true, out Digest, ExpectE.Success) != ResultE.Success)
                        return false;
                }
                else
                {
                    /* Chaining */
                    if (Command(INS.PKIGenerateHash, P1, 0xAF, buffer, ExpectE.Continue) != ResultE.Continue)
                        return false;
                    /* Dont repeat P1 in case of chaining */
                    P1 = 0x00;
                }

                offset += length;
            }

            return true;
        }

        public bool PKIGenerateSignature(byte KeyIdx, EPKIHash HashMode, byte[] Digest, out byte[] Signature)
        {
            Signature = null;
            byte P1 = (byte)HashMode;
            byte[] data = new byte[1 + Digest.Length];
            data[0] = KeyIdx;
            Array.Copy(Digest, 0, data, 1, Digest.Length);

            if (Command(INS.PKIGenerateSignature, P1, 0x00, data, ExpectE.Success) != ResultE.Success)
                return false;

            if (Command(INS.PKISendSignature, 0x00, 0x00, out Signature, ExpectE.Success) != ResultE.Success)
                return false;

            return true;
        }

        public bool PKIVerifySignature(byte KeyIdx, EPKIHash HashMode, byte[] Digest, byte[] Signature, out bool Valid)
        {
            Valid = false;
            byte P1 = (byte)HashMode;

            byte[] data = new byte[1 + Digest.Length + Signature.Length];
            data[0] = KeyIdx;
            Array.Copy(Digest, 0, data, 1, Digest.Length);
            Array.Copy(Signature, 0, data, 1 + Digest.Length, Signature.Length);

            int offset = 0;

            while (offset < data.Length)
            {
                int length = data.Length - offset;
                if (length > 255)
                    length = 255;
                byte[] buffer = BinUtils.Copy(data, offset, length);

                if (length < 255)
                {
                    /* Last block */
                    ResultE result = Command(INS.PKIVerifySignature, P1, 0x00, buffer, false, out ushort SW);
                    if (result != ResultE.Success)
                        return false;

                    if (SW == 0x9000)
                        Valid = true;
                }
                else
                {
                    /* Chaining */
                    ResultE result = Command(INS.PKIVerifySignature, P1, 0xAF, buffer, ExpectE.Continue);
                    if (result != ResultE.Continue)
                        return false;
                    /* Dont repeat P1 in case of chaining */
                    P1 = 0x00;
                }

                offset += length;
            }

            return true;
        }
    }
}
