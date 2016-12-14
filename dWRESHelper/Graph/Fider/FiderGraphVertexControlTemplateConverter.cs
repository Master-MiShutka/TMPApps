using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TMP.DWRES.Graph
{
    [ValueConversion(typeof(GraphVertexType), typeof(ControlTemplate))]
    public class FiderGraphVertexControlTemplateConverter : IValueConverter
    {
        public FrameworkElement FrameworkElement = new FrameworkElement();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return (FrameworkElement.TryFindResource("fiderDefaultVertexControlTemplate") as ControlTemplate);

            GraphVertexType vertexType = GraphVertexType.Node;

            if (Enum.IsDefined(typeof(GraphVertexType), value))
                vertexType = (GraphVertexType)Enum.Parse(typeof(GraphVertexType), value.ToString());

            switch (vertexType)
            {
                case GraphVertexType.Supply:
                    return (FrameworkElement.TryFindResource("fiderSupplyVertexControlTemplate") as ControlTemplate);
                case GraphVertexType.Node:
                    return (FrameworkElement.TryFindResource("fiderNodeVertexControlTemplate") as ControlTemplate);
                case GraphVertexType.Transformer:
                    return (FrameworkElement.TryFindResource("fiderTransformerVertexControlTemplate") as ControlTemplate);
                case GraphVertexType.unknown:
                    return (FrameworkElement.TryFindResource("fiderDefaultVertexControlTemplate") as ControlTemplate);
                default:
                    return (FrameworkElement.TryFindResource("fiderDefaultVertexControlTemplate") as ControlTemplate);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
