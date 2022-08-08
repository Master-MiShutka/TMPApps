using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TMP.Work.Emcos
{
    using TMP.Shared.Commands;

    public class GlobalCommands
    {
        public static readonly ICommand CloseCommand = new DelegateCommand(w => { if (w is System.Windows.Window window) window.Close(); });
    }
}
