using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using NLog;
using UIInfrastructure.Dialogs.Contracts;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowWithDialogs
{
    public partial class BaseApplication : Application, INotifyPropertyChanged
    {
        /// <summary>
        /// Defines the _consoleMode
        /// </summary>
        private bool consoleMode = false;

        private IWindowWithDialogs mainWindowWithDialogs;

        private static BaseApplication application;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private BindingErrorListener errorListener;

        private VisualTheme selectedVisualTheme;

        protected const int VISUAL_THEME_BASE_ID = 1001;

        /// <summary>
        /// Initializes static members of the <see cref="BaseApplication"/> class.
        /// </summary>
        static BaseApplication()
        {
            logger?.Trace("Запуск приложения");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            Dispatcher.CurrentDispatcher.UnhandledException += Dispatcher_UnhandledException;

            // LoadReferencedAssemblies();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMPApp"/> class.
        /// </summary>
        public BaseApplication()
        {
            application = this;

            logger?.Trace("Инициализация приложения");

            SetAppDomainCultures("ru-RU"); // "be-BY");

            this.Navigating += this.TMPApp_Navigating;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Overrides

        /// <summary>
        /// The OnStartup
        /// </summary>
        /// <param name="e">The <see cref="StartupEventArgs"/></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                this.consoleMode = true;
                logger?.Trace(string.Format("Приложению передано {0} аргументов", e.Args.Length));
                this.RunConsole(e.Args);
            }
            else
            {
                // base.OnStartup(e);
                this.RunApp();
                try
                {
                    this.Initialize();
                }
                catch (Exception ex)
                {
                    string message = "Не удалось проинициализировать приложение.\n" + GetExceptionDetails(ex) + "\nОбратитесь к разработчику.\nПрограмма аварийно завершается.";
                    logger?.Error(message);
                    if (Current.MainWindow == null)
                        MessageBox.Show(message, Instance.Title ?? "Ошибочка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    else
                        MessageBox.Show(Current.MainWindow, message, Instance.Title ?? "Ошибочка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    this.Shutdown(-1);
                    return;
                }

                try
                {
                    var mainWindow = this.MainWindow;

                    if (this.MainWindowWithDialogs == null)
                    {
                        string message = "MainWindow does not implement IWindowWithDialogs.\nПрограмма аварийно завершается.";
                        logger?.Error(message);
                        MessageBox.Show(message, Instance.Title ?? "Ошибочка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                        this.Shutdown(-1);
                        return;
                    }

                (this.MainWindowWithDialogs as Window).DataContext = this.MainViewModel;
                    (this.MainWindowWithDialogs as Window).Show();
                }
                catch (Exception ex)
                {
                    string message = GetExceptionDetails(ex) + "\nОбратитесь к разработчику.\nПрограмма аварийно завершается.";
                    logger?.Error(message);
                    try
                    {
                        MessageBox.Show(Current.MainWindow, message, Instance.Title ?? "Ошибочка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                    catch (ArgumentException)
                    {
                        MessageBox.Show(message, Instance.Title ?? "Ошибочка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }

                    this.Shutdown(-1);
                }
            }
        }

        /// <summary>
        /// The OnExit
        /// </summary>
        /// <param name="e">The <see cref="ExitEventArgs"/></param>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
#if DEBUG
            this.DetachBindingErrorListener();
#endif
            logger?.Trace("Завершение работы");
        }

        #endregion

        #region Properties

        public static ServiceInjector Services => ServiceInjector.Instance;

        public System.Configuration.ApplicationSettingsBase AppSettings { get; protected set; }

        /// <summary>
        /// Модель представления для окна
        /// </summary>
        public IMainViewModel MainViewModel { get; set; }

        /// <summary>
        /// Главное окно
        /// </summary>
        public IWindowWithDialogs MainWindowWithDialogs
        {
            get
            {
                if (this.mainWindowWithDialogs == null && Current.MainWindow is IWindowWithDialogs window)
                {
                    this.mainWindowWithDialogs = window;
                }

                return this.mainWindowWithDialogs;
            }

            set
            {
                this.mainWindowWithDialogs = value;
                Current.MainWindow = this.mainWindowWithDialogs as Window;
            }
        }

        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public virtual string Title { get; }

        /// <summary>
        /// Gets the WindowTitle
        /// </summary>
        public string WindowTitle => (this.MainWindow == null) ? "APP" : this.MainWindow.Title;

        #endregion

        #region Static properties

        public static BaseApplication Instance => application;

        /// <summary>
        /// Gets the AssemblyTitle
        /// </summary>
        public static string AssemblyTitle => Assembly.GetEntryAssembly().GetName().Name;

        public static string AssemblyDescription => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

        /// <summary>
        /// Gets the AssemblyEntryLocation
        /// </summary>
        public static string AssemblyEntryLocation => System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// Gets the AppDataFolder
        /// </summary>
        public static string AppDataFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                 System.IO.Path.DirectorySeparatorChar +
                                                 BaseApplication.Company;

        /// <summary>
        /// Gets the AppDataSettingFileName
        /// </summary>
        public static string AppDataSettingFileName => System.IO.Path.Combine(BaseApplication.AppDataFolder,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.settings", BaseApplication.AssemblyTitle));

        /// <summary>
        /// Gets the AppSessionFileName
        /// </summary>
        public static string AppSessionFileName => System.IO.Path.Combine(BaseApplication.AppDataFolder,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.session", BaseApplication.AssemblyTitle));

        /// <summary>
        /// Gets the MyDocumentsFolder
        /// </summary>
        public static string MyDocumentsFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static string Copyright => "© 2017-2022, Трус Михаил Петрович";

        public static string AppVersion
        {
            get
            {
                var v = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            }
        }

        /// <summary>
        /// Gets the Company
        /// </summary>
        public static string Company => "TMPApps";

        /// <summary>
        /// Gets the Company from assembly
        /// </summary>
        public static string CompanyFromAssembly
        {
            get
            {
                // Получение сборки приложения
                var assm = System.Reflection.Assembly.GetEntryAssembly();
                var at = typeof(System.Reflection.AssemblyCompanyAttribute);
                object[] customAttributes = null;
                try
                {
                    // Получение из метаданных коллекции аттрибутов
                    customAttributes = assm.GetCustomAttributes(at, false);
                }
                catch (Exception)
                {
                }

                // Получения из метаданных аттрибута компания
                System.Reflection.AssemblyCompanyAttribute ct =
                              (System.Reflection.AssemblyCompanyAttribute)customAttributes[0];
                return ct.Company;
            }
        }

        /// <summary>
        /// Путь к папке с данными программы
        /// </summary>
        public static string AppDataSettingsPath
        {
            get
            {
                // Получение сборки приложения
                var assm = System.Reflection.Assembly.GetEntryAssembly();
                var at = typeof(System.Reflection.AssemblyCompanyAttribute);
                object[] customAttributes = null;
                try
                {
                    // Получение из метаданных коллекции аттрибутов
                    customAttributes = assm.GetCustomAttributes(at, false);
                }
                catch (Exception)
                {
                }

                // Получения из метаданных аттрибута компания
                System.Reflection.AssemblyCompanyAttribute ct =
                              (System.Reflection.AssemblyCompanyAttribute)customAttributes[0];

                // получение пути к данным программы в папке пользователя
                if (ct != null)
                {
                    return System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        ct.Company,
                        assm.GetName().Name,
                        assm.GetName().Version.ToString());
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Путь к папке с исполняемым файлом программы
        /// </summary>
        public static string ExecutionPath => System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

        #endregion

        #region Public Static methods

        public static void DoEvents() // Реализация DoEvents в WPF
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
            }
        }

        public static void InvokeInUIThread(Action action)
        {
            if (Application.Current == null)
            {
                action();
                return;
            }

            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(action, DispatcherPriority.Background);
                return;
            }

            action();
        }

        /// <summary>
        /// The CorrectMainWindowSizeAndPos
        /// </summary>
        /// <param name="window">The <see cref="Window"/></param>
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

            if (window.Top + (window.Height / 2) > System.Windows.SystemParameters.VirtualScreenHeight)
            {
                window.Top = System.Windows.SystemParameters.VirtualScreenHeight - window.Height;
            }

            if (window.Left + (window.Width / 2) > System.Windows.SystemParameters.VirtualScreenWidth)
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

        /// <summary>
        /// The GetUserAppDataPath
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public static string GetUserAppDataPath()
        {
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
                          (AssemblyCompanyAttribute)r[0];

            // Build the User App Data Path
            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ct.Company,
                assm.GetName().Name,
                assm.GetName().Version.ToString());
        }

        /// <summary>
        /// The CreateAppDataFolder
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public static bool CreateAppDataFolder()
        {
            try
            {
                if (System.IO.Directory.Exists(BaseApplication.AppDataFolder) == false)
                {
                    System.IO.Directory.CreateDirectory(BaseApplication.AppDataFolder);
                }
            }
            catch (Exception exp)
            {
                logger?.Error(exp);
                return false;
            }

            return true;
        }

        /// <summary>
        /// The SetAppDomainCultures
        /// </summary>
        /// <param name="name">The <see cref="string"/></param>
        public static void SetAppDomainCultures(string name)
        {
            try
            {
                CultureInfo value = new CultureInfo(name);
                CultureInfo.DefaultThreadCurrentCulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;
                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }

            // If an exception occurs, we'll just fall back to the system default.
            catch (CultureNotFoundException e)
            {
                logger?.Error(e);
                return;
            }
            catch (ArgumentException e)
            {
                logger?.Error(e);
                return;
            }
        }

        /// <summary>
        /// The LoadThemeDictionary
        /// </summary>
        /// <param name="t">The <see cref="Type"/></param>
        /// <param name="themeName">The <see cref="string"/></param>
        /// <param name="colorScheme">The <see cref="string"/></param>
        /// <returns>The <see cref="ResourceDictionary"/></returns>
        public static ResourceDictionary LoadThemeDictionary(Type t, string themeName, string colorScheme)
        {
            Assembly controlAssembly = t.Assembly;
            AssemblyName themeAssemblyName = controlAssembly.GetName();

            object[] attrs = controlAssembly.GetCustomAttributes(typeof(ThemeInfoAttribute), false);
            if (attrs.Length > 0)
            {
                ThemeInfoAttribute ti = (ThemeInfoAttribute)attrs[0];

                if (ti.ThemeDictionaryLocation == ResourceDictionaryLocation.None)
                {
                    // There are no theme-specific resources.
                    return null;
                }

                if (ti.ThemeDictionaryLocation == ResourceDictionaryLocation.ExternalAssembly)
                {
                    themeAssemblyName.Name += "." + themeName;
                }
            }

            string relativePackUriForResources = "/" +
                themeAssemblyName.FullName +
                ";component/themes/" +
                themeName + "." +
                colorScheme + ".xaml";

            Uri resourceLocater = new System.Uri(relativePackUriForResources, System.UriKind.Relative);
            return Application.LoadComponent(resourceLocater) as ResourceDictionary;
        }

        #endregion

        #region Private methods

        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// The RunConsole
        /// </summary>
        /// <param name="args">The <see cref="string[]"/></param>
        protected void RunConsole(string[] args)
        {
            logger?.Trace("Отображение консоли");
            try
            {
                HConsoleHelper.InitConsoleHandles();
                int n = this.Title.Length;
                Console.WriteLine(string.Format("┌{0}┐", new string('─', 80 - 2)));
                Console.WriteLine(string.Format("│{0}{1}{0}│", new string(' ', (80 - 2 - n) / 2), this.Title));
                Console.WriteLine(string.Format("└{0}┘", new string('─', 80 - 2)));
                Console.WriteLine();
                Console.WriteLine("Console mode not implemented.\nPress <Enter>.");
                Console.ReadLine();
                logger?.Trace("Закрытие консоли");
                HConsoleHelper.ReleaseConsoleHandles();
            }
            catch (Exception e)
            {
                logger?.Error(e);

                Console.WriteLine();
                Console.WriteLine(new string('─', 80));
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine(GetExceptionDetails(e));
                Console.WriteLine();
                Console.WriteLine("Press <Enter>.");
                Console.ReadLine();

                Application.Current.Shutdown(-1);
            }

            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// The RunApp
        /// </summary>
        protected void RunApp()
        {
            logger?.Trace("Создание приложения WPF");
            try
            {
                //logger?.Trace("Отображение заставки");
                //SplashScreen splash = new SplashScreen(Assembly.GetAssembly(typeof(BaseApplication)), "SplashScreen.png");
                //splash.Show(false, false);

                //Task closeSplash = Task.Run(() =>
                //{
                //    Thread.Sleep(MINIMUM_SPLASH_TIME);
                //    logger?.Trace("Скрытие заставки");
                //    splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
                //});

                // подключение обработки ошибок привязки
#if DEBUG
                this.AttachBindingErrorListener();
#endif
            }
            catch (Exception ex)
            {
                string message = "Не удалось запустить приложение.\n" + GetExceptionDetails(ex) + "\nОбратитесь к разработчику.\nПрограмма аварийно завершается.";
                MessageBox.Show(Current.MainWindow, message, Instance.Title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                logger?.Error(ex, "Аварийное завершение!");
                this.Shutdown(-1);
            }
        }

        /// <summary>
        /// The TMPApp_Navigating
        /// </summary>
        /// <param name="sender">The <see cref="object"/></param>
        /// <param name="e">The <see cref="NavigatingCancelEventArgs"/></param>
        private void TMPApp_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (this.consoleMode)
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region Messages

        public Window CreateExternalWindow(
            bool showInTaskbar = false,
            bool showActivated = true,
            bool topmost = true,
            ResizeMode resizeMode = ResizeMode.NoResize,
            WindowStyle windowStyle = WindowStyle.None,
            WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterScreen,
            bool showTitleBar = false,

            bool showMinButton = false,
            bool showMaxButton = false,
            bool showCloseButton = false)
        {
            return new Window
            {
                ShowInTaskbar = showInTaskbar,
                ShowActivated = showActivated,
                Topmost = topmost,
                ResizeMode = resizeMode,
                WindowStyle = windowStyle,
                WindowStartupLocation = windowStartupLocation,
            };
        }

        /// <summary>
        /// Отобразить сообщение об ошибке при возникновении исключения
        /// </summary>
        /// <param name="exception">
        /// Исключение
        /// </param>
        public static void ShowError(object exception, Action? runAfter = null)
        {
            Exception e = (Exception)exception;
            if (e != null)
            {
                ShowError(exception: e, null, runAfter: runAfter);
            }
            else
            {
                ShowError(message: "Произошла неисправимая ошибка.\nПрограмма будет закрыта.", runAfter: runAfter);
            }
        }

        /// <summary>
        /// Отобразить сообщение об ошибке
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowError(string message, MessageBoxButton buttons = MessageBoxButton.OK, Action? runAfter = null)
        {
            Func<MessageBoxResult> func = () =>
            {
                System.Media.SystemSounds.Exclamation.Play();
                if (Current == null || Current.MainWindow == null)
                {
                    logger?.Error("ОШИБКА: " + message);
                    return MessageBox.Show(message, Instance?.Title, buttons, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    if (Current.MainWindow is IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        windowWithDialogs.ShowDialogError(message, Instance.Title, ok: runAfter);
                        return MessageBoxResult.OK;
                    }
                    else
                    {
                        logger?.Error("ОШИБКА: " + message);
                        return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            };
            if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() == false)
            {
                return (MessageBoxResult)DispatcherExtensions.InUi(func);
            }
            else
            {
                return func();
            }
        }

        /// <summary>
        /// Отобразить сообщение об ошибке при возникновении исключения указанного формата
        /// </summary>
        /// <param name="exception">Исключение</param>
        /// <param name="format">Формат сообщения</param>
        /// <returns></returns>
        public static MessageBoxResult ShowError(Exception exception, string format, MessageBoxButton buttons = MessageBoxButton.OK, Action? runAfter = null)
        {
            Func<MessageBoxResult> func = () =>
            {
                if (string.IsNullOrEmpty(format))
                {
                    format = "Произошла ошибка.\nОписание ошибки:\n{0}";
                }

                System.Media.SystemSounds.Exclamation.Play();
                string msg = string.Format(format, GetExceptionDetails(exception));

                if (Current == null || Current.MainWindow == null)
                {
                    logger?.Error(message: msg + "\nТрассировка:\n" + exception.StackTrace);
                    return MessageBox.Show(msg, Instance.Title, buttons, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    if (Current.MainWindow is IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        windowWithDialogs.ShowDialogError(msg, Instance.Title, ok: runAfter);
                        return MessageBoxResult.OK;
                    }
                    else
                    {
                        logger?.Error(msg + "\nТрассировка:\n" + exception.StackTrace);
                        return MessageBox.Show(Current.MainWindow, msg, Instance.Title, buttons, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            };

            if (Application.Current != null && Application.Current.Dispatcher.CheckAccess() == false)
            {
                return (MessageBoxResult)DispatcherExtensions.InUi(func);
            }
            else
            {
                return func();
            }
        }

        /// <summary>
        /// Отобразить предупреждающее сообщение
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowWarning(string message, MessageBoxButton buttons = MessageBoxButton.OK, Action? runAfter = null)
        {
            System.Media.SystemSounds.Hand.Play();

            Func<MessageBoxResult> func = () =>
            {
                if (Current == null || Current.MainWindow == null)
                {
                    logger?.Warn("ВНИМАНИЕ: " + message);
                    return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    if (Current.MainWindow is IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        windowWithDialogs.ShowDialogWarning(message, Instance.Title, ok: runAfter);
                        return MessageBoxResult.OK;
                    }
                    else
                    {
                        logger?.Warn("ВНИМАНИЕ: " + message);
                        return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }

        /// <summary>
        /// Отобразить информационное сообщение
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static MessageBoxResult ShowInfo(string message, Action onOk, MessageBoxButton buttons = MessageBoxButton.OK, Action? runAfter = null)
        {
            Func<MessageBoxResult> func = () =>
            {
                System.Media.SystemSounds.Asterisk.Play();

                if (Current == null || Current.MainWindow == null)
                {
                    logger?.Info("ИНФО: " + message);
                    return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
                else
                {
                    if (Current.MainWindow is IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        var dialog = windowWithDialogs.DialogInfo(message, Instance.Title, ok: runAfter);

                        dialog.Ok = onOk;
                        dialog.Show();

                        return MessageBoxResult.OK;
                    }
                    else
                    {
                        logger?.Info("ИНФО: " + message);
                        return MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            };
            return (MessageBoxResult)DispatcherExtensions.InUi(func);
        }

        /// <summary>
        /// Отобразить сообщение с вопросом
        /// <param name="message">Сообщение</param>
        /// <returns></returns>
        public static void ShowQuestion(string message, Action onYes, Action onNo, MessageBoxButton buttons = MessageBoxButton.YesNo)
        {
            Action func = () =>
            {
                System.Media.SystemSounds.Question.Play();
                if (Current == null || Current.MainWindow == null)
                {
                    var result = MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == MessageBoxResult.Yes)
                        onYes?.Invoke();
                    if (result == MessageBoxResult.No)
                        onNo?.Invoke();
                }
                else
                {
                    if (Current.MainWindow is IWindowWithDialogs windowWithDialogs && windowWithDialogs.DialogManager != null)
                    {
                        var dialog = windowWithDialogs.DialogQuestion(message, Instance.Title);

                        dialog.Yes = onYes;
                        dialog.No = onNo;

                        dialog.Show();
                    }
                    else
                    {
                        var result = MessageBox.Show(Current.MainWindow, message, Instance.Title, buttons, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == MessageBoxResult.Yes)
                            onYes?.Invoke();
                        if (result == MessageBoxResult.No)
                            onNo?.Invoke();
                    }
                }
            };
            DispatcherExtensions.InUi(func);
        }

        #endregion

        #region Logging and Error
        public static void ToDebug(Exception e)
        {
            System.Diagnostics.Debug.WriteLine(GetExceptionDetails(e));
        }

        /// <summary>
        /// Запуск отслеживания ошибок привязки
        /// </summary>
        public void AttachBindingErrorListener()
        {
            this.errorListener = new BindingErrorListener();
            this.errorListener.ErrorCatched += this.OnBindingErrorCatched;
        }

        /// <summary>
        /// Прекращение отслеживания ошибок привязки
        /// </summary>
        public void DetachBindingErrorListener()
        {
            if (this.IsAttached)
            {
                this.errorListener.ErrorCatched -= this.OnBindingErrorCatched;
                this.errorListener.Dispose();
                this.errorListener = null;
            }
        }

        [DebuggerStepThrough]
        private void OnBindingErrorCatched(string message)
        {
            logger?.Error(message);

            // throw new BindingException(message);
        }

        /// <summary>
        /// Включено ли отслеживание ошибок привязки
        /// </summary>
        public bool IsAttached => this.errorListener != null;

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowError(e.ExceptionObject);
        }

        private static void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            BaseApplication app = Application.Current as BaseApplication;

            string error = BaseApplication.GetExceptionDetails(e.Exception);
#if DEBUG
            System.Diagnostics.Debug.WriteLine(error);
#endif
            logger?.Error(e.Exception);
            if (e.Exception.GetType() == typeof(System.IO.FileNotFoundException))
            {
                ShowError("Не удалось найти файл.\n" + e.Exception.Message + "\nПопробуйте переустановить приложение.");
            }
            else if (e.Exception.InnerException != null)
            {
                ShowError(e.Exception.InnerException);
            }
            else
            {
                ShowError(error);
            }

            e.Handled = true;
        }

        public static string GetExceptionDetails(Exception exp)
        {
            string message = string.Empty;
            if (exp is AggregateException)
            {
                AggregateException ae = exp as AggregateException;
                foreach (var e in ae.InnerExceptions)
                {
                    message += Environment.NewLine + e.Message + Environment.NewLine;
                }

                ParseException(ae.InnerExceptions.LastOrDefault());
            }
            else
            {
                ParseException(exp);
            }

            message += "\n" + BuildStackTrace(exp);

            void ParseException(Exception e)
            {
                try
                {
                    // Write Message tree of inner exception into textual representation
                    message = e.Message;

                    Exception innerEx = e.InnerException;

                    for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                    {
                        string spaces = string.Empty;

                        for (int j = 0; j < i; j++)
                        {
                            spaces += "  ";
                        }

                        message += "\n" + spaces + "└─>" + innerEx.Message;
                    }
                }
                catch
                {
                }
            }

            return message;
        }

        public static string BuildStackTrace(Exception e)
        {
            var st = new StackTrace(e, true);
            var frames = st.GetFrames();
            var traceString = new StringBuilder();

            foreach (var frame in frames)
            {
                if (frame.GetFileLineNumber() < 1)
                    continue;

                traceString.Append("File: " + frame.GetFileName());
                traceString.Append(", Method:" + frame.GetMethod().Name);
                traceString.Append(", LineNumber: " + frame.GetFileLineNumber());
                traceString.Append("  -->  ");
            }

            return traceString.ToString();
        }

        #endregion

        #region Visual theme

        /*/// <summary>
        /// Событие, возникающее при изменении визуальной темы
        /// </summary>
        public event VisualThemeChangedEventHandler VisualThemeChanged;*/

        public List<VisualTheme> VisualThemesList
        { get; set; } = new List<VisualTheme>(new VisualTheme[6]
        {
            new VisualTheme() { ShortName = "Aero", FullName = "/PresentationFramework.Aero;component/themes/aero.normalcolor.xaml" },
            new VisualTheme() { ShortName = "Classic", FullName = "/PresentationFramework.classic;component/themes/classic.xaml" },
            new VisualTheme() { ShortName = "Luna normalcolor", FullName = "/PresentationFramework.Luna;component/themes/luna.normalcolor.xaml" },
            new VisualTheme() { ShortName = "Luna homestead", FullName = "/PresentationFramework.Luna;component/themes/luna.homestead.xaml" },
            new VisualTheme() { ShortName = "Luna metallic", FullName = "/PresentationFramework.Luna;component/themes/luna.metallic.xaml" },
            new VisualTheme() { ShortName = "Royale", FullName = "/PresentationFramework.Royale;component/themes/royale.normalcolor.xaml" },
        });

        /// <summary>
        /// Выбранная тема оформления
        /// </summary>
        /// <remarks>
        /// Если задана ссылка на настройки приложения <see cref="AppSettings"/>
        /// и в настройках есть параметр с названием "Theme",
        /// то выбранная тема будет сохраняться в этом параметре
        /// </remarks>
        public VisualTheme SelectedVisualTheme
        {
            get
            {
                if (this.selectedVisualTheme == null)
                {
                    try
                    {
                        this.selectedVisualTheme = this.AppSettings != null && this.AppSettings["Theme"] != null
                            ? (VisualTheme)this.AppSettings["Theme"]
                            : this.VisualThemesList[0];
                    }
                    catch
                    {
                        if (this.VisualThemesList?.Count > 0)
                        {
                            this.selectedVisualTheme = this.VisualThemesList[0];
                        }
                    }
                }

                return this.selectedVisualTheme;
            }

            set
            {
                this.selectedVisualTheme = value;
                this.OnPropertyChanged();
                this.LoadVisualTheme(this.selectedVisualTheme);
                if (this.AppSettings != null && this.AppSettings["Theme"] != null)
                {
                    if (this.AppSettings["Theme"] != value)
                    {
                        this.AppSettings["Theme"] = value;
                    }
                }
            }
        }

        public void LoadVisualTheme(VisualTheme theme)
        {
            if (theme == null)
                return;
            this.LoadVisualTheme(theme.FullName);
        }

        public void LoadVisualTheme(string themePath)
        {
            List<Uri>? dictionaries = null;
            if (Application.Current.Resources.MergedDictionaries != null)
            {
                dictionaries = Application.Current.Resources.MergedDictionaries.Where(d => d.Source?.OriginalString.StartsWith(@"/PresentationFramework") == false).Select(d => d.Source).ToList();

                // очищаем перед загрузкой темы
                this.Resources.MergedDictionaries.Clear();
            }

            // загружаем необходимые для работы ресурсы
            // загружаем тему
            try
            {
                Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themePath, UriKind.RelativeOrAbsolute) });
                if (dictionaries != null)
                {
                    dictionaries.ForEach((uri) => Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = uri }));
                }
            }
            catch (Exception)
            {
                this.MainWindowWithDialogs.ShowDialogError("Не удалось применить визуальную тему.");
            }
        }

        public void MakeMenu(Window mainWindow)
        {
            if (System.IO.Directory.Exists("Themes"))
            {
                System.IO.FileInfo[] localthemes = new System.IO.DirectoryInfo("Themes").GetFiles();
                foreach (var item in localthemes)
                {
                    this.VisualThemesList.Add(new VisualTheme { ShortName = item.Name, FullName = item.FullName });
                }
            }

            // Create a new submenu structure
            IntPtr hMenu = WindowsNative.SystemMenu.AddSysMenuSubMenu();
            if (hMenu != IntPtr.Zero)
            {
                // Build submenu items of hMenu
                uint index = 0;
                for (int i = 0; i < this.VisualThemesList.Count; i++)
                {
                    WindowsNative.SystemMenu.AddSysMenuSubItem(this.VisualThemesList[i].ShortName, index, VISUAL_THEME_BASE_ID + index, hMenu);
                    index++;
                }

                // Now add to main system menu (position 6)
                WindowsNative.SystemMenu.AddSysMenuItem("Визуальная тема", 0, 6, hMenu);
                WindowsNative.SystemMenu.AddSysMenuItem("-", 0, 7, IntPtr.Zero);
            }

            // Attach our WndProc handler to this Window
            System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle);
            source.AddHook(new System.Windows.Interop.HwndSourceHook(this.WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg == (int)WindowsNative.SystemMenu.WindowMessages.wmSysCommand)
            {
                int menuID = wParam.ToInt32();

                if (menuID <= (VISUAL_THEME_BASE_ID + this.VisualThemesList.Count()))
                {
                    if (menuID >= VISUAL_THEME_BASE_ID)
                    {
                        this.SelectedVisualTheme = this.VisualThemesList[menuID - VISUAL_THEME_BASE_ID];
                    }
                }
            }

            return IntPtr.Zero;
        }

        #endregion
    }
}
