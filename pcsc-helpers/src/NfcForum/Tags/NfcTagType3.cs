/**h* SpringCard.NfcForum.Tags/NfcTag
 *
 * NAME
 *   SpringCard API for NFC Forum :: NfcTagType3 class
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

    /**c* SpringCard.NfcForum.Tags/NfcTagType3
	 *
	 * NAME
	 *   NfcTagType3
	 * 
	 * DERIVED FROM
	 *   NfcTag
	 * 
	 * DESCRIPTION
	 *   Represents a Type 3 NFC Tag that has been discovered on a reader.
	 *
	 * SYNOPSIS
	 *   if (NfcTagType3.Recognize(channel))
	 *     NfcTag tag = NfcTagType3.Create(SCardChannel channel)
	 *
	 **/
    public class NfcTagType3 : NfcTag
    {
        private const string ATR_RC_S966 = "3B8F8001804F0CA00000030611003B0000000042";
                                              
        private const byte FELICA_BLOCK_LENGTH = 16;

        private List<NfcTlv> _tlvs = new List<NfcTlv>();


        /* Byte 1 SHALL be Nbr. Nbr indicates the number of blocks that can be read 
         * using one Check command. The NFC Forum device SHALL not change the value
         * of this field. */
        private byte _nbr;

        /* Byte 2 SHALL be Nbw.Nbw indicates the number of blocks that can be written
         * using one Update command.The NFC Forum device SHALL not change the value
         * of this field. */
        private byte _nbw;

        /* Byte 3 and Byte 4 SHALL be Nmaxb.Nmaxb indicates the maximum number of
         * Blocks available for NDEF data.Byte 3 SHALL be the upper byte, Byte 4
         * SHALL be the lower byte of the maximum number of Blocks.The NFC Forum
         * device SHALL not change the value of this field. */
        private ushort _nmaxb;

        /* Byte 9 SHALL be WriteFlag. Allowed values for the WriteFlag SHALL be:
         * - 00h: OFF (Writing data finished)
         * - 0Fh: ON (Writing data in progress)
         * NFC Forum Devices SHOULD set this flag to ON before writing to the Type 3
         * Tag and set it back to OFF after writing. */
        private byte _writef;

        /* Byte 10 SHALL be RWflag. Allowed values for the RWFlag SHALL be:
         * - 00h: Access Attribute: Read only.
         * - 01h: Access Attribute: Read/Write available.
         * An NFC Forum Device SHALL not try to write to a Type 3 Tag with 
         * this flag set to 00h, even if writing would be technically possible. 
         * Read-only Type 3 Tags always have this value set to 00h.
         *  An NFC Forum Device SHALL not change the value of the RWflag. */
        private byte _rwflag;

        /* Byte 11 to Byte 13 SHALL be Ln.Ln is the actual size of the stored NDEF data 
         * in bytes.Byte 11 SHALL be the upper byte, Byte 12 SHALL be middle byte and 
         * Byte 13 SHALL be the lower byte. The number of blocks containing NDEF data(Nbc) 
         * can be calculated by the following formula. Nbc=abs(Ln/16)
         * An NFC Forum Device SHALL update the Ln field with a correct value each
         * time NDEF data has been written. */
        private ulong _ln;

        /* Byte 14 and Byte 15 SHALL be a checksum calculated using the following formula:
         *  Checksum = Byte 0 + Byte 1 + .. + Byte 13
         *  Byte 14 SHALL be the upper byte of the checksum, Byte 15 SHALL be the lower 
         *  byte of the checksum.The NFC Forum device SHALL update the checksum with a 
         *  correct value every time any of the values of Bytes 0 to 13 are changed. */

        /* Attribute Information Block */
        private byte[] _aib = new byte[FELICA_BLOCK_LENGTH];


        /**
        * Constructor
        * */
        public NfcTagType3(ICardApduTransmitter Channel) : base(Channel, 3)
        {
            /* A Type 3 Tag can always be locked */
            _lockable = true;

            /* The T3TOP_Preamble precedes the scenario tables for all test cases. */
            _is_valid = T3TOP_Preamble(Channel, ref _formatted, ref _formattable, ref _locked, ref _aib, ref _nbr, ref _nbw, ref _nmaxb, ref _writef, ref _rwflag, ref _ln, ref _physical_capacity_from_cc, ref _capacity_from_cc);

            _version = 3;
        }


        /**
        * T3TOP_Preamble – obtain common (read/write/lock) informations
        * */
        private bool T3TOP_Preamble(ICardApduTransmitter channel, ref bool formatted, ref bool formattable, ref bool write_protected, ref byte[] aib, ref byte nbr, ref byte nbw, ref ushort nmaxb, ref byte writef,
            ref byte rwflag, ref ulong ln, ref long physical_capacity_from_cc, ref long capacity_from_cc)
        {
            ushort checksum;

            /*  First of all, read the first 16 bytes (Attribute Information Block)! */
            aib = ReadBinary(channel, 0, FELICA_BLOCK_LENGTH);

            if (aib == null)
            {
                Logger.Trace("Failed to read first block");
                return false;
            }

            formatted = false;
            formattable = true;
            write_protected = false;

            /* we don't know yet how to find those informations */
            if ((aib[0]&0xF0) > NFC_FORUM_VERSION_NUMBER)
            {
                Logger.Trace("Version mismatch!");
                return false;
            }

            nbr = aib[1];
            nbw = aib[2];
            nmaxb = (ushort)(aib[3] << 8 | aib[4]);
            writef = aib[9];
            rwflag = aib[10];
            ln = (ulong)(aib[11] << 16 | aib[12] << 8 | aib[13]);
            checksum = (ushort)(aib[14] << 8 | aib[15]);

            /* verify header checksum */
            if (CreateChecksum(aib) != checksum)
            {
                Logger.Trace("Wrong checksum!");
                return false;
            }

            formatted = true;
            formattable = false;
            write_protected = (rwflag == 0x00);

            /* get physical and logical size */
            physical_capacity_from_cc = (nmaxb + 1) * FELICA_BLOCK_LENGTH;
            capacity_from_cc = nmaxb * FELICA_BLOCK_LENGTH;
            Logger.Trace("Logical capacity:" + capacity_from_cc.ToString());
            Logger.Trace("Physical capacity:" + physical_capacity_from_cc.ToString());

            return true;

        }


        /*
        * Create header checksum
        * */
        private ushort CreateChecksum(byte[] data)
        {
            ushort checksum_verify = 0;
            if (data.Length < 14) return 0;

            for (int i = 0; i < 14; i++)
            {
                checksum_verify += data[i];
            }
            return checksum_verify;
        }

        /*
        * Update Attribute Information Block
        * */
        private bool UpdateAIB()
        {            
            /* update from unpacked values */
            _aib[1] = _nbr;
            _aib[2] = _nbw;
            _aib[3] = (byte)(_nmaxb / 0x0100); 
            _aib[4] = (byte)(_nmaxb % 0x0100); 
            _aib[9] = _writef;
            _aib[10] = _rwflag;
            _aib[11] = (byte)((_ln >> 16) & 0xFF);
            _aib[12] = (byte)((_ln >> 8) & 0xFF);
            _aib[13] = (byte)((_ln >> 0) & 0xFF);
             
            /* update checksum */
            ushort checksum = 0;            
            for (int i = 0; i < 14; i++)
            {
                checksum += _aib[i];
            }
            _aib[14] = (byte)(checksum / 0x0100);
            _aib[15] = (byte)(checksum % 0x0100);

            Logger.Trace("UpdateAIB: " + BinConvert.tohex(_aib));

            return WriteBinary(_channel, 0, _aib);
        }

        protected override bool WriteContent(byte[] ndef_content)
        {
            Logger.Trace("Writing the NFC Forum Type 3 Tag (length={0:d})", ndef_content.Length);
            ushort mustWrite;
            ushort rawSize;
            ushort canWrite = 0;
            ushort page = 1;
            ushort index = 0;

            if (ndef_content == null)
                return false;

            CardBuffer actual_content = new CardBuffer();
            actual_content.Append(ndef_content);
            mustWrite = (ushort)actual_content.Length;

            /* this is the NDEF message size */
            rawSize = mustWrite;

            if (actual_content.Length > Capacity())
            {
                Logger.Trace("The size of the content (with its TLVs) is bigger than the tag's capacity");
                return false;
            }

            /* Inform we are going to write */
            Logger.Trace("Setting Write flag!");
            _writef = 0x0F;
            if (!UpdateAIB())
            {
                Logger.Trace("AIB update error!");
                return false;
            }

            while (mustWrite > 0)
            {
                canWrite = mustWrite;
                if (canWrite > FELICA_BLOCK_LENGTH)
                {
                    canWrite = FELICA_BLOCK_LENGTH;
                }

                byte[] buffer = new byte[FELICA_BLOCK_LENGTH];
                BinUtils.CopyTo(buffer, 0, actual_content.Bytes, index, canWrite);

                if (!WriteBinary(_channel, page, buffer))
                    return false;

                mustWrite -= canWrite;
                index += canWrite;

                /* go to next page! */
                page++;
            }


            /* Inform we write is done and update length */
            Logger.Trace("Clearing Write flag and set new length!");
            _writef = 0x00;
            _ln = rawSize;
            if (!UpdateAIB())
            {
                Logger.Trace("AIB update error!");
                return false;
            }

            return true;
        }

        public override bool Format()
        {
            Logger.Trace("Formatting the NFC Forum Type 3 Tag");
            byte[] aib = new byte[16];
            ushort nmaxb = _nmaxb != 0 ? _nmaxb : (byte)13;

            aib[0] = NFC_FORUM_VERSION_NUMBER;
            aib[1] = _nbr != 0 ? _nbr : (byte) 4;
            aib[2] = _nbw != 0 ? _nbw : (byte) 1;
            aib[3] = (byte)(nmaxb / 0x0100);
            aib[4] = (byte)(nmaxb % 0x0100);
            /* 5..8 are RFU */
            aib[9] = 0x00;  /* _writef */
            aib[10] = 0x01; /* _rwflag */
            aib[11] = 0;    /* Ln */
            aib[12] = 0;    /* Ln */
            aib[13] = 0;    /* Ln */

            ushort checksum = CreateChecksum(aib);
            aib[14] = (byte)(checksum / 0x0100);
            aib[15] = (byte)(checksum % 0x0100);

            if (!WriteBinary(_channel, 0, aib))
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
            Logger.Trace("Locking the NFC Forum Type 3 Tag");

            /* Inform we are going to write */
            Logger.Trace("Setting Write flag!");
            _rwflag = 0x00;
            if (!UpdateAIB())
            {
                Logger.Trace("AIB update error!");
                return false;
            }

            /* OK! */
            _locked = true;
            _lockable = false;

            return true;
        }


        private bool WriteBinary(ICardApduTransmitter channel, ushort address, byte[] data)
        {
            CAPDU capdu = new CAPDU(0xFF, 0xD6, (byte)(address / 0x0100), (byte)(address % 0x0100), data);

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


        protected override bool Read()
        {
            Logger.Trace("Reading the NFC Forum Type 3 Tag");
            ushort nbc;
            ushort bn = 1;

            if (_writef == 0x0F)
            {
                Logger.Trace("Write flag needs to be cleaned!");
                _is_valid = false;
                return false;
            }

            /* read content */
            CardBuffer buffer = new CardBuffer();
            nbc = (ushort)(( _ln + FELICA_BLOCK_LENGTH -1 ) / FELICA_BLOCK_LENGTH);

            Logger.Trace("nbc: {0}", nbc);
            Logger.Trace("_ln: {0}", _ln);

            /* multiple blocks allowed? */
            if (_nbr>1)
            {
                ushort nbbb = (ushort)(nbc / _nbr);
                Logger.Trace("nbbb: {0}", nbbb);
                for (int cpt = 0; cpt < nbbb; cpt++)
                {                    
                    byte[] ndata = ReadBinary(_channel, (ushort)bn, (byte)(FELICA_BLOCK_LENGTH * _nbr));
                    if (ndata == null)
                    {
                        /* can't read, get out! */
                        break;
                    }
                    buffer.Append(ndata);
                    bn += _nbr;
                    nbc -= _nbr;
                }                
            }

            Logger.Trace("nbc (remaining): {0}", nbc);
            for (int cpt = 0; cpt < nbc; cpt++)
            {

                byte[] ndata = ReadBinary(_channel, (ushort)bn, FELICA_BLOCK_LENGTH);
                if (ndata == null)
                {
                    /* can't read, get out! */
                    break;
                }
                buffer.Append(ndata);
                bn += 1;
            }
            
            Logger.Trace("Read " + buffer.Length + " bytes of data from the Tag");

            if (buffer.Length == 0)
                return false;

            /* Is the tag empty ? */
            byte[] _raw_data = buffer.GetBytes();
            _is_empty = true;
            for (long i = 0; i < _capacity_from_cc; i++)
            {
                if (_raw_data[i] != 0)
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

            /* we need to add the default NDEF_MESSAGE_TLV header */
            AddMissingTL(buffer.GetBytes((long)_ln), out byte[] complete_buffer);
            if (!ParseUserData(complete_buffer, out byte[] ndef_data, ref _tlvs))
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

        /*
         * Create header checksum
         * */
        private void AddMissingTL(byte[] src, out byte[] dst)
        {
            dst = new byte[src.Length + (src.Length > 254 ? 4 : 2)];

            /* Create the missing first TL structure (we only have V) */
            dst[0] = NDEF_MESSAGE_TLV;
            if (src.Length > 254)
            {
                dst[1] = 0xFF;
                dst[2] = (byte)(src.Length / 0x0100);
                dst[3] = (byte)(src.Length % 0x0100);
                Buffer.BlockCopy(src, 0, dst, 4, src.Length);
            }
            else
            {
                dst[1] = (byte)(src.Length);
                Buffer.BlockCopy(src, 0, dst, 2, src.Length);
            }
        }

       


        /**m* SpringCard.NfcForum.Tags/NfcTagType3.RecognizeAtr
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

            if (s.Equals(ATR_RC_S966))
            {
                Logger.Trace("ATR: Felica Lite-S RC-S966");
                return true;
            }

            return false;
        }
        public static bool RecognizeAtr(SCardChannel channel)
        {
            CardBuffer atr = channel.CardAtr;

            return RecognizeAtr(atr);
        }



        /**m* SpringCard.NfcForum.Tags/NfcTagType3.Create
		 *
		 * NAME
		 *   NfcTagType3.Create
		 * 
		 * SYNOPSIS
		 *   public static NfcTagType3 Create(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Instanciates a new NfcTagType3 object for this card
		 * 
		 * SEE ALSO
		 *   NfcTagType3.Recognize
		 *
		 **/

        public static NfcTagType3 Create(SCardChannel channel)
        {
            return Create(channel, true);
        }

        public static NfcTagType3 Create(SCardChannel channel, bool read)
        {
            NfcTagType3 t = new NfcTagType3(channel);
            if(!t._is_valid) return null;
            if (read && !t.Read()) return null;
            return t;
        }


        /**f* SpringCard.NfcForum.Tags/NfcTagType3.Recognize
		 *
		 * NAME
		 *   NfcTagType3.Recognize
		 * 
		 * SYNOPSIS
		 *   public static bool Recognize(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Returns true if the card on the reader is a NFC Forum Type 3 Tag
		 *
		 * SEE ALSO
		 *   NfcTagType3.Create
		 * 
		 **/        
        public static bool Recognize(ICardApduTransmitter channel)
        {
            NfcTagType3 t = new NfcTagType3(channel);
            return t._is_valid;
        }
    }
}
