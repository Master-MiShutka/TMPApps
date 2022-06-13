namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;

    /// <summary>
    /// Базовый интерфейс, описывающий модель представления данных
    /// </summary>
    public interface IViewModel : WindowWithDialogs.IMainViewModel
    {
        /// <summary>
        /// Команда экспорта данных
        /// </summary>
        ICommand CommandExport { get; }

        /// <summary>
        /// Команда печати данных
        /// </summary>
        ICommand CommandPrint { get; }
    }

    public interface IViewModelWithDataView
    {
        /// <summary>
        /// Представление коллекции
        /// </summary>
        ICollectionView View { get; }
    }

    public interface IDataViewModel<T> : IViewModel, IViewModelWithDataView
    {
        /// <summary>
        /// Коллекция
        /// </summary>
        IEnumerable<T> Data { get; }
    }
}
