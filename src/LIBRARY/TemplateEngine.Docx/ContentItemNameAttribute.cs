namespace TemplateEngine.Docx
{
    using System;

    internal class ContentItemNameAttribute : Attribute
    {
        internal ContentItemNameAttribute(string name)
        {
            this.Name = name;
        }

        internal string Name { get; private set; }
    }
}
