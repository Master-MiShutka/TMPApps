namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System.Collections.Generic;
    using System.Linq;
    using TMP.WORK.AramisChetchiki.Model;

    public interface IMainViewModel : WindowWithDialogs.IMainViewModel
    {
        /// <summary>
        /// Команда для закрытия активного окна
        /// </summary>
        System.Windows.Input.ICommand CommandCloseWindow { get; }

        void ShowAllMeters();

        void ShowMetersCollection(System.Collections.Generic.IEnumerable<Meter> meters);

        void ShowMetersWithGroupingAtField(string fieldName);

        void ShowMeterFilteredByFieldValue(string fieldName, string value);

        /// <summary>
        /// Данные
        /// </summary>
        AramisData Data { get; }

        /// <summary>
        /// Коллекция счётчиков
        /// </summary>
        System.Collections.Generic.IEnumerable<Meter> Meters { get; }

        /// <summary>
        /// Коллекция удаленных счётчиков
        /// </summary>
        System.Collections.Generic.IEnumerable<Meter> DeletedMeters => this.Meters.Where(i => i.Удалён == true);

        /// <summary>
        /// Ссылка на модель данных отображаемого представления данных
        /// </summary>
        IViewModel CurrentViewModel { get; }

        /// <summary>
        /// Текущий режим работы
        /// </summary>
        Mode CurrentMode { get; }

        /// <summary>
        /// Выбранный файл данных
        /// </summary>
        Model.AramisDataInfo SelectedDataFileInfo { get; set; }

        /// <summary>
        /// Смена текущего режима работы
        /// </summary>
        /// <param name="newMode">новый режим</param>
        void ChangeMode(Mode newMode);

        /// <summary>
        /// Команда возврата к начальному экрану
        /// </summary>
        System.Windows.Input.ICommand CommandGoHome { get; }

        /// <summary>
        /// Отображает предыдущий режим
        /// </summary>
        void GoBack();

        /// <summary>
        /// Переход к начальному экрану, когда нет данных
        /// </summary>
        void GoStart();

        /// <summary>
        /// Отображает стартовую страницу
        /// </summary>
        void GoHome();

        /// <summary>
        /// Отображение указанной страницы параметров
        /// </summary>
        /// <param name="settingsPage"></param>
        void ShowSettingsPage(Model.ISettingsPage settingsPage);
    }
}
