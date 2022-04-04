using System;
using System.Runtime.InteropServices;
using System.Text;
using SpringCard.PCSC;
using SpringCard.PCSC.Native;

namespace SpringCard.PCSC.Native.Windows
{
    /// <summary>
    /// PC/SC API for Microsoft Win32/Win64 (x86/x64/IA64)
    /// </summary>
    internal sealed class WinSCardAPI : ISCardApi
    {
        private const int MAX_READER_NAME = 255;
        private const int CHAR_SIZE = sizeof(char);
        private const string WINSCARD_DLL = "winscard.dll";
        private const string KERNEL_DLL = "KERNEL32.DLL";

        public const int MAX_ATR_SIZE = 0x24;

        private IntPtr _dllHandle = IntPtr.Zero;

        public int CharSize => CHAR_SIZE;

        public int MaxAtrSize => MAX_ATR_SIZE;

        public Encoding TextEncoding { get; } = Encoding.Unicode;
        public bool IsWindows { get; } = true;


        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardEstablishContext(
            [In] uint dwScope,
            [In] IntPtr pvReserved1,
            [In] IntPtr pvReserved2,
            [In, Out] ref IntPtr phContext);

        public long EstablishContext(uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext)
        {
            IntPtr ctx = IntPtr.Zero;
            uint rc = SCardEstablishContext(dwScope, pvReserved1, pvReserved2, ref ctx);
            phContext = ctx;
            return rc;
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardReleaseContext([In] IntPtr hContext);

        public long ReleaseContext(IntPtr hContext)
        {
            return SCardReleaseContext(hContext);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardIsValidContext([In] IntPtr hContext);

        public long IsValidContext(IntPtr hContext)
        {
            return SCardIsValidContext(hContext);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardListReaders(
            [In] IntPtr hContext,
            [In] byte[] mszGroups,
            [Out] byte[] pmszReaders,
            [In, Out] ref uint pcchReaders);

        public long ListReaders(IntPtr hContext, string[] groups, out string[] readers)
        {
            uint dwReaders = 0;

            // initialize groups array
            byte[] mszGroups = null;
            if (groups != null)
                mszGroups = SCARD.Helpers.ConvertToByteArray(groups, TextEncoding);

            // determine the needed buffer size
            long rc = SCardListReaders(hContext, mszGroups, null, ref dwReaders);
            if (rc != SCARD.S_SUCCESS)
            {
                readers = null;
                return rc;
            }

            // initialize array
            byte[] mszReaders = new byte[dwReaders * CharSize];

            rc = SCardListReaders(hContext, mszGroups, mszReaders, ref dwReaders);
            if (rc != SCARD.S_SUCCESS)
            {
                readers = null;
                return rc;
            }

            readers = SCARD.Helpers.ConvertToStringArray(mszReaders, TextEncoding);
            return rc;
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardListReaderGroups(
            [In] IntPtr hContext,
            [Out] byte[] mszGroups,
            [In, Out] ref uint pcchGroups);

        public long ListReaderGroups(IntPtr hContext, out string[] groups)
        {
            uint dwGroups = 0;

            // determine the needed buffer size
            long rc = SCardListReaderGroups(hContext, null, ref dwGroups);
            if (rc != SCARD.S_SUCCESS)
            {
                groups = null;
                return rc;
            }

            // initialize array
            byte[] mszGroups = new byte[dwGroups * CharSize];
            rc = SCardListReaderGroups(hContext, mszGroups, ref dwGroups);
            if (rc != SCARD.S_SUCCESS)
            {
                groups = null;
                return rc;
            }

            groups = SCARD.Helpers.ConvertToStringArray(mszGroups, TextEncoding);
            return rc;
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardConnect(
            [In] IntPtr hContext,
            [In] byte[] szReader,
            [In] uint dwShareMode,
            [In] uint dwPreferredProtocols,
            [Out] out IntPtr phCard,
            [Out] out uint pdwActiveProtocol);

        public long Connect(IntPtr hContext, string szReaderName, uint dwShareMode, uint dwPreferredProtocols, out IntPtr phCard, out uint pdwActiveProtocol)
        {
            byte[] l_readerName = SCARD.Helpers.ConvertToByteArray(szReaderName, TextEncoding, CharSize);

            long rc = SCardConnect(
                hContext,
                l_readerName,
                dwShareMode,
                dwPreferredProtocols,
                out phCard,
                out pdwActiveProtocol);

            return rc;
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardReconnect(
            [In] IntPtr hCard,
            [In] uint dwShareMode,
            [In] uint dwPreferredProtocols,
            [In] uint dwInitialization,
            [Out] out uint pdwActiveProtocol);

        public long Reconnect(IntPtr hCard, uint dwShareMode, uint dwPreferredProtocols, uint dwInitialization, out uint pdwActiveProtocol)
        {
            long rc = SCardReconnect(
                hCard,
                dwShareMode,
                dwPreferredProtocols,
                dwInitialization,
                out pdwActiveProtocol);

            return rc;
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardDisconnect(
            [In] IntPtr hCard,
            [In] uint dwDisposition);

        public long Disconnect(IntPtr hCard, uint dwDisposition)
        {
            return SCardDisconnect(hCard, dwDisposition);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardBeginTransaction(
            [In] IntPtr hCard);

        public long BeginTransaction(IntPtr hCard)
        {
            return SCardBeginTransaction(hCard);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardEndTransaction(
            [In] IntPtr hCard,
            [In] uint dwDisposition);

        public long EndTransaction(IntPtr hCard, uint dwDisposition)
        {
            return SCardEndTransaction(hCard, dwDisposition);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardTransmit(
            [In] IntPtr hCard,
            [In] IntPtr pioSendPci,
            [In] byte[] pbSendBuffer,
            [In] uint cbSendLength,
            [In, Out] IntPtr pioRecvPci,
            [Out] byte[] pbRecvBuffer,
            [In, Out] ref uint pcbRecvLength);

        public long Transmit(IntPtr hCard, IntPtr pioSendPci, byte[] pbSendBuffer, uint cbSendLength, IntPtr pioRecvPci, byte[] pbRecvBuffer, ref uint pcbRecvLength)
        {
            if (pbRecvBuffer != null)
            {
                if (pcbRecvLength > pbRecvBuffer.Length || pcbRecvLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcbRecvLength));
                }
            }
            else if (pcbRecvLength != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pcbRecvLength));
            }

            if (pbSendBuffer != null)
            {
                if (cbSendLength > pbSendBuffer.Length || cbSendLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(cbSendLength));
                }
            }
            else if (cbSendLength != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cbSendLength));
            }

            return SCardTransmit(hCard, pioSendPci, pbSendBuffer, cbSendLength, pioRecvPci, pbRecvBuffer, ref pcbRecvLength);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardControl(
            [In] IntPtr hCard,
            [In] uint dwControlCode,
            [In] byte[] lpInBuffer,
            [In] uint nInBufferSize,
            [In, Out] byte[] lpOutBuffer,
            [In] uint nOutBufferSize,
            [Out] out uint lpBytesReturned);

        public long Control(IntPtr hCard, uint dwControlCode, byte[] pbSendBuffer, uint sendBufferLength, byte[] pbRecvBuffer, uint recvBufferLength, out uint lpBytesReturned)
        {
            if (pbSendBuffer == null && sendBufferLength > 0)
            {
                throw new ArgumentException("send buffer is null", nameof(sendBufferLength));
            }
            if ((pbSendBuffer != null && pbSendBuffer.Length < sendBufferLength) || sendBufferLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sendBufferLength));
            }
            if (pbRecvBuffer == null && recvBufferLength > 0)
            {
                throw new ArgumentException("receive buffer is null", nameof(recvBufferLength));
            }
            if ((pbRecvBuffer != null && pbRecvBuffer.Length < recvBufferLength) || recvBufferLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(recvBufferLength));
            }

            return SCardControl(hCard, dwControlCode, pbSendBuffer, sendBufferLength, pbRecvBuffer, recvBufferLength, out lpBytesReturned);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardStatus(
            [In] IntPtr hCard,
            [Out] byte[] szReaderName,
            [In, Out] ref uint pcchReaderLen,
            [Out] out uint pdwState,
            [Out] out uint pdwProtocol,
            [Out] byte[] pbAtr,
            [In, Out] ref uint pcbAtrLen);

        public long Status(IntPtr hCard, out string[] szReaderName, out uint pdwState, out uint pdwProtocol, out byte[] pbAtr)
        {
            byte[] readerName = new byte[MAX_READER_NAME * CharSize];
            uint readerNameSize = MAX_READER_NAME;

            pbAtr = new byte[MAX_ATR_SIZE];
            uint atrlen = MAX_ATR_SIZE;

            long rc = SCardStatus(hCard, readerName, ref readerNameSize, out pdwState, out pdwProtocol, pbAtr, ref atrlen);

            if (rc == SCARD.E_INSUFFICIENT_BUFFER || (MAX_READER_NAME < readerNameSize) || (pbAtr.Length < atrlen))
            {
                // second try
                if (MAX_READER_NAME < readerNameSize)
                {
                    // readername byte array was too short
                    readerName = new byte[readerNameSize * CharSize];
                }

                if (pbAtr.Length < atrlen)
                {
                    // ATR byte array was too short
                    pbAtr = new byte[atrlen];
                }

                rc = SCardStatus(hCard, readerName, ref readerNameSize, out pdwState, out pdwProtocol, pbAtr, ref atrlen);
            }

            if (rc == SCARD.S_SUCCESS)
            {
                if (atrlen < pbAtr.Length)
                {
                    Array.Resize(ref pbAtr, (int)atrlen);
                }
                szReaderName = SCARD.Helpers.ConvertToStringArray(readerName, TextEncoding);
            }
            else
            {
                szReaderName = null;
                pbAtr = null;
            }

            return rc;
        }

        public long Status(IntPtr hCard, out uint pdwState, out uint pdwProtocol, out byte[] pbAtr)
        {
            uint readerNameSize = 0;

            pbAtr = new byte[MAX_ATR_SIZE];
            uint atrlen = MAX_ATR_SIZE;

            long rc = SCardStatus(hCard, null, ref readerNameSize, out pdwState, out pdwProtocol, pbAtr, ref atrlen);
            if (rc == SCARD.S_SUCCESS)
            {
                if (atrlen < pbAtr.Length)
                {
                    Array.Resize(ref pbAtr, (int)atrlen);
                }
            }
            else
            {
                pbAtr = null;
            }

            return rc;
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardGetStatusChange(
            [In] IntPtr hContext,
            [In] uint dwTimeout,
            [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            SCARD_READERSTATE[] rgReaderStates,
            [In] uint cReaders);

        public long GetStatusChange(IntPtr hContext, uint dwTimeout, SCARD.READERSTATE[] rgReaderStates, uint cReaders)
        {
            SCARD_READERSTATE[] nativeReaderStates = null;

            if (rgReaderStates != null)
            {
                nativeReaderStates = new SCARD_READERSTATE[cReaders];
                for (uint i = 0; i < cReaders; i++)
                {
                    nativeReaderStates[i].szReader = rgReaderStates[i].szReader;
                    nativeReaderStates[i].pvUserData = rgReaderStates[i].pvUserData;
                    nativeReaderStates[i].dwCurrentState = rgReaderStates[i].dwCurrentState;
                    nativeReaderStates[i].dwEventState = rgReaderStates[i].dwEventState;
                    nativeReaderStates[i].cbAtr = rgReaderStates[i].cbAtr;
                    nativeReaderStates[i].rgbAtr = rgReaderStates[i].rgbAtr;
                }
            }

            long rc = SCardGetStatusChange(hContext, dwTimeout, nativeReaderStates, cReaders);

            if (rc != SCARD.S_SUCCESS || rgReaderStates == null)
            {
                return rc;
            }

            for (uint i = 0; i < cReaders; i++)
            {
                rgReaderStates[i].szReader = nativeReaderStates[i].szReader;
                rgReaderStates[i].pvUserData = nativeReaderStates[i].pvUserData;
                rgReaderStates[i].dwCurrentState = nativeReaderStates[i].dwCurrentState;
                rgReaderStates[i].dwEventState = nativeReaderStates[i].dwEventState;
                rgReaderStates[i].cbAtr = nativeReaderStates[i].cbAtr;
                rgReaderStates[i].rgbAtr = nativeReaderStates[i].rgbAtr;
            }

            return rc;
        }


        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardCancel(
            [In] IntPtr hContext);

        public long Cancel(IntPtr hContext)
        {
            return SCardCancel(hContext);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardGetAttrib(
            [In] IntPtr hCard,
            [In] uint dwAttrId,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] pbAttr,
            [In, Out] ref uint pcbAttrLen);

        public long GetAttrib(IntPtr hCard, uint dwAttrId, byte[] pbAttr, ref uint pcbAttrLen)
        {
            if (pbAttr == null && pcbAttrLen != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pcbAttrLen));
            }
            if (pbAttr != null && (pcbAttrLen < 0 || pcbAttrLen > pbAttr.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(pcbAttrLen));
            }

            return SCardGetAttrib(hCard, dwAttrId, pbAttr, ref pcbAttrLen);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardSetAttrib(
            [In] IntPtr hCard,
            [In] uint dwAttrId,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)]
            byte[] pbAttr,
            [In] uint cbAttrLen);


        public long SetAttrib(IntPtr hCard, uint dwAttrId, byte[] pbAttr, uint cbAttrLen)
        {
            if (pbAttr != null)
            {
                if (cbAttrLen > pbAttr.Length || cbAttrLen < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(cbAttrLen));
                }
            }

            return SCardSetAttrib(hCard, dwAttrId, pbAttr, cbAttrLen);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardGetReaderDeviceInstanceId(
            IntPtr context,
            [In] byte[] szReaderName,
            [Out] byte[] szDeviceInstanceId,
            [In, Out] ref uint pcchDeviceInstanceId);

        public long GetReaderDeviceInstanceId(IntPtr hContext, string szReaderName, out string szDeviceInstanceId)
        {
            uint pcchDeviceInstanceId = 0;
            byte[] l_ReaderName = SCARD.Helpers.ConvertToByteArray(szReaderName, TextEncoding, CharSize);

            long rc = SCardGetReaderDeviceInstanceId(hContext, l_ReaderName, null, ref pcchDeviceInstanceId);
            if (rc != SCARD.S_SUCCESS)
            {
                szDeviceInstanceId = null;
                return rc;
            }

            byte[] l_DeviceInstanceId = new byte[pcchDeviceInstanceId * CharSize];
            rc = SCardGetReaderDeviceInstanceId(hContext, l_ReaderName, l_DeviceInstanceId, ref pcchDeviceInstanceId);
            if (rc != SCARD.S_SUCCESS)
            {
                szDeviceInstanceId = null;
                return rc;
            }

            szDeviceInstanceId = SCARD.Helpers.ConvertToString(l_DeviceInstanceId, TextEncoding);
            return rc;
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardListReadersWithDeviceInstanceId(
            IntPtr context,
            [In] byte[] szDeviceInstanceId,
            [Out] byte[] mszReaders,
            [In, Out] ref uint pcchReaders);

        public long ListReadersWithDeviceInstanceId(IntPtr hContext, string InstanceId, out string[] ReaderNames)
        {
            uint pcchReaders = 0;
            byte[] l_InstanceId = SCARD.Helpers.ConvertToByteArray(InstanceId, TextEncoding, CharSize);

            long rc = SCardListReadersWithDeviceInstanceId(hContext, l_InstanceId, null, ref pcchReaders);
            if (rc != SCARD.S_SUCCESS)
            {
                ReaderNames = null;
                return rc;
            }

            byte[] l_ReaderNames = new byte[pcchReaders * CharSize];
            rc = SCardListReadersWithDeviceInstanceId(hContext, l_InstanceId, l_ReaderNames, ref pcchReaders);
            if (rc != SCARD.S_SUCCESS)
            {
                ReaderNames = null;
                return rc;
            }

            ReaderNames = SCARD.Helpers.ConvertToStringArray(l_ReaderNames, TextEncoding);
            return rc;
        }


        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardListCards(
            IntPtr hContext,
            byte[] pbAtr,
            byte[] rgguiInterfaces,
            uint cguidInterfaceCount,
            string mszCards,
            ref int pcchCards);

        public long ListCards(IntPtr hContext, byte[] pbAtr, byte[] rgguiInterfaces, uint cguidInterfaceCount, string mszCards, ref int pcchCards)
        {
            return SCardListCards(hContext, pbAtr, rgguiInterfaces, cguidInterfaceCount, mszCards, ref pcchCards);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardIntroduceCardType(
            IntPtr hContext,
            string szCardName,
            byte[] pguidPrimaryProvider,
            byte[] rgguidInterfaces,
            uint dwInterfaceCount,
            byte[] atr,
            byte[] pbAtrMask,
            uint cbAtrLen);

        public long IntroduceCardType(IntPtr hContext, string szCardName, byte[] pguidPrimaryProvider, byte[] rgguidInterfaces, uint dwInterfaceCount, byte[] atr, byte[] pbAtrMask, uint cbAtrLen)
        {
            return SCardIntroduceCardType(hContext, szCardName, pguidPrimaryProvider, rgguidInterfaces, dwInterfaceCount, atr, pbAtrMask, cbAtrLen);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardSetCardTypeProviderName(
            IntPtr hContext,
            string szCardName,
            uint dwProviderId,
            string szProvider);

        public long SetCardTypeProviderName(IntPtr hContext, string szCardName, uint dwProviderId, string szProvider)
        {
            return SCardSetCardTypeProviderName(hContext, szCardName, dwProviderId, szProvider);
        }

        [DllImport(WINSCARD_DLL, CharSet = CharSet.Unicode)]
        private static extern uint SCardFreeMemory(
            [In] IntPtr hContext,
            [In] IntPtr pvMem);


        // PCI
        private IntPtr _scard_pci_t0 = IntPtr.Zero;
        public IntPtr PCI_T0()
        {
            if (_scard_pci_t0 == IntPtr.Zero)
                _scard_pci_t0 = GetSymFromLib("g_rgSCardT0Pci");
            return _scard_pci_t0;
        }

        private IntPtr _scard_pci_t1 = IntPtr.Zero;
        public IntPtr PCI_T1()
        {
            if (_scard_pci_t1 == IntPtr.Zero)
                _scard_pci_t1 = GetSymFromLib("g_rgSCardT1Pci");
            return _scard_pci_t1;
        }

        private IntPtr _scard_pci_raw = IntPtr.Zero;
        public IntPtr PCI_RAW()
        {
            if (_scard_pci_raw == IntPtr.Zero)
                _scard_pci_raw = GetSymFromLib("g_rgSCardRawPci");
            return _scard_pci_raw;
        }

        // Windows specific DLL imports

        [DllImport(KERNEL_DLL, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(KERNEL_DLL, CharSet = CharSet.Ansi, ExactSpelling = true, EntryPoint = "GetProcAddress")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        public IntPtr GetSymFromLib(string symName)
        {
            // Step 1. load dynamic link library
            if (_dllHandle == IntPtr.Zero)
            {
                _dllHandle = LoadLibrary(WINSCARD_DLL);
                if (_dllHandle.Equals(IntPtr.Zero))
                {
                    throw new Exception("PInvoke call LoadLibrary() failed");
                }
            }

            // Step 2. search symbol name in memory
            var symPtr = GetProcAddress(_dllHandle, symName);

            if (symPtr.Equals(IntPtr.Zero))
            {
                throw new Exception("PInvoke call GetProcAddress() failed");
            }

            return symPtr;
        }

        public WinSCardAPI()
        {
            _dllHandle = LoadLibrary(WINSCARD_DLL);
            if (_dllHandle.Equals(IntPtr.Zero))
            {
                throw new Exception("PInvoke call LoadLibrary() failed");
            }
        }
    }
}
