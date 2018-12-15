using GoToWindow.Api;
using GoToWindow.Plugins.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GoToWindow.Windows
{
    /// <summary>
    /// MainWindow2.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow2 : Window
    {
        public MainWindow2()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            mListHotkey = Database.GetAllHotKey();
        }

        private void SetHotkey(IWindowEntry win)
        {
            bool find = false;
            foreach (var item in mListHotkey)
            {
                if (win.ProcessName.Equals(item.name))
                {
                    win.hotKey = item.key;
                    find = true;
                    break;
                }
            }
            if (!find)
            {
                win.hotKey =0;
            }
        }
        const int setElementWidth = 60;
        const int setElementMargin = 10;

        public void resetData()
        {
            var isNeedRest = false;
            var wins = WindowsListFactory.Load();
            if (mWins == null)
            {
                isNeedRest = true;
                mWins = wins;
                int i = 0;
                foreach (var win in wins.Windows)
                {
                    i++;
                    var btn = buildBtn(win, setElementWidth, setElementMargin);
                    if (i == 2) mFocusBtn = btn;
                    stackPanel.Children.Add(btn);
                }

            }
            else
            {
                for (int i = 0; i < wins.Windows.Count; i++)
                {
                    var win = wins.Windows.ElementAt(i);
                    //Console.WriteLine(i + "   " + win.ProcessName);
                    int findIndex = -1;
                    for (int j = 0; j < mWins.Windows.Count; j++)
                    {
                        var mWin = mWins.Windows.ElementAt(j);
                        if (win.HWnd == mWin.HWnd)
                        {
                            findIndex = j;
                            break;
                        }
                    }
                    if (findIndex > -1)
                    {
                        // 需要交换位置
                        if (i != findIndex)
                        {
                            isNeedRest = true;
                            var first = stackPanel.Children[i];
                            var second = stackPanel.Children[findIndex];
                            stackPanel.Children.Remove(first);
                            stackPanel.Children.Remove(second);
                            stackPanel.Children.Insert(i, second);
                            stackPanel.Children.Insert(findIndex, first);

                            var firstL = mWins.Windows.ElementAt(i);
                            var secondL = mWins.Windows.ElementAt(findIndex);
                            mWins.Windows.Remove(firstL);
                            mWins.Windows.Remove(secondL);
                            mWins.Windows.Insert(i, secondL);
                            mWins.Windows.Insert(findIndex, firstL);
                        }
                    }
                    else
                    {
                        isNeedRest = true;
                        mWins.Windows.Insert(i, win);
                        stackPanel.Children.Insert(i, buildBtn(win, setElementWidth, setElementMargin));
                    }
                    if (i == 1)
                    {
                        mFocusBtn = stackPanel.Children[1] as Button;
                    }
                    // 去掉多余的
                    if (i == wins.Windows.Count - 1)
                    {
                        if (mWins.Windows.Count > wins.Windows.Count)
                        {
                            isNeedRest = true;
                            stackPanel.Children.RemoveRange(wins.Windows.Count, mWins.Windows.Count - wins.Windows.Count);
                        }
                    }
                }
            }

            if (isNeedRest)
            {
                resetWindowSize();
            }
        }

        private void resetWindowSize()
        {
            Height = 100;
            Width = stackPanel.Children.Count * (setElementWidth + setElementMargin * 2) + 20;
        }
        private Button buildBtn(IWindowEntry win, int setElementWidth, int setElementMargin)
        {
            // http://www.bubuko.com/infodetail-2282119.html
            // https://blog.csdn.net/honantic/article/details/48781543

            //https://bbs.csdn.net/topics/392094061
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/99856840-f8ef-4547-9150-c4c46ec2f3df/need-dashed-focus-box?forum=wpf

            Button btn = new Button();
            btn.Width = btn.Height = setElementWidth;
            btn.Margin = new Thickness(setElementMargin, 0, setElementMargin, 0);
            btn.Background = new ImageBrush(CoreSearchResult.LoadIcon(win));
            btn.BorderBrush = null;
            btn.GotFocus += Btn_GotFocus;
            btn.FocusVisualStyle = (Style)FindResource("newFocusStyle");
            btn.PreviewMouseDown += Btn_PreviewMouseDown;
            btn.Tag = win;
            win.onHotkeyUpdate = OnHotkeyUpdateEvent;
            SetHotkey(win);
            return btn;
        }

        private void OnHotkeyUpdateEvent()
        {
            mListHotkey = Database.GetAllHotKey();
            // 把key更新
            foreach(var win in mWins.Windows)
            {
                SetHotkey(win);
            }
        }

        private void Btn_GotFocus(object sender, RoutedEventArgs e)
        {
            mFocusBtn = sender as Button;

            if (Keyboard.IsKeyUp(Key.LeftAlt))
            {
                onHotkeyEvent(Key.LeftAlt, Key.Up);
            }
        }

        private Button mRightClickBtn;
        private void Btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

            }
            else
            {
                ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
                cm.PlacementTarget = sender as Button;
                mRightClickBtn = sender as Button;
                cm.IsOpen = true;
            }
        }


        private bool isShow = false;
        private WindowsList mWins;
        private Button mFocusBtn;
        private List<TbHotKeyEntry> mListHotkey;
        private byte VK_MENU = 18;
        private int KEYEVENTF_EXTENDEDKEY =1;
        private int KEYEVENTF_KEYUP =2;

        public void ShowFront()
        {
            if (!isShow)
            {
                isShow = true;
                resetData();
                Activate();
                Show();
                // 需要确保显示
                IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

                SetForegroundWindowInternal(hwnd);
                if (mWins != null && mWins.Windows.Count > 1)
                {
                    mFocusBtn.Focus();
                }
            }
            else
            {
                // 接收 alt + tab 和 alt + shift +tab和 alt + esc按键
                onHotkeyEvent((Key)KeyboardHook.StaticKey, Key.Enter);
            }
        }

        override
        protected void OnKeyDown(KeyEventArgs e)
        {
            onHotkeyEvent(e.SystemKey, Key.Down);
        }

        private void MoveFocus(FocusNavigationDirection direction)
        {
            //var request = new TraversalRequest(direction);
            TraversalRequest request = new TraversalRequest(direction);

            // Gets the element with keyboard focus.
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus != null)
            {
                elementWithFocus.MoveFocus(request);
            }
        }

        // https://www.codeproject.com/Tips/76427/How-to-bring-window-to-top-with-SetForegroundWindo
        // https://www.cnblogs.com/rosesmall/p/5759804.html
        // https://blog.csdn.net/chengjunlin0793/article/details/49950387
        private void SetForegroundWindowInternal(IntPtr hWnd)
        {
            if (!SetForegroundWindow(hWnd))
            {
                keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                SetForegroundWindow(hWnd);
                keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            }
        }
        [DllImport("User32.dll")]
        public static extern void keybd_event(Byte bVk, Byte bScan, Int32 dwFlags, Int32 dwExtraInfo);

        [DllImport("user32.dll ", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll ", SetLastError = true)]
        private static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);
        private void showWin(IWindowEntry tag)
        {
            ShowWindow(tag.HWnd, 1);
            SwitchToThisWindow(tag.HWnd, true);
            HideWin();
        }
        private void onHotkeyEvent(Key key, Key direct)
        {
            //Console.WriteLine(key + "  " + direct);
            if (direct == Key.Enter)
            {
                if (Key.KanaMode == key) // tab key
                {
                    if (!Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        this.MoveFocus(FocusNavigationDirection.Next);
                    }
                    else
                    {
                        this.MoveFocus(FocusNavigationDirection.Previous);
                    }
                }
                else if (Key.Select == key) // esc key
                {
                    HideWin();
                }
            }
            else if (direct == Key.Up)
            {
                // 逻辑上只需要捕捉alt键，但有时候按键太快，还需要捕捉tab键
                if (key == Key.Tab && !Keyboard.IsKeyDown(Key.LeftAlt) || key == Key.LeftAlt)
                {
                    var tag = (IWindowEntry)mFocusBtn.Tag;
                    showWin(tag);
                }
            }
            else if (direct == Key.Down)
            {
                switch (key)
                {
                    case Key.Delete: removeWindow(); break;
                    case Key.Right: this.MoveFocus(FocusNavigationDirection.Next); break;
                    case Key.Left: this.MoveFocus(FocusNavigationDirection.Previous); break;
                    default:
                        foreach (var item in mWins.Windows) { if (item.hotKey > 0 && item.hotKey == (int)key) { showWin(item); break; } }
                        break;
                }
            }
        }

        private void removeWindow()
        {
            // 关闭窗口
            WindowEntryFactory.CloseWindow(mFocusBtn.Tag as IWindowEntry);

            int index = stackPanel.Children.IndexOf(mFocusBtn);
            mWins.Windows.Remove(mFocusBtn.Tag as IWindowEntry);
            stackPanel.Children.Remove(mFocusBtn);
            // 聚焦到后面一个Window
            if (index < stackPanel.Children.Count) stackPanel.Children[index].Focus();
            else if (index > 0) stackPanel.Children[index - 1].Focus();
            if (stackPanel.Children.Count == 0)
                HideWin();
            else
                resetWindowSize();

        }
        override
        protected void OnDeactivated(EventArgs e)
        {
            HideWin();
        }
        override
        protected void OnKeyUp(KeyEventArgs e)
        {
            onHotkeyEvent(e.Key, Key.Up);
        }
        public void HideWin()
        {
            if (isShow)
            {
                isShow = false;
                Hide();
            }
        }
        private void MenuItem_Add_Shortcut_Click(object sender, RoutedEventArgs e)
        {
            var win = new WinShortcut();
            win.Show(mRightClickBtn.Tag as IWindowEntry);
        }
    }
}
