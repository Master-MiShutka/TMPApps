using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    using Balance;
    public interface IBalanceSession
    {
        BalanceSessionInfo Info { get; set; }

        //SubstationsCollection Substations { get; set; }

        /// <summary>
        /// Коллекция элементов
        /// </summary>
        HierarchicalEmcosPointCollection BalancePoints { get; }

        /// <summary>
        /// Словарь пар код группы - баланс активной энергии
        /// </summary>
        Dictionary<int, Balance<ActiveEnergy>> ActiveEnergyBalanceById { get; }
        /// <summary>
        /// Словарь пар код группы - баланс реактивной энергии
        /// </summary>
        Dictionary<int, Balance<ReactiveEnergy>> ReactiveEnergyBalanceById { get; }
        /// <summary>
        /// Словарь пар код элемента - активная энергия
        /// </summary>
        Dictionary<int, ActiveEnergy> ActiveEnergyById { get; }
        /// <summary>
        /// Словарь пар код элемента - реактивная энергия
        /// </summary>
        Dictionary<int, ReactiveEnergy> ReactiveEnergyById { get; }
        /// <summary>
        /// Словарь пар код элемента - описание
        /// </summary>
        Dictionary<int, string> DescriptionsById { get; }
        /// <summary>
        /// Список подразделений
        /// </summary>
        IList<IHierarchicalEmcosPoint> Departaments { get; }
    }
}
