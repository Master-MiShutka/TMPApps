namespace TMP.Work.Emcos.Model.Balans
{
    /// <summary>
    /// Активная энергия
    /// </summary>
    public sealed class ActiveEnergy : IEnergy
    {
        public ActiveEnergy()
        {
            Plus = new EnergyEPlus();
            Minus = new EnergyEMinus();
        }
        /// <summary>
        /// Энергия со знаком +
        /// </summary>
        public IBaseEnergy Plus { get; }
        /// <summary>
        /// Энергия со знаком -
        /// </summary>
        public IBaseEnergy Minus { get; }
        /// <summary>
        /// Параметр, описывающий энергию
        /// </summary>
        public MSF_Param Parameter => new MSF_Param()
        {
            Id = "14",
            Name = "A энергия",
            MS = new MS_Param() { Id = "1" },
        };
    }

    /// <summary>
    /// Активная энергия A+
    /// </summary>
    public class EnergyEPlus : EnergyBase
    {
        public EnergyEPlus()
        {
            ;
        }
        /// <summary>
        /// Описание энергии
        /// </summary>
        public override string Description => "Активная энергия A+";
        /// <summary>
        /// Краткое описание
        /// </summary>
        public string ShortDescription => "A+";
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public override ML_Param DayhMlParam => MLPARAMS.A_PLUS_ENERGY_DAYS;
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public override ML_Param MonthMlParam => MLPARAMS.A_PLUS_ENERGY_MONTH;
    }
    /// <summary>
    /// Активная энергия A-
    /// </summary>
    public class EnergyEMinus : EnergyBase
    {
        public EnergyEMinus()
        {
            ;
        }
        /// <summary>
        /// Описание энергии
        /// </summary>
        public override string Description => "Активная энергия A-";
        /// <summary>
        /// Краткое описание
        /// </summary>
        public string ShortDescription => "A-";
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public override ML_Param DayhMlParam => MLPARAMS.A_MINUS_ENERGY_DAYS;
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public override ML_Param MonthMlParam => MLPARAMS.A_MINUS_ENERGY_MONTH;
    }
}