﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    public class InsertMarker : Control
    {
        static InsertMarker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InsertMarker),
                new FrameworkPropertyMetadata(typeof(InsertMarker)));
        }
    }
}
