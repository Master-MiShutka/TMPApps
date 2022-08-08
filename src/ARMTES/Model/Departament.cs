using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TMP.ARMTES.Model
{
    [XmlRoot("Departament")]
    public class Departament
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public string Description { get; set; }
    }
}
