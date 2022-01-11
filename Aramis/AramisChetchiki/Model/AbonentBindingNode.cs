namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class AbonentBindingNode : Shared.PropertyChangedBase
    {
        private bool isExpanded;
        private bool isMatch = true;
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
                if (value == this.header)
                {
                    return;
                }

                this.header = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.HasEmptyValue));
            }
        }

        public NodeType Type
        {
            get => this.type;
            set
            {
                if (value == this.type)
                {
                    return;
                }

                this.type = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.Icon));
            }
        }

        public ICollection<Meter> Meters { get => this.meters; set => this.SetProperty(ref this.meters, value); }

        public int MetersCount
        {
            get => this.metersCount;
            set
            {
                if (value == this.metersCount)
                {
                    return;
                }

                this.metersCount = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.ChildrenCount));
            }
        }

        public int NotBindingMetersCount
        {
            get => this.notBindingMetersCount;
            set
            {
                if (value == this.notBindingMetersCount)
                {
                    return;
                }

                this.notBindingMetersCount = value;
                this.RaisePropertyChanged();
            }
        }

        public bool HasEmptyValue => string.IsNullOrWhiteSpace(this.Header);

        public AbonentBindingNode Parent
        {
            get => this.parent;
            set
            {
                if (ReferenceEquals(value, this.parent))
                {
                    return;
                }

                this.parent = value;
                this.RaisePropertyChanged();
            }
        }

        public AbonentBindingNode()
        {
            this.Children = new System.Collections.ObjectModel.ObservableCollection<AbonentBindingNode>();
        }

        public AbonentBindingNode(AbonentBindingNode parent, string header, ICollection<Meter> meters, NodeType type) : this()
        {
            this.parent = parent;
            this.header = header;
            this.Meters = meters;
            this.type = type;
            this.notBindingMetersCount = this.metersCount = this.Meters.Count;
        }

        public System.Collections.ObjectModel.ObservableCollection<AbonentBindingNode> Children { get; }

        public void AddChildren(IEnumerable<AbonentBindingNode> children)
        {
            if (children == null)
            {
                return;
            }

            foreach (var child in children)
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

            foreach (var meter in meters)
            {
                this.Meters.Add(meter);
            }

            this.NotBindingMetersCount = this.MetersCount = this.Meters.Count;
        }

        public object Icon => this.Type switch
        {
            NodeType.Substation => Application.Current.TryFindResource("IconSubstation"),
            NodeType.Fider10 => Application.Current.TryFindResource("IconFider10"),
            NodeType.TP => Application.Current.TryFindResource("IconTp"),
            NodeType.Fider04 => Application.Current.TryFindResource("IconFider04"),
            NodeType.Group => Application.Current.TryFindResource("IconGroup"),
            _ => Application.Current.TryFindResource("IconDepartament"),
        };

        public int ChildrenCount => this.MetersCount;

        public enum NodeType
        {
            Substation,
            Fider10,
            TP,
            Fider04,
            Group,
            Departament,
        }

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
                    foreach (var child in this.Children)
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

        public void ApplyCriteria(string criteria, Stack<AbonentBindingNode> ancestors)
        {
            if (ancestors == null)
            {
                return;
            }

            if (this.IsCriteriaMatched(criteria))
            {
                this.IsMatch = true;
                foreach (var ancestor in ancestors)
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
            foreach (var child in this.Children)
            {
                child.ApplyCriteria(criteria, ancestors);
            }

            ancestors.Pop();
        }

        private string GetDebuggerDisplay()
        {
            return $"Type: {this.Type}, header: {this.Header}";
        }
    }
}
