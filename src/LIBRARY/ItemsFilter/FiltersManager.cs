namespace ItemsFilter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    public static class FiltersManager
    {
        private static readonly Dictionary<ICollectionView, WeakReference> filterPresenters = new Dictionary<ICollectionView, WeakReference>();

        // <summary>
        // Возвращает FilterPresenter, подключенный к переданному источнику
        // Если передан экзмепляр ICollectionView, FilterPresenter подключается к нему, иначе, FilterPresenter подключается к виду по умолчанию переданной коллекции
        // </summary>
        // <param name="source">ICollectionView коллекции или коллекция </param>
        // <returns>FilterPresenter, подключенный к коллекции, или null если коллекция не задана</returns>
        public static FilterPresenter TryGetFilterPresenter(IEnumerable source)
        {
            if (source == null)
            {
                return null;
            }

            if (!(source is ICollectionView sourceCollectionView))
            {
                sourceCollectionView = CollectionViewSource.GetDefaultView(source);
            }

            FilterPresenter instance = null;

            // GC.Collect();
            foreach (var entry in filterPresenters.ToArray())
            {
                if (!entry.Value.IsAlive)
                {
                    filterPresenters.Remove(entry.Key);
                }
            }

            if (filterPresenters.ContainsKey(sourceCollectionView))
            {
                var wr = filterPresenters[sourceCollectionView];
                instance = wr.Target as FilterPresenter;
            }

            if (instance == null)
            {
                instance = new FilterPresenter(sourceCollectionView);
                if (filterPresenters.ContainsKey(sourceCollectionView))
                {
                    filterPresenters[sourceCollectionView] = new WeakReference(instance);
                }
                else
                {
                    filterPresenters.Add(sourceCollectionView, new WeakReference(instance));
                }
            }

            return instance;
        }

        /// <summary>
        /// Удаляет экзмепляр ICollectionView из коллекции
        /// </summary>
        /// <param name="source">Экзмепляр ICollectionView</param>
        public static void RemoveCollectionView(ICollectionView source)
        {
            if (source == null)
            {
                return;
            }

            if (filterPresenters.ContainsKey(source) == false)
            {
                return;
            }

            var item = filterPresenters[source];
            if (item.Target is IDisposable disposable)
            {
                disposable.Dispose();
            }

            if (item.Target != null)
            {
                item.Target = null;
            }

            filterPresenters.Remove(source);
        }
    }
}
