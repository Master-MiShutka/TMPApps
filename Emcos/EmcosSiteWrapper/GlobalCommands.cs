using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TMP.Work.Emcos
{
    public class GlobalCommands
    {
        public static readonly ICommand CloseCommand = new Wpf.Common.DelegateCommand<System.Windows.Window>(w => { if (w != null) w.Close(); });
    }
}
