using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft;

using TMP.ARMTES.Model;

namespace TMP.ARMTES
{
    internal static class Webservice
    {
        public static string AmperWebService => GetAppSettingsValue("AmperWebService");
        public static string ArmtesServerAddress => GetAppSettingsValue("ArmtesServerAddress");

        #region Private methods

        internal static string GetAppSettingsValue(string name)
        {
            var value = System.Configuration.ConfigurationManager.AppSettings[name];
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(String.Format("Settings key '{0}' not found.", name));
            else
                return value;
        }

        internal static HttpContent GetHttpContent<T>(T data)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            return content;
        }

        internal static T GetDataFromService<T>(string serverAddress, bool needAuthorization, string requestString, System.Net.Http.HttpMethod method, HttpContent content = null) where T : class
        {
            if (method == null || (method.Method == "POST" && content == null))
                throw new ArgumentNullException();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(serverAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (needAuthorization)
                {
                    Task<HttpResponseMessage> authorizationTask = client.PostAsync(@"api/AuthorizationApi/Login?Login=sbyt&Password=sbyt", new StringContent(""));
                    authorizationTask.Wait();
                    HttpResponseMessage response = authorizationTask.Result;
                    if (response.StatusCode != HttpStatusCode.OK && response.ReasonPhrase != "OK")
                        return null;
                }
                if (method.Method == "POST")
                {
                    Task<HttpResponseMessage> task = client.PostAsync(requestString, content);
                    try
                    {
                        task.Wait();
                        HttpResponseMessage response = task.Result;
                        if (response.IsSuccessStatusCode)
                        {
                            Task<Stream> resultTask = response.Content.ReadAsStreamAsync();
                            task.Wait();
                            Stream resultStream = resultTask.Result;
                            T model;
                            var serializer = new Newtonsoft.Json.JsonSerializer();
                            using (StreamReader sr = new StreamReader(resultStream))
                            using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(sr))
                            {
                                model = serializer.Deserialize<T>(jsonTextReader);
                            }
                            return model;
                        }
                        else
                            return null;
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debugger.Log(0, "WARNING", SmallEngineViewerApp.GetExceptionDetails(e));
                        return null;
                    }
                }
                else
                {
                    T model = null;
                    Task<Stream> task = client.GetStreamAsync(requestString);
                    try
                    {
                        task.Wait(TimeSpan.FromMinutes(15));
                        Stream resultStream = task.Result;
                        var serializer = new Newtonsoft.Json.JsonSerializer();
                        serializer.StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeNonAscii;
                        using (StreamReader sr = new StreamReader(resultStream))
                        using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(sr))
                        {
                            model = serializer.Deserialize<T>(jsonTextReader);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debugger.Log(0, "WARNING", SmallEngineViewerApp.GetExceptionDetails(e));
                        return null;
                    }
                    return model;
                }
            }
        }
        internal static T GetDataFromArmtes<T>(string requestString, bool needAuthorization = false) where T : class
        {
            return GetDataFromService<T>(ArmtesServerAddress, needAuthorization, requestString, HttpMethod.Get);
        }
        internal static T PostDataAndGetResultFromArmtes<T>(string requestString, HttpContent content, bool needAuthorization = false) where T : class
        {
            return GetDataFromService<T>(ArmtesServerAddress, needAuthorization, requestString, HttpMethod.Post, content);
        }
        internal static T GetDataFromAmper<T>(string requestString, bool needAuthorization = false) where T : class
        {
            return GetDataFromService<T>(AmperWebService, needAuthorization, requestString, HttpMethod.Get);
        }
        internal static T PostDataAndGetResultFromAmper<T>(string requestString, HttpContent content, bool needAuthorization = false) where T : class
        {
            return GetDataFromService<T>(AmperWebService, needAuthorization, requestString, HttpMethod.Post, content);
        }

        #endregion

        #region Public methods

        public static bool CheckHostAvailablity(string url)
        {
            Uri uri = new Uri(url);


            System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            System.Net.NetworkInformation.PingReply reply;
            try
            {
                reply = pingSender.Send(uri.Host, timeout, buffer, options);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (System.Net.NetworkInformation.PingException)
            {
                return false;
            }
        }

        /// <summary>
        /// Список РЭС из Ампер-М
        /// </summary>
        /// <returns></returns>
        public static IList<EnterpriseFromAmper> GetEnterprisesFromAmper(string fesName)
        {
            IList<EnterpriseFromAmper> model = null;

            string post = @"q=%D0%92%D0%AB%D0%91%D0%A0%D0%90%D0%A2%D0%AC%20%D0%A0%D0%BE%D0%B4%D0%B8%D1%82%D0%B5%D0%BB%D1%8C.%D0%9A%D0%BE%D0%B4%20%D0%9A%D0%90%D0%9A%20ParentId,%20%D0%A0%D0%BE%D0%B4%D0%B8%D1%82%D0%B5%D0%BB%D1%8C.%D0%9F%D0%BE%D0%BB%D0%BD%D0%BE%D0%B5%D0%9D%D0%B0%D0%B8%D0%BC%D0%B5%D0%BD%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5%20%20%D0%9A%D0%90%D0%9A%20ParentFullName,%20%D0%A0%D0%BE%D0%B4%D0%B8%D1%82%D0%B5%D0%BB%D1%8C.%D0%A1%D0%BE%D0%BA%D1%80%D0%B0%D1%89%D0%B5%D0%BD%D0%BD%D0%BE%D0%B5%D0%9D%D0%B0%D0%B8%D0%BC%D0%B5%D0%BD%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5%20%D0%9A%D0%90%D0%9A%20ParentShortName,%20%D0%9A%D0%BE%D0%B4%20%D0%9A%D0%90%D0%9A%20Id,%20%D0%9F%D0%BE%D0%BB%D0%BD%D0%BE%D0%B5%D0%9D%D0%B0%D0%B8%D0%BC%D0%B5%D0%BD%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5%20%D0%9A%D0%90%D0%9A%20FullName,%20%D0%A1%D0%BE%D0%BA%D1%80%D0%B0%D1%89%D0%B5%D0%BD%D0%BD%D0%BE%D0%B5%D0%9D%D0%B0%D0%B8%D0%BC%D0%B5%D0%BD%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5%20%D0%9A%D0%90%D0%9A%20ShortName%0D%0A%D0%B8%D0%B7%20%D0%A1%D0%BF%D1%80%D0%B0%D0%B2%D0%BE%D1%87%D0%BD%D0%B8%D0%BA.%D0%A1%D1%83%D0%B1%D1%8A%D0%B5%D0%BA%D1%82%D1%8B%0D%0A%D0%B3%D0%B4%D0%B5%0D%0A%09%D0%A3%D1%87%D1%80%D0%B5%D0%B6%D0%B4%D0%B5%D0%BD%D0%B8%D1%8F%D0%A2%D0%B8%D0%BF.%D0%9D%D0%B0%D0%B8%D0%BC%D0%B5%D0%BD%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5%20=%20%22%D0%AD%D0%BD%D0%B5%D1%80%D0%B3%D0%BE%D1%81%D0%B8%D1%81%D1%82%D0%B5%D0%BC%D0%B0%22%0D%0A%09%D0%B8%20%D0%A0%D0%BE%D0%B4%D0%B8%D1%82%D0%B5%D0%BB%D1%8C.%D0%A1%D0%BE%D0%BA%D1%80%D0%B0%D1%89%D0%B5%D0%BD%D0%BD%D0%BE%D0%B5%D0%9D%D0%B0%D0%B8%D0%BC%D0%B5%D0%BD%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5%20=%20%22%D0%9E%D0%AD%D0%A1%22%0D%0A%09%D0%B8%20%D0%BD%D0%B5%20%D0%9F%D0%BE%D0%BB%D0%BD%D0%BE%D0%B5%D0%9D%D0%B0%D0%B8%D0%BC%D0%B5%D0%BD%D0%BE%D0%B2%D0%B0%D0%BD%D0%B8%D0%B5%20=%20%22%22";
            HttpContent content = new StringContent(post, System.Text.Encoding.UTF8, "text/plain");

            model = PostDataAndGetResultFromAmper<IList<EnterpriseFromAmper>>(string.Format(@"sql?f={0}&format=json", fesName), content);

            if (model != null && model.Count > 0)
                return model;
            else
                return null;
        }


        /// <summary>
        /// Информация по счётчику, расчётной точке, объекту учёта, договору из Ампер-М на основании ИД расчётной точки
        /// </summary>
        /// <param name="personalAccount">ИД расчётной точки в Ампер-М</param>
        /// <returns></returns>
        public static AmperGetPointInfo GetPersonalAccountInfoFromAmper(ulong personalAccount)
        {
            AmperGetPointInfo model = null;
            model = GetDataFromAmper<AmperGetPointInfo>(string.Format(@"getpoint?id={0}&format=json", personalAccount));

            if (model != null && model.Status == "200" && model.Found != "0")
                return model;
            else
                return null;
        }
        /// <summary>
        /// Поучение иеарархического списка подразделений
        /// </summary>
        /// <returns></returns>
        public static List<EnterpriseViewItem> GetEnterprises()
        {
            PageResult<EnterpriseViewItem> model = null;
            model = GetDataFromArmtes<PageResult<EnterpriseViewItem>>(@"api/SmallEngineApi/GetEnterprises");
            if (model != null)
                return model.Items;
            else
                return null;
        }
        /// <summary>
        /// Получение списка потребителей
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Subscriber> GetSubscribers()
        {
            SubscribersModel model = null;
            model = GetDataFromArmtes<SubscribersModel>(@"SubscribersRegistryEdit/GetSubscribers?pagenum=0&pagesize=100000", needAuthorization: true);

            if (model != null)
                return model.Subscribers;
            else
                return null;
        }
        /// <summary>
        /// Получение списка потребителей с полями из Ампер-М
        /// </summary>
        /// <param name="enterprise"></param>
        /// <param name="sectorTypeId"></param>
        /// <param name="canBeEmpty"></param>
        /// <returns></returns>
        public static IEnumerable<SmallEngineBillingObject> GetSmallEngineBillingObjects(string enterprise = "", string sectorTypeId = "SES", bool canBeEmpty = false)
        {
            PageResult<GetPersonalAccountsJsonImportingItem> model = null;
            model = GetDataFromArmtes<PageResult<GetPersonalAccountsJsonImportingItem>>(
                String.Format(@"api/PersonalAccountsApi/GetPersonalAccounts?enterprise={0}&sectorTypeId={1}&canBeEmpty={2}",
                    enterprise,
                    sectorTypeId,
                    canBeEmpty));

            if (model != null)
                return model.Items.Select(
                        jii =>
                            new SmallEngineBillingObject()
                            {
                                BillingPointPersonalAccount = jii.PersonalAccount,
                                ResName = jii.ResName,
                                SubscriberName = jii.SubscriberName,
                                ContractNumber = jii.ContractNumber,
                                AccountingPointContractNumber = jii.AccountingPointContractNumber,
                                CityName = jii.City,
                                AccountingPointName = jii.Flat,
                                SubscriberShortName = jii.SubscriberName,
                                ObjectAdress = jii.Street,
                                ObjectName = jii.House
                            }).ToList();
            else
                return null;
        }


        /// <summary>
        /// Список показаний по 4-м тарифам  с указанием ИД расчётной точки
        /// </summary>
        /// <param name="date">Начало отчётного месяца</param>
        /// <param name="enterprise">Сокращённое название подразделения</param>
        /// <returns></returns>
        public static List<AllTariffsExportIndicationViewItem> GetSmallEngineExportIndications(DateTime date, string enterprise = "")
        {
            PageResult<AllTariffsExportIndicationViewItem> model = null;
            string dateAsString = date.ToString("d.M.yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string request = string.Format(@"api/IndicationsExportApi/GetSmallEngineExportIndications?date={0}", dateAsString);
                if (String.IsNullOrWhiteSpace(enterprise) == false)
                request += String.Format("&enterprise={0}", enterprise);

            model = GetDataFromArmtes<PageResult<AllTariffsExportIndicationViewItem>>(request);

            if (model != null)
                return model.Items;
            else
                return null;
        }
        /// <summary>
        /// Список идентификаторов с ошибочными показаниями
        /// </summary>
        /// <param name="date"></param>
        /// <param name="errorCode"></param>
        /// <param name="enterprise"></param>
        /// <returns></returns>
        public static PageResult<ExportIndicationViewItem> GetSmallEngineErrorIndications(DateTime date, string errorCode = "", string enterprise = "")
        {
            PageResult<ExportIndicationViewItem> model = null;
            model = GetDataFromArmtes<PageResult<ExportIndicationViewItem>>(
                String.Format(@"api/IndicationsExportApi/GetSmallEngineErrorIndications?date={0}&errorCode={1}&enterprise={2}",
                    date.ToString("d.M.yyyy"),
                    errorCode,
                    enterprise));
            return model;
        }
        /// <summary>
        /// Список объектов СДСП
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="enterpriseName"></param>
        /// <returns></returns>
        public static PageResult<SmallEngineExportViewItem> GetSmallEngineObjects(DateTime fromDate, DateTime toDate, string enterpriseName)
        {
            PageResult<SmallEngineExportViewItem> model = null;
            model = GetDataFromArmtes<PageResult<SmallEngineExportViewItem>>(
                string.Format(@"api/SmallEngineApi/GetObjects?fromDate={0}&toDate={1}&enterpriseName={2}",
                    fromDate.ToString("d.M.yyyy"), toDate.ToString("d.M.yyyy"), enterpriseName),
                needAuthorization: false
                );

            return model;
        }
        /// <summary>
        /// Таблица с показаниями по точке
        /// </summary>
        /// <param name="accountingPointId"></param>
        /// <param name="profileId"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="sectorTypeId"></param>
        /// <param name="measureParameterName"></param>
        /// <returns></returns>
        public static TableIndications GetIndicatiosInTableView(ulong accountingPointId, Profile profileId, DateTime dateFrom, DateTime dateTo, string sectorTypeId, string measureParameterName = "")
        {
            TableIndications model = null;
            model = GetDataFromArmtes<TableIndications>(
                string.Format(@"api/TableIndicationsApi/GetIndicatiosInTableView?accountingPointId={0}&profileId={1}&&dateFrom={2}&dateTo={3}&sectorTypeId={4}&measureParameterName={5}",
                    accountingPointId, profileId, dateFrom.ToString("d.M.yyyy"), dateTo.ToString("d.M.yyyy"), sectorTypeId, measureParameterName),
                needAuthorization: false
                );

            return model;
        }
        /// <summary>
        /// Отчёт по системам
        /// </summary>
        /// <returns></returns>
        public static List<CollectorDevice> GetCollectorDevicesReport()
        {
            PageResult<CollectorDevice> model = null;
            model = GetDataFromArmtes<PageResult<CollectorDevice>>(@"api/CollectorsApi/Get");
            if (model != null)
                return model.Items;
            else
                return null;
        }


        /// <summary>
        /// Сводная информация по типам используемых счётчиков в указанном РЭС
        /// </summary>
        /// <param name="resName"></param>
        /// <returns></returns>
        public static PageResult<CountersTypeGroupItem> GetCountersGroupInformations(string resName = null)
        {
            PageResult<CountersTypeGroupItem> model = null;
            model = GetDataFromArmtes<PageResult<CountersTypeGroupItem>>(String.Format(@"api/CountersTypeApi/GetCountersGroupInformations?resName={0}", resName));
            return model;
        }
        /// <summary>
        /// Список "проблемных" счётчиков
        /// </summary>
        /// <param name="orderRule"></param>
        /// <returns></returns>
        public static List<MeterProblemInformationViewItem> GetMeterProblemInformations(OrderRule orderRule)
        {
            MeterProblemInformation model = null;
            model = GetDataFromArmtes<MeterProblemInformation>(String.Format(@"api/DeviceInformationApi/GetMeterProblemInformationModel?orderRule={0}", orderRule));
            if (model != null)
                return model.MeterProblemInformationViewItems;
            else
                return null;
        }
        /// <summary>
        /// История опроса устройства
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="channelId"></param>
        /// <param name="isChannel"></param>
        /// <returns></returns>
        public static List<DeviceSession> GetDeviceSessionHistory(int deviceId, int channelId, bool isChannel)
        {
            var post = new { DeviceId = deviceId, ChannelId = channelId, IsChannel = isChannel };
            HttpContent content = GetHttpContent<dynamic>(post);

            DeviceSessions model = null;
            model = PostDataAndGetResultFromArmtes<DeviceSessions>(@"api/DeviceSessionApi/GetDeviceSessionHistory", content);
            if (model != null)
                return model.Items;
            else
                return null;
        }

        public static PageResult<IndicationsAnalyzingItem> GetIndicationsAnalyzing(DateTime dateFrom, DateTime dateTo, Profile profile)
        {
            PageResult<IndicationsAnalyzingItem> model = null;
            model = GetDataFromArmtes<PageResult<IndicationsAnalyzingItem>>(
                String.Format(@"api/IndicationsAnalyzingApi/GetIndicationsAnalyzingModel?dateFrom={0}&dateTo={1}&profile={2}",
                    dateFrom.ToString("d.M.yyyy"),
                    dateTo.ToString("d.M.yyyy"),
                    profile));
            return model;
        }
        /// <summary>
        /// Информация по заменам счётчика
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public static List<MeterSubstitutionViewItem> GetMeterSubstitutionHistory(int deviceId)
        {
            var post = new { DeviceId = deviceId };
            HttpContent content = GetHttpContent<dynamic>(post);

            MeterSubstitutionView model = null;
            model = PostDataAndGetResultFromArmtes<MeterSubstitutionView>(@"api/MeterSerialApi/GetMeterSubstitutionHistory", content);
            if (model != null)
                return model.MeterSubstitutionViewItems;
            else
                return null;
        }
        /// <summary>
        /// Статистика для диаграмм
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="flatId"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public static ChartStatisticsViewModel Statistics(Profile profileId, int flatId, DateTime dateFrom, DateTime dateTo)
        {
            var post = new
            {
                ProfileId = profileId,
                FlatId = flatId,
                DateFrom = dateFrom.ToString("d.M.yyyy"),
                DateTo = dateTo.ToString("d.M.yyyy")
            };
            HttpContent content = GetHttpContent<dynamic>(post);

            ChartStatisticsViewModel model = null;

            /*
            string json = System.IO.File.ReadAllText("Statistics");
            using (var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serializer = new DataContractJsonSerializer(typeof(ChartStatisticsViewModel));
                model = (ChartStatisticsViewModel)serializer.ReadObject(memoryStream);
            }*/
            model = PostDataAndGetResultFromArmtes<ChartStatisticsViewModel>(@"api/SingleMeterApi/PostStatisticsViewModel", content);
            return model;
        }
        /// <summary>
        /// Статистика по СДСП
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="flatId"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        public static ChartStatisticsViewModel SmallEngineStatistics(Profile profileId, int flatId, DateTime dateFrom, DateTime dateTo)
        {
            var post = new
            {
                ProfileId = profileId,
                FlatId = flatId,
                DateFrom = dateFrom.ToString("d.M.yyyy"),
                DateTo = dateTo.ToString("d.M.yyyy")
            };
            HttpContent content = GetHttpContent<dynamic>(post);

            ChartStatisticsViewModel model = null;
            model = PostDataAndGetResultFromArmtes<ChartStatisticsViewModel>(@"api/SingleMeterApi/PostStatisticsViewModel", content);
            return model;
        }
        /// <summary>
        /// Список конфигураций по объектам
        /// </summary>
        /// <returns></returns>
        public static List<ConfigurationContainer> GetAllConfigurationInformation(int resId, string resName)
        {
            PageResult<ConfigurationContainer> model = null;
            model = GetDataFromArmtes<PageResult<ConfigurationContainer>>(string.Format(@"api/ConfigurationApi/GetAllConfigurations?resId={0}&resName={1}", resId, resName));

            if (model != null)
                return model.Items;
            else
                return null;
        }
        /// <summary>
        /// Конфигурация по объекту
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="sectorTypeId"></param>
        /// <returns></returns>
        public static object GetConfigurationFile(int deviceId, string sectorTypeId = null)
        {
            object model = null;
            model = GetDataFromArmtes<object>(
                string.Format(@"Home/GetConfigurationFile?deviceId={0}&sectorTypeId={1}", deviceId, sectorTypeId == null ? "" : sectorTypeId),
                needAuthorization: true
                );

            return model;

        }
        /// <summary>
        /// Задание географических координат объекта
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static CoordinatesInfo SetCoordinatesOfTheHouse(CoordinatesInfo info)
        {
            HttpContent content = GetHttpContent<CoordinatesInfo>(info);

            CoordinatesInfo model = null;
            model = PostDataAndGetResultFromArmtes<CoordinatesInfo>("api/MarkObjectsApi/MarkHouse", content);
            return model;
        }
        /// <summary>
        /// Географические координаты объекта
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public static CoordinatesInfo GetCoordinatesOfTheHouse(int elementId)
        {
            CoordinatesInfo model = null;
            model = GetDataFromArmtes<CoordinatesInfo>(
                String.Format("api/MarkObjectsApi/GetCoordinatesOfTheHouse?elementId={0}", elementId));
            return model;
        }
        /// <summary>
        /// Список дочерних элементов объекта
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static List<TreeViewItem> GetChildElementsInHouseHoldSector(string parentId)
        {
            TreeViewObjectsViewModel model = null;
            model = GetDataFromArmtes<TreeViewObjectsViewModel>(
                String.Format(@"api/TreeViewObjectsApi/GetChildElementsInHouseHoldSector?parentId={0}", parentId));
            if (model != null)
                return model.TreeViewItems;
            else
                return null;
        }

        #endregion
    }
}
