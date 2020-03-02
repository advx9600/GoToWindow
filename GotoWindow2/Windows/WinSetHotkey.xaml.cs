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
using GotoWindow2.DB;

namespace GotoWindow2.Windows
{
    /// <summary>
    /// WinSetHotkey.xaml 的交互逻辑
    /// </summary>
    public partial class WinSetHotkey : Window
    {
        private IWindowEntry mWin;
        private Action mCallback;
        public WinSetHotkey()
        {
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key[] ignoreKeys = { Key.LeftAlt, Key.RightAlt, Key.LeftCtrl, Key.LeftCtrl, Key.LeftShift, Key.RightShift };
            foreach (var ignoreKey in ignoreKeys)
            {
                if (ignoreKey == e.Key)
                    return;
            }

            if (e.Key == Key.Escape) Close();
            else
            {
                if (e.Key == Key.F11)
                {
                    // 进入详细设置
                    new WinSetHotkeyDetail().Show(mWin, mCallback);
                    Close();
                }
                else if (MessageBox.Show(e.Key == Key.Delete ? string.Format("确认删除快捷键{0}吗?", (Key)mWin.hotKey) : string.Format("确认快捷键是 {0} 吗", e.Key), "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    SaveHotKey(mWin, e.Key == Key.Delete ? 0 : e.Key);
                    Close();
                };
                e.Handled = true;
            }
        }

        internal void Show(IWindowEntry win, Action callback)
        {
            mWin = win;
            mCallback = callback;
            BoxInfo.Text = "请直接输入快捷键\n";
            BoxInfo.Text += "\n说明：按alt+tab，松开tab,然后按快捷键，即可切换到对应的窗口";
            BoxInfo.Text += "\n\n当前信息：" + win.ProcessName + "     快捷键:" + (Key)win.hotKey + "   " + win.Title;
            BoxInfo.Text += "\n\n按Delete，删除当前快捷键\n按F11进入详细设置";
            Show();
        }

        private void SaveHotKey(IWindowEntry win, Key key)
        {
            if (win.hotKey != (int)key)
            {
                win.hotKey = (int)key;
                TbHotkey.updateHotkey(win, string.IsNullOrEmpty(win.ExecutablePath) ? false : true);
                mCallback();
            }
        }
    }
}
