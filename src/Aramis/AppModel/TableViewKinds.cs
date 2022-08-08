using System.ComponentModel;

namespace TMP.WORK.AramisChetchiki.Model
{
    /// <summary>
    /// Перечисление вариантов отображения таблицы с данными
    /// </summary>
    public enum TableViewKinds
    {
        [Description("базовый")]
        BaseView,
        [Description("подробный")]
        DetailedView,
        [Description("краткий")]
        ShortView,
        [Description("оплата")]
        ОплатаView,
        [Description("привязка")]
        ПривязкаView,
        [Description("полный")]
        ПолныйView,
    }
}
