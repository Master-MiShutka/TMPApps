namespace TMP.Work.Emcos.Model.Balans
{
    /// <summary>
    /// Описание энергии - активной, реактивной
    /// </summary>
    public interface IEnergy
    {
        /// <summary>
        /// Энергия со знаком +
        /// </summary>
         IBaseEnergy Plus { get; }
        /// <summary>
        /// Энергия со знаком -
        /// </summary>
        IBaseEnergy Minus { get; }
        /// <summary>
        /// Параметр, описывающий энергию
        /// </summary>
        MSF_Param Parameter { get; }
    }
}
