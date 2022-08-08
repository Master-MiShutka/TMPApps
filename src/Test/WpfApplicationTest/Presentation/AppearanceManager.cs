﻿namespace WpfApplicationTest.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using WpfApplicationTest.Windows.Helpers;

    /// <summary>
    /// Manages the theme, font size and accent colors for a Modern UI application.
    /// </summary>
    public class AppearanceManager
        : NotifyPropertyChanged
    {
        /// <summary>
        /// The location of the dark theme resource dictionary.
        /// </summary>
        public static readonly Uri DarkThemeSource = new Uri("/WpfApplicationTest;component/Themes/ui.dark.xaml", UriKind.Relative);

        /// <summary>
        /// The location of the light theme resource dictionary.
        /// </summary>
        public static readonly Uri LightThemeSource = new Uri("/WpfApplicationTest;component/Themes/ui.light.xaml", UriKind.Relative);

        /// <summary>
        /// The resource key for the accent color.
        /// </summary>
        public const string KeyAccentColor = "AccentColor";

        /// <summary>
        /// The resource key for the accent brush.
        /// </summary>
        public const string KeyAccent = "Accent";

        /// <summary>
        /// The resource key for the default font size.
        /// </summary>
        public const string KeyDefaultFontSize = "DefaultFontSize";

        /// <summary>
        /// The resource key for the fixed font size.
        /// </summary>
        public const string KeyFixedFontSize = "FixedFontSize";

        private static AppearanceManager current = new AppearanceManager();

        /// <summary>
        /// Initializes a new instance of the <see cref="AppearanceManager"/> class.
        /// </summary>
        private AppearanceManager()
        {
            this.DarkThemeCommand = new RelayCommand(o => this.ThemeSource = DarkThemeSource, o => !DarkThemeSource.Equals(this.ThemeSource));
            this.LightThemeCommand = new RelayCommand(o => this.ThemeSource = LightThemeSource, o => !LightThemeSource.Equals(this.ThemeSource));
            this.SetThemeCommand = new RelayCommand(o =>
            {
                var uri = NavigationHelper.ToUri(o);
                if (uri != null)
                {
                    this.ThemeSource = uri;
                }
            }, o => o is Uri || o is string);
            this.LargeFontSizeCommand = new RelayCommand(o => this.FontSize = FontSize.Large);
            this.SmallFontSizeCommand = new RelayCommand(o => this.FontSize = FontSize.Small);
            this.AccentColorCommand = new RelayCommand(o =>
            {
                if (o is Color)
                {
                    this.AccentColor = (Color)o;
                }
                else
                {
                    // parse color from string
                    var str = o as string;
                    if (str != null)
                    {
                        this.AccentColor = (Color)ColorConverter.ConvertFromString(str);
                    }
                }
            }, o => o is Color || o is string);
        }

        private ResourceDictionary GetThemeDictionary()
        {
            // determine the current theme by looking at the app resources and return the first dictionary having the resource key 'WindowBackground' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("WindowBackground")
                    select dict).FirstOrDefault();
        }

        private Uri GetThemeSource()
        {
            var dict = this.GetThemeDictionary();
            if (dict != null)
            {
                return dict.Source;
            }

            // could not determine the theme dictionary
            return null;
        }

        private void SetThemeSource(Uri source, bool useThemeAccentColor)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var oldThemeDict = this.GetThemeDictionary();
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var themeDict = new ResourceDictionary { Source = source };

            // if theme defines an accent color, use it
            var accentColor = themeDict[KeyAccentColor] as Color?;
            if (accentColor.HasValue)
            {
                // remove from the theme dictionary and apply globally if useThemeAccentColor is true
                themeDict.Remove(KeyAccentColor);

                if (useThemeAccentColor)
                {
                    this.ApplyAccentColor(accentColor.Value);
                }
            }

            // add new before removing old theme to avoid dynamicresource not found warnings
            dictionaries.Add(themeDict);

            // remove old theme
            if (oldThemeDict != null)
            {
                dictionaries.Remove(oldThemeDict);
            }

            this.OnPropertyChanged(nameof(this.ThemeSource));
        }

        private void ApplyAccentColor(Color accentColor)
        {
            // set accent color and brush resources
            Application.Current.Resources[KeyAccentColor] = accentColor;
            Application.Current.Resources[KeyAccent] = new SolidColorBrush(accentColor);
        }

        private FontSize GetFontSize()
        {
            var defaultFontSize = Application.Current.Resources[KeyDefaultFontSize] as double?;

            if (defaultFontSize.HasValue)
            {
                return defaultFontSize.Value == 12D ? FontSize.Small : FontSize.Large;
            }

            // default large
            return FontSize.Large;
        }

        private void SetFontSize(FontSize fontSize)
        {
            if (this.GetFontSize() == fontSize)
            {
                return;
            }

            Application.Current.Resources[KeyDefaultFontSize] = fontSize == FontSize.Small ? 12D : 13D;
            Application.Current.Resources[KeyFixedFontSize] = fontSize == FontSize.Small ? 10.667D : 13.333D;

            this.OnPropertyChanged(nameof(this.FontSize));
        }

        private Color GetAccentColor()
        {
            var accentColor = Application.Current.Resources[KeyAccentColor] as Color?;

            if (accentColor.HasValue)
            {
                return accentColor.Value;
            }

            // default color: teal
            return Color.FromArgb(0xff, 0x1b, 0xa1, 0xe2);
        }

        private void SetAccentColor(Color value)
        {
            this.ApplyAccentColor(value);

            // re-apply theme to ensure brushes referencing AccentColor are updated
            var themeSource = this.GetThemeSource();
            if (themeSource != null)
            {
                this.SetThemeSource(themeSource, false);
            }

            this.OnPropertyChanged(nameof(this.AccentColor));
        }

        /// <summary>
        /// Gets the current <see cref="AppearanceManager"/> instance.
        /// </summary>
        public static AppearanceManager Current => current;

        /// <summary>
        /// The command that sets the dark theme.
        /// </summary>
        public ICommand DarkThemeCommand { get; private set; }

        /// <summary>
        /// The command that sets the light color theme.
        /// </summary>
        public ICommand LightThemeCommand { get; private set; }

        /// <summary>
        /// The command that sets a custom theme.
        /// </summary>
        public ICommand SetThemeCommand { get; private set; }

        /// <summary>
        /// The command that sets the large font size.
        /// </summary>
        public ICommand LargeFontSizeCommand { get; private set; }

        /// <summary>
        /// The command that sets the small font size.
        /// </summary>
        public ICommand SmallFontSizeCommand { get; private set; }

        /// <summary>
        /// The command that sets the accent color.
        /// </summary>
        public ICommand AccentColorCommand { get; private set; }

        /// <summary>
        /// Gets or sets the current theme source.
        /// </summary>
        public Uri ThemeSource
        {
            get => this.GetThemeSource();
            set => this.SetThemeSource(value, true);
        }

        /// <summary>
        /// Gets or sets the font size.
        /// </summary>
        public FontSize FontSize
        {
            get => this.GetFontSize();
            set => this.SetFontSize(value);
        }

        /// <summary>
        /// Gets or sets the accent color.
        /// </summary>
        public Color AccentColor
        {
            get => this.GetAccentColor();
            set => this.SetAccentColor(value);
        }
    }
}
