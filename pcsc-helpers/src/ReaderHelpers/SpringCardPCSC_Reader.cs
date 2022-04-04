using System;
using System.Drawing;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;
using System.Security.Cryptography;

namespace SpringCard.PCSC.ReaderHelpers
{
    public class SpringCardReader
    {
        protected SCardChannel channel;

        public Dictionary<string, string> Data { get; protected set; } = new Dictionary<string, string>();
        public bool IsSpringCore { get; protected set; }
        public bool IsSpringProx { get; protected set; }

        public SpringCardReader(SCardChannel channel)
        {
            this.channel = channel;
        }

        public string GetTextData(byte index)
        {
            byte[] data = channel.Control(new byte[] { 0x58, 0x20, index });
            if ((data == null) || (data.Length < 1) || (data[0] != 0x00))
                return null;
            return StrUtils.ToStr_UTF8(data, 1);
        }

        public string GetSlotName()
        {
            byte[] data = channel.Control(new byte[] { 0x58, 0x21 });
            if ((data == null) || (data.Length < 1) || (data[0] != 0x00))
                return null;
            return StrUtils.ToStr_UTF8(data, 1);
        }

        public bool ReadData()
        {
            string t;

            IsSpringCore = false;
            IsSpringProx = false;
            Data.Clear();

            t = GetTextData(0x01);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Vendor name", t);

            t = GetTextData(0x02);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Product name", t);

            t = GetSlotName();
            if (!string.IsNullOrEmpty(t))
                Data.Add("Slot name", t);

            t = GetTextData(0x03);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Serial number", t);

            t = GetTextData(0x04);
            if (!string.IsNullOrEmpty(t))
                Data.Add("USB ID", t);

            t = GetTextData(0x05);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Version", t);

            t = GetTextData(0x06);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Revision", t);

            t = GetTextData(0x07);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Firmware", t);

            t = GetTextData(0x08);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Build user", t);

            t = GetTextData(0x09);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Build date", t);

            t = GetTextData(0x0A);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Ethernet address", t);

            t = GetTextData(0x0B);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Bluetooth address", t);

            t = GetTextData(0x0C);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Bluetooth device name", t);

            t = GetTextData(0x0D);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Unique ID", t);

            t = GetTextData(0x0E);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Config ID", t);

            t = GetTextData(0x0F);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Hardware", t);

            t = GetTextData(0x10);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Contactless", t);

            t = GetTextData(0x11);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Smartcard", t);

            t = GetTextData(0x41);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Orig. Vendor name", t);

            t = GetTextData(0x42);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Orig. Product name", t);

            t = GetTextData(0x4A);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Orig. Ethernet address", t);

            t = GetTextData(0x4B);
            if (!string.IsNullOrEmpty(t))
                Data.Add("Orig. Bluetooth address", t);

            if (Data.Count == 0)
                return false;

            return true;
        }
    }
}
