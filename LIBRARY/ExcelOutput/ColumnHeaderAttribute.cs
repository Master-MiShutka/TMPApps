namespace TMP.ExcelOutput
{
    using System;

    /// <summary>
    /// An attribute that can be applied to a field or property which sets the column header in generated CSV or Excel export.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnHeaderAttribute : Attribute
    {
        public ColumnHeaderAttribute(string header)
        {
            this.Header = header;
        }

        public string Header { get; }
    }
}
