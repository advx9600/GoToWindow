using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace GoToWindow.Api
{
    public static class ProcessExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessExtensions).Assembly, "GoToWindow");

        // ReSharper disable InconsistentNaming
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        // ReSharper restore InconsistentNaming

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags, StringBuilder lpExeName, out int size);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);

        public static string GetExecutablePath(int id)
        {
            return GetExecutablePath(System.Diagnostics.Process.GetProcessById(id));
        }

        public static string GetExecutablePath(this Process process)
        {
            if (process == null || process.Id == 0)
                return null;

            try
            {
                //If running on Vista or later use the new function
                if (!string.IsNullOrEmpty(process.MainModule.FileName))
                {
                    return process.MainModule.FileName; ;
                }
                return Environment.OSVersion.Version.Major >= 6
                    ? GetExecutablePathAboveVista(process.Id)
                    : process.MainModule.FileName;
            }
            catch (Win32Exception exc)
            {
                Log.Error($"Error while trying to get executable path of process {process.ProcessName} with PID {process.Id}", exc);
                return null;
            }
        }

        private static string GetExecutablePathAboveVista(int processId)
        {
            var buffer = new StringBuilder(1024);
            var hprocess = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, processId);

            if (hprocess == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                if (QueryFullProcessImageName(hprocess, 0, buffer, out _))
                {
                    return buffer.ToString();
                }
            }
            finally
            {
                CloseHandle(hprocess);
            }

            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private static string GetMainModuleFilepath(int processId)
        {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using (var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null)
                    {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }

        public static void StartProcessAsync(string exePath)
        {
            Task task = new Task(() =>
            {
                StartProcess(exePath);
            });
            task.Start();
        }
        private static void StartProcess(string exeFile)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;

            p.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(exeFile);
            p.StartInfo.FileName = exeFile;

            p.StartInfo.CreateNoWindow = true;
            try
            {
                p.Start();
            }
            catch (Exception e)
            {
                // 如果普通权限运行失败，则以管理员身份运行
                // 参考网址 https://www.cnblogs.com/xuan52rock/p/5777694.html
                Console.WriteLine("run process failed\n" + e.Message);
                try
                {
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.Verb = "runas";
                    p.StartInfo.RedirectStandardOutput = false;
                    p.Start();
                }
                catch (Exception e2)
                {
                    //MessageBox.Show(e2.ToString() + "\n" + e.ToString());
                    Console.WriteLine("run as admin failed\n" + e2.Message);
                }
            }
        }
    }
}