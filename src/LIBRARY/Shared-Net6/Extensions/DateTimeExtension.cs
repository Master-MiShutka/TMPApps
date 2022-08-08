namespace TMP.Shared.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        public static int GetQuarter(this DateTime date)
        {
            return (date.Month + 2) / 3;
        }

        public static int GetQuarter(this DateOnly date)
        {
            return (date.Month + 2) / 3;
        }
    }
}
