namespace WpfMouseWheel.Windows.MotionFlow
{
    using System;
    using System.Windows.Media;
    using WpfMouseWheel.Diagnostics;

    public interface INativeMotionTarget
    {
        bool CanMove(IMotionInfo info, object context);

        int Coerce(IMotionInfo info, object context, int nativeDelta);

        void Move(IMotionInfo info, object context, int nativeDelta);
    }

    public interface IMotionTarget
    {
        bool CanMove(IMotionInfo info, object context);

        double Coerce(IMotionInfo info, object context, double delta);

        void Move(IMotionInfo info, object context, double delta);
    }

    public class NativeMotionTarget : INativeMotionTarget
    {
        public static readonly NativeMotionTarget Terminal = new NativeMotionTarget();

        public virtual bool CanMove(IMotionInfo info, object context)
        {
            return true;
        }

        public virtual int Coerce(IMotionInfo info, object context, int nativeDelta)
        {
            return nativeDelta;
        }

        public virtual void Move(IMotionInfo info, object context, int nativeDelta)
        {
        }
    }

    public class MotionTarget : IMotionTarget
    {
        public virtual bool CanMove(IMotionInfo info, object context)
        {
            return !DoubleEx.IsZero(this.Coerce(info, context, info.Direction));
        }

        public virtual double Coerce(IMotionInfo info, object context, double delta)
        {
            return delta;
        }

        public virtual void Move(IMotionInfo info, object context, double delta)
        {
        }
    }

    public class MotionTargetLink : MotionTarget
    {
        public IMotionTarget Next
        {
            get; set;
        }

        public override bool CanMove(IMotionInfo info, object context)
        {
            return this.Next.CanMove(info, context);
        }

        public override double Coerce(IMotionInfo info, object context, double delta)
        {
            return this.Next.Coerce(info, context, delta);
        }

        public override void Move(IMotionInfo info, object context, double delta)
        {
            this.Next.Move(info, context, delta);
        }
    }

    public class MotionSmoothingTarget : MotionTargetLink, IDisposable
    {
        public MotionSmoothingTarget(IMotionFilter filter)
        {
            this._filter = filter;
        }

        public double Precision
        {
            get;
            set;
        }

        public override void Move(IMotionInfo info, object context, double delta)
        {
            var t = TimeBase.Current.Elapsed;
            this._filter.NewInputDelta(t, delta, info);
            CompositionTarget.Rendering -= this.OnRendering;
            CompositionTarget.Rendering += this.OnRendering;
        }

        public void Dispose()
        {
            CompositionTarget.Rendering -= this.OnRendering;
        }

        private void OnRendering(object sender, EventArgs e)
        {
            var t = TimeBase.Current.Elapsed;
            double delta = this._remainder + this._filter.NextOutputDelta(t);
            if (DoubleEx.GreaterThanOrClose(Math.Abs(delta), this.Precision))
            {
                this._remainder = 0;
                this.Next.Move(null, null, delta);
            }
            else if (DoubleEx.IsZero(delta))
            {
                CompositionTarget.Rendering -= this.OnRendering;
            }
            else
            {
                this._remainder += delta;
            }
        }

        private readonly IMotionFilter _filter;
        private double _remainder;
    }
}
