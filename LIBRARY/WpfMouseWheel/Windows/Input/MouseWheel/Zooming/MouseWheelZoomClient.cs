namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using WpfMouseWheel.Windows.MotionFlow;

    public class MouseWheelZoomClient : MouseWheelClient
    {
        #region Fields
        private MouseWheelSmoothing _smoothing;
        private ModifierKeys _modifiers;
        #endregion

        #region Initialization
        public MouseWheelZoomClient(IMouseWheelController controller)
          : base(controller)
        {
        }
        #endregion

        #region IMouseWheelClient
        public override double MotionIncrement => this.ZoomElement.ZoomIncrement;
        #endregion

        #region MouseWheelClient
        protected override void OnLoading()
        {
            base.OnLoading();
            var element = this.Controller.Element;
            this._smoothing = MouseWheel.GetZoomSmoothing(element);
            this._modifiers = MouseWheel.GetZoomModifiers(element);
            MouseWheel.ZoomSmoothingProperty.AddValueChanged(element, this.OnSmoothingChanged);
            MouseWheel.ZoomModifiersProperty.AddValueChanged(element, this.OnModifiersChanged);
        }

        protected override void OnUnloading()
        {
            var element = this.Controller.Element;
            MouseWheel.ZoomModifiersProperty.RemoveValueChanged(element, this.OnModifiersChanged);
            MouseWheel.ZoomSmoothingProperty.RemoveValueChanged(element, this.OnSmoothingChanged);
            base.OnUnloading();
        }

        protected override IMouseWheelInputListener CreateBehavior()
        {
            if (this.Enhanced)
            {
                switch (this.Smoothing)
                {
                    case MouseWheelSmoothing.None: return new MouseWheelZoomBehavior(this);
                    case MouseWheelSmoothing.Linear: return new MouseWheelSmoothZoomBehavior(this, new MouseWheelLinearFilter());
                    case MouseWheelSmoothing.Smooth: return new MouseWheelSmoothZoomBehavior(this, new MouseWheelSmoothingFilter());
                    default: throw new NotSupportedException();
                }
            }
            else
            {
                return new MouseWheelNativeBehavior(this);
            }
        }
        #endregion

        #region IMouseWheelClient
        public override MouseWheelSmoothing Smoothing
        {
            get => this._smoothing;
            protected set
            {
                if (this._smoothing == value) return;
                this._smoothing = value;
                this.InvalidateBehavior();
            }
        }

        public override ModifierKeys Modifiers => this._modifiers;
        #endregion

        #region IMotionTarget
        public override bool CanMove(IMotionInfo info, object context)
        {
            var z = this.ZoomElement;
            return info.Direction < 0 ? z.CanIncreaseZoom : z.CanDecreaseZoom;
        }

        public override double Coerce(IMotionInfo info, object context, double delta)
        {
            var z = this.ZoomElement;
            int direction = info.Direction;
            if (direction < 0)
            {
                // increasing zoom
                var zoomableDelta = z.Zoom - z.MaxZoom;
                return Math.Max(zoomableDelta, delta);
            }
            else
            {
                // decreasing zoom
                var zoomableDelta = z.Zoom - z.MinZoom;
                return Math.Min(zoomableDelta, delta);
            }
        }

        public override void Move(IMotionInfo info, object context, double delta)
        {
            this.ZoomElement.Zoom -= delta;
        }
        #endregion

        #region Helpers
        private IZoomElement ZoomElement => (this.Controller as MouseWheelFrameworkLevelController).FrameworkLevelElement as IZoomElement;

        private void OnSmoothingChanged(object sender, EventArgs e) { this.Smoothing = MouseWheel.GetZoomSmoothing(sender as DependencyObject); }

        private void OnModifiersChanged(object sender, EventArgs e) { this._modifiers = MouseWheel.GetZoomModifiers(sender as DependencyObject); }
        #endregion
    }
}
