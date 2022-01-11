namespace WpfMouseWheel.Windows.MotionFlow
{
    public interface IMotionSinkConverter
    {
        double SinkToNormalized(double value);

        double NormalizedToSink(double value);
    }

    public interface IMotionSink : IMotionTarget, IMotionSinkConverter
    {
    }
}
