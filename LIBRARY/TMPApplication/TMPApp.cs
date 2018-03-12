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
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Navigation;
    using System.Windows.Threading;
    using TMP.Common.Logger;

    /// <summary>
    /// Defines the <see cref="TMPApp" />
    /// </summary>
    public partial class TMPApp : Application, IAppWithLogger
    {
        /// <summary>
        /// Defines the MINIMUM_SPLASH_TIME
        /// </summary>
        protected const int MINIMUM_SPLASH_TIME = 1500;// Miliseconds

        /// <summary>
        /// Defines the SPLASH_FADE_TIME
        /// </summary>
        protected const int SPLASH_FADE_TIME = 500;// Miliseconds

        /// <summary>
        /// Defines the _consoleMode
        /// </summary>
        private bool _consoleMode = false;

        /// <summary>
        /// Initializes static members of the <see cref="TMPApp"/> class.
        /// </summary>
        static TMPApp()
        {
            ServiceInjector.Instance.AddService<TMP.Common.Logger.ILoggerFacade>(Logger);

            Log("Запуск приложения");

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
            Log("Инициализация приложения");

            SetAppDomainCultures("ru-RU");// "be-BY");

            Navigating += TMPApp_Navigating;
        }

        /// <summary>
        /// The RunConsole
        /// </summary>
        /// <param name="args">The <see cref="string[]"/></param>
        private void RunConsole(string[] args)
        {
            Log("Отображение консоли");
            try
            {
                HConsoleHelper.InitConsoleHandles();
                int n = Title.Length;
                Console.WriteLine(String.Format("┌{0}┐", new String('─', 80 - 2)));
                Console.WriteLine(String.Format("│{0}{1}{0}│", new String(' ', (80 - 2 - n) / 2), Title));
                Console.WriteLine(String.Format("└{0}┘", new String('─', 80 - 2)));
                Console.WriteLine();
                Console.WriteLine("Console mode not implemented.\nPress <Enter>.");
                Console.ReadLine();
                Log("Закрытие консоли");
                HConsoleHelper.ReleaseConsoleHandles();
            }
            catch (Exception e)
            {
                LogException(e);

                Console.WriteLine();
                Console.WriteLine(new String('─', 80));
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
        private void RunApp()
        {
            Log("Создание приложения WPF");
            try
            {
                Log("Отображение заставки");
                SplashScreen splash = new SplashScreen(Assembly.GetAssembly(typeof(TMPApp)), "SplashScreen.png");
                splash.Show(false, false);

                Stopwatch timer = new Stopwatch();
                timer.Start();

                // подключение обработки ошибок привязки
#if DEBUG
                AttachBindingErrorListener();
#endif

                timer.Stop();
                int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
                if (remainingTimeToShowSplash > 0)
                    Thread.Sleep(remainingTimeToShowSplash);

                Log("Скрытие заставки");
                splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));

                /*var app = new TMPApp();
                app.Run();*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                LogException(ex);
                LogError("Аварийное завершение!");
                Application.Current.Shutdown(-1);
            }
        }

        /// <summary>
        /// The TMPApp_Navigating
        /// </summary>
        /// <param name="sender">The <see cref="object"/></param>
        /// <param name="e">The <see cref="NavigatingCancelEventArgs"/></param>
        private void TMPApp_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_consoleMode)
                e.Cancel = true;
        }

        /// <summary>
        /// The OnStartup
        /// </summary>
        /// <param name="e">The <see cref="StartupEventArgs"/></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                _consoleMode = true;
                Log(String.Format("Приложению передано {0} аргументов", e.Args.Length));
                RunConsole(e.Args);
            }
            else
            {
                base.OnStartup(e);
                if (StartupUri == null)
                {
                    LogError("Не задано главное окно!\nАварийное завершение!");
                    Application.Current.Shutdown(-1);
                }
                RunApp();
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
            DetachBindingErrorListener();
#endif
            Log("Завершение работы");
        }

        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public static string Title { get; set; }

        /// <summary>
        /// Gets the WindowTitle
        /// </summary>
        public static string WindowTitle => (Current.MainWindow == null) ? "APP" : Current.MainWindow.Title;

        /// <summary>
        /// Gets the AssemblyTitle
        /// </summary>
        public static string AssemblyTitle
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Name;
            }
        }

        /// <summary>
        /// Gets the AssemblyEntryLocation
        /// </summary>
        public static string AssemblyEntryLocation
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }

        /// <summary>
        /// Gets the AppDataFolder
        /// </summary>
        public static string AppDataFolder
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                 System.IO.Path.DirectorySeparatorChar +
                                                 TMPApp.Company;
            }
        }

        /// <summary>
        /// Gets the AppDataSettingFileName
        /// </summary>
        public static string AppDataSettingFileName
        {
            get
            {
                return System.IO.Path.Combine(TMPApp.AppDataFolder,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.settings", TMPApp.AssemblyTitle));
            }
        }

        /// <summary>
        /// Gets the AppSessionFileName
        /// </summary>
        public static string AppSessionFileName
        {
            get
            {
                return System.IO.Path.Combine(TMPApp.AppDataFolder,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.session", TMPApp.AssemblyTitle));
            }
        }

        /// <summary>
        /// Gets the MyDocumentsFolder
        /// </summary>
        public static string MyDocumentsFolder
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

        /// <summary>
        /// Gets the Company
        /// </summary>
        public static string Company
        {
            get
            {
                return "TMPApps";
            }
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

        /// <summary>
        /// The GetUserAppDataPath
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
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

        /// <summary>
        /// The CreateAppDataFolder
        /// </summary>
        /// <returns>The <see cref="bool"/></returns>
        public static bool CreateAppDataFolder()
        {
            try
            {
                if (System.IO.Directory.Exists(TMPApp.AppDataFolder) == false)
                    System.IO.Directory.CreateDirectory(TMPApp.AppDataFolder);
            }
            catch (Exception exp)
            {
                if (Logger != null)
                    Logger.LogException(exp);
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

                Thread.CurrentThread.CurrentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            // If an exception occurs, we'll just fall back to the system default.
            catch (CultureNotFoundException e)
            {
                Log(e);
                return;
            }
            catch (ArgumentException e)
            {
                Log(e);
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

        /// <summary>
        /// The CheckEnviroment
        /// </summary>
        private static void CheckEnviroment()
        {
            OperatingSystem OS = Environment.OSVersion;
            if (OS.Platform == PlatformID.Win32NT && OS.Version.Major == 5 && OS.Version.Minor <= 1)
            {
                MessageBox.Show("Вы используете устаревшую операционную систему Windows XP.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                LogWarning("Запуск на устаревшей системе!");
                //Application.Current.Shutdown();
                return;
            }

            // Проверка наличия установленного дотнет фреймворка версии 4
            Microsoft.Win32.RegistryKey installed_versions = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
            string[] version_names = installed_versions.GetSubKeyNames();
            //version names start with 'v', eg, 'v3.5' which needs to be trimmed off before conversion
            double Framework = Convert.ToDouble(version_names[version_names.Length - 1].Remove(0, 1), CultureInfo.InvariantCulture);
            int SP = Convert.ToInt32(installed_versions.OpenSubKey(version_names[version_names.Length - 1]).GetValue("SP", 0));
            if (Framework != 4.0)
            {
                MessageBox.Show("Не установлен .NET Framework версии 4.0.\nРабота программы невозможна и она будет закрыта.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                LogError("Аварийное завершение!");
                Application.Current.Shutdown(-1);
            }
        }

        // Checking the version using >= will enable forward compatibility, 
        // however you should always compile your code on newer versions of
        // the framework to ensure your app works the same.
        /// <summary>
        /// The CheckFor45DotVersion
        /// </summary>
        /// <param name="releaseKey">The <see cref="int"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393295)
            {
                return "4.6 or later";
            }
            if ((releaseKey >= 379893))
            {
                return "4.5.2 or later";
            }
            if ((releaseKey >= 378675))
            {
                return "4.5.1 or later";
            }
            if ((releaseKey >= 378389))
            {
                return "4.5 or later";
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }

        /// <summary>
        /// The Get45or451FromRegistry
        /// </summary>
        private static void Get45or451FromRegistry()
        {
            using (Microsoft.Win32.RegistryKey ndpKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    Console.WriteLine("Version: " + CheckFor45DotVersion((int)ndpKey.GetValue("Release")));
                }
                else
                {
                    Console.WriteLine("Version 4.5 or later is not detected.");
                }
            }
        }

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
            Assembly cachedAsm;
            if (LoadedAsmsCache.TryGetValue(args.Name, out cachedAsm))
                return cachedAsm;

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            string path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo != null && assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
            {
                path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
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
                    LogWarning("Не найдена сборка '" + args.Name + "'");
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
                        return null;
                    else
                        assemblyPath = assemblyPath2;
                }
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                return assembly;
            }
            catch (Exception e)
            {
                Log(e, "Ошибка загрузки библиотеки: {0}");
                LogError("Аварийное завершение!");
                Application.Current.Shutdown(-1);
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
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
                var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();
                var loadedNames = loadedAssemblies.Select(a => a.FullName.Substring(0, a.FullName.IndexOf(","))).ToArray();
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
                    foreach (var dll in System.IO.Directory.EnumerateFiles(baseDirectory, @"Libs\*.dll"))
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(dll);
                        if (librariesSet.Contains(name) == false)
                        {
                            librariesSet.Add(name);
                            toFindPaths.Add(dll);
                        }
                    }
                if (System.IO.Directory.Exists(System.IO.Path.Combine(parentDirectory, "Libs")) == true)
                    foreach (var dll in System.IO.Directory.EnumerateFiles(parentDirectory, @"Libs\*.dll"))
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(dll);
                        if (librariesSet.Contains(name) == false)
                        {
                            librariesSet.Add(name);
                            toFindPaths.Add(dll);
                        }
                    }
                // список сборок для загрузки
                // исключение системных
                toLoadReferenced = toLoadReferenced.Where(r => r.StartsWith("System.") == false && r.StartsWith("Windows.") == false).ToList();
                var toLoad = toFindPaths.Where(i => toLoadReferenced.Contains(System.IO.Path.GetFileNameWithoutExtension(i), StringComparer.InvariantCultureIgnoreCase)).ToList();
                // список не найденых сборок
                List<string> notFounded = new List<string>();
                var names = toLoad.Select(f => System.IO.Path.GetFileNameWithoutExtension(f)).ToList();
                foreach (var a in toLoadReferenced)
                    if (names.Contains(a, StringComparer.InvariantCultureIgnoreCase) == false)
                        notFounded.Add(a);
                if (notFounded.Count > 0)
                {
                    string message = String.Format("Программе не удалось найти и загрузить следующие библиотеки:\n{0}\nРабота программы невозможна и она будет закрыта.",
                        String.Join(", ", notFounded));
                    LogError("Аварийное завершение! " + message);
                    Application.Current.Shutdown(-1);
                }
                if (toLoad.Count > 0)
                    Log(String.Format("Загрузка {0} библиотек: [{1}]",
                        toLoad.Count,
                        String.Join(", ", toLoad.Select(i => System.IO.Path.GetFileName(i)).ToArray())));
                toLoad.ForEach(path =>
                {
                    try
                    {
                        AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
                        Assembly assembly = AppDomain.CurrentDomain.Load(assemblyName);
                        loadedAssemblies.Add(assembly);
                    }
                    catch (FileNotFoundException fnfe)
                    {
                        ShowError(String.Format("Не удалось найти библиотеку '{0}'.\nРабота программы невозможна и она будет закрыта.", fnfe.FileName));
                        LogError(fnfe.FusionLog);
                        LogError("Аварийное завершение!");
                        Application.Current.Shutdown(-1);
                    }
                    catch (System.Security.SecurityException se)
                    {
                        ShowError(String.Format("Нет разрешения на загрузку библиотеки '{0}'.\nРабота программы невозможна и она будет закрыта.", se.FailedAssemblyInfo.FullName));
                        LogException(se);
                        LogError("Аварийное завершение!");
                        Application.Current.Shutdown(-1);
                    }
                    catch (BadImageFormatException bife)
                    {
                        ShowError(String.Format("Библиотека '{0}' повреждена. Переустановите программу.\nРабота программы невозможна и она будет закрыта.", bife.FileName));
                        LogError(bife.FusionLog);
                        LogError("Аварийное завершение!");
                        Application.Current.Shutdown(-1);
                    }
                    catch (FileLoadException fle)
                    {
                        ShowError(String.Format("Не удалось загрузить библиотеку '{0}'.\nОписание ошибки: {1}\nРабота программы невозможна и она будет закрыта.", fle.FileName, fle.Message));
                        LogError(fle.FusionLog);
                        LogError("Аварийное завершение!");
                        Application.Current.Shutdown(-1);
                    }
                });
            }
            catch (Exception e)
            {
                ShowError(String.Format("При загрузке модулей произошла ошибка.\nОписание: {1}\nРабота программы невозможна и она будет закрыта.", e.Message));
                LogError("Аварийное завершение!");
                Application.Current.Shutdown(-1);
            }
        }
    }
}
