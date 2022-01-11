namespace TMPApplication.CustomWpfWindow
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using Native;

    [TemplatePart(Name = "PART_Max", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Close", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Min", Type = typeof(Button))]
    public class WindowButtonCommands : ContentControl, INotifyPropertyChanged
    {
        public string Settings => "Настройки программы";

        public string About => "О программе";

        public event ClosingWindowEventHandler ClosingWindow;

        public delegate void ClosingWindowEventHandler(object sender, ClosingWindowEventHandlerArgs args);

        public string Minimize
        {
            get
            {
                if (string.IsNullOrEmpty(minimize))
                {
                    minimize = this.GetCaption(900);
                }

                return minimize;
            }
        }

        public string Maximize
        {
            get
            {
                if (string.IsNullOrEmpty(maximize))
                {
                    maximize = this.GetCaption(901);
                }

                return maximize;
            }
        }

        public string Close
        {
            get
            {
                if (string.IsNullOrEmpty(closeText))
                {
                    closeText = this.GetCaption(905);
                }

                return closeText;
            }
        }

        public string Restore
        {
            get
            {
                if (string.IsNullOrEmpty(restore))
                {
                    restore = this.GetCaption(903);
                }

                return restore;
            }
        }

        private static string minimize;
        private static string maximize;
        private static string closeText;
        private static string restore;
        private Button min;
        private Button max;
        private Button close;
        private SafeLibraryHandle user32 = null;

        static WindowButtonCommands()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowButtonCommands), new FrameworkPropertyMetadata(typeof(WindowButtonCommands)));
        }

        private string GetCaption(int id)
        {
            if (this.user32 == null)
            {
                this.user32 = UnsafeNativeMethods.LoadLibrary(Environment.SystemDirectory + "\\User32.dll");
            }

            var sb = new StringBuilder(256);
            UnsafeNativeMethods.LoadString(this.user32, (uint)id, sb, sb.Capacity);
            return sb.ToString().Replace("&", string.Empty);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.ParentWindow = this.TryFindParent<WindowWithDialogs>();
            if (this.ParentWindow != null)
            {
                this.close = this.Template.FindName("PART_Close", this) as Button;
                if (this.close != null)
                {
                    this.close.Click += this.CloseClick;
                }

                this.max = this.Template.FindName("PART_Max", this) as Button;
                if (this.max != null)
                {
                    this.max.Click += this.MaximizeClick;
                }

                this.min = this.Template.FindName("PART_Min", this) as Button;
                if (this.min != null)
                {
                    this.min.Click += this.MinimizeClick;
                }
            }
        }

        protected void OnClosingWindow(ClosingWindowEventHandlerArgs args)
        {
            var handler = this.ClosingWindow;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            MS.Windows.Shell.SystemCommands.MinimizeWindow(this.ParentWindow);
        }

        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            if (this.ParentWindow.WindowState == WindowState.Maximized)
            {
                MS.Windows.Shell.SystemCommands.RestoreWindow(this.ParentWindow);
            }
            else
            {
                MS.Windows.Shell.SystemCommands.MaximizeWindow(this.ParentWindow);
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            var closingWindowEventHandlerArgs = new ClosingWindowEventHandlerArgs();
            this.OnClosingWindow(closingWindowEventHandlerArgs);

            if (closingWindowEventHandlerArgs.Cancelled)
            {
                return;
            }

            this.ParentWindow.Close();
        }

        private WindowWithDialogs parentWindow;

        public WindowWithDialogs ParentWindow
        {
            get => this.parentWindow;
            set
            {
                if (Equals(this.parentWindow, value))
                {
                    return;
                }

                this.parentWindow = value;
                this.RaisePropertyChanged(nameof(this.ParentWindow));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
