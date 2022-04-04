namespace SpringCard.LibCs.Windows.Forms
{
    partial class InfoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.pLeft = new System.Windows.Forms.Panel();
            this.lbSubTitle = new System.Windows.Forms.Label();
            this.lbTitle = new System.Windows.Forms.Label();
            this.pBottom = new System.Windows.Forms.Panel();
            this.pButtons = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.pMain = new System.Windows.Forms.Panel();
            this.pLeft.SuspendLayout();
            this.pBottom.SuspendLayout();
            this.pButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // pLeft
            // 
            this.pLeft.BackColor = System.Drawing.SystemColors.Window;
            this.pLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pLeft.Controls.Add(this.lbSubTitle);
            this.pLeft.Controls.Add(this.lbTitle);
            this.pLeft.Dock = System.Windows.Forms.DockStyle.Top;
            this.pLeft.Location = new System.Drawing.Point(0, 0);
            this.pLeft.Name = "pLeft";
            this.pLeft.Size = new System.Drawing.Size(857, 78);
            this.pLeft.TabIndex = 5;
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
            this.pBottom.Location = new System.Drawing.Point(0, 498);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(857, 44);
            this.pBottom.TabIndex = 6;
            // 
            // pButtons
            // 
            this.pButtons.Controls.Add(this.btnClose);
            this.pButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pButtons.Location = new System.Drawing.Point(606, 0);
            this.pButtons.Name = "pButtons";
            this.pButtons.Size = new System.Drawing.Size(251, 44);
            this.pButtons.TabIndex = 3;
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(165, 8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 25);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pMain
            // 
            this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pMain.Location = new System.Drawing.Point(0, 78);
            this.pMain.Name = "pMain";
            this.pMain.Size = new System.Drawing.Size(857, 420);
            this.pMain.TabIndex = 7;
            // 
            // InfoForm
            // 
            this.AcceptButton = this.btnClose;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(857, 542);
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.pLeft);
            this.Name = "InfoForm";
            this.pLeft.ResumeLayout(false);
            this.pBottom.ResumeLayout(false);
            this.pButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pLeft;
        private System.Windows.Forms.Label lbSubTitle;
        private System.Windows.Forms.Label lbTitle;
        private System.Windows.Forms.Panel pBottom;
        private System.Windows.Forms.Panel pButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pMain;
    }
}