using System;

namespace WindowWithDialogs
{
    [System.Xml.Serialization.XmlRoot("UITheme")]
    [Serializable]
    public class VisualTheme
    {
        public string ShortName { get; set; }

        public string FullName { get; set; }
    }
}
