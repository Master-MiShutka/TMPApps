namespace ItemsFilter.Model
{
    using System.ComponentModel;

    /// <summary>
    /// String filter compare mode.
    /// </summary>
    public enum StringFilterMode
    {
        [Description("начинается с")]
        StartsWith,
        [Description("заканчивается на")]
        EndsWith,
        [Description("содержит")]
        Contains,
        [Description("равно")]
        Equals,
    }
}
