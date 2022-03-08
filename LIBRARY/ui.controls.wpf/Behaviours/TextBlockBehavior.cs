namespace TMP.UI.Controls.WPF.Behaviours
{
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Microsoft.Xaml.Behaviors;

    public class TextBlockBehavior : Behavior<TextBlock>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SizeChanged += this.AssociatedObject_SizeChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SizeChanged -= this.AssociatedObject_SizeChanged;
        }

        private void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBlock temp = new TextBlock()
            {
                Text = this.AssociatedObject.Text,
                LineStackingStrategy = this.AssociatedObject.LineStackingStrategy,
                LineHeight = this.AssociatedObject.LineHeight,
                TextTrimming = TextTrimming.None,
                TextWrapping = this.AssociatedObject.TextWrapping,
                Height = this.AssociatedObject.Height
            };
            double maxwidth = this.AssociatedObject.MaxWidth - 10;
            double desiredHeight = double.PositiveInfinity;
            while (desiredHeight > this.AssociatedObject.MaxHeight)
            {
                temp.Measure(new Size(maxwidth, double.PositiveInfinity));
                maxwidth += 10;
                desiredHeight = temp.DesiredSize.Height;
            }

            this.AssociatedObject.MaxWidth = maxwidth;

        }
    }
}
