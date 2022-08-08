using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TMP.Native;
using TMP.Wpf.CommonControls.Dialogs;

namespace TMP.Wpf.CommonControls
{
    using System.Threading.Tasks;

    /// <summary>
    /// Расширенный класс окна
    /// </summary>
    [TemplatePart(Name = PART_Icon, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_TitleBar, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_WindowTitleBackground, Type = typeof(UIElement))]
    [TemplatePart(Name = PART_WindowButtonCommands, Type = typeof(WindowButtonCommands))]
    [TemplatePart(Name = PART_OverlayBox, Type = typeof(Grid))]
    [TemplatePart(Name = PART_TMPDialogContainer, Type = typeof(Grid))]
    public class TMPWindow : Window
    {
        private const string PART_Icon = "PART_Icon";
        private const string PART_TitleBar = "PART_TitleBar";
        private const string PART_WindowTitleBackground = "PART_WindowTitleBackground";
        private const string PART_WindowButtonCommands = "PART_WindowButtonCommands";
        private const string PART_OverlayBox = "PART_OverlayBox";
        private const string PART_TMPDialogContainer = "PART_TMPDialogContainer";

        #region DependencyProperty

        public static readonly DependencyProperty ShowIconOnTitleBarProperty = DependencyProperty.Register("ShowIconOnTitleBar", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty IconEdgeModeProperty = DependencyProperty.Register("IconEdgeMode", typeof(EdgeMode), typeof(TMPWindow), new PropertyMetadata(EdgeMode.Aliased));
        public static readonly DependencyProperty IconBitmapScalingModeProperty = DependencyProperty.Register("IconBitmapScalingMode", typeof(BitmapScalingMode), typeof(TMPWindow), new PropertyMetadata(BitmapScalingMode.HighQuality));
        public static readonly DependencyProperty ShowTitleBarProperty = DependencyProperty.Register("ShowTitleBar", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true, OnShowTitleBarPropertyChangedCallback, OnShowTitleBarCoerceValueCallback));

        public static readonly DependencyProperty ShowMinButtonProperty = DependencyProperty.Register("ShowMinButton", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowMaxRestoreButtonProperty = DependencyProperty.Register("ShowMaxRestoreButton", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));

        public static readonly DependencyProperty IsMinButtonEnabledProperty = DependencyProperty.Register("IsMinButtonEnabled", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty IsMaxRestoreButtonEnabledProperty = DependencyProperty.Register("IsMaxRestoreButtonEnabled", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty IsCloseButtonEnabledProperty = DependencyProperty.Register("IsCloseButtonEnabled", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowSystemMenuOnRightClickProperty = DependencyProperty.Register("ShowSystemMenuOnRightClick", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));

        public static readonly DependencyProperty TitlebarHeightProperty = DependencyProperty.Register("TitlebarHeight", typeof(int), typeof(TMPWindow), new PropertyMetadata(30, TitlebarHeightPropertyChangedCallback));
        public static readonly DependencyProperty TitleCapsProperty = DependencyProperty.Register("TitleCaps", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register("TitleForeground", typeof(Brush), typeof(TMPWindow));
        public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty = DependencyProperty.Register("IgnoreTaskbarOnMaximize", typeof(bool), typeof(TMPWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty TMPDialogOptionsProperty = DependencyProperty.Register("TMPDialogOptions", typeof(TMPDialogSettings), typeof(TMPWindow), new PropertyMetadata(new TMPDialogSettings()));
        public static readonly DependencyProperty ToggleFullScreenProperty = DependencyProperty.Register("ToggleFullScreen", typeof(bool), typeof(TMPWindow), new PropertyMetadata(default(bool), ToggleFullScreenPropertyChangedCallback));

        public static readonly DependencyProperty WindowTitleBrushProperty = DependencyProperty.Register("WindowTitleBrush", typeof(Brush), typeof(TMPWindow), new PropertyMetadata(Brushes.Transparent));
        public static readonly DependencyProperty GlowBrushProperty = DependencyProperty.Register("GlowBrush", typeof(SolidColorBrush), typeof(TMPWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty NonActiveGlowBrushProperty = DependencyProperty.Register("NonActiveGlowBrush", typeof(SolidColorBrush), typeof(TMPWindow), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(153, 153, 153)))); // #999999
        public static readonly DependencyProperty NonActiveBorderBrushProperty = DependencyProperty.Register("NonActiveBorderBrush", typeof(Brush), typeof(TMPWindow), new PropertyMetadata(Brushes.Gray));
        public static readonly DependencyProperty NonActiveWindowTitleBrushProperty = DependencyProperty.Register("NonActiveWindowTitleBrush", typeof(Brush), typeof(TMPWindow), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.Register("IconTemplate", typeof(DataTemplate), typeof(TMPWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty TitleTemplateProperty = DependencyProperty.Register("TitleTemplate", typeof(DataTemplate), typeof(TMPWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty IconOverlayBehaviorProperty = DependencyProperty.Register("IconOverlayBehavior", typeof(WindowCommandsOverlayBehavior), typeof(TMPWindow), new PropertyMetadata(WindowCommandsOverlayBehavior.Never));
        public static readonly DependencyProperty WindowButtonCommandsOverlayBehaviorProperty = DependencyProperty.Register("WindowButtonCommandsOverlayBehavior", typeof(WindowCommandsOverlayBehavior), typeof(TMPWindow), new PropertyMetadata(WindowCommandsOverlayBehavior.Always));

        public static readonly DependencyProperty WindowMinButtonStyleProperty = DependencyProperty.Register("WindowMinButtonStyle", typeof(Style), typeof(TMPWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty WindowMaxButtonStyleProperty = DependencyProperty.Register("WindowMaxButtonStyle", typeof(Style), typeof(TMPWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty WindowCloseButtonStyleProperty = DependencyProperty.Register("WindowCloseButtonStyle", typeof(Style), typeof(TMPWindow), new PropertyMetadata(null));

        public static readonly DependencyProperty UseNoneWindowStyleProperty = DependencyProperty.Register("UseNoneWindowStyle", typeof(bool), typeof(TMPWindow), new PropertyMetadata(false, OnUseNoneWindowStylePropertyChangedCallback));
        public static readonly DependencyProperty OverrideDefaultWindowCommandsBrushProperty = DependencyProperty.Register("OverrideDefaultWindowCommandsBrush", typeof(SolidColorBrush), typeof(TMPWindow));

        public static readonly DependencyProperty EnableDWMDropShadowProperty = DependencyProperty.Register("EnableDWMDropShadow", typeof(bool), typeof(TMPWindow), new PropertyMetadata(false));
        public static readonly DependencyProperty IsWindowDraggableProperty = DependencyProperty.Register("IsWindowDraggable", typeof(bool), typeof(TMPWindow), new PropertyMetadata(true));

        #endregion DependencyProperty

        #region DependencyProperty CallBacks

        private static void OnShowTitleBarPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (TMPWindow)d;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibiltyForAllTitleElements((bool)e.NewValue);
            }
        }

        private static object OnShowTitleBarCoerceValueCallback(DependencyObject d, object value)
        {
            // if UseNoneWindowStyle = true no title bar should be shown
            if (((TMPWindow)d).UseNoneWindowStyle)
            {
                return false;
            }
            return value;
        }

        private static void TitlebarHeightPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = (TMPWindow)dependencyObject;
            if (e.NewValue != e.OldValue)
            {
                window.SetVisibiltyForAllTitleElements((int)e.NewValue > 0);
            }
        }

        private static void OnUseNoneWindowStylePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                // if UseNoneWindowStyle = true no title bar should be shown
                var useNoneWindowStyle = (bool)e.NewValue;
                var window = (TMPWindow)d;
                window.ToggleNoneWindowStyle(useNoneWindowStyle);
            }
        }

        private static void ToggleFullScreenPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var TMPWindow = (TMPWindow)dependencyObject;
            if (e.OldValue != e.NewValue)
            {
                var fullScreen = (bool)e.NewValue;
                if (fullScreen)
                {
                    TMPWindow.UseNoneWindowStyle = true;
                    TMPWindow.IgnoreTaskbarOnMaximize = true;
                    TMPWindow.WindowState = WindowState.Maximized;
                }
                else
                {
                    TMPWindow.UseNoneWindowStyle = false;
                    TMPWindow.ShowTitleBar = true; // <-- this must be set to true
                    TMPWindow.IgnoreTaskbarOnMaximize = false;
                    TMPWindow.WindowState = WindowState.Normal;
                }
            }
        }

        #endregion DependencyProperty CallBacks

        private UIElement icon;
        private UIElement titleBar;
        private UIElement titleBarBackground;

        internal WindowButtonCommands WindowButtonCommands;

        internal Grid overlayBox;
        internal Grid tmpDialogContainer;
        private Storyboard overlayStoryboard;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the TMP.Wpf.CommonControls.TMPWindow class.
        /// </summary>
        public TMPWindow()
        {
            Loaded += this.TMPWindow_Loaded;
        }

        static TMPWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TMPWindow), new FrameworkPropertyMetadata(typeof(TMPWindow)));
        }

        #endregion Ctor

        #region Private Methods

        private void TMPWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.ToggleNoneWindowStyle(this.UseNoneWindowStyle);

            this.ResetAllWindowCommandsBrush();

            ThemeManager.IsThemeChanged += ThemeManagerOnIsThemeChanged;
            this.Unloaded += (o, args) => ThemeManager.IsThemeChanged -= ThemeManagerOnIsThemeChanged;
        }

        private void ThemeManagerOnIsThemeChanged(object sender, OnThemeChangedEventArgs e)
        {
            if (e.Accent != null)
            {
                ;
            }
        }

        private void SetVisibiltyForAllTitleElements(bool visible)
        {
            var newVisibility = visible && this.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
            if (this.icon != null)
            {
                var iconVisibility = this.IconOverlayBehavior.HasFlag(WindowCommandsOverlayBehavior.HiddenTitleBar) && !this.ShowTitleBar
                    || this.ShowIconOnTitleBar && this.ShowTitleBar ? Visibility.Visible : Visibility.Collapsed;
                this.icon.Visibility = iconVisibility;
            }
            if (this.titleBar != null)
            {
                this.titleBar.Visibility = newVisibility;
            }
            if (this.titleBarBackground != null)
            {
                this.titleBarBackground.Visibility = newVisibility;
            }
            if (this.WindowButtonCommands != null)
            {
                this.WindowButtonCommands.Visibility = this.WindowButtonCommandsOverlayBehavior.HasFlag(WindowCommandsOverlayBehavior.HiddenTitleBar) ?
                    Visibility.Visible : newVisibility;
            }

            SetWindowEvents();
        }

        private void ToggleNoneWindowStyle(bool useNoneWindowStyle)
        {
            // UseNoneWindowStyle means no title bar, window commands or min, max, close buttons
            if (useNoneWindowStyle)
            {
                ShowTitleBar = false;
            }
        }

        private void TMPWindow_SizeChanged(object sender, RoutedEventArgs e)
        {
            // this all works only for CleanWindow style

            var titleBarGrid = (Grid)titleBar;
            var titleBarLabel = (Label)titleBarGrid.Children[0];
            var titleControl = (ContentControl)titleBarLabel.Content;
            var iconContentControl = (ContentControl)icon;

            // Half of this TMPWindow
            var halfDistance = this.Width / 2;
            // Distance between center and left/right
            var distanceToCenter = titleControl.ActualWidth / 2;
            // Distance between right edge from LeftWindowCommands to left window side
            var distanceFromLeft = iconContentControl.ActualWidth;
            // Distance between left edge from RightWindowCommands to right window side
            var distanceFromRight = WindowButtonCommands.ActualWidth;
            // Margin
            const double horizontalMargin = 5.0;

            if ((distanceFromLeft + distanceToCenter + horizontalMargin < halfDistance) && (distanceFromRight + distanceToCenter + horizontalMargin < halfDistance))
            {
                Grid.SetColumn(titleBarGrid, 0);
                Grid.SetColumnSpan(titleBarGrid, 5);
            }
            else
            {
                Grid.SetColumn(titleBarGrid, 2);
                Grid.SetColumnSpan(titleBarGrid, 1);
            }
        }

        private void ClearWindowEvents()
        {
            // clear all event handlers first:
            if (titleBarBackground != null)
            {
                titleBarBackground.MouseDown -= TitleBarMouseDown;
                titleBarBackground.MouseUp -= TitleBarMouseUp;
            }
            if (titleBar != null)
            {
                titleBar.MouseDown -= TitleBarMouseDown;
                titleBar.MouseUp -= TitleBarMouseUp;
            }
            if (icon != null)
            {
                icon.MouseDown -= IconMouseDown;
            }
            MouseDown -= TitleBarMouseDown;
            MouseUp -= TitleBarMouseUp;
            SizeChanged -= TMPWindow_SizeChanged;
        }

        private void SetWindowEvents()
        {
            // clear all event handlers first
            this.ClearWindowEvents();

            // set mouse down/up for icon
            if (icon != null && icon.Visibility == Visibility.Visible)
            {
                icon.MouseDown += IconMouseDown;
            }

            // handle mouse events for PART_WindowTitleBackground -> TMPWindow
            if (titleBarBackground != null && titleBarBackground.Visibility == Visibility.Visible)
            {
                titleBarBackground.MouseDown += TitleBarMouseDown;
                titleBarBackground.MouseUp += TitleBarMouseUp;
            }

            // handle mouse events for PART_TitleBar -> TMPWindow and CleanWindow
            if (titleBar != null && titleBar.Visibility == Visibility.Visible)
            {
                titleBar.MouseDown += TitleBarMouseDown;
                titleBar.MouseUp += TitleBarMouseUp;

                // special title resizing for CleanWindow title
                if (titleBar.GetType() == typeof(Grid))
                {
                    SizeChanged += TMPWindow_SizeChanged;
                }
            }
            else
            {
                // handle mouse events for windows without PART_WindowTitleBackground or PART_TitleBar
                MouseDown += TitleBarMouseDown;
                MouseUp += TitleBarMouseUp;
            }
        }

        private void IconMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    Close();
                }
                else
                {
                    ShowSystemMenuPhysicalCoordinates(this, PointToScreen(new Point(0, TitlebarHeight)));
                }
            }
        }

        #endregion Private Methods

        #region Protected Methods

        protected void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            // if UseNoneWindowStyle = true no movement, no maximize please
            if (e.ChangedButton == MouseButton.Left && !this.UseNoneWindowStyle)
            {
                var mPoint = Mouse.GetPosition(this);

                if (IsWindowDraggable)
                {
                    IntPtr windowHandle = new WindowInteropHelper(this).Handle;
                    UnsafeNativeMethods.ReleaseCapture();
                    var wpfPoint = this.PointToScreen(mPoint);
                    var x = Convert.ToInt16(wpfPoint.X);
                    var y = Convert.ToInt16(wpfPoint.Y);
                    var lParam = (int)(uint)x | (y << 16);
                    UnsafeNativeMethods.SendMessage(windowHandle, Constants.WM_NCLBUTTONDOWN, Constants.HT_CAPTION, lParam);
                }

                var canResize = this.ResizeMode == ResizeMode.CanResizeWithGrip || this.ResizeMode == ResizeMode.CanResize;
                // we can maximize or restore the window if the title bar height is set (also if title bar is hidden)
                var isMouseOnTitlebar = mPoint.Y <= this.TitlebarHeight && this.TitlebarHeight > 0;
                if (e.ClickCount == 2 && canResize && isMouseOnTitlebar)
                {
                    if (this.WindowState == WindowState.Maximized)
                    {
                        MS.Windows.Shell.SystemCommands.RestoreWindow(this);
                    }
                    else
                    {
                        MS.Windows.Shell.SystemCommands.MaximizeWindow(this);
                    }
                }
            }
        }

        protected void TitleBarMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ShowSystemMenuOnRightClick)
            {
                var mousePosition = e.GetPosition(this);
                if (e.ChangedButton == MouseButton.Right && (UseNoneWindowStyle || mousePosition.Y <= TitlebarHeight))
                {
                    ShowSystemMenuPhysicalCoordinates(this, PointToScreen(mousePosition));
                }
            }
        }

        #endregion Protected Methods

        #region Private Static Methods

        private static void ShowSystemMenuPhysicalCoordinates(Window window, Point physicalScreenLocation)
        {
            if (window == null) return;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero || !UnsafeNativeMethods.IsWindow(hwnd))
                return;

            var hmenu = UnsafeNativeMethods.GetSystemMenu(hwnd, false);

            var cmd = UnsafeNativeMethods.TrackPopupMenuEx(hmenu, Constants.TPM_LEFTBUTTON | Constants.TPM_RETURNCMD,
                (int)physicalScreenLocation.X, (int)physicalScreenLocation.Y, hwnd, IntPtr.Zero);
            if (0 != cmd)
                UnsafeNativeMethods.PostMessage(hwnd, Constants.SYSCOMMAND, new IntPtr(cmd), IntPtr.Zero);
        }

        #endregion Private Static Methods

        #region Public Methods

        /// <summary>
        /// Ожидает пока окно не будет готово к взаимодействию
        /// </summary>
        /// <returns>Задача представляющая операцию и ее статус</returns>
        public System.Threading.Tasks.Task WaitForLoadAsync()
        {
            Dispatcher.VerifyAccess();

            if (this.IsLoaded) return new System.Threading.Tasks.Task(() => { });

            System.Threading.Tasks.TaskCompletionSource<object> tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

            RoutedEventHandler handler = null;
            handler = (sender, args) =>
            {
                this.Loaded -= handler;

                this.Focus();

                tcs.TrySetResult(null);
            };

            this.Loaded += handler;

            return tcs.Task;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            WindowButtonCommands = GetTemplateChild(PART_WindowButtonCommands) as WindowButtonCommands;

            overlayBox = GetTemplateChild(PART_OverlayBox) as Grid;
            tmpDialogContainer = GetTemplateChild(PART_TMPDialogContainer) as Grid;

            icon = GetTemplateChild(PART_Icon) as UIElement;
            titleBar = GetTemplateChild(PART_TitleBar) as UIElement;
            titleBarBackground = GetTemplateChild(PART_WindowTitleBackground) as UIElement;

            this.SetVisibiltyForAllTitleElements(this.TitlebarHeight > 0);
        }

        /// <summary>
        /// Begins to show the TMPWindow's overlay effect.
        /// </summary>
        /// <returns>A task representing the process.</returns>
        public System.Threading.Tasks.Task ShowOverlayAsync()
        {
            if (overlayBox == null) throw new InvalidOperationException("OverlayBox can not be founded in this TMPWindow's template. Are you calling this before the window has loaded?");

            if (IsOverlayVisible() && overlayStoryboard == null)
                return new System.Threading.Tasks.Task(() => { }); //No Task.FromResult in .NET 4.

            Dispatcher.VerifyAccess();

            overlayBox.Visibility = Visibility.Visible;

            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

            var sb = (Storyboard)this.Template.Resources["OverlayFastSemiFadeIn"];

            sb = sb.Clone();

            EventHandler completionHandler = null;
            completionHandler = (sender, args) =>
            {
                sb.Completed -= completionHandler;

                if (overlayStoryboard == sb)
                {
                    overlayStoryboard = null;
                }

                tcs.TrySetResult(null);
            };

            sb.Completed += completionHandler;

            overlayBox.BeginStoryboard(sb);

            overlayStoryboard = sb;

            return tcs.Task;
        }

        /// <summary>
        /// Begins to hide the TMPWindow's overlay effect.
        /// </summary>
        /// <returns>A task representing the process.</returns>
        public System.Threading.Tasks.Task HideOverlayAsync()
        {
            if (overlayBox == null) throw new InvalidOperationException("OverlayBox can not be founded in this TMPWindow's template. Are you calling this before the window has loaded?");

            if (overlayBox.Visibility == Visibility.Visible && overlayBox.Opacity == 0.0)
                return new System.Threading.Tasks.Task(() => { }); //No Task.FromResult in .NET 4.

            Dispatcher.VerifyAccess();

            var tcs = new System.Threading.Tasks.TaskCompletionSource<object>();

            var sb = (Storyboard)this.Template.Resources["OverlayFastSemiFadeOut"];

            sb = sb.Clone();

            EventHandler completionHandler = null;
            completionHandler = (sender, args) =>
            {
                sb.Completed -= completionHandler;

                if (overlayStoryboard == sb)
                {
                    overlayBox.Visibility = Visibility.Hidden;
                    overlayStoryboard = null;
                }

                tcs.TrySetResult(null);
            };

            sb.Completed += completionHandler;

            overlayBox.BeginStoryboard(sb);

            overlayStoryboard = sb;

            return tcs.Task;
        }
        public bool IsOverlayVisible()
        {
            if (overlayBox == null) throw new InvalidOperationException("OverlayBox can not be founded in this TMPWindow's template. Are you calling this before the window has loaded?");

            return overlayBox.Visibility == Visibility.Visible && overlayBox.Opacity >= 0.7;
        }

        public void ShowOverlay()
        {
            overlayBox.Visibility = Visibility.Visible;
            //overlayBox.Opacity = 0.7;
            overlayBox.SetCurrentValue(Grid.OpacityProperty, 0.7);
        }

        public void HideOverlay()
        {
            //overlayBox.Opacity = 0.0;
            overlayBox.SetCurrentValue(Grid.OpacityProperty, 0.0);
            overlayBox.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion Public Methods

        #region Properties

        public SolidColorBrush OverrideDefaultWindowCommandsBrush
        {
            get => (SolidColorBrush)this.GetValue(OverrideDefaultWindowCommandsBrushProperty);
            set => this.SetValue(OverrideDefaultWindowCommandsBrushProperty, value);
        }

        public bool EnableDWMDropShadow
        {
            get => (bool)GetValue(EnableDWMDropShadowProperty);
            set => SetValue(EnableDWMDropShadowProperty, value);
        }

        public bool IsWindowDraggable
        {
            get => (bool)GetValue(IsWindowDraggableProperty);
            set => SetValue(IsWindowDraggableProperty, value);
        }

        public TMPDialogSettings TMPDialogOptions
        {
            get => (TMPDialogSettings)GetValue(TMPDialogOptionsProperty);
            set => SetValue(TMPDialogOptionsProperty, value);
        }

        public WindowCommandsOverlayBehavior IconOverlayBehavior
        {
            get => (WindowCommandsOverlayBehavior)this.GetValue(IconOverlayBehaviorProperty);
            set => SetValue(IconOverlayBehaviorProperty, value);
        }

        /// <summary>
        /// Gets/sets the style for the MIN button style.
        /// </summary>
        public Style WindowMinButtonStyle
        {
            get => (Style)this.GetValue(WindowMinButtonStyleProperty);
            set => SetValue(WindowMinButtonStyleProperty, value);
        }

        /// <summary>
        /// Gets/sets the style for the MAX button style.
        /// </summary>
        public Style WindowMaxButtonStyle
        {
            get => (Style)this.GetValue(WindowMaxButtonStyleProperty);
            set => SetValue(WindowMaxButtonStyleProperty, value);
        }

        /// <summary>
        /// Gets/sets the style for the CLOSE button style.
        /// </summary>
        public Style WindowCloseButtonStyle
        {
            get => (Style)this.GetValue(WindowCloseButtonStyleProperty);
            set => SetValue(WindowCloseButtonStyleProperty, value);
        }

        public WindowCommandsOverlayBehavior WindowButtonCommandsOverlayBehavior
        {
            get => (WindowCommandsOverlayBehavior)this.GetValue(WindowButtonCommandsOverlayBehaviorProperty);
            set => SetValue(WindowButtonCommandsOverlayBehaviorProperty, value);
        }

        /// <summary>
        /// Gets/sets if the the system menu should popup on right click.
        /// </summary>
        public bool ShowSystemMenuOnRightClick
        {
            get => (bool)GetValue(ShowSystemMenuOnRightClickProperty);
            set => SetValue(ShowSystemMenuOnRightClickProperty, value);
        }

        public bool ShowIconOnTitleBar
        {
            get => (bool)GetValue(ShowIconOnTitleBarProperty);
            set => SetValue(ShowIconOnTitleBarProperty, value);
        }

        /// <summary>
        /// Gets/sets the icon content template to show a custom icon.
        /// </summary>
        public DataTemplate IconTemplate
        {
            get => (DataTemplate)GetValue(IconTemplateProperty);
            set => SetValue(IconTemplateProperty, value);
        }

        /// <summary>
        /// Gets/sets the title content template to show a custom title.
        /// </summary>
        public DataTemplate TitleTemplate
        {
            get => (DataTemplate)GetValue(TitleTemplateProperty);
            set => SetValue(TitleTemplateProperty, value);
        }

        /// <summary>
        /// Gets/sets whether the window will ignore (and overlap) the taskbar when maximized.
        /// </summary>
        public bool IgnoreTaskbarOnMaximize
        {
            get => (bool)this.GetValue(IgnoreTaskbarOnMaximizeProperty);
            set => SetValue(IgnoreTaskbarOnMaximizeProperty, value);
        }

        /// <summary>
        /// Gets/sets the brush used for the titlebar's foreground.
        /// </summary>
        public Brush TitleForeground
        {
            get => (Brush)GetValue(TitleForegroundProperty);
            set => SetValue(TitleForegroundProperty, value);
        }

        /// <summary>
        /// Gets/sets edge mode of the titlebar icon.
        /// </summary>
        public EdgeMode IconEdgeMode
        {
            get => (EdgeMode)this.GetValue(IconEdgeModeProperty);
            set => SetValue(IconEdgeModeProperty, value);
        }

        /// <summary>
        /// Gets/sets bitmap scaling mode of the titlebar icon.
        /// </summary>
        public BitmapScalingMode IconBitmapScalingMode
        {
            get => (BitmapScalingMode)this.GetValue(IconBitmapScalingModeProperty);
            set => SetValue(IconBitmapScalingModeProperty, value);
        }

        public bool ShowTitleBar
        {
            get => (bool)GetValue(ShowTitleBarProperty);
            set => SetValue(ShowTitleBarProperty, value);
        }

        /// <summary>
        /// Gets/sets the TitleBar's height.
        /// </summary>
        public int TitlebarHeight
        {
            get => (int)GetValue(TitlebarHeightProperty);
            set => SetValue(TitlebarHeightProperty, value);
        }

        /// <summary>
        /// Gets/sets whether the WindowStyle is None or not.
        /// </summary>
        public bool UseNoneWindowStyle
        {
            get => (bool)GetValue(UseNoneWindowStyleProperty);
            set => SetValue(UseNoneWindowStyleProperty, value);
        }

        /// <summary>
        /// Gets/sets if the minimize button is visible.
        /// </summary>
        public bool ShowMinButton
        {
            get => (bool)GetValue(ShowMinButtonProperty);
            set => SetValue(ShowMinButtonProperty, value);
        }

        /// <summary>
        /// Gets/sets if the Maximize/Restore button is visible.
        /// </summary>
        public bool ShowMaxRestoreButton
        {
            get => (bool)GetValue(ShowMaxRestoreButtonProperty);
            set => SetValue(ShowMaxRestoreButtonProperty, value);
        }

        /// <summary>
        /// Gets/sets if the close button is visible.
        /// </summary>
        public bool ShowCloseButton
        {
            get => (bool)GetValue(ShowCloseButtonProperty);
            set => SetValue(ShowCloseButtonProperty, value);
        }

        /// <summary>
        /// Gets/sets if the min button is enabled.
        /// </summary>
        public bool IsMinButtonEnabled
        {
            get => (bool)GetValue(IsMinButtonEnabledProperty);
            set => SetValue(IsMinButtonEnabledProperty, value);
        }

        /// <summary>
        /// Gets/sets if the max/restore button is enabled.
        /// </summary>
        public bool IsMaxRestoreButtonEnabled
        {
            get => (bool)GetValue(IsMaxRestoreButtonEnabledProperty);
            set => SetValue(IsMaxRestoreButtonEnabledProperty, value);
        }

        /// <summary>
        /// Gets/sets if the close button is enabled.
        /// </summary>
        public bool IsCloseButtonEnabled
        {
            get => (bool)GetValue(IsCloseButtonEnabledProperty);
            set => SetValue(IsCloseButtonEnabledProperty, value);
        }

        /// <summary>
        /// Gets/sets if the TitleBar's text is automatically capitalized.
        /// </summary>
        public bool TitleCaps
        {
            get => (bool)GetValue(TitleCapsProperty);
            set => SetValue(TitleCapsProperty, value);
        }

        /// <summary>
        /// Gets/sets the brush used for the Window's title bar.
        /// </summary>
        public Brush WindowTitleBrush
        {
            get => (Brush)GetValue(WindowTitleBrushProperty);
            set => SetValue(WindowTitleBrushProperty, value);
        }

        /// <summary>
        /// Gets/sets the brush used for the Window's glow.
        /// </summary>
        public SolidColorBrush GlowBrush
        {
            get => (SolidColorBrush)GetValue(GlowBrushProperty);
            set => SetValue(GlowBrushProperty, value);
        }

        /// <summary>
        /// Gets/sets the brush used for the Window's non-active glow.
        /// </summary>
        public SolidColorBrush NonActiveGlowBrush
        {
            get => (SolidColorBrush)GetValue(NonActiveGlowBrushProperty);
            set => SetValue(NonActiveGlowBrushProperty, value);
        }

        /// <summary>
        /// Gets/sets the brush used for the Window's non-active border.
        /// </summary>
        public Brush NonActiveBorderBrush
        {
            get => (Brush)GetValue(NonActiveBorderBrushProperty);
            set => SetValue(NonActiveBorderBrushProperty, value);
        }

        /// <summary>
        /// Gets/sets the brush used for the Window's non-active title bar.
        /// </summary>
        public Brush NonActiveWindowTitleBrush
        {
            get => (Brush)GetValue(NonActiveWindowTitleBrushProperty);
            set => SetValue(NonActiveWindowTitleBrushProperty, value);
        }

        /// <summary>
        /// Gets/sets the TitleBar/Window's Text.
        /// </summary>
        public string WindowTitle
        {
            get { return TitleCaps ? Title.ToUpper() : Title; }
        }

        public bool ToggleFullScreen
        {
            get => (bool)GetValue(ToggleFullScreenProperty);
            set => SetValue(ToggleFullScreenProperty, value);
        }

        #endregion Properties

        #region Dialog Manager

        public Task<MessageDialogResult> ShowMessage(string title, string message, TMPDialogSettings settings = null)
        {
            if (!this.IsOverlayVisible())
                this.ShowOverlay();

            //create the dialog control
            var dialog = new MessageDialog(this, settings)
            {
                Message = message,
                Title = title,
                ButtonStyle = MessageDialogStyle.Affirmative
            };

            SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(dialog);
            dialog.SizeChangedHandler = sizeHandler;

            EventHandler closeHandler = null;

            closeHandler = (sender, e) =>
            {
                dialog.CloseHandler -= closeHandler;

                this.Dispatcher.Invoke(new Func<Task>(() =>
                    {
                        this.SizeChanged -= sizeHandler;

                        this.tmpDialogContainer.Children.Remove(dialog); //remove the dialog from the container

                        return HandleOverlayOnHide(settings);
                    }));
            };

            dialog.CloseHandler += closeHandler;

            return dialog.SetupOkDialog();
        }
        private System.Threading.Tasks.Task HandleOverlayOnHide(TMPDialogSettings settings)
        {
            return (settings == null ? this.HideOverlayAsync() : System.Threading.Tasks.Task.Factory.StartNew(() => this.Dispatcher.Invoke(new Action(this.HideOverlay))));
        }
        private  SizeChangedEventHandler SetupAndOpenDialog(BaseTMPDialog dialog)
        {
            dialog.SetValue(Panel.ZIndexProperty, (int)this.overlayBox.GetValue(Panel.ZIndexProperty) + 1);
            dialog.MinHeight = this.ActualHeight / 4.0;
            dialog.MaxHeight = this.ActualHeight;

            SizeChangedEventHandler sizeHandler = (sender, args) =>
            {
                dialog.MinHeight = this.ActualHeight / 4.0;
                dialog.MaxHeight = this.ActualHeight;
            };

            this.SizeChanged += sizeHandler;

            this.tmpDialogContainer.Children.Add(dialog); //add the dialog to the container

            dialog.OnShown();
            return sizeHandler;
        }


        #endregion

        internal T GetPart<T>(string name) where T : DependencyObject
        {
            return GetTemplateChild(name) as T;
        }
    }
}