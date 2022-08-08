namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Windows;
    using WpfMouseWheel.Windows.MotionFlow;

    #region MouseWheelAdaptationBehavior
    public class MouseWheelAdaptationBehavior : MouseWheelBehavior
    {
        #region Fields
        private MouseWheelDebouncing _debouncing;
        #endregion

        #region Initialization
        public MouseWheelAdaptationBehavior(IMouseWheelClient client)
          : base(client)
        {
            var element = this.Client.Controller.Element;
            this.NestedMotionEnabled = MouseWheel.GetNestedMotion(element);
            this.Debouncing = MouseWheel.GetDebouncing(element);
            MouseWheel.NestedMotionProperty.AddValueChanged(element, this.OnNestedMotionChanged);
            MouseWheel.DebouncingProperty.AddValueChanged(element, this.OnDebouncingChanged);
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            var element = this.Client.Controller.Element;
            MouseWheel.NestedMotionProperty.RemoveValueChanged(element, this.OnNestedMotionChanged);
            MouseWheel.DebouncingProperty.RemoveValueChanged(element, this.OnDebouncingChanged);
            base.Dispose();
        }
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
        private void OnNestedMotionChanged(object sender, EventArgs e) { this.NestedMotionEnabled = MouseWheel.GetNestedMotion(sender as DependencyObject); }

        private void OnDebouncingChanged(object sender, EventArgs e) { this.Debouncing = MouseWheel.GetDebouncing(sender as DependencyObject); }
        #endregion
    }
    #endregion

    #region MouseWheelSmoothAdaptationBehavior
    public class MouseWheelSmoothAdaptationBehavior : MouseWheelAdaptationBehavior
    {
        #region Fields
        private readonly MotionSmoothingTarget _motionSmoothing;
        #endregion

        #region Initialization
        public MouseWheelSmoothAdaptationBehavior(IMouseWheelClient client, IMotionFilter motionFilter)
          : base(client)
        {
            this._motionSmoothing = new MotionSmoothingTarget(motionFilter) { Next = this };
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            this._motionSmoothing.Dispose();
            base.Dispose();
        }
        #endregion

        #region MouseWheelBehavior
        protected override IMotionTarget MotionInput => this._motionSmoothing;
        #endregion
    }
    #endregion
}
