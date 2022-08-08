using System;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Базовый класс, являющийся родителем для энергии
    /// </summary>
    public class BaseEnergy : IEnergy
    {
        public BaseEnergy()
        {
        }
        /// <summary>
        /// Энергия со знаком +
        /// </summary>
        public IDirectedEnergy Plus { get; protected set; }
        /// <summary>
        /// Энергия со знаком -
        /// </summary>
        public IDirectedEnergy Minus { get; protected set; }
        /// <summary>
        /// Параметр, описывающий энергию
        /// </summary>
        public virtual MSF_Param Parameter { get; }
        /// <summary>
        /// Возвращает описание произведенных корректировок
        /// </summary>
        public string Correction
        {
            get
            {
                var energyPlusCorrection = Plus != null && Plus.Value.HasValue ? Plus.Correction : String.Empty;
                var energyMinusCorrection = Minus != null && Minus.Value.HasValue ? Minus.Correction : String.Empty;

                if (String.IsNullOrEmpty(energyPlusCorrection) == false && String.IsNullOrEmpty(energyMinusCorrection) == false)
                    return String.Format("{0}\t{1}", energyPlusCorrection, energyMinusCorrection);
                if (String.IsNullOrEmpty(energyPlusCorrection) == true && String.IsNullOrEmpty(energyMinusCorrection) == true)
                    return String.Empty;
                if (String.IsNullOrEmpty(energyPlusCorrection) == false && String.IsNullOrEmpty(energyMinusCorrection) == true)
                    return energyPlusCorrection;
                if (String.IsNullOrEmpty(energyPlusCorrection) == true && String.IsNullOrEmpty(energyMinusCorrection) == false)
                    return energyMinusCorrection;
                return null;
            }
        }
        /// <summary>
        /// Очистка значений
        /// </summary>
        public void ClearData()
        {
            Plus.ClearData();
            Minus.ClearData();
        }
    }
}
