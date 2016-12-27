using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TMP.Work.AmperM.TestApp.EzSbyt
{
    using Shared;
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum EzSbytRequestFunctionType
    {
        [Description("{sql} Выполнение SQL запроса к базе данных филиала[ов]")]
        sql,
        [Description("{getobj} Получение данных по объекту (записи) выбранной таблицы базы данных")]
        getobj,
        [Description("{getpoint} Получение данных расчетной точки по выбранному идентификатору расчетной точки")]
        getpoint,
        [Description("{meta} Получение метаданных - список таблиц, полей базы данных филиала")]
        meta,
        [Description("{schema} Получение схемы подключения по выбранному абоненту в формате PNG")]
        schema
    }
}
