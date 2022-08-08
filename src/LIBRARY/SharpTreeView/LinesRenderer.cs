// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;

    internal class LinesRenderer : FrameworkElement
    {
        static LinesRenderer()
        {
            pen = new Pen(Brushes.LightGray, 1);
            pen.Freeze();
        }

        private static Pen pen;

        private SharpTreeNodeView NodeView => this.TemplatedParent as SharpTreeNodeView;

        protected override void OnRender(DrawingContext dc)
        {
            var indent = this.NodeView.CalculateIndent();
            var p = new Point(indent + 4.5, 0);

            if (!this.NodeView.Node.IsRoot || this.NodeView.ParentTreeView.ShowRootExpander)
            {
                dc.DrawLine(pen, new Point(p.X, this.ActualHeight / 2), new Point(p.X + 10, this.ActualHeight / 2));
            }

            if (this.NodeView.Node.IsRoot)
            {
                return;
            }

            if (this.NodeView.Node.IsLast)
            {
                dc.DrawLine(pen, p, new Point(p.X, this.ActualHeight / 2));
            }
            else
            {
                dc.DrawLine(pen, p, new Point(p.X, this.ActualHeight));
            }

            var current = this.NodeView.Node;
            while (true)
            {
                p.X -= 19;
                current = current.Parent;
                if (p.X < 0)
                {
                    break;
                }

                if (!current.IsLast)
                {
                    dc.DrawLine(pen, p, new Point(p.X, this.ActualHeight));
                }
            }
        }
    }
}
