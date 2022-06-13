namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Windows.Input;

    [DataContract]
    public abstract class BaseViewModel : BaseMainViewModel, IViewModel
    {
        protected BaseViewModel()
        {
            this.SubscribeMainViewModel();
        }

        internal ItemsFilter.FilterPresenter FilterPresenter { get; set; }

        protected static IMainViewModel MainViewModel => WindowWithDialogs.BaseApplication.Instance?.MainViewModel as IMainViewModel;

        /// <summary>
        /// Признак, указывающий, что данные загружены
        /// </summary>
        public virtual bool IsDataLoaded => MainViewModel?.Data != null;

        /// <summary>
        /// Команда экспорта данных
        /// </summary>
        public virtual ICommand CommandExport { get; protected set; }

        /// <summary>
        /// Команда печати данных
        /// </summary>
        public virtual ICommand CommandPrint { get; protected set; }

        public ICommand CommandSetSorting { get; protected set; }

        internal void SubscribeMainViewModel()
        {
            if (MainViewModel != null)
            {
                MainViewModel.PropertyChanged += this.MainViewModel_PropertyChanged;
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            if (MainViewModel != null)
            {
                MainViewModel.PropertyChanged -= this.MainViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Обработчик событий MainViewModel
        /// </summary>
        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsInitialized":
                    this.OnInitialized();
                    break;
                case "IsDataLoaded":
                    this.RaisePropertyChanged(nameof(this.IsDataLoaded));
                    break;
                case "Data":
                    this.RaisePropertyChanged(nameof(this.IsDataLoaded));
                    this.OnDataLoaded();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Вызывается при изменении Data в MainViewModel
        /// </summary>
        protected virtual void OnDataLoaded()
        {
        }

        /// <summary>
        /// Вызывается после завершения инициализации MainViewModel
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0450");
            return guid.GetHashCode();
        }
    }
}
