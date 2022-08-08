namespace TMP.ExcelOutput
{
    using System;
    using System.Globalization;

    public static class StringExtensions
    {
        /// <summary>
        ///     Returns a new string containing the specified number of characters from the left of the input string.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="length">The number of characters to return.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string Left(this string input, int length)
        {
            return string.IsNullOrEmpty(input)
                ? throw new ArgumentException("The input string is null or empty.", nameof(input))
                : length <= 0
                ? throw new ArgumentOutOfRangeException(nameof(length), "The length must be greater than 0.")
                : length > input.Length
                ? throw new ArgumentOutOfRangeException(nameof(length), "The length must be smaller or equal to the length of the input string.")
                : input.Substring(0, length);
        }

        /// <summary>
        ///     Returns a new string containing the specified number of characters from the right of the input string.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="length">The number of characters to return.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string Right(this string input, int length)
        {
            return string.IsNullOrEmpty(input)
                ? throw new ArgumentException("The input string is null or empty.", nameof(input))
                : length <= 0
                ? throw new ArgumentOutOfRangeException(nameof(length), "The length must be greater than 0.")
                : length > input.Length
                ? throw new ArgumentOutOfRangeException(nameof(length), "The length must be smaller or equal to the length of the input string.")
                : input.Substring(input.Length - length, length);
        }

        /// <summary>
        ///     Converts any object to string and then trims the new string.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string ToTrimmedString(this object input)
        {
            return input.ToString().Trim();
        }

        /// <summary>
        ///     Returns a new string with the specified number of characters added to the left of the input string.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="length">The number of characters to add.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string AddWhitespaceLeft(this string input, int length)
        {
            return string.IsNullOrEmpty(input)
                ? throw new ArgumentException("The input string is null or empty.", nameof(input))
                : length <= 0
                ? throw new ArgumentOutOfRangeException(nameof(length), "The length must be greater than 0.")
                : string.Concat(new string(' ', length), input);
        }

        /// <summary>
        ///     Returns a new string with the specified number of characters added to the right of the input string.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="length">The number of characters to add.</param>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public static string AddWhitespaceRight(this string input, int length)
        {
            return string.IsNullOrEmpty(input)
                ? throw new ArgumentException("The input string is null or empty.", nameof(input))
                : length <= 0
                ? throw new ArgumentOutOfRangeException(nameof(length), "The length must be greater than 0.")
                : string.Concat(input, new string(' ', length));
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to a <see cref="byte"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="byte"/></returns>
        public static byte ToByte(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = byte.TryParse(input, out byte result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a byte.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to a <see cref="byte"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="byte"/></returns>
        public static byte ToByte(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = byte.TryParse(input, numberStyle, formatProvider, out byte result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a byte.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to a <see cref="sbyte"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="sbyte"/></returns>
        public static sbyte ToSByte(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = sbyte.TryParse(input, out sbyte result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a sbyte.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to a <see cref="sbyte"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="sbyte"/></returns>
        public static sbyte ToSByte(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = sbyte.TryParse(input, numberStyle, formatProvider, out sbyte result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a sbyte.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="int"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="int"/></returns>
        public static int ToInt(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = int.TryParse(input, out int result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into an int.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="int"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="int"/></returns>
        public static int ToInt(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = int.TryParse(input, numberStyle, formatProvider, out int result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into an int.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="uint"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="uint"/></returns>
        public static uint ToUInt(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = uint.TryParse(input, out uint result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into an uint.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="uint"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="uint"/></returns>
        public static uint ToUInt(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = uint.TryParse(input, numberStyle, formatProvider, out uint result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into an uint.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to a <see cref="short"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="short"/></returns>
        public static short ToShort(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = short.TryParse(input, out short result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a short.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to a <see cref="short"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="short"/></returns>
        public static short ToShort(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = short.TryParse(input, numberStyle, formatProvider, out short result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a short.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="ushort"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="ushort"/></returns>
        public static ushort ToUShort(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = ushort.TryParse(input, out ushort result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into an ushort.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="ushort"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="ushort"/></returns>
        public static ushort ToUShort(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = ushort.TryParse(input, numberStyle, formatProvider, out ushort result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into an ushort.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="long"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="long"/></returns>
        public static long ToLong(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = long.TryParse(input, out long result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a long.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="long"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="long"/></returns>
        public static long ToLong(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = long.TryParse(input, numberStyle, formatProvider, out long result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a long.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="ulong"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="ulong"/></returns>
        public static ulong ToULong(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = ulong.TryParse(input, out ulong result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into an ulong.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="ulong"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="ulong"/></returns>
        public static ulong ToULong(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = ulong.TryParse(input, numberStyle, formatProvider, out ulong result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into an ulong.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="float"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="float"/></returns>
        public static float ToFloat(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = float.TryParse(input, out float result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a float.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="float"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="float"/></returns>
        public static float ToFloat(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = float.TryParse(input, numberStyle, formatProvider, out float result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a float.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="double"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="double"/></returns>
        public static double ToDouble(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = double.TryParse(input, out double result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a double.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="double"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="double"/></returns>
        public static double ToDouble(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = double.TryParse(input, numberStyle, formatProvider, out double result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a double.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="decimal"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="decimal"/></returns>
        public static decimal ToDecimal(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = decimal.TryParse(input, out decimal result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a decimal.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="decimal"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="numberStyle">The style to use for the number which is to be converted.</param>
        /// <returns><see cref="decimal"/></returns>
        public static decimal ToDecimal(this string input, IFormatProvider formatProvider, NumberStyles numberStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = decimal.TryParse(input, numberStyle, formatProvider, out decimal result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a decimal.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="bool"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="bool"/></returns>
        public static bool ToBool(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = bool.TryParse(input, out bool result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a bool.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="char"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="char"/></returns>
        public static char ToChar(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = char.TryParse(input, out char result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a char.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="DateTime"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <returns><see cref="DateTime"/></returns>
        public static DateTime ToDateTime(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = DateTime.TryParse(input, out DateTime result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a DateTime.");
        }

        /// <summary>
        /// Uses the <see cref="input"/> string and tries to convert it to an <see cref="DateTime"/>. An <see cref="InvalidCastException"/> will be
        /// thrown if the conversion fails.
        /// </summary>
        /// <param name="input">The source string for the operation.</param>
        /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for formatting the output.</param>
        /// <param name="dateTimeStyle">The style to use for the <see cref="DateTime"/> which is to be converted.</param>
        /// <returns><see cref="DateTime"/></returns>
        public static DateTime ToDateTime(this string input, IFormatProvider formatProvider, DateTimeStyles dateTimeStyle)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var parsed = DateTime.TryParse(input, formatProvider, dateTimeStyle, out DateTime result);
            return parsed ? result : throw new InvalidCastException("The input cannot be changed into a DateTime.");
        }
    }
}
