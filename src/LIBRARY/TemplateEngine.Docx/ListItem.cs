namespace TemplateEngine.Docx
{
    using System.Xml.Linq;

    public class ListItem
    {
        public bool IsListItem { get; set; }

        public XElement Element { get; set; }

        public int? AbstractNumId { get; set; }

        public int? NumId { get; set; }

        public int? Level { get; set; }

        public ListItem(bool isListItem)
        {
            this.IsListItem = isListItem;
        }

        public ListItem(XElement element, int? abstractNumId, int? numId, int? level, bool isListItem)
        {
            this.Element = element;
            this.AbstractNumId = abstractNumId;
            this.NumId = numId;
            this.Level = level;
            this.IsListItem = isListItem;
        }
    }
}
