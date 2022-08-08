using System.Collections.Generic;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Описание энергии определённого направления - приём или отдача (плюс или минус)
    /// </summary>
    public interface IDirectedEnergy
    {
        /// <summary>
        /// Описание
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Краткое описание
        /// </summary>
        string ShortDescription { get; }

        /// <summary>
        /// Используются данные за месяц, а не сумма суточных
        /// </summary>
        bool UseMonthValue { get; set; }
        /// <summary>
        /// Энергия без корректрировки
        /// </summary>
        double? Value { get; }
        /// <summary>
        /// Энергия с корректрировкой
        /// </summary>
        double? CorrectedValue { get; }
        /// <summary>
        /// Возвращает величину корректировки энергии
        /// </summary>
        double? CorrectionValue { get; set; }
        /// <summary>
        /// Описание корректировки
        /// </summary>
        string Correction { get; }
        /// <summary>
        /// Энергия за месяц
        /// </summary>
        double? MonthValue { get; set; }
        /// <summary>
        /// Суточные значения энергии
        /// </summary>
        IList<double?> DaysValues { get; set; }
        /// <summary>
        /// Суточные значения энергии со статусом
        /// </summary>
        IList<DataValue> DaysValuesWithStatus { get; }
        /// <summary>
        /// Статус данных за сутки
        /// </summary>
        string DaysValuesStatus { get; }
        /// <summary>
        /// Имеются ли отсутствующие данные хотя бы за одни сутки
        /// </summary>
        bool DaysValuesHasMissing { get; }


        /// <summary>
        /// Сумма суточных значений энергии
        /// </summary>
        double? SummOfDaysValue { get; }

        /// <summary>
        /// Еимеется ли разница между энергией за месяц и суммой суточных
        /// </summary>
        bool HasDifferenceBetweenDaysSumAndMonth { get; }
        /// <summary>
        /// Разница между энергией за месяц и суммой суточных
        /// </summary>
        double? DifferenceBetweenDaysSumAndMonth { get; }
        /// <summary>
        /// Среднее суточных значений
        /// </summary>
        double? DaysValuesAverage { get; }
        /// <summary>
        /// Макимальное суточных значений
        /// </summary>
        double? DaysValuesMax { get; }
        /// <summary>
        /// Минимальное суточных значений
        /// </summary>
        double? DaysValuesMin { get; }
        /// <summary>
        /// Имеются данные не за все сутки
        /// </summary>
        bool NotFullData { get; }



        /// <summary>
        /// Очистка значений
        /// </summary>
        void ClearData();

        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        ML_Param MonthMlParam { get; }
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        ML_Param DayMlParam { get; }
    }
}
