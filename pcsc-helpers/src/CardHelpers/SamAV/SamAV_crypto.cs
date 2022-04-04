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
        public static class CryptoPrimitives
        {
            public static byte[] TripleDES_Decrypt(byte[] CypherText, byte[] key, byte[] iv)
            {
                byte[] result = new byte[CypherText.Length];
                TripleDES tripleDESalg = TripleDES.Create();
                TripleDESCryptoServiceProvider tDes = tripleDESalg as TripleDESCryptoServiceProvider;
                MethodInfo mi = tDes.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                tDes.Mode = CipherMode.CBC;
                tDes.Padding = PaddingMode.Zeros;
                object[] Par = { key, tDes.Mode, iv, tDes.FeedbackSize, 64 }; //64
                ICryptoTransform trans = mi.Invoke(tDes, Par) as ICryptoTransform;
                trans.TransformBlock(CypherText, 0, CypherText.Length, result, 0);

                return result;
            }

            public static byte[] TripleDES_Encrypt(byte[] Plaintext, byte[] key, byte[] iv)
            {
                byte[] result = new byte[Plaintext.Length];

                TripleDES tripleDESalg = TripleDES.Create();
                TripleDESCryptoServiceProvider tDes = tripleDESalg as TripleDESCryptoServiceProvider;
                MethodInfo mi = tDes.GetType().GetMethod("_NewEncryptor", BindingFlags.NonPublic | BindingFlags.Instance);
                tDes.Mode = CipherMode.CBC;
                object[] Par = { key, tDes.Mode, iv, tDes.FeedbackSize, 0 };
                ICryptoTransform trans = mi.Invoke(tDes, Par) as ICryptoTransform;
                trans.TransformBlock(Plaintext, 0, Plaintext.Length, result, 0);

                return result;
            }

            public static byte[] AES_Decrypt(byte[] CypherText, byte[] key, byte[] iv)
            {
                AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
                aesCSP.BlockSize = 128;
                aesCSP.Key = key;
                aesCSP.IV = iv;
                aesCSP.Padding = PaddingMode.Zeros;
                aesCSP.Mode = CipherMode.CBC;

                ICryptoTransform xfrm = aesCSP.CreateDecryptor(key, iv);
                byte[] result = xfrm.TransformFinalBlock(CypherText, 0, CypherText.Length);

                return result;

            }

            public static byte[] AES_Encrypt(byte[] PlainText, byte[] key, byte[] iv)
            {
                AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
                aesCSP.BlockSize = 128;
                aesCSP.Key = key;
                aesCSP.IV = iv;
                aesCSP.Padding = PaddingMode.Zeros;
                aesCSP.Mode = CipherMode.CBC;

                ICryptoTransform xfrm = aesCSP.CreateEncryptor(key, iv);
                byte[] result = xfrm.TransformFinalBlock(PlainText, 0, PlainText.Length);

                return result;
            }

            public static byte[] RotateLeft(byte[] buffer, int count)
            {
                for (int i = 0; i < count; i++)
                    buffer = RotateLeft(buffer);
                return buffer;
            }

            public static byte[] RotateLeft(byte[] buffer)
            {
                byte[] result = new byte[buffer.Length];
                for (int i = 0; i < (buffer.Length) - 1; i++)
                    result[i] = buffer[i + 1];
                result[buffer.Length - 1] = buffer[0];
                return result;
            }

            public static byte[] RotateRight(byte[] buffer)
            {
                byte[] result = new byte[buffer.Length];
                for (int i = 0; i < (buffer.Length) - 1; i++)
                    result[i + 1] = buffer[i];
                result[0] = buffer[buffer.Length - 1];
                return result;
            }


            public static byte[] ComputeCrc16(byte[] buffer)
            {
                byte chBlock;
                ushort wCrc = 0x6363; /* ITU-V.41 */
                byte[] crc = new byte[2];

                for (int i = 0; i < buffer.Length; i++)
                {
                    chBlock = (byte)(buffer[i] ^ (byte)((wCrc) & 0x00FF));
                    chBlock = (byte)(chBlock ^ ((chBlock << 4)) & 0x00FF);
                    wCrc = (ushort)((wCrc >> 8) ^ ((ushort)chBlock << 8) ^ ((ushort)chBlock << 3) ^ ((ushort)chBlock >> 4));
                }
                crc[0] = (byte)(wCrc & 0x00FF);
                crc[1] = (byte)(wCrc >> 8);
                return crc;
            }


            public static byte[] ComputeCrc32(byte[] buffer)
            {
                UInt32 dwCrc = 0xFFFFFFFF;
                byte[] crc = new byte[4];

                for (int i = 0; i < buffer.Length; i++)
                {
                    dwCrc ^= buffer[i];
                    for (byte b = 0; b < 8; b++)
                    {
                        if ((dwCrc & 0x00000001) == 0x00000001)
                        {
                            dwCrc >>= 1;
                            dwCrc ^= 0xEDB88320;
                        }
                        else
                        {
                            dwCrc >>= 1;
                        }
                    }
                }

                crc[0] = (byte)(dwCrc & 0x000000FF);
                crc[1] = (byte)((dwCrc >> 8) & 0x000000FF);
                crc[2] = (byte)((dwCrc >> 16) & 0x000000FF);
                crc[3] = (byte)((dwCrc >> 24) & 0x000000FF);
                return crc;
            }

            public class CMAC
            {
                private const byte rb_xor_value = 0x87;
                private const byte block_size = 16;

                private byte[] Key;
                private byte[] SubKey1;
                private byte[] SubKey2;

                public CMAC(byte[] Key)
                {
                    this.Key = Key;
                    ComputeSubKeys();
                }

                public byte[] Compute8(byte[] Input, byte[] InitVector = null)
                {
                    byte[] Complete = Compute(Input, InitVector);
                    byte[] Output = new byte[8];
                    for (int i = 0; i < 8; i++)
                        Output[i] = Complete[2 * i + 1];

                    Logger.Debug("\tShort CMAC={0}", BinConvert.ToHex(Output));
                    return Output;
                }

                public byte[] Compute(byte[] Input, byte[] InitVector = null)
                {
                    if (Input == null)
                        Input = new byte[0];
                    if (InitVector == null)
                        InitVector = new byte[16];

                    Logger.Debug("CMAC compute");
                    Logger.Debug("\tInput={0}", BinConvert.ToHex(Input));
                    Logger.Debug("\tInitVector={0}", BinConvert.ToHex(InitVector));


                    int i, actual_length;

                    /*
                    {
                      Console.Write("Data to CMAC over: ");
                      for (int k=0; k<length; k++)
                        Console.Write(String.Format("{0:x02}", data[k]));
                      Console.Write("\n");
                    }
                    */

                    // Adapt the crypto mode if the sessionkey is done in CBC_Send_Decrypt:
                    // enCryptoMethod = (m_SessionKeyCryptoMethod == CRM_3DES_DF4 ? CRM_3DES_ISO:m_SessionKeyCryptoMethod);

                    // First we enlarge eNumOfBytes to a multiple of the cipher block size for allocating
                    // memory of the intermediate buffer. Zero padding will be done by the DF8Encrypt function.
                    // If we are ISO-authenticated, we have to do the spetial padding for the O-MAC:
                    if (Input.Length == 0)
                    {
                        actual_length = 16;
                    }
                    else
                    {
                        actual_length = Input.Length;
                        while ((actual_length % block_size) != 0)
                            actual_length++;
                    }

                    byte[] buffer = new byte[actual_length];
                    for (i = 0; i < actual_length; i++)
                        buffer[i] = 0;

                    Array.Copy(Input, 0, buffer, 0, Input.Length);

                    /*
                    {
                      Console.Write("Before padding, buffer: ");
                      for (int k=0; k<actual_length; k++)
                        Console.Write(String.Format("{0:x02}", buffer[k]));
                      Console.Write("\n");
                    }
                    */

                    /* Do the ISO padding and/or XORing */
                    if ((Input.Length == 0) || (Input.Length % block_size) != 0)
                    {
                        /* Block incomplete -> padding */
                        buffer[Input.Length] = 0x80;

                        Logger.Debug("Padded input={0}", BinConvert.ToHex(buffer));
                        Logger.Debug("XOR last block with SubKey2");

                        /*
                        {
                          Console.Write("after padding, buffer: ");
                          for (int k=0; k<actual_length; k++)
                            Console.Write(String.Format("{0:x02}", buffer[k]));
                          Console.Write("\n");
                        }
                        */

                        /* XOR the last eight bytes with CMAC_SubKey2 */
                        for (i = 0; i < block_size; i++)
                        {
                            buffer[buffer.Length - block_size + i] ^= (byte)(SubKey2[i]);
                        }
                    }
                    else
                    {
                        /* Block complete -> no padding */
                        Logger.Debug("No padding required - XOR last block with SubKey1");

                        /* XOR the last eight bytes with CMAC_SubKey1 */
                        for (i = 0; i < block_size; i++)
                            buffer[buffer.Length - block_size + i] ^= (byte)(SubKey1[i]);
                    }
                    Logger.Debug("\tPlain buffer={0}", BinConvert.ToHex(buffer));

                    /*
                    {
                      Console.Write("After padding, buffer: ");
                      for (int k=0; k<actual_length; k++)
                        Console.Write(String.Format("{0:x02}", buffer[k]));
                      Console.Write("\n");
                    }
                    */

                    buffer = AES_Encrypt(buffer, Key, InitVector);
                    Logger.Debug("\tCrypted buffer={0}", BinConvert.ToHex(buffer));

                    byte[] Output = new byte[16];
                    Array.Copy(buffer, buffer.Length - 16, Output, 0, 16);
                    Logger.Debug("\tOutput={0}", BinConvert.ToHex(Output));
                    return Output;
                }

                void ComputeSubKeys()
                {
                    Logger.Debug("CMAC init");
                    Logger.Debug("\tKey={0}", KeyToString(Key));

                    byte bMSB;
                    uint i;

                    SubKey1 = new byte[block_size];
                    SubKey2 = new byte[block_size];

                    // Generate the padding bytes for O-MAC by enciphering a zero block
                    // with the actual session key:
                    for (i = 0; i < SubKey1.Length; i++)
                        SubKey1[i] = 0;

                    /*
                    {
                      Console.Write("Before CipherSend, cmac_subkey_1=");
                      for (int k=0; k<t; k++)
                        Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
                      Console.Write("\n");
                    }
                    */

                    SubKey1 = AES_Encrypt(SubKey1, Key, new byte[16]);

                    /*
                    {
                      Console.Write("After CipherSend, cmac_subkey_1=");
                      for (int k=0; k<t; k++)
                        Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
                      Console.Write("\n");
                    }
                    */

                    // If the MSBit of the generated cipher == 1 -> K1 = (cipher << 1) ^ Rb ...
                    // store MSB:
                    bMSB = SubKey1[0];

                    // Shift the complete cipher for 1 bit ==> K1:
                    byte tmp;
                    for (i = 0; i < (UInt32)(block_size - 1); i++)
                    {
                        tmp = (byte)((SubKey1[i] << 1) & 0x00FE);
                        SubKey1[i] = tmp;

                        // add the carry over bit:
                        SubKey1[i] |= (byte)(((SubKey1[i + 1] & 0x80) != 0) ? 0x01 : 0x00);
                    }

                    tmp = (byte)((SubKey1[block_size - 1] << 1) & 0x00FE);
                    SubKey1[block_size - 1] = tmp;
                    if ((bMSB & 0x80) != 0)
                    {
                        // XOR with Rb:
                        SubKey1[block_size - 1] ^= rb_xor_value;
                    }

                    /*
                    {
                      Console.Write("After shift, cmac_subkey_1=");
                      for (int k=0; k<t; k++)
                        Console.Write(String.Format("{0:x02}", cmac_subkey_1[k]));
                      Console.Write("\n");
                    }
                    */

                    // store MSB:
                    bMSB = SubKey1[0];

                    // Shift K1 ==> K2:
                    for (i = 0; i < (UInt32)(block_size - 1); i++)
                    {
                        SubKey2[i] = (byte)((SubKey1[i] << 1) & 0x00FE);
                        SubKey2[i] |= (byte)(((SubKey1[i + 1] & 0x80) != 0) ? 0x01 : 0x00);
                    }
                    SubKey2[block_size - 1] = (byte)((SubKey1[block_size - 1] << 1) & 0x00FE);

                    if ((bMSB & 0x80) == 0x80)
                    {
                        // XOR with Rb:
                        SubKey2[block_size - 1] ^= rb_xor_value;
                    }

                    /*
                    {
                      Console.Write("After shift, cmac_subkey_2=");
                      for (int k=0; k<t; k++)
                        Console.Write(String.Format("{0:x02}", cmac_subkey_2[k]));
                      Console.Write("\n");
                    }
                    */


                    Logger.Debug("\tSubKey1={0}", KeyToString(SubKey1));
                    Logger.Debug("\tSubKey2={0}", KeyToString(SubKey2));
                }

                public static void SelfTest()
                {
                    CMAC c = new CMAC(BinConvert.HexToBytes("2B7E1516 28AED2A6 ABF71588 09CF4F3C"));
                    
                    if (!BinUtils.Equals(c.Compute(new byte[0]), BinConvert.HexToBytes("BB1D6929 E9593728 7FA37D12 9B756746")))
                        goto failed;
                    if (!BinUtils.Equals(c.Compute(BinConvert.HexToBytes("6BC1BEE2 2E409F96 E93D7E11 7393172A")), BinConvert.HexToBytes("070A16B4 6B4D4144 F79BDD9D D04A287C")))
                        goto failed;
                    if (!BinUtils.Equals(c.Compute(BinConvert.HexToBytes("6BC1BEE2 2E409F96 E93D7E11 7393172A AE2D8A57")), BinConvert.HexToBytes("7D85449E A6EA19C8 23A7BF78 837DFADE")))
                        goto failed;
                    if (!BinUtils.Equals(c.Compute(BinConvert.HexToBytes("6BC1BEE2 2E409F96 E93D7E11 7393172A AE2D8A57 1E03AC9C 9EB76FAC 45AF8E51 30C81C46 A35CE411 E5FBC119 1A0A52EF F69F2445 DF4F9B17 AD2B417B E66C3710")), BinConvert.HexToBytes("51F0BEBF 7E3B9D92 FC497417 79363CFE")))
                        goto failed;

                    return;

                failed:
                    throw new Exception("CMAC SelfTest failed");
                }
            }


            public static byte[] CalculateCMAC(byte[] Key, byte[] IV, byte[] input)
            {
                CMAC cmac = new CMAC(Key);
                return cmac.Compute(input, IV);

                Logger.Debug("CMAC");
                Logger.Debug("\tKey={0}", KeyToString(Key));
                Logger.Debug("\tIV={0}", BinConvert.ToHex(IV));
                Logger.Debug("\tInput={0}", BinConvert.ToHex(input));

                // First : calculate subkey1 and subkey2
                byte[] Zeros = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                //byte[] K = { 0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c } ;

                byte[] L = AES_Encrypt(Zeros, Key, IV);
                Logger.Debug("L={0}", KeyToString(L));


                byte[] SubKey1;
                byte[] SubKey2;
                int i = 0;
                byte Rb = 0x87;
                byte MSB_L = L[0];
                UInt32 decal;

                // calcul de Key 1
                for (i = 0; i < L.Length - 1; i++)
                {
                    decal = (UInt32)(L[i] << 1);
                    L[i] = (byte)(decal & 0x00FF);
                    if ((L[i + 1] & 0x80) == 0x80)
                    {
                        L[i] |= 0x01;
                    }
                    else
                    {
                        L[i] |= 0x00;
                    }
                }

                decal = (UInt32)(L[i] << 1);
                L[i] = (byte)(decal & 0x00FF);

                if (MSB_L >= 0x80)
                    L[L.Length - 1] ^= Rb;

                SubKey1 = L;
                Logger.Debug("SubKey1={0}", KeyToString(SubKey1));

                byte[] tmp = new byte[SubKey1.Length];
                for (int k = 0; k < SubKey1.Length; k++)
                    tmp[k] = SubKey1[k];

                // Calcul de key 2
                byte MSB_K1 = SubKey1[0];
                for (i = 0; i < L.Length - 1; i++)
                {
                    decal = (UInt32)(tmp[i] << 1);
                    tmp[i] = (byte)(decal & 0x00FF);
                    if ((tmp[i + 1] & 0x80) == 0x80)
                    {
                        tmp[i] |= 0x01;
                    }
                    else
                    {
                        tmp[i] |= 0x00;
                    }
                }
                decal = (UInt32)(tmp[i] << 1);
                tmp[i] = (byte)(decal & 0x00FF);
                if (MSB_K1 >= 0x80)
                    tmp[tmp.Length - 1] ^= Rb;
                SubKey2 = tmp;
                Logger.Debug("SubKey2={0}", KeyToString(SubKey2));

                byte[] result;

                if ((input == null) || (input.Length == 0))
                {
                    /*-------------------------------------------------*/
                    /* Cas 1 : la chaine est vide  	*/
                    /* a- On concatene avec 0x80000000..00	(data) */
                    /* b- on X-or avec Key2	(M1)*/
                    /* c- on encrypte en AES-128 avec K et IV */

                    input = new byte[16];
                    input[0] = 0x80;

                    byte[] M1 = new byte[input.Length];
                    for (int k = 0; k < input.Length; k++)
                        M1[k] = (byte)(input[k] ^ SubKey2[k]); // input			

                    Logger.Debug("M1=80..00 XOR SubKey2={0}", KeyToString(M1));
                    result = AES_Encrypt(M1, Key, IV);
                    Logger.Debug("E(M1)={0}", BinConvert.ToHex(result));
                }
                else if ((input.Length % 16) == 0)
                {
                    /*--------------------------------------------------*/
                    /* Cas 2 ! la chaine n'est pas vide et contient un multiple de 16 octets	*/
                    /* a- on X-or avec Key 1 (data)	*/
                    /* b- on encrypte en AES-128 avec K et IV	*/
                    // byte[] data = { 0x6b, 0xc1, 0xbe, 0xe2, 0x2e, 0x40, 0x9f, 0x96, 0xe9, 0x3d, 0x7e, 0x11, 0x73, 0x93, 0x17, 0x2a };			

                    byte[] M = new byte[input.Length];
                    Array.Copy(input, M, input.Length);
                    for (i = 0; i < 16; i++)
                        M[M.Length - 16 + i] ^= SubKey1[i];

                    Logger.Debug("M=Input, last block XORed with SubKey1={0}", KeyToString(M));
                    result = AES_Encrypt(M, Key, IV);
                    Logger.Debug("E(M)={0}", BinConvert.ToHex(result));
                }
                else
                {
                    /* Cas general */
                    int totalLength = input.Length;
                    while ((totalLength % 16) != 0)
                        totalLength++;
                    byte[] M = new byte[totalLength];
                    Array.Copy(input, M, input.Length);
                    M[input.Length] = 0x80;

                    Logger.Debug("M=Input padded={0}", KeyToString(M));

                    for (i = 0; i < 16; i++)
                        M[M.Length - 16 + i] ^= SubKey2[i];

                    Logger.Debug("M=Input padded, last block XORed with SubKey2={0}", KeyToString(M));

                    result = AES_Encrypt(M, Key, IV);
                    Logger.Debug("E(M)={0}", BinConvert.ToHex(result));
                }

                /*
                Console.Write("CMAC=");
                for (int k = 0; k< L.Length; k++)
                    Console.Write("-" + String.Format("{0:x02}", result[k]));
                Console.Write("\n");	
                */
                Logger.Debug("CMAC={0}", BinConvert.ToHex(result));

                byte[] output = new byte[16];
                Array.Copy(result, result.Length - 16, output, 0, 16);
                Logger.Debug("\tOutput={0}", BinConvert.ToHex(output));
                return output;

            }



        }
    }
}
