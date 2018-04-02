using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    public class BalansGrop : PropertyChangedBase
    {
        private Balans.IBalansGroup _group;
        public BalansGrop(Balans.IBalansGroup group, DatePeriod period)
        {
            if (group is Balans.SubstationSection || group is Balans.Substation)
            {
                _group = group;

                if (group is Balans.Substation)
                    Substation = group as Balans.Substation;
                else
                    Substation = group.Substation;

                _period = period;
                UpdateSubstationData();

                if (group.Children == null)
                    return;

                int daysCount = period.EndDate.Subtract(period.StartDate).Days + 1;

                //if (group.DailyEminus.Count != daysCount)
                //    throw new ArgumentOutOfRangeException("Количество дней между датой начала и датой окончания периода отличается от количества данных.");

                IList<Balans.IBalansItem> fiders = group.Items.Where(i => i.Type == ElementTypes.FIDER).ToList();
                IList<Balans.IBalansItem> powerTransformers = group.Items.Where(i => i.Type == ElementTypes.POWERTRANSFORMER).ToList();
                IList<Balans.IBalansItem> auxiliary = group.Items.Where(i => i.Type == ElementTypes.UNITTRANSFORMERBUS).ToList();

                FidersCount = fiders.Count;
                TransformersCount = powerTransformers.Count;
                AuxCount = auxiliary.Count;

                #region | Заголовки |
                Headers = new List<HeaderElement>();
                Headers.Add(new HeaderElement { Name="Дата", Code = null });             
                Headers.Add(new HeaderElement { Name = "Поступление по вводам", Code = null });
                Headers.Add(new HeaderElement { Name = "Отдача по вводам", Code = null });
                if (AuxCount != 0)
                    Headers.Add(new HeaderElement { Name = "ТСНш", Code = null });
                Headers.Add(new HeaderElement { Name = "Поступление по фидерам", Code = null });
                Headers.Add(new HeaderElement { Name = "Отдача по фидерам", Code = null });
                Headers.Add(new HeaderElement { Name = "Небаланс, кВт∙ч", Code = null });
                Headers.Add(new HeaderElement { Name = "Небаланс, %", Code = null });
                foreach (var t in powerTransformers)
                {
                    Headers.Add(new HeaderElement { Name = GetTitle(t.Name) + "\nE+", Code = t.Code });
                    Headers.Add(new HeaderElement { Name = GetTitle(t.Name) + "\nE-", Code = t.Code });
                }
                if (AuxCount != 0)
                    foreach (var a in auxiliary)
                    {
                        Headers.Add(new HeaderElement { Name = GetTitle(a.Name) + "\nE+", Code = a.Code });
                    }
                foreach (var f in fiders)
                {
                    Headers.Add(new HeaderElement { Name = GetTitle(f.Name) + "\nE+", Code = f.Code });
                    Headers.Add(new HeaderElement { Name = GetTitle(f.Name) + "\nE-", Code = f.Code });
                }
                #endregion
                #region | Данные |
                Items = new List<BalansGroupItem>(daysCount);

                for (int i = 0; i < daysCount; i++)
                {
                    var bgi = new BalansGroupItem();
                    bgi.Date = period.StartDate.AddDays(i).ToString("dd.MM.yyyy");

                    bgi.VvodaIn = powerTransformers.Sum(t => t.DailyEplus != null ? t.DailyEplus[i] : null);
                    bgi.VvodaOut = powerTransformers.Sum(t => t.DailyEminus != null ? t.DailyEminus[i] : null);

                    bgi.Tsn = auxiliary.Sum(a => a.DailyEplus != null ? a.DailyEplus[i] : null);

                    bgi.FideraIn = fiders.Sum(f => f.DailyEplus != null ? f.DailyEplus[i] : null);
                    bgi.FideraOut = fiders.Sum(f => f.DailyEminus != null ? f.DailyEminus[i] : null);

                    bgi.Unbalance = (bgi.VvodaIn + bgi.FideraIn) - (bgi.VvodaOut + bgi.FideraOut + (bgi.Tsn == null ? 0.0 : bgi.Tsn));
                    bgi.PercentageOfUnbalance = (bgi.VvodaIn + bgi.FideraIn) == 0.0 ? null : 100.0 * bgi.Unbalance / (bgi.VvodaIn + bgi.FideraIn);

                    bgi.Transformers = new List<double?>();
                    foreach (var t in powerTransformers)
                    {
                        bgi.Transformers.Add(t.DailyEplus != null ? t.DailyEplus[i] : null);
                        bgi.Transformers.Add(t.DailyEminus != null ? t.DailyEminus[i] : null);
                    }
                    if (AuxCount != 0)
                    {
                        bgi.Auxiliary = new List<double?>();
                        foreach (var a in auxiliary)
                            bgi.Auxiliary.Add(a.DailyEplus != null ? a.DailyEplus[i] : null);
                    }

                    bgi.Fiders = new List<double?>();
                    foreach (var f in fiders)
                    {
                        bgi.Fiders.Add(f.DailyEplus != null ? f.DailyEplus[i] : null);
                        bgi.Fiders.Add(f.DailyEminus != null ? f.DailyEminus[i] : null);
                    }

                    Items.Add(bgi);
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
                // Поступление по вводам
                Min.Add(Items.Min(t => t.VvodaIn.HasValue ? t.VvodaIn.Value : 0.0));
                Max.Add(Items.Max(t => t.VvodaIn.HasValue ? t.VvodaIn.Value : 0.0));
                Average.Add(Items.Average(t => t.VvodaIn.HasValue ? t.VvodaIn.Value : 0.0));
                Sum.Add(Items.Sum(t => t.VvodaIn.HasValue ? t.VvodaIn.Value : 0.0));
                // Отдача по вводам
                Min.Add(Items.Min(t => t.VvodaOut.HasValue ? t.VvodaOut.Value : 0.0));
                Max.Add(Items.Max(t => t.VvodaOut.HasValue ? t.VvodaOut.Value : 0.0));
                Average.Add(Items.Average(t => t.VvodaOut.HasValue ? t.VvodaOut.Value : 0.0));
                Sum.Add(Items.Sum(t => t.VvodaOut.HasValue ? t.VvodaOut.Value : 0.0));
                // ТСНш
                if (AuxCount != 0)
                {
                    Min.Add(Items.Min(t => t.Tsn.HasValue ? t.Tsn.Value : 0.0));
                    Max.Add(Items.Max(t => t.Tsn.HasValue ? t.Tsn.Value : 0.0));
                    Average.Add(Items.Average(t => t.Tsn.HasValue ? t.Tsn.Value : 0.0));
                    Sum.Add(Items.Sum(t => t.Tsn.HasValue ? t.Tsn.Value : 0.0));
                }
                // Поступление по фидерам
                Min.Add(Items.Min(t => t.FideraIn.HasValue ? t.FideraIn.Value : 0.0));
                Max.Add(Items.Max(t => t.FideraIn.HasValue ? t.FideraIn.Value : 0.0));
                Average.Add(Items.Average(t => t.FideraIn.HasValue ? t.FideraIn.Value : 0.0));
                Sum.Add(Items.Sum(t => t.FideraIn.HasValue ? t.FideraIn.Value : 0.0));
                // Отдача по фидерам
                Min.Add(Items.Min(t => t.FideraOut.HasValue ? t.FideraOut.Value : 0.0));
                Max.Add(Items.Max(t => t.FideraOut.HasValue ? t.FideraOut.Value : 0.0));
                Average.Add(Items.Average(t => t.FideraOut.HasValue ? t.FideraOut.Value : 0.0));
                Sum.Add(Items.Sum(t => t.FideraOut.HasValue ? t.FideraOut.Value : 0.0));
                // Небаланс, кВт∙ч
                Min.Add(Items.Min(t => t.Unbalance.HasValue ? t.Unbalance.Value : 0.0));
                Max.Add(Items.Max(t => t.Unbalance.HasValue ? t.Unbalance.Value : 0.0));
                Average.Add(Items.Average(t => t.Unbalance.HasValue ? t.Unbalance.Value : 0.0));
                Sum.Add(Items.Sum(t => t.Unbalance.HasValue ? t.Unbalance.Value : 0.0));
                // Небаланс, %
                Min.Add(Items.Min(t => t.PercentageOfUnbalance.HasValue ? t.PercentageOfUnbalance.Value : 0.0));
                Max.Add(Items.Max(t => t.PercentageOfUnbalance.HasValue ? t.PercentageOfUnbalance.Value : 0.0));
                Average.Add(Items.Average(t => t.PercentageOfUnbalance.HasValue ? t.PercentageOfUnbalance.Value : 0.0));
                Sum.Add(Items.Sum(t => t.PercentageOfUnbalance.HasValue ? t.PercentageOfUnbalance.Value : 0.0));
                // 
                foreach (var t in powerTransformers)
                {
                    Min.Add(t.DailyEplusValuesMin);
                    Max.Add(t.DailyEplusValuesMax);
                    Average.Add(t.DailyEplusValuesAverage);
                    Sum.Add(t.DailyEplusValuesSum);

                    Min.Add(t.DailyEminusValuesMin);
                    Max.Add(t.DailyEminusValuesMax);
                    Average.Add(t.DailyEminusValuesAverage);
                    Sum.Add(t.DailyEminusValuesSum);
                }
                if (AuxCount != 0)
                    foreach (var a in auxiliary)
                    {
                        Min.Add(a.DailyEplusValuesMin);
                        Max.Add(a.DailyEplusValuesMax);
                        Average.Add(a.DailyEplusValuesAverage);
                        Sum.Add(a.DailyEplusValuesSum);
                    }
                //
                foreach (var f in fiders)
                {
                    Min.Add(f.DailyEplusValuesMin);
                    Max.Add(f.DailyEplusValuesMax);
                    Average.Add(f.DailyEplusValuesAverage);
                    Sum.Add(f.DailyEplusValuesSum);

                    Min.Add(f.DailyEminusValuesMin);
                    Max.Add(f.DailyEminusValuesMax);
                    Average.Add(f.DailyEminusValuesAverage);
                    Sum.Add(f.DailyEminusValuesSum);
                }
                #endregion
            }
            else
                throw new ArgumentException("Ожидалась секция или подстанция");
        }

        public void UpdateSubstationData()
        {
            SubstationName = _group.Name;
            SubstationVvodaIn = _group.VvodaIn;
            SubstationVvodaOut = _group.VvodaOut;
            SubstationFideraIn = _group.FideraIn;
            SubstationFideraOut = _group.FideraOut;
            SubstationTsn = _group.Tsn;
            SubstationUnbalance = _group.Unbalance;
            SubstationPercentageOfUnbalance = _group.PercentageOfUnbalance;
        }

        private DatePeriod _period;
        public DatePeriod Period
        {
            get { return _period; }
            set { _period = value; RaisePropertyChanged("Period"); }
        }
        public Model.Balans.Substation Substation { get; private set; }
        public string SubstationName { get; private set; }
        public double? SubstationVvodaIn { get; private set; }
        public double? SubstationFideraIn { get; private set; }
        public double? SubstationVvodaOut { get; private set; }
        public double? SubstationFideraOut { get; private set; }
        public double? SubstationTsn { get; private set; }
        public double? SubstationUnbalance { get; private set; }
        public double? SubstationPercentageOfUnbalance { get; private set; }

        public int TransformersCount { get; private set; }
        public int AuxCount { get; private set; }
        public int FidersCount { get; private set; }

        public IList<HeaderElement> Headers { get; private set; }
        public IList<BalansGroupItem> Items { get; private set; }

        public IList<double?> Min { get; private set; }
        public IList<double?> Max { get; private set; }
        public IList<double?> Average { get; private set; }
        public IList<double?> Sum { get; private set; }

        private string GetTitle(string title)
        {
            title = title.Replace("10кВ ввод ", "").Replace("6кВ ввод ", "").Replace("0,4кВ ", "");
            int pos = title.IndexOf(',');
            if (pos >0 && title.Length - pos > 2)
            {
                title = title.Substring(pos + 1);
            }
            return title;
        }

        public struct HeaderElement
        {
            public string Name;
            public string Code;
        }
    }

    public class BalansGroupItem : PropertyChangedBase
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
