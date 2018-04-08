using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balans
{
    public interface IBalansItem : IEmcosPoint, System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Статус данных
        /// </summary>
        DataStatus Status { get; set; }
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
        /// Возвращает описание произведенных корректировок
        /// </summary>
        string Correction { get; }
        /// <summary>
        /// Активная энергия
        /// </summary>
        ActiveEnergy ActiveEnergy { get; set; }
        /// <summary>
        /// Реактивная энергия
        /// </summary>
        ReactiveEnergy ReactiveEnergy { get; set; }


        /*bool NotFullData { get; }
        bool NotFullDataPlus { get; }
        bool NotFullDataMinus { get; }
        string DataPlusStatus { get; }
        string DataMinusStatus { get; }*/

        /// <summary>
        /// Очистка значений энергий
        /// </summary>
        void Clear();
        /// <summary>
        /// Возвращает копию элемента
        /// </summary>
        /// <returns>Копия элемента</returns>
        IBalansItem Copy();
    }
}
