namespace WpfMouseWheel.Windows.Input
{
    using WpfMouseWheel.Windows.MotionFlow;

    public interface IMouseWheelShaft : INativeMotionTransfer
    {
        int Resolution
        {
            get;
        }
    }

    public class MouseWheelShaft : NativeMotionTransfer, IMouseWheelShaft
    {
        // TODO: still to implement some resource management with parent transfer case.
        public MouseWheelShaft(int resolution)
        {
            this.Resolution = resolution;
        }

        public int Resolution
        {
            get; private set;
        }
    }
}
