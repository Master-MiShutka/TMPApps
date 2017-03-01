// <copyright file="Utils.cs" company="Ataseven">
// <author>Emre Ataseven</author>
// Copyright (c) 2014 All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace TMP.MessageStructure.Utilities
{
    /// <summary>
    /// Utilities.
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Calculates checksum.
        /// </summary>
        /// <param name="crc">
        /// The crc.
        /// </param>
        /// <returns>
        /// The <see cref="byte"/>.
        /// </returns>
        public static byte CalculateChecksum(byte[] crc)
        {
            byte sum = 0x00;
            for (int i = 0; i <= crc.Length - 1; i++)
            {
                sum = (byte)(crc[i] + sum);
            }

            var checksum = (byte)(0xff - sum);
            checksum++;
            return checksum;
        }

        /// <summary>
        /// Splits camel case.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// Returns string, first letter capped.
        /// </returns>
        public static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z]|[0-9]+)", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }
}
