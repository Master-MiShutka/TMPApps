using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

using System.Diagnostics;

using System.Reflection;
using System.Windows;
using System.Globalization;

namespace TMP.ARMTES
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : TMPApplication.TMPApp
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {           
            ///////////////////////////////////////////////////
            Process ThisProcess = Process.GetCurrentProcess();
            Process[] SameProcesses = Process.GetProcessesByName(ThisProcess.ProcessName);
            if (SameProcesses.Length > 1) // уже запущена
            {
                MessageBox.Show("Другой экземпляр программы уже запущен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            TMP.ARMTES.Properties.Settings.Default.Save();
        }
    }
}