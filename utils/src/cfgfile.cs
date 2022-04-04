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
using System.Xml.XPath;

namespace SpringCard.LibCs
{
	/**
	 * \brief CFG file object
	 */	
	public class CfgFile : IConfigReader, IConfigWriter
    {
		/**
		 * \brief Default text encoding, when not specified explicitely in the constructor. Default is ASCII.
		 */
		public static Encoding DefaultEncoding = Encoding.ASCII;

        private struct Entry
		{
			public string Name;
			public string Value;
			public bool EqualSign;
		}

		private List<Entry> Entries = new List<Entry>();

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
		 * \brief Create an instance of the CfgFile object for the given CFG file. The instance is read only.
		 */
		public static CfgFile OpenReadOnly(string FileName)
		{
			CfgFile f = new CfgFile();
			f.ReadOnly = true;
			f.AutoSave = false;
			f.FileName = FileName;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the CfgFile object for the given CFG file. The instance is read only.
		 */
		public static CfgFile OpenReadOnly(string FileName, Encoding FileEncoding)
		{
			CfgFile f = new CfgFile();
			f.ReadOnly = true;
			f.AutoSave = false;
			f.FileName = FileName;
			f.FileEncoding = FileEncoding;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the CfgFile object for the given CFG file. The instance is read/write.
		 *
		 * If the AutoSave parameter is set to false, Save() must be called explicitly, otherwise the write operations are lost.
		 */
		public static CfgFile OpenReadWrite(string FileName, bool AutoSave = false)
		{
			CfgFile f = new CfgFile();
			f.ReadOnly = false;
			f.AutoSave = AutoSave;
			f.FileName = FileName;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the CfgFile object for the given CFG file. The instance is read/write.
		 *
		 * If the AutoSave parameter is set to false, Save() must be called explicitly, otherwise the write operations are lost.
		 */
		public static CfgFile OpenReadWrite(string FileName, Encoding FileEncoding, bool AutoSave = false)
		{
			CfgFile f = new CfgFile();
			f.ReadOnly = false;
			f.AutoSave = AutoSave;
			f.FileName = FileName;
			f.FileEncoding = FileEncoding;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the CfgFile object from a content already loaded in memory
		 */
		public static CfgFile CreateFromText(string Text)
		{
			string[] Lines = Text.Split('\n');
			return CreateFromLines(Lines);
		}

		/**
		 * \brief Create an instance of the CfgFile object from a content already loaded in memory
		 */				
		public static CfgFile CreateFromLines(string[] Lines)
		{
			CfgFile f = new CfgFile();
			f.ReadOnly = false;
			f.AutoSave = false;
			f.FileName = null;
			f.Populate(Lines);
			return f;
		}

		private CfgFile()
		{

		}

		/**
		 * \brief Create an instance of the CfgFile object from the given CFG file. The instance is read only, with AutoSave set to false. 
		 *
		 * \deprecated Use either OpenReadOnly() or OpenReadWrite()
		 */				
		public CfgFile(string FileName)
		{
			this.ReadOnly = true;
			this.AutoSave = false;
			this.FileName = FileName;
			this.Load();
		}

		/**
		 * \brief Create an instance of the CfgFile object from the given CFG file. The instance is read only, using the specified AutoSave value. 
		 *
		 * \deprecated Use OpenReadWrite()
		 */						
		public CfgFile(string FileName, bool AutoSave)
		{
			ReadOnly = true;
			this.AutoSave = AutoSave;
			this.FileName = FileName;
			Load();
		}

		/**
		 * \brief Create an instance of the CfgFile object from the given CFG file. The type of the instance depends on the parameters.
		 *
		 * \deprecated Use either OpenReadOnly() or OpenReadWrite()
		 */				
		public CfgFile(string FileName, bool AutoSave, bool ReadOnly)
		{
			this.ReadOnly = ReadOnly;
			this.AutoSave = AutoSave;
			this.FileName = FileName;
			Load();
		}

		private bool Populate(string[] Lines)
		{
			Entries.Clear();

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
					Entry e = new Entry();

					string[] t = strLine.Split(new char[] { '=' }, 2);
					e.Name = t[0].Trim();
					e.Value = null;
					if (t.Length > 1)
						e.Value = t[1].Trim();

					Entries.Add(e);
				}
			}
			return true;
		}

		/**
		 * \brief (Re)load the content from the file
		 */				
		public bool Load()
		{
			bool Result = true;
			TextReader cfgFile = null;
			string strLine = null;

			Entries.Clear();

			if (File.Exists(FileName))
			{
				try
				{
					cfgFile = new StreamReader(FileName, FileEncoding);

					strLine = cfgFile.ReadLine();

					while (strLine != null)
					{
						strLine = strLine.Trim();

						if (strLine.Contains(";"))
							strLine = strLine.Substring(0, strLine.IndexOf(';'));

						Entry e = new Entry();

						string[] t = strLine.Split(new char[] { '=' }, 2);
						e.Name = t[0].Trim();
						e.Value = null;
						if (t.Length > 1)
							e.Value = t[1].Trim();

						Entries.Add(e);

						strLine = cfgFile.ReadLine();
					}
					cfgFile.Close();
					cfgFile.Dispose();
				}
				catch
				{
					Result = false;
				}
				finally
				{
					if (cfgFile != null)
					{
						cfgFile.Close();
						cfgFile.Dispose();
					}
				}

			}
			else
			{
				Result = false;
			}

			return Result;
		}

		/**
		 * \brief Save the content to another CFG file
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
		 * \brief Save the content to another CFG file
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

			TextWriter cfgFile = new StreamWriter(FileName, false, FileEncoding);
			cfgFile.Write(Content());
			cfgFile.Close();
			cfgFile.Dispose();
		}

		/**
		 * \brief Serialize the content. Throw an exception in case of error
		 */
		public string Content()
		{
			string result = "";

			foreach (Entry entry in Entries)
			{
				if (entry.Value == null)
					result += entry.Name;
				else
					result += entry.Name + "=" + entry.Value;

				result += "\r\n";
			}

			return result;
		}

		/**
		 * \brief Read a string entry from the CFG file
		 */
		public string ReadString(string Name, string Default = "")
		{
			foreach (Entry entry in Entries)
			{
				if (entry.Name == Name)
				{
					return entry.Value;
				}
			}

			return Default;
		}

		/**
		 * \brief Read an integer entry from the CFG file
		 */
		public int ReadInteger(string Name, int Default = 0)
		{
			string s = ReadString(Name);
			int r;
			if (int.TryParse(s, out r))
				return r;
			return Default;
		}

        /**
		 * \brief Read an unsigned integer entry from the CFG file
		 */
        public uint ReadUnsigned(string Name, uint Default = 0)
        {
            string s = ReadString(Name);
            uint r;
            if (uint.TryParse(s, out r))
                return r;
            return Default;
        }

        /**
		 * \brief Read a boolean entry from the CFG file
		 */
        public bool ReadBoolean(string Name, bool Default = false)
		{
			bool valid;
			bool result = StrUtils.ReadBoolean(ReadString(Name, null), out valid);
			if (!valid)
				return Default;
			return result;
		}

		/**
		 * \brief Remove an entry from the CFG file
		 *
		 * \deprecated See Remove()
		 */				
		public bool Erase(string Name)
		{
			return Remove(Name);
		}
		
		/**
		 * \brief Remove an entry from the INI file
		 */				
		public bool Remove(string Name)
		{
			if (ReadOnly)
				return false;

			for (int i = 0; i < Entries.Count; i++)
			{
				Entry oldEntry = Entries[i];
				if (oldEntry.Name == Name)
				{
					Entries.Remove(oldEntry);
					break;
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

			for (int i = 0; i < Entries.Count; i++)
			{
				Entry oldEntry = Entries[i];
				if (oldEntry.Name == newEntry.Name)
				{
					Entries[i] = newEntry;
					isNewEntry = false;
					break;
				}
			}

			if (isNewEntry)
				Entries.Add(newEntry);

			if (AutoSave)
				return Save();

			return true;
		}

		/**
		 * \brief Write an empty entry into the CFG file
		 */		
		public bool WriteName(string Name)
		{
			Entry newEntry = new Entry();
			newEntry.Name = Name;
			newEntry.Value = null;
			newEntry.EqualSign = false;

			return WriteEntry(newEntry);
		}

		/**
		 * \brief Write a string entry into the CFG file
		 */		
		public bool WriteString(string Name, string Value)
		{
			Entry newEntry = new Entry();
			newEntry.Name = Name;
			newEntry.Value = Value;
			newEntry.EqualSign = true;

			return WriteEntry(newEntry);
		}

		/**
		 * \brief Write an integer entry into the CFG file
		 */		
		public bool WriteInteger(string Name, int Value)
		{
			return WriteString(Name, String.Format("{0}", Value));
		}

        /**
		 * \brief Write an unsigned integer entry into the CFG file
		 */
        public bool WriteUnsigned(string Name, uint Value)
        {
            return WriteString(Name, String.Format("{0}", Value));
        }

        /**
		 * \brief Write a boolean entry into the CFG file
		 */
        public bool WriteBoolean(string Name, bool value)
		{
			int i = value ? 1 : 0;
			return WriteInteger(Name, i);
		}

		/**
		 * \brief Return all the entries from the file
		 *
		 * If withAssociatedValues is set to true, the entries are returned as name=value, otherwise, only the name is returned
		 */				
		public string[] Read(bool withAssociatedValues)
		{
			List<string> Names = new List<string>();

			foreach (Entry entry in Entries)
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

			return Names.ToArray();
		}

	}

}
