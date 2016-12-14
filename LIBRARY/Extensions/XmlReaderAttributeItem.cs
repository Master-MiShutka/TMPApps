using System;
namespace TMP.Extensions
{
    /// <summary>
    /// Extended xml attribute
    /// </summary>
    public class XmlReaderAttributeItem
    {
        /// <summary>
        /// Name of attribute
        /// </summary>
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// Local name of attribute
        /// </summary>
        public string LocalName
        {
            get;
            set;
        }
        /// <summary>
        /// Value of attribute
        /// </summary>
        public string Value
        {
            get;
            set;
        }
        /// <summary>
        /// Prefix if any
        /// </summary>
        public string Prefix
        {
            get;
            set;
        }
        /// <summary>
        /// Has a value?
        /// </summary>
        public bool HasValue
        {
            get;
            set;
        }
    }
}
