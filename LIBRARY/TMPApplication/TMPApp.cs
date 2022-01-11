// **************************************************
//
// * TMPApp.cs
// * 06/03/2018
//
// **************************************************

namespace TMPApplication
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Navigation;
    using System.Windows.Threading;

    /// <summary>
    /// Defines the <see cref="TMPApp" />
    /// </summary>
    public partial class TMPApp : Application
    {
        #region Fields

        /// <summary>
        /// Defines the MINIMUM_SPLASH_TIME
        /// </summary>
        protected const int MINIMUM_SPLASH_TIME = 1500; // Miliseconds

        /// <summary>
        /// Defines the SPLASH_FADE_TIME
        /// </summary>
        protected const int SPLASH_FADE_TIME = 500; // Miliseconds

        /// <summary>
        /// Defines the _consoleMode
        /// </summary>
        private bool consoleMode = false;

        private WpfDialogs.Contracts.IWindowWithDialogs mainWindowWithDialogs;
        private static TMPApp tMPApp;

        private VisualTheme selectedVisualTheme;

        protected const int VISUAL_THEME_BASE_ID = 1001;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes static members of the <see cref="TMPApp"/> class.
        /// </summary>
        static TMPApp()
        {
            logger?.Trace("Запуск приложения");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            Dispatcher.CurrentDispatcher.UnhandledException += Dispatcher_UnhandledException;

            LoadReferencedAssemblies();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TMPApp"/> class.
        /// </summary>
        public TMPApp()
        {
            tMPApp = this;

            logger?.Trace("Инициализация приложения");

            SetAppDomainCultures("ru-RU"); // "be-BY");

            this.Navigating += this.TMPApp_Navigating;
        }

        #endregion

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
                        MessageBox.Show(Current.MainWindow,message, Instance.Title ?? "Ошибочка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    this.Shutdown(-1);
                    return;
                }

                try
                {
                    var mainWindow = this.MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.Loaded += (s, args) =>
                        {
                            this.MakeMenu(mainWindow);
                        };
                    }

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
        public WpfDialogs.Contracts.IWindowWithDialogs MainWindowWithDialogs
        {
            get
            {
                if (this.mainWindowWithDialogs == null && Current.MainWindow is WpfDialogs.Contracts.IWindowWithDialogs window)
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

        /*/// <summary>
        /// Событие, возникающее при изменении визуальной темы
        /// </summary>
        public event VisualThemeChangedEventHandler VisualThemeChanged;*/

        public List<VisualTheme> VisualThemesList = new List<VisualTheme>(new VisualTheme[6]
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
                        if (this.AppSettings != null && this.AppSettings["Theme"] != null)
                        {
                            this.selectedVisualTheme = (VisualTheme)this.AppSettings["Theme"];
                        }
                        else
                        {
                            this.selectedVisualTheme = this.VisualThemesList[0];
                        }
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

        #endregion

        #region Static properties

        public static TMPApp Instance => tMPApp;

        /// <summary>
        /// Gets the AssemblyTitle
        /// </summary>
        public static string AssemblyTitle => Assembly.GetEntryAssembly().GetName().Name;

        public static string AssemblyDescription => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        /// <summary>
        /// Gets the AssemblyEntryLocation
        /// </summary>
        public static string AssemblyEntryLocation => System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// Gets the AppDataFolder
        /// </summary>
        public static string AppDataFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                 System.IO.Path.DirectorySeparatorChar +
                                                 TMPApp.Company;

        /// <summary>
        /// Gets the AppDataSettingFileName
        /// </summary>
        public static string AppDataSettingFileName => System.IO.Path.Combine(TMPApp.AppDataFolder,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.settings", TMPApp.AssemblyTitle));

        /// <summary>
        /// Gets the AppSessionFileName
        /// </summary>
        public static string AppSessionFileName => System.IO.Path.Combine(TMPApp.AppDataFolder,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.session", TMPApp.AssemblyTitle));

        /// <summary>
        /// Gets the MyDocumentsFolder
        /// </summary>
        public static string MyDocumentsFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static string Copyright => "© 2017-2021, Трус Михаил Петрович";

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

        #region Public methods

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
                if (System.IO.Directory.Exists(TMPApp.AppDataFolder) == false)
                {
                    System.IO.Directory.CreateDirectory(TMPApp.AppDataFolder);
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

        #region Private static methods

        /// <summary>
        /// Defines the LoadedAsmsCache
        /// </summary>
        private static readonly Dictionary<string, Assembly> LoadedAsmsCache = new Dictionary<string, Assembly>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The OnResolveAssembly
        /// </summary>
        /// <param name="sender">The <see cref="object"/></param>
        /// <param name="args">The <see cref="ResolveEventArgs"/></param>
        /// <returns>The <see cref="Assembly"/></returns>
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (LoadedAsmsCache.TryGetValue(args.Name, out Assembly cachedAsm))
            {
                return cachedAsm;
            }

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo != null && assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = string.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                Assembly loadedAsm = null;
                if (stream == null)
                {
                    loadedAsm = FindAssembly(args.Name);
                }
                else
                {
                    byte[] assemblyRawBytes = new byte[stream.Length];
                    stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                    loadedAsm = Assembly.Load(assemblyRawBytes);
                }

                if (loadedAsm == null)
                {
                    logger?.Warn("Не найдена сборка '" + args.Name + "'");
                    return null;
                }

                LoadedAsmsCache.Add(args.Name, loadedAsm);
                return loadedAsm;
            }
        }

        /// <summary>
        /// The FindAssembly
        /// </summary>
        /// <param name="name">The <see cref="string"/></param>
        /// <returns>The <see cref="Assembly"/></returns>
        private static Assembly FindAssembly(string name)
        {
            try
            {
                string folderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string assemblyPath = System.IO.Path.Combine(folderPath, "Libs", new AssemblyName(name).Name + ".dll");
                string assemblyPath2 = System.IO.Path.Combine(folderPath, @"..\Libs", new AssemblyName(name).Name + ".dll");
                if (System.IO.File.Exists(assemblyPath) == false)
                {
                    if (System.IO.File.Exists(assemblyPath2) == false)
                    {
                        return null;
                    }
                    else
                    {
                        assemblyPath = assemblyPath2;
                    }
                }

                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }
            catch (Exception e)
            {
                logger?.Fatal(e, "Ошибка загрузки библиотеки: {0}");
                logger?.Error("Аварийное завершение!");

                // Application.Current.Shutdown(-1);
            }

            return null;
        }

        /// <summary>
        /// The LoadReferencedAssemblies
        /// </summary>
        private static void LoadReferencedAssemblies()
        {
            try
            {
                // список загруженных сборок
                var notDynamicLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.IsDynamic == false).ToList();
                var loadedPaths = notDynamicLoadedAssemblies.Select(a => a.Location).ToArray();
                var loadedNames = notDynamicLoadedAssemblies.Select(a => a.FullName.Substring(0, a.FullName.IndexOf(","))).ToArray();

                // список зависимых сборок
                var referencedAssemblies = Assembly.GetExecutingAssembly().GetReferencedAssemblies().ToList();
                var referencedPaths = referencedAssemblies.Select(a => a.Name).ToArray();

                // список сборок для загрузки, за вычетом уже загруженных
                var toLoadReferenced = referencedPaths.Where(r => !loadedNames.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

                string baseDirectory = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
                string parentDirectory = System.IO.Directory.GetParent(baseDirectory).FullName;

                // поиск сборок в заданных папках
                var toFindPaths = new List<string>();
                HashSet<string> librariesSet = new HashSet<string>();
                toFindPaths.AddRange(System.IO.Directory.EnumerateFiles(baseDirectory, @"*.dll"));
                toFindPaths.ForEach(i => librariesSet.Add(System.IO.Path.GetFileNameWithoutExtension(i)));
                if (System.IO.Directory.Exists(System.IO.Path.Combine(baseDirectory, "Libs")) == true)
                {
                    foreach (var dll in System.IO.Directory.EnumerateFiles(baseDirectory, @"Libs\*.dll"))
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(dll);
                        if (librariesSet.Contains(name) == false)
                        {
                            librariesSet.Add(name);
                            toFindPaths.Add(dll);
                        }
                    }
                }

                if (System.IO.Directory.Exists(System.IO.Path.Combine(parentDirectory, "Libs")) == true)
                {
                    foreach (var dll in System.IO.Directory.EnumerateFiles(parentDirectory, @"Libs\*.dll"))
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(dll);
                        if (librariesSet.Contains(name) == false)
                        {
                            librariesSet.Add(name);
                            toFindPaths.Add(dll);
                        }
                    }
                }

                // список сборок для загрузки
                // исключение системных
                toLoadReferenced = toLoadReferenced.Where(r => r.StartsWith("System.") == false && r.StartsWith("Windows.") == false && r.Equals("PresentationCore") == false).ToList();
                var toLoad = toFindPaths.Where(i => toLoadReferenced.Contains(System.IO.Path.GetFileNameWithoutExtension(i), StringComparer.InvariantCultureIgnoreCase)).ToList();

                // список не найденых сборок
                List<string> notFounded = new List<string>();
                var names = toLoad.Select(f => System.IO.Path.GetFileNameWithoutExtension(f)).ToList();
                foreach (var a in toLoadReferenced)
                {
                    if (names.Contains(a, StringComparer.InvariantCultureIgnoreCase) == false)
                    {
                        notFounded.Add(a);
                    }
                }

                if (notFounded.Count > 0)
                {
                    string message = string.Format("Программе не удалось найти и загрузить следующие библиотеки:\n{0}\nРабота программы невозможна и она будет закрыта.",
                        string.Join(", ", notFounded));
                    logger?.Error("Аварийное завершение! " + message);

                    ShowError(message);

                    Application.Current?.Shutdown(-1);
                    return;
                }

                if (toLoad.Count > 0)
                {
                    logger?.Trace(string.Format("Загрузка {0} библиотек: [{1}]",
                        toLoad.Count,
                        string.Join(", ", toLoad.Select(i => System.IO.Path.GetFileName(i)).ToArray())));
                }

                toLoad.ForEach(path =>
                {
                    try
                    {
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
                        Assembly assembly = AppDomain.CurrentDomain.Load(assemblyName);
                        if (assembly.IsDynamic == false)
                        {
                            notDynamicLoadedAssemblies.Add(assembly);
                        }
                    }
                    catch (FileNotFoundException fnfe)
                    {
                        ShowError(string.Format("Не удалось найти библиотеку '{0}'.\nРабота программы невозможна и она будет закрыта.", fnfe.FileName));
                        logger?.Error(fnfe.FusionLog);
                        logger?.Error("Аварийное завершение!");
                        Application.Current.Shutdown(-1);
                    }
                    catch (System.Security.SecurityException se)
                    {
                        ShowError(string.Format("Нет разрешения на загрузку библиотеки '{0}'.\nРабота программы невозможна и она будет закрыта.", se.FailedAssemblyInfo.FullName));
                        logger?.Error(se);
                        logger?.Error("Аварийное завершение!");
                        Application.Current.Shutdown(-1);
                    }
                    catch (BadImageFormatException bife)
                    {
                        ShowError(string.Format("Библиотека '{0}' повреждена. Переустановите программу.\nРабота программы невозможна и она будет закрыта.", bife.FileName));
                        logger?.Error(bife.FusionLog);
                        logger?.Error("Аварийное завершение!");
                        Application.Current.Shutdown(-1);
                    }
                    catch (FileLoadException fle)
                    {
                        ShowError(string.Format("Не удалось загрузить библиотеку '{0}'.\nОписание ошибки: {1}\nРабота программы невозможна и она будет закрыта.", fle.FileName, fle.Message));
                        logger?.Error(fle.FusionLog);
                        logger?.Error("Аварийное завершение!");
                        Application.Current.Shutdown(-1);
                    }
                });
            }
            catch (Exception e)
            {
                ShowError(string.Format("При загрузке модулей произошла ошибка.\nОписание: {0}\nРабота программы невозможна и она будет закрыта.", e.Message));
                logger?.Error("Аварийное завершение!");
                Application.Current.Shutdown(-1);
            }
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
                logger?.Trace("Отображение заставки");
                SplashScreen splash = new SplashScreen(Assembly.GetAssembly(typeof(TMPApp)), "SplashScreen.png");
                splash.Show(false, false);

                Task closeSplash = Task.Run(() =>
                {
                    Thread.Sleep(MINIMUM_SPLASH_TIME);
                    logger?.Trace("Скрытие заставки");
                    splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));
                });

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

        #region Themes support

        public void LoadVisualTheme(VisualTheme theme)
        {
            this.LoadVisualTheme(theme.FullName);
        }

        public void LoadVisualTheme(string themePath)
        {
            List<Uri> dictionaries = null;
            if (Application.Current.Resources.MergedDictionaries != null)
            {
                dictionaries = Application.Current.Resources.MergedDictionaries.Where(d => d.Source.OriginalString.StartsWith(@"/PresentationFramework") == false).Select(d => d.Source).ToList();

                // очищаем перед загрузкой темы
                this.Resources.MergedDictionaries.Clear();
            }

            // загружаем необходимые для работы ресурсы
            // загружаем тему
            try
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themePath, UriKind.RelativeOrAbsolute) });
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
            if (Directory.Exists("Themes"))
            {
                FileInfo[] localthemes = new DirectoryInfo("Themes").GetFiles();
                foreach (var item in localthemes)
                {
                    this.VisualThemesList.Add(new VisualTheme { ShortName = item.Name, FullName = item.FullName });
                }
            }

            // Create a new submenu structure
            IntPtr hMenu = TMP.Shared.SystemMenu.AddSysMenuSubMenu();
            if (hMenu != IntPtr.Zero)
            {
                // Build submenu items of hMenu
                uint index = 0;
                for (int i = 0; i < this.VisualThemesList.Count; i++)
                {
                    TMP.Shared.SystemMenu.AddSysMenuSubItem(this.VisualThemesList[i].ShortName, index, VISUAL_THEME_BASE_ID + index, hMenu);
                    index++;
                }

                // Now add to main system menu (position 6)
                TMP.Shared.SystemMenu.AddSysMenuItem("Визуальная тема", 0, 6, hMenu);
                TMP.Shared.SystemMenu.AddSysMenuItem("-", 0, 7, IntPtr.Zero);
            }

            // Attach our WndProc handler to this Window
            System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle);
            source.AddHook(new System.Windows.Interop.HwndSourceHook(this.WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg == (int)TMP.Shared.SystemMenu.WindowMessages.wmSysCommand)
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

        #endregion Themes support
    }

    [DataContract]
    public class VisualTheme
    {
        [DataMember]
        public string ShortName { get; set; }

        [DataMember]
        public string FullName { get; set; }
    }

    /*public class VisualThemeChangedArgs : EventArgs
    {
        public VisualThemeChangedArgs(VisualTheme visualTheme)
        {
            NewVisualTheme = visualTheme;
        }
        public VisualTheme NewVisualTheme { get; }
    }

    public delegate void VisualThemeChangedEventHandler(VisualThemeChangedArgs args);*/
}
