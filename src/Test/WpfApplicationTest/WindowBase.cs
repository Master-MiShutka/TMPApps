namespace WpfApplicationTest
{
    using System;
    using System.Windows;
    using TMP.Shared.Commands;

    public class WindowBase : Window
    {
        public WindowBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
        }

        // Relay Commands
        public RelayCommand MinimizeWindowCommand => new RelayCommand(param => this.OnMinimizeWindow());

        public RelayCommand MaximizeWindowCommand => new RelayCommand(param => this.OnMaximizeWindow());

        public RelayCommand RestoreWindowCommand => new RelayCommand(param => this.OnRestoreWindow());

        public RelayCommand CloseWindowCommand => new RelayCommand(param => this.OnCloseWindow());

        public RelayCommand AboutWindowCommand => new RelayCommand(param => this.OnAbout());

        public RelayCommand PreferencesWindowCommand => new RelayCommand(param => this.OnPreferences());

        private void OnMinimizeWindow()
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnMaximizeWindow()
        {
            if (this.WindowState == WindowState.Normal)
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

        // Dependency Properties
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(WindowBase));

        public string Header
        {
            get => (string)this.GetValue(HeaderProperty);
            set => this.SetValue(HeaderProperty, value);
        }

        public string MinimizeWindowToolTip { get; set; }

        public string RestoreWindowToolTip { get; set; }

        public string MaximizeWindowToolTip { get; set; }

        public string CloseWindowToolTip { get; set; }
    }
}
