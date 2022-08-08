using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections;
using TMP.Shared;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Базовый класс, представляющий элемент баланса энергии
    /// </summary>
    [DataContract(Name = "BalanceItem")]
    public abstract class BalanceItem : EmcosPoint, IBalanceItem, ITreeModel
    {
        #region Fields

        #endregion

        #region Constructor

        public BalanceItem()
        {
            UseMonthValue = false;
            ActiveEnergy = new ActiveEnergy();
            ReactiveEnergy = new ReactiveEnergy();
        }

        #endregion

        #region IBalanceItem implementation

        /// <summary>
        /// Ссылка на подстанцию
        /// </summary>
        [Magic]
        public Substation Substation { get; private set; }
        /// <summary>
        /// Используются данные за месяц, а не сумма суточных
        /// </summary>
        [DataMember()]
        [Magic]
        public bool UseMonthValue { get; set; }
        /// <summary>
        /// Активная энергия
        /// </summary>
        [Magic]
        public ActiveEnergy ActiveEnergy { get; set; }
        /// <summary>
        /// Реактивная энергия
        /// </summary>
        [Magic]
        public ReactiveEnergy ReactiveEnergy { get; set; }

        #endregion

        #region Public methods

        public void SetSubstation(Substation substation)
        {
            Substation = substation;
        }
        /// <summary>
        /// Очистка значений энергий
        /// </summary>
        public void ClearData()
        {
            UseMonthValue = false;
            ActiveEnergy.ClearData();
            ReactiveEnergy.ClearData();

            if (Children != null)
                foreach (IHierarchicalEmcosPoint item in Children)
                    if (item is IBalanceItem bi) 
                        bi.ClearData();

        }
        /// <summary>
        /// Возвращает копию элемента
        /// </summary>
        /// <returns>Копия элемента</returns>
        public virtual IBalanceItem Copy()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void RaisePropertiesChanged()
        {
            RaisePropertyChanged("EnergyIn");
            RaisePropertyChanged("EnergyOut");
        }

        #region ITreeModel

        public override IEnumerable GetParentChildren(object parent)
        {
            if (parent == null)
                return Children;

            var element = parent as IBalanceGroupItem;
            if (element == null)
                return null;
            else
                return element.Children;
        }

        public override bool HasParentChildren(object parent)
        {
            var element = parent as IBalanceGroupItem;
            if (element == null)
                return false;
            else
            {
                if (element.Children == null)
                    return false;
                else
                    if (element.Children.Count == 0)
                    return false;
                else
                    return true;
            }
        }

        #endregion
    }
}