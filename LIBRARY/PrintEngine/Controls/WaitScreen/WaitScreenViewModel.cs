namespace TMP.PrintEngine.Controls.WaitScreen
{
    using System;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;
    using TMP.PrintEngine.Extensions;
    using TMP.PrintEngine.Resources;
    using TMP.PrintEngine.ViewModels;

    public class WaitScreenViewModel : AViewModel, IWaitScreenViewModel
    {
        private Window waitScreenWindow;

        protected Window WaitScreenWindow
        {
            get
            {
                if (this.waitScreenWindow == null)
                {
                    if (((UserControl)this.View).Parent != null)
                    {
                        ((Window)((UserControl)this.View).Parent).Content = null;
                    }

                    this.waitScreenWindow = new Window
                    {
                        AllowsTransparency = true,
                        Content = this.View as UIElement,
                        WindowStyle = WindowStyle.None,
                        ShowInTaskbar = false,
                        Background = new SolidColorBrush(Colors.Transparent),
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        WindowState = WindowState.Normal,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        SizeToContent = SizeToContent.WidthAndHeight,
                    };
                }

                return this.waitScreenWindow;
            }
        }

        private readonly Dispatcher dispatcher;

        public WaitScreenViewModel(IWaitScreenView view)
            : base(view)
        {
            if (Application.Current != null)
            {
                this.dispatcher = Application.Current.Dispatcher;
            }

            this._hideTimer = new Timer();
            this._hideTimer.Elapsed += this.HideTimerElapsed;
            this._hideTimer.Interval = 300;
            this._hideTimer.Enabled = false;

            this.Enabled = true;

            ((UserControl)view).Loaded += this.WaitScreenPresenterLoaded;
        }

        private void WaitScreenPresenterLoaded(object sender, RoutedEventArgs e)
        {
            this._isLoaded = true;
        }

        public bool Enabled { get; set; }

        #region IWaitScreenViewModel Members

        public string Message
        {
            get => (string)this.GetValue(MessageProperty);

            set => this.SetValue(MessageProperty, value);
        }

        public void HideNow()
        {
            this.HideWaitScreenHandler();
        }

        #endregion

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
                "Message",
                typeof(string),
                typeof(WaitScreenViewModel));

        private void HideTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this._hideTimer.Stop();
            if (this.dispatcher != null)
            {
                this.dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(this.HideWaitScreenHandler));
            }
        }

        private void HideWaitScreenHandler()
        {
            if (((UserControl)this.View).Parent != null)
            {
                ((Window)((UserControl)this.View).Parent).Content = null;
            }

            this._isShown = false;
            this.WaitScreenWindow.Close();
            if (this.DisableParent && Application.Current != null)
            {
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    Application.Current.EnableWindow();
                    ////Application.Current.MainWindow.Focus();
                }
            }

            this.waitScreenWindow = null;
        }

        private bool _isShown;
        private bool _isLoaded;
        private readonly Timer _hideTimer;

        public bool Show()
        {
            return this.Show(Strings.WaitMessage, true);
        }

        public bool Show(string message)
        {
            return this.Show(message, true);
        }

        private void ShowWaitScreenHandler()
        {
            if (this._isShown)
            {
                this._hideTimer.Stop();
                return;
            }

            this._isShown = true;
            this.WaitScreenWindow.Owner = ApplicationExtention.MainWindow;
            this.WaitScreenWindow.Show();
            if (this.DisableParent && Application.Current != null)
            {
                Application.Current.DisableWindow(0.90);
            }
        }

        public bool Hide()
        {
            if (this._isShown == false)
            {
                return false;
            }

            if (this.Initiator != null)
            {
                return false;
            }

            this._hideTimer.Start();
            return true;
        }

        #region IWaitScreenViewModel Members

        public bool Show(string message, bool disableParent)
        {
            if (this._isShown)
            {
                return false;
            }

            this.Message = message;
            this.DisableParent = disableParent;
            if (this.Enabled && this.dispatcher != null)
            {
                this.dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(this.ShowWaitScreenHandler));
                this.Block(true);
            }

            return true;
        }

        private void Block(bool loaded)
        {
            if (Application.Current == null)
            {
                return;
            }

            while (true)
            {
                Application.Current.DoEvents();
                System.Threading.Thread.Sleep(5);
                if (this._isLoaded != loaded)
                {
                    continue;
                }

                break;
            }
        }

        #endregion

        public bool DisableParent { get; set; }

        #region IWaitScreenViewModel Members

        public object Initiator { get; set; }

        #endregion
    }
}
