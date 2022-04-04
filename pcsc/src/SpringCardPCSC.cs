/**
 *
 * \defgroup PCSC
 *
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D, Emilie.C and Jerome.I / SpringCard
 *
 * \page License
 *
 *	This software is part of the SPRINGCARD SDK FOR PC/SC
 *
 *   Redistribution and use in source (source code) and binary
 *   (object code) forms, with or without modification, are
 *   permitted provided that the following conditions are met :
 *
 *   1. Redistributed source code or object code shall be used
 *   only in conjunction with products (hardware devices) either
 *   manufactured, distributed or developed by SPRINGCARD,
 *
 *   2. Redistributed source code, either modified or
 *   un-modified, must retain the above copyright notice,
 *   this list of conditions and the disclaimer below,
 *
 *   3. Redistribution of any modified code must be clearly
 *   identified "Code derived from original SPRINGCARD 
 *   copyrighted source code", with a description of the
 *   modification and the name of its author,
 *
 *   4. Redistributed object code must reproduce the above
 *   copyright notice, this list of conditions and the
 *   disclaimer below in the documentation and/or other
 *   materials provided with the distribution,
 *
 *   5. The name of SPRINGCARD may not be used to endorse
 *   or promote products derived from this software or in any
 *   other form without specific prior written permission from
 *   SPRINGCARD.
 *
 *   THIS SOFTWARE IS PROVIDED BY SPRINGCARD "AS IS".
 *   SPRINGCARD SHALL NOT BE LIABLE FOR INFRINGEMENTS OF THIRD
 *   PARTIES RIGHTS BASED ON THIS SOFTWARE.
 *
 *   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 *   FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *
 *   SPRINGCARD DOES NOT WARRANT THAT THE FUNCTIONS CONTAINED IN
 *   THIS SOFTWARE WILL MEET THE USER'S REQUIREMENTS OR THAT THE
 *   OPERATION OF IT WILL BE UNINTERRUPTED OR ERROR-FREE.
 *
 *   IN NO EVENT SHALL SPRINGCARD BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 *   DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 *   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 *   OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 *   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
 *   THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 *   OF SUCH DAMAGE. 
 *
 **/
/*
 *
 * CHANGELOG
 *   ECL ../../2009 : early drafts
 *   JDA 21/04/2010 : first official release
 *   JDA 20/11/2010 : improved the SCardChannel object: implemented SCardControl, exported the hCard
 *   JDA 24/01/2011 : added static DefaultReader and DefaultCardChannel to ease 'quick and dirty' development for simple applications
 *   JDA 25/01/2011 : added SCardChannel.Reconnect methods
 *   JDA 16/01/2012 : improved CardBuffer, CAPDU and RAPDU objects for robustness
 *   JDA 12/02/2012 : added the SCardReaderList object to monitor all the readers
 *   JDA 26/03/2012 : added SCARD_PCI_T0, SCARD_PCI_T1 and SCARD_PCI_RAW
 *   JDA 07/02/2012 : minor improvements
 *   JDA 02/03/2016 : added Linux/Unix portability, support for execution on Mono
 *   JDA 21/10/2016 : major refactoring, portability for different platform now using a new implementation derived from the one in pcsc-sharp
 *
 **/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using SpringCard.LibCs;
using SpringCard.PCSC.Native;
using SpringCard.PCSC.Native.Windows;
using SpringCard.PCSC.Native.Linux;
using SpringCard.PCSC.Native.MacOsX;

namespace SpringCard.PCSC
{
	/**
	 * \brief Static class that gives a direct access to PC/SC functions (SCard... provided by winscard.dll or libpcsclite)
	 *
	 **/
	public static partial class SCARD
	{
        private static ISCardApi nativeMethods;

		private const string NotImplementedOnPcscLite = "This function is not implemented on PC/SC Lite systems";

		/**
		 *
		 * \brief Tell whether the PC/SC library provides trace/debug messages using SpringCard.LibCs.Logger class. Set to false to disable.
		 *
		 * \warning Sensitive content may be leaked if the Debug level is enabled in SpringCard.LibCs.Logger.
		 * 
		 **/
		public static ILogger SCardLogger = new DummyLogger();

		public static bool UseLogger
        {
			set
            {
				if (value)
                {
					SCardLogger = new Logger();
				}
				else
                {
					SCardLogger = new DummyLogger();
				}
            }
        }

		public static void SetLogger(ILogger logger)
		{
			SCardLogger = logger;
		}

		static SCARD()
        {
            PlatformID platform = Environment.OSVersion.Platform;

            switch (platform)
            {
                case PlatformID.Unix:
                    if (UNIX.GetUnameSysName() == UNIX.OS_NAME_OSX)
                    {
                        // Mono identifies MacOSX as Unix
                        goto case PlatformID.MacOSX;
                    }
                    nativeMethods = new PCSCliteLinux();
                    break;

                case PlatformID.MacOSX:
                    nativeMethods = new PCSCliteMacOsX();
                    break;

                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    nativeMethods = new WinSCardAPI();
                    break;

                default:
                    throw new NotSupportedException("Sorry, your OS platform is not supported.");
            }
        }

		#region Constants for parameters and status

		/**
		 * \brief dwScope parameter for EstablishContext(): scope is user space. Same as SCARD_SCOPE_USER in winscard
		 */
		public const uint SCOPE_USER = 0;
		/**
		 * \brief dwScope parameter for EstablishContext(): scope is the terminal. Same as SCARD_SCOPE_TERMINAL in winscard
		 */
		public const uint SCOPE_TERMINAL = 1;
		/**
		 * \brief dwScope parameter for EstablishContext(): scope is system. Same as SCARD_SCOPE_SYSTEM in winscard
		 */
		public const uint SCOPE_SYSTEM = 2;

		/**
		 * \brief groups parameter for ListReaders(): list all readers. Same as SCARD_ALL_READERS in winscard
		 */
		public const string ALL_READERS = "SCard$AllReaders\0\0";
		/**
		 * \brief groups parameter for ListReaders(): list the readers that are not in a specific group. Same as SCARD_DEFAULT_READERS in winscard
		 */
		public const string DEFAULT_READERS = "SCard$DefaultReaders\0\0";
		/**
		 * \brief groups parameter for ListReaders(): list local readers (deprecated and unused). Same as SCARD_LOCAL_READERS in winscard
		 */
		public const string LOCAL_READERS = "SCard$LocalReaders\0\0";
		/**
		 * \brief groups parameter for ListReaders(): list system readers (deprecated and unused). Same as SCARD_SYSTEM_READERS in winscard
		 */
		public const string SYSTEM_READERS = "SCard$SystemReaders\0\0";

		/**
		 * \brief share mode parameter for Connect(): take an exclusive access to the card. Same as SCARD_SHARE_EXCLUSIVE in winscard
		 */
		public const uint SHARE_EXCLUSIVE = 1;
		/**
		 * \brief share mode parameter for Connect(): accept to share the access to the card. Same as SCARD_SHARE_SHARED in winscard
		 */
		public const uint SHARE_SHARED = 2;
		/**
		 * \brief share mode parameter for Connect(): take a direct access to the reader (even if there is no card in the reader). Same as SCARD_SHARE_DIRECT in winscard
		 */
		public const uint SHARE_DIRECT = 3;

		/**
		 * \brief protocol parameter for Connect() and Status(): no active protocol (no card, direct access to the reader). Same as SCARD_PROTOCOL_UNSET in winscard
		 */
		public const uint PROTOCOL_NONE = 0;
		/**
		 * \brief protocol parameter for Connect() and Status(): protocol is T=0. Same as SCARD_PROTOCOL_T0 in winscard
		 */
		public const uint PROTOCOL_T0 = 1;
		/**
		 * \brief protocol parameter for Connect() and Status(): protocol is T=1. Same as SCARD_PROTOCOL_T1 in winscard
		 */
		public const uint PROTOCOL_T1 = 2;
		/**
		 * \brief protocol parameter for Connect() and Status(): protocol is RAW. Same as SCARD_PROTOCOL_RAW in winscard
		 */
		public const uint PROTOCOL_RAW = 4;

        /**
		 * \brief protocol parameter for Connect() and Status(): protocol is either NONE or RAW, depending on the operating system (NONE on Windows and Linux, RAW on Mac)
		 */
        public static uint PROTOCOL_DIRECT()
        {
            if (nativeMethods.IsWindows)
            {
                return PROTOCOL_NONE;
            }
            else
            {
                return PROTOCOL_RAW;
            }
        }

        /**
		 * \brief disposition parameter for Disconnect() and Reconnect(): leave the card as is. Same as SCARD_LEAVE_CARD in winscard
		 */
        public const uint LEAVE_CARD = 0;
		/**
		 * \brief disposition parameter for Disconnect() and Reconnect(): warm reset the card. Same as SCARD_RESET_CARD in winscard
		 */
		public const uint RESET_CARD = 1;
		/**
		 * \brief disposition parameter for Disconnect() and Reconnect(): power down the card. Same as SCARD_UNPOWER_CARD in winscard
		 */
		public const uint UNPOWER_CARD = 2;
		/**
		 * \brief disposition parameter for Disconnect() and Reconnect(): power down the card, and eject it in case of a motorized reader. Same as SCARD_EJECT_CARD in winscard
		 */
		public const uint EJECT_CARD = 3;

		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): no flag set. Same as SCARD_STATE_UNAWARE in winscard
		 */
		public const uint STATE_UNAWARE = 0x00000000;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): no information required. Same as SCARD_STATE_IGNORE in winscard
		 */
		public const uint STATE_IGNORE = 0x00000001;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the reader's state has changed since the last call. Same as SCARD_STATE_CHANGED in winscard
		 */
		public const uint STATE_CHANGED = 0x00000002;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the reader does not exist. Same as SCARD_STATE_UNKNOWN in winscard
		 */
		public const uint STATE_UNKNOWN = 0x00000004;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the reader's state is not available. Same as SCARD_STATE_UNAVAILABLE in winscard
		 */
		public const uint STATE_UNAVAILABLE = 0x00000008;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): there is no card in the reader. Same as SCARD_STATE_EMPTY in winscard
		 */
		public const uint STATE_EMPTY = 0x00000010;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): there is a card in the reader. Same as SCARD_STATE_PRESENT in winscard
		 */
		public const uint STATE_PRESENT = 0x00000020;

		public const uint STATE_ATRMATCH = 0x00000040;

		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the card in the reader is reserved for exclusive use by an application. Same as SCARD_STATE_EXCLUSIVE in winscard
		 */
		public const uint STATE_EXCLUSIVE = 0x00000080;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the card in the reader is connected by an application. Same as SCARD_STATE_INUSE in winscard
		 */
		public const uint STATE_INUSE = 0x00000100;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the card in the reader is unresponsive. Same as SCARD_STATE_MUTE in winscard
		 */
		public const uint STATE_MUTE = 0x00000200;
		/**
		 * \brief state flags for READERSTATE in GetStatusChange(): the card in the reader has been powered down. Same as SCARD_STATE_UNPOWERED in winscard
		 */
		public const uint STATE_UNPOWERED = 0x00000400;


		public const uint IOCTL_CSB6_PCSC_ESCAPE = 0x00312000;
		public const uint IOCTL_MS_CCID_ESCAPE = 0x003136B0;
		public const uint IOCTL_PCSCLITE_ESCAPE = 0x42000000 + 1;
		#endregion

		#region GetAttrib/SetAttrib
		public const uint ATTR_ATR_STRING = 0x00090303;
		public const uint ATTR_CHANNEL_ID = 0x00020110;
		public const uint ATTR_CHARACTERISTICS = 0x00060150;
		public const uint ATTR_CURRENT_BWT = 0x00080209;
		public const uint ATTR_CURRENT_CLK = 0x00080202;
		public const uint ATTR_CURRENT_CWT = 0x0008020A;
		public const uint ATTR_CURRENT_D = 0x00080204;
		public const uint ATTR_CURRENT_EBC_ENCODING = 0x0008020B;
		public const uint ATTR_CURRENT_F = 0x00080203;
		public const uint ATTR_CURRENT_IFSC = 0x00080207;
		public const uint ATTR_CURRENT_IFSD = 0x00080208;
		public const uint ATTR_CURRENT_N = 0x00080205;
		public const uint ATTR_CURRENT_PROTOCOL_TYPE = 0x00080201;
		public const uint ATTR_CURRENT_W = 0x00080206;
		public const uint ATTR_DEFAULT_CLK = 0x00030121;
		public const uint ATTR_DEFAULT_DATA_RATE = 0x00030123;
		public const uint ATTR_DEVICE_FRIENDLY_NAME = 0x7FFF0003;
		public const uint ATTR_DEVICE_IN_USE = 0x7FFF0002;
		public const uint ATTR_DEVICE_SYSTEM_NAME = 0x7FFF0003;
		public const uint ATTR_DEVICE_UNIT = 0x7FFF0001;
		public const uint ATTR_ICC_INTERFACE_STATUS = 0x00090301;
		public const uint ATTR_ICC_PRESENCE = 0x00090300;
		public const uint ATTR_ICC_TYPE_PER_ATR = 0x00090304;
		public const uint ATTR_MAX_CLK = 0x00030122;
		public const uint ATTR_MAX_DATA_RATE = 0x00030124;
		public const uint ATTR_MAX_IFSD = 0x00030125;
		public const uint ATTR_POWER_MGMT_SUPPORT = 0x00040131;
		public const uint ATTR_PROTOCOL_TYPES = 0x00030126;
		public const uint ATTR_VENDOR_IFD_SERIAL_NO = 0x00010103;
		public const uint ATTR_VENDOR_IFD_TYPE = 0x00010101;
		public const uint ATTR_VENDOR_IFD_VERSION = 0x00010102;
		public const uint ATTR_VENDOR_NAME = 0x00010100;
		#endregion

		#region Error codes
		public const uint S_SUCCESS = 0x00000000;
		public const uint F_INTERNAL_ERROR = 0x80100001;
		public const uint E_CANCELLED = 0x80100002;
		public const uint E_INVALID_HANDLE = 0x80100003;
		public const uint E_INVALID_PARAMETER = 0x80100004;
		public const uint E_INVALID_TARGET = 0x80100005;
		public const uint E_NO_MEMORY = 0x80100006;
		public const uint F_WAITED_TOO_LONG = 0x80100007;
		public const uint E_INSUFFICIENT_BUFFER = 0x80100008;
		public const uint E_UNKNOWN_READER = 0x80100009;
		public const uint E_TIMEOUT = 0x8010000A;
		public const uint E_SHARING_VIOLATION = 0x8010000B;
		public const uint E_NO_SMARTCARD = 0x8010000C;
		public const uint E_UNKNOWN_CARD = 0x8010000D;
		public const uint E_CANT_DISPOSE = 0x8010000E;
		public const uint E_PROTO_MISMATCH = 0x8010000F;
		public const uint E_NOT_READY = 0x80100010;
		public const uint E_INVALID_VALUE = 0x80100011;
		public const uint E_SYSTEM_CANCELLED = 0x80100012;
		public const uint F_COMM_ERROR = 0x80100013;
		public const uint F_UNKNOWN_ERROR = 0x80100014;
		public const uint E_INVALID_ATR = 0x80100015;
		public const uint E_NOT_TRANSACTED = 0x80100016;
		public const uint E_READER_UNAVAILABLE = 0x80100017;
		public const uint P_SHUTDOWN = 0x80100018;
		public const uint E_PCI_TOO_SMALL = 0x80100019;
		public const uint E_READER_UNSUPPORTED = 0x8010001A;
		public const uint E_DUPLICATE_READER = 0x8010001B;
		public const uint E_CARD_UNSUPPORTED = 0x8010001C;
		public const uint E_NO_SERVICE = 0x8010001D;
		public const uint E_SERVICE_STOPPED = 0x8010001E;
		public const uint E_UNEXPECTED = 0x8010001F;
		public const uint E_ICC_INSTALLATION = 0x80100020;
		public const uint E_ICC_CREATEORDER = 0x80100021;
		public const uint E_UNSUPPORTED_FEATURE = 0x80100022;
		public const uint E_DIR_NOT_FOUND = 0x80100023;
		public const uint E_FILE_NOT_FOUND = 0x80100024;
		public const uint E_NO_DIR = 0x80100025;
		public const uint E_NO_FILE = 0x80100026;
		public const uint E_NO_ACCESS = 0x80100027;
		public const uint E_WRITE_TOO_MANY = 0x80100028;
		public const uint E_BAD_SEEK = 0x80100029;
		public const uint E_INVALID_CHV = 0x8010002A;
		public const uint E_UNKNOWN_RES_MNG = 0x8010002B;
		public const uint E_NO_SUCH_CERTIFICATE = 0x8010002C;
		public const uint E_CERTIFICATE_UNAVAILABLE = 0x8010002D;
		public const uint E_NO_READERS_AVAILABLE = 0x8010002E;
		public const uint E_COMM_DATA_LOST = 0x8010002F;
		public const uint E_NO_KEY_CONTAINER = 0x80100030;
		public const uint W_UNSUPPORTED_CARD = 0x80100065;
		public const uint W_UNRESPONSIVE_CARD = 0x80100066;
		public const uint W_UNPOWERED_CARD = 0x80100067;
		public const uint W_RESET_CARD = 0x80100068;
		public const uint W_REMOVED_CARD = 0x80100069;
		public const uint W_SECURITY_VIOLATION = 0x8010006A;
		public const uint W_WRONG_CHV = 0x8010006B;
		public const uint W_CHV_BLOCKED = 0x8010006C;
		public const uint W_EOF = 0x8010006D;
		public const uint W_CANCELLED_BY_USER = 0x8010006E;
		public const uint W_CARD_NOT_AUTHENTICATED = 0x8010006F;
		#endregion

		/**
		 * \brief .NET wrapper for PCI_T0
		 *
		 * Use this constant with Transmit() if card protocol is T=0
		 */				
		public static IntPtr PCI_T0()
		{
            return nativeMethods.PCI_T0();
        }	
		
		/**
		 * \brief .NET wrapper for PCI_T1
		 *
		 * Use this constant with Transmit() if card protocol is T=1
		 */				
		public static IntPtr PCI_T1()
		{
            return nativeMethods.PCI_T1();
        }

		/**
		 * \brief .NET wrapper for PCI_RAW
		 *
		 * Use this constant with Transmit() if card protocol is RAW
		 */		
		public static IntPtr PCI_RAW()
		{
            return nativeMethods.PCI_RAW();
        }

		#region Static methods, provided by the 'native' WINSCARD library


		/**
		 * \brief .NET wrapper for SCardEstablishContext
		 *
		 * The application shall open a resource manager context for every thread, and must call ReleaseContext for all open contexts when exiting.
		 */
		public static uint EstablishContext(uint dwScope,
											IntPtr pvReserved1,
											IntPtr pvReserved2,
											out IntPtr phContext)
		{
            return (uint) nativeMethods.EstablishContext(dwScope, pvReserved1, pvReserved2, out phContext);
		}

        /**
		 *
		 * \brief .NET wrapper for SCardReleaseContext
		 *
		 */
        public static uint ReleaseContext(IntPtr hContext)
		{
            return (uint)nativeMethods.ReleaseContext(hContext);
		}

		/**
		 *
		 * \brief Internal structure used by GetStatusChange() (manipulate with care)
		 *
		 **/
		public struct READERSTATE
		{
			public string szReader;
			public IntPtr pvUserData;
			public uint dwCurrentState;
			public uint dwEventState;
			public uint cbAtr;
			public byte[] rgbAtr;
		}

		/**
		 * \brief .NET wrapper for SCardGetStatusChange
		 * 
		 * \details
		 *  This function blocks the execution of the thread until the state of a reader changes, or a timeout occurs.
		 */
		public static uint GetStatusChange(IntPtr hContext, uint dwTimeout, READERSTATE[] rgReaderStates, uint cReaders)
		{
            return (uint) nativeMethods.GetStatusChange(hContext, dwTimeout, rgReaderStates, cReaders);
		}

        public static uint GetStatusChange(IntPtr hContext, uint dwTimeout, READERSTATE[] rgReaderStates)
        {
            return (uint)nativeMethods.GetStatusChange(hContext, dwTimeout, rgReaderStates, (uint) rgReaderStates.Length);
        }

        /**
		 *
		 * \brief .NET wrapper for SCardCancel
		 *
		 **/
        public static uint Cancel(IntPtr hContext)
		{
            return (uint)nativeMethods.Cancel(hContext);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardConnect
		 *
		 **/
		public static uint Connect(IntPtr hContext,
								   string szReaderName,
								   uint dwShareMode,
								   uint dwPreferredProtocols,
								   out IntPtr phCard,
								   out uint pdwActiveProtocol)
		{
            return (uint)nativeMethods.Connect(hContext, szReaderName, dwShareMode, dwPreferredProtocols, out phCard, out pdwActiveProtocol);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardReconnect
		 *
		 **/
		public static uint Reconnect(IntPtr hCard,
                                     uint dwShareMode,
									 uint dwPreferredProtocols,
                                     uint dwInitialization,
									 out uint pdwActiveProtocol)
		{
            return (uint)nativeMethods.Reconnect(hCard, dwShareMode, dwPreferredProtocols, dwInitialization, out pdwActiveProtocol);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardDisconnect
		 *
		 **/
		public static uint Disconnect(IntPtr hCard, uint dwDisposition)
		{
            return (uint)nativeMethods.Disconnect(hCard, dwDisposition);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardStatus
		 *
		 **/
		public static uint Status(IntPtr hCard,
                                  out string[] szReaderNames,
								  out uint pdwState,
								  out uint pdwProtocol,
								  out byte[] pbAtr)
		{
            return (uint)nativeMethods.Status(hCard, out szReaderNames, out pdwState, out pdwProtocol, out pbAtr);
		}

        /**
		 *
		 * \brief .NET wrapper for SCardStatus - This version is faster for it does not have to retrieve the name of the reader
		 *
		 **/
        public static uint Status(IntPtr hCard,
                                  out uint pdwState,
                                  out uint pdwProtocol,
                                  out byte[] pbAtr)
        {
            return (uint)nativeMethods.Status(hCard, out pdwState, out pdwProtocol, out pbAtr);
        }

        /**
		 *
		 * \brief .NET wrapper for SCardTransmit
		 *
		 **/
        public static uint Transmit(IntPtr hCard, IntPtr pioSendPci,
									byte[] pbSendBuffer,
									uint cbSendLength,
									IntPtr pioRecvPci,
									[In, Out] byte[] pbRecvBuffer,
									[In, Out] ref uint pcbRecvLength)
		{
            return (uint)nativeMethods.Transmit(hCard, pioSendPci, pbSendBuffer, cbSendLength, pioRecvPci, pbRecvBuffer, ref pcbRecvLength);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardGetAttrib
		 *
		 **/
		public static uint GetAttrib(IntPtr hCard,
                                   uint dwAttrId,
								   [In, Out] byte[] pbAttr,
								   [In, Out] ref uint pcbAttrLength)
		{
            return (uint)nativeMethods.GetAttrib(hCard, dwAttrId, pbAttr, ref pcbAttrLength);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardControl
		 *
		 **/
		public static uint Control(IntPtr hCard,
                                   uint dwControlCode,
								   [In] byte[] lpInBuffer,
								   uint cbInBufferSize,
								   [In, Out] byte[] lpOutBuffer,
								   uint cbOutBufferSize,
								   out uint lpBytesReturned)
		{
            return (uint)nativeMethods.Control(hCard, dwControlCode, lpInBuffer, cbInBufferSize, lpOutBuffer, cbOutBufferSize, out lpBytesReturned);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardBeginTransaction
		 *
		 **/
		public static uint BeginTransaction(IntPtr hCard)
		{
            return (uint)nativeMethods.BeginTransaction(hCard);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardEndTransaction
		 *
		 **/
		public static uint EndTransaction(IntPtr hCard, uint dwDisposition)
		{
            return (uint)nativeMethods.EndTransaction(hCard, dwDisposition);
		}
		#endregion


		#region Static methods - ATR database

		/**
		 *
		 * \brief .NET wrapper for SCardListCards (Windows only)
		 *
		 **/		
		public static uint ListCards(IntPtr hContext, byte[] pbAtr, byte[] rgguiInterfaces, uint cguidInterfaceCount, string mszCards, ref int pcchCards)
		{
			return (uint)nativeMethods.ListCards(hContext, pbAtr, rgguiInterfaces, cguidInterfaceCount, mszCards, ref pcchCards);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardIntroduceCardType (Windows only)
		 *
		 **/				
		public static uint IntroduceCardType(IntPtr hContext, string szCardName, byte[] pguidPrimaryProvider, byte[] rgguidInterfaces, uint dwInterfaceCount, byte[] atr, byte[] pbAtrMask, uint cbAtrLen)
		{
            return (uint)nativeMethods.IntroduceCardType(hContext, szCardName, pguidPrimaryProvider, rgguidInterfaces, dwInterfaceCount, atr, pbAtrMask, cbAtrLen);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardSetCardTypeProviderName (Windows only)
		 *
		 **/		
		public static uint SetCardTypeProviderName(IntPtr hContext, string szCardName, uint dwProviderId, string szProvider)
		{
            return (uint)nativeMethods.SetCardTypeProviderName(hContext, szCardName, dwProviderId, szProvider);
		}

		#endregion

		#region Static methods - easy access to the list of readers

		public static uint ListReaders(IntPtr hContext, string Group, out string[] ReaderNames)
		{
            return (uint)nativeMethods.ListReaders(hContext, new string[] { Group }, out ReaderNames);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * \deprecated See ListReaders()
		 *
		 */
		public static string[] GetReaderList(IntPtr hContext, string Groups = null)
		{
			return ListReaders(hContext, Groups);
		}

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * \deprecated See ListReaders()
		 *
		 */
		public static string[] GetReaderList(uint Scope, string Groups)
		{
			return ListReaders(Scope, Groups);
		}
		
		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * \deprecated See ListReaders()
		 *
		 */
		public static string[] GetReaderList()
		{
			return ListReaders();
		}

        /**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * A valid hContext must be supplied - see EstablishContext()
		 *
		 */
        public static string[] ListReaders(IntPtr hContext)
        {
            long rc = nativeMethods.ListReaders(hContext, null, out string[] readers);
            if (rc != S_SUCCESS)
                return new string[0];
            return readers;
        }

        public static string[] ListReaders(IntPtr hContext, string Groups)
        {
            long rc = nativeMethods.ListReaders(hContext, new string[] { Groups }, out string[] readers);
            if (rc != S_SUCCESS)
                return new string[0];
            return readers;
        }

        public static string[] ListReaders(IntPtr hContext, string[] Groups)
        {
            long rc = nativeMethods.ListReaders(hContext, Groups, out string[] readers);
            if (rc != S_SUCCESS)
                return new string[0];
            return readers;
        }

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * A temporary context is created in the specified Scope
		 *
		 */		
		public static string[] ListReaders(uint Scope, string Groups)
		{
			long rc = nativeMethods.EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, out IntPtr hContext);
			if (rc != S_SUCCESS)
			{
				SCARD.SCardLogger.warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
				return new string[0];
			}

			string[] readers = GetReaderList(hContext, Groups);

			ReleaseContext(hContext);

			return readers;
		}

		/**
		 *
		 * \brief .NET wrapper for SCardListReaders
		 *
		 * A temporary context is created in the system scope
		 *
		 */		
		public static string[] ListReaders(string Groups = null)
		{
			return ListReaders(SCARD.SCOPE_SYSTEM, Groups);
		}

		/**
		 *
		 * \brief Alias for ListReaders()
		 *
		 */
		public static string[] Readers
		{
			get
			{
				return GetReaderList();
			}
		}

        /**
		 *
		 * \brief Does a reader exist?
		 *
		 */
        public static bool ReaderExists(string ReaderName)
        {
            string[] readers = ListReaders();
            if (readers != null)
            {
                foreach (string reader in readers)
                {
                    if (reader == ReaderName)
                        return true;
                }
            }
            return false;
        }

        /**
		 *
		 * \brief Find a reader given a search pattern
		 *
		 */
        public static string ReaderLike(string[] ReaderNames, string SearchPattern)
        {
            if ((ReaderNames == null) || (ReaderNames.Length == 0))
                return null;
            if ((SearchPattern == null) || (SearchPattern.Trim() == ""))
                return ReaderNames[0];

            SearchPattern = "^" + Regex.Escape(SearchPattern).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            var regexp = new Regex(SearchPattern, RegexOptions.IgnoreCase);
            foreach (string reader in ReaderNames)
                if (regexp.IsMatch(reader))
                    return reader;

            return null;
        }

        /**
		 *
		 * \brief Find a reader given a search pattern
		 *
		 */
        public static string ReaderLike(string SearchPattern)
        {
            return ReaderLike(ListReaders(), SearchPattern);
        }

        #endregion

        /**
		 *
		 * \brief .NET wrapper for SCardListReadersWithDeviceInstanceId (Windows only)
		 *
		 **/
        public static uint ListReadersWithDeviceInstanceId(IntPtr hContext, string InstanceId, out string[] ReaderNames)
		{
            return (uint)nativeMethods.ListReadersWithDeviceInstanceId(hContext, InstanceId, out ReaderNames);
        }

        /**
		 *
		 * \brief .NET wrapper for SCardGetReaderDeviceInstanceId (Windows only)
		 *
		 **/
        public static uint GetReaderDeviceInstanceId(IntPtr hContext, string ReaderName, out string InstanceId)
        {
            return (uint)nativeMethods.GetReaderDeviceInstanceId(hContext, ReaderName, out InstanceId);
        }

        /**
		 *
		 * \brief .NET wrapper for SCardListReadersWithDeviceInstanceId (Windows only)
		 *
		 **/
        public static string[] GetReaderListWithDeviceInstanceId(IntPtr hContext, string InstanceId)
        {
            uint rc = (uint)nativeMethods.ListReadersWithDeviceInstanceId(hContext, InstanceId, out string[] result);
            if (rc != S_SUCCESS)
                return new string[0];
            return result;
		}

		public static string[] GetReaderListWithDeviceInstanceId(uint Scope, string InstanceId)
		{
			uint rc = EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, out IntPtr hContext);
			if (rc != S_SUCCESS)
			{
				SCARD.SCardLogger.warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
                return new string[0];
            }

			string[] result = GetReaderListWithDeviceInstanceId(hContext, InstanceId);

			ReleaseContext(hContext);

			return result;
		}

		public static string[] GetReaderListWithDeviceInstanceId(string InstanceId)
		{
			return GetReaderListWithDeviceInstanceId(SCOPE_SYSTEM, InstanceId);
		}

        /**
		 *
		 * \brief .NET wrapper for SCardGetReaderDeviceInstanceId (Windows only)
		 *
		 **/
        public static string GetReaderDeviceInstanceId(IntPtr hContext, string ReaderName)
        {
            uint rc = (uint)nativeMethods.GetReaderDeviceInstanceId(hContext, ReaderName, out string result);
            if (rc != S_SUCCESS)
            {
				SCARD.SCardLogger.trace("SCardGetReaderDeviceInstanceId failed with error " + SCARD.ErrorToMessage(rc));
                return null;
            }
            return result;
        }

        public static Dictionary<string, string> ListReadersWithInstanceId(IntPtr hContext, string Groups = null)
        {
            string[] readerNames = ListReaders(hContext, Groups);
            if (readerNames == null)
                return null;
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string readerName in readerNames)
            {
                result[readerName] = GetReaderDeviceInstanceId(hContext, readerName);
            }
            return result;
        }

        public static Dictionary<string, string> ListReadersWithInstanceId(uint Scope, string Groups = null)
        {
            uint rc = EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, out IntPtr hContext);
            if (rc != S_SUCCESS)
            {
				SCARD.SCardLogger.warning("SCardEstablishContext failed with error " + ErrorToMessage(rc));
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> result = ListReadersWithInstanceId(hContext, Groups);

            ReleaseContext(hContext);

            return result;
        }

        public static Dictionary<string, string> ListReadersWithInstanceId(string Groups = null)
        {
            return ListReadersWithInstanceId(SCOPE_SYSTEM, Groups);
        }

        public static Dictionary<string, string> ListReadersByInstanceId(IntPtr hContext, string Groups = null)
        {
            string[] readerNames = ListReaders(hContext, Groups);
            if (readerNames == null)
                return null;
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string readerName in readerNames)
            {
                string deviceId = GetReaderDeviceInstanceId(hContext, readerName);
                if (result.ContainsKey(deviceId))
                {
                    result[deviceId] += "|";
                    result[deviceId] += readerName;
                }
                else
                {
                    result[deviceId] = readerName;
                }
            }
            return result;
        }

        public static Dictionary<string, string> ListReadersByInstanceId(uint Scope, string Groups = null)
        {
            uint rc = EstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, out IntPtr hContext);
            if (rc != S_SUCCESS)
            {
				SCARD.SCardLogger.warning("SCardEstablishContext failed with error " + SCARD.ErrorToMessage(rc));
                return new Dictionary<string, string>();
            }

            Dictionary<string, string> result = ListReadersByInstanceId(hContext, Groups);

            ReleaseContext(hContext);

            return result;
        }

        public static Dictionary<string, string> ListReadersByInstanceId(string Groups = null)
        {
            return ListReadersByInstanceId(SCARD.SCOPE_SYSTEM, Groups);
        }

        internal static class Helpers
        {
            public static int ToSCardError(IntPtr rc)
            {
                return (int) (unchecked((int)rc.ToInt64()));
            }

            public static int ToSCardError(int rc)
            {
                return rc;
            }

            public static byte[] ConvertToByteArray(string str, Encoding encoder, int suffixByteCount)
            {
                if (str == null)
                {
                    return null;
                }

                if (suffixByteCount == 0)
                {
                    return encoder.GetBytes(str);
                }

                var count = encoder.GetByteCount(str);
                var tmp = new byte[count + suffixByteCount];
                encoder.GetBytes(str, 0, str.Length, tmp, 0);
                return tmp;
            }

            public static byte[] ConvertToByteArray(IEnumerable<string> array, Encoding encoder)
            {
                var lst = new List<byte>();

                foreach (var s in array)
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        continue;
                    }

                    var encstr = encoder.GetBytes(s);
                    lst.AddRange(encstr);
                    lst.Add(0);
                }

                lst.Add(0);

                return lst.ToArray();
            }

            internal static string[] ConvertToStringArray(byte[] array, Encoding decoder)
            {
                if (array == null)
                {
                    return null;
                }

                var tmp = decoder.GetString(array);
                return RemoveEmptyStrings(tmp.Split('\0'));
            }

            internal static string ConvertToString(byte[] array, Encoding decoder)
            {
                string[] s = ConvertToStringArray(array, decoder);
                return s[0];
            }

            private static string[] RemoveEmptyStrings(string[] array)
            {
                List<string> result = new List<string>();
                foreach (string s in array)
                    if (!string.IsNullOrEmpty(s))
                        result.Add(s);
                return result.ToArray();
            }
        }
    }
}

