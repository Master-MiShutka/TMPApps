﻿namespace TMP.PrintEngine.Utils
{
    using System;
    using System.Data;
    using System.Linq;

    internal class DataTableUtil
    {
        public static void Validate(DataTable dataTable)
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                if (column.ExtendedProperties.ContainsKey("Width"))
                {
                    throw new FormatException(string.Format("Column Width not Defined for column : '{0}'", column.ColumnName));
                }
            }
        }
    }
}
