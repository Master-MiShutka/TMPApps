namespace TMP.Work.DocxReportGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : TMPApplication.TMPApp
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            this.logger?.Info("App OnStartup");
            try
            {
                // base.OnStartup(e);
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                this.logger?.Error(ex);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            DocxReportGenerator.Properties.Settings.Default.Save();
            this.logger?.Info("Попытка завершения работы");
        }
    }
}
