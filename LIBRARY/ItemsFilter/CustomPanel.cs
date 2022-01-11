namespace ItemsFilter
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    internal class CustomPanel
    {
    }

    /// <summary>
    /// A column based layout panel, that automatically
    /// wraps to new column when required. The user
    /// may also create a new column before an element
    /// using the
    /// </summary>
    public class ColumnedPanel : Panel
    {

        #region Ctor
        static ColumnedPanel()
        {
            // tell DP sub system, this DP, will affect
            // Arrange and Measure phases
            ColumnBreakBeforeProperty =
                DependencyProperty.RegisterAttached(
                "ColumnBreakBefore",
                typeof(bool), typeof(ColumnedPanel),
                new FrameworkPropertyMetadata() { AffectsArrange = true, AffectsMeasure = true });
            ColumnBreakAfterProperty =
                DependencyProperty.RegisterAttached(
                "ColumnBreakAfter",
                typeof(bool), typeof(ColumnedPanel),
                new FrameworkPropertyMetadata() { AffectsArrange = true, AffectsMeasure = true });
        }
        #endregion

        #region DPs

        /// <summary>
        /// Используется для создания новой колонки перед элементом
        /// </summary>
        public static DependencyProperty ColumnBreakBeforeProperty;

        public static void SetColumnBreakBefore(UIElement element, bool value)
        {
            element.SetValue(ColumnBreakBeforeProperty, value);
        }

        public static bool GetColumnBreakBefore(UIElement element)
        {
            return (bool)element.GetValue(ColumnBreakBeforeProperty);
        }

        /// <summary>
        /// Используется для создания новой колонки после элемента
        /// </summary>
        public static DependencyProperty ColumnBreakAfterProperty;

        public static void SetColumnBreakAfter(UIElement element, bool value)
        {
            element.SetValue(ColumnBreakAfterProperty, value);
        }

        public static bool GetColumnBreakAfter(UIElement element)
        {
            return (bool)element.GetValue(ColumnBreakAfterProperty);
        }
        #endregion

        #region Measure Override

        // From MSDN : When overridden in a derived class, measures the
        // size in layout required for child elements and determines a
        // size for the FrameworkElement-derived class
        protected override Size MeasureOverride(Size constraint)
        {
            Size currentColumnSize = new Size();
            Size panelSize = new Size();

            foreach (UIElement element in base.InternalChildren)
            {
                bool breakBefore = (element is ContentPresenter) ? GetColumnBreakBefore((element as ContentPresenter).Content as UIElement) : GetColumnBreakBefore(element);
                bool breakAfter = (element is ContentPresenter) ? GetColumnBreakAfter((element as ContentPresenter).Content as UIElement) : GetColumnBreakAfter(element);

                element.Measure(constraint);
                Size desiredSize = element.DesiredSize;

                if (breakBefore || currentColumnSize.Height + desiredSize.Height > constraint.Height)
                {
                    // Switch to a new column (either because the
                    // element has requested it or space has run out).
                    panelSize.Height = Math.Max(currentColumnSize.Height, panelSize.Height);
                    panelSize.Width += currentColumnSize.Width;
                    currentColumnSize = desiredSize;

                    // If the element is too high to fit using the
                    // maximum height of the line,
                    // just give it a separate column.
                    if (desiredSize.Height > constraint.Height)
                    {
                        panelSize.Height = Math.Max(desiredSize.Height, panelSize.Height);
                        panelSize.Width += desiredSize.Width;
                        currentColumnSize = new Size();
                    }
                }
                else
                if (breakAfter)
                {
                    currentColumnSize.Height += desiredSize.Height;
                    currentColumnSize.Width = Math.Max(desiredSize.Width, currentColumnSize.Width);

                    panelSize.Height = Math.Max(currentColumnSize.Height, panelSize.Height);
                    panelSize.Width += currentColumnSize.Width;
                    currentColumnSize = new Size();
                }
                else
                {
                    currentColumnSize.Height += desiredSize.Height;
                    currentColumnSize.Width = Math.Max(desiredSize.Width, currentColumnSize.Width);
                }
            }

            // Return the size required to fit all elements.
            // Ordinarily, this is the width of the constraint,
            // and the height is based on the size of the elements.
            // However, if an element is higher than the height given
            // to the panel,
            // the desired width will be the height of that column.
            panelSize.Height = Math.Max(currentColumnSize.Height, panelSize.Height);
            panelSize.Width += currentColumnSize.Width;
            return panelSize;
        }
        #endregion

        #region Arrange Override

        // From MSDN : When overridden in a derived class, positions child
        // elements and determines a size for a FrameworkElement derived
        // class.
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            int firstInLine = 0;

            Size currentColumnSize = new Size();

            double accumulatedWidth = 0;

            UIElementCollection elements = base.InternalChildren;
            for (int i = 0; i < elements.Count; i++)
            {

                Size desiredSize = elements[i].DesiredSize;

                bool breakBefore = (elements[i] is ContentPresenter) ? GetColumnBreakBefore((elements[i] as ContentPresenter).Content as UIElement) : GetColumnBreakBefore(elements[i]);
                bool breakAfter = (elements[i] is ContentPresenter) ? GetColumnBreakAfter((elements[i] as ContentPresenter).Content as UIElement) : GetColumnBreakAfter(elements[i]);

                // need to switch to another column
                if (breakBefore || currentColumnSize.Height + desiredSize.Height > arrangeBounds.Height)
                {
                    this.ArrangeColumn(accumulatedWidth, currentColumnSize.Width, firstInLine, i, arrangeBounds);

                    accumulatedWidth += currentColumnSize.Width;
                    currentColumnSize = desiredSize;

                    // the element is higher then the constraint - give it a separate column
                    if (desiredSize.Height > arrangeBounds.Height)
                    {
                        this.ArrangeColumn(accumulatedWidth, desiredSize.Width, i, ++i, arrangeBounds);
                        accumulatedWidth += desiredSize.Width;
                        currentColumnSize = new Size();
                    }

                    firstInLine = i;
                }
                else
                if (breakAfter)
                {
                    currentColumnSize.Height += desiredSize.Height;
                    currentColumnSize.Width = Math.Max(desiredSize.Width, currentColumnSize.Width);

                    this.ArrangeColumn(accumulatedWidth, currentColumnSize.Width, firstInLine, i + 1, arrangeBounds);

                    accumulatedWidth += currentColumnSize.Width;

                    currentColumnSize = new Size();
                    firstInLine = i + 1;
                    i++;
                }
                else // continue to accumulate a column
                {
                    currentColumnSize.Height += desiredSize.Height;
                    currentColumnSize.Width = Math.Max(desiredSize.Width, currentColumnSize.Width);
                }
            }

            if (firstInLine < elements.Count)
            {
                this.ArrangeColumn(accumulatedWidth, currentColumnSize.Width, firstInLine, elements.Count, arrangeBounds);
            }

            return arrangeBounds;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Arranges a single column of elements
        /// </summary>
        private void ArrangeColumn(double x, double columnWidth, int start, int end, Size arrangeBounds)
        {
            double y = 0;
            double totalChildHeight = 0;
            double totalChildWidth = 0;
            double widestChildWidth = 0;
            double xOffset = 0;

            UIElementCollection children = this.InternalChildren;
            UIElement child;

            for (int i = start; i < end; i++)
            {
                child = children[i];
                totalChildHeight += child.DesiredSize.Height;
                totalChildWidth += child.DesiredSize.Width;
                if (child.DesiredSize.Width > widestChildWidth)
                {
                    widestChildWidth = child.DesiredSize.Width;
                }
            }

            // work out y start offset within a given column
            y = (arrangeBounds.Height - totalChildHeight) / 2;

            for (int i = start; i < end; i++)
            {
                child = children[i];
                if (child.DesiredSize.Width < widestChildWidth)
                {
                    xOffset = (widestChildWidth - child.DesiredSize.Width) / 2;
                }

                child.Arrange(new Rect(x + xOffset, y, child.DesiredSize.Width, child.DesiredSize.Height));
                y += child.DesiredSize.Height;
                xOffset = 0;
            }
        }
        #endregion

    }
}