using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace TMP.UI.Controls.WPF.Reporting.Converters
{
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;
    [ValueConversion(typeof(IMatrixItem), typeof(Thickness))]
    public class HeaderBorderThicknessConverter : IValueConverter
    {
        #region Singleton Property

        public static HeaderBorderThicknessConverter Singleton
        {
            get
            {
                if (mg_singleton == null)
                    mg_singleton = new HeaderBorderThicknessConverter();

                return mg_singleton;
            }
        }

        private static HeaderBorderThicknessConverter mg_singleton;

        #endregion Singleton Property

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MatrixItemBase header = value as MatrixItemBase;
            if (header is MatrixColumnHeaderItem)
            {
                if (header.GridRow == 0)
                    return new Thickness(0d, 1d, 1d, 1d);
                else
                    return new Thickness(0d, 0d, 1d, 1d);
            }
            if (header is MatrixRowHeaderItem)
            {
                if (header.GridColumn == 0)
                    return new Thickness(1d, 0d, 1d, 1d);
                else
                    return new Thickness(0d, 0d, 1d, 1d);
            }
            if (header is MatrixSummaryHeaderItem)
            {
                if (header.GridRow == 0)
                    return new Thickness(0d, 1d, 1d, 1d);
                if (header.GridColumn== 0)
                    return new Thickness(1d, 0d, 1d, 1d);

                return new Thickness(0d, 1d, 1d, 1d);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}
