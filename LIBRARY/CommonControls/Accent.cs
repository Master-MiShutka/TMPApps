using System;
using System.Diagnostics;
using System.Windows;

namespace TMP.Wpf.CommonControls
{
    /// <summary>
    /// Объект, представляющий цвет переднего плана <see cref="AppTheme"/>.
    /// </summary>
    [DebuggerDisplay("accent={Name}, res={Resources.Source}")]
    public class Accent
    {
        /// <summary>
        /// Словарь ресурсов, представляющий этот акцент
        /// </summary>
        public ResourceDictionary Resources;

        /// <summary>
        /// Возвращает/задает имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Инициализация нового экземпляра класса
        /// </summary>
        public Accent()
        { }

        /// <summary>
        /// Инициализация нового экземпляра класса
        /// </summary>
        /// <param name="name">Имя нового акцента</param>
        /// <param name="resourceAddress">URI словаря ресурсов акцента</param>
        public Accent(string name, Uri resourceAddress)
        {
            Name = name;
            Resources = new ResourceDictionary { Source = resourceAddress };
        }
    }
}