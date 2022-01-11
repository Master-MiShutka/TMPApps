namespace ItemsFilter.View
{
    using ItemsFilter.Model;

    public interface IFilterView
    {
        IFilter ViewModel { get; }

        event ViewModelChangedEventHandler ViewModelChanged;
    }
}