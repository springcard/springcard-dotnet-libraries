/**
 *
 * \ingroup Windows
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Resources;
using System.IO;
using System.Management;
using System.Threading;

namespace SpringCard.LibCs.Windows.Forms
{
	/**
	 * \brief A versatile about form
	 */
	public partial class AboutForm : Form
	{
		private static Logger logger = new Logger(typeof(AboutForm).FullName);
		FormStyle style = FormStyle.Default;
		Thread showDataThread;

		/**
		 * \brief Create the form
		 */
		public AboutForm()
		{
			InitializeComponent();

			lbPrivate.Visible = AppUtils.IsSpringCardPrivate();

			Assembly programAssembly = Assembly.GetEntryAssembly();

            string formTitle = T._("About...");

            try
			{
				FileVersionInfo i = FileVersionInfo.GetVersionInfo(programAssembly.Location);
                lbProductBig.Text = i.ProductName;
                lbCompanyProduct.Text = i.CompanyName + " " + i.ProductName;
                formTitle = string.Format(T._("About {0}"), lbCompanyProduct.Text);
                string[] e = i.ProductVersion.Split('.');
				if (e.Length < 4)
					lbVersion.Text = "Version " + i.ProductVersion;
				else
					lbVersion.Text = "Version " + string.Format("{0}.{1} [{2}.{3}]", e[0], e[1], e[2], e[3]);
				lbCopyright.Text = "Copyright " + i.LegalCopyright;
				lbTrademarks.Text = i.LegalTrademarks;
			}
			catch (Exception e)
			{
				logger.debug(e.Message);
			}

            Text = formTitle;

            string resourceName = null;
			try
			{
				foreach (Type t in programAssembly.GetTypes())
				{
					string s = t.ToString();
					if (s.EndsWith(".Properties.Resources"))
					{
						resourceName = s;
						break;
					}
				}
			}
			catch (Exception e)
			{
				logger.debug(e.Message);
			}

			LoadTextFromFile(lbLicense, "LICENSE.TXT");
			LoadTextFromFile(lbTrademarks, "TRADEMARKS.TXT");
			LoadTextFromFile(lbCredits, "CREDITS.TXT");
			LoadTextFromFile(lbMore, "README.TXT");

			ResourceManager resourceManager = null;
			if (resourceName != null)
			{
				try
				{
					resourceManager = new ResourceManager(resourceName, programAssembly);
				}
				catch (Exception e)
				{
					logger.debug(e.Message);
				}
			}
			if (resourceManager != null)
			{
				LoadTextFromResource(lbLicense, resourceManager, "License");
				LoadTextFromResource(lbTrademarks, resourceManager, "Trademarks");
				LoadTextFromResource(lbCredits, resourceManager, "Credits");
				LoadTextFromResource(lbMore, resourceManager, "Readme");
			}

			if (string.IsNullOrEmpty(lbLicense.Text))
				tabs.TabPages.Remove(tabLicense);
			if (string.IsNullOrEmpty(lbTrademarks.Text))
				tabs.TabPages.Remove(tabTrademarks);
			if (string.IsNullOrEmpty(lbCredits.Text))
				tabs.TabPages.Remove(tabCredits);
			if (string.IsNullOrEmpty(lbMore.Text))
				tabs.TabPages.Remove(tabMore);
		}

		private void LoadTextFromFile(TextBox control, string fileName)
		{
			fileName = Application.StartupPath + @"\" + fileName;
			try
			{
				if (File.Exists(fileName))
				{
					string s = File.ReadAllText(fileName);
					if (!string.IsNullOrEmpty(s))
					{
						s = s.Replace("\r\n", "\n");
						s = s.Replace("\r", "\n");
						s = s.Replace("\n", Environment.NewLine);
						control.Text = s;
					}
				}
			}
			catch (Exception e)
			{
				logger.debug(e.Message);
			}
		}

		private void LoadTextFromResource(TextBox control, ResourceManager resourceManager, string entryName)
		{
			try
			{
				string s = resourceManager.GetString(entryName);
				if (!string.IsNullOrEmpty(s))
				{
					s = s.Replace("\r\n", "\n");
					s = s.Replace("\r", "\n");
					s = s.Replace("\n", Environment.NewLine);
					control.Text = s;
				}
			}
			catch (Exception e)
			{
				logger.debug(e.Message);
			}
		}

		/**
		 * \brief Define the form's style
		 */
		public void SetFormStyle(FormStyle style)
		{
			this.style = style;
		}

        /**
		 * \brief Define the form's main color
		 */
        public void SetHeaderColor(Color headerColor)
        {
            pHeader.BackColor = headerColor;
        }

        /**
		 * \brief Define the 'more' message
		 */
        public void SetMoreMessage(string moreMessage)
		{
			if (string.IsNullOrEmpty(moreMessage))
			{
				lbMore.Text = "";
			} else
			{
				lbMore.Text = moreMessage;
			}
		}

        private static AboutForm instance;

        /**
		 * \brief Display the about form as a detached dialog box
         */
        public static void DoShowDetached()
        {
            DoShowDetached(FormStyle.Default);
        }

        /**
		 * \brief Display the about form as a detached dialog box, specifying the style
         */
        public static void DoShowDetached(FormStyle style)
        {
            if (instance == null)
            {
                instance = new AboutForm();
                instance.style = style;
                instance.StartPosition = FormStartPosition.CenterScreen;
                instance.Show();
            }
            else
            {
                instance.BringToFront();
            }
        }

        /**
		 * \brief Display the about form
         * 
         * \deprecated Use DoShowDialog()
		 */
        public static void Display(Form parent = null)
        {
            DoShowDialog(parent);
        }

        /**
		 * \brief Display the about form as a modal dialog box
         */            
        public static void DoShowDialog(Form parent)
        {
            DoShowDialog(parent, FormStyle.Default);
        }

        /**
		 * \brief Display the about form as a modal dialog box, specifying the style
         */
        public static void DoShowDialog(Form parent, FormStyle style)
		{
			AboutForm form;
			form = new AboutForm();
            form.style = style;
			if (parent != null)
			{
				form.StartPosition = FormStartPosition.CenterParent;
				form.ShowDialog(parent);
			} else
			{
				form.StartPosition = FormStartPosition.CenterScreen;
				form.ShowDialog();
			}
		}
		
		void AboutFormLoad(object sender, EventArgs e)
		{
            Forms.ApplyStyle(this, this.style);

			lvSystem.Items.Clear();
			lvLibraries.Items.Clear();
			lvDrivers.Items.Clear();

			showDataThread = new Thread(new ThreadStart(ShowDataProc));
			showDataThread.Start();
		}

		void ShowDataProc()
		{
			ShowSystemInformation();
			ShowLibraryVersions();
			ShowDriverVersions();
		}

		delegate void ShowSystemInformationInvoker(Dictionary<string, string> info);
		void ShowSystemInformation(Dictionary<string, string> info)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new ShowSystemInformationInvoker(ShowSystemInformation), info);
				return;
			}

			foreach (KeyValuePair<string, string> entry in info)
			{
				string[] data = new string[2];
				data[0] = entry.Key;
				data[1] = entry.Value;
				lvSystem.Items.Add(new ListViewItem(data));
			}
		}

		void ShowSystemInformation()
		{
			ShowSystemInformation(SysInfo.Get());
		}

		delegate void AddAssemblyVersionInvoker(AppInfo.AssemblyInfo info);
		void AddAssemblyVersion(AppInfo.AssemblyInfo info)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new AddAssemblyVersionInvoker(AddAssemblyVersion), info);
				return;
			}

			string[] data = new string[3];
			data[0] = info.Name;
			data[1] = info.Version;
			data[2] = info.FileName;
			lvLibraries.Items.Add(new ListViewItem(data));			
		}
		
		void ShowLibraryVersions()
		{			
			AppInfo.AssemblyInfo[] versions;
			versions = AppInfo.GetAssembliesInfo(AppInfo.AssemblyInfoSelect.OnlyLocals);
			foreach (AppInfo.AssemblyInfo version in versions)
				AddAssemblyVersion(version);
			versions = AppInfo.GetAssembliesInfo(AppInfo.AssemblyInfoSelect.AllButLocals);
			foreach (AppInfo.AssemblyInfo version in versions)
				AddAssemblyVersion(version);
		}


		delegate void AddDriverVersionInvoker(Drivers.DriverInfo version);
		void AddDriverVersion(Drivers.DriverInfo version)
		{
			if (InvokeRequired)
			{
				BeginInvoke(new AddDriverVersionInvoker(AddDriverVersion), version);
				return;
			}

			string[] data = new string[6];
			data[0] = version.Name;
			data[1] = version.Version;
			data[2] = version.Date;
			data[3] = version.Class;
			data[4] = version.InfAlias;
			data[5] = version.InfSubDirectory;
			lvDrivers.Items.Add(new ListViewItem(data));
		}

		void ShowDriverVersions()
		{
			Drivers.DriverInfo[] versions = Drivers.EnumDrivers().ToArray();
			foreach (Drivers.DriverInfo version in versions)
				AddDriverVersion(version);
		}

		void LinkSpringCardLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://www.springcard.com");
		}

		void CopyInfoToClipboard()
		{
			string text = "";
			
			text += lbCompanyProduct.Text + Environment.NewLine;
			text += lbVersion.Text + Environment.NewLine;
			text += lbCopyright.Text + Environment.NewLine;
			
			text += Environment.NewLine;
			text += "Libraries:" + Environment.NewLine;
			text += "----------" + Environment.NewLine;
			foreach (ListViewItem item in lvLibraries.Items)
			{
				text += string.Format("{0,-40} {1,-38} {2}", item.SubItems[0].Text, item.SubItems[1].Text, item.SubItems[2].Text) + Environment.NewLine;
			}

			text += Environment.NewLine;
			text += "Drivers:" + Environment.NewLine;
			text += "--------" + Environment.NewLine;
			foreach (ListViewItem item in lvDrivers.Items)
			{
				text += string.Format("{0,-24} {1,-16} {2,-12} {3,-24} {4,-16}, {5}", item.SubItems[0].Text, item.SubItems[1].Text, item.SubItems[2].Text, item.SubItems[3].Text, item.SubItems[4].Text, item.SubItems[5].Text) + Environment.NewLine;
			}

			text += Environment.NewLine;
			text += "System:" + Environment.NewLine;
			text += "-------" + Environment.NewLine;
			foreach (ListViewItem item in lvSystem.Items)
			{
				text += string.Format("{0}: {1}", item.SubItems[0].Text, item.SubItems[1].Text) + Environment.NewLine;
			}

			if (!string.IsNullOrEmpty(lbLicense.Text))
			{
				text += Environment.NewLine;
				text += "Licences:" + Environment.NewLine;
				text += "--------" + Environment.NewLine;
				text += lbLicense.Text + Environment.NewLine;
			}

			if (!string.IsNullOrEmpty(lbTrademarks.Text))
			{
				text += Environment.NewLine;
				text += "Trademarks:" + Environment.NewLine;
				text += "----------" + Environment.NewLine;
				text += lbTrademarks.Text + Environment.NewLine;
			}

			if (!string.IsNullOrEmpty(lbMore.Text))
			{
				text += Environment.NewLine;
				text += "More:" + Environment.NewLine;
				text += "-----" + Environment.NewLine;
				text += lbMore.Text + Environment.NewLine;
			}
			
			text += Environment.NewLine;
			
			Clipboard.SetText(text);
		}

		private void btnCopyInfo_Click(object sender, EventArgs e)
        {
            CopyInfoToClipboard();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

		private void AboutForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			instance = null;
		}

        private void imgLogoColor_Click(object sender, EventArgs e)
        {

        }
    }
}
