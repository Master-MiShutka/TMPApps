namespace TMP.Shared.Windows.Extensions
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;

    public static class ApplicationExtention
    {
        public static event EventHandler WindowEnabled;

        public static event EventHandler WindowDisabled;

        private static Window mainWindow;

        public static Window MainWindow
        {
            set
            {
                if (!DifferentWindow(value))
                {
                    return;
                }

                ChangeMainWindow(value);
            }

            get => GetMainWindow();
        }

        public static Window ActiveWindow
        {
            get
            {
                if (Application.Current == null)
                {
                    return null;
                }
                else
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.IsActive)
                        {
                            return window;
                        }
                    }

                    return Application.Current.MainWindow;
                }
            }
        }

        #region Private Methods

        private static Window GetMainWindow()
        {
            return mainWindow ?? Application.Current.MainWindow;
        }

        private static void ChangeMainWindow(Window newMainWindow)
        {
            var oldMainWindow = mainWindow ?? Application.Current.MainWindow;
            mainWindow = newMainWindow ?? Application.Current.MainWindow;
            UnsetWindowActivationEventHandlers(oldMainWindow);
            SetWindowActivationEventHandlers(newMainWindow);
            PublishMainWindowChangedEvent(oldMainWindow);
        }

        private static void SetWindowActivationEventHandlers(Window window)
        {
            if (window != null && window != Application.Current.MainWindow)
            {
                window.Activated += WindowActivated;
                window.Deactivated += WindowDeactivated;
            }
        }

        private static void UnsetWindowActivationEventHandlers(Window window)
        {
            if (window != null && window != Application.Current.MainWindow)
            {
                window.Activated += WindowActivated;
                window.Deactivated += WindowDeactivated;
            }
        }

        private static void WindowDeactivated(object sender, EventArgs e)
        {
        }

        private static void WindowActivated(object sender, EventArgs e)
        {
        }

        private static void PublishMainWindowChangedEvent(Window oldMainWindow)
        {
        }

        private static bool DifferentWindow(Window newMainWindow)
        {
            return mainWindow != newMainWindow;
        }

        private static void DisableWindow(ContentControl window, double opacity)
        {
            window.Focusable = true;
            window.MouseEnter += WindowMouseEnter;
            if (((FrameworkElement)window.Content).IsEnabled == false)
            {
                return;
            }

            ((FrameworkElement)window.Content).IsEnabled = false;
            MakeDisabledOpacity(window, opacity);
            PublishMainWindowEnableChangedEven(window, false);
        }

        private static void MakeDisabledOpacity(UIElement window, double opacity)
        {
            window.Opacity = opacity;
        }

        private static void PublishMainWindowEnableChangedEven(ContentControl window, bool isEnabled)
        {
            if (WindowDisabled != null)
            {
                WindowDisabled(window, new EventArgs());
            }
        }

        private static bool ContainsValidWindow(Application source)
        {
            return source != null && source.Windows.Count > 0;
        }

        private static void WindowMouseEnter(object sender, MouseEventArgs e)
        {
            Keyboard.Focus(sender as UIElement);
        }

        #endregion

        #region Public Methods

        public static void DisableWindow(this Application source)
        {
            source.DisableWindow(0.5);
        }

        public static void DisableWindow(this Application source, double opacity)
        {
            if (!ContainsValidWindow(source))
            {
                return;
            }

            var window = MainWindow ?? source.Windows[0];
            DisableWindow(window, opacity);
        }

        public static void EnableWindow(this Application source)
        {
            if (source != null && source.Windows.Count > 0)
            {
                var window = MainWindow ?? source.Windows[0];
                if (window != null)
                {
                    window.MouseEnter -= WindowMouseEnter;
                    if (((FrameworkElement)window.Content).IsEnabled)
                    {
                        return;
                    }

                    ((FrameworkElement)window.Content).IsEnabled = true;
                    window.Opacity = 1;
                }

                if (WindowEnabled != null)
                {
                    WindowEnabled(source, new EventArgs());
                }
            }
        }

        public static bool IsWindowEnabled(this Application source)
        {
            if (source == null)
            {
                return false;
            }

            var isEnabled = true;
            if (source.Windows.Count > 0)
            {
                var window = MainWindow ?? source.Windows[0];
                if (window != null)
                {
                    isEnabled = ((FrameworkElement)window.Content).IsEnabled;
                }
            }

            return isEnabled;
        }

        public static void DoEvents(this Application source)
        {
            if (source == null)
            {
                return;
            }

            source.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        public static void Wait(this Application source, double miliseconds)
        {
            var start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < miliseconds)
            {
                source.DoEvents();
            }
        }

        private static int priorityCount;

        public static void ResetDoEventsPriority(this Application source)
        {
            priorityCount = 0;
        }

        public static void DoEvents(this Application source, int priority)
        {
            if (++priorityCount != priority)
            {
                return;
            }

            priorityCount = 0;
            DoEvents(source);
        }

        #endregion
    }
}
