using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace TMP.WORK.AramisChetchiki.Model
{
    using TMP.Extensions;
    using System.ComponentModel.DataAnnotations;

    [DataContract]
    [Serializable]
    public class Meter
    {
        [IgnoreDataMember]
        [DisplayName("Поверен")]
        [SummaryInfo]
        [Display(Order = 1, GroupName = "")]
        public bool Поверен
        {
            get
            {
                int year = Год_поверки + Период_поверки;
                DateTime date = new DateTime(year, Квартал_поверки * 3 - 2, 1);
                DateTime now = Дата_сравнения_поверки;

                if (year < now.Year)
                    return false;
                if (year == now.Year)
                {
                    if (Квартал_поверки < now.GetQuarter())
                        return false;
                }
                return true;
            }
        }
        [IgnoreDataMember]
        [DisplayName("Тип населённого пункта")]
        [SummaryInfo]
        [Display(Order = 2, GroupName = "")]
        public string Тип_населённого_пункта
        {
            get { return String.IsNullOrWhiteSpace(_населённый_пункт) ? "неизвестно" : (_населённый_пункт.StartsWith("г.") ? "город" : "село"); }
        }

        [DataMember]
        [DisplayName("Заключен ли договор")]
        [SummaryInfo]
        [Display(Order = 3, GroupName = "")]
        public bool Договор { get; set; }
        [DataMember]
        [DisplayName("На балансе РУП 'ГродноЭнерго'")]
        [SummaryInfo]
        [Display(Order = 4, GroupName = "")]
        public bool Баланс_РУП { get; set; }
        [DataMember]
        [DisplayName("Принадлежит РУП 'ГродноЭнерго'")]
        [SummaryInfo]
        [Display(Order = 5, GroupName = "")]
        public bool Принадлежность_РУП { get; set; }
        [DataMember]
        [DisplayName("Горячее водоснабжение")]
        [SummaryInfo]
        [Display(Order = 6, GroupName = "")]
        public bool Горячее_водоснабжение { get; set; }
        [DataMember]
        [DisplayName("Имеющие природный газ")]
        [SummaryInfo]
        [Display(Order = 7, GroupName = "")]
        public bool Имеющие_природный_газ { get; set; }
        [DataMember]
        [DisplayName("Многодетная семья")]
        public bool Многодетная_семья { get; set; }
        [DataMember]
        [DisplayName("Выносное АСКУЭ")]
        [Display(Order = 8, GroupName = "")]
        public bool Выносное_АСКУЭ { get; set; }
        [DataMember]
        [DisplayName("Лицевой счёт абонента")]
        [Display(Order = 9, GroupName = "")]
        public ulong Лицевой { get; set; }
        [DataMember]
        [DisplayName("Ф.И.О.")]
        [Display(Order = 10, GroupName = "")]
        public string ФИО { get; set; }
        [DataMember]
        [DisplayName("Последнее показание")]
        [Display(Order = 11, GroupName = "")]
        public double Посл_показание { get; set; }
        [DataMember]
        [DisplayName("Последнее показание при обходе")]
        [Display(Order = 12, GroupName = "")]
        public double Посл_показание_обхода { get; set; }
        [IgnoreDataMember]
        private DateTime? _дата_оплаты;
        [DataMember]
        [DisplayName("Дата оплаты")]
        [Display(Order = 13, GroupName = "")]
        public DateTime? Дата_оплаты
        {
            get { if (_дата_оплаты == null || _дата_оплаты == default(DateTime)) return new DateTime(1900, 1, 1); else return _дата_оплаты; }
            set { _дата_оплаты = value; }
        }
        [IgnoreDataMember]
        [DisplayName("Год оплаты")]
        [SummaryInfo]
        [Display(Order = 14, GroupName = "")]
        public int Год_оплаты { get { return Дата_оплаты.HasValue ? Дата_оплаты.Value.Year : 0; } }
        [IgnoreDataMember]
        private DateTime? _дата_обхода;
        [DataMember]
        [DisplayName("Дата обхода")]
        [Display(Order = 15, GroupName = "")]
        public DateTime? Дата_обхода
        {
            get { if (_дата_обхода == null || _дата_обхода == default(DateTime)) return new DateTime(1900, 1, 1); else return _дата_обхода; }
            set { _дата_обхода = value; }
        }
        [IgnoreDataMember]
        [DisplayName("Год обхода")]
        [SummaryInfo]
        [Display(Order = 16, GroupName = "")]
        public int Год_обхода { get { return Дата_обхода.HasValue ? Дата_обхода.Value.Year : 0; } }
        [DataMember]
        [DisplayName("Шифр тарифа")]
        [SummaryInfo]
        [Display(Order = 17, GroupName = "")]
        public string Шифр_тарифа { get; set; }
        [DataMember]
        [DisplayName("Категория")]
        [SummaryInfo]
        [Display(Order = 18, GroupName = "")]
        public string Категория { get; set; }
        [DataMember]
        [DisplayName("Расположение")]
        [SummaryInfo]
        [Display(Order = 19, GroupName = "")]
        public string Расположение { get; set; }
        [DataMember]
        [DisplayName("Место установки учёта")]
        [SummaryInfo]
        [Display(Order = 20, GroupName = "")]
        public string Место_установки { get; set; }
        [DataMember]
        [DisplayName("Использование")]
        [SummaryInfo]
        [Display(Order = 21, GroupName = "")]
        public string Использование { get; set; }
        [DataMember]
        [DisplayName("Примечание")]
        [Display(Order = 22, GroupName = "")]
        public string Примечание { get; set; }
        [DataMember]
        [DisplayName("Адрес")]
        [Display(Order = 23, GroupName = "")]
        public string Адрес { get; set; }
        [IgnoreDataMember]
        private string _населённый_пункт = string.Empty;
        [DataMember]
        [DisplayName("Населённый пункт")]
        [SummaryInfo]
        [Display(Order = 24, GroupName = "")]
        public string Населённый_пункт
        {
            get { return String.IsNullOrWhiteSpace(_населённый_пункт) ? (String.IsNullOrEmpty(Адрес) ? String.Empty : Адрес.Split(',')[0]) : _населённый_пункт; }
            set { _населённый_пункт = value; }
        }

        [DataMember]
        [DisplayName("Тип счётчика")]
        [Display(Order = 25, GroupName = "")]
        [SummaryInfo]
        public string Тип_счетчика { get; set; }
        [DataMember]
        [DisplayName("Ампераж счётчика")]
        [Display(Order = 26, GroupName = "")]
        public string Ампераж { get; set; }
        [DataMember]
        [DisplayName("Принцип действия счётчика")]
        [SummaryInfo]
        [Display(Order = 27, GroupName = "")]
        public string Принцип { get; set; }
        [DataMember]
        [DisplayName("Количество фаз счётчика")]
        [SummaryInfo]
        [Display(Order = 28, GroupName = "")]
        public byte Фаз { get; set; }
        [DataMember]
        [DisplayName("Номер счётчика")]
        [Display(Order = 29, GroupName = "")]
        public string Номер_счетчика { get; set; }
        [DataMember]
        [DisplayName("Квартал поверки счётчика")]
        [Display(Order = 30, GroupName = "")]
        public byte Квартал_поверки { get; set; }

        [IgnoreDataMember]
        private int _год_поверки;
        [DataMember]
        [DisplayName("Год поверки счётчика")]
        [SummaryInfo]
        [Display(Order = 31, GroupName = "")]
        public int Год_поверки
        {
            get { return _год_поверки; }
            set { _год_поверки = FixYear(value); }
        }

        private int FixYear(int value)
        {
            if (value < 100)
            {
                if (value > 50)
                    return 1900 + value;
                else
                    return 2000 + value;
            }
            else
                return value;
        }

        [DataMember]
        [DisplayName("Год выпуска счётчика")]
        [SummaryInfo]
        [Display(Order = 32, GroupName = "")]
        public int Год_выпуска { get; set; }
        [IgnoreDataMember]
        private DateTime? _дата_установки;
        [DataMember]
        [DisplayName("Дата установки учёта")]
        [Display(Order = 33, GroupName = "")]
        public DateTime? Дата_установки
        {
            get { if (_дата_установки == null || _дата_установки == default(DateTime)) return new DateTime(1900, 1, 1); else return _дата_установки; }
            set { _дата_установки = value; }
        }
        [IgnoreDataMember]
        [DisplayName("Год установки учёта")]
        [SummaryInfo]
        [Display(Order = 34, GroupName = "")]
        public int Год_установки { get { return Дата_установки.HasValue ? Дата_установки.Value.Year : 0; } }
        [IgnoreDataMember]
        [DisplayName("Поверка")]
        [Display(Order = 35, GroupName = "")]
        public string Поверка { get { return Квартал_поверки + " кв " + (Год_поверки < 10 ? "0" + Год_поверки.ToString() : Год_поверки.ToString()); } }
        [DataMember]
        [DisplayName("Период поверки")]
        [Display(Order = 36, GroupName = "")]
        public byte Период_поверки { get; set; }


        [IgnoreDataMember]
        private static DateTime _дата_сравнения_поверки = DateTime.Now;
        [IgnoreDataMember]
        public static DateTime Дата_сравнения_поверки
        {
            get { return _дата_сравнения_поверки; }
            set { _дата_сравнения_поверки = value; }
        }
        #region Привязка
        [DataMember]
        [DisplayName("Подстанция")]
        [SummaryInfo]
        [Display(Order = 37, GroupName = "")]
        public string Подстанция { get; set; }
        [DataMember]
        [DisplayName("Фидер 10 кВ")]
        [SummaryInfo]
        [Display(Order = 38, GroupName = "")]
        public string Фидер10 { get; set; }
        [DataMember]
        [DisplayName("ТП")]
        [SummaryInfo]
        [Display(Order = 39, GroupName = "")]
        public string ТП { get; set; }
        [DataMember]
        [DisplayName("Фидер 0,4 кВ")]
        [Display(Order = 40, GroupName = "")]
        public string Фидер04 { get; set; }
        [DataMember]
        [DisplayName("№ опоры")]
        [Display(Order = 41, GroupName = "")]
        public string Опора { get; set; }
        [IgnoreDataMember]
        private string _привязка { get; set; }
        [DataMember]
        [DisplayName("Привязка")]
        [Display(Order = 42, GroupName = "")]
        public string Привязка
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_привязка))
                    return _привязка = String.Join(", ", Подстанция, Фидер10, ТП, Фидер04, Опора);
                else
                    return _привязка;
            }
            set { _привязка = value; }
        }
        #endregion
        [DataMember]
        [DisplayName("Наличие АСКУЭ")]
        [SummaryInfo]
        [Display(Order = 43, GroupName = "")]
        public bool Аскуэ { get; set; }
        [DataMember]
        [DisplayName("SMS")]
        [Display(Order = 44, GroupName = "")]
        public string SMS { get; set; }
        [IgnoreDataMember]
        [DisplayName("Заполнено ли поле SMS")]
        [SummaryInfo]
        [Display(Order = 45, GroupName = "")]
        public bool HasSMS => String.IsNullOrWhiteSpace(SMS) == false;

        #region Наборы полей для отображения в таблицах

        public static IList<string> BaseViewColumns => new List<string>()
        {
            "Лицевой", "ФИО", "Адрес", "Тип_счетчика", "Номер_счетчика", "Поверка", "Поверен", "Примечание"
        };

        public static IList<string> DetailedViewColumns => new List<string>()
        {
            "Лицевой", "ФИО", "Адрес", "Тип_счетчика", "Номер_счетчика", "Поверка", "Поверен",
            "Год_установки", "Год_выпуска", "Год_обхода", "Год_оплаты",
            "Шифр_тарифа", "Категория", "Расположение", "Место_установки",
            "Использование", "Договор", "Баланс_РУП", "Многодетная_семья",
            "Тип_населённого_пункта", "Привязка", "Примечание"
        };

        public static IList<string> ShortViewColumns => new List<string>()
        {
            "Лицевой", "ФИО", "Адрес", "Тип_счетчика", "Номер_счетчика", "Поверка", "Поверен"
        };

        public static IList<string> ОплатаViewColumns => new List<string>()
        {
            "Лицевой", "ФИО", "Адрес", "Тип_счетчика", "Номер_счетчика", "Посл_показание", "Посл_показание_обхода", "Дата_оплаты", "Дата_обхода", "Шифр_тарифа"
        };

        public static IList<string> ПривязкаViewColumns => new List<string>()
        {
            "Лицевой", "ФИО", "Адрес", "Населённый_пункт", "Тип_счетчика", "Номер_счетчика", "Подстанция", "Фидер10", "ТП", "Фидер04", "Опора"
        };

        #endregion
    }
}
