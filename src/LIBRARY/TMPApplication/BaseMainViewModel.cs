namespace TMPApplication
{
    using TMP.Shared.Common;

    public class BaseMainViewModel : PropertyChangedBase, IMainViewModel
    {
        /// <summary>
        /// Статус
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Детальный статус
        /// </summary>
        public string DetailedStatus { get; set; }

        /// <summary>
        /// Признак, указывающий, что выполняется длительная операция
        /// </summary>
        public bool IsBusy { get; set; }

        /// <summary>
        /// Признак, указывающий, что выполняется анализ информации
        /// </summary>
        public bool IsAnalizingData { get; set; }

        /// <summary>
        /// Признак, указывающий, что данные загружены
        /// </summary>
        public bool IsDataLoaded { get; set; }
    }
}
