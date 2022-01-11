using System.Collections.Generic;
using System.ComponentModel;

namespace TMP.Work.Emcos.Model.Balance
{
    public interface IBalance : INotifyPropertyChanged
    {
        void Initialize(DatePeriod period);
        /// <summary>
        /// Ссылка на подстанцию
        /// </summary>
        Substation Substation { get; }
        /// <summary>
        /// Группа элементов для расчёта баланса
        /// </summary>
        IBalanceGroupItem BalanceGroup { get; }
        /// <summary>
        /// Выбранный тип энергии
        /// </summary>
        IEnergy Energy { get; }
        /// <summary>
        /// Приём по вводам
        /// </summary>
        double? VvodaIn { get; }
        /// <summary>
        /// Отдача по вводам
        /// </summary>
        double? VvodaOut { get; }
        /// <summary>
        /// Приём по фидерам
        /// </summary>
        double? FideraIn { get; }
        /// <summary>
        /// Отдача по фидерам
        /// </summary>
        double? FideraOut { get; }
        /// <summary>
        /// Приём по трансформаторам собственных нужд, подключённым к шинам
        /// </summary>
        double? TsnIn { get; }
        /// <summary>
        /// Отдача по трансформаторам собственных нужд, подключённым к шинам
        /// </summary>
        double? TsnOut { get; }
        /// <summary>
        /// Приём энергии
        /// </summary>
        double? EnergyIn { get; }
        /// <summary>
        /// Отдача энергии
        /// </summary>
        double? EnergyOut { get; }
        /// <summary>
        /// Небаланс
        /// </summary>
        double? Unbalance { get; }
        /// <summary>
        /// Процент небаланса
        /// </summary>
        double? PercentageOfUnbalance { get; }
        /// <summary>
        /// Допустимый процент небаланса
        /// </summary>
        double? MaximumAllowableUnbalance { get; set; }
        /// <summary>
        /// Превышает ли небаланс 3%
        /// </summary>
        bool ExcessUnbalance { get; }
        /// <summary>
        /// Описание
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Возвращает описание произведенных корректировок
        /// </summary>
        string Correction { get; }
        /// <summary>
        /// Список дочерних элементов группы, у которых имеется разница между суммой суточных и данными за месяц
        /// </summary>
        string DifferenceBetweenDailySumAndMonthToolTip { get; }

        #region Properties for day balance view

        DatePeriod Period { get; set; }

        int TransformersCount { get; set; }
        int AuxCount { get; set; }
        int FidersCount { get; set; }

        IList<HeaderElement> Headers { get;  }
        IList<DayBalance> Items { get; }

        IList<double?> Min { get;  }
        IList<double?> Max { get;  }
        IList<double?> Average { get; }
        IList<double?> Sum { get; }

        #endregion

    }
}
