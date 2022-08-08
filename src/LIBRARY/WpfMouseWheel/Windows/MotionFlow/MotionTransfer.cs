namespace WpfMouseWheel.Windows.MotionFlow
{
    using System;
    using System.Windows;

    public interface IMotionTransferOutput
    {
        IMotionInfo MotionInfo
        {
            get;
        }

        double Remainder
        {
            get;
        }

        bool Transfer(IMotionTarget target, object context);
    }

    public interface INativeMotionTransferOutput : IMotionTransferOutput
    {
        int NativeRemainder
        {
            get;
        }

        bool Transfer(INativeMotionTarget target, object context);
    }

    public interface IMotionTransfer : IMotionInput, IMotionTransferOutput
    {
    }

    public interface INativeMotionTransfer : INativeMotionInput, INativeMotionTransferOutput
    {
    }

    public class MotionTransfer : MotionElement, IMotionTransfer
    {
        public static readonly RoutedEvent TransferingEvent = EventManager.RegisterRoutedEvent("Transfering", RoutingStrategy.Bubble, typeof(MotionTransferEventHandler), typeof(MotionTransfer));
        public static readonly RoutedEvent TransferedEvent = EventManager.RegisterRoutedEvent("Transfered", RoutingStrategy.Bubble, typeof(MotionTransferEventHandler), typeof(MotionTransfer));

        public MotionTransfer()
        {
            this.Name = this.Id.ToString("'T'00");
        }

        public void Transmit(IMotionInfo info, double delta, IMotionOutput source)
        {
            if (info != null)
                this._info = info;
            this._sourceDelta += delta;
        }

        public void OnCoupledTransfer(IMotionInfo info, double delta, IMotionTransferOutput source)
        {
            if (source != this)
                this._sourceDelta -= delta;
        }

        public void Reset()
        {
            this._sourceDelta = 0;
        }

        public IMotionInfo MotionInfo => this._info;

        public double Remainder => this._sourceDelta;

        public bool Transfer(IMotionTarget target, object context)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            this.RaiseEvent(new MotionTransferEventArgs(this._info, this._sourceDelta) { RoutedEvent = TransferingEvent });

            if (Math.Sign(this._sourceDelta) == this._info.Direction)
            {
                var converter = this._info.Source as INativeMotionConverter;
                double targetDelta = target.Coerce(this._info, context, this._sourceDelta);
                if (!DoubleEx.IsZero(targetDelta))
                {
                    target.Move(this._info, context, targetDelta);
                    this._sourceDelta -= targetDelta;
                    this.RaiseEvent(new MotionTransferEventArgs(this._info, targetDelta) { RoutedEvent = TransferedEvent });
                }
            }

            return target.CanMove(this._info, context);
        }

        public override string ToString()
        {
            return string.Format("{0} : Remainder={1}", this.Name, this.Remainder);
        }

        private bool IsDirectionWrong => this._info.Direction != Math.Sign(this._sourceDelta);

        private IMotionInfo _info;
        private double _sourceDelta;
    }

    public class NativeMotionTransfer : MotionElement, INativeMotionTransfer
    {
        public static readonly RoutedEvent TransferingEvent = EventManager.RegisterRoutedEvent("Transfering", RoutingStrategy.Bubble, typeof(NativeMotionTransferEventHandler), typeof(NativeMotionTransfer));
        public static readonly RoutedEvent TransferedEvent = EventManager.RegisterRoutedEvent("Transfered", RoutingStrategy.Bubble, typeof(NativeMotionTransferEventHandler), typeof(NativeMotionTransfer));

        public NativeMotionTransfer()
        {
            this.Name = this.Id.ToString("'T'00");
        }

        public IMotionInfo MotionInfo => this._info;

        public double Remainder
        {
            get
            {
                if (this._info == null)
                    return 0;
                return (this._info.Source as INativeMotionConverter).NativeToNormalized(this._nativeSourceDelta);
            }
        }

        public int NativeRemainder => this._nativeSourceDelta;

        public virtual void Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            if (info != null)
                this._info = info;
            this._nativeSourceDelta += nativeDelta;

            // if (nativeDelta != 0)
            //  Debug.WriteLine("{0}{1}(delta = {2,4}) --> {3,4}{4}",
            //    Name, TransmitMethodSuffix(info, nativeDelta),
            //    nativeDelta, _nativeSourceDelta,
            //    _nativeSourceDelta != 0 && Math.Sign(_nativeSourceDelta) != info.NativeDirection ? " (##)" : "");
        }

        public void Reset()
        {
            this._nativeSourceDelta = this._nativeTransferCreditDelta = 0;
        }

        public bool Transfer(IMotionTarget target, object context)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            this.RaiseEvent(new NativeMotionTransferEventArgs(this._info, this._nativeSourceDelta) { RoutedEvent = TransferingEvent });

            if (Math.Sign(this._nativeSourceDelta) == this._info.NativeDirection)
            {
                var converter = this._info.Source as INativeMotionConverter;
                double sourceDelta = converter.NativeToNormalized(this._nativeSourceDelta);
                double targetDelta = target.Coerce(this._info, context, sourceDelta);
                if (!DoubleEx.IsZero(targetDelta))
                {
                    target.Move(this._info, context, targetDelta);
                    this.EndTransfer(converter.NormalizedToNative(targetDelta), converter.NativeResolutionFrequency);
                }
            }

            return target.CanMove(this._info, context);
        }

        public bool Transfer(INativeMotionTarget target, object context)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            this.RaiseEvent(new NativeMotionTransferEventArgs(this._info, this._nativeSourceDelta) { RoutedEvent = TransferingEvent });

            if (Math.Sign(this._nativeSourceDelta) == this._info.NativeDirection)
            {
                var converter = this._info.Source as INativeMotionConverter;
                int nativeTargetDelta = target.Coerce(this._info, context, this._nativeSourceDelta);
                if (!DoubleEx.IsZero(nativeTargetDelta))
                {
                    target.Move(this._info, context, nativeTargetDelta);
                    this.EndTransfer(nativeTargetDelta, converter.NativeResolutionFrequency);
                }
            }

            return target.CanMove(this._info, context);
        }

        public void OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            if (source != this)
            {
                this._nativeSourceDelta -= nativeDelta;

                // if (nativeDelta != 0)
                //  Debug.WriteLine("{0}{1}(delta = {2,4}) --> {3,4}{4}",
                //    Name, TransmitMethodSuffix(info, nativeDelta),
                //    nativeDelta, _nativeSourceDelta,
                //    _nativeSourceDelta != 0 && Math.Sign(_nativeSourceDelta) != info.NativeDirection ? " (##)" : "");
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: Remaining={1:F3}", this.Name, this.Remainder);
        }

        private void EndTransfer(int nativeTargetDelta, int resolutionFrequency)
        {
            this._nativeSourceDelta -= nativeTargetDelta;

            var nativeTransferDelta = nativeTargetDelta - this._nativeTransferCreditDelta;
            int nativeTransferSnappedDelta = MathEx.Snap(nativeTransferDelta, resolutionFrequency);
            this._nativeTransferCreditDelta = nativeTransferSnappedDelta - nativeTransferDelta;

            if (nativeTransferSnappedDelta != 0)
                this.RaiseEvent(new NativeMotionTransferEventArgs(this._info, nativeTransferSnappedDelta) { RoutedEvent = TransferedEvent });
        }

        private IMotionInfo _info;
        private int _nativeSourceDelta;
        private int _nativeTransferCreditDelta;
    }
}
