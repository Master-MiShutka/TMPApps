namespace TMPApplication.Behaviours
{
    using System;
    using System.Linq;
    using System.Management;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using Interactivity;
    using MS.Windows.Shell;
    using TMPApplication.CustomWpfWindow;
    using TMPApplication.Utils;

    /// <summary>
    /// Создание произвольного стиля окна
    /// </summary>
    public class BorderlessWindowBehavior : Behavior<Window>
    {
        private IntPtr handle;
        private HwndSource hwndSource;
        private WindowChrome windowChrome;
        private PropertyChangeNotifier borderThicknessChangeNotifier;
        private Thickness? savedBorderThickness;
        private Thickness? savedResizeBorderThickness;
        private PropertyChangeNotifier topMostChangeNotifier;
        private bool savedTopMost;

        // Versions can be taken from https://msdn.microsoft.com/library/windows/desktop/ms724832.aspx
        private bool isWindwos10OrHigher;

        private static bool IsWindows10OrHigher()
        {
            var version = Native.NtDll.RtlGetVersion();
            if (default(Version) == version)
            {
                // Snippet from Koopakiller https://dotnet-snippets.de/snippet/os-version-name-mit-wmi/4929
                using (var mos = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
                {
                    var attribs = mos.Get().OfType<ManagementObject>();

                    // caption = attribs.FirstOrDefault().GetPropertyValue("Caption").ToString() ?? "Unknown";
                    version = new Version((attribs.FirstOrDefault()?.GetPropertyValue("Version") ?? "0.0.0.0").ToString());
                }
            }

            return version >= new Version(10, 0);
        }

        protected override void OnAttached()
        {
            this.isWindwos10OrHigher = IsWindows10OrHigher();

            this.windowChrome = new WindowChrome
            {
#if NET4_5
                ResizeBorderThickness = SystemParameters.WindowResizeBorderThickness, 
#else
                ResizeBorderThickness = SystemParameters2.Current.WindowResizeBorderThickness,
#endif
                CaptionHeight = 10,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                IgnoreTaskbarOnMaximize = false,
                UseAeroCaptionButtons = false,
            };
            var window = this.AssociatedObject as WindowWithDialogs;
            if (window != null)
            {
                this.windowChrome.IgnoreTaskbarOnMaximize = window.IgnoreTaskbarOnMaximize;
                this.windowChrome.UseNoneWindowStyle = window.UseNoneWindowStyle;
                System.ComponentModel.DependencyPropertyDescriptor.FromProperty(WindowWithDialogs.IgnoreTaskbarOnMaximizeProperty, typeof(WindowWithDialogs))
                      .AddValueChanged(this.AssociatedObject, this.IgnoreTaskbarOnMaximizePropertyChangedCallback);
                System.ComponentModel.DependencyPropertyDescriptor.FromProperty(WindowWithDialogs.UseNoneWindowStyleProperty, typeof(WindowWithDialogs))
                      .AddValueChanged(this.AssociatedObject, this.UseNoneWindowStylePropertyChangedCallback);
            }

            this.AssociatedObject.SetValue(WindowChrome.WindowChromeProperty, this.windowChrome);

            // no transparency, because it hase more then one unwanted issues
            var windowHandle = new WindowInteropHelper(this.AssociatedObject).Handle;
            if (!this.AssociatedObject.IsLoaded && windowHandle == IntPtr.Zero)
            {
                try
                {
                    this.AssociatedObject.AllowsTransparency = false;
                }
                catch (Exception)
                {
                    // For some reason, we can't determine if the window has loaded or not, so we swallow the exception.
                }
            }

            this.AssociatedObject.WindowStyle = WindowStyle.None;

            this.savedBorderThickness = this.AssociatedObject.BorderThickness;
            this.savedResizeBorderThickness = this.windowChrome.ResizeBorderThickness;
            this.borderThicknessChangeNotifier = new PropertyChangeNotifier(this.AssociatedObject, Control.BorderThicknessProperty);
            this.borderThicknessChangeNotifier.ValueChanged += this.BorderThicknessChangeNotifierOnValueChanged;

            this.savedTopMost = this.AssociatedObject.Topmost;
            this.topMostChangeNotifier = new PropertyChangeNotifier(this.AssociatedObject, Window.TopmostProperty);
            this.topMostChangeNotifier.ValueChanged += this.TopMostChangeNotifierOnValueChanged;

            // #1823 try to fix another nasty issue
            // WindowState = Maximized
            // ResizeMode = NoResize
            if (this.AssociatedObject.ResizeMode == ResizeMode.NoResize)
            {
                this.windowChrome.ResizeBorderThickness = new Thickness(0);
            }

            var topmostHack = new Action(() =>
                                           {
                                               if (this.AssociatedObject.Topmost)
                                               {
                                                   var raiseValueChanged = this.topMostChangeNotifier.RaiseValueChanged;
                                                   this.topMostChangeNotifier.RaiseValueChanged = false;
                                                   this.AssociatedObject.Topmost = false;
                                                   this.AssociatedObject.Topmost = true;
                                                   this.topMostChangeNotifier.RaiseValueChanged = raiseValueChanged;
                                               }
                                           });
            this.AssociatedObject.LostFocus += (sender, args) => { topmostHack(); };
            this.AssociatedObject.Deactivated += (sender, args) => { topmostHack(); };
            this.AssociatedObject.Loaded += this.AssociatedObject_Loaded;
            this.AssociatedObject.Unloaded += this.AssociatedObject_Unloaded;
            this.AssociatedObject.Closed += this.AssociatedObjectClosed;
            this.AssociatedObject.SourceInitialized += this.AssociatedObject_SourceInitialized;
            this.AssociatedObject.StateChanged += this.OnAssociatedObjectHandleMaximize;

            base.OnAttached();
        }

        private void BorderThicknessChangeNotifierOnValueChanged(object sender, EventArgs e)
        {
            // It's bad if the window is null at this point, but we check this here to prevent the possible occurred exception
            var window = this.AssociatedObject;
            if (window != null)
            {
                this.savedBorderThickness = window.BorderThickness;
            }
        }

        private void TopMostChangeNotifierOnValueChanged(object sender, EventArgs e)
        {
            // It's bad if the window is null at this point, but we check this here to prevent the possible occurred exception
            var window = this.AssociatedObject;
            if (window != null)
            {
                this.savedTopMost = window.Topmost;
            }
        }

        private void UseNoneWindowStylePropertyChangedCallback(object sender, EventArgs e)
        {
            var window = sender as WindowWithDialogs;
            if (window != null && this.windowChrome != null)
            {
                if (!Equals(this.windowChrome.UseNoneWindowStyle, window.UseNoneWindowStyle))
                {
                    this.windowChrome.UseNoneWindowStyle = window.UseNoneWindowStyle;
                    this.ForceRedrawWindowFromPropertyChanged();
                }
            }
        }

        private void IgnoreTaskbarOnMaximizePropertyChangedCallback(object sender, EventArgs e)
        {
            var window = sender as WindowWithDialogs;
            if (window != null && this.windowChrome != null)
            {
                if (!Equals(this.windowChrome.IgnoreTaskbarOnMaximize, window.IgnoreTaskbarOnMaximize))
                {
                    // another special hack to avoid nasty resizing
                    // repro
                    // ResizeMode="NoResize"
                    // WindowState="Maximized"
                    // IgnoreTaskbarOnMaximize="True"
                    // this only happens if we change this at runtime
                    var removed = this._ModifyStyle(Native.WS.MAXIMIZEBOX | Native.WS.MINIMIZEBOX | Native.WS.THICKFRAME, 0);
                    this.windowChrome.IgnoreTaskbarOnMaximize = window.IgnoreTaskbarOnMaximize;
                    if (removed)
                    {
                        this._ModifyStyle(0, Native.WS.MAXIMIZEBOX | Native.WS.MINIMIZEBOX | Native.WS.THICKFRAME);
                    }

                    this.ForceRedrawWindowFromPropertyChanged();
                }
            }
        }

        /// <summary>Add and remove a native WindowStyle from the HWND.</summary>
        /// <param name="removeStyle">The styles to be removed.  These can be bitwise combined.</param>
        /// <param name="addStyle">The styles to be added.  These can be bitwise combined.</param>
        /// <returns>Whether the styles of the HWND were modified as a result of this call.</returns>
        /// <SecurityNote>
        ///   Critical : Calls critical methods
        /// </SecurityNote>
        [System.Security.SecurityCritical]
        private bool _ModifyStyle(Native.WS removeStyle, Native.WS addStyle)
        {
            if (this.handle == IntPtr.Zero)
            {
                return false;
            }

            var intPtr = Native.NativeMethods.GetWindowLongPtr(this.handle, Native.GWL.STYLE);
            var dwStyle = (Native.WS)(Environment.Is64BitProcess ? intPtr.ToInt64() : intPtr.ToInt32());
            var dwNewStyle = (dwStyle & ~removeStyle) | addStyle;
            if (dwStyle == dwNewStyle)
            {
                return false;
            }

            Native.NativeMethods.SetWindowLongPtr(this.handle, Native.GWL.STYLE, new IntPtr((int)dwNewStyle));
            return true;
        }

        private void ForceRedrawWindowFromPropertyChanged()
        {
            this.HandleMaximize();
            if (this.handle != IntPtr.Zero)
            {
                Native.UnsafeNativeMethods.RedrawWindow(this.handle, IntPtr.Zero, IntPtr.Zero, Native.Constants.RedrawWindowFlags.Invalidate | Native.Constants.RedrawWindowFlags.Frame);
            }
        }

        private bool isCleanedUp;

        private void Cleanup()
        {
            if (!this.isCleanedUp)
            {
                this.isCleanedUp = true;

                if (GetHandleTaskbar(this.AssociatedObject) && this.isWindwos10OrHigher)
                {
                    this.DeactivateTaskbarFix();
                }

                // clean up events
                if (this.AssociatedObject is WindowWithDialogs)
                {
                    System.ComponentModel.DependencyPropertyDescriptor.FromProperty(WindowWithDialogs.IgnoreTaskbarOnMaximizeProperty, typeof(WindowWithDialogs))
                          .RemoveValueChanged(this.AssociatedObject, this.IgnoreTaskbarOnMaximizePropertyChangedCallback);
                    System.ComponentModel.DependencyPropertyDescriptor.FromProperty(WindowWithDialogs.UseNoneWindowStyleProperty, typeof(WindowWithDialogs))
                          .RemoveValueChanged(this.AssociatedObject, this.UseNoneWindowStylePropertyChangedCallback);
                }

                this.AssociatedObject.Loaded -= this.AssociatedObject_Loaded;
                this.AssociatedObject.Unloaded -= this.AssociatedObject_Unloaded;
                this.AssociatedObject.Closed -= this.AssociatedObjectClosed;
                this.AssociatedObject.SourceInitialized -= this.AssociatedObject_SourceInitialized;
                this.AssociatedObject.StateChanged -= this.OnAssociatedObjectHandleMaximize;
                if (this.hwndSource != null)
                {
                    this.hwndSource.RemoveHook(this.WindowProc);
                }

                this.windowChrome = null;
            }
        }

        protected override void OnDetaching()
        {
            this.Cleanup();
            base.OnDetaching();
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Cleanup();
        }

        private void AssociatedObjectClosed(object sender, EventArgs e)
        {
            this.Cleanup();
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var returnval = IntPtr.Zero;

            switch (msg)
            {
                case Native.Constants.WM_NCPAINT:
                    handled = true;
                    break;
                case Native.Constants.WM_NCACTIVATE:
                    /* As per http://msdn.microsoft.com/en-us/library/ms632633(VS.85).aspx , "-1" lParam "does not repaint the nonclient area to reflect the state change." */
                    returnval = Native.UnsafeNativeMethods.DefWindowProc(hwnd, msg, wParam, new IntPtr(-1));
                    handled = true;
                    break;
                case (int)Standard.WM.WINDOWPOSCHANGING:
                    {
                        var pos = (Standard.WINDOWPOS)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(Standard.WINDOWPOS));
                        if ((pos.flags & (int)Standard.SWP.NOMOVE) != 0)
                        {
                            return IntPtr.Zero;
                        }

                        var wnd = this.AssociatedObject;
                        if (wnd == null || this.hwndSource?.CompositionTarget == null)
                        {
                            return IntPtr.Zero;
                        }

                        bool changedPos = false;

                        // Convert the original to original size based on DPI setting. Need for x% screen DPI.
                        var matrix = this.hwndSource.CompositionTarget.TransformToDevice;

                        var minWidth = wnd.MinWidth * matrix.M11;
                        var minHeight = wnd.MinHeight * matrix.M22;
                        if (pos.cx < minWidth)
                        {
                            pos.cx = (int)minWidth;
                            changedPos = true;
                        }

                        if (pos.cy < minHeight)
                        {
                            pos.cy = (int)minHeight;
                            changedPos = true;
                        }

                        var maxWidth = wnd.MaxWidth * matrix.M11;
                        var maxHeight = wnd.MaxHeight * matrix.M22;
                        if (pos.cx > maxWidth && maxWidth > 0)
                        {
                            pos.cx = (int)Math.Round(maxWidth);
                            changedPos = true;
                        }

                        if (pos.cy > maxHeight && maxHeight > 0)
                        {
                            pos.cy = (int)Math.Round(maxHeight);
                            changedPos = true;
                        }

                        if (!changedPos)
                        {
                            return IntPtr.Zero;
                        }

                        System.Runtime.InteropServices.Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }

                    break;
            }

            return returnval;
        }

        private void OnAssociatedObjectHandleMaximize(object sender, EventArgs e)
        {
            this.HandleMaximize();
        }

        private void HandleMaximize()
        {
            this.borderThicknessChangeNotifier.RaiseValueChanged = false;
            var raiseValueChanged = this.topMostChangeNotifier.RaiseValueChanged;
            this.topMostChangeNotifier.RaiseValueChanged = false;

            var window = this.AssociatedObject as WindowWithDialogs;

            if (this.AssociatedObject.WindowState == WindowState.Maximized)
            {
                // remove window border, so we can move the window from top monitor position
                this.AssociatedObject.BorderThickness = new Thickness(0);

                var ignoreTaskBar = window != null && window.IgnoreTaskbarOnMaximize;
                if (this.handle != IntPtr.Zero)
                {
                    this.windowChrome.ResizeBorderThickness = new Thickness(0);

                    // WindowChrome handles the size false if the main monitor is lesser the monitor where the window is maximized
                    // so set the window pos/size twice
                    IntPtr monitor = Native.UnsafeNativeMethods.MonitorFromWindow(this.handle, Native.Constants.MONITOR_DEFAULTTONEAREST);
                    if (monitor != IntPtr.Zero)
                    {
                        var monitorInfo = new Native.MONITORINFO();
                        Native.UnsafeNativeMethods.GetMonitorInfo(monitor, monitorInfo);

                        var desktopRect = ignoreTaskBar ? monitorInfo.rcMonitor : monitorInfo.rcWork;
                        var x = desktopRect.left;
                        var y = desktopRect.top;
                        var cx = Math.Abs(desktopRect.right - x);
                        var cy = Math.Abs(desktopRect.bottom - y);

                        if (ignoreTaskBar && this.isWindwos10OrHigher)
                        {
                            this.ActivateTaskbarFix();
                        }

                        Native.UnsafeNativeMethods.SetWindowPos(this.handle, new IntPtr(-2), x, y, cx, cy, 0x0040);
                    }
                }
            }
            else
            {
                var resizeBorderThickness = this.savedResizeBorderThickness.GetValueOrDefault(new Thickness(0));
                if (this.windowChrome.ResizeBorderThickness != resizeBorderThickness)
                {
                    this.windowChrome.ResizeBorderThickness = resizeBorderThickness;
                }

                // #2694 make sure the window is not on top after restoring window
                // this issue was introduced after fixing the windows 10 bug with the taskbar and a maximized window that ignores the taskbar
                if (GetHandleTaskbar(this.AssociatedObject) && this.isWindwos10OrHigher)
                {
                    this.DeactivateTaskbarFix();
                }
            }

            // fix nasty TopMost bug
            // - set TopMost="True"
            // - start mahapps demo
            // - TopMost works
            // - maximize window and back to normal
            // - TopMost is gone
            //
            // Problem with minimize animation when window is maximized #1528
            // 1. Activate another application (such as Google Chrome).
            // 2. Run the demo and maximize it.
            // 3. Minimize the demo by clicking on the taskbar button.
            // Note that the minimize animation in this case does actually run, but somehow the other
            // application (Google Chrome in this example) is instantly switched to being the top window,
            // and so blocking the animation view.
            this.AssociatedObject.Topmost = false;
            this.AssociatedObject.Topmost = this.AssociatedObject.WindowState == WindowState.Minimized || this.savedTopMost;

            this.borderThicknessChangeNotifier.RaiseValueChanged = true;
            this.topMostChangeNotifier.RaiseValueChanged = raiseValueChanged;
        }

        private void ActivateTaskbarFix()
        {
            var trayWndHandle = Standard.NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (trayWndHandle != IntPtr.Zero)
            {
                SetHandleTaskbar(this.AssociatedObject, true);
                Native.UnsafeNativeMethods.SetWindowPos(trayWndHandle, Native.Constants.HWND_BOTTOM, 0, 0, 0, 0, Native.Constants.TOPMOST_FLAGS);
                Native.UnsafeNativeMethods.SetWindowPos(trayWndHandle, Native.Constants.HWND_TOP, 0, 0, 0, 0, Native.Constants.TOPMOST_FLAGS);
                Native.UnsafeNativeMethods.SetWindowPos(trayWndHandle, Native.Constants.HWND_NOTOPMOST, 0, 0, 0, 0, Native.Constants.TOPMOST_FLAGS);
            }
        }

        private void DeactivateTaskbarFix()
        {
            var trayWndHandle = Native.NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (trayWndHandle != IntPtr.Zero)
            {
                SetHandleTaskbar(this.AssociatedObject, false);
                Native.UnsafeNativeMethods.SetWindowPos(trayWndHandle, Native.Constants.HWND_BOTTOM, 0, 0, 0, 0, Native.Constants.TOPMOST_FLAGS);
                Native.UnsafeNativeMethods.SetWindowPos(trayWndHandle, Native.Constants.HWND_TOP, 0, 0, 0, 0, Native.Constants.TOPMOST_FLAGS);
                Native.UnsafeNativeMethods.SetWindowPos(trayWndHandle, Native.Constants.HWND_TOPMOST, 0, 0, 0, 0, Native.Constants.TOPMOST_FLAGS);
            }
        }

        private static readonly DependencyProperty HandleTaskbarProperty
            = DependencyProperty.RegisterAttached(
                "HandleTaskbar",
                typeof(bool),
                typeof(BorderlessWindowBehavior), new FrameworkPropertyMetadata(false));

        private static bool GetHandleTaskbar(UIElement element)
        {
            return (bool)element.GetValue(HandleTaskbarProperty);
        }

        private static void SetHandleTaskbar(UIElement element, bool value)
        {
            element.SetValue(HandleTaskbarProperty, value);
        }

        private void AssociatedObject_SourceInitialized(object sender, EventArgs e)
        {
            this.handle = new WindowInteropHelper(this.AssociatedObject).Handle;
            if (IntPtr.Zero == this.handle)
            {
                throw new Exception("Uups, at this point we really need the Handle from the associated object!");
            }

            if (this.AssociatedObject.SizeToContent != SizeToContent.Manual && this.AssociatedObject.WindowState == WindowState.Normal)
            {
                // Another try to fix SizeToContent
                // without this we get nasty glitches at the borders
                this.AssociatedObject.Invoke(() =>
                    {
                        this.AssociatedObject.InvalidateMeasure();
                        Native.RECT rect;
                        if (Native.UnsafeNativeMethods.GetWindowRect(this.handle, out rect))
                        {
                            Native.UnsafeNativeMethods.SetWindowPos(this.handle, new IntPtr(-2), rect.left, rect.top, rect.Width, rect.Height, 0x0040);
                        }
                    });
            }

            this.hwndSource = HwndSource.FromHwnd(this.handle);
            if (this.hwndSource != null)
            {
                this.hwndSource.AddHook(this.WindowProc);
            }

            if (this.AssociatedObject.ResizeMode != ResizeMode.NoResize)
            {
                // handle size to content (thanks @lynnx).
                // This is necessary when ResizeMode != NoResize. Without this workaround,
                // black bars appear at the right and bottom edge of the window.
                var sizeToContent = this.AssociatedObject.SizeToContent;
                var snapsToDevicePixels = this.AssociatedObject.SnapsToDevicePixels;
                this.AssociatedObject.SnapsToDevicePixels = true;
                this.AssociatedObject.SizeToContent = sizeToContent == SizeToContent.WidthAndHeight ? SizeToContent.Height : SizeToContent.Manual;
                this.AssociatedObject.SizeToContent = sizeToContent;
                this.AssociatedObject.SnapsToDevicePixels = snapsToDevicePixels;
            }

            // handle the maximized state here too (to handle the border in a correct way)
            this.HandleMaximize();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            var window = sender as WindowWithDialogs;
            if (window == null)
            {
                return;
            }

            if (window.ResizeMode != ResizeMode.NoResize)
            {
                window.SetIsHitTestVisibleInChromeProperty<Border>("PART_Border");
                window.SetIsHitTestVisibleInChromeProperty<UIElement>("PART_Icon");
                window.SetWindowChromeResizeGripDirection("WindowResizeGrip", ResizeGripDirection.BottomRight);
            }
        }
    }
}
