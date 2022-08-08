using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace TMP.DWRES.Graph
{
    [ValueConversion(typeof(GraphVertexType), typeof(Brush))]
    public class VertexControlBorderBrushStyleConverter : IValueConverter
    {
        public FrameworkElement FrameworkElement = new FrameworkElement();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexBorderBrush") as SolidColorBrush);

            GraphVertexType vertexType = GraphVertexType.Node;

            if (Enum.IsDefined(typeof(GraphVertexType), value))
                vertexType = (GraphVertexType)Enum.Parse(typeof(GraphVertexType), value.ToString());

            switch (vertexType)
            {
                case GraphVertexType.Supply:
                    return (FrameworkElement.TryFindResource("fiderGraphSupplyVertexBorderBrush") as SolidColorBrush);
                case GraphVertexType.Node:
                    return (FrameworkElement.TryFindResource("fiderGraphNodeVertexBorderBrush") as SolidColorBrush);
                case GraphVertexType.Transformer:
                    return (FrameworkElement.TryFindResource("fiderGraphTransformerVertexBorderBrush") as SolidColorBrush);
                case GraphVertexType.unknown:
                    return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexBorderBrush") as SolidColorBrush);
                default:
                    return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexBorderBrush") as SolidColorBrush);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
