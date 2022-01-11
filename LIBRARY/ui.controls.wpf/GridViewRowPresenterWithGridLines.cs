namespace TMP.UI.Controls.WPF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;

    public class GridViewRowPresenterWithGridLines : GridViewRowPresenter
    {
        private static readonly Style DefaultSeparatorStyle;
        public static readonly DependencyProperty SeparatorStyleProperty;
        private readonly List<FrameworkElement> lines = new List<FrameworkElement>();

        static GridViewRowPresenterWithGridLines()
        {
            DefaultSeparatorStyle = new Style(typeof(Rectangle));
            DefaultSeparatorStyle.Setters.Add(new Setter(Shape.FillProperty, SystemColors.ControlLightBrush));
            SeparatorStyleProperty = DependencyProperty.Register("SeparatorStyle", typeof(Style), typeof(GridViewRowPresenterWithGridLines),
                                                                    new UIPropertyMetadata(DefaultSeparatorStyle, SeparatorStyleChanged));
        }

        public Style SeparatorStyle
        {
            get => (Style)this.GetValue(SeparatorStyleProperty);
            set => this.SetValue(SeparatorStyleProperty, value);
        }

        private IEnumerable<FrameworkElement> Children => LogicalTreeHelper.GetChildren(this).OfType<FrameworkElement>();

        private static void SeparatorStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (GridViewRowPresenterWithGridLines)d;
            var style = (Style)e.NewValue;
            foreach (FrameworkElement line in presenter.lines)
            {
                line.Style = style;
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var size = base.ArrangeOverride(arrangeSize);
            var children = this.Children.ToList();
            this.EnsureLines(children.Count);
            for (var i = 0; i < this.lines.Count; i++)
            {
                var child = children[i];
                var x = child.TransformToAncestor(this).Transform(new Point(child.ActualWidth, 0)).X + child.Margin.Right;
                var rect = new Rect(x, -this.Margin.Top, 1, size.Height + this.Margin.Top + this.Margin.Bottom);
                var line = this.lines[i];
                line.Measure(rect.Size);
                line.Arrange(rect);
            }

            return size;
        }

        private void EnsureLines(int count)
        {
            count = count - this.lines.Count;
            for (var i = 0; i < count; i++)
            {
                var line = (FrameworkElement)Activator.CreateInstance(this.SeparatorStyle.TargetType);

                // line = new Rectangle{Fill=Brushes.LightGray};
                line.Style = this.SeparatorStyle;
                this.AddVisualChild(line);
                this.lines.Add(line);
            }
        }

        protected override int VisualChildrenCount => base.VisualChildrenCount + this.lines.Count;

        protected override Visual GetVisualChild(int index)
        {
            var count = base.VisualChildrenCount;
            return index < count ? base.GetVisualChild(index) : this.lines[index - count];
        }
    }
}
