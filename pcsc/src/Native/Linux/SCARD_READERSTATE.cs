using System;
using System.Runtime.InteropServices;

namespace SpringCard.PCSC.Native.Linux
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SCARD_READERSTATE
    {
        internal string szReader;
        internal IntPtr pvUserData;
        internal IntPtr dwCurrentState;
        internal IntPtr dwEventState;
        internal IntPtr cbAtr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PCSCliteLinux.MAX_ATR_SIZE)]
        internal byte[] rgbAtr;
    }
}
