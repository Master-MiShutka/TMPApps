namespace TMP.ExcelXml
{
    using System;
    using System.Globalization;
    using System.Xml;
    using TMP.Extensions;

    /// <summary>
    /// Column class represents a column properties of a single column in a worksheet
    /// </summary>
    /// <remarks>
    /// Column class represents a column properties of a single column in a worksheet.
    /// <para>You cannot directly declare a instance of a column class from your code by using
    /// <c>new</c> keyword. The only way to access a column is to retrieve it from
    /// a worksheet by using the <see cref="TMP.ExcelXml.Worksheet.Columns"/>
    /// method of the <see cref="TMP.ExcelXml.Worksheet"/> class.</para>
    /// </remarks>
    public class Column
    {
        #region Private and Internal fields
        private ExcelXmlWorkbook ParentBook;
        private string styleID;
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the default width of the column
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets the hidden status of the column
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TMP.ExcelXml.XmlStyle"/> reference of the column.
        /// <para>Setting this option only affects cells which are added after this value is set. The
        /// cells which are added in the same column retain their original style settings.</para>
        /// </summary>
        public XmlStyle Style
        {
            get => this.ParentBook.GetStyleByID(this.styleID);

            set => this.styleID = this.ParentBook.AddStyle(value);
        }
        #endregion

        #region Constructor
        internal Column(Worksheet parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this.ParentBook = parent.ParentBook;
        }
        #endregion

        #region Export
        internal void Export(XmlWriter writer)
        {
            writer.WriteStartElement("Column");

            if (this.Width > 0)
            {
                writer.WriteAttributeString("ss", "Width", null, this.Width.ToString(
                    CultureInfo.InvariantCulture));
            }

            if (this.Hidden)
            {
                writer.WriteAttributeString("ss", "Hidden", null, "1");
                writer.WriteAttributeString("ss", "AutoFitWidth", null, "0");
            }

            // Has style? If yes, we only need to write the style if default line
            // style is not same as this one...
            if (!this.Style.ID.IsNullOrEmpty() && this.Style.ID != "Default")
            {
                writer.WriteAttributeString("ss", "StyleID", null, this.Style.ID);
            }

            writer.WriteEndElement();
        }
        #endregion
    }
}