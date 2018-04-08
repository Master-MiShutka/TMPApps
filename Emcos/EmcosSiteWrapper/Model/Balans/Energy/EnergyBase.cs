using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.Model.Balans
{
    public abstract class EnergyBase : PropertyChangedBase, IBaseEnergy
    {
        /// <summary>
        /// Описание энергии
        /// </summary>
        public virtual string Description { get; }
        /// <summary>
        /// Краткое описание
        /// </summary>
        public virtual string ShortDescription { get; }

        bool _useMonthValue = false;
        double? _correctionValue, _monthValue;
        IList<double?> _dailyValues;
        IList<DataValue> _dailyValuesWithStatus;

        /// <summary>
        /// Используются данные за месяц, а не сумма суточных
        /// </summary>
        public bool UseMonthValue
        {
            get => _useMonthValue;
            set
            {
                SetProp(ref _useMonthValue, value, "UseMonthValue");
                RaisePropertyChanged("Value");
                RaisePropertyChanged("CorrectedValue");
            }
        }

        /// <summary>
        /// Энергия без корректрировки
        /// </summary>
        public double? Value => UseMonthValue ? MonthValue : SummOfDaysValue;
        /// <summary>
        /// Энергия с корректировкой
        /// </summary>
        public double? CorrectedValue => CorrectionValue.HasValue ? (Value.HasValue ? Value.Value : 0d) + CorrectionValue : Value.Value;
        /// <summary>
        /// Величина корректировки энергии
        /// </summary>
        public double? CorrectionValue
        {
            get => _correctionValue;
            set
            {
                SetProp(ref _correctionValue, value, "CorrectionValue");
                RaisePropertyChanged("CorrectedValue");
                RaisePropertyChanged("Correction");
            }
        }
        /// <summary>
        /// Описание корректировки
        /// </summary>
        public string Correction
        {
            get
            {
                return CorrectionValue.HasValue ? string.Format("{0} {1:N0}", ShortDescription, CorrectionValue.Value) : null;
            }
        }

        /// <summary>
        /// Энергия за месяц
        /// </summary>
        public double? MonthValue
        {
            get => _monthValue;
            set
            {
                SetProp(ref _monthValue, value, "MonthValue");
                RaisePropertyChanged("HasDifferenceBetweenDailySumAndMonth");
                RaisePropertyChanged("DifferenceBetweenDailySumAndMonth");
            }
        }
        /// <summary>
        /// Суточные значения энергии
        /// </summary>
        public IList<double?> DailyValues
        {
            get => _dailyValues;
            set
            {
                SetProp(ref _dailyValues, value, "DailyValues");
                RaisePropertyChanged("DailyValuesWithStatus");
                RaisePropertyChanged("SummOfDaysValue");
                RaisePropertyChanged("HasDifferenceBetweenDailySumAndMonth");
                RaisePropertyChanged("DifferenceBetweenDailySumAndMonth");
                RaisePropertyChanged("DailyValuesAverage");
                RaisePropertyChanged("DailyValuesMax");
                RaisePropertyChanged("DailyValuesMin");
            }
        }
        /// <summary>
        /// Суточные значения энергии со статусом
        /// </summary>
        public IList<DataValue> DailyValuesWithStatus
        {
            get
            {
                if (DailyValues == null) return null;

                if (_dailyValuesWithStatus == null)
                {
                    _dailyValuesWithStatus = new List<DataValue>();
                    var max = DailyValues.Max();
                    foreach (double? item in DailyValues)
                        _dailyValuesWithStatus.Add((max == null || item == null)
                            ? new DataValue { DoubleValue = 0, PercentValue = 0, Status = DataValueStatus.Missing }
                            : new DataValue { DoubleValue = item.Value, PercentValue = item.Value / max, Status = DataValueStatus.Normal });
                }
                return _dailyValuesWithStatus;
            }
        }
        /// <summary>
        /// Сумма суточных значений энергии
        /// </summary>
        public double? SummOfDaysValue => DailyValues != null ? DailyValues.Sum(day => day.HasValue ? day.Value : 0d) : new Nullable<double>();

        /// <summary>
        /// Еимеется ли разница между энергией за месяц и суммой суточных
        /// </summary>
        public bool HasDifferenceBetweenDailySumAndMonth { get => DifferenceBetweenDailySumAndMonth > 0.5d; }
        /// <summary>
        /// Разница между энергией за месяц и суммой суточных
        /// </summary>
        public double? DifferenceBetweenDailySumAndMonth { get => (MonthValue ?? 0d) - (SummOfDaysValue ?? 0d); }
        /// <summary>
        /// Среднее суточных значений
        /// </summary>
        public double? DailyValuesAverage
        {
            get
            {
                double? value = new Nullable<double>();
                if (DailyValues != null)
                {
                    value = DailyValuesWithStatus
                        .Where(i => i.Status != DataValueStatus.Missing)
                        .Average((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }
        /// <summary>
        /// Макимальное суточных значений
        /// </summary>
        public double? DailyValuesMax
        {
            get
            {
                double? value = new Nullable<double>();
                if (DailyValues != null)
                {
                    value = DailyValuesWithStatus
                        .Where(i => i.Status != DataValueStatus.Missing)
                        .Max((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }
        /// <summary>
        /// Минимальное суточных значений
        /// </summary>
        public double? DailyValuesMin
        {
            get
            {
                double? value = new Nullable<double>();
                if (DailyValues != null)
                {
                    value = DailyValuesWithStatus
                        .Where(i => i.Status != DataValueStatus.Missing)
                        .Min((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }
        /// <summary>
        /// Имеются данные не за все сутки
        /// </summary>
        public bool NotFullData
        {
            get
            {
                return (DailyValues != null && DailyValues.Count > 0)
                  ? DailyValues.Any((item) => item.HasValue == false)
                  : false;
            }
        }



        /// <summary>
        /// Инициализация
        /// </summary>
        public void Init()
        {
            _useMonthValue = false;
            CorrectionValue = new Nullable<double>();
            MonthValue = new Nullable<double>();
            DailyValues = null;
        }
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public virtual ML_Param MonthMlParam { get => throw new NotImplementedException(); }
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public virtual ML_Param DayhMlParam { get => throw new NotImplementedException(); }
    }
}
