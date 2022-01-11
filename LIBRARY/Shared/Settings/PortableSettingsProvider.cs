namespace TMP.Shared.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml;

    public sealed class PortableSettingsProvider : SettingsProvider, IApplicationSettingsProvider
    {
        private const string className = "PortableSettingsProvider";
        private const string globalSettingsNodeName = "globalSettings";
        private const string _localSettingsNodeName = "localSettings";
        private const string _rootNodeName = "settings";
        private XmlDocument _xmlDocument;

        #region Private Methods

        private XmlNode GetSettingsNode(string name)
        {
            XmlNode settingsNode = this.RootNode.SelectSingleNode(name);

            if (settingsNode == null)
            {
                settingsNode = this.RootDocument.CreateElement(name);
                this.RootNode.AppendChild(settingsNode);
            }

            return settingsNode;
        }

        private string GetValue(SettingsProperty property)
        {
            XmlNode targetNode = this.IsGlobal(property) ? this.GlobalSettingsNode : this.LocalSettingsNode;
            XmlNode settingNode = targetNode.SelectSingleNode(string.Format("setting[@name='{0}']", property.Name));

            if (settingNode == null)
            {
                return property.DefaultValue != null ? property.DefaultValue.ToString() : string.Empty;
            }

            return settingNode.InnerText;
        }

        private bool IsGlobal(SettingsProperty property)
        {
            foreach (DictionaryEntry attribute in property.Attributes)
            {
                if ((Attribute)attribute.Value is SettingsManageabilityAttribute)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetValue(SettingsPropertyValue propertyValue)
        {
            XmlNode targetNode = this.IsGlobal(propertyValue.Property)
               ? this.GlobalSettingsNode
               : this.LocalSettingsNode;

            XmlNode settingNode = targetNode.SelectSingleNode(string.Format("setting[@name='{0}']", propertyValue.Name));

            if (settingNode != null)
            {
                settingNode.InnerText = propertyValue.SerializedValue?.ToString();
            }
            else
            {
                settingNode = this.RootDocument.CreateElement("setting");

                XmlAttribute nameAttribute = this.RootDocument.CreateAttribute("name");
                nameAttribute.Value = propertyValue.Name;

                settingNode.Attributes.Append(nameAttribute);
                settingNode.InnerText = propertyValue.SerializedValue?.ToString();

                targetNode.AppendChild(settingNode);
            }
        }

        #endregion

        #region Private Propeties

        private string FilePath => Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
                   string.Format("{0}.settings", this.ApplicationName));

        private XmlNode GlobalSettingsNode => this.GetSettingsNode(globalSettingsNodeName);

        private XmlNode LocalSettingsNode
        {
            get
            {
                XmlNode settingsNode = this.GetSettingsNode(_localSettingsNodeName);
                XmlNode machineNode = settingsNode.SelectSingleNode(Environment.MachineName.ToLowerInvariant());

                if (machineNode == null)
                {
                    machineNode = this.RootDocument.CreateElement(Environment.MachineName.ToLowerInvariant());
                    settingsNode.AppendChild(machineNode);
                }

                return machineNode;
            }
        }

        private XmlDocument RootDocument
        {
            get
            {
                if (this._xmlDocument == null)
                {
                    try
                    {
                        this._xmlDocument = new XmlDocument();
                        if (System.IO.File.Exists(this.FilePath) == false)
                        {
                            return this._xmlDocument = this.GetBlankXmlDocument();
                        }
                        else
                        {
                            this._xmlDocument.Load(this.FilePath);
                        }
                    }
                    catch (Exception)
                    {
                    }

                    if (this._xmlDocument.SelectSingleNode(_rootNodeName) != null)
                    {
                        return this._xmlDocument;
                    }

                    this._xmlDocument = this.GetBlankXmlDocument();
                }

                return this._xmlDocument;
            }
        }

        private XmlNode RootNode => this.RootDocument.SelectSingleNode(_rootNodeName);

        #endregion

        #region Public

        public XmlDocument GetBlankXmlDocument()
        {
            XmlDocument blankXmlDocument = new XmlDocument();
            blankXmlDocument.AppendChild(blankXmlDocument.CreateXmlDeclaration("1.0", "utf-8", string.Empty));
            blankXmlDocument.AppendChild(blankXmlDocument.CreateElement(_rootNodeName));

            return blankXmlDocument;
        }

        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            // do nothing
            return new SettingsPropertyValue(property);
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

            foreach (SettingsProperty property in collection)
            {
                values.Add(new SettingsPropertyValue(property)
                {
                    SerializedValue = this.GetValue(property),
                });
            }

            return values;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(this.Name, config);
        }

        public void Reset(SettingsContext context)
        {
            this.LocalSettingsNode.RemoveAll();
            this.GlobalSettingsNode.RemoveAll();

            this._xmlDocument.Save(this.FilePath);
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            foreach (SettingsPropertyValue propertyValue in collection)
            {
                this.SetValue(propertyValue);
            }

            try
            {
                this.RootDocument.Save(this.FilePath);
            }
            catch (Exception)
            {
                /*
                 * If this is a portable application and the device has been
                 * removed then this will fail, so don't do anything. It's
                 * probably better for the application to stop saving settings
                 * rather than just crashing outright. Probably.
                 */
            }
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties)
        {
        }

        public override string ApplicationName
        {
            get => Path.GetFileNameWithoutExtension(Application.ExecutablePath);
            set { }
        }

        public override string Name => className;

        #endregion
    }
}