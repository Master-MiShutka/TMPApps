using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Представляет ячейку заголовка столбца матрицы
    /// </summary>
    public class MatrixColumnHeaderItem : MatrixHeaderItemBase
    {
        public MatrixColumnHeaderItem(string columnHeader)
        {
            base.Header = columnHeader;
        }
        public MatrixColumnHeaderItem(string columnHeader, object tag = null)
        {
            base.Header = columnHeader;
            base.Tag = tag;
        }
        public MatrixColumnHeaderItem(string columnHeader, object tag = null, IList<IMatrixHeader> children = null)
        {
            base.Header = columnHeader;
            base.Tag = tag;
            base.Children = children;
        }
        public MatrixColumnHeaderItem(string columnHeader, IList<IMatrixHeader> children = null)
        {
            base.Header = columnHeader;
            base.Children = children;
        }

        public MatrixColumnHeaderItem(IMatrixHeader header)
        {
            base.Header = header.Header;
            base.Tag = header.Tag;
            base.Children = header.Children;
        }
    }
}
