// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    internal static class ExtensionMethods
    {
        public static T FindAncestor<T>(this DependencyObject d)
            where T : class
        {
            return AncestorsAndSelf(d).OfType<T>().FirstOrDefault();
        }

        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject d)
        {
            while (d != null)
            {
                yield return d;
                d = VisualTreeHelper.GetParent(d);
            }
        }

        public static void AddOnce(this IList list, object item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
    }
}
