﻿using System;

namespace GoToWindow.Api
{
	public interface IWindowEntry
	{
		IntPtr HWnd { get; set; }
		uint ProcessId { get; set; }
		string ProcessName { get; set; }
		//TODO: Windows 10 App Icons
		//string ProcessFileName { get; set; }
		string Title { get; set; }
		IntPtr IconHandle { get; set; }
		bool IsVisible { get; set; }

		bool Focus();
		bool IsForeground();
		bool IsSameWindow(IWindowEntry other);

        int hotKey { get; set; }
    }
}
