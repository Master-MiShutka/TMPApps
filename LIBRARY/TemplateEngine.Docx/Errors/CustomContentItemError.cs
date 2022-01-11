namespace TemplateEngine.Docx.Errors
{
    using System;

    internal class CustomContentItemError : IError, IEquatable<CustomContentItemError>
    {
        private const string ErrorMessageTemplate =
                    "{0} Content Control '{1}' {2}.";

        internal CustomContentItemError(IContentItem contentItem, string customMessage)
        {
            this.ContentItem = contentItem;
            this.customMessage = customMessage;
        }

        private readonly string customMessage;

        public string Message => string.Format(ErrorMessageTemplate,
                    this.ContentItem.GetContentItemName(),
                    this.ContentItem.Name, this.customMessage);

        public IContentItem ContentItem { get; private set; }

        #region Equals
        public bool Equals(CustomContentItemError other)
        {
            if (other == null)
            {
                return false;
            }

            return other.ContentItem.Equals(this.ContentItem) && other.Message.Equals(this.Message);
        }

        public bool Equals(IError other)
        {
            if (!(other is CustomContentItemError))
            {
                return false;
            }

            return this.Equals((CustomContentItemError)other);
        }

        public override int GetHashCode()
        {
            var customItemHash = this.ContentItem.GetHashCode();

            return new { customItemHash, this.customMessage }.GetHashCode();
        }
        #endregion
    }
}
