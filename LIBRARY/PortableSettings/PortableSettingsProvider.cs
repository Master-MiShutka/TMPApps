namespace TMP.Shared.Settings
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    public class PortableSettingsProvider : SettingsProvider
    {
        private const string XMLROOT = "configuration"; // XML Root node
        private const string CONFIGNODE = "configSections"; // Configuration declaration node
        private const string GROUPNODE = "sectionGroup"; // Configuration section group declaration node
        private const string USERNODE = "userSettings"; // User section node

        // Application Specific Node

        private static string configName;
        private static string configDirectory;
        private string aPPNODE;

        // Store instace of calling assembly
        private Assembly entryAssembly;

        private XmlDocument xmlDoc;

        public override string ApplicationName
        {
            get { return (this.entryAssembly.GetName().Name); }
            set { this.aPPNODE = value; }
        }

        private XmlDocument XMLConfig
        {
            get
            {
                // Check if we already have accessed the XML config file. If the xmlDoc object is empty, we have not.
                if (this.xmlDoc == null)
                {
                    this.xmlDoc = new XmlDocument();

                    // If we have not loaded the config, try reading the file from disk.
                    try
                    {
                        this.xmlDoc.Load(Path.Combine(this.GetAppPath(), this.GetSettingsFilename()));
                    }

                    // If the file does not exist on disk, catch the exception then create the XML template for the file.
                    catch (Exception)
                    {
                        // XML Declaration
                        // <?xml version="1.0" encoding="utf-8"?>
                        var dec = this.xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                        this.xmlDoc.AppendChild(dec);

                        // Create root node and append to the document
                        // <configuration>
                        var rootNode = this.xmlDoc.CreateElement(XMLROOT);
                        this.xmlDoc.AppendChild(rootNode);

                        // Create Configuration Sections node and add as the first node under the root
                        // <configSections>
                        var configNode = this.xmlDoc.CreateElement(CONFIGNODE);
                        this.xmlDoc.DocumentElement.PrependChild(configNode);

                        // Create the user settings section group declaration and append to the config node above
                        // <sectionGroup name="userSettings"...>
                        var groupNode = this.xmlDoc.CreateElement(GROUPNODE);
                        groupNode.SetAttribute("name", USERNODE);
                        groupNode.SetAttribute("type", "System.Configuration.UserSettingsGroup");
                        configNode.AppendChild(groupNode);

                        // Create the Application section declaration and append to the groupNode above
                        // <section name="AppName.Properties.Settings"...>
                        var newSection = this.xmlDoc.CreateElement("section");
                        newSection.SetAttribute("name", this.aPPNODE);
                        newSection.SetAttribute("type", "System.Configuration.ClientSettingsSection");
                        groupNode.AppendChild(newSection);

                        // Create the userSettings node and append to the root node
                        // <userSettings>
                        var userNode = this.xmlDoc.CreateElement(USERNODE);
                        this.xmlDoc.DocumentElement.AppendChild(userNode);

                        // Create the Application settings node and append to the userNode above
                        // <AppName.Properties.Settings>
                        var appNode = this.xmlDoc.CreateElement(this.aPPNODE);
                        userNode.AppendChild(appNode);
                    }
                }

                return this.xmlDoc;
            }
        }

        // Override the Initialize method
        public override void Initialize(string name, NameValueCollection config)
        {
            this.entryAssembly = Assembly.GetEntryAssembly();

            configName = this.entryAssembly.GetName().Name;

            // configDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), this.entryAssembly.GetName().Name);

            string executionPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            configDirectory = executionPath;

            /*if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }*/

            this.aPPNODE = configName + ".Properties.Settings";


            base.Initialize(this.ApplicationName, config);
        }

        // Simply returns the name of the settings file, which is the solution name plus ".config"
        public virtual string GetSettingsFilename()
        {
            return string.Format("{0}.config", this.ApplicationName);
        }

        // Gets current executable path in order to determine where to read and write the config file
        public virtual string GetAppPath()
        {
            return configDirectory;
            //return new FileInfo(callingAssembly.Location).DirectoryName;
        }

        // Override the ApplicationName property, returning the solution name.  No need to set anything, we just need to
        // retrieve information, though the set method still needs to be defined.


        // Retrieve settings from the configuration file
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sContext, SettingsPropertyCollection settingsColl)
        {
            // Create a collection of values to return
            var retValues = new SettingsPropertyValueCollection();

            // Create a temporary SettingsPropertyValue to reuse

            // Loop through the list of settings that the application has requested and add them
            // to our collection of return values.
            foreach (SettingsProperty sProp in settingsColl)
            {
                var setVal = new SettingsPropertyValue(sProp) { IsDirty = false, SerializedValue = this.GetSetting(sProp) };
                retValues.Add(setVal);
            }

            return retValues;
        }

        // Save any of the applications settings that have changed (flagged as "dirty")
        public override void SetPropertyValues(SettingsContext sContext, SettingsPropertyValueCollection settingsColl)
        {
            // Set the values in XML
            foreach (SettingsPropertyValue spVal in settingsColl)
            {
                this.SetSetting(spVal);
            }

            // Write the XML file to disk
            try
            {
                this.XMLConfig.Save(Path.Combine(this.GetAppPath(), this.GetSettingsFilename()));
            }
            catch (Exception ex)
            {
                // Create an informational message for the user if we cannot save the settings.
                // Enable whichever applies to your application type.

                // Uncomment the following line to enable a MessageBox for forms-based apps
                //MessageBox.Show(ex.Message, "Error writting configuration file to disk", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Uncomment the following line to enable a console message for console-based apps
                //Console.WriteLine("Error writing configuration file to disk: " + ex.Message);
            }
        }

        private object GetDefaultValue(SettingsProperty setProp)
        {
            object retVal;
            // Check to see if a default value is defined by the application.
            // If so, return that value, using the same rules for settings stored as Strings and XML as above
            if ((setProp.DefaultValue != null))
            {
                if (setProp.SerializeAs.ToString() == "String")
                {
                    retVal = setProp.DefaultValue.ToString();
                }
                else
                {
                    var settingType = setProp.PropertyType.ToString();
                    var xmlData = setProp.DefaultValue.ToString();
                    var xs = new XmlSerializer(typeof(string[]));
                    var data = (string[])xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));

                    switch (settingType)
                    {
                        case "System.Collections.Specialized.StringCollection":
                            var sc = new StringCollection();
                            sc.AddRange(data);
                            return sc;

                        default:
                            return string.Empty;
                    }
                }
            }
            else
            {
                retVal = string.Empty;
            }

            return retVal;
        }

        // Retrieve values from the configuration file, or if the setting does not exist in the file,
        // retrieve the value from the application's default configuration
        private object GetSetting(SettingsProperty setProp)
        {
            try
            {
                // Search for the specific settings node we are looking for in the configuration file.
                // If it exists, return the InnerText or InnerXML of its first child node, depending on the setting type.

                // If the setting is serialized as a string, return the text stored in the config
                if (setProp.SerializeAs.ToString() == "String")
                {
                    var node1 = this.XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']");

                    if (node1 != null)
                        return node1.FirstChild.InnerText;
                    else
                        return GetDefaultValue(setProp);
                }

                // This solves the problem with StringCollections throwing a NullReferenceException
                var node2 = this.XMLConfig.SelectSingleNode(string.Format("//setting[@name='{0}']", setProp.Name));
                if (node2 != null)
                {
                    var xmlData = node2.FirstChild.InnerXml;
                    return string.Format(@"{0}", xmlData);
                }
                else
                    return GetDefaultValue(setProp);
            }
            catch (Exception)
            {
                return GetDefaultValue(setProp);
            }
        }

        private void SetSetting(SettingsPropertyValue setProp)
        {
            // Define the XML path under which we want to write our settings if they do not already exist
            XmlNode settingNode = null;

            try
            {
                // Search for the specific settings node we want to update.
                // If it exists, return its first child node, (the <value>data here</value> node)
                var node = this.XMLConfig.SelectSingleNode(string.Format("//setting[@name='{0}']", setProp.Name));
                if (node != null)
                    settingNode = node.FirstChild;
            }
            catch (Exception)
            {
                settingNode = null;
            }

            // If we have a pointer to an actual XML node, update the value stored there
            if ((settingNode != null))
            {
                if (setProp.Property.SerializeAs.ToString() == "String" && setProp.SerializedValue != null)
                {
                    settingNode.InnerText = setProp.SerializedValue.ToString();
                }
                else
                {
                    // Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
                    // the value, otherwise .Net's configuration system complains about the additional declaration.
                    if (setProp.SerializedValue != null)
                        settingNode.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", string.Empty);
                }
            }
            else
            {
                // If the value did not already exist in this settings file, create a new entry for this setting

                // Search for the application settings node (<Appname.Properties.Settings>) and store it.
                var tmpNode = this.XMLConfig.SelectSingleNode(string.Format("//{0}", this.aPPNODE)) ?? this.XMLConfig.SelectSingleNode(string.Format("//{0}.Properties.Settings", this.aPPNODE));

                // Create a new settings node and assign its name as well as how it will be serialized
                var newSetting = this.xmlDoc.CreateElement("setting");
                newSetting.SetAttribute("name", setProp.Name);
                newSetting.SetAttribute("serializeAs", setProp.Property.SerializeAs.ToString() == "String" ? "String" : "Xml");

                // Append this node to the application settings node (<Appname.Properties.Settings>)
                tmpNode.AppendChild(newSetting);

                // Create an element under our named settings node, and assign it the value we are trying to save
                var valueElement = this.xmlDoc.CreateElement("value");
                if (setProp.Property.SerializeAs.ToString() == "String" && setProp.SerializedValue != null)
                {
                    valueElement.InnerText = setProp.SerializedValue.ToString();
                }
                else
                {
                    // Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
                    // the value, otherwise .Net's configuration system complains about the additional declaration
                    if (setProp.SerializedValue != null)
                        valueElement.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", string.Empty);
                }

                //Append this new element under the setting node we created above
                newSetting.AppendChild(valueElement);
            }
        }
    }
}