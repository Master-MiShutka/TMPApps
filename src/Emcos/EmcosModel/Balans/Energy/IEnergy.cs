namespace TMP.Work.Emcos.Model.Balance
{
    /// <summary>
    /// Описание энергии - активной, реактивной
    /// </summary>
    public interface IEnergy
    {
        /// <summary>
        /// Энергия со знаком +
        /// </summary>
         IDirectedEnergy Plus { get; }
        /// <summary>
        /// Энергия со знаком -
        /// </summary>
        IDirectedEnergy Minus { get; }
        /// <summary>
        /// Параметр, описывающий энергию
        /// </summary>
        MSF_Param Parameter { get; }
        /// <summary>
        /// Возвращает описание произведенных корректировок
        /// </summary>
        string Correction { get; }
        /// <summary>
        /// Очистка значений
        /// </summary>
        void ClearData();
    }
}
