namespace TMPApplication.Themes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// The AppearanceManager class manages all WPF theme related items.
    /// </summary>
    public class AppearanceManager : IAppearanceManager
    {
        #region fields
        private IThemeInfo _defaultTheme = null;

        private IThemeInfo _currentTheme = null;
        private string _currentThemeName = null;
        #endregion fields

        #region constructors
        public AppearanceManager()
        {
            ThemeSources = new List<Uri>();
        }
        #endregion constructors

        #region properties

        public IThemeInfos Themes { get; set; }


        /// <summary>
        /// Возвращает имя текущей выбранной темы
        /// </summary>
        public string ThemeName
        {
            get
            {
                return _currentThemeName;
            }
        }

        /// <summary>
        /// Возращает список словарей-ресурсов текущей темы
        /// </summary>
        public List<Uri> ThemeSources
        {

            get;
            private set;
        }

        /// <summary>
        /// Возвращает акцентирующий цвет
        /// </summary>
        public Color AccentColor
        {
            get
            {
                try
                {
                    return (Color)Application.Current.Resources[ResourceKeys.ControlAccentColorKey];
                }
                catch (Exception)
                {
                    return Color.FromRgb(255, 255, 255);
                }
            }
        }
        #endregion properties

        #region events
        public event ColorChangedEventHandler AccentColorChanged;
        #endregion events

        #region methods
        /// <summary>
        /// Возвращает тему по умолчанию приложения
        /// </summary>
        /// <param name="Themes"></param>
        /// <returns></returns>
        public IThemeInfo GetDefaultTheme()
        {
            return _defaultTheme;
        }

        /// <summary>
        /// Задает новую тему
        /// </summary>
        public void SetTheme(IThemeInfo theme, Color AccentColor)
        {
            if (theme == null)
                theme = GetDefaultTheme();

            try
            {
                _currentTheme = theme;

                SetThemeSourceAndAccentColor(theme.ThemeSources, AccentColor);

                _currentThemeName = theme.DisplayName;
                ThemeSources = new List<Uri>(theme.ThemeSources);
            }
            catch
            { }
        }
        /// <summary>
        /// Очистка списка тем и устновка нового списка тем по умолчанию
        /// </summary>
        /// <param name="themes"></param>
        public void Reset(List<IThemeInfo> themes = null)
        {
            Themes.RemoveAllThemeInfos();
            if (themes == null)
            {
                Themes.AddThemeInfo("Dark", new List<Uri>
                {
                    new Uri("/TMPApplication;component/Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute)
                });

                Themes.AddThemeInfo("Light", new List<Uri>
                {
                    new Uri("/TMPApplication;component/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute)
                });
            }
            else
                foreach (var theme in themes) Themes.AddThemeInfo(theme);

            _defaultTheme = Themes.GetThemeInfo("Dark");
        }

        private ResourceDictionary GetThemeDictionary()
        {
            // determine the current theme by looking at the app resources and return the first dictionary having the resource key 'WindowBackground' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("WindowBackground")
                    select dict).FirstOrDefault();
        }

        /// <summary>
        /// Is invoked whenever the application theme is changed
        /// and a new Accent Color is applied.
        /// 
        /// TODO XXX: Set AccentColor in other components (MsgBox) as well.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="accentColor"></param>
        private void SetThemeSourceAndAccentColor(List<Uri> sources, Color accentColor)
        {
            if (sources == null)
                throw new ArgumentNullException("source");

            // TODO XXX This needs adjustment to remove everything that was previously added
            //          and replace everything that was alredy there with same name ???
            var oldThemeDict = GetThemeDictionary();
            var dictionaries = Application.Current.Resources.MergedDictionaries;

            foreach (var item in sources)
            {
                try
                {
                    var themeDict = new ResourceDictionary { Source = item };

                    // add new before removing old theme to avoid dynamicresource not found warnings
                    dictionaries.Add(themeDict);
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);
                    Console.WriteLine(exp.StackTrace);
                }
            }

            try
            {
                if (accentColor != null)
                {
                    bool bColorChanged = false;

                    bColorChanged = (Application.Current.Resources[ResourceKeys.ControlAccentColorKey] == null &&
                                    accentColor != null);

                    if (Application.Current.Resources[ResourceKeys.ControlAccentColorKey] != null)
                    {
                        bColorChanged  = bColorChanged ||
                            ((Color)Application.Current.Resources[ResourceKeys.ControlAccentColorKey] !=  accentColor);
                    }

                    if (bColorChanged == true)
                    {
                        // Set accent color
                        Application.Current.Resources[ResourceKeys.ControlAccentColorKey] = accentColor;
                        Application.Current.Resources[ResourceKeys.ControlAccentBrushKey] = new SolidColorBrush(accentColor);

                        if (AccentColorChanged != null)
                        {
                            AccentColorChanged(this, new ColorChangedEventArgs(accentColor));
                        }
                    }
                }
            }
            catch { }

            // remove old theme
            if (oldThemeDict != null)
                dictionaries.Remove(oldThemeDict);
        }

        public Color GetWindowColorizationColor()
        {
            Native.DWMCOLORIZATIONcolors dwmcolors = new Native.DWMCOLORIZATIONcolors();
            Native.NativeMethods.DwmGetColorizationParameters(ref dwmcolors);

            return Color.FromArgb((byte)(dwmcolors.ColorizationColor >> 24),
                (byte)(dwmcolors.ColorizationColor >> 16),
                (byte)(dwmcolors.ColorizationColor >> 8),
                (byte)dwmcolors.ColorizationColor);
        }

        public Brush GetWindowColorizationBrush()
        {
            return new SolidColorBrush(GetWindowColorizationColor());
        }

        #endregion methods
    }
}
