namespace TMP.WORK.AramisChetchiki
{
    using System;

    internal partial class AramisDBParser
    {
        #region Converters

        private static float ConvertToFloat(decimal? value)
        {
            if (value is null)
            {
                return 0f;
            }

            return (float)value.Value;
        }

        private static uint ConvertToUInt(object value)
        {
            if (value is null or DBNull)
            {
                return 0;
            }

            if (value is string str)
            {
                bool res = uint.TryParse(value.ToString(), out uint result);

                if (res == false)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }

                return result;
            }

            try
            {
                return Convert.ToUInt32((int)value);
            }
            catch (OverflowException oe)
            {
                throw new Exception();
            }
            catch (Exception e)
            {
                return 0;
            }
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

        private static DateTime? ConvertToDateTime(object value)
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

        private static byte ConvertToByte(object value)
        {
            if (value is null or System.DBNull)
            {
                return 0;
            }

            if (value is int @int)
            {
                try
                {
                    return Convert.ToByte(@int);
                }
                catch (OverflowException oe)
                {
                    throw new Exception();
                }
                catch (Exception e)
                {
                    return 0;
                }
            }

            string s = value.ToString();
            bool res = byte.TryParse(s, System.Globalization.NumberStyles.Float, AppSettings.CurrentCulture, out byte result);

            if (res == false)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

            return result;
        }

        private static ushort ConvertToUShort(object value)
        {
            if (value is null or System.DBNull)
            {
                return 0;
            }

            if (value is int @int)
            {
                try
                {
                    return Convert.ToUInt16(@int);
                }
                catch (OverflowException oe)
                {
                    throw new Exception();
                }
                catch (Exception e)
                {
                    return 0;
                }
            }

            if (value is string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return 0;
                }

                bool res1 = ushort.TryParse(str, out ushort result1);

                if (res1 == false)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }

                return result1;
            }

            string s = value.ToString();
            bool res2 = ushort.TryParse(s, out ushort result2);

            if (res2 == false)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

            return result2;
        }

        private static byte ConvertToByte(object value, int startPos, int length, byte defaultValue)
        {
            if (value is null or System.DBNull)
            {
                return 0;
            }

            if (value is int @int)
            {
                try
                {
                    return Convert.ToByte(@int);
                }
                catch (OverflowException oe)
                {
                    throw new Exception();
                }
                catch (Exception e)
                {
                    return 0;
                }
            }

            string s = value.ToString();
            if (string.IsNullOrWhiteSpace(s) || s.Length < length || startPos >= s.Length)
            {
                return defaultValue;
            }

            try
            {
                string substring = s.Substring(startPos, length).Trim();
                return string.IsNullOrWhiteSpace(substring) ? defaultValue : Convert.ToByte(substring, AppSettings.CurrentCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static DateOnly ConvertToDateOnly(DateTime? dateTime)
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
