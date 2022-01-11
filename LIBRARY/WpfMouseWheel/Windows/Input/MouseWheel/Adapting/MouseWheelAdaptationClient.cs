namespace WpfMouseWheel.Windows.Input
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using WpfMouseWheel.Windows.MotionFlow;

    public class MouseWheelAdaptationClient : MouseWheelClient
    {
        #region Fields
        private MouseWheelSmoothing _smoothing;
        private ModifierKeys _modifiers;
        private double _minimum;
        private double _maximum;
        #endregion

        #region Initialization
        public MouseWheelAdaptationClient(IMouseWheelController controller)
          : base(controller)
        {
        }
        #endregion

        #region MouseWheelClient
        protected override void OnLoading()
        {
            base.OnLoading();
            var element = this.Controller.Element;
            this._smoothing = MouseWheel.GetSmoothing(element);
            this._modifiers = MouseWheel.GetModifiers(element);
            this._minimum = MouseWheel.GetMinimum(element);
            this._maximum = MouseWheel.GetMaximum(element);
            MouseWheel.SmoothingProperty.AddValueChanged(element, this.OnSmoothingChanged);
            MouseWheel.ModifiersProperty.AddValueChanged(element, this.OnModifiersChanged);
            MouseWheel.MinimumProperty.AddValueChanged(element, this.OnMinimumChanged);
            MouseWheel.MaximumProperty.AddValueChanged(element, this.OnMaximumChanged);
        }

        protected override void OnUnloading()
        {
            var element = this.Controller.Element;
            MouseWheel.MinimumProperty.RemoveValueChanged(element, this.OnMinimumChanged);
            MouseWheel.MaximumProperty.RemoveValueChanged(element, this.OnMaximumChanged);
            MouseWheel.ModifiersProperty.RemoveValueChanged(element, this.OnModifiersChanged);
            MouseWheel.SmoothingProperty.RemoveValueChanged(element, this.OnSmoothingChanged);
            base.OnUnloading();
        }

        protected override IMouseWheelInputListener CreateBehavior()
        {
            if (this.Enhanced)
            {
                switch (this.Smoothing)
                {
                    case MouseWheelSmoothing.None: return new MouseWheelAdaptationBehavior(this);
                    case MouseWheelSmoothing.Linear: return new MouseWheelSmoothAdaptationBehavior(this, new MouseWheelLinearFilter());
                    case MouseWheelSmoothing.Smooth: return new MouseWheelSmoothAdaptationBehavior(this, new MouseWheelSmoothingFilter());
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
        public override double MotionIncrement => MouseWheel.GetIncrement(this.Controller.Element);

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
            var element = this.Controller.Element;
            var value = MouseWheel.GetValue(element);
            return info.Direction < 0 ? value > this._minimum : value < this._maximum;
        }

        public override double Coerce(IMotionInfo info, object context, double delta)
        {
            var element = this.Controller.Element;
            var value = MouseWheel.GetValue(element);
            int direction = info.Direction;
            if (direction > 0)
            {
                var movableDelta = this._maximum - value;
                return Math.Min(movableDelta, delta);
            }
            else
            {
                var movableDelta = this._minimum - value;
                return Math.Max(movableDelta, delta);
            }
        }

        public override void Move(IMotionInfo info, object context, double delta)
        {
            var element = this.Controller.Element;
            MouseWheel.SetValue(element, MouseWheel.GetValue(element) + delta);
        }
        #endregion

        #region Helpers
        private void OnSmoothingChanged(object sender, EventArgs e) { this.Smoothing = MouseWheel.GetSmoothing(sender as DependencyObject); }

        private void OnModifiersChanged(object sender, EventArgs e) { this._modifiers = MouseWheel.GetModifiers(sender as DependencyObject); }

        private void OnMinimumChanged(object sender, EventArgs e) { this._minimum = MouseWheel.GetMinimum(sender as DependencyObject); }

        private void OnMaximumChanged(object sender, EventArgs e) { this._maximum = MouseWheel.GetMaximum(sender as DependencyObject); }
        #endregion
    }
}
