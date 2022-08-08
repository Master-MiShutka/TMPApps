using System.ComponentModel;

namespace TMP.WORK.AramisChetchiki.Model
{
    /// <summary>
    /// Перечисление типов навигации в AbonentInfoViewModel
    /// </summary>
    public enum NavigationMode
    {
        [Description("по лицевому счёту")]
        ByPersonalAccount,
        [Description("по адресу")]
        ByAddress,
        [Description("по привязке")]
        ByElectricalAddress,
    }
}
