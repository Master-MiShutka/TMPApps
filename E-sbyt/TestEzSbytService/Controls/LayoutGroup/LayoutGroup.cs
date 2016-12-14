using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TMP.Work.AmperM.TestApp.Controls
{
    public class LayoutGroup : StackPanel
    {
        public LayoutGroup()
        {
            Grid.SetIsSharedSizeScope(this, true);
        }
    }
}
