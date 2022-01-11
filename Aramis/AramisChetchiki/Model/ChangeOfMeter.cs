namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using TMP.Shared;

    [MessagePack.MessagePackObject]
    public class ChangeOfMeter : IModelWithPersonalId
    {
        [Display(Order = 0, GroupName = "Операция")]
        [DisplayName("Дата замены")]
        [DateTimeDataFormat]
        [MessagePack.Key(0)]
        public DateOnly ДатаЗамены
        {
            get;
            set;
        }

        [Display(Order = 1, GroupName = "Операция")]
        [DisplayName("Номер акта")]
        [MessagePack.Key(1)]
        public int? НомерАкта
        {
            get;
            set;
        }

        [Display(Order = 2, GroupName = "Операция")]
        [DisplayName("Ф.И.О. монтёра")]
        [MessagePack.Key(2)]
        public string Фамилия
        {
            get;
            set;
        }

        [Display(Order = 3, GroupName = "Абонент")]
        [DisplayName("Лицевой счёт абонента")]
        [PersonalAccountDataFormat]
        [MessagePack.Key(3)]
        public ulong Лицевой
        {
            get;
            set;
        }

        [Display(Order = 4, GroupName = "Абонент")]
        [DisplayName("Ф.И.О.")]
        [MessagePack.Key(4)]
        public string Фио
        {
            get;
            set;
        }

        [Display(Order = 5, GroupName = "Абонент")]
        [DisplayName("Населённый пункт")]
        [MessagePack.IgnoreMember]
        public string НаселённыйПункт => this.Адрес?.НаселённыйПункт;

        [Display(Order = 6, GroupName = "Абонент")]
        [DisplayName("Адрес")]
        [MessagePack.Key(6)]
        public BaseAddress Адрес
        {
            get;
            set;
        }

        [Display(Order = 7, GroupName = "Снятый счётчик")]
        [DisplayName("Тип снятого счётчика")]
        [MessagePack.Key(7)]
        public string ТипСнятогоСчетчика
        {
            get;
            set;
        }

        [Display(Order = 8, GroupName = "Снятый счётчик")]
        [DisplayName("Номер снятого счётчика")]
        [MessagePack.Key(8)]
        public string НомерСнятогоСчетчика
        {
            get;
            set;
        }

        [Display(Order = 9, GroupName = "Снятый счётчик")]
        [DisplayName("Показание снятого")]
        [MessagePack.Key(9)]
        public int ПоказаниеСнятого
        {
            get;
            set;
        }

        [Display(Order = 10, GroupName = "Снятый счётчик")]
        [DisplayName("Квартал поверки снятого")]
        [MessagePack.Key(10)]
        public byte КварталПоверкиСнятого
        {
            get;
            set;
        }

        [Display(Order = 11, GroupName = "Снятый счётчик")]
        [DisplayName("Год поверки снятого")]
        [MessagePack.Key(11)]
        public byte ГодПоверкиСнятого
        {
            get;
            set;
        }

        [Display(Order = 12, GroupName = "Снятый счётчик")]
        [DisplayName("Дата установки снятого")]
        [MessagePack.Key(12)]
        public DateOnly? ДатаУстановкиСнятого
        {
            get;
            set;
        }

        [Display(Order = 13, GroupName = "Снятый счётчик")]
        [DisplayName("Год выпуска снятого")]
        [MessagePack.Key(13)]
        public int? ГодВыпускаСнятого
        {
            get;
            set;
        }

        [Display(Order = 14, GroupName = "Установленный счётчик")]
        [DisplayName("Тип установленного счётчика")]
        [MessagePack.Key(14)]
        public string ТипУстановленногоСчетчика
        {
            get;
            set;
        }

        [Display(Order = 15, GroupName = "Установленный счётчик")]
        [DisplayName("Номер установленного счётчика")]
        [MessagePack.Key(15)]
        public string НомерУстановленногоСчетчика
        {
            get;
            set;
        }

        [Display(Order = 16, GroupName = "Установленный счётчик")]
        [DisplayName("Показание установленного")]
        [MessagePack.Key(16)]
        public int ПоказаниеУстановленного
        {
            get;
            set;
        }

        [Display(Order = 17, GroupName = "Установленный счётчик")]
        [DisplayName("Год выпуска установленного")]
        [MessagePack.Key(17)]
        public int? ГодВыпускаУстановленного
        {
            get;
            set;
        }

        [Display(Order = 18, GroupName = "Операция")]
        [MessagePack.Key(18)]
        public string Причина
        {
            get;
            set;
        }

        [Display(Order = 19)]
        [MessagePack.Key(19)]
        public bool ЭтоЭлектронный
        {
            get;
            set;
        }

        [DisplayName("Подстанция")]
        [Display(Order = 20, GroupName = "Привязка")]
        [MessagePack.Key(20)]
        public string Подстанция { get; set; }

        [DisplayName("Фидер 10 кВ")]
        [Display(Order = 21, GroupName = "Привязка")]
        [MessagePack.Key(21)]
        public string Фидер10 { get; set; }

        [DisplayName("Номер ТП")]
        [Display(Order = 22, GroupName = "Привязка")]
        [MessagePack.Key(22)]
        public string НомерТП { get; set; }

        [DisplayName("Наименование ТП")]
        [Display(Order = 23, GroupName = "Привязка")]
        [MessagePack.Key(23)]
        public string НаименованиеТП { get; set; }

        [DisplayName("Фидер 0,4 кВ")]
        [Display(Order = 24, GroupName = "Привязка")]
        [MessagePack.Key(24)]
        public int? Фидер04 { get; set; }

        [DisplayName("№ опоры")]
        [Display(Order = 25, GroupName = "Привязка")]
        [MessagePack.Key(25)]
        public string Опора { get; set; }
    }
}