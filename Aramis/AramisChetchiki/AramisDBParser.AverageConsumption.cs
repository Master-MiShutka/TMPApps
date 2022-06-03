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
        private static void SwapItems(int firstIndex, int secondIndex, List<Tuple<DateOnly, MeterEventType, object>> list)
        {
            (list[secondIndex], list[firstIndex]) = (list[firstIndex], list[secondIndex]);
        }

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

                //TODO: make Parallel
                var meters = aramisData.Meters.ToList();
                for (int index = 0; index < meters.Count; index++)
                //Parallel.ForEach(aramisData.Meters, meter =>
                {
                    workTask.UpdateUI(++processedRows, totalRows, stepNameString: "лицевой счет");

                    if (meters[index].Лицевой != 0 || meters[index].Удалён == false)
                    {
                        // контрольные показания
                        controlDatas.Clear();
                        if (aramisData.MetersControlData.ContainsKey(meters[index].Лицевой) == true)
                        {
                            controlDatas = aramisData.MetersControlData[meters[index].Лицевой].FirstOrDefault().Data.OrderByDescending(i => i.Date).ToList();
                        }

                        // оплаты по лицевому счёту
                        payments.Clear();
                        if (aramisData.Payments.ContainsKey(meters[index].Лицевой) == true)
                        {
                            // изначально оплаты отсортированы по возрастанию периода оплаты
                            payments = aramisData.Payments[meters[index].Лицевой].Reverse().ToList();
                        }

                        // замены по лицевому счёту
                        changes.Clear();
                        if (aramisData.ChangesOfMeters.ContainsKey(meters[index].Лицевой) == true)
                        {
                            changes = aramisData.ChangesOfMeters[meters[index].Лицевой].OrderByDescending(i => i.ДатаЗамены).ToList();
                        }

                        // построение списка и сортировка
                        this.BuildTemporaryListOfEvents(ref list, controlDatas, payments, changes);

                        if (list.Count != 0)
                        {
                            List<MeterEvent> meterEvents = this.BuildMeterEvents(ref list);

                            // построение списка событий
                            // пытаемся добавить, если не удалось, т.е. уже добавлен
                            if (events.TryAdd(meters[index].Лицевой, meterEvents) == false)
                            {
                                // если текущий счётчик не удален - удаляем ранее добавленный
                                if (meters[index].Удалён == false)
                                {
                                    events.Remove(meters[index].Лицевой, out IList<MeterEvent> removed);

                                    // попытка добавить текущий счётчик
                                    if (events.TryAdd(meters[index].Лицевой, meterEvents) == false)
                                    {
                                        if (System.Diagnostics.Debugger.IsAttached)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Can't add the events with personal ID = {meters[index].Лицевой}");
                                        }
                                        else
                                        {
                                            logger?.Warn($"Не удалось добавить события по лицевому счёту {meters[index].Лицевой}");
                                        }
                                    }
                                }
                            }

                            // расчёт среднемесячного потребления за последний год
                            meters[index].СреднеМесячныйРасходПоОплате = this.CalcAverageConsumptionByPayments(list);
                            meters[index].СреднеМесячныйРасходПоКонтрольнымПоказаниям = this.CalcAverageConsumptionByControlReadings(list);
                        }
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

        private void BuildTemporaryListOfEvents(ref List<Tuple<DateOnly, MeterEventType, object>> list, List<MeterControlData> controlDatas, List<Payment> payments, List<ChangeOfMeter> changes)
        {
            // общий список для значений
            list.Clear();

            // собираем список
            for (int index = 0; index < payments.Count; index++)
            {
                list.Add(new Tuple<DateOnly, MeterEventType, object>(payments[index].ПериодОплаты, MeterEventType.Payment, payments[index]));
            }

            for (int index = 0; index < changes.Count; index++)
            {
                list.Add(new Tuple<DateOnly, MeterEventType, object>(changes[index].ДатаЗамены, MeterEventType.Change, changes[index]));
            }

            for (int index = 0; index < controlDatas.Count; index++)
            {
                list.Add(new Tuple<DateOnly, MeterEventType, object>(controlDatas[index].Date, MeterEventType.Control, controlDatas[index]));
            }

            // делегат сравнения по дате
            Comparison<Tuple<DateOnly, MeterEventType, object>> comparison = (x, y) =>
            {
                return y.Item1.CompareTo(x.Item1);
            };

            // сортировка по дате - по убыванию
            list.Sort(comparison);

            // дополнительная сортировка - корректировка
            // проход по списку - от старым к новым
            for (int index = list.Count - 1; index > 0; index--)
            {
                Tuple<DateOnly, MeterEventType, object> current = list[index];
                Tuple<DateOnly, MeterEventType, object> next = list[index - 1];

                if (current.Item2 == MeterEventType.Payment && next.Item2 == MeterEventType.Change)
                {
                    ChangeOfMeter changeOfMeter = (ChangeOfMeter)next.Item3;
                    Payment payment = (Payment)current.Item3;

                    if (payment.Payments[0].ДатаОплаты > changeOfMeter.ДатаЗамены)
                        SwapItems(index, index - 1, list);
                }
                else
                if (current.Item1 == next.Item1)
                {
                    if (current.Item2 == MeterEventType.Control && next.Item2 == MeterEventType.Change)
                    {
                        ChangeOfMeter changeOfMeter = (ChangeOfMeter)next.Item3;
                        MeterControlData controlData = (MeterControlData)current.Item3;

                        int controlDigits = controlData.Value.ToString().Length;
                        int changePrevDigits = changeOfMeter.ПоказаниеСнятого.ToString().Length;
                        int changeNextDigits = changeOfMeter.LastReading.ToString().Length;

                        bool changed = false;

                        if (controlData.Value == changeOfMeter.LastReading)
                        {
                            SwapItems(index, index - 1, list);
                            changed = true;
                        }
                        else
                        {
                            if (controlDigits == changeNextDigits && controlData.Value > changeOfMeter.LastReading)
                            {
                                SwapItems(index, index - 1, list);
                                changed = true;
                            }

                            if (controlDigits == changePrevDigits && changeOfMeter.ПоказаниеСнятого > controlData.Value)
                            {
                                SwapItems(index, index - 1, list);
                                changed = true;
                            }
                        }

                        if (changed && index <= list.Count - 4 && index >= 4)
                        {
                            int someDatesCount = 0;
                            DateOnly date = list[index].Item1;
                            if (list[index - 1].Item1 == date)
                                someDatesCount++;
                            if (list[index - 2].Item1 == date)
                                someDatesCount++;
                            if (list[index - 3].Item1 == date)
                                someDatesCount++;
                            if (list[index - 4].Item1 == date)
                                someDatesCount++;

                            index -= someDatesCount;
                        }
                    }
                    else
                    {
                        if ((current.Item3 as IModelWithMeterLastReading).LastReading > (next.Item3 as IModelWithMeterLastReading).LastReading)
                        {
                            SwapItems(index, index - 1, list);
                        }
                    }
                }
            }
        }

        private List<MeterEvent> BuildMeterEvents(ref List<Tuple<DateOnly, MeterEventType, object>> list)
        {
            List<MeterEvent> meterEvents = new List<MeterEvent>();

            // показания из первой из конца записи в списке
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

                if (payment != null && prevReadings != payment.ПредыдущееПоказание)
                {
                    if (index - 1 > 0 && list[index - 1].Item3 is Payment payment1 && prevReadings == payment1.ПредыдущееПоказание)
                    {
                        SwapItems(index, index - 1, list);
                        item = list[index];
                        payment = item.Item3 as Payment;
                    }

                    if (index - 2 > 0 && list[index - 2].Item3 is Payment payment2 && prevReadings == payment2.ПредыдущееПоказание)
                    {
                        SwapItems(index, index - 2, list);
                        item = list[index];
                        payment = item.Item3 as Payment;
                    }

                    if (index - 3 > 0 && list[index - 3].Item3 is Payment payment3 && prevReadings == payment3.ПредыдущееПоказание)
                    {
                        SwapItems(index, index - 3, list);
                        item = list[index];
                        payment = item.Item3 as Payment;
                    }


                    if (index - 1 > 0 && list[index - 1].Item3 is ChangeOfMeter change1 && Math.Abs((long)change1.ПоказаниеСнятого - (long)prevReadings) <= 5_000)
                    {
                        SwapItems(index, index - 1, list);
                        item = list[index];
                        payment = item.Item3 as Payment;
                        change = item.Item3 as ChangeOfMeter;
                    }

                    if (index - 2 > 0 && list[index - 2].Item3 is ChangeOfMeter change2 && Math.Abs((long)change2.ПоказаниеСнятого - (long)prevReadings) <= 5_000)
                    {
                        SwapItems(index, index - 2, list);
                        item = list[index];
                        payment = item.Item3 as Payment;
                        change = item.Item3 as ChangeOfMeter;
                    }

                    if (index - 3 > 0 && list[index - 3].Item3 is ChangeOfMeter change3 && Math.Abs((long)change3.ПоказаниеСнятого - (long)prevReadings) <= 5_000)
                    {
                        SwapItems(index, index - 3, list);
                        item = list[index];
                        payment = item.Item3 as Payment;
                        change = item.Item3 as ChangeOfMeter;
                    }
                }

                switch (item.Item2)
                {
                    case MeterEventType.Change:
                        // если перенос недооплаченной энергии на показания установленного счетчика
                        if (prevReadings != change.ПоказаниеСнятого)
                        {
                            long diff = (long)change.ПоказаниеСнятого - (long)prevReadings;
                            int digits = change.РазрядностьУстановленного;

                            if (diff == 0)
                            {
                                prevReadings = change.LastReading;
                            }

                            // если недоплата
                            if (diff > 0)
                            {
                                // учет перехода через ноль
                                if ((long)change.LastReading - (long)diff < 0)
                                    prevReadings = (uint)(change.LastReading + (int)Math.Pow(10, digits)) - (uint)diff;
                                else
                                    prevReadings = change.LastReading - (uint)diff;
                            }
                            else
                            // если переплата
                            if (diff < 0)
                            {
                                prevReadings = change.LastReading + (uint)(-1 * diff);
                            }
                        }
                        else
                        {
                            prevReadings = change.LastReading;
                        }

                        meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Change, 0, change.LastReading));
                        meterDigitsCount = change.РазрядностьУстановленного;
                        break;
                    case MeterEventType.Payment:
                        //
                        meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Payment, payment.РазностьПоказаний, payment.ПоследнееПоказание));
                        prevReadings = payment.ПоследнееПоказание;
                        break;
                    case MeterEventType.Control:
                        meterEvents.Add(new MeterEvent(item.Item1, MeterEventType.Control, 0, control.Value));
                        break;
                    default:
                        break;
                }

                previousPayment = payment;
            }

            return meterEvents.OrderBy(i => i.Date).ToList();
        }

        private uint CalculateConsumption(uint meterPreviousReadings, uint meterNextReadings, byte meterDigitsCount, Payment previousPayment)
        {
            long consumption = (long)meterNextReadings - (long)meterPreviousReadings;

            if (consumption < 0 && Math.Abs(consumption) >= 1 && Math.Abs(consumption) <= 5)
            {
                return 0;
            }

            string strPrev = meterPreviousReadings.ToString();
            int digitsPrev = strPrev.Length;
            string strNext = meterNextReadings.ToString();
            int digitsNext = strNext.Length;

            if (strPrev.StartsWith("999"))
            {
                consumption = (meterNextReadings + (int)Math.Pow(10, digitsPrev)) - meterPreviousReadings;
            }

            if (strNext.StartsWith("999"))
            {
                consumption = (meterPreviousReadings + (int)Math.Pow(10, digitsNext)) - meterNextReadings;
            }

            if (consumption < 0 && Math.Abs(consumption) > 1)
            {
                consumption = (meterNextReadings + (int)Math.Pow(10, meterDigitsCount)) - meterPreviousReadings;
            }

            if (consumption < 0 || consumption >= 50_000)
            {
                if (digitsPrev != meterDigitsCount)
                {
                    consumption = (meterNextReadings + (int)Math.Pow(10, digitsPrev)) - meterPreviousReadings;

                    if (consumption >= 50_000)
                    {
                        consumption = this.CalculateConsumption(meterNextReadings, meterPreviousReadings, meterDigitsCount, previousPayment);

                        if (consumption >= 50_000)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
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

                consumption = Math.Abs((long)meterNextReadings - (long)meterPreviousReadings);

                if (consumption >= 5_000)
                {
                    System.Diagnostics.Debugger.Break();
                    return 0;
                }

                return (uint)consumption;
            }

            return (uint)consumption;
        }

        private uint CalculateMonthAverageConsumption(DateOnly startDate, DateOnly endDate, uint consumption)
        {
            uint days = (uint)(endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
            return days == 0 ? 0 : 30 * consumption / days;
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

            // сумма расходов за период
            uint summ = 0;

            DateOnly findDate = endDate.AddYears(-1);

            IEnumerable<ChangeOfMeter> changes = list.Where(i => i.Item2 == MeterEventType.Change).Select(i => i.Item3).Cast<ChangeOfMeter>();
            byte meterDigitsCount = changes.Any() ? changes.First().РазрядностьУстановленного : (byte)0;

            List<Tuple<DateOnly, MeterEventType, object>> paymentsAndChanges = list.Where(i => i.Item2 == MeterEventType.Payment || i.Item2 == MeterEventType.Change).ToList();

            // перебор всех оплат, ищем ближайшую дату оплаты, чтобы период был не меньше года
            for (int index = 0; index < paymentsAndChanges.Count; index++)
            {
                Tuple<DateOnly, MeterEventType, object> item = paymentsAndChanges[index];

                if (item.Item1 < findDate)
                {
                    break;
                }

                Payment payment = item.Item3 as Payment;

                if (item.Item2 == MeterEventType.Payment)
                {
                    summ += payment.РазностьПоказаний;
                    startDate = new DateOnly(payment.ПериодОплаты.Year, payment.ПериодОплаты.Month, 1);
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
                MeterControlData control = item.Item3 as MeterControlData;

                if (item.Item1 > findDate && found == false)
                {
                    // проверяем была ли замена счетчика
                    if (item.Item2 == MeterEventType.Change)
                    {
                        summ += this.CalculateConsumption(change.LastReading, endValue, meterDigitsCount, nullPayment);
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
                        summ += this.CalculateConsumption(change.LastReading, endValue, meterDigitsCount, nullPayment);
                    }
                    else if (item.Item2 == MeterEventType.Control)
                    {
                        summ += this.CalculateConsumption(control.Value, endValue, meterDigitsCount, nullPayment);
                    }

                    startDate = item.Item1;
                    found = true;
                    break;
                }
            }

            return this.CalculateMonthAverageConsumption(startDate, endDate, summ);
        }
    }
}
