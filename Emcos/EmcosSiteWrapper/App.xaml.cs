using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TMP.Work.Emcos
{
    using ServiceLocator;
    using MsgBox;
    using TMP.Common.Logger;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : TMPApplication.TMPApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ToLogInfo("App OnStartup");
            try
            {
                base.OnStartup(e);
                
                System.Net.ServicePointManager.DefaultConnectionLimit = 100;
                System.Net.ServicePointManager.Expect100Continue = false;

                ServiceContainer.Instance.AddService<IMessageBoxService>(new MessageBoxService());
                var main = new View.BalansView();
                App.Log.Log("Отображение главного окна.", Category.Info, Priority.None);
                CorrectMainWindowSizeAndPos();
                main.Show();
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
            ToLogInfo("Попытка завершения работы");
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
        public static string Title { get; private set; }
        public static string WindowTitle
        {
            get { return (Current.MainWindow == null) ? "APP" : Current.MainWindow.Title; }
        }
    }
}
