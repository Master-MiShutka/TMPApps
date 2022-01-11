namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using WpfMouseWheel.Windows.Controls;
    using WpfMouseWheel.Windows.MotionFlow;

    public class MouseWheelScrollClient : MouseWheelClient
    {
        public MouseWheelScrollClient(IMouseWheelController controller, Orientation orientation)
          : base(controller)
        {
            this._orientation = orientation;
            this._scrollAreaOrientation = this.ScrollViewer.GetScrollAreaOrientation();
        }

        public ScrollViewer ScrollViewer => this.Controller.Element as ScrollViewer;

        public Orientation ScrollAreaOrientation => this._scrollAreaOrientation;

        public Orientation Orientation => this._orientation;

        public bool CanContentScroll
        {
            get => this.ScrollViewer.CanContentScroll;
            set
            {
                if (this.CanContentScroll == value)
                    return;
                this.ScrollViewer.CanContentScroll = value;
            }
        }

        public override MouseWheelSmoothing Smoothing
        {
            get => this._smoothing;

            protected set
            {
                if (this._smoothing == value)
                    return;
                this._smoothing = value;
                this.InvalidateBehavior();
            }
        }

        public override ModifierKeys Modifiers => this._modifiers;

        public override bool IsActive(MouseWheelInputEventArgs e)
        {
            if (e.Orientation == Orientation.Horizontal)
            {
                this.EnsureLoaded();
                return this.Orientation == Orientation.Horizontal;
            }
            else
            {
                return base.IsActive(e);
            }
        }

        public override bool CanMove(IMotionInfo info, object context)
        {
            return !DoubleEx.IsZero(this.GetScrollableDisplacement(info.Direction));
        }

        public override double Coerce(IMotionInfo info, object context, double delta)
        {
            var direction = info.Direction;
            var scrollableDelta = this.GetScrollableDisplacement(direction);
            return direction < 0 ? Math.Max(scrollableDelta, delta) : Math.Min(scrollableDelta, delta);
        }

        public override void Move(IMotionInfo info, object context, double delta)
        {
            this.ScrollViewer.Scroll(this.Orientation, delta);
        }

        protected override void OnLoading()
        {
            base.OnLoading();
            var element = this.Controller.Element;
            if (this.Orientation == Orientation.Vertical)
            {
                this._scrollMode = MouseWheel.GetVScrollMode(element);
                this._smoothing = MouseWheel.GetVScrollSmoothing(element);
                this._modifiers = MouseWheel.GetVScrollModifiers(element);
                MouseWheel.VScrollModeProperty.AddValueChanged(element, this.OnVScrollModeChanged);
                MouseWheel.VScrollSmoothingProperty.AddValueChanged(element, this.OnVSmoothingChanged);
                MouseWheel.VScrollModifiersProperty.AddValueChanged(element, this.OnVModifiersChanged);

                MouseWheel.GetLogicalVScrollIncrement(element).SetOrientation(Orientation.Vertical);
                MouseWheel.GetPhysicalVScrollIncrement(element).SetOrientation(Orientation.Vertical);
            }
            else
            {
                this._scrollMode = MouseWheel.GetHScrollMode(element);
                this._smoothing = MouseWheel.GetHScrollSmoothing(element);
                this._modifiers = MouseWheel.GetHScrollModifiers(element);
                MouseWheel.HScrollModeProperty.AddValueChanged(element, this.OnHScrollModeChanged);
                MouseWheel.HScrollSmoothingProperty.AddValueChanged(element, this.OnHSmoothingChanged);
                MouseWheel.HScrollModifiersProperty.AddValueChanged(element, this.OnHModifiersChanged);

                MouseWheel.GetLogicalHScrollIncrement(element).SetOrientation(Orientation.Horizontal);
                MouseWheel.GetPhysicalHScrollIncrement(element).SetOrientation(Orientation.Horizontal);
            }
        }

        protected override void OnUnloading()
        {
            var element = this.Controller.Element;
            if (this.Orientation == Orientation.Vertical)
            {
                MouseWheel.VScrollSmoothingProperty.RemoveValueChanged(element, this.OnVSmoothingChanged);
                MouseWheel.VScrollModeProperty.RemoveValueChanged(element, this.OnVScrollModeChanged);
                MouseWheel.VScrollModifiersProperty.RemoveValueChanged(element, this.OnVModifiersChanged);
            }
            else
            {
                MouseWheel.HScrollSmoothingProperty.RemoveValueChanged(element, this.OnHSmoothingChanged);
                MouseWheel.HScrollModeProperty.RemoveValueChanged(element, this.OnHScrollModeChanged);
                MouseWheel.HScrollModifiersProperty.RemoveValueChanged(element, this.OnHModifiersChanged);
            }

            base.OnUnloading();
        }

        protected override IInputElement GetExitElement()
        {
            // skip mouse wheel event implementors (TextBoxBase subclasses)
            var implementor = GetMouseWheelEventImplementor((this.Controller as MouseWheelFrameworkLevelController).FrameworkLevelElement);
            if (implementor != null)
            {
                return implementor.GetVisualAncestors().OfType<IInputElement>().FirstOrDefault();
            }
            else
            {
                return base.GetExitElement();
            }
        }

        protected override IMouseWheelInputListener CreateBehavior()
        {
            if (this.Enhanced)
            {
                switch (this.ScrollMode)
                {
                    case MouseWheelScrollMode.Auto:
                        return this.CreateEnhancedAutoBehavior();
                    case MouseWheelScrollMode.Physical:
                        return this.CreateEnhancedPhysicalBehavior();
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                switch (this.ScrollMode)
                {
                    case MouseWheelScrollMode.Auto:
                        return this.CreateNativeAutoBehavior();
                    case MouseWheelScrollMode.Physical:
                        return this.CreateNativePhysicalBehaviorItem();
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private static IEnumerable<Type> MouseWheelEventImplementorTypes
        {
            get
            {
                yield return typeof(TextBoxBase);
                yield return typeof(FlowDocumentScrollViewer);
                yield return typeof(FlowDocumentPageViewer);
            }
        }

        private static bool ImplementsMouseWheelEvent(DependencyObject obj)
        {
            if (obj == null)
                return false;
            var objType = obj.GetType();
            return MouseWheelEventImplementorTypes.Any(t => t.IsAssignableFrom(objType));
        }

        private static DependencyObject GetMouseWheelEventImplementor(IFrameworkLevelElement element)
        {
            var templatedParent = element.TemplatedParent;
            return ImplementsMouseWheelEvent(templatedParent) ? templatedParent : null;
        }

        private bool LogicalScrollEnabled => this.ScrollViewer.CanContentScroll && this.ScrollAreaOrientation == this.Orientation;

        private bool HostImplementsMouseWheelEvent => ImplementsMouseWheelEvent(this.ScrollViewer.TemplatedParent);

        private MouseWheelScrollMode ScrollMode
        {
            get => this._scrollMode;
            set
            {
                if (this._scrollMode == value)
                    return;
                this._scrollMode = value;
                this.InvalidateBehavior();
            }
        }

        private IDisposable CreateScrollViewerManipulator(bool canContentScroll)
        {
            if (this.Orientation == this.ScrollAreaOrientation && canContentScroll != this.ScrollViewer.CanContentScroll && !this.HostImplementsMouseWheelEvent)
                return new ScrollViewerManipulator(this.ScrollViewer, canContentScroll);
            return null;
        }

        private double GetScrollableDisplacement(int direction)
        {
            return this.ScrollViewer.GetScrollableDisplacement(this.Orientation, direction);
        }

        private IMouseWheelInputListener CreateNativeAutoBehavior()
        {
            if (this.LogicalScrollEnabled)
                return this.CreateNativeLogicalBehaviorItem();
            else
                return this.CreateNativePhysicalBehaviorItem();
        }

        private IMouseWheelInputListener CreateNativeLogicalBehavior()
        {
            if (this.LogicalScrollEnabled)
                return this.CreateNativeLogicalBehaviorItem();
            else
                return this.CreateNativePhysicalBehaviorItem();
        }

        private IMouseWheelInputListener CreateNativePhysicalBehaviorItem()
        {
            return new MouseWheelNativeBehavior(this, this.CreateScrollViewerManipulator(false));
        }

        private IMouseWheelInputListener CreateNativeLogicalBehaviorItem()
        {
            return new MouseWheelNativeBehavior(this, this.CreateScrollViewerManipulator(true));
        }

        private IMouseWheelInputListener CreateEnhancedAutoBehavior()
        {
            if (this.LogicalScrollEnabled)
            {
                if (this.ScrollViewer.HasNestedScrollFrames() || this.HostImplementsMouseWheelEvent)
                    return this.CreateEnhancedPhysicalBehavior();
                else
                    return this.CreateEnhancedLogicalBehaviorItem();
            }
            else
            {
                return this.CreateEnhancedPhysicalBehavior();
            }
        }

        private IMouseWheelInputListener CreateEnhancedLogicalBehavior()
        {
            if (this.LogicalScrollEnabled)
            {
                if (this.HostImplementsMouseWheelEvent)
                    return this.CreateEnhancedPhysicalBehaviorItem();
                else
                    return this.CreateEnhancedLogicalBehaviorItem();
            }
            else
            {
                return this.CreateEnhancedPhysicalBehaviorItem();
            }
        }

        private IMouseWheelInputListener CreateEnhancedPhysicalBehavior()
        {
            switch (this.Smoothing)
            {
                case MouseWheelSmoothing.None:
                    return this.CreateEnhancedPhysicalBehaviorItem();
                case MouseWheelSmoothing.Linear:
                    return this.CreateEnhancedLinearBehaviorItem();
                case MouseWheelSmoothing.Smooth:
                    return this.CreateEnhancedSmoothBehaviorItem();
                default:
                    throw new NotImplementedException();
            }
        }

        private IMouseWheelInputListener CreateEnhancedSmoothingBehavior()
        {
            if (this.Smoothing == MouseWheelSmoothing.Linear)
                return this.CreateEnhancedLinearBehaviorItem();
            else
                return this.CreateEnhancedSmoothBehaviorItem();
        }

        private IMouseWheelInputListener CreateEnhancedLogicalBehaviorItem()
        {
            return new MouseWheelLogicalScrollBehavior(this, this.CreateScrollViewerManipulator(true));
        }

        private IMouseWheelInputListener CreateEnhancedPhysicalBehaviorItem()
        {
            return new MouseWheelPhysicalScrollBehavior(this, this.CreateScrollViewerManipulator(false));
        }

        private IMouseWheelInputListener CreateEnhancedLinearBehaviorItem()
        {
            return new MouseWheelSmoothScrollBehavior(this, this.CreateScrollViewerManipulator(false), new MouseWheelLinearFilter());
        }

        private IMouseWheelInputListener CreateEnhancedSmoothBehaviorItem()
        {
            return new MouseWheelSmoothScrollBehavior(this, this.CreateScrollViewerManipulator(false), new MouseWheelSmoothingFilter());
        }

        private void OnVSmoothingChanged(object sender, EventArgs e)
        {
            this.Smoothing = MouseWheel.GetVScrollSmoothing(sender as DependencyObject);
        }

        private void OnHSmoothingChanged(object sender, EventArgs e)
        {
            this.Smoothing = MouseWheel.GetHScrollSmoothing(sender as DependencyObject);
        }

        private void OnVScrollModeChanged(object sender, EventArgs e)
        {
            this.ScrollMode = MouseWheel.GetVScrollMode(sender as DependencyObject);
        }

        private void OnHScrollModeChanged(object sender, EventArgs e)
        {
            this.ScrollMode = MouseWheel.GetHScrollMode(sender as DependencyObject);
        }

        private void OnVModifiersChanged(object sender, EventArgs e)
        {
            this._modifiers = MouseWheel.GetVScrollModifiers(sender as DependencyObject);
        }

        private void OnHModifiersChanged(object sender, EventArgs e)
        {
            this._modifiers = MouseWheel.GetHScrollModifiers(sender as DependencyObject);
        }

        private readonly Orientation _scrollAreaOrientation;
        private readonly Orientation _orientation;
        private MouseWheelScrollMode _scrollMode;
        private MouseWheelSmoothing _smoothing;
        private ModifierKeys _modifiers;

        private class ScrollViewerManipulator : IDisposable
        {
            public ScrollViewerManipulator(ScrollViewer scrollViewer, bool canContentScroll)
            {
                this._scrollViewer = scrollViewer;
                this._canContentScroll = scrollViewer.CanContentScroll;
                scrollViewer.CanContentScroll = canContentScroll;
            }

            public void Dispose()
            {
                this._scrollViewer.CanContentScroll = this._canContentScroll;
            }

            private readonly ScrollViewer _scrollViewer;
            private readonly bool _canContentScroll;
        }
    }
}
