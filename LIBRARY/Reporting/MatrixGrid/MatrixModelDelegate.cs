using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    public class MatrixModelDelegate : MatrixBase
    {
        #region Public Properties

        public Func<IEnumerable<IMatrixHeader>> GetColumnHeaderValuesFunc { get; set; }
        public Func<IEnumerable<IMatrixHeader>> GetRowHeaderValuesFunc { get; set; }
        public Func<IMatrixHeader, IMatrixHeader, IMatrixDataCell> GetDataCellFunc { get; set; }

        #endregion

        #region Base Class Overrides

        protected override IEnumerable<IMatrixHeader> GetColumnHeaderValues()
        {
            var func = GetColumnHeaderValuesFunc;
            if (func == null)
                throw new ArgumentNullException("GetColumnHeaderValuesFunc is null!");
            else
                return func();
        }

        protected override IEnumerable<IMatrixHeader> GetRowHeaderValues()
        {
            var func = GetRowHeaderValuesFunc;
            if (func == null)
                throw new ArgumentNullException("GetRowHeaderValuesFunc is null!");
            else
                return func();
        }

        protected override IMatrixDataCell GetDataCell(IMatrixHeader rowHeaderValue, IMatrixHeader columnHeaderValue)
        {
            var func = GetDataCellFunc;
            if (func == null)
                throw new ArgumentNullException("GetCellValueFunc is null!");
            else
                return func(rowHeaderValue, columnHeaderValue);
        }

        #endregion // Base Class Overrides
    }
}
