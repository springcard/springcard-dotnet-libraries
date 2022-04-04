using SpringCard.LibCs;
using SpringCard.NfcForum.Ndef;
using SpringCard.PCSC;
using System;
/**h* SpringCard.NfcForum.Tags/NfcTag
*
* NAME
*   SpringCard API for NFC Forum :: NfcTagType2 class
* 
* COPYRIGHT
*   Copyright (c) SpringCard SAS, 2012
*   See LICENSE.TXT for information
*
**/
using System.Collections.Generic;
using System.Threading;


namespace SpringCard.NfcForum.Tags
{

    public class NfcType2DynamicLock
    {
        private ushort _Position = 0;
        private ushort _NumberOfBytesLockedPerBit = 0;
        private ushort _NumberOfDynamicLockBits = 0;
        private ushort _NumberOfDynamicLockBytes = 0;
        private byte[] _LockBits = null;

        public ushort Position { get => _Position; }
        public ushort NumberOfBytesLockedPerBit { get => _NumberOfBytesLockedPerBit; }
        public ushort NumberOfDynamicLockBits { get => _NumberOfDynamicLockBits; }
        public ushort NumberOfDynamicLockBytes { get => _NumberOfDynamicLockBytes; }

        public byte[] LockMap
        {
            get => _LockBits;
            set
            {
                if ((value != null) && (value.Length == _LockBits.Length) )
                {
                    BinUtils.CopyTo(_LockBits, value);
                }
            }
        }              

        internal long Set(byte[] toCheck)
        {
            /* unpack and calculate */
            byte NbrMajorOffset = (byte)((toCheck[0] >> 4) & 0x0F);
            byte NbrMinorOffset = (byte)(toCheck[0] & 0x0F);

            /* Number of Dynamic Lock bits */
            if (toCheck[1] == 0x00)
            {
                _NumberOfDynamicLockBits = 256;
            }
            else
            {
                _NumberOfDynamicLockBits = toCheck[1];
            }

            /* The number of Dynamic Lock bytes is then:
             * NumberOfDynamicLockBytes = ?NumberOfDynamicLockBits / 8?
             * If the division result is not an integer, the number of lock bytes is equal to
             * the closest integer that is larger than the division result. */
            _NumberOfDynamicLockBytes = (ushort)(_NumberOfDynamicLockBits / 8);
            if ((_NumberOfDynamicLockBits % 8) != 0)
            {
                _NumberOfDynamicLockBytes++;
            }

            byte BLPLB_Index = (byte)((toCheck[2] >> 4) & 0x0F);
            if ((BLPLB_Index < 2) || (BLPLB_Index > 10))
            {
                Logger.Error("BLPLB_Index is invalid!");
                return 0;
            }

            byte MOS_DLA = (byte)(toCheck[2] & 0x0F);
            if ((MOS_DLA < 2))
            {
                Logger.Error("MOS_DLA is invalid!");
                return 0;
            }

            /* Number of bytes locked per bit */
            _NumberOfBytesLockedPerBit = (ushort)(1 << BLPLB_Index);

            /* location of those Lock bits */
            _Position = (ushort)((NbrMajorOffset * (ushort)(1 << MOS_DLA)) + NbrMinorOffset);

            /* create buffer for lock bits! */
            _LockBits = new byte[_NumberOfDynamicLockBytes];

            Logger.Trace("NumberOfDynamicLockBits: {0:d}", _NumberOfDynamicLockBits);
            Logger.Trace("NumberOfDynamicLockBytes: {0:d}", _NumberOfDynamicLockBytes);
            Logger.Trace("Position: {0:d}", _Position);
            Logger.Trace("LockBits array size: {0:d}", _LockBits.Length);
                        
            return _Position;
        }

        internal long Set(long capacity_from_cc)
        {
            /* The default settings of the Dynamic Lock bits are:
             * Position: the first Dynamic Lock byte is the first byte after the T2T_Area
             * Number of bytes locked per bit: 8 */
            _NumberOfBytesLockedPerBit = 8;

            /* Number of Dynamic Lock bits: NumberOfDynamicLockBits = ?(T2T_Area_Size-48) / 8)? */
            /* Minus 48 because the first 48 bytes lock settings are in the Static Lock bits */
            _NumberOfDynamicLockBits = (ushort)((capacity_from_cc - 48) / _NumberOfBytesLockedPerBit);

            /* The number of Dynamic Lock bytes is then:
             * NumberOfDynamicLockBytes = ?NumberOfDynamicLockBits / 8?
             * If the division result is not an integer, the number of lock bytes is equal to
             * the closest integer that is larger than the division result. */
            _NumberOfDynamicLockBytes = (ushort)(_NumberOfDynamicLockBits / 8);
            if ((_NumberOfDynamicLockBits % 8) != 0)
            {
                _NumberOfDynamicLockBytes++;
            }

            /* location of those Lock bits */
            _Position = (ushort)((ushort)capacity_from_cc + 16);

            /* create buffer for lock bits! */
            _LockBits = new byte[_NumberOfDynamicLockBytes];

            Logger.Trace("NumberOfDynamicLockBits: {0:d}", _NumberOfDynamicLockBits);
            Logger.Trace("NumberOfDynamicLockBytes: {0:d}", _NumberOfDynamicLockBytes);
            Logger.Trace("Position: {0:d}", _Position);
            Logger.Trace("LockBits array size: {0:d}", _LockBits.Length);

            return capacity_from_cc;
        }

        internal bool Lock()
        {
            int bitCounter = 0, byteCounter = 0;
            byte filler = 0;

            /* create lock state */
            for (int cpt = 0; cpt< _NumberOfDynamicLockBits; cpt++)
            {
                filler <<= 1;
                filler |= 0x01;

                bitCounter++;
                if ((bitCounter == 8)||(cpt==(_NumberOfDynamicLockBits-1)))
                {
                    bitCounter = 0;
                    _LockBits[byteCounter] = filler;
                    byteCounter++;
                    filler = 0x00;
                }
            }

            return true;
        }
    }

    /**c* SpringCard.NfcForum.Tags/NfcTagType2
     *
     * NAME
     *   NfcTagType2
     * 
     * DERIVED FROM
     *   NfcTag
     * 
     * DESCRIPTION
     *   Represents a Type 2 NFC Tag that has been discovered on a reader.
     *
     * SYNOPSIS
     *   if (NfcTagType2.Recognize(channel))
     *     NfcTag tag = NfcTagType2.Create(SCardChannel channel)
     *
     **/
    public class NfcTagType2 : NfcTag
    {
        /* well known ATRs */
        private const string ATR_BASE = "3B8F8001804F0CA00000030603";
        private const string ATR_MIFARE_UL = "3B8F8001804F0CA0000003060300030000000068";
        private const string ATR_MIFARE_UL_C = "3B8F8001804F0CA00000030603003A0000000051";
        private const string ATR_MIFARE_UL_EV1 = "3B8F8001804F0CA00000030603003D0000000051";

        private const byte PAGE_SIZE = 4;                   /* in bytes */
        private const byte READ_4_PAGES = PAGE_SIZE * 4;
        private const byte READ_1_PAGE = PAGE_SIZE;
        private const byte READ_2_PAGES = PAGE_SIZE * 2;

        private const byte LOCK_PAGE_INDEX = 0x02;
        private const byte CC_PAGE_INDEX = 0x03;
        private const byte NDEF_TLV_PAGE_INDEX = 0x04;

        private List<NfcTlv> _tlvs = new List<NfcTlv>();
        
        private List<Tuple<byte, ushort, ushort, byte[]>> _special_controls = new List<Tuple<byte, ushort, ushort, byte[]>>();                
        private byte[] _cc = new byte[4];
        private bool _static_memory_model = true;
        private ushort _last_page = 0;
        private ushort _first_ndef_message_offset = 0;
        private NfcType2DynamicLock _lock_settings = new NfcType2DynamicLock();


        /**
         * Constructor
         * */
        public NfcTagType2(ICardApduTransmitter Channel) : base(Channel, 2)
        {
            CardBuffer buffer = new CardBuffer();
            byte[] data = null;
            ushort page = NDEF_TLV_PAGE_INDEX;

            /* A type 2 Tag can always be locked */
            _lockable = true;

            /* The T2TOP_Preamble precedes the scenario tables for all test cases. */
            _is_valid = T2TOP_Preamble(Channel, ref _formatted, ref _formattable, ref _locked, ref _cc, ref _capacity_from_cc);

            if (!_is_valid)
                return;

            /* this is the usable area for our NDEF Message */
            _usable_tlv_area_size = _capacity_from_cc;

            if (_capacity_from_cc > 48)
            {
                Logger.Trace("Type 2 Tag uses Dynamic Memory Layout");

                /* use the default settings, we may find a Lock control TLV later */
                _lock_settings.Set(_capacity_from_cc);
                _static_memory_model = false;

                /* look for exclusion areas */
                Logger.Trace("Looking for excluded areas..");

                /* Look for special controls */
                data = ReadBinary(_channel, page, READ_4_PAGES);
                if (data == null)
                {
                    Logger.Error("Error while looking for Lock/Memory control TLVs!");
                }
                buffer.Append(data);
                page += 4;
                while (!NfcTlv.LookForSpecialControls(buffer.GetBytes(), ref _special_controls))
                {
                    data = ReadBinary(_channel, page, READ_4_PAGES);
                    if (data == null)
                    {
                        Logger.Error("Error while looking for Lock/Memory control TLVs!");
                    }
                    buffer.Append(data);
                    page += 4;
                }

                Logger.Trace("{0:d} Lock/Memory control{1:s} found", _special_controls.Count, (_special_controls.Count > 0) ? "s" : "");

                /* look for Lock/Memory control TLVs */
                if (_special_controls.Count > 0)
                {
                    foreach (Tuple<byte, ushort, ushort, byte[]> anExclusion in _special_controls)
                    {
                        if (anExclusion.Item1 == 0)
                        {
                            /* this is a Lock control */
                            _lock_settings.Set(anExclusion.Item4);
                        }
                        /* this part is not usable for the NDEF message */
                        _usable_tlv_area_size -= (anExclusion.Item3 + 3) / 4 * 4;
                    }
                }

                /* load current lock bits */
                _lock_settings.LockMap = T2TOP_ReadLockBits(Channel, _lock_settings.Position, _lock_settings.NumberOfDynamicLockBytes);
                Logger.Trace("Lock bits area: " + BinConvert.tohex(_lock_settings.LockMap));

            }
            else
            {
                _static_memory_model = true;
                Logger.Trace("Type 2 Tag uses Static Memory Layout");
            }

            Logger.Trace("Usable TLV area is {0:d} bytes long", _usable_tlv_area_size);
            _last_page = (ushort)((ushort)(_capacity_from_cc / 4) + NDEF_TLV_PAGE_INDEX);
            Logger.Trace("Last page is {0:X4}", _last_page);

            
            /* look for the first NDEF message */
            if ( buffer.Length == 0)
            {
                page = NDEF_TLV_PAGE_INDEX;
                data = ReadBinary(_channel, page, READ_4_PAGES);
                if (data == null)
                {
                    Logger.Error("Error while looking for NDEF Message TLV!");
                }
                buffer.Append(data);
                page += 4;
            }

            /* start of NDEF message shall be in the first 8 pages */
            for (int cpt = 0; cpt < 2; cpt++)
            {
                bool found = false;
                NfcTlv.FindFirstNDEFMessage(buffer.GetBytes(), 0, out _first_ndef_message_offset,out found);

                if (found)
                {
                    Logger.Trace("NDEF message found at offset {0:X4}", _first_ndef_message_offset);
                    break;
                }

                /* not found, have a look at next pages */
                data = ReadBinary(_channel, page, READ_4_PAGES);
                if (data == null)
                {
                    Logger.Error("Error while looking for NDEF Message TLV!");
                }
                buffer.Append(data);
                page += 4;
            }

            _version = 2;
        }


        /**
        * T2TOP_Preamble – obtain common (read/write/lock) informations
        * */
        private bool T2TOP_Preamble(ICardApduTransmitter channel, ref bool formatted, ref bool formattable, ref bool write_protected, ref byte[] cc, ref long capacity)
        {
            byte[] cc_and_more = ReadBinary(channel, 0, READ_4_PAGES);
            
            if (cc_and_more == null)
            {
                Logger.Trace("Unable to read CC!");
                return false;
            }

            /* only copy the cc part */
            BinUtils.CopyTo(cc, 0, cc_and_more, 12, 4);

            if ((cc[0] == 0) && (cc[1] == 0) && (cc[2] == 0) && (cc[3] == 0))
            {
                /* The OTP bits are blank, assume the card is an unformatted type 2 Tag */
                Logger.Trace("OTP are blank");
                formatted = false;
                formattable = true;
                write_protected = false;
                return true;
            }

            if (cc[0] != NFC_FORUM_MAGIC_NUMBER)
            {
                /* The OTP bits contain something else */
                Logger.Trace("OTP are not blank");
                formatted = false;
                formattable = false;
                write_protected = true;
                return false;
            }

            /* The OTP bits contain the NFC NDEF MAGIC NUMBER, so this is a formatted type 2 Tag */
            Logger.Trace("OTP = NFC Forum magic number");
            formatted = true;
            formattable = false;
            write_protected = true;

            if ((cc[1] & 0xF0) != (NFC_FORUM_VERSION_NUMBER & 0xF0))
            {
                Logger.Trace("Version mismatch in OTP");
                return false;
            }

            /* set capacity based on the CC_2 field */
            capacity = cc[2] * 8;
            Logger.Trace("Capacity from CC: {0:d}", capacity);

            if ((cc[3] & 0xF0) == 0)
            {
                Logger.Trace("Free read access");
            }
            else
            {
                Logger.Trace("No read access");
                return false;
            }
            if ((cc[3] & 0x0F) == 0)
            {
                Logger.Trace("Free write access");
                write_protected = false;
            }
            else
            {
                Logger.Trace("No write access");
            }

            return true;
        }


        /**
         * Read Dynamic lock bits
         * */
        private byte[] T2TOP_ReadLockBits(ICardApduTransmitter channel, ushort position, ushort numberOfDynamicLockBytes)
        {
            ushort page = (ushort)(position / 4);
            byte length = (byte)((numberOfDynamicLockBytes + 3) / 4 * 4);
            byte[] data = ReadBinary(channel, page, length);
            if (data != null)
            {
                byte[] temp = new byte[numberOfDynamicLockBytes];
                BinUtils.CopyTo(temp, data, numberOfDynamicLockBytes);
                return temp;
            }
            return null;
        }

        /**
         * Write Dynamic lock bits
         * */
        private bool T2TOP_WriteLockBits( ushort position, byte[] lockBytes)
        {
            ushort page = GetPageFromAddress((ushort)position, out ushort alignement);
            ushort mustWrite;
            ushort index = 0;

            if (lockBytes == null)
            {
                return false;
            }

            mustWrite = (ushort)lockBytes.Length;
            if (mustWrite == 0)
                return false;

            ushort canWrite;
            /* write on an unaligned address? */
            if (alignement != 0)
            {
                canWrite = (ushort)(PAGE_SIZE - alignement);
                byte[] buffer = new byte[canWrite];
                BinUtils.CopyTo(buffer, 0, lockBytes, 0, canWrite);

                /* SECTOR_SELECT is done byt the IUT! */
                if (!WriteBinaryPartial(_channel, page, buffer, alignement))
                    return false;

                /* update index and size */
                mustWrite = (ushort)(mustWrite - canWrite);
                index += canWrite;
                page++;
            }

            while (mustWrite > 0)
            {
                canWrite = PAGE_SIZE;
                if (mustWrite < PAGE_SIZE)
                {
                    canWrite = mustWrite;
                }

                byte[] buffer = new byte[canWrite];
                BinUtils.CopyTo(buffer, 0, lockBytes, index, canWrite);

                /* SECTOR_SELECT is done byt the IUT! */
                if (canWrite != PAGE_SIZE)
                {
                    if (!WriteBinaryPartial(_channel, page, buffer, 0))
                        return false;
                }
                else
                {
                    if (!WriteBinary(_channel, page, buffer))
                        return false;
                }

                mustWrite -= canWrite;
                index += canWrite;
                page++;
            }

            return true;
        }


        /**
         * Address/Page convertion helpers
         * address is NOT CC relative!
         * */
        private ushort GetPageFromAddress(ushort address, out ushort offset)
        {
            offset = (ushort)(address % 4);
            return (ushort)((ushort)(address / 4));
        }

        protected override bool WriteContent(byte[] ndef_content)
        {            
            ushort mustWrite;
            ushort canWrite;
            ushort rawSize;
            ushort index = 0;
            byte[] header = null;

            Logger.Trace("Writing the NFC Forum type 2 Tag (length={0:d})", ndef_content.Length);

            if (ndef_content == null)
                return false;

            CardBuffer actual_content = new CardBuffer();
            rawSize = (ushort)ndef_content.Length;
            if (ndef_content.Length > 254)
            {
                header = new byte[4];
                header[0] = NDEF_MESSAGE_TLV;
                header[1] = 0x00;
            } else
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
            if (actual_content[actual_content.Length-1] != TERMINATOR_TLV)
            {                
                actual_content.AppendOne(TERMINATOR_TLV);
                mustWrite++;
                Logger.Trace("We must add a TERMINATOR TLV at the end of the Tag");
            }

            ushort page = (ushort)(NDEF_TLV_PAGE_INDEX + GetPageFromAddress((ushort)(_first_ndef_message_offset), out ushort alignement));
            ushort page_start = page;

            while ((mustWrite > 0) && (page < _last_page))
            {
                canWrite = PAGE_SIZE;

                if (mustWrite < PAGE_SIZE)
                {
                    canWrite = mustWrite;
                }

                byte[] buffer = new byte[PAGE_SIZE];
                BinUtils.CopyTo(buffer, 0, actual_content.Bytes, index, canWrite);
                if (!WriteBinary(_channel, page, buffer))
                    return false;

                mustWrite -= canWrite;
                index += canWrite;

                /* no need to go further */
                if (mustWrite == 0)
                {
                    /* we will use canWrite as an offset to add a TLV */
                    if (canWrite == PAGE_SIZE)
                    {
                        /* we will write on a new page! */
                        canWrite = 0;
                    } else
                    {
                        /* we are on the page! */
                        break;
                    }                    
                }

                /* go to next page! */
                page++;

                if (_special_controls.Count != 0)
                {
                    /* check if the next page will be in the exclusion part */
                    bool bypassDone = false;

                    while (!bypassDone)
                    {
                        bypassDone = true;
                        foreach (Tuple<byte, ushort, ushort, byte[]> anExclusion in _special_controls)
                        {
                            ushort offset = (ushort)(page * 4);
                            ushort start = anExclusion.Item2;
                            ushort end = (ushort)(anExclusion.Item2 + anExclusion.Item3);

                            if ((offset >= start) && (offset < end))
                            {
                                ushort bypassPages = (ushort)((anExclusion.Item3 + 3) / 4);
                                Logger.Trace("page {0:d} is in an exclusion area ({1:d} bytes to exclude at offset {2:d})", page, anExclusion.Item3, anExclusion.Item2);
                                Logger.Trace("Bypassing {0:d} page{1:s}.", bypassPages, (bypassPages > 0) ? "s" : "");
                                page += bypassPages;
                                bypassDone = false;
                            }
                        }
                    }
                }

            }

            /* eventually write the final header with Length field! */
            Logger.Trace("Updating length!");
            byte[] newlength = new byte[PAGE_SIZE];
            BinUtils.CopyTo(newlength, 0, actual_content.Bytes, 0, PAGE_SIZE);
            if (rawSize > 254)
            {
                newlength[1] = 0xFF;
                newlength[2] = (byte)(rawSize / 0x0100);
                newlength[3] = (byte)(rawSize % 0x0100);                
            } else
            {                
                newlength[1] = (byte)rawSize;
            }

            /* eventually write the final header with Length field! */
            if (!WriteBinary(_channel, page_start, newlength))
                return false;

            return true;
        }

        public override bool Format()
        {
            Logger.Trace("Formatting the NFC Forum type 2 Tag");

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

            if (!WriteBinary(_channel, 3, cc_block))
            {
                Logger.Trace("Can't write the CC bytes");
                return false;
            }

            user_data[0] = 0x00; // Erase first bytes
            user_data[1] = 0x00; // in order to avoid finding false
            user_data[2] = 0x00; // TLVs
            user_data[3] = 0x00;

            if (!WriteBinary(_channel, 4, user_data))
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
            Logger.Trace("Locking the NFC Forum type 2 Tag");

            Logger.Trace("Writing Static Lock area");

            byte[] cc_block = ReadBinary(_channel, CC_PAGE_INDEX, 4);
            if ((cc_block == null) || (cc_block.Length != 4))
                return false;

            /* No write access at all */
            cc_block[3] = 0x0F;

            if (!WriteBinary(_channel, CC_PAGE_INDEX, cc_block))
                return false;

            /* Write the LOCKs */
            byte[] lock_block = ReadBinary(_channel, LOCK_PAGE_INDEX, 4);
            if ((lock_block == null) || (lock_block.Length != 4))
                return false;

            /* No write access at all */
            lock_block[2] = 0xFF;
            lock_block[3] = 0xFF;

            if (!WriteBinary(_channel, LOCK_PAGE_INDEX, lock_block))
                return false;

            /* shall we update DynLock_Area? */
            if ( !_static_memory_model )
            {
                
                _lock_settings.Lock();                
                Logger.Trace("Lock bits area: " + BinConvert.tohex(_lock_settings.LockMap));

                Logger.Trace("Writing Dynamic Lock area");
                if (!T2TOP_WriteLockBits(_lock_settings.Position, _lock_settings.LockMap))
                {
                    Logger.Error("Can't write Dynamic Lock area");
                    return false;
                }
            }

            /* OK! */
            _locked = true;
            _lockable = false;

            return true;
        }

      

        protected override bool Read()
        {
            Logger.Trace("Reading the NFC Forum type 2 Tag");
            ushort page = NDEF_TLV_PAGE_INDEX;

            CardBuffer buffer = new CardBuffer();

            /* Try to discover an NDEF entry */
            ushort missing = 0;
            bool finished = false;
            byte read_length;                

            while (!finished)
            {
                /* SECTOR_SELECT is done byt the IUT! */

                /* We may allow to read per 16 bytes */
                read_length = findReadCapability(page);
                if (read_length == 0)
                {
                    break;
                }

                byte[] data = ReadBinary(_channel, page, read_length);
                if (data == null)
                {
                    Logger.Trace("Unable to read!");
                    return false;
                }

                /* append the new chunk to the final buffer */
                buffer.Append(data);
                finished = NfcTlv.CheckIfComplete(buffer.GetBytes(), out missing);

                if (!finished)
                {
                    /* look at next page(s) */
                    page = (ushort)(page + (read_length/PAGE_SIZE));

                    if (page > _last_page)
                    {
                        Logger.Trace("End of the T2T area reached!");
                        finished = true;
                        missing = 0;
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
                                ushort offset = (ushort)(page * 4);
                                ushort start = anExclusion.Item2;
                                ushort end = (ushort)(anExclusion.Item2 + anExclusion.Item3);

                                if ((offset >= start) && (offset < end))
                                {
                                    ushort bypassPages = (ushort)((anExclusion.Item3 + 3) / 4);
                                    Logger.Trace("page {0:d} is in an exclusion area ({1:d} bytes to exclude at offset {2:d})", page, anExclusion.Item3, anExclusion.Item2);
                                    Logger.Trace("Bypassing {0:d} page{1:s}.", bypassPages, (bypassPages > 0) ? "s" : "");
                                    page += bypassPages;
                                    bypassDone = false;
                                }
                            }
                        }
                    }
                }
            }

            if (missing != 0)                
            {
                Logger.Trace("Uncomplete TLV!");
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

        private byte findReadCapability(ushort page)
        {
            ushort lowestOffset = 0xFFFF;
            ushort offset = (ushort)(page * 4);

            if (page >= _last_page)
            {
                return 0;
            }

            /* find the next page */
            foreach (Tuple<byte, ushort, ushort, byte[]> anExclusion in _special_controls)
            {
                ushort start = anExclusion.Item2;
                if ((offset < anExclusion.Item2) && (anExclusion.Item2 < lowestOffset))
                {
                    lowestOffset = anExclusion.Item2;
                }
            }

            ushort canRead;
            if (lowestOffset != 0xFFFF)
            {
                canRead = (ushort)(((lowestOffset - offset) / PAGE_SIZE) * PAGE_SIZE);
            }
            else
            {
                /* there is no (more) exclusion */
                canRead = (ushort)((_last_page - page) * PAGE_SIZE);
            }


            if (canRead > READ_4_PAGES)
                canRead = READ_4_PAGES;

            return (byte)canRead;
        }

        /**
         * WriteBinaryPartial is responsible to deal with misaligned data packets
         * */
        private bool WriteBinaryPartial(ICardApduTransmitter channel, ushort page, byte[] data, ushort offset)
        {            
            if (data == null)
                return false;

            if (data.Length > PAGE_SIZE)
                return false;

            if (offset > 3)
                return false;

            /* read previous value */
            byte[] temp = ReadBinary(channel, page, READ_1_PAGE);
            if (temp == null)
                return false;

            Logger.Trace("Previous page content was " + BinConvert.tohex(temp));

            /* overwrite data (partial or full) */
            BinUtils.CopyTo(temp, offset, data, 0, data.Length);
                        
            /* do the final write */
            if (!WriteBinary(channel, page, temp))
                return false;

            Logger.Trace("New page content is  " + BinConvert.tohex(temp));

            return true;
        }

        private bool WriteBinary(ICardApduTransmitter channel, ushort page, byte[] data)
        {
            if (data == null)
                return false;

            if (data.Length != 4)
            {
                Logger.Trace("Type 2 Tag: Write Binary accepts only 4B");
                return false;
            }

            CAPDU capdu = new CAPDU(0xFF, 0xD6, (byte)(page / 0x0100), (byte)(page % 0x0100), data);

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

        /**m* SpringCard.NfcForum.Tags/NfcTagType2.RecognizeAtr
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
            if (s.Equals(ATR_MIFARE_UL))
            {
                Logger.Trace("ATR: Mifare UltraLight");
                return true;
            }
            if (s.Equals(ATR_MIFARE_UL_C))
            {
                Logger.Trace("ATR: Mifare UltraLight C");
                return true;
            }
            if (s.Equals(ATR_MIFARE_UL_EV1))
            {
                Logger.Trace("ATR: Mifare UltraLight EV1");
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


        /**m* SpringCard.NfcForum.Tags/NfcTagType2.Create
		 *
		 * NAME
		 *   NfcTagType2.Create
		 * 
		 * SYNOPSIS
		 *   public static NfcTagType2 Create(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Instanciates a new NfcTagType2 object for this card
		 * 
		 * SEE ALSO
		 *   NfcTagType2.Recognize
		 *
		 **/
        public static NfcTagType2 Create(SCardChannel channel)
        {            
            return Create(channel, true);
        }

        public static NfcTagType2 Create(SCardChannel channel, bool read)
        {
            NfcTagType2 t = new NfcTagType2(channel);
            if (!t._is_valid) return null;
            if (read && !t.Read()) return null;            
            return t;
        }


        /**f* SpringCard.NfcForum.Tags/NfcTagType2.Recognize
        *
        * NAME
        *   NfcTagType1.Recognize
        * 
        * SYNOPSIS
        *   public static bool Recognize(SCardChannel channel)
        *
        * DESCRIPTION
        *   Returns true if the card on the reader is a NFC Forum type 2 Tag
        *
        * SEE ALSO
        *   NfcTagType2.Create
        * 
        **/
        public static bool Recognize(ICardApduTransmitter channel)
        {
            NfcTagType2 t = new NfcTagType2(channel);
            return t._is_valid;
        }
    }
}
