using System.ComponentModel;
using TMP.Shared;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Статус данных
    /// </summary>
    public enum DataValueStatus
    {
        [Description("Нормальные")]
        Normal,
        [Description("Отсутствуют")]
        Missing
    }
    /// <summary>
    /// Данные
    /// </summary>
    [Magic]
    public class DataValue : TMP.Work.Emcos.PropertyChangedBase
    {
        public double? DoubleValue { get; set; }
        public double? PercentValue { get; set; }
        public DataValueStatus Status { get; set; } = DataValueStatus.Normal;
    }
}
