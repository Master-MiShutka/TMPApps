using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMP.Shared;

namespace TMP.Work.Emcos.Model.Balance
{
    public interface IBalanceItem : IHierarchicalEmcosPoint, System.ComponentModel.INotifyPropertyChanged, ITreeModel
    {
        /// <summary>
        /// Ссылка на подстанцию
        /// </summary>
        Substation Substation { get; }
        void SetSubstation(Substation substation);

        /// <summary>
        /// Используются данные за месяц, а не сумма суточных
        /// </summary>
        bool UseMonthValue { get; set; }
        /// <summary>
        /// Активная энергия
        /// </summary>
        ActiveEnergy ActiveEnergy { get; set; }
        /// <summary>
        /// Реактивная энергия
        /// </summary>
        ReactiveEnergy ReactiveEnergy { get; set; }

        /// <summary>
        /// Очистка значений энергий
        /// </summary>
        void ClearData();
        /// <summary>
        /// Возвращает копию элемента
        /// </summary>
        /// <returns>Копия элемента</returns>
        IBalanceItem Copy();
    }
}
