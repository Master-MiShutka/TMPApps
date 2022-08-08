namespace TMP.UI.WPF.Controls.TreeMap
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    public class TreeMapsPanel : Panel
    {
        #region fields

        private Rect emptyArea;
        private double _weightSum = 0;
        private List<WeightUIElement> items = new List<WeightUIElement>();

        #endregion

        #region dependency properties

        public static readonly DependencyProperty
          WeightProperty = DependencyProperty.RegisterAttached("Weight", typeof(double), typeof(TreeMapsPanel), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        #endregion

        #region enum

        protected enum RowOrientation
        {
            Horizontal,
            Vertical,
        }

        #endregion

        #region properties

        public static double GetWeight(DependencyObject uiElement)
        {
            if (uiElement == null)
            {
                return 0;
            }
            else
            {
                return (double)uiElement.GetValue(TreeMapsPanel.WeightProperty);
            }
        }

        public static void SetWeight(DependencyObject uiElement, double value)
        {
            if (uiElement != null)
            {
                uiElement.SetValue(TreeMapsPanel.WeightProperty, value);
            }
        }

        protected Rect EmptyArea
        {
            get => this.emptyArea;
            set => this.emptyArea = value;
        }

        protected List<WeightUIElement> ManagedItems => this.items;

        #endregion

        #region protected methods

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (WeightUIElement child in this.ManagedItems)
            {
                child.UIElement.Arrange(new Rect(child.ComputedLocation, child.ComputedSize));
            }

            return arrangeSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.EmptyArea = new Rect(0, 0, constraint.Width, constraint.Height);
            this.PrepareItems();

            double area = this.EmptyArea.Width * this.EmptyArea.Height;
            foreach (WeightUIElement item in this.ManagedItems)
            {
                item.RealArea = area * item.Weight / this._weightSum;
            }

            this.ComputeBounds();

            foreach (WeightUIElement child in this.ManagedItems)
            {
                if (this.IsValidSize(child.ComputedSize))
                {
                    child.UIElement.Measure(child.ComputedSize);
                }
                else
                {
                    child.UIElement.Measure(new Size(0, 0));
                }
            }

            return constraint;
        }

        protected virtual void ComputeBounds()
        {
            this.ComputeTreeMaps(this.ManagedItems);
        }

        protected double GetShortestSide()
        {
            return Math.Min(this.EmptyArea.Width, this.EmptyArea.Height);
        }

        protected RowOrientation GetOrientation()
        {
            return this.EmptyArea.Width > this.EmptyArea.Height ? RowOrientation.Horizontal : RowOrientation.Vertical;
        }

        protected virtual Rect GetRectangle(RowOrientation orientation, WeightUIElement item, double x, double y, double width, double height)
        {
            if (orientation == RowOrientation.Horizontal)
            {
                return new Rect(x, y, item.RealArea / height, height);
            }
            else
            {
                return new Rect(x, y, width, item.RealArea / width);
            }
        }

        protected virtual void ComputeNextPosition(RowOrientation orientation, ref double xPos, ref double yPos, double width, double height)
        {
            if (orientation == RowOrientation.Horizontal)
            {
                xPos += width;
            }
            else
            {
                yPos += height;
            }
        }

        protected void ComputeTreeMaps(List<WeightUIElement> items)
        {
            RowOrientation orientation = this.GetOrientation();

            double areaSum = 0;

            foreach (WeightUIElement item in items)
            {
                areaSum += item.RealArea;
            }

            Rect currentRow;
            if (orientation == RowOrientation.Horizontal)
            {
                currentRow = new Rect(this.emptyArea.X, this.emptyArea.Y, areaSum / this.emptyArea.Height, this.emptyArea.Height);
                this.emptyArea = new Rect(this.emptyArea.X + currentRow.Width, this.emptyArea.Y, Math.Max(0, this.emptyArea.Width - currentRow.Width), this.emptyArea.Height);
            }
            else
            {
                currentRow = new Rect(this.emptyArea.X, this.emptyArea.Y, this.emptyArea.Width, areaSum / this.emptyArea.Width);
                this.emptyArea = new Rect(this.emptyArea.X, this.emptyArea.Y + currentRow.Height, this.emptyArea.Width, Math.Max(0, this.emptyArea.Height - currentRow.Height));
            }

            double prevX = currentRow.X;
            double prevY = currentRow.Y;

            foreach (WeightUIElement item in items)
            {
                Rect rect = this.GetRectangle(orientation, item, prevX, prevY, currentRow.Width, currentRow.Height);

                item.AspectRatio = rect.Width / rect.Height;
                item.ComputedSize = rect.Size;
                item.ComputedLocation = rect.Location;

                this.ComputeNextPosition(orientation, ref prevX, ref prevY, rect.Width, rect.Height);
            }
        }

        #endregion

        #region private methods

        private bool IsValidSize(Size size)
        {
            return !size.IsEmpty && size.Width > 0 && size.Width != double.NaN && size.Height > 0 && size.Height != double.NaN;
        }

        private bool IsValidItem(WeightUIElement item)
        {
            return item != null && item.Weight != double.NaN && Math.Round(item.Weight, 0) != 0;
        }

        private void PrepareItems()
        {

            this._weightSum = 0;
            this.ManagedItems.Clear();

            foreach (UIElement child in this.Children)
            {
                WeightUIElement element = new WeightUIElement(child, TreeMapsPanel.GetWeight(child));
                if (this.IsValidItem(element))
                {
                    this._weightSum += element.Weight;
                    this.ManagedItems.Add(element);
                }
                else
                {
                    element.ComputedSize = Size.Empty;
                    element.ComputedLocation = new Point(0, 0);
                    element.UIElement.Measure(element.ComputedSize);
                    element.UIElement.Visibility = Visibility.Collapsed;
                }
            }

            this.ManagedItems.Sort(WeightUIElement.CompareByValueDecreasing);
        }

        #endregion

        #region inner classes

        protected class WeightUIElement
        {
            #region fields

            private double weight;
            private double area;
            private UIElement _element;
            private Size _desiredSize;
            private Point _desiredLocation;
            private double _ratio;

            #endregion

            #region ctors

            public WeightUIElement(UIElement element, double weight)
            {
                this._element = element;
                this.weight = weight;
            }

            #endregion

            #region properties

            internal Size ComputedSize
            {
                get => this._desiredSize;
                set => this._desiredSize = value;
            }

            internal Point ComputedLocation
            {
                get => this._desiredLocation;
                set => this._desiredLocation = value;
            }

            public double AspectRatio
            {
                get => this._ratio;
                set => this._ratio = value;
            }

            public double Weight => this.weight;

            public double RealArea
            {
                get => this.area;
                set => this.area = value;
            }

            public UIElement UIElement => this._element;

            #endregion

            #region static members

            public static int CompareByValueDecreasing(WeightUIElement x, WeightUIElement y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    if (y == null)
                    {
                        return 1;
                    }
                    else
                    {
                        return x.Weight.CompareTo(y.Weight) * -1;
                    }
                }
            }

            #endregion
        }

        #endregion

    }
}
