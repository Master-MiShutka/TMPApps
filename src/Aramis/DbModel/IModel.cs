namespace TMP.WORK.AramisChetchiki.DbModel
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
