using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Активная энергия A+
    /// </summary>
    public class EnergyAPlus : DirectedEnergyBase
    {
        public EnergyAPlus()
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
        public override string ShortDescription => "A+";
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public override ML_Param DayMlParam => MlParamsFactory.A_PLUS_ENERGY_DAYS;
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public override ML_Param MonthMlParam => MlParamsFactory.A_PLUS_ENERGY_MONTH;
    }
    /// <summary>
    /// Активная энергия A-
    /// </summary>
    public class EnergyAMinus : DirectedEnergyBase
    {
        public EnergyAMinus()
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
        public override string ShortDescription => "A-";
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public override ML_Param DayMlParam => MlParamsFactory.A_MINUS_ENERGY_DAYS;
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public override ML_Param MonthMlParam => MlParamsFactory.A_MINUS_ENERGY_MONTH;
    }
    /// <summary>
    /// Реактивная энергия R+
    /// </summary>
    public class EnergyRPlus : DirectedEnergyBase
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
        public override string ShortDescription => "R+";
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public override ML_Param DayMlParam => MlParamsFactory.R_PLUS_ENERGY_DAYS;
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public override ML_Param MonthMlParam => MlParamsFactory.R_PLUS_ENERGY_MONTH;
    }
    /// <summary>
    /// Реактивная энергия R-
    /// </summary>
    public class EnergyRMinus : DirectedEnergyBase
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
        public override string ShortDescription => "R-";
        /// <summary>
        /// Параметр - энергия за месяц
        /// </summary>
        public override ML_Param DayMlParam => MlParamsFactory.R_MINUS_ENERGY_DAYS;
        /// <summary>
        /// Параметр - энергия за сутки
        /// </summary>
        public override ML_Param MonthMlParam => MlParamsFactory.R_MINUS_ENERGY_MONTH;
    }
    /// <summary>
    /// Активная энергия
    /// </summary>
    public sealed class ActiveEnergy : BaseEnergy
    {
        public ActiveEnergy()
        {
            Plus = new EnergyAPlus();
            Minus = new EnergyAMinus();
        }
        /// <summary>
        /// Параметр, описывающий энергию
        /// </summary>
        public override MSF_Param Parameter => new MSF_Param()
        {
            Id = "14",
            Name = "A энергия",
            MS = new MS_Param() { Id = "1" },
        };
    }
    /// <summary>
    /// Реактивная энергия
    /// </summary>
    public sealed class ReactiveEnergy : BaseEnergy
    {
        public ReactiveEnergy()
        {
            Plus = new EnergyRPlus();
            Minus = new EnergyRMinus();
        }
        /// <summary>
        /// Параметр, описывающий энергию
        /// </summary>
        public override MSF_Param Parameter => new MSF_Param()
        {
            Id = "15",
            Name = "R энергия",
            MS = new MS_Param() { Id = "1" },
        };
    }
}
