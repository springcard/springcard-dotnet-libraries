/**h* SpringCard.NfcForum.Tags/NfcTag
 *
 * NAME
 *   SpringCard API for NFC Forum :: NfcTagType5 class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using SpringCard.NfcForum.Ndef;
using SpringCard.LibCs;
using SpringCard.PCSC;
using System.Collections.Generic;
using SpringCard.LibCs.Windows;

namespace SpringCard.NfcForum.Tags
{
	/**c* SpringCard.NfcForum.Tags/NfcTagType5
	 *
	 * NAME
	 *   NfcTagType5
	 * 
	 * DERIVED FROM
	 *   NfcTag
	 * 
	 * DESCRIPTION
	 *   Represents a Type 5 NFC Tag that has been discovered on a reader.
	 *
	 * SYNOPSIS
	 *   if (NfcTagType5.Recognize(channel))
	 *     NfcTag tag = NfcTagType5.Create(SCardChannel channel)
	 *
	 **/
	public class NfcTagType5 : NfcTag
	{
		private const string ATR_BASE = "3B8F8001804F0CA0000003060B";		
		private const string ATR_ICODE_SLI = "3B8F8001804F0CA0000003060B00140000000077";
		
		/* Version Treating */
		private const byte major_T5T_RW_VNo = 1;
		private const byte minor_T5T_RW_VNo = 3;

		private ushort _block_length = 32;
        private byte[] _CC;
		private byte[] _initreceived;
        private List<NfcTlv> _tlvs = new List<NfcTlv>();

		/* Capability Container */
		private bool _use_2_byte_address = false;
		private ushort _version_major = 0;
		private ushort _version_minor = 0;
		private bool _use_special_frame = false;
		private bool _use_multiblock_cmd = false;
        private long _first_forbidden_block = 0;
		private ushort _first_ndef_message_offset = 0;
		private byte[] _first_ndef_block = null;

		public override bool Format()
		{
			return false;
		}

		public override bool Lock()
		{
			Logger.Trace("Starting locking of the type 5 tag");
			
			Logger.Trace("Updating bits b1b0 of CC[1]");
			byte[] blockzero = new byte[_block_length];
			Array.Copy(_initreceived,blockzero,_block_length);
			blockzero[1] |= 0x03;
			if (!WriteBinary(_channel, 0, blockzero))
            {
				Logger.Trace("Locking failed");
				return false;
			}
			Logger.Trace("CL_Control");
			if (!CL_Control())
			{
				Logger.Trace("Failed to set up Option Flag");
			}
			Logger.Trace("Locking the {0:D} blocks",_first_forbidden_block);
			for (ushort i=0; i<_first_forbidden_block; i++)
            {
				if (!LockBlock(i))
                {
					Logger.Trace("Locking failed");
					return false;
                }
            }
			Logger.Trace("Locking complete");
			return true;
		}

        protected bool LockBlock(ushort numblock)
        {
			CAPDU capdu;
            if (_use_2_byte_address)
            {
				/* EXTENDED_LOCK_SINGLE_BLOCK */
				byte[] DataIn = new byte[2] { (byte)(numblock / 0x0100), (byte)(numblock % 0x0100) };
                capdu = new CAPDU(0xFF, 0xF6, 0x32, 0x00, DataIn, 0x00);
            }
            else
            {
                /* LOCK_SINGLE_BLOCK */
				if (numblock > 0xFF)
                {
					Logger.Trace("Tag does not use extended addresses");
					return false;
                }
				byte[] bytes_capdu = new byte[6] { 0xFF, 0xF6, 0x22, 0x00, 0x01, (byte)numblock };
				capdu = new CAPDU(bytes_capdu);
            }

            Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = _channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("LockBlock " + String.Format("{0:X4}", numblock) + " error");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("LockBlock " + String.Format("{0:X4}", numblock) + " failed " + SCARD.CardStatusWordsToString(rapdu.SW) + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}
			return true;
		}

        protected override bool WriteContent(byte[] ndef_content)
		{
			ushort mustWrite;
			ushort canWrite;
			ushort rawSize;
			byte[] header = null;
			ushort index = 0;
			ushort first_block;
			ushort ndef_offset;

			Logger.Trace("Writing the NFC Forum Type 5 Tag (length={0:d})", ndef_content.Length);

			if (ndef_content == null)
            {
				Logger.Trace("Content to write is NULL");
				return false;
			}

			if (_locked)
            {
				Logger.Trace("Tag is write-locked");
				return false;
			}

			if (!CL_Control())
            {
				Logger.Trace("Failed to set up WRITE_BINARY");
            }

			CardBuffer actual_content = new CardBuffer();
			rawSize = (ushort)ndef_content.Length;

			first_block = (ushort)((_first_ndef_message_offset + _CC.Length) / _block_length);
			ndef_offset = (ushort)((_first_ndef_message_offset + _CC.Length) % _block_length);
			Logger.Trace("We may write NDEF TLV at byte {0:D} (Block {1:D}, offset {2:D})", _first_ndef_message_offset + _CC.Length, first_block, ndef_offset);

			/* only if we wre not aligned.. */
			if (_first_ndef_block != null)
            {
				byte[] temp = new byte[ndef_offset];
				Array.Copy(_first_ndef_block, 0, temp, 0, ndef_offset);				
				actual_content.Append(temp);
			}

			if (ndef_content.Length > 254)
			{
				header = new byte[4];
				header[0] = NDEF_MESSAGE_TLV;
				header[1] = 0x00;
			}
			else
			{
				header = new byte[2];
				header[0] = NDEF_MESSAGE_TLV;
			}
			actual_content.Append(header);
			actual_content.Append(ndef_content);
			mustWrite = (ushort)actual_content.Length;

			/* this is the NDEF message size (without extra TERMINATOR) */
			if (actual_content.Length > Capacity())
			{
				Logger.Trace("The size of the content (with its TLVs) is bigger than the tag's capacity");
				return false;
			}

			/* shall we write an Terminator? */
			if (actual_content[actual_content.Length - 1] != TERMINATOR_TLV)
			{
				actual_content.AppendOne(TERMINATOR_TLV);
				mustWrite++;
				Logger.Trace("We must add a TERMINATOR TLV at the end of the Tag");
			}

			Logger.Debug("actual_content={0}", BinConvert.ToHex(actual_content.GetBytes()));
			ushort block = first_block;
			while ((mustWrite > 0) && (block < _first_forbidden_block))
			{
				canWrite = _block_length;
				if (mustWrite < _block_length)
				{
					canWrite = mustWrite;
				}

				byte[] buffer = new byte[_block_length];
				BinUtils.CopyTo(buffer, 0, actual_content.Bytes, index, canWrite);
				if (!WriteBinary(_channel, block, buffer))
					return false;

				mustWrite -= canWrite;
				index += canWrite;
				block++;
			}

			/* eventually write the final header with Length field! */
			Logger.Trace("Updating length!");
			bool length_on_two_blocks = false;
			byte[] newlength = new byte[_block_length*2];
			BinUtils.CopyTo(newlength, 0, actual_content.Bytes, 0, _block_length*2);
			if (rawSize > 254)
			{
				newlength[ndef_offset+1] = 0xFF;
				newlength[ndef_offset+2] = (byte)(rawSize / 0x0100);
				newlength[ndef_offset+3] = (byte)(rawSize % 0x0100);

				if ((ndef_offset + 3) > _block_length )
					length_on_two_blocks = true;
			}
			else
			{
				newlength[ndef_offset+1] = (byte)rawSize;
				if ((ndef_offset + 1) > _block_length)
					length_on_two_blocks = true;
			}

			/* eventually write the final header with Length field! */
			byte[] one_length_block = new byte[_block_length];
			
			BinUtils.CopyTo(one_length_block, 0, newlength, 0, _block_length);
			if (!WriteBinary(_channel, first_block, one_length_block))
				return false;

			if (length_on_two_blocks )
            {
				BinUtils.CopyTo(one_length_block, 0, newlength, _block_length, _block_length);
				if (!WriteBinary(_channel, (ushort)(first_block + 1), one_length_block))
					return false;
			}

			return true;
        }

		protected static bool WriteBinary(ICardApduTransmitter channel, ushort offset, byte[] buffer)
		{
			CAPDU capdu = new CAPDU(0xFF, 0xD6, (byte)(offset / 0x0100), (byte)(offset % 0x0100), buffer);

			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("WriteBinary " + String.Format("{0:X4}", offset) + "," + String.Format("{0:X2}", (byte)buffer.Length) + " error");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("WriteBinary " + String.Format("{0:X4}", offset) + "," + String.Format("{0:X2}", (byte)buffer.Length) + " failed " + SCARD.CardStatusWordsToString(rapdu.SW) + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}
			return true;
		}

		private bool CL_Control()
		{
			return CL_Control(out ushort sw);
		}
		private bool CL_Control(out ushort sw)
		{
			sw = 0x0000;
			
			byte param_multi_extended = 0x00;
			byte param_optionflag = 0x00;

			param_multi_extended |= (this._use_2_byte_address) ? (byte)0xC0 : (byte)0x40;
			param_multi_extended |= (this._use_multiblock_cmd) ? (byte)0x0A : (byte)0x05;

			param_optionflag |= (this._use_special_frame) ? (byte)0x3F : (byte)0x15;

			byte[] param_array = new byte[2] { param_multi_extended, param_optionflag };
			CAPDU capdu = new CAPDU(0xFF, 0xFB, 0xFD, 0x00, param_array, 0x00);
			
			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = _channel.Transmit(capdu);

			if (rapdu == null)
				return false;

			Logger.Trace("> " + rapdu.AsString(" "));

			sw = rapdu.SW;
			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("Bad status word " + SCARD.CardStatusWordsToString(rapdu.SW) + " while reading the card");
				return false;
			}
			return true;
		}

		public static bool CL_Control_T5Tmode(ICardApduTransmitter channel, T5T_Mode mode)
		{
			return CL_Control_T5Tmode(channel, mode, out ushort sw);
		}
		public static bool CL_Control_T5Tmode(ICardApduTransmitter channel, T5T_Mode mode, out ushort sw)
		{
			sw = 0x0000;

			CAPDU capdu = new CAPDU(0xFF, 0xFB, 0xFD, (byte)mode, 0x00);

			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
				return false;

			Logger.Trace("> " + rapdu.AsString(" "));

			sw = rapdu.SW;
			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("Bad status word " + SCARD.CardStatusWordsToString(rapdu.SW) + " while reading the card");
				return false;
			}
			return true;
		}

		protected override bool Read()
		{
			CardBuffer buffer = new CardBuffer();
			ushort ndef_offset = (ushort)((_first_ndef_message_offset + _CC.Length) % _block_length);
			ushort block = (ushort)((_first_ndef_message_offset + _CC.Length) / _block_length);
			byte[] data = null;
			ushort missing = 0;
			bool finished = false;

			Logger.Trace("Reading the NFC Forum type 5 Tag");

			/* only if we wre not aligned.. */
			if (_first_ndef_block != null)
			{
				data = new byte[_block_length - ndef_offset];
				Array.Copy(_first_ndef_block, ndef_offset, data, 0, _block_length - ndef_offset);				
				buffer.Append(data);
			}

			block++; /* bypass the block where the NDEF starts */
			while (!finished)
			{
				/* check if we are done or not */
				finished = NfcTlv.CheckIfComplete(buffer.GetBytes(), out missing);

				if (finished || (block >= _first_forbidden_block) )
                {
					finished = true;
                } else
                {
					data = NfcTag.ReadBinary(_channel, block++, 0);
					if (data == null)
					{
						Logger.Error("Error while reading a block!");
					}
					buffer.Append(data);
				}
			}

			if (!ParseUserData(buffer.GetBytes(), out byte[] ndef_data, ref _tlvs))
			{
				Logger.Trace("The parsing of the Tag failed");
				return false;
			}
			if (ndef_data == null)
			{
				Logger.Trace("The Tag doesn't contain a NDEF");
				_is_empty = true;
				return true;
			}
			//Logger.Trace("ndef_data is not NULL");
			_is_empty = false;

            Logger.Trace("ndef_data: " + BinConvert.ToHex(ndef_data));

            NdefObject[] t = NdefObject.Deserialize(ndef_data);
			if (t == null)
			{
				Logger.Trace("The NDEF is invalid or unsupported");
				return false;
			}
            //Logger.Trace("ndef_data:");
            //Logger.Trace("- Length: {0:D}", t.Length);
            Logger.Trace(t.Length + " NDEF record(s) found in the Tag");

			/* This NDEF is the new content of the tag */
            Content.Clear();
			for (int i = 0; i < t.Length; i++)
				Content.Add(t[i]);

			return true;
		}

		public static NfcTagType5 Create(ICardApduTransmitter Channel, T5T_Mode T5T_mode)
        {
			return Create(Channel, true, T5T_mode);
        }


		/**f* SpringCard.NfcForum.Tags/NfcTagType5.Create
		 *
		 * NAME
		 *   NfcTagType5.Create
		 * 
		 * SYNOPSIS
		 *   public static NfcTagType5 Create(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Instanciates a new NfcTagType5 object for this card
		 *
		 * SEE ALSO
		 *   NfcTagType5.Recognize
		 * 
		 **/
		public static NfcTagType5 Create(ICardApduTransmitter Channel, bool read, T5T_Mode T5T_mode)
		{
			NfcTagType5 t = new NfcTagType5(Channel,T5T_mode);
			if (!t._is_valid) return null;
			if (read && !t.Read()) return null;
            return t;
		}

		/**f* SpringCard.NfcForum.Tags/NfcTagType5.Recognize
		 *
		 * NAME
		 *   NfcTagType5.Recognize
		 * 
		 * SYNOPSIS
		 *   public static bool Recognize(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Return true if the card on the reader is a NFC Forum type 5 Tag
		 *
		 * SEE ALSO
		 *   NfcTagType5.Create
		 * 
		 **/
		public static bool Recognize(ICardApduTransmitter channel, T5T_Mode T5T_mode = T5T_Mode.Broadcast)
		{
			NfcTagType5 t = new NfcTagType5(channel, T5T_mode);
			return t._is_valid;
		}
		public static bool RecognizeAtr(SCardChannel channel)
		{
			CardBuffer atr = channel.CardAtr;
			return RecognizeAtr(atr);
		}

		public static bool RecognizeAtr(CardBuffer atr)
		{
			string s = atr.AsString("");
			if (s.Equals(ATR_ICODE_SLI))
			{
				Logger.Trace("ATR: ICODE SLI");
				return true;
			}
			if (s.StartsWith(ATR_BASE))
			{
				Logger.Trace("ATR: NFC Type-5 Tag");
				return true;
			}
			return false;
		}

		public NfcTagType5(ICardApduTransmitter Channel, T5T_Mode T5T_mode) : base(Channel, 5)
		{
			_is_valid = T5TOP_Preamble(Channel, T5T_mode);
			_version = 5;
		}

		/**
		* T5TOP_Preamble – obtain common (read/write/lock) informations
		* */
		private bool T5TOP_Preamble(ICardApduTransmitter channel, T5T_Mode T5T_mode)
        {
			ushort numblock = 0;

			if (!CL_Control_T5Tmode(channel, T5T_mode))
			{
				Logger.Trace("Failed to set mode to " + T5T_mode.ToString());
				return false;
			}
			else
			{
				Logger.Trace("Mode successfully set to " + T5T_mode.ToString());
			}

			byte[] bufferCC = NfcTag.ReadBinary(channel, numblock++, 0); // READ_SINGLE_BLOCK
			if (bufferCC == null)
			{
				Logger.Trace("Failed to read the CC");
				return false;
			}
			_block_length = (ushort)bufferCC.Length;
			Logger.Trace("Block length: {0:D} B", _block_length);

			if (bufferCC.Length != 4 && bufferCC.Length != 8 && bufferCC.Length != 16 && bufferCC.Length != 32)
			{
				Logger.Trace("CC block has invalid size : " + String.Format("{0:D}", bufferCC.Length));
				return false;
			}

			/* CC[0] : Magic number (E1 or E2) */
			if (bufferCC[0] != NFC_FORUM_MAGIC_NUMBER && bufferCC[0] != NFC_FORUM_MAGIC_NUMBER_2)
			{
				Logger.Trace("CC Field 0 is invalid");
				return false;
			}

			_use_2_byte_address = (bufferCC[0] == 0xE2);
			Logger.Trace(_use_2_byte_address ? "2-byte addresses" : "1-byte addresses");

			/* CC[1] : Version & Access conditions */
			byte read_access = (byte)((bufferCC[1] >> 2) & 0x03);
			if (read_access == 0x01 || read_access == 0x11)
			{
				Logger.Trace("CC Field 1 is invalid : read_access has RFU value");
				return false;
			}
			if (read_access == 0x10)
			{
				Logger.Trace("CC Field 1 is invalid : read_access has proprietary value");
				return false;
			}
			byte write_access = (byte)(bufferCC[1] & 0x03);
			if (write_access == 0x01)
			{
				Logger.Trace("CC Field 1 is invalid : write_access has RFU value");
				return false;
			}

			_version_major = (ushort)((bufferCC[1] >> 6) & 0x03);
			_version_minor = (ushort)((bufferCC[1] >> 4) & 0x03);
			Logger.Trace("Version: {0:D}.{1:D}", _version_major, _version_minor);
			_locked = (write_access != 0x00);
			Logger.Trace(_locked ? "Write-locked" : "Writable");

			/* Version Treating */
			if ((_version_major > major_T5T_RW_VNo) || (_version_major == major_T5T_RW_VNo && _version_minor > minor_T5T_RW_VNo))
			{
				Logger.Trace("Tag mapping version {0:D}.{1:D} is not supported. Latest compatible version is {2:D}.{3:D}", _version_major, _version_minor, major_T5T_RW_VNo, minor_T5T_RW_VNo);
				return false;
			}

			/* CC[3] : Message Length (MLEN) */
			_use_special_frame = ((bufferCC[3] >> 4) & 0x01) == 0x01;
			_lockable = ((bufferCC[3] >> 3) & 0x01) == 0x01;
			_use_multiblock_cmd = (bufferCC[3] & 0x01) == 0x01;
			Logger.Trace("Special frame: " + (_use_special_frame ? "YES" : "NO"));			
			Logger.Trace("Lockable: " + (_lockable ? "YES" : "NO"));			
			Logger.Trace("Multiblock: " + (_use_multiblock_cmd ? "YES" : "NO"));

			/* default capacity rule */
			_capacity_from_cc = 8 * (uint)bufferCC[2];

			/* CC[2] : T5T_Area Length (MLEN) */
			if (bufferCC[2] == 0x00)
			{
				if (bufferCC.Length == 4)
				{
					byte[] fullCC = new byte[8];
					bufferCC.CopyTo(fullCC, 0);
					byte[] endCC = NfcTag.ReadBinary(channel, numblock++, 0);
					if (endCC == null)
					{
						Logger.Trace("Fail reading CC");
						return false;
					}
					endCC.CopyTo(fullCC, 4);

					if (endCC[2] == 0 && endCC[3] == 0)
					{
						Logger.Trace("CC indicates size=0");
						return false;
					}
					bufferCC = fullCC;
				}
				
				/* update capacity */
				_capacity_from_cc = 0x100 * (uint)bufferCC[6] + bufferCC[7];
				_capacity_from_cc *= 8;
			}
			
			Logger.Trace("TLV_Area: {0:D} B", _capacity_from_cc);

			_CC = new byte[_use_2_byte_address ? 8 : 4];
			Array.Copy(bufferCC, _CC, _CC.Length);
			Logger.Trace("CC: " + BinConvert.ToHex(_CC));

			_initreceived = bufferCC;
			_usable_tlv_area_size = _capacity_from_cc;
			_physical_capacity_from_cc = _capacity_from_cc;

			long nb_full_blocks = (_CC.Length + _usable_tlv_area_size) / _block_length;
			long nb_lastbytes = (_CC.Length + _usable_tlv_area_size) % _block_length;
			_first_forbidden_block = nb_full_blocks + (nb_lastbytes < 2 ? 0 : 1);
			Logger.Trace("Tag contains {0:D} blocks ({1:D}*{2:D}+{3:D} B)", _first_forbidden_block, nb_full_blocks, _block_length, nb_lastbytes);

			_formattable = false;
			_formatted = true; // The tag has a valid CC


			/* try to obtain NDEF informations */
			CardBuffer buffer = new CardBuffer();
			byte[] data = null;
			ushort block_number = (ushort)(_block_length >= _CC.Length ? 1 : _CC.Length / _block_length);

			/* check if the NDEF starts within the fisrt block */
			if (_CC.Length < _block_length)
			{
				data = new byte[_block_length - _CC.Length];
				BinUtils.CopyTo(data, 0, bufferCC, _CC.Length, _block_length - _CC.Length);
				buffer.Append(data);

				bool found = false;
				NfcTlv.FindFirstNDEFMessage(buffer.GetBytes(), 0, out _first_ndef_message_offset, out found);

				if (found)
				{
					Logger.Trace("NDEF message found (1st block) at offset {0:X4}", _first_ndef_message_offset);
					_first_ndef_block = bufferCC;
					return true;
				}
			}

			/* we can't work on an empty array */
#if false
			if (data == null)
			{
				data = ReadBinary(_channel, block_number++, 0);
				if (data == null)
				{
					Logger.Error("Error while looking for NDEF Message TLV!");
				}
				buffer.Append(data);
			}
#endif
			bool useBypasser = false;
			RegistryCfgFile registry = RegistryCfgFile.OpenApplicationSectionReadWrite();
			if (registry != null)
			{
				useBypasser = registry.ReadBoolean("UIT_T5T_RFU_BYPASS", false);
				if ( useBypasser)
                {
					Logger.Trace("IUT uses T5T bypasser");
				}
			}
			

			ushort bypasser = 0;
			while (block_number< _first_forbidden_block)
            {
				bool found = false;

				if (buffer.Length != 0)
				{
					NfcTlv.FindFirstNDEFMessage(buffer.GetBytes(), 0, out _first_ndef_message_offset, out found, out bypasser);

					if (found)
					{
						Logger.Trace("NDEF message found at offset {0:X4}", _first_ndef_message_offset);

						/* copy it, we may use it to write a non block aligned NDEF */
						_first_ndef_block = data;
						break;
					}

					if (useBypasser && (bypasser > 0) && (buffer.Length < bypasser))
					{
						ushort bypass_block_num = (ushort)((bypasser - buffer.Length) / _block_length);
						if (bypass_block_num > 0)
						{
							byte[] bypass_data = new byte[bypass_block_num * _block_length];

							Logger.Trace("Bypasser = {0}, Buffer size={1}", bypasser, buffer.Length);
							Logger.Trace("Bypassing {0} block(s)", (bypasser - buffer.Length) / _block_length);
							block_number += bypass_block_num;
							_capacity_from_cc -= bypass_block_num * _block_length;
							buffer.Append(bypass_data);
						}
					}

					/* decrease actual capacity */
					_capacity_from_cc -= _block_length;
				}

				/* not found, have a look at next pages */
				data = ReadBinary(_channel, block_number++, 0);
				if (data == null)
				{
					Logger.Error("Error while looking for NDEF Message TLV!");
					return false;
				}
				buffer.Append(data);
			}

			Logger.Trace("Available capacity for writing {0} bytes", _capacity_from_cc);

			return true;
		}

	}
}
