using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;

using TMP.Wpf.Common.Controls.TreeListView;

namespace TMP.Work.Emcos.Model
{
    using Balans;
    public class BalansTreeModel : ITreeModel
    {
        private IList<Substation> _model;

        public BalansTreeModel(IList<Substation> model)
        {
            if (model == null)
                model = new ObservableCollection<Substation>();
            _model = model;
        }
        public IEnumerable GetChildren(object parent)
        {
            if (parent == null)
                return _model;

            var element = parent as IBalansGroup;
            if (element == null)
                return null;
            else
                return element.Children;
        }

        public bool HasChildren(object parent)
        {
            var element = parent as IBalansGroup;
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
}
