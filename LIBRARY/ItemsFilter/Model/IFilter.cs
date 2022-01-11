namespace ItemsFilter.Model
{
    using System;
    using System.ComponentModel;
    using ItemsFilter.ViewModel;

    /// <summary>
    /// Defines the contract for a filter used by the FilteredCollection
    /// </summary>
    public interface IFilter : INotifyPropertyChanged
    {
        /// <summary>
        /// Возвращает или задает название фильтра
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Get or set value that indicates are filter include in filter function.
        /// </summary>
        bool IsActive
        {
            get;
            set;
        }

        /// <summary>
        /// Возвращает FilterPresenter
        /// </summary>
        FilterPresenter FilterPresenter { get; }

        /// <summary>
        /// Determines whether the specified target is matching the criteria.
        /// </summary>
        void IsMatch(FilterPresenter sender, FilterEventArgs e);

        void Attach(FilterPresenter presenter);

        void Attach(FilterControlVm vm);

        void Detach(FilterPresenter presenter);

        void Detach(FilterControlVm vm);
    }
}
