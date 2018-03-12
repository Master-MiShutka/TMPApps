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

    public class MetersInfoViewModel : BaseViewModel
    {
        private Data _data;

        private SummaryInfoItem _metersCountPerTypesPivot;
        private DataTable _metersCountPerTypesPerPoverkaYearPivot;
        private DataTable _metersCountPerCategoryForReportPivot;
        private DataTable _metersCountPerLocalityPivot;

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

        public SummaryInfoItem MetersCountPerTypesPivot
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

        public DataTable MetersCountPerTypesPerPoverkaYearPivot
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

        public DataTable MetersCountPerCategoryForReportPivot
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

        public DataTable MetersCountPerLocalityPivot
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
                    this.MetersCountPerTypesPivot = SummaryInfoHelper.BuildSummaryInfoItem(this._data.Meters, "Тип_счетчика");

                    this.MetersCountPerTypesPerPoverkaYearPivot = this._data.Meters
                        .ToPivotTable(
                        (Meter item) => item.Год_поверки_для_отчётов,
                        (Meter item) => item.Тип_счетчика,
                        (items) => items.Any<Meter>() ? items.Count<Meter>() : 0,
                        (object selector) => this.MetersCountPerTypesPivot.OnlyFirst10Items.Any((SummaryInfoChildItem i) => i.Value == selector.ToString()));

                    this.MetersCountPerCategoryForReportPivot = this._data.Meters
                        .ToPivotTable(
                        (Meter item) => item.Тип_населённого_пункта,
                        (Meter item) => item.Группа_счётчика_для_отчётов,
                        (items) => items.Any<Meter>() ? items.Count<Meter>() : 0, 
                        null);

                    this.MetersCountPerLocalityPivot = this._data.Meters
                        .ToPivotTable(
                            (Meter item) => item.Принцип,
                            (Meter item) => item.Населённый_пункт, 
                            (items) => items.Any<Meter>() ? items.Count<Meter>() : 0, 
                            null);
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
