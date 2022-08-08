namespace TMPApplication.CustomWpfWindow
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media.Animation;
    using Native;

    public partial class GlowWindow : Window
    {
        private readonly Func<Point, Cursor> getCursor;
        private readonly Func<double, double> getHeight;
        private readonly Func<Point, HitTestValues> getHitTestValue;
        private readonly Func<double, double> getLeft;
        private readonly Func<double, double> getTop;
        private readonly Func<double, double> getWidth;
        private const double edgeSize = 20.0;
        private const double glowSize = 17.0;
        private IntPtr handle;
        private IntPtr ownerHandle;
        private static double? dpiFactor = null;
        private bool closing = false;

        public GlowWindow(Window owner, GlowDirection direction)
        {
            this.InitializeComponent();

            this.IsGlowing = true;
            this.AllowsTransparency = true;
            this.Closing += (sender, e) => e.Cancel = !this.closing;

            this.Owner = owner;
            this.glow.Visibility = Visibility.Collapsed;

            var b = new Binding("GlowBrush");
            b.Source = owner;
            this.glow.SetBinding(Glow.GlowBrushProperty, b);

            b = new Binding("NonActiveGlowBrush");
            b.Source = owner;
            this.glow.SetBinding(Glow.NonActiveGlowBrushProperty, b);

            switch (direction)
            {
                case GlowDirection.Left:
                    this.glow.Orientation = Orientation.Vertical;
                    this.glow.HorizontalAlignment = HorizontalAlignment.Right;
                    this.getLeft = (dpi) => Math.Round((owner.Left - glowSize) * dpi);
                    this.getTop = (dpi) => Math.Round((owner.Top - glowSize) * dpi);
                    this.getWidth = (dpi) => glowSize * dpi;
                    this.getHeight = (dpi) => (owner.ActualHeight + (glowSize * 2.0)) * dpi;
                    this.getHitTestValue = p => new Rect(0, 0, this.ActualWidth, edgeSize).Contains(p)
                                               ? HitTestValues.HTTOPLEFT
                                               : new Rect(0, this.ActualHeight - edgeSize, this.ActualWidth, edgeSize).Contains(p)
                                                     ? HitTestValues.HTBOTTOMLEFT
                                                     : HitTestValues.HTLEFT;
                    this.getCursor = p =>
                    {
                        return (owner.ResizeMode == ResizeMode.NoResize || owner.ResizeMode == ResizeMode.CanMinimize)
                                    ? owner.Cursor
                                    : new Rect(0, 0, this.ActualWidth, edgeSize).Contains(p)
                                         ? Cursors.SizeNWSE
                                         : new Rect(0, this.ActualHeight - edgeSize, this.ActualWidth, edgeSize).Contains(p)
                                               ? Cursors.SizeNESW
                                               : Cursors.SizeWE;
                    };
                    break;

                case GlowDirection.Right:
                    this.glow.Orientation = Orientation.Vertical;
                    this.glow.HorizontalAlignment = HorizontalAlignment.Left;
                    this.getLeft = (dpi) => Math.Round((owner.Left + owner.ActualWidth) * dpi);
                    this.getTop = (dpi) => Math.Round((owner.Top - glowSize) * dpi);
                    this.getWidth = (dpi) => glowSize * dpi;
                    this.getHeight = (dpi) => (owner.ActualHeight + (glowSize * 2.0)) * dpi;
                    this.getHitTestValue = p => new Rect(0, 0, this.ActualWidth, edgeSize).Contains(p)
                                               ? HitTestValues.HTTOPRIGHT
                                               : new Rect(0, this.ActualHeight - edgeSize, this.ActualWidth, edgeSize).Contains(p)
                                                     ? HitTestValues.HTBOTTOMRIGHT
                                                     : HitTestValues.HTRIGHT;
                    this.getCursor = p =>
                    {
                        return (owner.ResizeMode == ResizeMode.NoResize || owner.ResizeMode == ResizeMode.CanMinimize)
                                    ? owner.Cursor
                                    : new Rect(0, 0, this.ActualWidth, edgeSize).Contains(p)
                                         ? Cursors.SizeNESW
                                         : new Rect(0, this.ActualHeight - edgeSize, this.ActualWidth, edgeSize).Contains(p)
                                               ? Cursors.SizeNWSE
                                               : Cursors.SizeWE;
                    };
                    break;

                case GlowDirection.Top:
                    this.glow.Orientation = Orientation.Horizontal;
                    this.glow.VerticalAlignment = VerticalAlignment.Bottom;
                    this.getLeft = (dpi) => owner.Left * dpi;
                    this.getTop = (dpi) => Math.Round((owner.Top - glowSize) * dpi);
                    this.getWidth = (dpi) => Math.Round(owner.ActualWidth * dpi);
                    this.getHeight = (dpi) => glowSize * dpi;
                    this.getHitTestValue = p => new Rect(0, 0, edgeSize - glowSize, this.ActualHeight).Contains(p)
                                               ? HitTestValues.HTTOPLEFT
                                               : new Rect(this.Width - edgeSize + glowSize, 0, edgeSize - glowSize,
                                                          this.ActualHeight).Contains(p)
                                                     ? HitTestValues.HTTOPRIGHT
                                                     : HitTestValues.HTTOP;
                    this.getCursor = p =>
                    {
                        return (owner.ResizeMode == ResizeMode.NoResize || owner.ResizeMode == ResizeMode.CanMinimize)
                                    ? owner.Cursor
                                    : new Rect(0, 0, edgeSize - glowSize, this.ActualHeight).Contains(p)
                                         ? Cursors.SizeNWSE
                                         : new Rect(this.Width - edgeSize + glowSize, 0, edgeSize - glowSize, this.ActualHeight).
                                               Contains(p)
                                               ? Cursors.SizeNESW
                                               : Cursors.SizeNS;
                    };
                    break;

                case GlowDirection.Bottom:
                    this.glow.Orientation = Orientation.Horizontal;
                    this.glow.VerticalAlignment = VerticalAlignment.Top;
                    this.getLeft = (dpi) => owner.Left * dpi;
                    this.getTop = (dpi) => Math.Round((owner.Top + owner.ActualHeight) * dpi);
                    this.getWidth = (dpi) => Math.Round(owner.ActualWidth * dpi);
                    this.getHeight = (dpi) => glowSize * dpi;
                    this.getHitTestValue = p => new Rect(0, 0, edgeSize - glowSize, this.ActualHeight).Contains(p)
                                               ? HitTestValues.HTBOTTOMLEFT
                                               : new Rect(this.Width - edgeSize + glowSize, 0, edgeSize - glowSize,
                                                          this.ActualHeight).Contains(p)
                                                     ? HitTestValues.HTBOTTOMRIGHT
                                                     : HitTestValues.HTBOTTOM;
                    this.getCursor = p =>
                    {
                        return (owner.ResizeMode == ResizeMode.NoResize || owner.ResizeMode == ResizeMode.CanMinimize)
                                    ? owner.Cursor
                                    : new Rect(0, 0, edgeSize - glowSize, this.ActualHeight).Contains(p)
                                         ? Cursors.SizeNESW
                                         : new Rect(this.Width - edgeSize + glowSize, 0, edgeSize - glowSize, this.ActualHeight).
                                               Contains(p)
                                               ? Cursors.SizeNWSE
                                               : Cursors.SizeNS;
                    };
                    break;
            }

            owner.ContentRendered += (sender, e) => this.glow.Visibility = Visibility.Visible;
            owner.Activated += (sender, e) =>
            {
                this.Update();
                this.glow.IsGlow = true;
            };
            owner.Deactivated += (sender, e) =>
                this.glow.IsGlow = false;
            owner.LocationChanged += (sender, e) => this.Update();
            owner.SizeChanged += (sender, e) => this.Update();
            owner.StateChanged += (sender, e) => this.Update();
            owner.IsVisibleChanged += (sender, e) => this.Update();
            owner.Closed += (sender, e) =>
            {
                this.closing = true;
                this.Close();
            };
        }

        public double DpiFactor
        {
            get
            {
                if (dpiFactor == null)
                {
                    double dpiX = 96.0, dpiY = 96.0;

                    // #652, #752 check if Owner not null
                    var owner = this.Owner ?? (Application.Current != null ? Application.Current.MainWindow : null);
                    var source = owner != null ? PresentationSource.FromVisual(owner) : null;
                    if (source != null && source.CompositionTarget != null)
                    {
                        dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                        dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
                    }

                    dpiFactor = dpiX == dpiY ? dpiX / 96.0 : 1;
                }

                return dpiFactor.Value;
            }
        }

        public Storyboard OpacityStoryboard { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.OpacityStoryboard = this.TryFindResource("OpacityStoryboard") as Storyboard;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var source = (HwndSource)PresentationSource.FromVisual(this);
            WS ws = source.Handle.GetWindowLong();
            WSEX wsex = source.Handle.GetWindowLongEx();

            // ws |= WS.POPUP;
            wsex ^= WSEX.APPWINDOW;
            wsex |= WSEX.NOACTIVATE;

            source.Handle.SetWindowLong(ws);
            source.Handle.SetWindowLongEx(wsex);
            source.AddHook(this.WndProc);

            this.handle = source.Handle;
        }

        public void Update()
        {
            if (this.Owner.Visibility == Visibility.Hidden)
            {
                this.Visibility = Visibility.Hidden;

                this.UpdateCore();
            }
            else if (this.Owner.WindowState == WindowState.Normal)
            {
                if (this.closing)
                {
                    return;
                }

                this.Visibility = this.IsGlowing ? Visibility.Visible : Visibility.Collapsed;
                this.glow.Visibility = this.IsGlowing ? Visibility.Visible : Visibility.Collapsed;

                this.UpdateCore();
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

        public bool IsGlowing
        {
            set;
            get;
        }

        private void UpdateCore()
        {
            if (this.ownerHandle == IntPtr.Zero)
            {
                this.ownerHandle = new WindowInteropHelper(this.Owner).Handle;
            }

            NativeMethods.SetWindowPos(
                this.handle,
                this.ownerHandle,
                (int)this.getLeft(this.DpiFactor),
                (int)this.getTop(this.DpiFactor),
                (int)this.getWidth(this.DpiFactor),
                (int)this.getHeight(this.DpiFactor),
                SWP.NOACTIVATE);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == (int)WM.SHOWWINDOW)
            {
                if ((int)lParam == 3 && this.Visibility != Visibility.Visible) // 3 == SW_PARENTOPENING
                {
                    handled = true; // handle this message so window isn't shown until we want it to
                }
            }

            if (msg == (int)WM.MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(3);
            }

            if (msg == (int)WM.LBUTTONDOWN)
            {
                var pt = new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);

                NativeMethods.PostMessage(this.ownerHandle, (uint)WM.NCLBUTTONDOWN, (IntPtr)this.getHitTestValue(pt),
                                          IntPtr.Zero);
            }

            if (msg == (int)WM.NCHITTEST)
            {
                var ptScreen = new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);
                Point ptClient = this.PointFromScreen(ptScreen);
                Cursor cursor = this.getCursor(ptClient);
                if (cursor != this.Cursor)
                {
                    this.Cursor = cursor;
                }
            }

            return IntPtr.Zero;
        }
    }
}