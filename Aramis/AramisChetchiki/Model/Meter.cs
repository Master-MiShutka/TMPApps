namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.Serialization;
    using TMP.Extensions;
    using TMP.Shared;

    [KnownType(typeof(Address))]
    [KnownType(typeof(TransformerSubstation))]
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class Meter : IModelWithPersonalId
    {
        private const string UNKNOWN_STR = "#Н/Д";

        #region Контактные данные

        [DisplayName("Фамилия")]
        [Display(GroupName = "Контактные данные")]
        public string Фамилия { get; set; }

        [DisplayName("Имя")]
        [Display(GroupName = "Контактные данные")]
        public string Имя { get; set; }

        [DisplayName("Отчество")]
        [Display(GroupName = "Контактные данные")]
        public string Отчество { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Ф.И.О. полностью")]
        [Display(GroupName = "Контактные данные")]
        public string ФиоПолностью => $"{this.Фамилия} {this.Имя} {this.Отчество}";

        [MessagePack.IgnoreMember]
        [DisplayName("Ф.И.О. сокращенные")]
        [Display(GroupName = "Контактные данные")]
        public string ФиоСокращ
        {
            get
            {
                string f = string.IsNullOrWhiteSpace(this.Фамилия) ? string.Empty : this.Фамилия;
                string n = string.IsNullOrWhiteSpace(this.Имя) ? string.Empty : " " + this.Имя[0] + ".";
                string o = string.IsNullOrWhiteSpace(this.Отчество) ? string.Empty : this.Отчество[0] + ".";
                return $"{f}{n}{o}";
            }
        }

        [DisplayName("SMS")]
        [Display(GroupName = "Контактные данные")]
        public string SMS { get; set; }

        [DisplayName("Телефоны")]
        [Display(GroupName = "Контактные данные")]
        public string Телефоны { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Заполнено ли поле SMS")]
        [SummaryInfo]
        [Display(GroupName = "Контактные данные")]
        public bool HasSMS => string.IsNullOrWhiteSpace(this.SMS) == false;

        #endregion

        #region Адрес

        [DisplayName("Адрес")]
        [Display(GroupName = "Адрес")]
        public Address Адрес { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Тип населённой местности")]
        [Display(GroupName = "Адрес")]
        public string ТипНаселённойМестности => string.IsNullOrWhiteSpace(this.Адрес?.ТипНаселённогоПункта)
            ? UNKNOWN_STR
            : (this.Адрес.ТипНаселённогоПункта.StartsWith("г.", AppSettings.StringComparisonMethod) ? "город" : "село");

        [MessagePack.IgnoreMember]
        [DisplayName("Тип населённого пункта")]
        [SummaryInfo]
        [Display(GroupName = "Адрес")]
        public string ТипНаселённогоПункта => this.Адрес?.ТипНаселённогоПункта;

        [MessagePack.IgnoreMember]
        [SummaryInfo]
        [DisplayName("Населённый пункт")]
        [Display(GroupName = "Адрес")]
        public string НаселённыйПункт => this.Адрес?.НаселённыйПункт;

        [MessagePack.IgnoreMember]
        [DisplayName("Улица")]
        [Display(GroupName = "Адрес")]
        public string Улица => this.Адрес?.Улица;

        [MessagePack.IgnoreMember]
        [DisplayName("Улица с домом")]
        [Display(GroupName = "Адрес")]
        public string УлицаСДомом => this.Адрес?.УлицаСДомом;

        [MessagePack.IgnoreMember]
        [DisplayName("Улица с домом и квартирой")]
        [Display(GroupName = "Адрес")]
        public string УлицаСДомомИКв => this.Адрес?.УлицаСДомомИКв;

        [MessagePack.IgnoreMember]
        [DisplayName("Дом")]
        [Display(GroupName = "Адрес")]
        [TextDataFormat]
        public string Дом => this.Адрес?.Дом;

        [MessagePack.IgnoreMember]
        [DisplayName("Квартира")]
        [Display(GroupName = "Адрес")]
        [TextDataFormat]
        public string Квартира => this.Адрес?.Квартира;

        [MessagePack.IgnoreMember]
        [SummaryInfo]
        [DisplayName("Сельский совет")]
        [Display(GroupName = "Адрес")]
        public string СельскийСовет => this.Адрес?.СельскийСовет;

        [MessagePack.IgnoreMember]
        [DisplayName("МЖД")]
        [Display(GroupName = "Адрес")]
        public bool ЭтоМжд => this.Адрес != null ? this.Адрес.ЭтоМжд : false;

        [MessagePack.IgnoreMember]
        [DisplayName("Населенный пункт и улица с номером дома")]
        [Display(GroupName = "Адрес")]
        public string НаселённыйПунктИУлицаСНомеромДома => this.Адрес?.CityAndStreetWithHouse;

        #endregion

        #region Абонент

        [DisplayName("Категория")]
        [SummaryInfo]
        [Display(GroupName = "Абонент")]
        public string Категория { get; set; }

        [DisplayName("Коментарий")]
        [Display(GroupName = "Абонент")]
        public string Коментарий { get; set; }

        [DisplayName("Дата уведомления")]
        [Display(GroupName = "Абонент")]
        [DateTimeDataFormat]
        public DateOnly? ДатаУведомления { get; set; }

        [DisplayName("Дата отключения")]
        [Display(GroupName = "Абонент")]
        [DateTimeDataFormat]
        public DateOnly? ДатаОтключения { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Год отключения")]
        [Display(GroupName = "Абонент")]
        [SummaryInfo]
        public ushort? ГодОтключения => this.Отключён ? (ushort)this.ДатаОтключения.Value.Year : null;

        [MessagePack.IgnoreMember]
        [Display(GroupName = "Абонент")]
        [DisplayName("Отключён")]
        [SummaryInfo]
        public bool Отключён => this.ДатаОтключения.HasValue ? (this.ДатаОтключения.Value == default ? false : true) : false;

        [DisplayName("Удалён из базы")]
        [Display(GroupName = "Абонент")]
        [SummaryInfo]
        public bool Удалён { get; set; } = false;

        [DisplayName("Дата удаления из базы")]
        [Display(GroupName = "Абонент")]

        public DateOnly ДатаУдаления { get; set; }

        [DisplayName("Прописано человек")]
        [Display(GroupName = "Абонент")]
        [SummaryInfo]
        public byte? КолвоЧеловек { get; set; }

        #endregion

        #region Счётчик

        [DisplayName("Шифр тарифа")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        public string ШифрТарифа { get; set; }

        [DisplayName("Тип счётчика")]
        [Display(GroupName = "Счётчик")]
        [SummaryInfo]
        public string ТипСчетчика { get; set; }

        [DisplayName("Ампераж счётчика")]
        [Display(GroupName = "Счётчик")]
        [TextDataFormat]
        public string Ампераж { get; set; }

        [DisplayName("Принцип действия счётчика")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        public string Принцип { get; set; }

        [DisplayName("Количество фаз счётчика")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        public byte Фаз { get; set; }

        [DisplayName("Номер счётчика")]
        [Display(GroupName = "Счётчик")]
        [TextDataFormat]
        public string НомерСчетчика { get; set; }

        [DisplayName("Номера пломб")]
        [Display(GroupName = "Счётчик")]
        [TextDataFormat]
        public string НомераПломб { get; set; }

        [DisplayName("Мощность электроустановки, кВт")]
        [Display(GroupName = "Счётчик")]
        [SummaryInfo]
        public float? Мощность { get; set; }

        [DisplayName("Год выпуска счётчика")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        public ushort ГодВыпуска { get; set; }

        [MessagePack.IgnoreMember]
        private DateOnly? датаУстановки;

        [DisplayName("Дата установки учёта")]
        [Display(GroupName = "Счётчик")]
        [DateTimeDataFormat]

        public DateOnly ДатаУстановки
        {
            get => this.датаУстановки == null || this.датаУстановки == default ? default : this.датаУстановки.Value;
            set => this.датаУстановки = value;
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Год установки учёта")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        public ushort ГодУстановки => (ushort)this.ДатаУстановки.Year;

        [DisplayName("Показание при установке")]
        [Display(GroupName = "Счётчик")]
        public uint? ПоказаниеПриУстановке { get; set; }

        [DisplayName("Расчётное показание")]
        [Display(GroupName = "Счётчик")]
        public int? РасчПоказание { get; set; }

        [DisplayName("Последнее показание при обходе")]
        [Display(GroupName = "Счётчик")]
        public uint? ПослПоказаниеОбхода { get; set; }

        [MessagePack.IgnoreMember]
        [Display(GroupName = "Счётчик")]
        [DisplayName("Группа счётчика для отчётов")]
        [SummaryInfo]
        public string ГруппаСчётчикаДляОтчётов
        {
            get
            {
                string result = this.Фаз == 1
                    ? this.Принцип == "индукционный" ? "1ф инд." : this.Принцип == "электронный" ? "1ф эл." : "1ф неизв."
                    : this.Фаз == 3
                        ? this.Принцип == "индукционный" ? "3ф инд." : this.Принцип == "электронный" ? "3ф эл." : "3ф неизв."
                        : "неверное кол-во фаз";

                return result;
            }
        }

        [MessagePack.IgnoreMember]
        [Display(GroupName = "Счётчик")]
        [DisplayName("На балансе абонента")]
        [SummaryInfo]
        public bool НаБалансеАбонента => !(this.СчетчикНаБалансеГродноэнерго == true || this.ПринадлежностьРуп == true);

        [MessagePack.IgnoreMember]
        [Display(GroupName = "Счётчик")]
        [DisplayName("Электронный")]
        [SummaryInfo]
        public bool Электронный => this.Принцип == "электронный";

        [DisplayName("Количество используемых тарифов")]
        [Display(GroupName = "Счётчик")]
        public byte Тарифов { get; set; }

        [DisplayName("Количество используемых знаков")]
        [Display(GroupName = "Счётчик")]
        public byte Разрядность { get; set; }

        #endregion

        #region Счётчик-признаки

        [DisplayName("Расположение")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик-признаки")]
        public string Расположение { get; set; }

        [DisplayName("Место установки учёта")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик-признаки")]
        public string МестоУстановки { get; set; }

        [DisplayName("Использование")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик-признаки")]
        public string Использование { get; set; }

        [DisplayName("Наличие АСКУЭ")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик-признаки")]
        public bool Аскуэ { get; set; }

        #endregion

        #region Показания

        [DisplayName("Показание при отключении")]
        [Display(GroupName = "Счётчик")]
        public uint? ПоказаниеПриОтключении { get; set; }

        [DisplayName("Последнее оплаченное показание")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        public uint? ПоследнееОплаченноеПоказание { get; set; }

        [DisplayName("Предыдуще оплаченное показание")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        public uint? ПредыдущеОплаченноеПоказание { get; set; }

        #endregion

        #region Оплата

        [MessagePack.IgnoreMember]
        [Display(GroupName = "Оплата")]
        [DisplayName("Имеется переплата")]
        [SummaryInfo]
        public bool ЕстьПереплата => this.ДолгРуб.HasValue && this.ДолгРуб.Value < 0;

        [MessagePack.IgnoreMember]
        [Display(GroupName = "Оплата")]
        [DisplayName("Имеется долг")]
        [SummaryInfo]
        public bool ЕстьДолг => this.ДолгРуб.HasValue && this.ДолгРуб.Value > 0;

        [DisplayName("Долг, руб")]
        [Display(GroupName = "Оплата")]
        public float? ДолгРуб { get; set; }

        [DisplayName("ErrSumN")]
        [Display(GroupName = "Оплата")]
        public float? ErrSumN { get; set; }

        [DisplayName("ErrSumV")]
        [Display(GroupName = "Оплата")]
        public float? ErrSumV { get; set; }

        [DisplayName("Сумма оплаты")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        public float? СуммаОплаты { get; set; }

        [DisplayName("Период последней оплаты")]
        [Display(GroupName = "Оплата")]
        [DateTimeDataFormat]

        public DateOnly? ПериодПослОплаты { get; set; }

        [DisplayName("Среднее")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        public float? Среднее { get; set; }

        [DisplayName("Месяц")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        public int? Месяц { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Долг")]
        [Display(GroupName = "Оплата")]
        public double? Долг
        {
            get
            {
                if (this.Месяц.HasValue == false || this.Месяц == 0 || this.Среднее.HasValue == false)
                {
                    return 0;
                }

                int year = this.ПериодПослОплаты.HasValue ? this.ПериодПослОплаты.Value.Year : 0;
                int month = this.ПериодПослОплаты.HasValue ? this.ПериодПослОплаты.Value.Month : 0;
                DateTime now = DateTime.Now;

                int months = now.Year > year ? 12 - month + now.Month + ((now.Year - year - 1) * 12) : now.Month - month;

                float среднее = this.Среднее.Value;
                int месяц = this.Месяц.Value;

                return Math.Round(1d * среднее / месяц * (months - 1), 2);
            }
        }

        [MessagePack.IgnoreMember]
        private DateOnly? датаОплаты;

        [DisplayName("Дата оплаты")]
        [Display(GroupName = "Оплата")]
        [DateTimeDataFormat]
        public DateOnly ДатаОплаты
        {
            get => this.датаОплаты == null || this.датаОплаты == default ? default : this.датаОплаты.Value;
            set => this.датаОплаты = value;
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Год оплаты")]
        [SummaryInfo]
        [Display(GroupName = "Оплата")]
        public ushort ГодОплаты => (ushort)this.ДатаОплаты.Year;

        [DisplayName("Задолженник")]
        [Display(GroupName = "Оплата")]
        [SummaryInfo]
        public bool Задолженник { get; set; }

        [DisplayName("Задолженность, кВт∙ч")]
        [Display(GroupName = "Оплата")]
        [MessagePack.IgnoreMember]
        public uint Задолженность
        {
            get
            {
                if (this.ДатаОбхода == this.ДатаУстановки)
                {
                    return 0;
                }

                if (this.ДатаОбхода > this.ДатаОплаты)
                {
                    long value = (this.ПослПоказаниеОбхода ?? 0) - (this.ПоследнееОплаченноеПоказание ?? 0);
                    return value > 0 ? (uint)value : 0;
                }
                else
                {
                    long value = (this.ПослПоказаниеОбхода ?? 0) - (this.ПоследнееОплаченноеПоказание ?? 0);
                    if (value > 0)
                    {
                        ;
                    }
                }

                return 0;
            }
        }

        [DisplayName("Нет оплаты два и более расч. периода")]
        [Display(GroupName = "Оплата")]
        [SummaryInfo]
        [MessagePack.IgnoreMember]
        public bool НетОплатыДваПериода
        {
            get
            {
                if (this.ДатаОплаты == default)
                {
                    return true;
                }

                DateTime paymentDate = this.ДатаОплаты.ToDateTime(TimeOnly.MinValue);

                if ((DateTime.Now - paymentDate).Days >= 62)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [DisplayName("Среднемесячный расход по оплате, кВт∙ч")]
        [Display(GroupName = "Оплата")]
        [SummaryInfo]
        public uint? СреднеМесячныйРасходПоОплате { get; set; }

        [DisplayName("Среднемесячный расход по контрольным показаниям, кВт∙ч")]
        [Display(GroupName = "Оплата")]
        [SummaryInfo]
        public uint? СреднеМесячныйРасходПоКонтрольнымПоказаниям { get; set; }

        #endregion

        #region Обход

        [MessagePack.IgnoreMember]
        private DateOnly? датаОбхода;

        [DisplayName("Дата обхода")]
        [Display(GroupName = "Обход")]
        [DateTimeDataFormat]

        public DateOnly ДатаОбхода
        {
            get => this.датаОбхода == null || this.датаОбхода == default ? DateOnly.FromDateTime(new DateTime(1900, 1, 1)) : this.датаОбхода.Value;
            set => this.датаОбхода = value;
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Год обхода")]
        [SummaryInfo]
        [Display(GroupName = "Обход")]
        public ushort ГодОбхода => (ushort)this.датаОбхода?.Year;

        [MessagePack.IgnoreMember]
        [DisplayName("Обхода не было Х месяцев")]
        [Display(GroupName = "Обход")]
        public ushort ОбходаНеБылоМесяцев
        {
            get
            {
                DateOnly now = DateOnly.FromDateTime(DateTime.Now);
                DateOnly обход = this.датаОбхода ?? default;
                return (ushort)(((now.Year - обход.Year) * 12) + now.Month - обход.Month);
            }
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Обхода не было")]
        [SummaryInfo]
        [Display(GroupName = "Обход")]
        public string ОбходаНеБыло => this.ОбходаНеБылоМесяцев switch
        {
            0 => "был в этом месяце",
            > 0 and <= 3 => "был недавно",
            > 3 and <= 6 => "более 3 месяцев",
            > 6 and <= 12 => "более полугода",
            > 12 and <= 36 => "более 1 года",
            > 36 and < 60 => "более 3 лет",
            >= 60 => "очень давно",
        };

        [DisplayName("Контролёр")]
        [SummaryInfo]
        [Display(GroupName = "Обход")]
        [DisplayFormat(DataFormatString = "{0}")]
        public string Контролёр { get; set; }

        #endregion

        #region Поверка

        [MessagePack.IgnoreMember]
        [DisplayName("Поверен")]
        [SummaryInfo]
        [Display(GroupName = "Поверка")]
        public bool Поверен
        {
            get
            {
                int year = this.ГодПоверки + this.ПериодПоверки;
                DateOnly now = ДатаСравненияПоверки;

                if (year < now.Year)
                {
                    return false;
                }

                if (year == now.Year)
                {
                    if (this.КварталПоверки < now.GetQuarter())
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [DisplayName("Квартал поверки счётчика")]
        [Display(GroupName = "Поверка")]
        public byte КварталПоверки { get; set; }

        [MessagePack.IgnoreMember]
        private ushort годПоверки;

        [DisplayName("Год поверки счётчика")]
        [SummaryInfo]
        [Display(GroupName = "Поверка")]
        public ushort ГодПоверки
        {
            get => this.годПоверки;
            set => this.годПоверки = FixYear(value);
        }

        private static ushort FixYear(ushort value)
        {
            return (ushort)(value < 100 ? value > 50 ? 1900 + value : 2000 + value : value);
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Поверка")]
        [Display(GroupName = "Поверка")]
        public string Поверка => this.КварталПоверки + " кв " + (this.ГодПоверки < 10 ? "0" + this.ГодПоверки.ToString(AppSettings.CurrentCulture) : this.ГодПоверки.ToString(AppSettings.CurrentCulture));

        [DisplayName("Период поверки")]
        [Display(GroupName = "Поверка")]
        public byte ПериодПоверки { get; set; }

        [MessagePack.IgnoreMember]
        private static DateOnly дата_сравнения_поверки = DateOnly.FromDateTime(DateTime.Now);

        [MessagePack.IgnoreMember]
        public static DateOnly ДатаСравненияПоверки
        {
            get => дата_сравнения_поверки;
            set => дата_сравнения_поверки = value;
        }

        [MessagePack.IgnoreMember]
        [Display(GroupName = "Поверка")]
        [DisplayName("Год поверки для отчётов")]
        [SummaryInfo]
        public string ГодПоверкиДляОтчётов
        {
            get
            {
                int currentYear = DateTime.Now.Year;
                string result = (currentYear - this.годПоверки) switch
                {
                    0 => currentYear.ToString(AppSettings.CurrentCulture),
                    1 => (currentYear - 1).ToString(AppSettings.CurrentCulture),
                    2 => (currentYear - 2).ToString(AppSettings.CurrentCulture),
                    3 => (currentYear - 3).ToString(AppSettings.CurrentCulture),
                    4 => (currentYear - 4).ToString(AppSettings.CurrentCulture),
                    5 => (currentYear - 5).ToString(AppSettings.CurrentCulture),
                    6 => (currentYear - 6).ToString(AppSettings.CurrentCulture),
                    7 => (currentYear - 7).ToString(AppSettings.CurrentCulture),
                    8 => (currentYear - 8).ToString(AppSettings.CurrentCulture),
                    _ => "<" + (currentYear - 8).ToString(AppSettings.CurrentCulture),
                };
                return result;
            }
        }

        #endregion

        #region Привязка
        [DisplayName("Подстанция")]
        [SummaryInfo]
        [Display(GroupName = "Привязка")]
        public string Подстанция { get; set; }

        [DisplayName("Фидер 10 кВ")]
        [SummaryInfo]
        [Display(GroupName = "Привязка")]
        public string Фидер10 { get; set; }

        [DisplayName("ТП")]
        [SummaryInfo]
        [Display(GroupName = "Привязка")]
        public TransformerSubstation ТП { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Тип ТП")]
        [Display(GroupName = "Привязка")]
        public string ТПType => this.ТП?.Type;

        [MessagePack.IgnoreMember]
        [DisplayName("Номер ТП")]
        [Display(GroupName = "Привязка")]
        public ushort? ТПNumber => (this.ТП != null && this.ТП.IsEmpty) ? null : (ushort)this.ТП?.Number;

        [MessagePack.IgnoreMember]
        [DisplayName("Название ТП")]
        [Display(GroupName = "Привязка")]
        public string ТПName => this.ТП?.Name;

        [MessagePack.IgnoreMember]
        [DisplayName("Задан ли тип и номер ТП")]
        [Display(GroupName = "Привязка")]
        public bool ТПIsEmpty => this.ТП != null ? this.ТП.IsEmpty : true;

        [DisplayName("Фидер 0,4 кВ")]
        [Display(GroupName = "Привязка")]
        [TextDataFormat]
        public byte? Фидер04 { get; set; }

        [DisplayName("№ опоры")]
        [Display(GroupName = "Привязка")]
        [TextDataFormat]
        public string Опора { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Привязка")]
        [Display(GroupName = "Привязка")]
        public string Привязка => string.Join(", ", this.Подстанция, this.Фидер10, this.ТП, this.Фидер04, this.Опора);

        [MessagePack.IgnoreMember]
        [DisplayName("Привязка2")]
        [Display(GroupName = "Привязка")]
        public string Привязка2 => string.Join(", ", this.ТП, this.Фидер04, this.Опора);
        #endregion

        #region Признаки

        [DisplayName("Признаки")]
        [Display(GroupName = "Признаки")]
        public MeterSigns Signs { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Горячее водоснабжение")]
        [Display(GroupName = "Признаки")]
        public bool ГорячееВодоснабжение => this.Signs.HasFlag(MeterSigns.ГорячееВодоснабжение);

        [MessagePack.IgnoreMember]
        [SummaryInfo]
        [DisplayName("НеблагополучнаяСемья")]
        [Display(GroupName = "Признаки")]
        public bool НеблагополучнаяСемья => this.Signs.HasFlag(MeterSigns.НеблагополучнаяСемья);

        [MessagePack.IgnoreMember]
        [SummaryInfo]
        [DisplayName("МногодетнаяСемья")]
        [Display(GroupName = "Признаки")]
        public bool МногодетнаяСемья => this.Signs.HasFlag(MeterSigns.МногодетнаяСемья);

        [MessagePack.IgnoreMember]
        [DisplayName("ДомСемейногоТипа")]
        [Display(GroupName = "Признаки")]
        public bool ДомСемейногоТипа => this.Signs.HasFlag(MeterSigns.ДомСемейногоТипа);

        [MessagePack.IgnoreMember]
        [DisplayName("ИмеющиеПриродныйГаз")]
        [Display(GroupName = "Признаки")]
        public bool ИмеющиеПриродныйГаз => this.Signs.HasFlag(MeterSigns.ИмеющиеПриродныйГаз);

        [MessagePack.IgnoreMember]
        [DisplayName("РодителиИнвалиды")]
        [Display(GroupName = "Признаки")]
        public bool РодителиИнвалиды => this.Signs.HasFlag(MeterSigns.РодителиИнвалиды);

        [MessagePack.IgnoreMember]
        [DisplayName("НеполнаяСемья")]
        [Display(GroupName = "Признаки")]
        public bool НеполнаяСемья => this.Signs.HasFlag(MeterSigns.НеполнаяСемья);

        [MessagePack.IgnoreMember]
        [DisplayName("РебенокИнвалид")]
        [Display(GroupName = "Признаки")]
        public bool РебенокИнвалид => this.Signs.HasFlag(MeterSigns.РебенокИнвалид);

        [MessagePack.IgnoreMember]
        [DisplayName("ОпекунскиеСемьи")]
        [Display(GroupName = "Признаки")]
        public bool ОпекунскиеСемьи => this.Signs.HasFlag(MeterSigns.ОпекунскиеСемьи);

        [MessagePack.IgnoreMember]
        [DisplayName("Нерезиденты")]
        [Display(GroupName = "Признаки")]
        public bool Нерезиденты => this.Signs.HasFlag(MeterSigns.Нерезиденты);

        [MessagePack.IgnoreMember]
        [DisplayName("СлужебныйДом")]
        [Display(GroupName = "Признаки")]
        public bool СлужебныйДом => this.Signs.HasFlag(MeterSigns.СлужебныйДом);

        [MessagePack.IgnoreMember]
        [DisplayName("На балансе РУП 'ГродноЭнерго'")]
        [Display(GroupName = "Признаки")]
        public bool СчетчикНаБалансеГродноэнерго => this.Signs.HasFlag(MeterSigns.СчетчикНаБалансеГродноэнерго);

        [MessagePack.IgnoreMember]
        [DisplayName("ВыносноеАСКУЭ")]
        [Display(GroupName = "Признаки")]
        public bool ВыносноеАСКУЭ => this.Signs.HasFlag(MeterSigns.ВыносноеАСКУЭ);

        [SummaryInfo]
        [DisplayName("Заключен ли договор")]
        [Display(GroupName = "Признаки")]
        public bool Договор { get; set; }

        [DisplayName("Дата договора")]
        [Display(GroupName = "Признаки")]

        public DateOnly? ДатаДоговора { get; set; }

        [SummaryInfo]
        [DisplayName("Льгота")]
        [Display(GroupName = "Признаки")]
        public ushort? Льгота { get; set; }

        [DisplayName("Принадлежит РУП 'ГродноЭнерго'")]
        [Display(GroupName = "Признаки")]
        public bool ПринадлежностьРуп { get; set; }

        [DisplayName("Работник ГПО БелЭнерго")]
        [Display(GroupName = "Признаки")]
        [SummaryInfo]
        public bool РаботникБелЭнерго { get; set; } = false;

        #endregion

        #region Счёт

        [DisplayName("Лицевой счёт абонента")]
        [Display(GroupName = "Счёт")]
        [PersonalAccountDataFormat]
        public ulong Лицевой { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Базовый лицевой счёт")]
        [Display(GroupName = "Счёт")]
        [DataFormat(DataFormatString = "{0:##' '####' '####}", ExportFormatString = "##' '####' '####")]
        public ulong БазовыйЛицевой => this.Лицевой / 10;

        [DisplayName("Лицевой счёт в ЖКХ")]
        [Display(GroupName = "Счёт")]
        public ulong ЛицевойЖКХ { get; set; }

        #endregion

        #region Наборы полей для отображения в таблицах

        public static MeterFieldsCollection GetSetOfColumns(string viewName)
        {
            string[] list = viewName switch
            {
                "BaseView" => new string[] { nameof(Лицевой), nameof(ФиоСокращ), nameof(Адрес), nameof(ТипСчетчика), nameof(НомерСчетчика), nameof(Поверка), nameof(Поверен), nameof(ТП), nameof(Коментарий) },
                "DetailedView" => new string[]
                {
                        nameof(Лицевой), nameof(ФиоСокращ), nameof(Адрес), nameof(ТипСчетчика), nameof(НомерСчетчика), nameof(Поверка), nameof(Поверен),
                        nameof(ГодУстановки), nameof(ГодВыпуска), nameof(ГодОбхода), nameof(ГодОплаты),
                        nameof(ШифрТарифа), nameof(Категория), nameof(Расположение), nameof(МестоУстановки),
                        nameof(Использование), nameof(Договор), nameof(СчетчикНаБалансеГродноэнерго), nameof(ПринадлежностьРуп), nameof(МногодетнаяСемья),
                        nameof(ТипНаселённойМестности), nameof(Привязка), nameof(Коментарий),
                },
                "ShortView" => new string[] { nameof(Лицевой), nameof(ФиоСокращ), nameof(Адрес), nameof(ТипСчетчика), nameof(НомерСчетчика), nameof(Поверка), nameof(Поверен) },
                "ОплатаView" => new string[] { nameof(Лицевой), nameof(ФиоСокращ), nameof(Адрес), nameof(ТипСчетчика), nameof(НомерСчетчика), nameof(ПоследнееОплаченноеПоказание), nameof(ПослПоказаниеОбхода), nameof(ДатаОплаты), nameof(ДатаОбхода), nameof(ЕстьДолг), nameof(Долг), nameof(ДолгРуб) },
                "ПривязкаView" => new string[] { nameof(Лицевой), nameof(ФиоСокращ), nameof(НаселённыйПункт), nameof(УлицаСДомомИКв), nameof(ТипСчетчика), nameof(НомерСчетчика), nameof(Подстанция), nameof(Фидер10), nameof(ТП), nameof(ТПNumber), nameof(Фидер04), nameof(Опора) },
                "ПолныйView" => ModelHelper.MeterPropertiesNames.ToArray(),
                _ => ModelHelper.MeterPropertiesNames.ToArray(),
            };

            return new MeterFieldsCollection(list);
        }

        #endregion
    }
}
