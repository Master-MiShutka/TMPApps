using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TMP.Work.DocxReportGenerator
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
                //base.OnStartup(e);

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            DocxReportGenerator.Properties.Settings.Default.Save();
            LogInfo("Попытка завершения работы");
        }
    }
}
