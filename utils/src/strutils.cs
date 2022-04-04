/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SpringCard.LibCs
{
	/**
	 * \brief String manipulation utilities
	 */
	public class StrUtils
	{
		private static int numberOfTrailingZeros(int i)
		{
			// HD, Figure 5-14
			int y;
			if (i == 0) return 32;
			int n = 31;
			y = i << 16; if (y != 0) { n = n - 16; i = y; }
			y = i << 8; if (y != 0) { n = n - 8; i = y; }
			y = i << 4; if (y != 0) { n = n - 4; i = y; }
			y = i << 2; if (y != 0) { n = n - 2; i = y; }
			return n - (int)((uint)(i << 1) >> 31);
		}

		/**
		 * \brief Count the number of occurrences of a char in a string
		 */
		public static int CountTokens(string source, char searched)
		{
			int count = 0;
			foreach (char c in source)
			{
				if (c == searched)
					count++;
			}
			return count;
		}

		/**
		 * \brief Translate a base64-encoded string into its actual value
		 */
		public static byte[] Base64Decode(string message)
		{
			return Convert.FromBase64String(message);
		}

        /**
		 * \brief Translate a base64-encoded string into its actual value
		 */
        public static bool Base64TryDecode(string message, out byte[] value)
        {
            try
            {
                value = Base64Decode(message);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /**
		 * \brief Translate a base64url-encoded string into its actual value
		 */
        public static byte[] Base64UrlDecode(string message)
		{
			message = message.Replace('_', '/');
			message = message.Replace('-', '+');
			switch (message.Length % 4)
			{
				case 2:
					message += "==";
					break;
				case 3:
					message += "=";
					break;
			}
			return Convert.FromBase64String(message);
		}

        /**
		 * \brief Translate a base64url-encoded string into its actual value
		 */
        public static bool Base64UrlTryDecode(string message, out byte[] value)
        {
            try
            {
                value = Base64UrlDecode(message);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /**
		 * \brief Translate a base64url-encoded string into its actual value - expect the value is a string itself
		 */
        public static string Base64UrlDecodeString(string message, Encoding encoding)
		{
            return ToStr(Base64UrlDecode(message), encoding);
		}

        /**
		 * \brief Translate a base64url-encoded string into its actual value - expect the value is a string itself
		 */
        public static bool Base64UrlTryDecodeString(string message, Encoding encoding, out string value)
        {
            try
            {
                value = ToStr(Base64UrlDecode(message), encoding);
                return true;
            }
            catch
            {
                value = "";
                return false;
            }
        }

        /**
		 * \brief Translate a base64url-encoded string into its actual value - expect the value is a string itself
		 */
        public static string Base64UrlDecodeString(string message)
		{
			return Base64UrlDecodeString(message, Encoding.UTF8);
		}

        /**
		 * \brief Translate a base64url-encoded string into its actual value - expect the value is a string itself
		 */
        public static bool Base64UrlTryDecodeString(string message, out string value)
        {
            try
            {
                value = ToStr(Base64UrlDecode(message));
                return true;
            }
            catch
            {
                value = "";
                return false;
            }
        }

        /**
		 * \brief Translate an array of bytes into base64
		 */
        public static string Base64Encode(byte[] content, int lineLength = 0)
		{
            if (content == null)
                return "";

			string result = System.Convert.ToBase64String(content);
			if (lineLength > 0)
			{
				string t = "";
				while (result.Length > lineLength)
				{
					if (t.Length > 0) t = t + "\n";
					t = t + result.Substring(0, lineLength);
					result = result.Substring(lineLength);
				}
				if (result.Length > 0)
				{
					if (t.Length > 0) t = t + "\n";
					t = t + result;
				}
				result = t;
			}
			return result;
		}

		/**
		 * \brief Translate an array of bytes into base64url
		 */
		public static string Base64UrlEncode(byte[] content)
		{
            if (content == null)
                return "";

            string result = Convert.ToBase64String(content);
			result = result.TrimEnd('=');
			result = result.Replace('+', '-');
			result = result.Replace('/', '_');
			return result;
		}

		/**
		 * \brief Translate a string into base64url
		 */
		public static string Base64UrlEncode(string content, Encoding encoding)
		{
			return Base64UrlEncode(encoding.GetBytes(content));
		}

		/**
		 * \brief Translate a string into base64url
		 */
		public static string Base64UrlEncode(string content)
		{
			return Base64UrlEncode(content, Encoding.UTF8);
		}

		/*
		 * \brief Repeat a string
		 */
		public static string Repeat(string content, int count)
        {
			string result = "";
			for (int i = 0; i < count; i++)
				result += content;
			return result;
        }

		static StrUtils()
		{
			BASE32_DIGITS = "N4BVCXWMLK3JHGF2DSQPOI7UYTREZA65".ToCharArray();
			BASE32_MASK = BASE32_DIGITS.Length - 1;
			BASE32_SHIFT = numberOfTrailingZeros(BASE32_DIGITS.Length);
			for (int i = 0; i < BASE32_DIGITS.Length; i++) BASE32_CHARMAP[BASE32_DIGITS[i]] = i;
		}


		private static readonly char[] BASE32_DIGITS;
		private static readonly int BASE32_SHIFT;
		private static readonly int BASE32_MASK;
		private static Dictionary<char, int> BASE32_CHARMAP = new Dictionary<char, int>();

		public static byte[] Base32Decode(string encoded)
		{
			// Remove whitespace and separators
			encoded = encoded.Replace("-", "");
			encoded = encoded.Replace(" ", "");
			encoded = encoded.Trim();

			// Remove padding. Note: the padding is used as hint to determine how many
			// bits to decode from the last incomplete chunk (which is commented out
			// below, so this may have been wrong to start with).
			encoded = Regex.Replace(encoded, "[=]*$", "");

			// Canonicalize to all upper case
			encoded = encoded.ToUpper();
			if (encoded.Length == 0)
			{
				return new byte[0];
			}
			int encodedLength = encoded.Length;
			int outLength = encodedLength * BASE32_SHIFT / 8;
			byte[] result = new byte[outLength];
			int buffer = 0;
			int next = 0;
			int bitsLeft = 0;
			foreach (char c in encoded.ToCharArray())
			{
				if (!BASE32_CHARMAP.ContainsKey(c))
				{
					throw new ArgumentException("Illegal character: " + c);
				}
				buffer <<= BASE32_SHIFT;
				buffer |= BASE32_CHARMAP[c] & BASE32_MASK;
				bitsLeft += BASE32_SHIFT;
				if (bitsLeft >= 8)
				{
					result[next++] = (byte)(buffer >> (bitsLeft - 8));
					bitsLeft -= 8;
				}
			}
			// We'll ignore leftover bits for now.
			//
			// if (next != outLength || bitsLeft >= SHIFT) {
			//  throw new LicenseException("Bits left: " + bitsLeft);
			// }
			return result;
		}

		public static string Base32Encode(byte[] data)
		{
			if (data.Length == 0)
			{
				return "";
			}

			// SHIFT is the number of bits per output character, so the length of the
			// output is the length of the input multiplied by 8/SHIFT, rounded up.
			if (data.Length >= (1 << 28))
			{
				// The computation below will fail, so don't do it.
				throw new ArgumentOutOfRangeException("data");
			}

			int outputLength = (data.Length * 8 + BASE32_SHIFT - 1) / BASE32_SHIFT;
			StringBuilder result = new StringBuilder(outputLength);

			int buffer = data[0];
			int next = 1;
			int bitsLeft = 8;
			while (bitsLeft > 0 || next < data.Length)
			{
				if (bitsLeft < BASE32_SHIFT)
				{
					if (next < data.Length)
					{
						buffer <<= 8;
						buffer |= (data[next++] & 0xff);
						bitsLeft += 8;
					}
					else
					{
						int pad = BASE32_SHIFT - bitsLeft;
						buffer <<= pad;
						bitsLeft += pad;
					}
				}
				int index = BASE32_MASK & (buffer >> (bitsLeft - BASE32_SHIFT));
				bitsLeft -= BASE32_SHIFT;
				result.Append(BASE32_DIGITS[index]);
			}
			return result.ToString();
		}


		/**
		 * \brief Convert a string into an array of bytes. The '\0' terminator may be included if requested.
		 */
		public static byte[] ToBytes(string input, Encoding encoding, bool includeTerminator = false)
		{
			byte[] result = encoding.GetBytes(input);
			if (includeTerminator)
			{
				byte[] endMarker = new byte[1];
				result = BinUtils.Concat(result, endMarker);
			}
			return result;
		}

		/**
		 * \brief Convert a string into an array of bytes. The '\0' terminator may be included if requested.
		 */
		public static byte[] ToBytes(string input, bool includeTerminator = false)
		{
			return ToBytes(input, Encoding.UTF8, includeTerminator);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, Encoding encoding)
		{
            /* New JDA 2019/01/22: terminate the string correctly */
            int count;
            for (count=0; count<buffer.Length; count++)
                if (buffer[count] == 0)
                    break;
            return encoding.GetString(buffer, 0, count);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer)
		{
			return ToStr(buffer, Encoding.UTF8);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, int offset, Encoding encoding)
		{
			return encoding.GetString(buffer, offset, buffer.Length - offset);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, int offset)
		{
			return ToStr(buffer, offset, Encoding.UTF8);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, int offset, int length, Encoding encoding)
		{
			return encoding.GetString(buffer, offset, length);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, int offset, int length)
		{
			return ToStr(buffer, offset, length, Encoding.UTF8);
		}

		/**
		 * \brief Convert a string into an array of bytes, encoded in UTF8. The '\0' terminator may be included if requested.
		 */
		public static byte[] ToBytes_UTF8(string input, bool includeTerminator = false)
		{
			return ToBytes(input, Encoding.UTF8, includeTerminator);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an UTF8 string, into a string
		 */
		public static string ToStr_UTF8(byte[] buffer)
		{
			return ToStr(buffer, Encoding.UTF8);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an UTF8 string, into a string
		 */
		public static string ToStr_UTF8(byte[] buffer, int offset)
		{
			return ToStr(buffer, offset, Encoding.UTF8);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an UTF8 string, into a string
		 */
		public static string ToStr_UTF8(byte[] buffer, int offset, int length)
		{
			return ToStr(buffer, offset, length, Encoding.UTF8);
		}

		/**
		 * \brief Convert a string into an array of bytes, encoded in ASCII. The '\0' terminator may be included if requested.
		 */
		public static byte[] ToBytes_ASCII(string input, bool includeTerminator = false)
		{
			return ToBytes(input, Encoding.ASCII, includeTerminator);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an ASCII string, into a string
		 */
		public static string ToStr_ASCII(byte[] buffer)
		{
			return ToStr(buffer, Encoding.ASCII);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an ASCII string, into a string
		 */
		public static string ToStr_ASCII(byte[] buffer, int offset)
		{
			return ToStr(buffer, offset, Encoding.ASCII);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an ASCII string, into a string
		 */
		public static string ToStr_ASCII(byte[] buffer, int offset, int length)
		{
			return ToStr(buffer, offset, length, Encoding.ASCII);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an ASCII string, into a string. Non ASCII chars are replaced by '.'.
		 */
		public static string ToStr_ASCII_nice(byte[] buffer, bool stopOnZero = true)
		{
			string s = "";
			if (buffer != null)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					if ((buffer[i] == 0) && stopOnZero)
						break;
					if ((buffer[i] <= ' ') || (buffer[i] >= 128))
					{
						s = s + '.';
					}
					else
					{
						s = s + (char)buffer[i];
					}
				}
			}
			return s;
		}

		/**
		 * \brief Return true if the string contains an integer
		 */
		public static bool IsValidInteger(string value)
		{
			int r;
			return int.TryParse(value, out r);
		}

		/**
		 * \brief Return true if the string contains a boolean ("true", "false", "yes", "no" or an integer)
		 */
		public static bool IsValidBoolean(string value)
		{
			bool valid;
			ReadBoolean(value, out valid);
			return valid;
		}

		/**
		 * \brief Read a boolean value from a string: return true for "true", "yes" or a non-zero integer, false otherwise
		 */
		public static bool ReadBoolean(string value)
		{
			bool dummy;
			return ReadBoolean(value, out dummy);
		}

		/**
		 * \brief Read a boolean value from a string; the valid out parameter takes the return of IsValidBoolean()
		 */
		public static bool ReadBoolean(string value, out bool valid)
		{
			valid = false;
			if (string.IsNullOrEmpty(value))
				return false;

			value = value.ToLower();

			if (value.Equals("false") || value.Equals("no"))
			{
				valid = true;
				return false;
			}
			if (value.Equals("true") || value.Equals("yes"))
			{
				valid = true;
				return true;
			}

			int r;
			if (!int.TryParse(value, out r))
				return false;

			valid = true;
			if (r != 0)
				return true;
			else
				return false;
		}

        /**
		 * \brief Remove the diacritics characters from a string
		 */
        public static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }

	public static partial class StringExtensions
    {
		public static string FirstCharToLowerCase(this string str)
		{
			if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
				return str;

			return char.ToLower(str[0]) + str.Substring(1);
		}
	}

}
