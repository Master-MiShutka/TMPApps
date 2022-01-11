namespace TemplateEngine.Docx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [ContentItemName("Table")]
    public class TableContent : HiddenContent<TableContent>, IContentItem, IEquatable<TableContent>
    {
        public ICollection<TableRowContent> Rows { get; set; }

        public IEnumerable<string> FieldNames => this.Rows?.SelectMany(r => r.FieldNames).Distinct().ToList() ?? new List<string>();

        #region ctors

        public TableContent()
        {
        }

        public TableContent(string name)
        {
            this.Name = name;
        }

        public TableContent(string name, IEnumerable<TableRowContent> rows)
            : this(name)
        {
            this.Rows = rows.ToList();
        }

        public TableContent(string name, params TableRowContent[] rows)
            : this(name)
        {
            this.Rows = rows.ToList();
        }

        #endregion

        #region Fluent

        public static TableContent Create(string name, params TableRowContent[] rows)
        {
            return new TableContent(name, rows);
        }

        public static TableContent Create(string name, List<TableRowContent> rows)
        {
            return new TableContent(name, rows);
        }

        public TableContent AddRow(params IContentItem[] contentItems)
        {
            if (this.Rows == null)
            {
                this.Rows = new List<TableRowContent>();
            }

            this.Rows.Add(new TableRowContent(contentItems));
            return this;
        }

        #endregion

        #region Equals

        public bool Equals(TableContent other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Name.Equals(other.Name) &&
               this.Rows.SequenceEqual(other.Rows);
        }

        public override bool Equals(IContentItem other)
        {
            if (!(other is TableContent))
            {
                return false;
            }

            return this.Equals((TableContent)other);
        }

        public override int GetHashCode()
        {
            var hc = 0;
            if (this.Rows != null)
            {
                hc = this.Rows.Aggregate(hc, (current, p) => current ^ p.GetHashCode());
            }

            return new { this.Name, hc }.GetHashCode();
        }

        #endregion
    }
}
