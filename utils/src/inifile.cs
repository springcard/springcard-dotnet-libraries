/**
 *
 * \ingroup LibCs
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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SpringCard.LibCs
{
	/**
	 * \brief INI file object (pure .NET implementation, no dependency to Windows' libraries)
	 */
	[Serializable]
	public class IniFile
	{
		/**
		 * \brief Default text encoding, when not specified explicitely in the constructor. Default is ASCII.
		 */
		public static Encoding DefaultEncoding = Encoding.ASCII;

		[Serializable]
		private struct Entry
		{
			public string Section;
			public string Name;
			public string Value;
			public bool EqualSign;
		}

		/**
		 * \brief The list of sections in the INI file
		 */
		public List<string> Sections { get; private set; } = new List<string>();
		private List<Entry> Entries = new List<Entry>();

		public void Dump()
		{
			foreach (string section in Sections)
			{
				Console.WriteLine("[{0}]", section);
				foreach (Entry entry in Entries)
				{
					if (entry.Section == section)
					{
						Console.WriteLine("{0}={1}", entry.Name, entry.Value);
					}
				}
			}
		}


		public bool ReadOnly
		{
			get;
			private set;
		}

		private bool AutoSave = true;

		public string FileName
		{
			get;
			private set;
		}

		private Encoding FileEncoding = DefaultEncoding;

		/**
		 * \brief Create an instance of the IniFile object for the given INI file. The instance is read only.
		 */
		public static IniFile OpenReadOnly(string FileName)
		{
			IniFile f = new IniFile();
			f.ReadOnly = true;
			f.AutoSave = false;
			f.FileName = FileName;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the IniFile object for the given INI file. The instance is read only.
		 */
		public static IniFile OpenReadOnly(string FileName, Encoding FileEncoding)
		{
			IniFile f = new IniFile();
			f.ReadOnly = true;
			f.AutoSave = false;
			f.FileName = FileName;
			f.FileEncoding = FileEncoding;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the IniFile object for the given INI file. The instance is read/write.
		 *
		 * If the AutoSave parameter is set to false, Save() must be called explicitly, otherwise the write operations are lost.
		 */
		public static IniFile OpenReadWrite(string FileName, bool AutoSave = false)
		{
			IniFile f = new IniFile();
			f.ReadOnly = false;
			f.AutoSave = AutoSave;
			f.FileName = FileName;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the IniFile object for the given INI file. The instance is read/write.
		 *
		 * If the AutoSave parameter is set to false, Save() must be called explicitly, otherwise the write operations are lost.
		 */
		public static IniFile OpenReadWrite(string FileName, Encoding FileEncoding, bool AutoSave = false)
		{
			IniFile f = new IniFile();
			f.ReadOnly = false;
			f.AutoSave = AutoSave;
			f.FileName = FileName;
			f.FileEncoding = FileEncoding;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the IniFile object from a content already loaded in memory
		 */
		public static IniFile CreateFromText(string Text)
		{
			string[] Lines = Text.Split('\n');
			return CreateFromLines(Lines);
		}

		/**
		 * \brief Create an instance of the IniFile object from a content already loaded in memory
		 */		
		public static IniFile CreateFromLines(string[] Lines)
		{
			IniFile f = new IniFile();
			f.ReadOnly = false;
			f.AutoSave = false;
			f.FileName = null;
			f.Populate(Lines);
			return f;
		}

		private IniFile()
		{

		}

		/**
		 * \brief Create an instance of the IniFile object from the given INI file. The instance is read/write, with AutoSave set to true. 
		 *
		 * \deprecated Use either OpenReadOnly() or OpenReadWrite()
		 */		
		public IniFile(string FileName)
		{
			ReadOnly = false;
			AutoSave = true;
			this.FileName = FileName;
			Load();
		}

		/**
		 * \brief Create an instance of the IniFile object from the given INI file. The instance is read/write, using the specified AutoSave value. 
		 *
		 * \deprecated Use OpenReadWrite()
		 */				
		public IniFile(string FileName, bool AutoSave)
		{
			ReadOnly = false;
			this.AutoSave = AutoSave;
			this.FileName = FileName;
			Load();
		}

		/**
		 * \brief Create an instance of the IniFile object from the given INI file. The type of the instance depends on the parameters.
		 *
		 * \deprecated Use either OpenReadOnly() or OpenReadWrite()
		 */		
		public IniFile(string FileName, bool AutoSave, bool ReadOnly)
		{
			this.ReadOnly = ReadOnly;
			this.AutoSave = AutoSave;
			this.FileName = FileName;
			Load();
		}

		private bool Populate(string[] Lines)
		{
			Sections.Clear();
			Entries.Clear();

			string strSection = null;
			foreach (string Line in Lines)
			{
				string strLine = Line.Trim();

                if (strLine.Contains(";"))
                {
                    strLine = strLine.Substring(0, strLine.IndexOf(';'));
                    strLine = strLine.Trim();
				}

				if (strLine != "")
				{
					if (strLine.StartsWith("[") && strLine.EndsWith("]"))
					{
						/* Start of section */
						strSection = strLine.Substring(1, strLine.Length - 2);

						/* Add to sections */
						Sections.Add(strSection);
					}
					else
					{
						Entry e = new Entry();
						e.Section = strSection;

						string[] t = strLine.Split(new char[] { '=' }, 2);
						e.Name = t[0].Trim();
						e.Value = null;
						if (t.Length > 1)
							e.Value = t[1].Trim();

						Entries.Add(e);
					}
				}
			}
			return true;
		}

		/**
		 * \brief (Re)load the content from the file
		 */		
		public bool Load()
		{
			if (File.Exists(FileName))
			{
				try
				{
					string[] Lines = File.ReadAllLines(FileName);
					return Populate(Lines);
				}
				catch
				{

				}
			}

			return false;
		}

		/**
		 * \brief Save the content to another INI file
		 */
		public bool SaveTo(string FileName)
		{
			string oldFileName = this.FileName;
			this.FileName = FileName;
			if (!Save())
			{
				this.FileName = oldFileName;
				return false;
			}
			return true;
		}

		/**
		 * \brief Save the content to another INI file
		 */
		public bool SaveTo(string FileName, Encoding FileEncoding)
		{
			string oldFileName = this.FileName;
			Encoding oldEncoding = this.FileEncoding;
			this.FileName = FileName;
			this.FileEncoding = FileEncoding;
			if (!Save())
			{
				this.FileName = oldFileName;
				this.FileEncoding = oldEncoding;
				return false;
			}
			return true;
		}

		/**
		 * \brief Save the file. Return false in case of error
		 */
		public bool Save()
		{
			try
			{
				SaveEx();
				return true;
			}
			catch (Exception e)
			{
				Logger.Trace(e.Message);
				return false;
			}
		}

		/**
		 * \brief Save the file. Throw an exception in case of error
		 */
		public void SaveEx()
		{
			if (ReadOnly)
				throw new UnauthorizedAccessException();

			string strToSave = "";

			foreach (Entry entry in Entries)
			{
				if ((entry.Section == null) || (entry.Section == ""))
				{
					if (entry.Value == null)
						strToSave += entry.Name;
					else
						strToSave += entry.Name + "=" + entry.Value;

					strToSave += "\r\n";
				}
			}

			foreach (string section in Sections)
			{
				strToSave += "[" + section + "]" + "\r\n";

				foreach (Entry entry in Entries)
				{
					if (entry.Section == section)
					{
						if (entry.Value == null)
							strToSave += entry.Name;
						else
							strToSave += entry.Name + "=" + entry.Value;

						strToSave += "\r\n";
					}
				}

				strToSave += "\r\n";
			}

			TextWriter iniFile = new StreamWriter(FileName, false, FileEncoding);
			iniFile.Write(strToSave);
			iniFile.Close();
			iniFile.Dispose();
		}

		/**
		 * \brief Get the list of sections in the INI file
		 *
		 * \deprecated See Sections
		 */		
		public List<string> GetSections()
		{
			return Sections;
		}

		/**
		 * \brief Read a string entry from the INI file
		 */		
		public string ReadString(string Section, string Name, string Default = "")
		{
			foreach (Entry entry in Entries)
			{
				if (entry.Section == Section)
				{
					if (entry.Name == Name)
					{
						return entry.Value;
					}
				}
			}

			return Default;
		}

		/**
		 * \brief Read an integer entry from the INI file
		 */		
		public int ReadInteger(string Section, string Name, int Default = 0)
		{
			string s = ReadString(Section, Name);
			int r;
			if (int.TryParse(s, out r))
				return r;
			return Default;
		}

		/**
		 * \brief Read a boolean entry from the INI file
		 */		
		public bool ReadBoolean(string Section, string Name, bool Default = false)
		{
			bool valid;
			bool result = StrUtils.ReadBoolean(ReadString(Section, Name, null), out valid);
			if (!valid)
				return Default;
			return result;
		}

		/**
		 * \brief Remove an entry from the INI file
		 *
		 * \deprecated See Remove()
		 */				
		public bool Erase(string Section, string Name)
		{
			return Remove(Section, Name);
		}
		
		/**
		 * \brief Remove an entry from the INI file
		 */				
		public bool Remove(string Section, string Name)
		{
			if (ReadOnly)
				return false;

			for (int i = 0; i < Entries.Count; i++)
			{
				Entry oldEntry = Entries[i];
				if (oldEntry.Section == Section)
				{
					if (oldEntry.Name == Name)
					{
						Entries.Remove(oldEntry);
						break;
					}
				}
			}

			if (AutoSave)
				return Save();

			return true;

		}

		private bool WriteEntry(Entry newEntry)
		{
			if (ReadOnly)
				return false;

			bool isNewEntry = true;
			bool isNewSection = true;

			if (!String.IsNullOrEmpty(newEntry.Section))
			{
				for (int i = 0; i < Sections.Count; i++)
				{
					if (Sections[i] == newEntry.Section)
					{
						isNewSection = false;
						break;
					}
				}
			}
			else
			{
				isNewSection = false;
			}

			for (int i = 0; i < Entries.Count; i++)
			{
				Entry oldEntry = Entries[i];
				if (oldEntry.Section == newEntry.Section)
				{
					if (oldEntry.Name == newEntry.Name)
					{
						Entries[i] = newEntry;
						isNewEntry = false;
						break;
					}
				}
			}

			if (isNewSection)
				Sections.Add(newEntry.Section);

			if (isNewEntry)
				Entries.Add(newEntry);

			if (AutoSave)
				return Save();

			return true;
		}

		/**
		 * \brief Write an empty entry into the INI file
		 */						
		public bool WriteName(string Section, string Name)
		{
			Entry newEntry = new Entry();
			newEntry.Section = Section;
			newEntry.Name = Name;
			newEntry.Value = null;
			newEntry.EqualSign = false;

			return WriteEntry(newEntry);
		}

		/**
		 * \brief Write a string entry into the INI file
		 */		
		public bool WriteString(string Section, string Name, string Value)
		{
			Entry newEntry = new Entry();
			newEntry.Section = Section;
			newEntry.Name = Name;
			newEntry.Value = Value;
			newEntry.EqualSign = true;

			return WriteEntry(newEntry);
		}

		/**
		 * \brief Write an integer entry into the INI file
		 */		
		public bool WriteInteger(string Section, string Name, int Value)
		{
			return WriteString(Section, Name, String.Format("{0}", Value));
		}

		/**
		 * \brief Write a boolean entry into the INI file
		 */		
		public bool WriteBoolean(string Section, string Name, bool value)
		{
			int i = value ? 1 : 0;
			return WriteInteger(Section, Name, i);
		}


		/**
		 * \brief Return all the entries from a given section
		 *
		 * If withAssociatedValues is set to true, the entries are returned as name=value, otherwise, only the name is returned
		 */		
		public string[] ReadSection(string Section, bool withAssociatedValues = false)
		{
			List<string> Names = new List<string>();

			foreach (Entry entry in Entries)
			{
				if (entry.Section == Section)
				{
					if (withAssociatedValues)
					{
						Names.Add(entry.Name + "=" + entry.Value);
					}
					else
					{
						Names.Add(entry.Name);
					}
				}
			}

			return Names.ToArray();
		}

        public string[] ReadSections()
        {
            return Sections.ToArray();
        }


    }

}
