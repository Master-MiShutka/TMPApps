using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TMP.Work.Emcos.Model
{
    public class EmcosPointWithValueCollection : ObservableCollection<EmcosPointWithValue>
    {
        private EmcosPointWithValue _parent;

        public EmcosPointWithValueCollection(EmcosPointWithValue parent) : base()
        {
            this._parent = parent;
            this.FlatItemsList = new List<EmcosPointWithValue>();
        }

        public EmcosPointWithValueCollection(EmcosPointWithValue parent, IEnumerable<EmcosPointWithValue> collection) : base(collection)
        {
            this._parent = parent;
            this.FlatItemsList = new List<EmcosPointWithValue>();
        }

        public IList<EmcosPointWithValue> FlatItemsList { get; private set; }

        private void AddToFlatItemsList(EmcosPointWithValue item)
        {
            FlatItemsList.Add(item);

            if (item.Children != null)
                foreach (var child in item.Children)
                    AddToFlatItemsList((EmcosPointWithValue)child);
        }

        protected override void InsertItem(int index, EmcosPointWithValue item)
        {
            if (item != null)
                item.Parent = _parent;
            base.InsertItem(index, item);

            AddToFlatItemsList(item);
        }

        protected override void RemoveItem(int index)
        {
            EmcosPointWithValue oldItem = base[index];
            base.RemoveItem(index);
            if (oldItem != null)
                oldItem.Parent = null;
            FlatItemsList.Remove(oldItem);
        }

        protected override void SetItem(int index, EmcosPointWithValue item)
        {
            EmcosPointWithValue oldItem = base[index];

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
            foreach (EmcosPointWithValue item in base.Items)
            {
                if (item != null)
                    item.Parent = null;
            }
            base.ClearItems();
            FlatItemsList.Clear();
        }
    }   
}
