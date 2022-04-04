using System;
using System.Drawing;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;
using System.Security.Cryptography;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Text;

namespace SpringCard.PCSC.ReaderHelpers
{
    public class DriverProtect
    {
        /**
         *
         * \brief Name of mutex to protect operation on PC/SC driver (Windows only)
         *
         **/
        public const string MutexName = @"Global\SpringCardPcscDriverOperation";

        /**
         *
         * \brief Default timeout when waiting for mutex to protect operation on PC/SC driver (Windows only)
         *
         **/
        public const int MutexTimeout = 30000;

    }
}
