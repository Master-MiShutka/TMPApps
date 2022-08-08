using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMP.Shared;

namespace TMP.Work.Emcos.Model.Balance
{
    public class ActiveEnergyBalance : Balance<ActiveEnergy>
    {
        public ActiveEnergyBalance(IBalanceGroupItem group) : base(group)
        {

        }
    }

    public class ReactiveEnergyBalance : Balance<ReactiveEnergy>
    {
        public ReactiveEnergyBalance(IBalanceGroupItem group) : base(group)
        {

        }
    }


    /// <summary>
    /// Баланс энергии группы элементов
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Balance<T> : PropertyChangedBase, IBalance where T : IEnergy
    {
        #region Fields

        double? _maximumAllowableUnbalance, _maximumAllowableUnbalanceCalculated;

        Dictionary<IBalanceItem, IEnergy> _BalanceItemsEnergy;


        #endregion

        public Balance(IBalanceGroupItem group)
        {
            BalanceGroup = group;

            if (BalanceGroup.Formula == null)
                BalanceGroup.Formula = BalanceFormula.CreateDefault();

            _BalanceItemsEnergy = new Dictionary<IBalanceItem, IEnergy>();
            if (BalanceGroup.HasChildren)
                foreach (IBalanceItem item in BalanceGroup.Children)
                    if (_BalanceItemsEnergy.ContainsKey(item) == false)
                        _BalanceItemsEnergy.Add(item, GetEnergyForBalanceItem(item));

            BalanceGroup.PropertyChanged += BalanceGroup_PropertyChanged;
        }

        #region Private methods

        /// <summary>
        /// Отслеживание изменение свойств группы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BalanceGroup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Formula":
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Возвращает количество энергии, заданной свойством <see cref="Energy"/>, 
        /// указанного направления, для дочерних элментов группы, указанного типа
        /// </summary>
        /// <param name="elementType">Тип дочерних элементов</param>
        /// <param name="energyDirection">Направление энергии</param>
        /// <returns>Количество энергии</returns>
        private double? GetBalanceItemsEnergyValue(ElementTypes elementType, BalanceFormula.EnergyDirection energyDirection)
        {
            double? result = new Nullable<double>(0);
            if (BalanceGroup.HasChildren)
            {
                var items = BalanceGroup.Children.Where(i => i.ElementType == elementType);
                foreach (IBalanceItem item in items)
                {
                    IEnergy energy;
                    if (item.ActiveEnergy.Parameter == Energy.Parameter)
                        energy = item.ActiveEnergy;
                    else
                        energy = item.ReactiveEnergy;

                    var value = energyDirection == BalanceFormula.EnergyDirection.@in ? energy.Plus.CorrectedValue : energy.Minus.CorrectedValue;
                    if (value.HasValue)
                        result += value;
                }
            }
            else
                return null;
            return result;
        }
        /// <summary>
        /// Возвращает энергию элемента
        /// </summary>
        /// <param name="BalanceItem">Элемент баланса</param>
        /// <returns>Энегия - активная или реактивная</returns>
        private IEnergy GetEnergyForBalanceItem(IBalanceItem BalanceItem)
        {
            if (BalanceItem.ActiveEnergy.GetType() == typeof(T))
                return BalanceItem.ActiveEnergy;
            if (BalanceItem.ReactiveEnergy.GetType() == typeof(T))
                return BalanceItem.ReactiveEnergy;
            return null;
        }
        /// <summary>
        /// Возвращает значение энергии элемента
        /// </summary>
        /// <param name="BalanceItem">Элемент баланса</param>
        /// <param name="energyDirection">Направление энергии</param>
        /// <returns>Значение энергии</returns>
        private double? GetEnergyValueForBalanceItem(IBalanceItem BalanceItem, BalanceFormula.EnergyDirection energyDirection)
        {
            IEnergy energy = _BalanceItemsEnergy[BalanceItem];
            var value = energyDirection == BalanceFormula.EnergyDirection.@in
                ? energy.Plus.CorrectedValue
                : energy.Minus.CorrectedValue;
            return value;
        }

        private string GetTitle(string title)
        {
            title = title.Replace("10кВ ввод ", "").Replace("6кВ ввод ", "").Replace("0,4кВ ", "");
            int pos = title.IndexOf(',');
            if (pos > 0 && title.Length - pos > 2)
            {
                title = title.Substring(pos + 1);
            }
            return title;
        }
        /// <summary>
        /// Возвращает сумму значений указанной энергии за указанные сутки всех элементов указанного типа
        /// </summary>
        /// <param name="dayIndex">Индекс суток</param>
        /// <param name="listOfElementAndEnergy">Список пар элемент-энергия</param>
        /// <param name="energyDirection">Направление энергии</param>
        /// <returns>Сумма энергии элементов за сутки</returns>
        private double? GetBalanceItemDayEnergyValue(int dayIndex, IList<KeyValuePair<IBalanceItem, IEnergy>> listOfElementAndEnergy, BalanceFormula.EnergyDirection energyDirection)
        {
            double? result = new Nullable<double>(0);
            foreach (var item in listOfElementAndEnergy)
            {
                var value = energyDirection == BalanceFormula.EnergyDirection.@in ? item.Value.Plus.DaysValues[dayIndex] : item.Value.Minus.DaysValues[dayIndex];
                if (value.HasValue)
                    result += value;
            }
            return result;
        }

        #endregion

        #region Public methods

        public void Initialize(DatePeriod period)
        {
            Period = period;

            int daysCount = Period.EndDate.Subtract(Period.StartDate).Days + 1;

            //if (group.DailyEminus.Count != daysCount)
            //    throw new ArgumentOutOfRangeException("Количество дней между датой начала и датой окончания периода отличается от количества данных.");

            string plusEnergyName = Energy.Plus.ShortDescription;
            string minusEnergyName = Energy.Minus.ShortDescription;

            var fiders = _BalanceItemsEnergy.Where(i => i.Key.ElementType == ElementTypes.FIDER).ToList();
            var powerTransformers = _BalanceItemsEnergy.Where(i => i.Key.ElementType == ElementTypes.POWERTRANSFORMER).ToList();
            var auxiliary = _BalanceItemsEnergy.Where(i => i.Key.ElementType == ElementTypes.UNITTRANSFORMERBUS).ToList();

            FidersCount = fiders.Count;
            TransformersCount = powerTransformers.Count;
            AuxCount = auxiliary.Count;

            #region | Заголовки |

            Headers = new List<HeaderElement>
                {
                    new HeaderElement { Name = "Дата", Code = null },
                    new HeaderElement { Name = "Поступление по вводам", Code = null },
                    new HeaderElement { Name = "Отдача по вводам", Code = null }
                };
            if (auxiliary.Count != 0)
                Headers.Add(new HeaderElement { Name = "ТСНш", Code = null });
            Headers.Add(new HeaderElement { Name = "Поступление по фидерам", Code = null });
            Headers.Add(new HeaderElement { Name = "Отдача по фидерам", Code = null });
            Headers.Add(new HeaderElement { Name = "Небаланс, кВт∙ч", Code = null });
            Headers.Add(new HeaderElement { Name = "Небаланс, %", Code = null });
            foreach (var t in powerTransformers)
            {
                Headers.Add(new HeaderElement { Name = GetTitle(t.Key.Name) + "\n" + plusEnergyName, Code = t.Key.Code });
                Headers.Add(new HeaderElement { Name = GetTitle(t.Key.Name) + "\n" + minusEnergyName, Code = t.Key.Code });
            }
            if (auxiliary.Count != 0)
                foreach (var a in auxiliary)
                {
                    Headers.Add(new HeaderElement { Name = GetTitle(a.Key.Name) + "\n" + plusEnergyName, Code = a.Key.Code });
                }
            foreach (var f in fiders)
            {
                Headers.Add(new HeaderElement { Name = GetTitle(f.Key.Name) + "\n" + plusEnergyName, Code = f.Key.Code });
                Headers.Add(new HeaderElement { Name = GetTitle(f.Key.Name) + "\n" + minusEnergyName, Code = f.Key.Code });
            }
            #endregion
            #region | Данные |
            Items = new List<DayBalance>(daysCount);

            for (int i = 0; i < daysCount; i++)
            {
                var day = new DayBalance
                {
                    Date = Period.StartDate.AddDays(i).ToString("dd.MM.yyyy"),

                    VvodaIn = GetBalanceItemDayEnergyValue(i, powerTransformers, BalanceGroup.Formula.TransformersEnergyInDirection),
                    VvodaOut = GetBalanceItemDayEnergyValue(i, powerTransformers, BalanceGroup.Formula.TransformersEnergyOutDirection),

                    Tsn = GetBalanceItemDayEnergyValue(i, auxiliary, BalanceGroup.Formula.UnitTransformersEnergyInDirection),

                    FideraIn = GetBalanceItemDayEnergyValue(i, fiders, BalanceGroup.Formula.FidersEnergyInDirection),
                    FideraOut = GetBalanceItemDayEnergyValue(i, fiders, BalanceGroup.Formula.FidersEnergyOutDirection)
                };
                day.Unbalance = (day.VvodaIn + day.FideraIn) - (day.VvodaOut + day.FideraOut + (day.Tsn == null ? 0.0 : day.Tsn));
                day.PercentageOfUnbalance = (day.VvodaIn + day.FideraIn) == 0.0 ? null : 100.0 * day.Unbalance / (day.VvodaIn + day.FideraIn);

                day.Transformers = new List<double?>();
                foreach (var t in powerTransformers)
                {
                    day.Transformers.Add(t.Value.Plus.DaysValues?[i]);
                    day.Transformers.Add(t.Value.Minus.DaysValues?[i]);
                }
                if (auxiliary.Count != 0)
                {
                    day.Auxiliary = new List<double?>();
                    foreach (var a in auxiliary)
                        day.Auxiliary.Add(a.Value.Plus.DaysValues?[i]);
                }

                day.Fiders = new List<double?>();
                foreach (var f in fiders)
                {
                    day.Fiders.Add(f.Value.Plus.DaysValues?[i]);
                    day.Fiders.Add(f.Value.Minus.DaysValues?[i]);
                }

                Items.Add(day);
            }
            #endregion
            #region | Минимумы, максимумы и т.д. |
            //
            Min = new List<double?>();
            Max = new List<double?>();
            Average = new List<double?>();
            Sum = new List<double?>();
            // пропуск столбца с датой
            Min.Add(null);
            Max.Add(null);
            Average.Add(null);
            Sum.Add(null);

            Action<Func<DayBalance, double?>> action = (selector) =>
            {
                Min.Add(Items.Min(selector));
                Max.Add(Items.Max(selector));
                Average.Add(Items.Average(selector));
                Sum.Add(Items.Sum(selector));
            };
            // Поступление по вводам
            action(t => t.VvodaIn ?? 0.0);
            // Отдача по вводам
            action(t => t.VvodaOut ?? 0.0);
            // ТСНш
            if (auxiliary.Count != 0)
                action(t => t.Tsn ?? 0.0);
            // Поступление по фидерам
            action(t => t.FideraIn ?? 0.0);
            // Отдача по фидерам
            action(t => t.FideraOut ?? 0.0);
            // Небаланс, кВт∙ч
            action(t => t.Unbalance ?? 0.0);
            // Небаланс, %
            action(t => t.PercentageOfUnbalance ?? 0.0);
            // 
            foreach (var t in powerTransformers)
            {
                Min.Add(t.Value.Plus.DaysValuesMin);
                Max.Add(t.Value.Plus.DaysValuesMax);
                Average.Add(t.Value.Plus.DaysValuesAverage);
                Sum.Add(t.Value.Plus.SummOfDaysValue);

                Min.Add(t.Value.Minus.DaysValuesMin);
                Max.Add(t.Value.Minus.DaysValuesMax);
                Average.Add(t.Value.Minus.DaysValuesAverage);
                Sum.Add(t.Value.Minus.SummOfDaysValue);
            }
            if (auxiliary.Count != 0)
                foreach (var a in auxiliary)
                {
                    Min.Add(a.Value.Plus.DaysValuesMin);
                    Max.Add(a.Value.Plus.DaysValuesMax);
                    Average.Add(a.Value.Plus.DaysValuesAverage);
                    Sum.Add(a.Value.Plus.SummOfDaysValue);
                }
            //
            foreach (var f in fiders)
            {
                Min.Add(f.Value.Plus.DaysValuesMin);
                Max.Add(f.Value.Plus.DaysValuesMax);
                Average.Add(f.Value.Plus.DaysValuesAverage);
                Sum.Add(f.Value.Plus.SummOfDaysValue);

                Min.Add(f.Value.Minus.DaysValuesMin);
                Max.Add(f.Value.Minus.DaysValuesMax);
                Average.Add(f.Value.Minus.DaysValuesAverage);
                Sum.Add(f.Value.Minus.SummOfDaysValue);
            }
            #endregion
        }

        #endregion

        #region Properties
        /// <summary>
        /// Ссылка на подстанцию
        /// </summary>
        public Substation Substation => BalanceGroup is Substation ? BalanceGroup as Substation : BalanceGroup.Substation;

        /// <summary>
        /// Группа элементов для расчёта баланса
        /// </summary>
        public IBalanceGroupItem BalanceGroup { get; private set; }
        /// <summary>
        /// Выбранный тип энергии
        /// </summary>
        public IEnergy Energy
        {
            get
            {
                return GetEnergyForBalanceItem(BalanceGroup);
            }
        }
        /// <summary>
        /// Приём по вводам
        /// </summary>
        public double? VvodaIn { get { return GetBalanceItemsEnergyValue(ElementTypes.POWERTRANSFORMER, BalanceGroup.Formula.TransformersEnergyInDirection); } }
        /// <summary>
        /// Отдача по вводам
        /// </summary>
        public double? VvodaOut { get { return GetBalanceItemsEnergyValue(ElementTypes.POWERTRANSFORMER, BalanceGroup.Formula.TransformersEnergyOutDirection); } }
        /// <summary>
        /// Приём по фидерам
        /// </summary>
        public double? FideraIn { get { return GetBalanceItemsEnergyValue(ElementTypes.FIDER, BalanceGroup.Formula.FidersEnergyInDirection); } }
        /// <summary>
        /// Отдача по фидерам
        /// </summary>
        public double? FideraOut { get { return GetBalanceItemsEnergyValue(ElementTypes.FIDER, BalanceGroup.Formula.FidersEnergyOutDirection); } }
        /// <summary>
        /// Приём по трансформаторам собственных нужд, подключённым к шинам
        /// </summary>
        public double? TsnIn { get { return GetBalanceItemsEnergyValue(ElementTypes.UNITTRANSFORMERBUS, BalanceGroup.Formula.UnitTransformersEnergyInDirection); } }
        /// <summary>
        /// Отдача по трансформаторам собственных нужд, подключённым к шинам
        /// </summary>
        public double? TsnOut { get { return GetBalanceItemsEnergyValue(ElementTypes.UNITTRANSFORMERBUS, BalanceGroup.Formula.UnitTransformersEnergyOutDirection); } }
        /// <summary>
        /// Приём энергии
        /// </summary>
        public double? EnergyIn
        {
            get
            {
                return (VvodaIn.HasValue == false && FideraIn.HasValue == false)
                    ? new Nullable<double>()
                    : VvodaIn ?? 0d + FideraIn ?? 0d;
            }
        }
        /// <summary>
        /// Отдача энергии
        /// </summary>
        public double? EnergyOut
        {
            get
            {
                return (VvodaOut.HasValue == false && FideraOut.HasValue == false && TsnOut.HasValue == false)
                    ? new Nullable<double>()
                    : VvodaOut ?? 0d + FideraOut ?? 0d + TsnOut ?? 0d;
            }
        }
        /// <summary>
        /// Небаланс
        /// </summary>
        public double? Unbalance
        {
            get
            {
                return (EnergyIn.HasValue == false && EnergyOut.HasValue == false)
                    ? new Nullable<double>()
                    : EnergyIn ?? 0d - EnergyOut ?? 0d;
            }
        }
        /// <summary>
        /// Процент небаланса
        /// </summary>
        public double? PercentageOfUnbalance { get { return (EnergyIn.HasValue == false || Unbalance.HasValue == false || EnergyIn == 0.0) ? null : 100.0 * Unbalance / EnergyIn; } }
        /// <summary>
        /// Допустимый процент небаланса
        /// </summary>
        public double? MaximumAllowableUnbalance
        {
            get
            {
                if (_maximumAllowableUnbalance != null)
                    return _maximumAllowableUnbalance;

                if (_maximumAllowableUnbalanceCalculated == null)
                {
                    if (BalanceGroup.ElementType != ElementTypes.SUBSTATION || BalanceGroup.Children == null)
                        return null;
                    // плоский список
                    var items = BalanceGroup.Children
                        .SelectMany(i => i is IBalanceGroupItem bg ? bg.Children : null)
                        .Select(i => new { Item = i as IBalanceItem, Energy = _BalanceItemsEnergy[i as IBalanceItem] });

                    // погрешность учёта
                    double δсч = 0.5d;
                    // погрешность тр-ра тока
                    double δтт = 0.5d;
                    // погрешность тр-ра напряжения
                    double δтн = 0.5d;
                    // суммарная относительная погрешность
                    double teta = 1.1 * Math.Sqrt(δсч * δсч + δтт * δтт + δтн * δтн);

                    var powertrans = items
                        .Where(i => i.Item.ElementType == ElementTypes.POWERTRANSFORMER)
                        .Select(i => new
                        {
                            i.Item.Name,
                            In = GetEnergyValueForBalanceItem(i.Item, BalanceGroup.Formula.TransformersEnergyInDirection),
                            Out = GetEnergyValueForBalanceItem(i.Item, BalanceGroup.Formula.TransformersEnergyOutDirection)
                        }).ToList();
                    var fiders = items
                        .Where(i => i.Item.ElementType == ElementTypes.FIDER)
                        .Select(i => new
                        {
                            i.Item.Name,
                            In = GetEnergyValueForBalanceItem(i.Item, BalanceGroup.Formula.FidersEnergyInDirection),
                            Out = GetEnergyValueForBalanceItem(i.Item, BalanceGroup.Formula.FidersEnergyOutDirection)
                        }).ToList();
                    var tsn = items
                        .Where(i => (i.Item.ElementType == ElementTypes.UNITTRANSFORMER || i.Item.ElementType == ElementTypes.UNITTRANSFORMERBUS))
                        .Select(i => new
                        {
                            i.Item.Name,
                            In = GetEnergyValueForBalanceItem(i.Item, BalanceGroup.Formula.UnitTransformersEnergyInDirection),
                            Out = GetEnergyValueForBalanceItem(i.Item, BalanceGroup.Formula.UnitTransformersEnergyOutDirection)
                        }).ToList();

                    double energyInFiders = fiders
                        .Sum(i => i.In.HasValue ? i.In.Value / 1000d : 0);
                    double energyInTsn = tsn
                        .Sum(i => i.Out.HasValue ? i.Out.Value / 1000d : 0);
                    double energyInPower = powertrans
                        .Sum(i => i.In.HasValue ? i.In.Value / 1000d : 0);
                    double summEnergyIn = energyInFiders + energyInTsn + energyInPower;

                    double energySqrInPower = powertrans
                        .Sum(i => Math.Pow(i.In.HasValue ? i.In.Value / 1000d : 0, 2));
                    double energySqrInFiders = fiders
                        .Sum(i => Math.Pow(i.In.HasValue ? i.In.Value / 1000d : 0, 2));
                    double energySqrInTsn = tsn
                        .Sum(i => Math.Pow(i.Out.HasValue ? i.Out.Value / 1000d : 0, 2));
                    double summSqrEnergyIn = energySqrInPower + energySqrInFiders + energySqrInTsn;

                    _maximumAllowableUnbalanceCalculated = Math.Sqrt(Math.Pow(teta, 2) * summSqrEnergyIn / Math.Pow(summEnergyIn, 2));
                }
                return _maximumAllowableUnbalanceCalculated;
            }
            set
            {
                SetProp(ref _maximumAllowableUnbalance, value, "MaximumAllowableUnbalance");
            }
        }

        /// <summary>
        /// Превышает ли небаланс 3%
        /// </summary>
        public bool ExcessUnbalance
        {
            get { return Unbalance.HasValue ? Math.Abs(Unbalance.Value) > 3.0 : false; }
        }
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Возвращает описание произведенных корректировок
        /// </summary>
        public string Correction
        {
            get
            {
                if (BalanceGroup.Children == null)
                    return null;
                // плоский список
                IEnumerable<IHierarchicalEmcosPoint> items = BalanceGroup.Children.SelectMany(i => i is IBalanceGroupItem bg ? bg.Children : null);
                if (items == null) return null;
                var sb = new System.Text.StringBuilder();

                var itemsThatHaveCorrection = items
                    .Cast<IBalanceItem>()
                    .Select(i => new { i.Name, Energy = GetEnergyForBalanceItem(i) })
                    .Where(i => i.Energy.Plus.CorrectedValue.HasValue || i.Energy.Minus.CorrectedValue.HasValue);

                int maxTextWidth = itemsThatHaveCorrection.Max(i => i.Name.Length);
                maxTextWidth = maxTextWidth == 0 ? 25 : maxTextWidth;

                string f = String.Format("{{0,-{0}}}\t{{1}}\n", maxTextWidth);

                foreach (var item in itemsThatHaveCorrection)
                {
                    if (item.Energy.Plus.CorrectedValue.HasValue)
                        sb.AppendFormat(f, item.Name, item.Energy.Plus.Correction);
                    if (item.Energy.Minus.CorrectedValue.HasValue)
                        sb.AppendFormat(f, item.Name, item.Energy.Minus.Correction);
                }
                if (sb.Length == 0)
                    return null;
                else
                    return sb.ToString().Trim();
            }
        }
        /// <summary>
        /// Список дочерних элементов группы, у которых имеется разница между суммой суточных и данными за месяц
        /// </summary>
        public string DifferenceBetweenDailySumAndMonthToolTip
        {
            get
            {
                if (BalanceGroup.Children != null)
                {
                    var list = BalanceGroup.Children
                        .Cast<IBalanceItem>()
                        .Where(child => child.ElementType == ElementTypes.FIDER || child.ElementType == ElementTypes.POWERTRANSFORMER || child.ElementType == ElementTypes.UNITTRANSFORMER || child.ElementType == ElementTypes.UNITTRANSFORMERBUS)
                        .Where(child =>
                            {
                                IEnergy energy = GetEnergyForBalanceItem(child);
                                if (energy.Plus.HasDifferenceBetweenDaysSumAndMonth)
                                    return true;
                                if (energy.Minus.HasDifferenceBetweenDaysSumAndMonth)
                                    return true;
                                return false;
                            })
                        .Select(child => child.Name);

                    return string.Join(", ", list);
                }
                else
                    return null;
            }
        }

        #endregion

        #region Properties for day balance view

        [Magic]
        public DatePeriod Period { get; set; }
        [Magic]
        public int TransformersCount { get; set; }
        [Magic]
        public int AuxCount { get; set; }
        [Magic]
        public int FidersCount { get; set; }
        [Magic]
        public IList<HeaderElement> Headers { get; private set; }
        [Magic]
        public IList<DayBalance> Items { get; private set; }
        [Magic]
        public IList<double?> Min { get; private set; }
        [Magic]
        public IList<double?> Max { get; private set; }
        [Magic]
        public IList<double?> Average { get; private set; }
        [Magic]
        public IList<double?> Sum { get; private set; }

        #endregion
    }

    /// <summary>
    /// Структура для представления заголовка таблицы суточных балансов
    /// </summary>
    public struct HeaderElement
    {
        public string Name;
        public string Code;
    }

    /// <summary>
    /// Баланс энергии за сутки
    /// </summary>
    [Magic]
    public class DayBalance : PropertyChangedBase
    {
        public string Date { get; set; }
        public IList<double?> Transformers { get; set; }
        public IList<double?> Auxiliary { get; set; }
        public double? VvodaIn { get; set; }
        public double? VvodaOut { get; set; }
        public double? Tsn { get; set; }
        public double? FideraIn { get; set; }
        public double? FideraOut { get; set; }
        public double? Unbalance { get; set; }
        public double? PercentageOfUnbalance { get; set; }
        public IList<double?> Fiders { get; set; }
    }
}
