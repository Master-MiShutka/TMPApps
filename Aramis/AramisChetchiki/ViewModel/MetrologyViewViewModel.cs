namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Windows;
    using TMP.Extensions;
    using TMP.WORK.AramisChetchiki.Model;

    public class MetrologyViewViewModel : BaseMeterViewModel
    {
        #region Fields

        #endregion

        #region Constructors

        public MetrologyViewViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                // _mainViewModel = new MainViewModel();
                this.RaisePropertyChanged(nameof(this.View));
                return;
            }

            this.CommandPrint = null;
        }

        #endregion

        #region Properties

        #endregion

        #region Override Properties

        public override string ReportTitle => "График метрологической замены электросчётчиков";

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

        #region Override Methods

        protected override void OnViewBuilded()
        {
            if (this.FilterPresenter == null)
            {
                this.IsBusy = false;
                return;
            }

            /*string[] filters = new[] {
                nameof(Meter.СельскийСовет),
                nameof(Meter.НаселённыйПункт),
                nameof(Meter.ТипНаселённойМестности),
                nameof(Meter.Фаз),
                nameof(Meter.Принцип),
                nameof(Meter.Аскуэ),
                nameof(Meter.НаБалансеАбонента),
                nameof(Meter.Поверен),
            };

            // добавление фильтров
            TMPApplication.DispatcherExtensions.InUi(() =>
            {
                foreach (string filter in filters)
                {
                    this.Filters.Add((ItemsFilter.Model.IMultiValueFilter)this.FilterPresenter.TryGetFilter(filter, new ItemsFilter.Initializer.EqualFilterInitializer()));
                }

                this.IsBusy = false;
            });*/
        }

        #endregion

        #region Private Methods

        #endregion

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0459");
            return guid.GetHashCode();
        }
    }
}
