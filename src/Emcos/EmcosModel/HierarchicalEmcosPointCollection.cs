using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TMP.Work.Emcos.Model
{
    public class HierarchicalEmcosPointCollection : ObservableCollection<IHierarchicalEmcosPoint>
    {
        private IHierarchicalEmcosPoint _parent;

        public HierarchicalEmcosPointCollection() { }

        public HierarchicalEmcosPointCollection(IHierarchicalEmcosPoint parent) : base()
        {
            this._parent = parent;
            this.FlatItemsList = new List<IHierarchicalEmcosPoint>();
        }

        public HierarchicalEmcosPointCollection(IHierarchicalEmcosPoint parent, IEnumerable<IHierarchicalEmcosPoint> collection) : base(collection)
        {
            this._parent = parent;
            this.FlatItemsList = new List<IHierarchicalEmcosPoint>();
        }

        public void Create(IEnumerable<IHierarchicalEmcosPoint> collection)
        {
            this.ClearItems();

            foreach (IHierarchicalEmcosPoint point in collection)
            {
                this.Add(point);
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

        protected override void InsertItem(int index, IHierarchicalEmcosPoint item)
        {
            if (item != null)
                item.Parent = _parent;
            base.InsertItem(index, item);

            AddToFlatItemsList(item);
        }

        protected override void RemoveItem(int index)
        {
            IHierarchicalEmcosPoint oldItem = base[index];
            base.RemoveItem(index);
            if (oldItem != null)
                oldItem.Parent = null;
            FlatItemsList.Remove(oldItem);
        }

        protected override void SetItem(int index, IHierarchicalEmcosPoint item)
        {
            IHierarchicalEmcosPoint oldItem = base[index];

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
