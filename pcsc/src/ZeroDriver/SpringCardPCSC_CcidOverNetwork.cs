/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Threading;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.PCSC.ZeroDriver
{
    public class SCardReaderList_CcidOverNetwork : SCardReaderList_CcidOver
    {
        public const int MAX_SLOT = 8;
        public const int PROBE_INTERVAL = 5;
        public readonly object locker = new object();
        private ManualResetEvent syncExchange = new ManualResetEvent(false);
        protected Device tcpDevice;
        protected Thread asyncThread;
        private Logger logger;

        #region trace

        /** \brief Device state */

        public enum TcpDeviceState
        {
            Initializing,   /* the reader is being initialized */
            Missing,        /* The reader is not found in the nearby */
            Present,        /* The reader is present, but not connected yet */
            Lost,           /* The reader is/was present, but we loose connexion to driver or to reader */
            Error,          /* The reader is present, but the library has failed to connect it */
            Connecting,     /* The reader is found, the library is trying to connect it */
            Connected,      /* The library is connected to the reader */
            Idle,          /* The library is connected to the reader and the reader is suspended */
            WakeUp         /* The library is connected to the reader and the reader is wake up */
            //Disconnected
        }
        protected TcpDeviceState tcpDeviceState;

        public class Device : IDisposable
        {
            public delegate void DisconnectCallback(Device device);
            private DisconnectCallback OnDisconnect;
            public delegate void NotificationCallback(Device device, byte uuid, byte[] value);
            private NotificationCallback OnNotification;

            TcpClient clientSocket;
            NetworkStream clientStream;
            Thread receiverThread;

            private readonly object sync_request = new object();

            private byte[] recv_buffer;
            private ManualResetEvent syncEventWriteRead = new ManualResetEvent(false);
            private byte _endpoint;
            private bool _processing_cmd = false;
            private byte[] _cmde = new byte[13];
            private string ip_ID;
            private ushort port_ID;
            private DateTime _LastTimeDriverTransmit = DateTime.UtcNow;
            private enum State { Stop, Run };
            private State readerState;

            private Logger logger;

#region dispose
            bool disposed = false;
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected virtual void Dispose(bool disposing)
            {
                if (disposed)
                    return;

                // Note there are three interesting states here:
                // 1) CreateFile failed, _handle contains an invalid handle
                // 2) We called Dispose already, _handle is closed.
                // 3) _handle is null, due to an async exception before
                //    calling CreateFile. Note that the finalizer runs
                //    if the constructor fails.
                if (syncEventWriteRead != null)
                {
                    syncEventWriteRead.Close();
                    syncEventWriteRead = null;
                }
                if (clientStream != null)
                {
                    clientStream.Dispose();
                    clientStream = null;
                }
                if (clientSocket != null)
                {
                    clientSocket.Dispose();
                    clientSocket = null;
                }


                disposed = true;

            }
#endregion //dispose


            public void SetCallbacks(DisconnectCallback onDeviceDisconnect, NotificationCallback onDeviceNotification)
            {
                if (onDeviceDisconnect != null)
                    logger.trace("Registering Device Disconnected callback");
                this.OnDisconnect = onDeviceDisconnect;

                if (onDeviceNotification != null)
                    logger.trace("Registering Device Notification callback");
                this.OnNotification = onDeviceNotification;
            }
            public bool Connected { get; protected set; }
            public void ReportDisconnect()
            {
                OnDisconnect?.Invoke(this);
                Connected = false;
            }

            protected void ReportNotification(byte uuid, byte[] buffer)
            {
                OnNotification?.Invoke(this, uuid, buffer);
            }

            public Device()//string HostName, ushort TcpPort = 3999)
            {

            }

            public bool Connect(string HostName, DisconnectCallback onDeviceDisconnect, NotificationCallback onDeviceNotification, ushort TcpPort = 3999)
            {
                logger = new Logger(this.GetType().FullName, HostName);

                int TimeoutMs = 5000;
                SetCallbacks(onDeviceDisconnect, onDeviceNotification);
                try
                {
                    logger.trace("Creating client socket...");
                    clientSocket = new System.Net.Sockets.TcpClient();
                    IAsyncResult ar;
                    if (!string.IsNullOrEmpty(HostName))
                    {
                        logger.trace(string.Format("Connecting to host {0}, port {1}...", HostName, TcpPort));
                        ar = clientSocket.BeginConnect(HostName, TcpPort, null, null);
                    }
                    else if (HostName != null)
                    {
                        logger.trace(string.Format("Connecting to address {0}, port {1}...", HostName, TcpPort));
                        ar = clientSocket.BeginConnect(HostName, TcpPort, null, null);
                    }
                    else
                    {
                        logger.trace("Can't connect: no host and no address");
                        return false;
                    }

                    logger.trace(string.Format("Waiting for connection, timeout={0}...", TimeoutMs));
                    bool r = ar.AsyncWaitHandle.WaitOne(TimeoutMs, false);
                    logger.trace(string.Format("Done waiting, result={0}", r));

                    if (!r)
                    {
                        logger.trace("Failed to connect to remote TCP server");
                        clientSocket.Dispose();
                        return false;
                    }

                    logger.trace("End connect...");
                    clientSocket.EndConnect(ar);

                    if (!clientSocket.Connected)
                    {
                        logger.trace("State of TCP client socket is invalid");
                        return false;
                    }

                    //linkConnectionId = clientSocket.Client.RemoteEndPoint.ToString();

                    //logger.trace("Connected to {0}", linkConnectionId);
                    clientStream = clientSocket.GetStream();
                    logger.trace("Starting client recv thread");
                    receiverThread = new Thread(ReceiverThread);
                    receiverThread.Start();
                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    logger.warning(string.Format("Connection failed ({0})", e.Message));
                    return false;
                }

                ip_ID = HostName;
                port_ID = TcpPort;
                return true;
            }

            public void Disconnect(bool bKillTask)
            {
                logger.trace("Device: Disconnect : ");
                try
                {
                    if (bKillTask)
                    {
                        if (receiverThread != null)
                        {
                            logger.trace("Tcp: Stopping the receiver thread");
                            receiverThread.Interrupt();
                            /* send the command to wakeup thread and interrupt it */
                            //Send(t, 10000, false);
                            receiverThread.Join(1000);
                            logger.trace("Tcp: Receiver thread stopped.");
                            receiverThread = null;
                        }
                    }
                    if (clientStream != null)
                    {
                        logger.debug("Tcp: closing the stream");
                        clientStream.Close();
                        clientStream = null;
                    }
                    if (clientSocket != null)
                    {
                        logger.debug("Tcp: closing the socket");
                        clientSocket.Close();
                        clientSocket = null;
                    }
                }
                catch (Exception e)
                {
                    logger.warning("Exception in Disconnect: " + e.Message);
                }
            }
#if _1
            public void Disconnect(bool bKillTask)
            {
                logger.trace("Device: Disconnect : ");
                try
                {
                    if (bKillTask)
                    {
                        if (receiverThread != null)
                        {
                            logger.trace("Tcp: Stopping the receiver thread");
                            receiverThread.Interrupt();
                            /* send the command to wakeup thread and interrupt it */
                            //Send(t, 10000, false);
                            receiverThread.Join(1000);
                            logger.trace("Tcp: Receiver thread stopped.");
                            receiverThread = null;
                        }
                    }
                    readerState = State.Stop;
                    if (clientSocket != null)
                    {
                        logger.trace("Tcp: Closing the socket");
                        clientSocket.Close();
                        clientSocket = null;
                    }
                }
                catch (Exception e)
                {
                    logger.warning("Exception in Disconnect: " + e.Message);
                }
            }
#endif
            public bool DeviceState
            {
                get
                {
                    if (readerState == State.Run)
                        return true;

                    return false;
                }
            }

#region read_thread
            private bool IsBufferComplete(byte[] buffer, out int deal)
            {
                deal = 0;
                if ((buffer == null) || (buffer.Length < 11))
                {
                    logger.trace("Tcp: IsBufferComplete Lack Error buffer.Length: " + buffer.Length);
                    return false;
                }

                /* byte 0 is endpoint */
                /* byte 1 is message code */
                /* Length of data is on bytes 2 to 4 */
                int dataLength;
                dataLength = buffer[5]; dataLength *= 0x00000100;
                dataLength += buffer[4]; dataLength *= 0x00000100;
                dataLength += buffer[3]; dataLength *= 0x00000100;
                dataLength += buffer[2];

                if ((dataLength < 0) || (dataLength > 0x00010000))
                {
                    deal = -1;
                    logger.trace("Tcp: IsBufferComplete Error buffer.Length: " + buffer.Length + " dataLength: " + dataLength);
                    return true;
                }

                if (buffer.Length == (11 + dataLength))
                {
                    return true;
                }

                /* two commands inside the same trame */
                if (buffer.Length > (11 + dataLength))
                {
                    deal = (11 + dataLength);
                    return true;
                }

                return false;
            }

            protected bool CheckRecv(byte[] buffer, out int deal_done)
            {
                deal_done = 0;

                if (IsBufferComplete(buffer, out deal_done))
                {
                    /* means crazy size from ccid command */
                    if (deal_done == -1)
                    {
                        return true;
                    }
                    return true;
                }
                return false;
            }

            void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
            {
                Exception e = (Exception)args.ExceptionObject;
                Console.WriteLine("Exception {0} ({1})", e.Message, e.GetType().ToString());
                Console.WriteLine("Source: {0}", e.Source);
                Console.WriteLine("Methode: {0}", e.TargetSite);
                Console.WriteLine("Stacktrace:");
                foreach (string s in e.StackTrace.Split('\n'))
                    if (!string.IsNullOrWhiteSpace(s))
                        Console.WriteLine("\t{0}", s);
                Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
            }
            private void ReceiverThread()
            {
                int done = -1;
                int bytesRead = 0;
                int offset = 0;
                int idx = 0;
                int jdx = 0;

                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);

                recv_buffer = new byte[65536 + 34];

                readerState = State.Run;
                for (; ; )
                {
                    try
                    {
                        bytesRead = clientStream.Read(recv_buffer, offset, (recv_buffer.Length - offset));
                        if (bytesRead <= 0)
                        {
                            try
                            {
                                Thread.Sleep(10);
                            }
                            catch (ThreadInterruptedException e)
                            {
                                logger.trace("[Socket Recv] - interrupted ({0})", e.Message);
                                break;
                            }
                            continue;
                        }
                        if (!clientSocket.Connected)
                        {
                            try
                            {
                                Thread.Sleep(10);
                            }
                            catch (ThreadInterruptedException e)
                            {
                                logger.trace("[Socket Recv] - interrupted ({0})", e.Message);
                                break;
                            }

                            continue;
                        }
                        offset += bytesRead;

                        /* filter garbage character, get endpoint field header */
                        for (idx = 0; idx < offset; idx++)
                        {
                            if ((recv_buffer[idx] == CCID.EP_Control_To_PC) ||
                                (recv_buffer[idx] == CCID.EP_Bulk_RDR_To_PC) ||
                                (recv_buffer[idx] == CCID.EP_Interrupt))
                            {
                                break;
                            }
                        }

                        if (idx == offset)
                        {
                            offset = 0;
                            logger.trace("[Socket Recv] EP header is absent.");
                            continue;
                        }
                        /* get ccid cmde header */
                        for (jdx = idx + 1; jdx < offset; jdx++)
                        {
                            if ((recv_buffer[jdx] == CCID.GET_STATE) ||
                                (recv_buffer[jdx] == CCID.GET_DESCRIPTOR) ||
                                (recv_buffer[jdx] == CCID.SET_CONFIGURATION) ||
                                (recv_buffer[jdx] == CCID.RDR_TO_PC_NOTIFYSLOTCHANGE) ||
                                (recv_buffer[jdx] == CCID.RDR_TO_PC_DATABLOCK) ||
                                (recv_buffer[jdx] == CCID.RDR_TO_PC_SLOTSTATUS) ||
                                (recv_buffer[jdx] == 0x82) ||
                                (recv_buffer[jdx] == CCID.RDR_TO_PC_ESCAPE) ||
                                (recv_buffer[jdx] == 0x84))
                            {
                                break;
                            }
                        }

                        if ((jdx == offset) && (jdx > (idx + 1)))
                        {
                            offset = 0;
                            logger.warning("[Socket Recv] Ccid header is absent.");
                            AbortExchange();
                            ReportDisconnect();
                            break;
                        }

                        /* remove ep descriptor */
                        byte[] result = BinUtils.Copy(recv_buffer, 0, offset);

                        bool bFull = CheckRecv(result, out done);
                        if (bFull == false)
                        {
                            byte[] result1 = BinUtils.Copy(recv_buffer, 0, offset);
                            //logger.trace("[Socket Recv] wait rest of command>" + result1.Length);
                        }
                        else
                        {
                            if (done == 0)
                            {
                                offset = 0;
                                verbose_recv(result);

                                ReportNotification(result[0], BinUtils.Copy(result, 1, (result.Length - 1)));

                                /* notify slot change method is not concerned */
                                if ((result[0] != CCID.RDR_TO_PC_ESCAPE) && (result[1] != CCID.RDR_TO_PC_NOTIFYSLOTCHANGE))
                                {
                                    syncEventWriteRead.Set();
                                }
                                /* BUG ok only for one slot */
                                else if ((result[0] == CCID.RDR_TO_PC_ESCAPE) && (result[1] == CCID.RDR_TO_PC_NOTIFYSLOTCHANGE))
                                {
                                    /* force stop exchange when card is ejected */
                                    if (_processing_cmd == true)
                                    {
                                        /* 835001000000000000000003 */
                                        if (result.Length == 12)
                                        {
                                            if (result[11] == 0x02)
                                            {
                                                if (syncEventWriteRead.WaitOne(0) == false)
                                                {
                                                    logger.trace("[Socket Recv] AA Abort Write/Read sequence.");
                                                    
                                                    AbortExchange();
                                                    _processing_cmd = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (done > 0)
                            {
                                logger.trace("[Socket Recv] Write/Read synchronisation error between service and reader.");
                                //
                                logger.trace("[Socket Recv] < " + BinConvert.ToHex(result));
                                int idx_start = 0;
                                int idx_end = 0;
                                int dataLength;

                                for (idx = 0; idx < result.Length; idx++)
                                {
                                    if ((result[idx] == CCID.RDR_TO_PC_DATABLOCK) ||
                                         (result[idx] == CCID.RDR_TO_PC_SLOTSTATUS) ||
                                         (result[idx] == CCID.RDR_TO_PC_ESCAPE) ||
                                         (result[idx] == CCID.GET_DESCRIPTOR) ||
                                         (result[idx] == CCID.SET_CONFIGURATION) ||
                                         (result[idx + 1] == 0x00))
                                    {
                                        idx_start = idx;

                                        dataLength = result[idx + 5]; dataLength *= 0x00000100;
                                        dataLength += result[idx + 4]; dataLength *= 0x00000100;
                                        dataLength += result[idx + 3]; dataLength *= 0x00000100;
                                        dataLength += result[idx + 2];

                                        idx_end = (idx_start + dataLength + 10) + 1;
                                        ReportNotification(result[idx_start], BinUtils.Copy(result, idx_start + 1, (idx_end - idx_start) - 1));
                                        //logger.trace("[##### Socket Recv] < " + BinConvert.ToHex(RawData.CopyBuffer(result, idx_start, idx_end - idx_start)) + " " + idx_start + " " + idx_end);
                                        verbose_recv(result);

                                        if ((result[idx_start] != CCID.RDR_TO_PC_ESCAPE) && (result[idx_start + 1] != CCID.RDR_TO_PC_NOTIFYSLOTCHANGE))
                                        {
                                            syncEventWriteRead.Set();
                                        }
                                        /* BUG ok only for one slot */
                                        else if ((result[0] == CCID.RDR_TO_PC_ESCAPE) && (result[1] == CCID.RDR_TO_PC_NOTIFYSLOTCHANGE))
                                        {
                                            /* force stop exchange when card is ejected */
                                            if (_processing_cmd == true)
                                            {
                                                /* 835001000000000000000003 */
                                                if (result.Length == 12)
                                                {
                                                    if (result[11] == 0x02)
                                                    {
                                                        if ((syncEventWriteRead.WaitOne(0) == false))
                                                        {
                                                            logger.trace("[Socket Recv] BB Abort Write/Read sequence.");
                                                            
                                                            AbortExchange();
                                                            _processing_cmd = false;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        idx = idx_end - 1;
                                    }
                                }
                            }
                            else if (done < 0)
                            {
                                Logger.Error("[Socket Recv] Crazy CCID Command.");
                                offset = 0;
                            }
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        logger.trace(string.Format("[Socket Recv] Thread '{0}' awoken.", Thread.CurrentThread.Name));
                        AbortExchange();
                        ReportDisconnect();
                    }
                    catch (ThreadAbortException)
                    {
                        logger.trace(string.Format("[Socket Recv] Thread '{0}' aborted.", Thread.CurrentThread.Name));
                        AbortExchange();
                        ReportDisconnect();
                    }
                    catch (FileNotFoundException exception)
                    {
                        logger.trace($"{exception.ToString()}: {exception.Message}");

                        if (exception is System.IO.IOException)
                        {
                            logger.info("[Socket Recv] Socket has been closed");
                        }
                        else
                        {
                            logger.warning("[Socket Recv] Error in receiver thread: " + exception.Message);
                        }
                        AbortExchange();
                        ReportDisconnect();
                        break;
                    }
                    catch (Exception e)
                    {
                        if (e is System.IO.IOException)
                        {
                            logger.info("[Socket Recv] Socket has been closed");
                        }
                        else
                        {
                            logger.warning("[Socket Recv] Error in receiver thread: " + e.Message);
                        }
                        AbortExchange();
                        ReportDisconnect();
                        break;
                    }
                }
                readerState = State.Stop;
                logger.trace("[Socket Recv] Received thread ended");
                
            }
#endregion
            public bool AbortExchange()
            {
                if (_processing_cmd == true)
                {
                    logger.trace("AbortExchange " + BinConvert.ToHex(_cmde, 10));

                    syncEventWriteRead.Set();

                    force_answer(ref _cmde);

                    ReportNotification(_cmde[0], BinUtils.Copy(_cmde, 1, (_cmde.Length - 1)));
                }

                return true;
            }

            public void verbose_send(byte[] t)
            {
                if ((t[0] == 0x00) && (t[1] == 0x00))
                {
                    logger.debug("[PC_to_RDR_GetStatus]        " + BinConvert.ToHex(t));
                }
                else if ((t[0] == 0x00) && (t[1] == 0x06))
                {
                    logger.debug("[PC_to_RDR_GetDescriptor]    " + BinConvert.ToHex(t));
                }
                else if ((t[0] == 0x00) && (t[1] == CCID.SET_CONFIGURATION))
                {
                    logger.debug("[PC_to_RDR_SetConfiguration] " + BinConvert.ToHex(t));
                }
                else if (t[1] == CCID.PC_TO_RDR_ESCAPE)
                {
                    logger.debug("[PC_to_RDR_Escape]           " + BinConvert.ToHex(t));
                }
                else if (t[1] == CCID.PC_TO_RDR_GETSLOTSTATUS)
                {
                    logger.debug("[PC_to_RDR_GetSlotStatus]    " + BinConvert.ToHex(t));
                }
                else if (t[1] == CCID.PC_TO_RDR_ICCPOWEROFF)
                {
                    logger.debug("[PC_toRDR_IccPowerOff]       " + BinConvert.ToHex(t));
                }
                else if (t[1] == CCID.PC_TO_RDR_ICCPOWERON)
                {
                    logger.debug("[PC_to_RDR_IccPowerOn]       " + BinConvert.ToHex(t));
                }
                else if (t[1] == CCID.PC_TO_RDR_XFRBLOCK)
                {
                    logger.debug("[PC_to_RDR_XfrBlock]         " + BinConvert.ToHex(t));
                }
                else
                {
                    logger.warning("[PC_to_RDR_Unknow]         " + BinConvert.ToHex(t));
                }
            }
            public void verbose_recv(byte[] r)
            {
                /* RDR_to_PC_GetStatus */
                if ((r[0] == CCID.RDR_TO_PC_DATABLOCK) && (r[1] == 0x00))
                {
                    logger.debug("[RDR_to_PC_GetStatus]        " + BinConvert.ToHex(r));
                }
                else if ((r[0] == CCID.RDR_TO_PC_DATABLOCK) && (r[1] == 0x06))
                {
                    logger.debug("[RDR_to_PC_GetDescriptor]    " + BinConvert.ToHex(r));
                }
                else if ((r[0] == CCID.RDR_TO_PC_DATABLOCK) && (r[1] == CCID.SET_CONFIGURATION))
                {
                    logger.debug("[RDR_to_PC_SetConfiguration] " + BinConvert.ToHex(r));
                }
                /* RDR_to_PC_DataBlock */
                else if (r[1] == CCID.RDR_TO_PC_DATABLOCK)
                {
                    logger.debug("[RDR_to_PC_DataBlock]        " + BinConvert.ToHex(r));
                }
                /* RDR_to_PC_SlotStatus */
                else if (r[1] == CCID.RDR_TO_PC_SLOTSTATUS)
                {
                    logger.debug("[RDR_to_PC_SlotStatus]       " + BinConvert.ToHex(r));
                }
                /* RDR_to_PC_Escape */
                else if (r[1] == CCID.RDR_TO_PC_ESCAPE)
                {
                    logger.debug("[RDR_to_PC_Escape]           " + BinConvert.ToHex(r));
                }
                else if ((r[0] == CCID.RDR_TO_PC_ESCAPE) && (r[1] == CCID.RDR_TO_PC_NOTIFYSLOTCHANGE))
                {
                    logger.debug("[RDR_to_PC_NotifySlotChange] " + BinConvert.ToHex(r));
                }
                else if ((r[0] == CCID.EP_Control_To_RDR)  && (r[1] == CCID.GET_STATE))
                {
                    logger.debug("[RDR_to_PC_GetStatus] " + BinConvert.ToHex(r));
                }
                /* RDR_to_PC_Unknow */
                else
                {
                    logger.warning("[RDR_to_PC_Unknow]           " + BinConvert.ToHex(r));
                }
            }

            public void force_answer(ref byte[] t)
            {
                if (t[1] == CCID.SET_CONFIGURATION)
                {
                    /*
                     * [PC_to_RDR_SetConfiguration]0009000000000000000000
                     * [RDR_to_PC_SetConfiguration]8009000000000000000000
                     */
                    t[0] = CCID.EP_Control_To_PC;
                    t[1] = CCID.SET_CONFIGURATION;
                    t[2] = 0x00;
                    t[3] = 0x00;
                    t[4] = 0x00;
                    t[5] = 0x00;
                    t[8] = 0x00;
                    t[9] = 0x00;
                    t[10] = 0x00;
                }
                else if (t[1] == CCID.PC_TO_RDR_ESCAPE)
                {
                    /*
                     * [PC_to_RDR_Escape]6B03000000002D000000582001
                     * [RDR_to_PC_Escape]830B000000002D01000000537072696E6743617264
                     */
                    t[0] = CCID.EP_Bulk_RDR_To_PC;
                    t[1] = CCID.RDR_TO_PC_ESCAPE;
                    t[2] = 0x02;
                    t[3] = 0x00;
                    t[4] = 0x00;
                    t[5] = 0x00;
                    t[8] = 0x02;
                    t[9] = 0x00;
                    t[11] = 0x6F;
                    t[12] = 0x00;
                }
                else if (t[1] == CCID.PC_TO_RDR_GETSLOTSTATUS)
                {
                    /*
                     * [PC_to_RDR_GetSlotStatus]65000000000028000000
                     * [RDR_to_PC_SlotStatus]81000000000028000000
                     */
                    t[0] = CCID.EP_Bulk_RDR_To_PC;
                    t[1] = CCID.RDR_TO_PC_SLOTSTATUS;
                    t[8] = 0x02;
                    t[9] = 0x00;
                }
                else if (t[1] == CCID.PC_TO_RDR_ICCPOWEROFF)
                {
                    /*
                     * [PC_toRDR_IccPowerOff]63 00 00 00 00 00 2A 00 00 00
                     * [RDR_to_PC_SlotStatus]81 00 00 00 00 00 2A 01 00 02
                     */
                    t[0] = CCID.EP_Bulk_RDR_To_PC;
                    t[1] = CCID.RDR_TO_PC_SLOTSTATUS;
                    t[8] = 0x02;
                    t[9] = 0x00;

                }
                else if (t[1] == CCID.PC_TO_RDR_ICCPOWERON)
                {
                    /* [PC_to_RDR_IccPowerOn]62000000000037000000
                     * [RDR_to_PC_DataBlock]800200000000590000006A82
                     */
                    t[0] = CCID.EP_Bulk_RDR_To_PC;
                    t[1] = CCID.RDR_TO_PC_DATABLOCK;
                    t[2] = 0x02;
                    t[3] = 0x00;
                    t[4] = 0x00;
                    t[5] = 0x00;
                    t[8] = 0x02;
                    t[9] = 0x00;
                    t[11] = 0x6F;
                    t[12] = 0x00;
                }
                else if (t[1] == CCID.PC_TO_RDR_XFRBLOCK)
                {
                    /*
                     * [PC_to_RDR_XfrBlock]6F0E000000002500000000A4040009A00000030800001000
                     * [RDR_to_PC_DataBlock]800200000000250000006F00
                     */
                    t[0] = CCID.EP_Bulk_RDR_To_PC;
                    t[1] = CCID.RDR_TO_PC_DATABLOCK;
                    t[2] = 0x02;
                    t[3] = 0x00;
                    t[4] = 0x00;
                    t[5] = 0x00;
                    t[8] = 0x02;
                    t[9] = 0x00;
                    t[11] = 0x6F;
                    t[12] = 0x00;
                }
                verbose_recv(t);
            }

            public bool Send(byte[] t)
            {
                
                /* lock due to multithread possible access */
                lock (sync_request)
                {
                    //logger.trace("< " + BinConvert.ToHex(t));
                    _endpoint = t[0];
                    if (t.Length > 13)
                    {
                        Array.Copy(t, _cmde, 13);
                    }
                    else
                    {
                        Array.Copy(t, _cmde, t.Length);
                    }

                    _processing_cmd = true;

                    try
                    {
                        if ((clientSocket != null) && (clientSocket.Connected))
                        {
#if !_POP
                            verbose_send(t);
#endif
                            clientStream.Write(t, 0, t.Length);

                            /* wait answer from reader 55s inferior to driver request timeout 60s*/
#if _BEFORE
                            if (!syncEventWriteRead.WaitOne(55000))
                            {
                                logger.trace("[Socket Send] syncEventWriteRead Error");
                                syncEventWriteRead.Reset();
                                /* if no answer */
                                ReportNotification(t[0], BinUtils.Copy(t, 1, t.Length));
                                
                                return false;
                            }
#else
                            int cnt_timeout_loop = 0;
                            for (cnt_timeout_loop = 0; cnt_timeout_loop < 5500; cnt_timeout_loop++)
                            {
                                if (_processing_cmd == false)
                                {
                                    logger.trace("[Socket Send] Aborted due to card ejection");
                                    syncEventWriteRead.Reset();
                                    return true;
                                }
                                /* still no answer */
                                if (!syncEventWriteRead.WaitOne(0))
                                {
                                    try
                                    {
                                        Thread.Sleep(100);
                                    }
                                    catch (ThreadInterruptedException)
                                    {
                                        logger.trace("Thread '{0}' awoken.", Thread.CurrentThread.Name);
                                    }
                                    catch (ThreadAbortException)
                                    {
                                        logger.trace("Thread '{0}' aborted.", Thread.CurrentThread.Name);
                                    }
                                    continue;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            if (cnt_timeout_loop >= 5500)
                            {
                                logger.warning("[Socket Send] Aborted due to timeout");
                                _processing_cmd = false;
                                syncEventWriteRead.Reset();
                                /* if no answer */
                                force_answer(ref _cmde);
                                ReportNotification(_cmde[0], BinUtils.Copy(_cmde, 1, (_cmde.Length - 1)));
                                return false;
                            }
                            _processing_cmd = false;

#endif
                            syncEventWriteRead.Reset();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.warning("[Socket Send] Error: " + e.Message);
                        readerState = State.Stop;
                        return false;
                    }
                }
                return true;
            }
        }

#endregion

//#region tcp_device
        private bool _processing_cmd = false;

        public class InstantiateParams
        {
            public object InstanceTag;
            public BackgroundInstantiateCallback Callback;
            public BackgroundInstantiateCallbackEx CallbackEx;

            public Thread instantiateThread;
            public SCardReaderList_CcidOverNetwork instance;
            public string deviceAddress;
            public ushort devicePort;
        }

        public SCardReaderList_CcidOverNetwork() : base()
        {

        }
    

        public static void BackgroundInstantiate(BackgroundInstantiateCallback Callback, string Address, ushort Port = 3999)
        {
            if (Callback == null)
                return;

            InstantiateParams instantiateParams = new InstantiateParams();

            instantiateParams.Callback = Callback;
            instantiateParams.deviceAddress = Address;
            instantiateParams.devicePort = Port;

            instantiateParams.instantiateThread = new Thread(InstantiateProc);
            instantiateParams.instantiateThread.Start(instantiateParams);
        }

        public static void BackgroundInstantiate(object InstanceTag, BackgroundInstantiateCallbackEx CallbackEx, string Address, ushort Port = 3999)
        {
            if (CallbackEx == null)
                return;

            InstantiateParams instantiateParams = new InstantiateParams();

            instantiateParams.InstanceTag = InstanceTag;
            instantiateParams.CallbackEx = CallbackEx;
            instantiateParams.deviceAddress = Address;
            instantiateParams.devicePort = Port;

            instantiateParams.instantiateThread = new Thread(InstantiateProc);
            instantiateParams.instantiateThread.Start(instantiateParams);
        }

        private void TcpDeviceDisconnectCallback(Device tcpDevice)
        {
            logger.trace("TcpDeviceDisconnectCallback: disconnected");
            /* wait end of current asynchrone request */
            lock (locker)
            {
                syncExchange.Set();
                tcpDeviceState = TcpDeviceState.Missing;
            }
        }

        private void TcpDeviceNotificationCallback(Device tcpDevice, byte uuid, byte[] value)
        {
            if ((uuid == 0x83) && (value[0] == 0x50))
            {
                Recv(uuid, value);
                /* force stop exchange when card is ejected */
                if (_processing_cmd == true)
                {
                    /* 835001000000000000000003 */
                    if (value.Length == 11)
                    {
                        if (value[10] == 0x02)
                        {
                            if ((syncExchange.WaitOne(0) == false))
                            {
                                logger.trace("[Socket Recv] CC Abort Write/Read sequence.");                                
                                tcpDevice.AbortExchange();
                                _processing_cmd = false;
                            }
                        }
                    }
                }
            }
            else if ((uuid == CCID.EP_Bulk_RDR_To_PC) ||
                        (uuid == CCID.EP_Control_To_PC) ||
                        (uuid == CCID.EP_Control_To_RDR))
            {
                
                Recv(uuid, value);
                syncExchange.Set();
                logger.trace("\t" + BinConvert.ToHex(uuid) + " > " + BinConvert.ToHex(value));
            }
            else
            {
                logger.trace(string.Format("Unhandled notification for characteristic {0:X2}", uuid));
            }
        }
        public static SCardReaderList_CcidOverNetwork Instantiate(string Address, ushort Port = 3999, bool Start = true )
        {
            SCardReaderList_CcidOverNetwork r = new SCardReaderList_CcidOverNetwork();
            r.logger = new Logger(r.GetType().ToString(), Address);
            r.logger.trace("Connecting");
            try
            {
                
                if (!r.OpenDevice(Address, Port))
                    return null;

                if (!r.MakeReaderList())
                {
                    r.CloseDevice();
                    return null;
                }
                if (Start)
                {
                    if (!r.StartDevice())
                    {
                        r.CloseDevice();
                        return null;
                    }

                    r.logger.trace("Device ready...");
                }
                else
                {
                    r.SendControl(CCID.SET_CONFIGURATION, 0x0000);
                    r.WaitControl();
                }

                r.logger.trace("{0}:Connected", Address);
                r.ScheduleProbe();
                return r;
            }
            catch (Exception e)
            {
                r.logger.warning("{0}:InstantiateProc exception {1}", Address, e.Message);
                return null;
            }

        }

        private static void InstantiateProc(object p)
        {
            InstantiateParams instantiateParams = (InstantiateParams)p;

            Logger.Trace("CcidOverNetwork:Background instantiate");

            SCardReaderList_CcidOverNetwork instance = Instantiate(instantiateParams.deviceAddress, instantiateParams.devicePort);
            if (instance != null)
            {
                instance.InstanceTag = instantiateParams.InstanceTag;


                if (instantiateParams.Callback != null)
                {
                    Logger.Trace("CcidOverNetwork:Calling the callback");
                    instantiateParams.Callback(instance);
                }
                else if (instantiateParams.CallbackEx != null)
                {
                    Logger.Trace("CcidOverNetwork:Calling the callback");
                    instantiateParams.CallbackEx(instantiateParams.InstanceTag, instance);
                }
            }

        }

        protected bool OpenDevice(string HostName, ushort TcpPort = 3999)
        {
            logger.trace("CcidOverNetwork:OpenDevice " + HostName + ":" + TcpPort + "...");
            try
            {
                tcpDevice = new Device();
                if (tcpDevice == null)
                    return false;

                if (tcpDevice.Connect(HostName,
                    new Device.DisconnectCallback(TcpDeviceDisconnectCallback),
                        new Device.NotificationCallback(TcpDeviceNotificationCallback)) == false)
                {
                    logger.trace("Connect failed");
                    return false;
                }            
            }
            catch
            {
                return false;
            }
            return true;
        }

        protected override void CloseDevice()
        {
            logger.trace("CcidOverNetwork:CloseDevice...");
            //ExitCcidMode();

            if (tcpDevice != null)
                tcpDevice.Disconnect(true);
        }
        protected bool SendCcidPcToRdr(byte[] buffer)
        {
            int tosend = buffer.Length;

            if (tcpDevice == null)
            {
                logger.trace("SendCcidPcToRdr : device null");
                return false;
            }
            try
            {
                if (tcpDevice.Send(buffer) == false)
                {
                    logger.trace("SendCcidPcToRdr :Can't write to the device");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.warning(string.Format("SendCcidPcToRdr : " + ex.Message));
                return false;
            }
            return true;
        }
        private void BackgroundSender(Object obj)
        {
            byte[] buffer = (byte[])obj;
            
            /* no probe needed if exchange in progress */
            CancelProbe();
            
            if (!SendCcidPcToRdr(buffer))
            {
                logger.trace("Failed to send to the TCP device, closing");

                byte[] t = new byte[1 + buffer.Length];

                t[0] = 0x00;
                Array.Copy(buffer, 0, t, 1, (t.Length - 1));

                ExitCcidMode();

                if (tcpDevice != null)
                    tcpDevice.Disconnect(true);
                return;
            }
            ScheduleProbe();
        }

        protected bool SendAsync(byte endpoint, byte[] buffer_with_endpoint)
        {
            if ((endpoint == CCID.EP_Bulk_PC_To_RDR) || (endpoint == CCID.EP_Control_To_RDR))
            {
                asyncThread = new Thread(new ParameterizedThreadStart(BackgroundSender));
                asyncThread.Start(buffer_with_endpoint);
                return true;
            }
            logger.trace("Can't send to endpoint " + BinConvert.ToHex(endpoint));
            return false;
        }
        protected override bool Send(byte endpoint, byte[] buffer)
        {
            if (tcpDevice.DeviceState == false)
                return false;

            lock (locker)
            {
                byte[] buffer_with_endpoint = new byte[buffer.Length + 1];

                buffer_with_endpoint[0] = endpoint;
                Array.Copy(buffer, 0, buffer_with_endpoint, 1, buffer.Length);

                logger.trace(string.Format("\t" + BinConvert.ToHex(endpoint) + " < " + BinConvert.ToHex(buffer)));
                _processing_cmd = true;
                SendAsync(endpoint, buffer_with_endpoint);

                int cnt_timeout_loop = 0;
                for (cnt_timeout_loop = 0; cnt_timeout_loop < 5500; cnt_timeout_loop++)
                {
                    if (_processing_cmd == false)
                    {
                        logger.trace("[Socket Send] Aborted due to card ejection");
                        syncExchange.Reset();
                        return true;
                    }
                    /* still no answer */
                    if (!syncExchange.WaitOne(0))
                    {
                        try
                        {
                            Thread.Sleep(100);
                        }
                        catch (ThreadInterruptedException)
                        {
                            logger.trace("Thread '{0}' awoken.", Thread.CurrentThread.Name);
                        }
                        catch (ThreadAbortException)
                        {
                            logger.trace("Thread '{0}' aborted.", Thread.CurrentThread.Name);
                        }
                        continue;
                    }
                    else
                    {
                        break;
                    }

                }

                logger.trace(string.Format( "[Socket Send] {0} ms", cnt_timeout_loop * 10));

                if (cnt_timeout_loop >= 5500)
                {
                    logger.warning("[Socket Send] Aborted due to timeout");
                    _processing_cmd = false;
                    syncExchange.Reset();
                    /* if no answer */
                    /*force_answer(ref _cmde);
                    ReportNotification(_cmde[0], BinUtils.Copy(_cmde, 1, (_cmde.Length - 1)));*/
                    return false;
                }
                logger.trace("[Socket Send] Send Recv OK");
                syncExchange.Reset();
                return true;
            }
        }

        private bool IsInBufferComplete(byte[] buffer, int buffer_lenght, out int deal)
        {
            int offset = 0;
            bool bGetHeader = false;
            int dataLength;
            deal = 0;

            if (buffer == null)
            {
                logger.trace("IsInBufferComplete: buffer null");
                return false;
            }


            for (offset = 0; offset < buffer.Length; offset++)
            {
                if ((buffer[offset] == CCID.EP_Bulk_RDR_To_PC) ||
                    (buffer[offset] == CCID.EP_Control_To_PC) ||
                    (buffer[offset] == CCID.RDR_TO_PC_DATABLOCK) ||
                    (buffer[offset] == CCID.RDR_TO_PC_ESCAPE) ||
                    (buffer[offset] == CCID.RDR_TO_PC_NOTIFYSLOTCHANGE) ||
                    (buffer[offset] == CCID.RDR_TO_PC_SLOTSTATUS) ||
                    (buffer[offset] == CCID.EP_Control_To_RDR) ||
                    (buffer[offset] == CCID.GET_DESCRIPTOR) ||
                    (buffer[offset] == CCID.SET_CONFIGURATION)
                    )
                {
                    bGetHeader = true;
                    break;
                }
            }

            if (bGetHeader == false)
            {
                logger.trace("IsInBufferComplete: No header ");
                return false;
            }

            if (buffer_lenght < (offset + 10))
            {
                logger.trace(string.Format("IsInBufferComplete: IsBufferComplete Lack Error buffer.Length: " + buffer.Length));
                return false;
            }

            /* byte 0 is endpoint */
            /* byte 1 is message code */
            /* Length of data is on bytes 2 to 4 */
            dataLength = buffer[4 + offset] & 0x4F; dataLength *= 0x00000100;
            dataLength += buffer[3 + offset]; dataLength *= 0x00000100;
            dataLength += buffer[2 + offset]; dataLength *= 0x00000100;
            dataLength += buffer[1 + offset];

            if ((dataLength < 0) || (dataLength > 0x00010000))
            {
                deal = -1;
                logger.trace(string.Format("IsInBufferComplete: IsBufferComplete Error buffer.Length: " + buffer_lenght + " dataLength: " + dataLength));
                return true;
            }

            if (buffer_lenght == ((10 + offset) + dataLength))
            {
                return true;
            }

            /* two commands inside the same trame */
            if (buffer_lenght > ((10 + offset) + dataLength))
            {
                logger.trace(string.Format("IsInBufferComplete: + IsBufferComplete buffer.Length: " + buffer_lenght + " dataLength: " + dataLength));
                deal = ((10 + offset) + dataLength);
                return true;
            }

            return false;
        }
        protected bool CheckInCcidLength(byte[] buffer, int buffer_lenght, out int deal_done)
        {
            deal_done = 0;

            if (IsInBufferComplete(buffer, buffer_lenght, out deal_done))
            {
                /* means crazy size from ccid command */
                if (deal_done == -1)
                {
                    return true;
                }
                return true;
            }
            return false;
        }
        
        protected override bool StartDevice()
        {
            byte bOptions = 0;

            logger.trace("CcidOverNetwork:StartDevice...");

            deviceState = DeviceState.NotActive;

            if (!SendControl(CCID.SET_CONFIGURATION, 0x0001, 0, bOptions))
                return false;
            if (!WaitControl())
                return false;

            if (deviceState != DeviceState.Active)
                return false;

            return base.StartDevice();
        }
        
        protected bool EnterCcidMode()
        {
            logger.trace("Activating the PC/SC profile");
            if (tcpDevice != null)
            {
                //IrqInitialStatus = 0xAA;
                //logger.trace(string.Format("Init SlotCount={0} SlotInitialStatus={1:X2}", SlotCount, IrqInitialStatus));
            }
            byte[] ENTER_CCID_MODE = new byte[] { 0x00, CCID.SET_CONFIGURATION, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 };
            /*bDirectCommandToReader = true;
            if (last_ccid_response != null)
                last_ccid_response = null;*/
            if (tcpDevice.Send(ENTER_CCID_MODE) == false)
            {
                return false;
            }
            return true;
        }

        protected bool ExitCcidMode()
        {
            logger.trace("Leaving the PC/SC profile");
            byte[] EXIT_CCID_MODE = new byte[] { 0x00, CCID.SET_CONFIGURATION, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            /*bDirectCommandToReader = true;
            if (last_ccid_response != null)
                last_ccid_response = null;*/
            if (tcpDevice != null)
            {
                if (tcpDevice.Send(EXIT_CCID_MODE) == false)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }      

    }

}
