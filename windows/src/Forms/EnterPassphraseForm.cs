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
	/// Description of EnterPassphraseForm.
	/// </summary>
	public partial class EnterPassphraseForm : Form
	{
		public EnterPassphraseForm()
		{
			InitializeComponent();
			lbTitle.Text = T._("Encrypted content");
			lbSubTitle.Text = T._("This content is protected. Please enter the passphrase to decrypt it.");
			lbPrompt.Text = T._("Passphrase:");
			cbShowPassphrase.Text = T._("Show passphrase");
			cbRememberPassphrase.Text = T._("Remember this passphrase until I close the application");
			ePassphrase.PasswordChar = '•';
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
			lbSubTitle.Text = remark;
		}

		public void SetPrompt(string prompt)
		{
			lbPrompt.Text = prompt;
		}

		public void SetRememberable(bool yesno)
		{
			cbRememberPassphrase.Visible = yesno;
			cbRememberPassphrase.Enabled = yesno;
		}

		public void SetShowable(bool yesno)
		{
			cbShowPassphrase.Visible = yesno;
			cbShowPassphrase.Enabled = yesno;
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
				password = ePassphrase.Text;
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
			return cbRememberPassphrase.Enabled && cbRememberPassphrase.Checked;
		}

		public static bool Display(CPasswordInteractionSettings Settings, out string password, Form parent = null)
		{
			if (Settings == null)
				Settings = new CPasswordInteractionSettings();
			Settings.MustRemember = false;

			EnterPassphraseForm form = new EnterPassphraseForm();
			if (parent != null)
			{
				form.StartPosition = FormStartPosition.CenterParent;
			} else
			{
				form.StartPosition = FormStartPosition.CenterScreen;
			}

			if (Settings.Title != null)
				form.lbTitle.Text = Settings.Title;
			if (Settings.Remark != null)
				form.lbSubTitle.Text = Settings.Remark;
			form.cbRememberPassphrase.Visible = Settings.MayRemember;

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
				password = form.ePassphrase.Text;
				Settings.MustRemember = Settings.MayRemember && form.cbRememberPassphrase.Checked;
				return true;
			} else
			{
				password = null;
				return false;
			}
		}
		
		void CbShowPassphraseCheckedChanged(object sender, EventArgs e)
		{
			if (cbShowPassphrase.Checked)
			{
				ePassphrase.PasswordChar = '\0';
			}
			else
			{
				ePassphrase.PasswordChar = '•';
			}
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();	
		}

		private void EnterPassphraseForm_Shown(object sender, EventArgs e)
		{
			ePassphrase.Focus();
		}
	}
}
