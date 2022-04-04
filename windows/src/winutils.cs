using System;
using System.Linq;
#if !NET5_0_OR_GREATER
using System.Windows.Forms;
#endif
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SpringCard.LibCs;

namespace SpringCard.LibCs.Windows
{
	public class WinUtils
    {
        public static bool Debug = false;

#if !NET5_0_OR_GREATER

        public static void FatalError(string message, string title = "Internal error")
		{
			Logger.Fatal("{0}: {1}", title, message);
			MessageBox.Show(message + "\n\n" + T._("This is a fatal error. The application will now terminate."), title);
			Environment.Exit(0);
		}

		public static void ShowMessage(string message)
		{
			Logger.Info(message);
			MessageBox.Show(message);
		}

		public enum ScreenRatio
		{
			Unknown,
			r16x9,
			r4x3
		};

		public ScreenRatio GetScreenRatio()
		{
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			float ratio = (float) width / (float) height;
			if ((ratio > 1.3) && (ratio < 1.4))
				return ScreenRatio.r4x3;
			if ((ratio > 1.7) && (ratio < 1.8))
				return ScreenRatio.r16x9;
			return ScreenRatio.Unknown;
		}

		public static void GetOptimalFormSize(int minWidth, int maxWidth, int minHeight, int maxHeight, out int width, out int height, out bool maximized)
		{
			maximized = false;
			width = minWidth;
			height = minHeight;

			if (maxWidth > minWidth)
			{
				if (maxWidth > Screen.PrimaryScreen.Bounds.Width)
					width = Screen.PrimaryScreen.Bounds.Width;
				else
					width = maxWidth;
			}

			if (maxHeight > minHeight)
			{
				if (maxHeight > Screen.PrimaryScreen.Bounds.Height)
					height = Screen.PrimaryScreen.Bounds.Height;
				else
					height = maxHeight;
			}

			if (width >= Screen.PrimaryScreen.Bounds.Width) maximized = true;
			if (height >= Screen.PrimaryScreen.Bounds.Height) maximized = true;
		}

		public static void GetOptimalFormSize(Form f, int maxWidth = -1, int maxHeight = -1)
		{
			GetOptimalFormSize(f.Width, maxWidth, f.Height, maxHeight, out int width, out int height, out bool maximized);
			f.Width = width;
			f.Height = height;
			f.WindowState = maximized ? FormWindowState.Maximized : FormWindowState.Normal;
		}

		public static void GetOptimalDialogPosition(int width, int height, out int left, out int top, out bool maximized)
		{
			if ((width > 1.2 * Screen.PrimaryScreen.Bounds.Width) || (height > 1.2 * Screen.PrimaryScreen.Bounds.Height))
			{
				left = 0;
				top = 0;
				maximized = true;
			}
			else
			{
				left = (int)(0.1 * Screen.PrimaryScreen.Bounds.Width);
				top = (int)(0.1 * Screen.PrimaryScreen.Bounds.Height);
				maximized = false;
			}
		}

		public static void GetOptimalDialogPosition(Form f)
		{
			GetOptimalDialogPosition(f.Width, f.Height, out int left, out int top, out bool maximized);
			f.Left = left;
			f.Top = top;
			f.WindowState = maximized ? FormWindowState.Maximized : FormWindowState.Normal;
		}
#endif
	}
}
