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
		public class GelfLogSink : Sink
		{
			private Dictionary<string, string> constants = new Dictionary<string, string>();
			private GelfTcpSender sender = null;

			public GelfLogSink(Level level, string hostName, string applicationName, string serverName, int serverPort = 2202) : base(level)
            {
				try
				{
					constants["version"] = "1.1";
					if (string.IsNullOrEmpty(hostName))
						hostName = Environment.MachineName;
					constants["host"] = hostName;
					constants["_machine"] = Environment.MachineName;
					constants["_user"] = Environment.UserName + "@" + Environment.UserDomainName;
					if (string.IsNullOrEmpty(applicationName))
						applicationName = System.AppDomain.CurrentDomain.FriendlyName;
					constants["_application"] = applicationName;
					constants["_process_id"] = String.Format("{0}", Process.GetCurrentProcess().Id);

					consoleSink.Send(Level.Debug, Context, "Gelf sending to Server {0}:{1}", serverName, serverPort);
					sender = new GelfTcpSender(serverName, serverPort);
				}
				catch (Exception e)
				{
					sender = null;
					consoleSink.Send(Level.Warning, Context, string.Format("Failed to create Gelf Sender ({0})", e.Message));
				}
			}

			public void SetConstant(string Name, string Value)
            {
				constants[Name] = Value;
            }

			internal override void Send(Entry entry)
            {
				if (sender == null)
					return;

				SysLog.Severity severity = LevelToSeverity(entry.level);

				JSON json = new JSON();

				foreach (KeyValuePair<string, string> constant in constants)
					json.Add(constant.Key, constant.Value);

				string message = entry.message;
				if (!string.IsNullOrEmpty(entry.context))
					message = entry.context + ":" + message;

				json.Add("level", (int)severity);
				json.Add("short_message", message);

				try
				{
					sender.Send(json.AsString());
				}
				catch (Exception e)
				{
					consoleSink.Send(Level.Warning, Context, string.Format("Failed to send to remote Gelf Server ({0})", e.Message));
				}
			}

			public static GelfLogSink CreateSpringCardDev(Level level, string hostName, string applicationName)
            {
				GelfLogSink result = new GelfLogSink(level, hostName, applicationName, "gra1.logs.ovh.com", 2202);
				result.SetConstant("X-OVH-TOKEN", "c379d86b-ca8f-4122-ba76-1499f1110681");
				return result;
			}

			public static GelfLogSink CreateSpringCardProd(Level level, string hostName, string applicationName)
			{
				GelfLogSink result = new GelfLogSink(level, hostName, applicationName, "gra1.logs.ovh.com", 2202);
				result.SetConstant("X-OVH-TOKEN", "e01af581-2477-429b-84e2-5cb36a4e9326");
				return result;
			}

			public static GelfLogSink CreateSpringCardInfra(Level level, string hostName, string applicationName)
			{
				GelfLogSink result = new GelfLogSink(level, hostName, applicationName, "gra1.logs.ovh.com", 2202);
				result.SetConstant("X-OVH-TOKEN", "0bf01279-140a-4454-a250-18be069c8a13");
				return result;
			}
		}

		public static Level GelfLogLevel
		{
			set
			{
				lock (sinks)
				{
					foreach (Sink sink in sinks)
					{
						if (sink is GelfLogSink)
							sink.level = value;
					}
				}
			}
		}


		/**
		 * \brief Configure the Logger to send its messages to a GrayLog server
		 */
		public static void OpenGelf(Level level, string hostName, string applicationName, string serverName, int serverPort = 2202)
		{
			AddSink(new GelfLogSink(level, hostName, applicationName, serverName, serverPort), true);
		}

		/**
		 * \brief Configure the Logger to send its messages to a GrayLog server
		 */
		public static void OpenGelf(Level level, string applicationName, string serverAddr, int serverPort = 2202)
		{
			OpenGelf(level, null, applicationName, serverAddr, serverPort);
		}

		/**
		 * \brief Configure the Logger to send its messages to a GrayLog server
		 */
		public static void OpenGelf(Level level, string serverAddr, int serverPort = 2202)
		{
			OpenGelf(level, null, null, serverAddr, serverPort);
		}
	}
}
