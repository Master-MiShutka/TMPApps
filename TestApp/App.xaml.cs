using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TestApp
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SetAppDomainCultures("ru-RU");// "be-BY");
        }

        static App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcher_UnhandledException;           
        }
        private static void CurrentDispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ;
        }

        public static void SetAppDomainCultures(string name)
        {
            try
            {
                CultureInfo value = new CultureInfo(name);

                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            // If an exception occurs, we'll just fall back to the system default.
            catch (CultureNotFoundException e)
            {
                return;
            }
            catch (ArgumentException e)
            {
                return;
            }
        }

        public static void SetTheme(string themeName, string themeColor)
        {
            const BindingFlags staticNonPublic = BindingFlags.Static | BindingFlags.NonPublic;

            var presentationFrameworkAsm = Assembly.GetAssembly(typeof(Window));

            var themeWrapper = presentationFrameworkAsm.GetType("MS.Win32.UxThemeWrapper");

            var isActiveField = themeWrapper.GetField("_isActive", staticNonPublic);
            var themeColorField = themeWrapper.GetField("_themeColor", staticNonPublic);
            var themeNameField = themeWrapper.GetField("_themeName", staticNonPublic);

            // Set this to true so WPF doesn't default to classic.
            isActiveField.SetValue(null, true);

            themeColorField.SetValue(null, themeColor);
            themeNameField.SetValue(null, themeName);
        }
    }
}
