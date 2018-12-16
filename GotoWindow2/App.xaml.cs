using GoToWindow.Api;
using GotoWindow2.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GotoWindow2
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private KeyboardHookAlt _hook;
        MainWindow mMainWin;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _hook = KeyboardHookAlt.Hook(KeyHookCallback);
            mMainWin = new MainWindow();
        }

        private void KeyHookCallback()
        {
            mMainWin.ShowFront();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _hook.Dispose();
        }
    }
}
