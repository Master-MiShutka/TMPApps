using System.ComponentModel;

namespace TMP.WORK.AramisChetchiki.Model
{
    /// <summary>
    /// Перечисление вариантов формирования списка привязки абонентов
    /// </summary>
    public enum AbonentsBindingTreeMode
    {
        [Description("Подстанция ⇒ Фидер 10 кВ ⇒ ТП 10/0,4 кВ ⇒ Фидер 0,4 кВ")]
        Full,
        [Description("Фидер 10 кВ ⇒ ТП 10/0,4 кВ ⇒ Фидер 0,4 кВ")]
        WithoutSubstation,
        [Description("ТП 10/0,4 кВ ⇒ Фидер 0,4 кВ")]
        WithoutFider10,
    }
}
