namespace TemplateEngine.Docx.Processors
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TemplateEngine.Docx.Errors;

    internal class ProcessResult
    {
        protected ProcessResult(bool handled = true)
        {
            this.errors = new List<IError>();
            this.HandledItems = new Collection<IContentItem>();
            this.Handled = handled;
        }

        public static ProcessResult SuccessResult => new ProcessResult
        {
            Handled = true,
        };

        public static ProcessResult ErrorResult(IEnumerable<IError> errors)
        {
            return new ProcessResult
            {
                Handled = true,
                errors = errors.ToList(),
            };
        }

        public static ProcessResult NotHandledResult => new ProcessResult
        {
            Handled = false,
        };

        private List<IError> errors;

        public ReadOnlyCollection<IError> Errors => new ReadOnlyCollection<IError>(this.errors);

        public bool Success => this.Handled && !this.Errors.Any();

        public bool Handled { get; private set; }

        public ICollection<IContentItem> HandledItems { get; private set; }

        public ProcessResult AddItemToHandled(IContentItem handledItem)
        {
            if (!this.HandledItems.Contains(handledItem))
            {
                this.HandledItems.Add(handledItem);
            }

            var contentControlNotFoundErrors = this.Errors.OfType<ContentControlNotFoundError>()
                .Where(x => x.ContentItem.Equals(handledItem))
                .ToList();

            foreach (var error in contentControlNotFoundErrors)
            {
                this.errors.Remove(error);
            }

            this.Handled = true;

            return this;
        }

        public ProcessResult AddError(IError error)
        {
            if (this.errors.Contains(error))
            {
                return this;
            }

            var foundError = error as ContentControlNotFoundError;
            if (foundError != null)
            {
                if (this.HandledItems.Contains(foundError.ContentItem))
                {
                    return this;
                }
            }

            this.errors.Add(error);
            return this;
        }

        public ProcessResult Merge(ProcessResult another)
        {
            if (another == null)
            {
                return this;
            }

            if (!another.Success)
            {
                foreach (var error in another.Errors)
                {
                    this.AddError(error);
                }
            }

            foreach (var handledItem in another.HandledItems)
            {
                this.AddItemToHandled(handledItem);
            }

            this.Handled = this.Handled || another.Handled;

            return this;
        }
    }
}
