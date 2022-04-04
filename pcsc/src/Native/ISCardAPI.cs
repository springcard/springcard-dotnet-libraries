using System;
using System.Text;
using SpringCard.PCSC;

namespace SpringCard.PCSC.Native
{
    /// <summary>
    /// Gives access to the system's smart card API
    /// </summary>
    internal interface ISCardApi
    {
        IntPtr GetSymFromLib(string symName);

        int MaxAtrSize { get; }
        Encoding TextEncoding { get; }
        //int CharSize { get; }
        bool IsWindows { get; }

        long EstablishContext(uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext);
        long ReleaseContext(IntPtr hContext);
        long IsValidContext(IntPtr hContext);

        long ListReaders(IntPtr hContext, string[] groups, out string[] readers);
        long ListReaderGroups(IntPtr hContext, out string[] groups);

        long Connect(IntPtr hContext, string szReader, uint dwShareMode, uint dwPreferredProtocols, out IntPtr phCard, out uint pdwActiveProtocol);

        long Disconnect(IntPtr hCard, uint dwDisposition);

        long Reconnect(IntPtr hCard, uint dwShareMode, uint dwPreferredProtocols, uint dwInitialization, out uint pdwActiveProtocol);

        long BeginTransaction(IntPtr hCard);

        long EndTransaction(IntPtr hCard, uint dwDisposition);

        long Transmit(IntPtr hCard, IntPtr pioSendPci, byte[] pbSendBuffer, uint cbSendLength, IntPtr pioRecvPci, byte[] pbRecvBuffer, ref uint pcbRecvLength);

        long Control(IntPtr hCard, uint dwControlCode, byte[] pbSendBuffer, uint sendBufferLength, byte[] pbRecvBuffer, uint recvBufferLength, out uint lpBytesReturned);

        long Status(IntPtr hCard, out string[] szReaderName, out uint pdwState, out uint pdwProtocol, out byte[] pbAtr);
        long Status(IntPtr hCard, out uint pdwState, out uint pdwProtocol, out byte[] pbAtr);

        long GetStatusChange(IntPtr hContext, uint dwTimeout, SCARD.READERSTATE[] rgReaderStates, uint cReaders);

        long Cancel(IntPtr hContext);

        long GetAttrib(IntPtr hCard, uint dwAttrId, byte[] pbAttr, ref uint pcbAttrLen);

        long SetAttrib(IntPtr hCard, uint dwAttrId, byte[] pbAttr, uint cbAttrLen);

        /* PCI */
        IntPtr PCI_T0();
        IntPtr PCI_T1();
        IntPtr PCI_RAW();

        /* Specific to Windows */
        long GetReaderDeviceInstanceId(IntPtr hContext, string szReaderName, out string szDeviceInstanceId);
        long ListReadersWithDeviceInstanceId(IntPtr hContext, string InstanceId, out string[] ReaderNames);
        long ListCards(IntPtr hContext, byte[] pbAtr, byte[] rgguiInterfaces, uint cguidInterfaceCount, string mszCards, ref int pcchCards);
        long IntroduceCardType(IntPtr hContext, string szCardName, byte[] pguidPrimaryProvider, byte[] rgguidInterfaces, uint dwInterfaceCount, byte[] atr, byte[] pbAtrMask, uint cbAtrLen);
        long SetCardTypeProviderName(IntPtr hContext, string szCardName, uint dwProviderId, string szProvider);
    }
}
