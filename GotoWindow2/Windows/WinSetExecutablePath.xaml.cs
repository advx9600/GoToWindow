using GoToWindow.Api;
using GotoWindow2.DB;
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

namespace GotoWindow2.Windows
{
    /// <summary>
    /// WinSetExecutablePath.xaml 的交互逻辑
    /// </summary>
    public partial class WinSetExecutablePath : Window
    {
        private TbExecutablePathEntity mEntity;
        private Action mCallback;

        public WinSetExecutablePath()
        {
            InitializeComponent();
            refreshData();
        }

        private void refreshData()
        {
            dataGrid1.ItemsSource = TbExecutablePath.GetAll();
        }
        private void Button_Del_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).DataContext as TbExecutablePathEntity;
            TbExecutablePath.Del(tag);
            refreshData();
            mCallback();
        }
        private void Button_SetPath_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).DataContext as TbExecutablePathEntity;
            ShowDialogAndUpdate(tag, false);
        }

        private void ShowDialogAndUpdate(TbExecutablePathEntity entity, bool isCloseWin)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "exe file |*.exe";
            dialog.InitialDirectory = entity.path;
            dialog.Title = entity.name + "  设置路径";
            if (dialog.ShowDialog() == true)
            {
                entity.path = dialog.FileName;
                TbExecutablePath.UpdateOrAdd(entity);
                refreshData();
                mCallback();
                if (isCloseWin) Close();
            }
        }
        internal void Show(IWindowEntry windowEntry, Action onExePathWinUpdate)
        {
            mEntity = new TbExecutablePathEntity(windowEntry.ProcessName, windowEntry.ExecutablePath);
            mCallback = onExePathWinUpdate;
            Show();
            ShowDialogAndUpdate(mEntity, true);
        }
    }
}
