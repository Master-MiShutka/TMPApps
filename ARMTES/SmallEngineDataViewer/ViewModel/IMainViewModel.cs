using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.ARMTES.ViewModel
{
    interface IMainViewModel
    {
        String Status { get; set; }
        String DetailedStatus { get; set; }
        bool IsBusy { get; set; }

        void Init();
    }
}
