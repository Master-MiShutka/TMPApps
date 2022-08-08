namespace TMP.Common.RepositoryCommon
{
    using MessagePack;
    using MessagePack.Formatters;
    using MsgPack.Serialization;

    public interface IData
    {
        IDataFileInfo Info { get; set; }
    }
}
