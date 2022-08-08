namespace TemplateEngine.Docx
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using DocumentFormat.OpenXml.Packaging;

    internal class WordDocumentContainer : IDisposable, IDocumentContainer
    {
        private readonly WordprocessingDocument wordDocument;

        public XDocument MainDocumentPart { get; private set; }

        public XDocument NumberingPart { get; private set; }

        public XDocument StylesPart { get; private set; }

        internal List<NestedWordDocumentContainer> HeaderParts { get; private set; }

        internal List<NestedWordDocumentContainer> FooterParts { get; private set; }

        internal IEnumerable<ImagePart> ImagesPart { get; private set; }

        internal bool HasHeaders => this.HeaderParts != null && this.HeaderParts.Any();

        internal bool HasFooters => this.FooterParts != null && this.FooterParts.Any();

        internal WordDocumentContainer(WordprocessingDocument wordDocument)
        {
            this.wordDocument = wordDocument;

            this.MainDocumentPart = this.LoadPart(this.wordDocument.MainDocumentPart);
            this.NumberingPart = this.LoadPart(this.wordDocument.MainDocumentPart.NumberingDefinitionsPart);
            this.StylesPart = this.LoadPart(this.wordDocument.MainDocumentPart.StyleDefinitionsPart);

            this.ImagesPart = this.wordDocument.MainDocumentPart.ImageParts;

            this.HeaderParts = this.LoadHeaders(this.wordDocument.MainDocumentPart.HeaderParts);
            this.FooterParts = this.LoadFooters(this.wordDocument.MainDocumentPart.FooterParts);
        }

        internal WordDocumentContainer(XDocument templateSource, XDocument stylesPart = null, XDocument numberingPart = null, IEnumerable<ImagePart> imagesPart = null)
        {
            this.MainDocumentPart = templateSource;
            this.NumberingPart = numberingPart;
            this.StylesPart = stylesPart;
            this.ImagesPart = imagesPart;
        }

        public OpenXmlPart GetPartById(string partIdentifier)
        {
            if (this.wordDocument == null)
            {
                return null;
            }

            try
            {
                return this.wordDocument.MainDocumentPart.GetPartById(partIdentifier);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        public void RemovePartById(string partIdentifier)
        {
            if (this.wordDocument == null)
            {
                return;
            }

            var part = this.GetPartById(partIdentifier);
            if (part != null)
            {
                this.wordDocument.MainDocumentPart.DeletePart(part);
            }
        }

        public string AddImagePart(byte[] bytes)
        {
            if (this.wordDocument == null)
            {
                return null;
            }

            var imagePart = this.wordDocument.MainDocumentPart.AddImagePart(ImagePartType.Jpeg);

            using (var stream = new MemoryStream(bytes))
            {
                imagePart.FeedData(stream);
            }

            return this.wordDocument.MainDocumentPart.GetIdOfPart(imagePart);
        }

        internal void SaveChanges()
        {
            if (this.MainDocumentPart == null)
            {
                return;
            }

            // Serialize the XDocument object back to the package.
            using (var xw = XmlWriter.Create(this.wordDocument.MainDocumentPart.GetStream(FileMode.Create, FileAccess.Write)))
            {
                this.MainDocumentPart.Save(xw);
            }

            if (this.NumberingPart != null)
            {
                // Serialize the XDocument object back to the package.
                using (var xw = XmlWriter.Create(this.wordDocument.MainDocumentPart.NumberingDefinitionsPart.GetStream(FileMode.Create,
                            FileAccess.Write)))
                {
                    this.NumberingPart.Save(xw);
                }
            }

            foreach (var footer in this.FooterParts)
            {
                footer.Save();
            }

            foreach (var header in this.HeaderParts)
            {
                header.Save();
            }

            this.wordDocument.Close();
        }

        #region IDisposable
        public void Dispose()
        {
            if (this.wordDocument != null)
            {
                this.wordDocument.Dispose();
            }
        }

        #endregion

        private XDocument LoadPart(OpenXmlPart source)
        {
            if (source == null)
            {
                return null;
            }

            var part = source.Annotation<XDocument>();
            if (part != null)
            {
                return part;
            }

            using (var str = source.GetStream())
            using (var streamReader = new StreamReader(str))
            using (var xr = XmlReader.Create(streamReader))
            {
                part = XDocument.Load(xr);
            }

            return part;
        }

        private List<NestedWordDocumentContainer> LoadHeaders(IEnumerable<OpenXmlPart> partsList)
        {
            return partsList
                .Select(part =>
                    new HeaderWordDocumentContainer(
                        this.wordDocument.MainDocumentPart.GetIdOfPart(part),
                        this.wordDocument))
                .Cast<NestedWordDocumentContainer>()
                .ToList();
        }

        private List<NestedWordDocumentContainer> LoadFooters(IEnumerable<OpenXmlPart> partsList)
        {
            return partsList
                .Select(part =>
                    new FooterWordDocumentContainer(
                        this.wordDocument.MainDocumentPart.GetIdOfPart(part),
                        this.wordDocument))
                .Cast<NestedWordDocumentContainer>()
                .ToList();
        }
    }
}
