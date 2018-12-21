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
    /// WinSetHotkeyDetail.xaml 的交互逻辑
    /// </summary>
    public partial class WinSetHotkeyDetail : Window
    {
        private Action mCallback;

        public WinSetHotkeyDetail()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            dataGrid1.ItemsSource = TbHotkey.GetAll();
        }
        internal void Show(IWindowEntry win, Action callback)
        {
            mCallback = callback;
            Show();
        }

        private void Button_Del_Click(object sender, RoutedEventArgs e)
        {
            TbHotkey.Del((sender as Button).DataContext as TbHotKeyEntity);
            RefreshData();
            mCallback();
        }
        private void Button_SetExePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            var tag = (sender as Button).DataContext as TbHotKeyEntity;
            dialog.Filter = "exe file | *.exe";
            dialog.Title = "设置" + tag.name + "路径";
            dialog.InitialDirectory = tag.executablePath;
            if (dialog.ShowDialog() == true)
            {
                tag.executablePath = dialog.FileName;
                TbHotkey.updateHotkey(tag, true);
                RefreshData();
                mCallback();
            }
        }
    }
}
