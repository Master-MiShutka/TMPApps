using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    [DataContract]
    public abstract class DirectedEnergyBase : PropertyChangedBase, IDirectedEnergy
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
        IList<double?> _daysValues;
        IList<DataValue> _daysValuesWithStatus;

        /// <summary>
        /// Используются данные за месяц, а не сумма суточных
        /// </summary>
        [DataMember]
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
        /// Энергия без корректировки
        /// </summary>
        public double? Value => UseMonthValue ? MonthValue : SummOfDaysValue;
        /// <summary>
        /// Энергия с корректировкой
        /// </summary>
        public double? CorrectedValue => Value.HasValue == false ? new Nullable<double>() : Value ?? 0d + CorrectionValue ?? 0d;
        /// <summary>
        /// Величина корректировки энергии
        /// </summary>
        [DataMember]
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
                return CorrectionValue.HasValue 
                    ? string.Format("{0} {1}{2:N0}", ShortDescription, CorrectionValue.Value > 0 ? "+" : "", CorrectionValue.Value) 
                    : null;
            }
        }

        /// <summary>
        /// Энергия за месяц
        /// </summary>
        [DataMember]
        public double? MonthValue
        {
            get => _monthValue;
            set
            {
                SetProp(ref _monthValue, value, "MonthValue");
                RaisePropertyChanged("HasDifferenceBetweenDaysSumAndMonth");
                RaisePropertyChanged("DifferenceBetweenDaysSumAndMonth");
            }
        }
        /// <summary>
        /// Суточные значения энергии
        /// </summary>
        [DataMember]
        //[Newtonsoft.Json.JsonConverter(typeof(NullListToEmptyStringConverter))]
        public IList<double?> DaysValues
        {
            get => _daysValues;
            set
            {
                SetProp(ref _daysValues, value, "DaysValues");
                RaisePropertyChanged("DaysValuesWithStatus");
                RaisePropertyChanged("DaysValuesStatus");
                RaisePropertyChanged("DaysValuesHasMissing");
                RaisePropertyChanged("SummOfDaysValue");
                RaisePropertyChanged("HasDifferenceBetweenDaysSumAndMonth");
                RaisePropertyChanged("DifferenceBetweenDaysSumAndMonth");
                RaisePropertyChanged("DaysValuesAverage");
                RaisePropertyChanged("DaysValuesMax");
                RaisePropertyChanged("DaysValuesMin");
                RaisePropertyChanged("NotFullData");
            }
        }
        /// <summary>
        /// Суточные значения энергии со статусом
        /// </summary>
        public IList<DataValue> DaysValuesWithStatus
        {
            get
            {
                if (DaysValues == null) return null;

                if (_daysValuesWithStatus == null)
                {
                    _daysValuesWithStatus = new List<DataValue>();
                    var max = DaysValues.Where(i => i.HasValue).Max();
                    foreach (double? item in DaysValues)
                        _daysValuesWithStatus.Add((max == null || item == null)
                            ? new DataValue { DoubleValue = 0, PercentValue = 0, Status = DataValueStatus.Missing }
                            : new DataValue { DoubleValue = item.Value, PercentValue = item.Value / max, Status = DataValueStatus.Normal });
                }
                return _daysValuesWithStatus;
            }
        }
        /// <summary>
        /// Статус данных за сутки
        /// </summary>
        public string DaysValuesStatus
        {
            get
            {
                if (DaysValues == null || DaysValues.Count == 0)
                    return null;
                int missingDataCount = DaysValues.Where((d) => d.HasValue == false).Count();
                return missingDataCount == 0 ? null : String.Format("{0} из {1}", missingDataCount, DaysValues.Count);
            }
        }
        /// <summary>
        /// Имеются ли отсутствующие данные хотя бы за одни сутки
        /// </summary>
        public bool DaysValuesHasMissing
        {
            get
            {
                return (DaysValues != null && DaysValues.Count > 0)
                    ? DaysValues.Any((item) => item.HasValue == false)
                    : false;
            }
        }

        /// <summary>
        /// Сумма суточных значений энергии
        /// </summary>
        public double? SummOfDaysValue => DaysValues != null ? DaysValues.Sum(day => day ?? 0d) : new Nullable<double>();

        /// <summary>
        /// Еимеется ли разница между энергией за месяц и суммой суточных
        /// </summary>
        public bool HasDifferenceBetweenDaysSumAndMonth { get => DifferenceBetweenDaysSumAndMonth > 0.5d; }
        /// <summary>
        /// Разница между энергией за месяц и суммой суточных
        /// </summary>
        public double? DifferenceBetweenDaysSumAndMonth { get => (MonthValue ?? 0d) - (SummOfDaysValue ?? 0d); }
        /// <summary>
        /// Среднее суточных значений
        /// </summary>
        public double? DaysValuesAverage
        {
            get
            {
                double? value = new Nullable<double>();
                if (DaysValues != null)
                {
                    value = DaysValuesWithStatus
                        .Where(i => i.Status != DataValueStatus.Missing)
                        .Average((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }
        /// <summary>
        /// Макимальное суточных значений
        /// </summary>
        public double? DaysValuesMax
        {
            get
            {
                double? value = new Nullable<double>();
                if (DaysValues != null)
                {
                    value = DaysValuesWithStatus
                        .Where(i => i.Status != DataValueStatus.Missing)
                        .Max((i) => i.DoubleValue.Value);
                }
                return value;
            }
        }
        /// <summary>
        /// Минимальное суточных значений
        /// </summary>
        public double? DaysValuesMin
        {
            get
            {
                double? value = new Nullable<double>();
                if (DaysValues != null)
                {
                    value = DaysValuesWithStatus
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
                return (DaysValues != null && DaysValues.Count > 0)
                  ? DaysValues.Any((item) => item.HasValue == false)
                  : false;
            }
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        public void ClearData()
        {
            UseMonthValue = false;
            CorrectionValue = new Nullable<double>();
            MonthValue = new Nullable<double>();
            DaysValues = null;
        }
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public virtual ML_Param MonthMlParam { get => throw new NotImplementedException(); }
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public virtual ML_Param DayMlParam { get => throw new NotImplementedException(); }
    }
}
