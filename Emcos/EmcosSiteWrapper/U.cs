using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TMP.Work.Emcos
{
    public static class U
    {
        public static string GetExceptionDetails(Exception exp)
        {
            var messageBoxText = string.Empty;

            try
            {
                // Write Message tree of inner exception into textual representation
                messageBoxText = exp.Message;

                var innerEx = exp.InnerException;

                for (int i = 0; innerEx != null; i++, innerEx = innerEx.InnerException)
                {
                    var spaces = string.Empty;

                    for (int j = 0; j < i; j++)
                        spaces += "  ";

                    messageBoxText += "\n" + spaces + "└─>" + innerEx.Message;
                }
            }
            catch
            {
            }
            return messageBoxText;
        }

        public static AsyncCallback SyncContextCallback(AsyncCallback callback)
        {
            // Фиксируем производный от SyncronizationContext.Current объект вызывающего потока
            var sc = SynchronizationContext.Current;

            // При отсутствии контекста синхронизации возвращаем переданное в метод
            if (sc == null) return callback;

            // Возвращаем делегат, который отправляет в фиксированный контекст синхронизации
            // метод, передающий в  исходный вызов AsyncCallback аргумент IAsyncResult
            return asyncResult => sc.Post(result => callback((IAsyncResult)result), asyncResult);
        }

        /*****************************************************************/

        public static string InputBox(string title, string message, string initText = null)
        {
            var w = new Window();
            w.Owner = App.Current.MainWindow;
            w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            w.WindowStyle = WindowStyle.SingleBorderWindow;
            w.Title = title;
            w.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            w.TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            w.SizeToContent = SizeToContent.WidthAndHeight;
            w.ResizeMode = ResizeMode.NoResize;
            w.ShowInTaskbar = false;
            w.FontFamily = new System.Windows.Media.FontFamily("Calibri,Verdana,Tahoma");
            w.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            w.SetValue(System.Windows.Media.TextOptions.TextFormattingModeProperty, System.Windows.Media.TextFormattingMode.Ideal);
            w.Icon = App.Current.MainWindow.Icon;
            var grid = new Grid();
            grid.Margin = new Thickness(10d);
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            w.Content = grid;

            var tbMessage = new TextBlock();
            tbMessage.TextAlignment = TextAlignment.Center;
            tbMessage.Text = message;
            tbMessage.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            Grid.SetRow(tbMessage, 0);

            var tb = new TextBox();
            tb.TextAlignment = TextAlignment.Center;
            tb.Text = initText;
            tb.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            Grid.SetRow(tb, 1);

            var btnSave = new Button();
            btnSave.Padding = new Thickness(10, 2, 10, 2);
            btnSave.Margin = new Thickness(0, 10, 10, 2);
            btnSave.IsDefault = true;
            btnSave.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            btnSave.Content = "Сохранить";
            btnSave.Click += (s, e) =>
            {
                w.DialogResult = true;
                w.Close();
            };

            var btnCancel = new Button();
            btnCancel.Padding = new Thickness(10, 2, 10, 2);
            btnCancel.Margin = new Thickness(10, 10, 0, 2);
            btnCancel.IsCancel = true;
            btnCancel.SetResourceReference(Window.FontSizeProperty, "SubHeaderFontSize");
            btnCancel.Content = "Отменить";

            var sp = new StackPanel();
            sp.HorizontalAlignment = HorizontalAlignment.Center;
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(btnSave);
            sp.Children.Add(btnCancel);
            Grid.SetRow(sp, 2);

            grid.Children.Add(tbMessage);
            grid.Children.Add(tb);
            grid.Children.Add(sp);

            w.SetValue(System.Windows.Input.FocusManager.FocusedElementProperty, tb);

            var result = w.ShowDialog();

            if (result.HasValue)
                return tb.Text;
            else
                return null;
        }

        /*****************************************************************/

            /// <summary>
            /// Получение суточных данных по указанной подстанции
            /// </summary>
            /// <param name="startDate"></param>
            /// <param name="endDate"></param>
            /// <param name="substation"></param>
            /// <param name="cts"></param>
            /// <param name="callback"></param>
            /// <returns></returns>
        public static bool GetDaylyArchiveDataForSubstation(DateTime startDate, DateTime endDate, 
            Model.Balans.Substation substation, System.Threading.CancellationTokenSource cts, Action<int, int> callback)
        {
            bool error;

            int current = 0, total = 0;

            bool hasMonthData;
            var nextMonthOfEndDate = endDate.AddMonths(1);
            var nextMonthOfEndDate2 = new DateTime(nextMonthOfEndDate.Year, nextMonthOfEndDate.Month, 1);
            int lastDayInEndDateMonth = nextMonthOfEndDate2.AddDays(-1).Day;
            if (startDate.Day == 1 && endDate.Day == lastDayInEndDateMonth)
                hasMonthData = true;
            else
                hasMonthData = false;
            // измерения - А энергия за сутки
            var measurementsDaysPart = @"T2_TYPE_0=MSF&T2_NAME_0=А энергия&T2_MSF_ID_0=14&T2_MSF_NAME_0=А энергия&T2_AGGS_TYPE_ID_0=3&T2_MS_TYPE_ID_0=1";
            // А энергия за месяц
            var measurementsMonthPart = @"&T2_TYPE_1=MSF&T2_NAME_1=А энергия&T2_MSF_ID_1=14&T2_MSF_NAME_1=А энергия&T2_AGGS_TYPE_ID_1=4&T2_MS_TYPE_ID_1=1";
            // 
            var measurementsEndPartIfNotHasMonth = "&T2_COUNT=1&";
            var measurementsEndPartIfHasMonth = "&T2_COUNT=2&";

            substation.Status = Model.DataStatus.Processing;
            substation.Clear();
            error = false;

            string[] wids = null;

            if (substation.Children == null || substation.Children.Count == 0)
                error = true;
            else
            {
                if (substation.Items == null)
                {
                    App.LogInfo(String.Format("Подстанция <{0}> не имеет точек.", substation.Name));
                    error = true;
                }
                else
                {                   
                    try
                    {
                        var items = substation.Items.Cast<Model.IEmcosPoint>();
                        string points = EmcosSiteWrapper.Instance.CreatePointsParam(items);
                        wids = EmcosSiteWrapper.Instance.GetArchiveWIds(
                            hasMonthData 
                              ? measurementsDaysPart + measurementsMonthPart + measurementsEndPartIfHasMonth
                              : measurementsDaysPart + measurementsEndPartIfNotHasMonth,
                            points, startDate, endDate, cts.Token);
                    }
                    catch (Exception e)
                    {
                        App.LogInfo("GetSubstationsDaylyArchives - ошибка: " + e.Message);
                        error = true;
                    }
                    if (wids == null || wids.Length == 0)
                    {
                        App.LogInfo("GetSubstationsDaylyArchives - ошибка получен неверный wid. Подстанция - " + substation.Name);
                        error = true;
                    }
                }
            }
            if (error == false)
            {
                var mls = EmcosSiteWrapper.Instance.GetParamsForArchiveData(wids[0]);

                #region | получение архивных данных по каждой из точек |

                total = substation.Items.Count;

                foreach (Model.Balans.IBalansItem item in substation.Items)
                {
                    current++;
                    if (callback!= null)
                        callback(current, total);
                    if (cts.IsCancellationRequested)
                    {
                        break;
                    }
                    item.Status = Model.DataStatus.Processing;
                    try
                    {
                        Func<Model.ML_Param, IEnumerable<Model.ArchData>> getPointArchive = (ml) =>
                        {
                            var point = new Model.ArchAP { Id = item.Id, Type = "POINT", Name = item.Name };
                            IEnumerable<Model.ArchData> list;
                            try
                            {
                                list = EmcosSiteWrapper.Instance.GetArchiveData(ml, point, startDate, endDate).Result;
                            }
                            catch (Exception e)
                            {
                                App.LogInfo(String.Format("GetSubstationsDaylyArchives-getPointArchive. Ошибка получения данных по точке {0}. Сообщение: {1}",
                                    item.Id, e.Message));
                                return null;
                            }
                            return list;
                        };

                        IEnumerable<Model.ArchData> data_days_minus, data_days_plus, data_month_minus = null, data_month_plus = null;
                        // А+ энергия  за  сутки
                        data_days_plus = getPointArchive(Model.MLPARAMS.A_PLUS_ENERGY_DAYS);
                        // А- энергия  за  сутки
                        data_days_minus = getPointArchive(Model.MLPARAMS.A_MINUS_ENERGY_DAYS);

                        if (hasMonthData)
                        {// А+ энергия  за  месяц
                            data_month_plus = getPointArchive(Model.MLPARAMS.A_PLUS_ENERGY_MONTH);
                            // А- энергия  за  месяц
                            data_month_minus = getPointArchive(Model.MLPARAMS.A_MINUS_ENERGY_MONTH);
                        }
                        // разбор данных
                        IList<double?> days_plus = new List<double?>();
                        IList<double?> days_minus = new List<double?>();
                        double? summ;

                        ParseData(out days_plus, out summ, ref data_days_plus, item.Name);
                        item.DailyEplus = days_plus;
                        item.DayEplusValue = summ;

                        ParseData(out days_minus, out summ, ref data_days_minus, item.Name);
                        item.DailyEminus = days_minus;
                        item.DayEminusValue = summ;

                        double? month_plus, month_minus;
                        if (hasMonthData)
                        {
                            ParseData(out month_plus, ref data_month_plus, item.Name);
                            item.MonthEplus = month_plus;

                            ParseData(out month_minus, ref data_month_minus, item.Name);
                            item.MonthEminus = month_minus;
                        }
                    }
                    catch (Exception e)
                    {
                        App.LogInfo(String.Format("GetSubstationsDaylyArchives. Ошибка получения данных по точке: ID={0}, NAME={1}. Сообщение: {2}",
                                    item.Id, item.Name, e.Message));
                        continue;
                    }
                    item.Status = Model.DataStatus.Processed;
                }

                #endregion | получение архивных данных по каждой из точек |
            }
            substation.Status = Model.DataStatus.Processed;
            return error == false;
        }

        public static void GetDaylyArchiveDataForItem(DateTime startDate, DateTime endDate, Model.Balans.IBalansItem item, System.Threading.CancellationTokenSource cts)
        {
            bool error = false;

            bool hasMonthData;
            var nextMonthOfEndDate = endDate.AddMonths(1);
            var nextMonthOfEndDate2 = new DateTime(nextMonthOfEndDate.Year, nextMonthOfEndDate.Month, 1);
            int lastDayInEndDateMonth = nextMonthOfEndDate2.AddDays(-1).Day;
            if (startDate.Day == 1 && endDate.Day == lastDayInEndDateMonth)
                hasMonthData = true;
            else
                hasMonthData = false;
            // измерения - А энергия за сутки
            var measurementsDaysPart = @"T2_TYPE_0=MSF&T2_NAME_0=А энергия&T2_MSF_ID_0=14&T2_MSF_NAME_0=А энергия&T2_AGGS_TYPE_ID_0=3&T2_MS_TYPE_ID_0=1";
            // А энергия за месяц
            var measurementsMonthPart = @"&T2_TYPE_1=MSF&T2_NAME_1=А энергия&T2_MSF_ID_1=14&T2_MSF_NAME_1=А энергия&T2_AGGS_TYPE_ID_1=4&T2_MS_TYPE_ID_1=1";
            // 
            var measurementsEndPartIfNotHasMonth = "&T2_COUNT=1&";
            var measurementsEndPartIfHasMonth = "&T2_COUNT=2&";

            item.Status = Model.DataStatus.Processing;
            error = false;

            string[] wids = null;

            try
            {
                var points = EmcosSiteWrapper.Instance.CreatePointsParam(new List<Model.Balans.IBalansItem> { item });
                wids = EmcosSiteWrapper.Instance.GetArchiveWIds(
                    hasMonthData
                      ? measurementsDaysPart + measurementsMonthPart + measurementsEndPartIfHasMonth
                      : measurementsDaysPart + measurementsEndPartIfNotHasMonth,
                    points, startDate, endDate, cts.Token);
            }
            catch (Exception e)
            {
                App.LogInfo("GetDaylyArchiveDataForItem - GetArchiveWIds - ошибка: " + e.Message);
                error = true;
            }
            if (wids == null || wids.Length == 0)
            {
                App.LogInfo("GetDaylyArchiveDataForItem - ошибка получен неверный wid. Элемент - " + item.Name);
                error = true;
            }

            if (error == false)
            {
                var mls = EmcosSiteWrapper.Instance.GetParamsForArchiveData(wids[0]);

                try
                {
                    Func<Model.ML_Param, IEnumerable<Model.ArchData>> getPointArchive = (ml) =>
                    {
                        var point = new Model.ArchAP { Id = item.Id, Type = "POINT", Name = item.Name };
                        IEnumerable<Model.ArchData> list;
                        try
                        {
                            list = EmcosSiteWrapper.Instance.GetArchiveData(ml, point, startDate, endDate).Result;
                        }
                        catch (Exception e)
                        {
                            App.LogInfo(String.Format("GetDaylyArchiveDataForItem-getPointArchive. Ошибка получения данных по точке {0}. Сообщение: {1}",
                                item.Id, e.Message));
                            return null;
                        }
                        return list;
                    };

                    IEnumerable<Model.ArchData> data_days_minus, data_days_plus, data_month_minus = null, data_month_plus = null;
                    // А+ энергия  за  сутки
                    data_days_plus = getPointArchive(Model.MLPARAMS.A_PLUS_ENERGY_DAYS);
                    // А- энергия  за  сутки
                    data_days_minus = getPointArchive(Model.MLPARAMS.A_MINUS_ENERGY_DAYS);

                    if (hasMonthData)
                    {// А+ энергия  за  месяц
                        data_month_plus = getPointArchive(Model.MLPARAMS.A_PLUS_ENERGY_MONTH);
                        // А- энергия  за  месяц
                        data_month_minus = getPointArchive(Model.MLPARAMS.A_MINUS_ENERGY_MONTH);
                    }
                    // разбор данных
                    IList<double?> days_plus = new List<double?>();
                    IList<double?> days_minus = new List<double?>();
                    double? summ;

                    ParseData(out days_plus, out summ, ref data_days_plus, item.Name);
                    item.DailyEplus = days_plus;
                    item.Eplus = summ;

                    ParseData(out days_minus, out summ, ref data_days_minus, item.Name);
                    item.DailyEminus = days_minus;
                    item.Eminus = summ;

                    double? month_plus, month_minus;
                    if (hasMonthData)
                    {
                        ParseData(out month_plus, ref data_month_plus, item.Name);
                        item.MonthEplus = month_plus;

                        ParseData(out month_minus, ref data_month_minus, item.Name);
                        item.MonthEminus = month_minus;
                    }
                }
                catch (Exception e)
                {
                    App.LogInfo(String.Format("GetDaylyArchiveDataForItem. Ошибка получения данных по точке: ID={0}, NAME={1}. Сообщение: {2}",
                                item.Id, item.Name, e.Message));
                }
            }

            item.Status = Model.DataStatus.Processed;
        }

        /////////
        /// <summary>
        /// Разбор архивных данных за сутки
        /// </summary>
        /// <param name="list">результат - список значений</param>
        /// <param name="summ">результат - сумма значений</param>
        /// <param name="values">архивные данные</param>
        /// <param name="name">имя точки</param>
        private static void ParseData(out IList<double?> list, out double? summ, ref IEnumerable<Model.ArchData> values, string name)
        {
            list = new List<double?>();
            summ = new Nullable<double>();
            if (values != null)
            {
                list = new List<double?>();
                int normalDataCount = 0;
                summ = 0.0;
                foreach (Model.ArchData d in values)
                    if (d.SFS == "Нормальные данные" || d.SFS.Contains("Ручной ввод"))
                    {
                        double value = 0;
                        Double.TryParse(d.D, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
                        list.Add(value);
                        normalDataCount++;
                        summ += value;
                    }
                    else
                    {
                        list.Add(new Nullable<double>());
                        normalDataCount++;
                    }
            }
            else
                App.LogInfo(String.Format("ParseData данные за сутки. Ошибка в данных. Точка: {0}", name));
        }
        /// <summary>
        /// Разбор архивных данных за месяц
        /// </summary>
        /// <param name="result">результат</param>
        /// <param name="values">архивные данные</param>
        /// <param name="name">имя точки</param>
        private static void ParseData(out double? result, ref IEnumerable<Model.ArchData> values, string name)
        {
            result = new Nullable<double>();
            if (values != null && values.Count() == 1)
            {
                var d = values.FirstOrDefault();
                if (d.SFS == "Нормальные данные" || d.SFS.Contains("Ручной ввод"))
                {
                    double value = 0;
                    Double.TryParse(d.D, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
                    result = value;
                }
            }
            else
                App.LogInfo(String.Format("ParseData данные за месяц. Ошибка в данных. Точка: {0}", name));
        }
    }
}
