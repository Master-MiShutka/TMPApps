namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class FiderAnalizTreeItem : Shared.PropertyChangedBase
    {
        private IList<FiderAnalizMeter> childMeters;
        private FiderAnalizTreeItem parent;
        private FiderAnalizTreeItemType @type;
        private string header;
        private uint? consumption;
        private float? averageConsumption;
        private float? medianConsumption;
        private uint? notBindingAbonentsCount;
        private uint? notBindingAbonentsConsumption;
        private IList<FiderAnalizTreeItem> children = new ObservableCollection<FiderAnalizTreeItem>();
        private bool isExpanded;
        private bool isMatch = true;

        public FiderAnalizTreeItem() { }

        public FiderAnalizTreeItem(FiderAnalizTreeItem parent, string header, FiderAnalizTreeItemType type, IList<FiderAnalizMeter> meters)
        {
            this.parent = parent;
            this.header = header;
            this.type = type;
            this.childMeters = meters;
        }

        public void AddChildren(IEnumerable<FiderAnalizTreeItem> children)
        {
            if (children == null)
            {
                return;
            }

            foreach (FiderAnalizTreeItem child in children)
            {
                this.Children.Add(child);
                child.Parent = this;
            }

            this.CalculateConsumption();
        }

        public static string EmptyHeader => "(пусто)";

        internal IList<FiderAnalizMeter> ChildMeters
        {
            get => this.childMeters;
            set
            {
                if (this.SetProperty(ref this.childMeters, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChildMetersCount));
                    this.RaisePropertyChanged(nameof(this.Consumption));
                }
            }
        }

        internal uint ChildMetersCount => this.ChildMeters != null ? (uint)this.ChildMeters.Count : 0;

        public FiderAnalizTreeItem Parent { get => this.parent; set => this.SetProperty(ref this.parent, value); }

        public FiderAnalizTreeItemType Type { get => this.type; set => this.SetProperty(ref this.type, value); }

        public string Header { get => this.header; set => this.SetProperty(ref this.header, value); }

        public uint? Consumption => this.consumption;

        public float? AverageConsumption => this.averageConsumption;

        public float? MedianConsumption => this.medianConsumption;

        public uint? NotBindingAbonentsCount { get => this.notBindingAbonentsCount; set => this.SetProperty(ref this.notBindingAbonentsCount, value); }

        public uint? NotBindingAbonentsConsumption
        {
            get => this.notBindingAbonentsConsumption;
            set => this.SetProperty(ref this.notBindingAbonentsConsumption, value);
        }

        public IList<FiderAnalizTreeItem> Children
        {
            get => this.children;
            set
            {
                if (this.SetProperty(ref this.children, value))
                {
                    this.RaisePropertyChanged(nameof(this.HasChildren));
                    this.CalculateConsumption();
                }
            }
        }

        public bool HasChildren => this.Children != null && this.Children.Count > 0;

        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (value == this.isExpanded)
                {
                    return;
                }

                this.isExpanded = value;
                if (this.isExpanded)
                {
                    foreach (FiderAnalizTreeItem child in this.Children)
                    {
                        child.IsMatch = true;
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        public bool IsMatch
        {
            get => this.isMatch;
            set
            {
                if (value == this.isMatch)
                {
                    return;
                }

                this.isMatch = value;
                this.RaisePropertyChanged();
            }
        }

        private bool IsCriteriaMatched(string criteria)
        {
            return string.IsNullOrEmpty(criteria) || this.Header.Contains(criteria, AppSettings.StringComparisonMethod);
        }

        public void ApplyCriteria(string criteria, Stack<FiderAnalizTreeItem> ancestors)
        {
            if (ancestors == null)
            {
                return;
            }

            if (this.IsCriteriaMatched(criteria))
            {
                this.IsMatch = true;
                foreach (FiderAnalizTreeItem ancestor in ancestors)
                {
                    ancestor.IsMatch = true;
                    ancestor.IsExpanded = !string.IsNullOrEmpty(criteria);
                }
            }
            else
            {
                this.IsMatch = false;
            }

            ancestors.Push(this);
            foreach (FiderAnalizTreeItem child in this.Children)
            {
                child.ApplyCriteria(criteria, ancestors);
            }

            ancestors.Pop();
        }

        private void CalculateConsumption()
        {
            if (this.HasChildren)
            {
                IList<uint?> values = this.children.Select(child => child.Consumption).ToList();
                this.CalculateConsumption(values);

                var emptyChildren = this.children.Where(i => i.Header == EmptyHeader).ToList();
                int? emptyItemsCount = emptyChildren.Count == 0 ? null : emptyChildren.Count;
                this.notBindingAbonentsConsumption = (uint)emptyChildren.Sum(i => i.Consumption);

                this.RaisePropertyChanged(nameof(this.NotBindingAbonentsCount));
                this.RaisePropertyChanged(nameof(this.NotBindingAbonentsConsumption));
            }
            else
            {
                if (this.ChildMetersCount > 0)
                {
                    IList<uint?> values = this.ChildMeters.Select(child => child.Consumption).ToList();
                    this.CalculateConsumption(values);
                }
                else
                {
                    ;
                }
            }

            this.RaisePropertyChanged(nameof(this.HasChildren));
        }

        internal void CalculateConsumption(IList<uint?> values)
        {
            this.consumption = (uint)values.Sum(i => i ?? 0);
            this.averageConsumption = (uint)values.Average(i => i ?? 0);
            this.medianConsumption = (uint)values.Median();

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
    }

    public struct FiderAnalizMeter
    {
        public ulong Id { get; set; }

        public string Header { get; set; }

        public string Address { get; set; }

        public bool IsDisconnected { get; set; }

        public bool HasAskue { get; set; }

        public uint? Consumption { get; set; }

        public string Substation { get; set; }

        public string Fider10 { get; set; }

        public string Tp { get; set; }

        public string Fider04 { get; set; }

        public FiderAnalizMeter(Meter meter, uint? consumption)
        {
            this.Id = meter.Лицевой;
            this.Header = meter.ФиоСокращ;
            this.Address = meter.НаселённыйПунктИУлицаСНомеромДома;
            this.IsDisconnected = meter.Отключён;
            this.HasAskue = meter.Аскуэ;
            this.Consumption = consumption;

            this.Substation = meter.Подстанция;
            this.Fider10 = meter.Фидер10;
            this.Tp = meter.ТП?.ToString();
            this.Fider04 = meter.Фидер04?.ToString();
        }
    }
}