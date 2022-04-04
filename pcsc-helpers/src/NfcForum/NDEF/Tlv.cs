/**h* SpringCard.NfcForum.Ndef/NfcTlv
 *
 * NAME
 *   SpringCard API for NFC Forum :: NFC Tlv class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.Collections;
using System.Collections.Generic;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.NfcForum.Ndef
{

	/**c* SpringCard.NfcForum.Ndef/NfcTlv
	 *
	 * NAME
	 *   NfcTlv
	 * 
	 * DESCRIPTION
	 *   Represents a TLV that has been found on a NFC Type 2 Tag, found on a reader
	 *
	 * SYNOPSIS
	 *   NfcTlv tlv = new NfcTlv()
	 *   NfcTlv tlv = new NfcTlv(byte t, byte[] v)
	 * 
	 * USED BY
	 * 	 NfcTagType2
	 *
	 **/
	public class NfcTlv
	{
		private const byte FLAG_L_3_BYTES = 0xFF;
		private byte _t;
		private byte[] _v;

		public NfcTlv()
		{
			_t = 0;
			_v = null;
		}

		public NfcTlv(byte t, byte[] v)
		{
			_t = t;
			_v = v;
		}

		private NdefObject[] child = new NdefObject[0];


		/**v* SpringCard.NfcForum.Ndef/NfcTlv.T
		 *
		 * SYNOPSIS
		 *   public byte T
		 * 
		 * DESCRIPTION
		 *   Gets the Type of the TLV
		 *
		 *
		 **/
		public byte T
		{
			get
			{
				return _t;
			}
		}

		/**v* SpringCard.NfcForum.Ndef/NfcTlv.L
		 *
		 * SYNOPSIS
		 *   public long L
		 * 
		 * DESCRIPTION
		 *   Gets the Length of the TLV
		 *
		 *
		 **/
		public long L
		{
			get
			{
				if (_v == null)
					return 0;
				return _v.Length;
			}
		}

		/**v* SpringCard.NfcForum.Ndef/NfcTlv.V
		 *
		 * SYNOPSIS
		 *   public byte[] V
		 * 
		 * DESCRIPTION
		 *   Gets the Value of the TLV
		 *
		 *
		 **/
		public byte[] V
		{
			get
			{
				return _v;
			}
		}

		/**m* SpringCard.NfcForum.Ndef/NfcTlv.Serialize
		 *
		 * SYNOPSIS
		 *   public byte[] Serialize()
		 * 
		 * DESCRIPTION
		 *   Serializes the TLV and returns the corresponding byte array
		 *
		 **/
		public byte[] Serialize()
		{
			byte[] r = null;
			if (T == 0xFE && L==0) /* NfcTag.TERMINATOR_TLV */
            {
				return new byte[] { 0xFE };
            }
			if (L <= 254)
			{
				/* L is on 1 byte only */
				r = new byte[1 + 1 + L];
				r[0] = T;
				r[1] = (byte)L;
				for (long i = 0; i < L; i++)
					r[2 + i] = V[i];

			}
			else
				if (L < 65535)
			{
				/* L is on 3 bytes */
				r = new byte[1 + 3 + L];
				r[0] = T;
				r[1] = FLAG_L_3_BYTES;
				r[2] = (byte)(L / 0x0100);
				r[3] = (byte)(L % 0x0100);
				for (long i = 0; i < L; i++)
					r[4 + i] = V[i];

			}

			return r;
		}

		/**m* SpringCard.NfcForum.Ndef/NfcTlv.Unserialize
		 *
		 * SYNOPSIS
		 *   public static NfcTlv Unserialize(byte[] buffer, ref byte[] remaining_buffer)
		 * 
		 * DESCRIPTION
		 *   Constructs and returns a NfcTlv object from the "buffer" byte array.
		 * 	 The "remaining_buffer" array contains the bytes that follow the ones which belong to the constructed TLV.
		 *
		 **/
		public static NfcTlv Unserialize(byte[] buffer, out byte[] remaining_buffer)
		{
			byte t;
			long l;
			byte[] v = null;

			remaining_buffer = buffer;

			if (buffer == null)
				return null;

			t = buffer[0];

			/* specific case for Null and Terminator TLVs */
			if ((t == 0x00) || (t == 0xFE))
			{
				/* these are valid TLV, without length! */
				if (buffer.Length > 1)
				{
					remaining_buffer = new byte[buffer.Length - 1];
					BinUtils.CopyTo(remaining_buffer, 0, buffer, 1, buffer.Length - 1);
				}
				else
				{
					/* we must force an exit if we have nothing left in the buffer */
					remaining_buffer = null;
				}
				return new NfcTlv(t, v);
			}

			if (buffer.Length < 2)
				return null;

			long o;
			if (buffer[1] == FLAG_L_3_BYTES)
			{
				if (buffer.Length < 4)
					return null;
				l = buffer[2] * 0x0100 + buffer[3];
				o = 4;
			}
			else
			{
				l = buffer[1];
				o = 2;
			}

			if ((o + l) > buffer.Length)
				return null;

			if (l > 0)
			{
				v = new byte[l];
				BinUtils.CopyTo(v, 0, buffer, (int)o, (int)l);
			}

			o += l;

			if (o < buffer.Length)
			{
				remaining_buffer = new byte[buffer.Length - o];
				BinUtils.CopyTo(remaining_buffer, 0, buffer, (int)o, (int)(buffer.Length - o));
			}
			else
			{
				remaining_buffer = null;
			}

			return new NfcTlv(t, v);
		}

		/**m* SpringCard.NfcForum.Ndef/NfcTlv.add_child
		 *
		 * SYNOPSIS
		 *   public void add_child(Ndef ndef)
		 * 
		 * DESCRIPTION
		 *   Adds a child (ndef) to the NfcTlv object.
		 *
		 **/
		public void add_child(NdefObject ndef)
		{
			NdefObject[] tmp = new NdefObject[child.Length + 1];
			Array.Copy(child, tmp, child.Length);
			tmp[tmp.Length - 1] = ndef;
			child = new NdefObject[tmp.Length];
			Array.Copy(tmp, child, child.Length);
		}

		/**m* SpringCard.NfcForum.Ndef/NfcTlv.get_child
		 *
		 * SYNOPSIS
		 *   public Ndef get_child(int  i)
		 * 
		 * DESCRIPTION
		 *   Returns the ndef child, with index i.
		 * 	 In case the child doesn't exist, null is returned.
		 *
		 **/
		public NdefObject get_child(int i)
		{
			if ((i <= this.count_children()) && (i >= 0))
			{
				return child[i];
			}
			else
			{
				return null;
			}
		}

		/**m* SpringCard.NfcForum.Ndef/NfcTlv.count_children
		 *
		 * SYNOPSIS
		 *   public int count_children()
		 * 
		 * DESCRIPTION
		 *   Returns the number of children that the NfcTlv has.
		 *
		 **/
		public int count_children()
		{
			return child.Length;
		}


		/**m* SpringCard.NfcForum.Ndef/NfcTlv.Discover
		 *
		 * SYNOPSIS
		 *   public static NfcTlv Unserialize(byte[] buffer, ref byte[] remaining_buffer)
		 * 
		 * DESCRIPTION
		 *   Constructs and returns a NfcTlv object from the "buffer" byte array.
		 * 	 The "remaining_buffer" array contains the bytes that follow the ones which belong to the constructed TLV.
		 *
		 **/
		public static bool CheckIfComplete(byte[] toCheck, out ushort missing_bytes)
		{
			ushort index = 0;
			byte TLVType;
			ushort toCheckLength = (ushort)toCheck.Length;
			ushort TLVLength;
			bool isLast = false;

			/* default to "okay, we have enough" */
			missing_bytes = 0;

			while ((index < toCheckLength) && !isLast)
			{
				/* get type */
				TLVType = toCheck[index++];

				/* is this a Terminator TLV? */
				if (TLVType == 0xFE)
				{
					/* no need to go further if we have "nothing" or the "end" */
					return true;
				}

				/* is this a Null TLV? */
				if (TLVType != 0x00)
				{
					/* this is not a Null TLV, get the length field */
					if (index == toCheckLength)
					{
						/* we clearly don't have the length field, get out! */
						missing_bytes = 1;
						return false;
					}
					TLVLength = toCheck[index++];
					if (TLVLength == FLAG_L_3_BYTES)
					{
						/* we expect to find at least 2 bytes for the length */
						if ((toCheckLength - index) < 2)
						{
							/* we don't have enough bytes */
							missing_bytes = (ushort)(2 - (toCheckLength - index));
							return false;
						}
						TLVLength = (ushort)((toCheck[index] << 8) | toCheck[index + 1]);
						index += 2;
					}

					/* check if we have enough Value bytes */
					if ((index + TLVLength) > toCheckLength)
					{
						missing_bytes = (ushort)((index + TLVLength) - toCheckLength);
						return false;
					}

					/* loof for a potential next TLV entry */
					index += TLVLength;
				}
			}

			return false;
		}

		public static ulong FindFirstNDEFLength(byte[] toCheck, ulong offset = 0)
        {
			ulong messageLength = 0;

			if (toCheck == null)
				return 0;

			if (toCheck[offset++] != 0x03)
				return 0;

			messageLength = toCheck[offset++];
			if (messageLength == 0xFF )
            {
				messageLength = (ulong)( (toCheck[offset] * 256 ) + toCheck[offset+1]);
			}

			return messageLength;
		}

		public static bool FindFirstNDEFMessage(byte[] toCheck, out ushort offset)
		{
			bool found;
			ushort bypasser;
			return FindFirstNDEFMessage(toCheck, 0, out offset, out found, out bypasser);
		}
		public static bool FindFirstNDEFMessage(byte[] toCheck, ushort index, out ushort offset )
		{ 
			bool found;
			ushort bypasser;
			return FindFirstNDEFMessage(toCheck, index, out offset, out found, out bypasser);
		}

		public static bool FindFirstNDEFMessage(byte[] toCheck, ushort index, out ushort offset, out bool found)
		{
			ushort bypasser;
			return FindFirstNDEFMessage(toCheck, index, out offset, out found, out bypasser);
		}

		public static bool FindFirstNDEFMessage(byte[] toCheck, ushort index, out ushort offset, out bool found, out ushort bypasser)
		{
			byte TLVType;
			ushort toCheckLength = (ushort)toCheck.Length;
			ushort TLVLength;

			bypasser = 0;
			offset = 0;
			found = false;

			while (index < toCheckLength)
			{
				/* get type */
				TLVType = toCheck[index++];

				/* is this a Null TLV? */
				if (TLVType == 0x00)
				{
					continue;
				}

				/* we have enough bytes, look for the type */
				if (TLVType == 3)
				{
					Logger.Trace("NDEF_MESSAGE_TLV discovered");
					offset = (ushort)(index - 1);
					found = true;
					return true;
				}


				/* is this a Terminator TLV? */
				if (TLVType == 0xFE)
				{
					return true;
				}

				/* bypass type */
				bypasser++;

				/* check if we have enough bytes */
				if (index == toCheckLength)
				{
					/* we clearly don't have the length field, get out! */
					return false;
				}
				TLVLength = toCheck[index++];
				bypasser++;
				if (TLVLength == FLAG_L_3_BYTES)
				{
					/* we expect to find at least 2 bytes for the length */
					if ((toCheckLength - index) < 2)
					{
						/* we don't have enough bytes */
						return false;
					}
					TLVLength = (ushort)((toCheck[index] << 8) | toCheck[index + 1]);
					index += 2;
					bypasser += 2;
				}

				/** other types 04h-FDh and FDh are RFU and shall/should/may be bypassed!! **/
				bypasser += TLVLength;

				/* check if we have enough Value bytes */
				if ((index + TLVLength) > toCheckLength)
				{
					/* we don't have enough bytes */
					return false;
				}
				
				/* loof for a potential next TLV entry */
				index += TLVLength;
			}

			return true;
		}

		public static bool LookForSpecialControls(byte[] toCheck, ref List<Tuple<byte, ushort, ushort, byte[]>> exclusions)
		{
			ushort index = 0;
			byte TLVType;
			ushort toCheckLength = (ushort)toCheck.Length;
			ushort TLVLength;

			/* clean start */
			exclusions.Clear();

			while (index < toCheckLength)
			{
				/* get type */
				TLVType = toCheck[index++];

				/* is this a Null TLV? */
				if (TLVType == 0x00)
				{
					continue;
				}

				/* is this NOT a Lock or a Memory Control TLV? */
				if (TLVType > 0x02)
				{
					/* no need to go further if we have "nothing" or the "end" */
					return true;
				}

				/* check if we have enough bytes */
				if (index == toCheckLength)
				{
					/* we clearly don't have the length field, get out! */
					return false;
				}
				TLVLength = toCheck[index++];
				if (TLVLength == FLAG_L_3_BYTES)
				{
					/* we expect to find at least 2 bytes for the length */
					if ((toCheckLength - index) < 2)
					{
						/* we don't have enough bytes */
						return false;
					}
					TLVLength = (ushort)((toCheck[index] << 8) | toCheck[index + 1]);
					index += 2;
				}

				/* check if we have enough Value bytes */
				if ((index + TLVLength) > toCheckLength)
				{
					/* we don't have enough bytes */
					return false;
				}

				/* we have enough bytes, look for th type */
				if (TLVType == 1)
				{
					Logger.Trace("LOCK_CONTROL_TLV discovered");

					if (TLVLength != 3)
					{
						return false;
					}

					/* unpack and calculate */
					byte NbrMajorOffset = (byte)((toCheck[index] >> 4) & 0x0F);
					byte NbrMinorOffset = (byte)(toCheck[index] & 0x0F);

					ushort DLA_NbrLockBits;
					if (toCheck[index + 1] == 0x00)
					{
						DLA_NbrLockBits = 256;
					}
					else
					{
						DLA_NbrLockBits = toCheck[index + 1];
					}

					byte BLPLB_Index = (byte)((toCheck[index + 2] >> 4) & 0x0F);
					if ((BLPLB_Index < 2) || (BLPLB_Index > 10))
					{
						Logger.Error("BLPLB_Index is invalid!");
						return true;
					}

					byte MOS_DLA = (byte)(toCheck[index + 2] & 0x0F);
					if ((MOS_DLA < 2))
					{
						Logger.Error("MOS_DLA is invalid!");
						return true;
					}

					ushort BytesLockedPerLockBit = (ushort)(1 << BLPLB_Index);
					ushort DLA_NbrBytes = (ushort)(DLA_NbrLockBits / 8);
					ushort MajorOffset_Size_DLA = (ushort)(1 << MOS_DLA);
					ushort DLA_FirstByteAddr = (ushort)((NbrMajorOffset * MajorOffset_Size_DLA) + NbrMinorOffset);

					/*
					Logger.Trace("\tNbrMajorOffset={0:d}", NbrMajorOffset);
					Logger.Trace("\tNbrMinorOffset={0:d}", NbrMinorOffset);
					Logger.Trace("\tDLA_NbrLockBits={0:d}", DLA_NbrLockBits);
					Logger.Trace("\tBLPLB_Index={0:d}", BLPLB_Index);
					Logger.Trace("\tMOS_DLA={0:d}", MOS_DLA);
					Logger.Trace("\tBytesLockedPerLockBit={0:d}", BytesLockedPerLockBit);
					Logger.Trace("\tDLA_NbrBytes={0:d}", DLA_NbrBytes);
					Logger.Trace("\tMajorOffset_Size_DLA={0:d}", MajorOffset_Size_DLA);
					Logger.Trace("\tDLA_FirstByteAddr={0:d}", DLA_FirstByteAddr);
					*/

					/* we need to export those values */
					byte[] temp = new byte[3];
					BinUtils.CopyTo(temp, 0, toCheck, index, 3);
					exclusions.Add(new Tuple<byte, ushort, ushort, byte[]>(0, DLA_FirstByteAddr, DLA_NbrBytes, temp));
				}
				else
				if (TLVType == 2)
				{
					Logger.Trace("MEMORY_CONTROL_TLV discovered");

					/* unpack and calculate */
					byte NbrMajorOffset = (byte)((toCheck[index] >> 4) & 0x0F);
					byte NbrMinorOffset = (byte)(toCheck[index] & 0x0F);

					ushort Rsvd_Area_Size;

					if (toCheck[index + 1] == 0x00)
					{
						Rsvd_Area_Size = 256;
					}
					else
					{
						Rsvd_Area_Size = toCheck[index + 1];
					}

					byte MOS_DLA = (byte)(toCheck[index + 2] & 0x0F);
					if ((MOS_DLA < 2))
					{
						Logger.Error("MOS_DLA is invalid!");
						return true;
					}

					ushort MajorOffset_Size_RA = (ushort)(1 << MOS_DLA);
					ushort RA_FirstByteAddr = (ushort)((NbrMajorOffset * MajorOffset_Size_RA) + NbrMinorOffset);

					/*
					Logger.Trace("\tNbrMajorOffset={0:d}", NbrMajorOffset);
					Logger.Trace("\tNbrMinorOffset={0:d}", NbrMinorOffset);
					Logger.Trace("\tRsvd_Area_Size={0:d}", Rsvd_Area_Size);
					Logger.Trace("\tMOS_DLA={0:d}", MOS_DLA);
					Logger.Trace("\tMajorOffset_Size_RA={0:d}", MajorOffset_Size_RA);
					Logger.Trace("\tRA_FirstByteAddr={0:d}", RA_FirstByteAddr);
					*/

					byte[] temp = new byte[3];
					BinUtils.CopyTo(temp, 0, toCheck, index, 3);
					exclusions.Add(new Tuple<byte, ushort, ushort, byte[]>(1, RA_FirstByteAddr, Rsvd_Area_Size, temp));

				}
				/** other types 04h-FDh and FDh are RFU and shall be bypassed!! **/

				/* loof for a potential next TLV entry */
				index += TLVLength;
			}

			return true;
		}

		/* Lc field helper */		
		public static byte[] CreateLcField(long length, bool forceExtended)
		{
			byte[] Lc = null;

			/* Lc absent, The number of bytes in the Command Data field zero. */
			if (length == 0)
				return Lc;

			/* Short Field coding, Valid range: encodes the number of bytes in
			 * the Data field between 1 and 255. */
			if ((length < 256) && (forceExtended == false))
			{
				Lc = new byte[1];
				Lc[0] = (byte)length;
				return Lc;
			}

			/* Extended Field coding, Valid range: encodes the number of bytes
			 * in the Data field between 1 and 65535. */
			Lc = new byte[3];
			Lc[0] = (byte)0x00;
			Lc[1] = (byte)((length >> 8) & 0xFF);
			Lc[2] = (byte)((length >> 0) & 0xFF);
			return Lc;
		}
		
		public static byte[] CreateLcField(long length)
		{
			return CreateLcField(length, false);
		}

		/* Le field helper */
		public static byte[] CreateLeField(long length, bool LcIsPresent, bool forceExtended)
		{
			byte[] Le = null;

			/* Le absent, The maximum number of bytes expected in the Response
			 * Data field is zero. */
			if (length == 0)
				return Le;

			/* Le overflow, ie > 65536 */
			if (length > 65536)
				return Le;

			if (!forceExtended && (length <= 256) )
			{
				/* Short Field coding, Valid range: encodes the maximum number of
				 * bytes expected between 1 and 255 */
				Le = new byte[1];
				
				if (length==256)
                {
					/* 0x00: Encodes the maximum number of bytes expected equal to 256 */
					length = 0;
                }
								
				Le[0] = (byte)length;
				return Le;
			}

			if (LcIsPresent)
			{
				/* Extended Field coding with extended Lc field present */
				Le = new byte[2];

				if (length == 65536)
				{
					/* 0x0000: Encodes the maximum number of bytes expected equal to 65536 */
					length = 0;
				}

				/* Encodes the maximum number of bytes expected */	
				Le[0] = (byte)((length >> 8) & 0xFF);
				Le[1] = (byte)((length >> 0) & 0xFF);
				return Le;
			}
			else
			{
				/* Extended Field coding with absent Lc field */
				Le = new byte[3];

				if (length == 65536)
				{
					/* 0x0000: Encodes the maximum number of bytes expected equal to 65536 */
					length = 0;
				}

				/* Encodes the maximum number of bytes expected */
				Le[0] = (byte)0x00;
				Le[1] = (byte)((length >> 8) & 0xFF);
				Le[2] = (byte)((length >> 0) & 0xFF);
				return Le;
			}
		}

		public static byte[] CreateLeField(long length, bool LcIsPresent)
        {
			return CreateLeField(length, LcIsPresent, false);

		}

		/* Offset Data Object helper */
		public static byte[] CreateODOField(long offset)
		{
			byte[] ODO = new byte[5];
			ODO[0] = 0x54;
			ODO[1] = 0x03;
			ODO[2] = (byte)((offset >> 16) & 0xFF);
			ODO[3] = (byte)((offset >> 8) & 0xFF);
			ODO[4] = (byte)((offset >> 0) & 0xFF);
			return ODO;
		}

		/* Discretionary Data Object helper */
		public static byte[] CreateDDOField(long length)
		{
            byte[] DDO = null;

			if (length <= 127)
			{
				DDO = new byte[2];
				DDO[0] = 0x53;
				DDO[1] = (byte)length;
			}
			else
			if (length <= 253)
			{
				DDO = new byte[3];
				DDO[0] = 0x53;
				DDO[1] = 0x81;
				DDO[2] = (byte)length;
			}
			else
			{
				DDO = new byte[4];
				DDO[0] = 0x53;
				DDO[1] = 0x82;
				DDO[2] = (byte)((length >> 8) & 0xFF);
				DDO[3] = (byte)((length >> 0) & 0xFF);
			}
			return DDO;
		}
	}	
}
