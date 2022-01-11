using System.ComponentModel;
using System.Runtime.Serialization;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Описание формулы расчёта баланса подстанции
    /// </summary>
    [DataContract(Namespace = "http://tmp.work.Balance-substations.com")]
    public class BalanceFormula
    {
        //
        public enum EnergyDirection
        {
            [Description("направление +")]
            @in = 43,
            [Description("направление -")]
            @out = 45
        }
        public BalanceFormula()
        {
            TransformersEnergyInDirection = EnergyDirection.@in;
            FidersEnergyInDirection = EnergyDirection.@out;
            UnitTransformersEnergyInDirection = EnergyDirection.@in;
        }
        [DataMember]
        public EnergyDirection TransformersEnergyInDirection { get; set; }
        public EnergyDirection TransformersEnergyOutDirection
        {
            get { return Inverce(TransformersEnergyInDirection); }
        }
        [DataMember]
        public EnergyDirection FidersEnergyInDirection { get; set; }
        public EnergyDirection FidersEnergyOutDirection
        {
            get { return Inverce(FidersEnergyInDirection); }
        }
        [DataMember]
        public EnergyDirection UnitTransformersEnergyInDirection { get; set; }
        public EnergyDirection UnitTransformersEnergyOutDirection
        {
            get { return Inverce(UnitTransformersEnergyInDirection); }
        }

        public bool IsDefault
        {
            get
            {
                return TransformersEnergyInDirection == EnergyDirection.@in &&
                    FidersEnergyInDirection == EnergyDirection.@out &&
                    UnitTransformersEnergyInDirection == EnergyDirection.@in;
            }
        }

        private EnergyDirection Inverce(EnergyDirection energyDirection)
        {
            if (energyDirection == EnergyDirection.@in)
                return EnergyDirection.@out;
            else
                return EnergyDirection.@in;
        }

        /// <summary>
        /// Фабрика для создания формулы по умолчанию
        /// </summary>
        /// <returns></returns>
        public static BalanceFormula CreateDefault()
        {
            return new BalanceFormula();
        }
    }
}
