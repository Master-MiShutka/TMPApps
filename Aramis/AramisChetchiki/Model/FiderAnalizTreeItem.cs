namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;

    public class FiderAnalizTreeItem : Shared.Tree.TreeNode
    {
        private IList<FiderAnalizMeter> childMeters;
        private FiderAnalizTreeItemType @type;
        private uint? consumption;
        private float? averageConsumption;
        private float? medianConsumption;
        private uint? notBindingAbonentsCount;
        private uint? notBindingAbonentsConsumption;

        public FiderAnalizTreeItem() { }

        public FiderAnalizTreeItem(FiderAnalizTreeItem parent, string header, FiderAnalizTreeItemType type, IList<FiderAnalizMeter> meters)
            : base(parent, header)
        {
            this.type = type;
            this.childMeters = meters;

            this.CalculateConsumption();
        }

        public void AddChildren(IEnumerable<FiderAnalizTreeItem> children)
        {
            if (children == null)
            {
                return;
            }

            foreach (Shared.Tree.ITreeNode child in children)
            {
                this.Children.Add(child);
                child.Parent = this;
            }

            this.RaisePropertyChanged(nameof(this.HasChildren));

            this.CalculateConsumption();
        }

        public static string EmptyHeader => "(пусто)";

        public IList<FiderAnalizMeter> ChildMeters
        {
            get => this.childMeters;
            private set
            {
                if (this.SetProperty(ref this.childMeters, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChildMetersCount));
                    this.RaisePropertyChanged(nameof(this.PercentOfPermanentResidence));
                    this.RaisePropertyChanged(nameof(this.PercentOfSeasonalResidence));
                    this.RaisePropertyChanged(nameof(this.PercentOfPermanentResidenceWhichHasPayment));
                }
            }
        }

        public uint ChildMetersCount => this.ChildMeters != null ? (uint)this.ChildMeters.Count : 0;

        public FiderAnalizTreeItemType Type { get => this.type; set => this.SetProperty(ref this.type, value); }

        public uint? Consumption => this.consumption;

        public float? AverageConsumption => this.averageConsumption;

        public float? MedianConsumption => this.medianConsumption;

        public uint? NotBindingAbonentsCount { get => this.notBindingAbonentsCount; set => this.SetProperty(ref this.notBindingAbonentsCount, value); }

        public uint? NotBindingAbonentsConsumption
        {
            get => this.notBindingAbonentsConsumption;
            set => this.SetProperty(ref this.notBindingAbonentsConsumption, value);
        }

        public double PercentOfPermanentResidence => 100d * this.childMeters.Count(i => i.IsPermanentResidence) / this.ChildMetersCount;

        public double PercentOfSeasonalResidence => 100d * this.childMeters.Count(i => i.IsSeasonalResidence) / this.ChildMetersCount;

        public double PercentOfPermanentResidenceWhichHasPayment
        {
            get
            {
                var list = this.childMeters.Where(i => i.Consumption != null && i.IsPermanentResidence).ToList();
                var count = list.Count;
                var totalCount = this.childMeters.Count(i => i.IsPermanentResidence);

                return 100d * count / (totalCount == 0 ? 1_000_000 : totalCount) ;
            }
        }

        protected override void OnChildrenChanged()
        {
            this.CalculateConsumption();
        }

        private void CalculateConsumption()
        {
            if (this.ChildMetersCount > 0)
            {
                IList<uint?> values = this.ChildMeters.Select(child => child.Consumption).ToList();
                this.CalculateConsumption(values);
            }

            var emptyChildren = this.Children.Where(i => i.Header == EmptyHeader).ToList();
            if (emptyChildren.Any())
            {
                IList<FiderAnalizMeter> meters = emptyChildren.Cast<FiderAnalizTreeItem>().SelectMany(i => i.ChildMeters).ToList();

                IList<uint?> values = meters.Select(meter => meter.Consumption).ToList();

                this.notBindingAbonentsConsumption = (uint)values.Sum(i => i ?? 0);
                this.notBindingAbonentsCount = (uint)values.Count;

                this.RaisePropertyChanged(nameof(this.NotBindingAbonentsCount));
                this.RaisePropertyChanged(nameof(this.NotBindingAbonentsConsumption));
            }

            this.RaisePropertyChanged(nameof(this.HasChildren));
        }

        internal void CalculateConsumption(IList<uint?> values)
        {
            this.consumption = (uint)values.Sum(i => i ?? 0);
            this.averageConsumption = (uint)values.Average(i => i ?? 0);
            this.medianConsumption = values.Any(i => i != null) ? (uint)values.Median() : 0;

            this.RaisePropertyChanged(nameof(this.Consumption));
            this.RaisePropertyChanged(nameof(this.AverageConsumption));
            this.RaisePropertyChanged(nameof(this.MedianConsumption));
        }
    }

    public enum FiderAnalizTreeItemType
    {
        Substation,
        Fider10,
        TP,
        Fider04,
        Group,
        None,
    }

    public struct FiderAnalizMeter
    {
        internal string TownType { get; set; }

        [DisplayName("Лицевой счёт абонента")]
        [PersonalAccountDataFormat]
        public ulong Id { get; set; }

        [DisplayName("Ф.И.О.")]
        public string Header { get; set; }

        [DisplayName("Населённый пункт")]
        public string Town { get; set; }

        [DisplayName("Адрес")]
        public string Address { get; set; }

        [DisplayName("Отключён")]
        public bool IsDisconnected { get; set; }

        [DisplayName("Есть АСКУЭ")]
        public bool HasAskue { get; set; }

        [DisplayName("Потребление, кВт∙ч")]
        public uint? Consumption { get; set; }

        [DisplayName("Подстанция")]
        public string Substation { get; set; }

        [DisplayName("Фидер 10 кВ")]
        public string Fider10 { get; set; }

        [DisplayName("ТП 10/0,4 кВ")]
        public string Tp { get; set; }

        [DisplayName("Фидер 0,4 кВ")]
        public string Fider04 { get; set; }

        [DisplayName("Постоянное проживание")]
        public bool IsPermanentResidence { get; set; }

        [DisplayName("Сезонное проживание")]
        public bool IsSeasonalResidence { get; set; }

        public FiderAnalizMeter(Meter meter, uint? consumption)
        {
            this.Id = meter.Лицевой;
            this.Header = meter.ФиоСокращ;
            this.Town = meter.НаселённыйПункт;
            this.TownType = meter.ТипНаселённойМестности;
            this.Address = meter.УлицаСДомомИКв;
            this.IsDisconnected = meter.Отключён;
            this.HasAskue = meter.Аскуэ;
            this.Consumption = consumption;

            this.Substation = meter.Подстанция;
            this.Fider10 = meter.Фидер10;
            this.Tp = meter.ТП?.ToString();
            this.Fider04 = meter.Фидер04?.ToString();

            this.IsPermanentResidence = meter.Использование == "Постоянное";
            this.IsSeasonalResidence = meter.Использование == "Сезонное";
        }
    }
}