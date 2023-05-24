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
	public class LoggerReader : Logger.Sink
	{
		public class Entry : Logger.Entry
        {
            public LoggerReader reader { get; private set; }
            public string line { get; private set; }
            public InputFormat format { get; private set; }
            public bool multiline { get; private set; }
            private bool valid;

            private Entry(LoggerReader reader, Logger.Entry entry)
            {
                this.reader = reader;
                this.when = entry.when;
                this.level = entry.level;
                this.context = entry.context;
                this.message = entry.message;
                this.line = base.ToLine();
                this.valid = true;
            }

            private Entry(LoggerReader reader, string line, InputFormat format = InputFormat.SpringCardLogFile)
            {
                this.reader = reader;
                this.line = line.TrimEnd();
                this.format = format;
                switch (format)
                {
                    case InputFormat.SpringCardVerboseConsole:
                        this.valid = ParseSpringCardVerboseConsole();
                        break;
                    case InputFormat.SpringCardLogFile:
                        this.valid = ParseSpringCardLogFile();
                        break;
                }
            }

            private bool ParseSpringCardVerboseConsole()
            {
                string[] e = line.Split(new char[] { ' ' }, 3);
                level = Logger.Level.Trace;
                bool success = true;
                if (e.Length != 3)
                {
                    success = false;
                }
                if (success && (e[0].Length == 12))
                {
                    if (DateTime.TryParse(e[0], out DateTime dt))
                        when = dt;
                }
                if (success)
                {
                    context = e[1];
                }
                if (success)
                {
                    message = e[2];
                }
                return success;
            }

            private bool ParseSpringCardLogFile()
            {
                string[] e = line.Split(new char[] { '\t' }, 4);
                bool success = true;
                if (e.Length != 4)
                {
                    success = false;
                }
                if (success && (e[0].Length == 23))
                {
                    if (!DateTime.TryParse(e[0], out DateTime dt))
                        success = false;
                    else
                        when = dt;
                }
                if (success)
                {
                    try
                    {
                        level = Logger.StringToLevel(e[1], true);
                    }
                    catch
                    {
                        success = false;
                    }
                }
                if (success)
                {
                    context = e[2];
                }
                if (success)
                {
                    message = e[3];
                }
                return success;
            }

            internal void Append(string line)
            {
                line = line.TrimEnd();
                this.line += "\n" + line;
                this.message += "\n" + line;
                this.multiline = true;
            }

            public override string ToString()
            {
                return line;
            }

            public void WriteToConsole()
            {
                Logger.ConsoleSink sink = new Logger.ConsoleSink(Logger.Level.All, true, true, true, true);
                sink.Send(this);
            }

            internal static Entry Create(LoggerReader reader, string line, InputFormat format = InputFormat.SpringCardLogFile)
            {
                Entry result = new Entry(reader, line, format);
                if (!result.valid)
                    return null;
                return result;
            }

            internal static Entry Create(LoggerReader reader, Logger.Entry entry)
            {
                Entry result = new Entry(reader, entry);
                if (!result.valid)
                    return null;
                return result;
            }
        }

		public enum InputFormat
        {
            SpringCardVerboseConsole,
            SpringCardLogFile
        }

		public delegate void Callback(Entry entry);
		protected Callback callback;
        FileSystemWatcher watcher;

        public LoggerReader(Callback callback) : base(Logger.Level.All)
        {
			this.callback = callback;
        }

        internal override void Send(Logger.Entry entry)
        {
            if (callback != null)
                callback(LoggerReader.Entry.Create(this, entry));
        }

        public void BeginLive()
        {
            Logger.AddSink(this, false);
        }

        public void EndLive()
        {
            Logger.RemoveSink(this);
        }

        public void ReadLine(string line, InputFormat format = InputFormat.SpringCardLogFile)
        {
            Entry entry = Entry.Create(this, line, format);
            if (callback != null)
                callback(entry);
        }

		public void ReadFile(string fileName, InputFormat format = InputFormat.SpringCardLogFile)
        {
            Entry pendingEntry = null;

            foreach (string line in File.ReadAllLines(fileName))
            {
                Entry nextEntry = Entry.Create(this, line, format);
                if (nextEntry == null)
                {
                    /* This is the continuation of the previous entry */
                    if (pendingEntry != null)
                    {
                        pendingEntry.Append(line);
                        if (callback != null)
                            callback(pendingEntry);
                        pendingEntry = null;
                    }
                }
                else
                {
                    /* This is a new entry */
                    if (pendingEntry != null)
                        if (callback != null)
                            callback(pendingEntry);
                    pendingEntry = nextEntry;
                }
            }

            /* Don't forget last entry */
            if (pendingEntry != null)
                if (callback != null)
                    callback(pendingEntry);
        }

        public Entry ReadEntryFromLine(string line, InputFormat format = InputFormat.SpringCardLogFile)
        {
            return Entry.Create(this, line, format);
        }

        public List<Entry> ReadEntriesFromFile(string fileName, InputFormat format = InputFormat.SpringCardLogFile)
        {
            List<Entry> result = new List<Entry>();
            Entry pendingEntry = null;

            foreach (string line in File.ReadAllLines(fileName))
            {
                Entry nextEntry = Entry.Create(this, line, format);
                if (nextEntry == null)
                {
                    /* This is the continuation of the previous entry */
                    if (pendingEntry != null)
                    {
                        pendingEntry.Append(line);
                        result.Add(pendingEntry);
                        pendingEntry = null;
                    }
                }
                else
                {
                    /* This is a new entry */
                    if (pendingEntry != null)
                        result.Add(pendingEntry);
                    pendingEntry = nextEntry;
                }
            }

            /* Don't forget last entry */
            if (pendingEntry != null)
                result.Add(pendingEntry);

            return result;
        }

        public void WatchFile(string fileName, InputFormat format = InputFormat.SpringCardLogFile)
        {
            watcher = new FileSystemWatcher(fileName);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += WatcherOnChanged;
            watcher.Created += WatcherOnCreated;
            watcher.Deleted += WatcherOnDeleted;
            watcher.Renamed += WatcherOnRenamed;
            watcher.Error += WatcherOnError;
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

        }

        public void StopWatching()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }
        }

        private static void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
        }

        private static void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }

        private static void WatcherOnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void WatcherOnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }

        private static void WatcherOnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }
    }
}
