namespace TMP.Work.Emcos.Model.Balans
{
    /// <summary>
    /// Реактивная энергия
    /// </summary>
    public sealed class ReactiveEnergy : IEnergy
    {
        public ReactiveEnergy()
        {
            Plus = new EnergyRPlus();
            Minus = new EnergyRMinus();
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
            Id = "15",
            Name = "R энергия",
            MS = new MS_Param() { Id = "1" },
        };
    }

    /// <summary>
    /// Реактивная энергия R+
    /// </summary>
    public class EnergyRPlus : EnergyBase
    {
        public EnergyRPlus()
        {
            ;
        }
        /// <summary>
        /// Описание энергии
        /// </summary>
        public override string Description => "Реактивная энергия R+";
        /// <summary>
        /// Краткое описание
        /// </summary>
        public string ShortDescription => "R+";
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public override ML_Param DayhMlParam => MLPARAMS.R_PLUS_ENERGY_DAYS;
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public override ML_Param MonthMlParam => MLPARAMS.R_PLUS_ENERGY_MONTH;
    }
    /// <summary>
    /// Реактивная энергия R-
    /// </summary>
    public class EnergyRMinus : EnergyBase
    {
        public EnergyRMinus()
        {
            ;
        }
        /// <summary>
        /// Описание энергии
        /// </summary>
        public override string Description => "Реактивная энергия R-";
        /// <summary>
        /// Краткое описание
        /// </summary>
        public string ShortDescription => "R-";
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public override ML_Param DayhMlParam => MLPARAMS.R_MINUS_ENERGY_DAYS;
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public override ML_Param MonthMlParam => MLPARAMS.R_MINUS_ENERGY_MONTH;
    }
}
