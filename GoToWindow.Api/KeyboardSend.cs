﻿using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
    public static class KeyboardSend
    {
        public static byte LWin = 0x5B; //VK_LWIN
        public static byte LAlt = 0xA4; //VK_LMENU
        public static byte Tab = 0x09; //VK_TAB

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        // ReSharper disable InconsistentNaming
        private const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 2;
        // ReSharper restore InconsistentNaming

        public static void KeyPress(byte vKey)
        {
            KeyDown(vKey);
            KeyUp(vKey);
        }

        public static void KeyDown(byte vKey)
        {
            keybd_event(vKey, 0, KEYEVENTF_EXTENDEDKEY, 0);
        }

        public static void KeyUp(byte vKey)
        {
            keybd_event(vKey, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }
    }
}