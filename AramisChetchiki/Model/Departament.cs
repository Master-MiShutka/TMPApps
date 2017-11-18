using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.WORK.AramisChetchiki.Model
{
    [Serializable]
    public class Departament
    {
        public bool IsSelected { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string DataFileName { get; set; }
    }
}
