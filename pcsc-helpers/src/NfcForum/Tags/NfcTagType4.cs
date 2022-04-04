/**h* SpringCard.NfcForum.Tags/NfcTag
 *
 * NAME
 *   SpringCard API for NFC Forum :: NfcTagType4 class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.NfcForum.Ndef;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;

namespace SpringCard.NfcForum.Tags
{
	/**c* SpringCard.NfcForum.Tags/NfcTagType4
	 *
	 * NAME
	 *   NfcTagType4
	 * 
	 * DERIVED FROM
	 *   NfcTag
	 * 
	 * DERIVED BY
	 *   NfcTagType4_Desfire
	 * 
	 * DESCRIPTION
	 *   Represents a Type 4 NFC Tag that has been discovered on a reader.
	 *
	 * SYNOPSIS
	 *   if (NfcTagType4.Recognize(channel))
	 *     NfcTag tag = NfcTagType4.Create(SCardChannel channel)
	 *
	 **/
	public partial class NfcTagType4 : NfcTag
	{
		private const string NDEF_APPLICATION_ID_MAPPING_V2_PLUS = "D2760000850101";
		private const string NDEF_APPLICATION_ID_MAPPING_V1_ONLY = "D2760000850100";
		private const ushort NDEF_CC_FILE_ID = 0xE103;

		private List<NfcTlv> _tlvs = new List<NfcTlv>();

		private bool _application_selected = false;
		private static ushort _ndef_file_id = 0;
		private static long _ndef_file_size = 0;
		private static ushort _file_selected = 0;

		/* Defines the maximum data size that can be read from the T4T using a single READ_BINARY Command.
		 * Value			Description
		 * 0000h-000Eh		RFU
		 * 000Fh-FFFFh		Valid range */
		private ushort _MLe = 0;

		/* Defines the maximum data size that can be sent to the T4T using a single Command.
		 * Value			Description
		 * 0000h-000Ch		RFU
		 * 000Dh-FFFFh		Valid range (Needs at least to be able to send the Select NDEF Tag Application 
		 *					C-APDU to the T4T.) */
		private ushort _MLc = 0;

		/* Indicates the Mapping Version that is implemented on the T4T (see Section 4.5). Section 4.3.2 
		 * defines how the Reader/Writer needs to handle different Mapping Versions.
		 * Value			Description
		 * 20h				Mapping Version 2.0 (with the Standard Data Structure)
		 * 30h				Mapping Version 3.0 (with the Extended Data Structure) */
		private byte _T4T_VNo;

		/* this has to be gathered from the device itself, using a const for testing!! */
		private int _CCID_MAX_BUFFER_SZ = 254;
		
		/**
		 * Constructor
		 * */
		public NfcTagType4(ICardApduTransmitter Channel) : base(Channel, 4)
		{
			/* A type 4 Tag can always be locked */
			_lockable = true;

			/* The T2TOP_Preamble precedes the scenario tables for all test cases. */
			_is_valid = T4TOP_Preamble(Channel, ref _formatted, ref _formattable, ref _locked, ref _MLe, ref _MLc, ref _ndef_file_id, ref _ndef_file_size, ref _T4T_VNo);

			if (!_is_valid)
				return;

			/* we must set capacity at discovery stage */
			_capacity_from_cc = 0;
			if (_T4T_VNo <= 0x20)
			{
				if (_ndef_file_size > 2)
					_capacity_from_cc = _ndef_file_size - 2;
			}
			else
			if (_T4T_VNo == 0x30)
			{
				if (_ndef_file_size > 4)
					_capacity_from_cc = _ndef_file_size - 4;
			}

			RegistryCfgFile registry = RegistryCfgFile.OpenApplicationSectionReadWrite();
			if (registry != null)
			{
				_CCID_MAX_BUFFER_SZ = registry.ReadInteger("UIT_CCID_MAX_BUFFER_SIZE", 254);
			}
			Logger.Trace("IUT's APDU buffer size is {0:d} bytes long", _CCID_MAX_BUFFER_SZ);

			_version = 4;
		}


		/**
		 * T4TOP_Preamble – obtain common (read/write/lock) informations
		* */
		private bool T4TOP_Preamble(ICardApduTransmitter channel, ref bool formatted, ref bool formattable, ref bool write_protected, ref ushort max_le, ref ushort max_lc, ref ushort ndef_file_id, ref long ndef_file_size, ref byte mapping_version)
		{

			formatted = false;
			formattable = false;

			/* Select NDEF Tag Application */
			if (!SelectNfcApplication(channel, NDEF_APPLICATION_ID_MAPPING_V2_PLUS))
			{
				/* we may try to go back to the root first! */
				SelectRootApplication(channel);

				/* and try again */
				if (!SelectNfcApplication(channel, NDEF_APPLICATION_ID_MAPPING_V2_PLUS))
				{
					/* still not good, that's maybe an old v1 mapping..? */
					if (!SelectNfcApplication(channel, NDEF_APPLICATION_ID_MAPPING_V1_ONLY))
					{
						return false;
					}
				}
			}


			/* Select CC File */
			if (!SelectFile(channel, NDEF_CC_FILE_ID))
				return false;

			/* Get CC file lenth!  */
			byte[] cc_file_length = ReadBinary(channel, 0, 2);
			if ((cc_file_length == null) || (cc_file_length.Length != 2))
			{
				Logger.Trace("Failed to read the CC file length!");
				return false;
			}

			long cc_length = cc_file_length[0] * 0x0100 + cc_file_length[1];
			if (cc_length < 15)
			{
				Logger.Trace("Bad length in the CC file");
				return false;
			}

			Logger.Trace("CC file is {0:d} bytes long", cc_length);

			/* actually read the cc file */
			byte[] cc_file_content = ReadBinary(channel, 0, (byte)cc_length);

			if ((cc_file_content == null) || (cc_file_content.Length != cc_length))
			{
				Logger.Trace("Failed to read the CC file");
				return false;
			}

			mapping_version = (byte)(cc_file_content[2] & 0xF0);
			if ((mapping_version != 0x10) && (mapping_version != 0x20) && (mapping_version != 0x30))
			{
				Logger.Trace("Bad version in the CC file");
				return false;
			}

			max_le = (ushort)(cc_file_content[3] * 0x0100 + cc_file_content[4]);
			max_lc = (ushort)(cc_file_content[5] * 0x0100 + cc_file_content[6]);
			Logger.Trace("MLe={0:d}", max_le);
			Logger.Trace("MLc={0:d}", max_lc);

			Logger.Trace("Using Mapping Version {0:X2}", mapping_version);
			if (mapping_version == 0x20)
			{
				if (cc_file_content[7] != NDEF_FILE_CONTROL_TLV)
				{
					Logger.Trace("No NDEF File Control TLV in the CC file");
					return false;
				}

				if (cc_file_content[8] != 6)
				{
					Logger.Trace("Bad TLV's Length in the CC file");
					return false;
				}

				ndef_file_id = (ushort)(cc_file_content[9] * 0x0100 + cc_file_content[10]);
				Logger.Trace("Using File ID: " + String.Format("{0:X4}", ndef_file_id));

				ndef_file_size = (long)(cc_file_content[11] * 0x0100 + cc_file_content[12]);
				Logger.Trace("File size: {0:d}", ndef_file_size);

				if (cc_file_content[13] != 0x00)
				{
					Logger.Trace("No read access");
					return false;
				}

				if (cc_file_content[14] != 0x00)
				{
					Logger.Trace("No write access");
					write_protected = true;
				}
				else
					write_protected = false;
			}
			else
			if (mapping_version == 0x30)
			{
				if (cc_file_content[7] != ENDEF_FILE_CONTROL_TLV)
				{
					Logger.Trace("No ENDEF File Control TLV in the CC file");
					return false;
				}

				if (cc_file_content[8] != 8)
				{
					Logger.Trace("Bad TLV's Length in the CC file");
					return false;
				}

				ndef_file_id = (ushort)(cc_file_content[9] * 0x0100 + cc_file_content[10]);
				Logger.Trace("Using File ID: " + String.Format("{0:X4}", ndef_file_id));

				ndef_file_size = (long)((cc_file_content[11] << 24) | (cc_file_content[12] << 16) | (cc_file_content[13] << 8) | (cc_file_content[14] << 0));
				Logger.Trace("File size: {0:d}", ndef_file_size);

				if (cc_file_content[15] != 0x00)
				{
					Logger.Trace("No read access");
					return false;
				}

				if (cc_file_content[16] != 0x00)
				{
					Logger.Trace("No write access");
					write_protected = true;
				}
				else
					write_protected = false;

			}

			formatted = true;

			return true;
		}

		public override bool Format()
		{
			return false;
		}

		public override bool Lock()
		{
			if (!SelectNfcApplication(_channel))
				return false;
			if (!SelectFile(NDEF_CC_FILE_ID))
				return false;

			byte[] cc_write_control = ReadBinary(14, 1);
			if (cc_write_control == null)
				return false;

			cc_write_control[0] = 0xFF;
			return WriteBinary(14, cc_write_control);
		}

		protected bool SelectFile(ushort file_id)
		{
			if (!SelectFile(_channel, file_id))
				return false;
			_file_selected = file_id;
			return true;
		}


		/**
         * T4TOP_WriteNDEFLength – write (or clear) the NDEF message length
         * The Reader/Writer MAY write the first part of the NDEF Message 
         * in the same WRITE Command as used for the reset of NLEN or 
         * ENLEN field.
         * */
		private bool T4TOP_WriteNDEFLength(ICardApduTransmitter channel, ulong ndefLength)
		{
			byte[] length;

			if (_T4T_VNo == 0x20)
			{
				length = new byte[2];
				length[0] = (byte)((ndefLength >> 8) & 0xFF);
				length[1] = (byte)((ndefLength >> 0) & 0xFF);

			} else
			if (_T4T_VNo == 0x30)
            {
				length = new byte[4];
				length[0] = (byte)((ndefLength >> 24) & 0xFF);
				length[1] = (byte)((ndefLength >> 16) & 0xFF);
				length[2] = (byte)((ndefLength >> 8) & 0xFF);
				length[3] = (byte)((ndefLength >> 0) & 0xFF);
			} else
            {
				return false;
            }

			/* change length */
			return WriteBinary(_channel, 0, length);
		}

		protected override bool WriteContent(byte[] ndef_content)
		{
			Logger.Trace("Writing the NFC Forum Type 4 Tag (length={0:d})", ndef_content.Length);
			long toWrite;
			long rawSize;

			long offset_in_content = 0;
			long offset_in_file = 2;

			if (ndef_content == null)
				return false;

			/* 
			 * The Reader/Writer MAY write the first part of the NDEF Message
			 * in the same WRITE Command as used for the reset of NLEN or ENLEN field.
			 */			
			if (_T4T_VNo == 0x30)
			{
				offset_in_file = 4;				
			}

			CardBuffer actual_content = new CardBuffer();
			actual_content.Append(ndef_content);
			toWrite = actual_content.Length;

			/* this is the NDEF message size (without extra TERMINATOR) */
			rawSize = ndef_content.Length;

			if (actual_content.Length > Capacity())
			{
				Logger.Trace("The size of the content (with its TLVs) is bigger than the tag's capacity");
				return false;
			}

			/* select destination file! */
			if (!SelectFile(_ndef_file_id))
			{
				Logger.Trace("Failed to select the ENDEF file");
				return false;
			}

			/* start by clearing the actual Length field! */
			if (!T4TOP_WriteNDEFLength(_channel, 0))
				return false;

			/* write the new ndef message */
			long canWrite;
			bool finished = false;
			while (!finished)
			{				
				canWrite = T4T_GuessMaxLc();

				if (canWrite > toWrite)
					canWrite = toWrite;

				Logger.Trace("We can write at most {0:d} bytes starting at offset {1:d}", canWrite, offset_in_file);

				byte[] buffer = new byte[canWrite];
				BinUtils.CopyTo(buffer, 0, actual_content.Bytes, (int)offset_in_content, (int)canWrite);

				if ((offset_in_file > 32767) && (_T4T_VNo < 0x30))
				{
					Logger.Trace("ENDEF message is not allowed with a mapping version < 0x30!");
					return false;
				}

				if (!WriteBinary(_channel, offset_in_file, buffer))
				{
					Logger.Trace("Failed to write the NDEF file at offset " + offset_in_file);
					return false;
				}

				/* prepare for next chunk */
				offset_in_file += canWrite;
				offset_in_content += canWrite;
				toWrite -= canWrite;

				Logger.Trace("{0:d} bytes have been written, remains {1:d}", canWrite, toWrite);

				/* have we reached the end? */
				if (toWrite == 0)
				{
					finished = true;
				}
			}
			
			/* eventually write the final header with Length field! */
			if (!T4TOP_WriteNDEFLength(_channel, (ulong)rawSize))
				return false;

			return true;
		}


		protected override bool Read()
		{
			Logger.Trace("Reading the NFC Forum Type 4 Tag");

			long offset_in_file = 0;
			long ndef_announced_size = 0;

			/* at this stage we know: */
			_formatted = true;
			_application_selected = true;
			_file_selected = NDEF_CC_FILE_ID;			

			if (_T4T_VNo <= 0x20)
			{				
				if (!SelectFile(_ndef_file_id))
				{
					Logger.Trace("Failed to select the NDEF file");
					return false;
				}

				byte[] sizeChecker = ReadBinary(0, 2);
				offset_in_file += 2;
				if (sizeChecker == null)
				{
					Logger.Trace("Failed to read from the NDEF file");
					return false;
				}

				ndef_announced_size = (sizeChecker[0] << 8) | (sizeChecker[1] << 0);
				if (ndef_announced_size <= 4 )
				{
					Logger.Trace("Tag is empty");
					_is_empty = true;
					return true;
				}

				_is_empty = false;
/*
				ndef_announced_size = (long)(sizeChecker[0] * 0x0100 + sizeChecker[1]);

				if ((ndef_announced_size > (_ndef_file_size - 2)) || (ndef_announced_size > 0xFFFF))
				{
					Logger.Trace("The NDEF file contains an invalid length");
					return false;
				}
*/
			} else
			if (_T4T_VNo == 0x30)
			{
				if (!SelectFile(_ndef_file_id))
				{
					Logger.Trace("Failed to select the ENDEF file");
					return false;
				}

				byte[] sizeChecker = ReadBinary(0, 4);
				offset_in_file += 4;
				if (sizeChecker == null)
				{
					Logger.Trace("Failed to read from the ENDEF file");
					return false;
				}

				ndef_announced_size = (sizeChecker[0] << 24) | (sizeChecker[1] << 16) | (sizeChecker[2] << 8) | (sizeChecker[3] << 0);
				if (ndef_announced_size <= 6)
				{
					Logger.Trace("Tag is empty");
					_is_empty = true;
					return true;
				}

				_is_empty = false;
/*
				ndef_announced_size = (long)(sizeChecker[0] * 0x0100 + sizeChecker[1]);
				if ((ndef_announced_size > (_ndef_file_size - 4)) || (ndef_announced_size > 0xFFFF))
				{
					Logger.Trace("The ENDEF file contains an invalid length");
					return false;
				}
*/
			} else
            {
				Logger.Trace("Invalid mapping version!");
				return false;
			}

			/* now we can read! */
			Logger.Trace("NDEF message size: {0:d}", ndef_announced_size );			
			CardBuffer buffer = new CardBuffer();
			byte[] content = new byte[ndef_announced_size];

			
			long toRead = ndef_announced_size;
			long canRead;
			bool finished = false;
			
			while (!finished)
            {				
				canRead = T4T_GuessMaxLe();

				if (canRead > toRead)
					canRead = toRead;

				Logger.Trace("We can read at most {0:d} starting at offset {1:d}", canRead, offset_in_file);

				if ( ( offset_in_file > 32767) && (_T4T_VNo < 0x30 ) )
                {
					Logger.Trace("ENDEF message is not allowed with a mapping version < 0x30!");
                    return false;
				}
				Logger.Trace("Ready to read!");
				byte[] data = ReadBinary(_channel, offset_in_file, (ushort) canRead);
				if (data == null)
				{
					Logger.Trace("Unable to read!");
					return false;
				}

				/* append the new chunk to the final buffer */
				buffer.Append(data);

				/* prepare for next chunk */
				offset_in_file += data.Length;
				toRead -= data.Length;

				Logger.Trace("{0:d} bytes have been read, remains {1:d}", data.Length, toRead);

				/* have we reached the end? */
				if ( toRead == 0 )
				{
					finished = true;
                }
			}

			Logger.Trace("Read " + buffer.Length + " bytes of data from the Tag");
			
			byte[] raw_data = buffer.GetBytes();

			/* Is the tag empty ? */
			_is_empty = true;
			for (long i = 0; i < _capacity_from_cc; i++)
			{
				if (raw_data[i] != 0)
				{
					_is_empty = false;
					break;
				}
			}

			if (_is_empty)
			{
				Logger.Trace("The Tag is empty");
				return true;
			}

			NdefObject[] t = NdefObject.Deserialize(raw_data);
			if (t == null)
			{
				Logger.Trace("The NDEF is invalid or unsupported");
				return false;
			}

			Logger.Trace(t.Length + " NDEF record(s) found in the Tag");

			/* This NDEF is the new content of the tag */
			Content.Clear();
			for (int i = 0; i < t.Length; i++)
				Content.Add(t[i]);

			return true;


		}

		/* Calculate possible data length including a potential "Command with ODO header" */
		private long T4T_GuessMaxLe()
        {
			long deviceCapability = _CCID_MAX_BUFFER_SZ;
			long cardCapability = _MLe;

			/* check device's capabilities first! */
			if (cardCapability > deviceCapability)
				cardCapability = deviceCapability;
						
			if ( cardCapability <= 127 )
            {
				/* A maximum of 127 - bytes of NDEF Message following tags 53h + read
					data length ? {‘00’, …,‘7F’} */
				cardCapability -= 2;
			} else
			if (cardCapability <= 253)
			{
				/* A maximum of 253-bytes of NDEF Message following tags 53h + 81h +
					read data length ? {‘00’, …, ‘FD’} */
				cardCapability -= 3;

			} else
            {
				/* A number of bytes not exceeding (MLe minus 4) bytes of NDEF Message
					following tags 53h + 82h + read data length ? {‘0000’, …, MLe - 4} bytes
					if Le indicates Extended Field Coding */
				cardCapability -= 4;
			}

			return cardCapability;
		}


		/* Calculate possible data length including a potential "Command with ODO header" */
		private long T4T_GuessMaxLc()
		{
			long deviceCapability = _CCID_MAX_BUFFER_SZ;
			long cardCapability = _MLc;

			/* check device's capabilities first! */
			if (cardCapability > deviceCapability)
				cardCapability = deviceCapability;
						
			if (cardCapability <= 127)
			{
				/* A maximum of 127 - bytes of NDEF Message following tags 53h + read
					data length ? {‘00’, …,‘7F’} */
				cardCapability -= 15;
			}
			else
			if (cardCapability <= 253)
			{
				/* A maximum of 253-bytes of NDEF Message following tags 53h + 81h +
					read data length ? {‘00’, …, ‘FD’} */
				cardCapability -= 16;

			}
			else
			{
				/* A number of bytes not exceeding (MLe minus 4) bytes of NDEF Message
					following tags 53h + 82h + read data length ? {‘0000’, …, MLe - 4} bytes
					if Le indicates Extended Field Coding */
				cardCapability -= 17;
			}

			return cardCapability;
		}

		private bool SelectFile(ICardApduTransmitter channel, ushort file_id)
		{
			CAPDU capdu = new CAPDU(0x00, 0xA4, 0x00, 0x0C, (new CardBuffer(file_id)).GetBytes());

			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("SelectFile " + String.Format("{0:X4}", file_id) + " error");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("SelectFile " + String.Format("{0:X4}", file_id) + " failed " + SCARD.CardStatusWordsToString(rapdu.SW) + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}

			return true;
		}

		private bool SelectRootApplication(ICardApduTransmitter channel)
		{
			CAPDU capdu = new CAPDU(0x00, 0xA4, 0x00, 0x00, new byte[] { 0x3F, 0x00 }, 0x00);

			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("SelectRootApplication error");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("SelectRootApplication failed " + SCARD.CardStatusWordsToString(rapdu.SW) + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}

			return true;
		}

		private bool SelectNfcApplication(ICardApduTransmitter channel, string mapping_version_selector)
		{
			CAPDU capdu = new CAPDU(0x00, 0xA4, 0x04, 0x00, (new CardBuffer(mapping_version_selector)).GetBytes(), 0x00);

			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("SelectNfcApplication error");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("SelectNfcApplication failed " + SCARD.CardStatusWordsToString(rapdu.SW) + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}

			return true;
		}

		private bool SelectNfcApplication(ICardApduTransmitter channel)
		{
			return SelectNfcApplication(channel, NDEF_APPLICATION_ID_MAPPING_V2_PLUS);
		}



		private byte[] ReadBinary(long offset, ushort length)
		{
			byte[] r = ReadBinary(_channel, offset, length);
			if (r == null)
			{
				_application_selected = false;
				_file_selected = 0;
			}
			return r;
		}

		private byte[] ReadBinary(ICardApduTransmitter channel, long address, ushort expectedLength)
		{
			CAPDU capdu;

			/* more and 8KB? */
			if ( address > 32767)			
			{
				/* BER-TLV Lc is always present in that case! 
                 * if Lc uses Extended Field coding, so do Le!
                 */
				byte[] cmd = new byte[4];
				cmd[0] = 0x00;
				cmd[1] = 0xB1;
				cmd[2] = 0x00;
				cmd[3] = 0x00;

				byte[] ODO = NfcTlv.CreateODOField(address);

				/* allocate room for the BER-TLV answer header */
				if (expectedLength > 253)
				{
					/* A number of bytes not exceeding (MLe minus 4) bytes of NDEF Message
					following tags 53h + 82h + read data length ∈ {‘0000’, …, MLe - 4} bytes
					if Le indicates Extended Field Coding */
					expectedLength += 4;
				}
				else
				if (expectedLength > 127)
				{
					/* A maximum of 253-bytes of NDEF Message following tags 53h + 81h +
					read data length ∈ {‘00’, …, ‘FD’} */
					expectedLength += 3;
				}
				else
				{
					/* A maximum of 127 - bytes of NDEF Message following tags 53h + read
					data length ∈ {‘00’, …,‘7F’} */
					expectedLength += 2;
				}

				Logger.Trace("Expected length is {0:d}", expectedLength);


				/* Preare Le and Lc fields */
				byte[] Le = null;
                byte[] Lc = null;
				if (expectedLength <= 256)
				{
					/* use Short Field coding */
					Lc = NfcTlv.CreateLcField(5, false);					
					Le = NfcTlv.CreateLeField(expectedLength, true, false);
				}
                {
					/* use Extended Field coding */
					Lc = NfcTlv.CreateLcField(5, true);
					Le = NfcTlv.CreateLeField(expectedLength, true, true);
				}

				cmd = BinUtils.Concat(cmd, Lc);
				cmd = BinUtils.Concat(cmd, ODO);
				cmd = BinUtils.Concat(cmd, Le);
				capdu = new CAPDU(cmd);
			}
			else
			{							
				byte[] cmd = new byte[4];
				cmd[0] = 0x00;
				cmd[1] = 0xB0;
				cmd[2] = (byte)(address / 0x0100);
				cmd[3] = (byte)(address % 0x0100);

				/* Le will use Short or Extended Field coding */
				byte[] Le = NfcTlv.CreateLeField(expectedLength, false);
				cmd = BinUtils.Concat(cmd, Le);
				capdu = new CAPDU(cmd);				
			}
			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("ReadBinary " + String.Format("{0:X4}", address) + "," + String.Format("{0:X2}", (byte)expectedLength) + " error");
				return null;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("ReadBinary " + String.Format("{0:X4}", address) + "," + String.Format("{0:X2}", (byte)expectedLength) + " failed " + SCARD.CardStatusWordsToString(rapdu.SW) + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return null;
			}

			if (rapdu.hasData)
			{
				ushort index = 0;
				byte[] temp = rapdu.data.GetBytes();
				int rlength = temp.Length;

				if (address > 32767)
				{
					/* check BER-TLV */
					if (temp[index++] != 0x53 )
						return null;
										
					rlength--;

					/* length extension? */
					if (temp[index] == 0x81)
					{
						index += 2;
						rlength -= 2;
					} else
					if (temp[index] == 0x82)
					{
						index += 3;
						rlength -= 3;
					} else
                    {
						index++;
						rlength--;
					}

					return rapdu.data.GetBytes(index, rlength);
				}

				return rapdu.data.GetBytes();
			}
			return null;
		}



		private bool WriteBinary(long offset, byte[] buffer)
		{
			bool r = WriteBinary(_channel, offset, buffer);
			if (!r)
			{
				_application_selected = false;
				_file_selected = 0;
			}
			return r;
		}

		private bool WriteBinary(ICardApduTransmitter channel, long address, byte[] buffer)
		{
			CAPDU capdu;
            long length = buffer.Length;

			if (buffer == null)
				return false;

			if (length == 0)
				return false;

			Logger.Trace("Need to write {0:d} bytes", length);

			if (address > 32767)
			{
				byte[] cmd = new byte[4];
				cmd[0] = 0x00;
				cmd[1] = 0xD7;
				cmd[2] = 0x00;
				cmd[3] = 0x00;

				byte[] ODO = NfcTlv.CreateODOField(address);
				byte[] DDO = NfcTlv.CreateDDOField(length);
				long LcLength = length + DDO.Length + ODO.Length;
				byte[] Lc = NfcTlv.CreateLcField(LcLength);

				cmd = BinUtils.Concat(cmd, Lc);
				cmd = BinUtils.Concat(cmd, ODO);
				cmd = BinUtils.Concat(cmd, DDO);
				cmd = BinUtils.Concat(cmd, buffer);
				capdu = new CAPDU(cmd);
				
			}
			else
			{
				byte[] cmd = new byte[4];
				cmd[0] = 0x00;
				cmd[1] = 0xD6;
				cmd[2] = (byte)(address / 0x0100);
				cmd[3] = (byte)(address % 0x0100);

				byte[] Lc = NfcTlv.CreateLcField(length);

				cmd = BinUtils.Concat(cmd, Lc);
				cmd = BinUtils.Concat(cmd, buffer);
                capdu = new CAPDU(cmd);
			}

			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Logger.Trace("WriteBinary " + String.Format("{0:X4}", address) + "," + String.Format("{0:d}", buffer.Length) + " error");
				return false;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("WriteBinary " + String.Format("{0:X4}", address) + "," + String.Format("{0:d}", buffer.Length) + " failed " + SCARD.CardStatusWordsToString(rapdu.SW) + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}
			return true;
		}


		/**f* SpringCard.NfcForum.Tags/NfcTagType4.Create
		*
		* NAME
		*   NfcTagType4.Create
		* 
		* SYNOPSIS
		*   public static NfcTagType4 Create(SCardChannel channel)
		*
		* DESCRIPTION
		*   Instanciates a new NfcTagType4 object for this card
		*
		* SEE ALSO
		*   NfcTagType4.Recognize
		* 
		**/
		public static NfcTagType4 Create(SCardChannel channel)
		{
			return Create(channel, true);
		}

		public static NfcTagType4 Create(SCardChannel channel, bool read)
		{
			NfcTagType4 t = new NfcTagType4(channel);
			if (!t._is_valid) return null;
			if (read && !t.Read()) return null;
			return t;
		}


		/**f* SpringCard.NfcForum.Tags/NfcTagType4.Recognize
		 *
		 * NAME
		 *   NfcTagType3.Recognize
		 * 
		 * SYNOPSIS
		 *   public static bool Recognize(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Returns true if the card on the reader is a NFC Forum Type 4 Tag
		 *
		 * SEE ALSO
		 *   NfcTagType4.Create
		 * 
		 **/
		public static bool Recognize(ICardApduTransmitter channel)
		{
			NfcTagType4 t = new NfcTagType4(channel);
			return t._is_valid;
		}

	}
}
