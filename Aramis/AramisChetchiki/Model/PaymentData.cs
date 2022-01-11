namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using TMP.Shared;

    [MessagePack.MessagePackObject]
    public class PaymentData : IModelWithPersonalId
    {
        [Display(GroupName = "Оплата")]
        [DisplayName("Дата оплаты")]
        [DateTimeDataFormat]
        [MessagePack.Key(0)]
        public DateOnly ДатаОплаты
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Период оплаты")]
        [DateTimeDataFormat]
        [MessagePack.Key(1)]
        public DateOnly ПериодОплаты
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Лицевой счёт абонента")]
        [PersonalAccountDataFormat]
        [MessagePack.Key(2)]
        public ulong Лицевой
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Предыдущее показание")]
        [MessagePack.Key(3)]
        public int ПредыдущееПоказание
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Последнее показание")]
        [MessagePack.Key(4)]
        public int ПоследнееПоказание
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Разность показаний по квитанции")]
        [MessagePack.Key(5)]
        public int РазностьПоказанийПоКвитанции
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Разность показаний расчётная")]
        [MessagePack.Key(6)]
        public int РазностьПоказанийРасчётная
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Величина тарифа")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(7)]
        public decimal ВеличинаТарифа
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Сумма оплаты по квитанции")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(8)]
        public decimal СуммаОплаты
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Сумма оплаты расчётная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(9)]
        public decimal СуммаОплатыРасчётная
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Пеня оплаченная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(10)]
        public decimal ПеняОплаченная
        {
            get;
            set;
        }

        [Display(GroupName = "Оплата")]
        [DisplayName("Пеня выставленная")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        [MessagePack.Key(11)]
        public decimal ПеняВыставленая
        {
            get;
            set;
        }
    }
}
