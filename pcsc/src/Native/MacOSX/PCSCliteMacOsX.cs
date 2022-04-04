using System;
using System.Text;

namespace SpringCard.PCSC.Native.MacOsX
{
    /// <summary>
    /// PC/SC API for MacOS X
    /// </summary>
    internal sealed class PCSCliteMacOsX : ISCardApi
    {
        private const int MAX_READER_NAME = 255;
        private const int CHAR_SIZE = sizeof(byte);
        public const int MAX_ATR_SIZE = 33;

        public Encoding TextEncoding { get; } = Encoding.UTF8;

        public int CharSize => CHAR_SIZE;

        public int MaxAtrSize => MAX_ATR_SIZE;

        public bool IsWindows { get; } = false;

        public PCSCliteMacOsX()
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
            int ctx = 0;
            long rc = MacOsxNativeMethods.SCardEstablishContext(
                dwScope,
                pvReserved1,
                pvReserved2,
                ref ctx);
            phContext = (IntPtr)ctx;
            return rc;
        }

        public long ReleaseContext(IntPtr hContext)
        {
            return MacOsxNativeMethods.SCardReleaseContext(hContext.ToInt32());
        }

        public long IsValidContext(IntPtr hContext)
        {
            return MacOsxNativeMethods.SCardIsValidContext(hContext.ToInt32());
        }

        public long ListReaders(IntPtr hContext, string[] groups, out string[] readers)
        {
            uint dwReaders = 0;

            // initialize groups array
            byte[] mszGroups = null;
            if (groups != null)
                mszGroups = SCARD.Helpers.ConvertToByteArray(groups, TextEncoding);

            // determine the needed buffer size

            var ctx = hContext.ToInt32();
            var rc = MacOsxNativeMethods.SCardListReaders(
                    ctx,
                    mszGroups,
                    null,
                    ref dwReaders);

            if (rc != SCARD.S_SUCCESS)
            {
                readers = null;
                return rc;
            }

            // initialize array for returning reader names
            var mszReaders = new byte[dwReaders];

            rc = MacOsxNativeMethods.SCardListReaders(
                    ctx,
                    mszGroups,
                    mszReaders,
                    ref dwReaders);

            readers = (rc == SCARD.S_SUCCESS)
                ? SCARD.Helpers.ConvertToStringArray(mszReaders, TextEncoding)
                : null;

            return rc;
        }

        public long ListReaderGroups(IntPtr hContext, out string[] groups)
        {
            uint dwGroups = 0;

            // determine the needed buffer size
            var ctx = hContext.ToInt32();
            var rc = MacOsxNativeMethods.SCardListReaderGroups(
                    ctx,
                    null,
                    ref dwGroups);

            if (rc != SCARD.S_SUCCESS)
            {
                groups = null;
                return rc;
            }

            // initialize array for returning group names
            var mszGroups = new byte[dwGroups];

            rc = MacOsxNativeMethods.SCardListReaderGroups(
                    ctx,
                    mszGroups,
                    ref dwGroups);

            groups = (rc == SCARD.S_SUCCESS)
                ? SCARD.Helpers.ConvertToStringArray(mszGroups, TextEncoding)
                : null;

            return rc;
        }

        public long Connect(IntPtr hContext, string szReader, uint dwShareMode, uint dwPreferredProtocols, out IntPtr phCard, out uint pdwActiveProtocol)
        {
            var readername = SCARD.Helpers.ConvertToByteArray(szReader, TextEncoding, CharSize);
            var rc = MacOsxNativeMethods.SCardConnect(
                hContext.ToInt32(),
                readername,
                dwShareMode,
                dwPreferredProtocols,
                out var card,
                out pdwActiveProtocol);

            phCard = (IntPtr)card;

            return rc;
        }

        public long Reconnect(IntPtr hCard, uint dwShareMode, uint dwPreferredProtocols, uint dwInitialization, out uint pdwActiveProtocol)
        {
            return MacOsxNativeMethods.SCardReconnect(
                hCard.ToInt32(),
                dwShareMode,
                dwPreferredProtocols,
                dwInitialization,
                out pdwActiveProtocol);
        }

        public long Disconnect(IntPtr hCard, uint dwDisposition)
        {
            return MacOsxNativeMethods.SCardDisconnect(hCard.ToInt32(), dwDisposition);
        }

        public long BeginTransaction(IntPtr hCard)
        {
            return MacOsxNativeMethods.SCardBeginTransaction(hCard.ToInt32());
        }

        public long EndTransaction(IntPtr hCard, uint dwDisposition)
        {
            return MacOsxNativeMethods.SCardEndTransaction(hCard.ToInt32(), dwDisposition);
        }

        public long Transmit(IntPtr hCard, IntPtr pioSendPci, byte[] pbSendBuffer, uint pcbSendLength, IntPtr pioRecvPci, byte[] pbRecvBuffer, ref uint pcbRecvLength)
        {
            uint recvlen = 0;
            if (pbRecvBuffer != null)
            {
                if (pcbRecvLength > pbRecvBuffer.Length || pcbRecvLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcbRecvLength));
                }

                recvlen = pcbRecvLength;
            }
            else
            {
                if (pcbRecvLength != 0)
                    throw new ArgumentOutOfRangeException(nameof(pcbRecvLength));
            }

            uint sendbuflen = 0;
            if (pbSendBuffer != null)
            {
                if (pcbSendLength > pbSendBuffer.Length || pcbSendLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcbSendLength));
                }

                sendbuflen = pcbSendLength;
            }
            else
            {
                if (pcbSendLength != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(pcbSendLength));
                }
            }

            var rc = MacOsxNativeMethods.SCardTransmit(
                hCard.ToInt32(),
                pioSendPci,
                pbSendBuffer,
                sendbuflen,
                pioRecvPci,
                pbRecvBuffer,
                ref recvlen);

            pcbRecvLength = recvlen;
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

            var rc = MacOsxNativeMethods.SCardControl(
                hCard.ToInt32(),
                dwControlCode,
                pbSendBuffer,
                sendBufferLength,
                pbRecvBuffer,
                recvBufferLength,
                out lpBytesReturned);

            return rc;
        }

        public long Status(IntPtr hCard, out string[] szReaderName, out uint pdwState, out uint pdwProtocol, out byte[] pbAtr)
        {
            var readerName = new byte[MAX_READER_NAME * CharSize];
            uint readerNameSize = MAX_READER_NAME;

            pbAtr = new byte[MAX_ATR_SIZE];
            uint atrlen = MAX_ATR_SIZE;
            long rc = MacOsxNativeMethods.SCardStatus(
                hCard.ToInt32(),
                readerName,
                ref readerNameSize,
                out pdwState,
                out pdwProtocol,
                pbAtr,
                ref atrlen);

            if (rc == SCARD.E_INSUFFICIENT_BUFFER || (MAX_READER_NAME < readerNameSize) ||
                (pbAtr.Length < atrlen))
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

                rc = MacOsxNativeMethods.SCardStatus(
                    hCard.ToInt32(),
                    readerName,
                    ref readerNameSize,
                    out pdwState,
                    out pdwProtocol,
                    pbAtr,
                    ref atrlen);
            }

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
            uint readerNameSize = 0;

            pbAtr = new byte[MAX_ATR_SIZE];
            uint atrlen = MAX_ATR_SIZE;
            long rc = MacOsxNativeMethods.SCardStatus(
                hCard.ToInt32(),
                null,
                ref readerNameSize,
                out pdwState,
                out pdwProtocol,
                pbAtr,
                ref atrlen);

            if (rc == SCARD.E_INSUFFICIENT_BUFFER || (pbAtr.Length < (int)atrlen))
            {
                if (pbAtr.Length < (int)atrlen)
                {
                    // ATR byte array was too short
                    pbAtr = new byte[(int)atrlen];
                }

                rc = MacOsxNativeMethods.SCardStatus(
                    hCard.ToInt32(),
                    null,
                    ref readerNameSize,
                    out pdwState,
                    out pdwProtocol,
                    pbAtr,
                    ref atrlen);
            }

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
                    nativeReaderStates[i].dwCurrentState = rgReaderStates[i].dwCurrentState;
                    nativeReaderStates[i].dwEventState = rgReaderStates[i].dwEventState;
                    nativeReaderStates[i].cbAtr = rgReaderStates[i].cbAtr;
                    nativeReaderStates[i].rgbAtr = rgReaderStates[i].rgbAtr;
                }
            }

            var rc = MacOsxNativeMethods.SCardGetStatusChange(
                hContext.ToInt32(),
                dwTimeout,
                nativeReaderStates,
                cReaders);

            if (rc != SCARD.S_SUCCESS || rgReaderStates == null)
            {
                return rc;
            }

            for (var i = 0; i < cReaders; i++)
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

        public long Cancel(IntPtr hContext)
        {
            return MacOsxNativeMethods.SCardCancel(hContext.ToInt32());
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

            var rc = MacOsxNativeMethods.SCardGetAttrib(
                hCard.ToInt32(),
                dwAttrId,
                pbAttr,
                ref pcbAttrLen);

            return rc;
        }

        public long SetAttrib(IntPtr hCard, uint dwAttrId, byte[] sendBuffer, uint sendBufferLength)
        {
            uint cbAttrLen;

            if (sendBuffer != null)
            {
                if (sendBufferLength > sendBuffer.Length || sendBufferLength < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sendBufferLength));
                }

                cbAttrLen = sendBufferLength;
            }
            else
            {
                sendBufferLength = 0;
            }

            return MacOsxNativeMethods.SCardSetAttrib(
                    hCard.ToInt32(),
                    dwAttrId,
                    sendBuffer,
                    sendBufferLength);
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
            throw new MissingMethodException("GetReaderDeviceInstanceId not available on MacOsX");
        }

        public long ListReadersWithDeviceInstanceId(IntPtr hContext, string InstanceId, out string[] ReaderNames)
        {
            throw new MissingMethodException("ListReadersWithDeviceInstanceId not available on MacOsX");
        }

        public long ListCards(IntPtr hContext, byte[] pbAtr, byte[] rgguiInterfaces, uint cguidInterfaceCount, string mszCards, ref int pcchCards)
        {
            throw new MissingMethodException("ListCards not available on MacOsX");
        }

        public long IntroduceCardType(IntPtr hContext, string szCardName, byte[] pguidPrimaryProvider, byte[] rgguidInterfaces, uint dwInterfaceCount, byte[] atr, byte[] pbAtrMask, uint cbAtrLen)
        {
            throw new MissingMethodException("IntroduceCardType not available on MacOsX");
        }

        public long SetCardTypeProviderName(IntPtr hContext, string szCardName, uint dwProviderId, string szProvider)
        {
            throw new MissingMethodException("SetCardTypeProviderName not available on MacOsX");
        }

        public IntPtr GetSymFromLib(string symName)
        {
            return MacOsxNativeMethods.GetSymFromLib(symName);
        }
    }
}
