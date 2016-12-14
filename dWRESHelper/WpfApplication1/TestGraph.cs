using System;
using System.Collections.Generic;
using System.Linq;
using QuickGraph;

namespace WpfApplication1
{
    public class TestGraph : BidirectionalGraph<TestVertex, TestEdge>
    {
        public TestGraph() { }

        public TestGraph(bool allowParallelEdges)
            : base(allowParallelEdges) { }

        public TestGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity) { }

        public new void AddVertex(TestVertex v)
        {
            base.AddVertex(v);
        }

        public void AddEdge(int id, TestVertex s, TestVertex t)
        {
            base.AddEdge(new TestEdge(id, s, t));
        }
    }
}
