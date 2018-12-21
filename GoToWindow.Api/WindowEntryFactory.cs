using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GoToWindow.Api
{
    public static class WindowEntryFactory
    {
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        public static WindowEntry Create(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);

            return Create(hWnd, processId);
        }

        public static WindowEntry Create(IntPtr hWnd, uint processId)
        {
            var windowTitle = GetWindowTitle(hWnd);

            var iconHandle = WindowIcon.GetAppIcon(hWnd);
            var isVisible = !IsIconic(hWnd);
            var executablePath = ProcessExtensions.GetExecutablePath((int)processId);


            return new WindowEntry
            {
                HWnd = hWnd,
                Title = windowTitle,
                ProcessId = processId,
                IconHandle = iconHandle,
                IsVisible = isVisible,
                ExecutablePath = executablePath
            };
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            var lLength = GetWindowTextLength(hWnd);
            if (lLength == 0)
                return null;

            var builder = new StringBuilder(lLength);
            GetWindowText(hWnd, builder, lLength + 1);
            return builder.ToString();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, uint wParam, int lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        private const UInt32 WM_CLOSE = 0x0010;

        public static void CloseWindow(IWindowEntry win)
        {
            // https://blog.csdn.net/hellokandy/article/details/53408799
            SendMessageTimeout(win.HWnd, WM_CLOSE, 0, 0, 0, 50, out IntPtr iconHandle);
        }
    }
}
