/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace MS.Windows.Shell
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;
    using Standard;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "ToDo")]
    public class SystemParameters2 : INotifyPropertyChanged
    {
        private delegate void _SystemMetricUpdate(IntPtr wParam, IntPtr lParam);

        [ThreadStatic]
        private static SystemParameters2 threadLocalSingleton;

        private MessageWindow messageHwnd;

        private bool isGlassEnabled;
        private Color glassColor;
        private SolidColorBrush glassColorBrush;
        private Thickness windowResizeBorderThickness;
        private Thickness windowNonClientFrameThickness;
        private double captionHeight;
        private Size smallIconSize;
        private string uxThemeName;
        private string uxThemeColor;
        private bool isHighContrast;
        private CornerRadius windowCornerRadius;
        private Rect captionButtonLocation;

        private readonly Dictionary<WM, List<_SystemMetricUpdate>> UpdateTable;

        #region Initialization and Update Methods

        // Most properties exposed here have a way of being queried directly
        // and a way of being notified of updates via a window message.
        // This region is a grouping of both, for each of the exposed properties.
        private void _InitializeIsGlassEnabled()
        {
            this.IsGlassEnabled = NativeMethods.DwmIsCompositionEnabled();
        }

        private void _UpdateIsGlassEnabled(IntPtr wParam, IntPtr lParam)
        {
            // Neither the wParam or lParam are used in this case.
            this._InitializeIsGlassEnabled();
        }

        private void _InitializeGlassColor()
        {
            bool isOpaque;
            uint color;
            NativeMethods.DwmGetColorizationColor(out color, out isOpaque);
            color |= isOpaque ? 0xFF000000 : 0;

            this.WindowGlassColor = Utility.ColorFromArgbDword(color);

            var glassBrush = new SolidColorBrush(this.WindowGlassColor);
            glassBrush.Freeze();

            this.WindowGlassBrush = glassBrush;
        }

        private void _UpdateGlassColor(IntPtr wParam, IntPtr lParam)
        {
            bool isOpaque = lParam != IntPtr.Zero;
            uint color = unchecked((uint)(int)wParam.ToInt64());
            color |= isOpaque ? 0xFF000000 : 0;
            this.WindowGlassColor = Utility.ColorFromArgbDword(color);
            var glassBrush = new SolidColorBrush(this.WindowGlassColor);
            glassBrush.Freeze();
            this.WindowGlassBrush = glassBrush;
        }

        private void _InitializeCaptionHeight()
        {
            Point ptCaption = new Point(0, NativeMethods.GetSystemMetrics(SM.CYCAPTION));
            this.WindowCaptionHeight = DpiHelper.DevicePixelsToLogical(ptCaption).Y;
        }

        private void _UpdateCaptionHeight(IntPtr wParam, IntPtr lParam)
        {
            this._InitializeCaptionHeight();
        }

        private void _InitializeWindowResizeBorderThickness()
        {
            Size frameSize = new Size(
                NativeMethods.GetSystemMetrics(SM.CXSIZEFRAME),
                NativeMethods.GetSystemMetrics(SM.CYSIZEFRAME));
            Size frameSizeInDips = DpiHelper.DeviceSizeToLogical(frameSize);
            this.WindowResizeBorderThickness = new Thickness(frameSizeInDips.Width, frameSizeInDips.Height, frameSizeInDips.Width, frameSizeInDips.Height);
        }

        private void _UpdateWindowResizeBorderThickness(IntPtr wParam, IntPtr lParam)
        {
            this._InitializeWindowResizeBorderThickness();
        }

        private void _InitializeWindowNonClientFrameThickness()
        {
            Size frameSize = new Size(
                NativeMethods.GetSystemMetrics(SM.CXSIZEFRAME),
                NativeMethods.GetSystemMetrics(SM.CYSIZEFRAME));
            Size frameSizeInDips = DpiHelper.DeviceSizeToLogical(frameSize);
            int captionHeight = NativeMethods.GetSystemMetrics(SM.CYCAPTION);
            double captionHeightInDips = DpiHelper.DevicePixelsToLogical(new Point(0, captionHeight)).Y;
            this.WindowNonClientFrameThickness = new Thickness(frameSizeInDips.Width, frameSizeInDips.Height + captionHeightInDips, frameSizeInDips.Width, frameSizeInDips.Height);
        }

        private void _UpdateWindowNonClientFrameThickness(IntPtr wParam, IntPtr lParam)
        {
            this._InitializeWindowNonClientFrameThickness();
        }

        private void _InitializeSmallIconSize()
        {
            this.SmallIconSize = new Size(
                NativeMethods.GetSystemMetrics(SM.CXSMICON),
                NativeMethods.GetSystemMetrics(SM.CYSMICON));
        }

        private void _UpdateSmallIconSize(IntPtr wParam, IntPtr lParam)
        {
            this._InitializeSmallIconSize();
        }

        private void _LegacyInitializeCaptionButtonLocation()
        {
            // This calculation isn't quite right, but it's pretty close.
            // I expect this is good enough for the scenarios where this is expected to be used.
            int captionX = NativeMethods.GetSystemMetrics(SM.CXSIZE);
            int captionY = NativeMethods.GetSystemMetrics(SM.CYSIZE);

            int frameX = NativeMethods.GetSystemMetrics(SM.CXSIZEFRAME) + NativeMethods.GetSystemMetrics(SM.CXEDGE);
            int frameY = NativeMethods.GetSystemMetrics(SM.CYSIZEFRAME) + NativeMethods.GetSystemMetrics(SM.CYEDGE);

            Rect captionRect = new Rect(0, 0, captionX * 3, captionY);
            captionRect.Offset(-frameX - captionRect.Width, frameY);

            this.WindowCaptionButtonsLocation = captionRect;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "ToDo")]
        private void _InitializeCaptionButtonLocation()
        {
            // There is a completely different way to do this on XP.
            if (!Utility.IsOSVistaOrNewer || !NativeMethods.IsThemeActive())
            {
                this._LegacyInitializeCaptionButtonLocation();
                return;
            }

            var tbix = new TITLEBARINFOEX { cbSize = Marshal.SizeOf(typeof(TITLEBARINFOEX)) };
            IntPtr lParam = Marshal.AllocHGlobal(tbix.cbSize);
            try
            {
                Marshal.StructureToPtr(tbix, lParam, false);

                // This might flash a window in the taskbar while being calculated.
                // WM_GETTITLEBARINFOEX doesn't work correctly unless the window is visible while processing.
                NativeMethods.ShowWindow(this.messageHwnd.Handle, SW.SHOW);
                NativeMethods.SendMessage(this.messageHwnd.Handle, WM.GETTITLEBARINFOEX, IntPtr.Zero, lParam);
                tbix = (TITLEBARINFOEX)Marshal.PtrToStructure(lParam, typeof(TITLEBARINFOEX));
            }
            finally
            {
                NativeMethods.ShowWindow(this.messageHwnd.Handle, SW.HIDE);
                Utility.SafeFreeHGlobal(ref lParam);
            }

            // TITLEBARINFOEX has information relative to the screen.  We need to convert the containing rect
            // to instead be relative to the top-right corner of the window.
            RECT rcAllCaptionButtons = RECT.Union(tbix.rgrect_CloseButton, tbix.rgrect_MinimizeButton);

            // For all known themes, the RECT for the maximize box shouldn't add anything to the union of the minimize and close boxes.
            Assert.AreEqual(rcAllCaptionButtons, RECT.Union(rcAllCaptionButtons, tbix.rgrect_MaximizeButton));

            RECT rcWindow = NativeMethods.GetWindowRect(this.messageHwnd.Handle);

            // Reorient the Top/Right to be relative to the top right edge of the Window.
            var deviceCaptionLocation = new Rect(
                rcAllCaptionButtons.Left - rcWindow.Width - rcWindow.Left,
                rcAllCaptionButtons.Top - rcWindow.Top,
                rcAllCaptionButtons.Width,
                rcAllCaptionButtons.Height);

            Rect logicalCaptionLocation = DpiHelper.DeviceRectToLogical(deviceCaptionLocation);

            this.WindowCaptionButtonsLocation = logicalCaptionLocation;
        }

        private void _UpdateCaptionButtonLocation(IntPtr wParam, IntPtr lParam)
        {
            this._InitializeCaptionButtonLocation();
        }

        private void _InitializeHighContrast()
        {
            HIGHCONTRAST hc = NativeMethods.SystemParameterInfo_GetHIGHCONTRAST();
            this.HighContrast = (hc.dwFlags & HCF.HIGHCONTRASTON) != 0;
        }

        private void _UpdateHighContrast(IntPtr wParam, IntPtr lParam)
        {
            this._InitializeHighContrast();
        }

        private void _InitializeThemeInfo()
        {
            if (!NativeMethods.IsThemeActive())
            {
                this.UxThemeName = "Classic";
                this.UxThemeColor = string.Empty;
                return;
            }

            string name;
            string color;
            string size;
            NativeMethods.GetCurrentThemeName(out name, out color, out size);

            // Consider whether this is the most useful way to expose this...
            this.UxThemeName = System.IO.Path.GetFileNameWithoutExtension(name);
            this.UxThemeColor = color;
        }

        private void _UpdateThemeInfo(IntPtr wParam, IntPtr lParam)
        {
            this._InitializeThemeInfo();
        }

        private void _InitializeWindowCornerRadius()
        {
            // The radius of window corners isn't exposed as a true system parameter.
            // It instead is a logical size that we're approximating based on the current theme.
            // There aren't any known variations based on theme color.
            Assert.IsNeitherNullNorEmpty(this.UxThemeName);

            // These radii are approximate.  The way WPF does rounding is different than how
            //     rounded-rectangle HRGNs are created, which is also different than the actual
            //     round corners on themed Windows.  For now we're not exposing anything to
            //     mitigate the differences.
            var cornerRadius = default(CornerRadius);

            // This list is known to be incomplete and very much not future-proof.
            // On XP there are at least a couple of shipped themes that this won't catch,
            // "Zune" and "Royale", but WPF doesn't know about these either.
            // If a new theme was to replace Aero, then this will fall back on "classic" behaviors.
            // This isn't ideal, but it's not the end of the world.  WPF will generally have problems anyways.
            switch (this.UxThemeName.ToUpperInvariant())
            {
                case "LUNA":
                    cornerRadius = new CornerRadius(6, 6, 0, 0);
                    break;

                case "AERO":
                    // Aero has two cases.  One with glass and one without...
                    if (NativeMethods.DwmIsCompositionEnabled())
                    {
                        cornerRadius = new CornerRadius(8);
                    }
                    else
                    {
                        cornerRadius = new CornerRadius(6, 6, 0, 0);
                    }

                    break;

                case "CLASSIC":
                case "ZUNE":
                case "ROYALE":
                default:
                    cornerRadius = new CornerRadius(0);
                    break;
            }

            this.WindowCornerRadius = cornerRadius;
        }

        private void _UpdateWindowCornerRadius(IntPtr wParam, IntPtr lParam)
        {
            // Neither the wParam or lParam are used in this case.
            this._InitializeWindowCornerRadius();
        }

        #endregion Initialization and Update Methods

        /// <summary>
        /// Private constructor.  The public way to access this class is through the static Current property.
        /// </summary>
        private SystemParameters2()
        {
            // This window gets used for calculations about standard caption button locations
            // so it has WS_OVERLAPPEDWINDOW as a style to give it normal caption buttons.
            // This window may be shown during calculations of caption bar information, so create it at a location that's likely offscreen.
            this.messageHwnd = new MessageWindow((CS)0, WS.OVERLAPPEDWINDOW | WS.DISABLED, (WS_EX)0, new Rect(-16000, -16000, 100, 100), string.Empty, this._WndProc);
            this.messageHwnd.Dispatcher.ShutdownStarted += (sender, e) => Utility.SafeDispose(ref this.messageHwnd);

            // Fixup the default values of the DPs.
            this._InitializeIsGlassEnabled();
            this._InitializeGlassColor();
            this._InitializeCaptionHeight();
            this._InitializeWindowNonClientFrameThickness();
            this._InitializeWindowResizeBorderThickness();
            this._InitializeCaptionButtonLocation();
            this._InitializeSmallIconSize();
            this._InitializeHighContrast();
            this._InitializeThemeInfo();

            // WindowCornerRadius isn't exposed by true system parameters, so it requires the theme to be initialized first.
            this._InitializeWindowCornerRadius();

            this.UpdateTable = new Dictionary<WM, List<_SystemMetricUpdate>>
            {
                {
                    WM.THEMECHANGED,
                    new List<_SystemMetricUpdate>
                    {
                        this._UpdateThemeInfo,
                        this._UpdateHighContrast,
                        this._UpdateWindowCornerRadius,
                        this._UpdateCaptionButtonLocation,
                    }
                },
                {
                    WM.SETTINGCHANGE,
                    new List<_SystemMetricUpdate>
                    {
                        this._UpdateCaptionHeight,
                        this._UpdateWindowResizeBorderThickness,
                        this._UpdateSmallIconSize,
                        this._UpdateHighContrast,
                        this._UpdateWindowNonClientFrameThickness,
                        this._UpdateCaptionButtonLocation,
                    }
                },
                { WM.DWMNCRENDERINGCHANGED, new List<_SystemMetricUpdate> { this._UpdateIsGlassEnabled } },
                { WM.DWMCOMPOSITIONCHANGED, new List<_SystemMetricUpdate> { this._UpdateIsGlassEnabled } },
                { WM.DWMCOLORIZATIONCOLORCHANGED, new List<_SystemMetricUpdate> { this._UpdateGlassColor } },
            };
        }

        public static SystemParameters2 Current
        {
            get
            {
                if (threadLocalSingleton == null)
                {
                    threadLocalSingleton = new SystemParameters2();
                }

                return threadLocalSingleton;
            }
        }

        private IntPtr _WndProc(IntPtr hwnd, WM msg, IntPtr wParam, IntPtr lParam)
        {
            // Don't do this if called within the SystemParameters2 constructor
            if (this.UpdateTable != null)
            {
                List<_SystemMetricUpdate> handlers;
                if (this.UpdateTable.TryGetValue(msg, out handlers))
                {
                    Assert.IsNotNull(handlers);
                    foreach (var handler in handlers)
                    {
                        handler(wParam, lParam);
                    }
                }
            }

            return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        public bool IsGlassEnabled
        {
            get =>
                // return _isGlassEnabled;
                // It turns out there may be some lag between someone asking this
                // and the window getting updated.  It's not too expensive, just always do the check.
                NativeMethods.DwmIsCompositionEnabled();

            private set
            {
                if (value != this.isGlassEnabled)
                {
                    this.isGlassEnabled = value;
                    this._NotifyPropertyChanged(nameof(this.IsGlassEnabled));
                }
            }
        }

        public Color WindowGlassColor
        {
            get => this.glassColor;

            private set
            {
                if (value != this.glassColor)
                {
                    this.glassColor = value;
                    this._NotifyPropertyChanged(nameof(this.WindowGlassColor));
                }
            }
        }

        public SolidColorBrush WindowGlassBrush
        {
            get => this.glassColorBrush;

            private set
            {
                Assert.IsNotNull(value);
                Assert.IsTrue(value.IsFrozen);
                if (this.glassColorBrush == null || value.Color != this.glassColorBrush.Color)
                {
                    this.glassColorBrush = value;
                    this._NotifyPropertyChanged(nameof(this.WindowGlassBrush));
                }
            }
        }

        public Thickness WindowResizeBorderThickness
        {
            get => this.windowResizeBorderThickness;

            private set
            {
                if (value != this.windowResizeBorderThickness)
                {
                    this.windowResizeBorderThickness = value;
                    this._NotifyPropertyChanged(nameof(this.WindowResizeBorderThickness));
                }
            }
        }

        public Thickness WindowNonClientFrameThickness
        {
            get => this.windowNonClientFrameThickness;

            private set
            {
                if (value != this.windowNonClientFrameThickness)
                {
                    this.windowNonClientFrameThickness = value;
                    this._NotifyPropertyChanged(nameof(this.WindowNonClientFrameThickness));
                }
            }
        }

        public double WindowCaptionHeight
        {
            get => this.captionHeight;

            private set
            {
                if (value != this.captionHeight)
                {
                    this.captionHeight = value;
                    this._NotifyPropertyChanged(nameof(this.WindowCaptionHeight));
                }
            }
        }

        public Size SmallIconSize
        {
            get => new Size(this.smallIconSize.Width, this.smallIconSize.Height);

            private set
            {
                if (value != this.smallIconSize)
                {
                    this.smallIconSize = value;
                    this._NotifyPropertyChanged(nameof(this.SmallIconSize));
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ux", Justification = "ToDo")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ux", Justification = "ToDo")]
        public string UxThemeName
        {
            get => this.uxThemeName;

            private set
            {
                if (value != this.uxThemeName)
                {
                    this.uxThemeName = value;
                    this._NotifyPropertyChanged(nameof(this.UxThemeName));
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ux", Justification = "ToDo")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ux", Justification = "ToDo")]
        public string UxThemeColor
        {
            get => this.uxThemeColor;

            private set
            {
                if (value != this.uxThemeColor)
                {
                    this.uxThemeColor = value;
                    this._NotifyPropertyChanged(nameof(this.UxThemeColor));
                }
            }
        }

        public bool HighContrast
        {
            get => this.isHighContrast;

            private set
            {
                if (value != this.isHighContrast)
                {
                    this.isHighContrast = value;
                    this._NotifyPropertyChanged(nameof(this.HighContrast));
                }
            }
        }

        public CornerRadius WindowCornerRadius
        {
            get => this.windowCornerRadius;

            private set
            {
                if (value != this.windowCornerRadius)
                {
                    this.windowCornerRadius = value;
                    this._NotifyPropertyChanged(nameof(this.WindowCornerRadius));
                }
            }
        }

        public Rect WindowCaptionButtonsLocation
        {
            get => this.captionButtonLocation;

            private set
            {
                if (value != this.captionButtonLocation)
                {
                    this.captionButtonLocation = value;
                    this._NotifyPropertyChanged(nameof(this.WindowCaptionButtonsLocation));
                }
            }
        }

        #region INotifyPropertyChanged Members

        private void _NotifyPropertyChanged(string propertyName)
        {
            Assert.IsNeitherNullNorEmpty(propertyName);
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged Members
    }
}