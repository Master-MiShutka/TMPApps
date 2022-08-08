using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace TMP.ARMTES.Editor
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : TMPApplication.TMPApp
    {
        /// <summary>
        /// Создаёт экземпляр приложения
        /// </summary>
        App()
        {
            ToLogInfo("APP - Запуск редактора ARMTES");
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            ToLogInfo("APP - Shutting down");
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ToLogException(e.Exception);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ToLogInfo("APP - Started up");
        }
    }
}
