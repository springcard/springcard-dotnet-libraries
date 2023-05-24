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
	public partial class Logger : ILogger
	{
		public class FileSink : Sink
		{
			private static Mutex LogFileMutex = new Mutex();
			private DateTime retentionCheck = DateTime.Now.AddMinutes(10);
			public int retentionWeeks = 0;
			public string fileName = null;
			public bool useDate;

			public FileSink(Level level, string fileName, bool useDate, int retentionWeeks) : base(level)
			{
				this.fileName = fileName;
				this.useDate = useDate;
				this.retentionWeeks = retentionWeeks;

				try
				{
					string logFilePath = Path.GetDirectoryName(fileName);
					logFilePath = logFilePath.TrimEnd(Path.DirectorySeparatorChar);

					if (!string.IsNullOrEmpty(logFilePath))
						if (!Directory.Exists(logFilePath))
							Directory.CreateDirectory(logFilePath);
				}
				catch (Exception e)
				{
					consoleSink.Send(Level.Warning, Context, string.Format("Failed to create directory for log file {0} ({1})", fileName, e.Message));
				}
			}

			internal override void Send(Entry entry)
			{
				if (fileName == null)
					return;

				string actualFileName = fileName;
				if (useDate)
					actualFileName = FileNameWithDate(actualFileName);

				bool fMutexOwner = false;

				try
				{
					if (LogFileMutex != null)
					{
						fMutexOwner = LogFileMutex.WaitOne(500);
						if (!fMutexOwner)
							return;
					}

					File.AppendAllLines(actualFileName, entry.ToLines());
				}
				catch (Exception e)
				{
					consoleSink.Send(Level.Warning, Context, string.Format("Failed to write to file {0} ({1})", actualFileName, e.Message));
				}
				finally
				{
					if (retentionWeeks > 0)
					{
						if (DateTime.Now >= retentionCheck)
						{
							Purge();
							retentionCheck = DateTime.Now.AddHours(24);
						}
					}


					if (fMutexOwner)
						LogFileMutex.ReleaseMutex();
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

			private void Purge()
			{
				if (retentionWeeks <= 0)
					return;

				string logFilePath = Path.GetDirectoryName(fileName);
				if (!Directory.Exists(logFilePath))
					return;

				DateTime deleteBeforeTime = DateTime.Now.AddDays(-7 * retentionWeeks);

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

		}

		public static Level FileLogLevel
		{
			set
			{
				lock (sinks)
				{
					foreach (Sink sink in sinks)
					{
						if (sink is FileSink)
							sink.level = value;
					}
				}
			}
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(string fileName, bool useDate = false)
		{
			OpenLogFile(fileName, useDate, 0);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(Level level, string fileName, bool useDate = false)
		{
			OpenLogFile(level, fileName, useDate, 0);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(string fileName, bool useDate, int retentionWeeks)
		{
			OpenLogFile(Level.All, fileName, useDate, retentionWeeks);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(Level level, string fileName, bool useDate, int retentionWeeks)
		{
			AddSink(new FileSink(level, fileName, useDate, retentionWeeks), true);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(string fileName, int retentionWeeks)
		{
			OpenLogFile(fileName, false, retentionWeeks);
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */
		public static void OpenLogFile(Level level, string fileName, int retentionWeeks)
		{
			OpenLogFile(level, fileName, retentionWeeks);
		}
	}
}
