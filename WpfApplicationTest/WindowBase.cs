using System;
using System.Windows;

using TMP.Shared.Commands;

namespace WpfApplicationTest
{
    public class WindowBase : Window
    {
        public WindowBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
        }

        //Relay Commands
        public RelayCommand MinimizeWindowCommand
        {
            get
            {
                return new RelayCommand(param => OnMinimizeWindow());
            }
        }
        public RelayCommand MaximizeWindowCommand
        {
            get
            {
                return new RelayCommand(param => OnMaximizeWindow());
            }
        }
        public RelayCommand RestoreWindowCommand
        {
            get
            {
                return new RelayCommand(param => OnRestoreWindow());
            }
        }
        public RelayCommand CloseWindowCommand
        {
            get
            {
                return new RelayCommand(param => OnCloseWindow());
            }
        }
        public RelayCommand AboutWindowCommand
        {
            get
            {
                return new RelayCommand(param => OnAbout());
            }
        }
        public RelayCommand PreferencesWindowCommand
        {
            get
            {
                return new RelayCommand(param => OnPreferences());
            }
        }

        private void OnMinimizeWindow()
        {
            SystemCommands.MinimizeWindow(this);
        }
        private void OnMaximizeWindow()
        {
            if ((WindowState == WindowState.Normal))
            {
                SystemCommands.MaximizeWindow(this);
            }
        }
        private void OnRestoreWindow()
        {
            SystemCommands.RestoreWindow(this);
        }
        private void OnCloseWindow()
        {
            SystemCommands.CloseWindow(this);
        }
        private void OnAbout()
        {
            MessageBox.Show("A propos .....");
        }
        private void OnPreferences()
        {
            MessageBox.Show("Preferences ....");
        }

        //Dependency Properties
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(String), typeof(WindowBase));
        public String Header
        {
            get { return (String)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public string MinimizeWindowToolTip { get; set; }
        public string RestoreWindowToolTip { get; set; }
        public string MaximizeWindowToolTip { get; set; }
        public string CloseWindowToolTip { get; set; }
    }
}
