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
        private bool isByte(char c)
        {
            bool r = false;

            if ((c >= '0') && (c <= '9'))
            {
                r = true;
            }
            else
            if ((c >= 'A') && (c <= 'F'))
            {
                r = true;
            }
            else
            if ((c >= 'a') && (c <= 'f'))
            {
                r = true;
            }

            return r;
        }

        public static byte[] Padd(byte[] input)
        {
            return Padd(AuthTypeE.AES, input);
        }

        public static byte[] Padd(AuthTypeE keyType, byte[] input)
        {
            int blockSize = 0;
            switch (keyType)
            {
                case AuthTypeE.AES:
                    blockSize = 16;
                    break;
                case AuthTypeE.TDES_CRC16:
                case AuthTypeE.TDES_CRC32:
                    blockSize = 8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("keyType");
            }

            int paddedLength = input.Length;
            if ((paddedLength % blockSize) == 0)
            {
                paddedLength += blockSize;
            }
            else while ((paddedLength % blockSize) != 0)
            {
                paddedLength++;
            }

            byte[] output = new byte[paddedLength];
            Array.Copy(input, output, input.Length);
            output[input.Length] = 0x80;
            return output;
        }

        private const int ArmorSaltLength = 4;
        private static readonly byte[] ArmorDefaultKey = new byte[16] { 0x12, 0x3F, 0x63, 0x11, 0x5E, 0x04, 0x24, 0x5F, 0x35, 0x3A, 0x34, 0x0B, 0x24, 0x21, 0x30, 0x07 };

        private static byte[] CreateArmorSalt()
        {
            Random rand = new Random();
            byte[] result = new byte[ArmorSaltLength];
            for (int i = 0; i < result.Length; i++)
                result[i] = (byte)rand.Next(0x00, 0xFF);
            return result;
        }

        private static byte[] CreateArmorIV(byte[] salt)
        {
            byte[] result = new byte[8];
            for (int i = 0; i < result.Length; i++)
                result[i] = salt[i % salt.Length];
            return result;
        }

        private static byte[] EncryptDecryptArmor(byte[] keyValue, byte[] plainText, byte[] iv)
        {
            iv = BinUtils.EnsureSize(iv, 8, 8);
            byte[] result = new byte[plainText.Length];
            Array.Copy(plainText, result, plainText.Length);
            for (uint i = 0; i < plainText.Length; i += 16)
            {
                byte[] aes_padd = BinUtils.Concat(iv, BinUtils.FromQword(i));
                aes_padd = CryptoPrimitives.AES_Encrypt(aes_padd, keyValue, new byte[16]);
                for (int j = 0; j < 16; j++)
                {
                    if ((i + j) < result.Length)
                    {
                        result[i + j] ^= aes_padd[j];
                    }
                }
            }
            return result;
        }

        private static byte[] Armor(byte[] salt, byte[] keyValue, byte[] plainMessage)
        {
            if (salt == null)
                salt = CreateArmorSalt();
            if (salt.Length > ArmorSaltLength)
                salt = BinUtils.Copy(salt, 0, ArmorSaltLength);
            if (keyValue == null)
                keyValue = ArmorDefaultKey;

            byte[] IV = CreateArmorIV(salt);

            /* Compute and add the checksum */
            byte checksum = 0;
            for (int i = 0; i < plainMessage.Length; i++)
                checksum ^= (byte)plainMessage[i];
            byte[] buffer = BinUtils.Concat(plainMessage, checksum);

            /* Cipher */
            buffer = EncryptDecryptArmor(keyValue, buffer, IV);

            /* Add the salt before the ciphered message */
            buffer = BinUtils.Concat(salt, buffer);

            return buffer;
        }

        private static bool Unarmor(byte[] keyValue, byte[] armoredMessage, out byte[] plainMessage)
        {
            plainMessage = null;

            if (armoredMessage.Length < 1 + ArmorSaltLength)
                return false;

            if (keyValue == null)
                keyValue = ArmorDefaultKey;

            /* Extract the salt */
            byte[] salt = BinUtils.Copy(armoredMessage, 0, ArmorSaltLength);
            byte[] IV = CreateArmorIV(salt);
            byte[] buffer = BinUtils.Copy(armoredMessage, ArmorSaltLength, armoredMessage.Length - ArmorSaltLength);

            /* Decipher the buffer */
            buffer = EncryptDecryptArmor(keyValue, buffer, IV);

            /* Verify the checksum */
            byte checksum = 0;
            for (int i = 0; i < buffer.Length; i++)
                checksum ^= (byte)buffer[i];
            if (checksum != 0)
                return false;

            buffer = BinUtils.Copy(buffer, 0, buffer.Length - 1);
            plainMessage = buffer;
            return true;
        }
    }
}
