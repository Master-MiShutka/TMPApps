namespace TMP.WORK.AramisChetchiki.Model
{
    public enum Mode
    {
        /// <summary>
        /// Выбор файла с данными
        /// </summary>
        [Description("Инициализация")]
        [ViewModel(typeof(ViewModel.StartViewModel))]
        [View(typeof(Views.StartView))]
        [Browsable(false)]
        None,

        /// <summary>
        /// Загрузка файла с данными
        /// </summary>
        [Description("Загрузка данных")]
        [ViewModel(typeof(ViewModel.LoadingDataViewModel))]
        [View(typeof(Views.LoadingDataView))]
        [Browsable(false)]
        LoadingData,

        /// <summary>
        /// Параметры программы
        /// </summary>
        [View(typeof(Views.PreferencesView))]
        [ViewModel(typeof(ViewModel.PreferencesViewModel))]
        [Description("Параметры")]
        [Browsable(true)]
        Preferences,

        /// <summary>
        /// Начало
        /// </summary>
        [View(typeof(Views.HomeView))]
        [ViewModel(typeof(ViewModel.HomeViewModel))]
        [Description("Начало")]
        [Browsable(false)]
        Home,

        /// <summary>
        /// Свод
        /// </summary>
        [Description("Свод")]
        [View(typeof(Views.SummaryInfoView))]
        [ViewModel(typeof(ViewModel.SummaryInfoViewViewModel))]
        [Browsable(true)]
        SummaryInfo,

        /// <summary>
        /// Отчет по заменам
        /// </summary>
        [Description("Просмотр замен счётчиков")]
        [View(typeof(Views.ChangesOfMetersView))]
        [ViewModel(typeof(ViewModel.ChangesOfMetersViewModel))]
        [Browsable(true)]
        ChangesOfMeters,

        /// <summary>
        /// Привязка абонентов
        /// </summary>
        [Description("Привязка абонентов")]
        [View(typeof(Views.AbonentsBindingView))]
        [ViewModel(typeof(ViewModel.AbonentsBindingViewViewModel))]
        [Browsable(true)]
        AbonentsBinding,

        /// <summary>
        /// Сведения по счётчикам
        /// </summary>
        [Description("Сведения по счётчикам")]
        [View(typeof(Views.ViewCollectionView))]
        [ViewModel(typeof(ViewModel.ViewCollectionViewModel))]
        [Browsable(true)]
        ViewMeters,

        /// <summary>
        /// Полезный отпуск
        /// </summary>
        [Description("Оплата электроэнергии и пофидерный анализ")]
        [View(typeof(Views.PaymentsAndPofiderAnalizView))]
        [ViewModel(typeof(ViewModel.PaymentsAndPofiderAnalizViewModel))]
        [Browsable(true)]
        ElectricitySupply,

        [Description("Просмотр информации по абоненту")]
        [View(typeof(Views.AbonentInfoView))]
        [ViewModel(typeof(ViewModel.AbonentInfoViewModel))]
        [Browsable(true)]
        AbonentInfo,
    }
}
