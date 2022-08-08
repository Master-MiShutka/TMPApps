namespace WpfMouseWheel.Windows.MotionFlow
{
    using System.Diagnostics;
    using WpfMouseWheel.Maths;

    public interface IMotionTransform : IMotionInput, IMotionOutput
    {
    }

    public interface INativeMotionTransform : INativeMotionInput, INativeMotionOutput
    {
    }

    public class NativeMotionTransform : MotionElementLink, INativeMotionTransform
    {
        public NativeMotionTransform()
        {
            this.Next = NativeMotionTerminal.Current;
        }

        public virtual void Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            var nativeDeltaY = this.Transform(info, nativeDelta);

            // Debug.WriteLine("{0}{1}(delta = {2,4}) --> {3,4}", Name, TransmitMethodSuffix(info, nativeDelta), nativeDelta, nativeDeltaY);
            this.Next.Transmit(info, nativeDeltaY, this);
        }

        public virtual void OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            this.Next.OnCoupledTransfer(info, nativeDelta, source);
        }

        public virtual void Reset()
        {
            this.Next.Reset();
        }

        public virtual INativeMotionInput Next
        {
            get => this.GetNext() as INativeMotionInput;
            set => this.SetNext(value);
        }

        protected virtual int Transform(IMotionInfo info, int nativeDelta)
        {
            return nativeDelta;
        }
    }

    public class NativeDebouncedMotionTransformBase : NativeMotionTransform
    {
        public NativeDebouncedMotionTransformBase(Int32DifferentialFunctionPatternModulator debouncingFunction)
        {
            this._debouncingFunction = debouncingFunction;
        }

        public Int32DifferentialFunctionPatternModulator DebouncingFunction
        {
            [DebuggerStepThrough]
            get => this._debouncingFunction;
        }

        public override void Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            var nativeDeltaY = this.Transform(info, nativeDelta);

            // Debug.WriteLine("{0}{1}(delta = {2,4}) --> {3,4}", Name, TransmitMethodSuffix(info, nativeDelta), nativeDelta, nativeDeltaY);
            this.Next.Transmit(info, nativeDeltaY, source);
        }

        public override void Reset()
        {
            this._debouncingFunction.Reset();
            base.Reset();
        }

        protected override int Transform(IMotionInfo info, int nativeDelta)
        {
            return this.DebouncingFunction.DF(nativeDelta);
        }

        private readonly Int32DifferentialFunctionPatternModulator _debouncingFunction;
    }

    public class NativeDebouncedMotionTransform : NativeDebouncedMotionTransformBase
    {
        public NativeDebouncedMotionTransform(Int32DifferentialFunctionPatternModulator debouncingFunction)
          : base(debouncingFunction)
        {
            this.Initialize();
        }

        public override void OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            if (source != this._transferSource)
                this.CompensationInput.Transmit(info, -nativeDelta, this);
        }

        public override void Reset()
        {
            if (this._compensationInput != null)
                this._compensationInput.Reset();
            base.Reset();
        }

        private INativeMotionInput CompensationInput
        {
            get
            {
                if (this._compensationInput == null)
                {
                    this._compensationInput = new CompensationTransform(this.DebouncingFunction.Clone());
                    this._compensationInput.Next = this.Next;
                }

                return this._compensationInput;
            }
        }

        private void Initialize()
        {
            this.Name = this.Id.ToString("'R'00");
            this.AddHandler(NativeMotionTransfer.TransferingEvent, new NativeMotionTransferEventHandler(this.OnMotionTransfering));
        }

        private void OnMotionTransfering(object sender, NativeMotionTransferEventArgs e)
        {
            this._transferSource = e.Source;
        }

        private INativeMotionTransform _compensationInput;
        private object _transferSource;

        private class CompensationTransform : NativeDebouncedMotionTransformBase
        {
            public CompensationTransform(Int32DifferentialFunctionPatternModulator debouncingFunction)
              : base(debouncingFunction)
            {
                this.Name = this.Id.ToString("'r'00");
            }

            public override INativeMotionInput Next
            {
                get => this.GetNext(false) as INativeMotionInput;
                set => this.SetNext(value, false);
            }
        }
    }
}
