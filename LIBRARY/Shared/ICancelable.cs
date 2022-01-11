namespace TMP.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;

    public interface ICancelable
    {
        ICommand CancelCommand { get; set; }

        bool IsCanceled { get; set; }

        bool CanCanceled { get; }
    }
}
