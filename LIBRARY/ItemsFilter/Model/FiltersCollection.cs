namespace ItemsFilter.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    /// Коллекция фильтров
    /// </summary>
    public class FiltersCollection
    {
        private readonly ListDictionary dictionary = new ListDictionary();
        private readonly FilterPresenter parent;

        internal FiltersCollection(FilterPresenter parent)
        {
            this.parent = parent;
        }

        internal bool ContainsKey(Type filterKey)
        {
            return this.dictionary.Contains(filterKey);
        }

        internal IFilter this[Type key]
        {
            get => (IFilter)this.dictionary[key];

            set
            {
                var defer = this.parent.DeferRefresh();
                IFilter filter;
                if (this.dictionary.Contains(key))
                {
                    filter = (IFilter)this.dictionary[key];
                    filter.Detach(this.parent);
                }

                this.dictionary[key] = filter = value;
                filter.Attach(this.parent);
                defer.Dispose();
            }
        }

        internal void Remove(Type key)
        {
            if (this.dictionary.Contains(key))
            {
                var defer = this.parent.DeferRefresh();
                Filter filter = (Filter)this.dictionary[key];
                filter.Detach(this.parent);
                this.dictionary.Remove(key);
                defer.Dispose();
            }
        }

        internal void Remove(Filter filter)
        {
            Type key = null;
            IDictionaryEnumerator enumerator = this.dictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Value == filter)
                {
                    key = (Type)enumerator.Key;
                    break;
                }
            }

            if (key != null)
            {
                this.dictionary.Remove(key);
            }
        }

        /// <summary>
        /// Возвращает перечисление фильтров
        /// </summary>
        public IEnumerable<IFilter> Filters
        {
            get
            {
                var enumerator = this.dictionary.Values.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    yield return (IFilter)enumerator.Current;
                }
            }
        }

        /// <summary>
        /// Возвращает количество фильтров в коллекции
        /// </summary>
        public int Count => this.dictionary.Count;
    }
}