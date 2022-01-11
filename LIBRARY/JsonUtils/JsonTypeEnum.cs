namespace TMP.Common.JsonUtils.JsonClassGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum JsonTypeEnum
    {
        Anything,
        String,
        Boolean,
        Integer,
        Long,
        Float,
        Date,
        NullableInteger,
        NullableLong,
        NullableFloat,
        NullableBoolean,
        NullableDate,
        Object,
        Array,
        Dictionary,
        NullableSomething,
        NonConstrained,
    }
}