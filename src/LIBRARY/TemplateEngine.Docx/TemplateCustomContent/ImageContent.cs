namespace TemplateEngine.Docx
{
    using System;
    using System.Linq;

    [ContentItemName("Image")]
    public class ImageContent : HiddenContent<ImageContent>, IEquatable<ImageContent>
    {
        public ImageContent()
        {
        }

        public ImageContent(string name, byte[] binary)
        {
            this.Name = name;
            this.Binary = binary;
        }

        public byte[] Binary { get; set; }

        #region Equals

        public bool Equals(ImageContent other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Name.Equals(other.Name, StringComparison.InvariantCultureIgnoreCase) &&
                   this.Binary.SequenceEqual(other.Binary);
        }

        public override bool Equals(IContentItem other)
        {
            if (!(other is ImageContent))
            {
                return false;
            }

            return this.Equals((ImageContent)other);
        }

        public override int GetHashCode()
        {
            return new { this.Name, this.Binary }.GetHashCode();
        }

        #endregion
    }
}
