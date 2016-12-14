using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GraphSharp.Controls;
using QuickGraph;

namespace TMP.DWRES.Graph
{
    /// <summary>
    /// Interaction logic for GraphLayout.xaml
    /// </summary>
    public partial class GraphLayout : UserControl
    {
        public GraphLayout()
        {
            InitializeComponent();
            fiderGraphLayout.HighlightAlgorithmType = "Simple";
            fiderGraphLayout.LayoutAlgorithmType = "EfficientSugiyama";
            fiderGraphLayout.OverlapRemovalAlgorithmType = "FSA";
        }
        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register(
            "Graph",
            typeof(FiderGraph),
            typeof(GraphLayout),
            new FrameworkPropertyMetadata(null,
            new PropertyChangedCallback(OnGraphChanged)));
        public FiderGraph Graph
        {
            get { return (FiderGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        private static void OnGraphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FiderGraph g = (FiderGraph)e.NewValue;
            ((GraphLayout)d).fiderGraphLayout.Graph = g;
        }
    }
}
