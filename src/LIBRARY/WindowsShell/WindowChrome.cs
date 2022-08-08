/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace MS.Windows.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Data;
    using Standard;

    public enum ResizeGripDirection
    {
        None,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        Caption,
    }

    [Flags]
    public enum SacrificialEdge
    {
        None = 0,
        Left = 1,
        Top = 2,
        Right = 4,
        Bottom = 8,

        Office = Left | Right | Bottom,

        // Don't use "All" - Handling WM_NCCALCSIZE with a client rect shrunk in all directions implicitly creates a
        // normal sized caption area that doesn't actually properly participate with the rest of the implementation...
        // All = Left | Top | Right | Bottom,
    }

    public class WindowChrome : Freezable
    {
        private struct _SystemParameterBoundProperty
        {
            public string SystemParameterPropertyName { get; set; }

            public DependencyProperty DependencyProperty { get; set; }
        }

        // Named property available for fully extending the glass frame.
        public static Thickness GlassFrameCompleteThickness => new Thickness(-1);

        #region Attached Properties

        public static readonly DependencyProperty WindowChromeProperty = DependencyProperty.RegisterAttached(
            "WindowChrome",
            typeof(WindowChrome),
            typeof(WindowChrome),
            new PropertyMetadata(null, _OnChromeChanged));

        private static void _OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // The different design tools handle drawing outside their custom window objects differently.
            // Rather than try to support this concept in the design surface let the designer draw its own
            // chrome anyways.
            // There's certainly room for improvement here.
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
            {
                return;
            }

            var window = (Window)d;
            var newChrome = (WindowChrome)e.NewValue;

            Assert.IsNotNull(window);

            // Update the ChromeWorker with this new object.

            // If there isn't currently a worker associated with the Window then assign a new one.
            // There can be a many:1 relationship of Window to WindowChrome objects, but a 1:1 for a Window and a WindowChromeWorker.
            WindowChromeWorker chromeWorker = WindowChromeWorker.GetWindowChromeWorker(window);
            if (chromeWorker == null)
            {
                chromeWorker = new WindowChromeWorker();
                WindowChromeWorker.SetWindowChromeWorker(window, chromeWorker);
            }

            chromeWorker.SetWindowChrome(newChrome);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ToDo")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ToDo")]
        public static WindowChrome GetWindowChrome(Window window)
        {
            Verify.IsNotNull(window, "window");
            return (WindowChrome)window.GetValue(WindowChromeProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ToDo")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ToDo")]
        public static void SetWindowChrome(Window window, WindowChrome chrome)
        {
            Verify.IsNotNull(window, "window");
            window.SetValue(WindowChromeProperty, chrome);
        }

        public static readonly DependencyProperty IsHitTestVisibleInChromeProperty = DependencyProperty.RegisterAttached(
            "IsHitTestVisibleInChrome",
            typeof(bool),
            typeof(WindowChrome),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ToDo")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ToDo")]
        public static bool GetIsHitTestVisibleInChrome(IInputElement inputElement)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if (dobj == null)
            {
                throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
            }

            return (bool)dobj.GetValue(IsHitTestVisibleInChromeProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ToDo")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ToDo")]
        public static void SetIsHitTestVisibleInChrome(IInputElement inputElement, bool hitTestVisible)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if (dobj == null)
            {
                throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
            }

            dobj.SetValue(IsHitTestVisibleInChromeProperty, hitTestVisible);
        }

        public static readonly DependencyProperty ResizeGripDirectionProperty = DependencyProperty.RegisterAttached(
            "ResizeGripDirection",
            typeof(ResizeGripDirection),
            typeof(WindowChrome),
            new FrameworkPropertyMetadata(ResizeGripDirection.None, FrameworkPropertyMetadataOptions.Inherits));

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ToDo")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ToDo")]
        public static ResizeGripDirection GetResizeGripDirection(IInputElement inputElement)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if (dobj == null)
            {
                throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
            }

            return (ResizeGripDirection)dobj.GetValue(ResizeGripDirectionProperty);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "ToDo")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "ToDo")]
        public static void SetResizeGripDirection(IInputElement inputElement, ResizeGripDirection direction)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if (dobj == null)
            {
                throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
            }

            dobj.SetValue(ResizeGripDirectionProperty, direction);
        }

        #endregion Attached Properties

        #region Dependency Properties

        public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register(
            "CaptionHeight",
            typeof(double),
            typeof(WindowChrome),
            new PropertyMetadata(
                0d,
                (d, e) => ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint()),
            value => (double)value >= 0d);

        /// <summary>The extent of the top of the window to treat as the caption.</summary>
        public double CaptionHeight
        {
            get => (double)this.GetValue(CaptionHeightProperty);
            set => this.SetValue(CaptionHeightProperty, value);
        }

        public static readonly DependencyProperty ResizeBorderThicknessProperty = DependencyProperty.Register(
            "ResizeBorderThickness",
            typeof(Thickness),
            typeof(WindowChrome),
            new PropertyMetadata(default(Thickness)),
            (value) => ((Thickness)value).IsNonNegative());

        public Thickness ResizeBorderThickness
        {
            get => (Thickness)this.GetValue(ResizeBorderThicknessProperty);
            set => this.SetValue(ResizeBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty GlassFrameThicknessProperty = DependencyProperty.Register(
            "GlassFrameThickness",
            typeof(Thickness),
            typeof(WindowChrome),
            new PropertyMetadata(
                default(Thickness),
                (d, e) => ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint(),
                (d, o) => _CoerceGlassFrameThickness((Thickness)o)));

        private static object _CoerceGlassFrameThickness(Thickness thickness)
        {
            // If it's explicitly set, but set to a thickness with at least one negative side then
            // coerce the value to the stock GlassFrameCompleteThickness.
            if (!thickness.IsNonNegative())
            {
                return GlassFrameCompleteThickness;
            }

            return thickness;
        }

        public Thickness GlassFrameThickness
        {
            get => (Thickness)this.GetValue(GlassFrameThicknessProperty);
            set => this.SetValue(GlassFrameThicknessProperty, value);
        }

        public static readonly DependencyProperty UseAeroCaptionButtonsProperty = DependencyProperty.Register(
            "UseAeroCaptionButtons",
            typeof(bool),
            typeof(WindowChrome),
            new FrameworkPropertyMetadata(true));

        public bool UseAeroCaptionButtons
        {
            get => (bool)this.GetValue(UseAeroCaptionButtonsProperty);
            set => this.SetValue(UseAeroCaptionButtonsProperty, value);
        }

        public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty = DependencyProperty.Register(
            "IgnoreTaskbarOnMaximize",
            typeof(bool),
            typeof(WindowChrome),
            new FrameworkPropertyMetadata(false,
                (d, e) =>
                ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint()));

        public bool IgnoreTaskbarOnMaximize
        {
            get => (bool)this.GetValue(IgnoreTaskbarOnMaximizeProperty);
            set => this.SetValue(IgnoreTaskbarOnMaximizeProperty, value);
        }

        public static readonly DependencyProperty UseNoneWindowStyleProperty = DependencyProperty.Register(
            "UseNoneWindowStyle",
            typeof(bool),
            typeof(WindowChrome),
            new FrameworkPropertyMetadata(false, (d, e) => ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint()));

        public bool UseNoneWindowStyle
        {
            get => (bool)this.GetValue(UseNoneWindowStyleProperty);
            set => this.SetValue(UseNoneWindowStyleProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(WindowChrome),
            new PropertyMetadata(
                default(CornerRadius),
                (d, e) => ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint()),
            (value) => ((CornerRadius)value).IsValid());

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)this.GetValue(CornerRadiusProperty);
            set => this.SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty SacrificialEdgeProperty = DependencyProperty.Register(
            "SacrificialEdge",
            typeof(SacrificialEdge),
            typeof(WindowChrome),
            new PropertyMetadata(
                SacrificialEdge.None,
                (d, e) => ((WindowChrome)d)._OnPropertyChangedThatRequiresRepaint()),
            _IsValidSacrificialEdge);

        private static readonly SacrificialEdge SacrificialEdge_All = SacrificialEdge.Bottom | SacrificialEdge.Top | SacrificialEdge.Left | SacrificialEdge.Right;

        private static bool _IsValidSacrificialEdge(object value)
        {
            SacrificialEdge se = SacrificialEdge.None;
            try
            {
                se = (SacrificialEdge)value;
            }
            catch (InvalidCastException)
            {
                return false;
            }

            if (se == SacrificialEdge.None)
            {
                return true;
            }

            // Does this only contain valid bits?
            if ((se | SacrificialEdge_All) != SacrificialEdge_All)
            {
                return false;
            }

            // It can't sacrifice all 4 edges.  Weird things happen.
            if (se == SacrificialEdge_All)
            {
                return false;
            }

            return true;
        }

        public SacrificialEdge SacrificialEdge
        {
            get => (SacrificialEdge)this.GetValue(SacrificialEdgeProperty);
            set => this.SetValue(SacrificialEdgeProperty, value);
        }

        #endregion Dependency Properties

        protected override Freezable CreateInstanceCore()
        {
            return new WindowChrome();
        }

        private static readonly List<_SystemParameterBoundProperty> BoundProperties = new List<_SystemParameterBoundProperty>
        {
            new _SystemParameterBoundProperty { DependencyProperty = CornerRadiusProperty, SystemParameterPropertyName = "WindowCornerRadius" },
            new _SystemParameterBoundProperty { DependencyProperty = CaptionHeightProperty, SystemParameterPropertyName = "WindowCaptionHeight" },
            new _SystemParameterBoundProperty { DependencyProperty = ResizeBorderThicknessProperty, SystemParameterPropertyName = "WindowResizeBorderThickness" },
            new _SystemParameterBoundProperty { DependencyProperty = GlassFrameThicknessProperty, SystemParameterPropertyName = "WindowNonClientFrameThickness" },
        };

        public WindowChrome()
        {
            // Effective default values for some of these properties are set to be bindings
            // that set them to system defaults.
            // A more correct way to do this would be to Coerce the value iff the source of the DP was the default value.
            // Unfortunately with the current property system we can't detect whether the value being applied at the time
            // of the coersion is the default.
            foreach (var bp in BoundProperties)
            {
                // This list must be declared after the DP's are assigned.
                Assert.IsNotNull(bp.DependencyProperty);
                BindingOperations.SetBinding(
                    this,
                    bp.DependencyProperty,
                    new Binding
                    {
                        Source = SystemParameters2.Current,
                        Path = new PropertyPath(bp.SystemParameterPropertyName),
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    });
            }
        }

        private void _OnPropertyChangedThatRequiresRepaint()
        {
            var handler = this.PropertyChangedThatRequiresRepaint;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public event EventHandler PropertyChangedThatRequiresRepaint;
    }
}