using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Common.Logger
{
    public interface ILoggerFacade
    {
        /// <summary>
        /// Запись нового сообщения указанной категории и приоритета
        /// </summary>
        /// <param name="message">Сообщение для журнала</param>
        /// <param name="category">Категория сообщения</param>
        /// <param name="priority">Приоритет сообщения</param>
        void Log(string message, Category category = Category.Info, Priority priority = Priority.None);
        /// <summary>
        /// Запись исключения
        /// </summary>
        /// <param name="e"></param>
        void Log(Exception e);
    }
}
