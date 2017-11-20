using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Common.Logger
{
    /// <summary>
    /// Определяет приоритет записей <см cref="ILoggerFacade"/>.
    /// </summary>
    public enum Priority
    {
        /// <summary>
        /// Нет приоритета
        /// </summary>
        None = 0,

        /// <summary>
        /// Высокий приоритет
        /// </summary>
        High = 1,

        /// <summary>
        /// Средний приоритет
        /// </summary>
        Medium,

        /// <summary>
        /// Низкий приоритет
        /// </summary>
        Low
    }

}
