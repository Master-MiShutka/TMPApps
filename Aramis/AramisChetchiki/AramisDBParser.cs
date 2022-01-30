namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using DBF;
    using TMP.WORK.AramisChetchiki.Model;
    using TMPApplication;

    internal partial class AramisDBParser
    {
        public class DataFileRecord
        {
            public DataFileRecord() { }

            public string FileName { get; set; }

            public string Hash { get; set; }

            public DateTime LastModified { get; set; }
        }

        private const string NOTFOUNDED = "#Н/Д";
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Action<Exception> callBackAction;

        private readonly string aramisDbPath;
        private readonly string pathDBF;
        private readonly string pathDBFC;

        private readonly List<string> errors = new();
        private readonly ViewModel.LoadingDataViewModel workTasksProgressViewModel;

        private Dictionary<string, ICollection<KARTSCH>> dictionaryKARTSCH;
        private Dictionary<string, ICollection<KARTSCH>> dictionaryKARTSCHRemoved;
        private Dictionary<string, KARTTSCH> dictionaryKARTTSCH;
        private Dictionary<string, ASVIDYST> dictionaryASVIDYST;
        private Dictionary<string, KartTn> dictionaryKartTn;
        private Dictionary<string, KartSs> dictionaryKartSs;
        private Dictionary<string, KartSt> dictionaryKartSt;
        private Dictionary<string, KartTpr> dictionaryKartTpr;
        private Dictionary<string, KartIsp> dictionaryKartIsp;
        private Dictionary<string, KartKat> dictionaryKartKat;
        private Dictionary<int, Kartps> dictionaryKartps;
        private Dictionary<string, Kartfid> dictionaryKartfid;
        private Dictionary<int, Kartktp> dictionaryKartktp;
        private Dictionary<string, ASKONTR> dictionaryASKONTR;
        private Dictionary<string, KartPd> dictionaryKartPd;
        private IReadOnlyCollection<KARTAB> collectionKARTAB;
        private Dictionary<string, ICollection<Assmena>> dictionaryAssmena;
        private Dictionary<ulong, RemovAb> dictionaryRemovAb;

        // private Dictionary<string, ICollection<KartKvGd>> dictionaryKartKvGd;

        private MeterComparerByPersonalID meterComparerByPersonalID = new();

        private const int bufferedStreamBufferSize = 1_200_000;

        private const string DATA_FILES_FOLDER_NAME = "Data";
        private const string DATA_FILE_NAME_HASHES = "dbFilesHashes";
        private const string DATA_FILE_EXTENSION = ".data";
        private readonly string dataFilesPath = Path.Combine(TMPApp.ExecutionPath, DATA_FILES_FOLDER_NAME);

        private Dictionary<string, DataFileRecord> dataFilesInfo;

        private IComparer<DateOnly> dateOnlyComparerByAscending = new DateOnlyComparerByAscending();

        private IComparer<DateOnly> dateOnlyComparerByDescending = new DateOnlyComparerByDescending();

        public static string ErrorsFileName => "Ошибки.txt";

        public AramisDBParser(AramisDataInfo aramisDataInfo, ViewModel.LoadingDataViewModel workTasksProgressViewModel)
        {
            this.callBackAction = (e) => logger?.Error(e);

            if (aramisDataInfo == null)
            {
                throw new ArgumentNullException(nameof(aramisDataInfo));
            }

            this.workTasksProgressViewModel = workTasksProgressViewModel ?? throw new ArgumentNullException(nameof(workTasksProgressViewModel));
            this.aramisDbPath = aramisDataInfo.AramisDbPath;
            this.pathDBF = Path.Combine(this.aramisDbPath, "DBF");
            this.pathDBFC = Path.Combine(this.aramisDbPath, "DBFC");
        }

        public async Task<AramisData> GetDataAsync()
        {
            logger?.Info(">>> TMP.WORK.AramisChetchiki.AramisDBParser>GetData");

            System.Diagnostics.Debug.Assert(this.MainWindow != null);

            DispatcherExtensions.InUi(() => this.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate);

            // чтение хэшей из файла
            if (File.Exists(Path.Combine(this.dataFilesPath, DATA_FILE_NAME_HASHES)))
            {
                try
                {
                    string jsonString = File.ReadAllText(Path.Combine(this.dataFilesPath, DATA_FILE_NAME_HASHES));

                    DataFileRecord[] d = System.Text.Json.JsonSerializer.Deserialize<DataFileRecord[]>(jsonString);
                    this.dataFilesInfo = d.ToDictionary(i => i.FileName, i => i);
                }
                catch (Exception ex)
                {
                    this.callBackAction(ex);
                    this.dataFilesInfo = new Dictionary<string, DataFileRecord>();
                }
            }
            else
            {
                this.dataFilesInfo = new Dictionary<string, DataFileRecord>();
            }

            AramisData data = null;

            string msg = string.Empty;
            try
            {
                data = new AramisData();

                TaskScheduler taskScheduler = TaskScheduler.Current;

                // просмотр полезного отпуска (FORMAB71)
                /* var task = Task.Run(() => this.ParseAramisDbTable<ElectricitySupply>("полезный отпуск: FORMAB71", Path.Combine(this.pathDBF, "FORMAB71.DBF"), this.ParseElectricitySupplyRecord));

                var taskElectricitySupplyInfo = task.ContinueWith(
                    t =>
                    {
                        data.ElectricitySupplyInfo = t.Result;
                    }, taskScheduler); */

                // чтение оплат за электроэнергию
                Task taskPayments = Task.Run<IList<RawPaymentData>>(this.GetPaymentDatas)
                    .ContinueWith(
                        t =>
                        {
                            WorkTask workTask = new("группировка произведенных оплат абонентом")
                            {
                                Progress = 0d, ChildProgress = 0d,
                            };
                            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
                            int processedRecords = 0;

                            // группировка оплат по лицевому счету, затем по периоду оплаты (в одном периоде может быть несколько оплат)
                            IList<RawPaymentData> list = t.Result;
                            Dictionary<ulong, Dictionary<DateOnly, List<RawPaymentData>>> result = list
                                .GroupBy(i => i.Лицевой)
                                .ToDictionary(i => i.Key, e => e.GroupBy(element => element.ПериодОплаты).ToDictionary(i => i.Key, j => j.ToList()));

                            data.Payments = new();

                            int totalRecords = result.Count;
                            int childProcessed = 0, childCount = 0;
                            SortedList<DateOnly, Payment> sortedList = new SortedList<DateOnly, Payment>(this.dateOnlyComparerByAscending);

                            // по всем лицевым счетам
                            foreach (KeyValuePair<ulong, Dictionary<DateOnly, List<RawPaymentData>>> item in result)
                            {
                                sortedList.Clear();

                                childProcessed = 0;
                                childCount = item.Value.Count;

                                // по всем временным периодам
                                foreach (KeyValuePair<DateOnly, List<RawPaymentData>> p in item.Value)
                                {
                                    Payment payment = new Payment
                                    {
                                        Лицевой = item.Key,
                                        ПериодОплаты = p.Key,

                                        // массив оплат в этом периоде
                                        Payments = new PaymentData[p.Value.Count],
                                    };

                                    int index = 0;

                                    // по всем оплатам за этот период
                                    IOrderedEnumerable<RawPaymentData> pp = p.Value.OrderBy(i => i.ПредыдущееПоказание);
                                    foreach (RawPaymentData rawPayment in pp)
                                    {
                                        payment.Payments[index++] = PaymentData.FromRawData(rawPayment);
                                    }

                                    payment.ПредыдущееПоказание = payment.Payments.Min(i => i.ПредыдущееПоказание);
                                    payment.ПоследнееПоказание = payment.Payments.Max(i => i.ПоследнееПоказание);
                                    payment.СуммаОплаты = payment.Payments.Sum(i => i.СуммаОплатыРасчётная);

                                    sortedList.Add(payment.ПериодОплаты, payment);

                                    childProcessed++;
                                    workTask.UpdateUI(++processedRecords, totalRecords, ++childProcessed, childCount);
                                }

                                if (data.Payments.ContainsKey(item.Key))
                                {
                                    foreach (KeyValuePair<DateOnly, Payment> element in sortedList)
                                    {
                                        data.Payments[item.Key].Add(element.Value);
                                    }
                                }
                                else
                                {
                                    data.Payments.Add(item.Key, sortedList.Values);
                                }

                                workTask.UpdateUI(++processedRecords, totalRecords);
                            }

                            workTask.IsCompleted = true;
                            this.workTasksProgressViewModel.WorkTasks.Remove(workTask);
                        }, taskScheduler);

                // чтение контрольных показаний по лицевому счету
                Task taskControlData = Task.Run<IReadOnlyCollection<ControlData>>(() => this.ParseAramisDbTable<ControlData>("просмотр контрольных показаний", Path.Combine(this.pathDBF, "KARTKP.DBF"), this.ParseControlData))
                    .ContinueWith(
                        t =>
                        {
                            IList<ControlData> result = this.SortData(t.Result);

                            data.MetersControlData = new();

                            foreach (ControlData item in result)
                            {
                                if (data.MetersControlData.ContainsKey(item.Лицевой))
                                {
                                    data.MetersControlData[item.Лицевой].Add(item);
                                }
                                else
                                {
                                    data.MetersControlData.Add(item.Лицевой, new List<ControlData>(new[] { item }));
                                }
                            }
                        }, taskScheduler);

                // чтение таблиц
                Task taskMain = Task.Run(this.ReadTables);

                // получение названия РЭС
                Task taskPrepareTables = taskMain.ContinueWith(
                    t =>
                    {
                        (data.Info as AramisDataInfo).DepartamentName = this.dictionaryKartPd.Values.First().PNPD;
                    }, taskScheduler);

                IList<Meter> notDeletedMeters = null;
                Task taskGetAbonentsAndMeter = taskPrepareTables.ContinueWith(
                    t =>
                    {
                        // проход по всем абонентам
                        data.Meters = new ReadOnlyCollection<Meter>(this.GetMeters());
                        notDeletedMeters = data.Meters.Where(i => i.Удалён == false).ToList();
                    }, taskScheduler);

                // удаленные абоненты
                Task taskRemovedAbonents = taskGetAbonentsAndMeter.ContinueWith(
                    t =>
                    {
                        List<Meter> deletedMeters = data.Meters.Where(i => i.Удалён == true).ToList();
                        int totalRecords = deletedMeters.Count;
                        int processedRecords = 0;

                        WorkTask workTask = new("обработка удаленных абонентов")
                        {
                            Progress = 0d,
                        };
                        this.workTasksProgressViewModel.WorkTasks.Add(workTask);

                        for (int i = 0; i < totalRecords; i++)
                        {
                            Meter meter = deletedMeters[i];

                            if (this.dictionaryRemovAb.ContainsKey(meter.Лицевой))
                            {
                                meter.ДатаУдаления = this.dictionaryRemovAb[meter.Лицевой].DATE_ZAP;
                            }

                            workTask.UpdateUI(++processedRecords, totalRecords);
                        }

                        workTask.IsCompleted = true;
                        this.workTasksProgressViewModel.WorkTasks.Remove(workTask);

                    }, taskScheduler);

                Task taskMakeSummaryInfos = taskGetAbonentsAndMeter.ContinueWith(
                    t =>
                    {
                        // создание сводной информации
                        data.SummaryInfos = this.BuildSummaryInfo(notDeletedMeters);
                    }, taskScheduler);

                /* var taskPostMakeElectricitySupply = taskGetAbonentsAndMeter.ContinueWith(
                    t =>
                    {
                        Task.WaitAll(taskElectricitySupplyInfo);

                        // заполнение недостающих полей в таблице полезного отпуска электроэнергии
                        this.AddAdditionalInfoToElectricitySupply(data.ElectricitySupplyInfo, notDeletedMeters);
                    }, taskScheduler); */

                // просмотр замен счётчиков
                Task taskGetChangesOfMeters = taskPrepareTables.ContinueWith(
                    t =>
                    {
                        IList<ChangeOfMeter> result = this.GetChangesOfMeters();

                        data.ChangesOfMeters = new();

                        WorkTask workTask = new("обработка замен счётчиков")
                        {
                            Progress = 0d,
                        };
                        this.workTasksProgressViewModel.WorkTasks.Add(workTask);
                        int totalRecords = result.Count;
                        int processedRecords = 0;

                        foreach (ChangeOfMeter item in result)
                        {
                            if (data.ChangesOfMeters.ContainsKey(item.Лицевой))
                            {
                                data.ChangesOfMeters[item.Лицевой].Add(item);
                            }
                            else
                            {
                                data.ChangesOfMeters.Add(item.Лицевой, new List<ChangeOfMeter>(new[] { item }));
                            }

                            workTask.UpdateUI(++processedRecords, totalRecords);
                        }

                        workTask.IsCompleted = true;
                        this.workTasksProgressViewModel.WorkTasks.Remove(workTask);

                    }, taskScheduler);

                // расчет среднемесячного потребления
                Task taskCalcAverageConsumptionByAbonents = taskGetAbonentsAndMeter.ContinueWith(
                    t =>
                    {
                        WorkTask workTask = new("расчет среднемесячного потребления: ожидание готовности")
                        {
                            IsIndeterminate = true,
                        };
                        this.workTasksProgressViewModel.WorkTasks.Add(workTask);

                        Task.WaitAll(taskGetChangesOfMeters, taskPayments, taskControlData);

                        workTask.Status = "начинаем";
                        workTask.IsCompleted = true;
                        this.workTasksProgressViewModel.WorkTasks.Remove(workTask);

                        this.CalcAverageConsumptionByAbonents(data);
                    }, taskScheduler);

                await Task.WhenAll(
                    taskMain, taskPayments, taskControlData, taskPrepareTables, taskGetAbonentsAndMeter, taskGetChangesOfMeters, taskMakeSummaryInfos, taskCalcAverageConsumptionByAbonents, taskRemovedAbonents) //, taskPostMakeElectricitySupply)
                    .ConfigureAwait(false);

                // сохранение хэшей в файл
                System.Text.Json.JsonSerializerOptions options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string jsonString = System.Text.Json.JsonSerializer.Serialize(this.dataFilesInfo.Select(i => i.Value).ToArray(), options);
                File.WriteAllText(Path.Combine(this.dataFilesPath, DATA_FILE_NAME_HASHES), jsonString);
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
            }
            finally
            {
                // сообщение об ошибках
                if (this.errors.Count > 0)
                {
                    string errors = string.Join("\n", this.errors);
                    File.WriteAllText(ErrorsFileName, errors, System.Text.Encoding.UTF8);
                    DispatcherExtensions.InUi(() =>
                        App.ShowWarning(string.Format(AppSettings.CurrentCulture, "При чтении базы обнаружено {0} ошибок.\nОшибки сохранены в файл '{1}' в папке с программой.", this.errors.Count, ErrorsFileName)));
                }

                DispatcherExtensions.InUi(() => this.MainWindow.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal);
            }

            return data;
        }

        /// <summary>
        /// чтение таблиц
        /// </summary>
        private void ReadTables()
        {
            int totalRecords = 0, processedRecords = 0;

            System.Threading.Thread.CurrentThread.Name = "AramisDBParser-ReadTables";

            string taskName = "чтение таблиц базы данных";

            WorkTask workTask = new("чтение справочников")
            {
                Status = taskName,
                IsIndeterminate = true,
            };
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);

            int totalTables = 18;
            int processedTables = 1;

            void _(string msg)
            {
                workTask.UpdateUI(processedTables, totalTables, ++processedRecords, totalRecords, message: taskName + ": " + msg, stepNameString: "обработка таблицы");
            }

            void init(string msg)
            {
                _(msg);
                workTask.SetChildProgress(0, 0);
            }
;

            void finish(string msg)
            {
                processedTables++;
                _(msg);
            }

            void setChildProgress(int childValue, int childTotal)
            {
                workTask.SetChildProgress(childTotal, childValue);
            }

            string tableName = "справочник установленных счётчиков";
            init(tableName);
            this.dictionaryKARTSCH = new();
            IReadOnlyCollection<KARTSCH> tableKARTSCH = this.ParseAramisDbTable<KARTSCH>(tableName, Path.Combine(this.pathDBF, "KARTSCH.DBF"), this.ParseKARTSCHRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KARTSCH item in tableKARTSCH)
            {
                if (this.dictionaryKARTSCH.ContainsKey(item.LIC_SCH))
                {
                    this.dictionaryKARTSCH[item.LIC_SCH].Add(item);
                }
                else
                {
                    this.dictionaryKARTSCH.Add(item.LIC_SCH, new List<KARTSCH>(new[] { item }));
                }
            }

            finish(tableName);

            tableName = "справочник снятых счётчиков";
            init(tableName);
            this.dictionaryKARTSCHRemoved = new();
            IReadOnlyCollection<KARTSCH> tableKARTTSCHRemoved = this.ParseAramisDbTable<KARTSCH>(tableName, Path.Combine(this.pathDBFC, "KARTSCH.DBF"), this.ParseKARTSCHRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KARTSCH item in tableKARTTSCHRemoved)
            {
                if (this.dictionaryKARTSCHRemoved.ContainsKey(item.LIC_SCH))
                {
                    this.dictionaryKARTSCHRemoved[item.LIC_SCH].Add(item);
                }
                else
                {
                    this.dictionaryKARTSCHRemoved.Add(item.LIC_SCH, new List<KARTSCH>(new[] { item }));
                }
            }

            finish(tableName);

            tableName = "справочник типов счетчиков";
            init(tableName);
            this.dictionaryKARTTSCH = new();
            IReadOnlyCollection<KARTTSCH> tableKARTTSCH = this.ParseAramisDbTable<KARTTSCH>(tableName, Path.Combine(this.pathDBFC, "KARTTSCH.DBF"), this.ParseKARTTSCHRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KARTTSCH item in tableKARTTSCH)
            {
                if (this.dictionaryKARTTSCH.ContainsKey(item.COD_TSCH))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'KARTTSCH.DBF') поле 'COD_TSCH' с кодом {item.COD_TSCH}");
                    continue;
                }

                this.dictionaryKARTTSCH.Add(item.COD_TSCH, item);
            }

            finish(tableName);

            tableName = "справочник мест установки";
            init(tableName);
            this.dictionaryASVIDYST = new();
            IReadOnlyCollection<ASVIDYST> tableASVIDYST = this.ParseAramisDbTable<ASVIDYST>(tableName, Path.Combine(this.pathDBFC, "ASVIDYST.DBF"), this.ParseASVIDYSTRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (ASVIDYST item in tableASVIDYST)
            {
                if (this.dictionaryASVIDYST.ContainsKey(item.COD_SS))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'ASVIDYST.DBF') поле 'COD_SS' с кодом {item.COD_SS}");
                    continue;
                }

                this.dictionaryASVIDYST.Add(item.COD_SS, item);
            }

            finish(tableName);

            tableName = "справочник населенных пунктов";
            init(tableName);
            this.dictionaryKartTn = new();
            IReadOnlyCollection<KartTn> tableKartTn = this.ParseAramisDbTable<KartTn>(tableName, Path.Combine(this.pathDBFC, "KartTn.DBF"), this.ParseKartTnRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KartTn item in tableKartTn)
            {
                if (this.dictionaryKartTn.ContainsKey(item.COD_TN))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'KartTn.DBF') поле 'COD_TN' с кодом {item.COD_TN}");
                    continue;
                }

                this.dictionaryKartTn.Add(item.COD_TN, item);
            }

            finish(tableName);

            tableName = "справочник сельских советов";
            init(tableName);
            this.dictionaryKartSs = new();
            IReadOnlyCollection<KartSs> tableKartSs = this.ParseAramisDbTable<KartSs>(tableName, Path.Combine(this.pathDBFC, "KartSs.DBF"), this.ParseKartSsRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KartSs item in tableKartSs)
            {
                if (this.dictionaryKartSs.ContainsKey(item.COD_SS))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'KartSs.DBF') поле 'COD_SS' с кодом {item.COD_SS}");
                    continue;
                }

                this.dictionaryKartSs.Add(item.COD_SS, item);
            }

            finish(tableName);

            tableName = "справочник улиц";
            init(tableName);
            this.dictionaryKartSt = new();
            IReadOnlyCollection<KartSt> tableKartSt = this.ParseAramisDbTable<KartSt>(tableName, Path.Combine(this.pathDBFC, "KartSt.DBF"), this.ParseKartStRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KartSt item in tableKartSt)
            {
                if (this.dictionaryKartSt.ContainsKey(item.COD_ST))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'KartSt.DBF') поле 'COD_ST' с кодом {item.COD_ST}");
                    continue;
                }

                this.dictionaryKartSt.Add(item.COD_ST, item);
            }

            finish(tableName);

            tableName = "справочник токоприемников";
            init(tableName);
            this.dictionaryKartTpr = new();
            IReadOnlyCollection<KartTpr> tableKartTpr = this.ParseAramisDbTable<KartTpr>(tableName, Path.Combine(this.pathDBFC, "KartTpr.DBF"), this.ParseKartTprRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KartTpr item in tableKartTpr)
            {
                if (this.dictionaryKartTpr.ContainsKey(item.COD_TPR))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'KartTpr.DBF') поле 'COD_TPR' с кодом {item.COD_TPR}");
                    continue;
                }

                this.dictionaryKartTpr.Add(item.COD_TPR, item);
            }

            finish(tableName);

            tableName = "справочник использования";
            init(tableName);
            this.dictionaryKartIsp = new();
            IReadOnlyCollection<KartIsp> tableKartIsp = this.ParseAramisDbTable<KartIsp>(tableName, Path.Combine(this.pathDBFC, "KartIsp.DBF"), this.ParseKartIspRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KartIsp item in tableKartIsp)
            {
                if (this.dictionaryKartIsp.ContainsKey(item.COD_ISP))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'KartIsp.DBF') поле 'COD_ISP' с кодом {item.COD_ISP}");
                    continue;
                }

                this.dictionaryKartIsp.Add(item.COD_ISP, item);
            }

            finish(tableName);

            tableName = "справочник категорий";
            init(tableName);
            this.dictionaryKartKat = new();
            IReadOnlyCollection<KartKat> tableKartKat = this.ParseAramisDbTable<KartKat>(tableName, Path.Combine(this.pathDBFC, "KartKat.DBF"), this.ParseKartKatRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KartKat item in tableKartKat)
            {
                if (this.dictionaryKartKat.ContainsKey(item.COD_KAT))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'KartKat.DBF') поле 'COD_KAT' с кодом {item.COD_KAT}");
                    continue;
                }

                this.dictionaryKartKat.Add(item.COD_KAT, item);
            }

            finish(tableName);

            tableName = "подстанции";
            init(tableName);
            this.dictionaryKartps = new();
            IReadOnlyCollection<Kartps> tableKartps = this.ParseAramisDbTable<Kartps>(tableName, Path.Combine(this.pathDBFC, "Kartps.DBF"), this.ParseKartpsRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (Kartps item in tableKartps)
            {
                if (this.dictionaryKartps.ContainsKey(item.ПОДСТАНЦИЯ))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'Kartps.DBF') поле 'ПОДСТАНЦИЯ' с кодом {item.ПОДСТАНЦИЯ}");
                    continue;
                }

                this.dictionaryKartps.Add(item.ПОДСТАНЦИЯ, item);
            }

            finish(tableName);

            tableName = "фидера 10 кВ";
            init(tableName);
            this.dictionaryKartfid = new();
            IReadOnlyCollection<Kartfid> tableKartfid = this.ParseAramisDbTable<Kartfid>(tableName, Path.Combine(this.pathDBFC, "Kartfid.DBF"), this.ParseKartfidRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (Kartfid item in tableKartfid)
            {
                if (this.dictionaryKartfid.ContainsKey(item.ФИДЕР))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'Kartfid.DBF') поле 'ФИДЕР' с кодом {item.ФИДЕР}");
                    continue;
                }

                this.dictionaryKartfid.Add(item.ФИДЕР, item);
            }

            finish(tableName);

            tableName = "пс 10 кВ";
            init(tableName);
            this.dictionaryKartktp = new();
            IReadOnlyCollection<Kartktp> tableKartktp = this.ParseAramisDbTable<Kartktp>(tableName, Path.Combine(this.pathDBFC, "Kartktp.DBF"), this.ParseKartktpRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (Kartktp item in tableKartktp)
            {
                if (this.dictionaryKartktp.ContainsKey(item.КОД_ТП))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'Kartktp.DBF') поле 'КОД_ТП' с кодом {item.КОД_ТП}");
                    continue;
                }

                this.dictionaryKartktp.Add(item.КОД_ТП, item);
            }

            finish(tableName);

            tableName = "контролёры";
            init(tableName);
            this.dictionaryASKONTR = new();
            IReadOnlyCollection<ASKONTR> tableASKONTR = this.ParseAramisDbTable<ASKONTR>(tableName, Path.Combine(this.pathDBFC, "ASKONTR.DBF"), this.ParseASKONTRRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (ASKONTR item in tableASKONTR)
            {
                if (this.dictionaryASKONTR.ContainsKey(item.КОД_КОН))
                {
                    this.errors.Add($"Найден дубликат в справочнике '{tableName}' (файл 'ASKONTR.DBF') поле 'КОД_КОН' с кодом {item.КОД_КОН}");
                    continue;
                }

                this.dictionaryASKONTR.Add(item.КОД_КОН, item);
            }

            finish(tableName);

            tableName = "справочник абонентов";
            init(tableName);
            this.collectionKARTAB = this.ParseAramisDbTable<KARTAB>(tableName, Path.Combine(this.pathDBF, "KARTAB.DBF"), this.ParseKARTABRecord,
                skipDeletedRecords: false,
                removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            finish(tableName);

            tableName = "наименование РЭС";
            init(tableName);
            this.dictionaryKartPd = new();
            IReadOnlyCollection<KartPd> tableKartPd = this.ParseAramisDbTable<KartPd>(tableName, Path.Combine(this.pathDBFC, "KartPd.DBF"), this.ParseKartPdRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (KartPd item in tableKartPd)
            {
                this.dictionaryKartPd.Add(item.COD_PD, item);
            }

            finish(tableName);

            tableName = "перечень удаленных абонентов";
            init(tableName);
            this.dictionaryRemovAb = new();
            IReadOnlyCollection<RemovAb> tableRemovAb = this.ParseAramisDbTable<RemovAb>(tableName, Path.Combine(this.pathDBF, "RemovAb.DBF"), this.ParseRemovAbRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (RemovAb item in tableRemovAb)
            {
                // сохранение самой новой записи
                if (this.dictionaryRemovAb.ContainsKey(item.LIC_SCH))
                {
                    if (this.dictionaryRemovAb[item.LIC_SCH].DATE_ZAP < item.DATE_ZAP)
                    {
                        this.dictionaryRemovAb.Remove(item.LIC_SCH);

                        this.dictionaryRemovAb.Add(item.LIC_SCH, item);
                    }
                }
                else
                {
                    this.dictionaryRemovAb.Add(item.LIC_SCH, item);
                }
            }

            finish(tableName);

            // kaRtkvgd оплаты
            // kaRtkvgd.daTe_r 'Дата оплаты', kaRtkvgd.РАЗН_КН+kaRtkvgd.РАЗН_КС 'кВтч', kaRtkvgd.suMma_kn+kaRtkvgd.suMma_kc 'сумма (руб)', kaRtkvgd.peNya_k 'Пеня',

            workTask.IsCompleted = true;
        }

        private IReadOnlyCollection<T> ParseAramisDbTable<T>(
            string taskDescription,
            string tableFileName,
            Func<DbfRecord, T> parseDelegate,
            bool skipDeletedRecords = true,
            bool removeTaskAfterCompleted = false,
            Action<int, int> progressCallback = null)
        {
            Model.WorkTask workTask = new(taskDescription);
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            System.Collections.Concurrent.ConcurrentBag<T> data = null;

            try
            {
                IList<T> result = this.CheckAndLoadFromCache<T>(tableFileName, ref workTask);
                if (result != null)
                {
                    data = new System.Collections.Concurrent.ConcurrentBag<T>(result);
                }

                if (data == null)
                {
                    using DBF.DbfReader dbfReader = new DbfReader(tableFileName, System.Text.Encoding.GetEncoding(866), skipDeletedRecords);
                    {
                        int processedRows = 0;
                        int totalRows = dbfReader.DbfTable.Header.NumberOfRecords;
                        int currYear = DateTime.Now.Year;
                        workTask.UpdateStatus($"количество строк в таблице: {totalRows:N0}");

                        data = new System.Collections.Concurrent.ConcurrentBag<T>();

                        void toParse(DbfRecord record)
                        {
                            if (record != null)
                            {
                                data.Add(parseDelegate(record));

                                workTask.UpdateUI(++processedRows, totalRows, stepNameString: "строка");

                                progressCallback?.Invoke(processedRows, totalRows);
                            }
                        }

                        dbfReader.ParseRecords(toParse);
                        workTask.UpdateUI(totalRows, totalRows, stepNameString: "строка");

                        this.StoreHashAndSaveData(tableFileName, ref workTask, data.ToArray());
                    }
                }
            }
            catch (IOException ioex)
            {
                logger?.Error(ioex, TMPApp.GetExceptionDetails(ioex));
                return null;
            }
            catch (Exception ex)
            {
                logger?.Error($">>> TMP.WORK.AramisChetchiki.AramisDBParser>ParseAramisDbTAble<{typeof(T)}>\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                return null;
            }

            workTask.IsCompleted = true;

            if (removeTaskAfterCompleted)
            {
                this.workTasksProgressViewModel.WorkTasks.Remove(workTask);
            }

            return data;
        }

        private System.Windows.Window MainWindow
        {
            get
            {
                if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
                {
                    System.Windows.Window mainWindow = null;
                    void action()
                    {
                        mainWindow = TMPApp.Instance.MainWindow;
                    }

                    Application.Current.Dispatcher.Invoke(action, System.Windows.Threading.DispatcherPriority.Background);
                    return mainWindow;
                }
                else
                {
                    return TMPApp.Instance?.MainWindow;
                }
            }
        }
    }

    internal class MeterComparerByPersonalID : IComparer<Meter>
    {
        public int Compare(Meter x, Meter y)
        {
            return x.Лицевой.CompareTo(y.Лицевой);
        }
    }

    internal class DateOnlyComparerByAscending : IComparer<DateOnly>
    {
        public int Compare(DateOnly x, DateOnly y)
        {
            return x.CompareTo(y);
        }
    }

    internal class DateOnlyComparerByDescending : IComparer<DateOnly>
    {
        public int Compare(DateOnly x, DateOnly y)
        {
            return y.CompareTo(x);
        }
    }
}
