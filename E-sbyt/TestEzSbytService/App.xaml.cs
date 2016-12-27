using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Markup;
using System.Threading.Tasks;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace TMP.Work.AmperM.TestApp
{
  using TMP.Common.Logger;
  using ServiceLocator;
  using MsgBox;
  /// <summary>
  /// Логика взаимодействия для App.xaml
  /// </summary>
  public partial class App : TMPApplication.TMPApp
  {

    public static bool SetAllowUnsafeHeaderParsing()
    {
      //Get the assembly that contains the internal class
      Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
      if (aNetAssembly != null)
      {
        //Use the assembly in order to get the internal type for the internal class
        Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
        if (aSettingsType != null)
        {
          //Use the internal static property to get an instance of the internal settings class.
          //If the static instance isn't created allready the property will create it for us.
          object anInstance = aSettingsType.InvokeMember("Section",
            BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

          if (anInstance != null)
          {
            //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
            FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
            if (aUseUnsafeHeaderParsing != null)
            {
              aUseUnsafeHeaderParsing.SetValue(anInstance, true);
              return true;
            }
          }
        }
      }
      return false;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      ToLogInfo("App OnStartup");
      try
      {
        base.OnStartup(e);
        SetAllowUnsafeHeaderParsing();

        System.Net.ServicePointManager.DefaultConnectionLimit = 100;
        System.Net.ServicePointManager.Expect100Continue = false;

        ServiceContainer.Instance.AddService<IMessageBoxService>(new MessageBoxService());


        MainWindow mainWindow = new TestApp.MainWindow();
        CorrectMainWindowSizeAndPos();

        string path = null;
        ViewModel.MainWindowViewModel vm = new ViewModel.MainWindowViewModel(path);
        // после закрытия окна закрываем модель
        EventHandler handler = null;
        handler = delegate
        {
          vm.CloseCommand.Execute(null);
        };

        mainWindow.Closed += handler;

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
      base.OnExit(e);
      TMP.Work.AmperM.TestApp.Properties.Settings.Default.Save();
      ToLogInfo("Попытка завершения работы");
    }

    private void CorrectMainWindowSizeAndPos()
    {
      Properties.Settings settings = TestApp.Properties.Settings.Default;

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