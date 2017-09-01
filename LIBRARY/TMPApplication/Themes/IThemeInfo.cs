namespace TMPApplication.Themes
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Описывает тему WPF приложения с именем и списком адресов словарей-ресурсов
    /// </summary>
    public interface IThemeInfo
    {
        #region properties
        /// <summary>
        /// Возвращает имя темы
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Возвращает список адресов словарей-ресурсов
        /// </summary>
        List<Uri> ThemeSources { get; }
        #endregion properties

        #region methods
        /// <summary>
        /// Добавление дополнительного словаря-ресурсов к существующим
        /// </summary>
        /// <param name="additionalResource"></param>
        void AddResources(List<Uri> additionalResource);
        #endregion methods
    }
}
