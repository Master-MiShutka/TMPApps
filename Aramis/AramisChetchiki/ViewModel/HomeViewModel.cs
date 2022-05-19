namespace TMP.WORK.AramisChetchiki.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using TMP.Extensions;
    using TMP.Shared;
    using TMP.Shared.Commands;
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;
    using TMP.WORK.AramisChetchiki.Model;

    public class HomeViewModel : BaseViewModel
    {
        #region Fields
        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ObservableCollection<IMatrix> pivotCollection;

        private KeyValuePair<int, string> selectedQuarter;
        private int selectedYear;
        private string message = "?";
        private bool isPivotsBuilding = false;
        private bool pivotsBuildingCanceled = false;

        #endregion Fields

        #region Constructor

        public HomeViewModel()
        {
            this.Quarters = new ReadOnlyCollection<KeyValuePair<int, string>>(new[]
            {
                new KeyValuePair<int, string>(1, "I кв"),
                new KeyValuePair<int, string>(2, "II кв"),
                new KeyValuePair<int, string>(3, "III кв"),
                new KeyValuePair<int, string>(4, "IV кв"),
            });

            int nowYear = DateTime.Now.Year;
            this.Years = new ReadOnlyCollection<int>(Enumerable.Range(nowYear, 5).ToList());

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                this.DetailedStatus = "Какой-то текст...";
                this.IsBusy = false;

                System.Diagnostics.Debug.Write("HomeViewModel IsInDesignMode");
                return;
            }

            // команда получения данных из базы данных Арамис
            this.CommandGetData = new DelegateCommand(
                () =>
            {
                if (MainViewModel.SelectedDataFileInfo.IsLocal)
                {
                    MainViewModel.ChangeMode(Mode.LoadingData);
                }
                else
                {
                    this.ShowDialogWarning($"База данных '{MainViewModel.SelectedDataFileInfo.AramisDbPath}' более недоступна.\nПерейдите к параметрам и укажите новый путь\nк базе данных ПО 'Арамис'.");
                }
            },
                () => MainViewModel?.SelectedDataFileInfo != null);

            this.CommandShowHelp = new DelegateCommand(
                () =>
            {
                this.ShowDialogQuestion("Действильно нужна помощь?");
            });

            this.CommandShowPreferences = new DelegateCommand(
                () =>
            {
                MainViewModel.ChangeMode(Mode.Preferences);
            });

            this.CommandShowAll = new DelegateCommand(
                () =>
                {
                    MainViewModel.ChangeMode(Mode.ViewMeters);
                },
                () => this.IsDataLoaded);

            this.ShowViewCommands = new List<Tuple<ICommand, string>>();

            foreach (Model.Mode mode in Enum.GetValues(typeof(Model.Mode)))
            {
                System.Reflection.FieldInfo info = mode.GetType().GetField(mode.ToString());
                System.ComponentModel.BrowsableAttribute[] values = (System.ComponentModel.BrowsableAttribute[])info.GetCustomAttributes(typeof(System.ComponentModel.BrowsableAttribute), false);

                if (values?.Length == 1 && values[0].Browsable)
                {
                    this.ShowViewCommands.Add(new Tuple<ICommand, string>(
                        new DelegateCommand(
                        () =>
                        {
                            MainViewModel.ChangeMode(mode);
                        },
                        () => this.IsDataLoaded),
                        ModelHelper.ModesDescription[mode]));
                }
            }

            if (MainViewModel?.Meters != null)
            {
                this.Statistics = new MetersStatistics(MainViewModel.Meters);

                this.RaisePropertyChanged(nameof(this.Statistics));
            }

            this.PivotCollection = new ObservableCollection<IMatrix>();

            App.InvokeInUIThread(() => System.Windows.Data.BindingOperations.EnableCollectionSynchronization(this.PivotCollection, new object()));

            System.Threading.Tasks.Task.Run(this.Start);
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Команда получения данных
        /// </summary>
        public ICommand CommandGetData { get; }

        /// <summary>
        /// Команда отображения настроек
        /// </summary>
        public ICommand CommandShowPreferences { get; }

        /// <summary>
        /// Команда отображения справки
        /// </summary>
        public ICommand CommandShowHelp { get; }

        /// <summary>
        /// Команда отображения списка всех счётчиков
        /// </summary>
        public ICommand CommandShowAll { get; }

        /// <summary>
        /// Список команд для выбора режима работы
        /// </summary>
        public IList<Tuple<ICommand, string>> ShowViewCommands { get; }

        /// <summary>
        /// Выбранный квартал - пара номер квартала и и его название: I кв
        /// </summary>
        public KeyValuePair<int, string> SelectedQuarter
        {
            get
            {
                if (this.selectedQuarter.Key == 0)
                {
                    this.selectedQuarter = this.Quarters[Meter.ДатаСравненияПоверки.GetQuarter() - 1];
                }

                return this.selectedQuarter;
            }

            set
            {
                if (EqualityComparer<KeyValuePair<int, string>>.Default.Equals(value, this.selectedQuarter))
                {
                    return;
                }

                this.selectedQuarter = value;
                this.RaisePropertyChanged();
                this.UpdateПоверка();
            }
        }

        /// <summary>
        /// Выбранный год
        /// </summary>
        public int SelectedYear
        {
            get
            {
                if (this.selectedYear == 0)
                {
                    this.selectedYear = Meter.ДатаСравненияПоверки.Year;
                }

                return this.selectedYear;
            }

            set
            {
                if (value == this.selectedYear)
                {
                    return;
                }

                this.selectedYear = value;
                this.RaisePropertyChanged();
                this.UpdateПоверка();
            }
        }

        /// <summary>
        /// Список кварталов
        /// </summary>
        public ReadOnlyCollection<KeyValuePair<int, string>> Quarters { get; private set; }

        /// <summary>
        /// Список годов
        /// </summary>
        public ReadOnlyCollection<int> Years { get; private set; }

        /// <summary>
        /// Указывает, что данные получены очень давно
        /// </summary>
        public bool DataIsOld { get; private set; }

        /// <summary>
        /// Коллекция сводных таблиц
        /// </summary>
        public ObservableCollection<IMatrix> PivotCollection { get => this.pivotCollection; private set => this.SetProperty(ref this.pivotCollection, value); }

        public MetersStatistics Statistics { get; init; }

        #endregion Properties

        #region Private methods

        protected override void OnDataLoaded()
        {
            base.OnDataLoaded();

            DateOnly now = DateOnly.FromDateTime(DateTime.Now);
            DateOnly? date = Repository.Instance?.Data?.Info.Period?.StartDate;

            this.DataIsOld = date.HasValue && date.Value != default && (now.AddDays(-31) > date.Value);

            this.RaisePropertyChanged(nameof(this.DataIsOld));

            this.Start();
        }

        private void Start()
        {
            if (this.isPivotsBuilding)
            {
                this.pivotsBuildingCanceled = true;
                while (this.isPivotsBuilding)
                {
                    System.Threading.Tasks.Task.Delay(100);
                }
            }

            this.BuildPivots();
        }

        /// <summary>
        /// Обновление статуса поверки счётчиков
        /// </summary>
        private void UpdateПоверка()
        {
            if (MainViewModel.Data == null || MainViewModel.Meters == null || MainViewModel.Data.SummaryInfos == null)
            {
                return;
            }

            if (MainViewModel.Data.SummaryInfos is IList<SummaryInfoItem> == false)
            {
                return;
            }

            IList<SummaryInfoItem> summaryInfos = MainViewModel.Data.SummaryInfos as IList<SummaryInfoItem>;

            Meter.ДатаСравненияПоверки = new DateOnly(this.SelectedYear, (this.SelectedQuarter.Key * 3) - 2, 1);

            this.Status = "анализ данных";
            Task task = System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Thread.CurrentThread.Name = "UpdatePoverka";

                SummaryInfoItem item = MainViewModel.Data.SummaryInfos.FirstOrDefault(i => i.FieldName == "Поверен");
                int index = summaryInfos.IndexOf(item);
                if (item != null)
                {
                    item = SummaryInfoHelper.BuildSummaryInfoItem(MainViewModel.Meters, "Поверен");
                    App.Current.Dispatcher.Invoke((Action)(() => summaryInfos[index] = item));
                }
            });

            task.ContinueWith(
                t =>
            {
                this.IsBusy = false;
                this.Status = null;
            }, TaskScheduler.FromCurrentSynchronizationContext())
            .ContinueWith(
                t =>
            {
                MainViewModel.MatrixCache.Clear();
                this.BuildPivots();
            });
        }

        private void BuildPivots()
        {
            this.IsBusy = true;
            this.Status = "подготовка сводных таблиц ...";

            if(!(MainViewModel.Data == null || MainViewModel.Meters == null || MainViewModel.Meters.Any() == false))
            {
                Task task = Task.Run(() =>
                {
                    System.Threading.Thread.CurrentThread.Name = "BuildMessageAndPivots";
                    this.CreatePivots();
                })
                .ContinueWith((t) =>
                {
                    this.IsBusy = false;
                });
            }
        }

        private void Matrix_Builded(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            bool flag = !this.pivotCollection.All(matrix => matrix.IsBuilded);
            if (flag == false)
            {
                this.IsBusy = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1123:Do not place regions within elements", Justification = "<Pending>")]
        private void CreatePivots()
        {
            foreach (IMatrix mtrx in this.pivotCollection)
            {
                mtrx.Builded -= this.Matrix_Builded;
            }

            this.pivotCollection.Clear();

            bool add(IMatrix matrix)
            {
                if (this.pivotsBuildingCanceled)
                    return false;

                if (matrix == null)
                    return false;

                this.pivotCollection.Add(matrix);

                matrix.Builded += this.Matrix_Builded;

                if (MainViewModel.MatrixCache.ContainsKey(matrix.Header) == false)
                {
                    MainViewModel.MatrixCache.Add(matrix.Header, matrix.Items);
                }

                return true;
            }

            IList<IMatrixCell> matrixDefinition = null;
            IList<IMatrixCell> getMatrixDefinition(string id)
            {
                if (MainViewModel.MatrixCache.TryGetValue(id, out matrixDefinition) == false)
                    return null;
                return matrixDefinition;
            }

            Dictionary<ulong, IList<ChangeOfMeter>> changesOfMeters = MainViewModel.Data?.ChangesOfMeters;
            if (changesOfMeters == null)
            {
                changesOfMeters = new();
            }

            IEnumerable<Meter> allMeters = MainViewModel.Meters.Where(i => i.Удалён == false);
            int metersCount = allMeters.Count();

            IEnumerable<Meter> metersNotSplit = MainViewModel.Meters
                .Where(i => i.Удалён == false)
                .Where(i => i.МестоУстановки != "СПЛИТ");

            // населенные пункты по убыванию кол-ва счетчиков (первые 10)
            IEnumerable<SummaryInfoGroupItem> meterPerLocality = SummaryInfoHelper.BuildFirst10LargeGroups(metersNotSplit, nameof(Meter.НаселённыйПункт), SummaryInfoHelper.DefaultMeterFilterFunc);

            // населенные пункты по убыванию отсутствия обхода более года
            Dictionary<string, int> noVisitMoreThanOneYearPerLocality = metersNotSplit
                .Where(i => i.ОбходаНеБылоМесяцев >= 12)
                .GroupBy(i => i.НаселённыйПункт)
                .Select(i => new { Key = i.Key, Count = i.Count() })
                .OrderByDescending(i => i.Count)
                .ToDictionary(i => i.Key, i => i.Count);

            // населенные пункты по убыванию кол-ва не обойденных
            Dictionary<string, int> missingVisitPerLocality = metersNotSplit
                .Where(i => i.ОбходаНеБылоМесяцев >= 36)
                .GroupBy(i => i.НаселённыйПункт)
                .Select(i => new { Key = i.Key, Count = i.Count() })
                .OrderByDescending(i => i.Count)
                .ToDictionary(i => i.Key, i => i.Count);

            // населенные пункты по убыванию кол-ва обходов в этом году
            Dictionary<string, int> visitInCurrentYearPerLocality = metersNotSplit
                .Where(i => i.ОбходаНеБылоМесяцев < 12)
                .GroupBy(i => i.НаселённыйПункт)
                .Select(i => new { Key = i.Key, Count = i.Count() })
                .OrderByDescending(i => i.Count)
                .ToDictionary(i => i.Key, i => i.Count);

            // сельсоветы
            IList<string> provinces = allMeters
                .Select(i => i.СельскийСовет)
                .Distinct()
                .Select(i => i)
                .ToList<string>();

            // населенные пункты
            IList<string> localities = allMeters
                .Select(i => i.НаселённыйПункт)
                .Distinct()
                .Select(i => i)
                .ToList<string>();

            // количество счетчиков в населенных пунктах
            Dictionary<string, int> metersCountPerLocality = allMeters
                .GroupBy(i => i.НаселённыйПункт)
                .Select(i => new { Key = i.Key, Count = i.Count() })
                .OrderByDescending(i => i.Count)
                .ToDictionary(i => i.Key, i => i.Count);

            // количество не сплит-счетчиков в населенных пунктах
            Dictionary<string, int> metersNotSplitCountPerLocality = metersNotSplit
                .GroupBy(i => i.НаселённыйПункт)
                .Select(i => new { Key = i.Key, Count = i.Count() })
                .OrderByDescending(i => i.Count)
                .ToDictionary(i => i.Key, i => i.Count);

            // количество не сплит-счетчиков в населенных пунктах
            Dictionary<string, int> metersNotSplitAndNotHaveAskueCountPerLocality = metersNotSplit
                .Where(i => i.Аскуэ == false)
                .GroupBy(i => i.НаселённыйПункт)
                .Select(i => new { Key = i.Key, Count = i.Count() })
                .OrderByDescending(i => i.Count)
                .ToDictionary(i => i.Key, i => i.Count);

            // все типы счетчиков
            IEnumerable<SummaryInfoGroupItem> allMeterTypes = SummaryInfoHelper.BuildFirst10LargeGroups(allMeters, nameof(Meter.ТипСчетчика), SummaryInfoHelper.DefaultMeterFilterFunc);

            // типы электронных счетчиков
            IEnumerable<SummaryInfoGroupItem> electronicMeterTypes = SummaryInfoHelper.BuildFirst10LargeGroups(allMeters.Where(meter => meter.Принцип == "электронный"), nameof(Meter.ТипСчетчика), SummaryInfoHelper.DefaultMeterFilterFunc);

            const int countOfMeterTypePerLocality = 15;
            const int countOfLocalities = 50;

            string matrixHeader = null;
            string matrixDescription = null;

            int curYear = DateTime.Now.Year;

            // показывать данные за 8 лет
            const int yearsCount = 8;

            #region Свод по установке или замене счётчиков за последние восемь лет помесячно

            matrixHeader = "Свод по установке или замене счётчиков за последние восемь лет помесячно";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => Enumerable.Range(curYear - yearsCount + 1, yearsCount).Reverse().Select(i => MatrixHeaderCell.CreateRowHeader(i.ToString(AppSettings.CurrentCulture))),
                GetColumnHeaderValuesFunc = () => System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12)
                    .Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    List<DateOnly> l = changesOfMeters
                        .SelectMany(i => i.Value)
                        .Where(i => i.ДатаЗамены != default)
                        .Select(i => i.ДатаЗамены)
                        .ToList();
                    List<DateOnly> l1 = l
                        .Where(i => i.Year.ToString(AppSettings.CurrentCulture) == row.Header)
                        .ToList();

                    List<DateOnly> values = l1
                        .Where(i => string.Equals(i.ToString("MMMM", AppSettings.CurrentCulture), column.Header, AppSettings.StringComparisonMethod))
                        .ToList();
                    return values == null || values.Count == 0 ? new MatrixDataCell(string.Empty) : new MatrixDataCell(values.Count);
                },
            }) == false)
                return;
            #endregion

            #region Свод по установке или замене на электронный счётчик за последние восемь лет помесячно
            matrixHeader = "Свод по установке или замене на электронный счётчик за последние восемь лет помесячно";
            matrixDescription = "* количество электронных счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => Enumerable.Range(curYear - yearsCount + 1, yearsCount).Reverse().Select(i => MatrixHeaderCell.CreateRowHeader(i.ToString(AppSettings.CurrentCulture))),
                GetColumnHeaderValuesFunc = () => System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12)
                    .Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    List<DateOnly> l = changesOfMeters
                        .SelectMany(i => i.Value)
                        .Where(i => i.ДатаЗамены != default)
                        .Where(i => i.ЭтоЭлектронный)
                        .Select(i => i.ДатаЗамены)
                        .ToList();
                    List<DateOnly> l1 = l
                        .Where(i => i.Year.ToString(AppSettings.CurrentCulture) == row.Header)
                        .ToList();

                    List<DateOnly> values = l1
                        .Where(i => string.Equals(i.ToString("MMMM", AppSettings.CurrentCulture), column.Header, AppSettings.StringComparisonMethod))
                        .ToList();
                    if (values == null || values.Count == 0)
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                    else
                    {
                        return new MatrixDataCell(values.Count);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по категории счётчика и типу населённого пункта
            matrixHeader = "Свод по категории счётчика\n и типу населённого пункта";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => allMeters.Select(i => i.ГруппаСчётчикаДляОтчётов).Distinct().Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                GetColumnHeaderValuesFunc = () => allMeters.Select(i => i.ТипНаселённойМестности).Distinct().Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    List<Meter> list = allMeters
                        .Where(i => i.ГруппаСчётчикаДляОтчётов == row.Header)
                        .Where(i => i.ТипНаселённойМестности == column.Header)
                        .ToList();
                    if (list != null)
                    {
                        int count = list.Count;
                        return new MatrixDataCell(count) { ToolTip = $"{100 * count / metersCount:N1}%" };
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по н.п., преобладающим типам счетчиков и кол-ву фаз счетчика
            matrixHeader = "Свод по н.п., преобладающим типам счетчиков и\nкол-ву фаз счетчик";
            matrixDescription = "* количество счётчиков; \t* без АСКУЭ;\n* сплит-счётчики не учтены";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            // fix
            foreach (Meter meter in metersNotSplit)
            {
                if (meter.ТипСчетчика == null)
                {
                    meter.ТипСчетчика = string.Empty;
                }
            }

            // населенные пункты по убыванию кол-ва счетчиков (первые countOfLocalities)
            IEnumerable<SummaryInfoGroupItem> gr = SummaryInfoHelper.BuildFirst10LargeGroups(
                metersNotSplit,
                nameof(Meter.НаселённыйПункт),
                i => i.Аскуэ == false,
                countOfLocalities);

            var meterTypePerLocality = gr
                .Select(sigi => new
                {
                    Key = sigi.Key,
                    Count = sigi.Count,
                    MeterTypes = sigi.Value
                            .GroupBy(m => m.ТипСчетчика)
                            .Select(m => new
                            {
                                Key = m.Key,
                                Count = m.Count(),
                                Meters = m.ToList(),
                            })
                            .OrderByDescending(m => m.Count)
                            .Take(countOfMeterTypePerLocality)
                            .ToDictionary(i => i.Key, i => i),
                })
                .OrderByDescending(i => i.Count)
                .ToDictionary(i => i.Key, i => i);

            IEnumerable<IMatrixHeader> headerCells0 = allMeters
                .Select(i => i.Фаз)
                .Distinct()
                .Select(i => MatrixHeaderCell.CreateColumnHeader(i.ToString()))
                .ToList();

            MatrixWithSelector matrix = new MatrixWithSelector()
            {
                Parameters = meterTypePerLocality.Select(i => i.Key),
                SelectedParameter = meterTypePerLocality.First().Key,
                Header = matrixHeader,
                Description = matrixDescription,
                GetColumnHeaderValuesFunc = () => headerCells0,
            };
            matrix.GetRowHeaderValuesFunc = () => meterTypePerLocality[(string)matrix.SelectedParameter].MeterTypes.Select(meterType => MatrixHeaderCell.CreateRowHeader(meterType.Key, tag: meterType.Value.Count)).ToList();
            matrix.GetDataCellFunc = (row, column) =>
            {
                var group = meterTypePerLocality[(string)matrix.SelectedParameter];

                var list = group.MeterTypes[row.Header];

                List<Meter> meters = list.Meters;
                int totalMetersCountInLocality = meters.Count;

                int count = meters.Where(i => i.Фаз.ToString() == column.Header).Count();

                if (list != null)
                {
                    return new MatrixDataCell(count) { ToolTip = $"{100 * count / group.Count:N1}% от количества счётчиков в '{group.Key}'" };
                }
                else
                {
                    return new MatrixDataCell(string.Empty);
                }
            };
            if (add(matrix) == false)
                return;

            #endregion

            #region Свод по неповеренным по н.п., преобладающим типам счетчиков и кол-ву фаз счетчика
            matrixHeader = "Свод неповереных по н.п., преобладающим типам\nсчетчиков и кол-ву фаз счетчика";
            matrixDescription = "* количество неповереных счётчиков;\t* без АСКУЭ;\n* сплит-счётчики не учтены";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            var meterTypePerLocality2 = SummaryInfoHelper.BuildFirst10LargeGroups(
                metersNotSplit,
                nameof(Meter.НаселённыйПункт),
                i => i.Поверен == false && i.Аскуэ == false,
                countOfLocalities)
                .Select(sigi => new
                {
                    Key = sigi.Key,
                    Count = sigi.Count,
                    MeterTypes = sigi.Value
                            .GroupBy(m => m.ТипСчетчика)
                            .Select(m => new
                            {
                                Key = m.Key,
                                Count = m.Count(),
                                Meters = m.ToList(),
                            })
                            .OrderByDescending(m => m.Count)
                            .Take(countOfMeterTypePerLocality)
                            .ToDictionary(i => i.Key, i => i),
                })
                .OrderByDescending(i => i.Count)
                .ToDictionary(i => i.Key, i => i);

            IEnumerable<IMatrixHeader> headerCells2 = allMeters
                .Select(i => i.Фаз)
                .Distinct()
                .Select(i => MatrixHeaderCell.CreateColumnHeader(i.ToString()))
                .ToList();

            MatrixWithSelector matrix2 = new MatrixWithSelector()
            {
                Parameters = meterTypePerLocality2.Select(i => i.Key),
                SelectedParameter = meterTypePerLocality2.First().Key,
                Header = matrixHeader,
                Description = matrixDescription,
                GetColumnHeaderValuesFunc = () => headerCells2,
            };
            matrix2.GetRowHeaderValuesFunc = () => meterTypePerLocality2[(string)matrix2.SelectedParameter].MeterTypes.Select(meterType => MatrixHeaderCell.CreateRowHeader(meterType.Key, tag: meterType.Value.Count)).ToList();
            matrix2.GetDataCellFunc = (row, column) =>
            {
                var group = meterTypePerLocality2[(string)matrix2.SelectedParameter];

                var list = group.MeterTypes[row.Header];

                List<Meter> meters = list.Meters;
                int totalMetersCountInLocality = meters.Count;

                int count = meters.Where(i => i.Фаз.ToString() == column.Header).Count();

                if (list != null)
                {
                    return new MatrixDataCell(count) { ToolTip = $"{100 * count / group.Count:N1}% от количества счётчиков в '{group.Key}'" };
                }
                else
                {
                    return new MatrixDataCell(string.Empty);
                }
            };
            if (add(matrix2) == false)
                return;

            #endregion

            #region Свод по типам счётчиков
            matrixHeader = "Свод по типам счётчиков";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                ShowRowsTotal = false,
                GetRowHeaderValuesFunc = () => allMeterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                GetColumnHeaderValuesFunc = () => new string[] { "Количество", "%" }.Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    SummaryInfoGroupItem group = allMeterTypes.FirstOrDefault(i => i.Key.ToString() == row.Header);
                    if (column.Header == "%")
                    {
                        return new MatrixDataCell(group.Percent);
                    }
                    else
                    {
                        return new MatrixDataCell(group.Count);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по категории счётчика, состоянию метрологической поверки и типу населённого пункта
            matrixHeader = "Свод по категории счётчика, состоянию\nметрологической поверки и типу населённого пункта";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            IList<string> childsHeaderCells5 = allMeters
                .Select(i => i.ТипНаселённойМестности)
                .Distinct()
                .Select(i => i)
                .ToList<string>();

            IEnumerable<IMatrixHeader> headerCells51 = new IMatrixHeader[]
            {
                MatrixHeaderCell.CreateColumnHeader("Есть АСКУЭ", 0, 0, columnSpan: 4, children: new List<IMatrixHeader>()
            {
                MatrixHeaderCell.CreateColumnHeader("поверен", 1, 0, columnSpan: 2, children: new List<IMatrixHeader>(childsHeaderCells5.Select(i => MatrixHeaderCell.CreateColumnHeader(i)).ToList())),
                MatrixHeaderCell.CreateColumnHeader("не поверен", 1, 3, columnSpan: 2, children: new List<IMatrixHeader>(childsHeaderCells5.Select(i => MatrixHeaderCell.CreateColumnHeader(i)).ToList())),
            }),
                MatrixHeaderCell.CreateColumnHeader("Нет АСКУЭ", 0, 5, columnSpan: 4, children: new List<IMatrixHeader>()
            {
                MatrixHeaderCell.CreateColumnHeader("поверен", 1, 5, columnSpan: 2, children: new List<IMatrixHeader>(childsHeaderCells5.Select(i => MatrixHeaderCell.CreateColumnHeader(i)).ToList())),
                MatrixHeaderCell.CreateColumnHeader("не поверен", 1, 7, columnSpan: 2, children: new List<IMatrixHeader>(childsHeaderCells5.Select(i => MatrixHeaderCell.CreateColumnHeader(i)).ToList())),
            }),
            };

            var children5 = allMeters
                    .Select(i => i.ГруппаСчётчикаДляОтчётов)
                    .Distinct()
                    .ToList();

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => new IMatrixHeader[]
                {
                    MatrixHeaderCell.CreateRowHeader("абонента", children: children5.Select(i => MatrixHeaderCell.CreateRowHeader(i)).ToList()),
                    MatrixHeaderCell.CreateRowHeader("ЭСО", children: children5.Select(i => MatrixHeaderCell.CreateRowHeader(i)).ToList()),
                },
                ShowColumnsTotal = true,
                GetColumnHeaderValuesFunc = () => headerCells51,
                GetDataCellFunc = (row, column) =>
                {
                    string meterGroup = row.Header;
                    bool meterAbonentBalance = row.Parent.Header == "абонента";

                    bool hasAskue = column.Parent.Parent.Header == "Есть АСКУЭ";
                    bool isPoveren = column.Parent.Header == "поверен";

                    List<Meter> list = allMeters
                        .Where(i => i.НаБалансеАбонента == meterAbonentBalance)
                        .Where(i => i.ГруппаСчётчикаДляОтчётов == meterGroup)
                        .Where(i => i.Аскуэ == hasAskue)
                        .Where(i => i.ТипНаселённойМестности == column.Header)
                        .Where(i => i.Поверен == isPoveren)
                        .ToList();
                    if (list != null)
                    {
                        int count = list.Count;
                        return new MatrixDataCell(count) { ToolTip = $"{100 * count / metersCount:N1}%" };
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по использованию э/э для целей отопления, типу населённого пункта и наличию АСКУЭ
            matrixHeader = "Свод по использованию э/э для целей отопления,\nтипу населённого пункта и наличию АСКУЭ";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            var cityTypes = allMeters.Select(i => i.ТипНаселённойМестности).Distinct();

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                ShowColumnsTotal = true,
                ShowRowsTotal = true,
                GetColumnHeaderValuesFunc = () => new IMatrixHeader[]
                {
                    MatrixHeaderCell.CreateColumnHeader("Есть АСКУЭ"),
                    MatrixHeaderCell.CreateColumnHeader("Нет АСКУЭ"),
                },
                GetRowHeaderValuesFunc = () => new IMatrixHeader[]
                {
                    MatrixHeaderCell.CreateRowHeader("эл.\nотопление", children: cityTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i)).ToList()),
                    MatrixHeaderCell.CreateRowHeader("ж/д с\nэл.плитой", children: cityTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i)).ToList()),
                },
                GetDataCellFunc = (row, column) =>
                {
                    bool hasAskue = column.Header == "Есть АСКУЭ";
                    List<Meter> list = null;

                    switch (row.Parent.Header)
                    {
                        case "эл.\nотопление":
                            list = allMeters
                                .Where(i => i.Аскуэ == hasAskue)
                                .Where(i => i.ТипНаселённойМестности == row.Header)
                                .Where(i => i.Расположение == "Эо и гвс")
                                .ToList();
                            break;
                        case "ж/д с\nэл.плитой":
                            list = allMeters
                                .Where(i => i.Аскуэ == hasAskue)
                                .Where(i => i.ТипНаселённойМестности == row.Header)
                                .Where(i => i.Расположение == "Ж/д с эп эо и гвс")
                                .ToList();
                            break;
                        default:
                            break;
                    }

                    if (list != null)
                    {
                        int count = list.Count;
                        return new MatrixDataCell(count) { ToolTip = $"{100 * count / metersCount:N1}%" };
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;

            #endregion

            #region Свод по использованию э/э для целей отопления и наличию АСКУЭ
            matrixHeader = "Свод по использованию э/э для целей\nотопления и наличию АСКУЭ";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            // населенные пункты по убыванию кол-ва счетчиков (первые countOfLocalities)
            IEnumerable<SummaryInfoGroupItem> gr1 = SummaryInfoHelper.BuildFirst10LargeGroups(
                allMeters,
                nameof(Meter.НаселённыйПункт),
                i => i.Расположение == "Эо и гвс" || i.Расположение == "Ж/д с эп эо и гвс",
                12);

            var cityesWhereNagrev = gr1
                .Select(i => i.Key.ToString())
                .Distinct();

            if(add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                ShowColumnsTotal = true,
                ShowRowsTotal = true,
                GetColumnHeaderValuesFunc = () => new IMatrixHeader[]
                {
                    MatrixHeaderCell.CreateColumnHeader("Есть АСКУЭ"),
                    MatrixHeaderCell.CreateColumnHeader("Нет АСКУЭ"),
                },
                GetRowHeaderValuesFunc = () => cityesWhereNagrev.Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    bool hasAskue = column.Header == "Есть АСКУЭ";

                    List<Meter> list = gr1
                        .Where(i => i.Key == row.Header)
                        .Select(i => i.Value)
                        .First()
                        .Where(meter => meter.Аскуэ == hasAskue)
                        .ToList();

                    if (list != null)
                    {
                        int count = list.Count;
                        return new MatrixDataCell(count) { ToolTip = $"{100 * count / metersCount:N1}%" };
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;

            #endregion

            #region Свод по нас. пункту, количеству МЖД, наличию аскуэ
            matrixHeader = "Свод по нас. пункту, количеству МЖД";
            matrixDescription = "* количество МЖД";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            IEnumerable<string> citiesWithApartmentBuilding = allMeters
                .Where(i => i.МестоУстановки == "Лестничная клетка")
                .Where(i => i.ЭтоМжд)
                .Select(i => i.НаселённыйПункт)
                .Distinct();

            IEnumerable<IMatrixHeader> headersHasAskue = new IMatrixHeader[]
            {
                MatrixHeaderCell.CreateColumnHeader("есть АСКУЭ", tag: true),
                MatrixHeaderCell.CreateColumnHeader("нет АСКУЭ", tag: false),
            };

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => citiesWithApartmentBuilding.Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                GetColumnHeaderValuesFunc = () => headersHasAskue,
                GetDataCellFunc = (row, column) =>
                {
                    IEnumerable<Meter> list = allMeters
                        .Where(i => i.Адрес.City == row.Header)
                        .Where(i => i.МестоУстановки == "Лестничная клетка")
                        .Where(i => i.ЭтоМжд)
                        .Where(i => i.Аскуэ == (bool)column.Tag);

                    IEnumerable<string> listBuildings = list
                        .Select(i => i.НаселённыйПунктИУлицаСНомеромДома)
                        .Distinct();

                    if (listBuildings.Any())
                    {
                        int count = listBuildings.Count();
                        return new MatrixDataCell(count);
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по по н.п., мндукционным типам счетчиков и использованию
            matrixHeader = "Свод индукционных по н.п., преобладающим типам счетчиков и использованию";
            matrixDescription = "* количество индукционных счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            var meterTypePerLocality3 = SummaryInfoHelper.BuildFirst10LargeGroups(
                allMeters,
                nameof(Meter.НаселённыйПункт),
                i => i.Поверен == false && i.Аскуэ == false,
                countOfLocalities)
                .Select(sigi => new
                {
                    Key = sigi.Key,
                    CountOfAll = sigi.Count,
                    CountOfInd = sigi.Value.Where(i => i.Электронный == false).Count(),
                    MeterTypes = sigi.Value
                            .Where(i => i.Электронный == false)
                            .GroupBy(m => m.ТипСчетчика)
                            .Select(m => new
                            {
                                Key = m.Key,
                                Count = m.Count(),
                                Meters = m.ToList(),
                            })
                            .OrderByDescending(m => m.Count)
                            .Take(countOfMeterTypePerLocality)
                            .ToDictionary(i => i.Key, i => i),
                })
                .OrderByDescending(i => i.CountOfInd)
                .ToDictionary(i => i.Key, i => i);

            List<IMatrixHeader> headerCells3 = allMeters
                .Select(i => i.Использование)
                .Distinct()
                .Select(i => MatrixHeaderCell.CreateColumnHeader(i))
                .ToList();
            const string lastColumnHeader = "% от всех";
            headerCells3.Add(MatrixHeaderCell.CreateColumnHeader(lastColumnHeader));

            MatrixWithSelector matrix3 = new MatrixWithSelector()
            {
                Parameters = meterTypePerLocality3.Select(i => i.Key),
                SelectedParameter = meterTypePerLocality3.First().Key,
                Header = matrixHeader,
                Description = matrixDescription,
                GetColumnHeaderValuesFunc = () => headerCells3,
            };
            matrix3.GetRowHeaderValuesFunc = () => meterTypePerLocality3[(string)matrix3.SelectedParameter].MeterTypes.Select(meterType => MatrixHeaderCell.CreateRowHeader(meterType.Key, tag: meterType.Value.Count)).ToList();
            matrix3.GetDataCellFunc = (row, column) =>
            {
                var group = meterTypePerLocality3[(string)matrix3.SelectedParameter];

                var list = group.MeterTypes[row.Header];
                int metersCount = list.Count;
                List<Meter> meters = list.Meters;

                if (list != null)
                {
                    if (column.Header == lastColumnHeader)
                    {
                        return new MatrixDataCell($"{100 * metersCount / group.CountOfAll:N1}");
                    }
                    else
                    {
                        int count = meters.Where(i => i.Использование == column.Header).Count();
                        return new MatrixDataCell(count);
                    }
                }
                else
                {
                    return new MatrixDataCell(string.Empty);
                }
            };
            if (add(matrix3) == false)
                return;

            #endregion

            #region Свод по категории счётчика и сельсовету
            matrixHeader = "Свод по категории счётчика и сельсовету";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            IList<string> childsHeaderCells1 = allMeters
                .Select(i => i.ГруппаСчётчикаДляОтчётов)
                .Distinct()
                .Select(i => i)
                .ToList<string>();

            IEnumerable<IMatrixHeader> headerCells1 = new IMatrixHeader[]
            {
                MatrixHeaderCell.CreateColumnHeader("поверен", children: childsHeaderCells1.Select(i => MatrixHeaderCell.CreateColumnHeader(i)).ToList()),
                MatrixHeaderCell.CreateColumnHeader("не поверен", children: childsHeaderCells1.Select(i => MatrixHeaderCell.CreateColumnHeader(i)).ToList()),
            };

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => allMeters.Select(i => i.СельскийСовет).Distinct().Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetColumnHeaderValuesFunc = () => headerCells1,
                GetDataCellFunc = (row, column) =>
                {
                    List<Meter> list = allMeters
                        .Where(i => i.ГруппаСчётчикаДляОтчётов == column.Header)
                        .Where(i => i.СельскийСовет == row.Header)
                        .Where(i => i.Поверен == (column.Parent?.Header == "поверен"))
                        .ToList();
                    if (list != null)
                    {
                        int count = list.Count;
                        return new MatrixDataCell(count) { ToolTip = $"{100 * count / metersCount:N1}%" };
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по наличию обхода и сельсовету
            matrixHeader = "Свод по наличию обхода и сельсовету";
            matrixDescription = "* количество счётчиков\n* сплит-счётчики не учтены";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => metersNotSplit.Select(i => i.СельскийСовет).Distinct().Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                GetColumnHeaderValuesFunc = () => metersNotSplit.Select(i => i.ОбходаНеБыло).Distinct().Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    List<Meter> list = metersNotSplit
                        .Where(i => i.ОбходаНеБыло == column.Header)
                        .Where(i => i.СельскийСовет == row.Header)
                        .ToList();
                    if (list != null)
                    {
                        int count = list.Count;
                        return new MatrixDataCell(count) { ToolTip = $"{100 * count / metersCount:N1}%" };
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по населенному пункту и отсутствию обхода более года
            matrixHeader = "Свод по населенному пункту и отсутствию обхода более года";
            matrixDescription = "* количество счётчиков\n* сплит-счётчики не учтены";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            const string col1Header = "Кол-во без обхода"; // missingVisitPerLocality
            const string col2Header = "Кол-во не\nпосещ. > года"; // noVisitMoreThanOneYearPerLocality
            const string col3Header = "Кол-во посещ.\nза год"; // visitInCurrentYearPerLocality
            const string strToolTip = "% от общего количества счётчиков в н.п.";
            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => noVisitMoreThanOneYearPerLocality.Take(10).Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                GetColumnHeaderValuesFunc = () => new string[] { col1Header, col2Header, col3Header }.Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    int countMissingVisitPerLocality = missingVisitPerLocality.FirstOrDefault(i => i.Key.ToString() == row.Header).Value;
                    int countNoVisitMoreThanOneYearPerLocality = noVisitMoreThanOneYearPerLocality.FirstOrDefault(i => i.Key.ToString() == row.Header).Value;
                    int countVisitInCurrentYearPerLocality = visitInCurrentYearPerLocality.FirstOrDefault(i => i.Key.ToString() == row.Header).Value;

                    int metersInLocality = metersNotSplitCountPerLocality.FirstOrDefault(i => i.Key == row.Header).Value;

                    switch (column.Header)
                    {
                        case col1Header:
                            return new MatrixDataCell(countMissingVisitPerLocality) { ToolTip = $"{100 * countMissingVisitPerLocality / metersInLocality:N1}{strToolTip}" };
                        case col2Header:
                            return new MatrixDataCell(countNoVisitMoreThanOneYearPerLocality) { ToolTip = $"{100 * countNoVisitMoreThanOneYearPerLocality / metersInLocality:N1}{strToolTip}" };
                        case col3Header:
                            return new MatrixDataCell(countVisitInCurrentYearPerLocality) { ToolTip = $"{100 * countVisitInCurrentYearPerLocality / metersInLocality:N1}{strToolTip}" };
                        default:
                            throw new ArgumentOutOfRangeException("column.Header is unknown");
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по типу счётчика и году поверки
            matrixHeader = "Свод по типу счётчика и году поверки";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => allMeterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                GetColumnHeaderValuesFunc = () => allMeters.Select(i => i.ГодПоверкиДляОтчётов).Distinct().OrderBy(i => i).Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    SummaryInfoGroupItem group = allMeterTypes.FirstOrDefault(i => i.Key.ToString() == row.Header);
                    List<Meter> list = group.Value.Where(i => i.ГодПоверкиДляОтчётов == column.Header).ToList();
                    if (list != null)
                    {
                        return new MatrixDataCell(list.Count);
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по типам электронных счётчиков и году поверки
            matrixHeader = "Свод по типам электронных счётчиков и году поверки";
            matrixDescription = "* количество электронных счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => electronicMeterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                GetColumnHeaderValuesFunc = () => allMeters.Select(i => i.ГодПоверкиДляОтчётов).Distinct().OrderBy(i => i).Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    SummaryInfoGroupItem group = electronicMeterTypes.FirstOrDefault(i => i.Key.ToString() == row.Header);
                    List<Meter> list = group.Value.Where(i => i.ГодПоверкиДляОтчётов == column.Header).ToList();
                    if (list != null)
                    {
                        return new MatrixDataCell(list.Count);
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по населенному пункту и количеству просроченных
            matrixHeader = "Свод по населенному пункту и количеству просроченных";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                ShowRowsTotal = false,
                GetRowHeaderValuesFunc = () => meterPerLocality.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                GetColumnHeaderValuesFunc = () => new string[] { "Количество\nнеповеренных", "% от количества в н.п." }.Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    SummaryInfoGroupItem group = meterPerLocality.FirstOrDefault(i => i.Key.ToString() == row.Header);
                    int countPerLoacality = group.Value.Count;
                    List<Meter> list = group.Value.Where(i => i.Поверен == false).ToList();
                    if (list == null)
                    {
                        return new MatrixDataCell(0);
                    }

                    int count = list.Count;
                    if (column.Header == "% от количества в н.п.")
                    {
                        return new MatrixDataCell($"{100 * count / countPerLoacality:N1}%");
                    }
                    else
                    {
                        return new MatrixDataCell(count) { ToolTip = $"{100 * count / metersCount:N1}% от общего количества счётчиков" };
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по населенному пункту и принципу действия счётчика
            matrixHeader = "Свод по населенному пункту и принципу действия счётчика";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => meterPerLocality.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                GetColumnHeaderValuesFunc = () => allMeters.Select(i => i.Принцип).Distinct().Select(i => MatrixHeaderCell.CreateColumnHeader(i)),
                GetDataCellFunc = (row, column) =>
                {
                    SummaryInfoGroupItem group = meterPerLocality.FirstOrDefault(i => i.Key.ToString() == row.Header);
                    List<Meter> list = group.Value.Where(i => i.Принцип == column.Header).ToList();
                    if (list != null)
                    {
                        int count = list.Count;
                        return new MatrixDataCell(count) { ToolTip = $"{100 * count / metersCount:N1}%" };
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion

            #region Свод по подстанции и принципу действия
            matrixHeader = "Свод по подстанции и принципу действия";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix(matrixDefinition)
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => allMeters.Select(i => i.Подстанция).Distinct().OrderBy(i => i).Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                GetColumnHeaderValuesFunc = () => allMeters
                    .Select(i => i.Принцип).Distinct()
                    .Select(i => MatrixHeaderCell.CreateHeaderCell(i, allMeters.Select(ii => ii.Фаз).Distinct().OrderBy(ii => ii).Select(ii => MatrixHeaderCell.CreateHeaderCell(ii.ToString(AppSettings.CurrentCulture))).ToList<IMatrixHeader>())),
                GetDataCellFunc = (row, column) =>
                {
                    _ = byte.TryParse(column.Header, out byte фаз);

                    List<Meter> list = allMeters
                        .Where(i => i.Подстанция == row.Header)
                        .Where(i => i.Принцип == column.Parent.Header)
                        .Where(i => i.Фаз == фаз)
                        .ToList();
                    return list == null ? new MatrixDataCell(string.Empty) : new MatrixDataCell(list.Count);
                },
            }) == false)
                return;
            #endregion

            #region Свод по типу счётчика и году будущей поверки
            matrixHeader = "Перспективный план поверки счётчиков";
            matrixDescription = "* количество счётчиков";

            matrixDefinition = getMatrixDefinition(matrixHeader);

            if (add(new Matrix()
            {
                Header = matrixHeader,
                Description = matrixDescription,
                GetRowHeaderValuesFunc = () => allMeterTypes.Select(i => MatrixHeaderCell.CreateRowHeader(i.Key.ToString())),
                GetColumnHeaderValuesFunc = () => Enumerable.Range(curYear, yearsCount + 1).Select(i => MatrixHeaderCell.CreateColumnHeader(i.ToString(AppSettings.CurrentCulture))),
                GetDataCellFunc = (row, column) =>
                {
                    SummaryInfoGroupItem group = allMeterTypes.FirstOrDefault(i => i.Key.ToString() == row.Header);
                    int.TryParse(column.Header, out int year);
                    List<Meter> list = group.Value.Where(i => (i.ГодПоверки + i.ПериодПоверки) == year).ToList();
                    if (list != null)
                    {
                        return new MatrixDataCell(list.Count);
                    }
                    else
                    {
                        return new MatrixDataCell(string.Empty);
                    }
                },
            }) == false)
                return;
            #endregion
        }

        #endregion Private methods

        public override int GetHashCode()
        {
            System.Guid guid = new System.Guid("1A555AD8-D371-4E35-9852-0967B8EC0455");
            return guid.GetHashCode();
        }
    }
}
