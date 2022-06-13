namespace WindowWithDialogs
{
    using System;
    using System.ComponentModel;

    public interface IMainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Статус
        /// </summary>
        string Status { get; set; }

        /// <summary>
        /// Подробный статус
        /// </summary>
        string DetailedStatus { get; set; }

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
        bool IsDataLoaded { get; }
    }
}
