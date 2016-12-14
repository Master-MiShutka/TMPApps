using GraphSharp.Controls;

namespace WpfApplication1
{
    public class TestGraphLayout : GraphLayout<TestVertex, TestEdge, TestGraph>
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
