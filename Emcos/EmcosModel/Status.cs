namespace TMP.Work.Emcos.Model
{
    public enum DataStatus
    {
        Wait,
        Processing,
        Processed
    }

    public interface IProgress
    {
        int Progress { get; }
        DataStatus Status { get; set; }
    }
}
