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
    using TMP.Common.Logger;
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IAppWithLogger
    {
        public static TMP.Common.Logger.TextLogger LOG =
            new TextLogger(new System.IO.StreamWriter("log.txt", false, System.Text.Encoding.UTF8)
            { AutoFlush = true });

        public static readonly string LOG_FILE_NAME = System.IO.Path.GetFileNameWithoutExtension(Application.ResourceAssembly.Location) + ".log";

        ILoggerFacade IAppWithLogger.Log { get { return App.Log; } }

        public static ILoggerFacade Log { get; } =
            new TextLogger(new StreamWriter(LOG_FILE_NAME, false, System.Text.Encoding.UTF8)
            { AutoFlush = true });

        public App()
        {
            InitializeComponent();

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                AppDomain.CurrentDomain.UnhandledException += ShowErrorBox;
                Dispatcher.CurrentDispatcher.UnhandledException += Dispatcher_UnhandledException;
            }
            TaskScheduler.UnobservedTaskException += DotNet40_UnobservedTaskException;

            EventManager.RegisterClassHandler(typeof(Window),
                                              Hyperlink.RequestNavigateEvent,
                                              new RequestNavigateEventHandler(Window_RequestNavigate));
        }

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        //[System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public static void Main()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 100;
            System.Net.ServicePointManager.Expect100Continue = false;

            App.Log.Log("Запуск приложения", Category.Info, Priority.None);
            try
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

                var referencedPaths = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, @"Libs\*.dll");
                var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
                toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

                AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
                {
                    try
                    {
                        var folderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var assemblyPath = System.IO.Path.Combine(folderPath, "Libs", new AssemblyName(args.Name).Name + ".dll");
                        if (File.Exists(assemblyPath) == false) return null;
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        return assembly;
                    }
                    catch (Exception e)
                    {
                        App.Log.Log(e.Message, Category.Exception, Priority.High);
                    }
                    return null;
                };

                SetAppDomainCultures("ru-RU");// "be-BY");

                CorrectMainWindowSizeAndPos();
            }
            catch (Exception e)
            {
                LogException(e);
            }
            var app = new App();
            try
            {
                app.InitializeComponent();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            app.Run();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            App.Log.Log("Запуск", Category.Info, Priority.None);
            try
            {
                base.OnStartup(e);
                var main = new View.BalansView();
                App.Log.Log("Отображение главного окна.", Category.Info, Priority.None);
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
            App.Log.Log("Попытка завершения работы", Category.Info, Priority.None);
        }

        public static void LogException(Exception e)
        {
            var sb = new System.Text.StringBuilder();
            CreateExceptionString(sb, e, null);

            App.Log.Log(sb.ToString(), Category.Exception, Priority.High);
        }
        private static void CreateExceptionString(System.Text.StringBuilder sb, Exception e, string indent)
        {
            if (indent == null)
                indent = String.Empty;
            else
                if (indent.Length > 0)
                sb.AppendFormat("{0}Inner ", indent);
            sb.AppendFormat("Произошло исключение:\n{0}Тип: {1}", indent, e.GetType().FullName);
            sb.AppendFormat("\n{0}Сообщение: {1}", indent, e.Message);
            sb.AppendFormat("\n{0}Источник: {1}", indent, e.Source);
            sb.AppendFormat("\n{0}Трассировка стека: {1}", indent, e.StackTrace);

            if (e.InnerException != null)
            {
                sb.AppendLine();
                CreateExceptionString(sb, e.InnerException, indent + "  ");
            }
        }

        #region Exception Handling
        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(e.Exception.ToString());
            LogException(e.Exception);
            e.Handled = true;
        }
        static void ShowErrorBox(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                LogException(ex);
                Debug.WriteLine(ex.ToString());
                System.Windows.MessageBox.Show(ex.ToString(), "Sorry, we crashed");
            }
        }
        void DotNet40_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // On .NET 4.0, an unobserved exception in a task terminates the process unless we mark it as observed
            e.SetObserved();
        }
        #endregion
        private static void SetAppDomainCultures(string name)
        {
            try
            {
                var value = new CultureInfo(name);

                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            // If an exception occurs, we'll just fall back to the system default.
            catch (CultureNotFoundException)
            {
                return;
            }
            catch (ArgumentException)
            {
                return;
            }
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
        public static string GetUserAppDataPath()
        {
            var path = string.Empty;
            Assembly assm;
            Type at;
            object[] r;

            // Get the .EXE assembly
            assm = Assembly.GetEntryAssembly();
            // Get a 'Type' of the AssemblyCompanyAttribute
            at = typeof(AssemblyCompanyAttribute);
            // Get a collection of custom attributes from the .EXE assembly
            r = assm.GetCustomAttributes(at, false);
            // Get the Company Attribute
            var ct =
                          ((AssemblyCompanyAttribute)(r[0]));
            // Build the User App Data Path
            path = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ct.Company,
                assm.GetName().Name,
                assm.GetName().Version.ToString()
                );
            return path;
        }


        void Window_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri.Scheme == "resource")
            {
                //AvalonEditTextOutput output = new AvalonEditTextOutput();
                using (Stream s = typeof(App).Assembly.GetManifestResourceStream(typeof(App), e.Uri.AbsolutePath))
                {
                    using (StreamReader r = new StreamReader(s))
                    {
                        string line;
                        while ((line = r.ReadLine()) != null)
                        {
                            /*output.Write(line);
                            output.WriteLine();*/
                        }
                    }
                }
                //ILSpy.MainWindow.Instance.TextView.ShowText(output);
            }
        }
    }
}
