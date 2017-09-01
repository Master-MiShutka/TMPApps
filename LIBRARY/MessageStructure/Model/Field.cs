// <copyright file="Field.cs" company="Ataseven">
// <author>Emre Ataseven</author>
// Copyright (c) 2014 All Rights Reserved
// </copyright>
// -----------------------------------------------------------------------

namespace TMP.MessageStructure.Model
{
    using System;
    using System.Linq;
    using Utilities;

    /// <summary>
    /// The field.
    /// </summary>
    public class Field
    {
        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        public Field(string name, int length)
        {
            this.Name = name;
            this.Length = length;
            this.Resolution = 1;
            this.DataType = DataTypes.BYTE;
            var byteCount = (int)Math.Ceiling(length / 8f);
            this.Data = new byte[byteCount];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        public Field(string name, int length, DataTypes type)
            : this(name, length)
        {
            this.DataType = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="resolution">
        /// The resolution.
        /// </param>
        public Field(string name, int length, DataTypes type, int resolution)
            : this(name, length, type)
        {
            this.Resolution = resolution;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="resolution">
        /// The resolution.
        /// </param>
        public Field(int length, DataTypes type, int resolution)
            : this(string.Empty, length, type, resolution)
        {
            this.Resolution = resolution;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        public Field(int length, DataTypes type)
            : this(string.Empty, length, type)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///  Gets or sets the value updated.
        /// </summary>
        public Action ValueUpdated { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the resolution.
        /// </summary>
        public double Resolution { get; set; }

        /// <summary>
        /// Gets or sets the byte array data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public DataTypes DataType { get; set; }

        #endregion

        #region Public Methods

        //public static implicit operator Field(int rhs)
        //{
        //}

        /// <summary>
        /// Sets value of field.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public void SetValue(object value)
        {
            switch (this.DataType)
            {
                case DataTypes.INT:
                    this.SetValue((int)value);
                    break;
                case DataTypes.UINT:
                    this.SetValue((uint)value);
                    break;
                case DataTypes.SHORT:
                    this.SetValue((short)value);
                    break;
                case DataTypes.USHORT:
                    this.SetValue((ushort)value);
                    break;
                case DataTypes.FLOAT:
                    this.SetValue((float)value);
                    break;
                case DataTypes.DOUBLE:
                    this.SetValue((double)value);
                    break;
                case DataTypes.BYTE:
                    this.SetValue((byte)value);
                    break;
                case DataTypes.BYTEARRAY:
                    this.SetValue((byte[])value);
                    break;
                default:
                    throw new Exception("Hata");
            }
        }

        /// <summary>
        /// The set value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <typeparam name="T">
        /// Available types: int, uint, short, ushort, float, double, byte, char, byte[]
        /// </typeparam>
        public void SetValue<T>(T value)
        {
            var len = (int)Math.Ceiling(this.Length / 8f);

            if (typeof(T) == typeof(int))
            {
                var intvalue = (int)(Convert.ToInt32(value) / this.Resolution);
                this.Data = BitConverter.GetBytes(intvalue).Reverse().ToArray().PadTrimArray(len);
            }
            else if (typeof(T) == typeof(uint))
            {
                var uintvalue = (uint)(Convert.ToUInt32(value) / this.Resolution);
                this.Data = BitConverter.GetBytes(uintvalue).Reverse().ToArray().PadTrimArray(len);
            }
            else if (typeof(T) == typeof(short))
            {
                var shortvalue = (short)(Convert.ToInt16(value) / this.Resolution);
                this.Data = BitConverter.GetBytes(shortvalue).Reverse().ToArray();
            }
            else if (typeof(T) == typeof(ushort))
            {
                var ushortvalue = (ushort)(Convert.ToUInt16(value) / this.Resolution);
                this.Data = BitConverter.GetBytes(ushortvalue).Reverse().ToArray().PadTrimArray(len);
            }
            else if (typeof(T) == typeof(float))
            {
                var floatvalue = (float)(Convert.ToSingle(value) / this.Resolution);
                this.Data = BitConverter.GetBytes(floatvalue).PadTrimArray(len).Reverse().ToArray();
            }
            else if (typeof(T) == typeof(double))
            {
                double doublevalue = Convert.ToDouble(value) / this.Resolution;
                this.Data = BitConverter.GetBytes(doublevalue).Reverse().ToArray().PadTrimArray(len);
            }
            else if (typeof(T) == typeof(byte))
            {
                var bytevalue = (byte)(Convert.ToByte(value) / this.Resolution);
                this.Data = BitConverter.GetBytes(bytevalue).Reverse().ToArray().PadTrimArray(len);
            }
            else if (typeof(T) == typeof(char))
            {
                var charvalue = (char)Convert.ToChar(value);
                this.Data = BitConverter.GetBytes(charvalue).Reverse().ToArray().PadTrimArray(len);
            }
            else if (typeof(T) == typeof(byte[]))
            {
                this.Data = (byte[])(object)value;
            }
            else
            {
               throw new ArgumentException("value", "Invalid type.");
            }

            if (this.ValueUpdated != null)
            {
                this.ValueUpdated();
            }
        }

        /// <summary>
        /// The get value.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetValue()
        {
            return this.GetValue(this.DataType);
        }

        /// <summary>
        /// The get value.
        /// </summary>
        /// <typeparam name="T">
        /// Available types: int, uint, short, ushort, float, double, byte, char, byte[]
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// Returns value after converted to selected type.
        /// </returns>
        public T GetValue<T>()
        {
            if (typeof(T) == typeof(int))
            {
                var arr = this.Data.PadTrimArray(4);
                var value = (int)(BitConverter.ToInt32(arr.Reverse().ToArray(), 0) * this.Resolution);
                return (T)Convert.ChangeType(value, typeof(T));
            }

            if (typeof(T) == typeof(uint))
            {
                var arr = this.Data.PadTrimArray(4);
                var value = (uint)(BitConverter.ToUInt32(arr.Reverse().ToArray(), 0) * this.Resolution);
                return (T)Convert.ChangeType(value, typeof(T));
            }

            if (typeof(T) == typeof(short))
            {
                var arr = this.Data.PadTrimArray(2);
                var value = (short)(BitConverter.ToInt16(arr.Reverse().ToArray(), 0) * this.Resolution);
                return (T)Convert.ChangeType(value, typeof(T));
            }

            if (typeof(T) == typeof(ushort))
            {
                var arr = this.Data.PadTrimArray(2);
                var value = (ushort)(BitConverter.ToUInt16(arr.Reverse().ToArray(), 0) * this.Resolution);
                return (T)Convert.ChangeType(value, typeof(T));
            }

            if (typeof(T) == typeof(float))
            {
                var arr = this.Data.PadTrimArray(4);
                var value = (float)(BitConverter.ToSingle(arr.Reverse().ToArray(), 0) * this.Resolution);
                return (T)Convert.ChangeType(value, typeof(T));
            }

            if (typeof(T) == typeof(double))
            {
                var arr = this.Data.PadTrimArray(4);
                var value = BitConverter.ToDouble(arr.Reverse().ToArray(), 0) * this.Resolution;
                return (T)Convert.ChangeType(value, typeof(T));
            }

            if (typeof(T) == typeof(byte))
            {
                var value = (byte)(this.Data[0] * this.Resolution);
                return (T)Convert.ChangeType(value, typeof(T));
            } 
            
            if (typeof(T) == typeof(char))
            {
                var value = (char)this.Data[0];
                return (T)Convert.ChangeType(value, typeof(T));
            }

            if (typeof(T) == typeof(byte[]))
            {
                return (T)Convert.ChangeType(this.Data, typeof(T));
            }

            return default(T);
        }

        /// <summary>
        /// The to string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns string representation of value of field.
        /// </summary>
        /// <param name="dataType">
        /// The data type.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// String representation of field.
        /// </returns>
        private string GetValue(DataTypes dataType)
        {
            switch (dataType)
            {
                case DataTypes.INT:
                    return this.GetValue<int>().ToString();
                case DataTypes.UINT:
                    return this.GetValue<uint>().ToString();
                case DataTypes.SHORT:
                    return this.GetValue<short>().ToString();
                case DataTypes.USHORT:
                    return this.GetValue<ushort>().ToString();
                case DataTypes.FLOAT:
                    return this.GetValue<float>().ToString();
                case DataTypes.DOUBLE:
                    return this.GetValue<double>().ToString();
                case DataTypes.BYTE:
                    return this.GetValue<byte>().ToString();
                case DataTypes.CHAR:
                    return this.GetValue<char>().ToString();
                case DataTypes.BYTEARRAY:
                    return BitConverter.ToString(this.GetValue<byte[]>());
                default:
                    return null;
            }
        }

        #endregion
    }
}
