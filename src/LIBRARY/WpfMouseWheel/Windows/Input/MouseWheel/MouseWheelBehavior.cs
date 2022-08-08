namespace WpfMouseWheel.Windows.Input
{
    using System;
    using WpfMouseWheel.Windows.MotionFlow;

    public interface IMouseWheelBehavior : IMouseWheelInputListener, IMotionSink, INativeMotionTarget, IDisposable
    {
        bool NestedMotionEnabled
        {
            get;
        }

        MouseWheelDebouncing Debouncing
        {
            get;
        }
    }

    public abstract class MouseWheelBehavior : IMouseWheelBehavior
    {
        public MouseWheelBehavior(IMouseWheelClient client, IDisposable manipulator = null)
        {
            this._client = client;
            this._manipulator = manipulator;
        }

        public IMouseWheelController Controller => this.Client.Controller;

        public IMouseWheelClient Client => this._client;

        public abstract bool NestedMotionEnabled
        {
            get; protected set;
        }

        public abstract MouseWheelDebouncing Debouncing
        {
            get; protected set;
        }

        public virtual void OnPreviewInput(object sender, MouseWheelInputEventArgs e)
        {
            e.Handled = this.GetMotionShaft(e.Wheel) == null;
        }

        public virtual void OnInput(object sender, MouseWheelInputEventArgs e)
        {
            e.Handled = this.TransferMotion(e);
        }

        public bool CanMove(IMotionInfo info, object context)
        {
            return this.Client.CanMove(info, context);
        }

        public double Coerce(IMotionInfo info, object context, double delta)
        {
            var sinkDelta = this.NormalizedToSink(delta);
            if (DoubleEx.IsZero(sinkDelta))
                return 0;
            var sinkCoerced = this.Client.Coerce(info, context, this.CoerceSinkDelta(sinkDelta));
            if (DoubleEx.IsZero(sinkCoerced))
                return 0;
            return this.SinkToNormalized(sinkCoerced);
        }

        public void Move(IMotionInfo info, object context, double delta)
        {
            var sinkDelta = this.NormalizedToSink(delta);
            if (DoubleEx.IsZero(sinkDelta))
                return;
            this.Client.Move(info, context, sinkDelta);
        }

        public virtual double SinkToNormalized(double value)
        {
            return value / this.MotionIncrement;
        }

        public virtual double NormalizedToSink(double value)
        {
            return value * this.MotionIncrement;
        }

        public virtual void Dispose()
        {
            if (this._manipulator != null)
                this._manipulator.Dispose();
            if (this._wheel != null)
                this._wheel.ActiveTransferCaseChanged -= this.OnWheelActiveTransferCaseChanged;
        }

        bool INativeMotionTarget.CanMove(IMotionInfo info, object context)
        {
            return true;
        }

        int INativeMotionTarget.Coerce(IMotionInfo info, object context, int nativeDelta)
        {
            return nativeDelta;
        }

        void INativeMotionTarget.Move(IMotionInfo info, object context, int nativeDelta)
        {
            (context as MouseWheelInputEventArgs).RaiseNativeEvent(nativeDelta);
        }

        protected void InvalidateShaft()
        {
            this._shaft = null;
        }

        protected bool TransferMotionNative(MouseWheelInputEventArgs e)
        {
            return this.GetMotionShaft(e.Wheel).Transfer(this, e);
        }

        protected bool TransferMotion(MouseWheelInputEventArgs e)
        {
            var shaft = this.GetMotionShaft(e.Wheel);
            if (shaft.Transfer(this.MotionInput, e))
                return true; // client can still move - stop event propagation
            if (this.NestedMotionEnabled)
                return false; // let the mouse wheel event propagate up the visual tree
                              // empty the shaft transfer staging area and stop event propagation
            return shaft.Transfer(NativeMotionTarget.Terminal, e);
        }

        protected virtual IMotionTarget MotionInput => this;

        protected virtual double MotionIncrement => this.Client.MotionIncrement;

        protected virtual double CoerceSinkDelta(double sinkDelta)
        {
            return sinkDelta;
        }

        protected virtual IMouseWheelShaft GetMotionShaft(MouseWheel wheel, IMouseWheelTransferCase transferCase)
        {
            switch (this.Debouncing)
            {
                case MouseWheelDebouncing.Auto:
                    return this.GetMotionShaftAuto(wheel, transferCase, -1);
                case MouseWheelDebouncing.None:
                    return transferCase[0];  // no debouncing
                case MouseWheelDebouncing.Single:
                    return transferCase[1];  // one debouncing cell per click - same as a standard resolution notch
                default:
                    throw new NotImplementedException();
            }
        }

        protected IMouseWheelShaft GetMotionShaftAuto(MouseWheel wheel, IMouseWheelTransferCase transferCase, double debouncingCellCount)
        {
            var resolution = wheel.Resolution;
            if (DoubleEx.AreClose(resolution, (int)resolution))
                return transferCase[debouncingCellCount];  // the most granular debouncing
            else
                return transferCase[0];   // no debouncing if wheel resolution not integral
        }

        protected IMouseWheelShaft GetMotionShaftAuto(MouseWheel wheel, IMouseWheelTransferCase transferCase, int debouncingCellCount)
        {
            var resolution = wheel.Resolution;
            if (DoubleEx.AreClose(resolution, (int)resolution))
                return transferCase[debouncingCellCount];  // the most granular debouncing
            else
                return transferCase[0];   // no debouncing if wheel resolution not integral
        }

        private MouseWheel Wheel
        {
            get => this._wheel;
            set
            {
                if (this._wheel == value)
                    return;
                if (this._wheel != null)
                    this._wheel.ActiveTransferCaseChanged -= this.OnWheelActiveTransferCaseChanged;
                this._wheel = value;
                this._wheel.ActiveTransferCaseChanged += this.OnWheelActiveTransferCaseChanged;
                this._shaft = null;
            }
        }

        private IMouseWheelShaft GetMotionShaft(MouseWheel wheel)
        {
            this.Wheel = wheel;
            if (this._shaft == null)
                this._shaft = this.GetMotionShaft(wheel, wheel.ActiveTransferCase);
            wheel.ActiveTransferCase.ActiveShaft = this._shaft;
            return this._shaft;
        }

        private void OnWheelActiveTransferCaseChanged(object sender, EventArgs e)
        {
            this.InvalidateShaft();
        }

        private readonly IMouseWheelClient _client;
        private readonly IDisposable _manipulator;
        private IMouseWheelShaft _shaft;
        private MouseWheel _wheel;
    }

    public class MouseWheelNativeBehavior : MouseWheelBehavior
    {
        public MouseWheelNativeBehavior(IMouseWheelClient client, IDisposable manipulator = null)
            : base(client, manipulator) { }

        public override bool NestedMotionEnabled
        {
            get => false;

            protected set => throw new NotSupportedException();
        }

        public override MouseWheelDebouncing Debouncing
        {
            get => MouseWheelDebouncing.None;

            protected set => throw new NotSupportedException();
        }

        public override void OnInput(object sender, MouseWheelInputEventArgs e)
        {
            e.Handled = this.TransferMotionNative(e);
        }

        protected override IMouseWheelShaft GetMotionShaft(MouseWheel wheel, IMouseWheelTransferCase transferCase)
        {
            return transferCase[0];
        }
    }
}
