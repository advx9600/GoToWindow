using GoToWindow.Api;
using GotoWindow2.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
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
        private Mutex _mutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!WaitForOtherInstancesToShutDown())
            {
                MessageBox.Show(
                    "Another Go To Window instance is already running.",
                    "Go To Window",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                    );
                Current.Shutdown(1);
                return;
            }
            _hook = KeyboardHookAlt.Hook(KeyHookCallback);
            mMainWin = new MainWindow();
        }

        private void KeyHookCallback()
        {
            mMainWin.ShowFront();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_hook != null)
            {
                _hook.Dispose();
            }
            if (_mutex != null)
            {
                _mutex.Dispose();
                _mutex = null;
            }
        }

        private bool WaitForOtherInstancesToShutDown()
        {
            const int msBetweenAttempts = 500;
            for (var attempt = 0; attempt < 10; attempt++)
            {
                _mutex = new Mutex(true, "GoToWindow2", out bool isOnlyRunningProcessInstance);

                if (isOnlyRunningProcessInstance)
                    return true;

                Thread.Sleep(msBetweenAttempts);
            }

            return false;
        }
    }
}
