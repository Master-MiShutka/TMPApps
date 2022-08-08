using System;
using System.Diagnostics;
using System.Windows;

namespace TMP.Wpf.CommonControls
{
    /// <summary>
    /// Представляет фон темы приложения
    /// </summary>
    [DebuggerDisplay("apptheme={Name}, theme={Theme}, res={Resources.Source}")]
    public class AppTheme
    {
        /// <summary>
        /// Словарь ресурсов, представляющий тему приложения
        /// </summary>
        public ResourceDictionary Resources { get; private set; }

        /// <summary>
        /// Возвращает имя темы приложения
        /// </summary>
        public string Name { get; private set; }

        public AppTheme(string name, Uri resourceAddress)
        {
            if (name == null)
                throw new ArgumentException("name");

            if (resourceAddress == null)
                throw new ArgumentNullException(nameof(resourceAddress));

            this.Name = name;
            this.Resources = new ResourceDictionary { Source = resourceAddress };
        }
    }
}