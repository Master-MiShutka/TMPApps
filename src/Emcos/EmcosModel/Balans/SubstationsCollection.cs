using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TMP.Work.Emcos.Model.Balance
{
    public class SubstationsCollection : ObservableCollection<Substation>
    {
        private IBalanceGroupItem _parent;

        public SubstationsCollection()
        {
            this.FlatItemsList = new List<IHierarchicalEmcosPoint>();
        }

        public SubstationsCollection(IBalanceGroupItem parent) : base()
        {
            this._parent = parent;
            this.FlatItemsList = new List<IHierarchicalEmcosPoint>();
        }

        public SubstationsCollection(IBalanceGroupItem parent, IEnumerable<IHierarchicalEmcosPoint> collection) : this(parent)
        {
            if (collection == null)
                throw new ArgumentNullException();
            foreach (IHierarchicalEmcosPoint point in collection)
            {
                if (point is Substation substation)
                    this.Add(substation);
            }
        }

        public IList<IHierarchicalEmcosPoint> FlatItemsList { get; private set; }

        private void AddToFlatItemsList(IHierarchicalEmcosPoint item)
        {
            FlatItemsList.Add(item);

            if (item.Children != null)
                foreach (var child in item.Children)
                    AddToFlatItemsList((IHierarchicalEmcosPoint)child);
        }

        protected override void InsertItem(int index, Substation item)
        {
            if (item != null)
                item.Parent = _parent;
            base.InsertItem(index, item);

            AddToFlatItemsList(item);
        }

        protected override void RemoveItem(int index)
        {
            IBalanceGroupItem oldItem = base[index];
            base.RemoveItem(index);
            if (oldItem != null)
                oldItem.Parent = null;
            FlatItemsList.Remove(oldItem);
        }

        protected override void SetItem(int index, Substation item)
        {
            IBalanceGroupItem oldItem = base[index];

            int ind = FlatItemsList.IndexOf(oldItem);

            if (oldItem != null)
                oldItem.Parent = null;
            if (item != null)
                item.Parent = _parent;

            base.SetItem(index, item);

            FlatItemsList[ind] = item;
        }

        protected override void ClearItems()
        {
            foreach (IHierarchicalEmcosPoint item in base.Items)
            {
                if (item != null)
                    item.Parent = null;
            }
            base.ClearItems();
            FlatItemsList.Clear();
        }
    }
}
