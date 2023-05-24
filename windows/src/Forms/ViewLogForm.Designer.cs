/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 30/11/2017
 * Time: 09:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCard.LibCs.Windows.Forms
{
	partial class ViewLoggerForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewLoggerForm));
            this.pTop = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.rbTopMost = new System.Windows.Forms.RadioButton();
            this.panel5 = new System.Windows.Forms.Panel();
            this.rbDark = new System.Windows.Forms.RadioButton();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.rbGotoEnd = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rbPause = new System.Windows.Forms.RadioButton();
            this.rbPlay = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rbWarning = new System.Windows.Forms.RadioButton();
            this.rbInfo = new System.Windows.Forms.RadioButton();
            this.rbError = new System.Windows.Forms.RadioButton();
            this.rbFatal = new System.Windows.Forms.RadioButton();
            this.rbAll = new System.Windows.Forms.RadioButton();
            this.pFilter = new System.Windows.Forms.Panel();
            this.rbFilterOn = new System.Windows.Forms.RadioButton();
            this.rbFilterOff = new System.Windows.Forms.RadioButton();
            this.pMain = new System.Windows.Forms.Panel();
            this.lvLog = new System.Windows.Forms.ListView();
            this.columnHeader0 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lbVersionInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.tooltipInfo = new System.Windows.Forms.ToolTip(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.lbMessageCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbSelectedCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.pTop.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.pFilter.SuspendLayout();
            this.pMain.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pTop
            // 
            this.pTop.Controls.Add(this.panel6);
            this.pTop.Controls.Add(this.panel5);
            this.pTop.Controls.Add(this.button2);
            this.pTop.Controls.Add(this.button1);
            this.pTop.Controls.Add(this.panel4);
            this.pTop.Controls.Add(this.panel3);
            this.pTop.Controls.Add(this.panel2);
            this.pTop.Controls.Add(this.panel1);
            this.pTop.Controls.Add(this.pFilter);
            this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Name = "pTop";
            this.pTop.Size = new System.Drawing.Size(1584, 44);
            this.pTop.TabIndex = 0;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.rbTopMost);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel6.Location = new System.Drawing.Point(559, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(44, 44);
            this.panel6.TabIndex = 18;
            // 
            // rbTopMost
            // 
            this.rbTopMost.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbTopMost.AutoCheck = false;
            this.rbTopMost.Checked = true;
            this.rbTopMost.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.office_push_pin;
            this.rbTopMost.Location = new System.Drawing.Point(6, 6);
            this.rbTopMost.Name = "rbTopMost";
            this.rbTopMost.Size = new System.Drawing.Size(32, 32);
            this.rbTopMost.TabIndex = 0;
            this.rbTopMost.TabStop = true;
            this.tooltipInfo.SetToolTip(this.rbTopMost, "Form stay on top");
            this.rbTopMost.UseVisualStyleBackColor = true;
            this.rbTopMost.CheckedChanged += new System.EventHandler(this.rbTopMost_CheckedChanged);
            this.rbTopMost.Click += new System.EventHandler(this.rbTopMost_Click);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.rbDark);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel5.Location = new System.Drawing.Point(515, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(44, 44);
            this.panel5.TabIndex = 17;
            // 
            // rbDark
            // 
            this.rbDark.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbDark.AutoCheck = false;
            this.rbDark.Checked = true;
            this.rbDark.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.dark_mode;
            this.rbDark.Location = new System.Drawing.Point(6, 6);
            this.rbDark.Name = "rbDark";
            this.rbDark.Size = new System.Drawing.Size(32, 32);
            this.rbDark.TabIndex = 0;
            this.rbDark.TabStop = true;
            this.tooltipInfo.SetToolTip(this.rbDark, "Scroll to last message");
            this.rbDark.UseVisualStyleBackColor = true;
            this.rbDark.CheckedChanged += new System.EventHandler(this.rbDark_CheckedChanged);
            this.rbDark.Click += new System.EventHandler(this.rbDark_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(764, 11);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 16;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(683, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 15;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnCopyToClipboard);
            this.panel4.Controls.Add(this.btnSave);
            this.panel4.Controls.Add(this.btnOpen);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel4.Location = new System.Drawing.Point(396, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(119, 44);
            this.panel4.TabIndex = 14;
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Enabled = false;
            this.btnCopyToClipboard.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.notepad;
            this.btnCopyToClipboard.Location = new System.Drawing.Point(80, 6);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(32, 32);
            this.btnCopyToClipboard.TabIndex = 2;
            this.tooltipInfo.SetToolTip(this.btnCopyToClipboard, "Copy selected messages to clipboard");
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // btnSave
            // 
            this.btnSave.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.diskette;
            this.btnSave.Location = new System.Drawing.Point(42, 6);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(32, 32);
            this.btnSave.TabIndex = 1;
            this.tooltipInfo.SetToolTip(this.btnSave, "Save messages to a log file");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.open_folder;
            this.btnOpen.Location = new System.Drawing.Point(6, 6);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(32, 32);
            this.btnOpen.TabIndex = 0;
            this.tooltipInfo.SetToolTip(this.btnOpen, "Open an existing log file");
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.rbGotoEnd);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(352, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(44, 44);
            this.panel3.TabIndex = 13;
            // 
            // rbGotoEnd
            // 
            this.rbGotoEnd.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbGotoEnd.AutoCheck = false;
            this.rbGotoEnd.Checked = true;
            this.rbGotoEnd.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.arrow;
            this.rbGotoEnd.Location = new System.Drawing.Point(6, 6);
            this.rbGotoEnd.Name = "rbGotoEnd";
            this.rbGotoEnd.Size = new System.Drawing.Size(32, 32);
            this.rbGotoEnd.TabIndex = 0;
            this.rbGotoEnd.TabStop = true;
            this.tooltipInfo.SetToolTip(this.rbGotoEnd, "Scroll to last message");
            this.rbGotoEnd.UseVisualStyleBackColor = true;
            this.rbGotoEnd.CheckedChanged += new System.EventHandler(this.rbGotoEnd_CheckedChanged);
            this.rbGotoEnd.Click += new System.EventHandler(this.rbGotoEnd_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rbPause);
            this.panel2.Controls.Add(this.rbPlay);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(275, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(77, 44);
            this.panel2.TabIndex = 12;
            // 
            // rbPause
            // 
            this.rbPause.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbPause.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.pause;
            this.rbPause.Location = new System.Drawing.Point(42, 6);
            this.rbPause.Name = "rbPause";
            this.rbPause.Size = new System.Drawing.Size(32, 32);
            this.rbPause.TabIndex = 1;
            this.tooltipInfo.SetToolTip(this.rbPause, "Suspend live capture");
            this.rbPause.UseVisualStyleBackColor = true;
            this.rbPause.CheckedChanged += new System.EventHandler(this.rbPause_CheckedChanged);
            // 
            // rbPlay
            // 
            this.rbPlay.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbPlay.Checked = true;
            this.rbPlay.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.play;
            this.rbPlay.Location = new System.Drawing.Point(6, 6);
            this.rbPlay.Name = "rbPlay";
            this.rbPlay.Size = new System.Drawing.Size(32, 32);
            this.rbPlay.TabIndex = 0;
            this.rbPlay.TabStop = true;
            this.tooltipInfo.SetToolTip(this.rbPlay, "Activate live capture");
            this.rbPlay.UseVisualStyleBackColor = true;
            this.rbPlay.CheckedChanged += new System.EventHandler(this.rbPlay_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbWarning);
            this.panel1.Controls.Add(this.rbInfo);
            this.panel1.Controls.Add(this.rbError);
            this.panel1.Controls.Add(this.rbFatal);
            this.panel1.Controls.Add(this.rbAll);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(77, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(198, 44);
            this.panel1.TabIndex = 12;
            // 
            // rbWarning
            // 
            this.rbWarning.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbWarning.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.warning;
            this.rbWarning.Location = new System.Drawing.Point(120, 6);
            this.rbWarning.Name = "rbWarning";
            this.rbWarning.Size = new System.Drawing.Size(32, 32);
            this.rbWarning.TabIndex = 3;
            this.tooltipInfo.SetToolTip(this.rbWarning, "Show only messages with level=Warning, Error or Fatal");
            this.rbWarning.UseVisualStyleBackColor = true;
            this.rbWarning.CheckedChanged += new System.EventHandler(this.rbWarning_CheckedChanged);
            // 
            // rbInfo
            // 
            this.rbInfo.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbInfo.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.info;
            this.rbInfo.Location = new System.Drawing.Point(158, 6);
            this.rbInfo.Name = "rbInfo";
            this.rbInfo.Size = new System.Drawing.Size(32, 32);
            this.rbInfo.TabIndex = 4;
            this.tooltipInfo.SetToolTip(this.rbInfo, "Show only messages with level=Info, Warning, Error or Fatal");
            this.rbInfo.UseVisualStyleBackColor = true;
            this.rbInfo.CheckedChanged += new System.EventHandler(this.rbInfo_CheckedChanged);
            // 
            // rbError
            // 
            this.rbError.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbError.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.stop;
            this.rbError.Location = new System.Drawing.Point(82, 6);
            this.rbError.Name = "rbError";
            this.rbError.Size = new System.Drawing.Size(32, 32);
            this.rbError.TabIndex = 2;
            this.tooltipInfo.SetToolTip(this.rbError, "Show only messages with level=Error or Fatal");
            this.rbError.UseVisualStyleBackColor = true;
            this.rbError.CheckedChanged += new System.EventHandler(this.rbError_CheckedChanged);
            // 
            // rbFatal
            // 
            this.rbFatal.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbFatal.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.error;
            this.rbFatal.Location = new System.Drawing.Point(44, 6);
            this.rbFatal.Name = "rbFatal";
            this.rbFatal.Size = new System.Drawing.Size(32, 32);
            this.rbFatal.TabIndex = 1;
            this.tooltipInfo.SetToolTip(this.rbFatal, "Show only messages with level=Fatal");
            this.rbFatal.UseVisualStyleBackColor = true;
            this.rbFatal.CheckedChanged += new System.EventHandler(this.rbFatal_CheckedChanged);
            // 
            // rbAll
            // 
            this.rbAll.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbAll.Checked = true;
            this.rbAll.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.all_inclusive;
            this.rbAll.Location = new System.Drawing.Point(6, 6);
            this.rbAll.Name = "rbAll";
            this.rbAll.Size = new System.Drawing.Size(32, 32);
            this.rbAll.TabIndex = 0;
            this.rbAll.TabStop = true;
            this.tooltipInfo.SetToolTip(this.rbAll, "Show all messages");
            this.rbAll.UseVisualStyleBackColor = true;
            this.rbAll.CheckedChanged += new System.EventHandler(this.rbAll_CheckedChanged);
            // 
            // pFilter
            // 
            this.pFilter.Controls.Add(this.rbFilterOn);
            this.pFilter.Controls.Add(this.rbFilterOff);
            this.pFilter.Dock = System.Windows.Forms.DockStyle.Left;
            this.pFilter.Location = new System.Drawing.Point(0, 0);
            this.pFilter.Name = "pFilter";
            this.pFilter.Size = new System.Drawing.Size(77, 44);
            this.pFilter.TabIndex = 11;
            // 
            // rbFilterOn
            // 
            this.rbFilterOn.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbFilterOn.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.filter_filled_tool_symbol;
            this.rbFilterOn.Location = new System.Drawing.Point(42, 6);
            this.rbFilterOn.Name = "rbFilterOn";
            this.rbFilterOn.Size = new System.Drawing.Size(32, 32);
            this.rbFilterOn.TabIndex = 1;
            this.tooltipInfo.SetToolTip(this.rbFilterOn, "Filter by Source and/or Instance");
            this.rbFilterOn.UseVisualStyleBackColor = true;
            this.rbFilterOn.CheckedChanged += new System.EventHandler(this.rbFilterOn_CheckedChanged);
            this.rbFilterOn.Click += new System.EventHandler(this.rbFilterOn_Click);
            // 
            // rbFilterOff
            // 
            this.rbFilterOff.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbFilterOff.Checked = true;
            this.rbFilterOff.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.filter;
            this.rbFilterOff.Location = new System.Drawing.Point(6, 6);
            this.rbFilterOff.Name = "rbFilterOff";
            this.rbFilterOff.Size = new System.Drawing.Size(32, 32);
            this.rbFilterOff.TabIndex = 0;
            this.rbFilterOff.TabStop = true;
            this.tooltipInfo.SetToolTip(this.rbFilterOff, "Remove all filters");
            this.rbFilterOff.UseVisualStyleBackColor = true;
            this.rbFilterOff.CheckedChanged += new System.EventHandler(this.rbFilterOff_CheckedChanged);
            this.rbFilterOff.Click += new System.EventHandler(this.rbFilterOff_Click);
            // 
            // pMain
            // 
            this.pMain.Controls.Add(this.lvLog);
            this.pMain.Controls.Add(this.statusStrip1);
            this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pMain.Location = new System.Drawing.Point(0, 44);
            this.pMain.Name = "pMain";
            this.pMain.Size = new System.Drawing.Size(1584, 817);
            this.pMain.TabIndex = 1;
            // 
            // lvLog
            // 
            this.lvLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader0,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
            this.lvLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLog.FullRowSelect = true;
            this.lvLog.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvLog.HideSelection = false;
            this.lvLog.Location = new System.Drawing.Point(0, 0);
            this.lvLog.Name = "lvLog";
            this.lvLog.Size = new System.Drawing.Size(1584, 795);
            this.lvLog.TabIndex = 0;
            this.lvLog.UseCompatibleStateImageBehavior = false;
            this.lvLog.View = System.Windows.Forms.View.Details;
            this.lvLog.ItemMouseHover += new System.Windows.Forms.ListViewItemMouseHoverEventHandler(this.lvLog_ItemMouseHover);
            this.lvLog.SelectedIndexChanged += new System.EventHandler(this.lvLog_SelectedIndexChanged);
            this.lvLog.Resize += new System.EventHandler(this.lvLog_Resize);
            // 
            // columnHeader0
            // 
            this.columnHeader0.Text = "";
            this.columnHeader0.Width = 0;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Date";
            this.columnHeader1.Width = 70;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Time";
            this.columnHeader2.Width = 80;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Delta (ms)";
            this.columnHeader3.Width = 65;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Level";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Source";
            this.columnHeader5.Width = 400;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Instance";
            this.columnHeader6.Width = 400;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Message";
            this.columnHeader7.Width = 480;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbVersionInfo,
            this.lbMessageCount,
            this.lbSelectedCount});
            this.statusStrip1.Location = new System.Drawing.Point(0, 795);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1584, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lbVersionInfo
            // 
            this.lbVersionInfo.Name = "lbVersionInfo";
            this.lbVersionInfo.Size = new System.Drawing.Size(66, 17);
            this.lbVersionInfo.Text = "VersionInfo";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "*.log";
            this.openFileDialog1.Filter = "Log files|*.log|All files|*.*";
            this.openFileDialog1.Title = "Open a Log File";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "*.log";
            this.saveFileDialog1.Filter = "Log files|*.log|All files|*.*";
            this.saveFileDialog1.Title = "Save to a Log File";
            // 
            // lbMessageCount
            // 
            this.lbMessageCount.Name = "lbMessageCount";
            this.lbMessageCount.Size = new System.Drawing.Size(70, 17);
            this.lbMessageCount.Text = "Messages: 0";
            // 
            // lbSelectedCount
            // 
            this.lbSelectedCount.Name = "lbSelectedCount";
            this.lbSelectedCount.Size = new System.Drawing.Size(63, 17);
            this.lbSelectedCount.Text = "Selected: 0";
            // 
            // ViewLoggerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1584, 861);
            this.ControlBox = false;
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.pTop);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewLoggerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SpringCard Log Viewer";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ViewLogForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ViewLogForm_FormClosed);
            this.Shown += new System.EventHandler(this.ViewLogForm_Shown);
            this.Resize += new System.EventHandler(this.ViewLogForm_Resize);
            this.pTop.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.pFilter.ResumeLayout(false);
            this.pMain.ResumeLayout(false);
            this.pMain.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

		}
        private System.Windows.Forms.Panel pTop;
        private System.Windows.Forms.Panel pMain;
        private System.Windows.Forms.ListView lvLog;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader0;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton rbGotoEnd;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rbPause;
        private System.Windows.Forms.RadioButton rbPlay;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rbInfo;
        private System.Windows.Forms.RadioButton rbError;
        private System.Windows.Forms.RadioButton rbFatal;
        private System.Windows.Forms.RadioButton rbAll;
        private System.Windows.Forms.Panel pFilter;
        private System.Windows.Forms.RadioButton rbFilterOn;
        private System.Windows.Forms.RadioButton rbFilterOff;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.RadioButton rbWarning;
        private System.Windows.Forms.ToolTip tooltipInfo;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.RadioButton rbDark;
        private System.Windows.Forms.ToolStripStatusLabel lbVersionInfo;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.RadioButton rbTopMost;
        private System.Windows.Forms.Button btnCopyToClipboard;
        private System.Windows.Forms.ToolStripStatusLabel lbMessageCount;
        private System.Windows.Forms.ToolStripStatusLabel lbSelectedCount;
    }
}
