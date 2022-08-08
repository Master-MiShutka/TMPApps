using System;
using System.ComponentModel;

namespace TMP.Work.AmperM.TestApp.EzSbyt
{
    using Shared;
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FormatTypes
    {
        [Description("text")]
        text,
        xml,
        json,
        csv,
        native,
        webview
    }
}
