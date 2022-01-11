using System;
using System.Windows;
using System.Reflection;
using System.Collections.Generic;
using TMPApplication;

namespace TMP.Work.Emcos
{
    /// <summary>
    /// Interaction logic for EmcosSiteWrapperApp.xaml
    /// </summary>
    public partial class EmcosSiteWrapperApp : TMPApplication.TMPApp
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LogInfo("App OnStartup");
            try
            {
                base.OnStartup(e);
                
                System.Net.ServicePointManager.DefaultConnectionLimit = 100;
                System.Net.ServicePointManager.Expect100Continue = false;

                TMPApp.Services.AddService<EmcosSettings>(new EmcosSettings());

                LogInfo("Создание главного окна.");
                View.BalanceView mainWindow = new View.BalanceView();
                LogInfo("Отображение главного окна.");
                CorrectMainWindowSizeAndPos(mainWindow);
                LogInfo("Создание контроллера главного окна");
                var vm = new ViewModel.BalanceViewModel(mainWindow);
                mainWindow.DataContext = vm;

                mainWindow.Show();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Emcos.Properties.Settings.Default.Save();
            TMPApp.Services.GetService<EmcosSettings>().Save();
            base.OnExit(e);
            LogInfo("Попытка завершения работы");
        }
    }
}
