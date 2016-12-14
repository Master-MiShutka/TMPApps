using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace TMP.DWRES.Graph
{
    [ValueConversion(typeof(GraphVertexType), typeof(Brush))]
    public class VertexControlBackgroundStyleConverter : IValueConverter
    {
        public FrameworkElement FrameworkElement = new FrameworkElement();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexBackground") as SolidColorBrush);

            GraphVertexType vertexType = GraphVertexType.Node;

            if (Enum.IsDefined(typeof(GraphVertexType), value))
                vertexType = (GraphVertexType)Enum.Parse(typeof(GraphVertexType), value.ToString());

            switch (vertexType)
            {
                case GraphVertexType.Supply:
                    return (FrameworkElement.TryFindResource("fiderGraphSupplyVertexBackground") as SolidColorBrush);
                case GraphVertexType.Node:
                    return (FrameworkElement.TryFindResource("fiderGraphNodeVertexBackground") as SolidColorBrush);
                case GraphVertexType.Transformer:
                    return (FrameworkElement.TryFindResource("fiderGraphTransformerVertexBackground") as SolidColorBrush);
                case GraphVertexType.unknown:
                    return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexBackground") as SolidColorBrush);
                default:
                    return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexBackground") as SolidColorBrush);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
