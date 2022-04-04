/**
 *
 * \ingroup PCSC 
 *
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D and Emilie.C / SpringCard 
 *
 */
/*
 *	This software is part of the SPRINGCARD SDK FOR PC/SC
 *
 *   Redistribution and use in source (source code) and binary
 *   (object code) forms, with or without modification, are
 *   permitted provided that the following conditions are met :
 *
 *   1. Redistributed source code or object code shall be used
 *   only in conjunction with products (hardware devices) either
 *   manufactured, distributed or developed by SPRINGCARD,
 *
 *   2. Redistributed source code, either modified or
 *   un-modified, must retain the above copyright notice,
 *   this list of conditions and the disclaimer below,
 *
 *   3. Redistribution of any modified code must be clearly
 *   identified "Code derived from original SPRINGCARD 
 *   copyrighted source code", with a description of the
 *   modification and the name of its author,
 *
 *   4. Redistributed object code must reproduce the above
 *   copyright notice, this list of conditions and the
 *   disclaimer below in the documentation and/or other
 *   materials provided with the distribution,
 *
 *   5. The name of SPRINGCARD may not be used to endorse
 *   or promote products derived from this software or in any
 *   other form without specific prior written permission from
 *   SPRINGCARD.
 *
 *   THIS SOFTWARE IS PROVIDED BY SPRINGCARD "AS IS".
 *   SPRINGCARD SHALL NOT BE LIABLE FOR INFRINGEMENTS OF THIRD
 *   PARTIES RIGHTS BASED ON THIS SOFTWARE.
 *
 *   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 *   FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *
 *   SPRINGCARD DOES NOT WARRANT THAT THE FUNCTIONS CONTAINED IN
 *   THIS SOFTWARE WILL MEET THE USER'S REQUIREMENTS OR THAT THE
 *   OPERATION OF IT WILL BE UNINTERRUPTED OR ERROR-FREE.
 *
 *   IN NO EVENT SHALL SPRINGCARD BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 *   DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 *   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 *   OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 *   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
 *   THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 *   OF SUCH DAMAGE. 
 *
 **/
using System;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	#region CardBuffer class

	/**
	 *
	 * \brief The CardBuffer object eases the manipulation of byte arrays
	 *
	 **/
	public class CardBuffer
	{
		protected byte[] m_bytes;

		public byte[] Bytes
		{
			get
            {
				return m_bytes;
            }
			set
            {
				m_bytes = value;
				validate();
            }
		}


		protected virtual void validate()
        {

        }

		public CardBuffer()
		{

		}

		public CardBuffer(byte b)
		{
			m_bytes = new byte[1];
			m_bytes[0] = b;
			validate();
		}

		public CardBuffer(ushort w)
		{
			m_bytes = new byte[2];
			m_bytes[0] = (byte) (w / 0x0100);
			m_bytes[1] = (byte) (w % 0x0100);
			validate();
		}

		public CardBuffer(CardBuffer buffer)
        {
			m_bytes = buffer.Bytes;
			validate();
		}

		public CardBuffer(byte[]bytes)
		{
			m_bytes = bytes;
			validate();
		}

		public CardBuffer(byte[]bytes, long length)
		{
			SetBytes(bytes, length);
		}

		public CardBuffer(byte[]bytes, long offset, long length)
		{
			SetBytes(bytes, offset, length);
		}

		/**
		 * Create a CardBuffer from the given array of bytes, the array of bytes being specified as an hexadecimal string
		 */
		public CardBuffer(string str)
		{
			SetString(str);
		}

		public byte this[long offset]
		{
			get
			{
				return GetByte(offset);
			}
		}

		public byte GetByte(long offset)
		{
			if (m_bytes == null)
				return 0;

			if (offset >= m_bytes.Length)
				offset = 0;

			return m_bytes[offset];
		}

		public byte[] GetBytes()
		{
			return m_bytes;
		}

		public byte[] GetBytes(long length)
		{
			if (m_bytes == null)
				return null;

			if (length < 0)
				length = m_bytes.Length - length;

			if (length > m_bytes.Length)
				length = m_bytes.Length;

			byte[] r = new byte[length];
			for (long i=0; i<length; i++)
				r[i] = m_bytes[i];

			return r;
		}

		public byte[] GetBytes(long offset, long length)
		{
			if (m_bytes == null)
				return null;

			if (offset < 0)
				offset = m_bytes.Length - offset;

			if (offset >= m_bytes.Length)
				return null;
			
			if (length < 0)
				length = m_bytes.Length - length;
			
			byte[] r = new byte[length];
			for (long i=0; i<length; i++)
			{
				if (offset >= m_bytes.Length) break;
				r[i] = m_bytes[offset++];
			}

			return r;
		}

		public char[] GetChars(long offset, long length)
		{
			byte[] b = GetBytes(offset, length);

			if (b == null) return null;

			char[] c = new char[b.Length];
			for (long i=0; i<b.Length; i++)
				c[i] = (char) b[i];

			return c;
		}

		public void SetBytes(byte[]bytes)
		{
			m_bytes = bytes;
			validate();
		}

		public void SetBytes(byte[]bytes, long length)
		{
			m_bytes = new byte[length];

			long i;

			for (i = 0; i < length; i++)
				m_bytes[i] = bytes[i];

			validate();
		}

		public void SetBytes(byte[]bytes, long offset, long length)
		{
			m_bytes = new byte[length];

			long i;

			for (i = 0; i < length; i++)
				m_bytes[i] = bytes[offset + i];

			validate();
		}

		public void Append(byte[]bytes)
		{
			if ((bytes == null) || (bytes.Length == 0))
				return;

			byte[] old_bytes = GetBytes();

			if ((old_bytes == null) || (old_bytes.Length == 0))
			{
				SetBytes(bytes);
				return;
			}

			m_bytes = new byte[old_bytes.Length + bytes.Length];

			for (long i=0; i<old_bytes.Length; i++)
				m_bytes[i] = old_bytes[i];
			for (long i=0; i<bytes.Length; i++)
				m_bytes[old_bytes.Length+i] = bytes[i];

			validate();
		}

		public void AppendOne(byte b)
		{			
			byte[] old_bytes = GetBytes();

			if ((old_bytes == null) || (old_bytes.Length == 0))
			{
				m_bytes = new byte[1];
				m_bytes[0] = b;
				return;
			}

			m_bytes = new byte[old_bytes.Length + 1];

			for (long i=0; i<old_bytes.Length; i++)
				m_bytes[i] = old_bytes[i];

			m_bytes[old_bytes.Length] = b;

			validate();
		}

		public bool StartsWith(byte[] value)
		{
			for (int i=0; i<value.Length; i++)
			{
				if (i >= m_bytes.Length)
					break;
				if (value[i] != m_bytes[i])
					return false;
			}
			return true;
		}

		public bool StartsWith(CardBuffer value)
		{
			return StartsWith(value.m_bytes);
		}

		public void SetString(string str)
		{
			m_bytes = BinConvert.HexToBytes(str);
			validate();
		}

		public string GetString()
		{
			return BinConvert.ToHex(m_bytes);
		}

		public virtual string AsString(string separator)
		{
			string s = "";
			long i;

			if (m_bytes != null)
			{
				for (i = 0; i < m_bytes.Length; i++)
				{
					if (i > 0)
						s = s + separator;
					s = s + String.Format("{0:X02}", m_bytes[i]);
				}
			}

			return s;
		}

		public virtual string AsString()
		{
			return AsString("");
		}

        public override string ToString()
        {
            return AsString("");
        }

        public int Length
		{
			get
			{
				if (m_bytes == null)
					return 0;
				return m_bytes.Length;
			}
		}

		public static byte[] BytesFromString(string s)
		{
			byte[] r = new byte[s.Length];
			for (int i=0; i<r.Length; i++)
			{
				char c = s[i];
				r[i] = (byte) (c & 0x7F);
			}
			return r;
		}

		public static string StringFromBytes(byte[] a)
		{
			string r = "";
			if (a != null)
			{
				for (int i=0; i<a.Length; i++)
				{
					char c = (char) a[i];
					r += c;
				}
			}
			return r;
		}
	}
	#endregion

	#region CAPDU class

	/**
	 *
	 * \brief The CAPDU object is used to format and send COMMAND APDUs (according to ISO 7816-4) to the smartcard
	 *
	 **/
	public class CAPDU : CardBuffer
	{
		private bool m_valid;
		private bool m_extended;
		private bool m_has_lc;
		private bool m_has_le;

		protected override void validate()
        {
			m_valid = false;
			m_extended = false;
			m_has_lc = false;
			m_has_le = false;

			if (m_bytes != null)
            {
				if ((m_bytes.Length == 4) || (m_bytes.Length == 5))
				{
					/* Definitively a short APDU, with Le */
					m_valid = true;
					if (m_bytes.Length == 5)
						m_has_le = true;
				}
				else if ((m_bytes.Length > 5) && (m_bytes[4] != 0x00))
				{
					/* Possibly a short APDU */
					byte Lc = m_bytes[4];
					/* Without Le, or with Le */
					if ((m_bytes.Length == (5 + Lc)) || (m_bytes.Length == (6 + Lc)))
					{
						m_valid = true;
						m_has_lc = true;
						if (m_bytes.Length == (6 + Lc))
							m_has_le = true;
					}
				}
				else if ((m_bytes.Length == 7) && (m_bytes[4] == 0x00))
                {
					/* A long APDU with only Le */
					m_valid = true;
					m_extended = true;
					m_has_le = true;
				}
				else if ((m_bytes.Length > 7) && (m_bytes[4] == 0x00))
                {
					/* Possibly a long APDU */
					ushort Lc = (ushort) (m_bytes[5] * 0x0100 + m_bytes[6]);
					/* Without Le, or with Le */
					if ((m_bytes.Length == (7 + Lc)) || (m_bytes.Length == (9 + Lc)))
					{
						m_valid = true;
						m_extended = true;
						m_has_lc = true;
						if (m_bytes.Length == (9 + Lc))
							m_has_le = true;
					}
				}
			}
		}

		public CAPDU()
		{

		}

		/**
		 * Create a CAPDU from the given array of bytes
		 */
		public CAPDU(byte[]bytes)
		{
			m_bytes = bytes;
			validate();
		}

		/**
		 * Create a CAPDU from the given array of bytes, the array of bytes being specified as an hexadecimal string
		 */
		public CAPDU(string str) : base(str)
		{
			validate();
		}

		/**
		 * Create a CAPDU with only the CLA, INS, P1, P2 header
		 */

		public CAPDU(byte CLA, byte INS, byte P1, byte P2)
		{
			m_bytes = new byte[4];
			m_bytes[0] = CLA;
			m_bytes[1] = INS;
			m_bytes[2] = P1;
			m_bytes[3] = P2;
			validate();
		}

		/**
		 * Create a CAPDU with only the CLA, INS, P1, P2 header and a P3 (Le) entry
		 */

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte P3)
		{
			m_bytes = new byte[5];
			m_bytes[0] = CLA;
			m_bytes[1] = INS;
			m_bytes[2] = P1;
			m_bytes[3] = P2;
			m_bytes[4] = P3;
			validate();
		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header and some data
		 */

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data)
		{
			if (data == null)
				data = new byte[0];

			if (data.Length > 255)
            {
				/* Extended APDU */
				m_bytes = new byte[4 + 3 + data.Length];

				m_bytes[0] = CLA;
				m_bytes[1] = INS;
				m_bytes[2] = P1;
				m_bytes[3] = P2;

				m_bytes[4] = 0x00;
				m_bytes[5] = (byte)(data.Length / 0x0100);
				m_bytes[6] = (byte)(data.Length % 0x0100);

				for (int i = 0; i < data.Length; i++)
					m_bytes[7 + i] = data[i];
			}
			else
            {
				/* Short APDU */
				m_bytes = new byte[4 + 1 + data.Length];

				m_bytes[0] = CLA;
				m_bytes[1] = INS;
				m_bytes[2] = P1;
				m_bytes[3] = P2;

				m_bytes[4] = (byte)data.Length;

				for (int i = 0; i < data.Length; i++)
					m_bytes[5 + i] = data[i];
			}

			validate();
		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header and some data. The data field is taken from a string.
		 */
		public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data) : this(CLA, INS, P1, P2, StrUtils.ToBytes(data))
        {
			
		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header, some data and a LE.
		 */
		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data, byte LE) : this(CLA, INS, P1, P2, data, (ushort)LE)
        {

        }

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header, some data and a LE.
		 */
		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data, ushort LE)
		{
			if ((data == null) || (data.Length == 0))
			{
				if (LE > 255)
                {
					/* Extended APDU */
					m_bytes = new byte[4 + 3];

					m_bytes[0] = CLA;
					m_bytes[1] = INS;
					m_bytes[2] = P1;
					m_bytes[3] = P2;

					m_bytes[4] = 0x00;
					m_bytes[5] = (byte)(LE / 0x0100);
					m_bytes[6] = (byte)(LE % 0x0100);
				}
				else
                {
					/* Short APDU */
					m_bytes = new byte[4 + 1];

					m_bytes[0] = CLA;
					m_bytes[1] = INS;
					m_bytes[2] = P1;
					m_bytes[3] = P2;

					m_bytes[4] = (byte)LE;
				}
			}
			else if (data.Length > 255)
			{
				/* Extended APDU */
				m_bytes = new byte[4 + 3 + data.Length + 2];

				m_bytes[0] = CLA;
				m_bytes[1] = INS;
				m_bytes[2] = P1;
				m_bytes[3] = P2;

				m_bytes[4] = 0x00;
				m_bytes[5] = (byte)(data.Length / 0x0100);
				m_bytes[6] = (byte)(data.Length % 0x0100);

				for (int i = 0; i < data.Length; i++)
					m_bytes[7 + i] = data[i];

				m_bytes[7 + data.Length] = (byte)(LE / 0x0100);
				m_bytes[8 + data.Length] = (byte)(LE % 0x0100);
			}
			else
			{
				/* Short APDU */
				m_bytes = new byte[4 + 1 + data.Length + 1];

				m_bytes[0] = CLA;
				m_bytes[1] = INS;
				m_bytes[2] = P1;
				m_bytes[3] = P2;

				m_bytes[4] = (byte)data.Length;

				for (int i = 0; i < data.Length; i++)
					m_bytes[5 + i] = data[i];

				m_bytes[5 + data.Length] = (byte)LE;
			}

			validate();
		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header, some data and a LE. The data field is taken from a string.
		 */
		public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data, ushort LE) : this(CLA, INS, P1, P2, StrUtils.ToBytes(data), LE)
		{

		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header, some data and a LE. The data field is taken from a string.
		 */
		public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data, byte LE) : this(CLA, INS, P1, P2, StrUtils.ToBytes(data), LE)
		{

		}

		public bool IsoValid
        {
			get
            {
				validate();
				return m_valid;
            }
        }

		public bool Extended
        {
			get
            {
				validate();
				return m_extended;
			}
			set
            {
				m_extended = true;
			}
        }

		public byte CLA
		{
			get
			{
				if (m_bytes == null)
					return 0xFF;
				return m_bytes[0];
			}
			set
			{
				if (m_bytes == null)
					m_bytes = new byte[4];
				m_bytes[0] = value;
			}
		}

		public byte INS
		{
			get
			{
				if (m_bytes == null)
					return 0xFF;
				return m_bytes[1];
			}
			set
			{
				if (m_bytes == null)
					m_bytes = new byte[4];
				m_bytes[1] = value;
			}
		}

		public byte P1
		{
			get
			{
				if (m_bytes == null)
					return 0xFF;
				return m_bytes[2];
			}
			set
			{
				if (m_bytes == null)
					m_bytes = new byte[4];
				m_bytes[2] = value;
			}
		}

		public byte P2
		{
			get
			{
				if (m_bytes == null)
					return 0xFF;
				return m_bytes[3];
			}
			set
			{
				if (m_bytes == null)
					m_bytes = new byte[4];
				m_bytes[3] = value;
			}
		}

		public bool HasLc
		{
			get
			{
				validate();
				return m_has_lc;
			}
		}

		public bool HasLe
		{
			get
			{
				validate();
				return m_has_le;
			}
		}

		public bool Valid()
		{
			validate();
			return m_valid;
		}

		public byte Lc
		{
			get
			{
				if (!m_has_lc)
					return 0x00;
				return m_bytes[4];
			}
		}

		public ushort LcEx
		{
			get
			{
				if (!m_has_lc)
					return 0x00;

				if (m_extended)
				{
					return (ushort)((m_bytes[5] * 0x0100) + m_bytes[6]);
				}
				else
				{
					return m_bytes[4];
				}
			}
		}

		public byte Le
		{
			get
			{
				if (!m_has_le)
					return 0x00;
				return m_bytes[m_bytes.Length - 1];
			}

			set
			{
				if (m_bytes == null)
				{
					m_bytes = new byte[5];
					m_bytes[4] = value;
				}
				else
                {
					LeEx = value;
				}
			}
		}

		public ushort LeEx
		{
			get
			{
				if (m_extended)
				{
					return (ushort)((m_bytes[m_bytes.Length - 2] * 0x0100) + m_bytes[m_bytes.Length - 1]);
				}
				else
				{
					return m_bytes[m_bytes.Length - 1];
				}
			}

			set
            {
				validate();
				if ((value < 255) && !m_extended)
				{
					if (m_has_le)
					{
						/* Rewrite Le at the end of the existing short APDU */
						m_bytes[m_bytes.Length - 1] = (byte)value;
					}
					else
					{
						/* Append Le at the end of the existing short APDU */
						m_bytes = BinUtils.Concat(m_bytes, (byte)value);
					}
				}
				else if (m_extended)
				{
					if (m_has_le)
					{
						/* Rewrite Le at the end of the existing extended APDU */
						m_bytes[m_bytes.Length - 2] = (byte)(value / 0x0100);
						m_bytes[m_bytes.Length - 1] = (byte)(value % 0x0100);
					}
					else
					{
						/* Append Le at the end of the existing extended APDU */
						m_bytes = BinUtils.Concat(m_bytes, (byte)(value / 0x0100));
						m_bytes = BinUtils.Concat(m_bytes, (byte)(value % 0x0100));
					}
				}
				else
				{
					/* Must transform a short APDU into an extended APDU */
					byte Lc = m_bytes[4];

					byte[] t = new byte[4 + 3 + Lc + 2];

					/* CLA, INS, P1, P2 */
					for (int i = 0; i < 4; i++)
						t[i] = m_bytes[i];

					/* Lc */
					t[4] = 0x00;
					t[5] = (byte)(Lc / 0x0100);
					t[6] = (byte)(Lc % 0x0100);

					/* Data */
					for (int i = 0; i < Lc; i++)
					{
						t[7 + i] = m_bytes[5 + i];
					}

					/* Le */
					t[7 + Lc] = (byte)(value / 0x0100);
					t[8 + Lc] = (byte)(value % 0x0100);
				}
				validate();
			}
        }

		public CardBuffer data
		{
			get
			{
				byte[] t = DataBytes;

				if (t == null)
				{
					return null;
				}
				else
				{
					return new CardBuffer(t);
				}
			}

			set
			{
				if (value == null)
				{
					DataBytes = null;
				}
				else
				{
					DataBytes = value.GetBytes();
				}
			}
		}

		public byte[] DataBytes
		{
			get
			{
				if (!m_has_lc)
					return null;

				byte[] t = new byte[Lc];
				for (int i=0; i<t.Length; i++)
					t[i] = m_bytes[5+i];

				return t;
			}

			set
			{
				int length;
				uint apdu_size;

				if (value == null)
					length = 0;
				else
					length = value.Length;

				if (length == 0)
				{

				} else
					if (length < 256)
				{
					if (m_has_le)
						apdu_size = (uint) (6 + length);
					else
						apdu_size = (uint) (5 + length);

					byte[] t = new byte[apdu_size];

					if (Valid())
					{
						for (int i=0; i<4; i++)
							t[i] = m_bytes[i];
						if (m_has_le)
							t[t.Length-1] = m_bytes[m_bytes.Length-1];
					}

					for (int i=0; i<length; i++)
						t[5+i] = value[i];

					t[4] = (byte) length;

					m_bytes = t;
				} else
				{
					/* Oups ? */
				}
			}
		}

        public override string AsString()
        {
            return AsString("");
        }

        public override string ToString()
        {
            return AsString();
        }

    }
    #endregion

    #region RAPDU class

    /**
	 *
	 * \brief The RAPDU object is used to receive and decode RESPONSE APDUs (according to ISO 7816-4) from the smartcard
	 *
	 **/
    public class RAPDU : CardBuffer
	{
		private bool m_valid;

		protected override void validate()
		{
			m_valid = ((m_bytes != null) && (m_bytes.Length >= 2)) ? true : false;				
		}

		public bool isValid
		{
			get
			{
				return m_valid;
			}
		}

		public bool IsoValid
		{
			get
			{
				validate();
				return m_valid;
			}
		}

		public RAPDU(string buffer) : base(buffer)
		{

		}

		public RAPDU(CardBuffer buffer) : base(buffer)
		{

		}

		public RAPDU(byte[]bytes, int length) : base (bytes, length)
		{

		}

		public RAPDU(byte[]bytes) : base(bytes)
		{

		}

		public RAPDU(byte[]bytes, byte SW1, byte SW2)
		{
			byte[] t;
			if (bytes == null)
			{
				t = new byte[2];
				t[0] = SW1;
				t[1] = SW2;
			} else
			{
				t = new byte[bytes.Length + 2];
				for (int i=0; i<bytes.Length; i++)
					t[i] = bytes[i];
				t[bytes.Length] = SW1;
				t[bytes.Length+1] = SW2;
			}
			SetBytes(t);
		}

		public RAPDU(string data, byte sw1, byte sw2)
		{
			SetString(data);
			AppendOne(sw1);
			AppendOne(sw2);
		}

		public RAPDU(byte sw1, byte sw2)
		{
			byte[] t = new byte[2];
			t[0] = sw1;
			t[1] = sw2;
			SetBytes(t);
		}

		public RAPDU(ushort sw)
		{
			byte[] t = new byte[2];
			t[0] = (byte) (sw / 0x0100);
			t[1] = (byte) (sw % 0x0100);
			SetBytes(t);
		}

		public bool hasData
		{
			get
			{
				if ((m_bytes == null) || (m_bytes.Length < 2))
					return false;

				return true;
			}
		}

		public CardBuffer data
		{
			get
			{
				if ((m_bytes == null) || (m_bytes.Length < 2))
					return null;

				return new CardBuffer(m_bytes, m_bytes.Length - 2);
			}
		}

        public byte[] DataBytes
        {
            get
            {
                if ((m_bytes == null) || (m_bytes.Length < 2))
                    return new byte[0];

                return new CardBuffer(m_bytes, m_bytes.Length - 2).Bytes;
            }
        }

        public byte SW1
		{
			get
			{
				if ((m_bytes == null) || (m_bytes.Length < 2))
					return 0xCC;
				return m_bytes[m_bytes.Length - 2];
			}
		}

		public byte SW2
		{
			get
			{
				if ((m_bytes == null) || (m_bytes.Length < 2))
					return 0xCC;
				return m_bytes[m_bytes.Length - 1];
			}
		}

		public ushort SW
		{
			get
			{
				if ((m_bytes == null) || (m_bytes.Length < 2))
					return 0xCCCC;

				ushort r;

				r = m_bytes[m_bytes.Length - 2];
				r *= 0x0100;
				r += m_bytes[m_bytes.Length - 1];

				return r;
			}
		}

		public string SWString
		{
			get
			{
				return SCARD.CardStatusWordsToString(SW1, SW2);
			}
		}

        public override string AsString()
        {
            return AsString("");
        }

        public override string ToString()
        {
            return AsString();
        }

    }
    #endregion
}
