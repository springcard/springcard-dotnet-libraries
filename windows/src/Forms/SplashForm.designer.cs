/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCard.LibCs.Windows.Forms
{
	partial class SplashForm
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
            this.AutoCloseTimer = new System.Windows.Forms.Timer(this.components);
            this.lbVersion = new System.Windows.Forms.Label();
            this.lbDisclaimer1 = new System.Windows.Forms.Label();
            this.lbDisclaimer3 = new System.Windows.Forms.Label();
            this.lbDisclaimer2 = new System.Windows.Forms.Label();
            this.lbProduct = new System.Windows.Forms.Label();
            this.lbCopyright = new System.Windows.Forms.Label();
            this.pBottom = new System.Windows.Forms.Panel();
            this.lbPrivate = new System.Windows.Forms.Label();
            this.imgLogoColor = new System.Windows.Forms.PictureBox();
            this.imgLogoWhite = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogoColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogoWhite)).BeginInit();
            this.SuspendLayout();
            // 
            // AutoCloseTimer
            // 
            this.AutoCloseTimer.Enabled = true;
            this.AutoCloseTimer.Interval = 3500;
            this.AutoCloseTimer.Tick += new System.EventHandler(this.SplashFormClose);
            // 
            // lbVersion
            // 
            this.lbVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(7)))), ((int)(((byte)(20)))));
            this.lbVersion.ForeColor = System.Drawing.Color.Black;
            this.lbVersion.Location = new System.Drawing.Point(444, 376);
            this.lbVersion.Name = "lbVersion";
            this.lbVersion.Size = new System.Drawing.Size(144, 22);
            this.lbVersion.TabIndex = 1;
            this.lbVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbDisclaimer1
            // 
            this.lbDisclaimer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lbDisclaimer1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDisclaimer1.ForeColor = System.Drawing.Color.Black;
            this.lbDisclaimer1.Location = new System.Drawing.Point(0, 294);
            this.lbDisclaimer1.Name = "lbDisclaimer1";
            this.lbDisclaimer1.Size = new System.Drawing.Size(600, 23);
            this.lbDisclaimer1.TabIndex = 2;
            this.lbDisclaimer1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbDisclaimer3
            // 
            this.lbDisclaimer3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lbDisclaimer3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDisclaimer3.ForeColor = System.Drawing.Color.Black;
            this.lbDisclaimer3.Location = new System.Drawing.Point(0, 346);
            this.lbDisclaimer3.Name = "lbDisclaimer3";
            this.lbDisclaimer3.Size = new System.Drawing.Size(600, 23);
            this.lbDisclaimer3.TabIndex = 3;
            this.lbDisclaimer3.Text = "See the LICENSE.txt file or the \"About\" box for details.";
            this.lbDisclaimer3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbDisclaimer2
            // 
            this.lbDisclaimer2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lbDisclaimer2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDisclaimer2.ForeColor = System.Drawing.Color.Black;
            this.lbDisclaimer2.Location = new System.Drawing.Point(0, 320);
            this.lbDisclaimer2.Name = "lbDisclaimer2";
            this.lbDisclaimer2.Size = new System.Drawing.Size(600, 23);
            this.lbDisclaimer2.TabIndex = 4;
            this.lbDisclaimer2.Text = "This application is a free, unsupported software.";
            this.lbDisclaimer2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbProduct
            // 
            this.lbProduct.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(10)))), ((int)(((byte)(29)))));
            this.lbProduct.Font = new System.Drawing.Font("Segoe UI Semilight", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbProduct.ForeColor = System.Drawing.Color.Black;
            this.lbProduct.Location = new System.Drawing.Point(0, 130);
            this.lbProduct.Name = "lbProduct";
            this.lbProduct.Size = new System.Drawing.Size(600, 151);
            this.lbProduct.TabIndex = 5;
            this.lbProduct.Text = "Name of software";
            this.lbProduct.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbCopyright
            // 
            this.lbCopyright.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(7)))), ((int)(((byte)(20)))));
            this.lbCopyright.ForeColor = System.Drawing.Color.Black;
            this.lbCopyright.Location = new System.Drawing.Point(12, 376);
            this.lbCopyright.Name = "lbCopyright";
            this.lbCopyright.Size = new System.Drawing.Size(414, 22);
            this.lbCopyright.TabIndex = 6;
            this.lbCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pBottom
            // 
            this.pBottom.BackColor = System.Drawing.Color.Black;
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Location = new System.Drawing.Point(0, 374);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(600, 26);
            this.pBottom.TabIndex = 10;
            // 
            // lbPrivate
            // 
            this.lbPrivate.AutoSize = true;
            this.lbPrivate.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
            this.lbPrivate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbPrivate.Location = new System.Drawing.Point(221, 281);
            this.lbPrivate.Name = "lbPrivate";
            this.lbPrivate.Size = new System.Drawing.Size(158, 13);
            this.lbPrivate.TabIndex = 16;
            this.lbPrivate.Text = "SPRINGCARD PRIVATE MODE";
            this.lbPrivate.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // imgLogoColor
            // 
            this.imgLogoColor.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.logoColor;
            this.imgLogoColor.Location = new System.Drawing.Point(150, 12);
            this.imgLogoColor.Name = "imgLogoColor";
            this.imgLogoColor.Size = new System.Drawing.Size(299, 58);
            this.imgLogoColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgLogoColor.TabIndex = 17;
            this.imgLogoColor.TabStop = false;
            // 
            // imgLogoWhite
            // 
            this.imgLogoWhite.Image = global::SpringCard.LibCs.Windows.Forms.Properties.Resources.logoWhite;
            this.imgLogoWhite.Location = new System.Drawing.Point(150, 12);
            this.imgLogoWhite.Name = "imgLogoWhite";
            this.imgLogoWhite.Size = new System.Drawing.Size(299, 58);
            this.imgLogoWhite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgLogoWhite.TabIndex = 18;
            this.imgLogoWhite.TabStop = false;
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.imgLogoWhite);
            this.Controls.Add(this.lbPrivate);
            this.Controls.Add(this.imgLogoColor);
            this.Controls.Add(this.lbCopyright);
            this.Controls.Add(this.lbProduct);
            this.Controls.Add(this.lbDisclaimer2);
            this.Controls.Add(this.lbDisclaimer3);
            this.Controls.Add(this.lbDisclaimer1);
            this.Controls.Add(this.lbVersion);
            this.Controls.Add(this.pBottom);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashForm";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SplashForm_FormClosed);
            this.Click += new System.EventHandler(this.SplashFormClose);
            ((System.ComponentModel.ISupportInitialize)(this.imgLogoColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLogoWhite)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.Label lbVersion;
		private System.Windows.Forms.Timer AutoCloseTimer;
        private System.Windows.Forms.Label lbDisclaimer1;
        private System.Windows.Forms.Label lbDisclaimer3;
        private System.Windows.Forms.Label lbDisclaimer2;
        private System.Windows.Forms.Label lbProduct;
        private System.Windows.Forms.Label lbCopyright;
        private System.Windows.Forms.Panel pBottom;
        private System.Windows.Forms.Label lbPrivate;
        private System.Windows.Forms.PictureBox imgLogoColor;
        private System.Windows.Forms.PictureBox imgLogoWhite;
    }
}
