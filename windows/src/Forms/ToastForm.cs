/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 30/11/2017
 * Time: 09:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.LibCs;

namespace SpringCard.LibCs.Windows.Forms
{
	/// <summary>
	/// Description of ToastForm.
	/// </summary>
	public partial class ToastForm : Form
	{
		static List<ToastForm> _forms = new List<ToastForm>();
		public static int PositionY = -30;
		public static int PositionX = 20;
		private static Form _last_parent;
		private static int _number = 0;

		private string Hash;

		public static void Clear()
		{
			lock (_forms)
			{
				for (int i = 0; i < _forms.Count; i++)
				{
					_forms[i].tmrAutoClose.Interval = _forms.Count - i + 1;
					_forms[i].tmrAutoClose.Enabled = true;
				}
			}
		}

		public static void Relocate(Form parent)
		{
			lock (_forms)
			{
				_last_parent = parent;
				_Relocate();
			}
		}

		public static void Display(Form parent, string text, string caption, MessageBoxIcon icon, int autoClose)
        {
			ToastForm f = new ToastForm();
			f.Text = caption;
			f.lbMessage.Text = text;

			Bitmap b = null;
			switch (icon)
            {
				case MessageBoxIcon.Error:
					b = SystemIcons.Error.ToBitmap();
					break;
				case MessageBoxIcon.Question:
					b = SystemIcons.Question.ToBitmap();
					break;
				case MessageBoxIcon.Warning:
					b = SystemIcons.Warning.ToBitmap();
					break;
				case MessageBoxIcon.Information:
					b = SystemIcons.Information.ToBitmap();
					break;
			}
			if (b != null)
				f.imgIcon.Image = b;

			f.Hash = BinConvert.ToHex(MD5.Hash(text + "|" + caption));

			lock (_forms)
            {
				/* How many toasts have been created? */
				f.lbNumber.Text = _number.ToString();
				_number++;

				/* Suppress earlier toast having exactly the same message */
				for (int i = 0; i<_forms.Count; i++)
                {
					if (_forms[i].Hash == f.Hash)
                    {
						_forms.RemoveAt(i);
						break;
                    }
                }

				/* Add this toast to the list */
				_forms.Add(f);

				/* Suppress oldest toast when there are too many */
				int maxToastCount = (75 * parent.Height) / (110 * f.Height);
				if (maxToastCount < 2)
					maxToastCount = 2;
				while (_forms.Count > maxToastCount)
				{
					_forms[0].tmrAutoClose.Interval = 1;
					_forms[0].tmrAutoClose.Enabled = true;
					_forms.RemoveAt(0);
				}

				/* (Re)locate all the toasts */
				_Relocate(parent);
			}
			
			f.Show(parent);

			if (autoClose > 0)
			{
				f.tmrAutoClose.Interval = 1000 * autoClose;
				f.tmrAutoClose.Enabled = true;
			}
		}

		private static void _Relocate(Form parent = null)
        {
			if (parent != null)
				_last_parent = parent;
			for (int index = 0; index < _forms.Count; index++)
            {
				_forms[_forms.Count - index - 1].Locate(_last_parent, index);
            }
        }

		public void Locate(Form parent, int index)
        {
			lbIndex.Text = index.ToString();
			int delta_y = (Height * 110) * index / 100;
			if (PositionY >= 0)
			{
				Top = Top + PositionY + delta_y;
			}
			else
			{
				Top = parent.Top + parent.Height - Height + PositionY - delta_y;
			}
			if (PositionX >= 0)
			{
				Left = parent.Left + PositionX;
			}
			else
			{
				Left = parent.Left + parent.Width - Width + PositionX;
			}
		}

		public ToastForm()
		{
			InitializeComponent();
		}
		
		private void ToastForm_Shown(object sender, EventArgs e)
		{
			Logger.Debug("ToastForm:Shown");
		}

        private void tmrAutoClose_Tick(object sender, EventArgs e)
        {
			Close();
        }

        private void ToastForm_FormClosed(object sender, FormClosedEventArgs e)
        {
			lock (_forms)
            {
				if (_forms.Contains(this))
					_forms.Remove(this);
				_Relocate();
			}
		}

    }
}
