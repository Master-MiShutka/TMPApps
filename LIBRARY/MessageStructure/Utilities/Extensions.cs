// <copyright file="Extensions.cs" company="Ataseven">
// <author>Emre Ataseven</author>
// Copyright (c) 2014 All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace TMP.MessageStructure.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TMP.MessageStructure.Model;

    /// <summary>
    /// Extensions methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Cast message to derived type.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// Cast message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public static T Cast<T>(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "This message has no header message!");
            }

            return (T)Convert.ChangeType(message, typeof(T));
        }

        /// <summary>
        /// Splits string into chunks.
        /// </summary>
        /// <param name="str">
        /// Input string.
        /// </param>
        /// <param name="chunkSize">
        /// The chunk size.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public static IEnumerable<string> Split(this string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        /// <summary>
        /// Fits data.
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="bitLength">
        /// The bit length.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        public static byte[] Fit(this byte[] array, int bitLength)
        {
            var lrem = bitLength % 8;
            var rrem = 8 - lrem;

            for (int i = array.Length - 1; i >= 0; i--)
            {
                var rhs = (byte)(array[i] >> rrem);
                if (i - 1 >= 0)
                {
                    var lhs = (byte)(array[i - 1] << lrem);
                    rhs = (byte)(rhs | lhs);
                }

                array[i] = rhs;
            }

            return array;
        }

        /// <summary>
        /// Pads or trims array.
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="destLen">
        /// Destination length.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        public static byte[] PadTrimArray(this byte[] array, int destLen)
        {
            if (array.Length > destLen)
            {
                int k = array.Length - destLen;
                return array.Skip(k).Take(destLen).ToArray();
            }

            if (array.Length == destLen)
            {
                return array;
            }

            int l = destLen - array.Length;
            var list = array.ToList();
            for (int i = 0; i < l; i++)
            {
                list.Insert(0, default(byte));
            }

            return list.ToArray();
        }

        public static List<T> SetCapacity<T>(this List<T> messages, int capacity)
         where T : IMessage
        {
            Enumerable.Range(0, capacity).ToList().ForEach(p => messages.Add((T)Activator.CreateInstance(typeof(T))));
            return messages;
        }
    }
}
