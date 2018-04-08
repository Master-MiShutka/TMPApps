using System.Collections.Generic;

namespace TMP.Work.Emcos.Model.Balans
{
    /// <summary>
    /// Описание энергии
    /// </summary>
    public interface IBaseEnergy
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
        IList<double?> DailyValues { get; set; }
        /// <summary>
        /// Суточные значения энергии со статусом
        /// </summary>
        IList<DataValue> DailyValuesWithStatus { get; }
        /// <summary>
        /// Сумма суточных значений энергии
        /// </summary>
        double? SummOfDaysValue { get; }

        /// <summary>
        /// Еимеется ли разница между энергией за месяц и суммой суточных
        /// </summary>
        bool HasDifferenceBetweenDailySumAndMonth { get; }
        /// <summary>
        /// Разница между энергией за месяц и суммой суточных
        /// </summary>
        double? DifferenceBetweenDailySumAndMonth { get; }
        /// <summary>
        /// Среднее суточных значений
        /// </summary>
        double? DailyValuesAverage { get; }
        /// <summary>
        /// Макимальное суточных значений
        /// </summary>
        double? DailyValuesMax { get; }
        /// <summary>
        /// Минимальное суточных значений
        /// </summary>
        double? DailyValuesMin { get; }
        /// <summary>
        /// Имеются данные не за все сутки
        /// </summary>
        bool NotFullData { get; }



        /// <summary>
        /// Инициализация
        /// </summary>
        void Init();

        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        ML_Param MonthMlParam { get; }
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        ML_Param DayhMlParam { get; }
    }
}
