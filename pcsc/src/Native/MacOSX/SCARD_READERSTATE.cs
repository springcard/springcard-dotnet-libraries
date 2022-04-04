using System;
using System.Runtime.InteropServices;

namespace SpringCard.PCSC.Native.MacOsX
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct SCARD_READERSTATE
    {
        internal string szReader;
        internal IntPtr pvUserData;
        internal uint dwCurrentState;
        internal uint dwEventState;
        internal uint cbAtr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PCSCliteMacOsX.MAX_ATR_SIZE)]
        internal byte[] rgbAtr;
    }
}
