namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using ItemsFilter;
    using TMP.Extensions;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;

    /// <summary>
    /// Модель представления для окна просмотра коллекции счётчиков
    /// </summary>
    public class ViewCollectionViewModel : BaseMeterViewModel
    {
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string fieldDisplayName;
        private string fieldName;
        private string fieldValue;

        private const string DEFAULT_DATA_GRID_MESSAGE = "Пусто";
        private string dataGridMessage = DEFAULT_DATA_GRID_MESSAGE;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="meters">Коллекция счётчиков</param>
        /// <param name="fieldDisplayName">Отображаемое имя поля, по которому сгруппирована или отфильтрована коллекция</param>
        /// <param name="fieldName">Ммя поля, по которому сгруппирована или отфильтрована коллекция</param>
        /// <param name="fieldValue">Значение, по которому отфильтрована коллекция</param>
        public ViewCollectionViewModel(IEnumerable<Meter> meters = null, string fieldDisplayName = null, string fieldName = null, string fieldValue = null)
            : base(meters)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            this.IsBusy = true;
            this.DataGridMessage = this.Status = "Подоговка данных ...";

            this.fieldDisplayName = fieldDisplayName;
            this.fieldName = fieldName;
            this.fieldValue = fieldValue;
        }

        #region Properties

        public string WindowTitle
        {
            get
            {
                string title = "Просмотр списка";
                if (this.Data != null && this.Data.Any() == false)
                {
                    // не указаны поля - просто просмотр коллекции
                    if (string.IsNullOrWhiteSpace(this.fieldValue) && string.IsNullOrWhiteSpace(this.fieldDisplayName))
                    {
                        int total = 0;
                        foreach (var item in this.View.SourceCollection)
                        {
                            total++;
                        }

                        int filtered = this.Data.Count();
                        if (total != filtered)
                        {
                            title += " : всего: " + total.ToString("N0", AppSettings.CurrentCulture) + "; найдено: " + filtered.ToString("N0", AppSettings.CurrentCulture);
                        }
                        else
                        {
                            title += " : всего: " + total.ToString("N0", AppSettings.CurrentCulture);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(this.fieldDisplayName) == false)
                    {
                        title += " : группировка по полю '" + this.fieldDisplayName + "'";
                    }

                    if (string.IsNullOrWhiteSpace(this.fieldValue) == false)
                    {
                        title += " : группировка по значению '" + this.fieldValue + "'";
                    }
                }

                return title;
            }
        }

        /// <summary>
        /// Заголовок отчета
        /// </summary>
        public override string ReportTitle => "Перечень электросчётчиков";

        /// <summary>
        /// Описание отчета
        /// </summary>
        public override string ReportDescription
        {
            get
            {
                string result = $"условия отбора:{Environment.NewLine}{this.ActiveFiltersList},{Environment.NewLine}";
                result += $"дата формирования отчёта: {DateTime.Now:dd MMMM yyyy} г.";

                return result;
            }
        }

        public string DataGridMessage { get => this.dataGridMessage; private set => this.SetProperty(ref this.dataGridMessage, value); }

        #endregion Properties

        protected override void OnCollectionFiltered(FilteredEventArgs e)
        {
            base.OnCollectionFiltered(e);
            this.RaisePropertyChanged(nameof(this.WindowTitle));
        }

        protected override void OnViewBuilded()
        {
            base.OnViewBuilded();

            if (string.IsNullOrWhiteSpace(this.fieldValue) == true && string.IsNullOrWhiteSpace(this.fieldName) == false)
            {
                this.SortAndGroupByField(this.fieldName);
            }

            this.DataGridMessage = DEFAULT_DATA_GRID_MESSAGE;
            this.IsBusy = false;
        }
    }
}