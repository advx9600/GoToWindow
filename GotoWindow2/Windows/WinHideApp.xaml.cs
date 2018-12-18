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
    /// WinHideApp.xaml 的交互逻辑
    /// </summary>
    public partial class WinHideApp : Window
    {
        public List<TbHideWinEntry> mList;


        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/da5f36df-91e1-4a1a-9265-25c9a2b56414/binding-list-of-lists-to-datagrid?forum=wpf
        // https://stackoverflow.com/questions/3046003/adding-a-button-to-a-wpf-datagrid
        public WinHideApp()
        {
            InitializeComponent();
            refreshGrid();
        }

        private void refreshGrid()
        {
            mList = TbHideWin.GetAll();
            dataGrid1.ItemsSource = mList;
        }
        internal void Show(IWindowEntry win, Action onHideWinUpdate)
        {
            Show();
            if (MessageBox.Show(String.Format("确定隐藏 \n程序名:{0}\n标题:{1} \n    窗口吗？", win.ProcessName, win.Title), "隐藏窗口", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                TbHideWin.Add(win);
                onHideWinUpdate();
                Close();
            }
        }

        private void Button_Del_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button).DataContext as TbHideWinEntry;
            TbHideWin.Del(tag);
            refreshGrid();
        }
    }
}
