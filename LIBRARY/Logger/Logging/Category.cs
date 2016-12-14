using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Common.Logger
{
    /// <summary>
    /// Описание категории записи в журнале
    /// </summary>
    public enum Category
    {
        /// <summary>
        /// Отладка
        /// </summary>
        Debug,

        /// <summary>
        /// Исключение
        /// </summary>
        Exception,

        /// <summary>
        /// Информация
        /// </summary>
        Info,

        /// <summary>
        /// Предостережение
        /// </summary>
        Warn
    }
}
