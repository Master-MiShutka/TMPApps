using QuickGraph;
using System.Collections.Generic;


namespace TMP.DWRES.Graph
{
    public class FiderGraph :  BidirectionalGraph<FiderGraphVertex, FiderGraphEdge>
    {
        public FiderGraph() { }

        public FiderGraph(bool allowParallelEdges)
            : base(allowParallelEdges) { }

        public FiderGraph(bool allowParallelEdges, int vertexCapacity)
            : base(allowParallelEdges, vertexCapacity) { }

        #region Private Methods

        #endregion
    }
}
