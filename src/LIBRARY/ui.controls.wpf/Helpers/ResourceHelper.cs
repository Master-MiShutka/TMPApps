namespace TMP.UI.WPF.Controls.Helpers
{
    using System;
    using System.Reflection;
    using System.Windows;

    public class ResourceHelper
    {
        private static ResourceDictionary theme;

        public static T GetResource<T>(string key)
        {
            if (Application.Current.TryFindResource(key) is T resource)
            {
                return resource;
            }

            return default;
        }

        internal static T GetResourceInternal<T>(string key)
        {
            if (GetTheme()[key] is T resource)
            {
                return resource;
            }

            return default;
        }

        public static ResourceDictionary GetTheme() => theme ??= GetStandaloneTheme();

        public static ResourceDictionary GetStandaloneTheme()
        {
            return new()
            {
                Source = new Uri("pack://application:,,,/ui.wpf;component/Themes/generic.xaml"),
            };
        }
    }
}
