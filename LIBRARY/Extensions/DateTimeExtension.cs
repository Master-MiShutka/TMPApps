using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Extensions
{
    public static class DateTimeExtensions
    {
        public static int GetQuarter(this DateTime date)
        {
            return (date.Month + 2) / 3;
        }
    }
}
