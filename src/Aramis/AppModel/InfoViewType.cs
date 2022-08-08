using System.ComponentModel;

namespace TMP.WORK.AramisChetchiki.Model
{
    /// <summary>
    /// Перечисление вариантов отображения сводной информации
    /// </summary>
    public enum InfoViewType
    {
        /// <summary>
        /// В виде списка
        /// </summary>
        [Description("Список")]
        ViewAsList,

        /// <summary>
        /// В виде таблицы
        /// </summary>
        [Description("Таблица")]
        ViewAsTable,

        /// <summary>
        /// В виде диаграмм
        /// </summary>
        [Description("Диаграмма")]
        ViewAsDiagram,
    }
}
