namespace TemplateEngine.Docx.Processors
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using DocumentFormat.OpenXml.Wordprocessing;
    using TemplateEngine.Docx.Errors;

    internal class FieldsProcessor : IProcessor
    {
        private bool isNeedToRemoveContentControls;

        public IProcessor SetRemoveContentControls(bool isNeedToRemove)
        {
            this.isNeedToRemoveContentControls = isNeedToRemove;
            return this;
        }

        public ProcessResult FillContent(XElement contentControl, IEnumerable<IContentItem> items)
        {
            var processResult = ProcessResult.NotHandledResult;

            foreach (var contentItem in items)
            {
                processResult.Merge(this.FillContent(contentControl, contentItem));
            }

            if (processResult.Success && this.isNeedToRemoveContentControls)
            {
                contentControl.RemoveContentControl();
            }

            return processResult;
        }

        public ProcessResult FillContent(XElement contentControl, IContentItem item)
        {
            var processResult = ProcessResult.NotHandledResult;
            if (!(item is FieldContent))
            {
                processResult = ProcessResult.NotHandledResult;
                return processResult;
            }

            var field = item as FieldContent;

            // If there isn't a field with that name, add an error to the error string,
            // and continue with next field.
            if (contentControl == null)
            {
                processResult.AddError(new ContentControlNotFoundError(field));
                return processResult;
            }

            var newValue = field.IsHidden ? string.Empty : field.Value;
            contentControl.ReplaceContentControlWithNewValue(newValue);

            processResult.AddItemToHandled(item);

            return processResult;
        }
    }
}
