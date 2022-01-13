namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using DBF;
    using TMP.WORK.AramisChetchiki.Model;
    using TMP.WORK.AramisChetchiki.Properties;
    using TMPApplication;

    internal class AramisDBParser
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

        private readonly List<string> errors = new ();
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

        private MeterComparerByPersonalID meterComparerByPersonalID = new ();

        private const int bufferedStreamBufferSize = 1_200_000;

        private const string DATA_FILES_FOLDER_NAME = "Data";
        private const string DATA_FILE_NAME_HASHES = "dbFilesHashes";
        private const string DATA_FILE_EXTENSION = ".data";
        private readonly string dataFilesPath = Path.Combine(TMPApp.ExecutionPath, DATA_FILES_FOLDER_NAME);

        private Dictionary<string, DataFileRecord> dataFilesInfo;

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

                    var d = System.Text.Json.JsonSerializer.Deserialize<DataFileRecord[]>(jsonString);
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
                var taskPayments = Task.Run<IList<PaymentData>>(this.GetPaymentDatas)
                    .ContinueWith(
                        t =>
                        {
                            var result = t.Result;

                            data.PaymentDataInfo = new ();

                            foreach (var item in result)
                            {
                                if (data.PaymentDataInfo.ContainsKey(item.Лицевой))
                                {
                                    data.PaymentDataInfo[item.Лицевой].Add(item);
                                }
                                else
                                {
                                    data.PaymentDataInfo.Add(item.Лицевой, new List<PaymentData>(new[] { item }));
                                }
                            }
                        }, taskScheduler);

                // чтение контрольных показаний по лицевому счету
                var taskControlData = Task.Run<IReadOnlyCollection<ControlData>>(() => this.ParseAramisDbTable<ControlData>("просмотр контрольных показаний", Path.Combine(this.pathDBF, "KARTKP.DBF"), this.ParseControlData))
                    .ContinueWith(
                        t =>
                        {
                            var result = this.SortData(t.Result);

                            data.MetersControlData = new();

                            foreach (var item in result)
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
                var taskMain = Task.Run(this.ReadTables);

                // получение названия РЭС
                var taskPrepareTables = taskMain.ContinueWith(
                    t =>
                    {
                        (data.Info as AramisDataInfo).DepartamentName = this.dictionaryKartPd.Values.First().PNPD;
                    }, taskScheduler);

                IList<Meter> notDeletedMeters = null;
                var taskGetAbonentsAndMeter = taskPrepareTables.ContinueWith(
                    t =>
                    {
                        // проход по всем абонентам
                        data.Meters = new ReadOnlyCollection<Meter>(this.GetMeters());
                        notDeletedMeters = data.Meters.Where(i => i.Удалён == false).ToList();
                    }, taskScheduler);

                // удаленные абоненты
                var taskRemovedAbonents = taskGetAbonentsAndMeter.ContinueWith(
                    t =>
                    {
                        var deletedMeters = data.Meters.Where(i => i.Удалён == true).ToList();
                        int totalRecords = deletedMeters.Count;
                        int processedRecords = 0;

                        WorkTask workTask = new ("обработка удаленных абонентов")
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

                var taskMakeSummaryInfos = taskGetAbonentsAndMeter.ContinueWith(
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
                var taskGetChangesOfMeters = taskPrepareTables.ContinueWith(
                    t =>
                    {
                        var result = this.GetChangesOfMeters();

                        data.ChangesOfMeters = new ();

                        WorkTask workTask = new ("обработка замен счётчиков")
                        {
                            Progress = 0d,
                        };
                        this.workTasksProgressViewModel.WorkTasks.Add(workTask);
                        int totalRecords = result.Count;
                        int processedRecords = 0;

                        foreach (var item in result)
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
                var taskCalcAverageConsumptionByAbonents = taskGetAbonentsAndMeter.ContinueWith(
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
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
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
                    string errorsFileName = "Ошибки.txt";
                    string errors = string.Join("\n", this.errors);
                    File.WriteAllText(errorsFileName, errors, System.Text.Encoding.UTF8);
                    DispatcherExtensions.InUi(() =>
                        App.ShowWarning(string.Format(AppSettings.CurrentCulture, "При чтении базы обнаружено {0} ошибок.\nОшибки сохранены в файл '{1}' в папке с программой.", this.errors.Count, errorsFileName)));
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

            WorkTask workTask = new ("чтение справочников")
            {
                Status = taskName,
                IsIndeterminate = true,
            };
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);

            int totalTables = 18;
            int processedTables = 1;

            void _(string msg) => workTask.UpdateUI(processedTables, totalTables, ++processedRecords, totalRecords, message: taskName + ": " + msg, stepNameString: "обработка таблицы");

            void init(string msg)
            {
                _(msg);
                workTask.SetChildProgress(0, 0);
            };

            void finish(string msg)
            {
                processedTables++;
                _(msg);
            }

            void setChildProgress(int childValue, int childTotal) => workTask.SetChildProgress(childTotal, childValue);

            string tableName = "справочник установленных счётчиков";
            init(tableName);
            this.dictionaryKARTSCH = new ();
            IReadOnlyCollection<KARTSCH> tableKARTSCH = this.ParseAramisDbTable<KARTSCH>(tableName, Path.Combine(this.pathDBF, "KARTSCH.DBF"), this.ParseKARTSCHRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKARTSCH)
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
            this.dictionaryKARTSCHRemoved = new ();
            IReadOnlyCollection<KARTSCH> tableKARTTSCHRemoved = this.ParseAramisDbTable<KARTSCH>(tableName, Path.Combine(this.pathDBFC, "KARTSCH.DBF"), this.ParseKARTSCHRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKARTTSCHRemoved)
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
            this.dictionaryKARTTSCH = new ();
            IReadOnlyCollection<KARTTSCH> tableKARTTSCH = this.ParseAramisDbTable<KARTTSCH>(tableName, Path.Combine(this.pathDBFC, "KARTTSCH.DBF"), this.ParseKARTTSCHRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKARTTSCH)
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
            this.dictionaryASVIDYST = new ();
            IReadOnlyCollection<ASVIDYST> tableASVIDYST = this.ParseAramisDbTable<ASVIDYST>(tableName, Path.Combine(this.pathDBFC, "ASVIDYST.DBF"), this.ParseASVIDYSTRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableASVIDYST)
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
            this.dictionaryKartTn = new ();
            IReadOnlyCollection<KartTn> tableKartTn = this.ParseAramisDbTable<KartTn>(tableName, Path.Combine(this.pathDBFC, "KartTn.DBF"), this.ParseKartTnRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartTn)
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
            this.dictionaryKartSs = new ();
            IReadOnlyCollection<KartSs> tableKartSs = this.ParseAramisDbTable<KartSs>(tableName, Path.Combine(this.pathDBFC, "KartSs.DBF"), this.ParseKartSsRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartSs)
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
            this.dictionaryKartSt = new ();
            IReadOnlyCollection<KartSt> tableKartSt = this.ParseAramisDbTable<KartSt>(tableName, Path.Combine(this.pathDBFC, "KartSt.DBF"), this.ParseKartStRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartSt)
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
            this.dictionaryKartTpr = new ();
            IReadOnlyCollection<KartTpr> tableKartTpr = this.ParseAramisDbTable<KartTpr>(tableName, Path.Combine(this.pathDBFC, "KartTpr.DBF"), this.ParseKartTprRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartTpr)
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
            this.dictionaryKartIsp = new ();
            IReadOnlyCollection<KartIsp> tableKartIsp = this.ParseAramisDbTable<KartIsp>(tableName, Path.Combine(this.pathDBFC, "KartIsp.DBF"), this.ParseKartIspRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartIsp)
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
            this.dictionaryKartKat = new ();
            IReadOnlyCollection<KartKat> tableKartKat = this.ParseAramisDbTable<KartKat>(tableName, Path.Combine(this.pathDBFC, "KartKat.DBF"), this.ParseKartKatRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartKat)
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
            this.dictionaryKartps = new ();
            IReadOnlyCollection<Kartps> tableKartps = this.ParseAramisDbTable<Kartps>(tableName, Path.Combine(this.pathDBFC, "Kartps.DBF"), this.ParseKartpsRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartps)
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
            this.dictionaryKartfid = new ();
            IReadOnlyCollection<Kartfid> tableKartfid = this.ParseAramisDbTable<Kartfid>(tableName, Path.Combine(this.pathDBFC, "Kartfid.DBF"), this.ParseKartfidRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartfid)
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
            this.dictionaryKartktp = new ();
            IReadOnlyCollection<Kartktp> tableKartktp = this.ParseAramisDbTable<Kartktp>(tableName, Path.Combine(this.pathDBFC, "Kartktp.DBF"), this.ParseKartktpRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartktp)
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
            this.dictionaryASKONTR = new ();
            IReadOnlyCollection<ASKONTR> tableASKONTR = this.ParseAramisDbTable<ASKONTR>(tableName, Path.Combine(this.pathDBFC, "ASKONTR.DBF"), this.ParseASKONTRRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableASKONTR)
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
            collectionKARTAB = this.ParseAramisDbTable<KARTAB>(tableName, Path.Combine(this.pathDBF, "KARTAB.DBF"), this.ParseKARTABRecord,
                skipDeletedRecords: false,
                removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            finish(tableName);

            tableName = "наименование РЭС";
            init(tableName);
            this.dictionaryKartPd = new ();
            IReadOnlyCollection<KartPd> tableKartPd = this.ParseAramisDbTable<KartPd>(tableName, Path.Combine(this.pathDBFC, "KartPd.DBF"), this.ParseKartPdRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableKartPd)
            {
                this.dictionaryKartPd.Add(item.COD_PD, item);
            }
            finish(tableName);


            tableName = "перечень удаленных абонентов";
            init(tableName);
            this.dictionaryRemovAb = new ();
            IReadOnlyCollection<RemovAb> tableRemovAb = this.ParseAramisDbTable<RemovAb>(tableName, Path.Combine(this.pathDBF, "RemovAb.DBF"), this.ParseRemovAbRecord, removeTaskAfterCompleted: true, progressCallback: setChildProgress);
            foreach (var item in tableRemovAb)
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
                    this.dictionaryRemovAb.Add(item.LIC_SCH, item);
            }
            finish(tableName);

            // kaRtkvgd оплаты
            // kaRtkvgd.daTe_r 'Дата оплаты', kaRtkvgd.РАЗН_КН+kaRtkvgd.РАЗН_КС 'кВтч', kaRtkvgd.suMma_kn+kaRtkvgd.suMma_kc 'сумма (руб)', kaRtkvgd.peNya_k 'Пеня',

            workTask.IsCompleted = true;
        }

        /// <summary>
        /// Заполнение недостающих полей в таблице полезного отпуска электроэнергии
        /// </summary>
        /// <param name="electricitySupplies"></param>
        /// <param name="meters"></param>
        private void AddAdditionalInfoToElectricitySupply(IEnumerable<ElectricitySupply> electricitySupplies, IEnumerable<Meter> meters)
        {
            if (electricitySupplies == null || meters == null)
            {
                return;
            }

            Model.WorkTask workTask = new ("заполнение недостающих полей в таблице полезного отпуска электроэнергии");
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            int totalRows = (electricitySupplies as ICollection).Count;
            int processedRows = 0;

            try
            {
                List<Meter> metersList = new List<Meter>(meters);

                Parallel.ForEach(electricitySupplies, electricitySupply =>
                {
                    if (electricitySupply.Лицевой == 0)
                    {
                        return; 
                    }

                    // используем бинарный поиск в заранее отсортированном списке
                    int index = metersList.BinarySearch(new Meter { Лицевой = electricitySupply.Лицевой }, this.meterComparerByPersonalID);

                    Meter meter = null;
                    if (index > 0)
                    {
                        meter = metersList[index];
                    }
                    else
                    {
                        ;
                    }

                    if (meter != null)
                    {
                        electricitySupply.Адрес = meter.Адрес?.УлицаСДомомИКв;
                        electricitySupply.Населённый_пункт = meter.Адрес?.НаселённыйПункт;
                        electricitySupply.Подстанция = meter.Подстанция;
                        electricitySupply.Фидер10 = meter.Фидер10;

                        if (meter.НомерСчетчика == null)
                        {
                            logger?.Warn($"AddAdditionalInfoToElectricitySupply - Номер_счетчика is null, Лицевой: '{electricitySupply.Лицевой}'");
                        }

                        if (meter.ТП == null)
                        {
                            logger?.Warn($"AddAdditionalInfoToElectricitySupply - ТП is null, Лицевой: '{electricitySupply.Лицевой}'");
                        }
                        else
                        {
                            electricitySupply.НомерТП = meter.ТП.Number;
                            electricitySupply.ТипТП = meter.ТП.Type;
                            electricitySupply.НаименованиеТП = meter.ТП.Name;
                        }

                        electricitySupply.Фидер04 = meter.Фидер04;
                    }

                    workTask.UpdateUI(++processedRows, totalRows, stepNameString: "лицевой счет");
                });
            }
            catch (Exception ex)
            {
                logger?.Error($">>> TMP.WORK.AramisChetchiki.AramisDBParser>AddAdditionalInfoToElectricitySupply\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                return;
            }

            // fix
            workTask.UpdateUI(totalRows, totalRows, stepNameString: "лицевой счет");

            workTask.IsCompleted = true;
        }

        /// <summary>
        /// расчет среднемесячного потребления
        /// </summary>
        private void CalcAverageConsumptionByAbonents(AramisData aramisData)
        {
            if (aramisData.PaymentDataInfo == null || aramisData.MetersControlData == null || aramisData.Meters == null)
            {
                return;
            }

            Model.WorkTask workTask = new ("расчет среднемесячного потребления по абонентам");
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            int totalRows = (aramisData.Meters as ICollection).Count;
            int processedRows = 0;

            DateOnly now = DateOnly.FromDateTime(DateTime.Now);

            try
            {
                //Parallel.ForEach(c, meter =>
                foreach (var meter in aramisData.Meters)
                {
                    workTask.UpdateUI(++processedRows, totalRows, stepNameString: "лицевой счет");

                    if (meter.Лицевой == 0 || meter.Удалён)
                    {
                        continue;
                    }

                    int average = 0;
                    DateOnly endDate;
                    DateOnly startDate;
                    int startValue;
                    int endValue;

                    SortedSet<MeterEvent> meterEvents = new SortedSet<MeterEvent>();
                    meter.Events = meterEvents;

                    // контрольные показания
                    List<KeyValuePair<DateOnly, int>> controlDatas = new();
                    if (aramisData.MetersControlData.ContainsKey(meter.Лицевой) == true)
                    {
                        controlDatas = aramisData.MetersControlData[meter.Лицевой].FirstOrDefault().Data.OrderByDescending(i => i.Key).ToList();
                    }

                    // оплаты по лицевому счёту
                    List<PaymentData> payments = new();
                    if (aramisData.PaymentDataInfo.ContainsKey(meter.Лицевой) == true)
                    {
                        payments = aramisData.PaymentDataInfo[meter.Лицевой].OrderByDescending(i => i.ПериодОплаты).ToList();
                    }

                    // замены по лицевому счёту
                    List<ChangeOfMeter> changes = new();
                    if (aramisData.ChangesOfMeters.ContainsKey(meter.Лицевой) == true)
                    {
                        changes = aramisData.ChangesOfMeters[meter.Лицевой].OrderByDescending(i => i.ДатаЗамены).ToList();
                    }

                    // общий список для значений
                    List<Tuple<DateOnly, object>> list = new List<Tuple<DateOnly, object>>(payments.Count + changes.Count + controlDatas.Count);
                    // собираем список
                    foreach (var item in payments)
                    {
                        list.Add(new Tuple<DateOnly, object>(item.ПериодОплаты, item));
                    }
                    foreach (var item in changes)
                    {
                        list.Add(new Tuple<DateOnly, object>(item.ДатаЗамены, item));
                    }
                    foreach (var item in controlDatas)
                    {
                        list.Add(new Tuple<DateOnly, object>(item.Key, item.Value));
                    }

                    // делегат сравнения по дате
                    Comparison<Tuple<DateOnly, object>> comparison = (x, y) =>
                    {
                        return y.Item1.CompareTo(x.Item1);
                    };
                    // сортировка по дате
                    list.Sort(comparison);

                    if (list.Count == 0)
                        continue;

                    // конечная дата
                    endDate = list[0].Item1;
                    // начальная дата
                    startDate = list[list.Count - 1].Item1;

                    // оплаченная разность за последний период
                    endValue = (list[0].Item2 is PaymentData paymentData)
                        ? paymentData.ПоследнееПоказание
                        : ((list[0].Item2 is ChangeOfMeter changeOfMeter) ? changeOfMeter.ПоказаниеСнятого : 0);

                    // оплаченная разность за первый период из списка
                    startValue = payments.Count == 0 ? 0 : payments[payments.Count - 1].ПредыдущееПоказание;

                    // сумма расходов за период
                    int summ = 0;

                    DateOnly findDate = endDate.AddYears(-1);

                    bool found = false;
                    // перебор всех оплат, ищем ближайшую дату оплаты, чтобы период был не меньше года
                    foreach (Tuple<DateOnly, object> item in list)
                    {
                        if (item.Item1 > findDate && found == false)
                        {
                            // проверяем была ли замена счетчика
                            if (item.Item2 is ChangeOfMeter change)
                            {
                                summ += endValue - change.ПоказаниеУстановленного;
                                endValue = change.ПоказаниеСнятого;

                                meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Change, 0, change.ПоказаниеУстановленного));
                            }
                            else
                            {
                                if (item.Item2 is PaymentData payment)
                                {
                                    summ += endValue - payment.ПредыдущееПоказание;
                                    endValue = payment.ПредыдущееПоказание;

                                    meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Payment, 0, payment.РазностьПоказанийПоКвитанции));
                                }
                            }
                        }
                        else
                        {
                            if (item.Item2 is ChangeOfMeter change)
                            {
                                startDate = change.ДатаЗамены;

                                meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Change, 0, change.ПоказаниеУстановленного));
                            }
                            else
                            {
                                if (item.Item2 is PaymentData payment)
                                {
                                    startDate = payment.ПериодОплаты;

                                    meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Payment, payment.РазностьПоказанийПоКвитанции, payment.ПоследнееПоказание));
                                }
                                else if (item.Item2 is int control)
                                {
                                    meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Control, 0, control));
                                }
                            }

                            found = true;
                        }
                    }

                    int days = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
                    average = days == 0 ? 0 : 30 * summ / days;

                    meter.СреднеМесячныйРасход = average;

                    //(meter.Events as List<MeterEvent>).Sort((m1, m2) => m1.Date.CompareTo(m2.Date));
                }
                    //});
            }
            catch (Exception ex)
            {
                logger?.Error($">>> TMP.WORK.AramisChetchiki.AramisDBParser>AddAdditionalInfoToElectricitySupply\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                return;
            }

            // fix
            workTask.UpdateUI(totalRows, totalRows, stepNameString: "лицевой счет");

            workTask.IsCompleted = true;
        }

        /// <summary>
        /// Создание сводной информации
        /// </summary>
        /// <param name="meters">Список счетчиков</param>
        /// <returns></returns>
        private ObservableCollection<SummaryInfoItem> BuildSummaryInfo(IEnumerable<Meter> meters)
        {
            logger?.Info(">>> TMP.WORK.AramisChetchiki.AramisDBParser>BuildSummaryInfo");

            Model.WorkTask workTask = new ("формирование сводной информации");
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            if (meters == null)
            {
                return null;
            }

            int totalRows = ModelHelper.MeterSummaryInfoProperties.Count;
            int processedRows = 0;

            List<SummaryInfoItem> infos = new ();

            // по всем свойствам
            foreach (string field in Settings.Default.SummaryInfoFields)
            {
                infos.Add(SummaryInfoHelper.BuildSummaryInfoItem(meters, field));
                workTask.UpdateUI(++processedRows, totalRows, stepNameString: "формирование свода");
            }

            // fix
            workTask.UpdateUI(totalRows, totalRows, stepNameString: "формирование свода");

            workTask.IsCompleted = true;
            return new ObservableCollection<SummaryInfoItem>(infos);
        }

        /// <summary>
        /// чтение оплат за электроэнергию
        /// </summary>
        /// <returns></returns>
        private IList<PaymentData> GetPaymentDatas()
        {
            int filesCount = 0, processedFiles = 0;
            int totalRecords = 0, processedRecords = 0;

            WorkTask workTask = new ("чтение таблиц с данными по оплате")
            {
                Status = "подготовка ..."
            };
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);

            void _(string msg) => workTask.UpdateUI(processedFiles, filesCount, ++processedRecords, totalRecords, message: "таблица " + msg + $"\nзапись {processedRecords:N0} из {totalRecords:N0}", stepNameString: "обработка файла");

            System.Collections.Concurrent.ConcurrentBag<PaymentData> list = new ();
            try
            {
                var files = System.IO.Directory.GetFiles(this.pathDBF, "KARTKV??.DBF");
                filesCount = files.Length;

                string tableFileName = string.Empty;

                void toParse(DbfRecord record)
                {
                    if (record != null)
                    {
                        list.Add(this.ParsePaymentRecord(record));

                        _(System.IO.Path.GetFileName(tableFileName));
                    }
                }

                foreach (var file in files)
                {
                    var result = this.CheckAndLoadFromCache<PaymentData>(file, ref workTask);
                    if (result != null)
                    {
                        workTask.IsIndeterminate = false;
                        workTask.ChildProgress = 0;
                        int total = result.Count;
                        for (int i = 0; i < total; i++)
                        {
                            workTask.ChildProgress = 100d * i / total;
                            list.Add(result[i]);
                        }
                    }
                    else
                    {
                        tableFileName = file;

                        string digits = tableFileName.Substring(tableFileName.Length - 6, 2);

                        if (byte.TryParse(digits, out byte n) || string.Equals(digits, "GD", StringComparison.OrdinalIgnoreCase))
                        {
                            _(System.IO.Path.GetFileName(tableFileName));

                            using DBF.DbfReader dbfReader = new DbfReader(tableFileName, System.Text.Encoding.GetEncoding(866), true);
                            {
                                processedRecords = 0;
                                totalRecords = dbfReader.DbfTable.Header.NumberOfRecords;
                                _(System.IO.Path.GetFileName(tableFileName));

                                dbfReader.ParseRecords(toParse);
                            }

                            this.StoreHashAndSaveData(tableFileName, ref workTask, list.ToArray());
                        }
                    }

                    processedFiles++;
                }
            }
            catch (IOException ioex)
            {
                logger?.Error(ioex, TMPApp.GetExceptionDetails(ioex));
                return null;
            }
            catch (Exception ex)
            {
                logger?.Error($">>> read PaymentData\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                return null;
            }

            return this.SortData(list, workTask);
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
                var result = this.CheckAndLoadFromCache<T>(tableFileName, ref workTask);
                if (result != null)
                    data = new System.Collections.Concurrent.ConcurrentBag<T>(result);

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

        #region Model parsers

        /// <summary>
        /// получение таблицы с абонентами
        /// </summary>
        /// <returns></returns>
        private IList<Meter> GetMeters()
        {
            int totalRows = 0;
            int processedRows = 0;

            Model.WorkTask workTask = new("обработка картотеки");
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);

            // таблица с абонентами
            totalRows = this.collectionKARTAB.Count;

            List<Meter> metersList = new(this.collectionKARTAB.Count);

            string s = string.Empty;

            workTask.UpdateStatus($"количество строк в таблице абонентов: {totalRows:N0}");
            processedRows = 0;
            workTask.StartProcessing();

            foreach (KARTAB abonent in this.collectionKARTAB)
            {
                ICollection<KARTSCH> abonentMeters = this.GetDictionaryValue(this.dictionaryKARTSCH, abonent.LIC_SCH);

                if (abonentMeters != null && abonentMeters.Count != 0)
                {
                    var rowsGroupedByMeter = abonentMeters
                        .GroupBy(
                            i => i.N_SCH.ToString().Trim(),
                            (meterNumber, list) => new { MeterNumber = meterNumber, List = list.ToArray() });

                    foreach (var group in rowsGroupedByMeter)
                    {
                        KARTSCH meterDataRow = group.List[0];
                        Meter meter = this.ParseDataRowAndGetMeter(meterDataRow, abonent);
                        meter.Тарифов = group.List.Length > 1 ? (byte)group.List.Length : (byte)1;
                        metersList.Add(meter);
                    }
                }
                else
                {
                    Meter meter = this.ParseDataRowAndGetMeter(null, abonent);
                    meter.Тарифов = 0;
                    metersList.Add(meter);
                }

                workTask.UpdateUI(++processedRows, totalRows);
            }

            // fix
            workTask.UpdateUI(totalRows, totalRows);

            return this.SortData(metersList, workTask);
        }

        private Meter ParseDataRowAndGetMeter(KARTSCH meterData, KARTAB abonent)
        {
            Meter meter = new();
            try
            {
                #region Контактные данные

                meter.Удалён = abonent.IsDeleted;

                meter.Фамилия = abonent.FAM;
                meter.Имя = abonent.NAME;
                meter.Отчество = abonent.OTCH;

                meter.SMS = abonent.SMS;
                meter.Телефоны = abonent.TELEF;

                if (this.dictionaryKartTn.ContainsKey(abonent.COD_TN))
                {
                    var town = this.dictionaryKartTn[abonent.COD_TN];
                    var townName = town.TOWN;
                    string province = NOTFOUNDED;
                    if (this.dictionaryKartSs.ContainsKey(town.COD_SS))
                    {
                        province = this.dictionaryKartSs[town.COD_SS].СЕЛЬСОВЕТ;

                        if (string.IsNullOrEmpty(province) && townName != null && townName.StartsWith("г.", AppSettings.StringComparisonMethod))
                        {
                            province = "город";
                        }
                    }
                    else
                    {
                        if (string.Equals(province, NOTFOUNDED) && townName != null && townName.StartsWith("г.", AppSettings.StringComparisonMethod))
                        {
                            province = "город";
                        }
                    }

                    var street = this.GetDictionaryValue(this.dictionaryKartSt, abonent.COD_ST);

                    Address address = new(
                        townName,
                        street?.STREET,
                        abonent.HOME,
                        abonent.KV,
                        province);
                    meter.Адрес = address;
                }
                else
                {
                    var code = abonent.COD_TN;
                    this.errors.Add($"Не найден населенный пункт с кодом {code}");
                }

                #endregion

                #region Абонент

                meter.Категория = this.GetDictionaryValue(this.dictionaryKartKat, abonent.COD_KAT)?.KATEGAB;

                meter.Коментарий = abonent.KOMENT;

                meter.ДатаУведомления = GetDateOnly(abonent.ДАТА_ОТКПЛ);

                meter.ДатаОтключения = GetDateOnly(abonent.ДАТА_ОТКФК);
                meter.ПоказаниеПриОтключении = abonent.ПОКАЗАНИЯ;

                meter.Задолженник = abonent.PR_ZD;

                meter.КолвоЧеловек = abonent.ЧЛЕНОВ;

                #endregion

                #region Счётчик

                meter.ШифрТарифа = abonent.ШИФР;
                meter.Контролёр = this.GetDictionaryValue(this.dictionaryASKONTR, abonent.КОД_КОН)?.ФАМИЛИЯ;

                #region Оплата

                meter.ПериодПослОплаты = GetDateOnly(abonent.YEARMON);
                meter.Среднее = abonent.СРЕДНЕЕ;
                meter.Месяц = abonent.МЕСЯЦ;

                meter.ДатаОплаты = GetDateOnly(abonent.DATE_R);
                meter.СуммаОплаты = abonent.SUMMA_KN + abonent.SUMMA_KC;

                meter.ДолгРуб = abonent.ERRSUM;

                meter.ErrSumN = abonent.ERRSUMN;
                meter.ErrSumV = abonent.ERRSUMV;

                meter.ДатаОбхода = GetDateOnly(abonent.DATE_KON);

                #endregion

                #region Счётчик-признаки

                meter.Расположение = this.GetDictionaryValue(this.dictionaryKartTpr, abonent.COD_TPR)?.TPRIEM;
                meter.Использование = this.GetDictionaryValue(this.dictionaryKartIsp, abonent.COD_ISP)?.ISPIEM;

                #endregion

                #region Привязка

                if (this.dictionaryKartfid.ContainsKey(abonent.FIDER10))
                {
                    var fider10 = this.dictionaryKartfid[abonent.FIDER10];
                    string s = fider10.ФИДЕР;
                    meter.Фидер10 = string.IsNullOrWhiteSpace(s) ? string.Empty : fider10.НАИМЕНОВ + "-" + s;
                    meter.Подстанция = this.GetDictionaryValue(this.dictionaryKartps, fider10.ПОДСТАНЦИЯ)?.НАИМЕНОВ;
                }
                else
                {
                    this.errors.Add($"Не найдена информация по фидеру 10 кВ: л/с {meter.Лицевой}, код фидера {abonent.FIDER10}");
                }

                int n = 0;
                if (int.TryParse(abonent.НОМЕР_ТП, out n) == false)
                {
                    this.errors.Add($"Ошибка в номере ТП (поле НОМЕР_ТП) '{abonent.НОМЕР_ТП}' (Лицевой счет абонента={abonent.LIC_SCH})");
                }
                if (this.dictionaryKartktp.ContainsKey(n))
                {
                    var nameTp = this.GetDictionaryValue(this.dictionaryKartktp, n)?.НАИМ_ТП;
                    int? numberTp = this.GetDictionaryValue(this.dictionaryKartktp, n)?.НОМЕР_ТП;
                    var ss = this.GetDictionaryValue(this.dictionaryKartktp, n)?.PR_GS;
                    meter.ТП = new TransformerSubstation(nameTp, numberTp.GetValueOrDefault(), ss);
                }
                meter.Фидер04 = abonent.ФИДЕР;
                meter.Опора = abonent.НОМЕР_ОПОР;

                #endregion

                #region Признаки

                string priznak = string.IsNullOrWhiteSpace(abonent.PRIZNAK) ? string.Empty : abonent.PRIZNAK;
                if (priznak.Length is > 0 and not 15)
                {
                    System.Diagnostics.Debugger.Break();
                }

                var bitArray = new BitArray(15);
                for (byte index = 0; index < priznak.Length; index++)
                {
                    char bit = priznak[index];

                    if (bit == '1')
                    {
                        meter.Signs = (MeterSigns)((int)meter.Signs | (1 << index));
                    }
                }

                meter.Договор = string.IsNullOrWhiteSpace(abonent.DOG) == false && (abonent.DOG == "1" || abonent.DOG == "д");
                meter.ДатаДоговора = abonent.DATE_DOG.HasValue ? GetDateOnly(abonent.DATE_DOG) : null;

                meter.ПринадлежностьРуп = abonent.PR_MO;
                meter.Льгота = abonent.ЛЬГОТА;

                meter.РаботникБелЭнерго = abonent.RABOT;

                #endregion

                #region Счёт

                meter.Лицевой = ConvertToULong(abonent.LIC_SCH);
                meter.ЛицевойЖКХ = ConvertToULong(abonent.GKH_L1);

                #endregion

                if (meterData != null)
                {
                    var meterInfo = this.GetDictionaryValue(this.dictionaryKARTTSCH, meterData.COD_TSCH);
                    if (meterInfo != null)
                    {
                        meter.ТипСчетчика = meterInfo.NAME;
                        meter.Ампераж = meterInfo.TOK;
                        meter.ПериодПоверки = GetByte(meterInfo.PERIOD, 0, 1, 0);
                        meter.Фаз = (byte)meterInfo.ФАЗ;
                        meter.Принцип = meterInfo.TIP == "И" ? "индукционный" : (meterInfo.TIP == "Э" ? "электронный" : "неизвестный");
                        meter.Разрядность = meterInfo.ЗНАК;
                    }

                    meter.ПоследнееОплаченноеПоказание = meterData.DATA_NEW;
                    meter.ПредыдущеОплаченноеПоказание = meterData.DATA_OLD;

                    meter.РасчПоказание = abonent.RACHPOK;
                    meter.ПослПоказаниеОбхода = abonent.DATA_KON;

                    meter.НомерСчетчика = meterData.N_SCH;
                    string god = meterData.GODVYPUSKA.ToString();
                    meter.ГодВыпуска = Convert.ToInt32(string.IsNullOrWhiteSpace(god) ? "0" : god, AppSettings.CurrentCulture);

                    meter.ДатаУстановки = GetDateOnly(meterData.DUSTAN);
                    meter.ПоказаниеПриУстановке = meterData.PUSTAN ?? 0;

                    meter.НомераПломб = meterData.N_PLOMB + "; " + meterData.PLOMB_GS;
                    meter.Мощность = meterData.POWERS;

                    #region Счётчик-признаки

                    meter.МестоУстановки = this.GetDictionaryValue(this.dictionaryASVIDYST, meterData.COD_SS)?.MESTO;
                    meter.Аскуэ = abonent.ASKUE;

                    #endregion

                    #region Поверка

                    meter.КварталПоверки = GetByte(meterData.G_PROV, 0, 1, 1);
                    meter.ГодПоверки = GetByte(meterData.G_PROV, 2, 2, 0);

                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                logger?.Error($">>> TMP.WORK.AramisChetchiki.Repository>GetSelectedDepartamentData->AbonentsTable\n{meterData}\n>>>: {TMPApp.GetExceptionDetails(ex)}");
            }

            return meter;
        }

        /// <summary>
        /// получение замен счётчиков
        /// </summary>
        /// <returns></returns>
        private IList<ChangeOfMeter> GetChangesOfMeters()
        {
            string taskName = "таблица замен счётчиков";
            Model.WorkTask workTask = new (taskName);
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            string fileName = Path.Combine(this.aramisDbPath, "ChangesOfMeters" + DATA_FILE_EXTENSION);

            workTask.UpdateStatus($"загрузка данных из кэша ...");
            workTask.IsIndeterminate = true;

            ChangeOfMeter[] result;
            result = this.DeserializeData<ChangeOfMeter>(fileName);
            if (result != null)
            {
                List<ChangeOfMeter> data = new (result);
                return this.SortData(data, workTask);
            }
            else
            {
                int processedRows = 0;
                int totalRows = 0;

                this.dictionaryAssmena = new ();

                System.Collections.Concurrent.ConcurrentBag<Assmena> tableAssmena = new System.Collections.Concurrent.ConcurrentBag<Assmena>();
                using DBF.DbfReader dbfReader = new DbfReader(Path.Combine(this.pathDBF, "Assmena.DBF"), System.Text.Encoding.GetEncoding(866), skipDeletedRecords: true);
                {
                    processedRows = 0;
                    totalRows = dbfReader.DbfTable.Header.NumberOfRecords;
                    int currYear = DateTime.Now.Year;
                    workTask.UpdateStatus($"количество строк в таблице: {totalRows:N0}");

                    tableAssmena = new System.Collections.Concurrent.ConcurrentBag<Assmena>();

                    void toParse(DbfRecord record)
                    {
                        if (record != null)
                        {
                            tableAssmena.Add(this.ParseAssmenaRecord(record));

                            workTask.UpdateUI(++processedRows, totalRows, stepNameString: "строка");

                            workTask.SetProgress(totalRows, processedRows);
                        }
                    }

                    dbfReader.ParseRecords(toParse);
                    workTask.UpdateUI(totalRows, totalRows, stepNameString: "строка");
                }

                workTask.UpdateStatus("анализ полученных данных...");
                foreach (var item in tableAssmena)
                {
                    if (this.dictionaryAssmena.ContainsKey(item.ЛИЦ_СЧЕТ))
                    {
                        this.dictionaryAssmena[item.ЛИЦ_СЧЕТ].Add(item);
                    }
                    else
                    {
                        this.dictionaryAssmena.Add(item.ЛИЦ_СЧЕТ, new List<Assmena>(new[] { item }));
                    }
                }

                processedRows = 0;
                totalRows = this.dictionaryAssmena.Count;

                workTask.UpdateStatus($"количество замен: {totalRows:N0}");
                System.Collections.Concurrent.BlockingCollection<ChangeOfMeter> changes = new ();

                Parallel.ForEach(this.dictionaryAssmena.Values, changeOfMeterRow =>
                {
                    ChangeOfMeter changesOfMeters;
                    if (changeOfMeterRow.Count == 1)
                    {
                        changesOfMeters = this.ParseChangesOfMetersRecord(changeOfMeterRow.First());
                        changes.Add(changesOfMeters);
                    }
                    else
                    {
                        foreach (Assmena assmena in changeOfMeterRow)
                        {
                            changesOfMeters = this.ParseChangesOfMetersRecord(assmena);
                            if (changesOfMeters != null)
                            {
                                changes.Add(changesOfMeters);
                            }
                        }
                    }

                    workTask.UpdateUI(++processedRows, totalRows);
                });
                changes.CompleteAdding();

                workTask.UpdateStatus($"сохранение в кэш...");
                workTask.IsIndeterminate = true;
                this.SerializeData<ChangeOfMeter>(changes.ToArray(), fileName);

                // fix
                workTask.UpdateUI(totalRows, totalRows);

                return this.SortData(changes, workTask);
            }
        }

        /// <summary>
        /// Парсинг записи о замене счетчика
        /// </summary>
        /// <param name="assmena"></param>
        /// <returns></returns>
        private ChangeOfMeter ParseChangesOfMetersRecord(Assmena assmena)
        {
            ChangeOfMeter change = new();
            try
            {
                KARTTSCH meterType = this.GetDictionaryValue(this.dictionaryKARTTSCH, assmena.ТИП_СЧЕТЧ);
                ICollection<KARTSCH> meterInfos = this.dictionaryKARTSCH[assmena.ЛИЦ_СЧЕТ];

                KARTAB abonent = new KARTAB();
                var list = this.collectionKARTAB.Where(i => i.LIC_SCH == assmena.ЛИЦ_СЧЕТ && i.IsDeleted == false);
                if (list.Any())
                {
                    if (list.Count() > 1)
                    {
                        ;
                    }

                    abonent = this.collectionKARTAB.Where(i => i.LIC_SCH == assmena.ЛИЦ_СЧЕТ && i.IsDeleted == false).First();
                }
                else
                {
                    return null;
                }

                ICollection<KARTSCH> old_meter = this.GetDictionaryValue(this.dictionaryKARTSCHRemoved, assmena.ЛИЦ_СЧЕТ);

                change.ТипСнятогоСчетчика = meterType == null ? NOTFOUNDED : meterType.NAME;

                if (meterInfos != null && meterInfos.Count != 0)
                {
                    KARTSCH meterInfo1 = meterInfos.First();
                    change.ГодВыпускаУстановленного = meterInfo1.GODVYPUSKA;

                    KARTTSCH meterInfos1 = this.GetDictionaryValue(this.dictionaryKARTTSCH, meterInfo1.COD_TSCH);
                    if (meterInfos1 != null)
                    {
                        change.ТипУстановленногоСчетчика = meterInfos1.NAME;
                        change.ЭтоЭлектронный = meterInfos1.TIP == "Э";
                    }
                }

                if (old_meter != null && old_meter.Count != 0)
                {
                    KARTSCH meterInfo3 = old_meter.First();
                    change.КварталПоверкиСнятого = GetByte(meterInfo3.G_PROV, 0, 1, 1);
                    change.ГодПоверкиСнятого = GetByte(meterInfo3.G_PROV, 2, 2, 0);
                    change.ГодВыпускаСнятого = meterInfo3.GODVYPUSKA;
                    change.ДатаУстановкиСнятого = GetDateOnly(meterInfo3.DATE_UST);
                }

                change.Адрес = new BaseAddress(
                        this.dictionaryKartTn[abonent.COD_TN].TOWN,
                        this.dictionaryKartSt[abonent.COD_ST].STREET,
                        abonent.HOME,
                        abonent.KV);

                string name = abonent.NAME;
                string otch = abonent.OTCH;
                change.Фио = abonent.FAM + " " + name[0] + "." + otch[0] + ".";

                if (this.dictionaryKartfid.ContainsKey(abonent.FIDER10))
                {
                    var fider10 = this.dictionaryKartfid[abonent.FIDER10];
                    string s = fider10.ФИДЕР;
                    change.Фидер10 = string.IsNullOrWhiteSpace(s) ? string.Empty : fider10.НАИМЕНОВ + "-" + s;
                    change.Подстанция = this.GetDictionaryValue(this.dictionaryKartps, fider10.ПОДСТАНЦИЯ)?.НАИМЕНОВ;
                }

                int n = 0;
                if (int.TryParse(abonent.НОМЕР_ТП, out n) == false)
                {
                    this.errors.Add($"Ошибка в номере ТП (поле НОМЕР_ТП) '{abonent.НОМЕР_ТП}' (Лицевой счет абонента={abonent.LIC_SCH})");
                }
                if (this.dictionaryKartktp.ContainsKey(n) == false)
                {
                    ;
                }
                else
                {
                    var typeTp = this.dictionaryKartktp[n].НАИМ_ТП;
                    var nameTp = this.dictionaryKartktp[n].PR_GS;
                    int? numberTp = this.dictionaryKartktp[n].НОМЕР_ТП;
                    change.НомерТП = numberTp.GetValueOrDefault().ToString();
                    change.НаименованиеТП = typeTp + " " + nameTp;
                }
                change.Фидер04 = abonent.ФИДЕР;
                change.Опора = abonent.НОМЕР_ОПОР;

                change.Лицевой = ConvertToULong(assmena.ЛИЦ_СЧЕТ);
                change.НомерСнятогоСчетчика = assmena.НОМЕР_СНЯТ;
                change.ПоказаниеСнятого = assmena.ПОКАЗ_СНЯТ;

                change.НомерУстановленногоСчетчика = assmena.НОМЕР_УСТ;
                change.ПоказаниеУстановленного = assmena.ПОКАЗ_УСТ;
                change.ДатаЗамены = GetDateOnly(assmena.ДАТА_ЗАМЕН);
                change.НомерАкта = assmena.НОМЕР_АКТА;
                change.Фамилия = assmena.ФАМИЛИЯ;
                change.Причина = assmena.ПРИЧИНА;
            }
            catch (Exception ex)
            {
                logger?.Error($">>> ChangesOfMeters ParseRecord >>>: {TMPApp.GetExceptionDetails(ex)}");
            }

            return change;
        }

        /// <summary>
        /// получение полезного отпуска (FORMAB71)
        /// </summary>
        /// <returns></returns>
        private ElectricitySupply ParseElectricitySupplyRecord(DbfRecord record)
        {
            ElectricitySupply electricitySupply = new();
            if (record != null)
            {
                int currYear = DateTime.Now.Year;
                DateOnly period = GetDateOnly(record.GetValue<DateTime>("DATE_N"));
                if (period.Year == currYear || period.Year == currYear - 1)
                {
                    try
                    {
                        electricitySupply.ДатаОплаты = GetDateOnly(record.GetValue<DateTime>("DATE_OPL"));
                        electricitySupply.ОплаченныеПоказания = record.GetValue<int>("DATA_OPL");

                        electricitySupply.Период = period.AddMonths(-1); // оплата на месяц позже
                        electricitySupply.Лицевой = ConvertToULong(record.GetString("LIC_SCH"));
                        electricitySupply.Полезный_отпуск = record.GetValue<int>("POLOT");
                        electricitySupply.Задолженность = record.GetValue<int>("KVT_ZADOL");
                        electricitySupply.Тип_населённого_пункта = record.GetString("TIP");
                    }
                    catch (Exception ex)
                    {
                        logger?.Error($">>> TMP.WORK.AramisChetchiki.Repository>GetSelectedDepartamentData->electricitySupplyTable\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                    }

                    /*
                                                 System.Data.DataRow[] abonents = electricitySupplyRow.GetChildRows("ПО_абонент");

                    if (abonents != null && abonents.Length != 0)
                    {
                        DataRow abonent = abonents[0];
                        electricitySupply.Населённый_пункт = _getChildRelationValue(abonent, "населенный_пункт", "TOWN");
                        electricitySupply.Адрес = string.Join(", ", new string[]
                        {
                    _getChildRelationValue(abonent, "улицы", "STREET"),
                    _getString(abonent["HOME"]),
                    _getString(abonent["KV"])
                        });

                        var fider10Rows = abonent.GetChildRows("фидер10");
                        if (fider10Rows.Length > 0)
                        {
                            var fider10 = fider10Rows[0];
                            s = _getString(fider10["ФИДЕР"]);
                            electricitySupply.Фидер10 = String.IsNullOrWhiteSpace(s) ? String.Empty : _getString(fider10["НАИМЕНОВ"]) + "-" + s;
                            electricitySupply.Подстанция = _getChildRelationValue(fider10, "подстанция", "НАИМЕНОВ");
                        }
                        electricitySupply.ТП = _getChildRelationValue(abonent, "тп", "НАИМЕНОВ");
                        electricitySupply.Фидер04 = _getString(abonent["ФИДЕР"]);
                    }
                     * */
                }
            }
            else
            {
            }

            return electricitySupply;
        }

        /// <summary>
        /// получение информации по оплатам
        /// </summary>
        /// <returns></returns>
        private PaymentData ParsePaymentRecord(DbfRecord record)
        {
            PaymentData payment = new();

            if (record != null)
            {
                int currYear = DateTime.Now.Year;
                DateTime period = record.GetValue<DateTime>("YEARMON");
                if (period.Year == currYear || period.Year == currYear - 1)
                {
                    try
                    {
                        payment.ДатаОплаты = GetDateOnly(record.GetValue<DateTime?>("DATE_R"));
                        payment.ПериодОплаты = GetDateOnly(record.GetValue<DateTime?>("YEARMON"));

                        payment.Лицевой = ConvertToULong(record.GetString("LIC_SCH"));

                        payment.ПредыдущееПоказание = record.GetValue<int>("DATA_OLD");
                        payment.ПоследнееПоказание = record.GetValue<int>("DATA_NEW");

                        payment.РазностьПоказанийПоКвитанции = record.GetValue<int>("РАЗН_КН") + record.GetValue<int>("РАЗН_КС");
                        payment.РазностьПоказанийРасчётная = record.GetValue<int>("РАЗН_РН") + record.GetValue<int>("РАЗН_РС");

                        payment.ВеличинаТарифа = record.GetValue<decimal>("TAR_KN");

                        payment.СуммаОплаты = record.GetValue<decimal>("SUMMA_KN") + record.GetValue<decimal>("SUMMA_KC");

                        payment.СуммаОплатыРасчётная = record.GetValue<decimal>("SUMMA_RN") + record.GetValue<decimal>("SUMMA_RC") + record.GetValue<decimal>("PENYA_R");

                        payment.ПеняВыставленая = record.GetValue<decimal>("PENYA_R");
                        payment.ПеняОплаченная = record.GetValue<decimal>("PENYA_K");
                    }
                    catch (Exception ex)
                    {
                        logger?.Error($">>> AramisDBParser > ParsePaymentRecord\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                    }
                }
            }
            else
            {
            }
            return payment;
        }

        /// <summary>
        /// получение контрольных показаний по лицевому счету
        /// </summary>
        /// <returns></returns>
        private ControlData ParseControlData(DbfRecord record)
        {
            ControlData controlData = new();

            if (record != null)
            {
                controlData.Лицевой = ConvertToULong(record.GetString("LIC_SCH"));
                controlData.Оператор = record.GetString("ОПЕРАТОР");

                var list = new List<KeyValuePair<DateOnly, int>>(12);

                try
                {
                    DateOnly date;
                    int? value;
                    for (int ind = 1; ind <= 12; ind++)
                    {
                        date = GetDateOnly(record.GetValue<DateTime?>($"DATE_{ind}"));
                        value = record.GetValue<int>($"DATA_{ind}");

                        if (date != default && value.HasValue)
                        {
                            list.Add(new KeyValuePair<DateOnly, int>(date, value.Value));
                        }
                    }

                    controlData.Data = list;
                }
                catch (Exception ex)
                {
                    logger?.Error($">>> AramisDBParser > ParseControlData\n>>>: {TMPApp.GetExceptionDetails(ex)}");
                }
            }
            else
            {
            }
            return controlData;
        }

        /// <summary>
        /// справочник установленных счётчиков
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private KARTSCH ParseKARTSCHRecord(DbfRecord record) => new
                (
                    record.GetString("LIC_SCH"),
                    record.GetString("N_SCH"),
                    record.GetString("COD_TSCH"),
                    record.GetString("G_PROV"),
                    GetDate(record.GetValue<DateTime?>("DUSTAN")),
                    record.GetValue<int?>("DATA_OLD"),
                    record.GetValue<int?>("DATA_NEW"),
                    record.GetString("N_PLOMB"),
                    record.GetValue<int?>("GODVYPUSKA"),
                    record.GetValue<decimal?>("POWERS"),
                    record.GetString("COD_SS"),
                    GetDate(record.GetValue<DateTime?>("DATE_FAZ")),
                    record.GetString("PLOMB_GS"),
                    record.GetString("MATER"),
                    record.GetValue<int?>("PUSTAN"),
                    record.GetValue<int?>("DATA_UST"),
                    GetDate(record.GetValue<DateTime?>("DATE_UST")));

        private Kartfid ParseKartfidRecord(DbfRecord record) => new
                (
                    record.GetValue<int>("ПОДСТАНЦИЯ"),
                    record.GetString("ФИДЕР"),
                    record.GetString("НАИМЕНОВ"),
                    record.GetString("НАИМ_ПОД"));

        private ASKONTR ParseASKONTRRecord(DbfRecord record) => new
                (
                    record.GetString("КОД_КОН"),
                    record.GetString("ФАМИЛИЯ"));

        private Kartktp ParseKartktpRecord(DbfRecord record) => new
               (
                   record.GetValue<int>("КОД_ТП"),
                   record.GetString("ФИДЕР"),
                   record.GetValue<int?>("НОМЕР_ТП"),
                   record.GetString("НАИМ_ТП"),
                   record.GetValue<int?>("ПОДСТАНЦИЯ"),
                   record.GetValue<int?>("РЭС"),
                   record.GetString("НАИМЕНОВ"),
                   record.GetString("PR_GS"));

        private Kartps ParseKartpsRecord(DbfRecord record) => new
               (
                   record.GetValue<int>("ПОДСТАНЦИЯ"),
                   record.GetValue<int?>("РЭС"),
                   record.GetString("НАИМЕНОВ"));

        private KartKat ParseKartKatRecord(DbfRecord record) => new
               (
                   record.GetString("COD_KAT"),
                   record.GetString("KATEGAB"));

        private KartIsp ParseKartIspRecord(DbfRecord record) => new
                (
                    record.GetString("COD_ISP"),
                    record.GetString("ISPIEM"));

        private KartTpr ParseKartTprRecord(DbfRecord record) => new
                (
                    record.GetString("COD_TPR"),
                    record.GetString("TPRIEM"));

        private KartSt ParseKartStRecord(DbfRecord record) => new
                (
                    record.GetString("COD_ST"),
                    record.GetString("STREET"));

        private KartSs ParseKartSsRecord(DbfRecord record) => new
                (
                    record.GetString("COD_SS"),
                    record.GetString("СЕЛЬСОВЕТ"));

        private KartTn ParseKartTnRecord(DbfRecord record) => new
                (
                    record.GetString("COD_TN"),
                    record.GetString("TOWN"),
                    record.GetString("COD_SS"));

        private ASVIDYST ParseASVIDYSTRecord(DbfRecord record) => new
                (
                    record.GetString("COD_SS"),
                    record.GetString("MESTO"));

        private KARTTSCH ParseKARTTSCHRecord(DbfRecord record) => new
                (
                    record.GetString("COD_TSCH"),
                    record.GetString("NAME"),
                    record.GetString("TOK"),
                    record.GetString("PERIOD"),
                    record.GetString("TIP"),
                    record.GetValue<int>("ФАЗ"),
                    record.GetValue<decimal>("ЗНАК"));

        private KARTAB ParseKARTABRecord(DbfRecord record)
        {
            return new KARTAB
            {
                IsDeleted = record.IsDeleted,
                LIC_SCH = record.GetString("LIC_SCH"),
                FAM = record.GetString("FAM"),
                NAME = record.GetString("NAME"),
                OTCH = record.GetString("OTCH"),
                SMS = record.GetString("SMS"),
                COD_TN = record.GetString("COD_TN"),
                COD_ST = record.GetString("COD_ST"),
                HOME = record.GetString("HOME"),
                KV = record.GetString("KV"),

                RABOT = record.GetValue<bool>("RABOT"),

                ЧЛЕНОВ = record.GetValue<int?>("ЧЛЕНОВ"),
                TELEF = record.GetString("TELEF"),
                COD_KAT = record.GetString("COD_KAT"),
                COD_TPR = record.GetString("COD_TPR"),
                ШИФР = record.GetString("ШИФР"),
                COD_PD = record.GetString("COD_PD"),
                DKONT = record.GetValue<decimal?>("DKONT"),
                KOMENT = record.GetValue<string>("KOMENT"),
                PLIT = record.GetValue<bool>("PLIT"),

                YEARMON = record.GetValue<DateTime?>("YEARMON"),
                ERRSUM = record.GetValue<decimal?>("ERRSUM"),
                ERRSUMN = record.GetValue<decimal?>("ERRSUMN"),
                SUMMA_KN = record.GetValue<decimal?>("SUMMA_KN"),
                SUMMA_KC = record.GetValue<decimal?>("SUMMA_KC"),
                КОД_КОН = record.GetString("КОД_КОН"),
                СРЕДНЕЕ = record.GetValue<int?>("СРЕДНЕЕ"),
                МЕСЯЦ = record.GetValue<int?>("МЕСЯЦ"),
                ДАТА_ОТКПЛ = record.GetValue<DateTime?>("ДАТА_ОТКПЛ"),
                ДАТА_ОТКФК = record.GetValue<DateTime?>("ДАТА_ОТКФК"),
                НОМЕР_ТП = record.GetString("НОМЕР_ТП"),
                ФИДЕР = record.GetValue<int?>("ФИДЕР"),
                НОМЕР_ОПОР = record.GetString("НОМЕР_ОПОР"),
                СМЕНА = record.GetString("СМЕНА"),
                DATA_KON = record.GetValue<int?>("DATA_KON"),
                DATE_KON = record.GetValue<DateTime?>("DATE_KON"),
                ФАМИЛИЯ = record.GetString("ФАМИЛИЯ"),
                PR_OPL = record.GetString("PR_OPL"),
                COD_PRED = record.GetString("COD_PRED"),
                DATE_ZAP = record.GetValue<DateTime?>("DATE_ZAP"),
                COD_VID = record.GetString("COD_VID"),
                ПОКАЗАНИЯ = record.GetValue<int?>("ПОКАЗАНИЯ"),
                DATE_LGT = record.GetValue<DateTime?>("DATE_LGT"),
                ЛЬГОТА = record.GetValue<int?>("ЛЬГОТА"),
                ПРОЦЕНТ = record.GetValue<int?>("ПРОЦЕНТ"),
                PR_VN = record.GetValue<bool>("PR_VN"),
                PR_VO = record.GetValue<bool>("PR_VO"),
                PR_MO = record.GetValue<bool>("PR_MO"),
                PR_ZD = record.GetValue<bool>("PR_ZD"),
                DATE_R = record.GetValue<DateTime?>("DATE_R"),
                RACHPOK = record.GetValue<int?>("RACHPOK"),
                PENYA_T = record.GetValue<decimal?>("PENYA_T"),
                COD_ISP = record.GetString("COD_ISP"),
                DOG = record.GetString("DOG"),
                DATE_DOG = record.GetValue<DateTime?>("DATE_DOG"),
                FIDER10 = record.GetString("FIDER10"),
                ASKUE = record.GetValue<bool>("ASKUE"),
                ERRSUMV = record.GetValue<decimal?>("ERRSUMV"),
                KVT_LGT = record.GetValue<int?>("KVT_LGT"),
                // field 'PRIZNAK' not allowed to trim
                PRIZNAK = record.GetValue<string>("PRIZNAK"),
                GKH_L = record.GetString("GKH_L"),
                GKH_L1 = record.GetString("GKH_L1"),
            };
        }

        private Assmena ParseAssmenaRecord(DbfRecord record)
        {
            return new
                (
                    record.GetString("ЛИЦ_СЧЕТ"),
                    record.GetString("ТИП_СЧЕТЧ"),
                    record.GetString("НОМЕР_СНЯТ"),
                    record.GetValue<int>("ПОКАЗ_СНЯТ"),
                    record.GetString("НОМЕР_УСТ"),
                    record.GetValue<int>("ПОКАЗ_УСТ"),
                    record.GetValue<DateTime?>("ДАТА_ЗАМЕН"),
                    record.GetValue<int?>("НОМЕР_АКТА"),
                    record.GetString("ФАМИЛИЯ"),
                    record.GetString("ПРИЧИНА"));
        }

        private KartPd ParseKartPdRecord(DbfRecord record)
        {
            return new
                (
                    record.GetString("COD_PD"),
                    record.GetString("SNPD"),
                    record.GetString("PNPD"));
        }

        private RemovAb ParseRemovAbRecord(DbfRecord record)
        {
            return new
                (
                    ConvertToULong(record.GetString("LIC_SCH")),
                    record.GetString("FAM"),
                    record.GetString("NAME"),
                    record.GetString("OTCH"),
                    GetDateOnly(record.GetValue<DateTime?>("DATE_ZAP"))
                    );
        }

        #endregion

        #region Converters

        private static uint ConvertToUInt(object value)
        {
            if (value is null or DBNull)
            {
                return 0;
            }

            _ = uint.TryParse(value.ToString(), out uint result);
            return result;
        }

        private static ulong ConvertToULong(object value)
        {
            if (value is null or DBNull)
            {
                return 0;
            }

            _ = ulong.TryParse(value.ToString(), out ulong result);
            return result;
        }

        private static DateTime? GetDate(object value)
        {
            if (value == null)
            {
                return default(DateTime?);
            }

            if (value is DateTime dateTime)
            {
                return new DateTime?(dateTime);
            }

            string valueAsString = value.ToString().Trim();
            DateTime d = default;
            if (string.IsNullOrWhiteSpace(valueAsString))
            {
                return d;
            }

            bool result;
            try
            {
                result = DateTime.TryParse(valueAsString, out d);
            }
            catch
            {
                result = false;
            }

            return result ? new DateTime?(d) : null;
        }

        private static byte GetByte(object value, int startPos, int length, byte defaultValue)
        {
            if (value is null or System.DBNull)
            {
                return 0;
            }

            string s = value.ToString();
            if (string.IsNullOrWhiteSpace(s) || s.Length < length || startPos >= s.Length)
            {
                return defaultValue;
            }

            try
            {
                var substring = s.Substring(startPos, length).Trim();
                return string.IsNullOrWhiteSpace(substring) ? defaultValue : Convert.ToByte(substring, AppSettings.CurrentCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static DateOnly GetDateOnly(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return DateOnly.FromDateTime(dateTime.Value);
            }
            else
            {
                return default;
            }
        }

        #endregion

        private IList<T> CheckAndLoadFromCache<T>(string fileName, ref Model.WorkTask workTask)
        {
            var fileInfo = new System.IO.FileInfo(fileName);
            List<T> data = null;

            if (this.dataFilesInfo.ContainsKey(fileName))
            {
                workTask.UpdateStatus($"загрузка данных из кэша ...");
                workTask.IsIndeterminate = true;

                var info = this.dataFilesInfo[fileName];

                if (info.LastModified == fileInfo.LastWriteTime)
                {
                    T[] result = this.DeserializeData<T>(fileName);

                    if (result != null)
                        data = new List<T>(result);
                }
                else
                {
                    // расчет хэша файла и сравнение с ранее сохраненным
                    string hashAsString = this.CalculateSHA256(fileName);

                    if (string.Equals(info.Hash, hashAsString))
                    {
                        var result = this.DeserializeData<T>(fileName);
                        if (result != null)
                            data = new List<T>(result);
                    }
                }
            }
            return data;
        }

        private void StoreHashAndSaveData<T>(string fileName, ref WorkTask workTask, T[] data)
        {
            string msg = "вычисление кэш-суммы файла ...";
            workTask.UpdateStatus(msg);
            workTask.IsIndeterminate = true;

            string hashAsString = string.Empty;
            bool isOk = false;
            byte numberOfRetries = 1;
            do
            {
                try
                {
                    hashAsString = this.CalculateSHA256(fileName);
                    isOk = true;
                }
                catch (IOException ex)
                {
                    this.callBackAction(ex);
                    workTask.UpdateStatus(string.Format("{0}\nфайл используется другим процессом, попытка #{1}", msg, numberOfRetries));
                    Task.Delay(1_000);
                }
                numberOfRetries++;
            } while (isOk == false);

            var fileInfo = new System.IO.FileInfo(fileName);
            DataFileRecord dataFileRecord = new DataFileRecord() { FileName = fileName, Hash = hashAsString, LastModified = fileInfo.LastWriteTime };
            if (this.dataFilesInfo.ContainsKey(fileName))
                this.dataFilesInfo[fileName] = dataFileRecord;
            else
                this.dataFilesInfo.Add(fileName, dataFileRecord);

            this.SerializeData<T>(data, fileName);
        }

        private string GetDbTableNamePath(string fileName)
        {
            string s = Path.GetFileNameWithoutExtension(Path.GetRelativePath(this.aramisDbPath, fileName).Replace(Path.DirectorySeparatorChar, '-'));

            return Path.Combine(this.dataFilesPath, s + DATA_FILE_EXTENSION);
        }

        private void SerializeData<T>(T[] data, string fileName)
        {
            try
            {
                if (Directory.Exists(this.dataFilesPath) == false)
                {
                    Directory.CreateDirectory(this.dataFilesPath);
                }

                string fullFileName = this.GetDbTableNamePath(fileName);

                using System.IO.FileStream fs = new System.IO.FileStream(fullFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
                MessagePack.MessagePackSerializer.Serialize<T[]>(fs, data, MessagePack.MessagePackSerializer.DefaultOptions);
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
            }
        }

        private T[] DeserializeData<T>(string fileName)
        {
            try
            {
                string fullFileName = this.GetDbTableNamePath(fileName);

                if (File.Exists(fullFileName) == false)
                {
                    return null;
                }

                using System.IO.FileStream fs = new System.IO.FileStream(fullFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                T[] result = MessagePack.MessagePackSerializer.Deserialize<T[]>(fs, MessagePack.MessagePackSerializer.DefaultOptions);

                return result;
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
                return null;
            }
        }

        private string CalculateSHA256(string fileName)
        {
            string hashAsString = string.Empty;

            // Not sure if BufferedStream should be wrapped in using block
            using (var stream = new BufferedStream(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), bufferedStreamBufferSize))
            {
                using (System.Security.Cryptography.SHA256 mySHA256 = System.Security.Cryptography.SHA256.Create())
                {
                    // Compute the hash of the fileStream.
                    byte[] hashValue = mySHA256.ComputeHash(stream);

                    hashAsString = BitConverter.ToString(hashValue).Replace("-", string.Empty);
                }
            }

            return hashAsString;
        }

        private IList<T> SortData<T>(IEnumerable<T> source, WorkTask workTask = null)
            where T : IModelWithPersonalId
        {
            bool removeTaskAfterCompleted = false;

            if (workTask == null)
            {
                workTask = new("Обработка");
                this.workTasksProgressViewModel.WorkTasks.Add(workTask);
                workTask.StartProcessing();
                removeTaskAfterCompleted = true;
            }

            workTask.Status = "Сортировка данных";
            workTask.IsIndeterminate = true;

            var result = source.OrderBy(i => i.Лицевой).ToList();

            workTask.IsCompleted = true;

            if (removeTaskAfterCompleted)
            {
                this.workTasksProgressViewModel.WorkTasks.Remove(workTask);
            }

            return result;
        }

        private TValue GetDictionaryValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (key is string keyAsString && string.IsNullOrWhiteSpace(keyAsString))
            {
                return default;
            }
            else
            {
                if (dictionary.ContainsKey(key))
                {
                    return dictionary[key];
                }
                else
                {
                    return default;
                }
            }
        }

        private System.Windows.Window MainWindow
        {
            get
            {
                if (Application.Current != null && !Application.Current.Dispatcher.CheckAccess())
                {
                    System.Windows.Window mainWindow = null;
                    void action() => mainWindow = TMPApp.Instance.MainWindow;

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
}
