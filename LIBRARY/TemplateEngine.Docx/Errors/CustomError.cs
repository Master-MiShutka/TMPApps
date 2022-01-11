namespace TemplateEngine.Docx.Errors
{
    using System;

    internal class CustomError : IError, IEquatable<CustomError>
    {
        internal CustomError(string customMessage)
        {
            this.customMessage = customMessage;
        }

        private readonly string customMessage;

        public string Message => this.customMessage;

        #region Equals
        public bool Equals(IError other)
        {
            if (!(other is CustomError))
            {
                return false;
            }

            return this.Equals((CustomError)other);
        }

        public bool Equals(CustomError other)
        {
            return this.Message.Equals(other.Message);
        }

        public override int GetHashCode()
        {
            return this.Message.GetHashCode();
        }
        #endregion
    }
}
