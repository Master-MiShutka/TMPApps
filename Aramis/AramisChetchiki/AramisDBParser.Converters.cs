namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using DBF;
    using TMP.WORK.AramisChetchiki.Model;
    using TMP.WORK.AramisChetchiki.Properties;
    using TMPApplication;

    internal partial class AramisDBParser
    {
        #region Converters

        private static uint ConvertToUInt(object value)
        {
            if (value is null or DBNull)
            {
                return 0;
            }

            _ = uint.TryParse(value.ToString(), out uint result);
            return result;
        }

        private static ulong ConvertToULong(object value)
        {
            if (value is null or DBNull)
            {
                return 0;
            }

            _ = ulong.TryParse(value.ToString(), out ulong result);
            return result;
        }

        private static DateTime? GetDate(object value)
        {
            if (value == null)
            {
                return default(DateTime?);
            }

            if (value is DateTime dateTime)
            {
                return new DateTime?(dateTime);
            }

            string valueAsString = value.ToString().Trim();
            DateTime d = default;
            if (string.IsNullOrWhiteSpace(valueAsString))
            {
                return d;
            }

            bool result;
            try
            {
                result = DateTime.TryParse(valueAsString, out d);
            }
            catch
            {
                result = false;
            }

            return result ? new DateTime?(d) : null;
        }

        private static byte GetByte(object value, int startPos, int length, byte defaultValue)
        {
            if (value is null or System.DBNull)
            {
                return 0;
            }

            string s = value.ToString();
            if (string.IsNullOrWhiteSpace(s) || s.Length < length || startPos >= s.Length)
            {
                return defaultValue;
            }

            try
            {
                var substring = s.Substring(startPos, length).Trim();
                return string.IsNullOrWhiteSpace(substring) ? defaultValue : Convert.ToByte(substring, AppSettings.CurrentCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static DateOnly GetDateOnly(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return DateOnly.FromDateTime(dateTime.Value);
            }
            else
            {
                return default;
            }
        }

        #endregion
    }
}
