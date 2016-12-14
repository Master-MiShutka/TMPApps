using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace TMP.Work.AmperM.TestApp
{
    public static class Utils
    {
        public static T CloneJson<T>(this T source)
        {
            // не сериализовать null объект
            if (Object.ReferenceEquals(source, null))
                return default(T);
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
    }
}