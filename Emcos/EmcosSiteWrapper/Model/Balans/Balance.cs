using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.Model.Balans
{
    public class Balance<T> where T : IEnergy
    {
        #region Fields

        double? _maximumAllowableUnbalance, _maximumAllowableUnbalanceCalculated;

        Dictionary<IBalansItem, IEnergy> _balansItemsEnergy;

        #endregion

        public Balance(IBalansGroup group)
        {
            BalansGroup = group;

            _balansItemsEnergy = new Dictionary<IBalansItem, IEnergy>();
            if (BalansGroup.Children != null)
                foreach (IBalansItem item in BalansGroup.Children)
                    if (_balansItemsEnergy.ContainsKey(item) == false)
                        _balansItemsEnergy.Add(item, GetEnergyForBalansItem(item));
        }

        #region Private methods

        private double? GetBalansItemsEnergyValue(ElementTypes elementType, BalanceFormula.EnergyDirection energyDirection)
        {
            double? result = new Nullable<double>(0);
            if (BalansGroup.Children != null)
            {
                var items = BalansGroup.Children.Where(i => i.ElementType == elementType);
                foreach (IBalansItem item in items)
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

        private IEnergy GetEnergyForBalansItem(IBalansItem balansItem)
        {
            if (balansItem.ActiveEnergy.GetType() == typeof(T))
                return balansItem.ActiveEnergy;
            if (balansItem.ReactiveEnergy.GetType() == typeof(T))
                return balansItem.ReactiveEnergy;
            return null;
        }

        public double? GetEnergyValueForBalansItem(IBalansItem balansItem, BalanceFormula.EnergyDirection energyDirection)
        {
            IEnergy energy = _balansItemsEnergy[balansItem];
            var value = energyDirection == BalanceFormula.EnergyDirection.@in 
                ? energy.Plus.CorrectedValue 
                : energy.Minus.CorrectedValue;
            return value;
        }

        #endregion

        #region Public methods



        #endregion

        #region Properties
        /// <summary>
        /// Группа элементов для расчёта баланса
        /// </summary>
        public IBalansGroup BalansGroup { get; private set; }
        /// <summary>
        /// Выбранный тип энергии
        /// </summary>
        public IEnergy Energy
        {
            get
            {
                return GetEnergyForBalansItem(BalansGroup);
            }
        }
        /// <summary>
        /// Приём по вводам
        /// </summary>
        public double? VvodaIn { get { return GetBalansItemsEnergyValue(ElementTypes.POWERTRANSFORMER, BalansGroup.Formula.TransformersEnergyInDirection); } }
        /// <summary>
        /// Отдача по вводам
        /// </summary>
        public double? VvodaOut { get { return GetBalansItemsEnergyValue(ElementTypes.POWERTRANSFORMER, BalansGroup.Formula.TransformersEnergyOutDirection); } }
        /// <summary>
        /// Приём по фидерам
        /// </summary>
        public double? FideraIn { get { return GetBalansItemsEnergyValue(ElementTypes.FIDER, BalansGroup.Formula.FidersEnergyInDirection); } }
        /// <summary>
        /// Отдача по фидерам
        /// </summary>
        public double? FideraOut { get { return GetBalansItemsEnergyValue(ElementTypes.FIDER, BalansGroup.Formula.FidersEnergyOutDirection); } }
        /// <summary>
        /// Приём по трансформаторам собственных нужд, подключённым к шинам
        /// </summary>
        public double? TsnIn { get { return GetBalansItemsEnergyValue(ElementTypes.UNITTRANSFORMERBUS, BalansGroup.Formula.UnitTransformersEnergyInDirection); } }
        /// <summary>
        /// Отдача по трансформаторам собственных нужд, подключённым к шинам
        /// </summary>
        public double? TsnOut { get { return GetBalansItemsEnergyValue(ElementTypes.UNITTRANSFORMERBUS, BalansGroup.Formula.UnitTransformersEnergyOutDirection); } }
        /// <summary>
        /// Приём энергии
        /// </summary>
        public double? EnergyIn
        {
            get
            {
                return VvodaIn ?? 0d + FideraIn ?? 0d;
            }
        }
        /// <summary>
        /// Отдача энергии
        /// </summary>
        public double? EnergyOut
        {
            get
            {
                return VvodaOut ?? 0d + FideraOut ?? 0d + TsnOut ?? 0d;
            }
        }
        /// <summary>
        /// Небаланс
        /// </summary>
        public double? Unbalance
        {
            get
            {
                return EnergyIn - EnergyOut == 0.0 ? null : EnergyIn - EnergyOut;
            }
        }
        /// <summary>
        /// Процент небаланса
        /// </summary>
        public double? PercentageOfUnbalance { get { return EnergyIn == 0.0 ? null : 100.0 * Unbalance / EnergyIn; } }
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
                    if (BalansGroup.ElementType != ElementTypes.SUBSTATION || BalansGroup.Children == null)
                        return null;
                    // плоский список
                    var items = BalansGroup.Children
                        .SelectMany(i => i is IBalansGroup bg ? bg.Children : null)
                        .Select(i => new { i, Energy = _balansItemsEnergy[i] });

                    // погрешность учёта
                    double δсч = 0.5d;
                    // погрешность тр-ра тока
                    double δтт = 0.5d;
                    // погрешность тр-ра напряжения
                    double δтн = 0.5d;
                    // суммарная относительная погрешность
                    double teta = 1.1 * Math.Sqrt(δсч * δсч + δтт * δтт + δтн * δтн);

                    var powertrans = items
                        .Where(i => i.i.ElementType == ElementTypes.POWERTRANSFORMER)
                        .Select(i => new
                        {
                            i.i.Name,
                            In = GetEnergyValueForBalansItem(i.i, BalansGroup.Formula.TransformersEnergyInDirection),
                            Out = GetEnergyValueForBalansItem(i.i, BalansGroup.Formula.TransformersEnergyOutDirection)
                        }).ToList();
                    var fiders = items
                        .Where(i => i.i.ElementType == ElementTypes.FIDER)
                        .Select(i => new
                        {
                            i.i.Name,
                            In = GetEnergyValueForBalansItem(i.i, BalansGroup.Formula.FidersEnergyInDirection),
                            Out = GetEnergyValueForBalansItem(i.i, BalansGroup.Formula.FidersEnergyOutDirection)
                        }).ToList();
                    var tsn = items
                        .Where(i => (i.i.ElementType == ElementTypes.UNITTRANSFORMER || i.i.ElementType == ElementTypes.UNITTRANSFORMERBUS))
                        .Select(i => new
                        {
                            i.i.Name,
                            In = GetEnergyValueForBalansItem(i.i, BalansGroup.Formula.UnitTransformersEnergyInDirection),
                            Out = GetEnergyValueForBalansItem(i.i, BalansGroup.Formula.UnitTransformersEnergyOutDirection)
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
                if (BalansGroup.Children == null)
                    return null;
                // плоский список
                IEnumerable<IBalansItem> items = BalansGroup.Children.SelectMany(i => i is IBalansGroup bg ? bg.Children : null);
                if (items == null) return null;
                var sb = new System.Text.StringBuilder();

                var itemsThatHaveCorrection = items
                    .Select(i => new { i.Name, Energy = GetEnergyForBalansItem(i) })
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
                if (BalansGroup.Children != null)
                {
                    var list = BalansGroup.Children
                        .Where(child => child.ElementType == ElementTypes.FIDER || child.ElementType == ElementTypes.POWERTRANSFORMER || child.ElementType == ElementTypes.UNITTRANSFORMER || child.ElementType == ElementTypes.UNITTRANSFORMERBUS)
                        .Where(child =>
                            {
                                IEnergy energy = GetEnergyForBalansItem(child);
                                if (energy.Plus.HasDifferenceBetweenDailySumAndMonth)
                                    return true;
                                if (energy.Minus.HasDifferenceBetweenDailySumAndMonth)
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
    }
}
