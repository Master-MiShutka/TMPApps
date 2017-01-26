using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TMPApplication
{
    using TMP.Common.Logger;

    public class TMPApp : Application, IAppWithLogger
    {
        protected const int MINIMUM_SPLASH_TIME = 1500; // Miliseconds
        protected const int SPLASH_FADE_TIME = 500;     // Miliseconds

        #region Logging

        public static readonly string LOG_FILE_NAME =
            System.IO.Path.GetFileNameWithoutExtension(Application.ResourceAssembly.Location) + ".log";
        ILoggerFacade IAppWithLogger.Log { get { return TMPApp.Log; } }

        public static ILoggerFacade Log { get; } =
            new TextLogger(new StreamWriter(LOG_FILE_NAME, false, System.Text.Encoding.UTF8)
            { AutoFlush = true });

        public static void ToLogException(Exception e)
        {
            string message = GetExceptionDetails(e);
            Log.Log(message);
        }
        public static void ToLogInfo(string message)
        {
            Log.Log(message, Category.Info, Priority.Medium);
        }
        public static void ToLogError(string message)
        {
            Log.Log(message, Category.Exception, Priority.High);
        }
        #endregion
        #region Constructor

        static TMPApp()
        {
            try
            {
                System.Text.StringBuilder notLoadedLibraries = new System.Text.StringBuilder();

                AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
                {
                    try
                    {
                        string folderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string assemblyPath = System.IO.Path.Combine(folderPath, "Libs", new AssemblyName(args.Name).Name + ".dll");
                        string assemblyPath2 = System.IO.Path.Combine(folderPath, @"..\Libs", new AssemblyName(args.Name).Name + ".dll");
                        if (System.IO.File.Exists(assemblyPath) == false)
                        {
                            if (System.IO.File.Exists(assemblyPath2) == false)
                                return null;
                            else
                                assemblyPath = assemblyPath2;
                        }
                        Assembly assembly = Assembly.LoadFrom(assemblyPath);
                        return assembly;
                    }
                    catch (Exception e)
                    {
                        notLoadedLibraries.AppendFormat("{0}, ", args.Name);
                        ToLogException(e);
                    }
                    return null;
                };

                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

                string baseDirectory = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                string parentDirectory = System.IO.Directory.GetParent(baseDirectory).FullName;

                var referencedPaths = new List<string>();
                referencedPaths.AddRange(System.IO.Directory.EnumerateFiles(baseDirectory, @"*.dll"));
                if (System.IO.Directory.Exists(System.IO.Path.Combine(baseDirectory,"Libs")) == true)
                    referencedPaths.AddRange(System.IO.Directory.EnumerateFiles(baseDirectory, @"Libs\*.dll"));
                if (System.IO.Directory.Exists(System.IO.Path.Combine(parentDirectory, "Libs")) == true)
                    referencedPaths.AddRange(System.IO.Directory.EnumerateFiles(parentDirectory, @"Libs\*.dll"));

                var toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();
                if (toLoad.Count > 0)
                    TMPApp.ToLogInfo(String.Format("Необходимо найти и загрузить {0} библиотек: [{1}]", 
                        toLoad.Count,
                        String.Join(", ", toLoad.Select(i => System.IO.Path.GetFileName(i)).ToArray())));
                toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));

                if (notLoadedLibraries.Length > 0)
                {
                    notLoadedLibraries.Remove(notLoadedLibraries.Length - 2, 2);
                    System.Windows.MessageBox.Show(
                        String.Format("Программе не удалось найти и загрузить следующие библиотеки:\n{0}\n\nРабота программы невозможна и она будет закрыта.", notLoadedLibraries.ToString()),
                        "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    ToLogError("Аварийное завершение!");
                    Application.Current.Shutdown(-1);
                }

                SetAppDomainCultures("ru-RU");// "be-BY");
            }
            catch (Exception e)
            {
                ToLogError("Не удалось найти и загрузить библиотеку");
                ToLogException(e);
            }
        }

        public TMPApp()
        {
            TMPApp.Log.Log("Запуск приложения", Category.Info, Priority.None);

            AppDomain.CurrentDomain.UnhandledException += ShowErrorBox;
            Dispatcher.CurrentDispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            TMPApp.Log.Log("Отображение заставки", Category.Info, Priority.None);
            SplashScreen splash = new SplashScreen(Assembly.GetAssembly(typeof(TMPApp)), "SplashScreen.png");
            splash.Show(false, false);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            // подключение обработки ошибок привязки
#if DEBUG
            AttachBindingErrorListener();
#endif

            base.OnStartup(e);

            timer.Stop();
            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            TMPApp.Log.Log("Скрытие заставки", Category.Info, Priority.None);
            splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
            ;
        }
        protected override void OnExit(ExitEventArgs e)
        {
            TMPApp.Log.Log("Завершение работы", Category.Info, Priority.None);
            base.OnExit(e);
#if DEBUG
            DetachBindingErrorListener();
#endif
        }

        #region Exceptions
        BindingErrorListener errorListener;
        /// <summary>
        /// Запуск отслеживания ошибок привязки
        /// </summary>
        public void AttachBindingErrorListener()
        {
            errorListener = new BindingErrorListener();
            errorListener.ErrorCatched += OnBindingErrorCatched;
        }
        /// <summary>
        /// Прекращение отслеживания ошибок привязки
        /// </summary>
        public void DetachBindingErrorListener()
        {
            errorListener.ErrorCatched -= OnBindingErrorCatched;
            errorListener.Dispose();
            errorListener = null;
        }
        [DebuggerStepThrough]
        void OnBindingErrorCatched(string message)
        {
            ToLogError(message);
            //throw new BindingException(message);
        }
        /// <summary>
        /// Включено ли отслеживание ошибок привязки
        /// </summary>
        public bool IsAttached
        {
            get { return errorListener != null; }
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(TextLogger.GetExceptionDetails(e.Exception));
            ToLogException(e.Exception);
            e.Handled = true;
        }
        static void ShowErrorBox(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;            
            if (ex != null)
            {
                ToLogException(ex);
                string message = TextLogger.GetExceptionDetails(ex);
                Debug.WriteLine(message);
                System.Windows.MessageBox.Show(message, "Sorry, we crashed");
            }
            ToLogError("Аварийное завершение!");
            Application.Current.Shutdown(-1);
        }
        #endregion

        #region Properties

        public static string Title { get; private set; }
        public static string WindowTitle => (Current.MainWindow == null) ? "APP" : Current.MainWindow.Title;

        public static Dispatcher UIDispatcher { get; set; }

        #endregion

        #region Public Methods
        public static void CorrectMainWindowSizeAndPos(Window window)
        {
            if (window.Height > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                window.Height = System.Windows.SystemParameters.VirtualScreenHeight;
            }

            if (window.Width > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                window.Width = System.Windows.SystemParameters.VirtualScreenWidth;
            }
            if (window.Top + window.Height / 2 > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                window.Top = System.Windows.SystemParameters.VirtualScreenHeight - window.Height;
            }

            if (window.Left + window.Width / 2 > System.Windows.SystemParameters.VirtualScreenWidth)
            {
                window.Left = System.Windows.SystemParameters.VirtualScreenWidth - window.Width;
            }

            if (window.Top < 0)
            {
                window.Top = 0;
            }

            if (window.Left < 0)
            {
                window.Left = 0;
            }

        }
        public static string GetUserAppDataPath()
        {
            string path = string.Empty;
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
            AssemblyCompanyAttribute ct =
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
        public static void UIAction(Action action)
        {
            if (UIDispatcher == null)
                UIDispatcher = Dispatcher.CurrentDispatcher;
            if (UIDispatcher.CheckAccess())
                action();
            else
                UIDispatcher.BeginInvoke(((Action)(() => { action(); })));
        }
        public static MessageBoxResult ShowError(string message, string title = null)
        {
            return MessageBox.Show(message, title ?? Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static MessageBoxResult ShowWarning(string message, string title = null)
        {
            return MessageBox.Show(message, title ?? Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static MessageBoxResult ShowInfo(string message, string title = null)
        {
            return MessageBox.Show(message, title ?? Title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static MessageBoxResult ShowQuestion(string message, string title = null)
        {
            return MessageBox.Show(message, title ?? Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public static string GetExceptionDetails(Exception exp)
        {
            string message = string.Empty;
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

                    message += "\n" + spaces + "└─>" + innerEx.Message;
                }

                // Write complete stack trace info into details section
                //message += "\nStack trace:\n" + exp.ToString();
            }
            catch
            {
            }

            return message;

        }
        #endregion

        #region Private Methods
        protected static void SetAppDomainCultures(string name)
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
            catch (CultureNotFoundException)
            {
                return;
            }
            catch (ArgumentException)
            {
                return;
            }
        }
        #endregion
    }
}
