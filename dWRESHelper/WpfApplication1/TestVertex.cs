using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class TestVertex
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public TestVertex()
        {
            ID = 0;
            Name = "Test";
        }
        public TestVertex(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
