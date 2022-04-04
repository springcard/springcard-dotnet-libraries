/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 30/11/2017
 * Time: 09:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.LibCs;

namespace SpringCard.LibCs.Windows.Forms
{
	/// <summary>
	/// Description of EnterPasswordForm.
	/// </summary>
	public partial class EnterPasswordForm : Form
	{
		public EnterPasswordForm()
		{
			InitializeComponent();
			lbTitle.Text = T._("Encrypted content");
			lbRemark.Text = T._("This content is protected. Please enter the password to decrypt it.");
			lbPrompt.Text = T._("Password:");
			cbShowPassword.Text = T._("Show password");
			cbRememberPassword.Text = T._("Remember this password until I close the application");
			ePassword.PasswordChar = '•';
			btnOK.Text = T._("OK");
			btnCancel.Text = T._("Cancel");
		}

		private Form parent;

		public void SetParent(Form parent)
		{
			this.StartPosition = FormStartPosition.CenterParent;
			this.parent = parent;
		}

		/* interface IPasswordInteraction */
		/* ------------------------------ */

		public void SetTitle(string title)
		{
			lbTitle.Text = title;
		}

		public void SetRemark(string remark)
		{
			lbRemark.Text = remark;
		}

		public void SetPrompt(string prompt)
		{
			lbPrompt.Text = prompt;
		}

		public void SetRememberable(bool yesno)
		{
			cbRememberPassword.Visible = yesno;
			cbRememberPassword.Enabled = yesno;
		}

		public void SetShowable(bool yesno)
		{
			cbShowPassword.Visible = yesno;
			cbShowPassword.Enabled = yesno;
		}

		public bool ReadPassword(out string password)
		{
			DialogResult result;

			if (parent != null)
			{
				result = ShowDialog(parent);
			}
			else
			{
				result = ShowDialog();
			}

			if (result == DialogResult.OK)
			{
				password = ePassword.Text;
				return true;
			}
			else
			{
				password = null;
				return false;
			}
		}

		public bool RememberPassword()
		{
			return cbRememberPassword.Enabled && cbRememberPassword.Checked;
		}

		public static bool Display(CPasswordInteractionSettings Settings, out string password, Form parent = null)
		{
			if (Settings == null)
				Settings = new CPasswordInteractionSettings();
			Settings.MustRemember = false;

			EnterPasswordForm form = new EnterPasswordForm();
			if (parent != null)
			{
				form.Parent = parent;
				form.StartPosition = FormStartPosition.CenterParent;
			}
			else
			{
				form.StartPosition = FormStartPosition.CenterScreen;
			}

			if (Settings.Title != null)
				form.lbTitle.Text = Settings.Title;
			if (Settings.Remark != null)
				form.lbRemark.Text = Settings.Remark;
			form.cbRememberPassword.Visible = Settings.MayRemember;

			DialogResult result;
			if (parent != null)
			{
				result = form.ShowDialog(parent);
			}
			else
			{
				result = form.ShowDialog();
			}

			if (result == DialogResult.OK)
			{
				password = form.ePassword.Text;
				Settings.MustRemember = Settings.MayRemember && form.cbRememberPassword.Checked;
				return true;
			}
			else
			{
				password = null;
				return false;
			}
		}
		
		void CbShowPasswordCheckedChanged(object sender, EventArgs e)
		{
			if (cbShowPassword.Checked)
			{
				ePassword.PasswordChar = '\0';
			}
			else
			{
				ePassword.PasswordChar = '•';
			}
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();	
		}

		private void EnterPasswordForm_Shown(object sender, EventArgs e)
		{
			ePassword.Focus();
		}
	}
}
