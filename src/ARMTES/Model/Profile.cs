using System;
using System.ComponentModel;

namespace TMP.ARMTES.Model
{
    public enum Profile
    {
        [Description("текущие показания")]
        Current = 1,
        [Description("показания на начало суток")]
        Days = 2,
        [Description("показания на начало месяца")]
        Months = 3,
        [Description("показания на начало часа")]
        Hour = 4
    }
}
