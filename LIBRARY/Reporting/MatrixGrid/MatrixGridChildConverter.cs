using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Конвертер значений для информирования MatrixGrid об измененных свойствах Grid.Row и Grid.Column у ячеек
    /// </summary>
    class MatrixGridChildConverter : IValueConverter
    {
        #region Constructor

        public MatrixGridChildConverter(MatrixGrid matrixGrid)
        {
            _matrixGrid = matrixGrid;
        }

        #endregion // Constructor

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int index = (int)value;

                if (parameter == Grid.RowProperty)
                        _matrixGrid.InspectRowIndex(index);
                if (parameter == Grid.ColumnProperty)
                    _matrixGrid.InspectColumnIndex(index);
                if (parameter == Grid.RowSpanProperty)
                    _matrixGrid.InspectRowIndex(index);
                if (parameter == Grid.ColumnSpanProperty)
                    _matrixGrid.InspectRowIndex(index);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion // IValueConverter Members

        #region Fields

        readonly MatrixGrid _matrixGrid;

        #endregion // Fields
    }
}
