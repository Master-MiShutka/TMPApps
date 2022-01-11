namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TMP.UI.Controls.WPF.Reporting.MatrixGrid;
    using TMP.WORK.AramisChetchiki.Model;

    public static class SummaryInfoHelper
    {
        private const string ANOTHERSTRING = "(остальное)";
        private const string EMPTYSTRING = "(пусто)";
        private const string COUNTSTRINGFORMAT = "{0} ({1:N0})";
        private const string FIRST10STRING = "первые 10 по убыванию количества счетчиков: ";

        public static IEnumerable<SummaryInfoGroupItem> BuildGroups(IEnumerable<Meter> meters, string field)
        {
            var values = meters

                // группируем список по значению текущего свойства
                .GroupBy(i => ModelHelper.MeterGetPropertyValue(i, field));

            return values

                // создание типа: значение, список счетчиков с этим значением, количество
                .Select(selector: group => new SummaryInfoGroupItem
                {
                    Key = group.Key == null || string.IsNullOrWhiteSpace(group.Key.ToString()) ? EMPTYSTRING : group.Key,
                    HasEmptyValue = group.Key == null || string.IsNullOrWhiteSpace(group.Key.ToString()),
                    Value = group.ToList(),
                    Count = group.Count(),
                })

                // сортировка по убыванию количества значений
                .OrderByDescending(o => o.Count)
                .ToList();
        }

        public static bool DefaultMeterFilterFunc(Meter meter)
        {
            return true;
        }

        public static IEnumerable<SummaryInfoGroupItem> BuildFirst10LargeGroups(IEnumerable<Meter> meters, string field, Func<Meter, bool> filterFunc, int defaultGroupsCount = 10)
        {
            var values = meters
                // применяем фильтр
                .Where(meter => filterFunc(meter))
                // группируем список по значению текущего свойства
                .GroupBy(i => ModelHelper.MeterGetPropertyValue(i, field));

            IEnumerable<SummaryInfoGroupItem> data = values

                // создание типа: значение, список счетчиков с этим значением, количество
                .Select(group => new SummaryInfoGroupItem
                {
                    Key = group.Key == null || string.IsNullOrWhiteSpace(group.Key.ToString()) ? EMPTYSTRING : group.Key,
                    HasEmptyValue = group.Key == null || string.IsNullOrWhiteSpace(group.Key.ToString()),
                    Value = group.ToList(),
                    Count = group.Count(),
                })

                // сортировка по убыванию количества значений
                .OrderByDescending(o => o.Count)
                .ToList();

            // количество уникальных значений свойства
            int groupsCount = data.Count();

            // количество счетчиков
            int totalCount = data.Sum(i => i.Count);

            if (groupsCount > defaultGroupsCount)
            {
                var first = data.Take(defaultGroupsCount - 1);
                foreach (var item in first)
                {
                    item.Percent = 100 * item.Count / totalCount;
                }

                var last = data.Skip(defaultGroupsCount - 1);
                List<Meter> otherMeters = new ();
                foreach (var item in last)
                {
                    otherMeters.AddRange(item.Value);
                }

                List<SummaryInfoGroupItem> list = new ();
                list.AddRange(first);
                var other = new SummaryInfoGroupItem()
                {
                    Key = ANOTHERSTRING,
                    HasEmptyValue = false,
                    Value = otherMeters,
                    Count = otherMeters.Count,
                    Percent = 100 * otherMeters.Count / totalCount,
                };
                list.Add(other);
                return list;
            }
            else
            {
                return data;
            }
        }

        public static SummaryInfoItem BuildSummaryInfoItem(IEnumerable<Meter> meters, string field)
        {
            if (ModelHelper.MeterPropertyDisplayNames.ContainsKey(field) == false)
            {
                return null;
            }

            string fieldDisplayName = ModelHelper.MeterPropertyDisplayNames[field];

            SummaryInfoItem si = new (fieldDisplayName);
            si.FieldName = field;

            var data = BuildGroups(meters, field);

            // количество уникальных значений свойства
            int groupsCount = data.Count();
            string groupsInfo = string.Empty;
            if (groupsCount > 10)
            {
                // склоняем 'группа' согласно количества
                int mod = groupsCount % 100;
                if (mod == 0)
                {
                    groupsInfo += "не имеется групп.";
                }
                else
                if ((mod == 1 || (mod % 10) == 1) && mod != 11)
                {
                    groupsInfo += "имеется " + groupsCount.ToString(AppSettings.CurrentCulture) + " группа:";
                }
                else
                if ((mod >= 2 & mod <= 4 || (mod % 10 >= 2 & mod % 10 <= 4)) && mod / 10 != 1)
                {
                    groupsInfo += "имеется " + groupsCount.ToString(AppSettings.CurrentCulture) + " группы" + (groupsCount <= 10 ? ":" : ", " + FIRST10STRING);
                }
                else
                {
                    groupsInfo += "имеется " + groupsCount.ToString(AppSettings.CurrentCulture) + " групп" + (groupsCount <= 10 ? ":" : ", " + FIRST10STRING);
                }
            }

            if (groupsCount == 0)
            {
                groupsInfo += "не имеется групп.";
                return si;
            }

            // описание сводной информации
            si.Info = groupsInfo;

            // количество счетчиков
            int totalCount = data.Sum(i => i.Count);

            SummaryInfoChildItem createInfoChild(SummaryInfoGroupItem g)
            {
                return new SummaryInfoChildItem
                {
                    // значение поля - количество счетчиков со значением поля
                    Header = string.Format(AppSettings.CurrentCulture, COUNTSTRINGFORMAT, g.Key, g.Count),

                    // значение - количество счётчиков
                    Count = (uint)g.Count,

                    // процент от общего количества в группе
                    Percent = 100d * g.Count / totalCount,

                    // значение группируемого поля
                    Value = g.Key.ToString(),
                    IsEmpty = g.HasEmptyValue,
                };
            }

            si.AllItems = data.Select(group => createInfoChild(group)).ToList();

            si.OnlyFirst10Items = new List<SummaryInfoChildItem>();

            // создание списка с пустыми значениями
            IEnumerable<SummaryInfoChildItem> withEmptyValue = data.Where(g => g.HasEmptyValue).Select(group => createInfoChild(group));
            int withEmptyValueCount = data.Where(g => g.HasEmptyValue).Sum(g => g.Value.Count);

            void addOnlyFirst10ItemsChild(string header, uint count, double percent, string value)
            {
                si.OnlyFirst10Items.Add(new SummaryInfoChildItem()
                {
                    // значение поля - количество счетчиков со значением поля
                    Header = header,

                    // значение - количество счётчиков
                    Count = count,

                    // процент от общего количества в группе
                    Percent = percent,

                    // значение группируемого поля
                    Value = value,
                });
            }

            // создание списка с не пустыми значениями
            var dataWhereHasValues = data.Where(g => g.HasEmptyValue == false);
            IEnumerable<SummaryInfoChildItem> first;
            int lastCount = 0;

            if (groupsCount <= 10)
            {
                first = dataWhereHasValues.Select(group => createInfoChild(group));
            }
            else
            {
                first = dataWhereHasValues.Take(10).Select(group => createInfoChild(group));

                IEnumerable<SummaryInfoChildItem> last = dataWhereHasValues.Skip(10)
                    .Select(group => createInfoChild(group));
                lastCount = dataWhereHasValues.Skip(10).Sum(g => g.Value.Count);
            }

            foreach (var item in first)
            {
                si.OnlyFirst10Items.Add(item);
            }

            if (withEmptyValueCount > 0)
            {
                addOnlyFirst10ItemsChild(
                    string.Format(AppSettings.CurrentCulture, COUNTSTRINGFORMAT, EMPTYSTRING, withEmptyValueCount),
                    (uint)withEmptyValueCount,
                    100d * withEmptyValueCount / totalCount,
                    string.Empty);
            }

            if (lastCount > 0)
            {
                addOnlyFirst10ItemsChild(
                    string.Format(AppSettings.CurrentCulture, COUNTSTRINGFORMAT, ANOTHERSTRING, lastCount),
                    (uint)lastCount,
                    100d * lastCount / totalCount,
                    ANOTHERSTRING);
            }

            return si;
        }

        public static IEnumerable<IMatrix> GetEnergyPowerSuppyPivots(AramisData aramisData, DateOnly period)
        {
            if (aramisData == null)
            {
                return null;
            }

            List<IMatrix> pivots = new ();

            var data = aramisData.ElectricitySupplyInfo.Where(i => i.Период == period).ToList();
            var total = data.Sum(i => i.Полезный_отпуск);

            IEnumerable<IMatrixHeader> headerCells2 = new IMatrixHeader[]
            {
                    MatrixHeaderCell.CreateColumnHeader("Значение, кВт∙ч"),
                    MatrixHeaderCell.CreateColumnHeader("Доля, %"),
            };

            string notDefinedString = "(не указана)";

            var substationsPowerSuppy = data
                .GroupBy(i => i.Подстанция)
                .Select(i => new KeyValuePair<string, int>(string.IsNullOrWhiteSpace(i.Key) ? notDefinedString : i.Key, i.Sum(o => o.Полезный_отпуск)))
                .OrderByDescending(i => i.Value)
                .ToList();

            var substationsNames = substationsPowerSuppy
                .Select(i => i.Key)
                .ToList();

            pivots.Add(new Matrix()
            {
                Header = $"Полезный отпуск электроэнергии бытовым абонентам по подстанциям (за {period:MMMM yyyy} г.)",
                Description = " ",
                GetRowHeaderValuesFunc = () => substationsNames.Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                ShowColumnsTotal = true,
                GetColumnHeaderValuesFunc = () => headerCells2,
                GetDataCellFunc = (row, column) =>
                {
                    var value = substationsPowerSuppy
                        .Where(i => i.Key == row.Header)
                        .Select(i => i.Value)
                        .FirstOrDefault();
                    if (column.Header == "Доля, %")
                    {
                        return new MatrixDataCell(100d * value / total, "N1");
                    }
                    else
                    {
                        return new MatrixDataCell(value);
                    }
                },
            });

            string otherString = "(остальное)";

            var data1 = data
                .GroupBy(i => i.Населённый_пункт)
                .Select(i => new KeyValuePair<string, int>(i.Key, i.Sum(o => o.Полезный_отпуск)))
                .OrderByDescending(i => i.Value)
                .ToList();
            List<KeyValuePair<string, int>> energyPowerSupplyByStateList = new ();
            List<string> stateNames = new ();
            if (data1.Count > 15)
            {
                int lastCount = data1
                    .Skip(15)
                    .Count();
                otherString += $" {lastCount} шт.";

                var summOfLast = data1
                    .Skip(15)
                    .Sum(o => o.Value);

                var first = data1
                    .Take(15)
                    .ToList();

                energyPowerSupplyByStateList.AddRange(first);
                energyPowerSupplyByStateList.Add(new KeyValuePair<string, int>(otherString, summOfLast));

                var firstStateNames = data1
                    .Take(15)
                    .Select(i => i.Key)
                    .ToList();

                stateNames.AddRange(firstStateNames);
                stateNames.Add(otherString);
            }
            else
            {
                energyPowerSupplyByStateList = data1;
                stateNames = data1
                    .Select(i => i.Key)
                    .ToList();
            }

            pivots.Add(new Matrix()
            {
                Header = $"Полезный отпуск электроэнергии бытовым абонентам по населенным пунктам (за {period:MMMM yyyy} г.)",
                Description = "показаны 15 наиболее крупных",
                GetRowHeaderValuesFunc = () => stateNames.Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                ShowColumnsTotal = true,
                GetColumnHeaderValuesFunc = () => headerCells2,
                GetDataCellFunc = (row, column) =>
                {
                    var value = energyPowerSupplyByStateList
                        .Where(i => i.Key == row.Header)
                        .Select(i => i.Value)
                        .FirstOrDefault();
                    if (column.Header == "Доля, %")
                    {
                        return new MatrixDataCell(100d * value / total, "N1");
                    }
                    else
                    {
                        return new MatrixDataCell(value);
                    }
                },
            });

            var data2 = data
                .Where(i => i.Полезный_отпуск > 0)
                .ToList();

            var data3 = data2
                .GroupBy(i => i.Тип_населённого_пункта)
                .ToList();

            var data4 = data3
                .ToDictionary(i => i.Key, k => k.Count());

            Dictionary<string, Dictionary<string, int>> keyValuePairs = new ()
            {
                { "до 50 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск < 50)) },
                { "от 50 до 100 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск is >= 50 and < 100)) },
                { "от 100 до 150 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск is >= 100 and < 150)) },
                { "ровно 150 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск == 150)) },
                { "от 150 до 170 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск is > 150 and < 170)) },
                { "от 170 до 200 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск is >= 170 and < 200)) },
                { "от 200 до 300 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск is >= 200 and < 300)) },
                { "от 300 до 500 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск is >= 300 and < 500)) },
                { "от 500 до 1000 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск is >= 500 and < 1000)) },
                { "от 1000 до 3000 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск is >= 1000 and < 3000)) },
                { "более 3000 кВт∙ч", data3.ToDictionary(i => i.Key, k => k.Count(l => l.Полезный_отпуск >= 3000)) },
            };

            IList<string> headerCells3 = new List<string>()
            {
                    "Количество, шт", "Доля, %",
            };

            IList<string> headerCells4 = data2
                .Select(i => i.Тип_населённого_пункта)
                .Distinct()
                .ToList();

            pivots.Add(new Matrix()
            {
                Header = $"Распределение количества оплаченной электроэнергии бытовыми абонентами (за {period:MMMM yyyy} г.)",
                Description = " ",
                GetRowHeaderValuesFunc = () => keyValuePairs.Keys.Select(i => MatrixHeaderCell.CreateRowHeader(i)),
                ShowColumnsTotal = true,
                GetColumnHeaderValuesFunc = () => headerCells4.Select(i => MatrixHeaderCell.CreateColumnHeader(i, children: headerCells3.Select(k => MatrixHeaderCell.CreateColumnHeader(k)).ToList())).ToList(),
                GetDataCellFunc = (row, column) =>
                {
                    var value = keyValuePairs[row.Header][column.Parent.Header];
                    if (column.Header == "Доля, %")
                    {
                        return new MatrixDataCell(100d * value / data4[column.Parent.Header], "N1");
                    }
                    else
                    {
                        return new MatrixDataCell(value);
                    }
                },
            });

            return pivots;
        }
    }
}
