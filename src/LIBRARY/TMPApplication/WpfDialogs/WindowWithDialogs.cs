namespace TMPApplication.WpfDialogs
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using TMPApplication.WpfDialogs.Contracts;

    [TemplatePart(Name = PART_OverlayBox, Type = typeof(Grid))]
    [TemplatePart(Name = PART_DialogsContainer, Type = typeof(Grid))]
    [TemplatePart(Name = PART_Window_Content, Type = typeof(ContentPresenter))]
    public class WindowWithDialogs : Window, IWindowWithDialogs
    {
        #region fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal Grid overlayBox;
        internal Grid dialogsContainer;
        internal ContentPresenter contentPresenter;

        private const string PART_OverlayBox = "PART_OverlayBox";
        private const string PART_DialogsContainer = "PART_DialogsContainer";
        private const string PART_Window_Content = "PART_Window_Content";

        private IInputElement restoreFocus;
        private Storyboard overlayStoryboard;

        #endregion

        #region Constructor

        static WindowWithDialogs()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowWithDialogs), new FrameworkPropertyMetadata(typeof(WindowWithDialogs)));
        }

        public WindowWithDialogs()
        {
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.overlayBox = this.GetTemplateChild(PART_OverlayBox) as Grid;
            this.dialogsContainer = this.GetTemplateChild(PART_DialogsContainer) as Grid;
            this.contentPresenter = this.GetTemplateChild(PART_Window_Content) as ContentPresenter;

            this.DialogManager = new DialogManager(this.DialogsContainer, this.Dispatcher, this.contentPresenter);
        }

        #region Public Properties

        public Grid DialogsContainer => this.dialogsContainer;

        #region ScaleValue Depdency Property
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register(nameof(ScaleValue),
            typeof(double), typeof(WindowWithDialogs), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(CoerceScaleValue)));

        private static object CoerceScaleValue(DependencyObject o, object value)
        {
            if (o is WindowWithDialogs mainWindow)
            {
                return mainWindow.OnCoerceScaleValue((double)value);
            }
            else
            {
                return value;
            }
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is WindowWithDialogs mainWindow)
            {
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
            {
                return 1.0f;
            }

            value = Math.Max(0.1, value);
            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue)
        {
        }

        public double ScaleValue
        {
            get => (double)this.GetValue(ScaleValueProperty);
            set => this.SetValue(ScaleValueProperty, value);
        }
        #endregion

        #region About Command
        private static readonly DependencyProperty AboutCommandProperty = DependencyProperty.Register(nameof(AboutCommand), typeof(ICommand), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт команду "О программе"
        /// </summary>
        public ICommand AboutCommand
        {
            get => (ICommand)this.GetValue(AboutCommandProperty);
            set => this.SetValue(AboutCommandProperty, value);
        }

        #endregion
        #region Is About Button Enabled
        private static readonly DependencyProperty IsAboutButtonEnabledProperty = DependencyProperty.Register(nameof(IsAboutButtonEnabled), typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт доступность кнопки About
        /// </summary>
        public bool IsAboutButtonEnabled
        {
            get => (bool)this.GetValue(IsAboutButtonEnabledProperty);
            set => this.SetValue(IsAboutButtonEnabledProperty, value);
        }

        #endregion
        #region Is About Button Visible
        private static readonly DependencyProperty IsAboutButtonVisibleProperty = DependencyProperty.Register(nameof(IsAboutButtonVisible), typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт видимость кнопки About
        /// </summary>
        public bool IsAboutButtonVisible
        {
            get => (bool)this.GetValue(IsAboutButtonVisibleProperty);
            set => this.SetValue(IsAboutButtonVisibleProperty, value);
        }

        #endregion
        #region Settings Command
        private static readonly DependencyProperty SettingsCommandProperty = DependencyProperty.Register(nameof(SettingsCommand), typeof(ICommand), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт команду "Параметры"
        /// </summary>
        public ICommand SettingsCommand
        {
            get => (ICommand)this.GetValue(SettingsCommandProperty);
            set => this.SetValue(SettingsCommandProperty, value);
        }

        #endregion
        #region Is Settings Button Enabled
        private static readonly DependencyProperty IsSettingsButtonEnabledProperty = DependencyProperty.Register(nameof(IsSettingsButtonEnabled), typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт доступность кнопки Settings
        /// </summary>
        public bool IsSettingsButtonEnabled
        {
            get => (bool)this.GetValue(IsSettingsButtonEnabledProperty);
            set => this.SetValue(IsSettingsButtonEnabledProperty, value);
        }

        #endregion
        #region Is Settings Button Visible
        private static readonly DependencyProperty IsSettingsButtonVisibleProperty = DependencyProperty.Register(nameof(IsSettingsButtonVisible), typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт видимость кнопки Settings
        /// </summary>
        public bool IsSettingsButtonVisible
        {
            get => (bool)this.GetValue(IsSettingsButtonVisibleProperty);
            set => this.SetValue(IsSettingsButtonVisibleProperty, value);
        }
        #endregion

        #region IsContentDialogVisible

        /// <summary>
        /// Determine whether a ContentDialog is currenlty shown inside the <seealso cref="MetroWindow"/> or not.
        /// </summary>
        public bool IsContentDialogVisible
        {
            get => (bool)this.GetValue(IsContentDialogVisibleProperty);
            set => this.SetValue(IsContentDialogVisibleProperty, value);
        }

        /// <summary>
        /// Determine whether a ContentDialog is currenlty shown inside the <seealso cref="MetroWindow"/> or not.
        /// </summary>
        private static readonly DependencyProperty IsContentDialogVisibleProperty =
            DependencyProperty.Register(nameof(IsContentDialogVisible), typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(false));
        #endregion IsContentDialogVisible

        #endregion

        #region Overlay Methods

        /// <summary>
        /// Begins to show the MetroWindow's overlay effect.
        /// </summary>
        /// <returns>A task representing the process.</returns>
        public System.Threading.Tasks.Task ShowOverlayAsync()
        {
            if (this.overlayBox == null)
            {
                throw new InvalidOperationException("OverlayBox can not be founded in this MetroWindow's template. Are you calling this before the window has loaded?");
            }

            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

            if (this.IsOverlayVisible() && this.overlayStoryboard == null)
            {
                // No Task.FromResult in .NET 4.
                tcs.SetResult(null);
                return tcs.Task;
            }

            this.Dispatcher.VerifyAccess();

            this.overlayBox.Visibility = Visibility.Visible;

            var sb = (Storyboard)this.Template.Resources["OverlayFastSemiFadeIn"];

            sb = sb.Clone();

            void completionHandler(object sender, EventArgs args)
            {
                sb.Completed -= completionHandler;

                if (this.overlayStoryboard == sb)
                {
                    this.overlayStoryboard = null;

                    if (this.IsContentDialogVisible == false)
                    {
                        this.IsContentDialogVisible = true;
                    }
                }

                tcs.TrySetResult(null);
            }

            sb.Completed += completionHandler;

            this.overlayBox.BeginStoryboard(sb);

            this.overlayStoryboard = sb;

            return tcs.Task;
        }

        /// <summary>
        /// Begins to hide the MetroWindow's overlay effect.
        /// </summary>
        /// <returns>A task representing the process.</returns>
        public System.Threading.Tasks.Task HideOverlayAsync()
        {
            if (this.overlayBox == null)
            {
                throw new InvalidOperationException("OverlayBox can not be founded in this MetroWindow's template. Are you calling this before the window has loaded?");
            }

            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

            if (this.overlayBox.Visibility == Visibility.Visible && this.overlayBox.Opacity == 0.0)
            {
                // No Task.FromResult in .NET 4.
                tcs.SetResult(null);
                return tcs.Task;
            }

            this.Dispatcher.VerifyAccess();

            var sb = (Storyboard)this.Template.Resources["OverlayFastSemiFadeOut"];

            sb = sb.Clone();

            void completionHandler(object sender, EventArgs args)
            {
                sb.Completed -= completionHandler;

                if (this.overlayStoryboard == sb)
                {
                    this.overlayBox.Visibility = Visibility.Hidden;
                    this.overlayStoryboard = null;

                    if (this.IsContentDialogVisible == true)
                    {
                        this.IsContentDialogVisible = false;
                    }
                }

                tcs.TrySetResult(null);
            }

            sb.Completed += completionHandler;

            this.overlayBox.BeginStoryboard(sb);

            this.overlayStoryboard = sb;

            return tcs.Task;
        }

        public bool IsOverlayVisible()
        {
            if (this.overlayBox == null)
            {
                throw new InvalidOperationException("OverlayBox can not be founded in this MetroWindow's template. Are you calling this before the window has loaded?");
            }

            return this.overlayBox.Visibility == Visibility.Visible && this.overlayBox.Opacity >= 0.7;
        }

        public void ShowOverlay()
        {
            this.overlayBox.Visibility = Visibility.Visible;

            // overlayBox.Opacity = 0.7;
            this.overlayBox.SetCurrentValue(Grid.OpacityProperty, 0.5);

            if (this.IsContentDialogVisible == false)
            {
                this.IsContentDialogVisible = true;
            }
        }

        public void HideOverlay()
        {
            // overlayBox.Opacity = 0.0;
            this.overlayBox.SetCurrentValue(Grid.OpacityProperty, 0.0);
            this.overlayBox.Visibility = System.Windows.Visibility.Hidden;

            if (this.IsContentDialogVisible == true)
            {
                this.IsContentDialogVisible = false;
            }
        }

        /// <summary>
        /// Stores the given element, or the last focused element via FocusManager, for restoring the focus after closing a dialog.
        /// </summary>
        /// <param name="thisElement">The element which will be focused again.</param>
        public void StoreFocus(IInputElement thisElement = null) // [CanBeNull]
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.restoreFocus = thisElement ?? this.restoreFocus ?? FocusManager.GetFocusedElement(this);
            }));
        }

        public void RestoreFocus()
        {
            if (this.restoreFocus != null)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Keyboard.Focus(this.restoreFocus);
                    this.restoreFocus = null;
                }));
            }
        }

        /// <summary>
        /// Clears the stored element which would get the focus after closing a dialog.
        /// </summary>
        public void ResetStoredFocus()
        {
            this.restoreFocus = null;
        }

        #endregion

        #region | IWindowWithDialogs implementation |

        public IDialog LastDialog { get; protected set; }

        public IDialogManager DialogManager { get; private set; }

        public IDialog DialogError(string message, string caption = null, Action ok = null)
        {
            this.logger?.Error(message);

            if (this.DialogManager == null)
            {
                throw new ArgumentNullException("DialogManager must be not null!");
            }
#if Windows
            System.Media.SystemSounds.Exclamation.Play();
#endif
            if (string.IsNullOrEmpty(caption))
                DispatcherExtensions.InUi(() => caption = this.Title);

            var dialog = this.DialogManager.CreateMessageDialog(message, caption, DialogMode.Ok, System.Windows.MessageBoxImage.Error);
            dialog.Ok = ok;
            return this.LastDialog = dialog;
        }

        public IDialog DialogError(Exception e)
        {
            return this.LastDialog = this.DialogError(TMPApp.GetExceptionDetails(e));
        }

        public IDialog DialogError(Exception e, string format)
        {
            return this.LastDialog = this.DialogError(string.Format(format, TMPApp.GetExceptionDetails(e)));
        }

        public IDialog DialogWarning(string message, string caption = null, Action ok = null)
        {
            this.logger?.Warn(message);

            if (this.DialogManager == null)
            {
                throw new ArgumentNullException(nameof(this.DialogManager));
            }
#if Windows
            System.Media.SystemSounds.Hand.Play();
#endif
            if (string.IsNullOrEmpty(caption))
                DispatcherExtensions.InUi(() => caption = this.Title);

            var dialog = this.DialogManager.CreateMessageDialog(message, caption, DialogMode.Ok, System.Windows.MessageBoxImage.Warning);
            dialog.Ok = ok;
            return this.LastDialog = dialog;
        }

        public IDialog DialogInfo(string message, string caption = null)
        {
            if (this.DialogManager == null)
            {
                throw new ArgumentNullException("DialogManager must be not null!");
            }
#if Windows
            System.Media.SystemSounds.Asterisk.Play();
#endif
            var dialog = this.DialogManager.CreateMessageDialog(message, caption, DialogMode.Ok, System.Windows.MessageBoxImage.Information);
            return this.LastDialog = dialog;
        }

        public IDialog DialogInfo(string message, string caption = null, Action ok = null)
        {
            if (this.DialogManager == null)
            {
                throw new ArgumentNullException("DialogManager must be not null!");
            }
#if Windows
            System.Media.SystemSounds.Asterisk.Play();
#endif
            if (string.IsNullOrEmpty(caption))
                DispatcherExtensions.InUi(() => caption = this.Title);

            var dialog = this.DialogManager.CreateMessageDialog(message, caption, DialogMode.Ok, System.Windows.MessageBoxImage.Information);
            dialog.Ok = ok;
            return this.LastDialog = dialog;
        }

        public IDialog DialogInfo(string message, string caption = null, System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.None, DialogMode mode = DialogMode.Ok)
        {
            if (this.DialogManager == null)
            {
                throw new ArgumentNullException("DialogManager must be not null!");
            }
#if Windows
            switch (image)
            {
                case MessageBoxImage.None:
                    System.Media.SystemSounds.Beep.Play();
                    break;
                case MessageBoxImage.Error:
                    System.Media.SystemSounds.Exclamation.Play();
                    break;
                case MessageBoxImage.Warning:
                    System.Media.SystemSounds.Hand.Play();
                    break;
                case MessageBoxImage.Information:
                    System.Media.SystemSounds.Asterisk.Play();
                    break;
                case MessageBoxImage.Question:
                    System.Media.SystemSounds.Question.Play();
                    break;
            }
#endif
            var dialog = this.DialogManager.CreateMessageDialog(message, caption, mode, image);
            return this.LastDialog = dialog;
        }

        public IDialog DialogQuestion(string message, string caption = null, DialogMode mode = DialogMode.YesNo)
        {
            if (this.DialogManager == null)
            {
                throw new ArgumentNullException("DialogManager must be not null!");
            }
#if Windows
            System.Media.SystemSounds.Question.Play();
#endif
            if (string.IsNullOrEmpty(caption))
                DispatcherExtensions.InUi(() => caption = this.Title);

            var dialog = this.DialogManager.CreateMessageDialog(message, caption, mode, System.Windows.MessageBoxImage.Question);
            return this.LastDialog = dialog;
        }

        public IWaitDialog DialogWaitingScreen(string message, string caption = null, bool indeterminate = true, DialogMode mode = DialogMode.None)
        {
            if (this.DialogManager == null)
            {
                throw new ArgumentNullException("DialogManager must be not null!");
            }

            var dialog = this.DialogManager.CreateWaitDialog(message, mode, indeterminate);
            this.LastDialog = dialog;
            return dialog;
        }

        public ICustomContentDialog DialogCustom(System.Windows.Controls.Control content, string caption = null, DialogMode mode = DialogMode.None)
        {
            if (this.DialogManager == null)
            {
                throw new ArgumentNullException("DialogManager must be not null!");
            }

            var dialog = this.DialogManager.CreateCustomContentDialog(content, caption, mode);
            this.LastDialog = dialog;
            return dialog;
        }

        public IProgressDialog DialogProgress(string message, string caption = null, DialogMode mode = DialogMode.None, bool indeterminate = true)
        {
            if (this.DialogManager == null)
            {
                throw new ArgumentNullException(nameof(this.DialogManager));
            }

            var dialog = this.DialogManager.CreateProgressDialog(message, caption, mode, indeterminate);
            this.LastDialog = dialog;
            return dialog;
        }

        public void ShowDialogError(string message, string caption = null, Action ok = null)
        {
            this.LastDialog = this.DialogError(message, caption, ok);
            this.LastDialog.Show();
        }

        public void ShowDialogError(Exception e)
        {
            this.LastDialog = this.DialogError(e);
            this.LastDialog.Show();
        }

        public void ShowDialogError(Exception e, string format)
        {
            this.LastDialog = this.DialogError(e, format);
            this.LastDialog.Show();
        }

        public void ShowDialogWarning(string message, string caption = null, Action ok = null)
        {
            this.LastDialog = this.DialogWarning(message, caption, ok);
            this.LastDialog.Show();
        }

        public void ShowDialogInfo(string message, string caption = null)
        {
            this.LastDialog = this.DialogInfo(message, caption);
            this.LastDialog.Show();
        }

        public void ShowDialogInfo(string message, string caption = null, Action ok = null)
        {
            this.LastDialog = this.DialogInfo(message, caption, ok);
            this.LastDialog.Show();
        }

        public void ShowDialogInfo(string message, string caption = null, System.Windows.MessageBoxImage image = System.Windows.MessageBoxImage.None, DialogMode mode = DialogMode.Ok)
        {
            this.LastDialog = this.DialogInfo(message, caption, image, mode);
            this.LastDialog.Show();
        }

        public void ShowDialogQuestion(string message, string caption = null, DialogMode mode = DialogMode.YesNo)
        {
            this.LastDialog = this.DialogQuestion(message, caption, mode);
            this.LastDialog.Show();
        }

        public void ShowDialogWaitingScreen(string message, string caption = null, bool indeterminate = true, DialogMode mode = DialogMode.None)
        {
            this.LastDialog = this.DialogWaitingScreen(message, caption, indeterminate, mode);
            this.LastDialog.Show();
        }

        public void ShowDialogCustom(System.Windows.Controls.Control content, string caption = null, DialogMode mode = DialogMode.None)
        {
            this.LastDialog = this.DialogCustom(content, caption, mode);
            this.LastDialog.Show();
        }

        public void ShowDialogProgress(string message, string caption = null, DialogMode mode = DialogMode.None, bool indeterminate = true)
        {
            this.LastDialog = this.DialogProgress(message, caption, mode, indeterminate);
            this.LastDialog.Show();
        }

        #endregion

        #region | TaskbarItemInfo implementation |

        private TaskbarItemInfo taskbarItemInfo;

        TaskbarItemInfo IWindowWithDialogs.TaskbarItemInfo
        {
            get => this.taskbarItemInfo;
            set
            {
                if (ValueType.Equals(value, this.taskbarItemInfo))
                {
                    return;
                }

                this.taskbarItemInfo = value;
                if (this.TaskbarItemInfo == null)
                {
                    this.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
                }

                this.TaskbarItemInfo.Description = value.Description;
                switch (value.ProgressState)
                {
                    case TaskbarItemProgressState.None:
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                        break;
                    case TaskbarItemProgressState.Indeterminate:
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                        break;
                    case TaskbarItemProgressState.Normal:
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
                        this.TaskbarItemInfo.ProgressValue = value.ProgressValue;
                        break;
                    case TaskbarItemProgressState.Error:
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                        break;
                    case TaskbarItemProgressState.Paused:
                        this.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Paused;
                        break;
                    default:
                        break;
                }

                this.TaskbarItemInfo.Description = value.Description;
            }
        }

        #endregion
    }
}
