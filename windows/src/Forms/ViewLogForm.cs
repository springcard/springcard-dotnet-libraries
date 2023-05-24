/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 30/11/2017
 * Time: 09:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SpringCard.LibCs;

namespace SpringCard.LibCs.Windows.Forms
{
	/// <summary>
	/// Description of ViewLogForm.
	/// </summary>
	public partial class ViewLoggerForm : Form
	{
		private static Logger logger = new Logger(typeof(ViewLoggerForm).FullName);
		LoggerReader loggerReader;
		Thread loggerReaderThread;
		volatile bool running = false;
		ConcurrentQueue<LoggerReader.Entry> entriesQueue = new ConcurrentQueue<LoggerReader.Entry>();
		private EMode mode = EMode.File;

		public enum EMode
        {
			File,
			Live
		}

		static ViewLoggerForm backgroundInstance;
		static Thread backgroundThread;

		static void backgroundProc(object o)
        {
			Application.Run(backgroundInstance);
        }

		public static void ShowDetached()
		{
			if ((backgroundInstance == null) && (backgroundThread == null))
			{
				backgroundInstance = new ViewLoggerForm();
				backgroundInstance.BeginLive();
				backgroundThread = new Thread(backgroundProc);
				backgroundThread.SetApartmentState(ApartmentState.STA);
				backgroundThread.Start();
			}
		}

		public static void CanCloseDetached()
		{
			if (backgroundInstance != null)
			{
				backgroundInstance.EndLive();
			}
		}

		public static void CloseDetached()
        {
			if (backgroundInstance != null)
			{
				backgroundInstance.EndLive();
				backgroundInstance.Close();
				backgroundInstance = null;
			}
        }


		public ViewLoggerForm()
		{
			InitializeComponent();
			AppModuleInfo info = new AppModuleInfo(this.GetType().Assembly);
			lbVersionInfo.Text = string.Format("SpringCard LogViewer - {0}", info.IdentificationString, info.FullVersion);
			AppConfig.LoadFormAspect(this);
			rbTopMost.Checked = AppConfig.ReadBoolean("LogViewer.TopMost", true);
			TopMost = rbTopMost.Checked;
			rbDark.Checked = AppConfig.ReadBoolean("LogViewer.Dark", true);
			rbGotoEnd.Checked = AppConfig.ReadBoolean("LogViewer.GotoEnd", true);
			loggerReader = new LoggerReader(new LoggerReader.Callback(LoggerReaderCallback));
		}


		public void BeginLive(bool canClose = false)
        {
			this.mode = EMode.Live;
			ControlBox = canClose;
		}

		public void EndLive()
        {
			Logger.Info("(End of capture)");
			running = false;
			loggerReader.EndLive();
			ControlBox = true;
		}

		private void ViewLogForm_Shown(object sender, EventArgs e)
		{
			logger.debug("ViewLoggerForm:Shown");
			rbTopMost_CheckedChanged(sender, e);
			rbDark_CheckedChanged(sender, e);
			rbGotoEnd_CheckedChanged(sender, e);
			running = true;

			string t = AppInfo.Name + "." + FormatDateTime(DateTime.Now);
			t = t.Replace(" ", "_");
			t = t.Replace("/", "-");
			t = t.Replace(":", "-");

			saveFileDialog1.FileName = t + ".log";

			loggerReaderThread = new Thread(LoggerReaderThreadProc);
			loggerReaderThread.Start();

			if (mode == EMode.Live)
			{
				Text = string.Format("SpringCard LogViewer [LIVE] {0}", AppInfo.Name);
				btnOpen.Enabled = false;
				btnSave.Enabled = true;
				loggerReader.BeginLive();

				Logger.Info("{0}", AppInfo.IdentificationString);
				Logger.Info("Username is {0}", Environment.UserName);
				Logger.Info("Executable is {0}", AppUtils.ApplicationExeName);
				Logger.Info("Running in {0}", AppUtils.BaseDirectory);
				Logger.Info("(Capturing Logger output)");
			}
			else
            {
				ControlBox = true;
				Text = string.Format("SpringCard LogViewer");
				btnOpen.Enabled = true;
				btnSave.Enabled = false;
			}
		}

		private void ViewLogForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (mode == EMode.Live)
            {
				if (!ControlBox)
					e.Cancel = true;
			}

			if (!e.Cancel)
				running = false;
		}

		private void ViewLogForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			running = false;
			if (loggerReaderThread != null)
            {
				loggerReaderThread.Join(60);
				loggerReaderThread.Interrupt();
				loggerReaderThread.Join();
				loggerReaderThread = null;
            }
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string f = @"D:\dev\interne\internal.multiprod\_output\log\multiprod-20230119.log";
			loggerReader.ReadFile(f);
			btnSave.Enabled = true;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			string f = @"D:\dev\interne\internal.multiprod\_output\log\multiprod-20230119.log";
			loggerReader.ReadFile(f);
			btnSave.Enabled = true;
		}

		static new Font DefaultFont = new Font("Segoe UI", 8, FontStyle.Regular);

		class LoggerStyle
        {
			public Color Background { get; private set; }
			public Color Foreground { get; private set; }
			public Font Font { get; private set; }

			public LoggerStyle(Color Background, Color Foreground, FontStyle FontStyle)
            {
				this.Background = Background;
				this.Foreground = Foreground;
				this.Font = new Font(DefaultFont, FontStyle);
			}
		}

		class LoggerTheme
        {
			public Color Background { get; private set; }
			public Dictionary<Logger.Level, LoggerStyle> LevelStyles = new Dictionary<Logger.Level, LoggerStyle>();

			public LoggerTheme(Color Background, Dictionary<Logger.Level, LoggerStyle> LevelStyles)
            {
				this.Background = Background;
				this.LevelStyles = LevelStyles;
            }
		}

		static Dictionary<Logger.Level, LoggerStyle> lightStyles = new Dictionary<Logger.Level, LoggerStyle>
		{
			[Logger.Level.Debug] = new LoggerStyle(Color.White, Color.DimGray, FontStyle.Regular),
			[Logger.Level.Trace] = new LoggerStyle(Color.White, Color.Black, FontStyle.Regular),
			[Logger.Level.Info] = new LoggerStyle(Color.LightCyan, Color.MediumBlue, FontStyle.Regular),
			[Logger.Level.Warning] = new LoggerStyle(Color.Orange, Color.LightYellow, FontStyle.Bold),
			[Logger.Level.Error] = new LoggerStyle(Color.Red, Color.LightYellow, FontStyle.Bold),
			[Logger.Level.Fatal] = new LoggerStyle(Color.OrangeRed, Color.White, FontStyle.Bold),
			[Logger.Level.All] = new LoggerStyle(Color.White, Color.Black, FontStyle.Regular)
		};

		static LoggerTheme lightTheme = new LoggerTheme(Color.White, lightStyles);

		static Dictionary<Logger.Level, LoggerStyle> darkStyles = new Dictionary<Logger.Level, LoggerStyle>
		{
			[Logger.Level.Debug] = new LoggerStyle(Color.Black, Color.Gray, FontStyle.Regular),
			[Logger.Level.Trace] = new LoggerStyle(Color.Black, Color.White, FontStyle.Regular),
			[Logger.Level.Info] = new LoggerStyle(Color.Black, Color.DeepSkyBlue, FontStyle.Regular),
			[Logger.Level.Warning] = new LoggerStyle(Color.Orange, Color.LightYellow, FontStyle.Bold),
			[Logger.Level.Error] = new LoggerStyle(Color.Red, Color.LightYellow, FontStyle.Bold),
			[Logger.Level.Fatal] = new LoggerStyle(Color.OrangeRed, Color.White, FontStyle.Bold),
			[Logger.Level.All] = new LoggerStyle(Color.White, Color.Black, FontStyle.Regular)
		};

		static LoggerTheme darkTheme = new LoggerTheme(Color.Black, darkStyles);

		LoggerTheme theme = null;

		private bool IsEntryVisible(LoggerReader.Entry entry, string source, string instance)
        {
			if (rbFatal.Checked && entry.level > Logger.Level.Fatal) return false;
			if (rbError.Checked && entry.level > Logger.Level.Error) return false;
			if (rbWarning.Checked && entry.level > Logger.Level.Warning) return false;
			if (rbInfo.Checked && entry.level > Logger.Level.Info) return false;

			if (filterSource.Count > 0)
			{
				if (!string.IsNullOrEmpty(source))
				{
					if (!filterSource.Contains(source))
						return false;
				}
				else
				{
					if (!filterSource.Contains("{Empty source}"))
						return false;
				}
			}

			if (filterInstance.Count > 0)
			{
				if (!string.IsNullOrEmpty(instance))
				{
					if (!filterInstance.Contains(instance))
						return false;
				}
				else
				{
					if (!filterInstance.Contains("{Empty instance}"))
						return false;
				}
			}

			return true;
		}

		private string FormatDateTime(DateTime when)
        {
			return FormatDate(when) + " " + FormatTime(when);
		}

		private string FormatDate(DateTime when)
        {
			return string.Format("{0:D02}/{1:D02}/{2:D04}", when.Day, when.Month, when.Year);
		}

		private string FormatTime(DateTime when)
		{
			return string.Format("{0:D02}:{1:D02}:{2:D02}.{3:D03}", when.Hour, when.Minute, when.Second, when.Millisecond);
		}

		private void SplitContext(string context, out string source, out string instance)
        {
			string[] e = context.Split(new char[] { '#' }, 2);
			if (e.Length == 1)
				e = context.Split(new char[] { ':' }, 2);
			if (e.Length == 1)
            {
				source = e[0];
				instance = "";
            }
			else
            {
				source = e[0];
				instance = e[1];
			}
		}

		private void AddEntry(LoggerReader.Entry entry, ref LoggerReader.Entry previousEntry)
        {
			SplitContext(entry.context, out string source, out string instance);

			if (!IsEntryVisible(entry, source, instance))
            {
				return;
            }

			LoggerStyle loggerStyle = theme.LevelStyles[entry.level];

			ListViewItem item = new ListViewItem();

			item.BackColor = loggerStyle.Background;
			item.ForeColor = loggerStyle.Foreground;
			item.Font = loggerStyle.Font;

			/* Date */
			item.SubItems.Add(FormatDate(entry.when));
			/* Time */
			item.SubItems.Add(FormatTime(entry.when));
			/* Delta */
			if (previousEntry != null)
            {
				TimeSpan ts = entry.when - previousEntry.when;
				item.SubItems.Add(string.Format("{0}", ts.TotalMilliseconds));
			}
			else
            {
				item.SubItems.Add("");
			}
			/* Level */
			item.SubItems.Add(entry.level.ToString());
			/* Context and instance */
			item.SubItems.Add(source);
			item.SubItems.Add(instance);
			/* Message */
			item.SubItems.Add(entry.message);
			item.Tag = entry;

			lvLog.Items.Add(item);
			previousEntry = entry;
		}

		LoggerReader.Entry previousEntry = null;
		List<LoggerReader.Entry> allEntries = new List<LoggerReader.Entry>();
		List<string> filterSource = new List<string>();
		List<string> filterInstance = new List<string>();

		delegate void AddEntriesInvoker(List<LoggerReader.Entry> entries);

		private void AddEntries(List<LoggerReader.Entry> entries)
        {
			if (InvokeRequired)
            {
				Invoke(new AddEntriesInvoker(AddEntries), entries);
				return;
            }

			//lvLog.BeginUpdate();

			foreach (LoggerReader.Entry entry in entries)
			{
				allEntries.Add(entry);
				AddEntry(entry, ref previousEntry);
			}

			if (rbGotoEnd.Checked)
				GotoEndOfList();

			lbMessageCount.Text = string.Format("Messages: {0}", lvLog.Items.Count);
			//lvLog.EndUpdate();
		}

		void LoggerReaderThreadProc()
        {
            try
            {
				while (running)
                {
					while (entriesQueue.IsEmpty && running)
                    {
						Thread.Sleep(30);
                    }

					if (running)
                    {
						if (rbPlay.Checked)
						{
							List<LoggerReader.Entry> entries = new List<LoggerReader.Entry>();
							while (entriesQueue.TryDequeue(out LoggerReader.Entry entry) && (entries.Count < 1000))
								entries.Add(entry);
							AddEntries(entries);
						}
					}
				}
            }
			catch (Exception e)
            {
				Logger.Fatal(e.Message);
            }
        }

		private void LoggerReaderCallback(LoggerReader.Entry entry)
        {
			if (entry != null)
				entriesQueue.Enqueue(entry);
		}

        private void lvLog_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
			if ((e.Item != null) && (e.Item.Tag != null) && (e.Item.Tag is LoggerReader.Entry))
            {
				ToolTip tooltipDetails = new ToolTip();
				string m;
				m = string.Format("@{0} : {1}", e.Item.SubItems[2].Text, e.Item.SubItems[4].Text.ToUpper());
				m += Environment.NewLine;
				m += e.Item.SubItems[5].Text;
				m += Environment.NewLine;
				m += e.Item.SubItems[6].Text;
				m += Environment.NewLine;
				m += e.Item.SubItems[7].Text;
				tooltipDetails.Show(m, lvLog);
			}
        }

        private void lvLog_Resize(object sender, EventArgs e)
        {
			int freeWidth = lvLog.Width - 304;
			if (freeWidth <= 1280)
            {
				lvLog.Columns[5].Width = 400;
				lvLog.Columns[6].Width = 400;
				lvLog.Columns[7].Width = 480;
            }
			else if (freeWidth < 1472)
            {
				lvLog.Columns[5].Width = (400 * freeWidth) / 1280;
				lvLog.Columns[6].Width = (400 * freeWidth) / 1280;
				lvLog.Columns[7].Width = (480 * freeWidth) / 1280;
			}
			else
            {
				lvLog.Columns[5].Width = 460;
				lvLog.Columns[6].Width = 460;
				lvLog.Columns[7].Width = freeWidth - 920;
			}
		}

		void RedrawList()
        {
			if (!running)
				return;

			lvLog.BeginUpdate();
			lvLog.Items.Clear();
			previousEntry = null;
			foreach (LoggerReader.Entry entry in allEntries)
			{
				AddEntry(entry, ref previousEntry);
			}
			lvLog.EndUpdate();
		}

		void GotoEndOfList()
        {
			if (!running)
				return;

			if (rbGotoEnd.Checked)
				if (lvLog.Items.Count > 1)
					lvLog.Items[lvLog.Items.Count - 1].EnsureVisible();
		}

		private void rbAll_CheckedChanged(object sender, EventArgs e)
        {
			if (rbAll.Checked)
				RedrawList();
		}

        private void rbFatal_CheckedChanged(object sender, EventArgs e)
        {
			if (rbFatal.Checked)
				RedrawList();
		}

		private void rbError_CheckedChanged(object sender, EventArgs e)
        {
			if (rbError.Checked)
				RedrawList();
		}

		private void rbWarning_CheckedChanged(object sender, EventArgs e)
		{
			if (rbWarning.Checked)
				RedrawList();
		}

		private void rbInfo_CheckedChanged(object sender, EventArgs e)
        {
			if (rbInfo.Checked)
				RedrawList();
		}

		private void rbFilterOff_CheckedChanged(object sender, EventArgs e)
        {

		}

        private void rbFilterOn_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbGotoEnd_Click(object sender, EventArgs e)
        {
			rbGotoEnd.Checked = !rbGotoEnd.Checked;
		}

		private void rbDark_Click(object sender, EventArgs e)
		{
			rbDark.Checked = !rbDark.Checked;
		}

		private void rbTopMost_Click(object sender, EventArgs e)
		{
			rbTopMost.Checked = !rbTopMost.Checked;
		}

		private void rbFilterOn_Click(object sender, EventArgs e)
        {
			if (rbFilterOn.Checked)
            {
				ViewLoggerFilterForm f = new ViewLoggerFilterForm();

				List<string> sources = new List<string>();
				sources.Add("{Empty source}");
				List<string> instances = new List<string>();
				instances.Add("{Empty instance}");
				foreach (LoggerReader.Entry entry in allEntries)
				{
					SplitContext(entry.context, out string source, out string instance);

					if (!string.IsNullOrEmpty(source))
						if (!sources.Contains(source))
							sources.Add(source);

					if (!string.IsNullOrEmpty(instance))
						if (!instances.Contains(instance))
							instances.Add(instance);

				}
				f.SetDataAndFilters(sources, instances, filterSource, filterInstance);
				if (f.ShowDialog(this) == DialogResult.OK)
				{
					f.GetFilters(filterSource, filterInstance);
					if ((filterSource.Count == 0) && (filterInstance.Count == 0))
						rbFilterOff.Checked = true;
					RedrawList();
				}
			}
		}

        private void rbFilterOff_Click(object sender, EventArgs e)
        {
			if (rbFilterOff.Checked)
			{
				filterSource.Clear();
				filterInstance.Clear();
				RedrawList();
			}
		}

        private void ViewLogForm_Resize(object sender, EventArgs e)
        {
			AppConfig.SaveFormAspect(this);
        }

		Thread readFileThread;

		void ReadFileProc(object o)
		{
			string fileName = o as string;
			loggerReader.ReadFile(fileName);
			ReadFileDone();
		}

		delegate void ReadFileDoneInvoker();
		void ReadFileDone()
        {
			if (InvokeRequired)
            {
				Invoke(new ReadFileDoneInvoker(ReadFileDone));
				return;
            }

			Text = string.Format("SpringCard LogViewer [{0}] Read OK", Path.GetFileName(openFileDialog1.FileName));
		}

		private void btnOpen_Click(object sender, EventArgs e)
        {
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
				rbGotoEnd.Checked = false;
				Text = string.Format("SpringCard LogViewer reading...");
				lvLog.Items.Clear();			

				readFileThread = new Thread(ReadFileProc);
				readFileThread.Start(openFileDialog1.FileName);
			}
		}

        private void btnSave_Click(object sender, EventArgs e)
        {
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
				List<string> lines = new List<string>();
				foreach (LoggerReader.Entry entry in allEntries)
				{
					lines.AddRange(entry.ToLines());
				}
				File.WriteAllLines(saveFileDialog1.FileName, lines);
			}
		}

        private void rbPlay_CheckedChanged(object sender, EventArgs e)
        {
			if (rbPlay.Checked)
				if (rbGotoEnd.Checked)
					GotoEndOfList();
		}

		private void rbPause_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbGotoEnd_CheckedChanged(object sender, EventArgs e)
        {
			AppConfig.WriteBoolean("LogViewer.GotoEnd", rbGotoEnd.Checked);
			if (rbGotoEnd.Checked)
				GotoEndOfList();
		}

		private void rbTopMost_CheckedChanged(object sender, EventArgs e)
        {
			AppConfig.WriteBoolean("LogViewer.TopMost", rbTopMost.Checked);
			TopMost = rbTopMost.Checked;
		}

		private void rbDark_CheckedChanged(object sender, EventArgs e)
        {
			AppConfig.WriteBoolean("LogViewer.DarkTheme", rbDark.Checked);
			if (rbDark.Checked)
			{
				theme = darkTheme;
			}
			else
			{
				theme = lightTheme;
			}
			lvLog.BackColor = theme.Background;
			RedrawList();
			if (rbGotoEnd.Checked)
				GotoEndOfList();
		}

        private void lvLog_SelectedIndexChanged(object sender, EventArgs e)
        {
			btnCopyToClipboard.Enabled = (lvLog.SelectedItems.Count > 0);
			lbSelectedCount.Text = string.Format("Selected: {0}", lvLog.SelectedItems.Count);
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
			List<string> messages = new List<string>();

			foreach (ListViewItem item in lvLog.SelectedItems)
            {
				if (item.Tag != null)
					if (item.Tag is LoggerReader.Entry)
						messages.Add((item.Tag as LoggerReader.Entry).ToLine());
            }

			if (messages.Count > 0)
			{
				Clipboard.SetText(string.Join(Environment.NewLine, messages));
				if (messages.Count > 1)
				{
					ToastForm.Display(this, string.Format("{0} messages copied into Clipboard!", messages.Count));
				}
				else
                {
					ToastForm.Display(this, "Message copied into Clipboard!");
				}
				lvLog.SelectedItems.Clear();
			}
		}

    }
}
