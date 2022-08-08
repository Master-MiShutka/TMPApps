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
    using TMPApplication.CustomWpfWindow;
    using TMPApplication.WpfDialogs.Contracts;
    using TMP.UI.WPF.Controls.TableView;
    using TMP.Work.Emcos.Model.Balance;
    using TMP.Shared.Commands;

    /// <summary>
    /// Interaction logic for BalanceSubstationView.xaml
    /// </summary>
    public partial class BalanceSubstationView : WindowWithDialogs
    {
        private IBalance _balance;

        private Task _task;

        public BalanceSubstationView(IBalance balance)
        {
            _balance = balance;

            _task = new Task(() =>
            {
                //System.Threading.Thread.Sleep(5000);
                table.Dispatcher.BeginInvoke(
                    new Action(delegate ()
                    {
                        table.BeginInit();
                        Create_tableColumns();
                        //ate = State.Idle;
                        table.ItemsSource = _balance.Items;
                        table.EndInit();
                        IsBusy = false;
                    }));
            });

            _task.ContinueWith((t) =>
            {
                //State = State.Idle;
                EmcosSiteWrapperApp.LogError("Просмотр баланса подстанции - ошибка: " + t.Exception.Message);
                EmcosSiteWrapperApp.ShowError("Произошла ошибка.\n" + t.Exception.Message);
            }, TaskContinuationOptions.OnlyOnFaulted);

            InitializeComponent();
            
            _task.Start();

            DataContext = this;
            rootGrid.DataContext = _balance;

            _balance.PropertyChanged += Balance_PropertyChanged;
        }

        private void Balance_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
                //State = State.Busy;
                var substation = _balance.Substation.Copy() as Model.Balance.Substation;
                substation.ClearData();

                Utils.GetArchiveDataForSubstation(_balance.Period.StartDate, _balance.Period.EndDate, substation, cts, UpdateCallBack);

                foreach (Model.Balance.IBalanceItem i in substation.Items)
                {
                    i.SetSubstation(substation);
                }

                _balance.Initialize(_balance.Period);
                DispatcherExtensions.InUi(() =>
                  {
                      Create_tableColumns();
                      rootGrid.DataContext = _balance;
                      table.ItemsSource = _balance.Items;
                      //Progress = 0;
                  });

                //State = State.Idle;
            });
        }

        private void UpdateCallBack(int current, int total)
        {
            DispatcherExtensions.InUi(() =>
                  {
                      //Progress = 100 * current / total;
                  });
        }
        private void ShowItemDetails(Model.Balance.IBalanceItem item)
        {
            if (item == null)
                return;
            var biv = new BalanceItemView(_balance.Period);
            biv.Owner = this;
            biv.ShowInTaskbar = false;
            biv.DataContext = item;
            biv.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
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
                Title = _balance.Headers[headerIndex++].Name,
                ContextBinding = new Binding("Date"),
                CellTemplate = textCell
            });
            fixedColumnCount++;
            columns.Add(new TableViewColumn
            {
                Title = _balance.Headers[headerIndex].Name,
                ContextBinding = new Binding("VvodaIn"),
                Width = 110,
                TotalInfo = new TableViewColumnTotal(
                        _balance.Min[headerIndex],
                        _balance.Max[headerIndex],
                        _balance.Sum[headerIndex],
                        _balance.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            columns.Add(new TableViewColumn
            {
                Title = _balance.Headers[headerIndex].Name,
                ContextBinding = new Binding("VvodaOut"),
                Width = 85,
                TotalInfo = new TableViewColumnTotal(
                        _balance.Min[headerIndex],
                        _balance.Max[headerIndex],
                        _balance.Sum[headerIndex],
                        _balance.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            if (_balance.AuxCount != 0)
            {
                columns.Add(new TableViewColumn
                {
                    Title = _balance.Headers[headerIndex++].Name,
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
                Title = _balance.Headers[headerIndex].Name,
                ContextBinding = new Binding("FideraIn"),
                Width = 105,
                TotalInfo = new TableViewColumnTotal(
                        _balance.Min[headerIndex],
                        _balance.Max[headerIndex],
                        _balance.Sum[headerIndex],
                        _balance.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            columns.Add(new TableViewColumn
            {
                Title = _balance.Headers[headerIndex].Name,
                ContextBinding = new Binding("FideraOut"),
                Width = 85,
                TotalInfo = new TableViewColumnTotal(
                        _balance.Min[headerIndex],
                        _balance.Max[headerIndex],
                        _balance.Sum[headerIndex],
                        _balance.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            columns.Add(new TableViewColumn
            {
                Title = _balance.Headers[headerIndex].Name,
                ContextBinding = new Binding("Unbalance"),
                Width = 85,
                TotalInfo = new TableViewColumnTotal(
                        _balance.Min[headerIndex],
                        _balance.Max[headerIndex],
                        _balance.Sum[headerIndex],
                        _balance.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            headerIndex++;
            columns.Add(new TableViewColumn
            {
                Title = _balance.Headers[headerIndex].Name,
                ContextBinding = new Binding("PercentageOfUnbalance"),
                Width = 85,
                TotalInfo = new TableViewColumnTotal(
                        _balance.Min[headerIndex],
                        _balance.Max[headerIndex],
                        _balance.Sum[headerIndex],
                        _balance.Average[headerIndex]),
                CellTemplate = numbersCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            fixedColumnCount++;
            table.FixedColumnCount = fixedColumnCount;
            headerIndex++;
            for (int index = 0; index < _balance.TransformersCount * 2; index++)
            {
                columns.Add(new TableViewColumn
                {
                    Title = _balance.Headers[headerIndex].Name,
                    ContextBinding = new Binding("Transformers[" + index + "]"),
                    TotalInfo = new TableViewColumnTotal(
                        _balance.Min[headerIndex],
                        _balance.Max[headerIndex],
                        _balance.Sum[headerIndex],
                        _balance.Average[headerIndex]),
                    CellTemplate = numbersCell,
                    UseHistogramm = true,
                    ShowHistogramm = false
                });
                headerIndex++;
            }
            for (int index = 0; index < _balance.AuxCount; index++)
            {
                columns.Add(new TableViewColumn
                {
                    Title = _balance.Headers[headerIndex].Name,
                    ContextBinding = new Binding("Auxiliary[" + index + "]"),
                    TotalInfo = new TableViewColumnTotal(
                        _balance.Min[headerIndex],
                        _balance.Max[headerIndex],
                        _balance.Sum[headerIndex],
                        _balance.Average[headerIndex]),
                    UseHistogramm = true,
                    ShowHistogramm = false,
                    CellTemplate = numbersCell
                });
                headerIndex++;
            }
            for (int index = headerIndex; index < _balance.Headers.Count; index++)
            {
                var column = new TableViewColumn
                {
                    Title = _balance.Headers[index].Name,
                    ContextBinding = new Binding("Fiders[" + (index - headerIndex) + "]"),
                    TotalInfo = new TableViewColumnTotal(
                        _balance.Min[index],
                        _balance.Max[index],
                        _balance.Sum[index],
                        _balance.Average[index]),
                        CellTemplate = numbersCell,
                    UseHistogramm = true,
                    ShowHistogramm = true
                };
                if (_balance != null && _balance.Substation != null && _balance.Substation.Items != null)
                {
                    var bi = _balance.Substation.Items.Where(c => c.ElementType == Model.ElementTypes.FIDER && c.Code == _balance.Headers[index].Code).FirstOrDefault();
                    if (bi != null)
                    {
                        column.Tag = bi;
                        var menuItem = new MenuItem
                        {
                            Header = String.Format("Подробно о {0} ...", bi.Name),
                            Tag = bi,
                            Command = new DelegateCommand(i =>
                            {
                                if (i is IBalanceItem item) ShowItemDetails(item);
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

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            //State = State.Busy;
            //wait.Message = "Пожалуйста, подождите..\nПодготовка отчёта...";
            var task = new System.Threading.Tasks.Task(() =>
            {
                var reportFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _balance.Substation.Name + ".xlsx");
                int index = 0;
                while (System.IO.File.Exists(reportFileName) == true)
                {
                    try
                    {
                        System.IO.File.Delete(reportFileName);
                    }
                    catch (System.IO.IOException)
                    {
                        reportFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _balance.Substation.Name + " " + ++index + ".xlsx");
                    }
                }
                using (var sbe = new Export.SubstationExport(_balance))
                    sbe.Export(reportFileName);

                System.Diagnostics.Process.Start(reportFileName);
                //State = State.Idle;
                //DispatcherExtensions.InUi(() => wait.Message = "Пожалуйста, подождите..\nПодготовка данных.");
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
                EmcosSiteWrapperApp.LogError("Экспорт балансов подстанций - ошибка: " + sb.ToString());
                DispatcherExtensions.InUi(() => EmcosSiteWrapperApp.ShowError("Произошла ошибка при формировании отчёта.\nОбратитесь к разработчику."));
            }, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            task.Start(System.Threading.Tasks.TaskScheduler.Current);
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            //wait.Message = "Пожалуйста, подождите ...\nОбновление данных.";
            var cts = new System.Threading.CancellationTokenSource();

            var task = EmcosSiteWrapper.Instance.ExecuteAction(cts, () =>
            {
                //State = State.Busy;
                _balance.Substation.ClearData();

                Emcos.Utils.GetArchiveDataForSubstation(_balance.Period.StartDate, _balance.Period.EndDate, _balance.Substation, cts, UpdateCallBack);

                _balance.Initialize(_balance.Period);

                DispatcherExtensions.InUi(() =>
                  {
                      rootGrid.DataContext = _balance;
                      //Progress = 0;
                      //wait.Message = "Пожалуйста, подождите ...\nПодготовка данных.";
                  });

                //State = State.Idle;
            });
        }
    }
}
