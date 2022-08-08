namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using WpfMouseWheel.Windows.MotionFlow;

    #region MouseWheelScrollBehavior
    public abstract class MouseWheelScrollBehavior : MouseWheelBehavior
    {
        #region Fields
        private MouseWheelScrollIncrement _scrollIncrement;
        private MouseWheelDebouncing _debouncing;
        #endregion

        #region Initialization
        public MouseWheelScrollBehavior(MouseWheelScrollClient scrollClient, IDisposable manipulator)
          : base(scrollClient, manipulator)
        {
            var element = this.Client.Controller.Element;
            if (scrollClient.Orientation == Orientation.Vertical)
            {
                this.NestedMotionEnabled = MouseWheel.GetNestedVScroll(element);
                MouseWheel.NestedVScrollProperty.AddValueChanged(element, this.OnNestedVScrollChanged);
            }
            else
            {
                this.NestedMotionEnabled = MouseWheel.GetNestedHScroll(element);
                MouseWheel.NestedHScrollProperty.AddValueChanged(element, this.OnNestedHScrollChanged);
            }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            var element = this.Client.Controller.Element;
            if (this.ScrollClient.Orientation == Orientation.Vertical)
                MouseWheel.NestedVScrollProperty.RemoveValueChanged(element, this.OnNestedVScrollChanged);
            else
                MouseWheel.NestedHScrollProperty.RemoveValueChanged(element, this.OnNestedHScrollChanged);
            if (this._scrollIncrement != null)
                this._scrollIncrement.PropertyChanged -= this.OnScrollIncrementPropertyChanged;
            base.Dispose();
        }
        #endregion

        #region MouseWheelBehavior
        protected override double MotionIncrement => this.ScrollIncrement;
        #endregion

        #region IMouseWheelBehavior
        public override bool NestedMotionEnabled { get; protected set; }

        public override MouseWheelDebouncing Debouncing
        {
            get => this._debouncing;
            protected set
            {
                if (this._debouncing == value) return;
                this._debouncing = value;
                this.InvalidateShaft();
            }
        }
        #endregion

        #region Queries
        public MouseWheelScrollClient ScrollClient => this.Client as MouseWheelScrollClient;
        #endregion

        #region Properties
        protected MouseWheelScrollIncrement ScrollIncrement
        {
            get => this._scrollIncrement;
            set
            {
                if (this._scrollIncrement == value) return;
                if (this._scrollIncrement != null)
                    this._scrollIncrement.PropertyChanged -= this.OnScrollIncrementPropertyChanged;
                this._scrollIncrement = value;
                this._scrollIncrement.PropertyChanged += this.OnScrollIncrementPropertyChanged;
                this.InvalidateShaft();
            }
        }
        #endregion

        #region Helpers
        private void OnNestedVScrollChanged(object sender, EventArgs e) { this.NestedMotionEnabled = MouseWheel.GetNestedVScroll(sender as DependencyObject); }

        private void OnNestedHScrollChanged(object sender, EventArgs e) { this.NestedMotionEnabled = MouseWheel.GetNestedHScroll(sender as DependencyObject); }

        private void OnScrollIncrementPropertyChanged(object sender, PropertyChangedEventArgs e) { this.InvalidateShaft(); }
        #endregion
    }
    #endregion

    #region MouseWheelLogicalScrollBehavior
    public class MouseWheelLogicalScrollBehavior : MouseWheelScrollBehavior
    {
        #region Initialization
        public MouseWheelLogicalScrollBehavior(MouseWheelScrollClient scrollClient, IDisposable manipulator)
          : base(scrollClient, manipulator)
        {
            var element = this.Client.Controller.Element;
            if (scrollClient.Orientation == Orientation.Vertical)
            {
                this.Debouncing = MouseWheel.GetLogicalVScrollDebouncing(element);
                this.ScrollIncrement = MouseWheel.GetLogicalVScrollIncrement(element);
                MouseWheel.LogicalVScrollDebouncingProperty.AddValueChanged(element, this.OnDebouncingYChanged);
                MouseWheel.LogicalVScrollIncrementProperty.AddValueChanged(element, this.OnVScrollIncrementChanged);
            }
            else
            {
                this.Debouncing = MouseWheel.GetLogicalHScrollDebouncing(element);
                this.ScrollIncrement = MouseWheel.GetLogicalHScrollIncrement(element);
                MouseWheel.LogicalHScrollDebouncingProperty.AddValueChanged(element, this.OnDebouncingXChanged);
                MouseWheel.LogicalHScrollIncrementProperty.AddValueChanged(element, this.OnHScrollIncrementChanged);
            }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            var element = this.Client.Controller.Element;
            if (this.ScrollClient.Orientation == Orientation.Vertical)
            {
                MouseWheel.LogicalVScrollDebouncingProperty.RemoveValueChanged(element, this.OnDebouncingYChanged);
                MouseWheel.LogicalVScrollIncrementProperty.RemoveValueChanged(element, this.OnVScrollIncrementChanged);
            }
            else
            {
                MouseWheel.LogicalHScrollDebouncingProperty.RemoveValueChanged(element, this.OnDebouncingXChanged);
                MouseWheel.LogicalHScrollIncrementProperty.RemoveValueChanged(element, this.OnHScrollIncrementChanged);
            }

            base.Dispose();
        }
        #endregion

        #region MouseWheelBehavior
        protected override IMouseWheelShaft GetMotionShaft(MouseWheel wheel, IMouseWheelTransferCase transferCase)
        {
            switch (this.Debouncing)
            {
                case MouseWheelDebouncing.Auto: return this.GetMotionShaftAuto(wheel, transferCase, this.ScrollIncrement);
                case MouseWheelDebouncing.None: return transferCase[0];
                case MouseWheelDebouncing.Single: return transferCase[1];
                default: throw new NotImplementedException();
            }
        }

        protected override double CoerceSinkDelta(double sinkDelta) { return Math.Round(sinkDelta); }
        #endregion

        #region Helpers
        private void OnDebouncingYChanged(object sender, EventArgs e) { this.Debouncing = MouseWheel.GetLogicalVScrollDebouncing(sender as DependencyObject); }

        private void OnDebouncingXChanged(object sender, EventArgs e) { this.Debouncing = MouseWheel.GetLogicalHScrollDebouncing(sender as DependencyObject); }

        private void OnVScrollIncrementChanged(object sender, EventArgs e) { this.ScrollIncrement = MouseWheel.GetLogicalVScrollIncrement(sender as DependencyObject); }

        private void OnHScrollIncrementChanged(object sender, EventArgs e) { this.ScrollIncrement = MouseWheel.GetLogicalHScrollIncrement(sender as DependencyObject); }
        #endregion
    }
    #endregion

    #region MouseWheelFlowDocumentPageViewerScrollBehavior
    public class MouseWheelFlowDocumentPageViewerScrollBehavior : MouseWheelBehavior
    {
        #region Fields
        private MouseWheelDebouncing _debouncing;
        #endregion

        #region Initialization
        public MouseWheelFlowDocumentPageViewerScrollBehavior(IMouseWheelClient client)
          : base(client, null)
        {
            var element = this.Client.Controller.Element;
            this.NestedMotionEnabled = MouseWheel.GetNestedVScroll(element);
            this.Debouncing = MouseWheel.GetLogicalVScrollDebouncing(element);
            MouseWheel.NestedVScrollProperty.AddValueChanged(element, this.OnNestedVScrollChanged);
            MouseWheel.LogicalVScrollDebouncingProperty.AddValueChanged(element, this.OnDebouncingYChanged);
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            var element = this.Client.Controller.Element;
            MouseWheel.NestedVScrollProperty.RemoveValueChanged(element, this.OnNestedVScrollChanged);
            MouseWheel.LogicalVScrollDebouncingProperty.RemoveValueChanged(element, this.OnDebouncingYChanged);
            base.Dispose();
        }
        #endregion

        #region MouseWheelBehavior
        protected override double CoerceSinkDelta(double sinkDelta) { return Math.Round(sinkDelta); }
        #endregion

        #region IMouseWheelBehavior
        public override bool NestedMotionEnabled { get; protected set; }

        public override MouseWheelDebouncing Debouncing
        {
            get => this._debouncing;
            protected set
            {
                if (this._debouncing == value) return;
                this._debouncing = value;
                this.InvalidateShaft();
            }
        }
        #endregion

        #region Helpers
        private void OnNestedVScrollChanged(object sender, EventArgs e) { this.NestedMotionEnabled = MouseWheel.GetNestedVScroll(sender as DependencyObject); }

        private void OnDebouncingYChanged(object sender, EventArgs e) { this.Debouncing = MouseWheel.GetLogicalVScrollDebouncing(sender as DependencyObject); }
        #endregion
    }
    #endregion

    #region MouseWheelPhysicalScrollBehavior
    public class MouseWheelPhysicalScrollBehavior : MouseWheelScrollBehavior
    {
        #region Initialization
        public MouseWheelPhysicalScrollBehavior(MouseWheelScrollClient scrollClient, IDisposable manipulator)
          : base(scrollClient, manipulator)
        {
            var element = this.Client.Controller.Element;
            if (scrollClient.Orientation == Orientation.Vertical)
            {
                this.Debouncing = MouseWheel.GetPhysicalVScrollDebouncing(element);
                this.ScrollIncrement = MouseWheel.GetPhysicalVScrollIncrement(element);
                MouseWheel.PhysicalVScrollDebouncingProperty.AddValueChanged(element, this.OnDebouncingYChanged);
                MouseWheel.PhysicalVScrollIncrementProperty.AddValueChanged(element, this.OnVScrollIncrementChanged);
            }
            else
            {
                this.Debouncing = MouseWheel.GetPhysicalHScrollDebouncing(element);
                this.ScrollIncrement = MouseWheel.GetPhysicalHScrollIncrement(element);
                MouseWheel.PhysicalHScrollDebouncingProperty.AddValueChanged(element, this.OnDebouncingXChanged);
                MouseWheel.PhysicalHScrollIncrementProperty.AddValueChanged(element, this.OnHScrollIncrementChanged);
            }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            var element = this.Client.Controller.Element;
            if (this.ScrollClient.Orientation == Orientation.Vertical)
            {
                MouseWheel.PhysicalVScrollDebouncingProperty.RemoveValueChanged(element, this.OnDebouncingYChanged);
                MouseWheel.PhysicalVScrollIncrementProperty.RemoveValueChanged(element, this.OnVScrollIncrementChanged);
            }
            else
            {
                MouseWheel.PhysicalHScrollDebouncingProperty.RemoveValueChanged(element, this.OnDebouncingXChanged);
                MouseWheel.PhysicalHScrollIncrementProperty.RemoveValueChanged(element, this.OnHScrollIncrementChanged);
            }

            base.Dispose();
        }
        #endregion

        #region Helpers
        private void OnDebouncingYChanged(object sender, EventArgs e) { this.Debouncing = MouseWheel.GetPhysicalVScrollDebouncing(sender as DependencyObject); }

        private void OnDebouncingXChanged(object sender, EventArgs e) { this.Debouncing = MouseWheel.GetPhysicalHScrollDebouncing(sender as DependencyObject); }

        private void OnVScrollIncrementChanged(object sender, EventArgs e) { this.ScrollIncrement = MouseWheel.GetPhysicalVScrollIncrement(sender as DependencyObject); }

        private void OnHScrollIncrementChanged(object sender, EventArgs e) { this.ScrollIncrement = MouseWheel.GetPhysicalHScrollIncrement(sender as DependencyObject); }
        #endregion
    }
    #endregion

    #region MouseWheelSmoothScrollBehavior
    public class MouseWheelSmoothScrollBehavior : MouseWheelPhysicalScrollBehavior
    {
        #region Fields
        private readonly MotionSmoothingTarget _motionSmoothing;
        #endregion

        #region Initialization
        public MouseWheelSmoothScrollBehavior(MouseWheelScrollClient scrollClient, IDisposable manipulator, IMotionFilter motionFilter)
          : base(scrollClient, manipulator)
        {
            this._motionSmoothing = new MotionSmoothingTarget(motionFilter) { Next = this, Precision = this.SinkToNormalized(0.1) };
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            this._motionSmoothing.Dispose();
            base.Dispose();
        }
        #endregion

        #region IMouseWheelInputListener
        protected override IMotionTarget MotionInput => this._motionSmoothing;
        #endregion
    }
    #endregion
}
