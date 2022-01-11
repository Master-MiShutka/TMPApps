using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace TMP.ARMTES
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            TextWriterTraceListener writer = new TextWriterTraceListener(System.Console.Out);
            //Debug.Listeners.Add(writer);

            string executingAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            TraceListener listener = new TextWriterTraceListener(executingAssemblyName + ".log", executingAssemblyName);
            Trace.Listeners.Add(listener);

            Trace.TraceInformation("Application Startup");

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

            //Views.StartWindow wnd = new Views.StartWindow();
            //wnd.Show();

            Views.SESIndicationsWindow wnd = new Views.SESIndicationsWindow();

            ViewModel.SESIndicationsViewModel mainVm = new ViewModel.SESIndicationsViewModel(wnd);

            wnd.DataContext = mainVm;

            wnd.Loaded += (_, __) => Configuration.Instance.LoadData(() => mainVm.Init());

            wnd.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string msg = GetExceptionDetails(e.Exception);

            Trace.TraceError(msg);

            if (MessageBox.Show(msg + "\nНажмите ОК для завершения работы.", "Необработанная ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel)
                e.Handled = true;
            else
                App.Current.Shutdown(-1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Trace.TraceInformation("Application Exit");
            Trace.Flush();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Trace.TraceInformation("Starting");
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

        public static void ToDebug(Exception e)
        {
            int identLevel = System.Diagnostics.Debug.IndentLevel;
            do
            {
                Trace.TraceError(e.Message);

                System.Diagnostics.Debug.WriteLine(e.Message);
                e = e.InnerException;
                System.Diagnostics.Debug.Indent();
            } while (e != null);
            System.Diagnostics.Debug.IndentLevel = identLevel;
        }

    }
}
