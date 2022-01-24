namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.Serialization;
    using TMP.Extensions;
    using TMP.Shared;

    [KnownType(typeof(Address))]
    [KnownType(typeof(TransformerSubstation))]
    [MessagePack.MessagePackObject]
    public class Meter : IModelWithPersonalId
    {
        private const string UNKNOWN_STR = "#Н/Д";

        #region Контактные данные

        [DisplayName("Фамилия")]
        [Display(GroupName = "Контактные данные")]
        [MessagePack.Key(0)]
        public string Фамилия { get; set; }

        [DisplayName("Имя")]
        [Display(GroupName = "Контактные данные")]
        [MessagePack.Key(1)]
        public string Имя { get; set; }

        [DisplayName("Отчество")]
        [Display(GroupName = "Контактные данные")]
        [MessagePack.Key(2)]
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
        [MessagePack.Key(3)]
        public string SMS { get; set; }

        [DisplayName("Телефоны")]
        [Display(GroupName = "Контактные данные")]
        [MessagePack.Key(4)]
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
        [MessagePack.Key(5)]
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
        [MessagePack.Key(6)]
        public string Категория { get; set; }

        [DisplayName("Коментарий")]
        [Display(GroupName = "Абонент")]
        [MessagePack.Key(7)]
        public string Коментарий { get; set; }

        [DisplayName("Дата уведомления")]
        [Display(GroupName = "Абонент")]
        [DateTimeDataFormat]
        [MessagePack.Key(8)]
        public DateOnly? ДатаУведомления { get; set; }

        [DisplayName("Дата отключения")]
        [Display(GroupName = "Абонент")]
        [DateTimeDataFormat]
        [MessagePack.Key(9)]
        public DateOnly? ДатаОтключения { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Год отключения")]
        [Display(GroupName = "Абонент")]
        [SummaryInfo]
        public int? ГодОтключения => this.Отключён ? this.ДатаОтключения.Value.Year : null;

        [MessagePack.IgnoreMember]
        [Display(GroupName = "Абонент")]
        [DisplayName("Отключён")]
        [SummaryInfo]
        public bool Отключён => this.ДатаОтключения.HasValue ? (this.ДатаОтключения.Value == default ? false : true) : false;

        [DisplayName("Удалён из базы")]
        [Display(GroupName = "Абонент")]
        [SummaryInfo]
        [MessagePack.Key(10)]
        public bool Удалён { get; set; } = false;

        [DisplayName("Дата удаления из базы")]
        [Display(GroupName = "Абонент")]
        [MessagePack.Key(11)]
        public DateOnly ДатаУдаления { get; set; }

        [DisplayName("Прописано человек")]
        [Display(GroupName = "Абонент")]
        [SummaryInfo]
        [MessagePack.Key(12)]
        public int? КолвоЧеловек { get; set; }

        #endregion

        #region Счётчик

        [DisplayName("Шифр тарифа")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(13)]
        public string ШифрТарифа { get; set; }

        [DisplayName("Тип счётчика")]
        [Display(GroupName = "Счётчик")]
        [SummaryInfo]
        [MessagePack.Key(14)]
        public string ТипСчетчика { get; set; }

        [DisplayName("Ампераж счётчика")]
        [Display(GroupName = "Счётчик")]
        [TextDataFormat]
        [MessagePack.Key(15)]
        public string Ампераж { get; set; }

        [DisplayName("Принцип действия счётчика")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(16)]
        public string Принцип { get; set; }

        [DisplayName("Количество фаз счётчика")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(17)]
        public byte Фаз { get; set; }

        [DisplayName("Номер счётчика")]
        [Display(GroupName = "Счётчик")]
        [TextDataFormat]
        [MessagePack.Key(18)]
        public string НомерСчетчика { get; set; }

        [DisplayName("Номера пломб")]
        [Display(GroupName = "Счётчик")]
        [TextDataFormat]
        [MessagePack.Key(19)]
        public string НомераПломб { get; set; }

        [DisplayName("Мощность электроустановки, кВт")]
        [Display(GroupName = "Счётчик")]
        [SummaryInfo]
        [MessagePack.Key(20)]
        public decimal? Мощность { get; set; }

        [DisplayName("Год выпуска счётчика")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(21)]
        public int ГодВыпуска { get; set; }

        [MessagePack.IgnoreMember]
        private DateOnly? датаУстановки;

        [DisplayName("Дата установки учёта")]
        [Display(GroupName = "Счётчик")]
        [DateTimeDataFormat]
        [MessagePack.Key(22)]
        public DateOnly ДатаУстановки
        {
            get => this.датаУстановки == null || this.датаУстановки == default ? default : this.датаУстановки.Value;
            set => this.датаУстановки = value;
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Год установки учёта")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик")]
        public int ГодУстановки => this.ДатаУстановки.Year;

        [DisplayName("Показание при установке")]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(23)]
        public int ПоказаниеПриУстановке { get; set; }

        [DisplayName("Расчётное показание")]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(24)]
        public int? РасчПоказание { get; set; }

        [DisplayName("Последнее показание при обходе")]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(25)]
        public int? ПослПоказаниеОбхода { get; set; }

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
        [MessagePack.Key(26)]
        public byte Тарифов { get; set; }

        [DisplayName("Количество используемых знаков")]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(27)]
        public decimal Разрядность { get; set; }

        #endregion

        #region Счётчик-признаки

        [DisplayName("Расположение")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик-признаки")]
        [MessagePack.Key(28)]
        public string Расположение { get; set; }

        [DisplayName("Место установки учёта")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик-признаки")]
        [MessagePack.Key(29)]
        public string МестоУстановки { get; set; }

        [DisplayName("Использование")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик-признаки")]
        [MessagePack.Key(30)]
        public string Использование { get; set; }

        [DisplayName("Наличие АСКУЭ")]
        [SummaryInfo]
        [Display(GroupName = "Счётчик-признаки")]
        [MessagePack.Key(31)]
        public bool Аскуэ { get; set; }

        #endregion

        #region Показания

        [DisplayName("Показание при отключении")]
        [Display(GroupName = "Счётчик")]
        [MessagePack.Key(32)]
        public int? ПоказаниеПриОтключении { get; set; }

        [DisplayName("Последнее оплаченное показание")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        [MessagePack.Key(33)]
        public int? ПоследнееОплаченноеПоказание { get; set; }

        [DisplayName("Предыдуще оплаченное показание")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        [MessagePack.Key(34)]
        public int? ПредыдущеОплаченноеПоказание { get; set; }

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
        [MessagePack.Key(35)]
        public decimal? ДолгРуб { get; set; }

        [DisplayName("ErrSumN")]
        [Display(GroupName = "Оплата")]
        [MessagePack.Key(36)]
        public decimal? ErrSumN { get; set; }

        [DisplayName("ErrSumV")]
        [Display(GroupName = "Оплата")]
        [MessagePack.Key(37)]
        public decimal? ErrSumV { get; set; }

        [DisplayName("Сумма оплаты")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        [MessagePack.Key(38)]
        public decimal? СуммаОплаты { get; set; }

        [DisplayName("Период последней оплаты")]
        [Display(GroupName = "Оплата")]
        [DateTimeDataFormat]
        [MessagePack.Key(39)]
        public DateOnly? ПериодПослОплаты { get; set; }

        [DisplayName("Среднее")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        [MessagePack.Key(40)]
        public int? Среднее { get; set; }

        [DisplayName("Месяц")]
        [Display(GroupName = "Оплата")]
        [NumericDataFormat]
        [MessagePack.Key(41)]
        public int? Месяц { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Долг")]
        [Display(GroupName = "Оплата")]
        public uint? Долг
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

                int среднее = this.Среднее.Value;
                int месяц = this.Месяц.Value;

                return (uint)Math.Round(1d * среднее / месяц * (months - 1), 0);
            }
        }

        [MessagePack.IgnoreMember]
        private DateOnly? датаОплаты;

        [DisplayName("Дата оплаты")]
        [Display(GroupName = "Оплата")]
        [DateTimeDataFormat]
        [MessagePack.Key(42)]
        public DateOnly ДатаОплаты
        {
            get => this.датаОплаты == null || this.датаОплаты == default ? default : this.датаОплаты.Value;
            set => this.датаОплаты = value;
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Год оплаты")]
        [SummaryInfo]
        [Display(GroupName = "Оплата")]
        public int ГодОплаты => this.ДатаОплаты.Year;

        [DisplayName("Задолженник")]
        [Display(GroupName = "Оплата")]
        [SummaryInfo]
        [MessagePack.Key(43)]
        public bool Задолженник { get; set; }

        [DisplayName("Задолженность, кВт∙ч")]
        [Display(GroupName = "Оплата")]
        [MessagePack.IgnoreMember]
        public decimal Задолженность
        {
            get
            {
                if (this.ДатаОбхода == this.ДатаУстановки)
                {
                    return 0;
                }

                if (this.ДатаОбхода > this.ДатаОплаты)
                {
                    int value = (this.ПослПоказаниеОбхода.HasValue ? this.ПослПоказаниеОбхода.Value : 0) - (this.ПоследнееОплаченноеПоказание.HasValue ? this.ПоследнееОплаченноеПоказание.Value : 0);
                    return value > 0 ? value : 0;
                }
                else
                {
                    int value = (this.ПослПоказаниеОбхода.HasValue ? this.ПослПоказаниеОбхода.Value : 0) - (this.ПоследнееОплаченноеПоказание.HasValue ? this.ПоследнееОплаченноеПоказание.Value : 0);
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
                    return true;

                var paymentDate = this.ДатаОплаты.ToDateTime(TimeOnly.MinValue);

                if ((DateTime.Now - paymentDate).Days >= 62)
                    return true;
                else
                    return false;
            }
        }

        [DisplayName("Среднемесячный расход по оплате, кВт∙ч")]
        [Display(GroupName = "Оплата")]
        [SummaryInfo]
        [MessagePack.Key(44)]
        public int? СреднеМесячныйРасходПоОплате { get; set; }

        #endregion

        #region Обход

        [MessagePack.IgnoreMember]
        private DateOnly? датаОбхода;

        [DisplayName("Дата обхода")]
        [Display(GroupName = "Обход")]
        [DateTimeDataFormat]
        [MessagePack.Key(45)]
        public DateOnly ДатаОбхода
        {
            get => this.датаОбхода == null || this.датаОбхода == default ? DateOnly.FromDateTime(new DateTime(1900, 1, 1)) : this.датаОбхода.Value;
            set => this.датаОбхода = value;
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Год обхода")]
        [SummaryInfo]
        [Display(GroupName = "Обход")]
        public int ГодОбхода => this.ДатаОбхода.Year;

        [MessagePack.IgnoreMember]
        [DisplayName("Обхода не было Х месяцев")]
        [Display(GroupName = "Обход")]
        public int ОбходаНеБылоМесяцев
        {
            get
            {
                DateOnly now = DateOnly.FromDateTime(DateTime.Now);
                DateOnly обход = this.ДатаОбхода;
                return ((now.Year - обход.Year) * 12) + now.Month - обход.Month;
            }
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Обхода не было")]
        [SummaryInfo]
        [Display(GroupName = "Обход")]
        public string ОбходаНеБыло
        {
            get
            {
                return this.ОбходаНеБылоМесяцев switch
                {
                    0 => "был в этом месяце",
                    > 0 and <= 3 => "был недавно",
                    > 3 and <= 6 => "более 3 месяцев",
                    > 6 and <= 12 => "более полугода",
                    > 12 and <= 36 => "более 1 года",
                    > 36 and < 60 => "более 3 лет",
                    >= 60 => "очень давно",
                    _ => throw new NotImplementedException()
                };
            }
        }

        [DisplayName("Контролёр")]
        [SummaryInfo]
        [Display(GroupName = "Обход")]
        [DisplayFormat(DataFormatString = "{0}")]
        [MessagePack.Key(46)]
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
                uint year = this.ГодПоверки + this.ПериодПоверки;
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
        [MessagePack.Key(47)]
        public byte КварталПоверки { get; set; }

        [MessagePack.IgnoreMember]
        private uint годПоверки;

        [DisplayName("Год поверки счётчика")]
        [SummaryInfo]
        [Display(GroupName = "Поверка")]
        [MessagePack.Key(48)]
        public uint ГодПоверки
        {
            get => this.годПоверки;
            set => this.годПоверки = FixYear(value);
        }

        private static uint FixYear(uint value)
        {
            return value < 100 ? value > 50 ? 1900 + value : 2000 + value : value;
        }

        [MessagePack.IgnoreMember]
        [DisplayName("Поверка")]
        [Display(GroupName = "Поверка")]
        public string Поверка => this.КварталПоверки + " кв " + (this.ГодПоверки < 10 ? "0" + this.ГодПоверки.ToString(AppSettings.CurrentCulture) : this.ГодПоверки.ToString(AppSettings.CurrentCulture));

        [DisplayName("Период поверки")]
        [Display(GroupName = "Поверка")]
        [MessagePack.Key(49)]
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
        [MessagePack.Key(50)]
        public string Подстанция { get; set; }

        [DisplayName("Фидер 10 кВ")]
        [SummaryInfo]
        [Display(GroupName = "Привязка")]
        [MessagePack.Key(51)]
        public string Фидер10 { get; set; }

        [DisplayName("ТП")]
        [SummaryInfo]
        [Display(GroupName = "Привязка")]
        [MessagePack.Key(52)]
        public TransformerSubstation ТП { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Тип ТП")]
        [Display(GroupName = "Привязка")]
        public string ТПType => this.ТП?.Type;

        [MessagePack.IgnoreMember]
        [DisplayName("Номер ТП")]
        [Display(GroupName = "Привязка")]
        public int? ТПNumber => this.ТП?.Number < 0 ? null : this.ТП?.Number;

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
        [MessagePack.Key(53)]
        public int? Фидер04 { get; set; }

        [DisplayName("№ опоры")]
        [Display(GroupName = "Привязка")]
        [TextDataFormat]
        [MessagePack.Key(54)]
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
        [MessagePack.Key(55)]
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
        [MessagePack.Key(56)]
        public bool Договор { get; set; }

        [DisplayName("Дата договора")]
        [Display(GroupName = "Признаки")]
        [MessagePack.Key(57)]
        public DateOnly? ДатаДоговора { get; set; }

        [SummaryInfo]
        [DisplayName("Льгота")]
        [Display(GroupName = "Признаки")]
        [MessagePack.Key(58)]
        public int? Льгота { get; set; }

        [DisplayName("Принадлежит РУП 'ГродноЭнерго'")]
        [Display(GroupName = "Признаки")]
        [MessagePack.Key(59)]
        public bool ПринадлежностьРуп { get; set; }

        [DisplayName("Работник ГПО БелЭнерго")]
        [Display(GroupName = "Признаки")]
        [SummaryInfo]
        [MessagePack.Key(60)]
        public bool РаботникБелЭнерго { get; set; } = false;

        #endregion

        #region Счёт

        [DisplayName("Лицевой счёт абонента")]
        [Display(GroupName = "Счёт")]
        [PersonalAccountDataFormat]
        [MessagePack.Key(61)]
        public ulong Лицевой { get; set; }

        [MessagePack.IgnoreMember]
        [DisplayName("Базовый лицевой счёт")]
        [Display(GroupName = "Счёт")]
        [DataFormat(DataFormatString = "{0:##' '####' '####}", ExportFormatString = "##' '####' '####")]
        public ulong БазовыйЛицевой => this.Лицевой / 10;

        [DisplayName("Лицевой счёт в ЖКХ")]
        [Display(GroupName = "Счёт")]
        [MessagePack.Key(62)]
        public ulong ЛицевойЖКХ { get; set; }

        #endregion

        [MessagePack.Key(63)]
        public IEnumerable<MeterEvent> Events { get; set; }

        [DisplayName("Среднемесячный расход по контрольным показаниям, кВт∙ч")]
        [Display(GroupName = "Оплата")]
        [SummaryInfo]
        [MessagePack.Key(64)]
        public int? СреднеМесячныйРасходПоКонтрольнымПоказаниям { get; set; }

        #region Наборы полей для отображения в таблицах

        public static MeterFieldsCollection GetSetOfColumns(string viewName)
        {
            var list = viewName switch
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
