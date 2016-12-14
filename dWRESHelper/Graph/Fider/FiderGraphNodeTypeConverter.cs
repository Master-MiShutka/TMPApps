using System;
using System.Windows.Data;

namespace TMP.DWRES.Graph
{
    [ValueConversion(typeof(GraphVertexType), typeof(string))]
    public class FiderGraphNodeTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "NULL";

            GraphVertexType vertexType = GraphVertexType.Node;

            if (Enum.IsDefined(typeof(GraphVertexType), value))
                vertexType = (GraphVertexType)Enum.Parse(typeof(GraphVertexType), value.ToString());
            switch (vertexType)
            {
                case GraphVertexType.Supply:
                    return "центр питания";
                case GraphVertexType.Node:
                    return "узел";
                case GraphVertexType.Transformer:
                    return "трансформаторная подстанция";
                case GraphVertexType.unknown:
                    return "<неизвестно>";
                default:
                    return "<не определён>";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
