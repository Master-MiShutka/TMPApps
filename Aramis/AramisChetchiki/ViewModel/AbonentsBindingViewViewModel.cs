namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Data;
    using System.Windows.Input;
    using TMP.Extensions;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;

    public class AbonentsBindingViewViewModel : BaseMeterViewModel
    {
        private bool isVisualizing;
        private ObservableCollection<AbonentBindingNode> abonentBindingNodes;
        private AbonentBindingNode selectedAbonentBindingNode;
        private ICollection<TreeMapItem> treeMapItems;
        private string abonentBondingFilter;

        public AbonentsBindingViewViewModel()
        {
            if (this.Data == null)
            {
                throw new ArgumentNullException("Meters collection");
            }

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                this.IsBusy = false;
                return;
            }

            this.SelectedViewKind = TableViewKinds.ПривязкаView;

            this.IsBusy = true;
            this.Status = "Подоговка данных ...";

            System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(() =>
            {
                const string empty = "(пусто)";

                IList<AbonentBindingNode> groupMetersByProperty(IEnumerable<Meter> metersList, string propName, AbonentBindingNode.NodeType nodeType)
                {
                    // группируем список по значению свойства
                    List<IGrouping<object, Meter>> groups = metersList
                        .GroupBy(i => ModelHelper.MeterGetPropertyValue(i, propName))
                        .ToList();
                    if (groups.Count >= 1)
                    {
                        List<AbonentBindingNode> abonentBindingNodes = new();
                        foreach (IGrouping<object, Meter> group in groups)
                        {
                            AbonentBindingNode abn = new();
                            string header = group.Key == null || string.IsNullOrWhiteSpace(group.Key.ToString()) ? empty : group.Key.ToTrimmedString();
                            abn.Header = header;
                            abn.Meters = group.ToList();
                            abn.MetersCount = group.Count();
                            abn.Type = nodeType;
                            abonentBindingNodes.Add(abn);
                        }

                        return abonentBindingNodes.OrderBy(i => i.Header).ToList();
                    }
                    else
                    {
                        return null;
                    }
                }

                ICollection<Meter> list = null;
                IList<AbonentBindingNode> substationsNodes = groupMetersByProperty(this.Data, "Подстанция", AbonentBindingNode.NodeType.Substation);
                if (substationsNodes != null)
                {
                    foreach (AbonentBindingNode substation in substationsNodes)
                    {
                        IList<AbonentBindingNode> fiders = groupMetersByProperty(substation.Meters, "Фидер10", AbonentBindingNode.NodeType.Fider10);
                        if (fiders != null)
                        {
                            substation.AddChildren(fiders);
                            foreach (AbonentBindingNode fider in substation.Children)
                            {
                                IList<AbonentBindingNode> tps = groupMetersByProperty(fider.Meters, "ТП", AbonentBindingNode.NodeType.TP);
                                if (tps != null)
                                {
                                    fider.AddChildren(tps);
                                    foreach (AbonentBindingNode tp in fider.Children)
                                    {
                                        IList<AbonentBindingNode> fiders04 = groupMetersByProperty(tp.Meters, "Фидер04", AbonentBindingNode.NodeType.Fider04);
                                        if (fiders04 != null)
                                        {
                                            tp.AddChildren(fiders04);
                                        }
                                        else
                                        {
                                            tp.NotBindingMetersCount = tp.MetersCount;
                                        }
                                    }

                                    list = fider.Meters.Where(i => i.ТП == null || (i.ТП != null && i.ТП.IsEmpty)).ToList();
                                    fider.NotBindingMetersCount = fider.Children.Cast<AbonentBindingNode>().Sum(i => i.NotBindingMetersCount);

                                    if (list.Count > 0)
                                    {
                                        IEnumerable<AbonentBindingNode> emptyNodes = fider.Children.Cast<AbonentBindingNode>().Where(i => string.Equals(i.Header, empty, AppSettings.StringComparisonMethod));
                                        if (!emptyNodes.Any())
                                        {
                                            fider.Children.Insert(0, item: new AbonentBindingNode(
                                                fider,
                                                empty,
                                                list,
                                                AbonentBindingNode.NodeType.TP));
                                        }
                                    }
                                }
                                else
                                {
                                    fider.NotBindingMetersCount = fider.MetersCount;
                                }
                            }

                            list = substation.Meters.Where(i => string.IsNullOrWhiteSpace(i.Фидер10)).ToList();
                            substation.NotBindingMetersCount = substation.Children.Cast<AbonentBindingNode>().Sum(i => i.NotBindingMetersCount);

                            if (list.Count > 0)
                            {
                                IEnumerable<AbonentBindingNode> emptyNodes = substation.Children.Cast<AbonentBindingNode>().Where(i => string.Equals(i.Header, empty, AppSettings.StringComparisonMethod));
                                if (!emptyNodes.Any())
                                {
                                    substation.Children.Insert(0, new AbonentBindingNode(substation,
                                                                                         empty,
                                                                                         list,
                                                                                         AbonentBindingNode.NodeType.Fider10)
                                    {
                                        Parent = substation,
                                    });
                                }
                            }
                        }
                        else
                        {
                            substation.NotBindingMetersCount = substation.MetersCount;
                        }
                    }
                }

                list = this.Data
                    .Where(i =>
                        string.IsNullOrWhiteSpace(i.Подстанция) |
                        string.IsNullOrWhiteSpace(i.Фидер10) |
                        (i.ТП == null || (i.ТП != null && i.ТП.IsEmpty)) |
                        i.Фидер04.HasValue == false)
                    .ToList();

                AbonentBindingNode root = new()
                {
                    IsExpanded = true,
                    Type = AbonentBindingNode.NodeType.Departament,
                    Header = "РЭС",
                    Meters = new List<Meter>(this.Data),
                    MetersCount = this.Data.Count(),
                    NotBindingMetersCount = list.Count,
                };
                if (list.Count > 0)
                {
                    root.Children.Add(new AbonentBindingNode(
                        root,
                        "Не полная привязка",
                        list,
                        AbonentBindingNode.NodeType.Group));
                }

                if (!substationsNodes.Cast<AbonentBindingNode>().Any(i => string.Equals(i.Header, empty, AppSettings.StringComparisonMethod)))
                {
                    list = this.Data.Where(i => string.IsNullOrWhiteSpace(i.Подстанция)).ToList();
                    if (list.Count > 0)
                    {
                        root.Children.Add(new AbonentBindingNode(
                            root,
                            empty,
                            list,
                            AbonentBindingNode.NodeType.Substation));
                    }
                }

                root.AddChildren(substationsNodes);

                this.AbonentBindingNodes = new ObservableCollection<AbonentBindingNode>(root.Children);
            })
                .ContinueWith(t =>
                {
                    this.IsBusy = false;
                    this.Status = null;
                });
        }

        public ICommand ToggleIsVisualizing => new DelegateCommand(() => this.IsVisualizing = !this.IsVisualizing);

        public bool IsVisualizing
        {
            get => this.isVisualizing;
            set
            {
                if (this.SetProperty(ref this.isVisualizing, value))
                {
                    this.RaisePropertyChanged(nameof(this.ToggleIsVisualizing));
                }
            }
        }

        public ObservableCollection<AbonentBindingNode> AbonentBindingNodes
        {
            get => this.abonentBindingNodes;
            private set
            {
                if (this.SetProperty(ref this.abonentBindingNodes, value))
                {
                    this.RaisePropertyChanged(nameof(this.TotalMetersCount));
                    this.BuildTreeMapItems();
                }
            }
        }

        public AbonentBindingNode SelectedAbonentBindingNode
        {
            get => this.selectedAbonentBindingNode;
            set
            {
                if (this.SetProperty(ref this.selectedAbonentBindingNode, value))
                {
                    this.Reset();
                    this.RaisePropertyChanged(nameof(this.TotalMetersCount));
                    this.RaisePropertyChanged(nameof(this.ReportTitle));
                    this.BuildTreeMapItems();
                }
            }
        }

        public int TotalMetersCount => this.SelectedAbonentBindingNode == null
            ? (this.AbonentBindingNodes == null
                ? 0
                : this.AbonentBindingNodes[0].MetersCount)
            : this.SelectedAbonentBindingNode.MetersCount;

        public ICollection<TreeMapItem> TreeMapItems
        {
            get => this.treeMapItems;
            private set => this.SetProperty(ref this.treeMapItems, value);
        }

        /// <summary>
        /// Фильтр для отбора
        /// </summary>
        public string AbonentBondingFilter
        {
            get => this.abonentBondingFilter;
            set
            {
                this.SetProperty(ref this.abonentBondingFilter, value);
                this.ApplyFilter();
            }
        }

        public int VisibleAbonentBindingNodesCount => (this.AbonentBindingNodes == null) ? 0 : this.AbonentBindingNodes.Count(n => n.IsMatch);

        #region Methods

        private void ApplyFilter()
        {
            this.IsBusy = true;
            System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(() =>
            {
                foreach (AbonentBindingNode child in this.AbonentBindingNodes)
                {
                    child.ApplyCriteria(this.AbonentBondingFilter, new Stack<AbonentBindingNode>());
                }
            });
            task.ContinueWith(t =>
            {
                this.IsBusy = false;
                this.RaisePropertyChanged(nameof(this.VisibleAbonentBindingNodesCount));
            });
        }

        protected override ICollectionView BuildAndGetView()
        {
            if (this.selectedAbonentBindingNode != null)
            {
                this.IsBusy = true;
                ListCollectionView view = new ListCollectionView(this.selectedAbonentBindingNode.Meters.ToList());
                using (view.DeferRefresh())
                {
                    view.SortDescriptions.Add(new SortDescription(nameof(Meter.Подстанция), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(Meter.Фидер10), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(Meter.ТПNumber), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(Meter.Фидер04), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(Meter.Опора), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(Meter.НаселённыйПунктИУлицаСНомеромДома), ListSortDirection.Ascending));
                    view.SortDescriptions.Add(new SortDescription(nameof(Meter.Лицевой), ListSortDirection.Ascending));

                    // view.Filter = Filter;
                }

                this.IsBusy = false;
                return view;
            }
            else
            {
                return null;
            }
        }

        private void BuildTreeMapItems()
        {
            if (this.AbonentBindingNodes == null)
            {
                return;
            }

            AbonentBindingNode source = this.SelectedAbonentBindingNode ?? this.AbonentBindingNodes.FirstOrDefault();
            Func<IList<AbonentBindingNode>, ICollection<TreeMapItem>> recursiveBuild = null;
            recursiveBuild = (nodes) =>
            {
                ICollection<TreeMapItem> result = new List<TreeMapItem>();
                foreach (AbonentBindingNode node in nodes)
                {
                    TreeMapItem item = new(node.Header, node.MetersCount);
                    item.AddChildren(recursiveBuild(node.Children));
                    result.Add(item);
                }

                return result;
            };

            this.TreeMapItems = recursiveBuild(source.Children);
        }

        public override string ReportTitle => $"Сведения о привяке абонентов по '{this.SelectedAbonentBindingNode?.Header}";

        #endregion

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0452");
            return guid.GetHashCode();
        }

        public class TreeMapItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private string header;

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
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.ToolTip));
                }
            }

            private int metersCount;

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
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.ToolTip));
                }
            }

            public ICollection<TreeMapItem> Children { get; private set; }

            public TreeMapItem Parent { get; private set; }

            public string ToolTip
            {
                get
                {
                    string result = $"{this.Header}\n{this.MetersCount} счётчиков";
                    TreeMapItem p = this.Parent;
                    while (p != null)
                    {
                        result = p.Header + " \\ " + result;
                        p = p.Parent;
                    }

                    return result;
                }
            }

            public TreeMapItem()
            {
                this.Children = new List<TreeMapItem>();
            }

            public TreeMapItem(string header, int metersCount) : this()
            {
                this.Parent = null;
                this.header = header;
                this.metersCount = metersCount;
            }

            public void AddChildren(IEnumerable<TreeMapItem> children)
            {
                if (children == null)
                {
                    return;
                }

                foreach (TreeMapItem child in children)
                {
                    child.Parent = this;
                    this.Children.Add(child);
                }
            }

            public override string ToString()
            {
                return $"TreeMapItem - {this.Header}, count {this.MetersCount}";
            }

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
