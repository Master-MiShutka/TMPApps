namespace TemplateEngine.Docx
{
    using System;

    public abstract class HiddenContent<TBuilder> : IContentItem
        where TBuilder : HiddenContent<TBuilder>
    {
        protected HiddenContent()
        {
            this.instance = (TBuilder)this;
        }

        private readonly TBuilder instance;

        public TBuilder Hide()
        {
            this.IsHidden = true;
            return this.instance;
        }

        public TBuilder Hide(Func<TBuilder, bool> predicate)
        {
            if (predicate(this.instance))
            {
                this.IsHidden = true;
            }

            return this.instance;
        }

        public abstract bool Equals(IContentItem other);

        public string Name { get; set; }

        public bool IsHidden { get; set; }
    }
}
