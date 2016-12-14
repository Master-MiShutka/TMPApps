using QuickGraph;

namespace WpfApplication1
{
    public class TestEdge : Edge<TestVertex>
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public TestEdge(int id, TestVertex source, TestVertex target)
            : base(source, target)
        {
            ID = id;
            Name = string.Format("{0}-{1}", source.ID, target.ID);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
