using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    /// <summary>
    /// Базовый интерфейс, описывающий модель представления данных
    /// </summary>
    public interface IViewModel
    {
        /// <summary>
        /// Статус
        /// </summary>
        String Status { get; set; }
        /// <summary>
        /// Детальный статус
        /// </summary>
        String DetailedStatus { get; set; }
        /// <summary>
        /// Признак, указывающий, что выполняется длительная операция
        /// </summary>
        bool IsBusy { get; set; }

        /// <summary>
        /// Признак, указывающий, что выполняется анализ информации
        /// </summary>
        bool IsAnalizingData { get; set; }
        /// <summary>
        /// Признак, указывающий, что данные загружены
        /// </summary>
        bool IsDataLoaded { get; set; }

        /// <summary>
        /// Команда экспорта данных
        /// </summary>
        ICommand CommandExport { get; }
        /// <summary>
        /// Команда печати данных
        /// </summary>
        ICommand CommandPrint { get; }

        /// <summary>
        /// Команда для закрытия активного окна
        /// </summary>
        ICommand CommandCloseWindow { get; }
    }

    public interface IDataViewModel<T>
    {
        /// <summary>
        /// Коллекция
        /// </summary>
        ICollection<T> Data { get; set; }
        /// <summary>
        /// Представление коллекции
        /// </summary>
        ICollectionView View { get; set; }
    }
}
