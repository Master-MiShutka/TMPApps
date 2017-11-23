using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TMP.ARMTES.Model
{
    public class UnitedReportViewItem
    {
        [JsonProperty(PropertyName = "РЭС")]
        public string ResName { get; set; }
        [JsonProperty(PropertyName = "Город")]
        public string City { get; set; }
        [JsonProperty(PropertyName = "Улица")]
        public string Street { get; set; }
        [JsonProperty(PropertyName = "Дом")]
        public string House { get; set; }
        [JsonProperty(PropertyName = "Расчетные точки")]
        public int AccountingPointsCount { get; set; }
        [JsonProperty(PropertyName = "Все показания")]
        public int? AllIndications { get; set; }
        [JsonProperty(PropertyName = "Показания по ЛС")]
        public int? PersonalAccountIndications { get; set; }
        [JsonProperty(PropertyName = "Показания без ЛС")]
        public int? WithoutPersonalAccountIndications { get; set; }
        [JsonProperty(PropertyName = "Ошибки")]
        public int? TotalErrors { get; set; }
        [JsonProperty(PropertyName = "Ошибки по ЛС")]
        public int? ErrorsWithPersonalAccounts { get; set; }
        [JsonProperty("Баланс")]
        public string HousesOnBalance { get; set; }
    }
}
