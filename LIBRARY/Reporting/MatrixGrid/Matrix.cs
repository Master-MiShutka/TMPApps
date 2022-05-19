namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class Matrix : MatrixBase
    {
        public Matrix() { }

        public Matrix(IList<IMatrixCell> matrixCells)
            : base()
        {
            if (matrixCells == null)
                return;

            this.Items = new ReadOnlyCollection<IMatrixCell>(matrixCells);
        }

        #region Public Properties

        public Func<IEnumerable<IMatrixHeader>> GetColumnHeaderValuesFunc { get; set; }

        public Func<IEnumerable<IMatrixHeader>> GetRowHeaderValuesFunc { get; set; }

        public Func<IMatrixHeader, IMatrixHeader, IMatrixDataCell> GetDataCellFunc { get; set; }

        #endregion

        #region Base Class Overrides

        protected override IEnumerable<IMatrixHeader> GetColumnHeaderValues()
        {
            var func = this.GetColumnHeaderValuesFunc;
            if (func == null)
            {
                throw new ArgumentNullException("GetColumnHeaderValuesFunc is null!");
            }
            else
            {
                return func();
            }
        }

        protected override IEnumerable<IMatrixHeader> GetRowHeaderValues()
        {
            var func = this.GetRowHeaderValuesFunc;
            if (func == null)
            {
                throw new ArgumentNullException("GetRowHeaderValuesFunc is null!");
            }
            else
            {
                return func();
            }
        }

        protected override IMatrixDataCell GetDataCell(IMatrixHeader rowHeaderValue, IMatrixHeader columnHeaderValue)
        {
            var func = this.GetDataCellFunc;
            if (func == null)
            {
                throw new ArgumentNullException("GetCellValueFunc is null!");
            }
            else
            {
                return func(rowHeaderValue, columnHeaderValue);
            }
        }

        #endregion // Base Class Overrides
    }
}
