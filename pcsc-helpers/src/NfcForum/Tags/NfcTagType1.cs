/**h* SpringCard.NfcForum.Tags/NfcTag
 *
 * NAME
 *   SpringCard API for NFC Forum :: NfcTagType1 class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using SpringCard.LibCs;
using SpringCard.NfcForum.Ndef;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SpringCard.NfcForum.Tags
{

    /**c* SpringCard.NfcForum.Tags/NfcTagType1
	 *
	 * NAME
	 *   NfcTagType1
	 * 
	 * DERIVED FROM
	 *   NfcTag
	 * 
	 * DESCRIPTION
	 *   Represents a Type 1 NFC Tag that has been discovered on a reader.
	 *
	 * SYNOPSIS
	 *   if (NfcTagType1.Recognize(channel))
	 *     NfcTag tag = NfcTagType1.Create(SCardChannel channel)
	 *
	 **/
    public class NfcTagType1 : NfcTag
    {        
        private const string ATR_BASE = "3B8F8001804F0CA00000030602";
        private const string ATR_TOPAZ_96 = "3B8F8001804F0CA000000306020030000000005A";
        private const string ATR_TOPAZ_512 = "3B8F8001804F0CA00000030602002F0000000045";
                                              
        private const byte OFFSET_TLV_AREA = 12;
        private const byte RALL_WITH_HR_LENGTH = 122;
        private const byte RSEG = 128;
        private const byte CC_OFFSET = 8;        

        private List<NfcTlv> _tlvs = new List<NfcTlv>();
        private List<Tuple<byte, ushort, ushort, byte[]>> _special_controls = new List<Tuple<byte, ushort, ushort, byte[]>>();

        private byte[] _hr = new byte[2];
        private byte[] _cc = new byte[4];
        private byte[] _first_or_next_seg = null;

        private ushort _first_ndef_message_offset = OFFSET_TLV_AREA;
        private bool _dynamic_memory_model = false;

        /**
         * Constructor
         * */
        public NfcTagType1(ICardApduTransmitter Channel) : base(Channel, 1)
        {
            /* A type 1 Tag can always be locked */
            _lockable = true;

            /* The T1TOP_Preamble precedes the scenario tables for all test cases. */
            _is_valid = T1TOP_Preamble(Channel, ref _formatted, ref _formattable, ref _locked);

            _version = 1;
        }


        /**
        * T1TOP_ReadHR – obtain HR from the device
        * */
        public byte[] T1TOP_ReadHR(ICardApduTransmitter channel)
        {
            CAPDU capdu = new CAPDU(0xFF, 0xCA, 0xF0, 0x00);

            Logger.Trace("< " + capdu.AsString(" "));

            RAPDU rapdu = channel.Transmit(capdu);
            if (rapdu == null)
            {
                Logger.Trace("Error while reading the card");
                return null;
            }

            Logger.Trace("> " + rapdu.AsString(" "));

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


        /**
        * T1TOP_Preamble – obtain common (read/write/lock) informations
        * */
        private bool T1TOP_Preamble(ICardApduTransmitter channel, ref bool formatted, ref bool formattable, ref bool write_protected)
        {
            byte[] data = null; ;

            /*  First of all, retrieve HR from the device */
            _hr = T1TOP_ReadHR(channel);
            if (_hr == null)
            {
                Logger.Trace("Failed to read Header Rom");
                return false;
            }

            if ((_hr[0] & 0x0F) == 0x01)
            {
                Logger.Trace("Type 1 Tag uses Static Memory Layout");

                /*  First of all, read the Header Rom + 120 Bytes (1st segment in dynamic memory model)*/
                data = ReadBinary(channel, 0, RALL_WITH_HR_LENGTH);
                if (data == null)
                {
                    Logger.Trace("Failed to read Header Rom + CC + data!");
                    return false;
                }

                /* +2 to bypass HR, which is part of the RALL answer */
                BinUtils.CopyTo(_cc, 0, data, CC_OFFSET+2, 4);
            }
            else
            {
                Logger.Trace("Type 1 Tag uses Dynamic Memory Layout");
                _dynamic_memory_model = true;

                data = ReadBinary(_channel, 0, RSEG);
                if (data == null)
                {
                    Logger.Trace("Failed to read CC + data!");
                    return false;
                }
                BinUtils.CopyTo(_cc, 0, data, CC_OFFSET, 4);
            }


            /* extract CC */            
            if ((_cc[0] == 0) && (_cc[1] == 0) && (_cc[2] == 0) && (_cc[3] == 0))
            {
                /* The OTP bits are blank, assume the card is an unformatted type 2 Tag */
                Logger.Trace("CC is blank");
                return false;
            }

            if (_cc[0] != NFC_FORUM_MAGIC_NUMBER)
            {
                Logger.Trace("NMN <>  NFC Forum magic number");
                return false;
            }

            /* look for access */
            formatted = true;
            formattable = false;
            write_protected = true;
            if ((_cc[3] & 0xF0) == 0)
            {
                Logger.Trace("Free read access");
            }
            else
            {
                Logger.Trace("No read access");
                return false;
            }
            if ((_cc[3] & 0x0F) == 0)
            {
                Logger.Trace("Free write access");
                write_protected = false;
            }
            else
            {
                Logger.Trace("No write access");
            }

            /* capacity (TMS: Tag Memory Size) is defined by the tag type */
            _physical_capacity_from_cc = (ushort)((_cc[2] + 1) * 8);
            Logger.Trace("Physical Capacity:" + _physical_capacity_from_cc.ToString());

            if ((_hr[0] & 0x0F) == 0x01)
            {
                /* Type 1 Tag uses Static Memory Layout */

                /* Block 0 is the UID 
                 * Block D is Lock/Reserved
                 * Block E is Lock/Reserved */
                _capacity_from_cc = _physical_capacity_from_cc - (8 * 3) - 4; /* -4 for CC */
                Logger.Trace("Logical Capacity:" + _capacity_from_cc.ToString());

                /* keep it (no need to re-read it) */
                _first_or_next_seg = new byte[120];
                BinUtils.CopyTo(_first_or_next_seg, 0, data, 2, _first_or_next_seg.Length);
                                
                if (!NfcTlv.FindFirstNDEFMessage(_first_or_next_seg, (ushort)(CC_OFFSET + 4), out _first_ndef_message_offset))
                {
                    Logger.Trace("The NDEF is invalid or unsupported");
                    return false;
                }
            }
            else
            { 
                /* Type 1 Tag uses Dynamic Memory Layout */
                _dynamic_memory_model = true;

                /* Block 0 is the UID 
                 * Block D is Lock/Reserved
                 * Block E is Lock/Reserved */
                _capacity_from_cc = _physical_capacity_from_cc - (8 * 3) - 4; /* -4 for CC */

                byte[] raw = new byte[128 - CC_OFFSET - 4];
                BinUtils.CopyTo(raw, 0, data, CC_OFFSET + 4, raw.Length);

                NfcTlv.LookForSpecialControls(raw, ref _special_controls);                

                Logger.Trace("{0:d} Lock/Memory control{1:s} found", _special_controls.Count, (_special_controls.Count > 0) ? "s" : "");                

                /* look for Lock/Memory control TLVs */
                if (_special_controls.Count > 0)
                {
                    foreach (Tuple<byte, ushort, ushort, byte[]> anExclusion in _special_controls)
                    {
                        Logger.Trace("Settings: {0:d}, {1:d}, {2:d}", anExclusion.Item1, anExclusion.Item2, anExclusion.Item3);
                        Logger.Trace("Value   : " + BinConvert.tohex(anExclusion.Item4));
#if false
                        if (anExclusion.Item1 == 0)
                        {
                            /* this is a Lock control */
                            _lock_settings.Set(anExclusion.Item4);
                        }
#endif
                        /* this part is not usable for the NDEF message */
                        _capacity_from_cc -= anExclusion.Item3;
                    }
                }


                Logger.Trace("Logical Capacity:" + _capacity_from_cc.ToString());

                /* keep it (no need to re-read it) */
                _first_or_next_seg = new byte[128];
                BinUtils.CopyTo(_first_or_next_seg, 0, data, 0, _first_or_next_seg.Length);

                if (!NfcTlv.FindFirstNDEFMessage(_first_or_next_seg, (ushort)(CC_OFFSET + 4 + _special_controls.Count * 5), out _first_ndef_message_offset))
                {
                    Logger.Trace("The NDEF is invalid or unsupported");
                    return false;
                }

                /* we also need to exclude Blocks 0D and 0E */
                _special_controls.Add(new Tuple<byte, ushort, ushort, byte[]>(1, 0x0D*8, 16, null));
            }

            return true;
        }
             

        protected override bool WriteContent(byte[] ndef_content)
        {
            Logger.Trace("Writing the NFC Forum type 1 Tag (length={0:d})", ndef_content.Length);
            ushort mustWrite;
            ushort ndefMessageSize;

            Logger.Debug("_first_or_next_seg={0}", BinConvert.ToHex(_first_or_next_seg));

            if (ndef_content == null)
                return false;

            /* we may use WRITE-E or WRITE-E8 commands, it implies that we re-use the current tag
             * informations */

            CardBuffer actual_content = new CardBuffer();
            byte[] beforeNdefMessage = new byte[_first_ndef_message_offset];
            BinUtils.CopyTo(beforeNdefMessage, 0, _first_or_next_seg, _first_ndef_message_offset);

            byte[] ndef_header = null;

            /* create ndef header */
            ndefMessageSize = (ushort)ndef_content.Length;
            if (ndef_content[ndefMessageSize - 1] == TERMINATOR_TLV)
            {
                ndefMessageSize--;
            }

            if (ndefMessageSize > 254)
            {
                ndef_header = new byte[4];
                ndef_header[1] = 0xFF;
                ndef_header[2] = (byte)(ndefMessageSize / 0x0100);
                ndef_header[3] = (byte)(ndefMessageSize % 0x0100);
            } else
            {
                ndef_header = new byte[2];
                ndef_header[1] = (byte)(ndefMessageSize % 0x0100);
            }
            ndef_header[0] = NDEF_MESSAGE_TLV;

            actual_content.Append(beforeNdefMessage);
            actual_content.Append(ndef_header);
#if false
            if (!_dynamic_memory_model)
            {
                /* we can simply add our ndef message here */
                actual_content.Append(ndef_content);
            } else
            {
                /* we must avoid protected areas while adding our ndef message */
                ushort to_copy = ndefMessageSize;
                ushort src_index = (ushort)(_first_ndef_message_offset + ndef_header.Length);
                ushort dst_index = 0;
                while (to_copy != 0)
                {
                    /* copy one byte */
                    actual_content.AppendOne(ndef_content[dst_index++]);                    
                    to_copy--;
                    src_index++;

                    if ((to_copy!=0) && (_special_controls.Count != 0) && (src_index>=104))
                    {
                        /* check if the next page will be in the exclusion part */
                        bool bypassDone = false;

                        while (!bypassDone)
                        {
                            bypassDone = true;
                            foreach (Tuple<byte, ushort, ushort, byte[]> anExclusion in _special_controls)
                            {
                                ushort start = anExclusion.Item2;
                                ushort end = (ushort)(anExclusion.Item2 + anExclusion.Item3);

                                if ((src_index >= start) && (src_index < end))
                                {
                                    Logger.Trace("byte {0:d} is in an exclusion area ({1:d} bytes to exclude at offset {2:d})", src_index, anExclusion.Item3, anExclusion.Item2);
                                    byte[] bytesFromProtectedArea = new byte[anExclusion.Item3];
                                    BinUtils.CopyTo(bytesFromProtectedArea, 0, _first_or_next_seg, anExclusion.Item2, bytesFromProtectedArea.Length);
                                    actual_content.Append(bytesFromProtectedArea);
                                    Logger.Debug("must overwrite {0}", BinConvert.ToHex(bytesFromProtectedArea));
                                    src_index += anExclusion.Item3;
                                    bypassDone = false;
                                }
                            }
                        }
                    }
                }
            }
#endif
            /* we can simply add our ndef message here */
            actual_content.Append(ndef_content);

            Logger.Debug("actual_content={0}", BinConvert.ToHex(actual_content.GetBytes()));

            mustWrite = (ushort)actual_content.Length;

            if ((ndef_header.Length + ndef_content.Length) > Capacity())
            {
                Logger.Trace("The size of the content (with its TLVs) is bigger than the tag's capacity");
                return false;
            }

            /* shall we write an Terminator? */
            if ((ndef_header.Length + ndef_content.Length) == Capacity())
            {
                Logger.Trace("The content fullfills tag's capacity");
            }
            else if (actual_content[actual_content.Length - 1] != TERMINATOR_TLV)
            {
                actual_content.AppendOne(TERMINATOR_TLV);
                mustWrite++;
                Logger.Trace("We must add a TERMINATOR TLV at the end of the Tag");
            }

            /* And now write */
            ushort address = _first_ndef_message_offset;

            /* we are going to use WRITE-E8 command, we must be 8 bytes aligned */
            if ( _dynamic_memory_model )
            {
                /* we must start at the base address (write the actual value) */
                address = 0; // (ushort)(address / 8 * 8);
            } else
            {
                /* we must start by the length! */
                address++;
            }
            Logger.Trace("Starting to write at address {0:d}", address);
            ushort offset = address;
            mustWrite -= offset;

            while (mustWrite > 0)
            {
                if (_dynamic_memory_model)
                {
                    /* we can write 8 bytes at once */
                    byte[] buffer = new byte[8];
                    byte canWrite = 8;

                    if (mustWrite < 8)
                    {
                        canWrite = (byte)mustWrite;
                    }

                    BinUtils.CopyTo(buffer, 0, actual_content.Bytes, offset, canWrite);

                    Logger.Debug("WRITE-E8: {0}", BinConvert.ToHex(buffer));

                    if (!WriteBinary8Bytes(_channel, address, buffer))
                        return false;

                    offset += canWrite;
                    address += 8;
                    mustWrite -= canWrite;


                    if ((mustWrite != 0) && (_special_controls.Count != 0))
                    {
                        /* check if the next page will be in the exclusion part */
                        bool bypassDone = false;

                        while (!bypassDone)
                        {
                            bypassDone = true;
                            foreach (Tuple<byte, ushort, ushort, byte[]> anExclusion in _special_controls)
                            {
                                ushort start = anExclusion.Item2;
                                ushort end = (ushort)(anExclusion.Item2 + anExclusion.Item3);

                                if ((address >= start) && (address < end))
                                {
                                    Logger.Trace("address {0:d} is in an exclusion area ({1:d} bytes to exclude at offset {2:d})", address, anExclusion.Item3, anExclusion.Item2);
                                    Logger.Trace("Bypassing {0:d} bytes", anExclusion.Item3);
                                    address += anExclusion.Item3;
                                    bypassDone = false;
                                }
                            }
                        }
                    }
                } else
                {
                    /* we can only write per byte */
                    byte[] buffer = new byte[1];
                    BinUtils.CopyTo(buffer, 0, actual_content.Bytes, offset, 1);
                    if (!WriteBinaryByte(_channel, address, buffer[0]))
                        return false;
                    offset ++;
                    address ++;
                    mustWrite --;
                }               
            }

            return true;
        }

        public override bool Format()
        {
            Logger.Trace("Formatting the NFC Forum type 1 Tag");

            byte[] cc_block = new byte[4];
            byte[] user_data = new byte[4];

            long capacity;

            capacity = Capacity();
            capacity /= 8;
            if (capacity > 255)
                capacity = 255;

            cc_block[0] = NFC_FORUM_MAGIC_NUMBER;
            cc_block[1] = NFC_FORUM_VERSION_NUMBER;
            cc_block[2] = (byte)(capacity);
            cc_block[3] = 0x00;

            if (!WriteBinary(_channel, 8, cc_block))
            {
                Logger.Trace("Can't write the CC bytes");
                return false;
            }


            user_data[0] = 0x00; // Erase first bytes
            user_data[1] = 0x00; // in order to avoid finding false
            user_data[2] = 0x00; // TLVs
            user_data[3] = 0x00;

            if (!WriteBinary(_channel, 12, user_data))
            {
                Logger.Trace("Can't write the 1st page of user data");
                return false;
            }


            /* The Tag is now formatted */
            _formatted = true;
            /* So it's not formattable anymore */
            _formattable = false;
            /* We consider it is empty */
            _is_empty = true;

            return true;
        }

        public override bool Lock()
        {
            Logger.Trace("Locking the NFC Forum type 1 Tag");

            /* No write access at all (static memory model) */
            if (!WriteBinaryByte(_channel, 11, 0x0F))
                return false;
            if (!WriteBinaryByte(_channel, 112, 0xFF))
                return false;
            if (!WriteBinaryByte(_channel, 113, 0xFF))
                return false;

            /* OK! */
            _locked = true;
            _lockable = false;

            return true;
        }


        /* WriteBinaryByte is only usable on the first segment! */
        private bool WriteBinaryByte(ICardApduTransmitter channel, ushort address, byte onebyte)
        {
            byte[] one = new byte[1];
            one[0] = onebyte;

            CAPDU capdu = new CAPDU(0xFF, 0xD6, (byte)(0x00), (byte)(address & 0xFF), one);

            Logger.Trace("< " + capdu.AsString(" "));
            RAPDU rapdu = channel.Transmit(capdu);
            if (rapdu == null)
            {
                Logger.Trace("Error while writing the card");
                return false;
            }

            Logger.Trace("> " + rapdu.AsString(" "));

            if (rapdu.SW != 0x9000)
            {
                Logger.Trace("Bad status word " + SCARD.CardStatusWordsToString(rapdu.SW) + " while writing the card");
                return false;
            }

            return true;
        }

        /* WriteBinaryByte is only usable on the first segment! */
        private bool WriteBinary8Bytes(ICardApduTransmitter channel, ushort address, byte[] eightbytes)
        {
            CAPDU capdu = new CAPDU(0xFF, 0xD6, (byte)(address / 0x0100), (byte)(address % 0x0100), eightbytes);

            Logger.Trace("< " + capdu.AsString(" "));
            RAPDU rapdu = channel.Transmit(capdu);
            if (rapdu == null)
            {
                Logger.Trace("Error while writing the card");
                return false;
            }

            Logger.Trace("> " + rapdu.AsString(" "));

            if (rapdu.SW != 0x9000)
            {
                Logger.Trace("Bad status word " + SCARD.CardStatusWordsToString(rapdu.SW) + " while writing the card");
                return false;
            }

            return true;
        }

        private bool WriteBinary(ICardApduTransmitter channel, ushort address, byte[] data)
        {
            int length = data.Length;
            int offset = 0;

            if (data == null)
                return false;

            if (length == 0)
                return false;

            if (length > 8)
            {
                Logger.Trace("type 1 Tag: Write Binary accepts a maximum of 8 bytes");
                return false;
            }

            /* we can only write byte per byte in Static memory model */
            if ( !_dynamic_memory_model )
            {                
                while (length > 0)
                {
                    if (!WriteBinaryByte(channel, address, data[offset]))
                    {
                        return false;
                    }

                    /* go to next byte */
                    offset++;
                    length--;
                    address++;
                }
            }
            else
            {                
                if ( ( (address % 8) != 0 ) || length != 8 )
                {
                    /* we shall use the write byte method */
                    while (length > 0)
                    { 
                        if (!WriteBinaryByte(channel, address, data[offset]))
                        {
                            return false;
                        }

                        /* go to next byte */
                        offset++;
                        length--;
                        address++;
                    }
                } else
                {
                    /* we can write 8 bytes at once! */
                    if (!WriteBinary8Bytes(channel, address, data))
                    {
                        return false;
                    }
                }                   
            }

            return true;
        }

        protected override bool Read()
        {
            Logger.Trace("Reading the NFC Forum type 1 Tag");            
            ushort ndef_announced_size = 0;

            CardBuffer buffer = new CardBuffer();

            ndef_announced_size = (ushort)NfcTlv.FindFirstNDEFLength(_first_or_next_seg, _first_ndef_message_offset);

            if (ndef_announced_size == 0)
            {
                Logger.Trace("NDEF message is empty");
                return true;
            }

            /* make place for length */
            if (ndef_announced_size > 254 )
            {
                ndef_announced_size += 4;
            } else
            {
                ndef_announced_size += 2;
            }

            byte[] raw_data = new byte[ndef_announced_size];
            if (_dynamic_memory_model)
            {
                ushort to_copy = ndef_announced_size;
                ushort segment = 0;

                /* working on a Dynamic Memory Layout */
                Logger.Trace("The NDEF is incomplete, we only have the first part...");

                /* start offset */
                ushort src_index = _first_ndef_message_offset;
                ushort dst_index = 0;

                /* work on the first segment */
                while ( to_copy > 0 )
                {
                    if (src_index == _first_or_next_seg.Length)
                    {
                        segment++;
                        Logger.Trace("Reading segment {0:d}", segment);                        
                        _first_or_next_seg = ReadBinary(_channel, (ushort)(segment * 128), RSEG);
                        if (_first_or_next_seg == null)
                        {
                            Logger.Trace("Failed to read new segment!");
                            return false;
                        }
                        src_index = 0;
                    }

                    /* copy one byte */
                    raw_data[dst_index++] = _first_or_next_seg[src_index++];
                    to_copy--;

                    if (to_copy == 0)
                    {
                        Logger.Trace("End of the T1T area reached!");
                    } else
                    if (_special_controls.Count != 0)
                    {
                        /* check if the next page will be in the exclusion part */
                        bool bypassDone = false;

                        while (!bypassDone)
                        {
                            bypassDone = true;
                            foreach (Tuple<byte, ushort, ushort, byte[]> anExclusion in _special_controls)
                            {
                                ushort start = anExclusion.Item2;
                                ushort end = (ushort)(anExclusion.Item2 + anExclusion.Item3);

                                if (( (src_index+(segment*128)) >= start) && ((src_index + (segment * 128)) < end))
                                {
                                    Logger.Trace("byte {0:d} is in an exclusion area ({1:d} bytes to exclude at offset {2:d})", (src_index + (segment * 128)), anExclusion.Item3, anExclusion.Item2);
                                    src_index += anExclusion.Item3;
                                    bypassDone = false;
                                }
                            }
                        }
                    }
                }
            } else
            {
                /* it could be a dynamic memory mapping with a small ndef message or a static memory mapping */
                Logger.Trace("The NDEF is complete, we have {0:d} bytes", ndef_announced_size);
                BinUtils.CopyTo(raw_data, 0, _first_or_next_seg, _first_ndef_message_offset, ndef_announced_size);
            }

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

            if (!ParseUserData(raw_data, out byte[] ndef_data, ref _tlvs))
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

            _is_empty = false;

            NdefObject[] t = NdefObject.Deserialize(ndef_data);
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


        /**m* SpringCard.NfcForum.Tags/NfcTagType1.RecognizeAtr
		 *
		 * SYNOPSIS
		 *   public static bool RecognizeAtr(CardBuffer atr)
		 * 	 public static bool RecognizeAtr(SCardChannel channel)
		 * 
		 * 
		 * DESCRIPTION
		 *   Checks wether the ATR of the card corresponds to the ATR
		 * 	 of a Mifare Ultralight or a Mifare Ultralight C card.
		 *   Returns true on success.
		 *
		 **/
        public static bool RecognizeAtr(CardBuffer atr)
        {
            string s = atr.AsString("");

            if (s.Equals(ATR_TOPAZ_96))
            {
                Logger.Trace("ATR: Topaze 96");
                return true;
            }
            if (s.Equals(ATR_TOPAZ_512))
            {
                Logger.Trace("ATR: Topaze 512");
                return true;
            }
            if (s.StartsWith(ATR_BASE))
            {
                Logger.Trace("ATR: ISO/IEC 14443-3 type A");
                return true;
            }

            return false;
        }

        public static bool RecognizeAtr(SCardChannel channel)
        {
            CardBuffer atr = channel.CardAtr;

            return RecognizeAtr(atr);
        }


        /**m* SpringCard.NfcForum.Tags/NfcTagType1.Create
		 *
		 * NAME
		 *   NfcTagType1.Create
		 * 
		 * SYNOPSIS
		 *   public static NfcTagType1 Create(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Instanciates a new NfcTagType1 object for this card
		 * 
		 * SEE ALSO
		 *   NfcTagType1.Recognize
		 *
		 **/
        public static NfcTagType1 Create(SCardChannel channel)
        {
            return Create(channel, true);
        }
        public static NfcTagType1 Create(SCardChannel channel, bool read)
        {
            NfcTagType1 t = new NfcTagType1(channel);
            if (!t._is_valid) return null;
            if (read && !t.Read()) return null;
            return t;
        }


        /**f* SpringCard.NfcForum.Tags/NfcTagType1.Recognize
         *
         * NAME
         *   NfcTagType1.Recognize
         * 
         * SYNOPSIS
         *   public static bool Recognize(SCardChannel channel)
         *
         * DESCRIPTION
         *   Returns true if the card on the reader is a NFC Forum type 1 Tag
         *
         * SEE ALSO
         *   NfcTagType1.Create
         * 
         **/
        public static bool Recognize(ICardApduTransmitter channel)
        {
            NfcTagType1 t = new NfcTagType1(channel);
            return t._is_valid;
        }

    }
}
