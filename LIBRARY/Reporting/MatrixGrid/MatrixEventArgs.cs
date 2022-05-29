using System;
using System.Collections.Generic;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    public class MatrixEventArgs : EventArgs
    {
        private readonly IMatrix matrix;

        internal MatrixEventArgs(IMatrix matrix)
        {
            this.matrix = matrix;
        }

        /// <summary>
        /// Matrix
        /// </summary>
        public IMatrix Matrix => this.matrix;
    }

    public class MatrixBuildedEventArgs : EventArgs
    {
        private readonly IList<IMatrixCell> items;
        private readonly IMatrixCell[,] cells;

        internal MatrixBuildedEventArgs(IList<IMatrixCell> items, IMatrixCell[,] cells)
        {
            this.items = items;
            this.cells = cells;
        }

        /// <summary>
        /// Items of Matrix
        /// </summary>
        public IList<IMatrixCell> Items => this.items;

        /// <summary>
        /// Cells of Matrix
        /// </summary>
        public IMatrixCell[,] Cells => this.cells;
    }

    public delegate void MatrixBuildedEventHandler(object sender, MatrixBuildedEventArgs e);

    public delegate void MatrixEventHandler(object sender, MatrixEventArgs e);
}
