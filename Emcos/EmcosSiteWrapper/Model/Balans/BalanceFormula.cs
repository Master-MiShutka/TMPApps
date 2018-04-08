namespace TMP.Work.Emcos.Model.Balans
{
    public class BalanceFormula
    {
        public enum EnergyDirection
        {
            @in = 43,
            @out = 45
        }
        public BalanceFormula()
        {
            TransformersEnergyInDirection = EnergyDirection.@in;
            FidersEnergyInDirection = EnergyDirection.@out;
            UnitTransformersEnergyInDirection = EnergyDirection.@in;
        }
        public EnergyDirection TransformersEnergyInDirection { get; set; }
        public EnergyDirection TransformersEnergyOutDirection
        {
            get { return Inverce(TransformersEnergyInDirection); }
        }
        public EnergyDirection FidersEnergyInDirection { get; set; }
        public EnergyDirection FidersEnergyOutDirection
        {
            get { return Inverce(FidersEnergyInDirection); }
        }
        public EnergyDirection UnitTransformersEnergyInDirection { get; set; }
        public EnergyDirection UnitTransformersEnergyOutDirection
        {
            get { return Inverce(UnitTransformersEnergyInDirection); }
        }

        private EnergyDirection Inverce(EnergyDirection energyDirection)
        {
            if (energyDirection == EnergyDirection.@in)
                return EnergyDirection.@out;
            else
                return EnergyDirection.@in;
        }
    }
}
