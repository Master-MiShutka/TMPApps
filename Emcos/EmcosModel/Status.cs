namespace TMP.Work.Emcos.Model
{
    /// <summary>
    /// Статус данных
    /// </summary>
    public enum DataStatus
    {
        /// <summary>
        /// ожидание
        /// </summary>
        Wait,
        /// <summary>
        /// обработка
        /// </summary>
        Processing,
        /// <summary>
        /// обработаны
        /// </summary>
        Processed
    }

    public interface IProgress
    {
        int Progress { get; }
        DataStatus Status { get; set; }
    }
}
