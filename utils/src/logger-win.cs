/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
#if !NET5_0_OR_GREATER
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Windows.Markup;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpringCard.LibCs
{
	/**
	 * \brief Utility to log execution information on the console, the trace or debug output, the system event collector (Windows or Syslog) or a remote GrayLog server
	 */
	public partial class Logger
	{
		public class EventLogSink : Sink
		{
			EventLog eventLog = null;
			object locker = new object();
			string log;
			string source;

			public EventLogSink(Level level, string log, string source) : base(level)
            {
				this.log = log;
				this.source = source;
				lock (locker)
				{
					Open();
				}
            }

			internal override void Send(Entry entry)
            {
#if !NET5_0_OR_GREATER
				bool reset = false;
				lock (locker)
				{
					if (eventLog != null)
					{
						try
						{
							string s = "";

							if (!string.IsNullOrEmpty(entry.context))
								s = s + entry.context + "\n";
							s = s + entry.message;

							switch (entry.level)
							{
								case Level.Fatal:
								case Level.Error:
									eventLog.WriteEntry(s, EventLogEntryType.Error);
									break;
								case Level.Warning:
									eventLog.WriteEntry(s, EventLogEntryType.Warning);
									break;
								case Level.Info:
									eventLog.WriteEntry(s, EventLogEntryType.Information);
									break;
								case Level.Trace:
									eventLog.WriteEntry(s + "\n(Trace)", EventLogEntryType.Information);
									break;
								case Level.Debug:
									eventLog.WriteEntry(s + "\n(Debug)", EventLogEntryType.Information);
									break;
								default:
									break;
							}
						}
						catch (Exception e)
						{
							consoleSink.Send(Level.Warning, Context, string.Format("Failed to write into EventLog ({0})", e.Message));
							reset = true;
						}
					}
				}
				if (reset)
                {
					Reset();
                }
#endif
			}

			private void Open()
			{
#if !NET5_0_OR_GREATER
				try
				{
					if (!EventLog.SourceExists(source))
					{
						EventLog.CreateEventSource(source, log);
					}

					eventLog = new EventLog();
					eventLog.Log = log;
					eventLog.Source = source;
				}
				catch (Exception e)
				{
					eventLog = null;
					consoleSink.Send(Level.Warning, Context, string.Format("Failed to open EventLog ({0})", e.Message));
				}
#endif
			}

			private void Reset()
			{
#if !NET5_0_OR_GREATER
				lock (locker)
				{
					if (eventLog != null)
					{
						try
						{
							eventLog.Clear();
							eventLog = null;
							Open();
							eventLog.Clear();
						}
						catch (Exception e)
						{
							consoleSink.Send(Level.Warning, Context, string.Format("Failed to reset EventLog ({0})", e.Message));
						}
					}
					else
					{
						Open();
					}
				}
			}
#endif
		}

		public static Level EventLogLevel
		{
			set
			{
				lock (sinks)
				{
					foreach (Sink sink in sinks)
					{
						if (sink is EventLogSink)
							sink.level = value;
					}
				}
			}
		}

		/**
		 * \brief Configure the Logger to send its messages to Windows' event log
		 */
		public static void OpenEventLog(Level level, string log, string source)
		{
			AddSink(new EventLogSink(level, log, source), true);
		}


		/**
		 * \brief Configure the Logger to send its messages to Windows' event log
		 */
		public static void OpenEventLog(string log, string source)
		{
			OpenEventLog(Level.Info, log, source);
		}
	}
}

#endif