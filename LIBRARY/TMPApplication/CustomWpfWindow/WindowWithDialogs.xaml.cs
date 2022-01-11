namespace TMPApplication.CustomWpfWindow
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using MS.Windows.Shell;
    using Native;
    using TMPApplication.WpfDialogs;
    using TMPApplication.WpfDialogs.Contracts;

    /// <summary>
    /// Interaction logic for WindowWithDialogs.xaml
    /// </summary>

    [TemplatePart(Name = PART_Icon, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_TitleBar, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_WindowButtonCommands, Type = typeof(WindowButtonCommands))]

    [TemplatePart(Name = PART_OverlayBox, Type = typeof(Grid))]
    [TemplatePart(Name = PART_DialogsContainer, Type = typeof(Grid))]
    [TemplatePart(Name = PART_WindowTitleThumb, Type = typeof(Thumb))]

    [TemplatePart(Name = PART_Window_Content, Type = typeof(ContentPresenter))]
    public class WindowWithDialogs : Window, IWindowWithDialogs
    {
        #region fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal WindowButtonCommands windowButtonCommands;
        internal Grid overlayBox;
        internal Grid dialogsContainer;
        internal ContentPresenter contentPresenter;
        private Thumb windowTitleThumb;

        private const string PART_Icon = "PART_Icon";
        private const string PART_TitleBar = "PART_TitleBar";
        private const string PART_WindowButtonCommands = "PART_WindowButtonCommands";

        private const string PART_OverlayBox = "PART_OverlayBox";
        private const string PART_DialogsContainer = "PART_DialogsContainer";
        private const string PART_WindowTitleThumb = "PART_WindowTitleThumb";

        private const string PART_Window_Content = "PART_Window_Content";

        private IInputElement restoreFocus;
        private Storyboard overlayStoryboard;

        #endregion

        #region ctor

        static WindowWithDialogs()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowWithDialogs), new FrameworkPropertyMetadata(typeof(WindowWithDialogs)));
        }

        public WindowWithDialogs()
        {
            this.CommandBindings.Add(new CommandBinding(MS.Windows.Shell.SystemCommands.CloseWindowCommand, this.OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(MS.Windows.Shell.SystemCommands.MaximizeWindowCommand, this.OnMaximizeWindow, this.OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(MS.Windows.Shell.SystemCommands.MinimizeWindowCommand, this.OnMinimizeWindow, this.OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(MS.Windows.Shell.SystemCommands.RestoreWindowCommand, this.OnRestoreWindow, this.OnCanResizeWindow));

            this.SourceInitialized += new EventHandler(this.Window_SourceInitialized);

            this.Loaded += this.WindowWithDialogs_Loaded;
        }

        #endregion

        #region properties

        public Grid OverlayBox => this.overlayBox;

        public Grid DialogsContainer => this.dialogsContainer;

        #region Show Window Icon
        private static readonly DependencyProperty ShowIconProperty = DependencyProperty.Register("ShowIcon", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт видимость иконки окна
        /// </summary>
        public bool ShowIcon
        {
            get => (bool)this.GetValue(ShowIconProperty);
            set => this.SetValue(ShowIconProperty, value);
        }

        #endregion
        #region Icon Edge Mode
        private static readonly DependencyProperty IconEdgeModeProperty = DependencyProperty.Register("IconEdgeMode", typeof(EdgeMode), typeof(WindowWithDialogs), new PropertyMetadata(EdgeMode.Aliased));

        /// <summary>
        /// Возвращает или задаёт режим отрисовки краев иконки
        /// </summary>
        public EdgeMode IconEdgeMode
        {
            get => (EdgeMode)this.GetValue(IconEdgeModeProperty);
            set => this.SetValue(IconEdgeModeProperty, value);
        }

        #endregion
        #region Icon Bitmap Scaling Mode
        private static readonly DependencyProperty IconBitmapScalingModeProperty = DependencyProperty.Register("IconBitmapScalingMode", typeof(BitmapScalingMode), typeof(WindowWithDialogs), new PropertyMetadata(BitmapScalingMode.HighQuality));

        /// <summary>
        /// Возвращает или задаёт аалгоритм масштабирования иконки
        /// </summary>
        public BitmapScalingMode IconBitmapScalingMode
        {
            get => (BitmapScalingMode)this.GetValue(IconBitmapScalingModeProperty);
            set => this.SetValue(IconBitmapScalingModeProperty, value);
        }

        #endregion
        #region Icon Template
        private static readonly DependencyProperty IconTemplateProperty = DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт шаблон иконки
        /// </summary>
        public DataTemplate IconTemplate
        {
            get => (DataTemplate)this.GetValue(IconTemplateProperty);
            set => this.SetValue(IconTemplateProperty, value);
        }
        #endregion

        #region ShowDialogsOverTitleBar
        private static readonly DependencyProperty ShowDialogsOverTitleBarProperty = DependencyProperty.Register("ShowDialogsOverTitleBar", typeof(bool), typeof(WindowWithDialogs), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Возвращает или задаёт отображать ли диалог поверх панели заголовка окна
        /// </summary>
        public bool ShowDialogsOverTitleBar
        {
            get => (bool)this.GetValue(ShowDialogsOverTitleBarProperty);
            set => this.SetValue(ShowDialogsOverTitleBarProperty, value);
        }

        #endregion
        #region Show TitleBar
        private static readonly DependencyProperty ShowTitleBarProperty = DependencyProperty.Register("ShowTitleBar", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true, OnShowTitleBarPropertyChangedCallback, OnShowTitleBarCoerceValueCallback));

        /// <summary>
        /// Возвращает или задаёт видимость панели заголовка
        /// </summary>
        public bool ShowTitleBar
        {
            get => (bool)this.GetValue(ShowTitleBarProperty);
            set => this.SetValue(ShowTitleBarProperty, value);
        }

        private static void OnShowTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (WindowWithDialogs)d;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibiltyForAllTitleElements((bool)e.NewValue);
            }
        }

        private static object OnShowTitleBarCoerceValueCallback(DependencyObject d, object value)
        {
            // if UseNoneWindowStyle = true no title bar should be shown
            if (((WindowWithDialogs)d).UseNoneWindowStyle)
            {
                return false;
            }

            return value;
        }

        #endregion
        #region Title Foreground
        private static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register("TitleForeground", typeof(Brush), typeof(WindowWithDialogs));

        /// <summary>
        /// Возвращает или задаёт кисть для отрисовки заголовка окна
        /// </summary>
        public Brush TitleForeground
        {
            get => (Brush)this.GetValue(TitleForegroundProperty);
            set => this.SetValue(TitleForegroundProperty, value);
        }

        #endregion
        #region Title Brush
        private static readonly DependencyProperty TitleBrushProperty = DependencyProperty.Register("TitleBrush", typeof(Brush), typeof(WindowWithDialogs), new PropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Возвращает или задаёт кисть для заливки панели заголовка окна
        /// </summary>
        public Brush TitleBrush
        {
            get => (Brush)this.GetValue(TitleBrushProperty);
            set => this.SetValue(TitleBrushProperty, value);
        }

        #endregion
        #region Title Template
        private static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register("TitleTemplate", typeof(DataTemplate), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт шаблон заголовка окна
        /// </summary>
        public DataTemplate TitleTemplate
        {
            get => (DataTemplate)this.GetValue(TitleTemplateProperty);
            set => this.SetValue(TitleTemplateProperty, value);
        }

        #endregion
        #region NonActiveWindowTitleBrush
        private static readonly DependencyProperty NonActiveWindowTitleBrushProperty =
            DependencyProperty.Register("NonActiveWindowTitleBrush", typeof(Brush), typeof(WindowWithDialogs), new PropertyMetadata(Brushes.Gray));

        /// <summary>
        /// Возвращает или задаёт кисть для панели заголовка окна, когда оно в не активном состоянии
        /// </summary>
        public Brush NonActiveWindowTitleBrush
        {
            get => (Brush)this.GetValue(NonActiveWindowTitleBrushProperty);
            set => this.SetValue(NonActiveWindowTitleBrushProperty, value);
        }
        #endregion

        #region Show Min Button
        private static readonly DependencyProperty ShowMinButtonProperty = DependencyProperty.Register("ShowMinButton", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт видимость кнопки минимизирования окна
        /// </summary>
        public bool ShowMinButton
        {
            get => (bool)this.GetValue(ShowMinButtonProperty);
            set => this.SetValue(ShowMinButtonProperty, value);
        }

        #endregion
        #region Is Window Min Button Enabled
        private static readonly DependencyProperty IsMinButtonEnabledProperty = DependencyProperty.Register("IsMinButtonEnabled", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт доступность кнопки минимизирования окна
        /// </summary>
        public bool IsMinButtonEnabled
        {
            get => (bool)this.GetValue(IsMinButtonEnabledProperty);
            set => this.SetValue(IsMinButtonEnabledProperty, value);
        }

        #endregion
        #region Min Button Style
        private static readonly DependencyProperty MinButtonStyleProperty = DependencyProperty.Register("MinButtonStyle", typeof(Style), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт стиль кнопки минимизирования окна
        /// </summary>
        public Style MinButtonStyle
        {
            get => (Style)this.GetValue(MinButtonStyleProperty);
            set => this.SetValue(MinButtonStyleProperty, value);
        }
        #endregion

        #region Show MaxRestore Button
        private static readonly DependencyProperty ShowMaxRestoreButtonProperty = DependencyProperty.Register("ShowMaxRestoreButton", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт видимость кнопки разворачивания окна
        /// </summary>
        public bool ShowMaxRestoreButton
        {
            get => (bool)this.GetValue(ShowMaxRestoreButtonProperty);
            set => this.SetValue(ShowMaxRestoreButtonProperty, value);
        }

        #endregion
        #region Is Window Min Button Enabled
        private static readonly DependencyProperty IsMaxRestoreButtonEnabledProperty = DependencyProperty.Register("IsMaxRestoreButtonEnabled", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт доступность кнопки разворачивания окна
        /// </summary>
        public bool IsMaxRestoreButtonEnabled
        {
            get => (bool)this.GetValue(IsMaxRestoreButtonEnabledProperty);
            set => this.SetValue(IsMaxRestoreButtonEnabledProperty, value);
        }

        #endregion
        #region MaxRestore Button Style
        private static readonly DependencyProperty MaxRestoreButtonStyleProperty = DependencyProperty.Register("MaxRestoreButtonStyle", typeof(Style), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт стиль кнопки разворачивания окна
        /// </summary>
        public Style MaxRestoreButtonStyle
        {
            get => (Style)this.GetValue(MaxRestoreButtonStyleProperty);
            set => this.SetValue(MaxRestoreButtonStyleProperty, value);
        }
        #endregion

        #region Show Close Button
        private static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт видимость кнопки закрытия окна
        /// </summary>
        public bool ShowCloseButton
        {
            get => (bool)this.GetValue(ShowCloseButtonProperty);
            set => this.SetValue(ShowCloseButtonProperty, value);
        }

        #endregion
        #region Is Window Min Button Enabled
        private static readonly DependencyProperty IsCloseButtonEnabledProperty = DependencyProperty.Register("IsCloseButtonEnabled", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт доступность кнопки закрытия окна
        /// </summary>
        public bool IsCloseButtonEnabled
        {
            get => (bool)this.GetValue(IsCloseButtonEnabledProperty);
            set => this.SetValue(IsCloseButtonEnabledProperty, value);
        }

        #endregion
        #region Close Button Style
        private static readonly DependencyProperty CloseButtonStyleProperty = DependencyProperty.Register("CloseButtonStyle", typeof(Style), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт стиль кнопки закрытия окна
        /// </summary>
        public Style CloseButtonStyle
        {
            get => (Style)this.GetValue(CloseButtonStyleProperty);
            set => this.SetValue(CloseButtonStyleProperty, value);
        }
        #endregion

        #region Window Button Style
        private static readonly DependencyProperty WindowButtonStyleProperty = DependencyProperty.Register("WindowButtonStyle", typeof(Style), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт стиль кнопки окна
        /// </summary>
        public Style WindowButtonStyle
        {
            get => (Style)this.GetValue(WindowButtonStyleProperty);
            set => this.SetValue(WindowButtonStyleProperty, value);
        }
        #endregion

        #region About Command
        private static readonly DependencyProperty AboutCommandProperty = DependencyProperty.Register("AboutCommand", typeof(ICommand), typeof(WindowWithDialogs), new PropertyMetadata(null));

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
        private static readonly DependencyProperty IsAboutButtonEnabledProperty = DependencyProperty.Register("IsAboutButtonEnabled", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

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
        private static readonly DependencyProperty IsAboutButtonVisibleProperty = DependencyProperty.Register("IsAboutButtonVisible", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

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
        private static readonly DependencyProperty SettingsCommandProperty = DependencyProperty.Register("SettingsCommand", typeof(ICommand), typeof(WindowWithDialogs), new PropertyMetadata(null));

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
        private static readonly DependencyProperty IsSettingsButtonEnabledProperty = DependencyProperty.Register("IsSettingsButtonEnabled", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

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
        private static readonly DependencyProperty IsSettingsButtonVisibleProperty = DependencyProperty.Register("IsSettingsButtonVisible", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Возвращает или задаёт видимость кнопки Settings
        /// </summary>
        public bool IsSettingsButtonVisible
        {
            get => (bool)this.GetValue(IsSettingsButtonVisibleProperty);
            set => this.SetValue(IsSettingsButtonVisibleProperty, value);
        }
        #endregion

        #region UseNoneWindowStyle
        public static readonly DependencyProperty UseNoneWindowStyleProperty = DependencyProperty.Register("UseNoneWindowStyle", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(false, OnUseNoneWindowStylePropertyChangedCallback));

        /// <summary>
        /// Gets/sets whether the WindowStyle is None or not.
        /// Setting UseNoneWindowStyle="True" on a <seealso cref="MetroWindow"/>
        /// is equivalent to not showing the titlebar of the window.
        /// </summary>
        public bool UseNoneWindowStyle
        {
            get => (bool)this.GetValue(UseNoneWindowStyleProperty);
            set => this.SetValue(UseNoneWindowStyleProperty, value);
        }

        private static void OnUseNoneWindowStylePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                // if UseNoneWindowStyle = true no title bar should be shown
                var useNoneWindowStyle = (bool)e.NewValue;
                var window = (WindowWithDialogs)d;
                window.ToggleNoneWindowStyle(useNoneWindowStyle);
            }
        }

        private void ToggleNoneWindowStyle(bool useNoneWindowStyle)
        {
            // UseNoneWindowStyle means no title bar
            if (useNoneWindowStyle)
            {
                this.ShowTitleBar = false;
            }
        }
        #endregion UseNoneWindowStyle

        #region IsWindowDraggable
        private static readonly DependencyProperty IsWindowDraggableProperty = DependencyProperty.Register("IsWindowDraggable", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        public bool IsWindowDraggable
        {
            get => (bool)this.GetValue(IsWindowDraggableProperty);
            set => this.SetValue(IsWindowDraggableProperty, value);
        }
        #endregion IsWindowDraggable

        #region ShowSystemMenuOnRightClick
        private static readonly DependencyProperty ShowSystemMenuOnRightClickProperty = DependencyProperty.Register("ShowSystemMenuOnRightClick", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(true));

        /// <summary>
        /// Gets/sets if the the system menu should popup on right click.
        /// </summary>
        public bool ShowSystemMenuOnRightClick
        {
            get => (bool)this.GetValue(ShowSystemMenuOnRightClickProperty);
            set => this.SetValue(ShowSystemMenuOnRightClickProperty, value);
        }
        #endregion ShowSystemMenuOnRightClick

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
            DependencyProperty.Register("IsContentDialogVisible", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(false));
        #endregion IsContentDialogVisible

        #region IgnoreTaskbarOnMaximize
        public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty =
            DependencyProperty.Register("IgnoreTaskbarOnMaximize", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(false));

        /// <summary>
        /// Возвращает или задаёт игнорировать ли панель задач при разворачивании окна
        /// </summary>
        public bool IgnoreTaskbarOnMaximize
        {
            get => (bool)this.GetValue(IgnoreTaskbarOnMaximizeProperty);
            set => this.SetValue(IgnoreTaskbarOnMaximizeProperty, value);
        }
        #endregion

        #region GlowBrush
        private static readonly DependencyProperty GlowBrushProperty =
            DependencyProperty.Register("GlowBrush", typeof(SolidColorBrush), typeof(WindowWithDialogs), new PropertyMetadata(null));

        /// <summary>
        /// Возвращает или задаёт кисть для эффекта свечения
        /// </summary>
        public SolidColorBrush GlowBrush
        {
            get => (SolidColorBrush)this.GetValue(GlowBrushProperty);
            set => this.SetValue(GlowBrushProperty, value);
        }

        #endregion
        #region NonActiveGlowBrush
        private static readonly DependencyProperty NonActiveGlowBrushProperty =
            DependencyProperty.Register("NonActiveGlowBrush", typeof(SolidColorBrush), typeof(WindowWithDialogs), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(153, 153, 153)))); // #999999

        /// <summary>
        /// Возвращает или задаёт кисть для эффекта свечения, когда окно в не активном состоянии
        /// </summary>
        public SolidColorBrush NonActiveGlowBrush
        {
            get => (SolidColorBrush)this.GetValue(NonActiveGlowBrushProperty);
            set => this.SetValue(NonActiveGlowBrushProperty, value);
        }
        #endregion

        #region NonActiveBorderBrush
        private static readonly DependencyProperty NonActiveBorderBrushProperty =
            DependencyProperty.Register("NonActiveBorderBrush", typeof(Brush), typeof(WindowWithDialogs), new PropertyMetadata(Brushes.Gray));

        /// <summary>
        /// Возвращает или задаёт кисть для границы окна, когда оно в не активном состоянии
        /// </summary>
        public Brush NonActiveBorderBrush
        {
            get => (Brush)this.GetValue(NonActiveBorderBrushProperty);
            set => this.SetValue(NonActiveBorderBrushProperty, value);
        }
        #endregion

        #region ToggleFullScreen
        private static readonly DependencyProperty ToggleFullScreenProperty =
            DependencyProperty.Register("ToggleFullScreen", typeof(bool), typeof(WindowWithDialogs), new PropertyMetadata(false, ToggleFullScreenPropertyChangedCallback));

        /// <summary>
        /// Возвращает или задаёт отображение окна во весь экран
        /// </summary>
        public bool ToggleFullScreen
        {
            get => (bool)this.GetValue(ToggleFullScreenProperty);
            set => this.SetValue(ToggleFullScreenProperty, value);
        }

        private static void ToggleFullScreenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var WindowWithDialogs = (WindowWithDialogs)dependencyObject;
            if (e.OldValue != e.NewValue)
            {
                var fullScreen = (bool)e.NewValue;
                if (fullScreen)
                {
                    WindowWithDialogs.UseNoneWindowStyle = true;
                    WindowWithDialogs.IgnoreTaskbarOnMaximize = true;
                    WindowWithDialogs.WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowWithDialogs.UseNoneWindowStyle = false;
                    WindowWithDialogs.ShowTitleBar = true; // <-- this must be set to true
                    WindowWithDialogs.IgnoreTaskbarOnMaximize = false;
                    WindowWithDialogs.WindowState = WindowState.Normal;
                }
            }
        }
        #endregion

        protected IntPtr CriticalHandle
        {
            get
            {
                var value = typeof(Window)
                    .GetProperty("CriticalHandle", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this, Array.Empty<object>());
                return (IntPtr)value;
            }
        }

        #endregion

        #region private methods

        private void WindowWithDialogs_Loaded(object sender, RoutedEventArgs e)
        {
            this.ToggleNoneWindowStyle(this.UseNoneWindowStyle);
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Utils.WindowSizing.WindowInitialized(this);
        }

        #endregion

        #region methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.windowButtonCommands = this.GetTemplateChild(PART_WindowButtonCommands) as WindowButtonCommands;

            this.overlayBox = this.GetTemplateChild(PART_OverlayBox) as Grid;
            this.dialogsContainer = this.GetTemplateChild(PART_DialogsContainer) as Grid;
            this.windowTitleThumb = this.GetTemplateChild(PART_WindowTitleThumb) as Thumb;

            this.SetVisibiltyForAllTitleElements(this.ShowTitleBar);

            this.contentPresenter = this.GetTemplateChild(PART_Window_Content) as ContentPresenter;

            this.DialogManager = new WpfDialogs.DialogManager(this.DialogsContainer, this.Dispatcher, this.contentPresenter);
        }

        /// <summary>
        /// Method connects the <see cref="Thumb"/> object on the window chrome
        /// with the correct drag events to let user drag the window on the screen.
        /// </summary>
        /// <param name="windowTitleThumb"></param>
        public void SetWindowEvents(Thumb windowTitleThumb)
        {
            this.ClearWindowEvents(); // clear all event handlers first

            if (windowTitleThumb != null)
            {
                windowTitleThumb.PreviewMouseLeftButtonUp += this.WindowTitleThumbOnPreviewMouseLeftButtonUp;
                windowTitleThumb.DragDelta += this.WindowTitleThumbMoveOnDragDelta;
                windowTitleThumb.MouseDoubleClick += this.WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
                windowTitleThumb.MouseRightButtonUp += this.WindowTitleThumbSystemMenuOnMouseRightButtonUp;

                // Replace old referenc to Thumb since we seem to be looking at another instance
                // This could be a Thumb that overlays the window, for example, in a view
                // So, the Thumb may live in a view placed into the window to support dragging from there.
                if (this.windowTitleThumb != windowTitleThumb)
                {
                    this.windowTitleThumb = windowTitleThumb;
                }
            }
        }

        private void SetVisibiltyForAllTitleElements(bool visible)
        {
            this.SetWindowEvents(this.windowTitleThumb);
        }

        private void ClearWindowEvents()
        {
            // clear all event handlers first:
            if (this.windowTitleThumb != null)
            {
                this.windowTitleThumb.PreviewMouseLeftButtonUp -= this.WindowTitleThumbOnPreviewMouseLeftButtonUp;
                this.windowTitleThumb.DragDelta -= this.WindowTitleThumbMoveOnDragDelta;
                this.windowTitleThumb.MouseDoubleClick -= this.WindowTitleThumbChangeWindowStateOnMouseDoubleClick;
                this.windowTitleThumb.MouseRightButtonUp -= this.WindowTitleThumbSystemMenuOnMouseRightButtonUp;
            }
        }

        /// <summary>
        /// Gets the template child with the given name.
        /// </summary>
        /// <typeparam name="T">The interface type inheirted from DependencyObject.</typeparam>
        /// <param name="name">The name of the template child.</param>
        internal T GetPart<T>(string name)
            where T : class
        {
            return this.GetTemplateChild(name) as T;
        }

        /// <summary>
        /// Gets the template child with the given name.
        /// </summary>
        /// <param name="name">The name of the template child.</param>
        internal DependencyObject GetPart(string name)
        {
            return this.GetTemplateChild(name);
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            MS.Windows.Shell.SystemCommands.CloseWindow(this);
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            MS.Windows.Shell.SystemCommands.MaximizeWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            MS.Windows.Shell.SystemCommands.MinimizeWindow(this);
        }

        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            MS.Windows.Shell.SystemCommands.RestoreWindow(this);
        }

        #region WindowTitleThumbEvents
        internal static void DoWindowTitleThumbOnPreviewMouseLeftButtonUp(WindowWithDialogs window, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (mouseButtonEventArgs.Source == mouseButtonEventArgs.OriginalSource)
            {
                Mouse.Capture(null);
            }
        }

        internal static void DoWindowTitleThumbMoveOnDragDelta(Thumb thumb, WindowWithDialogs window, DragDeltaEventArgs dragDeltaEventArgs)
        {
            if (thumb == null)
            {
                throw new ArgumentNullException(nameof(thumb));
            }

            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            // drag only if IsWindowDraggable is set to true
            if (!window.IsWindowDraggable ||
                (!(Math.Abs(dragDeltaEventArgs.HorizontalChange) > 2) && !(Math.Abs(dragDeltaEventArgs.VerticalChange) > 2)))
            {
                return;
            }

            // tage from DragMove internal code
            window.VerifyAccess();

            var cursorPos = NativeMethods.GetCursorPos();

            // if the window is maximized dragging is only allowed on title bar (also if not visible)
            var windowIsMaximized = window.WindowState == WindowState.Maximized;
            ////var isMouseOnTitlebar = Mouse.GetPosition(thumb).Y <= window.TitlebarHeight && window.TitlebarHeight > 0;
            ////if (!isMouseOnTitlebar && windowIsMaximized)
            ////{
            ////    return;
            ////}

            // for the touch usage
            UnsafeNativeMethods.ReleaseCapture();

            if (windowIsMaximized)
            {
                var cursorXPos = cursorPos.X;
                EventHandler windowOnStateChanged = null;
                windowOnStateChanged = (sender, args) =>
                {
                    // window.Top = 2;
                    // window.Left = Math.Max(cursorXPos - window.RestoreBounds.Width / 2, 0);
                    window.StateChanged -= windowOnStateChanged;
                    if (window.WindowState == WindowState.Normal)
                    {
                        Mouse.Capture(thumb, CaptureMode.Element);
                    }
                };
                window.StateChanged += windowOnStateChanged;
            }

            var criticalHandle = window.CriticalHandle;

            // DragMove works too
            // window.DragMove();
            // instead this 2 lines
            NativeMethods.SendMessage(criticalHandle, WM.SYSCOMMAND, (IntPtr)SC.MOUSEMOVE, IntPtr.Zero);
            NativeMethods.SendMessage(criticalHandle, WM.LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
        }

        internal static void DoWindowTitleThumbChangeWindowStateOnMouseDoubleClick(WindowWithDialogs window, MouseButtonEventArgs mouseButtonEventArgs)
        {
            // restore/maximize only with left button
            if (mouseButtonEventArgs.ChangedButton == MouseButton.Left)
            {
                // we can maximize or restore the window if the title bar height is set (also if title bar is hidden)
                var canResize = window.ResizeMode == ResizeMode.CanResizeWithGrip || window.ResizeMode == ResizeMode.CanResize;
                var isMouseOnTitlebar = true; //// var isMouseOnTitlebar = mousePos.Y <= window.TitlebarHeight && window.TitlebarHeight > 0;
                if (canResize && isMouseOnTitlebar)
                {
                    if (window.WindowState == WindowState.Maximized)
                    {
                        MS.Windows.Shell.SystemCommands.RestoreWindow(window);
                    }
                    else
                    {
                        MS.Windows.Shell.SystemCommands.MaximizeWindow(window);
                    }

                    mouseButtonEventArgs.Handled = true;
                }
            }
        }

        private static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
        {
            if (window == null)
            {
                return;
            }

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero || !UnsafeNativeMethods.IsWindow(hwnd))
            {
                return;
            }

            var hmenu = UnsafeNativeMethods.GetSystemMenu(hwnd, false);

            var cmd = UnsafeNativeMethods.TrackPopupMenuEx(hmenu, Constants.TPM_LEFTBUTTON | Constants.TPM_RETURNCMD,
                (int)physicalScreenLocation.X, (int)physicalScreenLocation.Y, hwnd, IntPtr.Zero);
            if (0 != cmd)
            {
                UnsafeNativeMethods.PostMessage(hwnd, Constants.SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
            }
        }

        internal static void DoWindowTitleThumbSystemMenuOnMouseRightButtonUp(WindowWithDialogs window, MouseButtonEventArgs e)
        {
            if (window.ShowSystemMenuOnRightClick)
            {
                // show menu only if mouse pos is on title bar or if we have a window with none style and no title bar
                var mousePos = e.GetPosition(window);
                ////if ((mousePos.Y <= window.TitlebarHeight && window.TitlebarHeight > 0) || (window.UseNoneWindowStyle && window.TitlebarHeight <= 0))
                ////{
                ShowSystemMenuPhysicalCoordinates(window, window.PointToScreen(mousePos));
                ////}
            }
        }

        private void WindowTitleThumbOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DoWindowTitleThumbOnPreviewMouseLeftButtonUp(this, e);
        }

        private void WindowTitleThumbMoveOnDragDelta(object sender, DragDeltaEventArgs dragDeltaEventArgs)
        {
            DoWindowTitleThumbMoveOnDragDelta(sender as Thumb, this, dragDeltaEventArgs);
        }

        private void WindowTitleThumbChangeWindowStateOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            DoWindowTitleThumbChangeWindowStateOnMouseDoubleClick(this, mouseButtonEventArgs);
        }

        private void WindowTitleThumbSystemMenuOnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DoWindowTitleThumbSystemMenuOnMouseRightButtonUp(this, e);
        }
        #endregion WindowTitleThumbEvents

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

            EventHandler completionHandler = null;
            completionHandler = (sender, args) =>
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
            };

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

            EventHandler completionHandler = null;
            completionHandler = (sender, args) =>
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
            };

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
                caption = this.Title;

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
                throw new ArgumentNullException("DialogManager must be not null!");
            }
#if Windows
            System.Media.SystemSounds.Hand.Play();
#endif
            if (string.IsNullOrEmpty(caption))
                caption = this.Title;

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
                caption = this.Title;

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
                caption = this.Title;

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
                throw new ArgumentNullException("DialogManager must be not null!");
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
