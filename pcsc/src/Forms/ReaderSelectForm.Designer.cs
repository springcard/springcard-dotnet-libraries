/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/03/2012
 * Heure: 12:29
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
namespace SpringCard.PCSC.Forms
{
  partial class ReaderSelectForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReaderSelectForm));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.pTop = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.pBottom = new System.Windows.Forms.Panel();
            this.imgRefresh = new System.Windows.Forms.PictureBox();
            this.btnRefresh = new System.Windows.Forms.LinkLabel();
            this.cbRemember = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.pMain = new System.Windows.Forms.Panel();
            this.lvReaders = new System.Windows.Forms.ListView();
            this.colReaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pTop.SuspendLayout();
            this.pBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgRefresh)).BeginInit();
            this.pMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "right_circular.png");
            // 
            // pTop
            // 
            this.pTop.BackColor = System.Drawing.SystemColors.Window;
            this.pTop.Controls.Add(this.label3);
            this.pTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pTop.ForeColor = System.Drawing.Color.White;
            this.pTop.Location = new System.Drawing.Point(0, 0);
            this.pTop.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.pTop.Name = "pTop";
            this.pTop.Size = new System.Drawing.Size(684, 58);
            this.pTop.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label3.Location = new System.Drawing.Point(11, 18);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(163, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Select the PC/SC Reader :";
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
            this.pBottom.TabIndex = 2;
            // 
            // imgRefresh
            // 
            this.imgRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.imgRefresh.Image = ((System.Drawing.Image)(resources.GetObject("imgRefresh.Image")));
            this.imgRefresh.Location = new System.Drawing.Point(11, 14);
            this.imgRefresh.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.imgRefresh.Name = "imgRefresh";
            this.imgRefresh.Size = new System.Drawing.Size(16, 16);
            this.imgRefresh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgRefresh.TabIndex = 4;
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
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.TabStop = true;
            this.btnRefresh.Text = "Refresh the list";
            this.btnRefresh.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
            this.btnRefresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.BtnRefreshLinkClicked);
            // 
            // cbRemember
            // 
            this.cbRemember.AutoSize = true;
            this.cbRemember.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbRemember.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbRemember.ForeColor = System.Drawing.Color.Black;
            this.cbRemember.Location = new System.Drawing.Point(11, 33);
            this.cbRemember.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.cbRemember.Name = "cbRemember";
            this.cbRemember.Size = new System.Drawing.Size(393, 21);
            this.cbRemember.TabIndex = 2;
            this.cbRemember.Text = "Remember the settings and re-open the same reader next time";
            this.cbRemember.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.ForeColor = System.Drawing.Color.Black;
            this.btnCancel.Location = new System.Drawing.Point(589, 14);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 44);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.btnOK.ForeColor = System.Drawing.Color.Black;
            this.btnOK.Location = new System.Drawing.Point(494, 14);
            this.btnOK.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 44);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
            // 
            // pMain
            // 
            this.pMain.Controls.Add(this.lvReaders);
            this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pMain.Location = new System.Drawing.Point(0, 58);
            this.pMain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pMain.Name = "pMain";
            this.pMain.Padding = new System.Windows.Forms.Padding(10, 12, 10, 12);
            this.pMain.Size = new System.Drawing.Size(684, 277);
            this.pMain.TabIndex = 3;
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
            this.lvReaders.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvReaders.HideSelection = false;
            this.lvReaders.Location = new System.Drawing.Point(10, 12);
            this.lvReaders.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lvReaders.MultiSelect = false;
            this.lvReaders.Name = "lvReaders";
            this.lvReaders.Size = new System.Drawing.Size(664, 253);
            this.lvReaders.TabIndex = 2;
            this.lvReaders.UseCompatibleStateImageBehavior = false;
            this.lvReaders.View = System.Windows.Forms.View.Details;
            this.lvReaders.SelectedIndexChanged += new System.EventHandler(this.LvReadersSelectedIndexChanged);
            this.lvReaders.DoubleClick += new System.EventHandler(this.LvReadersDoubleClick);
            // 
            // colReaderName
            // 
            this.colReaderName.Text = "Reader name";
            this.colReaderName.Width = 564;
            // 
            // ReaderSelectForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(684, 401);
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.pTop);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReaderSelectForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reader selection";
            this.Shown += new System.EventHandler(this.ReaderSelectFormShown);
            this.pTop.ResumeLayout(false);
            this.pTop.PerformLayout();
            this.pBottom.ResumeLayout(false);
            this.pBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgRefresh)).EndInit();
            this.pMain.ResumeLayout(false);
            this.ResumeLayout(false);

    }
    private System.Windows.Forms.ImageList imageList;
    private System.Windows.Forms.ColumnHeader colReaderName;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOK;
    private System.Windows.Forms.ListView lvReaders;
    private System.Windows.Forms.Panel pMain;
    private System.Windows.Forms.Panel pTop;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Panel pBottom;
    private System.Windows.Forms.CheckBox cbRemember;
    private System.Windows.Forms.LinkLabel btnRefresh;
    private System.Windows.Forms.PictureBox imgRefresh;
  }
}
