namespace TMP.ExcelXml
{
    using System;
    using TMP.Extensions;

    internal class NamedRange
    {
        #region Private and Internal fields
        internal Range Range;
        internal Worksheet Worksheet;
        internal string Name;
        #endregion

        #region Constructor
        internal NamedRange(Range range, string name, Worksheet ws)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            if (name.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Worksheet = ws;
            this.Range = range;
            this.Name = name;
        }
        #endregion
    }
}
