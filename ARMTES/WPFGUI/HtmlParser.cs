using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMP.Wpf.CommonControls.Dialogs;

namespace TMP.ARMTES
{
    using TMP.ARMTES.Model;
    public class HtmlParser
    {
        #region View Element

        public static SelectElementModel ViewElement(string data, string parentId, ProgressDialogController progressController)
        {
            //Debug.Assert(String.IsNullOrEmpty(data), "data is empty!");

            SelectElementModel result = new SelectElementModel();
            //data = System.IO.File.ReadAllText("data");

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(data);

            if (doc.DocumentNode.ChildNodes.Count > 1)
            {
                HtmlAgilityPack.HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@id='collectorsDiv']/table/tbody/tr");

                if (null != nodes)
                {
                    HtmlAgilityPack.HtmlNode row;
                    HtmlAgilityPack.HtmlNodeCollection fields;

                    bool nextIsObject = false;
                    bool nextIsCounter = false;
                    int startIndex = 0;
                    int toProcessRows = 0;

                    HtmlNode td;
                    HtmlNode img;
                    string text;

                    int collectorId = 1;

                    try
                    {
                        result.Collectors = new List<Collector>();

                        // количество точек
                        int pointsCount = nodes.Count;
                        if (progressController != null)
                            progressController.SetMessage(String.Format("Найдено {0} расчётных точек. Получение данных...", nodes.Count));

                        double progress = 0.0d;

                        int globalRowIndex = 0;
                        while (globalRowIndex < nodes.Count)
                        {
                            row = nodes[globalRowIndex];
                            fields = row.SelectNodes("td");

                            toProcessRows = 1;
                            if (fields[0].HasAttributes && fields[0].Attributes["rowspan"] != null)
                                Int32.TryParse(fields[0].Attributes["rowspan"].Value, out toProcessRows);

                            Collector collector = new Collector();
                            try
                            {
                                #region разбор системы

                                collector.Id = collectorId.ToString();
                                collectorId++;

                                // статус модема
                                td = fields[0];
                                if (td.ChildNodes["img"] != null)
                                {
                                    img = td.ChildNodes["img"];
                                    if (img.Attributes["src"] != null)
                                        if (img.Attributes["src"].Value.Contains("не отвечает"))
                                            collector.IsAnswered = false;
                                        else
                                            collector.IsAnswered = true;
                                }

                                // это УСПД
                                td = fields[1];
                                if (td.HasChildNodes && td.ChildNodes["img"] != null)
                                {
                                    collector.IsUSPD = true;
                                    img = td.ChildNodes["img"];
                                    if (img.Attributes["src"] != null)
                                        if (img.Attributes["src"].Value.Contains("не отвечает"))
                                            collector.IsAnsweredUSPD = false;
                                        else
                                            collector.IsAnsweredUSPD = true;
                                }
                                else
                                {
                                    collector.IsUSPD = false;
                                }

                                // УСПД сетевой адрес
                                td = fields[2];
                                text = td.InnerText;
                                byte bvalue;
                                Byte.TryParse(text, out bvalue);
                                collector.NetworkAddress = bvalue;

                                // номер GSM - PhoneNumberColumn
                                td = fields[3];
                                text = td.InnerText;
                                collector.PhoneNumber = text;

                                #endregion
                            }
                            catch (Exception e)
                            {
                                throw e;
                            }
                            nextIsObject = true;

                            int processedRows = 0;
                            while (processedRows < toProcessRows)
                            {

                                row = nodes[globalRowIndex];
                                fields = row.SelectNodes("td");
                                startIndex = nextIsObject ? 4 : 0;

                                int countersCount = 1;
                                if (fields[startIndex].HasAttributes && fields[startIndex].Attributes["rowspan"] != null)
                                    Int32.TryParse(fields[startIndex].Attributes["rowspan"].Value, out countersCount);
                                AccountingObject accountingObject = new AccountingObject();
                                try
                                {
                                    #region разбор объекта                               

                                    // населенный пункт - CityColumn
                                    td = fields[startIndex++];
                                    text = td.InnerText;
                                    accountingObject.City = text;

                                    // № договора - ContractColumn
                                    td = fields[startIndex++];
                                    text = td.InnerText;
                                    accountingObject.Contract = text;

                                    // Наименование абонента - SubscriberColumn
                                    td = fields[startIndex++];
                                    text = td.InnerText;
                                    accountingObject.Subscriber = text;

                                    // Объект учета - ObjectColumn
                                    td = fields[startIndex++];
                                    text = td.InnerText;
                                    accountingObject.Name = text;

                                    // Номер ТП - TpColumn
                                    td = fields[startIndex++];
                                    text = td.InnerText;
                                    accountingObject.Tp = text;

                                    if (td.Attributes["onclick"] != null)
                                    {
                                        string link = td.Attributes["onclick"].Value;

                                        string[] linkParts = link.Split(new char[] { '(', ',', ')' });

                                        if (linkParts.Length != 4)
                                            throw new ArgumentException("Не удалось разобрать ссылку объекта.");

                                        string deviceId = linkParts[1];
                                        string selectedElementId = linkParts[2].Trim(new char[] { '\'', '\'' });


                                        accountingObject.TpLink = link;
                                        accountingObject.Id = deviceId;
                                    }
                                    else
                                        Debugger.Break();

                                    #endregion
                                }
                                catch (Exception e)
                                {
                                    throw e;
                                }
                                nextIsCounter = true;
                                for (int k = 0; k < countersCount; k++)
                                {

                                    row = nodes[globalRowIndex];
                                    fields = row.SelectNodes("td");
                                    startIndex = nextIsObject ? 9 : (nextIsCounter ? 5 : 0);
                                    nextIsObject = false;
                                    nextIsCounter = false;

                                    try
                                    {
                                        #region разбор учёта
                                        Counter counter = new Counter();
                                        long lvalue = 0;
                                        // статус счётчика
                                        td = fields[startIndex++];
                                        if (td.ChildNodes["img"] != null)
                                        {
                                            img = td.ChildNodes["img"];
                                            if (img.Attributes["src"] != null)
                                            {
                                                if (img.Attributes["src"].Value.Contains("не отвечает"))
                                                    counter.IsAnswered = false;
                                                else
                                                    counter.IsAnswered = true;
                                                if (img.Attributes["src"].Value.Contains("wrongSettings"))
                                                    counter.HasWrongSettings = true;
                                                else
                                                    counter.HasWrongSettings = false;
                                            }
                                            counter.AmperPointId = img.Attributes["alt"].Value;

                                            string link = td.Attributes["onclick"].Value;

                                            if (string.IsNullOrEmpty(link))
                                                Debugger.Break();

                                            string[] linkParts = link.Split(new char[] { '(', ',', ')' });

                                            if (linkParts.Length != 4)
                                                System.Diagnostics.Debugger.Break();

                                            string counterId = linkParts[1];
                                            string selectedElementId = linkParts[2].Trim(new char[] { '\'', '\'' });

                                            counter.Id = counterId;
                                            counter.CounterLink = link;
                                        }

                                        // Расчётная точка
                                        td = fields[startIndex++];
                                        text = td.InnerText;
                                        counter.AccountingPoint = text;
                                        if (td.HasAttributes && td.Attributes["class"] != null)
                                        {
                                            if (td.Attributes["class"].Value.Contains("noPersonalAccount"))
                                                counter.MissingPersonalAccount = true;
                                            else
                                                counter.MissingPersonalAccount = false;
                                        }

                                        // тип
                                        td = fields[startIndex++];
                                        text = td.InnerText;
                                        counter.CounterType = text;

                                        // сетевой аддрес
                                        td = fields[startIndex++];
                                        text = td.InnerText;
                                        counter.CounterNetworkAdress = text;

                                        // заводской номер
                                        td = fields[startIndex++];
                                        text = td.InnerText;
                                        counter.SerialNumber = text;

                                        // предыдущие показания
                                        td = fields[startIndex++];
                                        text = td.InnerText;
                                        lvalue = 0;
                                        long.TryParse(text, out lvalue);
                                        counter.PreviousIndication = String.IsNullOrWhiteSpace(text) ? new Nullable<long>() : lvalue;

                                        counter.PreviousIndications = new Indications();
                                        counter.PreviousIndications.Tarriff0 = GetIndication(text);

                                        // текущие показания
                                        td = fields[startIndex++];
                                        text = td.InnerText;
                                        lvalue = 0;
                                        long.TryParse(text, out lvalue);
                                        counter.NextIndication = String.IsNullOrWhiteSpace(text) ? new Nullable<long>() : lvalue;

                                        counter.NextIndications = new Indications();
                                        counter.NextIndications.Tarriff0 = GetIndication(text);

                                        // разница
                                        td = fields[startIndex++];
                                        text = td.InnerText;
                                        lvalue = 0;
                                        long.TryParse(text, out lvalue);
                                        counter.Difference = String.IsNullOrWhiteSpace(text) ? new Nullable<long>() : lvalue;

                                        #endregion

                                        accountingObject.Counters.Add(counter);
                                    }
                                    catch (Exception e)
                                    {
                                        throw e;
                                    }
                                    processedRows++;
                                    globalRowIndex++;

                                    progress = (double)globalRowIndex / pointsCount;
                                    if (progressController != null)
                                        progressController.SetProgress(progress);
                                }
                                collector.Objects.Add(accountingObject);
                            }
                            result.Collectors.Add(collector);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentOutOfRangeException("Ошибка разбора данных: структура данных информации об объекте изменилась.\r\nОбратитесь к разработчику.", e);
                    }
                }
                // статистика
                nodes = doc.DocumentNode.SelectNodes("/div/div/div/table/tbody/tr");

                if (null != nodes)
                {
                    Statistics statistics = new Statistics();

                    HtmlNode td;
                    string text;
                    int value = 0;

                    #region Разбор данных

                    // Всего модемов
                    td = nodes[0].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.ModemsCount = Int32.TryParse(text, out value) ? value : 0;

                    // Отвечающих модемов
                    td = nodes[1].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.AnsweredModemsCount = Int32.TryParse(text, out value) ? value : 0;

                    // Не отвечающих модемов
                    td = nodes[2].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.NotAnsweredModemsCount = Int32.TryParse(text, out value) ? value : 0;

                    // Всего УСПД
                    td = nodes[3].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.UspdCount = Int32.TryParse(text, out value) ? value : 0;

                    // Отвечающих УСПД
                    td = nodes[4].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.AnsweredUspdCount = Int32.TryParse(text, out value) ? value : 0;

                    // Не отвечающих УСПД
                    td = nodes[5].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.NotAnsweredUspdCount = Int32.TryParse(text, out value) ? value : 0;

                    // Всего точек
                    td = nodes[6].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.CountersCount = Int32.TryParse(text, out value) ? value : 0;

                    // Отвечающих счетчиков
                    td = nodes[7].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.AnsweredCountersCount = Int32.TryParse(text, out value) ? value : 0;

                    // Не отвечающих счетчиков
                    td = nodes[8].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.NotAnsweredCountersCount = Int32.TryParse(text, out value) ? value : 0;

                    // Недостающих данных
                    td = nodes[9].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    statistics.MissingDataCount = Int32.TryParse(text, out value) ? value : 0;

                    #endregion

                    result.Statistics = statistics;
                }
            }
            return result;
        }

        #endregion View Element

        #region View Device

        public static ViewDeviceModel ViewDevice(string data, ProgressDialogController progressController)
        {
            ViewDeviceModel result = new ViewDeviceModel();

            try
            {

                HtmlNode td;
                string text;

                //data = System.IO.File.ReadAllText("data");

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                if (doc == null) return null;
                doc.LoadHtml(data);
                if (doc.DocumentNode == null && doc.DocumentNode.ChildNodes == null) return null;

                if (doc.DocumentNode.ChildNodes.Count > 1)
                {
                    #region Разбор секции с информацией о канале связи

                    HtmlAgilityPack.HtmlNode jqTabsDevices = doc.DocumentNode.SelectNodes("//div[@id='jqTabsDevices']").Single();
                    if (jqTabsDevices == null) return null;
                    HtmlNodeCollection sessionInformation = jqTabsDevices.SelectNodes("div[2]/div/table/tbody/tr");
                    if (sessionInformation == null) return null;

                    result.Session = new SessionInformation();

                    // производитель модема
                    td = sessionInformation[2].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    result.Session.ModemManufacturer = text;

                    // Модель устройства
                    td = sessionInformation[3].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    result.Session.Model = text;

                    // описание
                    td = sessionInformation[4].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    result.Session.Description = text;

                    // статус
                    td = sessionInformation[6].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    result.Session.CurrentStatus = text;

                    // сеанс
                    td = sessionInformation[7].SelectNodes("td[2]").Single();
                    text = td.InnerText;
                    DateTime date = new DateTime();
                    result.Session.LastSessionDate = DateTime.TryParse(text, out date) ? date : date;

                    #endregion

                    #region Разбор секции с показаниями

                    HtmlAgilityPack.HtmlNode jqTabsBalances = doc.DocumentNode.SelectNodes("//div[@id='jqTabsBalances']").Single();
                    if (jqTabsBalances == null) return null;
                    HtmlNodeCollection counters = jqTabsBalances.SelectNodes("table/tbody/tr");
                    if (counters == null) return null;

                    result.CountersIndications = new List<IndicationViewItem>();

                    byte startIndex = 0;
                    for (int i = 0; i < counters.Count; i++)
                    {
                        HtmlNodeCollection hnc = counters[i].SelectNodes("td");
                        startIndex = 0;

                        IndicationViewItem ivi = new IndicationViewItem();
                        ivi.PreviousIndications = new Indications();
                        ivi.NextIndications = new Indications();

                        #region Парсинг
                        // точка
                        td = hnc[startIndex++];
                        text = td.InnerText;
                        ivi.AccountingPoint = text;

                        // тип
                        td = hnc[startIndex++];
                        text = td.InnerText;
                        ivi.CounterType = text;

                        // предыдущие показания T0
                        td = hnc[startIndex++];
                        ivi.PreviousIndications.Tarriff0 = GetIndication(td.InnerText);
                        // предыдущие показания T1
                        td = hnc[startIndex++];
                        ivi.PreviousIndications.Tarriff1 = GetIndication(td.InnerText);
                        // предыдущие показания T2
                        td = hnc[startIndex++];
                        ivi.PreviousIndications.Tarriff2 = GetIndication(td.InnerText);
                        // предыдущие показания T3
                        td = hnc[startIndex++];
                        ivi.PreviousIndications.Tarriff3 = GetIndication(td.InnerText);
                        // предыдущие показания T4
                        td = hnc[startIndex++];
                        ivi.PreviousIndications.Tarriff4 = GetIndication(td.InnerText);
                        // предыдущие показания достоверность
                        td = hnc[startIndex++];
                        text = td.InnerText;
                        ivi.PreviousIndications.DataReliability = text;

                        // текущие показания T0
                        td = hnc[startIndex++];
                        ivi.NextIndications.Tarriff0 = GetIndication(td.InnerText);
                        // текущие показания T1
                        td = hnc[startIndex++];
                        ivi.NextIndications.Tarriff1 = GetIndication(td.InnerText);
                        // текущие показания T2
                        td = hnc[startIndex++];
                        ivi.NextIndications.Tarriff2 = GetIndication(td.InnerText);
                        // текущие показания T3
                        td = hnc[startIndex++];
                        ivi.NextIndications.Tarriff3 = GetIndication(td.InnerText);
                        // текущие показания T4
                        td = hnc[startIndex++];
                        ivi.NextIndications.Tarriff4 = GetIndication(td.InnerText);
                        // предыдущие показания достоверность
                        td = hnc[startIndex++];
                        text = td.InnerText;
                        ivi.NextIndications.DataReliability = text;

                        // разница
                        td = hnc[startIndex++];
                        ivi.Difference = GetIndication(td.InnerText);

                        #endregion

                        result.CountersIndications.Add(ivi);
                    }
                    #endregion

                    #region Качество показаний

                    if (result.QualityIndications == null)
                        result.QualityIndications = new List<QualityIndications>();

                    HtmlNodeCollection indicationsQualityMonths = doc.DocumentNode.SelectNodes("//table[contains(@class,'tableQualityIndications')]");

                    if (indicationsQualityMonths != null)
                    {
                        int monthsCount = indicationsQualityMonths.Count;
                        for (int monthIndex = 0; monthIndex < monthsCount; monthIndex++)
                        {
                            QualityIndications qi = new QualityIndications();
                            HtmlNode m = indicationsQualityMonths[monthIndex].SelectNodes("thead/tr[1]/th[2]").Single();
                            qi.Period = m == null ? "???" : m.InnerText;
                            qi.PointsData = ParseMonthQualityIndications(indicationsQualityMonths[monthIndex].SelectNodes("tbody/tr"));

                            result.QualityIndications.Add(qi);
                        }
                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {
                //TODO: Добавить логирование
                App.LogException(ex);
                return null;
            }

            return result;
        }

        #endregion

        #region View Counter

        public static ViewCounterModel ViewCounter(string data, ProgressDialogController progressController)
        {
            HtmlNode td;
            string text;

            ViewCounterModel result = new ViewCounterModel();

            //data = System.IO.File.ReadAllText("data");

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            if (doc == null) return null;
            doc.LoadHtml(data);
            if (doc.DocumentNode == null && doc.DocumentNode.ChildNodes == null) return null;

            if (doc.DocumentNode.ChildNodes.Count > 1)
            {
                #region Разбор секции с информацией о канале связи

                HtmlAgilityPack.HtmlNode jqTabsDevices = doc.DocumentNode.SelectNodes("//div[@id='jqTabsDevices']").Single();
                if (jqTabsDevices == null) return null;
                HtmlNodeCollection info = jqTabsDevices.SelectNodes("div[2]/table/tbody/tr");
                if (info == null) return null;

                // Наименование точки учета
                td = info[0].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.AccountPoint = text;

                // Тип счетчика
                td = info[1].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.CounterType = text;

                // Заводской номер
                td = info[2].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                result.CounterNumber = text;

                // Сетевой адрес
                td = info[3].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.CounterNetworkAddress = text;

                // Коэффициент трансформации
                td = info[5].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.Ktt = text;

                // Производитель
                td = info[6].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                result.CounterManufacturer = text;

                // Тип учёта
                td = info[7].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                result.AccountType = text;

                // Полное название абонента
                td = info[8].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                result.AbonentFullName = text;
                // Название абонента
                td = info[9].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                result.AbonentName = text;
                // Короткое название абонента
                td = info[10].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                result.AbonentShortName = text;

                // Подстанция
                td = info[11].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.Substation = text;

                // Название объекта
                td = info[12].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.ObjectName = text;

                // Название точки учета
                td = info[13].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.AccountPointName = text;

                // Номер ТП
                td = info[14].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.TP = text;

                // Адрес объекта
                td = info[15].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.ObjectAddress = text;

                // Населенный пункт объекта
                td = info[16].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.ObjectState = text;

                // Адрес абонента
                td = info[17].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.AbonentAddress = text;

                // Фидер
                td = info[18].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.Fider = text;

                // Номер договора
                td = info[19].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.DogNumber = text;

                // Родительский лиц счет
                td = info[20].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                result.AmperParentPointId = text;

                // РЭС
                td = info[21].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.Departament = text;

                // Зав. номер из расч системы
                td = info[22].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.AmperCounterNumber = text;

                // Лицевой счет
                td = info[23].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                result.AmperPointId = text;

                // Текущий статус
                td = info[24].SelectNodes("td[2]").Single();
                text = td.InnerText;
                result.Status = text;

                // Последний сеанс
                td = info[25].SelectNodes("td[2]").Single();
                text = td.InnerText.Trim();
                DateTime date = new DateTime();
                result.LastSessionDate = DateTime.TryParse(
                    text,
                    System.Globalization.CultureInfo.CreateSpecificCulture("en-US"),
                    System.Globalization.DateTimeStyles.None,
                    out date) ? date : date;

                #endregion

                #region Разбор секции с показаниями

                HtmlAgilityPack.HtmlNode jqTabsSingleMeterIndications = doc.DocumentNode.SelectNodes("//div[@id='jqTabsSingleMeterIndications']").Single();
                info = jqTabsSingleMeterIndications.SelectNodes("table/tbody/tr/td");
                if (info == null) return null;


                IndicationViewItem ivi = new IndicationViewItem();
                ivi.PreviousIndications = new Indications();
                ivi.NextIndications = new Indications();

                #region Парсинг

                int startIndex = 0;

                // точка
                td = info[startIndex++];
                text = td.InnerText;
                ivi.AccountingPoint = text;

                // тип
                td = info[startIndex++];
                text = td.InnerText;
                ivi.CounterType = text;

                // предыдущие показания T0
                td = info[startIndex++];
                ivi.PreviousIndications.Tarriff0 = GetIndication(td.InnerText);
                // предыдущие показания T1
                td = info[startIndex++];
                ivi.PreviousIndications.Tarriff1 = GetIndication(td.InnerText);
                // предыдущие показания T2
                td = info[startIndex++];
                ivi.PreviousIndications.Tarriff2 = GetIndication(td.InnerText);
                // предыдущие показания T3
                td = info[startIndex++];
                ivi.PreviousIndications.Tarriff3 = GetIndication(td.InnerText);
                // предыдущие показания T4
                td = info[startIndex++];
                ivi.PreviousIndications.Tarriff4 = GetIndication(td.InnerText);
                // предыдущие показания достоверность
                td = info[startIndex++];
                text = td.InnerText;
                ivi.PreviousIndications.DataReliability = text;

                // текущие показания T0
                td = info[startIndex++];
                ivi.NextIndications.Tarriff0 = GetIndication(td.InnerText);
                // текущие показания T1
                td = info[startIndex++];
                ivi.NextIndications.Tarriff1 = GetIndication(td.InnerText);
                // текущие показания T2
                td = info[startIndex++];
                ivi.NextIndications.Tarriff2 = GetIndication(td.InnerText);
                // текущие показания T3
                td = info[startIndex++];
                ivi.NextIndications.Tarriff3 = GetIndication(td.InnerText);
                // текущие показания T4
                td = info[startIndex++];
                ivi.NextIndications.Tarriff4 = GetIndication(td.InnerText);
                // предыдущие показания достоверность
                td = info[startIndex++];
                text = td.InnerText;
                ivi.NextIndications.DataReliability = text;

                // разница
                td = info[startIndex++];
                ivi.Difference = GetIndication(td.InnerText);

                #endregion

                result.IndicationViewItem = ivi;

                #endregion
            }
            return result;
        }

        #endregion

        private static List<PointQualityIndications> ParseMonthQualityIndications(HtmlNodeCollection nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("ParseMonthQualityIndications - argument is null!");

            List<PointQualityIndications> result = new List<PointQualityIndications>();
            HtmlNode td;
            string text;

            int countersCount = nodes.Count;

            for (int i = 0; i < countersCount; i++)
            {
                PointQualityIndications qi = new PointQualityIndications();

                HtmlNodeCollection row = nodes[i].SelectNodes("td");

                td = row[0].SelectNodes("span").Single();
                text = td.InnerText;
                qi.PointName = text;

                int daysCount = row.Count - 1;
                qi.Values = new List<PointQualityIndicationsLegend>(daysCount);
                for (int day = 1; day <= daysCount; day++)
                {
                    PointQualityIndicationsLegend qil = new PointQualityIndicationsLegend();
                    qil.Interval = row[day].InnerText;
                    string quality = row[day].Attributes["class"].Value;
                    if (quality.Contains("greenQuality"))
                        qil.Type = PointQualityIndicationsType.Reliable;
                    else
                        if (quality.Contains("redQuality"))
                        qil.Type = PointQualityIndicationsType.Reliable;
                    else
                        qil.Type = PointQualityIndicationsType.NotRead;
                    qi.Values.Add(qil);
                }
                result.Add(qi);
            }
            return result;
        }

        internal static double? GetIndication(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
                return new Nullable<double>();
            double value = 0.0d;
            if (double.TryParse(data, out value))
                return value;
            else
            {
                if (double.TryParse(data, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out value))
                    return value;

                return new Nullable<double>();
            }
        }
    }
}