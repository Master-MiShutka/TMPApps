using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    public interface IMainViewModel
    {
        void ShowAllMeters();
        void ShowMetersWithGroupingAtField(string fieldName);
        void ShowMeterFilteredByFieldValue(string fieldName, string value);

        string GetDepartamentName(Model.Departament departament);
    }
}
