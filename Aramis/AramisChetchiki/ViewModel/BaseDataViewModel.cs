namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using DataGridWpf;
    using ItemsFilter.Model;
    using TMP.Extensions;
    using TMP.Shared;
    using TMP.Shared.Commands;
    using TMP.WORK.AramisChetchiki.Extensions;
    using TMP.WORK.AramisChetchiki.Model;

    [DataContract]
    public class BaseDataViewModel<T> : BaseViewModel, IDataViewModel<T>
        where T : IModel
    {
        private IEnumerable<T> data;
        private ICollectionView view;

        private int collectionViewItemsCount;

        private bool isViewBuilding;

        private TableViewKinds selectedViewKind = AppSettings.Default.SelectedTableViewKind;
        private ObservableCollection<DataGridWpfColumnViewModel> tableColumns;
        private string textToFind;

        private IEnumerable<PlusPropertyDescriptor> propertyDescriptors;

        protected const string NOITEMSSTRING = "ничего нет";
        private string noItemsMessage = NOITEMSSTRING;
        private ObservableCollection<ItemsFilter.Model.IFilter> filters = new ObservableCollection<ItemsFilter.Model.IFilter>();
        protected const string none = "(нет)";

        private readonly Dictionary<string, Dictionary<ItemsFilter.Model.IFilter, string>> selectedFilters = new();
        private readonly string ACTIVEFILTERLISTSEPARATOR = "; ";

        private FiltersWindow fw;
        private readonly FilterViewModel filterViewModel = new();

        private string activeFiltersList;

        public BaseDataViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            this.IsBusy = true;
            this.Status = "загрузка ...";

            // fix
            var v = this.View;

            this.CommandExport = new DelegateCommand(this.DoExport, this.CanExecuteCheckView);
            this.CommandPrint = new DelegateCommand(this.DoPrint, this.CanExecuteCheckView);
            this.CommandViewDetailsBySelectedItem = new DelegateCommand<object>(this.DoViewDetailsBySelectedItem);
            this.CommandChangeViewKind = new DelegateCommand<TableViewKinds>(this.DoChangeViewKind, this.CanExecuteCheckData);
            this.CommandSetSorting = new DelegateCommand(this.DoSetSorting, this.CanExecuteCheckData);
            this.SetupTableViewKinds = new DelegateCommand<object>(this.DoSetupTableViewKinds, this.CanExecuteCheckData);
            this.SaveCurrentTableViewKindAsNew = new DelegateCommand(this.DoSaveCurrentTableViewKindAsNew, this.CanExecuteCheckData);
            this.CommandShowFilters = new DelegateCommand(this.DoShowFilters, this.CanExecuteCheckView);

            this.CommandCleanFilters = new DelegateCommand(this.DoCleanFilters, this.CanExecuteCleanFilters);

            // загрузка сохраненных колонок таблицы
            this.TryLoadTableColumnsFromSettings();

            // окно фильтров
            this.fw = new FiltersWindow() { Owner = App.Current.MainWindow, DataContext = this.filterViewModel };

            ItemsFilter.FilterListExtensions.FiltersChanged += this.FilterListExtensions_FiltersChanged;

            this.RaisePropertyChanged(nameof(this.View));
            this.RaisePropertyChanged(nameof(this.Data));
        }

        public BaseDataViewModel(IEnumerable<T> data = null)
            : this()
        {
            this.data = typeof(T) == typeof(Meter) && data == null ? MainViewModel.Data?.Meters?.Cast<T>().ToList() : data;
            this.RaisePropertyChanged(nameof(this.View));
            this.RaisePropertyChanged(nameof(this.Data));
        }

        protected override void OnDispose()
        {
            ItemsFilter.FilterListExtensions.FiltersChanged -= this.FilterListExtensions_FiltersChanged;

            ItemsFilter.FiltersManager.RemoveCollectionView(this.view);
            if (this.FilterPresenter != null)
            {
                this.FilterPresenter.Filtered -= this.CollectionFiltered;
            }

            if (this.Data != null)
            {
                if (this.Data is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            if (this.fw != null && this.fw.IsVisible)
            {
                this.fw.Close();
            }

            if (this.filterViewModel != null & this.filterViewModel.FilterPresenter != null)
            {
                this.filterViewModel.FilterPresenter.Filtered -= this.CollectionFiltered;
            }

            this.view = null;

            byte[] bytes = Common.RepositoryCommon.MessagePackSerializer.ToBytes<ObservableCollection<DataGridWpfColumnViewModel>>(this.TableColumns);
            string rawdata = Convert.ToBase64String(bytes);
            string thisHashCode = this.GetHashCode().ToString();

            if (AppSettings.Default.ViewModelsTableColumns.ContainsKey(thisHashCode))
            {
                AppSettings.Default.ViewModelsTableColumns[thisHashCode] = rawdata;
            }
            else
            {
                if (this.TableColumns != null && this.TableColumns.Count > 0)
                {
                    AppSettings.Default.ViewModelsTableColumns.Add(thisHashCode, rawdata);
                }
            }
            AppSettings.Default.Save();
        }

        protected override void OnDataLoaded()
        {
            base.OnDataLoaded();

            this.view = null;
            this.RaisePropertyChanged(nameof(this.View));

            this.RaisePropertyChanged(nameof(this.Data));
        }

        public void SortAndGroupByField(string fieldName)
        {
            IEnumerable<string> fieldDisplayNames = from prop in this.PropertyDescriptors
                                                    where prop.Name == fieldName
                                                    select prop.DisplayName;

            if (fieldDisplayNames.Any())
            {
                using (this.view.DeferRefresh())
                {
                    this.view.GroupDescriptions.Add(new PropertyGroupDescription(fieldName));
                    this.view.SortDescriptions.Add(new SortDescription(fieldName, ListSortDirection.Ascending));
                    Utils.GroupingFields = fieldDisplayNames.First();
                }
            }
        }

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("C78578C1-C425-44EF-9EA6-5F93CE44F999");
            return guid.GetHashCode();
        }

        #region Propperties

        /// <summary>
        /// Команда настройки вариантов отображения
        /// </summary>
        public ICommand SetupTableViewKinds { get; private set; }

        /// <summary>
        /// Команда сохранения текущего вида
        /// </summary>
        public ICommand SaveCurrentTableViewKindAsNew { get; private set; }

        public ICommand CommandViewDetailsBySelectedItem { get; private set; }

        /// <summary>
        /// Команда выбора вида отображения данных
        /// </summary>
        public ICommand CommandChangeViewKind { get; protected set; }

        /// <summary>
        /// Команда отображения списка фильтров
        /// </summary>
        public ICommand CommandShowFilters { get; private set; }

        /// <summary>
        /// Команда удаления примененных фильтров
        /// </summary>
        public ICommand CommandCleanFilters { get; private set; }

        /// <summary>
        /// Заголовок отчета
        /// </summary>
        public virtual string ReportTitle { get; } = "Экспорт информации";

        /// <summary>
        /// Описание отчета
        /// </summary>
        public virtual string ReportDescription { get; } = string.Empty;

        /// <summary>
        /// Функция возвращаюшая
        /// </summary>
        public virtual Func<IModel, string, string, object> GetValueDelegate => ModelHelper.GetPropertyValue;

        /// <summary>
        /// Перечеь свойств
        /// </summary>
        public virtual IEnumerable<PlusPropertyDescriptor> PropertyDescriptors => this.propertyDescriptors ??= ModelHelper.GetPropertyDescriptors(typeof(T), this.SelectedViewKind);

        public virtual Predicate<T> DataFilter => (i) => true;

        /// <summary>
        /// Коллекция данных
        /// </summary>
        public IEnumerable<T> Data
        {
            get => this.data?.Where(i => this.DataFilter(i));
            protected set
            {
                if (this.SetProperty(ref this.data, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsDataNullOrEmpty));
                    this.RaisePropertyChanged(nameof(this.View));
                    this.RaisePropertyChanged(nameof(this.ItemsCount));
                    this.RaisePropertyChanged(nameof(this.PercentOfTotal));
                    this.RaisePropertyChanged(nameof(this.IsFilteredItems));

                    (this.CommandChangeViewKind as DelegateCommand<TableViewKinds>)?.RaiseCanExecuteChanged();
                    (this.CommandSetSorting as DelegateCommand)?.RaiseCanExecuteChanged();
                    (this.SetupTableViewKinds as DelegateCommand)?.RaiseCanExecuteChanged();
                    (this.SaveCurrentTableViewKindAsNew as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsDataNullOrEmpty => this.Data == null ? false : this.Data.Any();

        /// <summary>
        /// Представление коллекции
        /// </summary>
        public virtual ICollectionView View
        {
            get
            {
                if (this.Data == null)
                {
                    this.view = null;
                }
                else
                {
                    if (this.view == null && this.isViewBuilding == false)
                    {
                        this.isViewBuilding = true;

                        // Mouse.OverrideCursor = Cursors.Wait;

                        this.view = this.BuildAndGetView();

                        App.InvokeInUIThread(() =>
                        {
                            (this.CommandExport as DelegateCommand)?.RaiseCanExecuteChanged();
                            (this.CommandPrint as DelegateCommand)?.RaiseCanExecuteChanged();
                            (this.CommandShowFilters as DelegateCommand)?.RaiseCanExecuteChanged();
                        });

                        if (this.view != null)
                        {
                            if (this.FilterPresenter != null)
                            {
                                this.FilterPresenter.Filtered -= this.CollectionFiltered;
                            }

                            this.FilterPresenter = ItemsFilter.FiltersManager.TryGetFilterPresenter(this.view);
                            this.FilterPresenter.Filtered += this.CollectionFiltered;

                            this.filterViewModel.CollectionView = this.view;

                            (this.view as INotifyPropertyChanged).PropertyChanged += this.View_PropertyChanged;

                            this.view.Refresh();

                            this.view.Filter = this.Filter;
                        }

                        // Mouse.OverrideCursor = null;
                        this.isViewBuilding = false;

                        this.OnViewBuilded();
                    }
                }

                return this.view;
            }

            // protected set => SetProperty(ref _view, value);
        }

        /// <summary>
        /// Исходное количество элементов
        /// </summary>
        public int ItemsCount => this.Data == null ? 0 : this.Data.Count();

        /// <summary>
        /// Отображаемое количество элементов
        /// </summary>
        public int CollectionViewItemsCount
        {
            get => this.collectionViewItemsCount;
            set
            {
                if (this.SetProperty(ref this.collectionViewItemsCount, value))
                {
                    this.RaisePropertyChanged(nameof(this.PercentOfTotal));
                    this.RaisePropertyChanged(nameof(this.IsFilteredItems));

                    if (this.collectionViewItemsCount == 0)
                    {
                        this.NoItemsMessage = "нет данных\nизмените условия фильтров";
                    }
                    else
                    {
                        this.NoItemsMessage = NOITEMSSTRING;
                    }
                }
            }
        }

        /// <summary>
        /// Процент отображаемых данных
        /// </summary>
        public double PercentOfTotal => this.ItemsCount == 0 ? 0d : 100d * this.CollectionViewItemsCount / this.ItemsCount;

        /// <summary>
        /// Указывает отфильтрованы ли данные
        /// </summary>
        public bool IsFilteredItems => this.collectionViewItemsCount < this.ItemsCount;

        /// <summary>
        /// Выбранный вид отображения данных
        /// </summary>
        public TableViewKinds SelectedViewKind
        {
            get => this.selectedViewKind;
            set
            {
                TableViewKinds oldValue = this.selectedViewKind;
                if (this.SetProperty(ref this.selectedViewKind, value))
                {
                    this.RaisePropertyChanged(nameof(this.TableColumns));
                    this.SetTableColumnsVisibility(oldValue, false);

                    this.SetTableColumnsVisibility(this.selectedViewKind, true);

                    // TableColumns = new ObservableCollection<DataGridWpfColumnViewModel>(values.Item1);
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(this.PropertyDescriptors));

                    // RaisePropertyChanged(nameof(TableColumns));
                    AppSettings.Default.SelectedTableViewKind = this.selectedViewKind;
                    AppSettings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Коллекция столбцов таблицы
        /// </summary>
        public ObservableCollection<DataGridWpfColumnViewModel> TableColumns
        {
            get
            {
                if (this.tableColumns == null)
                {
                    IEnumerable<PlusPropertyDescriptor> propertyDescriptors = ModelHelper.GetPropertyDescriptors(typeof(T));
                    Tuple<IEnumerable<DataGridWpfColumnViewModel>, List<string>> values = Utils.GenerateColumns(propertyDescriptors);

                    if (values.Item1 == null)
                    {
                        return null;
                    }

                    this.tableColumns = new ObservableCollection<DataGridWpfColumnViewModel>(values.Item1);

                    if (typeof(T) == typeof(Meter))
                    {
                        this.tableColumns.AsParallel().ForAll(cvm => cvm.IsVisible = false);

                        this.UpdateTableColumnsVisibility();
                    }
                }

                return this.tableColumns;
            }
            private set => this.SetProperty(ref this.tableColumns, value);
        }

        /// <summary>
        /// Вохвращает представление коллекции
        /// </summary>
        /// <returns></returns>
        protected virtual ICollectionView BuildAndGetView()
        {
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.Data);
            return collectionView;
        }

        /// <summary>
        /// Текст для поиска
        /// </summary>
        public string TextToFind
        {
            get => this.textToFind;
            set
            {
                this.SetProperty(ref this.textToFind, value);
                this.view?.Refresh();
            }
        }

        public string NoItemsMessage { get => this.noItemsMessage; protected set => this.SetProperty(ref this.noItemsMessage, value); }

        /// <summary>
        /// Коллекция дополнительных фильтров
        /// </summary>
        public ICollection<ItemsFilter.Model.IFilter> Filters => this.filters;

        public string ActiveFiltersList
        {
            get
            {
                if (this.activeFiltersList == null)
                {
                    Dictionary<string, string> filtersDictonary = new();

                    foreach (IFilter filter in this.filters)
                    {
                        if (filter.IsActive)
                        {
                            string name = (filter is IPropertyFilter propertyFilter) ? propertyFilter.PropertyInfo.Name : filter.Name;
                            name = Utils.ConvertFromTitleCase(name);

                            string value = string.Empty;
                            switch (filter)
                            {
                                case ItemsFilter.Model.IStringFilter:
                                    value = (filter as ItemsFilter.Model.IStringFilter).Value;
                                    break;
                                case ItemsFilter.Model.IMultiValueFilter mf:
                                    value = string.Join(", ", mf.SelectedValues);
                                    break;
                                default:
                                    break;
                            }

                            if (filtersDictonary.ContainsKey(name) == false)
                            {
                                filtersDictonary.Add(name, value);
                            }
                        }
                    }

                    System.Text.StringBuilder stringBuilder = new(1_000);
                    int count = filtersDictonary.Count;
                    int index = 0;
                    foreach (KeyValuePair<string, string> item in filtersDictonary)
                    {
                        if (index != count - 1)
                        {
                            stringBuilder.AppendFormat(AppSettings.CurrentCulture, "{0}: '{1}'{2}", item.Key, item.Value, this.ACTIVEFILTERLISTSEPARATOR);
                        }
                        else
                        {
                            stringBuilder.AppendFormat(AppSettings.CurrentCulture, "{0}: '{1}'. ", item.Key, item.Value);
                        }

                        index++;
                    }

                    if (this.selectedFilters.Count > 0)
                    {
                        count = this.selectedFilters.Count;
                        index = 0;

                        foreach (KeyValuePair<string, Dictionary<IFilter, string>> item in this.selectedFilters)
                        {
                            string propertyName = item.Key;
                            int childCount = item.Value.Count;
                            int childIndex = 0;

                            foreach (KeyValuePair<IFilter, string> child in item.Value)
                            {
                                if (childIndex != childCount - 1)
                                {
                                    stringBuilder.AppendFormat(AppSettings.CurrentCulture, "{0}: {1}{2}", propertyName, child.Value, this.ACTIVEFILTERLISTSEPARATOR);
                                }
                                else
                                {
                                    stringBuilder.AppendFormat(AppSettings.CurrentCulture, "{0}: {1}", propertyName, child.Value);
                                }

                                childIndex++;
                            }

                            if (index != count - 1)
                            {
                                stringBuilder.Append(this.ACTIVEFILTERLISTSEPARATOR);
                            }

                            index++;
                        }
                    }
                    this.activeFiltersList = stringBuilder.ToString();
                }

                return this.activeFiltersList;
            }
            private set => this.SetProperty(ref this.activeFiltersList, value);
        }

        #endregion

        #region Methods

        #region CommandActions

        private void DoExport()
        {
            this.IsBusy = true;

            this.Status = "Экспорт данных";
            this.DetailedStatus = "подготовка ...";

            System.Threading.Tasks.Task task = System.Threading.Tasks.Task.Run(() =>
            {
                this.view?.Export(
                    this.GetFieldsAndFormats(),
                    this.ReportTitle,
                    this.ReportDescription,
                    this.GetValueDelegate,
                    (msg) => this.DetailedStatus = msg);
            });
            task.ContinueWith(
                t =>
                {
                    this.ShowDialogError($"Произошла ошибка при формировании отчёта.\nОписание: {App.GetExceptionDetails(t.Exception)}");
                }, this.IsBusyCancellationTokenSource.Token, System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());

            task.ContinueWith(
                t =>
                {
                    this.IsBusy = false;
                    this.Status = null;
                    this.DetailedStatus = null;
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DoPrint()
        {
            this.IsBusy = true;
            App.DoEvents();

            this.Status = "Печать данных";
            this.DetailedStatus = "подготовка ...";
            App.DoEvents();
            try
            {
                FlowDocument doc = this.GenerateFlowDocumentFromPrint();
                if (doc == null)
                {
                    return;
                }

                PrintPreviewWindow window = new PrintPreviewWindow(doc)
                {
                    Owner = App.Current.MainWindow,
                };
                window.ShowDialog();
            }
            catch (Exception e)
            {
                this.ShowDialogError($"Произошла ошибка при формировании отчёта.\nОписание: {App.GetExceptionDetails(e)}");
            }
            finally
            {
                this.IsAnalizingData = false;
                this.IsBusy = false;
                this.Status = null;
                this.DetailedStatus = null;
            }
        }

        private void DoViewDetailsBySelectedItem(object selectedItem)
        {
            if (selectedItem is not Meter meter)
            {
                return;
            }

            Controls.MeterView control = new Controls.MeterView
            {
                DataContext = new ViewModel.MeterViewViewModel(meter),
            };
            Action dialogCloseAction = this.ShowCustomDialog(control, "-= Подробная информация =-");
            control.CloseAction = dialogCloseAction;
        }

        private void DoChangeViewKind(TableViewKinds kind)
        {
            // SelectedViewKind = (TableViewKinds)Enum.Parse(typeof(TableViewKinds), kind);
            this.SelectedViewKind = kind;
        }

        private void DoSetSorting()
        {
            Controls.SelectorFieldsAndSortCollectionView control = new Controls.SelectorFieldsAndSortCollectionView(this.TableColumns, this.view);
            Action dialogCloseAction = this.ShowCustomDialog(control, "-= Выбор полей, их порядок и сортировка =-", TMPApplication.WpfDialogs.DialogMode.None);
            control.CloseAction = dialogCloseAction;
        }

        private void DoSetupTableViewKinds(object o)
        {
            Controls.SettingsPages.TablesSettings control = new Controls.SettingsPages.TablesSettings();
            Action dialogCloseAction = this.ShowCustomDialog(control, "-= Настройка полей таблиц =-");
            control.CloseAction = dialogCloseAction;
        }

        private void DoSaveCurrentTableViewKindAsNew()
        {
            this.ShowDialogInfo("Ещё не реализовано.");
        }

        private void DoShowFilters()
        {
            if (this.fw.IsVisible == false)
            {
                this.fw.Show();
            }
        }

        private void DoCleanFilters()
        {
            foreach (IFilter filter in this.filters)
            {
                if (filter.IsActive)
                {
                    filter.IsActive = false;
                }
            }

            this.RaisePropertyChanged(nameof(this.ActiveFiltersList));
        }

        protected bool CanExecuteCheckView()
        {
            return this.view != null;
        }

        protected bool CanExecuteCheckData()
        {
            return this.IsDataNullOrEmpty;
        }

        protected bool CanExecuteCheckData(object o)
        {
            return this.IsDataNullOrEmpty;
        }

        protected bool CanExecuteCheckData(TableViewKinds o)
        {
            return this.IsDataNullOrEmpty;
        }

        protected bool CanExecuteCleanFilters()
        {
            return string.IsNullOrEmpty(this.ActiveFiltersList) == false;
        }

        #endregion

        private void TryLoadTableColumnsFromSettings()
        {
            string thisHashCode = this.GetHashCode().ToString();
            if (AppSettings.Default.ViewModelsTableColumns.ContainsKey(thisHashCode))
            {
                try
                {
                    string rawdata = AppSettings.Default.ViewModelsTableColumns[thisHashCode];
                    byte[] bytes = Convert.FromBase64String(rawdata);
                    var columns = Common.RepositoryCommon.MessagePackDeserializer.FromBytes<ObservableCollection<DataGridWpfColumnViewModel>>(bytes);

                    if (columns != null)
                        this.TableColumns = columns;
                }
                catch (Exception e)
                {
                }
            }
        }

        private void SetTableColumnsVisibility(TableViewKinds selectedViewKind, bool isVisible)
        {
            IEnumerable<PlusPropertyDescriptor> propDescriptors = ModelHelper.GetPropertyDescriptors(typeof(T), selectedViewKind);
            if (propDescriptors == null)
            {
                return;
            }

            IEnumerable<string> newFieldNames = propDescriptors.Select(ppd => ppd.Name).ToList();
            List<DataGridWpfColumnViewModel> items = this.TableColumns
                .Where(cvm => newFieldNames.Contains(cvm.FieldName))
                .ToList();
            foreach (DataGridWpfColumnViewModel item in items)
            {
                item.IsVisible = isVisible;
                this.RaisePropertyChanged(nameof(this.TableColumns));
            }
        }

        private void UpdateTableColumnsVisibility()
        {
            this.SetTableColumnsVisibility(this.selectedViewKind, true);
        }

        /// <summary>
        /// Фильтрация коллекции
        /// </summary>
        /// <param name="obj">По чем фильтруем</param>
        /// <returns></returns>
        protected virtual bool Filter(object obj)
        {
            // if (Data == null) return true;
            // if (String.IsNullOrWhiteSpace(TextToFind) == false)
            // {
            //    string s = TextToFind.ToLower();
            //    SummaryInfoItem item = obj as SummaryInfoItem;
            //    if (item != null && item.FieldName.ToLower().Contains(s) || item.Header.ToLower().Contains(s))
            //        return true;
            //    return false;
            // }
            // else
            return true;
        }

        /// <summary>
        /// Возвращает словарь вида поле:форматы данных
        /// </summary>
        /// <returns></returns>
        protected Dictionary<string, DataCellFormats> GetFieldsAndFormats()
        {
            Dictionary<string, DataCellFormats> fieldsAndFormats = new Dictionary<string, DataCellFormats>();
            Application.Current.Dispatcher.Invoke(() =>
            {
                IOrderedEnumerable<DataGridWpfColumnViewModel> columns = this.TableColumns
                    .Where(c => c.IsVisible)
                    .OrderBy(c => c.DisplayIndex);
                var columnsFields = columns.Select(c => new
                {
                    Key = c.FieldName,
                    ContentFormat = c.CellContentStringFormat,
                    ExportFormat = c.CellContentExportStringFormat,
                });
                foreach (var item in columnsFields)
                {
                    fieldsAndFormats.Add(item.Key, new DataCellFormats() { ContentDisplayFormat = item.ContentFormat, ExcelFormat = item.ExportFormat, ClipboardFormat = item.ExportFormat });
                }
            });
            return fieldsAndFormats;
        }

        /// <summary>
        /// Возвращает FlowDocument для печати
        /// </summary>
        /// <returns></returns>
        protected FlowDocument GenerateFlowDocumentFromPrint()
        {
            using DataTable dataTable = this.view.ToDataTable(this.GetFieldsAndFormats(), this.GetValueDelegate);

            System.Windows.Controls.PrintDialog printDialog = new();
            printDialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(System.Printing.PageMediaSizeName.ISOA4);
            printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

            FlowDocument doc = new()
            {
                PageHeight = printDialog.PrintableAreaHeight,
                PageWidth = printDialog.PrintableAreaWidth,

                FontFamily = new FontFamily("Verdana;Tahoma"),
                FontSize = 10d,
                ColumnWidth = double.PositiveInfinity,
            };

            Paragraph p = new(new Run(this.ReportTitle))
            { FontSize = 16, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };
            doc.Blocks.Add(p);

            Table table = new()
            {
                CellSpacing = 0,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
            };

            foreach (object _ in dataTable.Columns)
            {
                table.Columns.Add(new TableColumn());
            }

            int columnsCount = dataTable.Columns.Count;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                App.DoEvents();
                TableRowGroup rg = new();
                TableRow row = new();

                foreach (DataColumn column in dataTable.Columns)
                {
                    App.DoEvents();
                    TableCell cell = new(new Paragraph(new Run(dataRow[column].ToString())))
                    { BorderBrush = new SolidColorBrush(Colors.Gray), BorderThickness = new Thickness(0.5) };
                    row.Cells.Add(cell);
                }

                rg.Rows.Add(row);

                row = new TableRow();
                TableCell line = new() { Padding = new Thickness(0), BorderBrush = new SolidColorBrush(Colors.Black), BorderThickness = new Thickness(0, 0, 0, 1), ColumnSpan = columnsCount };
                row.Cells.Add(line);
                rg.Rows.Add(row);

                table.RowGroups.Add(rg);
            }

            doc.Blocks.Add(table);

            return doc;
        }

        public void Reset()
        {
            this.view = null;
            this.RaisePropertyChanged(nameof(this.View));
        }

        protected virtual void OnViewBuilded()
        {
            if (this.IsBusy)
                this.IsBusy = false;
        }

        #region Filters

        protected virtual void OnCollectionFiltered(ItemsFilter.FilteredEventArgs e)
        {
        }

        protected virtual void OnFiltersChanged(ItemsFilter.Model.IFilter filter)
        {
        }

        private void CollectionFiltered(object sender, ItemsFilter.FilteredEventArgs e)
        {
            this.ActiveFiltersList = null;
            this.OnCollectionFiltered(e);
            this.RaisePropertyChanged(nameof(this.ActiveFiltersList));
        }

        private void FilterListExtensions_FiltersChanged(ItemsFilter.ViewModel.FilterControlVm vm, ItemsFilter.Model.IFilter filter)
        {
            ItemsFilter.Model.IPropertyFilter propertyFilter = filter as ItemsFilter.Model.IPropertyFilter;
            string propertyName = Utils.ConvertFromTitleCase(propertyFilter.PropertyInfo.Name);

            if (filter.IsActive)
            {
                string value = string.Empty;

                if (filter is ItemsFilter.Model.IComparableFilter comparableFilter)
                {
                    value = comparableFilter.Name.ToUpperInvariant() + " " + comparableFilter.CompareTo.ToTrimmedString();
                }

                if (filter is ItemsFilter.Model.IStringFilter stringFilter)
                {
                    value = stringFilter.Mode.GetDescription() + " '" + stringFilter.Value + "'";
                }

                if (filter is ItemsFilter.Model.IMultiValueFilter multiValueFilter)
                {
                    value = string.Join(", ", multiValueFilter.SelectedValues);
                }

                if (filter is ItemsFilter.Model.IRangeFilter rangeFilter)
                {
                    value = "от '" + rangeFilter.CompareFrom + "' до '" + rangeFilter.CompareTo + "'";
                }

                value = string.Format(AppSettings.CurrentCulture, "'{0}'", value);

                if (this.selectedFilters.ContainsKey(propertyName))
                {
                    if (this.selectedFilters[propertyName].ContainsKey(filter))
                    {
                        this.selectedFilters[propertyName][filter] = value;
                    }
                    else
                    {
                        this.selectedFilters[propertyName].Add(filter, value);
                    }
                }
                else
                {
                    this.selectedFilters.Add(propertyName, new Dictionary<ItemsFilter.Model.IFilter, string>
                    {
                        { filter, value },
                    });
                }
            }
            else
            {
                if (this.selectedFilters.ContainsKey(propertyName))
                {
                    Dictionary<IFilter, string> dictionary = this.selectedFilters[propertyName];
                    if (dictionary.ContainsKey(filter))
                    {
                        dictionary.Remove(filter);
                    }
                }
            }

            this.OnFiltersChanged(filter);

            this.ActiveFiltersList = null;
            this.RaisePropertyChanged(propertyName: nameof(this.ActiveFiltersList));
        }

        #endregion

        private void View_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Count")
            {
                switch (sender)
                {
                    case CollectionView collectionView:
                        this.CollectionViewItemsCount = collectionView.Count;
                        break;
                    case ICollection<T> collection:
                        this.CollectionViewItemsCount = collection.Count;
                        break;
                    case System.Collections.IList list:
                        this.CollectionViewItemsCount = list.Count;
                        break;
                    case IEnumerable<T> enumerable:
                        this.CollectionViewItemsCount = enumerable.Count();
                        break;
                    default:
                        break;
                }
                this.RaisePropertyChanged(propertyName: nameof(this.PercentOfTotal));
            }
        }

        #endregion
    }

}
