﻿namespace TMP.UI.WPF.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;

    public class LayoutGroup : StackPanel
    {
        public LayoutGroup()
        {
            Grid.SetIsSharedSizeScope(this, true);
        }
    }
}