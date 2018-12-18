using System;
using System.Collections.Generic;
using System.Linq;
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
using GoToWindow.Api;

namespace GoToWindow.Windows
{
    /// <summary>
    /// WinShortcut.xaml 的交互逻辑
    /// </summary>
    public partial class WinShortcut : Window
    {
        public WinShortcut()
        {
            InitializeComponent();
        }
        private IWindowEntry mWin;
        internal void Show(IWindowEntry win)
        {
            mWin = win;
            BoxInfo.Text = win.ProcessName + "     快捷键:" + (Key)win.hotKey + "\n" + win.Title;
            BoxInfo.Text += "\n按alt+tab，松开tab,然后按快捷键，即可切换到对应的窗口";
            BoxInfo.Text += "\n按Delete，删除当前快捷键";
            Show();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
            else
            {

                if (MessageBox.Show(e.Key == Key.Delete ? string.Format("确认快捷键{0}吗?", (Key)mWin.hotKey) : string.Format("确认快捷键是 {0} 吗", e.Key), "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    SaveHotKey(mWin, e.Key == Key.Delete ? 0 : e.Key);
                    Close();
                };
                e.Handled = true;
            }
        }

        private void SaveHotKey(IWindowEntry win, Key key)
        {
            if (win.hotKey != (int)key)
            {
                win.hotKey = (int)key;
                Database.updateHotkey(win);
                //win.onHotkeyUpdate();
            }
        }
    }
}
