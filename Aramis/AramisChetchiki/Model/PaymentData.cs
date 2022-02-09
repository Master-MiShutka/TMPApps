namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using TMP.Shared;

    internal class RawPaymentData : IModelWithPersonalId
    {
        public ulong Лицевой { get; set; }

        public DateOnly ДатаОплаты { get; set; }

        public DateOnly ПериодОплаты { get; set; }

        public int ПредыдущееПоказание { get; set; }

        public int ПоследнееПоказание { get; set; }

        public int РазностьПоказанийПоКвитанции { get; set; }

        public int РазностьПоказанийРасчётная { get; set; }

        public decimal ВеличинаТарифа { get; set; }

        public decimal СуммаОплаты { get; set; }

        public decimal СуммаОплатыРасчётная { get; set; }

        public decimal? ПеняОплаченная { get; set; }

        public decimal? ПеняВыставленая { get; set; }
    }

    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    [System.Diagnostics.DebuggerDisplay("{ДатаОплаты} :: {ПериодОплаты} :: {ПредыдущееПоказание} :: {ПоследнееПоказание} :: {РазностьПоказанийПоКвитанции}")]
    public class PaymentData
    {
        [Display(GroupName = "Оплата")]
        [DisplayName("Дата оплаты")]
        [DateTimeDataFormat]
        [System.Text.Json.Serialization.JsonConverter(typeof(TMP.Common.RepositoryCommon.DateOnlyConverter))]
        [MessagePack.MessagePackFormatter(typeof(TMP.Common.RepositoryCommon.DateOnlyFormatter))]
        public DateOnly ДатаОплаты { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Период оплаты")]
        [DateTimeDataFormat]
        [System.Text.Json.Serialization.JsonConverter(typeof(TMP.Common.RepositoryCommon.DateOnlyConverter))]
        [MessagePack.MessagePackFormatter(typeof(TMP.Common.RepositoryCommon.DateOnlyFormatter))]
        public DateOnly ПериодОплаты { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Предыдущее показание")]
        public uint ПредыдущееПоказание { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Последнее показание")]
        public uint ПоследнееПоказание { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Разность показаний по квитанции")]
        public uint РазностьПоказанийПоКвитанции { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Разность показаний расчётная")]
        public uint РазностьПоказанийРасчётная { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Величина тарифа")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public float ВеличинаТарифа { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Сумма оплаты по квитанции")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public float СуммаОплаты { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Сумма оплаты расчётная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public float? СуммаОплатыРасчётная { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Пеня оплаченная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public float? ПеняОплаченная { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Пеня выставленная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public float? ПеняВыставленая { get; set; }

        internal static PaymentData FromRawData(RawPaymentData rawPaymentData)
        {
            return new PaymentData()
            {
                ДатаОплаты = rawPaymentData.ДатаОплаты,
                ПериодОплаты = rawPaymentData.ПериодОплаты,
                ПредыдущееПоказание = (uint)rawPaymentData.ПредыдущееПоказание,
                ПоследнееПоказание = (uint)rawPaymentData.ПоследнееПоказание,
                РазностьПоказанийПоКвитанции = (uint)rawPaymentData.РазностьПоказанийПоКвитанции,
                РазностьПоказанийРасчётная = (uint)rawPaymentData.РазностьПоказанийРасчётная,
                ВеличинаТарифа = (float)rawPaymentData.ВеличинаТарифа,
                СуммаОплаты = (float)rawPaymentData.СуммаОплаты,
                СуммаОплатыРасчётная = (float?)rawPaymentData.СуммаОплатыРасчётная,
                ПеняОплаченная = (float?)rawPaymentData.ПеняОплаченная,
                ПеняВыставленая = (float?)rawPaymentData.ПеняВыставленая,
            };
        }
    }

    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    [System.Diagnostics.DebuggerDisplay("{Лицевой} :: {ПериодОплаты} :: {ПредыдущееПоказание} :: {ПоследнееПоказание} :: {РазностьПоказаний}")]
    public class Payment : IModelWithPersonalId, IModelWithMeterLastReading
    {
        [Display(GroupName = "Оплата")]
        [DisplayName("Период оплаты")]
        [DateTimeDataFormat]
        [System.Text.Json.Serialization.JsonConverter(typeof(TMP.Common.RepositoryCommon.DateOnlyConverter))]
        [MessagePack.MessagePackFormatter(typeof(TMP.Common.RepositoryCommon.DateOnlyFormatter))]
        public DateOnly ПериодОплаты { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Лицевой счёт абонента")]
        [PersonalAccountDataFormatAttribute]
        public ulong Лицевой { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Предыдущее показание")]
        public uint ПредыдущееПоказание { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Последнее показание")]
        public uint ПоследнееПоказание { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Разность показаний")]
        [MessagePack.IgnoreMember]
        public uint РазностьПоказаний
        {
            get
            {
                long consumption = this.ПоследнееПоказание - this.ПредыдущееПоказание;

                if (consumption < 0)
                {
                    int meterDigitsCount = this.ПредыдущееПоказание.ToString().Length;
                    var c = (this.ПоследнееПоказание + (int)Math.Pow(10, meterDigitsCount)) - this.ПредыдущееПоказание;

                    if (c < 0 || c >= 50_000)
                    {
                        System.Diagnostics.Debugger.Break();
                    }

                    return (uint)c;
                }


                return (uint)consumption;
            }
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Сумма оплаты")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public float СуммаОплаты { get; set; }

        public PaymentData[] Payments { get; set; }

        [MessagePack.IgnoreMember]
        public bool HasPayments => this.Payments?.Length > 0;

        [MessagePack.IgnoreMember]
        public uint LastReading => this.ПоследнееПоказание;
    }
}
