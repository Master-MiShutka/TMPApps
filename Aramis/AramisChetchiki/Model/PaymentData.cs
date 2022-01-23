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

        public decimal ПеняОплаченная { get; set; }

        public decimal ПеняВыставленая { get; set; }
    }

    [MessagePack.MessagePackObject]
    [System.Diagnostics.DebuggerDisplay("{ДатаОплаты} :: {ПериодОплаты} :: {ПредыдущееПоказание} :: {ПоследнееПоказание} :: {РазностьПоказанийПоКвитанции}")]
    public class PaymentData
    {
        [Display(GroupName = "Оплата")]
        [DisplayName("Дата оплаты")]
        [DateTimeDataFormat]
        [MessagePack.Key(0)]
        public DateOnly ДатаОплаты { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Период оплаты")]
        [DateTimeDataFormat]
        [MessagePack.Key(1)]
        public DateOnly ПериодОплаты { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Предыдущее показание")]
        [MessagePack.Key(2)]
        public int ПредыдущееПоказание { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Последнее показание")]
        [MessagePack.Key(3)]
        public int ПоследнееПоказание { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Разность показаний по квитанции")]
        [MessagePack.Key(4)]
        public int РазностьПоказанийПоКвитанции { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Разность показаний расчётная")]
        [MessagePack.Key(5)]
        public int РазностьПоказанийРасчётная { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Величина тарифа")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(6)]
        public decimal ВеличинаТарифа { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Сумма оплаты по квитанции")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(7)]
        public decimal СуммаОплаты { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Сумма оплаты расчётная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(8)]
        public decimal СуммаОплатыРасчётная { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Пеня оплаченная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(9)]
        public decimal ПеняОплаченная { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Пеня выставленная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(10)]
        public decimal ПеняВыставленая { get; set; }

        internal static PaymentData FromRawData(RawPaymentData rawPaymentData)
        {
            return new PaymentData()
            {
                ДатаОплаты = rawPaymentData.ДатаОплаты,
                ПериодОплаты = rawPaymentData.ПериодОплаты,
                ПредыдущееПоказание = rawPaymentData.ПредыдущееПоказание,
                ПоследнееПоказание = rawPaymentData.ПоследнееПоказание,
                РазностьПоказанийПоКвитанции = rawPaymentData.РазностьПоказанийПоКвитанции,
                РазностьПоказанийРасчётная = rawPaymentData.РазностьПоказанийРасчётная,
                ВеличинаТарифа = rawPaymentData.ВеличинаТарифа,
                СуммаОплаты = rawPaymentData.СуммаОплаты,
                СуммаОплатыРасчётная = rawPaymentData.СуммаОплатыРасчётная,
                ПеняОплаченная = rawPaymentData.ПеняОплаченная,
                ПеняВыставленая = rawPaymentData.ПеняВыставленая,
            };
        }
    }

    [MessagePack.MessagePackObject]
    [System.Diagnostics.DebuggerDisplay("{Лицевой} :: {ПериодОплаты} :: {ПредыдущееПоказание} :: {ПоследнееПоказание} :: {РазностьПоказаний}")]
    public class Payment : IModelWithPersonalId, IModelWithMeterLastReading
    {
        [Display(GroupName = "Оплата")]
        [DisplayName("Период оплаты")]
        [DateTimeDataFormat]
        [MessagePack.Key(0)]
        public DateOnly ПериодОплаты { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Лицевой счёт абонента")]
        [PersonalAccountDataFormat]
        [MessagePack.Key(1)]
        public ulong Лицевой { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Предыдущее показание")]
        [MessagePack.Key(2)]
        public int ПредыдущееПоказание { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Последнее показание")]
        [MessagePack.Key(3)]
        public int ПоследнееПоказание { get; set; }

        [Display(GroupName = "Оплата")]
        [DisplayName("Разность показаний")]
        [MessagePack.IgnoreMember]
        public int РазностьПоказаний => this.ПоследнееПоказание - this.ПредыдущееПоказание;

        [Display(GroupName = "Оплата")]
        [DisplayName("Сумма оплаты")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(4)]
        public decimal СуммаОплаты { get; set; }

        [MessagePack.Key(5)]
        public PaymentData[] Payments { get; set; }

        [MessagePack.IgnoreMember]
        public bool HasPayments => this.Payments?.Length > 0;

        [MessagePack.IgnoreMember]
        public int LastReading => this.ПоследнееПоказание;
    }
}
