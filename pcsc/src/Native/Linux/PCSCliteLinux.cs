using System;
using System.Text;

namespace SpringCard.PCSC.Native.Linux
{
    /// <summary>
    /// PC/SC API for Linux (x86/x64) 
    /// </summary>
    internal sealed class PCSCliteLinux : ISCardApi
    {
        private const int MAX_READER_NAME = 255;
        private const int CHAR_SIZE = sizeof(byte);
        public const int MAX_ATR_SIZE = 33;

        public Encoding TextEncoding { get; } = Encoding.UTF8;

        public int CharSize => CHAR_SIZE;

        public int MaxAtrSize => MAX_ATR_SIZE;

        public bool IsWindows { get; } = false;

        public PCSCliteLinux()
        {
        }

        private long ToLong(IntPtr rc)
        {
            return unchecked((int)rc.ToInt64());
        }
        private uint ToUint(IntPtr rc)
        {
            return unchecked((uint)rc.ToInt64());
        }

        public long EstablishContext(uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext)
        {
            var ctx = IntPtr.Zero;
            var rc = ToLong(LinuxNativeMethods.SCardEstablishContext(
                (IntPtr)dwScope,
                pvReserved1,
                pvReserved2,
                ref ctx));
            phContext = ctx;
            return rc;
        }

        public long ReleaseContext(IntPtr hContext)
        {
            return ToLong(LinuxNativeMethods.SCardReleaseContext(hContext));
        }

        public long IsValidContext(IntPtr hContext)
        {
            return ToLong(LinuxNativeMethods.SCardIsValidContext(hContext));
        }

        public long ListReaders(IntPtr hContext, string[] groups, out string[] readers)
        {
            var dwReaders = IntPtr.Zero;

            // initialize groups array
            byte[] mszGroups = null;
            if (groups != null)
                mszGroups = SCARD.Helpers.ConvertToByteArray(groups, TextEncoding);

            // determine the needed buffer size
            var rc = ToLong(
                LinuxNativeMethods.SCardListReaders(hContext,
                    mszGroups,
                    null,
                    ref dwReaders));

            if (rc != SCARD.S_SUCCESS)
            {
                readers = null;
                return rc;
            }

            // initialize array for returning reader names
            var mszReaders = new byte[(int)dwReaders];

            rc = ToLong(
                LinuxNativeMethods.SCardListReaders(hContext,
                    mszGroups,
                    mszReaders,
                    ref dwReaders));

            readers = (rc == SCARD.S_SUCCESS)
                ? SCARD.Helpers.ConvertToStringArray(mszReaders, TextEncoding)
                : null;

            return rc;
        }

        public long ListReaderGroups(IntPtr hContext, out string[] groups)
        {
            var dwGroups = IntPtr.Zero;

            // determine the needed buffer size
            var rc = ToLong(
                LinuxNativeMethods.SCardListReaderGroups(
                    hContext,
                    null,
                    ref dwGroups));

            if (rc != SCARD.S_SUCCESS)
            {
                groups = null;
                return rc;
            }

            // initialize array for returning group names
            var mszGroups = new byte[(int)dwGroups];

            rc = ToLong(
                LinuxNativeMethods.SCardListReaderGroups(
                    hContext,
                    mszGroups,
                    ref dwGroups));

            groups = (rc == SCARD.S_SUCCESS)
                ? SCARD.Helpers.ConvertToStringArray(mszGroups, TextEncoding)
                : null;

            return rc;
        }

        public long Connect(IntPtr hContext, string szReader, uint dwShareMode, uint dwPreferredProtocols, out IntPtr phCard, out uint pdwActiveProtocol)
        {
            var readername = SCARD.Helpers.ConvertToByteArray(szReader, TextEncoding, CharSize);

            var result = LinuxNativeMethods.SCardConnect(hContext,
                readername,
                (IntPtr)dwShareMode,
                (IntPtr)dwPreferredProtocols,
                out phCard,
                out var activeproto);

            pdwActiveProtocol = (uint)activeproto;

            return ToLong(result);
        }

        public long Reconnect(IntPtr hCard, uint dwShareMode, uint dwPreferredProtocols, uint dwInitialization, out uint pdwActiveProtocol)
        {
            var result = LinuxNativeMethods.SCardReconnect(
                hCard,
                (IntPtr)dwShareMode,
                (IntPtr)dwPreferredProtocols,
                (IntPtr)dwInitialization,
                out var activeproto);

            pdwActiveProtocol = (uint)activeproto;
            return ToLong(result);
        }

        public long Disconnect(IntPtr hCard, uint dwDisposition)
        {
            return ToLong(LinuxNativeMethods.SCardDisconnect(hCard, (IntPtr)dwDisposition));
        }

        public long BeginTransaction(IntPtr hCard)
        {
            return ToLong(LinuxNativeMethods.SCardBeginTransaction(hCard));
        }

        public long EndTransaction(IntPtr hCard, uint dwDisposition)
        {
            return ToLong(LinuxNativeMethods.SCardEndTransaction(hCard, (IntPtr)dwDisposition));
        }

        public long Transmit(IntPtr hCard, IntPtr pioSendPci, byte[] pbSendBuffer, uint pcbSendLength, IntPtr pioRecvPci, byte[] pbRecvBuffer, ref uint pcbRecvLength)
        {
            var recvlen = IntPtr.Zero;
            if (pbRecvBuffer != null)
            {
                if (pcbRecvLength > pbRecvBuffer.Length || pcbRecvLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcbRecvLength));
                }

                recvlen = (IntPtr)pcbRecvLength;
            }
            else
            {
                if (pcbRecvLength != 0)
                    throw new ArgumentOutOfRangeException(nameof(pcbRecvLength));
            }

            var sendbuflen = IntPtr.Zero;
            if (pbSendBuffer != null)
            {
                if (pcbSendLength > pbSendBuffer.Length || pcbSendLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcbSendLength));
                }

                sendbuflen = (IntPtr)pcbSendLength;
            }
            else
            {
                if (pcbSendLength != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcbSendLength));
                }
            }

            var rc = ToLong(LinuxNativeMethods.SCardTransmit(
                hCard,
                pioSendPci,
                pbSendBuffer,
                sendbuflen,
                pioRecvPci,
                pbRecvBuffer,
                ref recvlen));

            pcbRecvLength = (uint)recvlen;
            return rc;
        }

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

            var sendbuflen = (IntPtr)sendBufferLength;
            var recvbuflen = (IntPtr)recvBufferLength;

            var rc = ToLong(LinuxNativeMethods.SCardControl(
                hCard,
                (IntPtr)dwControlCode,
                pbSendBuffer,
                sendbuflen,
                pbRecvBuffer,
                recvbuflen,
                out var bytesret));

            lpBytesReturned = (uint)bytesret;

            return rc;
        }

        public long Status(IntPtr hCard, out string[] szReaderName, out uint pdwState, out uint pdwProtocol, out byte[] pbAtr)
        {
            var readerName = new byte[MAX_READER_NAME * CharSize];
            var readerNameSize = (IntPtr)MAX_READER_NAME;

            pbAtr = new byte[MAX_ATR_SIZE];
            var atrlen = (IntPtr)pbAtr.Length;
            var rc = ToLong(LinuxNativeMethods.SCardStatus(
                hCard,
                readerName,
                ref readerNameSize,
                out var state,
                out var proto,
                pbAtr,
                ref atrlen));

            if (rc == SCARD.E_INSUFFICIENT_BUFFER || (MAX_READER_NAME < ((int)readerNameSize)) ||
                (pbAtr.Length < (int)atrlen))
            {
                // second try

                if (MAX_READER_NAME < ((int)readerNameSize))
                {
                    // readername byte array was too short
                    readerName = new byte[(int)readerNameSize * CharSize];
                }

                if (pbAtr.Length < (int)atrlen)
                {
                    // ATR byte array was too short
                    pbAtr = new byte[(int)atrlen];
                }

                rc = ToLong(LinuxNativeMethods.SCardStatus(
                    hCard,
                    readerName,
                    ref readerNameSize,
                    out state,
                    out proto,
                    pbAtr,
                    ref atrlen));
            }

            pdwState = ToUint(state);
            pdwProtocol = ToUint(proto);

            if (rc == SCARD.S_SUCCESS)
            {
                //state = state.Mask(STATUS_MASK);
                if ((int)atrlen < pbAtr.Length)
                {
                    Array.Resize(ref pbAtr, (int)atrlen);
                }

                if (((int)readerNameSize) < (readerName.Length / CharSize))
                {
                    Array.Resize(ref readerName, (int)readerNameSize * CharSize);
                }

                szReaderName = SCARD.Helpers.ConvertToStringArray(readerName, TextEncoding);
            }
            else
            {
                szReaderName = null;
            }

            return rc;
        }

        public long Status(IntPtr hCard, out uint pdwState, out uint pdwProtocol, out byte[] pbAtr)
        {
            var readerNameSize = (IntPtr)0;

            pbAtr = new byte[MAX_ATR_SIZE];
            var atrlen = (IntPtr)pbAtr.Length;
            var rc = ToLong(LinuxNativeMethods.SCardStatus(
                hCard,
                null,
                ref readerNameSize,
                out var state,
                out var proto,
                pbAtr,
                ref atrlen));

            if (rc == SCARD.E_INSUFFICIENT_BUFFER || (pbAtr.Length < (int)atrlen))
            {
                if (pbAtr.Length < (int)atrlen)
                {
                    // ATR byte array was too short
                    pbAtr = new byte[(int)atrlen];
                }

                rc = ToLong(LinuxNativeMethods.SCardStatus(
                    hCard,
                    null,
                    ref readerNameSize,
                    out state,
                    out proto,
                    pbAtr,
                    ref atrlen));
            }

            pdwState = ToUint(state);
            pdwProtocol = ToUint(proto);

            if (rc == SCARD.S_SUCCESS)
            {
                //state = state.Mask(STATUS_MASK);
                if ((int)atrlen < pbAtr.Length)
                {
                    Array.Resize(ref pbAtr, (int)atrlen);
                }
            }

            return rc;
        }

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
                    nativeReaderStates[i].dwCurrentState = (IntPtr) rgReaderStates[i].dwCurrentState;
                    nativeReaderStates[i].dwEventState = (IntPtr)rgReaderStates[i].dwEventState;
                    nativeReaderStates[i].cbAtr = (IntPtr)rgReaderStates[i].cbAtr;
                    nativeReaderStates[i].rgbAtr = rgReaderStates[i].rgbAtr;
                }
            }

            var rc = ToLong(LinuxNativeMethods.SCardGetStatusChange(
                hContext,
                (IntPtr)dwTimeout,
                nativeReaderStates,
                (IntPtr)cReaders));

            if (rc != SCARD.S_SUCCESS || rgReaderStates == null)
            {
                return rc;
            }

            for (uint i = 0; i < cReaders; i++)
            {
                rgReaderStates[i].szReader = nativeReaderStates[i].szReader;
                rgReaderStates[i].pvUserData = nativeReaderStates[i].pvUserData;
                rgReaderStates[i].dwCurrentState = ToUint(nativeReaderStates[i].dwCurrentState);
                rgReaderStates[i].dwEventState = ToUint(nativeReaderStates[i].dwEventState);
                rgReaderStates[i].cbAtr = ToUint(nativeReaderStates[i].cbAtr);
                rgReaderStates[i].rgbAtr = nativeReaderStates[i].rgbAtr;
            }

            return rc;
        }

        public long Cancel(IntPtr hContext)
        {
            return ToLong(LinuxNativeMethods.SCardCancel(hContext));
        }

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

            var attrlen = (IntPtr)pcbAttrLen;
            var rc = ToLong(LinuxNativeMethods.SCardGetAttrib(
                hCard,
                (IntPtr)dwAttrId,
                pbAttr,
                ref attrlen));

            pcbAttrLen = (uint)attrlen;
            return rc;
        }

        public long SetAttrib(IntPtr hCard, uint attributeId, byte[] sendBuffer, uint sendBufferLength)
        {
            IntPtr cbAttrLen;

            if (sendBuffer != null)
            {
                if (sendBufferLength > sendBuffer.Length || sendBufferLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sendBufferLength));
                }

                cbAttrLen = (IntPtr)sendBufferLength;
            }
            else
            {
                cbAttrLen = IntPtr.Zero;
            }

            return ToLong(
                LinuxNativeMethods.SCardSetAttrib(
                    hCard,
                    (IntPtr)attributeId,
                    sendBuffer,
                    cbAttrLen));
        }

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

        /* Specific to Windows */
        public long GetReaderDeviceInstanceId(IntPtr hContext, string szReaderName, out string szDeviceInstanceId)
        {
            throw new MissingMethodException("GetReaderDeviceInstanceId not available on Linux");
        }

        public long ListReadersWithDeviceInstanceId(IntPtr hContext, string InstanceId, out string[] ReaderNames)
        {
            throw new MissingMethodException("ListReadersWithDeviceInstanceId not available on Linux");
        }

        public long ListCards(IntPtr hContext, byte[] pbAtr, byte[] rgguiInterfaces, uint cguidInterfaceCount, string mszCards, ref int pcchCards)
        {
            throw new MissingMethodException("ListCards not available on Linux");
        }

        public long IntroduceCardType(IntPtr hContext, string szCardName, byte[] pguidPrimaryProvider, byte[] rgguidInterfaces, uint dwInterfaceCount, byte[] atr, byte[] pbAtrMask, uint cbAtrLen)
        {
            throw new MissingMethodException("IntroduceCardType not available on Linux");
        }

        public long SetCardTypeProviderName(IntPtr hContext, string szCardName, uint dwProviderId, string szProvider)
        {
            throw new MissingMethodException("SetCardTypeProviderName not available on Linux");
        }

        public IntPtr GetSymFromLib(string symName)
        {
            return LinuxNativeMethods.GetSymFromLib(symName);
        }


    }
}
