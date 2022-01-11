namespace TMP.Extensions
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// A implementation of a base application settings class
    /// </summary>
    /// <remarks>
    /// XmlSettings is a application settings class which saves all properties of all classes
    /// derived from it to the a .settings.xml file of the same name as of the application. All
    /// base types and any classes which support serialization (like generic lists and arrays or
    /// even custom classes which support xml serialization) can be members of the derived class.
    /// If any other member is found, a exception is thrown.
    /// <para><code>XmlSettingAttribute</code> and <code>XmlSettingIgnore</code> attributes
    /// can be applied to the properties</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class TestSettings : XmlSettings
    /// {
    /// 	[XmlSettingIgnore]
    /// 	public string TestString { get; set; }
    ///
    /// 	[XmlSetting("Integar")]
    /// 	public int TestIntegar { get; set; }
    ///
    /// 	public List&lt;string&gt; GenericListOfString { get; set; }
    ///
    /// 	public TestSettings()
    /// 	{
    /// 		TestString = "Hello";
    /// 		TestIntegar = 0;
    ///
    /// 		GenericListOfString = new List&lt;string&gt;();
    /// 		GenericListOfString.Add("item0");
    /// 		GenericListOfString.Add("item1");
    /// 		GenericListOfString.Add("item2");
    /// 		GenericListOfString.Add("item3");
    /// 		GenericListOfString.Add("item4");
    /// 		GenericListOfString.Add("item5");
    /// 	}
    /// }
    /// </code>
    /// Somewhere in the code...
    /// <code>
    /// TestSettings ts = new TestSettings();
    /// ts.Save();
    /// </code>
    /// This outputs this xml file...
    /// <code>
    /// &lt;Configuration&gt;
    ///   &lt;Settings&gt;
    ///     &lt;TestSettings&gt;
    ///       &lt;Integar&gt;0&lt;/Integar&gt;
    ///       &lt;GenericListOfString&gt;
    ///         &lt;ArrayOfString xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"&gt;
    ///           &lt;string&gt;item0&lt;/string&gt;
    ///           &lt;string&gt;item1&lt;/string&gt;
    ///           &lt;string&gt;item2&lt;/string&gt;
    ///           &lt;string&gt;item3&lt;/string&gt;
    ///           &lt;string&gt;item4&lt;/string&gt;
    ///           &lt;string&gt;item5&lt;/string&gt;
    ///         &lt;/ArrayOfString&gt;
    ///       &lt;/GenericListOfString&gt;
    ///     &lt;/TestSettings&gt;
    ///   &lt;/Settings&gt;
    /// &lt;/Configuration&gt;
    /// </code>
    /// Note that TestString is not outputed, because of the use of XmlIgnore attribute and name
    /// of Integar is outputed instead of TestIntegar because of the use of XmlAttribute(attributeName)
    /// attribute.
    /// </example>
    public abstract class XmlSettings
    {
        private const string passwordKey = "XmlSettingsPK_L0020P";
        private const string defaultRootXml = "<Configuration><Settings></Settings></Configuration>";
        private const string defaultRootNotePath = "Configuration/Settings";
        private static string XmlSettingsFileName;
        private static XmlDocument XmlSettingsFile;

        public static string ExecutablePath { get; set; }

        static XmlSettings()
        {
            XmlSettings.XmlSettingsFileName = Path.ChangeExtension(ExecutablePath, ".settings.xml");
            XmlSettings.XmlSettingsFile = new XmlDocument();
            try
            {
                XmlSettings.XmlSettingsFile.Load(XmlSettings.XmlSettingsFileName);
            }
            catch
            {
                XmlSettings.XmlSettingsFile.LoadXml("<Configuration><Settings></Settings></Configuration>");
            }
        }

        private static XmlNode CreateMissingNode(string xPath)
        {
            string[] array = xPath.Split(new char[]
            {
                '/',
            });
            string text = string.Empty;
            XmlNode xmlNode = XmlSettings.XmlSettingsFile.SelectSingleNode("Configuration/Settings");
            string[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                string text2 = array2[i];
                text += text2;
                XmlNode xmlNode2 = XmlSettings.XmlSettingsFile.SelectSingleNode(text);
                if (xmlNode2 == null)
                {
                    XmlNode expr_68 = xmlNode;
                    string innerXml = expr_68.InnerXml;
                    expr_68.InnerXml = string.Concat(new string[]
                    {
                        innerXml,
                        "<",
                        text2,
                        "></",
                        text2,
                        ">",
                    });
                }

                xmlNode = XmlSettings.XmlSettingsFile.SelectSingleNode(text);
                text += "/";
            }

            return xmlNode;
        }

        private static bool IsSystemDataType(PropertyInfo property)
        {
            string fullName = property.PropertyType.FullName;
            bool result;
            switch (fullName)
            {
                case "System.DateTime":
                case "System.Byte":
                case "System.SByte":
                case "System.Double":
                case "System.Single":
                case "System.Decimal":
                case "System.Int64":
                case "System.Int32":
                case "System.Int16":
                case "System.UInt64":
                case "System.UInt32":
                case "System.UInt16":
                case "System.Boolean":
                case "System.Char":
                case "System.String":
                    result = true;
                    return result;
            }

            result = false;
            return result;
        }

        private static bool IsIgnored(PropertyInfo property)
        {
            XmlSettingIgnoreAttribute[] array = (XmlSettingIgnoreAttribute[])property.GetCustomAttributes(typeof(XmlSettingIgnoreAttribute), false);
            return array != null && array.Length > 0;
        }

        private string GetPropertyName(PropertyInfo property, out bool encrypt)
        {
            XmlSettingAttribute[] array = (XmlSettingAttribute[])property.GetCustomAttributes(typeof(XmlSettingAttribute), false);
            encrypt = false;
            string result;
            if (array != null && array.Length > 0)
            {
                encrypt = array[0].Encrypt;
                if (!string.IsNullOrEmpty(array[0].Name))
                {
                    result = base.GetType().Name + "/" + array[0].Name;
                    return result;
                }
            }

            result = base.GetType().Name + "/" + property.Name;
            return result;
        }

        private void SaveProperty(PropertyInfo property)
        {
            if (!XmlSettings.IsIgnored(property))
            {
                bool flag;
                string propertyName = this.GetPropertyName(property, out flag);
                string text = "Configuration/Settings/" + propertyName;
                XmlNode xmlNode = XmlSettings.XmlSettingsFile.SelectSingleNode(text);
                if (xmlNode == null)
                {
                    xmlNode = XmlSettings.CreateMissingNode(text);
                }

                if (XmlSettings.IsSystemDataType(property))
                {
                    string text2;
                    if (property.PropertyType.FullName == "System.DateTime")
                    {
                        text2 = ((DateTime)property.GetValue(this, null)).ToString("yyyy-MM-dd\\Thh:mm:ss.fff", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        text2 = property.GetValue(this, null).ToString();
                    }

                    if (flag)
                    {
                        text2 = EncryptDecrypt.Encrypt(text2, passwordKey);
                    }

                    xmlNode.InnerText = text2;
                }
                else
                {
                    if (!property.PropertyType.IsSerializable)
                    {
                        throw new NotSupportedException("Unsupported data found in " + base.GetType().Name + " class");
                    }

                    XmlSerializer xmlSerializer = new XmlSerializer(property.PropertyType);
                    XmlNode documentElement;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        xmlSerializer.Serialize(memoryStream, property.GetValue(this, null));
                        memoryStream.Position = 0L;
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(memoryStream);
                        documentElement = xmlDocument.DocumentElement;
                    }

                    xmlNode.RemoveAll();
                    xmlNode.AppendChild(XmlSettings.XmlSettingsFile.ImportNode(documentElement, true));
                }
            }
        }

        /// <summary>
        /// Saves the derived class to settings file
        /// </summary>
        public void Save()
        {
            PropertyInfo[] properties = base.GetType().GetProperties();
            PropertyInfo[] array = properties;
            for (int i = 0; i < array.Length; i++)
            {
                PropertyInfo property = array[i];
                this.SaveProperty(property);
            }

            XmlSettings.XmlSettingsFile.Save(XmlSettings.XmlSettingsFileName);
        }

        private void LoadProperty(PropertyInfo property)
        {
            if (!XmlSettings.IsIgnored(property))
            {
                bool flag;
                string propertyName = this.GetPropertyName(property, out flag);
                string xpath = "Configuration/Settings/" + propertyName;
                XmlNode xmlNode = XmlSettings.XmlSettingsFile.SelectSingleNode(xpath);
                if (xmlNode != null)
                {
                    string text = xmlNode.InnerText;
                    if (flag)
                    {
                        text = EncryptDecrypt.Decrypt(text, passwordKey);
                    }

                    if (XmlSettings.IsSystemDataType(property))
                    {
                        if (property.PropertyType.FullName == "System.DateTime")
                        {
                            try
                            {
                                DateTime dateTime = DateTime.ParseExact(text, "yyyy-MM-dd\\Thh:mm:ss.fff", CultureInfo.InvariantCulture);
                                property.SetValue(this, dateTime, null);
                            }
                            catch (FormatException)
                            {
                            }
                        }
                        else
                        {
                            try
                            {
                                property.SetValue(this, Convert.ChangeType(text, property.PropertyType, CultureInfo.InvariantCulture), null);
                            }
                            catch (FormatException)
                            {
                            }
                        }
                    }
                    else
                    {
                        if (!property.PropertyType.IsSerializable)
                        {
                            throw new NotSupportedException("Unsupported data found in " + base.GetType().Name + " class");
                        }

                        XmlSerializer xmlSerializer = new XmlSerializer(property.PropertyType);
                        if (xmlNode.FirstChild != null)
                        {
                            XmlNodeReader xmlReader = new XmlNodeReader(xmlNode.FirstChild);
                            property.SetValue(this, xmlSerializer.Deserialize(xmlReader), null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the derived class from settings file
        /// </summary>
        public void Load()
        {
            PropertyInfo[] properties = base.GetType().GetProperties();
            PropertyInfo[] array = properties;
            for (int i = 0; i < array.Length; i++)
            {
                PropertyInfo property = array[i];
                this.LoadProperty(property);
            }
        }
    }
}
