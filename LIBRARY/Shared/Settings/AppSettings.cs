using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Xml;

namespace TMP.Shared.Settings
{
    public class ClassAppSettings
    {
        private string filePath;
        public ClassAppSettings(string strPath)
        {
            this.filePath = strPath;
            this.InitializeConfigFile();
        }
        private void InitializeConfigFile()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(this.filePath);
            stringBuilder.Append(Assembly.GetExecutingAssembly().GetName());
            stringBuilder.Append(".config");
            this.filePath = stringBuilder.ToString();
            bool flag = !File.Exists(this.filePath);
            if (flag)
            {
                StreamWriter streamWriter = new StreamWriter(File.Open(this.filePath, FileMode.Create));
                streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                streamWriter.WriteLine("<configuration>");
                streamWriter.WriteLine("  <appSettings>");
                streamWriter.WriteLine("    <!--   User application and configured property settings go here.-->");
                streamWriter.WriteLine("    <!--   Example: <add key=\"settingName\" value=\"settingValue\"/> -->");
                streamWriter.WriteLine("  </appSettings>");
                streamWriter.WriteLine("</configuration>");
                streamWriter.Close();
            }
        }
        public string GetSetting(string key)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(this.filePath);
            XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key=\"" + key + "\"]");
            bool flag = xmlNode != null;
            string result;
            if (flag)
            {
                result = xmlNode.Attributes.GetNamedItem("value").Value;
            }
            else
            {
                result = null;
            }
            return result;
        }
        public void SaveSetting(string key, string value)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(this.filePath);
            XmlElement xmlElement = (XmlElement)xmlDocument.DocumentElement.SelectSingleNode("/configuration/appSettings/add[@key=\"" + key + "\"]");
            bool flag = xmlElement != null;
            if (flag)
            {
                xmlElement.Attributes.GetNamedItem("value").Value = value;
            }
            else
            {
                xmlElement = xmlDocument.CreateElement("add");
                xmlElement.SetAttribute("key", key);
                xmlElement.SetAttribute("value", value);
                XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("/configuration/appSettings");
                flag = (xmlNode != null);
                if (flag)
                {
                    xmlNode.AppendChild(xmlElement);
                }
                else
                {
                    try
                    {
                        xmlNode = xmlDocument.DocumentElement.SelectSingleNode("/configuration");
                        xmlNode.AppendChild(xmlDocument.CreateElement("appSettings"));
                        xmlNode = xmlDocument.DocumentElement.SelectSingleNode("/configuration/appSettings");
                        xmlNode.AppendChild(xmlElement);
                    }
                    catch (Exception expr_ED)
                    {
                       // ProjectData.SetProjectError(expr_ED);
                        Exception innerException = expr_ED;
                        throw new Exception("Could not set value", innerException);
                    }
                }
            }
            xmlDocument.Save(this.filePath);
        }
    }
}
