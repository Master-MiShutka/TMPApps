namespace TemplateEngine.Docx
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    [ContentItemName("Repeat")]
    public class RepeatContent : HiddenContent<RepeatContent>, IEquatable<RepeatContent>
    {
        #region properties

        public ICollection<Content> Items { get; set; }

        public IEnumerable<string> FieldNames => this.Items?.SelectMany(r => r.FieldNames).Distinct().ToList() ?? new List<string>();

        #endregion properties

        #region ctors

        public RepeatContent()
        {
        }

        public RepeatContent(string name)
        {
            this.Name = name;
        }

        public RepeatContent(string name, IEnumerable<Content> items)
            : this(name)
        {
            this.Items = items.ToList();
        }

        public RepeatContent(string name, params Content[] items)
            : this(name)
        {
            this.Items = items.ToList();
        }

        #endregion

        #region Fluent

        public static RepeatContent Create(string name, params Content[] items)
        {
            return new RepeatContent(name, items);
        }

        public static RepeatContent Create(string name, IEnumerable<Content> items)
        {
            return new RepeatContent(name, items);
        }

        public RepeatContent AddItem(Content item)
        {
            if (this.Items == null)
            {
                this.Items = new Collection<Content>();
            }

            this.Items.Add(item);
            return this;
        }

        public RepeatContent AddItem(params IContentItem[] contentItems)
        {
            if (this.Items == null)
            {
                this.Items = new Collection<Content>();
            }

            this.Items.Add(new Content(contentItems));
            return this;
        }

        #endregion

        #region Equals

        public bool Equals(RepeatContent other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Name.Equals(other.Name) &&
                   this.Items.SequenceEqual(other.Items);
        }

        public override bool Equals(IContentItem other)
        {
            if (!(other is RepeatContent))
            {
                return false;
            }

            return this.Equals((RepeatContent)other);
        }

        public override int GetHashCode()
        {
            var hc = 0;
            if (this.Items != null)
            {
                hc = this.Items.Aggregate(hc, (current, p) => current ^ p.GetHashCode());
            }

            return new { this.Name, hc }.GetHashCode();
        }

        #endregion
    }
}
