using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;

using TMP.Shared;
using System.Linq;

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
        public IEnumerable GetParentChildren(object parent)
        {
            if (parent == null)
                return _model;

            var element = parent as AuxiliaryReportItem;
            if (element == null)
                return null;
            else
                return element.Children;
        }

        public bool HasParentChildren(object parent)
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
        private double? _aPlus, _aMinus, _rPlus, _rMinus;

        public string Type { get; set; }
        public string Name { get; set; }
        public double? APlus
        {
            get
            {
                if (HasChildren == false)
                    return _aPlus;
                else
                    return Children.Where(i => i.APlus.HasValue).Sum(i => i.APlus ?? 0d);
            }
            set { _aPlus = value; }
        }
        public double? AMinus
        {
            get
            {
                if (HasChildren == false)
                    return _aMinus;
                else
                    return Children.Where(i => i.AMinus.HasValue).Sum(i => i.AMinus ?? 0d);
            }
            set { _aMinus = value; }
        }
        public double? RPlus
        {
            get
            {
                if (HasChildren == false)
                    return _rPlus;
                else
                    return Children.Where(i => i.RPlus.HasValue).Sum(i => i.RPlus ?? 0d);
            }
            set { _rPlus = value; }
        }
        public double? RMinus
        {
            get
            {
                if (HasChildren == false)
                    return _rMinus;
                else
                    return Children.Where(i => i.RMinus.HasValue).Sum(i => i.RMinus ?? 0d);
            }
            set { _rMinus = value; }
        }
        public ICollection<AuxiliaryReportItem> Children { get; set; }

        public bool HasChildren => Children != null && Children.Count > 0;

        public AuxiliaryReportItem()
        {
            Children = null;// new ObservableCollection<AuxiliaryReportItem>();
        }
    }
}
