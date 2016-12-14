using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;


namespace TMP.DWRES.Graph
{
    [ValueConversion(typeof(GraphVertexType), typeof(Brush))]
    public class VertexControlDropShadowEffectColorStyleConverter : IValueConverter
    {
        public FrameworkElement FrameworkElement = new FrameworkElement();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexDropShadowEffectColor") as SolidColorBrush);

            GraphVertexType vertexType = GraphVertexType.Node;

            if (Enum.IsDefined(typeof(GraphVertexType), value))
                vertexType = (GraphVertexType)Enum.Parse(typeof(GraphVertexType), value.ToString());

            switch (vertexType)
            {
                case GraphVertexType.Supply:
                    return (FrameworkElement.TryFindResource("fiderGraphSupplyVertexDropShadowEffectColor") as SolidColorBrush);
                case GraphVertexType.Node:
                    return (FrameworkElement.TryFindResource("fiderGraphNodeVertexDropShadowEffectColor") as SolidColorBrush);
                case GraphVertexType.Transformer:
                    return (FrameworkElement.TryFindResource("fiderGraphTransformerVertexDropShadowEffectColor") as SolidColorBrush);
                case GraphVertexType.unknown:
                    return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexDropShadowEffectColor") as SolidColorBrush);
                default:
                    return (FrameworkElement.TryFindResource("fiderGraphDefaultVertexDropShadowEffectColor") as SolidColorBrush);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
