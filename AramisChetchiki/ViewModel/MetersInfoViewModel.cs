using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using TMP.Extensions;
    using TMP.UI.Controls.WPF;
    using TMP.WORK.AramisChetchiki.Model;
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;
    using System.Collections.ObjectModel;

    public class MetersInfoViewModel : BaseViewModel
    {
        private Data _data;

        ICollection<IMatrix> _pivotCollection;

        #region Constructors

        public MetersInfoViewModel() : base(null)
        {
            this.CommandUpdate = new DelegateCommand(delegate
               {
                   this.PrepareData();
               }, () => this._data != null, "Обновление");
        }

        public MetersInfoViewModel(Data data) : this()
        {
            bool flag = data == null;
            bool flag2 = flag;
            if (flag2)
            {
                throw new ArgumentNullException("Data is null!");
            }
            this._data = data;
            this.PrepareData();
        }

        #endregion

        #region Propperties

        public ICommand CommandUpdate { get; private set; }

        public ICollection<IMatrix> PivotCollection
        {
            get
            {
                return this._pivotCollection;
            }
            private set
            {
                this._pivotCollection = value;
                base.RaisePropertyChanged("PivotCollection");
            }
        }

        #endregion

        #region Private Methods

        private void PrepareData()
        {
            IsBusy = true;
            base.Status = "Подготовка данных";
            base.DetailedStatus = "подготовка ...";
            Task task = Task.Factory.StartNew(delegate
            {
                if (!(this._data == null || this._data.Meters == null || this._data.Meters.Count == 0))
                {
                    _pivotCollection = new List<IMatrix>();


                    int curYear = DateTime.Now.Year;
                    const int yearsCount = 3;
                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по установке или замене счётчиков за последние три года помесячно",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => Enumerable.Range(curYear - yearsCount, yearsCount).Reverse().Select(i => MatrixHeaderCell.CreateRowHeader(i.ToString())),
                        GetColumnHeaderValuesFunc = () => System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12)
                            .Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var l = this._data.ChangesOfMeters
                                .Where(i => i.Дата_замены.HasValue)
                                .Select(i => i.Дата_замены.Value)
                                .ToList();
                            var l1 = l
                                .Where(i => i.Year.ToString() == row.Header)
                                .ToList();

                            var values = l1
                                .Where(i => string.Equals(i.ToString("MMMM"), column.Header))
                                .ToList();
                            if (values == null || values.Count() == 0)
                                return new MatrixDataCell(string.Empty);
                            else
                                return new MatrixDataCell(values.Count());
                        }
                    });

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по установке или замене на электронный счётчик за последние три года помесячно",
                        Description = "* количество электронных счётчиков",
                        GetRowHeaderValuesFunc = () => Enumerable.Range(curYear - yearsCount, yearsCount).Reverse().Select(i => MatrixHeaderCell.CreateRowHeader(i.ToString())),
                        GetColumnHeaderValuesFunc = () => System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12)
                            .Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var l = this._data.ChangesOfMeters
                                .Where(i => i.Дата_замены.HasValue)
                                .Where(i => i.ЭтоЭлектронный)
                                .Select(i => i.Дата_замены.Value)
                                .ToList();
                            var l1 = l
                                .Where(i => i.Year.ToString() == row.Header)
                                .ToList();

                            var values = l1
                                .Where(i => string.Equals(i.ToString("MMMM"), column.Header))
                                .ToList();
                            if (values == null || values.Count() == 0)
                                return new MatrixDataCell(string.Empty);
                            else
                                return new MatrixDataCell(values.Count());
                        }
                    });


                    int metersCount = this._data.Meters.Count;

                    IEnumerable<SummaryInfoGroupItem> meterTypes = SummaryInfoHelper.BuildFirst10LargeGroups(this._data.Meters, "Тип_счетчика");

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по типам счётчиков",
                        Description = "* количество счётчиков",
                        ShowRowsTotal = false,
                        GetRowHeaderValuesFunc = () => meterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => new string[] { "Количество", "%" }.Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = meterTypes.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            if (column.Header == "%")
                                return new MatrixDataCell(group.Percent);
                            else
                                return new MatrixDataCell(group.Count);
                        }
                    });

                    // Свод по типу счётчика и году поверки
                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по типу счётчика и году поверки",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => meterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Год_поверки_для_отчётов).Distinct().OrderBy(i => i).Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = meterTypes.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            var list = group.Value.Where(i => i.Год_поверки_для_отчётов == column.Header).ToList();
                            if (list != null)
                                return new MatrixDataCell(list.Count());
                            else
                                return new MatrixDataCell(0);
                        }
                    });

                    // Свод по категории счётчика и типу населённого пункта
                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по категории счётчика и\nтипу населённого пункта",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => this._data.Meters.Select(i => i.Группа_счётчика_для_отчётов).Distinct().Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Тип_населённого_пункта).Distinct().Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var list = this._data.Meters
                                .Where(i => i.Группа_счётчика_для_отчётов == row.Header)
                                .Where(i => i.Тип_населённого_пункта == column.Header)
                                .ToList();
                            if (list != null)
                            {
                                int count = list.Count();
                                return new MatrixDataCell(count) { ToolTip = String.Format("{0:N1}%", 100 * count / metersCount) };
                            }
                            else
                                return new MatrixDataCell(0);
                        }
                    });

                    // Свод по населенному пункту и принципу действия счётчика
                    IEnumerable<SummaryInfoGroupItem> meterPerLocality = SummaryInfoHelper.BuildFirst10LargeGroups(this._data.Meters, "Населённый_пункт");

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по населенному пункту и\nпринципу действия счётчика",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => meterPerLocality.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Принцип).Distinct().Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = meterPerLocality.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            var list = group.Value.Where(i => i.Принцип == column.Header).ToList();
                            if (list != null)
                            {
                                int count = list.Count();
                                return new MatrixDataCell(count) { ToolTip = String.Format("{0:N1}%", 100 * count / metersCount) };
                            }
                            else
                                return new MatrixDataCell(0);
                        }
                    });

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по населенному пункту и\nколичеству просроченных",
                        Description = "* количество счётчиков",
                        ShowRowsTotal = false,
                        GetRowHeaderValuesFunc = () => meterPerLocality.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => new string[] { "Количество\nнеповеренных", "% от количества в н.п." }.Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                        GetDataCellFunc = (row, column) =>
                        {
                            var group = meterPerLocality.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            int countPerLoacality = group.Value.Count;
                            var list = group.Value.Where(i => i.Поверен == false).ToList();
                            if (list == null)
                                return new MatrixDataCell(0);

                            int count = list.Count();
                            if (column.Header == "% от количества в н.п.")
                            {
                                return new MatrixDataCell(String.Format("{0:N1}%", 100 * count / countPerLoacality));
                            }
                            else
                            {
                                
                                return new MatrixDataCell(count) { ToolTip = String.Format("{0:N1}% от общего количества счётчиков", 100 * count / metersCount) };
                            }
                        }
                    });

                    _pivotCollection.Add(new MatrixModelDelegate()
                    {
                        Header = "Свод по подстанции и\nпринципу действия",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => this._data.Meters.Select(i => i.Подстанция).Distinct().OrderBy(i => i).Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                        GetColumnHeaderValuesFunc = () => this._data.Meters
                            .Select(i => i.Принцип).Distinct()
                            .Select(i => MatrixHeaderCell.CreateHeaderCell(i, this._data.Meters.Select(ii => ii.Фаз).Distinct().OrderBy(ii => ii).Select(ii => MatrixHeaderCell.CreateHeaderCell(ii.ToString())).ToList<IMatrixHeader>())),
                        GetDataCellFunc = (row, column) =>
                        {
                            byte фаз = 0;
                            byte.TryParse(column.Header, out фаз);

                            var list = this._data.Meters
                                .Where(i => i.Подстанция == row.Header)
                                .Where(i => i.Принцип == column.Parent.Header)
                                .Where(i => i.Фаз == фаз)
                                .ToList();
                            if (list == null)
                                return new MatrixDataCell(0);
                            return new MatrixDataCell(list.Count);
                        }
                    });
                }
            });
            task.ContinueWith(delegate (Task t)
            {
                base.IsBusy = false;
                base.Status = null;
                base.DetailedStatus = null;
                base.RaisePropertyChanged("PivotCollection");
            });
            task.ContinueWith(delegate (Task t)
            {
                MessageBox.Show(
                    string.Format(
                        "Произошла ошибка при подготовке данных.\nОписание: {0}", 
                        App.GetExceptionDetails(t.Exception)), 
                    "Ошибка", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Hand);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        #endregion

    }
}
