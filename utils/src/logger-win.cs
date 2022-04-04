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
		[field: NonSerialized]
		private EventLog eventLog = null;

		/**
		 * \brief Configure the Logger to send its messages to Windows' event log
		 */
		public static void OpenEventLog(string log, string source)
		{
			defaultInstance.openEventLog(log, source);
		}

		/**
		 * \brief Configure the Logger to send its messages to Windows' event log
		 */
		public static void OpenEventLog(Level level, string log, string source)
		{
			defaultInstance.openEventLog(log, source);
			defaultInstance.eventLogLevel = level;
		}


		/**
		 * \brief Load the Logger settings from the application's registry branch.
		 */
		public void loadConfigFromRegistry(string CompanyName, string ProductName)
		{
			try
			{
				Level verboseLevel = Level.None;

				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + CompanyName + "\\" + ProductName, false);
				string s = (string)k.GetValue("VerboseLevel", "");

				int i;
				if (int.TryParse(s, out i))
				{
					verboseLevel = IntToLevel(i);
				}
				else
				{
					for (i = (int)Level.Debug; i > (int)Level.None; i--)
					{
						if (s.ToLower() == ((Level)i).ToString().ToLower())
						{
							verboseLevel = (Level)i;
							break;
						}
					}
				}

				logSettings.consoleLevel = verboseLevel;

				s = (string)k.GetValue("VerboseFile");
				if (!string.IsNullOrEmpty(s))
				{
					logSettings.logFileName = s;
					if (LogFileMutex == null)
						LogFileMutex = new Mutex();
				}

				s = (string)k.GetValue("VerboseFileDate", "0");
				if (int.TryParse(s, out i))
				{
					if (i != 0)
					{
						logSettings.logFileNameWithDate = true;
					}
				}

			}
			catch
			{
			}
		}

		public void openEventLog(string log, string source)
		{
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
				warning("Failed to open Event Log ({0})", e.Message);
			}
		}

		public void openEventLog(Level level, string log, string source)
		{
			openEventLog(log, source);
			logSettings.eventLogLevel = level;
		}

		public Level eventLogLevel
		{
			get => logSettings.eventLogLevel;
			set => logSettings.eventLogLevel = value;
		}

		private void sendToEventLog(Level level, string context, string message)
		{
			if (eventLog != null)
			{
				try
				{
					lock (eventLog)
					{
						string s = "";

						if (!string.IsNullOrEmpty(context))
							s = s + context + "\n";
						s = s + message;

						switch (level)
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
				}
				catch (Exception e)
				{
					sendToConsole(DateTime.UtcNow, Level.Warning, "EventLog", string.Format("Failed to write into Event Log ({0})", e.Message));
					resetEventLog();
					eventLog.WriteEntry("(Clear log)", EventLogEntryType.Error);
				}
			}
		}

		private void resetEventLog()
		{
			if (eventLog != null)
			{
				try
				{
					lock (eventLog)
					{
						string log = eventLog.Log;
						string source = eventLog.Source;
						eventLog.Clear();
						eventLog = null;
						openEventLog(log, source);
						eventLog.Clear();
					}
				}
				catch { }
			}
		}
	}
}

#endif