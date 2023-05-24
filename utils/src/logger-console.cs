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
		public class ConsoleSink : Sink
        {
			private static object ConsoleLock = new object();
			public bool showDate = false;
			public bool showTime = true;
			public bool showContext = true;
			public bool useColors = true;

			public ConsoleSink(Level level, bool showContext, bool showTime, bool useColors) : base(level)
            {
				this.showContext = showContext;
				this.showTime = showTime;
				this.useColors = useColors;
            }

			public ConsoleSink(Level level, bool showContext, bool showDate, bool showTime, bool useColors) : base(level)
			{
				this.showContext = showContext;
				this.showDate = showDate;
				this.showTime = showTime;
				this.useColors = useColors;
			}

			internal override void Send(Entry entry)
            {
				try
				{
					lock (ConsoleLock)
					{
						if (useColors)
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

						if (entry.message.Contains("\b"))
						{
							Console.SetCursorPosition(0, Console.CursorTop - 1);
						}

						if (showDate)
						{
							Console.Write("{0:D04}-{1:D02}-{2:D02} ", entry.when.Year, entry.when.Month, entry.when.Day);
						}

						if (showTime)
						{
							Console.Write("{0:D02}:{1:D02}:{2:D02}.{3:D03} ", entry.when.Hour, entry.when.Minute, entry.when.Second, entry.when.Millisecond);
						}

						if (showContext && !string.IsNullOrEmpty(entry.context))
						{
							if (useColors)
							{
								Console.ForegroundColor = ConsoleColor.DarkGray;
							}

							if (entry.context.EndsWith(":"))
								Console.Write(entry.context);
							else
								Console.Write("{0} ", entry.context);
						}

						if (useColors)
						{
							switch (entry.level)
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

						Console.Write(entry.message);

						if (useColors)
						{
							Console.BackgroundColor = InitialBackgroundColor;
							Console.ForegroundColor = InitialForegroundColor;
						}

						Console.WriteLine();
					}
				}
				catch { }
			}
        }

		/**
		 * \brief Level of details directed the console (stdout)
		 */
		public static Level ConsoleLevel
		{
			get => consoleSink.GetLevel();
			set => consoleSink.SetLevel(value);
		}

		/**
		 * \brief Include the time of execution in the console output
		 */
		public static bool ConsoleShowTime
		{
			get => consoleSink.showTime;
			set => consoleSink.showTime = value;
		}

		/**
		 * \brief Include the class and method (if this information is available) in the console output
		 */
		public static bool ConsoleShowContext
		{
			get => consoleSink.showContext;
			set => consoleSink.showContext = value;
		}

		/**
		 * \brief Use colors to decorate the console output
		 */
		public static bool ConsoleUseColors
		{
			get => consoleSink.useColors;
			set => consoleSink.useColors = value;
		}

		/**
		 * \brief Short cut to disable all log outputs -but console- if the program is undergoing unit tests
		 */
		public static void OnlyConsole()
		{
			bool found = false;
			for (int i=sinks.Count-1; i>=0; i--)
            {
				if (sinks[i] is ConsoleSink)
                {
					found = true;
                }
				else
				{
					sinks.RemoveAt(i);
				}
            }
			if (!found)
			{
				sinks.Add(consoleSink);
			}
		}

		public static void BeginConsole()
		{
			bool found = false;
			for (int i = sinks.Count - 1; i >= 0; i--)
			{
				if (sinks[i] is ConsoleSink)
					found = true;
			}
			if (!found)
            {
				sinks.Add(consoleSink);
            }
		}

		public static void EndConsole()
        {
			for (int i = sinks.Count - 1; i >= 0; i--)
			{
				if (sinks[i] is ConsoleSink)
					sinks.RemoveAt(i);
			}
		}
	}
}
