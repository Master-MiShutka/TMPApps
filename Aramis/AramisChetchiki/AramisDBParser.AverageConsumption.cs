namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using TMP.WORK.AramisChetchiki.Model;
    using TMPApplication;

    internal partial class AramisDBParser
    {
        /// <summary>
        /// расчет среднемесячного потребления
        /// </summary>
        private void CalcAverageConsumptionByAbonents(AramisData aramisData)
        {
            if (aramisData.Payments == null || aramisData.MetersControlData == null || aramisData.Meters == null)
            {
                return;
            }

            Model.WorkTask workTask = new("расчет среднемесячного потребления по абонентам");
            this.workTasksProgressViewModel.WorkTasks.Add(workTask);
            workTask.StartProcessing();

            int totalRows = (aramisData.Meters as ICollection).Count;
            int processedRows = 0;

            DateOnly now = DateOnly.FromDateTime(DateTime.Now);

            try
            {
                Parallel.ForEach(aramisData.Meters, meter =>
                //foreach (var meter in aramisData.Meters)
                {
                    workTask.UpdateUI(++processedRows, totalRows, stepNameString: "лицевой счет");

                    if (meter.Лицевой == 0 || meter.Удалён)
                    {
                        return;
                    }

                    // контрольные показания
                    List<MeterControlData> controlDatas = new();
                    if (aramisData.MetersControlData.ContainsKey(meter.Лицевой) == true)
                    {
                        controlDatas = aramisData.MetersControlData[meter.Лицевой].FirstOrDefault().Data.OrderByDescending(i => i.Date).ToList();
                    }

                    // оплаты по лицевому счёту
                    List<Payment> payments = new();
                    if (aramisData.Payments.ContainsKey(meter.Лицевой) == true)
                    {
                        // изначально оплаты отсортированы по возрастанию периода оплаты
                        payments = aramisData.Payments[meter.Лицевой].Reverse().ToList();
                    }

                    // замены по лицевому счёту
                    List<ChangeOfMeter> changes = new();
                    if (aramisData.ChangesOfMeters.ContainsKey(meter.Лицевой) == true)
                    {
                        changes = aramisData.ChangesOfMeters[meter.Лицевой].OrderByDescending(i => i.ДатаЗамены).ToList();
                    }

                    // общий список для значений
                    List<Tuple<DateOnly, MeterEventType, object>> list = new List<Tuple<DateOnly, MeterEventType, object>>(payments.Count + changes.Count + controlDatas.Count);

                    // собираем список
                    foreach (var item in payments)
                    {
                        list.Add(new Tuple<DateOnly, MeterEventType, object>(item.ПериодОплаты, MeterEventType.Payment, item));
                    }

                    foreach (var item in changes)
                    {
                        list.Add(new Tuple<DateOnly, MeterEventType, object>(item.ДатаЗамены, MeterEventType.Change, item));
                    }

                    foreach (var item in controlDatas)
                    {
                        list.Add(new Tuple<DateOnly, MeterEventType, object>(item.Date, MeterEventType.Control, item));
                    }

                    // делегат сравнения по дате
                    Comparison<Tuple<DateOnly, MeterEventType, object>> comparison = (x, y) =>
                    {
                        return y.Item1.CompareTo(x.Item1);
                    };

                    // сортировка по дате - по убыванию
                    list.Sort(comparison);

                    if (list.Count != 0)
                    {

                        // построение списка событий
                        meter.Events = this.BuildMeterEvents(list);

                        // расчёт среднемесячного потребления за последний год
                        meter.СреднеМесячныйРасходПоОплате = this.CalcAverageConsumptionByPayments(list);
                        meter.СреднеМесячныйРасходПоКонтрольнымПоказаниям = this.CalcAverageConsumptionByControlReadings(list);
                    }
                //}
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

        private int CalculateConsumption(int meterPreviousReadings, int meterNextReadings, byte meterDigitsCount, Payment previousPayment)
        {
            int consumption = meterNextReadings - meterPreviousReadings;

            if (consumption < 0 && Math.Abs(consumption) > 1 && Math.Abs(consumption) <= 5)
            {
                return 0;
            }

            if (consumption < 0 && Math.Abs(consumption) > 1)
            {
                consumption = (meterNextReadings + (int)Math.Pow(10, meterDigitsCount)) - meterPreviousReadings;
            }

            if (consumption < 0 || consumption >= 50_000)
            {
                if (previousPayment != null && previousPayment.HasPayments)
                {
                    var readingsList = previousPayment.Payments.Where(i => i.ПоследнееПоказание == meterNextReadings).ToList();

                    if (readingsList.Any() && readingsList.Count == 1)
                    {
                        int reading = readingsList.First().ПредыдущееПоказание;
                        consumption = meterNextReadings - reading;

                        if (consumption > 0)
                            return consumption;
                    }
                }

                //System.Diagnostics.Debugger.Break();
                return 0;
            }

            return consumption;
        }

        private int CalculateMonthAverageConsumption(DateOnly startDate, DateOnly endDate, int consumption)
        {
            int days = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
            return days == 0 ? 0 : 30 * consumption / days;
        }

        private SortedSet<MeterEvent> BuildMeterEvents(List<Tuple<DateOnly, MeterEventType, object>> list)
        {
            SortedSet<MeterEvent> meterEvents = new SortedSet<MeterEvent>();

            // показания из первой записи в списке
            int prevReadings = 0;
            switch (list[^1].Item2)
            {
                case MeterEventType.None:
                    break;
                case MeterEventType.Control:
                    prevReadings = (list[^1].Item3 as MeterControlData).Value;
                    break;
                case MeterEventType.Change:
                    prevReadings = (list[^1].Item3 as ChangeOfMeter).ПоказаниеСнятого;
                    break;
                case MeterEventType.Payment:
                    prevReadings = (list[^1].Item3 as Payment).ПредыдущееПоказание;
                    break;
                default:
                    break;
            }

            // список замен
            var changes = list.Where(i => i.Item2 == MeterEventType.Change).Select(i => i.Item3).Cast<ChangeOfMeter>();

            // кол-во знаков на счетчике: выбор последней замены в списке - т.е. самая первая замена
            byte meterDigitsCount = changes.Any() ? (byte)changes.Last().РазрядностьУстановленного : (byte)0;

            Payment previousPayment = null;

            // проход по списку с конца - старые даты будут в начале
            for (int index = list.Count - 1; index >= 0; index--)
            {
                Tuple<DateOnly, MeterEventType, object> item = list[index];
                ChangeOfMeter change = item.Item3 as ChangeOfMeter;
                Payment payment = item.Item3 as Payment;
                MeterControlData control = item.Item3 as MeterControlData;

                switch (item.Item2)
                {
                    case MeterEventType.Change:
                        int consumption = this.CalculateConsumption(prevReadings, change.ПоказаниеСнятого, change.РазрядностьСнятого, previousPayment);

                        meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Change, consumption, change.ПоказаниеУстановленного));
                        prevReadings = change.ПоказаниеУстановленного;
                        meterDigitsCount = change.РазрядностьУстановленного;
                        break;
                    case MeterEventType.Payment:
                        meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Payment, payment.РазностьПоказаний, payment.ПоследнееПоказание));
                        prevReadings = payment.ПоследнееПоказание;
                        break;
                    case MeterEventType.Control:
                        meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Control, 0, control.Value));
                        prevReadings = control.Value;
                        break;
                    default:
                        break;
                }

                previousPayment = payment;
            }

            return meterEvents;
        }

        private int CalcAverageConsumptionByPayments(List<Tuple<DateOnly, MeterEventType, object>> list)
        {
            // конечная дата
            DateOnly endDate = list[0].Item1;

            // начальная дата
            DateOnly startDate = endDate;

            var payments = list.Where(i => i.Item2 == MeterEventType.Payment).Select(i => i.Item3).Cast<Payment>().ToList();
            int endValue = payments.Count > 0 ? payments[0].LastReading : -1;

            if (endValue == -1)
            {
                return -1;
            }

            // сумма расходов за период
            int summ = 0;

            DateOnly findDate = endDate.AddYears(-1);

            bool found = false;

            var changes = list.Where(i => i.Item2 == MeterEventType.Change).Select(i => i.Item3).Cast<ChangeOfMeter>();
            byte meterDigitsCount = changes.Any() ? (byte)changes.First().РазрядностьУстановленного : (byte)0;

            var paymentsAndChanges = list.Where(i => i.Item2 == MeterEventType.Payment || i.Item2 == MeterEventType.Change).ToList();

            int paymentIndex = 0;
            Payment previousPayment = null;

            // перебор всех оплат, ищем ближайшую дату оплаты, чтобы период был не меньше года
            for (int index = 0; index < paymentsAndChanges.Count; index++)
            {
                Tuple<DateOnly, MeterEventType, object> item = paymentsAndChanges[index];
                ChangeOfMeter change = item.Item3 as ChangeOfMeter;
                Payment payment = item.Item3 as Payment;
                MeterControlData control = item.Item3 as MeterControlData;

                previousPayment = (paymentIndex >= payments.Count) ? null : payments[paymentIndex];

                if (item.Item1 > findDate && found == false)
                {
                    // проверяем была ли замена счетчика
                    if (item.Item2 == MeterEventType.Change)
                    {
                        summ += this.CalculateConsumption(change.ПоказаниеУстановленного, endValue, meterDigitsCount, previousPayment);
                        meterDigitsCount = change.РазрядностьСнятого;
                        endValue = change.ПоказаниеСнятого;
                    }
                    else
                    {
                        if (item.Item2 == MeterEventType.Payment)
                        {
                            summ += this.CalculateConsumption(payment.ПоследнееПоказание, endValue, meterDigitsCount, previousPayment);
                            endValue = payment.ПредыдущееПоказание;
                            paymentIndex++;
                        }
                    }
                }
                else
                {
                    if (item.Item2 == MeterEventType.Change)
                    {
                        summ += this.CalculateConsumption(change.ПоказаниеУстановленного, endValue, meterDigitsCount, previousPayment);
                        meterDigitsCount = change.РазрядностьСнятого;
                        startDate = change.ДатаЗамены;
                    }
                    else
                    {
                        if (item.Item2 == MeterEventType.Payment)
                        {
                            summ += this.CalculateConsumption(payment.ПоследнееПоказание, endValue, meterDigitsCount, previousPayment);
                            startDate = payment.ПериодОплаты;
                        }
                    }

                    found = true;
                    break;
                }
            }

            return this.CalculateMonthAverageConsumption(startDate, endDate, summ);
        }

        private int CalcAverageConsumptionByControlReadings(List<Tuple<DateOnly, MeterEventType, object>> list)
        {
            // конечная дата
            DateOnly endDate = list[0].Item1;

            // начальная дата
            DateOnly startDate = endDate;

            var controlReadings = list.Where(i => i.Item2 == MeterEventType.Control).Select(i => i.Item3).Cast<MeterControlData>();
            int endValue = controlReadings.Any() ? controlReadings.First().LastReading : -1;

            if (endValue == -1)
            {
                return -1;
            }

            // сумма расходов за период
            int summ = 0;

            DateOnly findDate = endDate.AddYears(-1);

            bool found = false;

            var changes = list.Where(i => i.Item2 == MeterEventType.Change).Select(i => i.Item3).Cast<ChangeOfMeter>();
            byte meterDigitsCount = changes.Any() ? (byte)changes.First().РазрядностьУстановленного : (byte)0;

            var controlReadingsAndChanges = list.Where(i => i.Item2 == MeterEventType.Control || i.Item2 == MeterEventType.Change).ToList();

            for (int index = 0; index < controlReadingsAndChanges.Count; index++)
            {
                Tuple<DateOnly, MeterEventType, object> item = controlReadingsAndChanges[index];
                ChangeOfMeter change = item.Item3 as ChangeOfMeter;
                Payment payment = item.Item3 as Payment;
                MeterControlData control = item.Item3 as MeterControlData;

                if (item.Item1 > findDate && found == false)
                {
                    // проверяем была ли замена счетчика
                    if (item.Item2 == MeterEventType.Change)
                    {
                        summ += this.CalculateConsumption(change.ПоказаниеУстановленного, endValue, meterDigitsCount, null);
                        meterDigitsCount = change.РазрядностьСнятого;
                        endValue = change.ПоказаниеСнятого;
                    }
                    else
                    {
                        if (item.Item2 == MeterEventType.Control)
                        {
                            summ += this.CalculateConsumption(control.Value, endValue, meterDigitsCount, null);
                            endValue = control.Value;
                        }
                    }
                }
                else
                {
                    if (item.Item2 == MeterEventType.Change)
                    {
                        summ += this.CalculateConsumption(change.ПоказаниеУстановленного, endValue, meterDigitsCount, null);
                        meterDigitsCount = change.РазрядностьСнятого;
                        endValue = change.ПоказаниеСнятого;
                        startDate = item.Item1;
                        found = true;
                        break;
                    }
                    else
                    {
                        if (item.Item2 == MeterEventType.Control)
                        {
                            summ += this.CalculateConsumption(control.Value, endValue, meterDigitsCount, null);
                            startDate = item.Item1;
                            endValue = control.Value;
                            found = true;
                            break;
                        }
                    }
                }
            }

            return this.CalculateMonthAverageConsumption(startDate, endDate, summ);
        }
    }
}
