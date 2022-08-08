using System.ComponentModel;

namespace TMP.Wpf.CommonControls.ItemsFilter.Model {
    
    /// <summary>
    /// String filter compare mode.
    /// </summary>
    public enum StringFilterMode {
        [Description("начинается с")]
        StartsWith,
        [Description("заканчивается на")]
        EndsWith,
        [Description("содержит")]
        Contains,
        [Description("равно")]
        Equals
    }
}
