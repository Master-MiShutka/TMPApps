namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("Type: {Type}, header: {Header}")]
    public class AbonentBindingNode : Shared.Tree.TreeNode
    {
        private NodeType type;
        private ICollection<Meter> meters;
        private int metersCount;
        private int notBindingMetersCount;

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

        public int ChildrenCount => this.MetersCount;

        public AbonentBindingNode() { }

        public AbonentBindingNode(AbonentBindingNode parent, string header, ICollection<Meter> meters, NodeType type)
            : base(parent, header)
        {
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

        protected override void OnChildrenChanged()
        {
            ;
        }
    }
}
