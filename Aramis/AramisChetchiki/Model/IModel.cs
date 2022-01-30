namespace TMP.WORK.AramisChetchiki.Model
{
    public interface IModel
    {
    }

    public interface IModelWithPersonalId : IModel
    {
        ulong Лицевой { get; }
    }

    public interface IModelWithMeterLastReading : IModel
    {
        uint LastReading { get; }
    }
}
