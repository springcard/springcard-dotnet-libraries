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
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Markup;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpringCard.LibCs
{
	public partial class Logger
	{
		public class SysLogSink : Sink
		{
			private ISyslogMessageSerializer sysLogSerializer = null;
			private ISyslogMessageSender sysLogSender = null;

			SysLog.Facility facility;
			string hostName;
			string applicationName;

			public SysLogSink(Level level, SysLog.Facility facility, string hostName, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false) : base(level)
            {
				try
				{
					this.facility = facility;

					if (string.IsNullOrEmpty(hostName))
						this.hostName = Environment.MachineName;
					else
						this.hostName = hostName;

					if (string.IsNullOrEmpty(applicationName))
						this.applicationName = AppDomain.CurrentDomain.FriendlyName;
					else
						this.applicationName = applicationName;

					this.applicationName += string.Format("[{0}]", Process.GetCurrentProcess().Id);

					if (useRfc5424)
						sysLogSerializer = new SyslogRfc5424MessageSerializer();
					else
						sysLogSerializer = new SyslogRfc3164MessageSerializer();

					if (serverAddr == null)
                    {
						consoleSink.Send(Level.Debug, Context, "SysLog broadcasting to UDP port {0}", serverPort);
					}
					else
                    {
						consoleSink.Send(Level.Debug, Context, "SysLog sending to Server {0} over UDP port {1}", serverAddr, serverPort);
					}

					sysLogSender = new SyslogUdpSender(serverAddr, serverPort);
				}
				catch (Exception e)
				{
					sysLogSender = null;
					sysLogSerializer = null;
					consoleSink.Send(Level.Warning, Context, string.Format("Failed to create SysLog Sender ({0})", e.Message));
				}
			}

			internal override void Send(Entry entry)
            {
				if ((sysLogSender == null) || (sysLogSerializer == null))
					return;

				SysLog.Severity severity = LevelToSeverity(entry.level);

				string sysLogText = RemoveDiacritics(entry.message);
				if (!string.IsNullOrEmpty(entry.context))
				{
					sysLogText = string.Format("{0}\t{1}", RemoveDiacritics(entry.context), sysLogText);
				}

				SysLog.Message sysLogMessage = new SysLog.Message(DateTimeOffset.Now, facility, severity, hostName, applicationName, sysLogText);

				try
				{
					sysLogSender.Send(sysLogMessage, sysLogSerializer);
				}
				catch (Exception e)
				{
					consoleSink.Send(Level.Warning, Context, string.Format("Failed to send to remote SysLog Server ({0})", e.Message));
				}
			}
		}

		private static SysLog.Severity LevelToSeverity(Level level)
		{
			SysLog.Severity result;

			switch (level)
			{
				case Level.Trace:
					result = SysLog.Severity.Informational;
					break;
				case Level.Info:
					result = SysLog.Severity.Notice;
					break;
				case Level.Warning:
					result = SysLog.Severity.Warning;
					break;
				case Level.Error:
					result = SysLog.Severity.Error;
					break;
				case Level.Fatal:
					result = SysLog.Severity.Critical;
					break;
				case Level.All:
				case Level.Debug:
				default:
					result = SysLog.Severity.Debug;
					break;
			}

			return result;
		}

		public static Level SysLogLevel
		{
			set
			{
				lock (sinks)
				{
					foreach (Sink sink in sinks)
					{
						if (sink is SysLogSink)
							sink.level = value;
					}
				}
			}
		}


		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(Level level, SysLog.Facility facility, string hostName, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			AddSink(new SysLogSink(level, facility, hostName, applicationName, serverAddr, serverPort, useRfc5424), true);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(SysLog.Facility facility, string hostName, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(Level.All, facility, hostName, applicationName, serverAddr, serverPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(SysLog.Facility facility, Level level, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(level, facility, null, applicationName, serverAddr, serverPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(SysLog.Facility facility, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(Level.All, facility, null, null, serverAddr, serverPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(SysLog.Facility facility, Level level, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(level, facility, null, null, serverAddr, serverPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send messages to a SysLog server
		 */
		public static void OpenSysLog(string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(Level.All, SysLog.Facility.LocalUse0, null, null, serverAddr, serverPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(Level level, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(level, SysLog.Facility.LocalUse0, null, null, serverAddr, serverPort, useRfc5424);
		}
	}
}
