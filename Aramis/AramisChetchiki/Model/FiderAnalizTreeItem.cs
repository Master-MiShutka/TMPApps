namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class FiderAnalizTreeItem : Shared.PropertyChangedBase
    {
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

        public FiderAnalizTreeItem(FiderAnalizTreeItem parent, string header, IList<Meter> meters, FiderAnalizTreeItemType type)
        {
            this.parent = parent;
            this.header = header;
            this.ChildMeters = meters;
            this.type = type;
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
        }

        internal IList<Meter> ChildMeters { get; set; }

        internal uint ChildMetersCount { get; set; }

        public FiderAnalizTreeItem Parent { get => this.parent; set => this.SetProperty(ref this.parent, value); }

        public FiderAnalizTreeItemType Type { get => this.type; set => this.SetProperty(ref this.type, value); }

        public string Header { get => this.header; set => this.SetProperty(ref this.header, value); }

        public uint? Consumption { get => this.consumption; set => this.SetProperty(ref this.consumption, value); }

        public float? AverageConsumption { get => this.averageConsumption; set => this.SetProperty(ref this.averageConsumption, value); }

        public float? MedianConsumption { get => this.medianConsumption; set => this.SetProperty(ref this.medianConsumption, value); }

        public uint? NotBindingAbonentsCount { get => this.notBindingAbonentsCount; set => this.SetProperty(ref this.notBindingAbonentsCount, value); }

        public uint? NotBindingAbonentsConsumption { get => this.notBindingAbonentsConsumption; set => this.SetProperty(ref this.notBindingAbonentsConsumption, value); }

        public IList<FiderAnalizTreeItem> Children
        {
            get => this.children; set
            {
                if (this.SetProperty(ref this.children, value))
                {
                    this.RaisePropertyChanged(nameof(this.HasChildren));
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
    }

    public enum FiderAnalizTreeItemType
    {
        Substation,
        Fider10,
        TP,
        Fider04,
        Group,
    }
}
