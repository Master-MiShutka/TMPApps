namespace TMP.Shared.Common
{
    using System.Collections.Generic;

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
    public class ChildItemCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
        where T : class, IChildItem<T>
    {
        private readonly T parent;

        public ChildItemCollection(T parent)
            : base()
        {
            this.parent = parent;
        }

        public ChildItemCollection(T parent, IEnumerable<T> collection)
            : base(collection)
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
            T oldItem = this[index];
            base.RemoveItem(index);
            if (oldItem != null)
            {
                oldItem.Parent = default;
            }
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem = this[index];

            if (oldItem != null)
            {
                oldItem.Parent = default;
            }

            if (item != null)
            {
                item.Parent = this.parent;
            }

            base.SetItem(index, item);
        }

        protected override void ClearItems()
        {
            foreach (T item in this.Items)
            {
                if (item != null)
                {
                    item.Parent = default;
                }
            }

            base.ClearItems();
        }
    }
}
