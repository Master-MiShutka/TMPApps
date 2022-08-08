namespace TemplateEngine.Docx
{
    using System;

    public interface IContentItem : IEquatable<IContentItem>
    {
        string Name { get; set; }

        bool IsHidden { get; set; }
    }
}
