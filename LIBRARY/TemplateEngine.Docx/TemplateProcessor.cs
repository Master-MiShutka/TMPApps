namespace TemplateEngine.Docx
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using DocumentFormat.OpenXml.Packaging;
    using TemplateEngine.Docx.Errors;
    using TemplateEngine.Docx.Processors;

    public class TemplateProcessor : IDisposable
    {
        private readonly WordDocumentContainer wordDocument;
        private bool isNeedToRemoveContentControls;
        private bool _isNeedToNoticeAboutErrors;

        public XDocument Document => this.wordDocument.MainDocumentPart;

        public XDocument NumberingPart => this.wordDocument.NumberingPart;

        public XDocument StylesPart => this.wordDocument.StylesPart;

        public IEnumerable<ImagePart> ImagesPart => this.wordDocument.ImagesPart;

        public Dictionary<string, XDocument> HeaderParts => this.wordDocument.HeaderParts
                    .Select(x => new { x.Identifier, x.MainDocumentPart })
                    .ToDictionary(x => x.Identifier, y => y.MainDocumentPart);

        public Dictionary<string, IEnumerable<ImagePart>> HeaderImagesParts => this.wordDocument.HeaderParts
                    .Select(x => new { x.Identifier, x.ImagesPart })
                    .ToDictionary(x => x.Identifier, y => y.ImagesPart);

        public Dictionary<string, XDocument> FooterParts => this.wordDocument.FooterParts
                    .Select(x => new { x.Identifier, x.MainDocumentPart })
                    .ToDictionary(x => x.Identifier, y => y.MainDocumentPart);

        public Dictionary<string, IEnumerable<ImagePart>> FooterImagesParts => this.wordDocument.FooterParts
                    .Select(x => new { x.Identifier, x.ImagesPart })
                    .ToDictionary(x => x.Identifier, y => y.ImagesPart);

        private TemplateProcessor(WordprocessingDocument wordDocument)
        {
            this.wordDocument = new WordDocumentContainer(wordDocument);
            this._isNeedToNoticeAboutErrors = true;
        }

        public TemplateProcessor(string fileName) : this(WordprocessingDocument.Open(fileName, true))
        {
        }

        public TemplateProcessor(Stream stream) : this(WordprocessingDocument.Open(stream, true))
        {
        }

        public TemplateProcessor(XDocument templateSource, XDocument stylesPart = null, XDocument numberingPart = null)
        {
            this._isNeedToNoticeAboutErrors = true;
            this.wordDocument = new WordDocumentContainer(templateSource, stylesPart, numberingPart);
        }

        public TemplateProcessor SetRemoveContentControls(bool isNeedToRemove)
        {
            this.isNeedToRemoveContentControls = isNeedToRemove;
            return this;
        }

        public TemplateProcessor SetNoticeAboutErrors(bool isNeedToNotice)
        {
            this._isNeedToNoticeAboutErrors = isNeedToNotice;
            return this;
        }

        public TemplateProcessor FillContent(Content content)
        {
            var processor = new ContentProcessor(
                new ProcessContext(this.wordDocument))
                .SetRemoveContentControls(this.isNeedToRemoveContentControls);

            var processResult = processor.FillContent(this.Document.Root.Element(W.body), content);

            if (this.wordDocument.HasFooters)
            {
                foreach (var footer in this.wordDocument.FooterParts)
                {
                    var footerProcessor = new ContentProcessor(
                        new ProcessContext(footer))
                        .SetRemoveContentControls(this.isNeedToRemoveContentControls);

                    var footerProcessResult = footerProcessor.FillContent(footer.MainDocumentPart.Element(W.footer), content);
                    processResult.Merge(footerProcessResult);
                }
            }

            if (this.wordDocument.HasHeaders)
            {
                foreach (var header in this.wordDocument.HeaderParts)
                {
                    var headerProcessor = new ContentProcessor(
                        new ProcessContext(header))
                        .SetRemoveContentControls(this.isNeedToRemoveContentControls);

                    var headerProcessResult = headerProcessor.FillContent(header.MainDocumentPart.Element(W.header), content);
                    processResult.Merge(headerProcessResult);
                }
            }

            if (this._isNeedToNoticeAboutErrors)
            {
                this.AddErrors(processResult.Errors);
            }

            return this;
        }

        private IEnumerable<Field> GetSdtChildsOfXElement(XElement sdtContent)
        {
            if (sdtContent == null)
            {
                return null;
            }

            var wp = sdtContent.Elements(W.p);
            if (wp != null)
            {
                var wpsdt = wp.Elements().Where(i => i.Name == W.sdt);
                if (wpsdt != null)
                {
                    List<Field> items = new List<Field>();
                    foreach (var el in wpsdt)
                    {
                        var fields = this.GetAllFieldsFromXElement(el);
                        if (fields != null)
                        {
                            items.AddRange(fields);
                        }
                    }

                    if (items.Count == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return items;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<Field> GetAllFieldsFromXElement(XElement element)
        {
            if (element == null)
            {
                return null;
            }

            var wsdts = element.FirstLevelDescendantsAndSelf(W.sdt);
            if (wsdts != null)
            {
                return wsdts
                    .Select(x => new Field()
                    {
                        Name = x.Element(W.sdtPr).Element(W.tag).Attribute(W.val).Value,
                        Children = this.GetSdtChildsOfXElement(x.Element(W.sdtContent)),
                    });
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Field> GetAllFields()
        {
            if (this.Document == null || this.Document.Root == null)
            {
                return null;
            }

            return this.GetAllFieldsFromXElement(this.Document.Root.Element(W.body));
        }

        public void SaveChanges()
        {
            this.wordDocument.SaveChanges();
        }

        /// <summary>
        /// Adds a list of errors as red text on yellow at the beginning of the document.
        /// </summary>
        /// <param name="errors">List of errors.</param>
        private void AddErrors(IList<IError> errors)
        {
            if (errors.Any())
            {
                this.Document.Root
                    .Element(W.body)
                    .AddFirst(errors.Select(s =>
                        new XElement(W.p,
                            new XElement(W.r,
                                new XElement(W.rPr,
                                    new XElement(W.color,
                                        new XAttribute(W.val, "red")),
                                    new XElement(W.sz,
                                        new XAttribute(W.val, "28")),
                                    new XElement(W.szCs,
                                        new XAttribute(W.val, "28")),
                                    new XElement(W.highlight,
                                        new XAttribute(W.val, "yellow"))),
                                new XElement(W.t, s.Message)))));
            }
        }

        public void Dispose()
        {
            if (this.wordDocument != null)
            {
                this.wordDocument.Dispose();
            }
        }
    }
}
