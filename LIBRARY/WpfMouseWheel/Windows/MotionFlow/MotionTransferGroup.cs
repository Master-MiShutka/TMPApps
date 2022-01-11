namespace WpfMouseWheel.Windows.MotionFlow
{
    using System;

    public class MotionTransferGroup : MotionElement, IMotionTransfer
    {
        public MotionTransferGroup(IMotionInput input, IMotionTransferOutput output)
        {
            this.Input = input;
            this.Output = output;
        }

        public IMotionInfo MotionInfo => this.Output.MotionInfo;

        public double Remainder => this.Output.Remainder;

        public void Transmit(IMotionInfo info, double delta, IMotionOutput source)
        {
            this.Input.Transmit(info, delta, source);
        }

        public void OnCoupledTransfer(IMotionInfo info, double delta, IMotionTransferOutput source)
        {
            this.Input.OnCoupledTransfer(info, delta, source);
        }

        public void Reset()
        {
            this.Input.Reset();
        }

        public bool Transfer(IMotionTarget target, object context)
        {
            return this.Output.Transfer(target, context);
        }

        public override string ToString()
        {
            return string.Format("Input={{{0}}}, Output={{{1}}}", this.Input, this.Output);
        }

        protected IMotionInput Input
        {
            get; private set;
        }

        protected IMotionTransferOutput Output
        {
            get; private set;
        }
    }

    public class NativeMotionTransferGroup : MotionElement, INativeMotionTransfer
    {
        public NativeMotionTransferGroup(INativeMotionInput input, INativeMotionTransferOutput output)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (output == null)
                throw new ArgumentNullException("output");

            input.SetParent(this);
            this.Input = input;
            this.Output = output;
        }

        public IMotionInfo MotionInfo => this.Output.MotionInfo;

        public double Remainder => this.Output.Remainder;

        public int NativeRemainder => this.Output.NativeRemainder;

        public void Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source)
        {
            this.Input.Transmit(info, nativeDelta, source);
        }

        public void OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source)
        {
            this.Input.OnCoupledTransfer(info, nativeDelta, source);
        }

        public void Reset()
        {
            this.Input.Reset();
        }

        public bool Transfer(IMotionTarget target, object context)
        {
            return this.Output.Transfer(target, context);
        }

        public bool Transfer(INativeMotionTarget target, object context)
        {
            return this.Output.Transfer(target, context);
        }

        public override string ToString()
        {
            return string.Format("Input={{{0}}}, Output={{{1}}}", this.Input, this.Output);
        }

        protected INativeMotionInput Input
        {
            get; private set;
        }

        protected INativeMotionTransferOutput Output
        {
            get; private set;
        }
    }
}
