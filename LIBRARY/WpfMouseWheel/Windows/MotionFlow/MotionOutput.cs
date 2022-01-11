namespace WpfMouseWheel.Windows.MotionFlow
{
    public interface INativeMotionOutput
    {
        INativeMotionInput Next
        {
            get; set;
        }
    }

    public interface IMotionOutput
    {
        IMotionInput Next
        {
            get; set;
        }
    }
}
