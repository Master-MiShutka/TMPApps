using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;

using TMP.Wpf.Common.Controls.TreeListView;

namespace TMP.Work.Emcos.Model
{
    public class AuxiliaryReportTreeModel : ITreeModel
    {
        private IList<AuxiliaryReportItem> _model;

        public AuxiliaryReportTreeModel(IList<AuxiliaryReportItem> model)
        {
            if (model == null)
                model = new ObservableCollection<AuxiliaryReportItem>();
            _model = model;
        }
        public IEnumerable GetChildren(object parent)
        {
            if (parent == null)
                return _model;

            var element = parent as AuxiliaryReportItem;
            if (element == null)
                return null;
            else
                return element.Children;
        }

        public bool HasChildren(object parent)
        {
            var element = parent as AuxiliaryReportItem;
            if (element == null)
                return false;
            else
            {
                if (element.Children == null)
                    return false;
                else
                    if (element.Children.Count == 0)
                    return false;
                else
                    return true;
            }
        }

        public int Count { get { return _model == null ? 0 : _model.Count; } }
    }

    public class AuxiliaryReportItem
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public double? Value { get; set; }
        public ICollection<AuxiliaryReportItem> Children { get; set; }
        public AuxiliaryReportItem()
        {
            Children = null;// new ObservableCollection<AuxiliaryReportItem>();
        }
    }
}
