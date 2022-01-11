namespace TemplateEngine.Docx.Errors
{
    using System;

    internal class ContentControlNotFoundError : IError, IEquatable<ContentControlNotFoundError>, IEquatable<IError>
    {
        private const string ErrorMessageTemplate =
                    "{0} Content Control '{1}' not found.";

        internal ContentControlNotFoundError(IContentItem contentItem)
        {
            this.ContentItem = contentItem;
        }

        public string Message => string.Format(ErrorMessageTemplate, this.ContentItem.GetContentItemName(), this.ContentItem.Name);

        public IContentItem ContentItem { get; private set; }

        #region Equals
        public bool Equals(ContentControlNotFoundError other)
        {
            if (other == null)
            {
                return false;
            }

            return other.ContentItem.Equals(this.ContentItem);
        }

        public bool Equals(IError other)
        {
            if (!(other is ContentControlNotFoundError))
            {
                return false;
            }

            return this.Equals((ContentControlNotFoundError)other);
        }

        public override int GetHashCode()
        {
            return this.ContentItem.GetHashCode();
        }
        #endregion
    }
}
