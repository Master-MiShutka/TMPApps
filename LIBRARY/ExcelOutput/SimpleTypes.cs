using System;
using System.Reflection;

namespace TMP.ExcelOutput
{
    public static class SimpleTypes
    {
        private static readonly Func<Type, bool> SimpleTypesPredicate;

        static SimpleTypes()
        {
            SimpleTypesPredicate = t =>
                                   t == typeof(byte) ||
                                   t == typeof(sbyte) ||
                                   t == typeof(int) ||
                                   t == typeof(uint) ||
                                   t == typeof(short) ||
                                   t == typeof(ushort) ||
                                   t == typeof(long) ||
                                   t == typeof(ulong) ||
                                   t == typeof(float) ||
                                   t == typeof(double) ||
                                   t == typeof(decimal) ||
                                   t == typeof(bool) ||
                                   t == typeof(char) ||
                                   t == typeof(DateTime) ||
                                   t == typeof(string) ||
                                   t.BaseType == typeof(Enum);
        }

        /// <summary>
        /// Evaluates whether <see cref="T"/> is a simple type.
        /// </summary>
        /// <typeparam name="T">T is only valid for System.Type.</typeparam>
        /// <param name="type">The type object which is to be evaluated.</param>
        /// <returns><see cref="bool"/></returns>
        public static bool IsSimpleType<T>(this T type)
            where T : Type
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return SimpleTypesPredicate(underlyingType ?? type);
        }

        /// <summary>
        /// Evaluates to see whether <see cref="type"/> is a simple type.
        /// </summary>
        /// <param name="type">The type object which is to be evaluated.</param>
        /// <returns><see cref="bool"/></returns>
        public static bool IsSimpleType(Type type)
        {
            return type.IsSimpleType();
        }
    }
}
