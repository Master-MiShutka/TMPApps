namespace TemplateEngine.Docx
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using DocumentFormat.OpenXml.Packaging;

    internal abstract class NestedWordDocumentContainer : IDisposable, IDocumentContainer
    {
        internal string Identifier { get; private set; }

        private readonly WordprocessingDocument mainWordDocument;
        protected readonly OpenXmlPart document;

        public XDocument MainDocumentPart { get; private set; }

        public XDocument NumberingPart { get; private set; }

        public XDocument StylesPart { get; private set; }

        public OpenXmlPart GetPartById(string partIdentifier)
        {
            return this.document.GetPartById(partIdentifier);
        }

        public void RemovePartById(string partIdentifier)
        {
            this.document.DeletePart(this.GetPartById(partIdentifier));
        }

        public abstract string AddImagePart(byte[] bytes);

        public IEnumerable<ImagePart> ImagesPart { get; protected set; }

        protected NestedWordDocumentContainer(string identifier, WordprocessingDocument document)
        {
            this.Identifier = identifier;
            this.mainWordDocument = document;
            this.document = this.GetPart();

            this.MainDocumentPart = this.LoadPart(this.document);
            this.NumberingPart = this.LoadPart(document.MainDocumentPart.NumberingDefinitionsPart);
            this.StylesPart = this.LoadPart(document.MainDocumentPart.StyleDefinitionsPart);
        }

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

        internal OpenXmlPart GetPart()
        {
            if (this.mainWordDocument == null)
            {
                return null;
            }

            try
            {
                return this.mainWordDocument.MainDocumentPart.GetPartById(this.Identifier);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        internal void Save()
        {
            using (var xw = XmlWriter.Create(this.GetPart().GetStream(FileMode.Create, FileAccess.Write)))
            {
                this.MainDocumentPart.Save(xw);
            }
        }

        #region IDisposable
        public void Dispose()
        {
        }

        #endregion

    }
}
