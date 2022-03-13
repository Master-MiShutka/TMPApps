namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("Type: {Type}, header: {Header}")]
    public class AbonentBindingNode : Shared.PropertyChangedBase
    {
        private string header;
        private NodeType type;
        private ICollection<Meter> meters;
        private int metersCount;
        private int notBindingMetersCount;
        private AbonentBindingNode parent;

        public string Header
        {
            get => this.header;
            set
            {
                if (this.SetProperty(ref this.header, value))
                {
                    this.RaisePropertyChanged(nameof(this.HasEmptyValue));
                }
            }
        }

        public NodeType Type { get => this.type; set => this.SetProperty(ref this.type, value); }

        public ICollection<Meter> Meters { get => this.meters; set => this.SetProperty(ref this.meters, value); }

        public int MetersCount
        {
            get => this.metersCount;
            set
            {
                if (this.SetProperty(ref this.metersCount, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChildrenCount));
                }
            }
        }

        public int NotBindingMetersCount { get => this.notBindingMetersCount; set => this.SetProperty(ref this.notBindingMetersCount, value); }

        public bool HasEmptyValue => string.IsNullOrWhiteSpace(this.Header);

        public AbonentBindingNode Parent { get => this.parent; set => this.SetProperty(ref this.parent, value); }

        public System.Collections.ObjectModel.ObservableCollection<AbonentBindingNode> Children { get; } = new System.Collections.ObjectModel.ObservableCollection<AbonentBindingNode>();

        public int ChildrenCount => this.MetersCount;

        public AbonentBindingNode() { }

        public AbonentBindingNode(AbonentBindingNode parent, string header, ICollection<Meter> meters, NodeType type)
        {
            this.parent = parent;
            this.header = header;
            this.meters = meters;
            this.type = type;
            this.notBindingMetersCount = this.metersCount = this.Meters.Count;
        }

        public void AddChildren(IEnumerable<AbonentBindingNode> children)
        {
            if (children == null)
            {
                return;
            }

            foreach (AbonentBindingNode child in children)
            {
                this.Children.Add(child);
                child.Parent = this;
            }
        }

        public void AddNotBindings(ICollection<Meter> meters)
        {
            if (meters == null || this.Meters == null)
            {
                throw new System.InvalidOperationException();
            }

            foreach (Meter meter in meters)
            {
                this.Meters.Add(meter);
            }

            this.NotBindingMetersCount = this.MetersCount = this.Meters.Count;
        }
    }
}
