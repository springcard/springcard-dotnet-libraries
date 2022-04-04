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
	/// Description of CreatePasswordForm.
	/// </summary>
	public partial class CreatePasswordForm : Form
	{
		public CreatePasswordForm()
		{
			InitializeComponent();
			lbTitle.Text = T._("Encrypt content");
			lbRemark.Text = T._("This content will be protected by a password. It is important that you keep this password secret, and do not forget it.");
			lbPrompt.Text = T._("Password:");
			ePassword.PasswordChar = '•';
			eConfirmation.PasswordChar = '•';
			btnOK.Text = T._("OK");
			btnCancel.Text = T._("Cancel");
		}

		private Form parent;
		private bool showable;

		public void SetParent(Form parent)
		{
			this.StartPosition = FormStartPosition.CenterParent;
			this.parent = parent;
		}

		/* interface ICreatePasswordInteraction */
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
			showable = yesno;
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

			CreatePasswordForm form = new CreatePasswordForm();
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

			DialogResult result;
			if (parent != null)
			{
				result = form.ShowDialog(parent);
			} else
			{
				result = form.ShowDialog();
			}
			
			if (result == DialogResult.OK)
			{
				password = form.ePassword.Text;
				return true;
			} else
			{			
				password = null;
				return false;
			}
		}
		
		void EPasswordKeyUp(object sender, KeyEventArgs e)
		{
			btnOK.Enabled = (ePassword.Text == eConfirmation.Text);
		}
		
		void EConfirmationKeyUp(object sender, KeyEventArgs e)
		{
			btnOK.Enabled = (ePassword.Text == eConfirmation.Text);
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cbShowPassword_CheckedChanged(object sender, EventArgs e)
		{
			if (showable && cbShowPassword.Checked)
			{
				ePassword.PasswordChar = '\0';
				eConfirmation.PasswordChar = '\0';
			}
			else
			{
				ePassword.PasswordChar = '•';
				eConfirmation.PasswordChar = '•';
			}
		}

		private void CreatePasswordForm_Shown(object sender, EventArgs e)
		{
			ePassword.Focus();
		}
	}
}
