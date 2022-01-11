namespace TMP.UI.Controls.WPF
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using TMP.UI.Controls.WPF.Extensions;

    /// <summary>
    /// Defines a flexible grid area that consists of columns and rows.
    /// Depending on the orientation, either the rows or the columns are auto-generated,
    /// and the children's position is set according to their index.
    ///
    /// Partially based on work at http://rachel53461.wordpress.com/2011/09/17/wpf-grids-rowcolumn-count-properties/
    /// </summary>
    public class AutoGrid : Grid
    {
        public static readonly DependencyProperty ChildHorizontalAlignmentProperty = DependencyProperty.Register(
                nameof(ChildHorizontalAlignment),
                typeof(HorizontalAlignment),
                typeof(AutoGrid),
                new FrameworkPropertyMetadata(DependencyProperty.UnsetValue, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnChildHorizontalAlignmentChanged)));

        public static readonly DependencyProperty ChildMarginProperty = DependencyProperty.Register(
            nameof(ChildMargin),
            typeof(Thickness),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(DependencyProperty.UnsetValue, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnChildMarginChanged)));

        public static readonly DependencyProperty ChildVerticalAlignmentProperty = DependencyProperty.Register(
            nameof(ChildVerticalAlignment),
            typeof(VerticalAlignment),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(DependencyProperty.UnsetValue, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnChildVerticalAlignmentChanged)));

        public static readonly DependencyProperty ColumnCountProperty = DependencyProperty.RegisterAttached(
            nameof(ColumnCount),
            typeof(int),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnColumnCountChanged)));

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached(
            nameof(Columns),
            typeof(string),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(DependencyProperty.UnsetValue, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnColumnsChanged)));

        public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.RegisterAttached(
            nameof(ColumnWidth),
            typeof(GridLength),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(GridLength.Auto, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnColumnWidthChanged)));

        public static readonly DependencyProperty IsAutoIndexingProperty = DependencyProperty.Register(
            nameof(IsAutoIndexing),
            typeof(bool),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty RowCountProperty = DependencyProperty.RegisterAttached(
            nameof(RowCount),
            typeof(int),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnRowCountChanged)));

        public static readonly DependencyProperty RowHeightProperty = DependencyProperty.RegisterAttached(
            nameof(RowHeight),
            typeof(GridLength),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(GridLength.Auto, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnRowHeightChanged)));

        public static readonly DependencyProperty RowsProperty = DependencyProperty.RegisterAttached(
            nameof(Rows),
            typeof(string),
            typeof(AutoGrid),
            new FrameworkPropertyMetadata(DependencyProperty.UnsetValue, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnRowsChanged)));

        /// <summary>
        /// Gets or sets the child horizontal alignment.
        /// </summary>
        /// <value>The child horizontal alignment.</value>
        [Category("Layout")]
        [Description("Presets the horizontal alignment of all child controls")]
        public HorizontalAlignment ChildHorizontalAlignment
        {
            get => (HorizontalAlignment)this.GetValue(ChildHorizontalAlignmentProperty);
            set => this.SetValue(ChildHorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the child margin.
        /// </summary>
        /// <value>The child margin.</value>
        [Category("Layout")]
        [Description("Presets the margin of all child controls")]
        public Thickness ChildMargin
        {
            get => (Thickness)this.GetValue(ChildMarginProperty);
            set => this.SetValue(ChildMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the child vertical alignment.
        /// </summary>
        /// <value>The child vertical alignment.</value>
        [Category("Layout")]
        [Description("Presets the vertical alignment of all child controls")]
        public VerticalAlignment ChildVerticalAlignment
        {
            get => (VerticalAlignment)this.GetValue(ChildVerticalAlignmentProperty);
            set => this.SetValue(ChildVerticalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets the column count
        /// </summary>
        [Category("Layout")]
        [Description("Defines a set number of columns")]
        public int ColumnCount
        {
            get => (int)this.GetValue(ColumnCountProperty);
            set => this.SetValue(ColumnCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the columns
        /// </summary>
        [Category("Layout")]
        [Description("Defines all columns using comma separated grid length notation")]
        public string Columns
        {
            get => (string)this.GetValue(ColumnsProperty);
            set => this.SetValue(ColumnsProperty, value);
        }

        /// <summary>
        /// Gets or sets the fixed column width
        /// </summary>
        [Category("Layout")]
        [Description("Presets the width of all columns set using the ColumnCount property")]
        public GridLength ColumnWidth
        {
            get => (GridLength)this.GetValue(ColumnWidthProperty);
            set => this.SetValue(ColumnWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the children are automatically indexed.
        /// <remarks>
        /// The default is <c>true</c>.
        /// Note that if children are already indexed, setting this property to <c>false</c> will not remove their indices.
        /// </remarks>
        /// </summary>
        [Category("Layout")]
        [Description("Set to false to disable the auto layout functionality")]
        public bool IsAutoIndexing
        {
            get => (bool)this.GetValue(IsAutoIndexingProperty);
            set => this.SetValue(IsAutoIndexingProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation.
        /// <remarks>The default is Vertical.</remarks>
        /// </summary>
        /// <value>The orientation.</value>
        [Category("Layout")]
        [Description("Defines the directionality of the autolayout. Use vertical for a column first layout, horizontal for a row first layout.")]
        public Orientation Orientation
        {
            get => (Orientation)this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of rows
        /// </summary>
        [Category("Layout")]
        [Description("Defines a set number of rows")]
        public int RowCount
        {
            get => (int)this.GetValue(RowCountProperty);
            set => this.SetValue(RowCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the fixed row height
        /// </summary>
        [Category("Layout")]
        [Description("Presets the height of all rows set using the RowCount property")]
        public GridLength RowHeight
        {
            get => (GridLength)this.GetValue(RowHeightProperty);
            set => this.SetValue(RowHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the rows
        /// </summary>
        [Category("Layout")]
        [Description("Defines all rows using comma separated grid length notation")]
        public string Rows
        {
            get => (string)this.GetValue(RowsProperty);
            set => this.SetValue(RowsProperty, value);
        }

        /// <summary>
        /// Handles the column count changed event
        /// </summary>
        public static void OnColumnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue < 0)
            {
                return;
            }

            var grid = d as AutoGrid;

            // look for an existing column definition for the height
            var width = GridLength.Auto;
            if (grid.ColumnDefinitions.Count > 0)
            {
                width = grid.ColumnDefinitions[0].Width;
            }

            // clear and rebuild
            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < (int)e.NewValue; i++)
            {
                grid.ColumnDefinitions.Add(
                    new ColumnDefinition() { Width = width });
            }
        }

        /// <summary>
        /// Handle the columns changed event
        /// </summary>
        public static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((string)e.NewValue == string.Empty)
            {
                return;
            }

            var grid = d as AutoGrid;
            grid.ColumnDefinitions.Clear();

            var defs = Parse((string)e.NewValue);
            foreach (var def in defs)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = def });
            }
        }

        /// <summary>
        /// Handle the fixed column width changed event
        /// </summary>
        public static void OnColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as AutoGrid;

            // add a default column if missing
            if (grid.ColumnDefinitions.Count == 0)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // set all existing columns to this width
            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                grid.ColumnDefinitions[i].SetCurrentValue(ColumnDefinition.WidthProperty, (GridLength)e.NewValue);
            }
        }

        /// <summary>
        /// Handle the fixed row height changed event
        /// </summary>
        public static void OnRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as AutoGrid;

            // add a default row if missing
            if (grid.RowDefinitions.Count == 0)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            // set all existing rows to this height
            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                grid.RowDefinitions[i].SetCurrentValue(RowDefinition.HeightProperty, (GridLength)e.NewValue);
            }
        }

        /// <summary>
        /// Parse an array of grid lengths from comma delim text
        /// </summary>
        /// <returns>GridLength[]</returns>
        public static GridLength[] Parse(string text)
        {
            var tokens = text.Split(',');
            var definitions = new GridLength[tokens.Length];
            for (var i = 0; i < tokens.Length; i++)
            {
                var str = tokens[i];
                double value;

                // ratio
                if (str.Contains('*'))
                {
                    if (!double.TryParse(str.Replace("*", string.Empty), out value))
                    {
                        value = 1.0;
                    }

                    definitions[i] = new GridLength(value, GridUnitType.Star);
                    continue;
                }

                // pixels
                if (double.TryParse(str, out value))
                {
                    definitions[i] = new GridLength(value);
                    continue;
                }

                // auto
                definitions[i] = GridLength.Auto;
            }

            return definitions;
        }

        /// <summary>
        /// Handles the row count changed event
        /// </summary>
        public static void OnRowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue < 0)
            {
                return;
            }

            var grid = d as AutoGrid;

            // look for an existing row to get the height
            var height = GridLength.Auto;
            if (grid.RowDefinitions.Count > 0)
            {
                height = grid.RowDefinitions[0].Height;
            }

            // clear and rebuild
            grid.RowDefinitions.Clear();
            for (int i = 0; i < (int)e.NewValue; i++)
            {
                grid.RowDefinitions.Add(
                    new RowDefinition() { Height = height });
            }
        }

        /// <summary>
        /// Handle the rows changed event
        /// </summary>
        public static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((string)e.NewValue == string.Empty)
            {
                return;
            }

            var grid = d as AutoGrid;
            grid.RowDefinitions.Clear();

            var defs = Parse((string)e.NewValue);
            foreach (var def in defs)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = def });
            }
        }

        #region Overrides

        /// <summary>
        /// Measures the children of a <see cref="T:System.Windows.Controls.Grid"/> in anticipation of arranging them during the <see cref="M:ArrangeOverride"/> pass.
        /// </summary>
        /// <param name="constraint">Indicates an upper limit size that should not be exceeded.</param>
        /// <returns>
        ///     <see cref="Size"/> that represents the required size to arrange child content.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            this.PerformLayout();
            return base.MeasureOverride(constraint);
        }

        #endregion Overrides

        /// <summary>
        /// Called when [child horizontal alignment changed].
        /// </summary>
        private static void OnChildHorizontalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as AutoGrid;
            foreach (UIElement child in grid.Children)
            {
                child.SetCurrentValue(FrameworkElement.HorizontalAlignmentProperty, grid.ChildHorizontalAlignment);
            }
        }

        /// <summary>
        /// Called when [child layout changed].
        /// </summary>
        private static void OnChildMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as AutoGrid;
            foreach (UIElement child in grid.Children)
            {
                child.SetCurrentValue(FrameworkElement.MarginProperty, grid.ChildMargin);
            }
        }

        /// <summary>
        /// Called when [child vertical alignment changed].
        /// </summary>
        private static void OnChildVerticalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as AutoGrid;
            foreach (UIElement child in grid.Children)
            {
                child.SetCurrentValue(FrameworkElement.VerticalAlignmentProperty, grid.ChildVerticalAlignment);
            }
        }

        /// <summary>
        /// Handled the redraw properties changed event
        /// </summary>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Apply child margins and layout effects such as alignment
        /// </summary>
        private void ApplyChildLayout(UIElement child)
        {
            if (child is null)
            {
                throw new System.ArgumentNullException(nameof(child));
            }

            child.SetIfDefault(FrameworkElement.MarginProperty, this.ChildMargin);

            child.SetIfDefault(HorizontalAlignmentProperty, this.ChildHorizontalAlignment);

            child.SetIfDefault(FrameworkElement.VerticalAlignmentProperty, this.ChildVerticalAlignment);
        }

        /// <summary>
        /// Clamp a value to its maximum.
        /// </summary>
        /// <returns>value</returns>
        private int Clamp(int value, int max)
        {
            return (value > max) ? max : value;
        }

        /// <summary>
        /// Perform the grid layout of row and column indexes
        /// </summary>
        private void PerformLayout()
        {
            var fillRowFirst = this.Orientation == Orientation.Horizontal;
            var rowCount = this.RowDefinitions.Count;
            var colCount = this.ColumnDefinitions.Count;

            if (rowCount == 0 || colCount == 0)
            {
                return;
            }

            var position = 0;
            var skip = new bool[rowCount, colCount];
            foreach (UIElement child in this.Children)
            {
                var childIsCollapsed = child.Visibility == Visibility.Collapsed;
                if (this.IsAutoIndexing && !childIsCollapsed)
                {
                    if (fillRowFirst)
                    {
                        var row = this.Clamp(position / colCount, rowCount - 1);
                        var col = this.Clamp(position % colCount, colCount - 1);
                        if (skip[row, col])
                        {
                            position++;
                            row = position / colCount;
                            col = position % colCount;
                        }

                        Grid.SetRow(child, row);
                        Grid.SetColumn(child, col);
                        position += Grid.GetColumnSpan(child);

                        var offset = Grid.GetRowSpan(child) - 1;
                        while (offset > 0)
                        {
                            skip[row + offset--, col] = true;
                        }
                    }
                    else
                    {
                        var row = this.Clamp(position % rowCount, rowCount - 1);
                        var col = this.Clamp(position / rowCount, colCount - 1);
                        if (skip[row, col])
                        {
                            position++;
                            row = position % rowCount;
                            col = position / rowCount;
                        }

                        Grid.SetRow(child, row);
                        Grid.SetColumn(child, col);
                        position += Grid.GetRowSpan(child);

                        var offset = Grid.GetColumnSpan(child) - 1;
                        while (offset > 0)
                        {
                            skip[row, col + offset--] = true;
                        }
                    }
                }

                this.ApplyChildLayout(child);
            }
        }
    }
}
