namespace TemplateEngine.Docx
{
    using System.Collections.Generic;
    using System.IO;
    using DocumentFormat.OpenXml.Packaging;

    internal class FooterWordDocumentContainer : NestedWordDocumentContainer
    {
        internal FooterWordDocumentContainer(string identifier, WordprocessingDocument document)
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

            var imagePart = ((FooterPart)this.document).AddImagePart(ImagePartType.Jpeg);

            using (var stream = new MemoryStream(bytes))
            {
                imagePart.FeedData(stream);
            }

            return this.document.GetIdOfPart(imagePart);
        }

        private IEnumerable<ImagePart> GetImagesPart()
        {
            return ((FooterPart)this.document).ImageParts;
        }
    }
}
