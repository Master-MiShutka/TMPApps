﻿namespace TMP.UI.WPF.Reporting.MatrixGrid
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Конвертер значений для информирования MatrixGrid об измененных свойствах Grid.Row и Grid.Column у ячеек
    /// </summary>
    internal class MatrixGridChildConverter : IValueConverter
    {
        #region Constructor

        public MatrixGridChildConverter(MatrixGrid matrixGrid)
        {
            this.matrixGrid = matrixGrid;
        }

        #endregion // Constructor

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                int index = (int)value;

                if (parameter == Grid.RowProperty)
                {
                    this.matrixGrid.InspectRowIndex(index);
                }

                if (parameter == Grid.ColumnProperty)
                {
                    this.matrixGrid.InspectColumnIndex(index);
                }

                if (parameter == Grid.RowSpanProperty)
                {
                    this.matrixGrid.InspectRowIndex(index);
                }

                if (parameter == Grid.ColumnSpanProperty)
                {
                    this.matrixGrid.InspectRowIndex(index);
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion // IValueConverter Members

        #region Fields

        private readonly MatrixGrid matrixGrid;

        #endregion // Fields
    }
}
