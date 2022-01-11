namespace TemplateEngine.Docx
{
    using System.Collections.Generic;
    using System.IO;
    using DocumentFormat.OpenXml.Packaging;

    internal class HeaderWordDocumentContainer : NestedWordDocumentContainer
    {
        internal HeaderWordDocumentContainer(string identifier, WordprocessingDocument document)
            : base(identifier, document)
        {
            this.ImagesPart = this.GetImagesPart();
        }

        public override string AddImagePart(byte[] bytes)
        {
            if (this.document == null)
            {
                return null;
            }

            var imagePart = (this.document as HeaderPart)?.AddImagePart(ImagePartType.Jpeg);
            if (imagePart == null)
            {
                return string.Empty;
            }

            using (var stream = new MemoryStream(bytes))
            {
                imagePart.FeedData(stream);
            }

            return this.document.GetIdOfPart(imagePart);
        }

        private IEnumerable<ImagePart> GetImagesPart()
        {
            return ((HeaderPart)this.document).ImageParts;
        }
    }
}
