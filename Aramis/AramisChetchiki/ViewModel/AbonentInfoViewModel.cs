namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Windows.Input;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Model;
    using TMP.WORK.AramisChetchiki.Properties;

    public class AbonentInfoViewModel : BaseMeterViewModel
    {
        private ulong personalAccountFilter;
        private int selectedIndex;
        private NavigationMode selectedNavigationMode;

        private MeterViewViewModel meterViewViewModel;

        public AbonentInfoViewModel()
        {
            this.PreviousItem = new DelegateCommand(
                () =>
            {
                this.View.MoveCurrentToPrevious();
            },
                (o) => this.View != null);

            this.NextItem = new DelegateCommand(
                () =>
            {
                this.View.MoveCurrentToNext();
            },
                (o) => this.View != null);

            this.FindById = new DelegateCommand(
                () =>
            {
                // View.MoveCurrentToNext();
            },
                (o) => this.View != null);

            this.View?.MoveCurrentToFirst();
        }

        #region Commands

        public ICommand PreviousItem { get; }

        public ICommand NextItem { get; }

        public ICommand FindById { get; }

        #endregion

        #region Properties

        public ulong PersonalAccountFilter
        {
            get => this.personalAccountFilter;
            set
            {
                if (this.SetProperty(ref this.personalAccountFilter, value))
                {
                    this.RaisePropertyChanged(nameof(this.DataFilter));
                }
            }
        }

        public int SelectedIndex
        {
            get => this.selectedIndex;
            set
            {
                if (this.SetProperty(ref this.selectedIndex, value))
                {
                    this.View?.MoveCurrentToPosition(this.selectedIndex);
                }
            }
        }

        public NavigationMode SelectedNavigationMode { get => this.selectedNavigationMode; set => this.SetProperty(ref this.selectedNavigationMode, value); }

        public MeterViewViewModel MeterViewViewModel { get => this.meterViewViewModel; set => this.SetProperty(ref this.meterViewViewModel, value); }

        #endregion

        #region Methods

        public override Predicate<Meter> DataFilter => (meter) =>
        {
            if (this.PersonalAccountFilter == 0)
            {
                return true;
            }
            else
            {
                return meter.Лицевой.ToString(AppSettings.CurrentCulture).StartsWith(this.PersonalAccountFilter.ToString(AppSettings.CurrentCulture), AppSettings.StringComparisonMethod);
            }
        };

        protected override void OnViewBuilded()
        {
            base.OnViewBuilded();

            this.View.CurrentChanged += this.View_CurrentChanged;
        }

        private void View_CurrentChanged(object sender, EventArgs e)
        {
            this.MeterViewViewModel = new MeterViewViewModel((Meter)this.View.CurrentItem);
        }

        #endregion
    }
}
