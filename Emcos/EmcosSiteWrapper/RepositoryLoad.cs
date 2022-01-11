using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos
{
    using TMP.Common.RepositoryCommon;
    using TMP.Work.Emcos.Model;
    using TMP.Work.Emcos.Model.Balance;

    /// Функции для загрузки сессии или получения информации из существующих сессий
    public partial class Repository
    {
        #region Constants

        private const string PART_Info = "Info";
        private const string PART_BalancePoints = "BalancePoints";
        private const string PART_BalanceGroupsFormulaById = "BalanceGroupsFormulaById";
        private const string PART_ActiveEnergyBalanceById = "ActiveEnergyBalanceById";
        private const string PART_ReactiveEnergyBalanceById = "ReactiveEnergyBalanceById";
        private const string PART_ActiveEnergyById = "ActiveEnergyById";
        private const string PART_ReactiveEnergyById = "ReactiveEnergyById";
        private const string PART_DescriptionsById = "DescriptionsById";

        #endregion

        #region Public Methods

        /// <summary>
        /// Загрузка конфигурации
        /// </summary>
        /// <returns>True если успешно</returns>
        public bool LoadConfiguration()
        {
            try
            {
                ConfigPoints = LoadConfigPoints();
                //using (System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(CONFIGURATION_FILENAME, FileMode.Open, FileAccess.Read))
                //{
                //    ConfigPoints = LoadBalancePointsFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_BalancePoints, UriKind.Relative))));
                //    BalanceGroupsFormulaById = LoadBalanceFormulasFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_BalanceGroupsFormulaById, UriKind.Relative)))),
                //}
                _pointsCollection = ConfigPoints?.FlatItemsList;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Поддерживается ли сессия
        /// </summary>
        /// <param name="fileName">Имя файла сессии</param>
        /// <returns>True если поддерживается</returns>
        public bool IsSupportedFileName(string fileName)
        {
            return (GetSessionVersion(fileName) >= 0) ? true : false;
        }

        /// <summary>
        /// Возвращает версию файла сессии
        /// </summary>
        /// <param name="fileName">Имя файла сессии</param>
        /// <returns>-1 - файл не поддреживается, 0 - файл старого формата, 1 - нового</returns>
        public int GetSessionVersion(string fileName)
        {
            byte[] zipHeader = { 0x1F, 0x8B, 0x08 };
            byte[] packageHeader = { 0x50, 0x4B, 0x03, 0x04, 0x14 };

            if (Path.GetExtension(fileName) == SESSION_FILE_EXTENSION)
            {
                byte[] bytes = new byte[5];
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                using (System.IO.MemoryStream mso = new System.IO.MemoryStream())
                {
                    fs.Read(bytes, 0, bytes.Length);
                    mso.Write(bytes, 0, bytes.Length);
                }
                if (bytes.Take(3).SequenceEqual(zipHeader))
                    return IsSupportedSessionVersion(LoadVersionFromZip(fileName)) ? 0 : -1;
                else
                    if (bytes.Take(5).SequenceEqual(packageHeader))
                    {
                        try
                        {
                            BalanceSessionInfo info = LoadSessionInfo(fileName);
                            return IsSupportedSessionVersion(info.Version) ? 1 : -1;
                        }
                        catch (IOException ioe)
                        {
                            EmcosSiteWrapperApp.LogException(ioe);
                            return -1;
                        }
                        catch (FileFormatException ffe)
                        {
                            EmcosSiteWrapperApp.LogException(ffe);
                            return -1;
                        }
                        catch { return -1; }
                    }
                    else
                        return -1;
            }
            else
                return -1;
        }

        /// <summary>
        /// Возвращает информацию о сессии
        /// </summary>
        /// <param name="fileName">Имя файла сессии</param>
        /// <returns>Информация о сессии</returns>
        public BalanceSessionInfo LoadSessionInfo(string fileName, int versionNumber = 1)
        {
            try
            {
                if (versionNumber == 1)
                    using (System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(fileName, FileMode.Open))
                    {
                        return LoadSessionInfoFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_Info, UriKind.Relative))));
                    }
                else
                    if (versionNumber == 0)
                    return LoadSessionInfoFromOldFileFormat(fileName);
                else
                    throw new ArgumentOutOfRangeException("Session file version number");
            }
            catch (FileFormatException)
            {
                return LoadSessionInfoFromOldFileFormat(fileName);
            }
            catch (IOException ioe)
            {
                EmcosSiteWrapperApp.LogException(ioe);
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        /// <returns>True если загрузка произошла успешно</returns>
        public bool Load()
        {
            bool result = false;
            // имя файла с сессией
            string sessionFileNameToLoad = null;
            // проверка есть файл, с которым работали в последний раз
            if (File.Exists(Path.Combine(SESSIONS_FOLDER, "lastsession")))
            {
                EmcosSiteWrapperApp.LogInfo("Обнаружен файл с именем файла последней сессии.");
                var lastusedfile = string.Empty;
                try
                {
                    EmcosSiteWrapperApp.LogInfo("Попытка чтения имени файла последней сессии.");
                    // чтение имени файла
                    lastusedfile = File.ReadAllText(Path.Combine(SESSIONS_FOLDER, "lastsession")).Trim();
                    EmcosSiteWrapperApp.LogInfo("Имя файла последней сессии получено.");
                    if (File.Exists(lastusedfile))
                    {
                        sessionFileNameToLoad = lastusedfile;
                        EmcosSiteWrapperApp.LogInfo("Файл последней сессии существует, попытаемся его загрузить.");
                    }
                    else
                        EmcosSiteWrapperApp.LogInfo("Указанный файл последней сессии не найден.");
                }
                catch (System.IO.IOException ex)
                {
                    EmcosSiteWrapperApp.LogInfo("Ошибка при чтении имени файла последней сессии.");
                    _callBackAction(ex);
                }
            }
            else
            {
                if (String.IsNullOrWhiteSpace(BALANCE_SESSION_FILENAME) || File.Exists(Path.Combine(SESSIONS_FOLDER, BALANCE_SESSION_FILENAME + SESSION_FILE_EXTENSION)) == false)
                {
                    EmcosSiteWrapperApp.LogInfo("Сессия не обнаружена.");
                    return false;
                }
            }
            //
            if (LoadSessionData(sessionFileNameToLoad))
            {
                EmcosSiteWrapperApp.LogInfo("Сессия обнаружена и загружена.");
                ActiveSession.Info.IsLoaded = true;
                Loaded?.Invoke(null, EventArgs.Empty);
                result = true;
            }
            else
            {
                EmcosSiteWrapperApp.LogInfo(String.Format("Не удалось загрузить сессию. Файл [{0}].",
                    sessionFileNameToLoad ?? BALANCE_SESSION_FILENAME + SESSION_FILE_EXTENSION));
            }
            return result;
        }
        /// <summary>
        /// Загрузка данных из указанного файла
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>True если загрузка произошла успешно</returns>
        public bool LoadFromFile(string fileName)
        {
            bool result = LoadSessionData(fileName);
            ActiveSession.Info.IsLoaded = true;
            Loaded?.Invoke(null, EventArgs.Empty);
            return result;
        }

        #endregion

        #region Private Methods

        private HierarchicalEmcosPointCollection LoadConfigPoints()
        {
            if (File.Exists(CONFIGURATION_FILENAME))
            {
                try
                {
                    //var result = BaseRepository<EmcosPoint>.XmlDeSerialize(LIST_Balance_POINTS_FILENAME, _callBackAction);
                    var result = BaseDeserializer<EmcosPoint>.GzJsonDeSerialize(CONFIGURATION_FILENAME, _callBackAction);
                    return result?.Children;
                }
                catch (Exception ex)
                {
                    _callBackAction(ex);
                    return null;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Загрузка сессии
        /// </summary>
        /// <param name="fileName">Имя файла, если не указано, то загрузка из стандартного файла <see cref="BALANCE_SESSION_FILENAME"/></param>
        /// <returns>True если загрузка произошла успешно</returns>
        private bool LoadSessionData(string fileName = null)
        {
            bool mustStoreLastSessionFileName = true;
            try
            {
                if (String.IsNullOrWhiteSpace(fileName))
                {
                    fileName = BALANCE_SESSION_FILENAME + SESSION_FILE_EXTENSION;
                    mustStoreLastSessionFileName = false;
                }
                else
                    if (Path.GetExtension(fileName).ToLowerInvariant() != SESSION_FILE_EXTENSION)
                    fileName = fileName + SESSION_FILE_EXTENSION;

                var fi = new FileInfo(Path.Combine(SESSIONS_FOLDER, fileName));
                if (fi.Exists == false)
                    return false;

                BalanceSession balanceSession = null;

                bool isOldVersionFile = false;
                try
                {
                    if (Path.IsPathRooted(fileName) == false)
                        fileName = Path.Combine(SESSIONS_FOLDER, fileName);

                    // попытка прочитать файл как пакет
                    using (System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(fileName, FileMode.Open, FileAccess.Read))
                    {
                            BalanceSessionInfo info = LoadSessionInfoFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_Info, UriKind.Relative))));
                            if (IsSupportedSessionVersion(info.Version))
                            {
                                void unknownVersion()
                                {
                                    string msg = String.Format("Файл '{1}'\nнеизвестной версии - {0}\nЗагрузка невозможна.\nОбновите программу или обратитесь к разработчику.", info.Version, fi.FullName);
                                    EmcosSiteWrapperApp.LogError(msg);
                                    EmcosSiteWrapperApp.ShowError(msg);
                                }

                                switch (info.Version.Major)
                                {
                                    case 1:
                                        switch (info.Version.Minor)
                                        {
                                            case 0:
                                                isOldVersionFile = true;
                                                break;
                                            case 1:
                                            balanceSession = LoadDataFromFileVersion_1_1(package);
                                                break;
                                            default:
                                                unknownVersion();
                                                return false;
                                        }
                                        break;
                                    default:
                                        unknownVersion();
                                        return false;
                                }
                            }
                    }
                }
                catch (IOException ioe)
                {
                    EmcosSiteWrapperApp.LogException(ioe);
                    isOldVersionFile = true;
                }
                catch (Exception e) { isOldVersionFile = true; }

                if (isOldVersionFile)
                {
                    balanceSession = LoadDataFromFileVersion_1_0(fi.FullName);
                }

                if (mustStoreLastSessionFileName)
                    File.WriteAllText(Path.Combine(SESSIONS_FOLDER, "lastsession"), fileName);

                ActiveSession = balanceSession;
                return balanceSession != null;
            }
            catch (Exception ex)
            {
                _callBackAction(ex);
                ActiveSession = null;
                return false;
            }
        }
        /// <summary>
        /// Загрузка файла сессии версии 1.0
        /// </summary>
        /// <remarks>
        /// Файл сессии версии 1.0 представляет собой сериализованный в json объект BalanceSession дополнительно сжатый gzip
        /// </remarks>
        /// <param name="fileName">Имя файла</param>
        /// <param name="balanceSession">Ссылка на сессию</param>
        /// <returns>Сессия</returns>
        private BalanceSession LoadDataFromFileVersion_1_0(string fileName)
        {
            const string UNKNOWN_DEPARTAMENT_NAME = "<неизвестно>";

            BalanceSession balanceSession = new BalanceSession();
            try
            {
                dynamic  obj = ParseJsonFromFile(fileName);
                if (obj != null)
                {
                    var fi = new FileInfo(fileName);
                    BalanceSessionInfo balanceSessionInfo = LoadSessionInfoFromOldFileFormat(obj);
                    balanceSessionInfo.FileSize = fi.Length;
                    balanceSession.Info = balanceSessionInfo;

                    if (obj.Substations != null)
                    {
                        balanceSession.BalancePoints.Clear();

                        // создание списка департаментов (рэс)
                        HashSet<string> departamentsSet = new HashSet<string>();
                        foreach (var item in obj.Substations)
                        {
                            string departament = item?.Departament?.ToString();
                            if (String.IsNullOrWhiteSpace(departament) == false && departamentsSet.Contains(departament) == false)
                                departamentsSet.Add(departament);
                        }
                        Dictionary<string, IHierarchicalEmcosPoint> departamentsDictionary = new Dictionary<string, IHierarchicalEmcosPoint>();
                        foreach (var item in departamentsSet)
                            departamentsDictionary.Add(item, new EmcosPoint() { ElementType = ElementTypes.DEPARTAMENT, TypeCode = "RES", Name = item });
                        departamentsDictionary.Add("?", new EmcosPoint() { ElementType = ElementTypes.DEPARTAMENT, TypeCode = "RES", Name = UNKNOWN_DEPARTAMENT_NAME });
                        System.Collections.IList parentList = departamentsDictionary["?"].Children;

                        void parseBase(IHierarchicalEmcosPoint source, dynamic data)
                        {
                            try
                            {
                                source.Id = data.Id ?? 0;
                                source.Code = data.Code;
                                source.Name = data.Title;
                                source.Status = data.Status;
                                source.Description = data.Description;
                                if (String.IsNullOrEmpty(source.Description) == false)
                                    balanceSession.DescriptionsById.Add(source.Id, source.Description);
                            }
                            catch (Exception e)
                            {
                                _callBackAction(e);
                            }
                        }

                        object getPropertyValue(string propertyName, dynamic jObject)
                        {
                            if ((jObject is Newtonsoft.Json.Linq.JObject o) && (o.Property(propertyName) != null))
                            {
                                return o.Property(propertyName).Value;
                            }
                            else
                                return null;
                        }

                        double? getDoubleValue(string propertyName, dynamic jObject)
                        {
                            object value = getPropertyValue(propertyName, jObject);
                            if (value == null)
                                return null;
                            else
                                return Convert.ToDouble(value);
                        }
                        bool getBoolValue(string propertyName, dynamic jObject)
                        {
                            object value = getPropertyValue(propertyName, jObject);
                            if (value == null)
                                return false;
                            else
                                return Convert.ToBoolean(value);
                        }

                        void parseTokens(IEnumerable<Newtonsoft.Json.Linq.JToken> tokens)
                        {
                            foreach (Newtonsoft.Json.Linq.JToken jobj in tokens)
                            {
                                if (jobj.Children().Where(i => (i is Newtonsoft.Json.Linq.JProperty p) && p.Name == "$type").FirstOrDefault() is Newtonsoft.Json.Linq.JProperty typeProperty)
                                {
                                    string tokenType = typeProperty.Value.ToString();
                                    if (String.IsNullOrWhiteSpace(tokenType) == false)
                                    {
                                        string t = tokenType.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)[0];
                                        string[] parts = t.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                        if (parts.Length > 1 && parts[0] == "TMP")
                                        {
                                            string part = parts[parts.Length - 1];

                                            IBalanceGroupItem group = null;
                                            dynamic data = jobj;
                                            switch (part)
                                            {
                                                case "Substation":
                                                    group = new Substation
                                                    {
                                                        Departament = data.Departament,
                                                        Voltage = data.Voltage
                                                    };
                                                    break;
                                                case "SubstationSection":
                                                    group = new SubstationSection
                                                    {
                                                        Voltage = data.Voltage
                                                    };
                                                    break;
                                                case "SubstationPowerTransformers":
                                                    group = new SubstationPowerTransformers();
                                                    break;
                                                case "SubstationAuxiliary":
                                                    group = new SubstationAuxiliary();
                                                    break;
                                                default:
                                                    {
                                                        IBalanceItem item = null;
                                                        switch (part)
                                                        {
                                                            case "Fider":
                                                                item = new Fider();
                                                                break;
                                                            case "PowerTransformer":
                                                                item = new PowerTransformer();
                                                                break;
                                                            case "UnitTransformer":
                                                                item = new UnitTransformer();
                                                                break;
                                                            case "UnitTransformerBus":
                                                                item = new UnitTransformerBus();
                                                                break;
                                                            default:
                                                                System.Diagnostics.Debugger.Break();
                                                                break;
                                                        }
                                                        if (item != null)
                                                            parseBase(item, data);

                                                        object value = getPropertyValue("DailyEplus", data);
                                                        string daysValues = value?.ToString();
                                                        item.ActiveEnergy.Plus.DaysValues = (daysValues != null && daysValues.StartsWith("<"))
                                                            ? null
                                                            : (value as Newtonsoft.Json.Linq.JToken)?.ToObject<List<double?>>();

                                                        value = getPropertyValue("DailyEminus", data);
                                                        daysValues = value?.ToString();
                                                        item.ActiveEnergy.Minus.DaysValues = (daysValues != null && daysValues.StartsWith("<"))
                                                            ? null
                                                            : (value as Newtonsoft.Json.Linq.JToken)?.ToObject<List<double?>>();

                                                        item.ActiveEnergy.Plus.MonthValue = getDoubleValue("MonthEplus", data);
                                                        item.ActiveEnergy.Minus.MonthValue = getDoubleValue("MonthEminus", data);

                                                        item.ActiveEnergy.Plus.CorrectionValue = getDoubleValue("Eplus", data) - getDoubleValue("DayEplusValue", data);
                                                        item.ActiveEnergy.Minus.CorrectionValue = getDoubleValue("Eminus", data) - getDoubleValue("DayEminusValue", data);

                                                        item.ActiveEnergy.Plus.UseMonthValue = getBoolValue("UseMonthValue", data);
                                                        item.ActiveEnergy.Minus.UseMonthValue = getBoolValue("UseMonthValue", data);

                                                        if (group != null)
                                                            group.Children.Add(item);
                                                        if (item == null && group == null)
                                                            System.Diagnostics.Debugger.Break();
                                                    }
                                                    break;
                                            }
                                            if (group != null)
                                            {
                                                parseBase(group, data);

                                                if (group is Substation substation)
                                                    departamentsDictionary[substation.Departament].Children.Add(substation);
                                                else
                                                    parentList.Add(group);

                                                var childrenProperty = jobj.Children()
                                                    .Where(i => (i is Newtonsoft.Json.Linq.JProperty p) && p.Name == "Children")
                                                    .FirstOrDefault() as Newtonsoft.Json.Linq.JProperty;
                                                if (childrenProperty != null && childrenProperty.Value != null && childrenProperty.Value is Newtonsoft.Json.Linq.JArray childrenArray)
                                                {
                                                    System.Collections.IList oldParentList = parentList;
                                                    parentList = group.Children;
                                                    parseTokens(childrenArray);
                                                    parentList = oldParentList;
                                                }
                                                else
                                                    System.Diagnostics.Debugger.Break();
                                            }
                                        }
                                    } // значение типа токена пустое
                                    else
                                        System.Diagnostics.Debugger.Break();
                                } // не найден тип токена
                                else
                                    System.Diagnostics.Debugger.Break();
                            }
                        }
                        parseTokens(obj.Substations);

                        foreach (var item in departamentsDictionary)
                            if (item.Key != "?")
                                balanceSession.BalancePoints.Add(item.Value);
                            else
                                if (item.Value.HasChildren)
                                    balanceSession.BalancePoints.Add(item.Value);

                        foreach (var substation in balanceSession.Substations)
                        {
                            balanceSession.ActiveEnergyBalanceById.Add(substation.Id, new Balance<ActiveEnergy>(substation));
                            balanceSession.ReactiveEnergyBalanceById.Add(substation.Id, new Balance<ReactiveEnergy>(substation));
                        }
                    }
                }
            }
            catch (Exception ex) { _callBackAction(ex); }

            //var session = BaseRepository<BalanceSession>.GzJsonDeSerialize(Path.Combine(SESSIONS_FOLDER, fileName), _callBackAction);

            //if (session != null)
            //{
            //    session.FileName = fi.Name;
            //    session.FileSize = fi.Length;
            //    session.LastModifiedDate = fi.LastWriteTime;
            //    session.IsLoaded = true;
            //    // сохранение имени файла последней сессии
            //    if (mustStoreLastSessionFileName)
            //        File.WriteAllText(Path.Combine(SESSIONS_FOLDER, "lastsession"), fileName);

            //    //
            //    if (session.BalancePoints != null)
            //        foreach (IHierarchicalEmcosPoint point in session.BalancePoints)
            //        {
            //            point.Status = DataStatus.Processed;
            //            foreach (var item in point.Children.FlatItemsList)
            //                item.Status = DataStatus.Processed;
            //        }
            //}
            return balanceSession;
        }
        
        /// <summary>
        /// Загрузка файла сессии версии 1.1
        /// </summary>
        /// <returns>Сессия</returns>
        private BalanceSession LoadDataFromFileVersion_1_1(System.IO.Packaging.Package package)
        {
            BalanceSession balanceSession = new BalanceSession
            {
                Info = LoadSessionInfoFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_Info, UriKind.Relative)))),

                BalancePoints = LoadBalancePointsFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_BalancePoints, UriKind.Relative)))),
                ActiveEnergyBalanceById = LoadActiveEnergyBalanceFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_ActiveEnergyBalanceById, UriKind.Relative)))),
                ReactiveEnergyBalanceById = LoadReactiveEnergyBalanceFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_ReactiveEnergyBalanceById, UriKind.Relative)))),
                ActiveEnergyById = LoadActiveEnergyFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_ActiveEnergyById, UriKind.Relative)))),
                ReactiveEnergyById = LoadReactiveEnergyFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_ReactiveEnergyById, UriKind.Relative)))),
                DescriptionsById = LoadDescriptionsFromPackage(package.GetPart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(PART_DescriptionsById, UriKind.Relative))))
            };
            return balanceSession;
        }

        /// <summary>
        /// Определяет поддерживается ли указанная версия файла сессии
        /// </summary>
        /// <param name="version">версия файла сессии</param>
        /// <returns>True, если поддерживается</returns>
        private bool IsSupportedSessionVersion(Version version)
        {
            if (version == null)
                return false;

            switch (version.CompareTo(LastSupportedVersion))
            {
                case 0:
                    return true;
                case 1:
                    return false;
                case -1:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Чтение версии из файла сессии старого типа
        /// </summary>
        /// <param name="fileName">имя файла сессии</param>
        /// <returns></returns>
        private Version LoadVersionFromZip(string fileName)
        {
            try
            {
                dynamic obj = ParseJsonFromFile(fileName);
                if (obj != null)
                {
                    return obj.Version;
                }
                else
                    return null;
            }
            catch { return null; }
        }
        /// <summary>
        /// Чтение информации о сессии из файла сессии старого типа
        /// </summary>
        /// <param name="fileName">имя файла сессии</param>
        /// <returns>информации о сессии</returns>
        private BalanceSessionInfo LoadSessionInfoFromOldFileFormat(string fileName)
        {
            try
            {
                dynamic obj = ParseJsonFromFile(fileName);
                BalanceSessionInfo balanceSessionInfo = LoadSessionInfoFromOldFileFormat(obj);
                var fi = new FileInfo(fileName);
                balanceSessionInfo.FileSize = fi.Length;
                return balanceSessionInfo;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }
        /// <summary>
        /// Чтение информации о сессии из файла сессии старого типа
        /// </summary>
        /// <param name="jsonObject">json объект</param>
        /// <returns>информации о сессии</returns>
        private BalanceSessionInfo LoadSessionInfoFromOldFileFormat(dynamic jsonObject)
        {
            if (jsonObject != null)
            {
                BalanceSessionInfo balanceSessionInfo = new BalanceSessionInfo
                {
                    Version = jsonObject.Version,
                    LastModifiedDate = jsonObject.LastModifiedDate,
                    Period = jsonObject.Period.ToObject<DatePeriod>(),
                    FileName = jsonObject.FileName
                };
                return balanceSessionInfo;
            }
            else
                return null;
        }

        /// <summary>
        /// Чтение информации о сессии из файла сессии нового типа
        /// </summary>
        /// <param name="fileName">имя файла сессии</param>
        /// <returns>информации о сессии</returns>
        private BalanceSessionInfo LoadSessionInfoFromPackage(System.IO.Packaging.PackagePart packagePart)
        {
            try
            {
                BalanceSessionInfo bsi = BaseDeserializer<BalanceSessionInfo>.JsonDeSerializeFromStream(packagePart.GetStream(), _callBackAction);
                return bsi;
            }
            catch (IOException ioe)
            {
                _callBackAction(ioe);
                return null;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }

        private HierarchicalEmcosPointCollection LoadBalancePointsFromPackage(System.IO.Packaging.PackagePart packagePart)
        {
            try
            {
                HierarchicalEmcosPointCollection epc = BaseDeserializer<HierarchicalEmcosPointCollection>.JsonDeSerializeFromStream(packagePart.GetStream(), _callBackAction);
                return epc;
            }
            catch (IOException ioe)
            {
                _callBackAction(ioe);
                return null;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }
        private Dictionary<int, BalanceFormula> LoadBalanceFormulasFromPackage(System.IO.Packaging.PackagePart packagePart)
        {
            try
            {
                Dictionary<int, BalanceFormula> data = BaseDeserializer<Dictionary<int, BalanceFormula>>.JsonDeSerializeFromStream(packagePart.GetStream(), _callBackAction);
                return data;
            }
            catch (IOException ioe)
            {
                _callBackAction(ioe);
                return null;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }
        private Dictionary<int, Balance<ActiveEnergy>> LoadActiveEnergyBalanceFromPackage(System.IO.Packaging.PackagePart packagePart)
        {
            try
            {
                Dictionary<int, Balance<ActiveEnergy>> data = BaseDeserializer<Dictionary<int, Balance<ActiveEnergy>>>.JsonDeSerializeFromStream(packagePart.GetStream(), _callBackAction);
                return data;
            }
            catch (IOException ioe)
            {
                _callBackAction(ioe);
                return null;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }
        private Dictionary<int, Balance<ReactiveEnergy>> LoadReactiveEnergyBalanceFromPackage(System.IO.Packaging.PackagePart packagePart)
        {
            try
            {
                Dictionary<int, Balance<ReactiveEnergy>> data = BaseDeserializer<Dictionary<int, Balance<ReactiveEnergy>>>.JsonDeSerializeFromStream(packagePart.GetStream(), _callBackAction);
                return data;
            }
            catch (IOException ioe)
            {
                _callBackAction(ioe);
                return null;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }
        private Dictionary<int, ActiveEnergy> LoadActiveEnergyFromPackage(System.IO.Packaging.PackagePart packagePart)
        {
            try
            {
                Dictionary<int, ActiveEnergy> data = BaseDeserializer<Dictionary<int, ActiveEnergy>>.JsonDeSerializeFromStream(packagePart.GetStream(), _callBackAction);
                return data;
            }
            catch (IOException ioe)
            {
                _callBackAction(ioe);
                return null;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }
        private Dictionary<int, ReactiveEnergy> LoadReactiveEnergyFromPackage(System.IO.Packaging.PackagePart packagePart)
        {
            try
            {
                Dictionary<int, ReactiveEnergy> data = BaseDeserializer<Dictionary<int, ReactiveEnergy>>.JsonDeSerializeFromStream(packagePart.GetStream(), _callBackAction);
                return data;
            }
            catch (IOException ioe)
            {
                _callBackAction(ioe);
                return null;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }
        private Dictionary<int, string> LoadDescriptionsFromPackage(System.IO.Packaging.PackagePart packagePart)
        {
            try
            {
                Dictionary<int, string> data = BaseDeserializer<Dictionary<int, string>>.JsonDeSerializeFromStream(packagePart.GetStream(), _callBackAction);
                return data;
            }
            catch (IOException ioe)
            {
                _callBackAction(ioe);
                return null;
            }
            catch (Exception e)
            {
                _callBackAction(e);
                return null;
            }
        }

        private string ReadJson(string fileName)
        {
            string json = string.Empty;
            try
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                using (System.IO.MemoryStream mso = new System.IO.MemoryStream())
                using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress, false))
                {
                    byte[] bytes = new byte[500_000];
                    int cnt;
                    while ((cnt = gz.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        mso.Write(bytes, 0, cnt);
                    }
                    json = Encoding.UTF8.GetString(mso.ToArray());
                }
            }
            catch (Exception e) { _callBackAction(e); }
            return json;
        }

        private dynamic ParseJsonString(string json)
        {
            if (String.IsNullOrWhiteSpace(json))
                return null;
            return Newtonsoft.Json.Linq.JToken.Parse(json);
        }
        private dynamic ParseJsonFromFile(string fileName)
        {
            return ParseJsonString(ReadJson(fileName));
        }

        #endregion
    }
}
