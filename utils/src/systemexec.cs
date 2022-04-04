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
using System.Diagnostics;
#if !NET5_0_OR_GREATER
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace SpringCard.LibCs
{
	/**
	 * \brief Utilities to run and control external programs
	 */
	public class SystemExec
	{
		/**
		 * \breif Run an external program and returns its stdout as a single string 
		 */
		public static string ExecuteToString(string FileName, string Arguments = null)
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.RedirectStandardError = false;
			p.StartInfo.FileName = FileName;
			p.StartInfo.Arguments = Arguments;
			try
			{
				if (p.Start())
				{
					string result = p.StandardOutput.ReadToEnd();
					p.WaitForExit();
					return result;
				}
				Logger.Error("Failed to run {0}", FileName);
				if (!string.IsNullOrEmpty(Arguments))
					Logger.Error("Arguments: {0}", Arguments);
			}
			catch (Exception e)
			{
				Logger.Error("Failed to run {0} (exception {1})", FileName, e.Message);
			}
			return null;
		}

		/**
		 * \breif Run an external program and returns its stdout as a single string 
		 */
		public static bool Execute(string FileName, string Arguments = null)
		{
			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = false;
			p.StartInfo.RedirectStandardError = false;
			p.StartInfo.FileName = FileName;
			p.StartInfo.Arguments = Arguments;
			try
			{
				if (p.Start())
					return true;
				Logger.Error("Failed to run {0}", FileName);
				if (!string.IsNullOrEmpty(Arguments))
					Logger.Error("Arguments: {0}", Arguments);
			}
			catch (Exception e)
			{
				Logger.Error("Failed to run {0} (exception {1})", FileName, e.Message);
			}
			return false;
		}

	}
}