namespace TMP.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;

    /// <summary>
    /// Object extensions methods which are available if you are using VS2008
    /// </summary>
    public static class ObjectExtensions
    {
        private static XmlDocument staticDoc;

        /// <summary>
        /// Checks if the string is empty or only contains spaces
        /// </summary>
        /// <param name="text">String on which extension method is used</param>
        /// <returns>true if empty, false otherwise</returns>
        public static bool IsNullOrEmpty(this string text)
        {
            return text == null || string.IsNullOrEmpty(text.Trim());
        }

        /// <summary>
        /// Checks if the string is a valid email address
        /// </summary>
        /// <param name="inputEmail">String on which extension method is used</param>
        /// <returns>true if valid email address, false otherwise</returns>
        public static bool IsValidEmail(this string inputEmail)
        {
            return inputEmail.IsValidEmail(false);
        }

        /// <summary>
        /// Checks if the string is empty or only contains spaces
        /// </summary>
        /// <param name="inputEmail">String on which extension method is used</param>
        /// <param name="isEmptyValid">Is a empty string is considered a valid email address?</param>
        /// <returns>true if empty, false otherwise</returns>
        public static bool IsValidEmail(this string inputEmail, bool isEmptyValid)
        {
            bool result;
            if (inputEmail.IsNullOrEmpty())
            {
                result = isEmptyValid;
            }
            else
            {
                string pattern = "^([a-zA-Z0-9_\\-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([a-zA-Z0-9\\-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$";
                Regex regex = new Regex(pattern);
                result = regex.IsMatch(inputEmail);
            }

            return result;
        }

        /// <summary>
        /// XML encodes a string
        /// </summary>
        /// <param name="text">String on which extension method is used</param>
        /// <returns>returns the encoded string</returns>
        public static string XmlEncode(this string text)
        {
            string result;
            if (text == null)
            {
                result = string.Empty;
            }
            else
            {
                if (ObjectExtensions.staticDoc == null)
                {
                    ObjectExtensions.staticDoc = new XmlDocument();
                    ObjectExtensions.staticDoc.LoadXml("<text></text>");
                }

                XmlDocument staticDoc;
                Monitor.Enter(staticDoc = ObjectExtensions.staticDoc);
                try
                {
                    ObjectExtensions.staticDoc.LastChild.InnerText = text;
                    result = ObjectExtensions.staticDoc.LastChild.InnerXml;
                }
                finally
                {
                    Monitor.Exit(staticDoc);
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a string to a integar form
        /// </summary>
        /// <typeparam name="T">Integar type</typeparam>
        /// <param name="text">String to parse</param>
        /// <param name="variable">Variable to store value to</param>
        /// <returns>true if parse was successful, false otherwiseParsed string</returns>
        public static bool ParseToInt<T>(this string text, out T variable)
        {
            if (!typeof(T).IsPrimitive && typeof(T).FullName != "System.Decimal")
            {
                throw new ArgumentException("'variable' must only be a primitive type");
            }

            decimal num;
            bool result;
            if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out num))
            {
                variable = default(T);
                result = false;
            }
            else
            {
                variable = (T)(object)Convert.ChangeType(num, typeof(T), CultureInfo.InvariantCulture);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Checks whether a enum value is correct or not
        /// </summary>
        /// <param name="enumerator">Enumerator on which extension method is used</param>
        /// <returns>returns true if valid, false otherwise</returns>
        /// <remarks>
        /// This is a interesting function. If you set a enum to a int value, you can use
        /// this function to test the value to be correct or not. What is special about this
        /// function is that if you use the Flags attribute for the enum, it will check if the
        /// value is a right bit wise match. This can better be understood with the example
        /// provided.
        /// </remarks>
        /// <example>
        /// <code>
        /// public enum NonFlagEnum
        /// {
        /// 	One = 1,
        /// 	Two = 2,
        /// 	Three = 4,
        /// 	Four = 8,
        /// 	Five = 16
        /// }
        ///
        /// [Flags]
        /// public enum FlagEnum
        /// {
        /// 	One = 1,
        /// 	Two = 2,
        /// 	Three = 4,
        /// 	Four = 8,
        /// 	Five = 16,
        /// }
        ///
        /// // Non flaged
        /// NonFlagEnum nfe = NonFlagEnum.One;
        /// nfe.IsValid(); // true
        ///
        /// nfe = (NonFlagEnum) 5;
        /// nfe.IsValid(); // false
        ///
        /// nfe = (NonFlagEnum) 21;
        /// nfe.IsValid(); // false
        ///
        /// nfe = (NonFlagEnum) 32;
        /// nfe.IsValid(); // false
        ///
        /// nfe = NonFlagEnum.One | NonFlagEnum.Four;
        /// nfe.IsValid(); // false because 9 is not present in the enum
        ///
        /// // Flaged
        /// FlagEnum fe = FlagEnum.One;
        /// fe.IsValid(); // true
        ///
        /// fe = (FlagEnum) 5;
        /// // true because value will be FlagEnum.One | FlagEnum.Three
        /// fe.IsValid();
        ///
        /// fe = (FlagEnum) 21;
        /// // true because value will be:
        /// // FlagEnum.Five | FlagEnum.Three | FlagEnum.One
        /// fe.IsValid();
        ///
        /// fe = (FlagEnum) 32;
        /// fe.IsValid(); // false
        ///
        /// fe = FlagEnum.One | FlagEnum.Four;
        /// fe.IsValid(); // true
        /// </code>
        /// </example>
        public static bool IsValid(this Enum enumerator)
        {
            bool flag = Enum.IsDefined(enumerator.GetType(), enumerator);
            bool result;
            if (!flag)
            {
                FlagsAttribute[] array = (FlagsAttribute[])enumerator.GetType().GetCustomAttributes(typeof(FlagsAttribute), false);
                if (array != null && array.Length > 0)
                {
                    result = enumerator.ToString().Contains(",");
                    return result;
                }
            }

            result = flag;
            return result;
        }

        /// <summary>
        /// Returns enum's description
        /// </summary>
        /// <param name="enumerator">Enumerator on which extension method is used</param>
        /// <returns>If the enum value has a <code>DescriptionAttribute</code>, returns the
        /// description, otherwise returns <code>ToString()</code></returns>
        public static string GetDescription(this Enum enumerator)
        {
            FieldInfo field = enumerator.GetType().GetField(enumerator.ToString());
            DescriptionAttribute[] array = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            string result;
            if (array != null && array.Length > 0)
            {
                result = array[0].Description;
            }
            else
            {
                result = enumerator.ToString();
            }

            return result;
        }

        /// <summary>
        /// Check whether a flag is set in a particular enum value
        /// </summary>
        /// <param name="enumerator">Enumerator on which extension method is used</param>
        /// <param name="enumFlag">Enumerator flag to check</param>
        /// <returns>returns true if flag set, false otherwise</returns>
        public static bool IsFlagSet(this Enum enumerator, Enum enumFlag)
        {
            bool result;
            if (!enumerator.IsValid())
            {
                result = false;
            }
            else
            {
                if (!enumFlag.IsValid())
                {
                    result = false;
                }
                else
                {
                    if (enumerator.GetType() != enumFlag.GetType())
                    {
                        result = false;
                    }
                    else
                    {
                        int num = (int)Enum.Parse(enumerator.GetType(), enumerator.ToString());
                        int num2 = (int)Enum.Parse(enumFlag.GetType(), enumFlag.ToString());
                        result = (num & num2) == num2;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns difference of two dates
        /// </summary>
        /// <param name="date">DateTime on which extension method is used</param>
        /// <param name="dateToCompare">The second date to compare difference with</param>
        /// <returns>return DateSpan instance of the calculated date difference</returns>
        /// <remarks>The computation always is date - dateToCompare, so dateToCompare should be
        /// lesser than date. If it is not, the answer is always a little bit... ahem... weird.</remarks>
        public static DateSpan DateDifference(this DateTime date, DateTime dateToCompare)
        {
            int num = ((date.Year - dateToCompare.Year) * 12) + date.Month - dateToCompare.Month;
            int days;
            if (date.Day < dateToCompare.Day)
            {
                int day = dateToCompare.Day;
                int month;
                int year;
                if (date.Month == 1)
                {
                    month = 12;
                    year = date.Year - 1;
                }
                else
                {
                    month = date.Month - 1;
                    year = date.Year;
                }

                DateTime d = new DateTime(year, month, day);
                days = (date - d).Days;
                num--;
            }
            else
            {
                days = date.Day - dateToCompare.Day;
            }

            return new DateSpan
            {
                Years = num / 12,
                Months = num % 12,
                Days = days,
            };
        }

        /// <summary>
        /// Extension method to get a single attribute and its value
        /// </summary>
        /// <param name="reader">XmlReader on which extension method is used</param>
        /// <param name="attribute">Attribute to read</param>
        /// <returns>XmlReaderAttributeItem instance if attribute found, null otherwise</returns>
        public static XmlReaderAttributeItem GetSingleAttribute(this XmlReader reader, string attribute)
        {
            return reader.GetSingleAttribute(attribute, false);
        }

        /// <summary>
        /// Extension method to get a single attribute and its value
        /// </summary>
        /// <param name="reader">XmlReader on which extension method is used</param>
        /// <param name="attribute">Attribute to read</param>
        /// <param name="moveToEnd">Move to the end of the element?</param>
        /// <returns>XmlReaderAttributeItem instance if attribute found, null otherwise</returns>
        public static XmlReaderAttributeItem GetSingleAttribute(this XmlReader reader, string attribute, bool moveToEnd)
        {
            string name = reader.Name;
            if (moveToEnd && reader.IsEmptyElement)
            {
                moveToEnd = false;
            }

            XmlReaderAttributeItem result;
            foreach (XmlReaderAttributeItem current in reader.GetAttributes())
            {
                if (current.LocalName == attribute)
                {
                    if (moveToEnd)
                    {
                        while (reader.Read() && (!(reader.Name == name) || reader.NodeType != XmlNodeType.EndElement))
                        {
                        }
                    }

                    result = current;
                    return result;
                }
            }

            if (moveToEnd)
            {
                while (reader.Read() && (!(reader.Name == name) || reader.NodeType != XmlNodeType.EndElement))
                {
                }
            }

            result = null;
            return result;
        }

        /// <summary>
        /// Extension method to get a all attribute of a element
        /// </summary>
        /// <param name="reader">XmlReader on which extension method is used</param>
        /// <returns>List of all attributes as XmlReaderAttributeItem</returns>
        public static IEnumerable<XmlReaderAttributeItem> GetAttributes(this XmlReader reader)
        {
            List<XmlReaderAttributeItem> list = new List<XmlReaderAttributeItem>();
            IEnumerable<XmlReaderAttributeItem> result;
            if (!reader.HasAttributes)
            {
                result = list;
            }
            else
            {
                reader.MoveToFirstAttribute();
                list.Add(ObjectExtensions.ReadAttribute(reader));
                while (reader.MoveToNextAttribute())
                {
                    list.Add(ObjectExtensions.ReadAttribute(reader));
                }

                result = list;
            }

            return result;
        }

        private static XmlReaderAttributeItem ReadAttribute(XmlReader reader)
        {
            XmlReaderAttributeItem xmlReaderAttributeItem = new XmlReaderAttributeItem();
            xmlReaderAttributeItem.Name = reader.Name;
            xmlReaderAttributeItem.LocalName = reader.LocalName;
            xmlReaderAttributeItem.Prefix = reader.Prefix;
            xmlReaderAttributeItem.HasValue = reader.HasValue;
            if (xmlReaderAttributeItem.HasValue)
            {
                xmlReaderAttributeItem.Value = reader.Value;
            }
            else
            {
                xmlReaderAttributeItem.Value = string.Empty;
            }

            return xmlReaderAttributeItem;
        }

        /// <summary>
        /// Parses a enum from a string
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="value">String to parse</param>
        /// <returns>Parsed enum value</returns>
        public static T ParseEnum<T>(string value)
        {
            return (T)(object)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Checks where a type is a numeric type
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>true if type is numeric type, false otherwise</returns>
        public static bool IsNumericType(Type type)
        {
            string fullName = type.FullName;
            string text = fullName;
            bool result;
            switch (text)
            {
                case "System.SByte":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.Byte":
                case "System.UInt16":
                case "System.UInt32":
                case "System.UInt64":
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                    result = true;
                    return result;
            }

            result = false;
            return result;
        }
    }
}
