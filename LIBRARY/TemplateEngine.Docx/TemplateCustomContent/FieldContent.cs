namespace TemplateEngine.Docx
{
    using System;

    [ContentItemName("Field")]
    public class FieldContent : HiddenContent<FieldContent>, IEquatable<FieldContent>
    {
        public FieldContent()
        {
        }

        public FieldContent(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Value { get; set; }

        #region Equals

        public bool Equals(FieldContent other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Name.Equals(other.Name) &&
                   this.Value.Equals(other.Value);
        }

        public override bool Equals(IContentItem other)
        {
            if (!(other is FieldContent))
            {
                return false;
            }

            return this.Equals((FieldContent)other);
        }

        public override int GetHashCode()
        {
            return new { this.Name, this.Value }.GetHashCode();
        }

        #endregion
    }
}
