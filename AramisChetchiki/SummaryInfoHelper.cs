using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMP.WORK.AramisChetchiki.Model;

namespace TMP.WORK.AramisChetchiki
{
    public static class SummaryInfoHelper
    {
        private const string ANOTHER_STRING = "(остальное)";
        private const string EMPTY_STRING = "(пусто)";
        private const string COUNT_STRING_FORMAT = "{0} ({1:N0})";
        private const string FIRST_10_STRING = "первые 10 по убыванию количества счетчиков: ";


        public static IEnumerable<SummaryInfoGroupItem> BuildGroups(ICollection<Meter> meters, string field)
        {
            var values = meters
                // группируем список по значению текущего свойства
                .GroupBy(i => ModelHelper.MeterGetPropertyValue(i, field));

            return values
                // создание типа: значение, список счетчиков с этим значением, количество
                .Select(group => new SummaryInfoGroupItem
                {
                    Key = group.Key == null || String.IsNullOrWhiteSpace(group.Key.ToString()) ? EMPTY_STRING : group.Key,
                    HasEmptyValue = group.Key == null || String.IsNullOrWhiteSpace(group.Key.ToString()),
                    Value = group.ToList(),
                    Count = group.Count()
                })
                // сортировка по убыванию количества значений
                .OrderByDescending(o => o.Count)
                .ToList();
        }

        public static IEnumerable<SummaryInfoGroupItem> BuildFirst10LargeGroups(ICollection<Meter> meters, string field)
        {
            var values = meters
                // группируем список по значению текущего свойства
                .GroupBy(i => ModelHelper.MeterGetPropertyValue(i, field));

            IEnumerable<SummaryInfoGroupItem> data = values
                // создание типа: значение, список счетчиков с этим значением, количество
                .Select(group => new SummaryInfoGroupItem
                {
                    Key = group.Key == null || String.IsNullOrWhiteSpace(group.Key.ToString()) ? EMPTY_STRING : group.Key,
                    HasEmptyValue = group.Key == null || String.IsNullOrWhiteSpace(group.Key.ToString()),
                    Value = group.ToList(),
                    Count = group.Count()
                })
                // сортировка по убыванию количества значений
                .OrderByDescending(o => o.Count)
                .ToList();

            // количество уникальных значений свойства
            int groupsCount = data.Count();

            // количество счетчиков
            int totalCount = data.Sum(i => i.Count);

            if (groupsCount > 10)
            {
                var first = data.Take(9);
                foreach (var item in first)
                    item.Percent = 100 * item.Count / totalCount;

                var last = data.Skip(9);
                List<Meter> otherMeters = new List<Meter>();
                foreach (var item in last)
                    otherMeters.AddRange(item.Value);

                List<SummaryInfoGroupItem> list = new List<SummaryInfoGroupItem>();
                list.AddRange(first);
                var other = new SummaryInfoGroupItem()
                {
                    Key = ANOTHER_STRING,
                    HasEmptyValue = false,
                    Value = otherMeters,
                    Count = otherMeters.Count,
                    Percent = 100 * otherMeters.Count / totalCount
                };
                list.Add(other);
                return list;
            }
            else
                return data;
        }


        public static SummaryInfoItem BuildSummaryInfoItem(ICollection<Meter> meters, string field)
        {
            string fieldDisplayName = ModelHelper.MeterPropertyDisplayNames[field];

            SummaryInfoItem si = new SummaryInfoItem(fieldDisplayName);
            si.FieldName = field;

            var data = BuildGroups(meters, field);

            // количество уникальных значений свойства
            int groupsCount = data.Count();
            string groupsInfo = String.Empty;
            if (groupsCount > 10)
            {
                // склоняем 'группа' согласно количества
                int mod = groupsCount % 100;
                if (mod == 0)
                    groupsInfo += "не имеется групп.";
                else
                if ((mod == 1 || (mod % 10) == 1) && mod != 11)
                    groupsInfo += "имеется " + groupsCount.ToString() + " группа:";
                else
                if ((mod >= 2 & mod <= 4 || (mod % 10 >= 2 & mod % 10 <= 4)) && mod / 10 != 1)
                    groupsInfo += "имеется " + groupsCount.ToString() + " группы" + (groupsCount <= 10 ? ":" : ", " + FIRST_10_STRING);
                else
                    groupsInfo += "имеется " + groupsCount.ToString() + " групп" + (groupsCount <= 10 ? ":" : ", " + FIRST_10_STRING);
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

            Func<SummaryInfoGroupItem, SummaryInfoChildItem> createInfoChild = g =>
            {
                return new SummaryInfoChildItem
                {
                        // значение поля - количество счетчиков со значением поля
                        Header = String.Format(COUNT_STRING_FORMAT, g.Key, g.Count),
                        // значение - количество счётчиков
                        Count = (uint)g.Count,
                        // процент от общего количества в группе
                        Percent = 100d * g.Count / totalCount,
                        // значение группируемого поля
                        Value = g.Key.ToString(),
                        IsEmpty = g.HasEmptyValue
                };
            };

            si.AllItems = data.Select(group => createInfoChild(group)).ToList();

            si.OnlyFirst10Items = new List<SummaryInfoChildItem>();

            // создание списка с пустыми значениями
            IEnumerable<SummaryInfoChildItem> withEmptyValue = data.Where(g => g.HasEmptyValue).Select(group => createInfoChild(group));
            int withEmptyValueCount = data.Where(g => g.HasEmptyValue).Sum(g => g.Value.Count);

            Action<string, uint, double, string> addOnlyFirst10ItemsChild = (header, count, percent, value) =>
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
                          Value = value
                  });
              };

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
                si.OnlyFirst10Items.Add(item);

            if (withEmptyValueCount > 0)
                addOnlyFirst10ItemsChild(
                String.Format(COUNT_STRING_FORMAT, EMPTY_STRING, withEmptyValueCount),
                (uint)withEmptyValueCount,
                100d * withEmptyValueCount / totalCount,
                string.Empty);

            if (lastCount > 0)
                addOnlyFirst10ItemsChild(
                    String.Format(COUNT_STRING_FORMAT, ANOTHER_STRING, lastCount),
                    (uint)lastCount,
                    100d * lastCount / totalCount,
                    ANOTHER_STRING);

            return si;
        }
    }
}
