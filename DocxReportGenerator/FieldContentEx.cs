using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TemplateEngine.Docx;

namespace TMP.Work.DocxReportGenerator
{
    public class FieldContentEx
    {
        public FieldContentEx(FieldContentEx parent, IContentItem contentItem)
        {
            Parent = parent;
            AssociatedField = contentItem;
        }

        public bool IsEnabled { get; set; } = false;

        public FieldTypes FieldType { get; set; } = FieldTypes.Content;

        public IContentItem AssociatedField { get; set; }

        public FieldContentEx Parent { get; set; }

        public FieldTypes ParentFieldType => Parent == null ? FieldTypes.Content : Parent.FieldType;

        public IList<FieldContentEx> Children { get; set; }

        public bool HasChildren => Children != null && Children.Count > 0;

    }
    /// <summary>
    /// 
    /// </summary>
    public enum FieldTypes
    {
        Content, List, Table, Image
    }
}
