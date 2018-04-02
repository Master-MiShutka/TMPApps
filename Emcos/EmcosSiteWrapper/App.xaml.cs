using System;
using System.Windows;
using System.Reflection;
using System.Collections.Generic;

namespace TMP.Work.Emcos
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : TMPApplication.TMPApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LogInfo("App OnStartup");
            try
            {
                base.OnStartup(e);
                
                System.Net.ServicePointManager.DefaultConnectionLimit = 100;
                System.Net.ServicePointManager.Expect100Continue = false;

                View.BalansView mainWindow = new View.BalansView();
                App.LogInfo("Отображение главного окна.");
                CorrectMainWindowSizeAndPos();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            TMP.Work.Emcos.Properties.Settings.Default.Save();
            base.OnExit(e);
            LogInfo("Попытка завершения работы");
        }

        private static void CorrectMainWindowSizeAndPos()
        {
            var settings = TMP.Work.Emcos.Properties.Settings.Default;

            if (settings.MainWindowHeight > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                settings.MainWindowHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            }

            if (settings.MainWindowWidth > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                settings.MainWindowWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            }
            if (settings.MainWindowTop + settings.MainWindowHeight / 2 > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                settings.MainWindowTop = System.Windows.SystemParameters.VirtualScreenHeight - settings.MainWindowHeight;
            }

            if (settings.MainWindowLeft + settings.MainWindowWidth / 2 > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                settings.MainWindowLeft = System.Windows.SystemParameters.VirtualScreenWidth - settings.MainWindowWidth;
            }

            if (settings.MainWindowTop < 0)
            {
                settings.MainWindowTop = 0;
            }

            if (settings.MainWindowLeft < 0)
            {
                settings.MainWindowLeft = 0;
            }

        }

    }
}
