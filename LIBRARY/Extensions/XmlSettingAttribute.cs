using System;

namespace TMP.Extensions
{
    /// <summary>
    /// Define the name of a property, or force encryption or both
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class XmlSettingAttribute : Attribute
    {
        private string nameValue;
        /// <summary>
        /// Description of the property which will be shown in the settings file
        /// </summary>
        public string Name
        {
            get
            {
                return this.nameValue;
            }
        }
        /// <summary>
        /// If true, the value of the property will be encrypted
        /// </summary>
        public bool Encrypt
        {
            get;
            set;
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public XmlSettingAttribute()
        {
            this.nameValue = "";
            this.Encrypt = false;
        }
        /// <summary>
        /// Define the name of a property
        /// </summary>
        /// <param name="name">Description of the property</param>
        public XmlSettingAttribute(string name)
        {
            this.nameValue = name;
            this.Encrypt = false;
        }
    }
}
