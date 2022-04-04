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
	/// Description of CreatePassphraseForm.
	/// </summary>
	public partial class CreatePassphraseForm : Form
	{
		public CreatePassphraseForm()
		{
			InitializeComponent();
			lbTitle.Text = T._("Encrypt content");
			lbSubTitle.Text = T._("This content will be protected by a passphrase. It is important that you keep this passphrase secret, and do not forget it.");
			lbPrompt.Text = T._("Passphrase:");
			ePassphrase.PasswordChar = '•';
			eConfirmation.PasswordChar = '•';
			btnOK.Text = T._("OK");
			btnCancel.Text = T._("Cancel");
		}

		private Form parent;
		private bool showable;
		private int min_length = 0;
		private int max_length = 0;

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
			showable = yesno;
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

			CreatePassphraseForm form = new CreatePassphraseForm();
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
				form.lbSubTitle.Text = Settings.Remark;

			if (Settings.Prompt != null)
				form.lbPrompt.Text = Settings.Prompt;
			if (Settings.Confirm != null)
				form.lbConfirm.Text = Settings.Confirm;

			if (Settings.MinLength != 0)
            {
				if (Settings.MaxLength != 0)
                {
					form.lbHint.Text = string.Format("{0} characters min, {1} max", Settings.MinLength, Settings.MaxLength);
                }
				else
                {
					form.lbHint.Text = string.Format("{0} characters min", Settings.MinLength);
				}
            }
			form.min_length = Settings.MinLength;
			form.max_length = Settings.MaxLength;

			form.SetRememberable(Settings.MayRemember);

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
				return true;
			} else
			{
				password = null;
				return false;
			}
		}

		bool AcceptPassphrase()
        {
			if (ePassphrase.Text != eConfirmation.Text)
				return false;
			if ((min_length > 0) && (ePassphrase.Text.Length < min_length))
				return false;
			if ((max_length > 0) && (ePassphrase.Text.Length > max_length))
				return false;
			return true;
		}

		void EPassphraseKeyUp(object sender, KeyEventArgs e)
		{
			btnOK.Enabled = AcceptPassphrase();
		}
		
		void EConfirmationKeyUp(object sender, KeyEventArgs e)
		{
			btnOK.Enabled = AcceptPassphrase();
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cbShowPassphrase_CheckedChanged(object sender, EventArgs e)
		{
			if (showable && cbShowPassphrase.Checked)
			{
				ePassphrase.PasswordChar = '\0';
				eConfirmation.PasswordChar = '\0';
			}
			else
			{
				ePassphrase.PasswordChar = '•';
				eConfirmation.PasswordChar = '•';
			}
		}

		private void CreatePassphraseForm_Shown(object sender, EventArgs e)
		{
			ePassphrase.Focus();
		}
	}
}
