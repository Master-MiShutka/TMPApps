// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    public class SharpGridView : GridView
    {
        static SharpGridView()
        {
            ItemContainerStyleKey =
                new ComponentResourceKey(typeof(SharpTreeView), "GridViewItemContainerStyleKey");
        }

        public static ResourceKey ItemContainerStyleKey { get; private set; }

        protected override object ItemContainerDefaultStyleKey => ItemContainerStyleKey;
    }
}
