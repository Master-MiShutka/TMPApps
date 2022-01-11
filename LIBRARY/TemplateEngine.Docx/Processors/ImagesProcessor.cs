namespace TemplateEngine.Docx.Processors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using TemplateEngine.Docx.Errors;

    internal class ImagesProcessor : IProcessor
    {
        private readonly ProcessContext context;

        public ImagesProcessor(ProcessContext context)
        {
            this.context = context;
        }

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

            if (!(item is ImageContent))
            {
                processResult = ProcessResult.NotHandledResult;
                return processResult;
            }

            var field = item as ImageContent;

            // If there isn't a field with that name, add an error to the error string,
            // and continue with next field.
            if (contentControl == null)
            {
                processResult.AddError(new ContentControlNotFoundError(field));
                return processResult;
            }

            if (item.IsHidden)
            {
                var graphic = contentControl.DescendantsAndSelf(W.drawing).First();
                graphic.Remove();
            }
            else
            {
                var blip = contentControl.DescendantsAndSelf(A.blip).First();
                if (blip == null)
                {
                    processResult.AddError(new CustomContentItemError(field, "doesn't contain an image for replace"));
                    return processResult;
                }

                var imageId = blip.Attribute(R.embed).Value;

                var imagePart = (ImagePart)this.context.Document.GetPartById(imageId);

                if (imagePart != null)
                {
                    this.context.Document.RemovePartById(imageId);
                }

                var imagePartId = this.context.Document.AddImagePart(field.Binary);

                blip.Attribute(R.embed).SetValue(imagePartId);
            }

            processResult.AddItemToHandled(item);
            return processResult;
        }
    }
}
