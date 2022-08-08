using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TMP.Wpf.CommonControls.Dialogs
{
    public interface IModalDialogHelper
    {
        string Text { get; }

        ICommand CloseCommand { get; }

        void Show(string text);

        void Close();
    }
}
