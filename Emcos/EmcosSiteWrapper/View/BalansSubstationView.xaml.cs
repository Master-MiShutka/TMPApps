using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TMP.Work.Emcos.View
{
    using TMPApplication;
    using TMP.Wpf.Common.Controls.TableView;
    /// <summary>
    /// Interaction logic for BalansSubstationView.xaml
    /// </summary>
    public partial class BalansSubstationView : Window, IStateObject
    {
        #region | Реализация IStateObject |
        public string Log { get { return null; } set { throw new NotImplementedException(); } }
        private int _progress = 0;
        public int Progress {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged("Progress");
            }
        }
        private State _state = State.Busy;
        public State State { get { return _state; }
            set {
                _state = value;
                RaisePropertyChanged("State");
            }
        }

        #endregion

        #region | Реализация INotifyPropertyChanged |

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private Model.BalansGrop _balansGroup;

        private Task _task;

        public BalansSubstationView(Model.BalansGrop balansGroup)
        {
            _task = new Task(() =>
            {
                //System.Threading.Thread.Sleep(5000);
                table.Dispatcher.BeginInvoke(
                    new Action(delegate ()
                    {
                        Create_tableColumns();
                        State = State.Idle;
                        table.ItemsSource = _balansGroup.Items;
                        table.EndInit();
                    }));
            });

            _task.ContinueWith((t) =>
            {
                State = State.Idle;
                App.LogError("Просмотр баланса подстанции - ошибка: " + t.Exception.Message);
                App.ShowError("Произошла ошибка.\n" + t.Exception.Message);
            }, TaskContinuationOptions.OnlyOnFaulted);

            _balansGroup = balansGroup;

            InitializeComponent();

            State = State.Busy;
            table.BeginInit();
            _task.Start();

            DataContext = this;
            rootGrid.DataContext = _balansGroup;

            _balansGroup.PropertyChanged += _balansGroup_PropertyChanged;
        }

        private void _balansGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Period")
            {
                UpdateData();
            }
        }

        private void UpdateData()
        {
            rootGrid.DataContext = null;
            // обновление данных согласно нового временного периода
            var cts = new System.Threading.CancellationTokenSource();

            var task = EmcosSiteWrapper.Instance.ExecuteAction(cts, () =>
            {
                State = State.Busy;
                var substation = _balansGroup.Substation.Copy() as Model.Balans.Substation;

            U.GetDaylyArchiveDataForSubstation(_balansGroup.Period.StartDate, _balansGroup.Period.EndDate, substation, cts, UpdateCallBack);

                foreach (Model.Balans.IBalansItem i in substation.Items)
                {
                    i.SetSubstation(substation);
                }

                _balansGroup.PropertyChanged -= _balansGroup_PropertyChanged;
                _balansGroup = new Model.BalansGrop(substation, _balansGroup.Period);
                _balansGroup.PropertyChanged += _balansGroup_PropertyChanged;

                DispatcherExtensions.InUi(() =>
                  {
                      Create_tableColumns();
                      rootGrid.DataContext = _balansGroup;
                      table.ItemsSource = _balansGroup.Items;
                      Progress = 0;
                  });

                State = State.Idle;
            });
        }

        private void UpdateCallBack(int current, int total)
        {
            DispatcherExtensions.InUi(() =>
                  {
                      Progress = 100 * current / total;
                  });
        }
        private void ShowItemDetails(Model.Balans.IBalansItem item)
        {
            if (item == null)
                return;
            var biv = new BalansItemView(_balansGroup.Period);
            biv.Owner = this;
            biv.ShowInTaskbar = false;
            biv.DataContext = item;
            biv.ShowDialog();

            _balansGroup.UpdateSubstationData();
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
        private bool Create_tableColumns()
        {
            int fixedColumnCount = 0;

            table.Columns.Clear();
            var textCell = this.TryFindResource("textTableViewCellTemplate") as DataTemplate;
            if (textCell == null)
                throw new NullReferenceException("Не найден шаблон: textTableViewCellTemplate");
            var numbersCell = this.TryFindResource("numberTableViewCellTemplate") as DataTemplate;
            if (numbersCell == null)
                throw new NullReferenceException("Не найден шаблон: numberTableViewCellTemplate");

            var columnTitleTemplate = this.TryFindResource("tableViewColumnHeaderDataTemplate") as DataTemplate;
            if (columnTitleTemplate == null)
                throw new NullReferenceException("Не найден шаблон: tableViewColumnHeaderDataTemplate");

            var columns = new System.Collections.ObjectModel.ObservableCollection<TableViewColumn>();

            int headerIndex = 0;
            columns.Add(new TableViewColumn
            {
                Title = _balansGroup.Headers[headerIndex++].Name,
                ContextBinding = new Binding("Date"),
                CellTemplate = textCell
            });
            fixedColumnCount++;
            columns.Add(new TableViewColumn
            {
                Title = _balansGroup.Headers[headerIndex].Name,
                ContextBinding = new Binding("VvodaIn"),
                Width = 110,
                TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[headerIndex],
                        _balansGroup.Max[headerIndex],
                        _balansGroup.Sum[headerIndex],
                        _balansGroup.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            columns.Add(new TableViewColumn
            {
                Title = _balansGroup.Headers[headerIndex].Name,
                ContextBinding = new Binding("VvodaOut"),
                Width = 85,
                TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[headerIndex],
                        _balansGroup.Max[headerIndex],
                        _balansGroup.Sum[headerIndex],
                        _balansGroup.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            if (_balansGroup.AuxCount != 0)
            {
                columns.Add(new TableViewColumn
                {
                    Title = _balansGroup.Headers[headerIndex++].Name,
                    ContextBinding = new Binding("Tsn"),
                    Width = 70,
                    CellTemplate = numbersCell,
                    UseHistogramm = true,
                    ShowHistogramm = true,
                });
                fixedColumnCount++;
            }
            columns.Add(new TableViewColumn
            {
                Title = _balansGroup.Headers[headerIndex].Name,
                ContextBinding = new Binding("FideraIn"),
                Width = 105,
                TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[headerIndex],
                        _balansGroup.Max[headerIndex],
                        _balansGroup.Sum[headerIndex],
                        _balansGroup.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            columns.Add(new TableViewColumn
            {
                Title = _balansGroup.Headers[headerIndex].Name,
                ContextBinding = new Binding("FideraOut"),
                Width = 85,
                TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[headerIndex],
                        _balansGroup.Max[headerIndex],
                        _balansGroup.Sum[headerIndex],
                        _balansGroup.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            columns.Add(new TableViewColumn
            {
                Title = _balansGroup.Headers[headerIndex].Name,
                ContextBinding = new Binding("Unbalance"),
                Width = 85,
                TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[headerIndex],
                        _balansGroup.Max[headerIndex],
                        _balansGroup.Sum[headerIndex],
                        _balansGroup.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            columns.Add(new TableViewColumn
            {
                Title = _balansGroup.Headers[headerIndex].Name,
                ContextBinding = new Binding("PercentageOfUnbalance"),
                Width = 85,
                TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[headerIndex],
                        _balansGroup.Max[headerIndex],
                        _balansGroup.Sum[headerIndex],
                        _balansGroup.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            table.FixedColumnCount = fixedColumnCount;
            headerIndex++;
            for (int index = 0; index < _balansGroup.TransformersCount * 2; index++)
            {
                columns.Add(new TableViewColumn
                {
                    Title = _balansGroup.Headers[headerIndex].Name,
                    ContextBinding = new Binding("Transformers[" + index + "]"),
                    TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[headerIndex],
                        _balansGroup.Max[headerIndex],
                        _balansGroup.Sum[headerIndex],
                        _balansGroup.Average[headerIndex]),
                    CellTemplate = numbersCell,
                    UseHistogramm = true,
                    ShowHistogramm = false
                });
                headerIndex++;
            }
            for (int index = 0; index < _balansGroup.AuxCount; index++)
            {
                columns.Add(new TableViewColumn
                {
                    Title = _balansGroup.Headers[headerIndex].Name,
                    ContextBinding = new Binding("Auxiliary[" + index + "]"),
                    TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[headerIndex],
                        _balansGroup.Max[headerIndex],
                        _balansGroup.Sum[headerIndex],
                        _balansGroup.Average[headerIndex]),
                    UseHistogramm = true,
                    ShowHistogramm = false,
                    CellTemplate = numbersCell
                });
                headerIndex++;
            }
            for (int index = headerIndex; index < _balansGroup.Headers.Count; index++)
            {
                var column = new TableViewColumn
                {
                    Title = _balansGroup.Headers[index].Name,
                    ContextBinding = new Binding("Fiders[" + (index - headerIndex) + "]"),
                    TotalInfo = new TableViewColumnTotal(
                        _balansGroup.Min[index],
                        _balansGroup.Max[index],
                        _balansGroup.Sum[index],
                        _balansGroup.Average[index]),
                        CellTemplate = numbersCell,
                    UseHistogramm = true,
                    ShowHistogramm = true
                };
                if (_balansGroup != null && _balansGroup.Substation != null && _balansGroup.Substation.Items != null)
                {
                    var bi = _balansGroup.Substation.Items.Where(c => c.Type == Model.ElementTypes.FIDER && c.Code == _balansGroup.Headers[index].Code).FirstOrDefault();
                    if (bi != null)
                    {
                        column.Tag = bi;
                        var menuItem = new MenuItem
                        {
                            Header = String.Format("Подробно о {0} ...", bi.Name),
                            Tag = bi,
                            Command = new Wpf.Common.DelegateCommand<Model.Balans.IBalansItem>(i =>
                            {
                                if (i != null) ShowItemDetails(i);
                            }),
                            CommandParameter = bi
                        };
                        if (column.ContextMenu != null && column.ContextMenu.Items != null)
                            column.ContextMenu.Items.Add(menuItem);
                        else
                        {
                            column.ContextMenu = new ContextMenu();
                            column.ContextMenu.Items.Add(menuItem);
                        }
                    }
                }
                columns.Add(column);
            }

            foreach (TableViewColumn column in columns)
                column.TitleTemplate = columnTitleTemplate;
            table.Columns = columns;

            return true;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            ;
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            State = State.Busy;
            wait.Message = "Пожалуйста, подождите..\nПодготовка отчёта...";
            var task = new System.Threading.Tasks.Task(() =>
            {
                var reportFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _balansGroup.SubstationName + ".xlsx");
                int index = 0;
                while (System.IO.File.Exists(reportFileName) == true)
                {
                    try
                    {
                        System.IO.File.Delete(reportFileName);
                    }
                    catch (System.IO.IOException)
                    {
                        reportFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _balansGroup.SubstationName + " " + ++index + ".xlsx");
                    }
                }
                using (var sbe = new Export.SubstationExport(_balansGroup))
                    sbe.Export(reportFileName);

                System.Diagnostics.Process.Start(reportFileName);
                State = State.Idle;
                DispatcherExtensions.InUi(() => wait.Message = "Пожалуйста, подождите..\nПодготовка данных.");
            }, System.Threading.Tasks.TaskCreationOptions.AttachedToParent);


            task.ContinueWith((s) =>
            {
                var sb = new System.Text.StringBuilder();
                Exception ex = s.Exception.Flatten();
                while (ex != null)
                    if (ex.InnerException != null)
                    {
                        sb.AppendLine(ex.InnerException.Message);
                        ex = ex.InnerException;
                    }
                App.LogError("Экспорт балансов подстанций - ошибка: " + sb.ToString());
                DispatcherExtensions.InUi(() => App.ShowError("Произошла ошибка при формировании отчёта.\nОбратитесь к разработчику."));
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            task.Start(System.Threading.Tasks.TaskScheduler.Current);
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            wait.Message = "Пожалуйста, подождите ...\nОбновление данных.";
            var cts = new System.Threading.CancellationTokenSource();

            var task = EmcosSiteWrapper.Instance.ExecuteAction(cts, () =>
            {
                State = State.Busy;

                U.GetDaylyArchiveDataForSubstation(_balansGroup.Period.StartDate, _balansGroup.Period.EndDate, _balansGroup.Substation, cts, UpdateCallBack);

                _balansGroup.PropertyChanged -= _balansGroup_PropertyChanged;
                _balansGroup = new Model.BalansGrop(_balansGroup.Substation, _balansGroup.Period);
                _balansGroup.PropertyChanged += _balansGroup_PropertyChanged;

                DispatcherExtensions.InUi(() =>
                  {
                      rootGrid.DataContext = _balansGroup;
                      Progress = 0;
                      wait.Message = "Пожалуйста, подождите ...\nПодготовка данных.";
                  });

                State = State.Idle;
            });
        }
    }
}
