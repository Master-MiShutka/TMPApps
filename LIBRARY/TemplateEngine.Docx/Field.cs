using System;
using System.Collections.Generic;
using System.Linq;

namespace TemplateEngine.Docx
{
    public class Field
    {
        public string Name { get; set; }
        public IEnumerable<Field> Children { get; set; }
        public bool HasChildren => Children != null && Children.Count() > 0;
    }
}
