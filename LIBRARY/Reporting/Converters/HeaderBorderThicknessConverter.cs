using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace TMP.UI.Controls.WPF.Reporting.Converters
{
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;

    public class HeaderBorderThicknessConverter : IMultiValueConverter
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

        public object Convert(object value, object parameter = null)
        {
            return this.Convert(new object[] { value }, null, parameter, null);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IMatrixCell cell = values[0] as IMatrixCell;
            if (cell == null)
                return DependencyProperty.UnsetValue;

            IMatrix matrix = null;
            if (values.Length > 1)
                matrix = values[1] as IMatrix;


            if (cell.CellType == MatrixCellType.DataCell)
            {
                if (matrix != null)
                {
                    if (matrix.Size.Width - 1 == cell.GridColumn)
                        return new Thickness(0d, 0d, 2d, 1d);
                }
                return new Thickness(0d, 0d, 1d, 1d);
            }

            IMatrixHeader header = cell as IMatrixHeader;
            IMatrixSummaryCell summaryCell = cell as IMatrixSummaryCell;

            if (header != null)
            {
                if (header.CellType == MatrixCellType.Empty)
                {
                    return new Thickness(0d, 0d, 2d, 2d);
                }
                if (header.CellType == MatrixCellType.ColumnHeader || header.CellType == MatrixCellType.ColumnsGroupHeader)
                {
                    if (header.GridRow == 0)
                    {
                        if (header.ChildrenCount == 0)
                            return new Thickness(0d, 2d, 1d, 2d);
                        else
                            return new Thickness(0d, 2d, 1d, 1d);
                    }
                    else
                    {
                        if (header.ChildrenCount == 0)
                            return new Thickness(0d, 0d, 1d, 2d);
                        else
                            return new Thickness(0d, 0d, 1d, 1d);
                    }
                }
                if (header.CellType == MatrixCellType.RowHeader || header.CellType == MatrixCellType.RowsGroupHeader)
                {
                    if (header.GridColumn == 0)
                    {
                        if (header.ChildrenCount == 0)
                            return new Thickness(2d, 0d, 2d, 1d);
                        else
                            return new Thickness(2d, 0d, 1d, 1d);
                    }
                    else
                    {
                        if (header.ChildrenCount == 0)
                            return new Thickness(0d, 0d, 2d, 1d);
                        else
                            return new Thickness(0d, 0d, 1d, 1d);
                    }
                }
                if (header.CellType == MatrixCellType.ColumnSummaryHeader)
                {
                    if (header.GridRow == 0)
                        return new Thickness(1d, 2d, 2d, 2d);
                    return new Thickness(0d, 1d, 1d, 1d);
                }
                if (header.CellType == MatrixCellType.RowSummaryHeader)
                {
                    if (header.GridColumn == 0)
                        return new Thickness(2d, 1d, 2d, 2d);
                    return new Thickness(0d, 1d, 1d, 1d);
                }
            }
            if (summaryCell != null)
            {
                if (summaryCell.SummaryType == MatrixSummaryType.RowSummary || summaryCell.SummaryType == MatrixSummaryType.RowsGroupSummary)
                {
                    return new Thickness(2d, 0d, 2d, 2d);
                }
                if (summaryCell.SummaryType == MatrixSummaryType.ColumnSummary || summaryCell.SummaryType == MatrixSummaryType.ColumnsGroupSummary)
                {
                    return new Thickness(0d, 2d, 2d, 2d);
                }
                if (summaryCell.SummaryType == MatrixSummaryType.TotalGroupSummary)
                {
                    return new Thickness(0d, 0d, 1d, 1d);
                }
                if (summaryCell.SummaryType == MatrixSummaryType.TotalSummary)
                {
                    return new Thickness(0d, 0d, 2d, 2d);
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
