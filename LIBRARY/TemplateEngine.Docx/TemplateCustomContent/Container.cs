namespace TemplateEngine.Docx
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    [JsonObject]
    public abstract class Container : IEnumerable<IContentItem>, IEquatable<Container>
    {
        protected Container()
        {

            this.Repeats = new List<RepeatContent>();
            this.Lists = new List<ListContent>();
            this.Tables = new List<TableContent>();
            this.Fields = new List<FieldContent>();
            this.Images = new List<ImageContent>();
        }

        protected Container(params IContentItem[] contentItems)
        {
            if (contentItems != null)
            {
                this.Repeats = contentItems.OfType<RepeatContent>().ToList();
                this.Lists = contentItems.OfType<ListContent>().ToList();
                this.Tables = contentItems.OfType<TableContent>().ToList();
                this.Fields = contentItems.OfType<FieldContent>().ToList();
                this.Images = contentItems.OfType<ImageContent>().ToList();
            }
        }

        protected IEnumerable<IContentItem> All
        {
            get
            {
                var result = new List<IContentItem>();

                if (this.Repeats != null)
                {
                    result = result.Concat(this.Repeats).ToList();
                }

                if (this.Tables != null)
                {
                    result = result.Concat(this.Tables).ToList();
                }

                if (this.Lists != null)
                {
                    result = result.Concat(this.Lists).ToList();
                }

                if (this.Fields != null)
                {
                    result = result.Concat(this.Fields).ToList();
                }

                if (this.Images != null)
                {
                    result = result.Concat(this.Images).ToList();
                }

                return result;
            }
        }

        public ICollection<RepeatContent> Repeats { get; set; }

        public ICollection<TableContent> Tables { get; set; }

        public ICollection<ListContent> Lists { get; set; }

        public ICollection<FieldContent> Fields { get; set; }

        public ICollection<ImageContent> Images { get; set; }

        public IContentItem GetContentItem(string name)
        {
            return this.All.FirstOrDefault(t => t.Name == name);
        }

        [JsonIgnore]
        public IEnumerable<string> FieldNames
        {
            get
            {
                var repeatsFieldNames = this.Repeats == null
                    ? new List<string>()
                    : this.Repeats.Select(t => t.Name)
                        .Concat(this.Repeats.SelectMany(t => t.Items.SelectMany(r => r.FieldNames)));

                var tablesFieldNames = this.Tables == null
                    ? new List<string>()
                    : this.Tables.Select(t => t.Name)
                        .Concat(this.Tables.SelectMany(t => t.Rows.SelectMany(r => r.FieldNames)));

                var listsFieldNames = this.Lists == null
                            ? new List<string>()
                            : this.Lists.Select(l => l.Name)
                                .Concat(this.Lists.SelectMany(l => l.FieldNames));

                var imagesFieldNames = this.Images == null
                    ? new List<string>()
                    : this.Images.Select(f => f.Name);

                var fieldNames = this.Fields == null ? new List<string>() : this.Fields.Select(f => f.Name);

                return repeatsFieldNames
                    .Concat(tablesFieldNames)
                    .Concat(listsFieldNames)
                    .Concat(imagesFieldNames)
                    .Concat(fieldNames);
            }
        }

        #region Fluent
        protected Container AddField(string name, string value)
        {
            if (this.Fields == null)
            {
                this.Fields = new List<FieldContent>();
            }

            this.Fields.Add(new FieldContent(name, value));
            return this;
        }

        protected Container AddTable(TableContent table)
        {
            if (this.Tables == null)
            {
                this.Tables = new List<TableContent>();
            }

            this.Tables.Add(table);
            return this;
        }

        protected Container AddList(ListContent list)
        {
            if (this.Lists == null)
            {
                this.Lists = new List<ListContent>();
            }

            this.Lists.Add(list);
            return this;
        }

        protected Container AddImage(ImageContent image)
        {
            if (this.Images == null)
            {
                this.Images = new List<ImageContent>();
            }

            this.Images.Add(image);
            return this;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<IContentItem> GetEnumerator()
        {
            return this.All.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region Equals
        public bool Equals(Container other)
        {
            if (other == null)
            {
                return false;
            }

            return this.All.SequenceEqual(other);
        }

        public override int GetHashCode()
        {
            var hc = 0;

            hc = this.All.Aggregate(hc, (current, p) => current ^ p.GetHashCode());

            return hc;
        }
        #endregion
    }
}
