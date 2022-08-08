using GraphSharp.Controls;

namespace TMP.DWRES.Graph
{    
    public class FiderGraphLayout : GraphLayout<FiderGraphVertex, FiderGraphEdge, FiderGraph>
    {
        #region Ctor
        public FiderGraphLayout()
        {
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            this.HighlightAlgorithmType = "Simple";
            this.OverlapRemovalAlgorithmType = "FSA";
            this.LayoutAlgorithmType = "FR";
        }
        #endregion

        public FiderGraphLayout Clone()
        {
            return new FiderGraphLayout()
            {
                HighlightAlgorithmType = this.HighlightAlgorithmType,
                OverlapRemovalAlgorithmType = this.OverlapRemovalAlgorithmType,
                LayoutAlgorithmType = this.LayoutAlgorithmType,
                Graph = this.Graph
            };
        }
    }
}
