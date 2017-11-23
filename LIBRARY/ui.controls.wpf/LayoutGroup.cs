using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TMP.UI.Controls.WPF
{
    public class LayoutGroup : StackPanel
    {
        public LayoutGroup()
        {
            Grid.SetIsSharedSizeScope(this, true);
        }
    }
}