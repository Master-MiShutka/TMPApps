using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DWRES.Graph
{
    [ValueConversion(typeof(GraphVertexType), typeof(ControlTemplate))]
    public class GraphStyleConverter : IValueConverter
    {
        public FrameworkElement FrameworkElement = new FrameworkElement();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return (FrameworkElement.TryFindResource("graphVertexDefaultTemplate") as ControlTemplate);

            GraphVertexType vertexType = GraphVertexType.Node;

            if (Enum.IsDefined(typeof(GraphVertexType), value))
                vertexType = (GraphVertexType)Enum.Parse(typeof(GraphVertexType), value.ToString());

            switch (vertexType)
            {
                case GraphVertexType.Supply:
                    return (FrameworkElement.TryFindResource("graphSupplyVertexTemplate") as ControlTemplate);
                case GraphVertexType.Node:
                    return (FrameworkElement.TryFindResource("graphNodeVertexTemplate") as ControlTemplate);
                case GraphVertexType.Transformer:
                    return (FrameworkElement.TryFindResource("graphTransformerVertexTemplate") as ControlTemplate);
                case GraphVertexType.unknown:
                    return (FrameworkElement.TryFindResource("graphVertexDefaultTemplate") as ControlTemplate);
                default:
                    return (FrameworkElement.TryFindResource("graphVertexDefaultTemplate") as ControlTemplate);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
