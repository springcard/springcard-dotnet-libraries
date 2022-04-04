namespace SpringCard.LibCs.Windows.Forms
{
    partial class TextForm
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
            this.eText = new System.Windows.Forms.TextBox();
            this.pLeft.SuspendLayout();
            this.pBottom.SuspendLayout();
            this.pButtons.SuspendLayout();
            this.pMain.SuspendLayout();
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
            this.pLeft.Size = new System.Drawing.Size(584, 78);
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
            this.pBottom.Location = new System.Drawing.Point(0, 717);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(584, 44);
            this.pBottom.TabIndex = 7;
            // 
            // pButtons
            // 
            this.pButtons.Controls.Add(this.btnClose);
            this.pButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.pButtons.Location = new System.Drawing.Point(333, 0);
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
            this.pMain.Controls.Add(this.eText);
            this.pMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pMain.Location = new System.Drawing.Point(0, 78);
            this.pMain.Name = "pMain";
            this.pMain.Padding = new System.Windows.Forms.Padding(6);
            this.pMain.Size = new System.Drawing.Size(584, 639);
            this.pMain.TabIndex = 8;
            // 
            // eText
            // 
            this.eText.BackColor = System.Drawing.SystemColors.Control;
            this.eText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.eText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eText.Location = new System.Drawing.Point(6, 6);
            this.eText.Multiline = true;
            this.eText.Name = "eText";
            this.eText.Size = new System.Drawing.Size(572, 627);
            this.eText.TabIndex = 0;
            // 
            // TextForm
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(584, 761);
            this.Controls.Add(this.pMain);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.pLeft);
            this.Name = "TextForm";
            this.Text = "TextForm";
            this.pLeft.ResumeLayout(false);
            this.pBottom.ResumeLayout(false);
            this.pButtons.ResumeLayout(false);
            this.pMain.ResumeLayout(false);
            this.pMain.PerformLayout();
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
        private System.Windows.Forms.TextBox eText;
    }
}