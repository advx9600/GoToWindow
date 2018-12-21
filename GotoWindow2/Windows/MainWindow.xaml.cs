using GoToWindow.Api;
using GoToWindow.Plugins.Core.ViewModel;
using GotoWindow2.DB;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GotoWindow2.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            mListHotkey = TbHotkey.GetAllHotKey();
            mListHideWin = TbHideWin.GetAll();
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
                win.hotKey = 0;
            }
        }
        const int setElementWidth = 60;
        const int setElementMargin = 10;

        public void resetData()
        {
            var isNeedRest = false;
            var wins = WindowsListFactory.Load();
            // remove hide window
            if (mListHideWin.Count > 0)
                for (int i = 0; i < wins.Windows.Count; i++)
                {
                    var win = wins.Windows.ElementAt(i);
                    for (int j = 0; j < mListHideWin.Count; j++)
                    {
                        if (win.ProcessName.Equals(mListHideWin.ElementAt(j).name) && win.Title.Equals(mListHideWin.ElementAt(j).title))
                        {
                            wins.Windows.RemoveAt(i--);
                            break;
                        }
                    }
                }

            if (mWins == null)
            {
                isNeedRest = true;
                mWins = wins;
                int i = 0;
                foreach (var win in wins.Windows)
                {
                    i++;
                    var btn = buildBtn(win, setElementWidth, setElementMargin);
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
                    // 去掉多余的
                    if (i == wins.Windows.Count - 1)
                    {
                        while (mWins.Windows.Count > wins.Windows.Count)
                        {
                            isNeedRest = true;
                            stackPanel.Children.RemoveAt(mWins.Windows.Count - 1);
                            mWins.Windows.RemoveAt(mWins.Windows.Count - 1);
                        }
                    }
                }
            }

            if (stackPanel.Children.Count > 0)
                mFocusBtn = stackPanel.Children[stackPanel.Children.Count > 1 ? 1 : 0] as Button;

            if (isNeedRest)
            {
                resetWindowSize();
            }
        }

        private void resetWindowSize()
        {
            Height = 100;
            Width = stackPanel.Children.Count * (setElementWidth + setElementMargin * 2) + 20;
            Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
            Top = SystemParameters.PrimaryScreenHeight / 2 - Height / 2;
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
            var backImg = CoreSearchResult.LoadIcon(win);
            if (backImg == null && !string.IsNullOrEmpty(win.ProcessName))
            {
                btn.Background = GoToWindow.Plugins.Core.Utils.IconLoader.DrawText(win.ProcessName, null, System.Drawing.Color.White, System.Drawing.Color.Transparent);
            }
            else
            {
                btn.Background = new ImageBrush(backImg);
            }
            btn.BorderBrush = null;
            btn.GotFocus += Btn_GotFocus;
            btn.FocusVisualStyle = null;
            btn.PreviewMouseDown += Btn_PreviewMouseDown;
            btn.Tag = win;
            SetHotkey(win);
            return btn;
        }

        private void OnHotkeyUpdateEvent()
        {
            mListHotkey = TbHotkey.GetAllHotKey();
            // 把key更新
            foreach (var win in mWins.Windows)
            {
                SetHotkey(win);
            }
        }

        private void OnHideWinUpdate()
        {
            mListHideWin = TbHideWin.GetAll();
        }
        private void Btn_GotFocus(object sender, RoutedEventArgs e)
        {
            mFocusBtn = sender as Button;
        }

        private Button mRightClickBtn;
        private void Btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                switchToWin(((Button)sender).Tag as IWindowEntry);
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
        private List<TbHideWinEntry> mListHideWin;

        public void ShowFront()
        {
            if (!isShow)
            {
                resetData();
                if (mWins != null && mWins.Windows.Count > 0)
                {
                    isShow = true;
                    Activate();
                    Show();
                    // 需要确保显示
                    IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

                    WindowToForeground.SetForegroundWindowInternal(hwnd);
                    mFocusBtn.Focus();
                }
            }
            else
            {
                // 接收 alt + tab 和 alt + shift +tab和 alt + esc按键
                onHotkeyEvent((Key)KeyboardHookAlt.StaticKey, Key.Enter);
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

        private void switchToWin(IWindowEntry tag)
        {
            WindowToForeground.ForceWindowToForeground(tag.HWnd);
            HideWin();
        }
        private void onHotkeyEvent(Key key, Key direct)
        {
            // Console.WriteLine(key + "  " + direct+" alt:"+ Keyboard.IsKeyDown(Key.LeftAlt));

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
                if (key == Key.Escape) HideWin();
            }
            else if (direct == Key.Down)
            {
                switch (key)
                {
                    case Key.Delete: removeWindow(); break;
                    case Key.Right: this.MoveFocus(FocusNavigationDirection.Next); break;
                    case Key.Left: this.MoveFocus(FocusNavigationDirection.Previous); break;
                    default:
                        var find = false;
                        // 如果是自定义的按键
                        foreach (var item in mWins.Windows)
                        {
                            if (item.hotKey > 0 && item.hotKey == (int)key)
                            { find = true; switchToWin(item); break; }
                        }
                        // 如果该窗口未打开，则打开该应用
                        if (!find)
                        {
                            foreach (var item in mListHotkey)
                            {
                                if ((int)key == item.key)
                                {
                                    try
                                    {
                                        ProcessExtensions.StartProcessAsync(string.IsNullOrEmpty(item.executablePath) ? item.name : item.executablePath);
                                        find = true;
                                        HideWin();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("start process failed:" + e.Message);
                                    }
                                    break;
                                }
                            }
                        }
                        if (!find)
                        {
                            // 如果不是自定义的热键
                            switch (key)
                            {
                                case Key.D2:
                                    var findD2Action = false;
                                    // 寻找第二个图标的相同的窗口,找到则显示
                                    for (var i = 2; i < mWins.Windows.Count; i++)
                                    {
                                        if (mWins.Windows.ElementAt(1).ProcessName.Equals(mWins.Windows.ElementAt(i).ProcessName))
                                        {
                                            switchToWin(mWins.Windows.ElementAt(i));
                                            findD2Action = true;
                                            break;
                                        }
                                    }
                                    if (!findD2Action)
                                    {
                                        // 寻找第一个图标的相同的窗口,找到则显示
                                        for (var i = 2; i < mWins.Windows.Count; i++)
                                        {
                                            if (mWins.Windows.ElementAt(0).ProcessName.Equals(mWins.Windows.ElementAt(i).ProcessName))
                                            {
                                                switchToWin(mWins.Windows.ElementAt(i));
                                                findD2Action = true;
                                                break;
                                            }
                                        }
                                    }
                                    break; // 数字键2,打开第二个相同的程序窗口
                            }
                        }
                        break;
                }
            }
            // 逻辑上只需要捕捉alt键，但有时候按键太快,会捕捉失败，需要特别判断
            if (Keyboard.IsKeyUp(Key.LeftAlt) && isShow)
            {
                if (mFocusBtn != null)
                {
                    var tag = (IWindowEntry)mFocusBtn.Tag;
                    switchToWin(tag);
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
        private void MenuItem_SetHotKey_Click(object sender, RoutedEventArgs e)
        {
            new WinSetHotkey().Show(mRightClickBtn.Tag as IWindowEntry, OnHotkeyUpdateEvent);
        }

        private void MenuItem_Exit_Application_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void MenuItem_NotShow_Click(object sender, RoutedEventArgs e)
        {
            new WinHideApp().Show(mRightClickBtn.Tag as IWindowEntry, OnHideWinUpdate);
        }
    }
}
