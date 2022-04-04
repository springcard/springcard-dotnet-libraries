using System;
using System.Runtime.InteropServices;

namespace SpringCard.PCSC.Native.Windows
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct SCARD_READERSTATE
    {
        internal string szReader;

        internal IntPtr pvUserData;
        internal uint dwCurrentState;
        internal uint dwEventState;
        internal uint cbAtr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = WinSCardAPI.MAX_ATR_SIZE, ArraySubType = UnmanagedType.U1)]
        internal byte[] rgbAtr;
    }
}
