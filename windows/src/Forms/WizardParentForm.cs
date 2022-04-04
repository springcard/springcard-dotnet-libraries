/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 21/11/2017
 * Time: 17:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
	public partial class WizardParentForm : Form, WizardParent
	{
		private static Logger logger = new Logger(typeof(WizardParentForm).FullName);
		List<WizardChild> breadcrumbs = new List<WizardChild>();
		object data;
		bool nextIsLast;
		
		public WizardParentForm(string title, WizardChild firstChild, object data = null)
		{
			InitializeComponent();
			Text = title;
			breadcrumbs.Add(firstChild);
			this.data = data;
		}
		
		void InitializeChild(WizardChild aChild)
		{
			aChild.WizardizeChild(this);
		}
		
		void Disappear(WizardChild aChild)
		{
			logger.debug("Hiding " + aChild.GetType().FullName);
			aChild.Disappear();
			lbTitle.Text = "";
			lbSubTitle.Text = "";
			btnPrev.Enabled = false;
			btnNext.Enabled = false;
			btnCancel.Enabled = false;
		}
		
		void Appear(WizardChild aChild)
		{
			InitializeChild(aChild);
			logger.debug("Showing " + aChild.GetType().FullName);
			aChild.Appear(data);
			lbTitle.Text = aChild.GetTitle();
			lbSubTitle.Text = aChild.GetSubtitle();
			if (pImage.Visible)
			{
				lbTitle.Width = pImage.Left - lbTitle.Left - 6;
				lbSubTitle.Width = pImage.Left - lbSubTitle.Left - 6;
			}
			else
			{
				lbTitle.Width = pLeft.Width - lbTitle.Left - 12;
				lbSubTitle.Width = pLeft.Width - lbSubTitle.Left - 12;
			}

		}
	
		public void GotoPrevious()
		{			
			if (breadcrumbs.Count < 2)
				return;

			nextIsLast = false;

			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			WizardChild previousChild = breadcrumbs[breadcrumbs.Count - 2];
			
			Disappear(currentChild);
			breadcrumbs.RemoveAt(breadcrumbs.Count - 1);
			Appear(previousChild);
		}
		
		public void GotoNext()
		{
			if (breadcrumbs.Count < 1)
				return;
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			WizardChild nextChild = currentChild.GetNext();
			if (nextChild == null)
				return;
			
			Disappear(currentChild);
			breadcrumbs.Add(nextChild);
			Appear(nextChild);
		}
		
		public void DisableCancel()
		{
			btnCancel.Enabled = false;
		}
		
		public void EnableCancel(WizardCancelText cancelText = WizardCancelText.Cancel)
		{
			logger.debug("\tAdding cancel button ({0})", cancelText.ToString());

			switch (cancelText)
			{
				case WizardCancelText.Cancel :
					btnCancel.Text = T._("Cancel");
					break;			
				case WizardCancelText.Close :
					btnCancel.Text = T._("Close");
					break;			
				case WizardCancelText.Exit :
					btnCancel.Text = T._("Exit");
					break;								
				case WizardCancelText.Terminate :
					btnCancel.Text = T._("Terminate");
					break;
			}
			
			btnCancel.Enabled = true;			
		}

		public void DisablePrevious()
		{
			btnPrev.Enabled = false;
		}
		
		public void EnablePrevious()
		{
			logger.debug("\tAdding previous button ({0})", "Previous");

			btnPrev.Enabled = true;
		}

		public void DisableNext()
		{
			btnNext.Enabled = false;
		}

		public void EnableNextLast(WizardNextText nextText = WizardNextText.Next)
		{
			EnableNext(nextText);
			nextIsLast = true;
			logger.debug("\tNext button is the end of the path");
		}

		public void EnableNext(WizardNextText nextText = WizardNextText.Next)
		{
			nextIsLast = false;
			logger.debug("\tAdding next button ({0})", nextText.ToString());

			switch (nextText)
			{
				case WizardNextText.Next:
					btnNext.Text = ">>";
					break;
				case WizardNextText.Confirm:
					btnNext.Text = T._("Confirm");
					break;
				case WizardNextText.Apply:
					btnNext.Text = T._("Apply");
					break;
				case WizardNextText.Save:
					btnNext.Text = T._("Save");
					break;
				case WizardNextText.Terminate:
					btnNext.Text = T._("Terminate");
					break;
			}

			btnNext.Enabled = true;
		}
		
		public void WizardizeChildForm(Form childForm)
		{
			childForm.TopLevel = false;
			childForm.FormBorderStyle = FormBorderStyle.None;
			childForm.AutoScroll = true;
			pMain.Controls.Add(childForm);
			childForm.Dock = DockStyle.Fill;			
		}

		void WizardParentFormShown(object sender, EventArgs e)
		{
			logger.debug("WizardParentFormShown");
			Appear(breadcrumbs[0]);
		}

		void BtnPrevClick(object sender, EventArgs e)
		{
			logger.debug("[<- Previous]");
			
			bool Accept = true;
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			currentChild.PreviousClicked(ref Accept);
			
			if (Accept)
				GotoPrevious();
		}
		
		void BtnNextClick(object sender, EventArgs e)
		{
			logger.debug("[-> Next]");
			
			bool Accept = true;
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			currentChild.NextClicked(ref Accept);

			if (Accept)
			{
				if (nextIsLast)
					Close();
				else
					GotoNext();
			}
		}

		void BtnCancelClick(object sender, EventArgs e)
		{
			WizardCancelAccept Accept = WizardCancelAccept.Accept;

			logger.debug("[Cancel]");
			
			WizardChild currentChild = breadcrumbs[breadcrumbs.Count - 1];
			currentChild.CancelClicked(ref Accept);
			
			if (Accept == WizardCancelAccept.Deny)
				return;
			
			Close();
		}
		
		void WizardParentFormFormClosing(object sender, FormClosingEventArgs e)
		{

		}
		
		void WizardParentFormKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.PageUp :
				case Keys.Left :
					if (btnPrev.Enabled)
						BtnPrevClick(sender, null);
					break;
					
				case Keys.PageDown :
				case Keys.Right :
					if (btnNext.Enabled)
						BtnNextClick(sender, null);
					break;					
			}
		}
		
	}
}
