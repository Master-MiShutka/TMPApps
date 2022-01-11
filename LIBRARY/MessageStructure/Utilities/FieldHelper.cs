// <copyright file="FieldHelper.cs" company="Ataseven">
// <author>Emre Ataseven</author>
// Copyright (c) 2014 All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace TMP.MessageStructure.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TMP.MessageStructure.Model;

    /// <summary>
    /// The field helper.
    /// </summary>
    public static class FieldHelper
    {
        /// <summary>
        /// Concatenates values of fields as byte array.
        /// </summary>
        /// <param name="fields">
        /// The fields.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        public static byte[] ToByteArray(this List<Field> fields)
        {
            var sb = new StringBuilder();
            foreach (var field in fields)
            {
                var concat = new string(
                        string.Join(string.Empty, field.Data.Select(p => Convert.ToString(p, 2).PadLeft(8, '0')))
                            .Skip((field.Data.Length * 8) - field.Length)
                            .Take(field.Length)
                            .ToArray());
                sb.Append(concat);
            }

            if (sb.Length % 8 != 0)
            {
                var pad = new string('0', 8 - (sb.Length % 8));
                sb.Insert(0, pad);
            }

            var data = sb.ToString().Split(8).Select(p => Convert.ToByte(p, 2)).ToArray();
            return data;
        }

        /// <summary>
        /// Concatenates values of fields as byte array.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        public static byte[] ToByteArray(this IMessage message)
        {
            return ToByteArray(message.Fields);
        }

        /// <summary>
        /// Fills fields of message with byte array data.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="data">
        /// The byte array data.
        /// </param>
        public static void Fill(this IMessage message, byte[] data)
        {
            int tracker = 0;
            int wrem = 0;
            foreach (var field in message.Fields)
            {
                var len = (int)Math.Ceiling(field.Length / 8f);
                var rem = field.Length % 8;

                field.Data = new byte[len];
                Array.Copy(data, tracker, field.Data, 0, len);

                if (rem != 0)
                {
                    if (field.Data.Length > 1)
                    {
                        field.Data = field.Data.Fit(field.Length);
                    }
                    else
                    {
                        var lastByte = field.Data[len - 1];
                        var x = (lastByte >> (8 - (wrem + rem))) & ((byte)Math.Pow(2, rem) - 1);
                        field.Data[len - 1] = (byte)x;
                    }
                }

                tracker += len;
                wrem += rem;
                if (wrem % 8 != 0)
                {
                    tracker--;
                }
                else
                {
                    wrem = 0;
                }
            }
        }
    }
}
