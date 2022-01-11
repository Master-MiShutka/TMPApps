namespace WpfMouseWheel.Windows.MotionFlow
{
    using System;

    public interface IMotionInfo
    {
        IMotionInfo Source
        {
            get;
        }

        TimeSpan Time
        {
            get;
        }

        TimeSpan Delay
        {
            get;
        }

        double Velocity
        {
            get;
        }

        double Speed
        {
            get;
        }

        int NativeDirection
        {
            get;
        }

        int Direction
        {
            get;
        }

        bool DirectionChanged
        {
            get;
        }
    }
}
