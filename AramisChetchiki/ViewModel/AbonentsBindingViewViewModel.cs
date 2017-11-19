using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using TMP.UI.Controls.WPF;
using TMP.WORK.AramisChetchiki.Model;
using TMP.Extensions;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    public class AbonentsBindingViewViewModel : BaseViewModel
    {
        private IList<Meter> _listOfMeters;

        public AbonentsBindingViewViewModel() { }
        public AbonentsBindingViewViewModel(ICollection<Meter> meters)
        {
            if (meters == null)
                throw new ArgumentNullException("Meters collection");
            IsAnalizingData = true;
            Status = "Подоговка данных ...";
            var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                const string empty = "(пусто)";

                Func<IEnumerable<Meter>, string, AbonentBindingNode.NodeType, IList<AbonentBindingNode>> groupMetersByProperty = (metersList, propName, nodeType) =>
                {
                    // группируем список по значению свойства
                    var groups = metersList
                        .GroupBy(i => ModelHelper.MeterGetPropertyValue(i, propName))
                        .ToList();
                    if (groups.Count >= 1)
                    {
                        List<AbonentBindingNode> abonentBindingNodes = new List<AbonentBindingNode>();
                        foreach (IGrouping<object, Meter> group in groups)
                        {
                            AbonentBindingNode abn = new AbonentBindingNode();
                            string header = group.Key == null || String.IsNullOrWhiteSpace(group.Key.ToString()) ? empty : group.Key.ToTrimmedString();
                            abn.Header = header;
                            abn.Meters = group.ToList();
                            abn.MetersCount = group.Count();
                            abn.Type = nodeType;
                            abonentBindingNodes.Add(abn);
                        }
                        return abonentBindingNodes.OrderBy(i => i.Text).ToList();
                    }
                    else
                        return null;
                };

                List<Meter> list = null;
                var nodes = groupMetersByProperty(meters, "Подстанция", AbonentBindingNode.NodeType.Substation);
                if (nodes != null)
                    foreach (AbonentBindingNode substation in nodes)
                    {
                        var fiders = groupMetersByProperty(substation.Meters, "Фидер10", AbonentBindingNode.NodeType.Fider10);
                        if (fiders != null)
                        {
                            substation.Children.AddRange(fiders);
                            foreach (AbonentBindingNode fider in substation.Children)
                            {
                                var tps = groupMetersByProperty(fider.Meters, "ТП", AbonentBindingNode.NodeType.TP);
                                if (tps != null)
                                {
                                    fider.Children.AddRange(tps);
                                    foreach (AbonentBindingNode tp in fider.Children)
                                    {
                                        var fiders04 = groupMetersByProperty(tp.Meters, "Фидер04", AbonentBindingNode.NodeType.Fider04);
                                        if (fiders04 != null)
                                        {
                                            tp.Children.AddRange(fiders04);
                                        }
                                        else
                                            tp.NotBindingMetersCount = tp.MetersCount;
                                    }
                                    list = fider.Meters.Where(i => String.IsNullOrWhiteSpace(i.ТП)).ToList();
                                    fider.NotBindingMetersCount = fider.Children.Cast<AbonentBindingNode>().Sum(i => i.NotBindingMetersCount); ;

                                    if (list.Count > 0)
                                    {
                                        var emptyNodes = fider.Children.Cast<AbonentBindingNode>().Where(i => String.Equals(i.Header, empty));
                                        if (emptyNodes.Count() == 0)
                                            fider.Children.Insert(0, new AbonentBindingNode(
                                            empty,
                                            list,
                                            AbonentBindingNode.NodeType.TP
                                            ));
                                    }
                                }
                                else
                                    fider.NotBindingMetersCount = fider.MetersCount;
                            }
                            list = substation.Meters.Where(i => String.IsNullOrWhiteSpace(i.Фидер10)).ToList();
                            substation.NotBindingMetersCount = substation.Children.Cast<AbonentBindingNode>().Sum(i => i.NotBindingMetersCount); ;

                            if (list.Count > 0)
                            {
                                var emptyNodes = substation.Children.Cast<AbonentBindingNode>().Where(i => String.Equals(i.Header, empty));
                                if (emptyNodes.Count() == 0)
                                    substation.Children.Insert(0, new AbonentBindingNode(
                                    empty,
                                    list,
                                    AbonentBindingNode.NodeType.Fider10
                                    ));
                            }
                        }
                        else
                            substation.NotBindingMetersCount = substation.MetersCount;
                    }

                list = meters.Where(i =>
                    String.IsNullOrWhiteSpace(i.Подстанция) |
                    String.IsNullOrWhiteSpace(i.Фидер10) |
                    String.IsNullOrWhiteSpace(i.ТП) |
                    String.IsNullOrWhiteSpace(i.Фидер04)
                    ).ToList();
                AbonentBindingNodes = new AbonentBindingNode() {
                    Type = AbonentBindingNode.NodeType.Departament,
                    Header = "РЭС",
                    Meters = new List<Meter>(meters),
                    MetersCount = meters.Count,
                    NotBindingMetersCount = list.Count };
                if (list.Count > 0)
                    AbonentBindingNodes.Children.Add(new AbonentBindingNode(
                        "Не полная привязка",
                        list,
                        AbonentBindingNode.NodeType.Group
                        ));
                list = meters.Where(i => String.IsNullOrWhiteSpace(i.Подстанция)).ToList();
                if (list.Count > 0)
                    AbonentBindingNodes.Children.Add(new AbonentBindingNode(
                        empty,
                        list,
                        AbonentBindingNode.NodeType.Substation
                        ));
                AbonentBindingNodes.Children.AddRange(nodes);

                IsAnalizingData = false;
                Status = null;
            });

            var fields = Meter.ПривязкаViewColumns.Select(name => new Xceed.Wpf.DataGrid.TableField()
            {
                Type = ModelHelper.MeterPropertiesCollection[name].PropertyType,
                DisplayOrder = ModelHelper.MeterPropertiesCollection[name].Order,
                Name = name,
                DisplayName = ModelHelper.MeterPropertiesCollection[name].DisplayName,
                GroupName = ModelHelper.MeterPropertiesCollection[name].GroupName,
                IsVisible = ModelHelper.MeterPropertiesCollection[name].IsVisible
            });
            TableColumns = new List<Xceed.Wpf.DataGrid.ColumnBase>(Xceed.Wpf.DataGrid.Extensions.DataGridControlExtensions.BuildColumns(fields));
        }

        public ICommand ToggleIsVisualizing => new DelegateCommand(() => IsVisualizing = !IsVisualizing);

        private bool _isVisualizing = false;
        public bool IsVisualizing
        {
            get => _isVisualizing;
            set
            {
                _isVisualizing = value;
                RaisePropertyChanged("IsVisualizing");
            }
        }

        public IList<Meter> ListOfMeters
        {
            get { return _listOfMeters; }
            set { _listOfMeters = value; RaisePropertyChanged("ListOfMeters"); }
        }
        public List<Xceed.Wpf.DataGrid.ColumnBase> TableColumns { get; }

        private AbonentBindingNode _abonentBindingNodes = null;
        public AbonentBindingNode AbonentBindingNodes
        {
            get { return _abonentBindingNodes; }
            set
            {
                _abonentBindingNodes = value;
                RaisePropertyChanged("AbonentBindingNodes");
            }
        }

        private AbonentBindingNode _selectedAbonentBindingNode = null;
        public AbonentBindingNode SelectedAbonentBindingNode
        {
            get { return _selectedAbonentBindingNode; }
            set
            {
                _selectedAbonentBindingNode = value;
                RaisePropertyChanged("SelectedAbonentBindingNode");
                PrepareMetersList();
            }
        }

        private void PrepareMetersList()
        {
            if (_selectedAbonentBindingNode != null)
            {
                ListOfMeters = new List<Meter>(_selectedAbonentBindingNode.Meters);
            }
        }
    }
}
