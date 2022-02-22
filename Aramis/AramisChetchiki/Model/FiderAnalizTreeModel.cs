namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using TMP.Shared;

    public class FiderAnalizTreeModel : ITreeModel
    {
        private IList<FiderAnalizTreeItem> model;

        public FiderAnalizTreeModel(IList<FiderAnalizTreeItem> model)
        {
            if (model == null)
                model = new ObservableCollection<FiderAnalizTreeItem>();
            this.model = model;
        }

        public IEnumerable GetParentChildren(object parent)
        {
            if (parent == null)
                return this.model;

            FiderAnalizTreeItem element = parent as FiderAnalizTreeItem;
            return element?.Children;
        }

        public bool HasParentChildren(object parent)
        {
            return parent is FiderAnalizTreeItem element && (element.Children != null && element.Children.Count != 0);
        }

        public int Count => this.model == null ? 0 : this.model.Count;
    }
}
