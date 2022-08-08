using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace TMP.WPFControls.Controls
{
    public class AppHeaderControl : Control
    {
        private static string _copyright = "© 2015, Ведущий инженер отдела сбыта\r\nэлектроэнергии Ошмянских ЭС\r\nТрус Михаил Петрович";
        private static string _noDescription = "нет описания";

        static AppHeaderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppHeaderControl), new FrameworkPropertyMetadata(typeof(AppHeaderControl)));
        }

        public AppHeaderControl()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            AppVersion = string.Format(CultureInfo.InvariantCulture, "Версия приложения: {0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);

            AppCopyright = _copyright;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        #region Dependency properties

        public static readonly DependencyProperty AppVersionProperty = DependencyProperty.Register("AppVersion", typeof(string), typeof(AppHeaderControl),
                                        new PropertyMetadata("0.0.0.0"));

        public static readonly DependencyProperty AppDescriptionProperty = DependencyProperty.Register("AppDescription", typeof(string), typeof(AppHeaderControl),
                                new PropertyMetadata(_noDescription));

        public static readonly DependencyProperty AppCopyrightProperty = DependencyProperty.Register("AppCopyright", typeof(string), typeof(AppHeaderControl),
                    new PropertyMetadata(_copyright));

        #endregion Dependency properties

        #region Properties

        public string AppVersion
        {
            get { return (string)GetValue(AppVersionProperty); }
            set { SetValue(AppVersionProperty, value); }
        }

        public string AppDescription
        {
            get { return (string)GetValue(AppDescriptionProperty); }
            set { SetValue(AppDescriptionProperty, value); }
        }

        public string AppCopyright
        {
            get { return (string)GetValue(AppCopyrightProperty); }
            set { SetValue(AppCopyrightProperty, value); }
        }

        #endregion Properties
    }
}