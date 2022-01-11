using System.Collections.Generic;
using TMP.Shared;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Интерфейс, описывающий группу для расчёта баланса энергии
    /// </summary>
    public interface IBalanceGroupItem : IBalanceItem, ITreeModel
    {
        /// <summary>
        /// Плоский список дочерних элементов
        /// </summary>
        IList<IBalanceItem> Items { get; }
        /// <summary>
        /// Формула, задающая порядок расчета баланса
        /// </summary>
        BalanceFormula Formula { get; set; }
        /// <summary>
        /// Баланс активной энергии
        /// </summary>
        Balance<ActiveEnergy> ActiveEnergyBalance { get; set; }
        /// <summary>
        /// Баланс реактивной энергии
        /// </summary>
        Balance<ReactiveEnergy> ReactiveEnergyBalance { get; set; }

        void Cleanup();
        void RecalculateBalance();
    }
}
