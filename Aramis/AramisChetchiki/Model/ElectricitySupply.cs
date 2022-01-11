namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    [MessagePack.MessagePackObject]
    public class ElectricitySupply : IModelWithPersonalId
    {
        [Display(GroupName = "")]
        [DisplayName("Период")]
        [MessagePack.Key(0)]
        public DateOnly? Период
        {
            get;
            set;
        }

        [Display(GroupName = "")]
        [DisplayName("Лицевой счёт абонента")]
        [MessagePack.Key(1)]
        public ulong Лицевой
        {
            get;
            set;
        }

        [Display(GroupName = "")]
        [DisplayName("Тип населённого пункта")]
        [MessagePack.Key(2)]
        public string Тип_населённого_пункта
        {
            get;
            set;
        }

        [Display(GroupName = "")]
        [DisplayName("Полезный отпуск")]
        [MessagePack.Key(3)]
        public int Полезный_отпуск
        {
            get;
            set;
        }

        [Display(GroupName = "")]
        [DisplayName("Полезный отпуск")]
        [MessagePack.Key(4)]
        public int Задолженность
        {
            get;
            set;
        }

        [Display(GroupName = "")]
        [DisplayName("Дата оплаты")]
        [MessagePack.Key(5)]
        public DateOnly ДатаОплаты
        {
            get;
            set;
        }

        [Display(GroupName = "")]
        [DisplayName("Опл показания")]
        [MessagePack.Key(6)]
        public decimal ОплаченныеПоказания
        {
            get;
            set;
        }

        [DisplayName("Адрес")]
        [Display(GroupName = "")]
        [MessagePack.Key(7)]
        public string Адрес { get; set; }

        [MessagePack.IgnoreMember]
        private string населённый_пункт = string.Empty;

        [DisplayName("Населённый пункт")]
        [SummaryInfo]
        [Display(GroupName = "")]
        [MessagePack.Key(8)]
        public string Населённый_пункт
        {
            get => string.IsNullOrWhiteSpace(this.населённый_пункт) ? (string.IsNullOrEmpty(this.Адрес) ? string.Empty : this.Адрес.Split(',')[0]) : this.населённый_пункт;
            set => this.населённый_пункт = value;
        }

        [DisplayName("Подстанция")]
        [SummaryInfo]
        [Display(GroupName = "")]
        [MessagePack.Key(9)]
        public string Подстанция { get; set; }

        [DisplayName("Фидер 10 кВ")]
        [SummaryInfo]
        [Display(GroupName = "")]
        [MessagePack.Key(10)]
        public string Фидер10 { get; set; }

        [DisplayName("Номер ТП")]
        [SummaryInfo]
        [Display(GroupName = "")]
        [MessagePack.Key(11)]
        public int НомерТП { get; set; }

        [DisplayName("Тип ТП")]
        [SummaryInfo]
        [Display(GroupName = "")]
        [MessagePack.Key(12)]
        public string ТипТП { get; set; }

        [DisplayName("Наименование ТП")]
        [SummaryInfo]
        [Display(GroupName = "")]
        [MessagePack.Key(13)]
        public string НаименованиеТП { get; set; }

        [DisplayName("Фидер 0,4 кВ")]
        [Display(GroupName = "")]
        [MessagePack.Key(14)]
        public int? Фидер04 { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Период")]
        [Display(GroupName = "")]
        public string Period => this.Период.HasValue ? this.Период.Value.ToString("MMM.yyyy", AppSettings.CurrentCulture) : string.Empty;
    }
}