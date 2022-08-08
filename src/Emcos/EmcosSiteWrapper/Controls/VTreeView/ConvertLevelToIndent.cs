using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace TMP.Work.Emcos.Controls.VTreeView
{
    public class ConvertLevelToIndent : IValueConverter
    {
        public double Length { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return 0;
            int v = 0;
            Int32.TryParse(value.ToString(), out v);

            return v * Length;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Not supported - ConvertBack should never be called in a OneWay Binding.");
        }
    }
}
