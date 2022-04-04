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
	partial class InputDialogForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Panel pLeft;
		private System.Windows.Forms.Label lbPrompt;
		private System.Windows.Forms.Panel pBottom;
		private System.Windows.Forms.Panel pBottomLeft;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		
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
            this.pLeft = new System.Windows.Forms.Panel();
            this.lbPrompt = new System.Windows.Forms.Label();
            this.pBottom = new System.Windows.Forms.Panel();
            this.pBottomLeft = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.eInput = new System.Windows.Forms.TextBox();
            this.pLeft.SuspendLayout();
            this.pBottom.SuspendLayout();
            this.pBottomLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // pLeft
            // 
            this.pLeft.BackColor = System.Drawing.SystemColors.Window;
            this.pLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pLeft.Controls.Add(this.lbPrompt);
            this.pLeft.Dock = System.Windows.Forms.DockStyle.Top;
            this.pLeft.Location = new System.Drawing.Point(0, 0);
            this.pLeft.Name = "pLeft";
            this.pLeft.Size = new System.Drawing.Size(464, 58);
            this.pLeft.TabIndex = 5;
            // 
            // lbPrompt
            // 
            this.lbPrompt.Location = new System.Drawing.Point(12, 15);
            this.lbPrompt.Name = "lbPrompt";
            this.lbPrompt.Size = new System.Drawing.Size(439, 28);
            this.lbPrompt.TabIndex = 1;
            this.lbPrompt.Text = "lbPrompt";
            // 
            // pBottom
            // 
            this.pBottom.Controls.Add(this.pBottomLeft);
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Location = new System.Drawing.Point(0, 106);
            this.pBottom.Name = "pBottom";
            this.pBottom.Size = new System.Drawing.Size(464, 36);
            this.pBottom.TabIndex = 11;
            // 
            // pBottomLeft
            // 
            this.pBottomLeft.Controls.Add(this.btnCancel);
            this.pBottomLeft.Controls.Add(this.btnOK);
            this.pBottomLeft.Dock = System.Windows.Forms.DockStyle.Right;
            this.pBottomLeft.Location = new System.Drawing.Point(292, 0);
            this.pBottomLeft.Name = "pBottomLeft";
            this.pBottomLeft.Size = new System.Drawing.Size(172, 36);
            this.pBottomLeft.TabIndex = 3;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(84, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(3, 3);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.BtnOKClick);
            // 
            // eInput
            // 
            this.eInput.Location = new System.Drawing.Point(11, 73);
            this.eInput.Name = "eInput";
            this.eInput.Size = new System.Drawing.Size(440, 20);
            this.eInput.TabIndex = 13;
            // 
            // InputDialogForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(464, 142);
            this.ControlBox = false;
            this.Controls.Add(this.eInput);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.pLeft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputDialogForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Input dialog";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.InputDialogForm_Shown);
            this.pLeft.ResumeLayout(false);
            this.pBottom.ResumeLayout(false);
            this.pBottomLeft.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
        private System.Windows.Forms.TextBox eInput;
    }
}
