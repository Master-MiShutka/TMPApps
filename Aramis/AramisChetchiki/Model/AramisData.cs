namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using TMP.Common.RepositoryCommon;

    [MessagePack.MessagePackObject]
    [KnownType(typeof(AramisDataInfo))]
    [KnownType(typeof(Meter))]
    [KnownType(typeof(ChangeOfMeter))]
    [KnownType(typeof(SummaryInfoItem))]
    public class AramisData : TMP.Common.RepositoryCommon.IData
    {
        /// <summary>
        /// Информация о данных
        /// </summary>
        [MessagePack.Key(0)]
        public IDataFileInfo Info { get; set; }

        /// <summary>
        /// Список счётчиков
        /// </summary>
        [MessagePack.Key(1)]
        public IEnumerable<Meter> Meters { get; set; }

        /// <summary>
        /// Замены счётчиков по лицевому счету
        /// </summary>
        [MessagePack.Key(2)]
        public Dictionary<ulong, IList<ChangeOfMeter>> ChangesOfMeters { get; set; }

        /// <summary>
        /// Список сводной информации по базе данных
        /// </summary>
        [MessagePack.Key(3)]
        public IEnumerable<SummaryInfoItem> SummaryInfos { get; set; }

        /// <summary>
        /// Информация о полезном отпуске
        /// </summary>
        [MessagePack.Key(4)]
        public IEnumerable<ElectricitySupply> ElectricitySupplyInfo { get; set; }

        /// <summary>
        /// Информация об произведенных оплатах по лицевому счету
        /// </summary>
        [MessagePack.Key(5)]
        public Dictionary<ulong, IList<PaymentData>> PaymentDataInfo { get; set; }

        /// <summary>
        /// Контрольные показания по лицевому счету
        /// </summary>
        [MessagePack.Key(6)]
        public Dictionary<ulong, IList<ControlData>> MetersControlData { get; set; }

        public AramisData()
        {
            this.Info = new AramisDataInfo();
        }
    }
}
