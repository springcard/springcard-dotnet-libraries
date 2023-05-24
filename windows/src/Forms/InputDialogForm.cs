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
	/// Description of InputDialogForm.
	/// </summary>
	public partial class InputDialogForm : Form
	{
		private static Logger logger = new Logger(typeof(InputDialogForm).FullName);

		public InputDialogForm()
		{
			InitializeComponent();
			Text = T._("Input dialog");
			lbPrompt.Text = T._("This is a generic prompt. Please personalize.");
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
			Text = title;
		}

		public void SetPrompt(string prompt)
		{
			lbPrompt.Text = prompt;
		}

		public static bool Display(string Title, string Prompt, string Default, out string Result, Form parentForm = null)
		{
			InputDialogForm form = new InputDialogForm();

			if (Title != null)
				form.Text = Title;
			if (Prompt != null)
				form.lbPrompt.Text = Prompt;
			if (Default != null)
				form.eInput.Text = Default;

			DialogResult result;
			if (parentForm != null)
			{
				logger.debug("InputDialogForm.ShowDialog (with parent)");
				form.StartPosition = FormStartPosition.CenterParent;
				result = form.ShowDialog(parentForm);
			}
			else
			{
				logger.debug("InputDialogForm.ShowDialog (detached)");
				form.StartPosition = FormStartPosition.CenterScreen;
				result = form.ShowDialog();
			}
			logger.debug("InputDialogForm.ShowDialog -> {0}", result.ToString());

			if (result == DialogResult.OK)
			{
				Result = form.eInput.Text;
				logger.debug("InputDialogForm->input is '{0}', returning true", Result);
				return true;
			}
			else
			{
				Result = null;
				logger.debug("InputDialogForm->returning false", Result);
				return false;
			}
		}
			
		void BtnOKClick(object sender, EventArgs e)
		{
			logger.debug("InputDialogForm:OK");
			DialogResult = DialogResult.OK;
			Close();	
		}

		private void InputDialogForm_Shown(object sender, EventArgs e)
		{
			logger.debug("InputDialogForm:Shown");
			eInput.Focus();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			logger.debug("InputDialogForm:Cancel");
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
