namespace ItemsFilter
{
    using System;
    using ItemsFilter.Model;

    /// <summary>
    /// Provides data for the <see cref="Filtered"/> event
    /// </summary>
    public class ViewModelChangedEventArgs : EventArgs
    {
        private readonly IFilter oldFilter;
        private readonly IFilter newFilter;

        internal ViewModelChangedEventArgs(IFilter oldFilter, IFilter newFilter)
        {
            this.oldFilter = oldFilter;
            this.newFilter = newFilter;
        }

        /// <summary>
        /// Old IFilter as ViewModel
        /// </summary>
        public IFilter OldViewModel => this.oldFilter;

        /// <summary>
        /// New IFilter as ViewModel
        /// </summary>
        public IFilter NewViewModel => this.newFilter;
    }

    public delegate void ViewModelChangedEventHandler(object? sender, ViewModelChangedEventArgs e);
}