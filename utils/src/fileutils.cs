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
using System.Security.AccessControl;
using System.Security.Principal;

namespace SpringCard.LibCs
{
	/**
	 * \brief File-related utilities
	 */
	public static class FileUtils
	{
		/**
		 * \brief Look for a specified file in a few directories 
		 */
		public static string LocateFile(string FileName, string[] SearchDirectories)
		{
			if (File.Exists(FileName))
				return FileName;
			
			foreach (string Directory in SearchDirectories)
			{
				string CompleteFileName = Directory + Path.DirectorySeparatorChar + FileName;
				if (File.Exists(CompleteFileName))
					return CompleteFileName;
			}
			
			return null;
        }


#if !NET5_0_OR_GREATER
        /**
		 * \brief Check the user's permissions over a directory
		 */
        public static bool CheckDirectoryPermissions(string DirectoryName, FileSystemRights accessType)
        {
            bool hasAccess = true;
            try
            {
                AuthorizationRuleCollection collection = Directory
                                            .GetAccessControl(DirectoryName)
                                            .GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

                foreach (FileSystemAccessRule rule in collection)
                {
                    if ((rule.FileSystemRights & accessType) > 0)
                    {
                        return hasAccess;
                    }
                }

            }
            catch (Exception)
            {
                hasAccess = false;
            }
            return hasAccess;
        }
#endif
        /**
		 * \brief Create a directory
		 */
        public static bool CreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        /**
		 * \brief Copy a file
		 */
        public static bool CopyFile(string SourceFileName, string DestFileName, bool ForceOverWrite)
        {
            try
            {
                File.Copy(SourceFileName, DestFileName, ForceOverWrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /**
            * \brief Copy the content of a directory to another
            */
        public static bool CopyDirectory(string SourceDirName, string DestDirName, bool RecurseSubDirs, bool ForceOverWrite)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(SourceDirName);

            if (!dir.Exists)
            {
                return false;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(DestDirName))
            {
                Directory.CreateDirectory(DestDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(DestDirName, file.Name);
                file.CopyTo(temppath, ForceOverWrite);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (RecurseSubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(DestDirName, subdir.Name);
                    if (!CopyDirectory(subdir.FullName, temppath, RecurseSubDirs, ForceOverWrite))
                        return false;
                }
            }

            return true;
        }

        /**
		 * \brief Delete a directory
		 */
        public static bool DeleteDirectory(string DirectoryName, bool RecurseSubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(DirectoryName);

            if (!dir.Exists)
            {
                return true;
            }

            if (RecurseSubDirs)
            {
                foreach (var subDir in dir.EnumerateDirectories())
                {
                    if (!DeleteDirectory(subDir.FullName, RecurseSubDirs))
                        return false;
                }
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.IsReadOnly = false;
                file.Delete();
            }

            dir.Delete();
            return true;
        }

        /**
		 * \brief Return the extension of a filename
		 */
        public static string Extension(string FileName)
		{
			return Path.GetExtension(FileName);
		}

		/**
		 * \brief Return the base name of a filename
		 */
		public static string BaseName(string FileName)
		{
			return Path.GetFileName(FileName);
		}

		/**
		 * \brief Return the directory name of a filename
		 */
		public static string DirName(string FileName)
		{
			return Path.GetDirectoryName(FileName);
		}

        public static bool IsReadable(string FileName)
        {
            try
            {
                File.Open(FileName, FileMode.Open, FileAccess.Read).Dispose();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static bool IsWritable(string FileName)
        {
            try
            {
                File.Open(FileName, FileMode.Append, FileAccess.Write).Dispose();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static bool IsDirectoryWritable(string DirectoryName)
        {
#if !NET5_0_OR_GREATER
			return CheckDirectoryPermissions(DirectoryName,
                FileSystemRights.CreateDirectories |
                FileSystemRights.CreateFiles |
                FileSystemRights.DeleteSubdirectoriesAndFiles |
                FileSystemRights.ListDirectory |
                FileSystemRights.Read |
                FileSystemRights.ReadData |
                FileSystemRights.Write |
                FileSystemRights.WriteData);
#else
            throw new System.ArgumentException("IsDirectoryWritable", "NETCore 5.0");
#endif
            
        }

    }
}