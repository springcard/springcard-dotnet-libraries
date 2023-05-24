/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 04/24/2015
 * Time: 13:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCard.PCSC.Forms
{
	partial class ReaderSelectAnyForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Panel pTop;
		private System.Windows.Forms.Panel pBottom;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPagePcsc;
		private System.Windows.Forms.TabPage tagPageCcidSerial;
		private System.Windows.Forms.TabPage tabPageCcidTcp;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.CheckBox cbCcidOverSerialLPCD;
		private System.Windows.Forms.CheckBox cbCcidOverSerialNotifications;
		private System.Windows.Forms.ComboBox cbCcidOverSerialCommName;
		private System.Windows.Forms.Label label1;
		
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReaderSelectAnyForm));
            this.pTop = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.pBottom = new System.Windows.Forms.Panel();
            this.imgRefresh = new System.Windows.Forms.PictureBox();
            this.btnRefresh = new System.Windows.Forms.LinkLabel();
            this.cbRemember = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPagePcsc = new System.Windows.Forms.TabPage();
            this.lvReaders = new System.Windows.Forms.ListView();
            this.colReaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tagPageCcidSerial = new System.Windows.Forms.TabPage();
            this.cbCcidOverSerialLPCD = new System.Windows.Forms.CheckBox();
            this.cbCcidOverSerialNotifications = new System.Windows.Forms.CheckBox();
            this.cbCcidOverSerialCommName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPageCcidTcp = new System.Windows.Forms.TabPage();
            this.eCcidOverTcpAddr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tagPageCcidBle = new System.Windows.Forms.TabPage();
            this.lvBleDevices = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pTop.SuspendLayout();
            this.pBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgRefresh)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPagePcsc.SuspendLayout();
            this.tagPageCcidSerial.SuspendLayout();
            this.tabPageCcidTcp.SuspendLayout();
            this.tagPageCcidBle.SuspendLayout();
            this.SuspendLayout();
            // 
            // pTop
            // 
            this.pTop.BackColor = System.Drawing.SystemColors.Window;
            this.pTop.Controls.Add(this.label3);
            this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pTop.ForeColor = System.Drawing.SystemColors.WindowText;
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pTop.Name = "pTop";
            this.pTop.Size = new System.Drawing.Size(684, 58);
            this.pTop.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.Window;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label3.Location = new System.Drawing.Point(11, 18);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(255, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Select the PC/SC or Zero-Driver Reader :";
            // 
            // pBottom
            // 
            this.pBottom.BackColor = System.Drawing.SystemColors.Control;
            this.pBottom.Controls.Add(this.imgRefresh);
            this.pBottom.Controls.Add(this.btnRefresh);
            this.pBottom.Controls.Add(this.cbRemember);
            this.pBottom.Controls.Add(this.btnCancel);
            this.pBottom.Controls.Add(this.btnOK);
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pBottom.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pBottom.Location = new System.Drawing.Point(0, 335);
            this.pBottom.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(684, 66);
            this.pBottom.TabIndex = 1;
            // 
            // imgRefresh
            // 
            this.imgRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.imgRefresh.Image = ((System.Drawing.Image)(resources.GetObject("imgRefresh.Image")));
            this.imgRefresh.Location = new System.Drawing.Point(11, 14);
            this.imgRefresh.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.imgRefresh.Name = "imgRefresh";
            this.imgRefresh.Size = new System.Drawing.Size(16, 16);
            this.imgRefresh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgRefresh.TabIndex = 6;
            this.imgRefresh.TabStop = false;
            this.imgRefresh.Click += new System.EventHandler(this.ImgRefreshClick);
            // 
            // btnRefresh
            // 
            this.btnRefresh.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
            this.btnRefresh.AutoSize = true;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.btnRefresh.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.btnRefresh.Location = new System.Drawing.Point(26, 13);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(94, 17);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.TabStop = true;
            this.btnRefresh.Text = "Refresh the list";
            this.btnRefresh.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
            this.btnRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BtnRefreshLinkClicked);
            // 
            // cbRemember
            // 
            this.cbRemember.AutoSize = true;
            this.cbRemember.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbRemember.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cbRemember.Location = new System.Drawing.Point(11, 33);
            this.cbRemember.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbRemember.Name = "cbRemember";
            this.cbRemember.Size = new System.Drawing.Size(393, 21);
            this.cbRemember.TabIndex = 2;
            this.cbRemember.Text = "Remember the settings and re-open the same reader next time";
            this.cbRemember.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            this.cbRemember.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCancel.Location = new System.Drawing.Point(589, 14);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 44);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnOK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOK.Location = new System.Drawing.Point(494, 14);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 44);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPagePcsc);
            this.tabControl1.Controls.Add(this.tagPageCcidSerial);
            this.tabControl1.Controls.Add(this.tabPageCcidTcp);
            this.tabControl1.Controls.Add(this.tagPageCcidBle);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 58);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(684, 277);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.TabControl1SelectedIndexChanged);
            // 
            // tabPagePcsc
            // 
            this.tabPagePcsc.BackColor = System.Drawing.Color.White;
            this.tabPagePcsc.Controls.Add(this.lvReaders);
            this.tabPagePcsc.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPagePcsc.Location = new System.Drawing.Point(4, 26);
            this.tabPagePcsc.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPagePcsc.Name = "tabPagePcsc";
            this.tabPagePcsc.Padding = new System.Windows.Forms.Padding(7, 7, 7, 7);
            this.tabPagePcsc.Size = new System.Drawing.Size(676, 247);
            this.tabPagePcsc.TabIndex = 0;
            this.tabPagePcsc.Text = "PC/SC Reader";
            // 
            // lvReaders
            // 
            this.lvReaders.BackColor = System.Drawing.SystemColors.Window;
            this.lvReaders.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvReaders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colReaderName});
            this.lvReaders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvReaders.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvReaders.ForeColor = System.Drawing.SystemColors.WindowText;
            this.lvReaders.FullRowSelect = true;
            this.lvReaders.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvReaders.HideSelection = false;
            this.lvReaders.Location = new System.Drawing.Point(7, 7);
            this.lvReaders.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lvReaders.MultiSelect = false;
            this.lvReaders.Name = "lvReaders";
            this.lvReaders.Size = new System.Drawing.Size(662, 233);
            this.lvReaders.TabIndex = 1;
            this.lvReaders.UseCompatibleStateImageBehavior = false;
            this.lvReaders.View = System.Windows.Forms.View.Details;
            this.lvReaders.SelectedIndexChanged += new System.EventHandler(this.LvReadersSelectedIndexChanged);
            this.lvReaders.DoubleClick += new System.EventHandler(this.LvReadersDoubleClick);
            // 
            // colReaderName
            // 
            this.colReaderName.Text = "Reader name";
            this.colReaderName.Width = 761;
            // 
            // tagPageCcidSerial
            // 
            this.tagPageCcidSerial.BackColor = System.Drawing.Color.White;
            this.tagPageCcidSerial.Controls.Add(this.cbCcidOverSerialLPCD);
            this.tagPageCcidSerial.Controls.Add(this.cbCcidOverSerialNotifications);
            this.tagPageCcidSerial.Controls.Add(this.cbCcidOverSerialCommName);
            this.tagPageCcidSerial.Controls.Add(this.label1);
            this.tagPageCcidSerial.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tagPageCcidSerial.Location = new System.Drawing.Point(4, 26);
            this.tagPageCcidSerial.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tagPageCcidSerial.Name = "tagPageCcidSerial";
            this.tagPageCcidSerial.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tagPageCcidSerial.Size = new System.Drawing.Size(678, 242);
            this.tagPageCcidSerial.TabIndex = 1;
            this.tagPageCcidSerial.Text = "CCID over Serial";
            // 
            // cbCcidOverSerialLPCD
            // 
            this.cbCcidOverSerialLPCD.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbCcidOverSerialLPCD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbCcidOverSerialLPCD.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCcidOverSerialLPCD.Location = new System.Drawing.Point(10, 121);
            this.cbCcidOverSerialLPCD.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbCcidOverSerialLPCD.Name = "cbCcidOverSerialLPCD";
            this.cbCcidOverSerialLPCD.Size = new System.Drawing.Size(220, 32);
            this.cbCcidOverSerialLPCD.TabIndex = 14;
            this.cbCcidOverSerialLPCD.Text = "Use low power polling";
            this.cbCcidOverSerialLPCD.UseVisualStyleBackColor = true;
            // 
            // cbCcidOverSerialNotifications
            // 
            this.cbCcidOverSerialNotifications.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbCcidOverSerialNotifications.Checked = true;
            this.cbCcidOverSerialNotifications.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCcidOverSerialNotifications.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbCcidOverSerialNotifications.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCcidOverSerialNotifications.Location = new System.Drawing.Point(10, 83);
            this.cbCcidOverSerialNotifications.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbCcidOverSerialNotifications.Name = "cbCcidOverSerialNotifications";
            this.cbCcidOverSerialNotifications.Size = new System.Drawing.Size(220, 32);
            this.cbCcidOverSerialNotifications.TabIndex = 13;
            this.cbCcidOverSerialNotifications.Text = "Use notifications";
            this.cbCcidOverSerialNotifications.UseVisualStyleBackColor = true;
            // 
            // cbCcidOverSerialCommName
            // 
            this.cbCcidOverSerialCommName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCcidOverSerialCommName.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbCcidOverSerialCommName.FormattingEnabled = true;
            this.cbCcidOverSerialCommName.Location = new System.Drawing.Point(207, 16);
            this.cbCcidOverSerialCommName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbCcidOverSerialCommName.Name = "cbCcidOverSerialCommName";
            this.cbCcidOverSerialCommName.Size = new System.Drawing.Size(204, 25);
            this.cbCcidOverSerialCommName.TabIndex = 12;
            this.cbCcidOverSerialCommName.SelectedIndexChanged += new System.EventHandler(this.CbCcidOverSerialCommNameSelectedIndexChanged);
            this.cbCcidOverSerialCommName.TextUpdate += new System.EventHandler(this.CbCcidOverSerialCommNameTextUpdate);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(10, 19);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Communication port:";
            // 
            // tabPageCcidTcp
            // 
            this.tabPageCcidTcp.BackColor = System.Drawing.SystemColors.Window;
            this.tabPageCcidTcp.Controls.Add(this.eCcidOverTcpAddr);
            this.tabPageCcidTcp.Controls.Add(this.label2);
            this.tabPageCcidTcp.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabPageCcidTcp.ForeColor = System.Drawing.SystemColors.WindowText;
            this.tabPageCcidTcp.Location = new System.Drawing.Point(4, 26);
            this.tabPageCcidTcp.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPageCcidTcp.Name = "tabPageCcidTcp";
            this.tabPageCcidTcp.Size = new System.Drawing.Size(678, 242);
            this.tabPageCcidTcp.TabIndex = 2;
            this.tabPageCcidTcp.Text = "CCID over TCP";
            // 
            // eCcidOverTcpAddr
            // 
            this.eCcidOverTcpAddr.Location = new System.Drawing.Point(206, 17);
            this.eCcidOverTcpAddr.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.eCcidOverTcpAddr.Name = "eCcidOverTcpAddr";
            this.eCcidOverTcpAddr.Size = new System.Drawing.Size(199, 25);
            this.eCcidOverTcpAddr.TabIndex = 1;
            this.eCcidOverTcpAddr.TextChanged += new System.EventHandler(this.ECcidOverTcpAddrTextChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(10, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(191, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Hostname or IPv4 address:";
            // 
            // tagPageCcidBle
            // 
            this.tagPageCcidBle.Controls.Add(this.lvBleDevices);
            this.tagPageCcidBle.Location = new System.Drawing.Point(4, 26);
            this.tagPageCcidBle.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tagPageCcidBle.Name = "tagPageCcidBle";
            this.tagPageCcidBle.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tagPageCcidBle.Size = new System.Drawing.Size(678, 242);
            this.tagPageCcidBle.TabIndex = 3;
            this.tagPageCcidBle.Text = "CCID over BLE";
            this.tagPageCcidBle.UseVisualStyleBackColor = true;
            // 
            // lvBleDevices
            // 
            this.lvBleDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.lvBleDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvBleDevices.FullRowSelect = true;
            this.lvBleDevices.HideSelection = false;
            this.lvBleDevices.Location = new System.Drawing.Point(3, 2);
            this.lvBleDevices.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.lvBleDevices.MultiSelect = false;
            this.lvBleDevices.Name = "lvBleDevices";
            this.lvBleDevices.Size = new System.Drawing.Size(672, 238);
            this.lvBleDevices.TabIndex = 15;
            this.lvBleDevices.UseCompatibleStateImageBehavior = false;
            this.lvBleDevices.View = System.Windows.Forms.View.Details;
            this.lvBleDevices.SelectedIndexChanged += new System.EventHandler(this.LvBleDevicesSelectedIndexChanged);
            this.lvBleDevices.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LvBleDevicesDoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "BT_ADDR";
            this.columnHeader1.Width = 140;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Device name";
            this.columnHeader2.Width = 220;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Main service UUID";
            this.columnHeader3.Width = 190;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "RSSI";
            this.columnHeader4.Width = 50;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Status";
            this.columnHeader5.Width = 220;
            // 
            // ReaderSelectAnyForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(684, 401);
            this.ControlBox = false;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.pTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReaderSelectAnyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reader selection";
            this.Shown += new System.EventHandler(this.ReaderSelectAnyFormShown);
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            this.pBottom.ResumeLayout(false);
            this.pBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgRefresh)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPagePcsc.ResumeLayout(false);
            this.tagPageCcidSerial.ResumeLayout(false);
            this.tagPageCcidSerial.PerformLayout();
            this.tabPageCcidTcp.ResumeLayout(false);
            this.tabPageCcidTcp.PerformLayout();
            this.tagPageCcidBle.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.ColumnHeader colReaderName;
		private System.Windows.Forms.ListView lvReaders;
		private System.Windows.Forms.CheckBox cbRemember;
		private System.Windows.Forms.TextBox eCcidOverTcpAddr;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox imgRefresh;
		private System.Windows.Forms.LinkLabel btnRefresh;
		private System.Windows.Forms.TabPage tagPageCcidBle;
		private System.Windows.Forms.ListView lvBleDevices;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
	}
}
