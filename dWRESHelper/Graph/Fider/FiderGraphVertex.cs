using System.Diagnostics;

namespace TMP.DWRES.Graph
{
    [DebuggerDisplay("{ID}-{Name}")]
    public class FiderGraphVertex
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public bool KAState { get; set; }
        public bool AB { get; set; }
        public GraphVertexType Type { get; set; }
        public FiderGraphVertex()
        {
            ID = default(int);
            Name = "<нет>";
        }
        public FiderGraphVertex(int id, string name, bool kastate, GraphVertexType type, bool abonent = false)
        {
            ID = id;
            Name = name;
            KAState = kastate;
            AB = abonent;
            Type = type;
        }
        public FiderGraphVertex(int id, string name)
        {
            ID = id;
            Name = name;
        }
        public FiderGraphVertex(int id)
        {
            ID = id;
            Name = string.Format("нет (ИД {0})", id);
        }

        public FiderGraphVertex(FiderGraphVertex vertex)
        {
            this.ID = vertex.ID;
            this.Name = vertex.Name;
            this.KAState = vertex.KAState;
            this.AB = vertex.AB;
            this.Type = vertex.Type;
        }

        public override string ToString()
        {
            return string.Format("Вершина '{1}' с ИД '{0}'", ID, Name);
        }
    }

    public enum GraphVertexType
    {
        unknown,
        Supply,
        Node,
        Transformer        
    }   
}
