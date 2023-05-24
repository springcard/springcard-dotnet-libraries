using System;
using System.Drawing;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;
using System.Security.Cryptography;

namespace SpringCard.PCSC.ReaderHelpers
{
    public partial class SpringProx : SpringCardReader
    {
        public SpringProx(SCardChannel channel) : base(channel)
        {
            IsSpringProx = true;
        }
    }
}
