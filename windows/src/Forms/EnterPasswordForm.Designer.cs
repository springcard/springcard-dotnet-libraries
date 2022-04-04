﻿/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 30/11/2017
 * Time: 09:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SpringCard.LibCs.Windows.Forms
{
	partial class EnterPasswordForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Panel pLeft;
		private System.Windows.Forms.Label lbRemark;
		private System.Windows.Forms.Label lbTitle;
		private System.Windows.Forms.Label lbPrompt;
		private System.Windows.Forms.CheckBox cbShowPassword;
		private System.Windows.Forms.CheckBox cbRememberPassword;
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
            this.lbRemark = new System.Windows.Forms.Label();
            this.lbTitle = new System.Windows.Forms.Label();
            this.lbPrompt = new System.Windows.Forms.Label();
            this.cbShowPassword = new System.Windows.Forms.CheckBox();
            this.cbRememberPassword = new System.Windows.Forms.CheckBox();
            this.pBottom = new System.Windows.Forms.Panel();
            this.pBottomLeft = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.ePassword = new System.Windows.Forms.TextBox();
            this.pLeft.SuspendLayout();
            this.pBottom.SuspendLayout();
            this.pBottomLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // pLeft
            // 
            this.pLeft.BackColor = System.Drawing.SystemColors.Window;
            this.pLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pLeft.Controls.Add(this.lbRemark);
            this.pLeft.Controls.Add(this.lbTitle);
            this.pLeft.Dock = System.Windows.Forms.DockStyle.Top;
            this.pLeft.Location = new System.Drawing.Point(0, 0);
            this.pLeft.Name = "pLeft";
            this.pLeft.Size = new System.Drawing.Size(464, 78);
            this.pLeft.TabIndex = 5;
            // 
            // lbRemark
            // 
            this.lbRemark.Location = new System.Drawing.Point(12, 31);
            this.lbRemark.Name = "lbRemark";
            this.lbRemark.Size = new System.Drawing.Size(439, 28);
            this.lbRemark.TabIndex = 1;
            this.lbRemark.Text = "lbSubTitle";
            // 
            // lbTitle
            // 
            this.lbTitle.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbTitle.Location = new System.Drawing.Point(12, 11);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(439, 23);
            this.lbTitle.TabIndex = 0;
            this.lbTitle.Text = "lbTitle";
            // 
            // lbPrompt
            // 
            this.lbPrompt.Location = new System.Drawing.Point(12, 95);
            this.lbPrompt.Name = "lbPrompt";
            this.lbPrompt.Size = new System.Drawing.Size(100, 23);
            this.lbPrompt.TabIndex = 7;
            this.lbPrompt.Text = "Password:";
            // 
            // cbShowPassword
            // 
            this.cbShowPassword.Location = new System.Drawing.Point(12, 144);
            this.cbShowPassword.Name = "cbShowPassword";
            this.cbShowPassword.Size = new System.Drawing.Size(440, 24);
            this.cbShowPassword.TabIndex = 1;
            this.cbShowPassword.Text = "Show password";
            this.cbShowPassword.UseVisualStyleBackColor = true;
            this.cbShowPassword.CheckedChanged += new System.EventHandler(this.CbShowPasswordCheckedChanged);
            // 
            // cbRememberPassword
            // 
            this.cbRememberPassword.Location = new System.Drawing.Point(12, 165);
            this.cbRememberPassword.Name = "cbRememberPassword";
            this.cbRememberPassword.Size = new System.Drawing.Size(440, 24);
            this.cbRememberPassword.TabIndex = 2;
            this.cbRememberPassword.Text = "Remember password";
            this.cbRememberPassword.UseVisualStyleBackColor = true;
            // 
            // pBottom
            // 
            this.pBottom.Controls.Add(this.pBottomLeft);
            this.pBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pBottom.Location = new System.Drawing.Point(0, 205);
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
            // ePassword
            // 
            this.ePassword.Location = new System.Drawing.Point(144, 92);
            this.ePassword.Name = "ePassword";
            this.ePassword.Size = new System.Drawing.Size(180, 20);
            this.ePassword.TabIndex = 13;
            // 
            // EnterPasswordForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(464, 241);
            this.ControlBox = false;
            this.Controls.Add(this.ePassword);
            this.Controls.Add(this.pBottom);
            this.Controls.Add(this.cbRememberPassword);
            this.Controls.Add(this.cbShowPassword);
            this.Controls.Add(this.lbPrompt);
            this.Controls.Add(this.pLeft);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnterPasswordForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter password";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.EnterPasswordForm_Shown);
            this.pLeft.ResumeLayout(false);
            this.pBottom.ResumeLayout(false);
            this.pBottomLeft.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
        private System.Windows.Forms.TextBox ePassword;
    }
}
