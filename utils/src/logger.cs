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
		private const string Context = "Logger";

		private static ObservableCollection<Entry> LogFifo = null;

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

		public Level level = Level.All;
		public string context = null;
		public string instance = null;

		/**
		 * \brief Level of details directed to Visual Studio's debug window if the program (or the library) is compiled with DEBUG active, and is launched from the IDE
		 */
		public static Level DebugLevel = Level.Debug;

		/**
		 * \brief Level of details directed to Visual Studio's output window if the program (or the library) is compiled with TRACE active, and is launched from the IDE
		 */
		public static Level TraceLevel = Level.Trace;

		[Serializable]
		public class Entry
		{
			public DateTime when { get; protected set; }
			public string context { get; protected set; }
			public Level level { get; protected set; }
			public string message { get; protected set; }

			protected Entry()
            {

            }

			internal Entry(Level level, string context, string message, params object[] args)
            {
				this.when = DateTime.UtcNow;
				this.level = level;
				this.context = context;
				if (args == null)
					this.message = message;
				else
					this.message = string.Format(message, args);
            }

			public string ToLine()
			{
				string prefix = when.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" + level.ToString() + "\t";

				if (!string.IsNullOrEmpty(context))
					prefix += context + "\t";
				else
					prefix += "\t";

				return prefix + message;
			}

			public List<string> ToLines()
            {
				List<string> result = new List<string>();

				string prefix = when.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" + level.ToString() + "\t";

				if (!string.IsNullOrEmpty(context))
					prefix += context + "\t";
				else
					prefix += "\t";

				foreach (string messageToken in message.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
				{
					result.Add(prefix + messageToken);
				}

				return result;
			}
		}

		private static ConsoleSink consoleSink = new ConsoleSink(Level.Info, true, true, true);
		private static List<Sink> sinks = new List<Sink>() { consoleSink };

		public static void AddSink(Sink sink, bool exclusive)
        {
			lock (sinks)
            {
				if (exclusive)
				{
					for (int i = sinks.Count - 1; i >= 0; i--)
					{
						if (sinks[i].GetType().FullName == sink.GetType().FullName)
							sinks.RemoveAt(i);
					}
				}
				sinks.Add(sink);
			}
        }

		public static void RemoveSink(Sink sink)
		{
			lock (sinks)
			{
				if (sinks.Contains(sink))
					sinks.Remove(sink);
			}
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
							Entry entry = LogFifo[0];
							LogFifo.RemoveAt(0);
							Core(entry);
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
		public static void ReadArgs(string[] args)
		{
			try
			{
				string fileName = null;
				Level fileLevel = Level.Trace;

				if (args != null)
				{
					for (int i = 0; i < args.Length; i++)
					{
						string s = args[i].ToLower();
						if (s.Equals("--console"))
						{
							if (ConsoleLevel < Level.Info)
								ConsoleLevel = Level.Info;
						}
						else if (s.Equals("--debug"))
						{
							if (ConsoleLevel < Level.Debug)
								ConsoleLevel = Level.Debug;
						}
						else if (s.StartsWith("--verbose") || (s.StartsWith("-v")))
						{
							if (ConsoleLevel < Level.Trace)
								ConsoleLevel = Level.Trace;
							s = s.Substring(s.Length - 1);
							if (s.StartsWith("=")) s = s.Substring(1);
							if (s.Length > 0)
							{
								int v;
								if (int.TryParse(s, out v))
									ConsoleLevel = IntToLevel(v);
							}
						}
						else if (s.StartsWith("--logfile="))
						{
							s = s.Substring(10);
							if (s.Length > 0)
								fileName = s;
						}
						else if (s.StartsWith("--loglevel="))
						{
							s = s.Substring(11);
							if (s.Length > 0)
							{
								int v;
								if (int.TryParse(s, out v))
									fileLevel = IntToLevel(v);
							}
						}
						else if (s.StartsWith("--syslog="))
                        {
							s = s.Substring(9);
							if (s.Length > 0)
								OpenSysLog(s);
						}
						else if (s.StartsWith("--syslog"))
                        {
							OpenSysLog(null);
                        }
						else if (s == "--gelf-springcard-dev")
						{
							AddSink(GelfLogSink.CreateSpringCardDev(Level.All, Environment.MachineName, null), false);
						}
						else if (s == "--gelf-springcard-prod")
						{
							AddSink(GelfLogSink.CreateSpringCardProd(Level.All, Environment.MachineName, null), false);
						}
						else if (s == "--gelf-springcard-infra")
						{
							AddSink(GelfLogSink.CreateSpringCardInfra(Level.All, Environment.MachineName, null), false);
						}
					}
				}

				if (!string.IsNullOrEmpty(fileName) && (fileLevel > Level.None))
                {
					OpenLogFile(fileLevel, fileName);
                }
			}
			catch { }
		}

#endregion

		private static void Core(Level level, string context, string instance, string message, params object[] args)
		{
			if (context == null)
				context = "";
			if (instance != null)
				context += "#" + instance;

			Entry entry = new Entry(level, context, message, args);

			if (LogFifo != null)
			{
				try
				{
					lock (LogFifo)
					{
						LogFifo.Add(entry);
					}
				}
				catch { }
			}
			else
			{
				Core(entry);
			}
		}

        private void core(Level level, string message, params object[] args)
		{
            if (level > this.level)
                return;
			Core(level, this.context, this.instance, message, args);
		}

		/**
		 * \brief Log a message
		 */
		private static void Core(Entry entry)
		{
			try
			{
				if (entry == null)
					return;

				if (entry.level <= DebugLevel)
				{
#if !NET5_0_OR_GREATER
					if (System.Diagnostics.Debug.Listeners.Count > 0)
					{
						System.Diagnostics.Debug.WriteLine(entry.message);
					}
#else
					/* no debug in netcore 5. */
					if (System.Diagnostics.Trace.Listeners.Count > 0)
					{
						System.Diagnostics.Trace.WriteLine(entry.message);
					}
#endif
				}
				else if (entry.level <= TraceLevel)
				{
					if (System.Diagnostics.Trace.Listeners.Count > 0)
					{
						System.Diagnostics.Trace.WriteLine(entry.message);
					}
				}

				lock (sinks)
				{
					foreach (Sink sink in sinks)
					{
						if (entry.level <= sink.level)
							sink.Send(entry);
					}
				}

				if (LogCallback != null)
				{
					if (entry.context == null)
					{
						LogCallback(entry.level, entry.message);
					}
					else
					{
						LogCallback(entry.level, entry.context + ": " + entry.message);
					}
				}
			}
			catch (Exception e)
			{
				consoleSink.Send(Level.Warning, Context, e.Message);
			}
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
			catch (Exception e)
            {
				Console.WriteLine("Failed RemoveDiacritics ({0})", e.Message);
			}
			return null;
		}

#region Constructors

		private string CreateContext()
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

		private void Create(Logger parent, string context, string instance)
        {
			if (parent != null)
			{
				level = parent.level;
				context = parent.context;
				instance = parent.instance;
			}

			if (context != null)
			{
				if (context.ToLower().StartsWith("springcard."))
					context = context.Substring(11);
				this.context = context;
			}

			if (this.context == null)
            {
				this.context = CreateContext();
            }

			if (instance != null)
			{
				this.instance = instance;
			}
		}

		public Logger(Logger parent, string context, string instance)
		{
			Create(parent, context, instance);
		}

		public Logger(Logger parent, object owner, string instance)
		{
			Create(parent, owner.GetType().FullName, instance);
		}

		public Logger(Logger parent, string context)
		{
			Create(parent, context, null);
		}

		public Logger(Logger parent, object owner)
		{
			Create(parent, owner.GetType().FullName, null);
		}

		public Logger(Logger parent)
		{
			Create(parent, null, null);
		}


		public Logger()
        {
			Create(null, null, null);
		}

		public Logger(string context)
        {
			Create(null, context, null);
		}

		public Logger(string context, string instance)
        {
			Create(null, context, instance);
		}

		public Logger(object owner)
        {
			Create(null, owner.GetType().FullName, null);
		}

		public Logger(object owner, string instance)
        {
			Create(null, owner.GetType().FullName, instance);
		}

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
			core(level, message, args);
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
		public static Level StringToLevel(string strLevel, bool exact = false)
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

			if (exact)
				throw new ArgumentOutOfRangeException("Invalid level");

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
						LogFifo = new ObservableCollection<Entry>();
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

#if !NET5_0_OR_GREATER
		/**
		 * \brief Load the Logger settings from the application's registry branch.
		 */
		public static void LoadConfigFromRegistry(string CompanyName, string ProductName)
		{
			try
			{
				Level level = Level.None;

				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + CompanyName + "\\" + ProductName, false);
				string s = (string)k.GetValue("VerboseLevel", "");

				int i;
				if (int.TryParse(s, out i))
				{
					level = IntToLevel(i);
				}
				else
				{
					for (i = (int)Level.Debug; i > (int)Level.None; i--)
					{
						if (s.ToLower() == ((Level)i).ToString().ToLower())
						{
							level = (Level)i;
							break;
						}
					}
				}

				consoleSink.SetLevel(level);

				string fileName = (string)k.GetValue("VerboseFile");
				if (!string.IsNullOrEmpty(fileName))
				{
					bool useDate = false;
					s = (string)k.GetValue("VerboseFileDate", "0");
					if (int.TryParse(s, out i))
					{
						if (i != 0)
						{
							useDate = true;
						}
					}
					OpenLogFile(level, fileName, useDate);
				}
			}
			catch
			{
			}
		}
#endif

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
			Core(level, null, null, message, args);
		}

		/**
		 * \brief Log a message (with context)
		 */
		public static void LogEx(string context, Level level, string message)
		{
            Core(level, context, null, message, null);
        }

		/**
		 * \brief Log a message (with context)
		 */
		public static void LogEx(string context, Level level, string message, params object[] args)
		{
            Core(level, context, null, message, args);
        }

#endregion
	}
}
