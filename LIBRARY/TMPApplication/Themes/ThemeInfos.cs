namespace TMPApplication.Themes
{
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Определение класса, управляющего информацией о темах
    /// </summary>
    public class ThemeInfos : IThemeInfos
    {
        Dictionary<string, IThemeInfo> mDic = new Dictionary<string, IThemeInfo>();


        public void AddThemeInfo(IThemeInfo theme)
        {
            mDic.Add(theme.DisplayName, theme);
        }

        /// <summary>
        /// Добавление новой темы
        /// </summary>
        /// <param name="name"></param>
        /// <param name="themeSources"></param>
        public void AddThemeInfo(string name, List<Uri> themeSources)
        {
            mDic.Add(name, new ThemeInfo(name, themeSources));
        }

        /// <summary>
        /// Возвращает тему с указанным именем.
        /// Возвращает null, если тема отсутствует.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IThemeInfo GetThemeInfo(string name)
        {
            IThemeInfo ret = null;
            mDic.TryGetValue(name, out ret);

            return ret;
        }

        /// <summary>
        /// Удаление существующей темы
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IThemeInfo RemoveThemeInfo(string name)
        {
            IThemeInfo ret = null;
            if (mDic.TryGetValue(name, out ret) == true)
            {
                mDic.Remove(name);
                return ret;
            }

            return ret;
        }

        /// <summary>
        /// Удаление всех тем
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public void RemoveAllThemeInfos()
        {
            mDic.Clear();
        }

        /// <summary>
        /// Перечисление имеющихся тем
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IThemeInfo> GetThemeInfos()
        {
            foreach (var item in mDic.Values)
                yield return item;
        }
    }
}
