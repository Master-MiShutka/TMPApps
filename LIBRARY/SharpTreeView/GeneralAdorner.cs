// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

namespace ICSharpCode.TreeView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class GeneralAdorner : Adorner
    {
        public GeneralAdorner(UIElement target)
            : base(target)
        {
        }

        private FrameworkElement child;

        public FrameworkElement Child
        {
            get => this.child;

            set
            {
                if (this.child != value)
                {
                    this.RemoveVisualChild(this.child);
                    this.RemoveLogicalChild(this.child);
                    this.child = value;
                    this.AddLogicalChild(value);
                    this.AddVisualChild(value);
                    this.InvalidateMeasure();
                }
            }
        }

        protected override int VisualChildrenCount => this.child == null ? 0 : 1;

        protected override Visual GetVisualChild(int index)
        {
            return this.child;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (this.child != null)
            {
                this.child.Measure(constraint);
                return this.child.DesiredSize;
            }

            return new Size();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.child != null)
            {
                this.child.Arrange(new Rect(finalSize));
                return finalSize;
            }

            return new Size();
        }
    }
}
