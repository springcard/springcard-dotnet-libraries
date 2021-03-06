/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 02/03/2012
 * Time: 10:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCard.LibCs.Windows.Forms
{
	partial class AboutForm
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
			if (disposing) 
			{
				if (components != null)
					components.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.pBottom = new System.Windows.Forms.Panel();
            this.btnOK = new System.Windows.Forms.Button();
            this.linkSpringCard = new System.Windows.Forms.LinkLabel();
            this.pMain = new System.Windows.Forms.Panel();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabLibraries = new System.Windows.Forms.TabPage();
            this.lvLibraries = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabDrivers = new System.Windows.Forms.TabPage();
            this.lvDrivers = new System.Windows.Forms.ListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabSystem = new System.Windows.Forms.TabPage();
            this.lvSystem = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabLicense = new System.Windows.Forms.TabPage();
            this.lbLicense = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.tabTrademarks = new System.Windows.Forms.TabPage();
            this.lbTrademarks = new System.Windows.Forms.TextBox();
            this.tabCredits = new System.Windows.Forms.TabPage();
            this.lbCredits = new System.Windows.Forms.TextBox();
            this.tabMore = new System.Windows.Forms.TabPage();
            this.lbMore = new System.Windows.Forms.TextBox();
            this.lbCopyright = new System.Windows.Forms.Label();
            this.lbVersion = new System.Windows.Forms.Label();
            this.lbCompanyProduct = new System.Windows.Forms.Label();
            this.pHeader = new System.Windows.Forms.Panel();
            this.lbProductBig = new System.Windows.Forms.Label();
            this.pLogo = new System.Windows.Forms.Panel();
            this.imgLogoWhite = new System.Windows.Forms.PictureBox();
            this.imgLogoColor = new System.Windows.Forms.PictureBox();
            this.pTop = new System.Windows.Forms.Panel();
            this.lbPrivate = new System.Windows.Forms.Label();
            this.btnCopyInfo = new System.Windows.Forms.Button();
            this.pBottom.SuspendLayout();
            this.pMain.SuspendLayout();
            this.tabs.SuspendLayout();
            this.tabLibraries.SuspendLayout();
            this.tabDrivers.SuspendLayout();
            this.tabSystem.SuspendLayout();
            this.tabLicense.SuspendLayout();
            this.tabTrademarks.SuspendLayout();
            this.tabCredits.SuspendLayout();
            this.tabMore.SuspendLayout();
            this.pHeader.SuspendLayout();
            this.pLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogoWhite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogoColor)).BeginInit();
            this.pTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pBottom
            // 
            this.pBottom.BackColor = System.Drawing.SystemColors.Control;
            this.pBottom.Controls.Add(this.btnOK);
            this.pBottom.Controls.Add(this.linkSpringCard);
            resources.ApplyResources(this.pBottom, "pBottom");
            this.pBottom.Name = "pBottom";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // linkSpringCard
            // 
            this.linkSpringCard.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
            resources.ApplyResources(this.linkSpringCard, "linkSpringCard");
            this.linkSpringCard.DisabledLinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkSpringCard.LinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkSpringCard.Name = "linkSpringCard";
            this.linkSpringCard.TabStop = true;
            this.linkSpringCard.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
            this.linkSpringCard.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkSpringCardLinkClicked);
            // 
            // pMain
            // 
            this.pMain.BackColor = System.Drawing.SystemColors.Control;
            this.pMain.Controls.Add(this.tabs);
            resources.ApplyResources(this.pMain, "pMain");
            this.pMain.Name = "pMain";
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabLibraries);
            this.tabs.Controls.Add(this.tabDrivers);
            this.tabs.Controls.Add(this.tabSystem);
            this.tabs.Controls.Add(this.tabLicense);
            this.tabs.Controls.Add(this.tabTrademarks);
            this.tabs.Controls.Add(this.tabCredits);
            this.tabs.Controls.Add(this.tabMore);
            resources.ApplyResources(this.tabs, "tabs");
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            // 
            // tabLibraries
            // 
            this.tabLibraries.BackColor = System.Drawing.SystemColors.Control;
            this.tabLibraries.Controls.Add(this.lvLibraries);
            resources.ApplyResources(this.tabLibraries, "tabLibraries");
            this.tabLibraries.Name = "tabLibraries";
            // 
            // lvLibraries
            // 
            this.lvLibraries.BackColor = System.Drawing.SystemColors.Control;
            this.lvLibraries.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvLibraries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            resources.ApplyResources(this.lvLibraries, "lvLibraries");
            this.lvLibraries.FullRowSelect = true;
            this.lvLibraries.GridLines = true;
            this.lvLibraries.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvLibraries.HideSelection = false;
            this.lvLibraries.MultiSelect = false;
            this.lvLibraries.Name = "lvLibraries";
            this.lvLibraries.ShowGroups = false;
            this.lvLibraries.UseCompatibleStateImageBehavior = false;
            this.lvLibraries.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // tabDrivers
            // 
            this.tabDrivers.Controls.Add(this.lvDrivers);
            resources.ApplyResources(this.tabDrivers, "tabDrivers");
            this.tabDrivers.Name = "tabDrivers";
            this.tabDrivers.UseVisualStyleBackColor = true;
            // 
            // lvDrivers
            // 
            this.lvDrivers.BackColor = System.Drawing.SystemColors.Control;
            this.lvDrivers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvDrivers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11});
            resources.ApplyResources(this.lvDrivers, "lvDrivers");
            this.lvDrivers.FullRowSelect = true;
            this.lvDrivers.GridLines = true;
            this.lvDrivers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvDrivers.HideSelection = false;
            this.lvDrivers.MultiSelect = false;
            this.lvDrivers.Name = "lvDrivers";
            this.lvDrivers.ShowGroups = false;
            this.lvDrivers.UseCompatibleStateImageBehavior = false;
            this.lvDrivers.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader6
            // 
            resources.ApplyResources(this.columnHeader6, "columnHeader6");
            // 
            // columnHeader7
            // 
            resources.ApplyResources(this.columnHeader7, "columnHeader7");
            // 
            // columnHeader8
            // 
            resources.ApplyResources(this.columnHeader8, "columnHeader8");
            // 
            // columnHeader9
            // 
            resources.ApplyResources(this.columnHeader9, "columnHeader9");
            // 
            // columnHeader10
            // 
            resources.ApplyResources(this.columnHeader10, "columnHeader10");
            // 
            // columnHeader11
            // 
            resources.ApplyResources(this.columnHeader11, "columnHeader11");
            // 
            // tabSystem
            // 
            this.tabSystem.BackColor = System.Drawing.SystemColors.Control;
            this.tabSystem.Controls.Add(this.lvSystem);
            resources.ApplyResources(this.tabSystem, "tabSystem");
            this.tabSystem.Name = "tabSystem";
            // 
            // lvSystem
            // 
            this.lvSystem.BackColor = System.Drawing.SystemColors.Control;
            this.lvSystem.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvSystem.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5});
            resources.ApplyResources(this.lvSystem, "lvSystem");
            this.lvSystem.FullRowSelect = true;
            this.lvSystem.GridLines = true;
            this.lvSystem.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSystem.HideSelection = false;
            this.lvSystem.MultiSelect = false;
            this.lvSystem.Name = "lvSystem";
            this.lvSystem.ShowGroups = false;
            this.lvSystem.UseCompatibleStateImageBehavior = false;
            this.lvSystem.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // columnHeader5
            // 
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // tabLicense
            // 
            this.tabLicense.BackColor = System.Drawing.SystemColors.Control;
            this.tabLicense.Controls.Add(this.lbLicense);
            this.tabLicense.Controls.Add(this.linkLabel1);
            resources.ApplyResources(this.tabLicense, "tabLicense");
            this.tabLicense.Name = "tabLicense";
            // 
            // lbLicense
            // 
            this.lbLicense.AcceptsReturn = true;
            resources.ApplyResources(this.lbLicense, "lbLicense");
            this.lbLicense.Name = "lbLicense";
            this.lbLicense.ReadOnly = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(5)))), ((int)(((byte)(15)))));
            // 
            // tabTrademarks
            // 
            this.tabTrademarks.BackColor = System.Drawing.SystemColors.Control;
            this.tabTrademarks.Controls.Add(this.lbTrademarks);
            resources.ApplyResources(this.tabTrademarks, "tabTrademarks");
            this.tabTrademarks.Name = "tabTrademarks";
            // 
            // lbTrademarks
            // 
            this.lbTrademarks.AcceptsReturn = true;
            resources.ApplyResources(this.lbTrademarks, "lbTrademarks");
            this.lbTrademarks.Name = "lbTrademarks";
            this.lbTrademarks.ReadOnly = true;
            // 
            // tabCredits
            // 
            this.tabCredits.BackColor = System.Drawing.SystemColors.Control;
            this.tabCredits.Controls.Add(this.lbCredits);
            resources.ApplyResources(this.tabCredits, "tabCredits");
            this.tabCredits.Name = "tabCredits";
            // 
            // lbCredits
            // 
            this.lbCredits.AcceptsReturn = true;
            resources.ApplyResources(this.lbCredits, "lbCredits");
            this.lbCredits.Name = "lbCredits";
            this.lbCredits.ReadOnly = true;
            // 
            // tabMore
            // 
            this.tabMore.BackColor = System.Drawing.SystemColors.Control;
            this.tabMore.Controls.Add(this.lbMore);
            resources.ApplyResources(this.tabMore, "tabMore");
            this.tabMore.Name = "tabMore";
            // 
            // lbMore
            // 
            resources.ApplyResources(this.lbMore, "lbMore");
            this.lbMore.Name = "lbMore";
            this.lbMore.ReadOnly = true;
            // 
            // lbCopyright
            // 
            resources.ApplyResources(this.lbCopyright, "lbCopyright");
            this.lbCopyright.ForeColor = System.Drawing.Color.Black;
            this.lbCopyright.Name = "lbCopyright";
            // 
            // lbVersion
            // 
            resources.ApplyResources(this.lbVersion, "lbVersion");
            this.lbVersion.ForeColor = System.Drawing.Color.Black;
            this.lbVersion.Name = "lbVersion";
            // 
            // lbCompanyProduct
            // 
            resources.ApplyResources(this.lbCompanyProduct, "lbCompanyProduct");
            this.lbCompanyProduct.ForeColor = System.Drawing.Color.Black;
            this.lbCompanyProduct.Name = "lbCompanyProduct";
            // 
            // pHeader
            // 
            this.pHeader.BackColor = System.Drawing.Color.White;
            this.pHeader.Controls.Add(this.lbProductBig);
            this.pHeader.Controls.Add(this.pLogo);
            resources.ApplyResources(this.pHeader, "pHeader");
            this.pHeader.Name = "pHeader";
            // 
            // lbProductBig
            // 
            resources.ApplyResources(this.lbProductBig, "lbProductBig");
            this.lbProductBig.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbProductBig.Name = "lbProductBig";
            // 
            // pLogo
            // 
            this.pLogo.Controls.Add(this.imgLogoWhite);
            this.pLogo.Controls.Add(this.imgLogoColor);
            resources.ApplyResources(this.pLogo, "pLogo");
            this.pLogo.Name = "pLogo";
            // 
            // imgLogoWhite
            // 
            resources.ApplyResources(this.imgLogoWhite, "imgLogoWhite");
            this.imgLogoWhite.Name = "imgLogoWhite";
            this.imgLogoWhite.TabStop = false;
            // 
            // imgLogoColor
            // 
            resources.ApplyResources(this.imgLogoColor, "imgLogoColor");
            this.imgLogoColor.Name = "imgLogoColor";
            this.imgLogoColor.TabStop = false;
            // 
            // pTop
            // 
            this.pTop.Controls.Add(this.lbPrivate);
            this.pTop.Controls.Add(this.btnCopyInfo);
            this.pTop.Controls.Add(this.lbCompanyProduct);
            this.pTop.Controls.Add(this.lbCopyright);
            this.pTop.Controls.Add(this.lbVersion);
            resources.ApplyResources(this.pTop, "pTop");
            this.pTop.Name = "pTop";
            // 
            // lbPrivate
            // 
            resources.ApplyResources(this.lbPrivate, "lbPrivate");
            this.lbPrivate.Name = "lbPrivate";
            // 
            // btnCopyInfo
            // 
            resources.ApplyResources(this.btnCopyInfo, "btnCopyInfo");
            this.btnCopyInfo.Name = "btnCopyInfo";
            this.btnCopyInfo.UseVisualStyleBackColor = true;
            this.btnCopyInfo.Click += new System.EventHandler(this.btnCopyInfo_Click);
            // 
            // AboutForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.btnOK;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.pTop);
            this.Controls.Add(this.pHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AboutForm_FormClosed);
            this.Load += new System.EventHandler(this.AboutFormLoad);
            this.pBottom.ResumeLayout(false);
            this.pBottom.PerformLayout();
            this.pMain.ResumeLayout(false);
            this.tabs.ResumeLayout(false);
            this.tabLibraries.ResumeLayout(false);
            this.tabDrivers.ResumeLayout(false);
            this.tabSystem.ResumeLayout(false);
            this.tabLicense.ResumeLayout(false);
            this.tabLicense.PerformLayout();
            this.tabTrademarks.ResumeLayout(false);
            this.tabTrademarks.PerformLayout();
            this.tabCredits.ResumeLayout(false);
            this.tabCredits.PerformLayout();
            this.tabMore.ResumeLayout(false);
            this.tabMore.PerformLayout();
            this.pHeader.ResumeLayout(false);
            this.pHeader.PerformLayout();
            this.pLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgLogoWhite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogoColor)).EndInit();
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            this.ResumeLayout(false);

		}
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.LinkLabel linkSpringCard;
		private System.Windows.Forms.PictureBox imgLogoWhite;
		private System.Windows.Forms.Panel pLogo;
		private System.Windows.Forms.Panel pHeader;
		private System.Windows.Forms.Label lbCompanyProduct;
		private System.Windows.Forms.Label lbVersion;
		private System.Windows.Forms.Label lbCopyright;
		private System.Windows.Forms.Panel pMain;
		private System.Windows.Forms.Panel pBottom;
		private System.Windows.Forms.TabControl tabs;
		private System.Windows.Forms.TabPage tabTrademarks;
		private System.Windows.Forms.TabPage tabLicense;
		private System.Windows.Forms.TabPage tabSystem;
		private System.Windows.Forms.TabPage tabLibraries;
		private System.Windows.Forms.ListView lvLibraries;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ListView lvSystem;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.TabPage tabMore;
		private System.Windows.Forms.TabPage tabCredits;
		private System.Windows.Forms.TextBox lbLicense;
		private System.Windows.Forms.TextBox lbTrademarks;
		private System.Windows.Forms.TextBox lbCredits;
		private System.Windows.Forms.TextBox lbMore;
        private System.Windows.Forms.Label lbProductBig;
        private System.Windows.Forms.Panel pTop;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCopyInfo;
        private System.Windows.Forms.PictureBox imgLogoColor;
        private System.Windows.Forms.TabPage tabDrivers;
        private System.Windows.Forms.ListView lvDrivers;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.Label lbPrivate;
    }
}
