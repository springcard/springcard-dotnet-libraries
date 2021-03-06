using System;
using System.Runtime.InteropServices;

namespace SpringCard.PCSC.Native.Linux
{
    [StructLayout(LayoutKind.Sequential)]
    internal class SCARD_IO_REQUEST
    {
        internal SCARD_IO_REQUEST() {
            dwProtocol = IntPtr.Zero;
        }

        internal IntPtr dwProtocol; // Protocol identifier
        internal IntPtr cbPciLength; // Protocol Control Inf Length
    }
}
