namespace TemplateEngine.Docx.Processors
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using DocumentFormat.OpenXml.Packaging;

    internal class ProcessContext
    {
        internal IDocumentContainer Document { get; private set; }

        internal Dictionary<int, int> LastNumIds { get; private set; }

        internal ProcessContext(IDocumentContainer document)
        {
            this.Document = document;
            this.LastNumIds = new Dictionary<int, int>();
        }
    }
}
