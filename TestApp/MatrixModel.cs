using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMP.UI.Controls.WPF.Reporting.MatrixGrid;

namespace TestApp
{
    public class MatrixModel : INotifyPropertyChanged
    {
        IMatrix _model;

        public MatrixModel()
        {
            SampleText = "This is sample text";
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                SampleText = "DESIGN!";
            }
            else
                SampleText = "RUNTIME!";
        }

        string _sampleText;
        public string SampleText { get { return _sampleText; } set { _sampleText = value; RaisePropertyChanged("SampleText"); } }

        public IMatrix Model
        {
            get
            {
                if (_model == null)
                    Task.Factory.StartNew(() =>
                    {
                        Model = Get(true);
                    });
                return _model;
            }
            set
            { _model = value; RaisePropertyChanged("Model"); } }

        IMatrix Get(bool delayed = false)
        {
            Random rnd = new Random();

            Func<int, List<IMatrixHeader>> generateChilds = null;
            generateChilds = (count) =>
            {
                List<IMatrixHeader> result = new List<IMatrixHeader>();
                for (int i = count; i > 0; i--)
                {
                    result.Add(MatrixHeaderCell.CreateRowHeader(rnd.Next(100).ToString(), generateChilds(rnd.Next(count - 1))));
                }
                return result;
            };

            return new MatrixModelDelegate()
            {
                Header = "Sample title",
                Description = "sample description",
                GetRowHeaderValuesFunc = () => Enumerable.Range(rnd.Next(100), rnd.Next(5))
                    .Select(i => MatrixHeaderCell.CreateHeaderCell(i.ToString(), children: generateChilds(rnd.Next(5)))
                    ),
                GetColumnHeaderValuesFunc = () => System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12)
                    .Select(i => MatrixHeaderCell.CreateHeaderCell(i, generateChilds(rnd.Next(3)))),
                GetDataCellFunc = (row, column) =>
                {
                    if (delayed) System.Threading.Thread.Sleep(10);
                    IMatrixDataCell cell = new MatrixDataCell(rnd.Next(5, 100));
                    cell.ToolTip = cell.ToString();
                    return cell;
                }
            };
        }

        public void Go()
        {
            Model = null;
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null)
        {
            if (Equals(field, value)) { return false; }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
        protected void RaisePropertyChanged(string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)

        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion
    }
}
