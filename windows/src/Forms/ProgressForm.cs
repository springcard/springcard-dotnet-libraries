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
	/// Description of ProgressForm.
	/// </summary>
	public partial class ProgressForm : Form, IProgressInteraction
	{
		public ProgressForm()
		{
			InitializeComponent();
			Text = T._("Please wait...");
			lbTitle.Text = T._("Please wait...");
			lbSubTitle.Text = T._("Please wait...");
			btnCancel.Text = T._("Cancel");
		}

		static ProgressForm instance;

		public static void Create()
		{
			if (instance == null)
				instance = new ProgressForm();
		}

		public void SetTitle(string title, string subtitle)
		{
			lbTitle.Text = title;
			lbSubTitle.Text = subtitle;
			if (Visible)
				Update();
		}

		public void SetTitle(string title)
		{
			lbTitle.Text = title;
			lbSubTitle.Text = T._("Please wait...");
			if (Visible)
				Update();
		}

		public void SetSubTitle(string subtitle)
		{
			lbSubTitle.Text = subtitle;
			if (Visible)
				Update();
		}

		public bool ProgressBegin()
		{
			Text = T._("Please wait...");
			pbProgress.Value = 0;
			if (!Visible)
				Show();
			Update();
			return true;
		}

		public bool Progress(int percent)
		{
			Text = T._("{0}% complete", percent);
			pbProgress.Value = percent;
			if (!Visible)
				Show();
			Update();
			return true;
		}

		public bool ProgressEnd()
		{
			Text = T._("Done");
			pbProgress.Value = 100;
			if (Visible)
				Hide();
			return true;
		}
	}
}
