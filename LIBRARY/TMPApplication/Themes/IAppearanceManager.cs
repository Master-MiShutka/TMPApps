namespace TMPApplication.Themes
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    public interface IAppearanceManager
    {
        #region properties
        /// <summary>
        /// Возврашает имя текущей выбранной темы
        /// </summary>
        string ThemeName { get; }

        /// <summary>
        /// Возвращает список словарей-ресурсов текущей темы
        /// </summary>
        List<Uri> ThemeSources { get; }

        Color AccentColor { get; }
        #endregion properties

        #region events
        event ColorChangedEventHandler AccentColorChanged;
        #endregion events

        #region methods
        /// <summary>
        /// Возвращает тему по умолчанию для приложения
        /// </summary>
        /// <returns></returns>
        IThemeInfo GetDefaultTheme();

        /// <summary>
        /// Задает новую тему для приложения
        /// </summary>
        /// <param name="theme">Тема</param>
        /// <param name="AccentColor">Акцентирующий цвет</param>
        void SetTheme(IThemeInfo theme, Color AccentColor);

        /// <summary>
        /// Очистка списка тем и устновка нового списка тем по умолчанию
        /// </summary>
        /// <param name="themes"></param>
        void Reset(List<IThemeInfo> themes = null);

        #endregion methods
    }
}
