using GraphSharp.Controls;

namespace DWRES.Graph
{
    public class TestGraphLayout : GraphLayout<GraphVertex, GraphEdge, FiderGraph>
    {
        #region Ctor
        public TestGraphLayout()
        {
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            this.HighlightAlgorithmType = "Simple";
            this.OverlapRemovalAlgorithmType = "FSA";
        }
        #endregion
    }
}
