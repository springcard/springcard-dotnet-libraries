using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Xml.XPath;
using System.ServiceProcess;

namespace SpringCard.LibCs.Windows
{
    public class Services
    {
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool QueryServiceStatus(System.IntPtr serviceHandle, ref ServiceStatus serviceStatus);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr serviceHandle, ref ServiceStatus serviceStatus);

        public static bool SetServiceStatus(IntPtr serviceHandle, ServiceState serviceState, int serviceWaitHint = 0)
        {
            try
            {
                ServiceStatus serviceStatus = new ServiceStatus();
                if (!QueryServiceStatus(serviceHandle, ref serviceStatus))
                    return false;
                serviceStatus.dwCurrentState = serviceState;
                serviceStatus.dwWaitHint = serviceWaitHint;
                return SetServiceStatus(serviceHandle, ref serviceStatus);
            }
            catch
            {
                return false;
            }
        }
    }
}
