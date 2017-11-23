using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace TMP.WORK.AramisChetchiki
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ViewModel.IMainViewModel MainViewModel { get; private set; }

        public static void ToDebug(Exception e)
        {
            int identLevel = System.Diagnostics.Debug.IndentLevel;
            do
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                e = e.InnerException;
                System.Diagnostics.Debug.Indent();
            } while (e != null);
            System.Diagnostics.Debug.IndentLevel = identLevel;
        }

        public static string GetExceptionDetails(Exception exp)
        {
            string message = string.Empty;
            if (exp is AggregateException)
            {
                AggregateException ae = exp as AggregateException;
                foreach (var e in ae.InnerExceptions)
                    message += Environment.NewLine + e.Message + Environment.NewLine;
            }
            else
            try
            {
                // Write Message tree of inner exception into textual representation
                message = exp.Message;

                Exception innerEx = exp.InnerException;

                for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                {
                    string spaces = string.Empty;

                    for (int j = 0; j < i; j++)
                        spaces += "  ";

                    message += Environment.NewLine + spaces + "└─>" + innerEx.Message;
                }
            }
            catch { }
            return message;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement), 
                new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

            ThemeHelper.SetTheme("luna", "normalcolor");

            MainWindow window = new MainWindow();
            window.DataContext = MainViewModel = new ViewModel.MainViewModel();
            window.Show();
        }

        public static void DoEvents()//Реализация DoEvents в WPF
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        /// <summary>
        /// Sets the WPF system theme.
        /// </summary>
        /// <param name="themeName">The name of the theme. (ie "aero")</param>
        /// <param name="themeColor">The name of the color. (ie "normalcolor")</param>
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

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (MessageBox.Show(App.GetExceptionDetails(e.Exception) + "\nНажмите ОК для завершения работы.", "Необработанная ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel)
                e.Handled = true;
            else
                App.Current.Shutdown(-1);
        }
    }
}
