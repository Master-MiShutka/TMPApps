using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Extensions
{
    /// <summary>
    /// Mark a property of this class to exclude this property from getting saved
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class XmlSettingIgnoreAttribute : Attribute
    {
    }
}
