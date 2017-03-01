// <copyright file="IMessage.cs" company="Ataseven">
// <author>Emre Ataseven</author>
// Copyright (c) 2014 All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace TMP.MessageStructure.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Message interface.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets fields.
        /// </summary>
        List<Field> Fields { get; }

        /// <summary>
        /// Returns byte array representation of message.
        /// </summary>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// Byte array representation of message.
        /// </returns>
        byte[] GetMessage();

        /// <summary>
        /// Fills fields of message with input byte array.
        /// </summary>
        /// <param name="data">
        /// The byte array data.
        /// </param>
        void SetMessage(byte[] data);
    }
}
