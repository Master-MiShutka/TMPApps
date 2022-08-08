namespace TMP.Work.DocxReportGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using TemplateEngine.Docx;

    public class FieldContentEx
    {
        public FieldContentEx(FieldContentEx parent, IContentItem contentItem)
        {
            this.Parent = parent;
            this.AssociatedField = contentItem;
        }

        public bool IsEnabled { get; set; } = false;

        public FieldTypes FieldType { get; set; } = FieldTypes.Content;

        public IContentItem AssociatedField { get; set; }

        public FieldContentEx Parent { get; set; }

        public FieldTypes ParentFieldType => this.Parent == null ? FieldTypes.Content : this.Parent.FieldType;

        public IList<FieldContentEx> Children { get; set; }

        public bool HasChildren => this.Children != null && this.Children.Count > 0;
    }

    /// <summary>
    ///
    /// </summary>
    public enum FieldTypes
    {
        Content, List, Table, Image,
    }
}
