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

    public class MetersInfoViewModel : BaseViewModel
    {
        private Data _data;

        private IMatrix _metersCountPerTypesPivot;
        private IMatrix _metersCountPerTypesPerPoverkaYearPivot;
        private IMatrix _metersCountPerCategoryForReportPivot;
        private IMatrix _metersCountPerLocalityPivot;

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

        public IMatrix MetersCountPerTypesPivot
        {
            get
            {
                return this._metersCountPerTypesPivot;
            }
            private set
            {
                this._metersCountPerTypesPivot = value;
                base.RaisePropertyChanged("MetersCountPerTypesPivot");
            }
        }

        public IMatrix MetersCountPerTypesPerPoverkaYearPivot
        {
            get
            {
                return this._metersCountPerTypesPerPoverkaYearPivot;
            }
            private set
            {
                this._metersCountPerTypesPerPoverkaYearPivot = value;
                base.RaisePropertyChanged("MetersCountPerTypesPerPoverkaYearPivot");
            }
        }

        public IMatrix MetersCountPerCategoryForReportPivot
        {
            get
            {
                return this._metersCountPerCategoryForReportPivot;
            }
            private set
            {
                this._metersCountPerCategoryForReportPivot = value;
                base.RaisePropertyChanged("MetersCountPerCategoryForReportPivot");
            }
        }

        public IMatrix MetersCountPerLocalityPivot
        {
            get
            {
                return this._metersCountPerLocalityPivot;
            }
            private set
            {
                this._metersCountPerLocalityPivot = value;
                base.RaisePropertyChanged("MetersCountPerLocalityPivot");
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
                    IEnumerable<SummaryInfoGroupItem> meterTypes = SummaryInfoHelper.BuildFirst10LargeGroups(this._data.Meters, "Тип_счетчика");

                    this.MetersCountPerTypesPivot = new MatrixModelDelegate()
                    {
                        Header = "Свод по типам счётчиков",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => meterTypes.Select(i => new MatrixRowHeaderItem(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => new string[] { "Количество", "%" }.Select(i => new MatrixColumnHeaderItem(i)),
                        GetCellValueFunc = (row, column) =>
                        {
                            var group = meterTypes.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            if (column.Header == "%")
                                return group.Percent;
                            else
                                return group.Count;
                        }
                    };

                    // Свод по типу счётчика и году поверки
                    this.MetersCountPerTypesPerPoverkaYearPivot = new MatrixModelDelegate()
                    {
                        Header = "Свод по типу счётчика и году поверки",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => meterTypes.Select(i => new MatrixRowHeaderItem(i.Key.ToString())),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Год_поверки_для_отчётов).Distinct().OrderBy(i => i).Select(i => new MatrixColumnHeaderItem(i)),
                        GetCellValueFunc = (row, column) =>
                        {
                            var group = meterTypes.Where(i => i.Key.ToString() == row.Header).FirstOrDefault();
                            var list = group.Value.Where(i => i.Год_поверки_для_отчётов == column.Header).ToList();
                            if (list != null)
                                return list.Count();
                            else
                                return 0;
                        }
                    };

                    // Свод по категории счётчика и типу населённого пункта
                    this.MetersCountPerCategoryForReportPivot = new MatrixModelDelegate()
                    {
                        Header = "Свод по категории счётчика и\nтипу населённого пункта",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => this._data.Meters.Select(i => i.Группа_счётчика_для_отчётов).Distinct().Select(i => new MatrixRowHeaderItem(i)),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Тип_населённого_пункта).Distinct().Select(i => new MatrixColumnHeaderItem(i)),
                        GetCellValueFunc = (row, column) =>
                        {
                            var list = this._data.Meters
                                .Where(i => i.Группа_счётчика_для_отчётов == row.Header)
                                .Where(i => i.Тип_населённого_пункта == column.Header)
                                .ToList();
                            if (list != null)
                                return list.Count();
                            else
                                return 0;
                        }
                    };

                    // Свод по населенному пункту и принципу действия счётчика
                    SummaryInfoItem infoCountMetersPerLocality = SummaryInfoHelper.BuildSummaryInfoItem(this._data.Meters, "Населённый_пункт");
                    this.MetersCountPerLocalityPivot = new MatrixModelDelegate()
                    {
                        Header = "Свод по населенному пункту и\nпринципу действия счётчика",
                        Description = "* количество счётчиков",
                        GetRowHeaderValuesFunc = () => infoCountMetersPerLocality.OnlyFirst10Items.Select(i => new MatrixRowHeaderItem(i.Value)),
                        GetColumnHeaderValuesFunc = () => this._data.Meters.Select(i => i.Принцип).Distinct().Select(i => new MatrixColumnHeaderItem(i)),
                        GetCellValueFunc = (row, column) =>
                        {
                            var list = this._data.Meters
                                .Where(i => i.Принцип == column.Header)
                                .Where(i => i.Населённый_пункт == row.Header)
                                .ToList();
                            if (list != null)
                                return list.Count();
                            else
                                return 0;
                        }
                    };
                }
            });
            task.ContinueWith(delegate (Task t)
            {
                base.IsBusy = false;
                base.Status = null;
                base.DetailedStatus = null;
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
