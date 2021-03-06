/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 21/11/2017
 * Time: 17:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCard.LibCs.Windows.Forms
{
	partial class WizardParentForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Panel pMain;
		private System.Windows.Forms.Panel pLeft;
		private System.Windows.Forms.Panel pBottom;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnNext;
		private System.Windows.Forms.Button btnPrev;
		private System.Windows.Forms.Label lbSubTitle;
		private System.Windows.Forms.Label lbTitle;
		
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
		
        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()		
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardParentForm));
            this.pMain = new System.Windows.Forms.Panel();
            this.pLeft = new System.Windows.Forms.Panel();
            this.lbSubTitle = new System.Windows.Forms.Label();
            this.lbTitle = new System.Windows.Forms.Label();
            this.pBottom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrev = new System.Windows.Forms.Button();
            this.pButtons = new System.Windows.Forms.Panel();
            this.pImage = new System.Windows.Forms.Panel();
            this.imgWizard = new System.Windows.Forms.PictureBox();
            this.pLeft.SuspendLayout();
            this.pBottom.SuspendLayout();
            this.pButtons.SuspendLayout();
            this.pImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgWizard)).BeginInit();
            this.SuspendLayout();
            // 
            // pMain
            // 
            this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pMain.Location = new System.Drawing.Point(0, 78);
            this.pMain.Name = "pMain";
            this.pMain.Size = new System.Drawing.Size(624, 319);
            this.pMain.TabIndex = 5;
            // 
            // pLeft
            // 
            this.pLeft.BackColor = System.Drawing.SystemColors.Window;
            this.pLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pLeft.Controls.Add(this.pImage);
            this.pLeft.Controls.Add(this.lbSubTitle);
            this.pLeft.Controls.Add(this.lbTitle);
            this.pLeft.Dock = System.Windows.Forms.DockStyle.Top;
            this.pLeft.Location = new System.Drawing.Point(0, 0);
            this.pLeft.Name = "pLeft";
            this.pLeft.Size = new System.Drawing.Size(624, 78);
            this.pLeft.TabIndex = 4;
            // 
            // lbSubTitle
            // 
            this.lbSubTitle.Location = new System.Drawing.Point(12, 31);
            this.lbSubTitle.Name = "lbSubTitle";
            this.lbSubTitle.Size = new System.Drawing.Size(535, 40);
            this.lbSubTitle.TabIndex = 1;
            this.lbSubTitle.Text = "lbSubTitle";
            // 
            // lbTitle
            // 
            this.lbTitle.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTitle.Location = new System.Drawing.Point(12, 11);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(535, 23);
            this.lbTitle.TabIndex = 0;
            this.lbTitle.Text = "lbTitle";
            // 
            // pBottom
            // 
            this.pBottom.Controls.Add(this.pButtons);
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Location = new System.Drawing.Point(0, 397);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(624, 44);
            this.pBottom.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(165, 8);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancelClick);
            // 
            // btnNext
            // 
            this.btnNext.Enabled = false;
            this.btnNext.Location = new System.Drawing.Point(84, 8);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 25);
            this.btnNext.TabIndex = 1;
            this.btnNext.Text = ">>";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.BtnNextClick);
            // 
            // btnPrev
            // 
            this.btnPrev.Enabled = false;
            this.btnPrev.Location = new System.Drawing.Point(3, 8);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(75, 25);
            this.btnPrev.TabIndex = 0;
            this.btnPrev.Text = "<<";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.BtnPrevClick);
            // 
            // pButtons
            // 
            this.pButtons.Controls.Add(this.btnPrev);
            this.pButtons.Controls.Add(this.btnCancel);
            this.pButtons.Controls.Add(this.btnNext);
            this.pButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pButtons.Location = new System.Drawing.Point(373, 0);
            this.pButtons.Name = "pButtons";
            this.pButtons.Size = new System.Drawing.Size(251, 44);
            this.pButtons.TabIndex = 3;
            // 
            // pImage
            // 
            this.pImage.Controls.Add(this.imgWizard);
            this.pImage.Dock = System.Windows.Forms.DockStyle.Right;
            this.pImage.Location = new System.Drawing.Point(546, 0);
            this.pImage.Name = "pImage";
            this.pImage.Size = new System.Drawing.Size(78, 78);
            this.pImage.TabIndex = 3;
            // 
            // imgWizard
            // 
            this.imgWizard.Image = ((System.Drawing.Image)(resources.GetObject("imgWizard.Image")));
            this.imgWizard.Location = new System.Drawing.Point(3, 7);
            this.imgWizard.Name = "imgWizard";
            this.imgWizard.Size = new System.Drawing.Size(64, 64);
            this.imgWizard.TabIndex = 3;
            this.imgWizard.TabStop = false;
            // 
            // WizardParentForm
            // 
            this.AcceptButton = this.btnNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.pLeft);
            this.Controls.Add(this.pBottom);
            this.Name = "WizardParentForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "WizardParentForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WizardParentFormFormClosing);
            this.Shown += new System.EventHandler(this.WizardParentFormShown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WizardParentFormKeyDown);
            this.pLeft.ResumeLayout(false);
            this.pBottom.ResumeLayout(false);
            this.pButtons.ResumeLayout(false);
            this.pImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgWizard)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion
        private System.Windows.Forms.Panel pButtons;
        private System.Windows.Forms.Panel pImage;
        private System.Windows.Forms.PictureBox imgWizard;
    }
}
