namespace TemplateEngine.Docx.Processors
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    internal interface IProcessor
    {

        IProcessor SetRemoveContentControls(bool isNeedToRemove);

        ProcessResult FillContent(XElement contentControl, IEnumerable<IContentItem> items);
    }
}