namespace TMP.Shared
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines the contract for an object that has a object
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    public interface IChildItem<T>
        where T : IChildItem<T>
    {
        T Parent { get; set; }
    }

    /// <summary>
    /// Collection of child items. This collection automatically set the
    /// Parent property of the child items when they are added or removed
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    public class ChildItemCollection<T> : ObservableCollection<T>
        where T : class, IChildItem<T>
    {
        private T parent;

        public ChildItemCollection(T parent) : base()
        {
            this.parent = parent;
        }

        public ChildItemCollection(T parent, IEnumerable<T> collection) : base(collection)
        {
            this.parent = parent;
        }

        protected override void InsertItem(int index, T item)
        {
            if (item != null)
            {
                item.Parent = this.parent;
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            T oldItem = base[index];
            base.RemoveItem(index);
            if (oldItem != null)
            {
                oldItem.Parent = default(T);
            }
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem = base[index];

            if (oldItem != null)
            {
                oldItem.Parent = default(T);
            }

            if (item != null)
            {
                item.Parent = this.parent;
            }

            base.SetItem(index, item);
        }

        protected override void ClearItems()
        {
            foreach (T item in base.Items)
            {
                if (item != null)
                {
                    item.Parent = default(T);
                }
            }

            base.ClearItems();
        }
    }
}
