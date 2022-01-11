namespace WpfMouseWheel.Windows.MotionFlow
{
    using System;
    using System.Diagnostics;

    public interface INativeMotionSourceInput : INativeMotionInput
    {
        void Transmit(int timeStamp, int nativeDelta);
    }

    public interface INativeMotionConverter
    {
        int NativeResolutionFrequency
        {
            get;
        }

        double NativeToNormalized(int value);

        int NormalizedToNative(double value);
    }

    public interface INativeMotionSource : IMotionInfo, INativeMotionSourceInput, INativeMotionOutput, INativeMotionConverter
    {
    }

    public abstract class NativeMotionSource : MotionElementLink, INativeMotionSource
    {
        public NativeMotionSource()
        {
            this.Next = NativeMotionTerminal.Current;
        }

        public IMotionInfo Source => this;

        public TimeSpan Time => TimeSpan.FromMilliseconds(this._timeStamp);

        public TimeSpan Delay
        {
            get => this._delay;

            internal set => this._delay = value;
        }

        public double Velocity => this._velocity;

        public double Speed => Math.Abs(this._velocity);

        public int Direction => -this._nativeDirection;

        public bool DirectionChanged => this._previousNativeDirection != this._nativeDirection;

        public int NativeDirection
        {
            get => this._nativeDirection;

            private set
            {
                if (this._nativeDirection == value)
                    return;
                this._previousNativeDirection = this._nativeDirection;
                this._nativeDirection = value;
            }
        }

        public void Transmit(int timeStamp, int nativeDelta)
        {
            var info = this.PreTransmit(timeStamp, nativeDelta);
            this.Transmit(info, nativeDelta, null);
        }

        public void Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            this.Next.Transmit(info, nativeDelta, this);
        }

        public void OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            this.Next.OnCoupledTransfer(info, nativeDelta, source);
        }

        public void Reset()
        {
            this.Next.Reset();
        }

        public INativeMotionInput Next
        {
            [DebuggerStepThrough]
            get => this.GetNext() as INativeMotionInput;

            [DebuggerStepThrough]
            set => this.SetNext(value);
        }

        public abstract int NativeResolutionFrequency
        {
            get;
        }

        public abstract double NativeToNormalized(int value);

        public abstract int NormalizedToNative(double value);

        public virtual IMotionInfo PreTransmit(int timeStamp, int nativeDelta)
        {
            if (nativeDelta != 0)
            {
                this.NativeDirection = Math.Sign(nativeDelta);

                this.UpdateTimings(timeStamp);

                var delta = this.NativeToNormalized(nativeDelta);
                this.UpdateVelocity(delta);
            }

            return this;
        }

        private void UpdateTimings(long timeStamp)
        {
            // for information on timestamp seee http://msdn.microsoft.com/en-us/library/ms644939(VS.85).aspx

            if (timeStamp < 0)
                throw new ArgumentOutOfRangeException("timeStamp");

            if (this._timeStamp >= 0)
            {
                // fix timestamp wrapping issue
                if (timeStamp < this._timeStamp)
                    timeStamp += int.MaxValue;
                this._delay = TimeSpan.FromMilliseconds(timeStamp - this._timeStamp);
            }

            this._timeStamp = timeStamp;
        }

        private void UpdateVelocity(double delta)
        {
            if (this._delay != TimeSpan.Zero)
                this._velocity = delta / this._delay.TotalSeconds;
        }

        private long _timeStamp = -1;
        private TimeSpan _delay = TimeSpan.Zero;
        private int _previousNativeDirection;
        private int _nativeDirection;
        private double _velocity;
    }
}
