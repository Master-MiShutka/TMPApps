﻿namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;

    /// <summary>
    /// Базовый интерфейс, описывающий модель представления данных
    /// </summary>
    public interface IViewModel : TMPApplication.IMainViewModel
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

    public interface IDataViewModel<T> : IViewModel
    {
        /// <summary>
        /// Коллекция
        /// </summary>
        IEnumerable<T> Data { get; }

        /// <summary>
        /// Представление коллекции
        /// </summary>
        ICollectionView View { get; }
    }
}