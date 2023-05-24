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
	partial class ToastForm
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
            this.imgIcon = new System.Windows.Forms.PictureBox();
            this.lbMessage = new System.Windows.Forms.Label();
            this.tmrAutoClose = new System.Windows.Forms.Timer(this.components);
            this.lbNumber = new System.Windows.Forms.Label();
            this.lbIndex = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.imgIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // imgIcon
            // 
            this.imgIcon.BackColor = System.Drawing.SystemColors.Info;
            this.imgIcon.Location = new System.Drawing.Point(10, 12);
            this.imgIcon.Name = "imgIcon";
            this.imgIcon.Size = new System.Drawing.Size(32, 32);
            this.imgIcon.TabIndex = 0;
            this.imgIcon.TabStop = false;
            // 
            // lbMessage
            // 
            this.lbMessage.ForeColor = System.Drawing.SystemColors.InfoText;
            this.lbMessage.Location = new System.Drawing.Point(52, 4);
            this.lbMessage.Name = "lbMessage";
            this.lbMessage.Size = new System.Drawing.Size(297, 52);
            this.lbMessage.TabIndex = 2;
            this.lbMessage.Text = "Ligne 1\r\nLigne 2\r\nLigne 3\r\nLigne 4";
            this.lbMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tmrAutoClose
            // 
            this.tmrAutoClose.Tick += new System.EventHandler(this.tmrAutoClose_Tick);
            // 
            // lbNumber
            // 
            this.lbNumber.AutoSize = true;
            this.lbNumber.Location = new System.Drawing.Point(272, 42);
            this.lbNumber.Name = "lbNumber";
            this.lbNumber.Size = new System.Drawing.Size(16, 13);
            this.lbNumber.TabIndex = 3;
            this.lbNumber.Text = "...";
            this.lbNumber.Visible = false;
            // 
            // lbIndex
            // 
            this.lbIndex.AutoSize = true;
            this.lbIndex.Location = new System.Drawing.Point(294, 42);
            this.lbIndex.Name = "lbIndex";
            this.lbIndex.Size = new System.Drawing.Size(16, 13);
            this.lbIndex.TabIndex = 4;
            this.lbIndex.Text = "...";
            this.lbIndex.Visible = false;
            // 
            // ToastForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Info;
            this.ClientSize = new System.Drawing.Size(361, 60);
            this.Controls.Add(this.lbIndex);
            this.Controls.Add(this.lbNumber);
            this.Controls.Add(this.lbMessage);
            this.Controls.Add(this.imgIcon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ToastForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Toast dialog";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ToastForm_FormClosed);
            this.Shown += new System.EventHandler(this.ToastForm_Shown);
            this.Click += new System.EventHandler(this.ToastForm_Click);
            ((System.ComponentModel.ISupportInitialize)(this.imgIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private System.Windows.Forms.PictureBox imgIcon;
        private System.Windows.Forms.Label lbMessage;
        private System.Windows.Forms.Timer tmrAutoClose;
        private System.Windows.Forms.Label lbNumber;
        private System.Windows.Forms.Label lbIndex;
    }
}
