/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace MS.Windows.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Standard;
    using HANDLE_MESSAGE = System.Collections.Generic.KeyValuePair<Standard.WM, Standard.MessageHandler>;

    public class WindowChromeWorker : DependencyObject
    {
        private static readonly Version presentationFrameworkVersion = Assembly.GetAssembly(typeof(Window)).GetName().Version;

        /// <summary>
        /// Is this using WPF4?
        /// </summary>
        /// <remarks>
        /// There are a few specific bugs in Window in 3.5SP1 and below that require workarounds
        /// when handling WM_NCCALCSIZE on the HWND.
        /// </remarks>
        private static bool _IsPresentationFrameworkVersionLessThan4 => presentationFrameworkVersion < new Version(4, 0);

        // Delegate signature used for Dispatcher.BeginInvoke.
        private delegate void _Action();

        #region Fields

        private const SWP SwpFlags = SWP.FRAMECHANGED | SWP.NOSIZE | SWP.NOMOVE | SWP.NOZORDER | SWP.NOOWNERZORDER | SWP.NOACTIVATE;

        private readonly List<HANDLE_MESSAGE> _messageTable;

        /// <summary>The Window that's chrome is being modified.</summary>
        private Window window;

        /// <summary>Underlying HWND for the _window.</summary>
        private IntPtr _hwnd;

        private HwndSource _hwndSource = null;
        private bool _isHooked = false;

        // These fields are for tracking workarounds for WPF 3.5SP1 behaviors.
        private bool _isFixedUp = false;

        private bool _isUserResizing = false;
        private bool _hasUserMovedWindow = false;
        private Point _windowPosAtStartOfUserMove = default(Point);

        /// <summary>Object that describes the current modifications being made to the chrome.</summary>
        private WindowChrome _chromeInfo;

        // Keep track of this so we can detect when we need to apply changes.  Tracking these separately
        // as I've seen using just one cause things to get enough out of sync that occasionally the caption will redraw.
        private WindowState _lastRoundingState;

        private WindowState _lastMenuState;
        private bool _isGlassEnabled;

        private WINDOWPOS _previousWP;

        #endregion Fields

        public WindowChromeWorker()
        {
            this._messageTable = new List<HANDLE_MESSAGE>
            {
                new HANDLE_MESSAGE(WM.SETTEXT,               this._HandleSetTextOrIcon),
                new HANDLE_MESSAGE(WM.SETICON,               this._HandleSetTextOrIcon),
                new HANDLE_MESSAGE(WM.SYSCOMMAND,            this._HandleRestoreWindow),
                new HANDLE_MESSAGE(WM.NCACTIVATE,            this._HandleNCActivate),
                new HANDLE_MESSAGE(WM.NCCALCSIZE,            this._HandleNCCalcSize),
                new HANDLE_MESSAGE(WM.NCHITTEST,             this._HandleNCHitTest),
                new HANDLE_MESSAGE(WM.NCRBUTTONUP,           this._HandleNCRButtonUp),
                new HANDLE_MESSAGE(WM.SIZE,                  this._HandleSize),
                new HANDLE_MESSAGE(WM.WINDOWPOSCHANGING,     this._HandleWindowPosChanging),
                new HANDLE_MESSAGE(WM.WINDOWPOSCHANGED,      this._HandleWindowPosChanged),
                new HANDLE_MESSAGE(WM.GETMINMAXINFO,         this._HandleGetMinMaxInfo),
                new HANDLE_MESSAGE(WM.DWMCOMPOSITIONCHANGED, this._HandleDwmCompositionChanged),
                new HANDLE_MESSAGE(WM.ENTERSIZEMOVE,         this._HandleEnterSizeMoveForAnimation),
                new HANDLE_MESSAGE(WM.MOVE,                  this._HandleMoveForRealSize),
                new HANDLE_MESSAGE(WM.EXITSIZEMOVE,          this._HandleExitSizeMoveForAnimation),
            };

            if (_IsPresentationFrameworkVersionLessThan4)
            {
                this._messageTable.AddRange(new[]
                {
                   new HANDLE_MESSAGE(WM.SETTINGCHANGE,         this._HandleSettingChange),
                   new HANDLE_MESSAGE(WM.ENTERSIZEMOVE,         this._HandleEnterSizeMove),
                   new HANDLE_MESSAGE(WM.EXITSIZEMOVE,          this._HandleExitSizeMove),
                   new HANDLE_MESSAGE(WM.MOVE,                  this._HandleMove),
                });
            }
        }

        public void SetWindowChrome(WindowChrome newChrome)
        {
            this.VerifyAccess();
            Assert.IsNotNull(this.window);

            if (newChrome == this._chromeInfo)
            {
                // Nothing's changed.
                return;
            }

            if (this._chromeInfo != null)
            {
                this._chromeInfo.PropertyChangedThatRequiresRepaint -= this._OnChromePropertyChangedThatRequiresRepaint;
            }

            this._chromeInfo = newChrome;
            if (this._chromeInfo != null)
            {
                this._chromeInfo.PropertyChangedThatRequiresRepaint += this._OnChromePropertyChangedThatRequiresRepaint;
            }

            this._ApplyNewCustomChrome();
        }

        private void _OnChromePropertyChangedThatRequiresRepaint(object sender, EventArgs e)
        {
            this._UpdateFrameState(true);
        }

        public static readonly DependencyProperty WindowChromeWorkerProperty = DependencyProperty.RegisterAttached(
            "WindowChromeWorker",
            typeof(WindowChromeWorker),
            typeof(WindowChromeWorker),
            new PropertyMetadata(null, _OnChromeWorkerChanged));

        private static void _OnChromeWorkerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var w = (Window)d;
            var cw = (WindowChromeWorker)e.NewValue;

            // The WindowChromeWorker object should only be set on the window once, and never to null.
            Assert.IsNotNull(w);
            Assert.IsNotNull(cw);
            Assert.IsNull(cw.window);

            cw._SetWindow(w);
        }

        private void _SetWindow(Window window)
        {
            Assert.IsNull(this.window);
            Assert.IsNotNull(window);

            this.window = window;

            // There are potentially a couple funny states here.
            // The window may have been shown and closed, in which case it's no longer usable.
            // We shouldn't add any hooks in that case, just exit early.
            // If the window hasn't yet been shown, then we need to make sure to remove hooks after it's closed.
            this._hwnd = new WindowInteropHelper(this.window).Handle;

            // On older versions of the framework the client size of the window is incorrectly calculated.
            // We need to modify the template to fix this on behalf of the user.

            // This should only be required on older versions of the framework, but because of a DWM bug in Windows 7 we're exposing
            // the SacrificialEdge property which requires this kind of fixup to be a bit more ubiquitous.
            Utility.AddDependencyPropertyChangeListener(this.window, Window.TemplateProperty, this._OnWindowPropertyChangedThatRequiresTemplateFixup);
            Utility.AddDependencyPropertyChangeListener(this.window, Window.FlowDirectionProperty, this._OnWindowPropertyChangedThatRequiresTemplateFixup);

            this.window.Closed += this._UnsetWindow;

            // Use whether we can get an HWND to determine if the Window has been loaded.
            if (IntPtr.Zero != this._hwnd)
            {
                // We've seen that the HwndSource can't always be retrieved from the HWND, so cache it early.
                // Specifically it seems to sometimes disappear when the OS theme is changing.
                this._hwndSource = HwndSource.FromHwnd(this._hwnd);
                Assert.IsNotNull(this._hwndSource);
                this.window.ApplyTemplate();

                if (this._chromeInfo != null)
                {
                    this._ApplyNewCustomChrome();
                }
            }
            else
            {
                this.window.SourceInitialized += (sender, e) =>
                {
                    this._hwnd = new WindowInteropHelper(this.window).Handle;
                    Assert.IsNotDefault(this._hwnd);
                    this._hwndSource = HwndSource.FromHwnd(this._hwnd);
                    Assert.IsNotNull(this._hwndSource);

                    if (this._chromeInfo != null)
                    {
                        this._ApplyNewCustomChrome();
                    }
                };
            }
        }

        private void _UnsetWindow(object sender, EventArgs e)
        {
            Utility.RemoveDependencyPropertyChangeListener(this.window, Window.TemplateProperty, this._OnWindowPropertyChangedThatRequiresTemplateFixup);
            Utility.RemoveDependencyPropertyChangeListener(this.window, Window.FlowDirectionProperty, this._OnWindowPropertyChangedThatRequiresTemplateFixup);

            if (this._chromeInfo != null)
            {
                this._chromeInfo.PropertyChangedThatRequiresRepaint -= this._OnChromePropertyChangedThatRequiresRepaint;
            }

            this._RestoreStandardChromeState(true);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ToDo")]
        public static WindowChromeWorker GetWindowChromeWorker(Window window)
        {
            Verify.IsNotNull(window, "window");
            return (WindowChromeWorker)window.GetValue(WindowChromeWorkerProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ToDo")]
        public static void SetWindowChromeWorker(Window window, WindowChromeWorker chrome)
        {
            Verify.IsNotNull(window, "window");
            window.SetValue(WindowChromeWorkerProperty, chrome);
        }

        private void _OnWindowPropertyChangedThatRequiresTemplateFixup(object sender, EventArgs e)
        {
            if (this._chromeInfo != null && this._hwnd != IntPtr.Zero)
            {
                // Assume that when the template changes it's going to be applied.
                // We don't have a good way to externally hook into the template
                // actually being applied, so we asynchronously post the fixup operation
                // at Loaded priority, so it's expected that the visual tree will be
                // updated before _FixupTemplateIssues is called.
                this.window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (_Action)this._FixupTemplateIssues);
            }
        }

        private void _ApplyNewCustomChrome()
        {
            if (this._hwnd == IntPtr.Zero)
            {
                // Not yet hooked.
                return;
            }

            if (this._chromeInfo == null)
            {
                this._RestoreStandardChromeState(false);
                return;
            }

            if (!this._isHooked)
            {
                this._hwndSource.AddHook(this._WndProc);
                this._isHooked = true;
            }

            // allow animation
            this._ModifyStyle(0, WS.CAPTION);

            this._FixupTemplateIssues();

            // Force this the first time.
            this._UpdateSystemMenu(this.window.WindowState);
            this._UpdateFrameState(true);

            NativeMethods.SetWindowPos(this._hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpFlags);
        }

        private void _FixupTemplateIssues()
        {
            Assert.IsNotNull(this._chromeInfo);
            Assert.IsNotNull(this.window);

            if (this.window.Template == null)
            {
                // Nothing to fixup yet.  This will get called again when a template does get set.
                return;
            }

            // Guard against the visual tree being empty.
            if (VisualTreeHelper.GetChildrenCount(this.window) == 0)
            {
                // The template isn't null, but we don't have a visual tree.
                // Hope that ApplyTemplate is in the queue and repost this, because there's not much we can do right now.
                this.window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (_Action)this._FixupTemplateIssues);
                return;
            }

            Thickness templateFixupMargin = default(Thickness);
            Transform templateFixupTransform = null;

            if (_IsPresentationFrameworkVersionLessThan4)
            {
                RECT rcWindow = NativeMethods.GetWindowRect(this._hwnd);
                RECT rcAdjustedClient = this._GetAdjustedWindowRect(rcWindow);

                Rect rcLogicalWindow = DpiHelper.DeviceRectToLogical(new Rect(rcWindow.Left, rcWindow.Top, rcWindow.Width, rcWindow.Height));
                Rect rcLogicalClient = DpiHelper.DeviceRectToLogical(new Rect(rcAdjustedClient.Left, rcAdjustedClient.Top, rcAdjustedClient.Width, rcAdjustedClient.Height));

                Thickness nonClientThickness = new Thickness(
                   rcLogicalWindow.Left - rcLogicalClient.Left,
                   rcLogicalWindow.Top - rcLogicalClient.Top,
                   rcLogicalClient.Right - rcLogicalWindow.Right,
                   rcLogicalClient.Bottom - rcLogicalWindow.Bottom);

                templateFixupMargin = new Thickness(
                    0,
                    0,
                    -(nonClientThickness.Left + nonClientThickness.Right),
                    -(nonClientThickness.Top + nonClientThickness.Bottom));

                // The negative thickness on the margin doesn't properly get applied in RTL layouts.
                // The width is right, but there is a black bar on the right.
                // To fix this we just add an additional RenderTransform to the root element.
                // This works fine, but if the window is dynamically changing its FlowDirection then this can have really bizarre side effects.
                // This will mostly work if the FlowDirection is dynamically changed, but there aren't many real scenarios that would call for
                // that so I'm not addressing the rest of the quirkiness.
                if (this.window.FlowDirection == FlowDirection.RightToLeft)
                {
                    templateFixupTransform = new MatrixTransform(1, 0, 0, 1, -(nonClientThickness.Left + nonClientThickness.Right), 0);
                }
                else
                {
                    templateFixupTransform = null;
                }
            }

            if (this._chromeInfo.SacrificialEdge != SacrificialEdge.None)
            {
                if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Top))
                {
                    templateFixupMargin.Top -= SystemParameters2.Current.WindowResizeBorderThickness.Top;
                }

                if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Left))
                {
                    templateFixupMargin.Left -= SystemParameters2.Current.WindowResizeBorderThickness.Left;
                }

                if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Bottom))
                {
                    templateFixupMargin.Bottom -= SystemParameters2.Current.WindowResizeBorderThickness.Bottom;
                }

                if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Right))
                {
                    templateFixupMargin.Right -= SystemParameters2.Current.WindowResizeBorderThickness.Right;
                }
            }

            var rootElement = (FrameworkElement)VisualTreeHelper.GetChild(this.window, 0);
            rootElement.Margin = templateFixupMargin;
            rootElement.RenderTransform = templateFixupTransform;

            if (_IsPresentationFrameworkVersionLessThan4)
            {
                if (!this._isFixedUp)
                {
                    this._hasUserMovedWindow = false;
                    this.window.StateChanged += this._FixupRestoreBounds;

                    this._isFixedUp = true;
                }
            }
        }

        private void _FixupRestoreBounds(object sender, EventArgs e)
        {
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);
            if (this.window.WindowState == WindowState.Maximized || this.window.WindowState == WindowState.Minimized)
            {
                // Old versions of WPF sometimes force their incorrect idea of the Window's location
                // on the Win32 restore bounds.  If we have reason to think this is the case, then
                // try to undo what WPF did after it has done its thing.
                if (this._hasUserMovedWindow)
                {
                    this._hasUserMovedWindow = false;
                    WINDOWPLACEMENT wp = NativeMethods.GetWindowPlacement(this._hwnd);

                    RECT adjustedDeviceRc = this._GetAdjustedWindowRect(new RECT { Bottom = 100, Right = 100 });
                    Point adjustedTopLeft = DpiHelper.DevicePixelsToLogical(
                        new Point(
                            wp.rcNormalPosition.Left - adjustedDeviceRc.Left,
                            wp.rcNormalPosition.Top - adjustedDeviceRc.Top));

                    this.window.Top = adjustedTopLeft.Y;
                    this.window.Left = adjustedTopLeft.X;
                }
            }
        }

        private RECT _GetAdjustedWindowRect(RECT rcWindow)
        {
            // This should only be used to work around issues in the Framework that were fixed in 4.0
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);

            var style = (WS)NativeMethods.GetWindowLongPtr(this._hwnd, GWL.STYLE);
            var exstyle = (WS_EX)NativeMethods.GetWindowLongPtr(this._hwnd, GWL.EXSTYLE);

            return NativeMethods.AdjustWindowRectEx(rcWindow, style, false, exstyle);
        }

        // Windows tries hard to hide this state from applications.
        // Generally you can tell that the window is in a docked position because the restore bounds from GetWindowPlacement
        // don't match the current window location and it's not in a maximized or minimized state.
        // Because this isn't doced or supported, it's also not incredibly consistent.  Sometimes some things get updated in
        // different orders, so this isn't absolutely reliable.
        private bool _IsWindowDocked
        {
            get
            {
                // We're only detecting this state to work around .Net 3.5 issues.
                // This logic won't work correctly when those issues are fixed.
                Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);

                if (this.window.WindowState != WindowState.Normal)
                {
                    return false;
                }

                RECT adjustedOffset = this._GetAdjustedWindowRect(new RECT { Bottom = 100, Right = 100 });
                Point windowTopLeft = new Point(this.window.Left, this.window.Top);
                windowTopLeft -= (Vector)DpiHelper.DevicePixelsToLogical(new Point(adjustedOffset.Left, adjustedOffset.Top));

                return this.window.RestoreBounds.Location != windowTopLeft;
            }
        }

        /// A borderless window lost his animation, with this we bring it back.
        private bool _MinimizeAnimation => SystemParameters.MinimizeAnimation && this._chromeInfo.IgnoreTaskbarOnMaximize == false; // && _chromeInfo.UseNoneWindowStyle == false;

        #region WindowProc and Message Handlers

        private IntPtr _WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Only expecting messages for our cached HWND.
            Assert.AreEqual(hwnd, this._hwnd);

            var message = (WM)msg;
            foreach (var handlePair in this._messageTable)
            {
                if (handlePair.Key == message)
                {
                    return handlePair.Value(message, wParam, lParam, out handled);
                }
            }

            return IntPtr.Zero;
        }

        private IntPtr _HandleSetTextOrIcon(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            bool modified = this._ModifyStyle(WS.VISIBLE, 0);

            // Setting the caption text and icon cause Windows to redraw the caption.
            // Letting the default WndProc handle the message without the WS_VISIBLE
            // style applied bypasses the redraw.
            IntPtr lRet = NativeMethods.DefWindowProc(this._hwnd, uMsg, wParam, lParam);

            // Put back the style we removed.
            if (modified)
            {
                this._ModifyStyle(0, WS.VISIBLE);
            }

            handled = true;
            return lRet;
        }

        private IntPtr _HandleRestoreWindow(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            WINDOWPLACEMENT wpl = NativeMethods.GetWindowPlacement(this._hwnd);
            if (SC.RESTORE == (SC)wParam.ToInt32() && wpl.showCmd == SW.SHOWMAXIMIZED && this._MinimizeAnimation)
            {
                bool modified = this._ModifyStyle(WS.DLGFRAME, 0);

                IntPtr lRet = NativeMethods.DefWindowProc(this._hwnd, uMsg, wParam, lParam);

                // Put back the style we removed.
                if (modified)
                {
                    // allow animation
                    if (this._ModifyStyle(0, WS.DLGFRAME))
                    {
                        this._UpdateFrameState(true);
                    }
                }

                handled = true;
                return lRet;
            }
            else
            {
                handled = false;
                return IntPtr.Zero;
            }
        }

        private IntPtr _HandleNCActivate(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // Despite MSDN's documentation of lParam not being used,
            // calling DefWindowProc with lParam set to -1 causes Windows not to draw over the caption.

            // Directly call DefWindowProc with a custom parameter
            // which bypasses any other handling of the message.
            IntPtr lRet = NativeMethods.DefWindowProc(this._hwnd, WM.NCACTIVATE, wParam, new IntPtr(-1));
            handled = true;
            return lRet;
        }

        /// <summary>
        /// This method handles the window size if the taskbar is set to auto-hide.
        /// </summary>
        private static RECT AdjustWorkingAreaForAutoHide(IntPtr monitorContainingApplication, RECT area)
        {
            // maybe we can use ReBarWindow32 isntead Shell_TrayWnd
            IntPtr hwnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            IntPtr monitorWithTaskbarOnIt = NativeMethods.MonitorFromWindow(hwnd, (uint)MonitorOptions.MONITOR_DEFAULTTONEAREST);

            var abd = new APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = hwnd;
            NativeMethods.SHAppBarMessage((int)ABMsg.ABM_GETTASKBARPOS, ref abd);
            bool autoHide = Convert.ToBoolean(NativeMethods.SHAppBarMessage((int)ABMsg.ABM_GETSTATE, ref abd));

            if (!autoHide || !Equals(monitorContainingApplication, monitorWithTaskbarOnIt))
            {
                return area;
            }

            switch (abd.uEdge)
            {
                case (int)ABEdge.ABE_LEFT:
                    area.Left += 2;
                    break;

                case (int)ABEdge.ABE_RIGHT:
                    area.Right -= 2;
                    break;

                case (int)ABEdge.ABE_TOP:
                    area.Top += 2;
                    break;

                case (int)ABEdge.ABE_BOTTOM:
                    area.Bottom -= 2;
                    break;

                default:
                    return area;
            }

            return area;
        }

        // There was a regression in DWM in Windows 7 with regard to handling WM_NCCALCSIZE to effect custom chrome.
        // When windows with glass are maximized on a multimonitor setup the glass frame tends to turn black.
        // Also when windows are resized they tend to flicker black, sometimes staying that way until resized again.
        //
        // This appears to be a bug in DWM related to device bitmap optimizations.  At least on RTM Win7 we can avoid
        // the problem by making the client area not extactly match the non-client area, so we added the SacrificialEdge property.
        private IntPtr _HandleNCCalcSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // lParam is an [in, out] that can be either a RECT* (wParam == FALSE) or an NCCALCSIZE_PARAMS*.
            // Since the first field of NCCALCSIZE_PARAMS is a RECT and is the only field we care about
            // we can unconditionally treat it as a RECT.
            var redraw = false;

            if (NativeMethods.GetWindowPlacement(this._hwnd).showCmd == SW.MAXIMIZE)
            {
                if (this._MinimizeAnimation)
                {
                    IntPtr mon = NativeMethods.MonitorFromWindow(this._hwnd, (uint)MonitorOptions.MONITOR_DEFAULTTONEAREST);
                    MONITORINFO mi = NativeMethods.GetMonitorInfo(mon);

                    RECT rc = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                    NativeMethods.DefWindowProc(this._hwnd, WM.NCCALCSIZE, wParam, lParam);
                    RECT def = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                    def.Top = (int)(rc.Top + NativeMethods.GetWindowInfo(this._hwnd).cyWindowBorders);

                    // monitor an work area will be equal if taskbar is hidden
                    if (mi.rcMonitor.Height == mi.rcWork.Height && mi.rcMonitor.Width == mi.rcWork.Width)
                    {
                        def = AdjustWorkingAreaForAutoHide(mon, def);
                    }

                    Marshal.StructureToPtr(def, lParam, true);

                    redraw = true;
                }
            }

            if (this._chromeInfo.SacrificialEdge != SacrificialEdge.None)
            {
                Thickness windowResizeBorderThicknessDevice = DpiHelper.LogicalThicknessToDevice(SystemParameters2.Current.WindowResizeBorderThickness);
                var rcClientArea = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Top))
                {
                    rcClientArea.Top += (int)windowResizeBorderThicknessDevice.Top;
                }

                if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Left))
                {
                    rcClientArea.Left += (int)windowResizeBorderThicknessDevice.Left;
                }

                if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Bottom))
                {
                    rcClientArea.Bottom -= (int)windowResizeBorderThicknessDevice.Bottom;
                }

                if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Right))
                {
                    rcClientArea.Right -= (int)windowResizeBorderThicknessDevice.Right;
                }

                Marshal.StructureToPtr(rcClientArea, lParam, false);

                redraw = true;
            }

            handled = true;
            return redraw ? new IntPtr((int)WVR.REDRAW | (int)WVR.VALIDRECTS) : IntPtr.Zero;
        }

        private HT _GetHTFromResizeGripDirection(ResizeGripDirection direction)
        {
            bool compliment = this.window.FlowDirection == FlowDirection.RightToLeft;
            switch (direction)
            {
                case ResizeGripDirection.Bottom:
                    return HT.BOTTOM;

                case ResizeGripDirection.BottomLeft:
                    return compliment ? HT.BOTTOMRIGHT : HT.BOTTOMLEFT;

                case ResizeGripDirection.BottomRight:
                    return compliment ? HT.BOTTOMLEFT : HT.BOTTOMRIGHT;

                case ResizeGripDirection.Left:
                    return compliment ? HT.RIGHT : HT.LEFT;

                case ResizeGripDirection.Right:
                    return compliment ? HT.LEFT : HT.RIGHT;

                case ResizeGripDirection.Top:
                    return HT.TOP;

                case ResizeGripDirection.TopLeft:
                    return compliment ? HT.TOPRIGHT : HT.TOPLEFT;

                case ResizeGripDirection.TopRight:
                    return compliment ? HT.TOPLEFT : HT.TOPRIGHT;

                case ResizeGripDirection.Caption:
                    return HT.CAPTION;

                default:
                    return HT.NOWHERE;
            }
        }

        private IntPtr _HandleNCHitTest(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // Let the system know if we consider the mouse to be in our effective non-client area.
            var mousePosScreen = new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam));
            Rect windowPosition = this._GetWindowRect();

            Point mousePosWindow = mousePosScreen;
            mousePosWindow.Offset(-windowPosition.X, -windowPosition.Y);
            mousePosWindow = DpiHelper.DevicePixelsToLogical(mousePosWindow);

            // If the app is asking for content to be treated as client then that takes precedence over _everything_, even DWM caption buttons.
            // This allows apps to set the glass frame to be non-empty, still cover it with WPF content to hide all the glass,
            // yet still get DWM to draw a drop shadow.
            IInputElement inputElement = this.window.InputHitTest(mousePosWindow);
            if (inputElement != null)
            {
                if (WindowChrome.GetIsHitTestVisibleInChrome(inputElement))
                {
                    handled = true;
                    return new IntPtr((int)HT.CLIENT);
                }

                ResizeGripDirection direction = WindowChrome.GetResizeGripDirection(inputElement);
                if (direction != ResizeGripDirection.None)
                {
                    handled = true;
                    return new IntPtr((int)this._GetHTFromResizeGripDirection(direction));
                }
            }

            // It's not opted out, so offer up the hittest to DWM, then to our custom non-client area logic.
            if (this._chromeInfo.UseAeroCaptionButtons)
            {
                IntPtr lRet;
                if (Utility.IsOSVistaOrNewer && this._chromeInfo.GlassFrameThickness != default(Thickness) && this._isGlassEnabled)
                {
                    // If we're on Vista, give the DWM a chance to handle the message first.
                    handled = NativeMethods.DwmDefWindowProc(this._hwnd, uMsg, wParam, lParam, out lRet);

                    if (IntPtr.Zero != lRet)
                    {
                        // If DWM claims to have handled this, then respect their call.
                        return lRet;
                    }
                }
            }

            HT ht = this._HitTestNca(
                DpiHelper.DeviceRectToLogical(windowPosition),
                DpiHelper.DevicePixelsToLogical(mousePosScreen));

            handled = true;
            return new IntPtr((int)ht);
        }

        private IntPtr _HandleNCRButtonUp(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // Emulate the system behavior of clicking the right mouse button over the caption area
            // to bring up the system menu.
            if (HT.CAPTION == (HT)wParam.ToInt32())
            {
                SystemCommands.ShowSystemMenuPhysicalCoordinates(this.window, new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam)));
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            const int SIZE_MAXIMIZED = 2;

            // Force when maximized.
            // We can tell what's happening right now, but the Window doesn't yet know it's
            // maximized.  Not forcing this update will eventually cause the
            // default caption to be drawn.
            WindowState? state = null;
            if (wParam.ToInt32() == SIZE_MAXIMIZED)
            {
                state = WindowState.Maximized;
            }

            this._UpdateSystemMenu(state);

            // Still let the default WndProc handle this.
            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleWindowPosChanging(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            var wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

            // we don't do bitwise operations cuz we're checking for this flag being the only one there
            // I have no clue why this works, I tried this because VS2013 has this flag removed on fullscreen window movws
            if (this._chromeInfo.IgnoreTaskbarOnMaximize && this._GetHwndState() == WindowState.Maximized && wp.flags == (int)SWP.FRAMECHANGED)
            {
                wp.flags = 0;
                Marshal.StructureToPtr(wp, lParam, true);
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleWindowPosChanged(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // http://blogs.msdn.com/oldnewthing/archive/2008/01/15/7113860.aspx
            // The WM_WINDOWPOSCHANGED message is sent at the end of the window
            // state change process. It sort of combines the other state change
            // notifications, WM_MOVE, WM_SIZE, and WM_SHOWWINDOW. But it doesn't
            // suffer from the same limitations as WM_SHOWWINDOW, so you can
            // reliably use it to react to the window being shown or hidden.
            this._UpdateSystemMenu(null);

            if (!this._isGlassEnabled)
            {
                Assert.IsNotDefault(lParam);
                var wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                if (!wp.Equals(this._previousWP))
                {
                    this._previousWP = wp;
                    this._SetRoundingRegion(wp);
                }

                // if (wp.Equals(_previousWP) && wp.flags.Equals(_previousWP.flags))
                //                {
                //                    handled = true;
                //                    return IntPtr.Zero;
                //                }
                this._previousWP = wp;
            }

            // Still want to pass this to DefWndProc
            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleGetMinMaxInfo(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            /*
             * This is a workaround for wrong windows behaviour.
             * If a Window sets the WindoStyle to None and WindowState to maximized and we have a multi monitor system
             * we can move the Window only one time. After that it's not possible to move the Window back to the
             * previous monitor.
             * This fix is not really a full fix. Moving the Window back gives us the wrong size, because
             * MonitorFromWindow gives us the wrong (old) monitor! This is fixed in _HandleMoveForRealSize.
             */
            var ignoreTaskBar = this._chromeInfo.IgnoreTaskbarOnMaximize; // || _chromeInfo.UseNoneWindowStyle;
            WindowState state = this._GetHwndState();
            if (ignoreTaskBar && state == WindowState.Maximized)
            {
                MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
                IntPtr monitor = NativeMethods.MonitorFromWindow(this._hwnd, (uint)MonitorOptions.MONITOR_DEFAULTTONEAREST);
                if (monitor != IntPtr.Zero)
                {
                    MONITORINFO monitorInfo = NativeMethods.GetMonitorInfoW(monitor);
                    RECT rcWorkArea = monitorInfo.rcWork;
                    RECT rcMonitorArea = monitorInfo.rcMonitor;

                    mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                    mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);

                    mmi.ptMaxSize.x = Math.Abs(monitorInfo.rcMonitor.Width);
                    mmi.ptMaxSize.y = Math.Abs(monitorInfo.rcMonitor.Height);
                    mmi.ptMaxTrackSize.x = mmi.ptMaxSize.x;
                    mmi.ptMaxTrackSize.y = mmi.ptMaxSize.y;
                }

                Marshal.StructureToPtr(mmi, lParam, true);
            }

            /* Setting handled to false enables the application to process it's own Min/Max requirements,
             * as mentioned by jason.bullard (comment from September 22, 2011) on http://gallery.expression.microsoft.com/ZuneWindowBehavior/ */
            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleDwmCompositionChanged(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            this._UpdateFrameState(false);

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleSettingChange(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // There are several settings that can cause fixups for the template to become invalid when changed.
            // These shouldn't be required on the v4 framework.
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);

            this._FixupTemplateIssues();

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleEnterSizeMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // This is only intercepted to deal with bugs in Window in .Net 3.5 and below.
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);

            this._isUserResizing = true;

            // On Win7 if the user is dragging the window out of the maximized state then we don't want to use that location
            // as a restore point.
            Assert.Implies(this.window.WindowState == WindowState.Maximized, Utility.IsOSWindows7OrNewer);
            if (this.window.WindowState != WindowState.Maximized)
            {
                // Check for the docked window case.  The window can still be restored when it's in this position so
                // try to account for that and not update the start position.
                if (!this._IsWindowDocked)
                {
                    this._windowPosAtStartOfUserMove = new Point(this.window.Left, this.window.Top);
                }

                // Realistically we also don't want to update the start position when moving from one docked state to another (or to and from maximized),
                // but it's tricky to detect and this is already a workaround for a bug that's fixed in newer versions of the framework.
                // Not going to try to handle all cases.
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleEnterSizeMoveForAnimation(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            if (this._MinimizeAnimation && this._GetHwndState() == WindowState.Maximized)
            {
                /* we only need to remove DLGFRAME ( CAPTION = BORDER | DLGFRAME )
                 * to prevent nasty drawing
                 * removing border will cause a 1 off error on the client rect size
                 * when maximizing via aero snapping, because max by aero snapping
                 * will call this method, resulting in a 2px black border on the side
                 * when maximized.
                 */
                this._ModifyStyle(WS.DLGFRAME, 0);
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleMoveForRealSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            /*
             * This is a workaround for wrong windows behaviour (with multi monitor system).
             * If a Window sets the WindoStyle to None and WindowState to maximized
             * we can move the Window to different monitor with maybe different dimension.
             * But after moving to the previous monitor we got a wrong size (from the old monitor dimension).
             */
            WindowState state = this._GetHwndState();
            if (state == WindowState.Maximized)
            {
                IntPtr monitorFromWindow = NativeMethods.MonitorFromWindow(this._hwnd, (uint)MonitorOptions.MONITOR_DEFAULTTONEAREST);
                if (monitorFromWindow != IntPtr.Zero)
                {
                    var ignoreTaskBar = this._chromeInfo.IgnoreTaskbarOnMaximize; // || _chromeInfo.UseNoneWindowStyle;
                    MONITORINFO monitorInfo = NativeMethods.GetMonitorInfoW(monitorFromWindow);
                    RECT rcMonitorArea = ignoreTaskBar ? monitorInfo.rcMonitor : monitorInfo.rcWork;
                    /*
                     * ASYNCWINDOWPOS
                     * If the calling thread and the thread that owns the window are attached to different input queues,
                     * the system posts the request to the thread that owns the window. This prevents the calling thread
                     * from blocking its execution while other threads process the request.
                     *
                     * FRAMECHANGED
                     * Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window,
                     * even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only
                     * when the window's size is being changed.
                     *
                     * NOCOPYBITS
                     * Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client
                     * area are saved and copied back into the client area after the window is sized or repositioned.
                     *
                     */
                    NativeMethods.SetWindowPos(this._hwnd, IntPtr.Zero, rcMonitorArea.Left, rcMonitorArea.Top, rcMonitorArea.Width, rcMonitorArea.Height, SWP.ASYNCWINDOWPOS | SWP.FRAMECHANGED | SWP.NOCOPYBITS);
                }
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleExitSizeMoveForAnimation(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            if (this._MinimizeAnimation)
            {
                // restore DLGFRAME
                if (this._ModifyStyle(0, WS.DLGFRAME))
                {
                    this._UpdateFrameState(true);
                }
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleExitSizeMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // This is only intercepted to deal with bugs in Window in .Net 3.5 and below.
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);

            this._isUserResizing = false;

            // On Win7 the user can change the Window's state by dragging the window to the top of the monitor.
            // If they did that, then we need to try to update the restore bounds or else WPF will put the window at the maximized location (e.g. (-8,-8)).
            if (this.window.WindowState == WindowState.Maximized)
            {
                Assert.IsTrue(Utility.IsOSWindows7OrNewer);
                this.window.Top = this._windowPosAtStartOfUserMove.Y;
                this.window.Left = this._windowPosAtStartOfUserMove.X;
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            // This is only intercepted to deal with bugs in Window in .Net 3.5 and below.
            Assert.IsTrue(_IsPresentationFrameworkVersionLessThan4);

            if (this._isUserResizing)
            {
                this._hasUserMovedWindow = true;
            }

            handled = false;
            return IntPtr.Zero;
        }

        #endregion WindowProc and Message Handlers

        /// <summary>Add and remove a native WindowStyle from the HWND.</summary>
        /// <param name="removeStyle">The styles to be removed.  These can be bitwise combined.</param>
        /// <param name="addStyle">The styles to be added.  These can be bitwise combined.</param>
        /// <returns>Whether the styles of the HWND were modified as a result of this call.</returns>
        private bool _ModifyStyle(WS removeStyle, WS addStyle)
        {
            Assert.IsNotDefault(this._hwnd);
            var dwStyle = (WS)NativeMethods.GetWindowLongPtr(this._hwnd, GWL.STYLE).ToInt32();
            var dwNewStyle = (dwStyle & ~removeStyle) | addStyle;
            if (dwStyle == dwNewStyle)
            {
                return false;
            }

            NativeMethods.SetWindowLongPtr(this._hwnd, GWL.STYLE, new IntPtr((int)dwNewStyle));
            return true;
        }

        /// <summary>
        /// Get the WindowState as the native HWND knows it to be.  This isn't necessarily the same as what Window thinks.
        /// </summary>
        private WindowState _GetHwndState()
        {
            var wpl = NativeMethods.GetWindowPlacement(this._hwnd);
            switch (wpl.showCmd)
            {
                case SW.SHOWMINIMIZED: return WindowState.Minimized;
                case SW.SHOWMAXIMIZED: return WindowState.Maximized;
            }

            return WindowState.Normal;
        }

        /// <summary>
        /// Get the bounding rectangle for the window in physical coordinates.
        /// </summary>
        /// <returns>The bounding rectangle for the window.</returns>
        private Rect _GetWindowRect()
        {
            // Get the window rectangle.
            RECT windowPosition = NativeMethods.GetWindowRect(this._hwnd);
            return new Rect(windowPosition.Left, windowPosition.Top, windowPosition.Width, windowPosition.Height);
        }

        /// <summary>
        /// Update the items in the system menu based on the current, or assumed, WindowState.
        /// </summary>
        /// <param name="assumeState">
        /// The state to assume that the Window is in.  This can be null to query the Window's state.
        /// </param>
        /// <remarks>
        /// We want to update the menu while we have some control over whether the caption will be repainted.
        /// </remarks>
        private void _UpdateSystemMenu(WindowState? assumeState)
        {
            const MF mfEnabled = MF.ENABLED | MF.BYCOMMAND;
            const MF mfDisabled = MF.GRAYED | MF.DISABLED | MF.BYCOMMAND;

            WindowState state = assumeState ?? this._GetHwndState();

            if (null != assumeState || this._lastMenuState != state)
            {
                this._lastMenuState = state;

                IntPtr hmenu = NativeMethods.GetSystemMenu(this._hwnd, false);
                if (IntPtr.Zero != hmenu)
                {
                    var dwStyle = (WS)NativeMethods.GetWindowLongPtr(this._hwnd, GWL.STYLE).ToInt32();

                    bool canMinimize = Utility.IsFlagSet((int)dwStyle, (int)WS.MINIMIZEBOX);
                    bool canMaximize = Utility.IsFlagSet((int)dwStyle, (int)WS.MAXIMIZEBOX);
                    bool canSize = Utility.IsFlagSet((int)dwStyle, (int)WS.THICKFRAME);

                    switch (state)
                    {
                        case WindowState.Maximized:
                            NativeMethods.EnableMenuItem(hmenu, SC.RESTORE, mfEnabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MOVE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.SIZE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MINIMIZE, canMinimize ? mfEnabled : mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MAXIMIZE, mfDisabled);
                            break;

                        case WindowState.Minimized:
                            NativeMethods.EnableMenuItem(hmenu, SC.RESTORE, mfEnabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MOVE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.SIZE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MINIMIZE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MAXIMIZE, canMaximize ? mfEnabled : mfDisabled);
                            break;

                        default:
                            NativeMethods.EnableMenuItem(hmenu, SC.RESTORE, mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MOVE, mfEnabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.SIZE, canSize ? mfEnabled : mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MINIMIZE, canMinimize ? mfEnabled : mfDisabled);
                            NativeMethods.EnableMenuItem(hmenu, SC.MAXIMIZE, canMaximize ? mfEnabled : mfDisabled);
                            break;
                    }
                }
            }
        }

        private void _UpdateFrameState(bool force)
        {
            if (IntPtr.Zero == this._hwnd)
            {
                return;
            }

            // Don't rely on SystemParameters2 for this, just make the check ourselves.
            bool frameState = NativeMethods.DwmIsCompositionEnabled();

            if (force || frameState != this._isGlassEnabled)
            {
                this._isGlassEnabled = frameState && this._chromeInfo.GlassFrameThickness != default(Thickness);

                if (!this._isGlassEnabled)
                {
                    this._SetRoundingRegion(null);
                }
                else
                {
                    this._ClearRoundingRegion();
                }

                if (this._MinimizeAnimation)
                {
                    // allow animation
                    this._ModifyStyle(0, WS.CAPTION);
                }
                else
                {
                    // no animation
                    this._ModifyStyle(WS.CAPTION, 0);
                }

                // update the glass frame too, if the user sets the glass frame thickness to 0 at run time
                this._ExtendGlassFrame();

                NativeMethods.SetWindowPos(this._hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpFlags);
            }
        }

        private void _ClearRoundingRegion()
        {
            NativeMethods.SetWindowRgn(this._hwnd, IntPtr.Zero, NativeMethods.IsWindowVisible(this._hwnd));
        }

        private static RECT _GetClientRectRelativeToWindowRect(IntPtr hWnd)
        {
            RECT windowRect = NativeMethods.GetWindowRect(hWnd);
            RECT clientRect = NativeMethods.GetClientRect(hWnd);

            POINT test = new POINT() { x = 0, y = 0 };
            NativeMethods.ClientToScreen(hWnd, ref test);
            clientRect.Offset(test.x - windowRect.Left, test.y - windowRect.Top);
            return clientRect;
        }

        private void _SetRoundingRegion(WINDOWPOS? wp)
        {
            // We're early - WPF hasn't necessarily updated the state of the window.
            // Need to query it ourselves.
            WINDOWPLACEMENT wpl = NativeMethods.GetWindowPlacement(this._hwnd);

            if (wpl.showCmd == SW.SHOWMAXIMIZED)
            {
                RECT rcMax;
                if (this._MinimizeAnimation)
                {
                    rcMax = _GetClientRectRelativeToWindowRect(this._hwnd);
                }
                else
                {
                    int left;
                    int top;

                    if (wp.HasValue)
                    {
                        left = wp.Value.x;
                        top = wp.Value.y;
                    }
                    else
                    {
                        Rect r = this._GetWindowRect();
                        left = (int)r.Left;
                        top = (int)r.Top;
                    }

                    IntPtr hMon = NativeMethods.MonitorFromWindow(this._hwnd, (uint)MonitorOptions.MONITOR_DEFAULTTONEAREST);

                    MONITORINFO mi = NativeMethods.GetMonitorInfo(hMon);
                    rcMax = this._chromeInfo.IgnoreTaskbarOnMaximize ? mi.rcMonitor : mi.rcWork;

                    // The location of maximized window takes into account the border that Windows was
                    // going to remove, so we also need to consider it.
                    rcMax.Offset(-left, -top);
                }

                IntPtr hrgn = IntPtr.Zero;
                try
                {
                    hrgn = NativeMethods.CreateRectRgnIndirect(rcMax);
                    NativeMethods.SetWindowRgn(this._hwnd, hrgn, NativeMethods.IsWindowVisible(this._hwnd));
                    hrgn = IntPtr.Zero;
                }
                finally
                {
                    Utility.SafeDeleteObject(ref hrgn);
                }
            }
            else
            {
                Size windowSize;

                // Use the size if it's specified.
                if (null != wp && !Utility.IsFlagSet(wp.Value.flags, (int)SWP.NOSIZE))
                {
                    windowSize = new Size((double)wp.Value.cx, (double)wp.Value.cy);
                }
                else if (null != wp && (this._lastRoundingState == this.window.WindowState))
                {
                    return;
                }
                else
                {
                    windowSize = this._GetWindowRect().Size;
                }

                this._lastRoundingState = this.window.WindowState;

                IntPtr hrgn = IntPtr.Zero;
                try
                {
                    double shortestDimension = Math.Min(windowSize.Width, windowSize.Height);

                    double topLeftRadius = DpiHelper.LogicalPixelsToDevice(new Point(this._chromeInfo.CornerRadius.TopLeft, 0)).X;
                    topLeftRadius = Math.Min(topLeftRadius, shortestDimension / 2);

                    if (_IsUniform(this._chromeInfo.CornerRadius))
                    {
                        // RoundedRect HRGNs require an additional pixel of padding.
                        hrgn = _CreateRoundRectRgn(new Rect(windowSize), topLeftRadius);
                    }
                    else
                    {
                        // We need to combine HRGNs for each of the corners.
                        // Create one for each quadrant, but let it overlap into the two adjacent ones
                        // by the radius amount to ensure that there aren't corners etched into the middle
                        // of the window.
                        hrgn = _CreateRoundRectRgn(new Rect(0, 0, (windowSize.Width / 2) + topLeftRadius, (windowSize.Height / 2) + topLeftRadius), topLeftRadius);

                        double topRightRadius = DpiHelper.LogicalPixelsToDevice(new Point(this._chromeInfo.CornerRadius.TopRight, 0)).X;
                        topRightRadius = Math.Min(topRightRadius, shortestDimension / 2);
                        Rect topRightRegionRect = new Rect(0, 0, (windowSize.Width / 2) + topRightRadius, (windowSize.Height / 2) + topRightRadius);
                        topRightRegionRect.Offset((windowSize.Width / 2) - topRightRadius, 0);
                        Assert.AreEqual(topRightRegionRect.Right, windowSize.Width);

                        _CreateAndCombineRoundRectRgn(hrgn, topRightRegionRect, topRightRadius);

                        double bottomLeftRadius = DpiHelper.LogicalPixelsToDevice(new Point(this._chromeInfo.CornerRadius.BottomLeft, 0)).X;
                        bottomLeftRadius = Math.Min(bottomLeftRadius, shortestDimension / 2);
                        Rect bottomLeftRegionRect = new Rect(0, 0, (windowSize.Width / 2) + bottomLeftRadius, (windowSize.Height / 2) + bottomLeftRadius);
                        bottomLeftRegionRect.Offset(0, (windowSize.Height / 2) - bottomLeftRadius);
                        Assert.AreEqual(bottomLeftRegionRect.Bottom, windowSize.Height);

                        _CreateAndCombineRoundRectRgn(hrgn, bottomLeftRegionRect, bottomLeftRadius);

                        double bottomRightRadius = DpiHelper.LogicalPixelsToDevice(new Point(this._chromeInfo.CornerRadius.BottomRight, 0)).X;
                        bottomRightRadius = Math.Min(bottomRightRadius, shortestDimension / 2);
                        Rect bottomRightRegionRect = new Rect(0, 0, (windowSize.Width / 2) + bottomRightRadius, (windowSize.Height / 2) + bottomRightRadius);
                        bottomRightRegionRect.Offset((windowSize.Width / 2) - bottomRightRadius, (windowSize.Height / 2) - bottomRightRadius);
                        Assert.AreEqual(bottomRightRegionRect.Right, windowSize.Width);
                        Assert.AreEqual(bottomRightRegionRect.Bottom, windowSize.Height);

                        _CreateAndCombineRoundRectRgn(hrgn, bottomRightRegionRect, bottomRightRadius);
                    }

                    NativeMethods.SetWindowRgn(this._hwnd, hrgn, NativeMethods.IsWindowVisible(this._hwnd));
                    hrgn = IntPtr.Zero;
                }
                finally
                {
                    // Free the memory associated with the HRGN if it wasn't assigned to the HWND.
                    Utility.SafeDeleteObject(ref hrgn);
                }
            }
        }

        private static IntPtr _CreateRoundRectRgn(Rect region, double radius)
        {
            // Round outwards.
            if (DoubleUtilities.AreClose(0, radius))
            {
                return NativeMethods.CreateRectRgn(
                    (int)Math.Floor(region.Left),
                    (int)Math.Floor(region.Top),
                    (int)Math.Ceiling(region.Right),
                    (int)Math.Ceiling(region.Bottom));
            }

            // RoundedRect HRGNs require an additional pixel of padding on the bottom right to look correct.
            return NativeMethods.CreateRoundRectRgn(
                (int)Math.Floor(region.Left),
                (int)Math.Floor(region.Top),
                (int)Math.Ceiling(region.Right) + 1,
                (int)Math.Ceiling(region.Bottom) + 1,
                (int)Math.Ceiling(radius),
                (int)Math.Ceiling(radius));
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "HRGNs", Justification = "ToDo")]
        private static void _CreateAndCombineRoundRectRgn(IntPtr hrgnSource, Rect region, double radius)
        {
            IntPtr hrgn = IntPtr.Zero;
            try
            {
                hrgn = _CreateRoundRectRgn(region, radius);
                CombineRgnResult result = NativeMethods.CombineRgn(hrgnSource, hrgnSource, hrgn, RGN.OR);
                if (result == CombineRgnResult.ERROR)
                {
                    throw new InvalidOperationException("Unable to combine two HRGNs.");
                }
            }
            finally
            {
                Utility.SafeDeleteObject(ref hrgn);
            }
        }

        private static bool _IsUniform(CornerRadius cornerRadius)
        {
            if (!DoubleUtilities.AreClose(cornerRadius.BottomLeft, cornerRadius.BottomRight))
            {
                return false;
            }

            if (!DoubleUtilities.AreClose(cornerRadius.TopLeft, cornerRadius.TopRight))
            {
                return false;
            }

            if (!DoubleUtilities.AreClose(cornerRadius.BottomLeft, cornerRadius.TopRight))
            {
                return false;
            }

            return true;
        }

        private void _ExtendGlassFrame()
        {
            Assert.IsNotNull(this.window);

            // Expect that this might be called on OSes other than Vista.
            if (!Utility.IsOSVistaOrNewer)
            {
                // Not an error.  Just not on Vista so we're not going to get glass.
                return;
            }

            if (IntPtr.Zero == this._hwnd)
            {
                // Can't do anything with this call until the Window has been shown.
                return;
            }

            // Ensure standard HWND background painting when DWM isn't enabled.
            if (!NativeMethods.DwmIsCompositionEnabled())
            {
                // Apply the transparent background to the HWND for disabled DwmIsComposition too
                // but only if the window has the flag AllowsTransparency turned on
                if (this.window.AllowsTransparency)
                {
                    this._hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;
                }
                else
                {
                    this._hwndSource.CompositionTarget.BackgroundColor = SystemColors.WindowColor;
                }
            }
            else
            {
                // This makes the glass visible at a Win32 level so long as nothing else is covering it.
                // The Window's Background needs to be changed independent of this.

                // Apply the transparent background to the HWND
                // but only if the window has the flag AllowsTransparency turned on
                if (this.window.AllowsTransparency)
                {
                    this._hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;
                }

                // Thickness is going to be DIPs, need to convert to system coordinates.
                Thickness deviceGlassThickness = DpiHelper.LogicalThicknessToDevice(this._chromeInfo.GlassFrameThickness);

                if (this._chromeInfo.SacrificialEdge != SacrificialEdge.None)
                {
                    Thickness windowResizeBorderThicknessDevice = DpiHelper.LogicalThicknessToDevice(SystemParameters2.Current.WindowResizeBorderThickness);
                    if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Top))
                    {
                        deviceGlassThickness.Top -= windowResizeBorderThicknessDevice.Top;
                        deviceGlassThickness.Top = Math.Max(0, deviceGlassThickness.Top);
                    }

                    if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Left))
                    {
                        deviceGlassThickness.Left -= windowResizeBorderThicknessDevice.Left;
                        deviceGlassThickness.Left = Math.Max(0, deviceGlassThickness.Left);
                    }

                    if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Bottom))
                    {
                        deviceGlassThickness.Bottom -= windowResizeBorderThicknessDevice.Bottom;
                        deviceGlassThickness.Bottom = Math.Max(0, deviceGlassThickness.Bottom);
                    }

                    if (Utility.IsFlagSet((int)this._chromeInfo.SacrificialEdge, (int)SacrificialEdge.Right))
                    {
                        deviceGlassThickness.Right -= windowResizeBorderThicknessDevice.Right;
                        deviceGlassThickness.Right = Math.Max(0, deviceGlassThickness.Right);
                    }
                }

                var dwmMargin = new MARGINS
                {
                    // err on the side of pushing in glass an extra pixel.
                    cxLeftWidth = (int)Math.Ceiling(deviceGlassThickness.Left),
                    cxRightWidth = (int)Math.Ceiling(deviceGlassThickness.Right),
                    cyTopHeight = (int)Math.Ceiling(deviceGlassThickness.Top),
                    cyBottomHeight = (int)Math.Ceiling(deviceGlassThickness.Bottom),
                };

                NativeMethods.DwmExtendFrameIntoClientArea(this._hwnd, ref dwmMargin);
            }
        }

        /// <summary>
        /// Matrix of the HT values to return when responding to NC window messages.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member", Justification = "ToDo")]
        private static readonly HT[,] _HitTestBorders = new[,]
        {
            { HT.TOPLEFT,    HT.TOP,     HT.TOPRIGHT    },
            { HT.LEFT,       HT.CLIENT,  HT.RIGHT       },
            { HT.BOTTOMLEFT, HT.BOTTOM,  HT.BOTTOMRIGHT },
        };

        private HT _HitTestNca(Rect windowPosition, Point mousePosition)
        {
            // Determine if hit test is for resizing, default middle (1,1).
            int uRow = 1;
            int uCol = 1;
            bool onResizeBorder = false;

            // Determine if the point is at the top or bottom of the window.
            if (mousePosition.Y >= windowPosition.Top && mousePosition.Y < windowPosition.Top + this._chromeInfo.ResizeBorderThickness.Top + this._chromeInfo.CaptionHeight)
            {
                onResizeBorder = mousePosition.Y < (windowPosition.Top + this._chromeInfo.ResizeBorderThickness.Top);
                uRow = 0; // top (caption or resize border)
            }
            else if (mousePosition.Y < windowPosition.Bottom && mousePosition.Y >= windowPosition.Bottom - (int)this._chromeInfo.ResizeBorderThickness.Bottom)
            {
                uRow = 2; // bottom
            }

            // Determine if the point is at the left or right of the window.
            if (mousePosition.X >= windowPosition.Left && mousePosition.X < windowPosition.Left + (int)this._chromeInfo.ResizeBorderThickness.Left)
            {
                uCol = 0; // left side
            }
            else if (mousePosition.X < windowPosition.Right && mousePosition.X >= windowPosition.Right - this._chromeInfo.ResizeBorderThickness.Right)
            {
                uCol = 2; // right side
            }

            // If the cursor is in one of the top edges by the caption bar, but below the top resize border,
            // then resize left-right rather than diagonally.
            if (uRow == 0 && uCol != 1 && !onResizeBorder)
            {
                uRow = 1;
            }

            HT ht = _HitTestBorders[uRow, uCol];

            if (ht == HT.TOP && !onResizeBorder)
            {
                ht = HT.CAPTION;
            }

            return ht;
        }

        #region Remove Custom Chrome Methods

        private void _RestoreStandardChromeState(bool isClosing)
        {
            this.VerifyAccess();

            this._UnhookCustomChrome();

            if (!isClosing)
            {
                this._RestoreTemplateFixups();
                this._RestoreGlassFrame();
                this._RestoreHrgn();

                this.window.InvalidateMeasure();
            }
        }

        private void _UnhookCustomChrome()
        {
            Assert.IsNotNull(this.window);

            if (this._isHooked)
            {
                Assert.IsNotDefault(this._hwnd);
                Assert.IsNotNull(this._hwndSource);

                this._hwndSource.RemoveHook(this._WndProc);
                this._isHooked = false;
            }
        }

        private void _RestoreTemplateFixups()
        {
            // This margin is only necessary if the client rect is going to be calculated incorrectly by WPF.
            // This bug was fixed in V4 of the framework.
            // But it still needs to happen if there was a SacrificialEdge.

            // Assert.IsTrue(_isFixedUp);
            Assert.Implies(_IsPresentationFrameworkVersionLessThan4, () => this._isFixedUp);

            var rootElement = (FrameworkElement)VisualTreeHelper.GetChild(this.window, 0);

            // Undo anything that was done before.
            rootElement.Margin = new Thickness();

            this.window.StateChanged -= this._FixupRestoreBounds;
            this._isFixedUp = false;
        }

        private void _RestoreGlassFrame()
        {
            Assert.IsNull(this._chromeInfo);
            Assert.IsNotNull(this.window);

            // Expect that this might be called on OSes other than Vista
            // and if the window hasn't yet been shown, then we don't need to undo anything.
            if (!Utility.IsOSVistaOrNewer || this._hwnd == IntPtr.Zero)
            {
                return;
            }

            this._hwndSource.CompositionTarget.BackgroundColor = SystemColors.WindowColor;

            if (NativeMethods.DwmIsCompositionEnabled())
            {
                // If glass is enabled, push it back to the normal bounds.
                var dwmMargin = new MARGINS();
                NativeMethods.DwmExtendFrameIntoClientArea(this._hwnd, ref dwmMargin);
            }
        }

        private void _RestoreHrgn()
        {
            this._ClearRoundingRegion();
            NativeMethods.SetWindowPos(this._hwnd, IntPtr.Zero, 0, 0, 0, 0, SwpFlags);
        }

        #endregion Remove Custom Chrome Methods
    }
}