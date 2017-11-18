namespace TMPApplication.Themes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Интерфейс для классов-менеджеров тем
    /// </summary>
    public interface IThemeInfos
    {
        void AddThemeInfo(IThemeInfo theme);

        /// <summary>
        /// Добавление темы
        /// </summary>
        /// <param name="name"></param>
        /// <param name="themeSources"></param>
        void AddThemeInfo(string name, List<Uri> themeSources);

        /// <summary>
        /// Возвращает тему с указанным именем.
        /// Возвращает null, если тема отсутствует.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IThemeInfo GetThemeInfo(string name);

        /// <summary>
        /// Удаление существующей темы
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IThemeInfo RemoveThemeInfo(string name);

        /// <summary>
        /// Удаление всех тем
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        void RemoveAllThemeInfos();

        /// <summary>
        /// Перечисление имеющихся тем
        /// </summary>
        /// <returns></returns>
        IEnumerable<IThemeInfo> GetThemeInfos();
    }
}
