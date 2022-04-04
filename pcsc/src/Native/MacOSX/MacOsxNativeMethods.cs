using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SpringCard.PCSC.Native.MacOsX
{
    internal static class MacOsxNativeMethods
    {
        private static IntPtr _libHandle = IntPtr.Zero;
        private const string PCSC_LIB = "PCSC.framework/PCSC";
        private const string DL_LIB = "libdl.dylib";

        public static IntPtr GetSymFromLib(string symName) {
            // Step 1. load dynamic link library
            if (_libHandle == IntPtr.Zero) {
                _libHandle = dlopen(PCSC_LIB, (int) DLOPEN_FLAGS.RTLD_LAZY);
                if (_libHandle.Equals(IntPtr.Zero)) {
                    throw new Exception("PInvoke call dlopen() failed");
                }
            }

            // Step 2. search symbol name in memory
            var symPtr = dlsym(_libHandle, symName);

            if (symPtr.Equals(IntPtr.Zero)) {
                throw new Exception("PInvoke call dlsym() failed");
            }

            return symPtr;
        }

        [DllImport(PCSC_LIB)]
        internal static extern int SCardEstablishContext(
            [In] uint dwScope,
            [In] IntPtr pvReserved1,
            [In] IntPtr pvReserved2,
            [In, Out] ref int phContext);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardReleaseContext(
            [In] int hContext);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardIsValidContext(
            [In] int hContext);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardListReaders(
            [In] int hContext,
            [In] byte[] mszGroups,
            [Out] byte[] mszReaders,
            [In, Out] ref uint pcchReaders);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardListReaderGroups(
            [In] int hContext,
            [Out] byte[] mszGroups,
            [In, Out] ref uint pcchGroups);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardConnect(
            [In] int hContext,
            [In] byte[] szReader,
            [In] uint dwShareMode,
            [In] uint dwPreferredProtocols,
            [Out] out int phCard,
            [Out] out uint pdwActiveProtocol);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardReconnect(
            [In] int hCard,
            [In] uint dwShareMode,
            [In] uint dwPreferredProtocols,
            [In] uint dwInitialization,
            [Out] out uint pdwActiveProtocol);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardDisconnect(
            [In] int hCard,
            [In] uint dwDisposition);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardBeginTransaction(
            [In] int hCard);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardEndTransaction(
            [In] int hCard,
            [In] uint dwDisposition);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardTransmit(
            [In] int hCard,
            [In] IntPtr pioSendPci,
            [In] byte[] pbSendBuffer,
            [In] uint cbSendLength,
            [In, Out] IntPtr pioRecvPci,
            [Out] byte[] pbRecvBuffer,
            [In, Out] ref uint pcbRecvLength);

        [DllImport(PCSC_LIB, EntryPoint = "SCardControl132")]
        internal static extern int SCardControl(
            [In] int hCard,
            [In] uint dwControlCode,
            [In] byte[] pbSendBuffer,
            [In] uint cbSendLength,
            [Out] byte[] pbRecvBuffer,
            [In] uint pcbRecvLength,
            [Out] out uint lpBytesReturned);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardStatus(
            [In] int hCard,
            [Out] byte[] szReaderName,
            [In, Out] ref uint pcchReaderLen,
            [Out] out uint pdwState,
            [Out] out uint pdwProtocol,
            [Out] byte[] pbAtr,
            [In, Out] ref uint pcbAtrLen);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardGetStatusChange(
            [In] int hContext,
            [In] uint dwTimeout,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            SCARD_READERSTATE[] rgReaderStates,
            [In] uint cReaders);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardCancel(
            [In] int hContext);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardGetAttrib(
            [In] int hCard,
            [In] uint dwAttrId,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] pbAttr,
            [In, Out] ref uint pcbAttrLen);

        [DllImport(PCSC_LIB)]
        internal static extern int SCardSetAttrib(
            [In] int hCard,
            [In] uint dwAttrId,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] pbAttr,
            [In] uint cbAttrLen);

        [DllImport(DL_LIB)]
        internal static extern IntPtr dlopen(
            [In] string szFilename,
            [In] uint flag);

        [DllImport(DL_LIB)]
        internal static extern IntPtr dlsym(
            [In] IntPtr handle,
            [In] string szSymbol);

        [DllImport(DL_LIB)]
        internal static extern int dlclose(
            [In] IntPtr handle);
    }
}
