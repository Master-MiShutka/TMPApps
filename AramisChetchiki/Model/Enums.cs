using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TMP.WORK.AramisChetchiki.Model
{
    public enum Mode
    {
        [Description("Список счётчиков")]
        MetersList,
        [Description("Отчет по заменам")]
        ChangesOfMeters,
        [Description("Привязка абонентов")]
        AbonentsBinding,
        [Description("Сведения по поверке учёта")]
        Metrology,
        [Description("Сведения учётам")]
        MetersInfo
    }

    /// <summary>
    /// Перечисление вариантов отображения сводной информации
    /// </summary>
    public enum InfoViewType
    {
        [Description("Список")]
        ViewAsList,
        [Description("Таблица")]
        ViewAsTable,
        [Description("Диаграмма")]
        ViewAsDiagram
    }

    public enum TableView
    {
        [Description("по умолчанию")]
        BaseView,
        [Description("подробный")]
        DetailedView,
        [Description("краткий")]
        ShortView,
        [Description("оплата")]
        ОплатаView,
        [Description("привязка")]
        ПривязкаView
    }
}
