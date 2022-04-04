/**h* SpringCard.NfcForum.Tags/NfcTag
 *
 * NAME
 *   SpringCard API for NFC Forum :: NfcTag class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.Collections.Generic;
using SpringCard.NfcForum.Ndef;
using SpringCard.LibCs;
using SpringCard.PCSC;
using System.Threading;


/**c* SpringCard.NfcForum.Tags/NfcTag
 *
 * NAME
 *   NfcTag
 * 
 * DESCRIPTION
 *   Represents an NFC Tag that has been discovered on a reader
 *
 * DERIVED BY
 *   NfcTagType2
 *   NfcTagType4
 *   NfcTagType5
 *
 * SYNOPSIS
 *   NfcTag tag = new NfcTag(SCardChannel channel)
 *
 **/
namespace SpringCard.NfcForum.Tags
{
	public enum T5T_Mode : byte
	{
		Broadcast = 0x10,
		Addressed = 0x11,
		Selected = 0x12
	}
	public abstract class NfcTag
	{
		public const byte NFC_FORUM_MAGIC_NUMBER	= 0xE1;
		public const byte NFC_FORUM_MAGIC_NUMBER_2  = 0xE2;
		public const byte NFC_FORUM_VERSION_NUMBER	= 0x10;
		
		public const byte LOCK_CONTROL_TLV 		= 0x01;
		public const byte MEMORY_CONTROL_TLV 	= 0x02;
		public const byte NDEF_MESSAGE_TLV 		= 0x03;
		public const byte NDEF_FILE_CONTROL_TLV = 0x04;
		public const byte ENDEF_FILE_CONTROL_TLV = 0x06;
		public const byte PROPRIETARY_TLV 		= 0xFD;
		public const byte TERMINATOR_TLV 		= 0xFE;
		public const byte NULL_TLV				= 0x00;
		
		protected ICardApduTransmitter _channel = null;

		protected bool _is_valid = false;				/* flag to know if the tag is consistent or not */
		protected long _capacity_from_cc = 0;           /* size of the Tag's NDEF storage container (value from CC) */
		protected long _usable_tlv_area_size = 0;		/* usable tlv area */
		protected long _physical_capacity_from_cc = 0;	/* physical size of the tag */
		protected bool _is_empty = false;
		protected bool _formatted = false;
		protected bool _formattable = false;
		protected bool _locked = false;
		protected bool _lockable = false;
		protected byte _version = 0;

		public byte Type { get; private set; }

		protected NfcTag(ICardApduTransmitter Channel, byte Type)
		{
			_channel = Channel;
			this.Type = Type;
		}

		/**m* SpringCard.NfcForum.Tags/NfcTag.Content
		 *
		 * SYNOPSIS
		 *   public List<NDEF> Content
		 * 
		 * DESCRIPTION
		 *   The list of NDEF objects found on the Tag.
		 *   To change the Tag's content, update this list before
		 *   calling NfcTag.Write
		 *
		 * SEE ALSO
		 *   NfcTag.ContentSize
		 *
		 **/
		public List<NdefObject> Content = new List<NdefObject>();

		public byte GetVersion()
        {
			return _version;
		}

		protected abstract bool WriteContent(byte[] content);
		
		/**m* SpringCard.NfcForum.Tags/NfcTag.Recognize
		 *
		 * SYNOPSIS
		 *   public static bool Recognize(SCardChannel cardchannel, out NfcTag tag, out string msg)
		 * 
		 * DESCRIPTION
		 *	 Determines if the card on the reader is a NFC Forum compliant tag
		 *	 It first checks the ATR to determine if the card can be type 2 or
		 * 	 a type 4 and tries to recognize the content of the tag.
		 * 	 It creates either a NfcTagType2 or a NfcTagType4 and returns true if the tag is recognized.
		 * 	 It returns false if the card is not a NFC Forum tag
		 *
		 * SEE ALSO
		 *   NfcTagType2.RecognizeAtr
		 *   NfcTagType2.Recognize
		 * 	 NfcTagType2.Create
		 *   NfcTagType4.Recognize
		 * 	 NfcTagType4.Create
		 * 
		 *
		 **/

		public static bool RecognizeAndRead(SCardChannel cardchannel, out NfcTag tag, T5T_Mode t5t_mode)
		{
			RecognizeOptions options = new RecognizeOptions();
			options.T5T_mode = t5t_mode;
			options.Read = true;
			options.RecognizeDesfireEv1 = false;
			if (!Recognize(cardchannel, options, out tag, out RecognizeResult result))
			{
				return false;
			}
			return true;
		}

		public static bool RecognizeAndRead(SCardChannel cardchannel, out NfcTag tag)
		{
			RecognizeOptions options = new RecognizeOptions();
			options.T5T_mode = T5T_Mode.Broadcast;
			options.Read = true;
			options.RecognizeDesfireEv1 = false;
			if (!Recognize(cardchannel, options, out tag, out RecognizeResult result))
			{
				return false;
			}
			return true;
		}

		public static bool Recognize(SCardChannel cardchannel, out NfcTag tag, T5T_Mode t5t_mode)
		{
			RecognizeOptions options = new RecognizeOptions();
			options.T5T_mode = t5t_mode;
			options.Read = false;
			options.RecognizeDesfireEv1 = false;
			if (!Recognize(cardchannel, options, out tag, out RecognizeResult result))
			{
				return false;
			}
			return true;
		}

		public static bool Recognize(SCardChannel cardchannel, out NfcTag tag)
		{
			RecognizeOptions options = new RecognizeOptions();
			options.T5T_mode = T5T_Mode.Broadcast;
			options.Read = false;
			options.RecognizeDesfireEv1 = false;
			if (!Recognize(cardchannel, options, out tag, out RecognizeResult result))
			{
				return false;
			}
			return true;
		}

		public class RecognizeOptions
        {
			public bool RecognizeDesfireEv1 = false;
			public bool Read = false;
			public T5T_Mode T5T_mode = T5T_Mode.Broadcast;
		}

		public class RecognizeResult
		{
			public enum ErrorReasonE
            {
				Unknown,
				UnsupportedATR,
				InvalidContent
            }

			public ErrorReasonE ErrorReason = ErrorReasonE.Unknown;
			public byte Type = 0;
			public bool IsDesfireEv1 = false;
		}

		/* this default version allows to perform the NFC Forum preambule sequence only! */
		public static bool Recognize(SCardChannel cardchannel, RecognizeOptions options, out NfcTag tag, out RecognizeResult result)
		{
			result = new RecognizeResult();

			if (NfcTagType1.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC Forum type 1 Tag");
				result.Type = 1;
				tag = NfcTagType1.Create(cardchannel, options.Read);
				if (tag == null)
	            {
					Logger.Trace("Based on its content, this card is not a NFC Forum type 1 Tag, sorry");
					result.ErrorReason = RecognizeResult.ErrorReasonE.InvalidContent;
				}
				return true;
			}
			
			if (NfcTagType2.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC Forum type 2 Tag");
				result.Type = 2;
				tag = NfcTagType2.Create(cardchannel, options.Read);
				if (tag == null)
				{
					Logger.Trace("Based on its content, this card is not a NFC Forum type 2 Tag, sorry");
					result.ErrorReason = RecognizeResult.ErrorReasonE.InvalidContent;
				}
				return true;
			}
			
			if (NfcTagType3.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC Forum type 3 Tag");
				result.Type = 3;
				tag = NfcTagType3.Create(cardchannel, options.Read);
				if (tag == null)
				{
					Logger.Trace("Based on its content, this card is not a NFC Forum type 3 Tag, sorry");
					result.ErrorReason = RecognizeResult.ErrorReasonE.InvalidContent;
				}
				return true;
			}
			
			if (NfcTagType5.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC type 5 Tag");
				result.Type = 5;
				tag = NfcTagType5.Create(cardchannel, options.Read, options.T5T_mode);
				if (tag == null)
				{
					Logger.Trace("Based on its content, this card is not a NFC Forum type 5 Tag, sorry");
					result.ErrorReason = RecognizeResult.ErrorReasonE.InvalidContent;
				}
				return true;
			}

			/* Eventually look if that's a type 4 */
			/* we can't recognize the card based on its ATR, we must try to use the card! */
			Logger.Trace("This card is -maybe- a NFC type 4 Tag");
			tag = NfcTagType4.Create(cardchannel, options.Read);
			if (tag != null)
            {
				Logger.Trace("This is a NFC Forum type 4 Tag");
				result.Type = 4;
				return true;
			}

			result.ErrorReason = RecognizeResult.ErrorReasonE.UnsupportedATR;
			if (options.RecognizeDesfireEv1)
            {
				Logger.Trace("Is the card a Desfire EV1 card?");
				if (NfcTagType4.IsDesfireEV1(cardchannel))
                {
					Logger.Trace("The card is a Desfire EV1 card");
					result.Type = 4;
					result.IsDesfireEv1 = true;
                }
				else
                {
					Logger.Trace("The card is not a Desfire card?");
				}
            }

			return false;
		}

		/*
		 * Try to identify the Tag type by its ATR!
		 * Only suitable for tag 1,2,3,5 !
		 * */
		public static byte IdentifyByAtr(SCardChannel cardchannel)
		{
			byte res = 0;
			
			if (NfcTagType1.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC type 1 Tag");
				res = 1;
			}
			else
			if (NfcTagType2.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC type 2 Tag");
				res = 2;
			}
			else
			if (NfcTagType3.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC type 3 Tag");
				res = 3;
			}
			else
			if (NfcTagType5.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC type 5 Tag");
				res = 5;
			}
			else
			{
				Logger.Trace("Unable to define the tag type based on its ATR!");				
			}
			return res;
		}

		private byte[] SerializeContent()
		{
			if ((Content == null) || (Content.Count == 0))
			{
				Logger.Trace("Nothing to serialize");
				return null;
			}

            byte[] result = new byte[0];
			
			for (int i=0; i<Content.Count; i++)
			{
				if (Content[i].useTriche)
                {
					Logger.Trace("Workaround to pass COMPRION test : ndef is 12345");
					result = BinUtils.Concat(result, NdefObject.refArrayTriche);
				}
                else
                {
					Content[i].SetMessageBegin((i == 0));
					Content[i].SetMessageEnd((i == (Content.Count - 1)));
					result = BinUtils.Concat(result, Content[i].Serialize());
                }                
			}
			
			return result;
		}

		
		/**m* SpringCard.NfcForum.Tags/NfcTag.IsEmpty
		 *
		 * SYNOPSIS
		 *   public bool IsEmpty()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag doesn't contain any data.
		 *
		 * SEE ALSO
		 *   NfcTag.Write
		 *
		 **/
		public bool IsEmpty()
		{
			return _is_empty;
		}

		
		/**m* SpringCard.NfcForum.Tags/NfcTag.IsFormatted
		 *
		 * SYNOPSIS
		 *   public bool IsFormatted()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag is ready to store a NDEF content.
		 *
		 * SEE ALSO
		 *   NfcTag.IsFormattable
		 *   NfcTag.Format
		 *
		 **/
		public bool IsFormatted()
		{
			return _formatted;
		}

		
		/**m* SpringCard.NfcForum.Tags/NfcTag.IsFormattable
		 *
		 * SYNOPSIS
		 *   public bool IsFormattable()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag be formatted to store a NDEF content.
		 *
		 * SEE ALSO
		 *   NfcTag.IsFormatted
		 *   NfcTag.Format
		 *
		 **/
		public bool IsFormattable()
		{
			return (!_locked && !_formatted && _formattable);
		}

		
		/**m* SpringCard.NfcForum.Tags/NfcTag.IsLocked
		 *
		 * SYNOPSIS
		 *   public bool IsLocked()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag is locked in read-only state
		 *
		 * SEE ALSO
		 *   NfcTag.IsLockable
		 *   NfcTag.Lock
		 *
		 **/
		public bool IsLocked()
		{
			return _locked;
		}

		/**m* SpringCard.NfcForum.Tags/NfcTag.IsLockable
		 *
		 * SYNOPSIS
		 *   public bool IsLockable()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag could be locked in read-only state.
		 *
		 * SEE ALSO
		 *   NfcTag.IsLocked
		 *   NfcTag.Lock
		 *
		 **/
		public bool IsLockable()
		{
			return (!_locked && _lockable);
		}
		
		/**m* SpringCard.NfcForum.Tags/NfcTag.Capacity
		 *
		 * SYNOPSIS
		 *   public long Capacity()
		 * 
		 * DESCRIPTION
		 *   Returns the size of the Tag's NDEF storage container.
		 * 
		 * SEE ALSO
		 *   NfcTag.ContentSize
		 *
		 **/
		public long Capacity()
		{
			return _capacity_from_cc;
		}

		/**m* SpringCard.NfcForum.Tags/NfcTag.PhysicalCapacity
		 *
		 * SYNOPSIS
		 *   public long PhysicalCapacity()
		 * 
		 * DESCRIPTION
		 *   Returns the the Tag's physical capacity.
		 * 
		 * SEE ALSO
		 *   NfcTag.ContentSize
		 *
		 **/
		public long PhysicalCapacity()
		{
			return _physical_capacity_from_cc;
		}

		/**m* SpringCard.NfcForum.Tags/NfcTag.ContentSize
		 *
		 * SYNOPSIS
		 *   public long ContentSize()
		 * 
		 * DESCRIPTION
		 *   Returns the actual size of the Tag's NDEF content.
		 * 
		 * SEE ALSO
		 *   NfcTag.Capacity
		 *
		 **/
		public long ContentSize()
		{
			byte[] bytes = SerializeContent();
			
			if ((bytes == null) || (bytes.Length == 0))
				return 0;
			
			return bytes.Length;
		}


		/**m* SpringCard.NfcForum.Tags/NfcTag.Format
		 *
		 * SYNOPSIS
		 *   public bool Format()
		 * 
		 * DESCRIPTION
		 *   Formats the physical Tag currently on the reader.
		 *   This is only possible if IsFormattable returns true.
		 *   Return true on success.
		 * 
		 * SEE ALSO
		 *   NfcTag.IsFormatted
		 *   NfcTag.IsFormatable
		 *   NfcTag.Write
		 *
		 **/
		public abstract bool Format();

		/**m* SpringCard.NfcForum.Tags/NfcTag.Write
		 *
		 * SYNOPSIS
		 *   public bool Write(bool skip_checks = false)
		 * 
		 * DESCRIPTION
		 *   Writes the new content of the NfcTag object to the physical
		 *   Tag currently on the reader.
		 * 
		 *   If parameter skip_checks is true, the function doesn't verify whether
		 *   the Tag is writable or not before trying to write.
		 *   Returns true on success.
		 * 
		 * NOTES
		 *   The Tag must already be formatted. See NfcTag.IsFormatted and NfcTag.Format.
		 *
		 **/
		public bool Write()
		{
			return Write(false);
		}

		public bool Write(bool skip_checks)
		{
			if (!IsFormatted() && !skip_checks)
			{
				Logger.Trace("The Tag is not formatted");
				return false;
			}
			
			if (IsLocked() && !skip_checks)
			{
				Logger.Trace("The Tag is not writable");
				return false;
			}

			byte[] bytes = SerializeContent();
			
			if ((bytes == null) || (bytes.Length == 0))
			{
				Logger.Trace("Nothing to write on the Tag");
				return false;
			}
			
			if (bytes.Length > Capacity())
			{
				Logger.Trace("The size of the content is bigger than the Tag's capacity. {0:D} / {1:D}", bytes.Length, Capacity());
				Logger.Trace(BinConvert.ToHex(bytes));
				return false;
			}
			
			Logger.Trace("Writing the Tag...");

			if (!WriteContent(bytes))
			{
				Logger.Trace("Write failed!");
				return false;
			}
			
			Logger.Trace("Write success!");
			return true;
		}
		
		/**m* SpringCard.NfcForum.Tags/NfcTag.Lock
		 *
		 * SYNOPSIS
		 *   public bool Lock()
		 * 
		 * DESCRIPTION
		 *   Sets physical Tag currently on the reader in read-only (locked) state.
		 *   This is only possible if IsLockable returns true.
		 *   Returns true on success.
		 *
		 **/
		public abstract bool Lock();

		protected abstract bool Read();
		
		public byte[] GetUid()
		{
			return GetUid(_channel);
		}
		
		protected static byte[] GetUid(ICardApduTransmitter channel)
		{
			CAPDU capdu = new CAPDU(0xFF, 0xCA, 0x00, 0x00, 0x00);

			Logger.Trace("< " + capdu.AsString(" "));
			
			RAPDU rapdu =  channel.Transmit(capdu);				
			if (rapdu == null)
			{
				Logger.Trace("Error while getting the card's UID");
				return null;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("Bad status word " + SCARD.CardStatusWordsToString(rapdu.SW) + " while getting the card's UID");
				return null;
			}
			
			if (!rapdu.hasData)
			{
				Logger.Trace("Empty response");
				return null;
			}
			
			return rapdu.data.GetBytes();
		}


		public bool ParseUserData(byte[] user_data, out byte[] ndef_data, ref List<NfcTlv> tlvs)
		{
			ndef_data = null;
			byte[] buffer = user_data;

			tlvs.Clear();

			while (buffer != null)
			{
				if (buffer == null)
				{
					Logger.Trace("We have reached the end of the buffer (no Terminator TLV found)!");
					break;
				}

				NfcTlv tlv = NfcTlv.Unserialize(buffer, out buffer);

				if (tlv == null)
				{
					Logger.Trace("An invalid content has been found (not a T,L,V)");
					Logger.Debug("\tBuffer={0}", BinConvert.ToHex(buffer));
					break;
				}

				if ((tlv.T != NULL_TLV) && (tlv.T != TERMINATOR_TLV))
				{
					Logger.Debug("Working on "+BinConvert.ToHex(tlv.T) + "," + BinConvert.ToHex((uint)tlv.L) + "," + BinConvert.ToHex(tlv.V));
				}
				switch (tlv.T)
				{
					case NDEF_MESSAGE_TLV:
						Logger.Trace("Found a NDEF TLV");
						if (ndef_data != null)
						{
							Logger.Trace("The Tag has already a NDEF, ignoring this one");
						}
						else
						{
							Logger.Debug("\tLength={0}", tlv.L);
							//Logger.Debug("\tData={0}", BinConvert.ToHex(tlv.V));
							ndef_data = tlv.V;
						}
						break;
					case LOCK_CONTROL_TLV:
						Logger.Trace("Found a LOCK CONTROL TLV");
						break;
					case MEMORY_CONTROL_TLV:
						Logger.Trace("Found a MEMORY CONTROL TLV");
						break;
					case PROPRIETARY_TLV:
						Logger.Trace("Found a PROPRIETARY TLV");
						break;
					case TERMINATOR_TLV:
						Logger.Trace("Found a TERMINATOR TLV");
						/* After a terminator... we terminate */
						buffer = null;
						break;
					case NULL_TLV:
						/* Null TLVs can be used for padding in the TLVs_Area.
						 * The TLVs_Area contains zero or more Null TLVs.
						 * When it parses the content of the TLVs_Area, the Reader/Writer
						 * SHALL ignore a Null TLV and jump over it. */
						Logger.Trace("Found a NULL TLV");
						break;
					default:
						Logger.Trace("Found an unsupported TLV (T=" + tlv.T + ")");
                        break;

				}

				if (tlv.T != NULL_TLV)
				{
					Logger.Debug("Adding it to the list of TLVs...");
					tlvs.Add(tlv);					
				}
			}
			
			Logger.Debug("Done");
			return true;
		}

		public static bool ParseUserData(byte[] user_data, out byte[] ndef_data)
		{
			ndef_data = null;
			byte[] buffer = user_data;

			while (buffer != null)
			{
				if (buffer == null)
				{
					Logger.Trace("We have reached the end of the buffer (no Terminator TLV found)!");
					break;
				}

				NfcTlv tlv = NfcTlv.Unserialize(buffer, out buffer);

				if ((tlv.T != NULL_TLV) && (tlv.T != TERMINATOR_TLV))
				{
					Logger.Debug("Working on " + BinConvert.ToHex(tlv.T) + "," + BinConvert.ToHex((uint)tlv.L) + "," + BinConvert.ToHex(tlv.V));
				}

				if (tlv == null)
				{
					Logger.Trace("An invalid content has been found (not a T,L,V)");
					break;
				}

				switch (tlv.T)
				{
					case NDEF_MESSAGE_TLV:
						Logger.Trace("Found a NDEF TLV");
						if (ndef_data != null)
						{
							Logger.Trace("The Tag has already a NDEF, ignoring this one");
						}
						else
						{
							ndef_data = tlv.V;
						}
						break;
					case LOCK_CONTROL_TLV:
						Logger.Trace("Found a LOCK CONTROL TLV");
						break;
					case MEMORY_CONTROL_TLV:
						Logger.Trace("Found a MEMORY CONTROL TLV");
						break;
					case PROPRIETARY_TLV:
						Logger.Trace("Found a PROPRIETARY TLV");
						break;
					case TERMINATOR_TLV:
						Logger.Trace("Found a TERMINATOR TLV");
						/* After a terminator... we terminate */
						buffer = null;
						break;
					case NULL_TLV:
						/* Null TLVs can be used for padding in the TLVs_Area.
						 * The TLVs_Area contains zero or more Null TLVs.
						 * When it parses the content of the TLVs_Area, the Reader/Writer
						 * SHALL ignore a Null TLV and jump over it. */
						Logger.Trace("Found a NULL TLV");
						break;
					default:
						Logger.Trace("Found an unsupported TLV (T=" + tlv.T + ")");
						break;
				}
			}

			return true;
		}


		/*
		 * Common read binary function
		 * */
		public static byte[] ReadBinary(ICardApduTransmitter channel, ushort address, byte length)
		{
			return ReadBinary(channel, address, length, out ushort sw);
		}

		public static byte[] ReadBinary(ICardApduTransmitter channel, ushort address, byte length, out ushort sw)
        {
			sw = 0x0000;

			CAPDU capdu = new CAPDU(0xFF, 0xB0, (byte)(address / 0x0100), (byte)(address % 0x0100), length);

			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = null;

			for (int retry = 0; retry < 4; retry++)
			{
				rapdu = channel.Transmit(capdu);

				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;

				Thread.Sleep(10);
			}

			if (rapdu == null)
			{
				Logger.Trace("Error while reading the card");
				return null;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			sw = rapdu.SW;
			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("Bad status word " + SCARD.CardStatusWordsToString(rapdu.SW) + " while reading the card");
				return null;
			}

			if (!rapdu.hasData)
			{
				Logger.Trace("Empty response");
				return null;
			}

			return rapdu.data.GetBytes();
		}


		/*
		* Common device functions
		* */
		public static void IUT14443Halt(ICardApduTransmitter channel)
		{
			/* detect if that's a Type 1 */
			CAPDU capdu = new CAPDU(0xFF, 0xFB, 0x12, 0x01);
			RAPDU rapdu = null;

			Logger.Trace("< " + capdu.AsString(" "));

			for (int retry = 0; retry < 4; retry++)
			{
				rapdu = channel.Transmit(capdu);

				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;

				Thread.Sleep(15);
			}

			if ((rapdu == null) || (rapdu.SW != 0x9000))
			{
				Logger.Trace("Error while sending HALT!");
			}
		}
	}

}
