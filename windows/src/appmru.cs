using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET5_0_OR_GREATER
using System.Windows.Forms;
#endif
using Microsoft.Win32;

namespace SpringCard.LibCs.Windows
{
	public class AppMRU
	{
		private static Logger logger = new Logger(typeof(AppMRU).FullName);

		#region Private members
		private string NameOfProgram;
		private string SubKeyName;
#if NET5_0_OR_GREATER
		private object ParentMenuItem;
#else
		private ToolStripMenuItem ParentMenuItem;
#endif
		private Action<object, EventArgs> OnRecentFileClick;
		private Action<object, EventArgs> OnClearRecentFilesClick;

		private void _onClearRecentFiles_Click(object obj, EventArgs evt)
		{
			try
			{
				RegistryKey rK = Registry.CurrentUser.OpenSubKey(this.SubKeyName, true);
				if (rK == null)
					return;
				string[] values = rK.GetValueNames();
				foreach (string valueName in values)
					rK.DeleteValue(valueName, true);
				rK.Close();
#if NET5_0_OR_GREATER

#else
				this.ParentMenuItem.DropDownItems.Clear();
				this.ParentMenuItem.Enabled = false;
#endif

			}
			catch (Exception ex)
			{
				logger.trace(ex.ToString());
			}
			if (OnClearRecentFilesClick != null)
				this.OnClearRecentFilesClick(obj, evt);
		}
		
		private void _refreshRecentFilesMenu()
		{
			RegistryKey rK;			
			string s;
#if NET5_0_OR_GREATER

#else
			ToolStripItem tSI;
#endif


			try
			{
				rK = Registry.CurrentUser.OpenSubKey(this.SubKeyName, false);
				if (rK == null)
				{
#if NET5_0_OR_GREATER

#else
					this.ParentMenuItem.Enabled = false;
#endif

					return;
				}
			}
			catch (Exception ex)
			{
                if (WinUtils.Debug)
                    logger.trace("Cannot open recent files registry key:\n" + ex.ToString());
				return;
			}
#if NET5_0_OR_GREATER

#else
			this.ParentMenuItem.DropDownItems.Clear();
#endif

			string[] valueNames = rK.GetValueNames();
			if(valueNames.Length > 0)
			{
				Array.Reverse(valueNames);
			}
			foreach (string valueName in valueNames)
			{
				s = rK.GetValue(valueName, null) as string;
				if (s == null)
					continue;
#if NET5_0_OR_GREATER

#else
				tSI = this.ParentMenuItem.DropDownItems.Add(s);
				tSI.Click += new EventHandler(this.OnRecentFileClick);
#endif

			}
#if NET5_0_OR_GREATER

#else
			if (this.ParentMenuItem.DropDownItems.Count == 0)
			{
				this.ParentMenuItem.Enabled = false;
				return;
			}

			this.ParentMenuItem.DropDownItems.Add("-");
			tSI = this.ParentMenuItem.DropDownItems.Add(T._("Clear list"));
			tSI.Click += new EventHandler(this._onClearRecentFiles_Click);
			this.ParentMenuItem.Enabled = true;
#endif

		}
		#endregion

		#region Public members
		/// <summary>
		/// Return list of recent items
		/// </summary>
		/// <returns></returns>
		public List<string> getRecentItems()
		{
			string[] valueNames = null;
			string s;
			List<string> files = new List<string>();
			RegistryKey rK;
			try
			{
				rK = Registry.CurrentUser.OpenSubKey(this.SubKeyName, false);
				if (rK == null)
					return files;
				}
			catch (Exception ex)
			{
				logger.trace("Cannot open recent files registry key:\n" + ex.ToString());
				return files;
			}

			valueNames = rK.GetValueNames();
			if(valueNames.Length > 0)
				Array.Reverse(valueNames);

			foreach (string valueName in valueNames)
			{
				s = rK.GetValue(valueName, null) as string;
				if (s == null)
					continue;
				files.Add(s);
			}			
			return files;
		}
		
		
		public void AddRecentFile(string fileNameWithFullPath)
		{
			string s;
			try
			{
				RegistryKey rK = Registry.CurrentUser.CreateSubKey(this.SubKeyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
				for (int i = 0; true; i++)
				{
					s = rK.GetValue(i.ToString(), null) as string;
					if (s == null)
					{
						rK.SetValue(i.ToString(), fileNameWithFullPath);
						rK.Close();
						break;
					}
					else if (s == fileNameWithFullPath)
					{
						rK.Close();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.trace(ex.ToString());
			}
			this._refreshRecentFilesMenu();
		}

		public void RemoveRecentFile(string fileNameWithFullPath)
		{
			try
			{
				RegistryKey rK = Registry.CurrentUser.OpenSubKey(this.SubKeyName, true);
				string[] valuesNames = rK.GetValueNames();
				foreach (string valueName in valuesNames)
				{
					if ((rK.GetValue(valueName, null) as string) == fileNameWithFullPath)
					{
						rK.DeleteValue(valueName, true);
						this._refreshRecentFilesMenu();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.trace(ex.ToString());
			}
			this._refreshRecentFilesMenu();
		}
		#endregion

#if NET5_0_OR_GREATER


#else
		/// <exception cref="ArgumentException">If anything is null or nameOfProgram contains a forward slash or is empty.</exception>
		public AppMRU(ToolStripMenuItem parentMenuItem, string nameOfProgram, Action<object, EventArgs> onRecentFileClick, Action<object, EventArgs> onClearRecentFilesClick = null)
		{
			if(parentMenuItem == null || onRecentFileClick == null ||
				nameOfProgram == null || nameOfProgram.Length == 0 || nameOfProgram.Contains("\\"))
				throw new ArgumentException(T._("Bad argument"));

			this.ParentMenuItem = parentMenuItem;
			this.NameOfProgram = nameOfProgram;
			this.OnRecentFileClick = onRecentFileClick;
			this.OnClearRecentFilesClick = onClearRecentFilesClick;
			this.SubKeyName = string.Format("Software\\SpringCard\\{0}\\MRU", this.NameOfProgram);

			this._refreshRecentFilesMenu();
		}
#endif
		// Pour être utilisée sans menu (pour la welcome page par exemple)
		public AppMRU(string nameOfProgram)
		{
			if(nameOfProgram == null || nameOfProgram.Length == 0 || nameOfProgram.Contains("\\"))
				throw new ArgumentException("Bad argument.");

			this.NameOfProgram = nameOfProgram;
			this.SubKeyName = string.Format("Software\\SpringCard\\{0}\\MRU", this.NameOfProgram);
			//this._refreshRecentFilesMenu();
		}
		
	}
		
}
