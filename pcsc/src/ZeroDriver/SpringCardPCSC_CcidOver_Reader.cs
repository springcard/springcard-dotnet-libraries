/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using System.Diagnostics;
using System.Threading;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.ZeroDriver
{
    public class SCardReader_CcidOver : SCardReader
    {
        public SCardReaderList_CcidOver ParentReaderList { get; private set; }
        byte ReaderSlot;

        public SCardReader_CcidOver(SCardReaderList_CcidOver ParentReaderList, byte ReaderSlot)
        {
            this.ParentReaderList = ParentReaderList;
            this.ReaderSlot = ReaderSlot;
            readerName = ParentReaderList.Readers[ReaderSlot];
        }

        ~SCardReader_CcidOver()
        {

        }

        internal void SetReaderName(string readerName)
        {
            this.readerName = readerName;
        }

        public bool Available
        {
            get
            {
                if (ParentReaderList == null) return false;
                return ParentReaderList.Available;
            }
        }

        public override SCardChannel CreateChannel()
        {
            return new SCardChannel_CcidOver(ParentReaderList, ReaderSlot);
        }

        public override bool Control(CardBuffer cctrl, out CardBuffer rctrl)
        {
            rctrl = null;
            SCardChannel_CcidOver channel = new SCardChannel_CcidOver(this.ParentReaderList, 0);
            if (!channel.ConnectDirect())
                return false;
            bool rc = channel.Control(cctrl, out rctrl);
            channel.DisconnectLeave();
            return rc;
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

        protected override void MonitorProc()
        {
            uint state = 0;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);

            while (statusMonitorRunning)
            {

                uint rc;

                try
                {
                    rc = ParentReaderList.GetStatusChange(ReaderSlot, ref state, 250);
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }

                if (!statusMonitorRunning)
                    break;

                if (rc == SCARD.E_TIMEOUT)
                    continue;

                if (rc != SCARD.S_SUCCESS)
                {
                    _last_error = rc;

                    if (onReaderStateChangeEx != null)
                        onReaderStateChangeEx(this, 0, null);

                    if (onReaderStateChange != null)
                        onReaderStateChange(0, null);

                    if (onReaderStateErrorEx != null)
                        onReaderStateErrorEx(this);

                    if (onReaderStateError != null)
                        onReaderStateError();

                    break;
                }

                if ((state & SCARD.STATE_CHANGED) != 0)
                {
                    state = state & ~SCARD.STATE_CHANGED;

                    CardBuffer card_atr = null;

                    if ((state & SCARD.STATE_PRESENT) != 0)
                        card_atr = new CardBuffer(ParentReaderList.GetAtr(ReaderSlot));

                    if (onReaderStateChangeEx != null)
                        onReaderStateChangeEx(this, state, card_atr);

                    if (onReaderStateChange != null)
                        onReaderStateChange(state, card_atr);
                }
            }

        }
    }
}
