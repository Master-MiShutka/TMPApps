﻿namespace TemplateEngine.Docx
{
    using System.Xml.Linq;
    using DocumentFormat.OpenXml.Packaging;

    internal interface IDocumentContainer
    {
        XDocument MainDocumentPart { get; }

        XDocument NumberingPart { get; }

        XDocument StylesPart { get; }

        OpenXmlPart GetPartById(string partIdentifier);

        void RemovePartById(string partIdentifier);

        string AddImagePart(byte[] bytes);
    }
}
