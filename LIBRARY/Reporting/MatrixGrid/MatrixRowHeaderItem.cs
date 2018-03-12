using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Представляет собой ячейку заголовка строки матрицы
    /// </summary>
    public class MatrixRowHeaderItem : MatrixHeaderItemBase
    {
        public MatrixRowHeaderItem(string rowHeader)
        {
            base.Header = rowHeader;
        }
        public MatrixRowHeaderItem(string rowHeader, object tag = null)
        {
            base.Header = rowHeader;
            base.Tag = tag;
        }
        public MatrixRowHeaderItem(string rowHeader, object tag = null, IList<IMatrixHeader> children = null)
        {
            base.Header = rowHeader;
            base.Tag = tag;
            base.Children = children;
        }
        public MatrixRowHeaderItem(string rowHeader, IList<IMatrixHeader> children = null)
        {
            base.Header = rowHeader;
            base.Children = children;
        }
        public MatrixRowHeaderItem(IMatrixHeader header)
        {
            base.Header = header.Header;
            base.Tag = header.Tag;
            base.Children = header.Children;
        }
    }
}
