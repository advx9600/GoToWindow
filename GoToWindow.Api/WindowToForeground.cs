﻿using System;
using System.Runtime.InteropServices;
using log4net;

namespace GoToWindow.Api
{
	/// <remarks>
	/// Thanks to Shlomi Ohayon for the solution: http://www.shloemi.com/2012/09/solved-setforegroundwindow-win32-api-not-always-works/
	/// </remarks>
	public static class WindowToForeground
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(WindowToForeground).Assembly, "GoToWindow");

// ReSharper disable InconsistentNaming
		private const int SC_RESTORE = 0xF120;
		private const uint WM_SYSCOMMAND = 0x0112;

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWPLACEMENT
		{
			public int Length;
			public int Flags;
			public ShowWindowCommands ShowCmd;
			public POINT MinPosition;
			public POINT MaxPosition;
			public RECT NormalPosition;
		}

		public enum ShowWindowCommands
		{
			Hide = 0,
			Normal = 1,
			ShowMinimized = 2,
			Maximize = 3,
			ShowMaximized = 3,
			ShowNoActivate = 4,
			Show = 5,
			Minimize = 6,
			ShowMinNoActive = 7,
			ShowNA = 8,
			Restore = 9,
			ShowDefault = 10,
			ForceMinimize = 11
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X, Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left, Top, Right, Bottom;
		}
		// ReSharper restore InconsistentNaming

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

		[DllImport("kernel32.dll")]
		public static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		[DllImport("user32.dll")]
		internal static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);


		public static void AttachedThreadInputAction(Action action)
		{
			var foreThread = GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero);
			var appThread = GetCurrentThreadId();
			var threadsAttached = false;

			try
			{
				threadsAttached =
					foreThread == appThread ||
					AttachThreadInput(foreThread, appThread, true);

				if (threadsAttached)
					action();
				else 
					Log.Warn(
						$"Cannow bring window to foreground. Could not attach to thread {appThread} from fore thread {foreThread}");
			}
			finally
			{
				if (threadsAttached)
					AttachThreadInput(foreThread, appThread, false);
			}
		}

		public static bool ForceWindowToForeground(IntPtr hwnd)
		{
			bool result = false;

			AttachedThreadInputAction(() =>
			{
				GetWindowPlacement(hwnd, out WINDOWPLACEMENT state);
				if (state.ShowCmd == ShowWindowCommands.ShowMinimized)
				{
					PostMessage(hwnd, WM_SYSCOMMAND, SC_RESTORE, 0);
				}

				SetForegroundWindow(hwnd);

				result = true;
			});

			return result;
		}

        // https://www.codeproject.com/Tips/76427/How-to-bring-window-to-top-with-SetForegroundWindo
        // https://www.cnblogs.com/rosesmall/p/5759804.html
        // https://blog.csdn.net/chengjunlin0793/article/details/49950387
        public static void SetForegroundWindowInternal(IntPtr hWnd)
        {
            if (!SetForegroundWindow(hWnd))
            {
                KeyboardSend.Alt_KEYEVENTF_EXTENDEDKEY();
                SetForegroundWindow(hWnd);
                KeyboardSend.Alt_KEYEVENTF_KEYUP();
            }
        }
    }
}
