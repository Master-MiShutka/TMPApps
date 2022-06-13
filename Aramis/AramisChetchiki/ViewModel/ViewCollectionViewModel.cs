namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using ItemsFilter;
    using TMP.Extensions;
    using TMP.WORK.AramisChetchiki.Model;

    /// <summary>
    /// Модель представления для окна просмотра коллекции счётчиков
    /// </summary>
    public class ViewCollectionViewModel : BaseMeterViewModel
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private string fieldDisplayName;
        private string fieldName;
        private string fieldValue;

        private const string DEFAULT_DATA_GRID_MESSAGE = "Пусто";
        private string dataGridMessage = DEFAULT_DATA_GRID_MESSAGE;

        #endregion

        #region Constructors

        public ViewCollectionViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.RaisePropertyChanged(nameof(this.View));
                return;
            }

            this.DataGridMessage = this.Status = "Подготовка данных ...";

            this.logger.Info("Constructor");
        }

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

            this.logger.Info("Constructor with collection");

            this.IsBusy = true;
            this.DataGridMessage = this.Status = "Подготовка данных ...";

            this.fieldDisplayName = fieldDisplayName;
            this.fieldName = fieldName;
            this.fieldValue = fieldValue;
        }

        #endregion

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
                        foreach (object item in this.View.SourceCollection)
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

        public string DataGridMessage { get => this.dataGridMessage; private set => this.SetProperty(ref this.dataGridMessage, value); }

        #region Override Properties

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
                string result = $"параметры: с {Meter.ДатаСравненияПоверки.GetQuarter()} кв {Meter.ДатаСравненияПоверки.Year} г счётчик считается не поверенным,{Environment.NewLine}";
                if (string.IsNullOrWhiteSpace(this.ActiveFiltersList) == false)
                {
                    result += $"условия отбора:{Environment.NewLine}{this.ActiveFiltersList},{Environment.NewLine}";
                }

                result += $"дата формирования отчёта: {DateTime.Now:dd MMMM yyyy} г.";

                return result;
            }
        }

        #endregion

        #endregion Properties

        protected override void OnCollectionFiltered(FilteredEventArgs e)
        {
            base.OnCollectionFiltered(e);
            this.RaisePropertyChanged(nameof(this.WindowTitle));
        }

        protected override void OnViewBuilded()
        {
            base.OnViewBuilded();

            if (this.FilterPresenter == null)
            {
                this.IsBusy = false;
                return;
            }

            string[] filters = new[] {
                nameof(Meter.СельскийСовет),
                nameof(Meter.НаселённыйПункт),
                nameof(Meter.ТипНаселённойМестности),
                nameof(Meter.Фаз),
                nameof(Meter.Принцип),
                nameof(Meter.Аскуэ),
                nameof(Meter.НаБалансеАбонента),
                nameof(Meter.Поверен),
                nameof(Meter.ЭтоМжд),
                nameof(Meter.ЕстьДолг),
                nameof(Meter.Задолженник),
                nameof(Meter.НетОплатыДваПериода),
            };

            // добавление фильтров
            WindowWithDialogs.DispatcherExtensions.InUi(() =>
            {
                foreach (string filter in filters)
                {
                    this.Filters.Add((ItemsFilter.Model.IMultiValueFilter)this.FilterPresenter.TryGetFilter(filter, new ItemsFilter.Initializer.EqualFilterInitializer()));
                }

                this.IsBusy = false;
            });

            if (string.IsNullOrWhiteSpace(this.fieldValue) == true && string.IsNullOrWhiteSpace(this.fieldName) == false)
            {
                this.SortAndGroupByField(this.fieldName);
            }

            this.DataGridMessage = DEFAULT_DATA_GRID_MESSAGE;
            this.IsBusy = false;
        }

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0463");
            return guid.GetHashCode();
        }
    }
}