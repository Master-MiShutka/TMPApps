// Copyright (c) Microsoft Corporation.  All rights reserved.

namespace MS.WindowsAPICodePack.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Microsoft.WindowsAPICodePack.Resources;
    using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

    /// <summary>
    /// Represents the OLE struct PROPVARIANT.
    /// This class is intended for internal use only.
    /// </summary>
    /// <remarks>
    /// Originally sourced from http://blogs.msdn.com/adamroot/pages/interop-with-propvariants-in-net.aspx
    /// and modified to support additional types including vectors and ability to set values
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1900:ValueTypeFieldsShouldBePortable", MessageId = "_ptr2", Justification = "ToDo")]
    [StructLayout(LayoutKind.Explicit)]
    public sealed class PropVariant : IDisposable
    {
        #region Vector Action Cache

        // A static dictionary of delegates to get data from array's contained within PropVariants
        private static Dictionary<Type, Action<PropVariant, Array, uint>> vectorActions = null;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "ToDo")]
        private static Dictionary<Type, Action<PropVariant, Array, uint>> GenerateVectorActions()
        {
            Dictionary<Type, Action<PropVariant, Array, uint>> cache = new Dictionary<Type, Action<PropVariant, Array, uint>>();

            cache.Add(typeof(short), (pv, array, i) =>
            {
                short val;
                PropVariantNativeMethods.PropVariantGetInt16Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(ushort), (pv, array, i) =>
            {
                ushort val;
                PropVariantNativeMethods.PropVariantGetUInt16Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(int), (pv, array, i) =>
            {
                int val;
                PropVariantNativeMethods.PropVariantGetInt32Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(uint), (pv, array, i) =>
            {
                uint val;
                PropVariantNativeMethods.PropVariantGetUInt32Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(long), (pv, array, i) =>
            {
                long val;
                PropVariantNativeMethods.PropVariantGetInt64Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(ulong), (pv, array, i) =>
            {
                ulong val;
                PropVariantNativeMethods.PropVariantGetUInt64Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(DateTime), (pv, array, i) =>
            {
                System.Runtime.InteropServices.ComTypes.FILETIME val;
                PropVariantNativeMethods.PropVariantGetFileTimeElem(pv, i, out val);

                long fileTime = GetFileTimeAsLong(ref val);

                array.SetValue(DateTime.FromFileTime(fileTime), i);
            });

            cache.Add(typeof(bool), (pv, array, i) =>
            {
                bool val;
                PropVariantNativeMethods.PropVariantGetBooleanElem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(double), (pv, array, i) =>
            {
                double val;
                PropVariantNativeMethods.PropVariantGetDoubleElem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(float), (pv, array, i) => // float
            {
                float[] val = new float[1];
                Marshal.Copy(pv.ptr2, val, (int)i, 1);
                array.SetValue(val[0], (int)i);
            });

            cache.Add(typeof(decimal), (pv, array, i) =>
            {
                int[] val = new int[4];
                for (int a = 0; a < val.Length; a++)
                {
                    val[a] = Marshal.ReadInt32(pv.ptr2,
                        ((int)i * sizeof(decimal)) + (a * sizeof(int))); // index * size + offset quarter
                }

                array.SetValue(new decimal(val), i);
            });

            cache.Add(typeof(string), (pv, array, i) =>
            {
                string val = string.Empty;
                PropVariantNativeMethods.PropVariantGetStringElem(pv, i, ref val);
                array.SetValue(val, i);
            });

            return cache;
        }
        #endregion

        #region Dynamic Construction / Factory (Expressions)

        /// <summary>
        /// Attempts to create a PropVariant by finding an appropriate constructor.
        /// </summary>
        /// <param name="value">Object from which PropVariant should be created.</param>
        public static PropVariant FromObject(object value)
        {
            if (value == null)
            {
                return new PropVariant();
            }
            else
            {
                var func = GetDynamicConstructor(value.GetType());
                return func(value);
            }
        }

        // A dictionary and lock to contain compiled expression trees for constructors
        private static Dictionary<Type, Func<object, PropVariant>> cache = new Dictionary<Type, Func<object, PropVariant>>();
        private static object padlock = new object();

        // Retrieves a cached constructor expression.
        // If no constructor has been cached, it attempts to find/add it.  If it cannot be found
        // an exception is thrown.
        // This method looks for a public constructor with the same parameter type as the object.
        private static Func<object, PropVariant> GetDynamicConstructor(Type type)
        {
            lock (padlock)
            {
                // initial check, if action is found, return it
                Func<object, PropVariant> action;
                if (!cache.TryGetValue(type, out action))
                {
                    // iterates through all constructors
                    ConstructorInfo constructor = typeof(PropVariant)
                        .GetConstructor(new Type[] { type });

                    if (constructor == null)
                    { // if the method was not found, throw.
                        throw new ArgumentException(LocalizedMessages.PropVariantTypeNotSupported);
                    }
                    else // if the method was found, create an expression to call it.
                    {
                        // create parameters to action
                        var arg = Expression.Parameter(typeof(object), "arg");

                        // create an expression to invoke the constructor with an argument cast to the correct type
                        var create = Expression.New(constructor, Expression.Convert(arg, type));

                        // compiles expression into an action delegate
                        action = Expression.Lambda<Func<object, PropVariant>>(create, arg).Compile();
                        cache.Add(type, action);
                    }
                }

                return action;
            }
        }

        #endregion

        #region Fields

        [FieldOffset(0)]
        private decimal @decimal;

        // This is actually a VarEnum value, but the VarEnum type
        // requires 4 bytes instead of the expected 2.
        [FieldOffset(0)]
        private ushort valueType;

        // Reserved Fields
        // [FieldOffset(2)]
        // ushort _wReserved1;
        // [FieldOffset(4)]
        // ushort _wReserved2;
        // [FieldOffset(6)]
        // ushort _wReserved3;

        // In order to allow x64 compat, we need to allow for
        // expansion of the IntPtr. However, the BLOB struct
        // uses a 4-byte int, followed by an IntPtr, so
        // although the valueData field catches most pointer values,
        // we need an additional 4-bytes to get the BLOB
        // pointer. The valueDataExt field provides this, as well as
        // the last 4-bytes of an 8-byte value on 32-bit
        // architectures.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "ToDo")]
        [FieldOffset(12)]
        private IntPtr ptr2;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "ToDo")]
        [FieldOffset(8)]
        private IntPtr ptr;
        [FieldOffset(8)]
        private int int32;
        [FieldOffset(8)]
        private uint uint32;
        [FieldOffset(8)]
        private byte @byte;
        [FieldOffset(8)]
        private sbyte @sbyte;
        [FieldOffset(8)]
        private short @short;
        [FieldOffset(8)]
        private ushort @ushort;
        [FieldOffset(8)]
        private long @long;
        [FieldOffset(8)]
        private ulong @ulong;
        [FieldOffset(8)]
        private double @double;
        [FieldOffset(8)]
        private float @float;

        #endregion // struct fields

        #region Constructors

        /// <summary>
        /// Default constrcutor
        /// </summary>
        public PropVariant()
        {
            // left empty
        }

        /// <summary>
        /// Set a string value
        /// </summary>
        public PropVariant(string value)
        {
            if (value == null)
            {
                throw new ArgumentException(LocalizedMessages.PropVariantNullString, nameof(value));
            }

            this.valueType = (ushort)VarEnum.VT_LPWSTR;
            this.ptr = Marshal.StringToCoTaskMemUni(value);
        }

        /// <summary>
        /// Set a string vector
        /// </summary>
        public PropVariant(string[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromStringVector(value, (uint)value.Length, this);
        }

        /// <summary>
        /// Set a bool vector
        /// </summary>
        public PropVariant(bool[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromBooleanVector(value, (uint)value.Length, this);
        }

        /// <summary>
        /// Set a short vector
        /// </summary>
        public PropVariant(short[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromInt16Vector(value, (uint)value.Length, this);
        }

        /// <summary>
        /// Set a short vector
        /// </summary>
        public PropVariant(ushort[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromUInt16Vector(value, (uint)value.Length, this);
        }

        /// <summary>
        /// Set an int vector
        /// </summary>
        public PropVariant(int[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromInt32Vector(value, (uint)value.Length, this);
        }

        /// <summary>
        /// Set an uint vector
        /// </summary>
        public PropVariant(uint[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromUInt32Vector(value, (uint)value.Length, this);
        }

        /// <summary>
        /// Set a long vector
        /// </summary>
        public PropVariant(long[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromInt64Vector(value, (uint)value.Length, this);
        }

        /// <summary>
        /// Set a ulong vector
        /// </summary>
        public PropVariant(ulong[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromUInt64Vector(value, (uint)value.Length, this);
        }

        /// <summary>>
        /// Set a double vector
        /// </summary>
        public PropVariant(double[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            PropVariantNativeMethods.InitPropVariantFromDoubleVector(value, (uint)value.Length, this);
        }

        /// <summary>
        /// Set a DateTime vector
        /// </summary>
        public PropVariant(DateTime[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            System.Runtime.InteropServices.ComTypes.FILETIME[] fileTimeArr =
                new System.Runtime.InteropServices.ComTypes.FILETIME[value.Length];

            for (int i = 0; i < value.Length; i++)
            {
                fileTimeArr[i] = DateTimeToFileTime(value[i]);
            }

            PropVariantNativeMethods.InitPropVariantFromFileTimeVector(fileTimeArr, (uint)fileTimeArr.Length, this);
        }

        /// <summary>
        /// Set a bool value
        /// </summary>
        public PropVariant(bool value)
        {
            this.valueType = (ushort)VarEnum.VT_BOOL;
            this.int32 = (value == true) ? -1 : 0;
        }

        /// <summary>
        /// Set a DateTime value
        /// </summary>
        public PropVariant(DateTime value)
        {
            this.valueType = (ushort)VarEnum.VT_FILETIME;

            System.Runtime.InteropServices.ComTypes.FILETIME ft = DateTimeToFileTime(value);
            PropVariantNativeMethods.InitPropVariantFromFileTime(ref ft, this);
        }

        /// <summary>
        /// Set a byte value
        /// </summary>
        public PropVariant(byte value)
        {
            this.valueType = (ushort)VarEnum.VT_UI1;
            this.@byte = value;
        }

        /// <summary>
        /// Set a sbyte value
        /// </summary>
        public PropVariant(sbyte value)
        {
            this.valueType = (ushort)VarEnum.VT_I1;
            this.@sbyte = value;
        }

        /// <summary>
        /// Set a short value
        /// </summary>
        public PropVariant(short value)
        {
            this.valueType = (ushort)VarEnum.VT_I2;
            this.@short = value;
        }

        /// <summary>
        /// Set an unsigned short value
        /// </summary>
        public PropVariant(ushort value)
        {
            this.valueType = (ushort)VarEnum.VT_UI2;
            this.@ushort = value;
        }

        /// <summary>
        /// Set an int value
        /// </summary>
        public PropVariant(int value)
        {
            this.valueType = (ushort)VarEnum.VT_I4;
            this.int32 = value;
        }

        /// <summary>
        /// Set an unsigned int value
        /// </summary>
        public PropVariant(uint value)
        {
            this.valueType = (ushort)VarEnum.VT_UI4;
            this.uint32 = value;
        }

        /// <summary>
        /// Set a decimal  value
        /// </summary>
        public PropVariant(decimal value)
        {
            this.@decimal = value;

            // It is critical that the value type be set after the decimal value, because they overlap.
            // If valuetype is written first, its value will be lost when _decimal is written.
            this.valueType = (ushort)VarEnum.VT_DECIMAL;
        }

        /// <summary>
        /// Create a PropVariant with a contained decimal array.
        /// </summary>
        /// <param name="value">Decimal array to wrap.</param>
        public PropVariant(decimal[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.valueType = (ushort)(VarEnum.VT_DECIMAL | VarEnum.VT_VECTOR);
            this.int32 = value.Length;

            // allocate required memory for array with 128bit elements
            this.ptr2 = Marshal.AllocCoTaskMem(value.Length * sizeof(decimal));
            for (int i = 0; i < value.Length; i++)
            {
                int[] bits = decimal.GetBits(value[i]);
                Marshal.Copy(bits, 0, this.ptr2, bits.Length);
            }
        }

        /// <summary>
        /// Create a PropVariant containing a float type.
        /// </summary>
        public PropVariant(float value)
        {
            this.valueType = (ushort)VarEnum.VT_R4;

            this.@float = value;
        }

        /// <summary>
        /// Creates a PropVariant containing a float[] array.
        /// </summary>
        public PropVariant(float[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.valueType = (ushort)(VarEnum.VT_R4 | VarEnum.VT_VECTOR);
            this.int32 = value.Length;

            this.ptr2 = Marshal.AllocCoTaskMem(value.Length * sizeof(float));

            Marshal.Copy(value, 0, this.ptr2, value.Length);
        }

        /// <summary>
        /// Set a long
        /// </summary>
        public PropVariant(long value)
        {
            this.@long = value;
            this.valueType = (ushort)VarEnum.VT_I8;
        }

        /// <summary>
        /// Set a ulong
        /// </summary>
        public PropVariant(ulong value)
        {
            this.valueType = (ushort)VarEnum.VT_UI8;
            this.@ulong = value;
        }

        /// <summary>
        /// Set a double
        /// </summary>
        public PropVariant(double value)
        {
            this.valueType = (ushort)VarEnum.VT_R8;
            this.@double = value;
        }

        #endregion

        #region Uncalled methods - These are currently not called, but I think may be valid in the future.

        /// <summary>
        /// Set an IUnknown value
        /// </summary>
        /// <param name="value">The new value to set.</param>
        internal void SetIUnknown(object value)
        {
            this.valueType = (ushort)VarEnum.VT_UNKNOWN;
            this.ptr = Marshal.GetIUnknownForObject(value);
        }

        /// <summary>
        /// Set a safe array value
        /// </summary>
        /// <param name="array">The new value to set.</param>
        internal void SetSafeArray(Array array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            const ushort vtUnknown = 13;
            IntPtr psa = PropVariantNativeMethods.SafeArrayCreateVector(vtUnknown, 0, (uint)array.Length);

            IntPtr pvData = PropVariantNativeMethods.SafeArrayAccessData(psa);
            try // to remember to release lock on data
            {
                for (int i = 0; i < array.Length; ++i)
                {
                    object obj = array.GetValue(i);
                    IntPtr punk = (obj != null) ? Marshal.GetIUnknownForObject(obj) : IntPtr.Zero;
                    Marshal.WriteIntPtr(pvData, i * IntPtr.Size, punk);
                }
            }
            finally
            {
                PropVariantNativeMethods.SafeArrayUnaccessData(psa);
            }

            this.valueType = (ushort)VarEnum.VT_ARRAY | (ushort)VarEnum.VT_UNKNOWN;
            this.ptr = psa;
        }

        #endregion

        #region public Properties

        /// <summary>
        /// Gets or sets the variant type.
        /// </summary>
        public VarEnum VarType
        {
            get => (VarEnum)this.valueType;
            set => this.valueType = (ushort)value;
        }

        /// <summary>
        /// Checks if this has an empty or null value
        /// </summary>
        /// <returns></returns>
        public bool IsNullOrEmpty => this.valueType == (ushort)VarEnum.VT_EMPTY || this.valueType == (ushort)VarEnum.VT_NULL;

        /// <summary>
        /// Gets the variant value.
        /// </summary>
        public object Value
        {
            get
            {
                switch ((VarEnum)this.valueType)
                {
                    case VarEnum.VT_I1:
                        return this.@sbyte;
                    case VarEnum.VT_UI1:
                        return this.@byte;
                    case VarEnum.VT_I2:
                        return this.@short;
                    case VarEnum.VT_UI2:
                        return this.@ushort;
                    case VarEnum.VT_I4:
                    case VarEnum.VT_INT:
                        return this.int32;
                    case VarEnum.VT_UI4:
                    case VarEnum.VT_UINT:
                        return this.uint32;
                    case VarEnum.VT_I8:
                        return this.@long;
                    case VarEnum.VT_UI8:
                        return this.@ulong;
                    case VarEnum.VT_R4:
                        return this.@float;
                    case VarEnum.VT_R8:
                        return this.@double;
                    case VarEnum.VT_BOOL:
                        return this.int32 == -1;
                    case VarEnum.VT_ERROR:
                        return this.@long;
                    case VarEnum.VT_CY:
                        return this.@decimal;
                    case VarEnum.VT_DATE:
                        return DateTime.FromOADate(this.@double);
                    case VarEnum.VT_FILETIME:
                        return DateTime.FromFileTime(this.@long);
                    case VarEnum.VT_BSTR:
                        return Marshal.PtrToStringBSTR(this.ptr);
                    case VarEnum.VT_BLOB:
                        return this.GetBlobData();
                    case VarEnum.VT_LPSTR:
                        return Marshal.PtrToStringAnsi(this.ptr);
                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(this.ptr);
                    case VarEnum.VT_UNKNOWN:
                        return Marshal.GetObjectForIUnknown(this.ptr);
                    case VarEnum.VT_DISPATCH:
                        return Marshal.GetObjectForIUnknown(this.ptr);
                    case VarEnum.VT_DECIMAL:
                        return this.@decimal;
                    case VarEnum.VT_ARRAY | VarEnum.VT_UNKNOWN:
                        return CrackSingleDimSafeArray(this.ptr);
                    case VarEnum.VT_VECTOR | VarEnum.VT_LPWSTR:
                        return this.GetVector<string>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_I2:
                        return this.GetVector<short>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_UI2:
                        return this.GetVector<ushort>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_I4:
                        return this.GetVector<int>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_UI4:
                        return this.GetVector<uint>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_I8:
                        return this.GetVector<long>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_UI8:
                        return this.GetVector<ulong>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_R4:
                        return this.GetVector<float>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_R8:
                        return this.GetVector<double>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_BOOL:
                        return this.GetVector<bool>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_FILETIME:
                        return this.GetVector<DateTime>();
                    case VarEnum.VT_VECTOR | VarEnum.VT_DECIMAL:
                        return this.GetVector<decimal>();
                    default:
                        // if the value cannot be marshaled
                        return null;
                }
            }
        }

        #endregion

        #region Private Methods

        private static long GetFileTimeAsLong(ref System.Runtime.InteropServices.ComTypes.FILETIME val)
        {
            return (((long)val.dwHighDateTime) << 32) + val.dwLowDateTime;
        }

        private static System.Runtime.InteropServices.ComTypes.FILETIME DateTimeToFileTime(DateTime value)
        {
            long hFT = value.ToFileTime();
            System.Runtime.InteropServices.ComTypes.FILETIME ft =
                new System.Runtime.InteropServices.ComTypes.FILETIME();
            ft.dwLowDateTime = (int)(hFT & 0xFFFFFFFF);
            ft.dwHighDateTime = (int)(hFT >> 32);
            return ft;
        }

        private object GetBlobData()
        {
            byte[] blobData = new byte[this.int32];

            IntPtr pBlobData = this.ptr2;
            Marshal.Copy(pBlobData, blobData, 0, this.int32);

            return blobData;
        }

        private Array GetVector<T>()
        {
            int count = PropVariantNativeMethods.PropVariantGetElementCount(this);
            if (count <= 0)
            {
                return null;
            }

            lock (padlock)
            {
                if (vectorActions == null)
                {
                    vectorActions = GenerateVectorActions();
                }
            }

            Action<PropVariant, Array, uint> action;
            if (!vectorActions.TryGetValue(typeof(T), out action))
            {
                throw new InvalidCastException(LocalizedMessages.PropVariantUnsupportedType);
            }

            Array array = new T[count];
            for (uint i = 0; i < count; i++)
            {
                action(this, array, i);
            }

            return array;
        }

        private static Array CrackSingleDimSafeArray(IntPtr psa)
        {
            uint cDims = PropVariantNativeMethods.SafeArrayGetDim(psa);
            if (cDims != 1)
            {
                throw new ArgumentException(LocalizedMessages.PropVariantMultiDimArray, nameof(psa));
            }

            int lBound = PropVariantNativeMethods.SafeArrayGetLBound(psa, 1U);
            int uBound = PropVariantNativeMethods.SafeArrayGetUBound(psa, 1U);

            int n = uBound - lBound + 1; // uBound is inclusive

            object[] array = new object[n];
            for (int i = lBound; i <= uBound; ++i)
            {
                array[i] = PropVariantNativeMethods.SafeArrayGetElement(psa, ref i);
            }

            return array;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the object, calls the clear function.
        /// </summary>
        public void Dispose()
        {
            PropVariantNativeMethods.PropVariantClear(this);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~PropVariant()
        {
            this.Dispose();
        }

        #endregion

        /// <summary>
        /// Provides an simple string representation of the contained data and type.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0}: {1}", this.Value, this.VarType.ToString());
        }
    }
}
