namespace TMP.WORK.AramisChetchiki.DbModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using TMP.Shared.DataConverters;
    using TMP.Shared.DataFormatters;

    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    [System.Diagnostics.DebuggerDisplay("ID={Лицевой} :: Date={ДатаЗамены} :: Prev={ПоказаниеСнятого} :: Next={LastReading}")]
    public class ChangeOfMeter : IModelWithPersonalId, IModelWithMeterLastReading
    {
        [Display(Order = 0, GroupName = "Операция")]
        [DisplayName("Дата замены")]
        [DateTimeDataFormat]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateOnlyConverter))]
        [MessagePack.MessagePackFormatter(typeof(DateOnlyFormatter))]
        public DateOnly ДатаЗамены { get; set; }

        [Display(Order = 1, GroupName = "Операция")]
        [DisplayName("Номер акта")]
        public uint? НомерАкта { get; set; }

        [Display(Order = 2, GroupName = "Операция")]
        [DisplayName("Ф.И.О. монтёра")]
        public string Фамилия { get; set; }

        [Display(Order = 3, GroupName = "Абонент")]
        [DisplayName("Лицевой счёт абонента")]
        [PersonalAccountDataFormatAttribute]
        public ulong Лицевой { get; set; }

        [Display(Order = 4, GroupName = "Абонент")]
        [DisplayName("Ф.И.О.")]
        public string Фио { get; set; }

        [Display(Order = 5, GroupName = "Абонент")]
        [DisplayName("Населённый пункт")]
        [MessagePack.IgnoreMember]
        public string НаселённыйПункт => this.Адрес?.City;

        [Display(Order = 6, GroupName = "Абонент")]
        [DisplayName("Адрес")]
        public Address Адрес { get; set; }

        [Display(Order = 7, GroupName = "Снятый счётчик")]
        [DisplayName("Тип снятого счётчика")]
        public string ТипСнятогоСчетчика { get; set; }

        [Display(Order = 8, GroupName = "Снятый счётчик")]
        [DisplayName("Номер снятого счётчика")]
        public string НомерСнятогоСчетчика { get; set; }

        [Display(Order = 9, GroupName = "Снятый счётчик")]
        [DisplayName("Показание снятого")]
        public uint ПоказаниеСнятого { get; set; }

        [Display(Order = 10, GroupName = "Снятый счётчик")]
        [DisplayName("Квартал поверки снятого")]
        public byte КварталПоверкиСнятого { get; set; }

        [Display(Order = 11, GroupName = "Снятый счётчик")]
        [DisplayName("Год поверки снятого")]
        public byte ГодПоверкиСнятого { get; set; }

        [Display(Order = 12, GroupName = "Снятый счётчик")]
        [DisplayName("Дата установки снятого")]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateOnlyConverterNullable))]
        [MessagePack.MessagePackFormatter(typeof(DateOnlyFormatterNullable))]
        public DateOnly? ДатаУстановкиСнятого { get; set; }

        [Display(Order = 13, GroupName = "Снятый счётчик")]
        [DisplayName("Год выпуска снятого")]
        public ushort? ГодВыпускаСнятого { get; set; }

        [Display(Order = 14, GroupName = "Установленный счётчик")]
        [DisplayName("Тип установленного счётчика")]
        public string ТипУстановленногоСчетчика { get; set; }

        [Display(Order = 15, GroupName = "Установленный счётчик")]
        [DisplayName("Номер установленного счётчика")]
        public string НомерУстановленногоСчетчика { get; set; }

        [Display(Order = 16, GroupName = "Установленный счётчик")]
        [DisplayName("Показание установленного")]
        public uint LastReading { get; set; }

        [Display(Order = 17, GroupName = "Установленный счётчик")]
        [DisplayName("Год выпуска установленного")]
        public ushort? ГодВыпускаУстановленного { get; set; }

        [Display(Order = 18, GroupName = "Операция")]
        public string Причина { get; set; }

        [Display(Order = 19)]
        public bool СнятЭлектронный { get; set; }

        [Display(Order = 20)]
        public bool УстановленЭлектронный { get; set; }

        [DisplayName("Подстанция")]
        [Display(Order = 21, GroupName = "Привязка")]
        public string Подстанция { get; set; }

        [DisplayName("Фидер 10 кВ")]
        [Display(Order = 22, GroupName = "Привязка")]
        public string Фидер10 { get; set; }

        [DisplayName("Номер ТП")]
        [Display(Order = 23, GroupName = "Привязка")]
        public string НомерТП { get; set; }

        [DisplayName("Наименование ТП")]
        [Display(Order = 24, GroupName = "Привязка")]
        public string НаименованиеТП { get; set; }

        [DisplayName("Фидер 0,4 кВ")]
        [Display(Order = 25, GroupName = "Привязка")]
        public byte? Фидер04 { get; set; }

        [DisplayName("№ опоры")]
        [Display(Order = 26, GroupName = "Привязка")]
        public string Опора { get; set; }

        [DisplayName("Разрядность снятого счётчика")]
        [Display(Order = 27, GroupName = "Снятый счётчик")]
        public byte РазрядностьСнятого { get; set; }

        [DisplayName("Разрядность установленного счётчика")]
        [Display(Order = 28, GroupName = "Установленный счётчик")]
        public byte РазрядностьУстановленного { get; set; }
    }
}