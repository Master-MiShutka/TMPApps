using System.Collections.Generic;

namespace TMP.Work.Emcos.Model.Balans
{
    public interface IBalansGroup : IBalansItem
    {
        /// <summary>
        /// Дочерние элементы группы
        /// </summary>
        ICollection<IBalansItem> Children { get; set; }
        //
        //IList<IBalansItem> Items { get; }
        /// <summary>
        /// Формула, задающая порядок расчета баланса
        /// </summary>
        BalanceFormula Formula { get; set; }

        void UpdateChildren();
    }
}
