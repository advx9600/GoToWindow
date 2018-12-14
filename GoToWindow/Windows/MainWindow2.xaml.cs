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
                    Console.WriteLine(i + "   " + win.ProcessName);
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
                        mWins = wins;
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
            return btn;
        }

        private void Btn_GotFocus(object sender, RoutedEventArgs e)
        {
            mFocusBtn = sender as Button;
        }

        private void Btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

            }
            else
            {
                ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
                cm.PlacementTarget = sender as Button;
                cm.IsOpen = true;
            }
        }


        private bool isShow = false;
        private WindowsList mWins;
        private Button mFocusBtn;


        public void ShowFront()
        {
            if (!isShow)
            {
                isShow = true;
                resetData();
                Show();
                Activate();
                if (mWins != null && mWins.Windows.Count > 1)
                {
                    mFocusBtn.Focus();
                }

            }
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4 ||
               Keyboard.Modifiers == ModifierKeys.Control && e.SystemKey == Key.Escape)
            {
                e.Handled = true;
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }

        }

        override
        protected void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape: HideWin(); break;
                case Key.Delete: removeWindow(); break;
                default: onHotkeyEvent(e.Key, Key.Down); break;
            }
        }

        private void onHotkeyEvent(Key key, Key down)
        {

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
            MessageBox.Show("aadsf");
        }
    }
}
