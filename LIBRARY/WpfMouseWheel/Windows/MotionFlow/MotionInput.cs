namespace WpfMouseWheel.Windows.MotionFlow
{
    public interface INativeMotionInput : IMotionElementInput
    {
        void Transmit(IMotionInfo info, int nativeDelta, INativeMotionOutput source);

        void OnCoupledTransfer(IMotionInfo info, int nativeDelta, INativeMotionTransferOutput source);

        void Reset();
    }

    public interface IMotionInput : IMotionElementInput
    {
        void Transmit(IMotionInfo info, double delta, IMotionOutput source);

        void OnCoupledTransfer(IMotionInfo info, double delta, IMotionTransferOutput source);

        void Reset();
    }
}
