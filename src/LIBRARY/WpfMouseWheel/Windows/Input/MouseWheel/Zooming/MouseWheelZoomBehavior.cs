namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Windows;
    using WpfMouseWheel.Windows.MotionFlow;

    #region MouseWheelZoomBehavior
    public class MouseWheelZoomBehavior : MouseWheelBehavior
    {
        #region Fields
        private MouseWheelDebouncing _debouncing;
        #endregion

        #region Initialization
        public MouseWheelZoomBehavior(IMouseWheelClient client)
          : base(client)
        {
            var element = this.Client.Controller.Element;
            this.NestedMotionEnabled = MouseWheel.GetNestedZoom(element);
            this.Debouncing = MouseWheel.GetZoomDebouncing(element);
            MouseWheel.NestedZoomProperty.AddValueChanged(element, this.OnNestedZoomChanged);
            MouseWheel.ZoomDebouncingProperty.AddValueChanged(element, this.OnDebouncingChanged);
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            var element = this.Client.Controller.Element;
            MouseWheel.NestedZoomProperty.RemoveValueChanged(element, this.OnNestedZoomChanged);
            MouseWheel.ZoomDebouncingProperty.RemoveValueChanged(element, this.OnDebouncingChanged);
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
        private void OnNestedZoomChanged(object sender, EventArgs e) { this.NestedMotionEnabled = MouseWheel.GetNestedZoom(sender as DependencyObject); }

        private void OnDebouncingChanged(object sender, EventArgs e) { this.Debouncing = MouseWheel.GetZoomDebouncing(sender as DependencyObject); }
        #endregion
    }
    #endregion

    #region MouseWheelSmoothZoomBehavior
    public class MouseWheelSmoothZoomBehavior : MouseWheelZoomBehavior
    {
        #region Fields
        private readonly MotionSmoothingTarget _motionSmoothing;
        #endregion

        #region Initialization
        public MouseWheelSmoothZoomBehavior(IMouseWheelClient client, IMotionFilter motionFilter)
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
