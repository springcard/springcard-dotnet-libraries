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
	public interface ILogger
    {
		//void debug(string message);
		void debug(string message, params object[] args);
		//void trace(string message);
		void trace(string message, params object[] args);
		//void info(string message);
		void info(string message, params object[] args);
		//void warning(string message);
		void warning(string message, params object[] args);
		//void error(string message);
		void error(string message, params object[] args);
		//void fatal(string message);
		void fatal(string message, params object[] args);
	}

    public class DummyLogger : ILogger
    {
		public void debug(string message, params object[] args) { }
		public void error(string message, params object[] args) { }
		public void fatal(string message, params object[] args) { }
		public void info(string message, params object[] args) { }
        public void trace(string message, params object[] args) { }
		public void warning(string message, params object[] args) { }
    }

    [Serializable]
	/**
	 * \brief Utility to log execution information on the console, the trace or debug output, the system event collector (Windows or Syslog) or a remote GrayLog server
	 */
	public partial class Logger : ILogger
	{
#region Mutex
		private static object ConsoleLock = new object();
		private static Mutex LogFileMutex = null;
#endregion
#region Queue
		private static ObservableCollection<LogObject> LogFifo = null;
#endregion

        /**
		 * \brief The level of details in the output or log
		 *
		 * \warning Debug level may leak sensitive information!
		 */
        public enum Level
		{
			None = -1, /*!< Completely disable the log */
			Fatal, /*!< Log Fatal messages only, i.e. unrecoverable errors or exceptions that leads to the termination of the program */
			Error, /*!< Same as Fatal + also log Error messages, for instance errors or exceptions in the program itself */
			Warning, /*!< Same as Error + also log Warning messages, for instance recoverable errors in system calls or user-related errors in the program */
			Info, /*!< Same as Warning + also log Info messages, to see how the program behaves */
			Trace, /*!< Same as Info + also log Trace messages to follow the execution flow */
			Debug, /*!< Same as Trace + also log Debug messages, with detailed information regarding the execution flow and parameters */
			All /*!< Log all messages (same as Debug) */
		};

		[Serializable]
		private class LogSettings : ICloneable
		{
			public string context;
			public Level debugLevel = Level.Trace;
			public Level traceLevel = Level.Trace;
			public Level consoleLevel = Level.None;
			public bool consoleShowTime = true;
			public bool consoleShowContext = true;
			public bool consoleUseColors = true;
			public Level logFileLevel;
			public string logFileName;
			public bool logFileNameWithDate;
			public int logFileRetentionWeeks;
			public Level gelfLevel = Level.Warning;
			public Level eventLogLevel = Level.Warning;
			public Level sysLogLevel = Level.Warning;
			public SysLog.Facility sysLogFacility = SysLog.Facility.LocalUse0;
			public string sysLogMachineName;
			public string sysLogApplicationName;

			public object Clone()
			{
				return this.MemberwiseClone();
			}
		}

		private ISyslogMessageSerializer sysLogSerializer = null;
		private ISyslogMessageSender sysLogSender = null;

		public Level level
		{
			set
			{
				logSettings.consoleLevel = value;
				logSettings.logFileLevel = value;
				logSettings.gelfLevel = value;
				logSettings.eventLogLevel = value;
				logSettings.sysLogLevel = value;
			}
		}

		private LogSettings logSettings = new LogSettings();

		[Serializable]
		private class LogObject
		{
			public Logger instance;
			public DateTime when;
			public string context;
			public Level level;
			public string message;
			public object[] args;
		}
		private static void LogFifo_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				try
				{
					lock (LogFifo)
					{
						while (LogFifo.Count > 0)
						{
							LogObject logObject = LogFifo[0];
							LogFifo.RemoveAt(0);
							logObject.instance.loggerCore(logObject);
						}
					}
				}
				catch { }
			}
		}

#region Config


		/**
		 * \brief Load the Logger settings from the program's command line
		 */
		public void readArgs(string[] args)
		{
			try
			{
				if (args != null)
				{
					for (int i = 0; i < args.Length; i++)
					{
						string s = args[i].ToLower();
						if (s.Equals("--console"))
						{
							if (logSettings.consoleLevel < Level.Info)
								logSettings.consoleLevel = Level.Info;
						}
						else if (s.Equals("--debug"))
						{
							if (logSettings.consoleLevel < Level.Debug)
								logSettings.consoleLevel = Level.Debug;
						}
						else if (s.StartsWith("--verbose") || (s.StartsWith("-v")))
						{
							if (logSettings.consoleLevel < Level.Trace)
								logSettings.consoleLevel = Level.Trace;
							s = s.Substring(s.Length - 1);
							if (s.StartsWith("=")) s = s.Substring(1);
							if (s.Length > 0)
							{
								int v;
								if (int.TryParse(s, out v))
									logSettings.consoleLevel = IntToLevel(v);
							}
						}
						else if (s.StartsWith("--logfile="))
						{
							s = s.Substring(10);
							if (s.Length > 0)
							{
								OpenLogFile(s, true);
							}
						}
						else if (s.StartsWith("--loglevel="))
						{
							s = s.Substring(11);
							if (s.Length > 0)
							{
								int v;
								if (int.TryParse(s, out v))
									logSettings.logFileLevel = IntToLevel(v);
							}
						}
					}
				}
			}
			catch { }
		}

#endregion

#region Console		

		private void sendToConsole(DateTime when, Level level, string context, string message)
		{
			try
			{
				lock (ConsoleLock)
				{
					if (logSettings.consoleUseColors)
					{
						switch (Console.BackgroundColor)
						{
							case ConsoleColor.Black:
							case ConsoleColor.DarkGreen:
							case ConsoleColor.DarkRed:
							case ConsoleColor.DarkBlue:
							case ConsoleColor.DarkCyan:
							case ConsoleColor.DarkMagenta:
							case ConsoleColor.DarkYellow:
								break;

							default:
								Console.BackgroundColor = ConsoleColor.Black;
								break;
						}


						Console.ForegroundColor = ConsoleColor.Gray;
					}

					if (message.Contains("\b"))
					{
						Console.SetCursorPosition(0, Console.CursorTop - 1);
					}

					if (logSettings.consoleShowTime)
					{
						Console.Write("{0:D02}:{1:D02}:{2:D02}.{3:D03} ", when.Hour, when.Minute, when.Second, when.Millisecond);
					}

					if (logSettings.consoleShowContext && !string.IsNullOrEmpty(context))
					{
						if (logSettings.consoleUseColors)
						{
							Console.ForegroundColor = ConsoleColor.DarkGray;
						}

						if (context.EndsWith(":"))
							Console.Write(context);
						else
							Console.Write("{0} ", context);
					}

					if (logSettings.consoleUseColors)
					{
						switch (level)
						{
							case Level.Fatal:
								Console.BackgroundColor = ConsoleColor.Red;
								Console.ForegroundColor = ConsoleColor.White;
								break;
							case Level.Error:
								Console.BackgroundColor = ConsoleColor.Red;
								Console.ForegroundColor = ConsoleColor.Yellow;
								break;
							case Level.Warning:
								Console.ForegroundColor = ConsoleColor.Yellow;
								break;
							case Level.Info:
								Console.ForegroundColor = ConsoleColor.Cyan;
								break;
							case Level.Trace:
								Console.ForegroundColor = ConsoleColor.White;
								break;
							case Level.Debug:
								Console.ForegroundColor = ConsoleColor.Gray;
								break;
						}
					}

					Console.Write(message);

					if (logSettings.consoleUseColors)
					{
						Console.BackgroundColor = InitialBackgroundColor;
						Console.ForegroundColor = InitialForegroundColor;
					}

					Console.WriteLine();
				}
			}
			catch { }
		}

#endregion

#region SysLog		

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

		public void openSysLog(SysLog.Facility facility, string hostName, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			try
			{
				logSettings.sysLogFacility = facility;

				if (string.IsNullOrEmpty(hostName))
					logSettings.sysLogMachineName = Environment.MachineName;
				else
					logSettings.sysLogMachineName = hostName;

				if (string.IsNullOrEmpty(applicationName))
					logSettings.sysLogApplicationName = System.AppDomain.CurrentDomain.FriendlyName;
				else
					logSettings.sysLogApplicationName = applicationName;

				logSettings.sysLogApplicationName += string.Format("[{0}]", Process.GetCurrentProcess().Id);
				if (useRfc5424)
					sysLogSerializer = new SyslogRfc5424MessageSerializer();
				else
					sysLogSerializer = new SyslogRfc3164MessageSerializer();
				sysLogSender = new SyslogUdpSender(serverAddr, serverPort);
			}
			catch (Exception e)
			{
				sysLogSender = null;
				sysLogSerializer = null;
				warning("Failed to create Syslog sender ({0})", e.Message);
			}
		}

		public void openSysLog(Level level, SysLog.Facility facility, string hostName, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			openSysLog(facility, hostName, applicationName, serverAddr, serverPort, useRfc5424);
			logSettings.sysLogLevel = level;
		}

		public void openSysLog(SysLog.Facility facility, Level level, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			openSysLog(facility, null, applicationName, serverAddr, serverPort, useRfc5424);
			logSettings.sysLogLevel = level;
		}

		public void openSysLog(SysLog.Facility facility, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			openSysLog(facility, null, null, ServerAddr, ServerPort, useRfc5424);
		}

		public void openSysLog(SysLog.Facility facility, Level level, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			openSysLog(facility, null, null, ServerAddr, ServerPort, useRfc5424);
			logSettings.sysLogLevel = level;
		}

		public void openSysLog(string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			openSysLog(SysLog.Facility.LocalUse0, ServerAddr, ServerPort, useRfc5424);
		}

		public void openSysLog(Level level, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			openSysLog(SysLog.Facility.LocalUse0, ServerAddr, ServerPort, useRfc5424);
			logSettings.sysLogLevel = level;
		}

		public Level sysLogLevel
		{
			get => logSettings.sysLogLevel;
			set => logSettings.sysLogLevel = value;
		}

		private void sendToSysLog(Level level, string context, string message)
		{
			if ((sysLogSender == null) || (sysLogSerializer == null))
				return;
			SysLog.Severity severity = LevelToSeverity(level);
			string sysLogText = RemoveDiacritics(message);
			if (!string.IsNullOrEmpty(context))
			{
				sysLogText = string.Format("{0}\t{1}", RemoveDiacritics(context), sysLogText);
			}
			SysLog.Message sysLogMessage = new SysLog.Message(DateTimeOffset.Now, logSettings.sysLogFacility, severity, logSettings.sysLogMachineName, logSettings.sysLogApplicationName, sysLogText);

			try
			{
				sysLogSender.Send(sysLogMessage, sysLogSerializer);
			}
			catch (Exception e)
			{
				sendToConsole(DateTime.UtcNow, Level.Warning, "SysLog", String.Format("Failed to send to remote server ({0})", e.Message));
				Console.WriteLine(String.Format("Failed to send to remote server ({0})", e.Message));
			}
		}

#endregion

#region Gelf

		private Dictionary<string, string> gelfConstants;
		private GelfTcpSender gelfSender = null;

		/**
		 * \brief Configure the Logger to send its messages to a GrayLog server
		 */
		public void openGelf(Level level, string hostName, string applicationName, string serverName, int serverPort = 2202)
		{
			try
			{
				sendToConsole(DateTime.UtcNow, Level.Debug, "Gelf", "Sending to server " + serverName + ":" + serverPort);
				logSettings.gelfLevel = level;
				gelfSender = new GelfTcpSender(serverName, serverPort);
				gelfConstants = new Dictionary<string, string>();
				gelfConstants["version"] = "1.1";
				if (String.IsNullOrEmpty(hostName))
					hostName = Environment.MachineName;
				gelfConstants["host"] = hostName;
				gelfConstants["_machine"] = Environment.MachineName;
				gelfConstants["_user"] = Environment.UserName + "@" + Environment.UserDomainName;
				if (String.IsNullOrEmpty(applicationName))
					applicationName = System.AppDomain.CurrentDomain.FriendlyName;
				gelfConstants["_application"] = applicationName;
				gelfConstants["_process_id"] = String.Format("{0}", Process.GetCurrentProcess().Id);
			}
			catch (Exception e)
			{
				gelfSender = null;
				Log(Level.Warning, String.Format("Failed to create Gelf sender ({0})", e.Message));
			}
		}

		/**
		 * \brief Set a GrayLog constant parameter
		 */
		public void setGelfConstant(string Name, string Value)
		{
			gelfConstants[Name] = Value;
		}

		private void sendToGelf(Level level, string context, string message)
		{
			if (gelfSender == null)
				return;

			SysLog.Severity severity = LevelToSeverity(level);

			JSON json = new JSON();

			foreach (KeyValuePair<string, string> constant in gelfConstants)
				json.Add(constant.Key, constant.Value);

			if (context != null)
				message = context + ":" + message;

			json.Add("level", (int)severity);
			json.Add("short_message", message);

			try
			{
				gelfSender.Send(json.AsString());
			}
			catch (Exception e)
			{
				sendToConsole(DateTime.UtcNow, Level.Warning, "Gelf", String.Format("Failed to send to remote server ({0})", e.Message));
			}
		}

#endregion

#region LogFile

		private DateTime fileRetentionCheck = DateTime.Now.AddMinutes(10);

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public void openLogFile(string fileName, bool useDate = false)
		{
			logSettings.logFileName = fileName;
			logSettings.logFileNameWithDate = useDate;
			logSettings.logFileRetentionWeeks = 0;

			string logFilePath = Path.GetDirectoryName(fileName);
			logFilePath = logFilePath.TrimEnd(Path.DirectorySeparatorChar);

			if (!String.IsNullOrEmpty(logFilePath))
				if (!Directory.Exists(logFilePath))
					Directory.CreateDirectory(logFilePath);
			if (LogFileMutex == null)
				LogFileMutex = new Mutex();
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public void openLogFile(Level level, string fileName, bool useDate = false)
		{
			logSettings.logFileLevel = level;
			openLogFile(fileName, useDate);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public void openLogFile(string fileName, bool useDate, int retentionWeeks)
		{
			openLogFile(fileName, useDate);
			logSettings.logFileRetentionWeeks = retentionWeeks;
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public void openLogFile(Level level, string fileName, bool useDate, int retentionWeeks)
		{
			openLogFile(level, fileName, useDate);
			logSettings.logFileRetentionWeeks = retentionWeeks;
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public void openLogFile(string fileName, int retentionWeeks)
		{
			openLogFile(fileName, true, retentionWeeks);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public void openLogFile(Level level, string fileName, int retentionWeeks)
		{
			openLogFile(level, fileName, true, retentionWeeks);
		}

		public Level logFileLevel
		{
			get => logSettings.logFileLevel;
			set => logSettings.logFileLevel = value;
		}

		private void FileRetentionPurge()
		{
			if (logSettings.logFileRetentionWeeks <= 0)
				return;

			string logFilePath = Path.GetDirectoryName(logSettings.logFileName);
			if (!Directory.Exists(logFilePath))
				return;

			DateTime deleteBeforeTime = DateTime.Now.AddDays(-7 * logSettings.logFileRetentionWeeks);

			string[] files = Directory.GetFiles(logFilePath);
			foreach (string file in files)
			{
				FileInfo fi = new FileInfo(file);
				if (fi.LastAccessTime < deleteBeforeTime)
				{
					try
					{
						fi.Delete();
					}
					catch { }
				}
			}
		}

		private static string FileNameWithDate(string fileName)
		{
			DateTime now = DateTime.Now;
			string str_now = String.Format("{0:D04}{1:D02}{2:D02}", now.Year, now.Month, now.Day);

			string[] parts = fileName.Split('.');

			string result = "";
			for (int i = 0; i < parts.Length; i++)
			{
				if (i > 0)
					result += ".";
				result += parts[i];
				if (i == parts.Length - 2)
					result += "-" + str_now;
			}

			return result;
		}

		private void sendToFile(DateTime when, Level level, string context, string message)
		{
			if (logSettings.logFileName == null)
				return;

			string aFileName = logSettings.logFileName;
			if (logSettings.logFileNameWithDate)
				aFileName = FileNameWithDate(aFileName);

			string[] aMessage = message.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

			bool fMutexOwner = false;

			try
			{
				if (LogFileMutex != null)
				{
					fMutexOwner = LogFileMutex.WaitOne(500);
					if (!fMutexOwner)
						return;
				}

				string s = when.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" + level.ToString() + "\t";

				if (!string.IsNullOrEmpty(context))
					s = s + context + "\t";
				else
					s = s + "\t";

				foreach (string m in aMessage)
				{
					File.AppendAllText(aFileName, s + m + Environment.NewLine);
				}
			}
			catch
			{

			}
			finally
			{
				if (logSettings.logFileRetentionWeeks > 0)
				{
					if (DateTime.Now >= fileRetentionCheck)
					{
						FileRetentionPurge();
						fileRetentionCheck = DateTime.Now.AddHours(24);
					}
				}


				if (fMutexOwner)
					LogFileMutex.ReleaseMutex();
			}

		}

#endregion


		/**
		 * \brief Log a message
		 */
		private void loggerCore(DateTime when, string context, Level level, string message, params object[] args)
		{
			if (level == Level.None)
				return;
			if (message == null)
				return;
			if (string.IsNullOrEmpty(context))
				context = logSettings.context;
			if (string.IsNullOrEmpty(context))
				context = guessContext();

			LogObject logObject = new LogObject();
			logObject.instance = this;
			logObject.when = when;
			logObject.level = level;
			logObject.context = context;
			logObject.message = message;
			logObject.args = args;

			if (LogFifo != null)
			{
				try
				{
					lock (LogFifo)
					{
						LogFifo.Add(logObject);
					}
				}
				catch { }
			}
			else
			{
				loggerCore(logObject);
			}
		}

		private void loggerCore(string context, Level level, string message, params object[] args)
		{
			loggerCore(DateTime.UtcNow, context, level, message, args);
		}

		private void loggerCore(Level level, string message, params object[] args)
		{
			loggerCore(DateTime.UtcNow, null, level, message, args);
		}

		/**
		 * \brief Log a message
		 */
		private void loggerCore(LogObject logObject)
		{
			try
			{
				string message;

				if (logObject == null)
					return;

				if (logObject.args != null)
					message = string.Format(logObject.message, logObject.args);
				else
					message = logObject.message;

				if ((logObject.level <= logSettings.debugLevel) || (logObject.level <= defaultInstance.logSettings.debugLevel))
				{
#if !NET5_0_OR_GREATER
					if (System.Diagnostics.Debug.Listeners.Count > 0)
					{
						System.Diagnostics.Debug.WriteLine(message);
					}
#else
					/* no debug in netcore 5. */
					if (System.Diagnostics.Trace.Listeners.Count > 0)
					{
						System.Diagnostics.Trace.WriteLine(message);
					}
#endif
				}
				else if ((logObject.level <= logSettings.traceLevel) || (logObject.level <= defaultInstance.logSettings.traceLevel))
				{
					if (System.Diagnostics.Trace.Listeners.Count > 0)
					{
						System.Diagnostics.Trace.WriteLine(message);
					}
				}

#if !NET5_0_OR_GREATER
				if ((logObject.level <= logSettings.eventLogLevel) || (logObject.level <= defaultInstance.logSettings.eventLogLevel))
				{
					if (eventLog != null)
						sendToEventLog(logObject.level, logObject.context, message);
				}
#endif
				if ((logObject.level <= logSettings.sysLogLevel) || (logObject.level <= defaultInstance.logSettings.sysLogLevel))
				{
					if (sysLogSender != null)
						sendToSysLog(logObject.level, logObject.context, message);
				}

				if ((logObject.level <= logSettings.gelfLevel) || (logObject.level <= defaultInstance.logSettings.gelfLevel))
				{
					if (gelfSender != null)
						sendToGelf(logObject.level, logObject.context, message);
				}

				if ((logObject.level <= logSettings.logFileLevel) || (logObject.level <= defaultInstance.logSettings.logFileLevel))
				{
					if (!string.IsNullOrEmpty(logSettings.logFileName))
						sendToFile(logObject.when, logObject.level, logObject.context, message);
				}

				if ((logObject.level <= logSettings.consoleLevel) || (logObject.level <= defaultInstance.logSettings.consoleLevel))
				{
					sendToConsole(logObject.when, logObject.level, logObject.context, message);
				}

				if (LogCallback != null)
				{
					if (logObject.context == null)
					{
						LogCallback(logObject.level, message);
					}
					else
					{
						LogCallback(logObject.level, logObject.context + ": " + message);
					}
				}
			}
			catch { }
		}


		private string guessContext()
		{
			StackFrame[] frames = new StackTrace().GetFrames();
			string thisAssembly = frames[0].GetMethod().Module.Assembly.FullName;
			foreach (StackFrame frame in frames)
			{
				string otherAssembly = frame.GetMethod().ReflectedType.Assembly.FullName;
				if (otherAssembly != thisAssembly)
				{
					string[] t = otherAssembly.Split(',');
					if (t[0].ToLower() == "mscorlib")
						return null;
					return t[0];
				}
			}

			return null;
		}


		private static string RemoveDiacritics(string text)
		{
			try
			{
#if NET5_0_OR_GREATER


				/*.NET Core supports only ASCII, ISO - 8859 - 1 and Unicode encodings, whereas.NET Framework supports much more.
				 * However, .NET Core can be extended to support additional encodings like Windows - 1252, Shift - JIS, GB2312 by registering the CodePagesEncodingProvider from the System.Text.Encoding.CodePages NuGet package.
				 * After the NuGet package is installed the following steps as described in the documentation for the CodePagesEncodingProvider class must be done to register the provider:
				 * Add a reference to the System.Text.Encoding.CodePages.dll assembly to your project.
				 * Retrieve a CodePagesEncodingProvider object from the static Instance property.
				 * Pass the CodePagesEncodingProvider object to the Encoding.RegisterProvider method.
				 */
				byte[] b = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(text);
#else
				byte[] b = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(text);
#endif
				string r = System.Text.Encoding.UTF8.GetString(b);
				return r;
			}
			catch(Exception ex)
            {
				Console.WriteLine(String.Format("Failed RemoveDiacritics ({0})", ex.Message));
			}
			return null;
		}

#region Constructors

        public Logger(Logger parent, string context, string instance)
		{
			if (parent == null)
				parent = defaultInstance;

			if (parent != null)
			{
				if (parent.logSettings != null)
					this.logSettings = (LogSettings)parent.logSettings.Clone();
#if !NET5_0_OR_GREATER
				this.eventLog = parent.eventLog;
#endif
				this.sysLogSerializer = parent.sysLogSerializer;
				this.sysLogSender = parent.sysLogSender;
			}

			if (!string.IsNullOrEmpty(context))
            {
				this.logSettings.context = context;
				if (this.logSettings.context.ToLower().StartsWith("springcard."))
					this.logSettings.context = this.logSettings.context.Substring(11);
			}
			if (!string.IsNullOrEmpty(instance))
			{
				this.logSettings.context += "#" + instance;
			}
		}

		public Logger(Logger parent, object owner, string instance) : this(parent, owner.GetType().FullName, instance) {}
		public Logger(Logger parent, string context) : this(parent, context, "") {}
		public Logger(Logger parent, object owner) : this(parent, owner.GetType().FullName, "") { }
		public Logger(Logger parent) : this(parent, "", "") { }



		public Logger() : this(defaultInstance) { }
		public Logger(string context) : this(defaultInstance, context) { }
		public Logger(string context, string instance) : this(defaultInstance, context, instance) { }
		public Logger(object owner) : this(defaultInstance, owner) { }
		public Logger(object owner, string instance) : this(defaultInstance, owner, instance) { }
#endregion

        /**
		 * \brief Log a Debug-level message (non-static)
		 */
        public void debug(string message)
		{
			log(Level.Debug, message);
		}

		/**
		 * \brief Log a Debug-level message (non-static)
		 */
		public void debug(string message, params object[] args)
		{
			log(Level.Debug, message, args);
		}

		/**
		 * \brief Log a Trace-level message (non-static)
		 */
		public void trace(string message)
		{
			log(Level.Trace, message);
		}

		/**
		 * \brief Log a Trace-level message (non-static)
		 */
		public void trace(string message, params object[] args)
		{
			log(Level.Trace, message, args);
		}

		/**
		 * \brief Log an Info-level message (non-static)
		 */
		public void info(string message)
		{
			log(Level.Info, message);
		}

		/**
		 * \brief Log an Info-level message (non-static)
		 */
		public void info(string message, params object[] args)
		{
			log(Level.Info, message, args);
		}

		/**
		 * \brief Log a Warning-level message (non-static)
		 */
		public void warning(string message)
		{
			log(Level.Warning, message);
		}

		/**
		 * \brief Log a Warning-level message (non-static)
		 */
		public void warning(string message, params object[] args)
		{
			log(Level.Warning, message, args);
		}

		/**
		 * \brief Log an Error-level message (non-static)
		 */
		public void error(string message)
		{
			log(Level.Error, message);
		}

		/**
		 * \brief Log an Error-level message (non-static)
		 */
		public void error(string message, params object[] args)
		{
			log(Level.Error, message, args);
		}

		/**
		 * \brief Log a Fatal-level message (non-static)
		 */
		public void fatal(string message)
		{
			log(Level.Fatal, message);
		}

		/**
		 * \brief Log a Fatal-level message (non-static)
		 */
		public void fatal(string message, params object[] args)
		{
			log(Level.Fatal, message, args);
		}

		/**
		 * \brief Log a message (non-static)
		 */
		public void log(Level level, string message)
		{
			log(level, message, null);
		}

		/**
		 * \brief Log a message (non-static)
		 */
		public void log(Level level, string message, params object[] args)
		{
			loggerCore(level, message, args);
		}

		public void openLogFile(string fileName)
		{
			logSettings.logFileName = fileName;
		}

		public void openLogFile(Level level, string fileName)
		{
			logSettings.logFileLevel = level;
			logSettings.logFileName = fileName;
		}

#region Utilities

		/**
 * \brief Translate a numeric value into a valid Level enum value
 */
		public static Level IntToLevel(int intLevel)
		{
			switch (intLevel)
			{
				case -1:
					return Level.None;
				case 0:
					return Level.Fatal;
				case 1:
					return Level.Error;
				case 2:
					return Level.Warning;
				case 3:
					return Level.Info;
				case 4:
					return Level.Trace;
				case 5:
					return Level.Debug;
				default:
					if (intLevel >= 6)
						return Level.All;
					return Level.Warning;
			}
		}

		/**
		 * \brief Translate a text value into a valid Level enum value
		 */
		public static Level StringToLevel(string strLevel)
		{
			if (strLevel == null)
				return Level.Info;

			if (strLevel.StartsWith("=") || strLevel.StartsWith(":"))
				strLevel = strLevel.Substring(1);

			int intLevel;
			if (int.TryParse(strLevel, out intLevel))
				return IntToLevel(intLevel);

			strLevel = strLevel.ToLower();
			switch (strLevel)
			{
				case "e":
				case "err":
				case "error":
					return Level.Error;
				case "w":
				case "warn":
				case "warning":
					return Level.Warning;
				case "i":
				case "info":
					return Level.Info;
				case "t":
				case "tr":
				case "trace":
					return Level.Trace;
				case "d":
				case "debug":
					return Level.Debug;
				case "a":
				case "all":
					return Level.All;
			}

			return Level.Info;
		}

#endregion

#region Static settings

        public static bool UseFifo
		{
			get
			{
				if (LogFifo != null)
					return true;
				return false;
			}
			set
			{
				if (value)
				{
					if (LogFifo == null)
					{
						LogFifo = new ObservableCollection<LogObject>();
						LogFifo.CollectionChanged += LogFifo_CollectionChanged;
					}
				}
				else
				{
					LogFifo = null;
				}
			}
		}

		static readonly ConsoleColor InitialForegroundColor = Console.ForegroundColor;
		static readonly ConsoleColor InitialBackgroundColor = Console.BackgroundColor;

		public delegate void LogCallbackDelegate(Level level, string message);
		public static LogCallbackDelegate LogCallback = null;

		public class LoggerTraceListener : TraceListener
		{
			public override void Write(string s)
			{
				Log(Level.Trace, s);
			}
			public override void WriteLine(string s)
			{
				Log(Level.Trace, s);
			}
		}

		public static void CaptureTrace()
		{
			System.Diagnostics.Trace.Listeners.Add(new LoggerTraceListener());
		}

		public class LoggerDebugListener : TraceListener
		{
			public override void Write(string s)
			{
				Log(Level.Debug, s);
			}
			public override void WriteLine(string s)
			{
				Log(Level.Debug, s);
			}
		}

		public static void CaptureDebug()
		{
#if !NET5_0_OR_GREATER
			System.Diagnostics.Debug.Listeners.Add(new LoggerDebugListener());
#else
			throw new System.ArgumentException("CaptureDebug", "NETCore 5.0");
#endif
		}

#endregion

#region Default instance and exports

		private static Logger defaultInstance = new Logger();

		/**
		 * \brief Level of details directed to Visual Studio's debug window if the program (or the library) is compiled with DEBUG active, and is launched from the IDE
		 */
		public static Level DebugLevel
		{
			get => defaultInstance.logSettings.debugLevel;
			set => defaultInstance.logSettings.debugLevel = value;
		}

		/**
		 * \brief Level of details directed to Visual Studio's output window if the program (or the library) is compiled with TRACE active, and is launched from the IDE
		 */
		public static Level TraceLevel
		{
			get => defaultInstance.logSettings.traceLevel;
			set => defaultInstance.logSettings.traceLevel = value;
		}

		/**
		 * \brief Level of details directed the console (stdout)
		 */
		public static Level ConsoleLevel
		{
			get => defaultInstance.logSettings.consoleLevel;
			set => defaultInstance.logSettings.consoleLevel = value;
		}

		/**
		 * \brief Include the time of execution in the console output
		 */
		public static bool ConsoleShowTime
		{
			get => defaultInstance.logSettings.consoleShowTime;
			set => defaultInstance.logSettings.consoleShowTime = value;
		}

		/**
		 * \brief Include the class and method (if this information is available) in the console output
		 */
		public static bool ConsoleShowContext
		{
			get => defaultInstance.logSettings.consoleShowContext;
			set => defaultInstance.logSettings.consoleShowContext = value;
		}

		/**
		 * \brief Use colors to decorate the console output
		 */
		public static bool ConsoleUseColors
		{
			get => defaultInstance.logSettings.consoleUseColors;
			set => defaultInstance.logSettings.consoleUseColors = value;
		}

		/**
		 * \brief Short cut to disable all log outputs -but console- if the program is undergoing unit tests
		 */
		public static bool ConsoleOnly
		{
			set
			{
				if (value)
				{
					defaultInstance.logSettings.debugLevel = Level.None;
					defaultInstance.logSettings.traceLevel = Level.None;
					defaultInstance.logSettings.eventLogLevel = Level.None;
					defaultInstance.logSettings.sysLogLevel = Level.None;
					defaultInstance.logSettings.gelfLevel = Level.None;
					defaultInstance.logSettings.logFileLevel = Level.None;
				}
			}
		}

		/**
		 * \brief Load the Logger settings from the program's command line
		 */
		public static void ReadArgs(string[] args)
		{
			defaultInstance.readArgs(args);
		}

		/**
		 * \brief Configure the Logger to send its messages to a GrayLog server
		 */
		public static void OpenGelf(Level level, string hostName, string applicationName, string serverName, int serverPort = 2202)
		{
			defaultInstance.openGelf(level, hostName, applicationName, serverName, serverPort);
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

		/**
		 * \brief Configure the Logger to send its messages to SpringCard's GrayLog server
		 *
		 * \warning Do not use this feature in your own application!
		 */
		public static void OpenGelf_SpringCardNet(Level level, string hostName, string applicationName)
		{
			OpenGelf(level, hostName, applicationName, "discover.logs.ovh.com", 2202);
			SetGelfConstant("X-OVH-TOKEN", "03154cce-3aa3-480e-96d0-893d51ffabdb");
		}

		/**
		 * \brief Configure the Logger to send its messages to SpringCard's GrayLog server
		 *
		 * \warning Do not use this feature in your own application!
		 */
		public static void OpenGelf_SpringCardNet(Level level, string applicationName)
		{
			OpenGelf_SpringCardNet(level, null, applicationName);
		}

		/**
		 * \brief Set a GrayLog constant parameter
		 */
		public static void SetGelfConstant(string Name, string Value)
		{
			defaultInstance.gelfConstants[Name] = Value;
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(SysLog.Facility facility, string hostName, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			defaultInstance.openSysLog(facility, hostName, applicationName, serverAddr, serverPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(SysLog.Facility facility, Level level, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(facility, null, applicationName, serverAddr, serverPort, useRfc5424);
			defaultInstance.sysLogLevel = level;
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(SysLog.Facility facility, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(facility, null, null, ServerAddr, ServerPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(SysLog.Facility facility, Level level, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(facility, null, null, ServerAddr, ServerPort, useRfc5424);
			defaultInstance.sysLogLevel = level;
		}

		/**
		 * \brief Configure the Logger to send messages to a SysLog server
		 */
		public static void OpenSysLog(string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(SysLog.Facility.LocalUse0, ServerAddr, ServerPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */
		public static void OpenSysLog(Level level, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(SysLog.Facility.LocalUse0, ServerAddr, ServerPort, useRfc5424);
			defaultInstance.sysLogLevel = level;
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(string fileName, bool useDate = false)
		{
			defaultInstance.openLogFile(fileName, useDate);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(Level level, string fileName, bool useDate = false)
		{
			defaultInstance.openLogFile(level, fileName, useDate);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(string fileName, bool useDate, int retentionWeeks)
		{
			defaultInstance.openLogFile(fileName, useDate, retentionWeeks);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(Level level, string fileName, bool useDate, int retentionWeeks)
		{
			defaultInstance.openLogFile(level, fileName, useDate, retentionWeeks);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(string fileName, int retentionWeeks)
		{
			defaultInstance.openLogFile(fileName, retentionWeeks);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(Level level, string fileName, int retentionWeeks)
		{
			defaultInstance.openLogFile(level, fileName, retentionWeeks);
		}

		/**
		 * \brief Level of details directed the file
		 */
		public static Level FileLogLevel
		{
			get => defaultInstance.logSettings.logFileLevel;
			set => defaultInstance.logSettings.logFileLevel = value;
		}

		/**
		 * \brief Log a Debug-level message
		 */
		public static void Debug(string message)
		{
			Log(Level.Debug, message);
		}

		/**
		 * \brief Log a Debug-level message
		 */
		public static void Debug(string message, params object[] args)
		{
			Log(Level.Debug, message, args);
		}

		/**
		 * \brief Log a Debug-level message (with context)
		 */
		public static void DebugEx(string context, string message)
		{
			LogEx(context, Level.Debug, message);
		}

		/**
		 * \brief Log a Debug-level message (with context)
		 */
		public static void DebugEx(string context, string message, params object[] args)
		{
			LogEx(context, Level.Debug, message, args);
		}

		/**
		 * \brief Log a Trace-level message
		 */
		public static void Trace(string message)
		{
			Log(Level.Trace, message);
		}
		/**
		 * \brief Log a Trace-level message
		 */
		public static void Trace(string message, params object[] args)
		{
			Log(Level.Trace, message, args);
		}

		/**
		 * \brief Log a Trace-level message (with context)
		 */
		public static void TraceEx(string context, string message)
		{
			LogEx(context, Level.Trace, message);
		}
		/**
		 * \brief Log a Trace-level message (with context)
		 */
		public static void TraceEx(string context, string message, params object[] args)
		{
			LogEx(context, Level.Trace, message, args);
		}

		/**
		 * \brief Log an Info-level message
		 */
		public static void Info(string message)
		{
			Log(Level.Info, message);
		}
		/**
		 * \brief Log an Info-level message
		 */
		public static void Info(string message, params object[] args)
		{
			Log(Level.Info, message, args);
		}

		/**
		 * \brief Log an Info-level message (with context)
		 */
		public static void InfoEx(string context, string message)
		{
			LogEx(context, Level.Info, message);
		}
		/**
		 * \brief Log an Info-level message (with context)
		 */
		public static void InfoEx(string context, string message, params object[] args)
		{
			LogEx(context, Level.Info, message, args);
		}

		/**
		 * \brief Log a Warning-level message
		 */
		public static void Warning(string message)
		{
			Log(Level.Warning, message);
		}
		/**
		 * \brief Log a Warning-level message
		 */
		public static void Warning(string message, params object[] args)
		{
			Log(Level.Warning, message, args);
		}

		/**
		 * \brief Log a Warning-level message (with context)
		 */
		public static void WarningEx(string context, string message)
		{
			LogEx(context, Level.Warning, message);
		}
		/**
		 * \brief Log a Warning-level message (with context)
		 */
		public static void WarningEx(string context, string message, params object[] args)
		{
			LogEx(context, Level.Warning, message, args);
		}

		/**
		 * \brief Log an Error-level message
		 */
		public static void Error(string message)
		{
			Log(Level.Error, message);
		}
		/**
		 * \brief Log an Error-level message
		 */
		public static void Error(string message, params object[] args)
		{
			Log(Level.Error, message, args);
		}

		/**
		 * \brief Log an Error-level message (with context)
		 */
		public static void ErrorEx(string context, string message)
		{
			LogEx(context, Level.Error, message);
		}
		/**
		 * \brief Log an Error-level message (with context)
		 */
		public static void ErrorEx(string context, string message, params object[] args)
		{
			LogEx(context, Level.Error, message, args);
		}

		/**
		 * \brief Log a Fatal-level message
		 */
		public static void Fatal(string message)
		{
			Log(Level.Fatal, message);
		}
		/**
		 * \brief Log a Fatal-level message
		 */
		public static void Fatal(string message, params object[] args)
		{
			Log(Level.Fatal, message, args);
		}

		/**
		 * \brief Log a Fatal-level message (with context)
		 */
		public static void FatalEx(string context, string message)
		{
			LogEx(context, Level.Fatal, message);
		}
		/**
		 * \brief Log a Fatal-level message (with context)
		 */
		public static void FatalEx(string context, string message, params object[] args)
		{
			LogEx(context, Level.Fatal, message, args);
		}

		/**
		 * \brief Log a message
		 */
		public static void Log(Level level, string message)
		{
			Log(level, message, null);
		}

		/**
		 * \brief Log a message
		 */
		public static void Log(Level level, string message, params object[] args)
		{
			defaultInstance.loggerCore(null, level, message, args);
		}

		/**
		 * \brief Log a message (with context)
		 */
		public static void LogEx(string context, Level level, string message)
		{
			defaultInstance.loggerCore(context, level, message, null);
		}

		/**
		 * \brief Log a message (with context)
		 */
		public static void LogEx(string context, Level level, string message, params object[] args)
		{
			defaultInstance.loggerCore(context, level, message, args);
		}

#endregion
	}
}
