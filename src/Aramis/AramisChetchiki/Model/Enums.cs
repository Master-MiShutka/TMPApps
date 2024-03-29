﻿namespace TMP.WORK.AramisChetchiki.Model
{
    using System.ComponentModel;
    using TMP.Shared;

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

    /// <summary>
    /// Перечисление вариантов отображения сводной информации
    /// </summary>
    public enum InfoViewType
    {
        /// <summary>
        /// В виде списка
        /// </summary>
        [Description("Список")]
        ViewAsList,

        /// <summary>
        /// В виде таблицы
        /// </summary>
        [Description("Таблица")]
        ViewAsTable,

        /// <summary>
        /// В виде диаграмм
        /// </summary>
        [Description("Диаграмма")]
        ViewAsDiagram,
    }

    /// <summary>
    /// Перечисление вариантов отображения таблицы с данными
    /// </summary>
    public enum TableViewKinds
    {
        [Description("базовый")]
        BaseView,
        [Description("подробный")]
        DetailedView,
        [Description("краткий")]
        ShortView,
        [Description("оплата")]
        ОплатаView,
        [Description("привязка")]
        ПривязкаView,
        [Description("полный")]
        ПолныйView,
    }

    /// <summary>
    /// Перечисление типов навигации в AbonentInfoViewModel
    /// </summary>
    public enum NavigationMode
    {
        [Description("по лицевому счёту")]
        ByPersonalAccount,
        [Description("по адресу")]
        ByAddress,
        [Description("по привязке")]
        ByElectricalAddress,
    }

    /// <summary>
    /// Перечисление вариантов формирования списка привязки абонентов
    /// </summary>
    public enum AbonentsBindingTreeMode
    {
        [Description("Подстанция ⇒ Фидер 10 кВ ⇒ ТП 10/0,4 кВ ⇒ Фидер 0,4 кВ")]
        Full,
        [Description("Фидер 10 кВ ⇒ ТП 10/0,4 кВ ⇒ Фидер 0,4 кВ")]
        WithoutSubstation,
        [Description("ТП 10/0,4 кВ ⇒ Фидер 0,4 кВ")]
        WithoutFider10,
    }



    /// <summary>
    /// Тип элемента, используемый в привязке абонента к электросети
    /// </summary>
    public enum NodeType
    {
        Substation,
        Fider10,
        TP,
        Fider04,
        Group,
        Departament,
    }
}
