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
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
#if !NET5_0_OR_GREATER
using System.Windows.Forms;
#endif

namespace SpringCard.LibCs.Windows
{
	public class AppUtils
	{
		private static Logger logger = new Logger(typeof(AppUtils).FullName);

		public const string CompanyName = "SpringCard";
		public static string RunAsSID = null;
		public const string AutoRunRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

		public static RegistryKey ApplicationUserRegistryKey(string ApplicationRelativePath, bool OpenReadWrite = false)
		{
			RegistryKey key;

			if (RunAsSID == null)
			{
				key = Registry.CurrentUser;
			}
			else
			{
				key = Registry.Users.OpenSubKey(RunAsSID, OpenReadWrite);
			}

			if (key != null)
			{
				if (OpenReadWrite)
				{
					key = key.CreateSubKey(ApplicationRelativePath);
				}
				else
				{
					key = key.OpenSubKey(ApplicationRelativePath);
				}
			}

			return key;
		}

		/// <summary>
		/// Used to know if the current assembly is running from an install folder, we hope...
		/// </summary>
		/// <param name="gestfileReference"></param>
		/// <returns></returns>
		public static bool isRunningFromInstallationFolder(string gestfileReference)
		{
			string t = System.Reflection.Assembly.GetEntryAssembly().Location;
			t = Path.GetDirectoryName(t);
			return t.ToLower().Trim().Contains(gestfileReference.Trim().ToLower());
		}

		public static bool IsRunningFromNetwork()
        {
			return IsNetworkDirectory(ExeDirectory);
		}

		public static bool IsNetworkDirectory(string DirectoryName)
        {
			return IsNetworkDrive(Path.GetPathRoot(DirectoryName));
		}

		public static bool IsNetworkDrive(string DriveName)
        {
			try
			{
				DriveInfo info = new DriveInfo(DriveName);
				if (info.DriveType == DriveType.Network)
				{
					return true;
				}
				return false;
			}
			catch { }
			try
			{
				Uri uri = new Uri(DriveName);
				return uri.IsUnc;
			}
			catch
			{
				return false;
			}
		}

		static class NativeMethods
		{
			[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern IntPtr GetCurrentProcess();

			[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)]string moduleName);

			[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPWStr)]string procName);

			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);
		}

		/// <summary>
		/// The function determines whether the current operating system is a
		/// 64-bit operating system.
		/// </summary>
		/// <returns>
		/// The function returns true if the operating system is 64-bit;
		/// otherwise, it returns false.
		/// </returns>
		public static bool Is64BitOperatingSystem()
		{
			if (IntPtr.Size == 8) {  // 64-bit programs run only on Win64
				return true;
			} else {  // 32-bit programs run on both 32-bit and 64-bit Windows
				// Detect whether the current process is a 32-bit process
				// running on a 64-bit system.
				bool flag;
				return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
				         NativeMethods.IsWow64Process(NativeMethods.GetCurrentProcess(), out flag)) && flag);
			}
		}

		public static bool Is64BitProcess()
		{
			if (IntPtr.Size == 8)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// The function determins whether a method exists in the export
		/// table of a certain module.
		/// </summary>
		/// <param name="moduleName">The name of the module</param>
		/// <param name="methodName">The name of the method</param>
		/// <returns>
		/// The function returns true if the method specified by methodName
		/// exists in the export table of the module specified by moduleName.
		/// </returns>
		static bool DoesWin32MethodExist(string moduleName, string methodName)
		{
			IntPtr moduleHandle = NativeMethods.GetModuleHandle(moduleName);
			if (moduleHandle == IntPtr.Zero) {
				return false;
			}
			return (NativeMethods.GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
		}

		public static string BaseDirectory
		{
			get
			{
				string t = ExeDirectory;

				if (t.ToLower().EndsWith("_output")) {
					t = Path.GetDirectoryName(t);
					t = t.Replace("_output", "");
				}

				if (t.ToLower().EndsWith("release") || t.ToLower().EndsWith("debug"))
					t = Path.GetDirectoryName(t);

				if (t.ToLower().EndsWith("bin"))
					t = Path.GetDirectoryName(t);

				return t;
			}
		}

		public static string ExeDirectory
		{
			get
			{
				return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			}
		}


		private static string applicationName = null;

		public static string ApplicationName
		{
			get
			{
				if (applicationName == null)
					applicationName = ApplicationNameAuto();
				return applicationName;
			}
			set
			{
				applicationName = value;
			}
		}

		public static string ApplicationNameAuto()
		{
			return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
		}
		
		public static string ApplicationTitle(bool withVersion = false)
		{
			FileVersionInfo i = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
			string result = i.CompanyName + " " + i.ProductName;
			if (withVersion)
			{
				string[] e = i.ProductVersion.Split('.');
				if (e.Length < 4)
					result += i.ProductVersion;
				else
					result += " v." + string.Format("{0}.{1} [{2}.{3}]", e[0], e[1], e[2], e[3]);
			}
			return result;
		}
		
		public static string ApplicationExeName
		{
			get
			{
				return Assembly.GetEntryAssembly().Location;
			}
		}

        public static bool RegisterForAutoStart(string Parameters, bool Globals = false)
        {
            try
            {
                RegistryKey k;

                if (Globals)
                    k = Registry.LocalMachine.OpenSubKey(AutoRunRegistryKey, true);
                else
                    k = Registry.CurrentUser.OpenSubKey(AutoRunRegistryKey, true);

                string CmdLine = Assembly.GetEntryAssembly().Location;

				if (!string.IsNullOrEmpty(Parameters))
				{
					CmdLine = "\"" + CmdLine + "\"";
					CmdLine += " " + Parameters;
				}

                k.SetValue(ApplicationName, CmdLine);

                if (Globals)
                    UnregisterForAutoStart(false);

                return IsRegisteredForAutoStart(Globals);
            }
            catch
            {
                return false;
            }
        }

        public static bool RegisterForAutoStart(bool Globals = false)
        {
            return RegisterForAutoStart(null, Globals);
        }

        public static bool UnregisterForAutoStart(bool Globals = false)
        {
            try
            {
                RegistryKey k;

                if (Globals)
                    k = Registry.LocalMachine.OpenSubKey(AutoRunRegistryKey, true);
                else
                    k = Registry.CurrentUser.OpenSubKey(AutoRunRegistryKey, true);

                k.DeleteValue(ApplicationName);

                if (Globals)
                    UnregisterForAutoStart(false);

                return !IsRegisteredForAutoStart(Globals);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsRegisteredForAutoStart(bool Globals = false)
        {
            try
            {
                RegistryKey k;

                if (Globals)
                    k = Registry.LocalMachine.OpenSubKey(AutoRunRegistryKey, false);
                else
                    k = Registry.CurrentUser.OpenSubKey(AutoRunRegistryKey, false);

                string r = (string)k.GetValue(ApplicationName);
                if (string.IsNullOrEmpty(r)) return false;
                if (!r.StartsWith(Assembly.GetEntryAssembly().Location)) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
#if !NET5_0_OR_GREATER

        public static bool BringToForeground(Form form)
        {
            return SetForegroundWindow(form.Handle);
        }

#endif
		[DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
		[DllImport("user32.dll")] private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
		[DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private const int SW_HIDE = 0;
		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMAXIMIZED = 3;
		private const int SW_SHOWNOACTIVATE = 4;
		private const int SW_RESTORE = 9;
		private const int SW_SHOWDEFAULT = 10;

        private const UInt32 WM_CLOSE = 0x0010;

        private static Mutex singleInstanceMutex = null;
        private static bool singleInstanceOwner = false;
        private static string UniqueName = null;

		public static bool IsSingleInstance(string uniqueName = null)
		{
			try
			{
				if (singleInstanceMutex == null)
					DeclareInstance(uniqueName);
			}
			catch (TypeInitializationException ex)
            {
				DeclareInstance(uniqueName);
			}

			return singleInstanceOwner;
		}

		public static void DeclareInstance(string uniqueName = null)
		{
			string processName = Process.GetCurrentProcess().ProcessName;

			if (singleInstanceMutex == null)
			{
				if (uniqueName == null)
					uniqueName = processName;

                UniqueName = uniqueName;

                string mutexName = "Local\\SingleInstance_" + CompanyName + "_" + uniqueName;

				singleInstanceMutex = new Mutex(false, mutexName);
			}

			if (!singleInstanceOwner)
				singleInstanceOwner = singleInstanceMutex.WaitOne(0);
		}

		public static void ReleaseInstance()
		{
			if (singleInstanceMutex != null)
			{
				if (singleInstanceOwner)
					singleInstanceMutex.ReleaseMutex();

				singleInstanceMutex.Dispose();
				singleInstanceMutex = null;
			}
		}

        public static bool RestorePreviousInstance()
        {
            return RestorePreviousInstance(false);
        }


        public static bool RestorePreviousInstance(bool UseEvent)
        {
            if (UseEvent)
            {
                return RestorePreviousInstanceEvent();
            }
            else
            {
                return RestorePreviousInstanceWindow();
            }
        }

        public static bool RestorePreviousInstanceEvent()
        {
            bool result = false;
            string eventName = "Local\\SingleInstanceEvent_" + CompanyName + "_" + UniqueName;
            try
            { 
                EventWaitHandle eventHandle = EventWaitHandle.OpenExisting(eventName);
                result = eventHandle.Set();
                eventHandle.Close();
            }
            catch { }
            return result;
        }

        public static bool WaitRestoreInstanceEvent()
        {
            bool result = false;
            string eventName = "Local\\SingleInstanceEvent_" + CompanyName + "_" + UniqueName;
            try
            {
                logger.debug("Creating event {0}", eventName);
                EventWaitHandle eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, eventName, out bool createdNew);
                if (createdNew)
                {
                    logger.debug("Waiting for event");
                    result = eventHandle.WaitOne();
                    logger.debug("Wait done, rc={0}", result);
                }
                else
                {
                    logger.debug("Event already exists");
                }
                eventHandle.Close();
            }
            catch (Exception e)
            {
                logger.warning(e.Message);
            }
            return result;
        }

        public static bool RestorePreviousInstanceWindow()
        { 
            Process thisProcess = Process.GetCurrentProcess();
			string processName = thisProcess.ProcessName;
			Process[] processes = Process.GetProcessesByName(processName);

			if (processes.Length > 1)
			{
				for (int i=0; i<processes.Length; i++)
				{
					if (processes[i].Id != thisProcess.Id)
					{
						/* Restore this instance */
						IntPtr hWnd = processes[i].MainWindowHandle;
						if (IsIconic(hWnd))
							ShowWindowAsync(hWnd, SW_RESTORE);
						if (SetForegroundWindow(hWnd))
							return true;
					}
				}
			}

			return false;
		}

        public static string ApplicationGuid
		{
			get
			{
				Assembly assembly = Assembly.GetEntryAssembly();
				try
				{
					GuidAttribute attribute = (GuidAttribute) assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
					return attribute.Value;
				}
				catch
				{
					/* The application does not have a GUID */
					return null;
				}
			}
		}

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

		public static void ApplicationExit()
		{
			Logger.Debug("Invoking garbagge collector...");
			GC.Collect();

			ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
			if (currentThreads.Count > 0)
				Logger.Debug("{0} thread(s) are still running, killing them...", currentThreads.Count);

			foreach (ProcessThread thread in currentThreads)
			{
				IntPtr ptrThread = OpenThread(1, false, (uint)thread.Id);
				
				//if (AppDomain.GetCurrentThreadId() != thread.Id)
				if (Process.GetCurrentProcess().Threads[0].Id != thread.Id)
				{
					try
					{
						TerminateThread(ptrThread, 1);
					}
					catch { }
				}
			}

			Logger.Debug("Invoking garbagge collector again");
			GC.Collect();
			Logger.Debug("Done exiting");
		}


		/// <summary>
		/// Validate a (auth) key
		/// </summary>
		/// <param name="key">The key to validate</param>
		/// <param name="maxLength">The awaited key size</param>
		/// <returns>True if the key is valid, else false</returns>
		/*public static bool isKeyValid(string key, int maxLength = 16)
		{
			if (key.Trim().Equals(""))
				return false;

			if (key.Length != maxLength)
				return false;

			for (int i = 0; i < key.Length; i++)
				if (!BinConvert.IsHex(key[i]))
					return false;
			return true;
		}*/

		public static bool IsSpringCardPrivate()
        {
			string t;

			t = Environment.GetEnvironmentVariable("SPRINGCARD_PRIVATE");

			if (!string.IsNullOrEmpty(t))
				return StrUtils.ReadBoolean(t);

			t = Environment.UserDomainName;
			if (!string.IsNullOrEmpty(t) && (t.ToLower() == CompanyName.ToLower()))
				return true;

			return false;
		}

		public static bool VerifyAssemblies(bool messageOnError = true)
        {
			if (!AppCheck.VerifyAssemblies())
            {
				if (messageOnError || IsSpringCardPrivate())
				{
					string[] missingAssemblies = AppCheck.GetMissingAssemblyNames();

					MessageBox.Show(
						"This application failed to start because one or more of its components are missing:\n\n" +
						string.Join("\n", missingAssemblies) + "\n\n" +
						"Re-installing the application may solve the problem.",
						"Required assemblies missing",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}

				return false;
			}

			return true;
		}

		public static bool SetLanguage(string lang, string[] supportedLangages = null, bool messageOnError = true)
		{
			return SetLanguage(lang, new string[0], supportedLangages, messageOnError);
		}

		public static bool SetLanguage(string lang, string catalogs, string[] supportedLangages = null, bool messageOnError = true)
        {
			return SetLanguage(lang, catalogs.Split(';'), supportedLangages, messageOnError);
		}

		public static bool SetLanguage(string lang, string[] catalogs, string[] supportedLangages = null, bool messageOnError = true)
		{
			Translation.DefaultCulture = new System.Globalization.CultureInfo("en");

			if (!string.IsNullOrEmpty(lang))
			{
				if (supportedLangages == null)
					supportedLangages = new string[] { "en", "fr" };

				for (int i=0; i<supportedLangages.Length; i++)
                {
					if (lang == supportedLangages[i])
					{
						VerifyAssemblies();

						Logger.Debug("Selecting language {0}", lang);
						Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
						Translation.Culture = Thread.CurrentThread.CurrentUICulture;
						Logger.Debug("Loading translations...");

						bool result = true;

						string[] assemblyNames = AppCheck.GetLoadedAssemblyNames();

						for (int j = 0; j < assemblyNames.Length; j++)
						{
							string catalog = Path.GetFileNameWithoutExtension(assemblyNames[j]);
							if (catalog.StartsWith("SpringCard."))
							{
								if (!Translation.AddCatalog(catalog))
									result = false;
							}
						}

						if (catalogs != null)
						{
							for (int j = 0; j < catalogs.Length; j++)
							{
								if (!Translation.AddCatalog(catalogs[j]))
									result = false;
							}
						}

						if (!result)
						{
							if (messageOnError)
							{
								MessageBox.Show(
									"The specified language does not exist or its support files are missing. Try to run the application with the default languague.",
									"Unsupported language",
									MessageBoxButtons.OK, MessageBoxIcon.Error);
							}
						}
						return result;
					}
				}
			}

			return true;
		}

	}

}
