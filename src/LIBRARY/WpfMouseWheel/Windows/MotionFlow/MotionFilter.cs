namespace WpfMouseWheel.Windows.MotionFlow
{
    using System;

    public interface IMotionFilter
    {
        void NewInputDelta(TimeSpan t, double delta, IMotionInfo info);

        double NextOutputDelta(TimeSpan t);
    }
}
