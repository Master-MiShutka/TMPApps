namespace TMPApplication.Themes
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Определяет тему WPF приложения
    /// </summary>
    public class ThemeInfo : IThemeInfo
    {
        #region constructors
        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <param name="themeName"></param>
        /// <param name="themeSources"></param>
        public ThemeInfo(string themeName,
                         List<Uri> themeSources)
            : this()
        {
            DisplayName = themeName;

            if (themeSources != null)
            {
                foreach (var item in themeSources)
                    ThemeSources.Add(new Uri(item.OriginalString, UriKind.Relative));
            }
        }
        protected ThemeInfo()
        {
            DisplayName = string.Empty;
            ThemeSources = new List<Uri>();
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Возвращает название темы
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Возвращает список словарей-ресурсов темы
        /// </summary>
        public List<Uri> ThemeSources { get; private set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Добавление словаря-ресурсов темы
        /// </summary>
        /// <param name="additionalResource"></param>
        public void AddResources(List<Uri> additionalResource)
        {
            foreach (var item in additionalResource)
                ThemeSources.Add(item);
        }
        #endregion methods
    }
}
