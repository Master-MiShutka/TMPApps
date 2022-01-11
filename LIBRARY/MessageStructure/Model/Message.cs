// <copyright file="Message.cs" company="Ataseven">
// <author>Emre Ataseven</author>
// Copyright (c) 2014 All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace TMP.MessageStructure.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TMP.MessageStructure.Utilities;

    /// <summary>
    /// The message.
    /// </summary>
    public abstract class Message : IMessage
    {
        #region Fields

        /// <summary>
        /// The checksum field.
        /// </summary>
        protected Field CheckSumField;

        /// <summary>
        /// Fields of message.
        /// </summary>
        private List<Field> fields;

        /// <summary>
        /// Fields of message without checksum field.
        /// </summary>
        private List<Field> fieldsWoChecksum;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the header message.
        /// </summary>
        public Message HeaderMessage { get; set; }

        /// <summary>
        /// Gets or sets the footer message.
        /// </summary>
        public Message FooterMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ıs checksum exists.
        /// </summary>
        public bool IsChecksumExists { get; set; }

        /// <summary>
        /// Gets the fields.
        /// </summary>
        public virtual List<Field> Fields
        {
            get
            {
                if (this.fields == null)
                {
                    this.GatherFields();
                    return this.fields;
                }

                return this.fields;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Fills fields of message with input byte array.
        /// </summary>
        /// <param name="data">
        /// The byte array data.
        /// </param>
        public virtual void SetMessage(byte[] data)
        {
            this.Fill(data);
        }

        /// <summary>
        /// Returns byte array representation of message.
        /// </summary>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// Byte array representation of message.
        /// </returns>
        public virtual byte[] GetMessage()
        {
            this.ReCalculateCheckSum();
            return this.ToByteArray();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            if (this.fields == null)
            {
                this.GatherFields();
            }

            try
            {
                return string.Join(
                    Environment.NewLine,
                    this.fields.Select(p => p.Name + ":\t" + (p.GetValue() == "\0" ? "Null Character" : p.GetValue())));
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Initializes fields of message.
        /// </summary>
        protected void InitFields()
        {
            this.GatherFields();

            var fieldProperties =
                this.GetType()
                    .GetProperties()
                    .Where(p => p.PropertyType == typeof(Field))
                    .ToList();

            foreach (var a in fieldProperties)
            {
                object o = a.GetValue(this, null);
                if (o == null)
                {
                    continue;
                }

                var f = o as Field;
                if (f != null && f.Name == string.Empty)
                {
                    f.Name = Utils.SplitCamelCase(a.Name);
                }
            }
        }

        /// <summary>
        /// Returns calculated checksum value.
        /// </summary>
        /// <returns>
        /// The <see cref="byte"/>.
        /// </returns>
        protected virtual byte GetCheckSum()
        {
            return Utils.CalculateChecksum(this.Fields.ToByteArray());
        }

        /// <summary>
        /// Calculates checksum and sets checksum field.
        /// </summary>
        protected void ReCalculateCheckSum()
        {
            if (this.fields == null)
            {
                this.GatherFields();
            }

            if (this.IsChecksumExists && this.CheckSumField != null)
            {
                this.CheckSumField.SetValue(Utils.CalculateChecksum(this.fieldsWoChecksum.ToByteArray()));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gathers all fields.
        /// </summary>
        private void GatherFields()
        {
            // this.fields =
            //    this.GetType()
            //        .GetProperties()
            //        .Where(p => p.PropertyType == typeof(Field))
            //        .Select(p => p.GetValue(this, null))
            //        .Cast<Field>()
            //        .ToList();
            var fieldProperties = this.GetType().GetProperties().ToList();
            this.fields = new List<Field>();

            foreach (var a in fieldProperties)
            {
                object o = a.GetValue(this, null);
                if (o == null)
                {
                    continue;
                }

                if (o is Field)
                {
                    this.fields.Add(o as Field);
                }
                else if (o is IEnumerable<Message>)
                {
                    var y = o as IEnumerable<Message>;
                    this.fields.AddRange(y.SelectMany(p => p.Fields));
                }
            }

            if (this.HeaderMessage != null)
            {
                this.fields.InsertRange(0, this.HeaderMessage.Fields);
            }

            if (this.FooterMessage != null)
            {
                this.fields.AddRange(this.FooterMessage.Fields);
            }

            if (this.IsChecksumExists)
            {
                this.CheckSumField = this.fields.Last();

                if (this.fieldsWoChecksum == null)
                {
                    this.fieldsWoChecksum = this.Fields.Except(new List<Field> { this.CheckSumField }).ToList();
                }

                this.fieldsWoChecksum.ForEach(p => p.ValueUpdated = this.ReCalculateCheckSum);
            }
        }

        #endregion
    }
}
