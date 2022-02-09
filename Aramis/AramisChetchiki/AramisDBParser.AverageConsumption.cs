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
                // результат
                IDictionary<ulong, IList<MeterEvent>> events = new Dictionary<ulong, IList<MeterEvent>>(); //System.Collections.Concurrent.ConcurrentDictionary<ulong, IList<MeterEvent>>();

                // контрольные показания
                List<MeterControlData> controlDatas = new();

                // оплаты по лицевому счёту
                List<Payment> payments = new();

                // замены по лицевому счёту
                List<ChangeOfMeter> changes = new();

                // общий список для значений
                List<Tuple<DateOnly, MeterEventType, object>> list = new List<Tuple<DateOnly, MeterEventType, object>>(payments.Count + changes.Count + controlDatas.Count);

                //Parallel.ForEach(aramisData.Meters, meter =>

                foreach (Meter meter in aramisData.Meters)
                {
                    workTask.UpdateUI(++processedRows, totalRows, stepNameString: "лицевой счет");

                    if (meter.Лицевой == 0 || meter.Удалён)
                    {
                        continue;// return;
                    }

                    // контрольные показания
                    controlDatas.Clear();
                    if (aramisData.MetersControlData.ContainsKey(meter.Лицевой) == true)
                    {
                        controlDatas = aramisData.MetersControlData[meter.Лицевой].FirstOrDefault().Data.OrderByDescending(i => i.Date).ToList();
                    }

                    // оплаты по лицевому счёту
                    payments.Clear();
                    if (aramisData.Payments.ContainsKey(meter.Лицевой) == true)
                    {
                        // изначально оплаты отсортированы по возрастанию периода оплаты
                        payments = aramisData.Payments[meter.Лицевой].Reverse().ToList();
                    }

                    // замены по лицевому счёту
                    changes.Clear();
                    if (aramisData.ChangesOfMeters.ContainsKey(meter.Лицевой) == true)
                    {
                        changes = aramisData.ChangesOfMeters[meter.Лицевой].OrderByDescending(i => i.ДатаЗамены).ToList();
                    }

                    // общий список для значений
                    list.Clear();

                    // собираем список
                    foreach (Payment item in payments)
                    {
                        list.Add(new Tuple<DateOnly, MeterEventType, object>(item.ПериодОплаты, MeterEventType.Payment, item));
                    }

                    foreach (ChangeOfMeter item in changes)
                    {
                        list.Add(new Tuple<DateOnly, MeterEventType, object>(item.ДатаЗамены, MeterEventType.Change, item));
                    }

                    foreach (MeterControlData item in controlDatas)
                    {
                        list.Add(new Tuple<DateOnly, MeterEventType, object>(item.Date, MeterEventType.Control, item));
                    }

                    // делегат сравнения по дате
                    Comparison<Tuple<DateOnly, MeterEventType, object>> comparison = (x, y) =>
                    {
                        if (x.Item1 == y.Item1)
                        {
                            return (y.Item3 as IModelWithMeterLastReading).LastReading.CompareTo((x.Item3 as IModelWithMeterLastReading).LastReading);
                        }
                        else
                        {
                            return y.Item1.CompareTo(x.Item1);
                        }
                    };

                    // сортировка по дате - по убыванию
                    list.Sort(comparison);

                    if (list.Count != 0)
                    {
                        List<MeterEvent> meterEvents = this.BuildMeterEvents(list);

                        // построение списка событий
                        // пытаемся добавить, если не удалось, т.е. уже добавлен
                        if (events.TryAdd(meter.Лицевой, meterEvents) == false)
                        {
                            // если текущий счётчик не удален - удаляем ранее добавленный
                            if (meter.Удалён == false)
                            {
                                events.Remove(meter.Лицевой, out IList<MeterEvent> removed);

                                // попытка добавить текущий счётчик
                                if (events.TryAdd(meter.Лицевой, meterEvents) == false)
                                {
                                    if (System.Diagnostics.Debugger.IsAttached)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Can't add the events with personal ID = {meter.Лицевой}");
                                    }
                                    else
                                    {
                                        logger?.Warn($"Не удалось добавить события по лицевому счёту {meter.Лицевой}");
                                    }
                                }
                            }
                        }

                        // расчёт среднемесячного потребления за последний год
                        meter.СреднеМесячныйРасходПоОплате = this.CalcAverageConsumptionByPayments(list);
                        meter.СреднеМесячныйРасходПоКонтрольнымПоказаниям = this.CalcAverageConsumptionByControlReadings(list);
                    }

                    //});
                }

                aramisData.Events = events.ToDictionary(i => i.Key, j => j.Value);
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

        private uint CalculateConsumption(uint meterPreviousReadings, uint meterNextReadings, byte meterDigitsCount, Payment previousPayment)
        {
            long consumption = (long)meterNextReadings - (long)meterPreviousReadings;

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
                int digits = meterPreviousReadings.ToString().Length;

                if (digits != meterDigitsCount)
                {
                    consumption = (meterNextReadings + (int)Math.Pow(10, digits)) - meterPreviousReadings;

                    if (consumption >= 50_000)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    return (uint)consumption;
                }
                else
                {
                    if (previousPayment != null && previousPayment.HasPayments)
                    {
                        List<PaymentData> readingsList = previousPayment.Payments.Where(i => i.ПоследнееПоказание == meterNextReadings).ToList();

                        if (readingsList.Any() && readingsList.Count == 1)
                        {
                            uint reading = readingsList.First().ПредыдущееПоказание;
                            consumption = meterNextReadings - reading;

                            if (consumption > 0)
                            {
                                return (uint)consumption;
                            }
                        }
                    }
                }

                System.Diagnostics.Debugger.Break();
                return 0;
            }

            return (uint)consumption;
        }

        private uint CalculateMonthAverageConsumption(DateOnly startDate, DateOnly endDate, uint consumption)
        {
            ushort days = (ushort)(endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
            return days == 0 ? 0 : 30 * consumption / days;
        }

        private List<MeterEvent> BuildMeterEvents(List<Tuple<DateOnly, MeterEventType, object>> list)
        {
            List<MeterEvent> meterEvents = new List<MeterEvent>();

            // показания из первой записи в списке
            uint prevReadings = 0;
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
            IEnumerable<ChangeOfMeter> changes = list.Where(i => i.Item2 == MeterEventType.Change).Select(i => i.Item3).Cast<ChangeOfMeter>();

            // кол-во знаков на счетчике: выбор последней замены в списке - т.е. самая первая замена
            byte meterDigitsCount = changes.Any() ? changes.Last().РазрядностьУстановленного : (byte)0;

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
                        uint consumption = this.CalculateConsumption(prevReadings, change.ПоказаниеСнятого, change.РазрядностьСнятого, previousPayment);

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

            return meterEvents.OrderBy(i => i.Date).ToList();
        }

        private uint CalcAverageConsumptionByPayments(List<Tuple<DateOnly, MeterEventType, object>> list)
        {
            // конечная дата
            DateOnly endDate = list[0].Item1;

            // начальная дата
            DateOnly startDate = endDate;

            List<Payment> payments = list.Where(i => i.Item2 == MeterEventType.Payment).Select(i => i.Item3).Cast<Payment>().ToList();

            if (payments.Count == 0)
            {
                return 0;
            }

            uint endValue = payments[0].LastReading;

            // сумма расходов за период
            uint summ = 0;

            DateOnly findDate = endDate.AddYears(-1);

            bool found = false;

            IEnumerable<ChangeOfMeter> changes = list.Where(i => i.Item2 == MeterEventType.Change).Select(i => i.Item3).Cast<ChangeOfMeter>();
            byte meterDigitsCount = changes.Any() ? changes.First().РазрядностьУстановленного : (byte)0;

            List<Tuple<DateOnly, MeterEventType, object>> paymentsAndChanges = list.Where(i => i.Item2 == MeterEventType.Payment || i.Item2 == MeterEventType.Change).ToList();

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

        private uint CalcAverageConsumptionByControlReadings(List<Tuple<DateOnly, MeterEventType, object>> list)
        {
            // конечная дата
            DateOnly endDate = list[0].Item1;

            // начальная дата
            DateOnly startDate = endDate;

            IEnumerable<MeterControlData> controlReadings = list.Where(i => i.Item2 == MeterEventType.Control).Select(i => i.Item3).Cast<MeterControlData>();
            if (controlReadings.Any() == false)
            {
                return 0;
            }

            uint endValue = controlReadings.First().LastReading;

            // сумма расходов за период
            uint summ = 0;

            DateOnly findDate = endDate.AddYears(-1);

            bool found = false;

            IEnumerable<ChangeOfMeter> changes = list.Where(i => i.Item2 == MeterEventType.Change).Select(i => i.Item3).Cast<ChangeOfMeter>();
            byte meterDigitsCount = changes.Any() ? changes.First().РазрядностьУстановленного : (byte)0;

            List<Tuple<DateOnly, MeterEventType, object>> controlReadingsAndChanges = list.Where(i => i.Item2 == MeterEventType.Control || i.Item2 == MeterEventType.Change).ToList();

            Payment nullPayment = null;

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
                        summ += this.CalculateConsumption(change.ПоказаниеУстановленного, endValue, meterDigitsCount, nullPayment);
                        meterDigitsCount = change.РазрядностьСнятого;
                        endValue = change.ПоказаниеСнятого;
                    }
                    else
                    {
                        if (item.Item2 == MeterEventType.Control)
                        {
                            summ += this.CalculateConsumption(control.Value, endValue, meterDigitsCount, nullPayment);
                            endValue = control.Value;
                        }
                    }
                }
                else
                {
                    if (item.Item2 == MeterEventType.Change)
                    {
                        summ += this.CalculateConsumption(change.ПоказаниеУстановленного, endValue, meterDigitsCount, nullPayment);
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
                            summ += this.CalculateConsumption(control.Value, endValue, meterDigitsCount, nullPayment);
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
