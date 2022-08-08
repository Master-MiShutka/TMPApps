namespace TemplateEngine.Docx
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Field
    {
        public string Name { get; set; }

        public IEnumerable<Field> Children { get; set; }

        public bool HasChildren => this.Children != null && this.Children.Count() > 0;
    }
}
