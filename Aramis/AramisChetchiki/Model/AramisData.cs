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
        public Dictionary<ulong, IList<Payment>> Payments { get; set; }

        /// <summary>
        /// Контрольные показания по лицевому счету
        /// </summary>
        [MessagePack.Key(6)]
        public Dictionary<ulong, IList<ControlData>> MetersControlData { get; set; }

        /// <summary>
        /// События по каждому лицевому счёту: замена счётчика, оплата, контрольные показания
        /// </summary>
        [MessagePack.Key(7)]
        public Dictionary<ulong, IList<MeterEvent>> Events { get; set; }

        public AramisData()
        {
            this.Info = new AramisDataInfo();

            this.Events = new Dictionary<ulong, IList<MeterEvent>>();
        }

        public void Clear()
        {
            this.Info = null;

            if (this.Meters != null && this.Meters is ICollection<Meter> meters)
            {
                if (meters is IReadOnlyCollection<Meter> meters2)
                {
                    meters2 = null;
                }
                else
                {
                    meters.Clear();
                    meters = null;
                }
            }
            else
            {
                this.Meters = null;
            }

            this.ChangesOfMeters?.Clear();
            this.ChangesOfMeters = null;

            if (this.SummaryInfos != null && this.SummaryInfos is ICollection<SummaryInfoItem> summaryInfos)
            {
                if (summaryInfos is IReadOnlyCollection<SummaryInfoItem> summaryInfos2)
                {
                    summaryInfos2 = null;
                }
                else
                {
                    summaryInfos.Clear();
                    summaryInfos = null;
                }
            }
            else
            {
                this.SummaryInfos = null;
            }

            if (this.ElectricitySupplyInfo != null && this.ElectricitySupplyInfo is ICollection<ElectricitySupply> electricitySupplis)
            {
                if (electricitySupplis is IReadOnlyCollection<ElectricitySupply> electricitySupplis2)
                {
                    electricitySupplis2 = null;
                }
                else
                {
                    electricitySupplis.Clear();
                    electricitySupplis = null;
                }
            }
            else
            {
                this.ElectricitySupplyInfo = null;
            }

            if (this.Payments != null)
            {
                foreach (KeyValuePair<ulong, IList<Payment>> item in this.Payments)
                {
                    item.Value?.Clear();
                }
                this.Payments.Clear();
                this.Payments = null;
            }

            if (this.MetersControlData != null)
            {
                foreach (KeyValuePair<ulong, IList<ControlData>> item in this.MetersControlData)
                {
                    item.Value?.Clear();
                }
                this.MetersControlData.Clear();
                this.MetersControlData = null;
            }

            if (this.Events != null)
            {
                foreach (KeyValuePair<ulong, IList<MeterEvent>> item in this.Events)
                {
                    item.Value?.Clear();
                }
                this.Events.Clear();
                this.Events = null;
            }
        }
    }
}
