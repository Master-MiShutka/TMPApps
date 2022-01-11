namespace TMP.UI.Controls.WPF.TableView
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    [TemplatePart(Name = "PART_HeaderPresenter", Type = typeof(TableViewHeaderPresenter))]
    [TemplatePart(Name = "PART_HeaderPanel", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_RowsPresenter", Type = typeof(TableViewRowsPresenter))]
    [TemplatePart(Name = "PART_RowsPanel", Type = typeof(Panel))]
    public class TableView : Control
    {
        public event EventHandler<TableViewColumnEventArgs> ColumnWidthChanged;

        public event EventHandler<TableViewColumnEventArgs> SortingChanged;

        internal TableViewHeaderPresenter HeaderRowPresenter { get; set; }

        internal TableViewRowsPresenter RowsPresenter { get; set; }

        internal TableViewCellsPresenter SelectedCellsPresenter { get; set; }

        internal TableViewCell SelectedCell { get; set; }

        private Rect fixedClipRect = Rect.Empty;

        internal Rect FixedClipRect
        {
            get
            {
                if (this.fixedClipRect == Rect.Empty)
                {
                    double width = 0.0;
                    if (this.Columns.Count >= this.FixedColumnCount)
                    {
                        for (int i = 0; i < this.FixedColumnCount; ++i)
                        {
                            width += this.Columns[i].Width;
                        }
                    }

                    this.fixedClipRect = new Rect(this.HorizontalScrollOffset, 0, width, 0);
                }

                return this.fixedClipRect;
            }
        }

        internal void ResetFixedClipRect()
        {
            this.fixedClipRect = Rect.Empty;
        }

        public double HeaderHeight => this.HeaderRowPresenter == null ? 0 : this.HeaderRowPresenter.Height;

        public ItemCollection Items
        {
            get
            {
                if (this.RowsPresenter != null)
                {
                    return this.RowsPresenter.Items;
                }

                return null;
            }
        }

        // Constructors
        static TableView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TableView), new FrameworkPropertyMetadata(typeof(TableView)));
        }

        public TableView()
          : base()
        {
            this.ResetFixedClipRect();

            this.Columns = new ObservableCollection<TableViewColumn>();

            // SnapsToDevicePixels = true;
            // UseLayoutRounding = true;
            // Columns.CollectionChanged += ColumnsChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.HeaderRowPresenter = this.GetTemplateChild("PART_HeaderPresenter") as TableViewHeaderPresenter;
            this.RowsPresenter = this.GetTemplateChild("PART_RowsPresenter") as TableViewRowsPresenter;
        }

        internal void NotifyColumnWidthChanged(TableViewColumn column)
        {
            if (this.ColumnWidthChanged != null)
            {
                this.ColumnWidthChanged(this, new TableViewColumnEventArgs(column));
            }
        }

        internal void NotifySortingChanged(TableViewColumn column)
        {
            if (this.SortingChanged != null)
            {
                this.SortingChanged(this, new TableViewColumnEventArgs(column));
            }
        }

        private void ColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    (item as TableViewColumn).ParentTableView = this;
                }
            }

            /*foreach (var col in Columns)
              col.ParentTableView = this;*/

            if (this.HeaderRowPresenter != null)
            {
                this.ResetFixedClipRect();
                this.HeaderRowPresenter.HeaderInvalidateArrange();
            }

            if (this.RowsPresenter != null)
            {
                this.RowsPresenter.ColumnsChanged();
            }
        }

        private void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (this.HeaderRowPresenter != null)
            {
                this.ResetFixedClipRect();
                this.HeaderRowPresenter.HeaderInvalidateArrange();
            }

            if (this.RowsPresenter != null)
            {
                this.RowsPresenter.RowsInvalidateArrange();
            }
        }

        internal int IndexOfRow(TableViewCellsPresenter cp)
        {
            if (this.RowsPresenter != null)
            {
                return this.RowsPresenter.ItemContainerGenerator.IndexFromContainer(cp);
            }

            return -1;
        }

        internal void FocusedRowChanged(TableViewCellsPresenter cp)
        {
            this.FocusedRowIndex = this.IndexOfRow(cp);
            this.SelectedRowIndex = this.FocusedRowIndex;
        }

        internal void FocusedColumnChanged(TableViewColumn col)
        {
            this.FocusedColumnIndex = this.Columns.IndexOf(col);
            this.SelectedColumnIndex = this.FocusedColumnIndex;
        }

        public TableViewColumnHeader GetColumnHeaderAtLocation(Point loc)
        {
            if (this.HeaderRowPresenter != null)
            {
                return this.HeaderRowPresenter.GetColumnHeaderAtLocation(loc);
            }

            return null;
        }

        public TableViewColumn GetColumnAtLocation(Point loc)
        {
            var ch = this.GetColumnHeaderAtLocation(loc);
            if (ch != null)
            {
                return this.Columns[ch.ColumnIndex];
            }

            return null;
        }

        public object GetItemAtLocation(Point loc)
        {
            loc.Y -= this.HeaderRowPresenter.RenderSize.Height;
            if (this.RowsPresenter != null)
            {
                return this.RowsPresenter.GetItemAtLocation(loc);
            }

            return null;
        }

        public int GetCellIndexAtLocation(Point loc)
        {
            loc.Y -= this.HeaderRowPresenter.RenderSize.Height;
            if (this.RowsPresenter != null)
            {
                return this.RowsPresenter.GetCellIndexAtLocation(loc);
            }

            return -1;
        }

        #region Dependency Properties

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TableView)d).NotifyPropertyChanged(d, e);
        }

        private static void OnUIPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TableView t = (TableView)d;
            if (t != null && t.Columns != null)
            {
                foreach (TableViewColumn c in t.Columns)
                {
                    if (c.ShowHistogramm)
                    {
                        c.InvalidateCells();
                    }
                }
            }
        }

        //------------------------
        public static readonly DependencyProperty CellNavigationProperty =
            DependencyProperty.Register(nameof(CellNavigation), typeof(bool), typeof(TableView), new PropertyMetadata(true));

        public bool CellNavigation
        {
            get => (bool)this.GetValue(CellNavigationProperty);
            set => this.SetValue(CellNavigationProperty, value);
        }

        //------------------------
        public static readonly DependencyProperty ShowVerticalGridLinesProperty =
            DependencyProperty.Register(nameof(ShowVerticalGridLines), typeof(bool), typeof(TableView), new PropertyMetadata(true));

        public bool ShowVerticalGridLines
        {
            get => (bool)this.GetValue(ShowVerticalGridLinesProperty);
            set => this.SetValue(ShowVerticalGridLinesProperty, value);
        }

        //------------------------
        public static readonly DependencyProperty ShowHorizontalGridLinesProperty =
            DependencyProperty.Register(nameof(ShowHorizontalGridLines), typeof(bool), typeof(TableView), new PropertyMetadata(false));

        public bool ShowHorizontalGridLines
        {
            get => (bool)this.GetValue(ShowHorizontalGridLinesProperty);
            set => this.SetValue(ShowHorizontalGridLinesProperty, value);
        }

        //---------------
        public static readonly DependencyProperty ItemsSourceProperty =
           DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TableView), new FrameworkPropertyMetadata(null, OnItemsSourcePropertyChanged));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tv = d as TableView;
            if (tv != null)
            {
                if (tv.RowsPresenter != null)
                {
                    tv.RowsPresenter.ItemsSource = e.NewValue as IEnumerable;
                }
            }
        }

        //--------------
        public static readonly DependencyProperty SelectedRowIndexProperty =
            DependencyProperty.Register(nameof(SelectedRowIndex), typeof(object), typeof(TableView), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int SelectedRowIndex
        {
            get => (int)this.GetValue(SelectedRowIndexProperty);
            set => this.SetValue(SelectedRowIndexProperty, value);
        }

        //--------------
        public static readonly DependencyProperty SelectedColumnIndexProperty =
            DependencyProperty.Register(nameof(SelectedColumnIndex), typeof(int), typeof(TableView), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int SelectedColumnIndex
        {
            get => (int)this.GetValue(SelectedColumnIndexProperty);
            set => this.SetValue(SelectedColumnIndexProperty, value);
        }

        //--------------
        public static readonly DependencyProperty FocusedRowIndexProperty =
            DependencyProperty.Register(nameof(FocusedRowIndex), typeof(object), typeof(TableView), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int FocusedRowIndex
        {
            get => (int)this.GetValue(FocusedRowIndexProperty);
            set => this.SetValue(FocusedRowIndexProperty, value);
        }

        //--------------
        public static readonly DependencyProperty FocusedColumnIndexProperty =
            DependencyProperty.Register(nameof(FocusedColumnIndex), typeof(int), typeof(TableView), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int FocusedColumnIndex
        {
            get => (int)this.GetValue(FocusedColumnIndexProperty);
            set => this.SetValue(FocusedColumnIndexProperty, value);
        }

        //--------------
        public static readonly DependencyProperty FixedColumnCountProperty =
            DependencyProperty.Register(nameof(FixedColumnCount), typeof(int), typeof(TableView), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public int FixedColumnCount
        {
            get => (int)this.GetValue(FixedColumnCountProperty);
            set => this.SetValue(FixedColumnCountProperty, value);
        }

        //--------------
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(ObservableCollection<TableViewColumn>), typeof(TableView),
                new PropertyMetadata(
                    new ObservableCollection<TableViewColumn>(),
                    new PropertyChangedCallback(TableView.OnColumnsPropertyChanged)));

        private static void OnColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tv = d as TableView;
            if (e.OldValue != null)
            {
                ((ObservableCollection<TableViewColumn>)e.OldValue).CollectionChanged -= tv.ColumnsChanged;
            }

            if (e.NewValue != null)
            {
                ((ObservableCollection<TableViewColumn>)e.NewValue).CollectionChanged += tv.ColumnsChanged;
            }
        }

        public ObservableCollection<TableViewColumn> Columns
        {
            get => (ObservableCollection<TableViewColumn>)this.GetValue(ColumnsProperty);
            set => this.SetValue(ColumnsProperty, value);
        }

        /* private ObservableCollection<TableViewColumn> _columns;

         public ObservableCollection<TableViewColumn> Columns
         {
             get { return _columns; }
             set
             {
                 if (_columns != null)
                     _columns.CollectionChanged -= ColumnsChanged;

                 _columns = value;

                 if (_columns != null)
                     _columns.CollectionChanged += ColumnsChanged;
                 ColumnsChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _columns));
             }
             //      get { return (ObservableCollection<TableViewColumn>)GetValue(ColumnsProperty); }
             //      set { SetValue(ColumnsProperty, value); }
         }*/

        //--------------
        public static readonly DependencyProperty HorizontalScrollOffsetProperty =
          DependencyProperty.Register(nameof(HorizontalScrollOffset), typeof(double), typeof(TableView), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public double HorizontalScrollOffset
        {
            get => (double)this.GetValue(HorizontalScrollOffsetProperty);
            set => this.SetValue(HorizontalScrollOffsetProperty, value);
        }

        //----------------------------
        public static readonly DependencyProperty CellsPanelStyleProperty =
          DependencyProperty.Register(nameof(CellsPanelStyle), typeof(Style), typeof(TableView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public Style CellsPanelStyle
        {
            get => (Style)this.GetValue(CellsPanelStyleProperty);
            set => this.SetValue(CellsPanelStyleProperty, value);
        }

        //----------------------------
        public static readonly DependencyProperty RowsPanelStyleProperty =
          DependencyProperty.Register(nameof(RowsPanelStyle), typeof(Style), typeof(TableView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public Style RowsPanelStyle
        {
            get => (Style)this.GetValue(RowsPanelStyleProperty);
            set => this.SetValue(RowsPanelStyleProperty, value);
        }

        //----------------------------
        public static readonly DependencyProperty HeaderPanelStyleProperty =
          DependencyProperty.Register(nameof(HeaderPanelStyle), typeof(Style), typeof(TableView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public Style HeaderPanelStyle
        {
            get => (Style)this.GetValue(HeaderPanelStyleProperty);
            set => this.SetValue(HeaderPanelStyleProperty, value);
        }

        //----------------------------
        public static readonly DependencyProperty GridLinesBrushProperty =
          DependencyProperty.Register(nameof(GridLinesBrush), typeof(Brush), typeof(TableView), new FrameworkPropertyMetadata(Brushes.DarkSlateGray));

        public Brush GridLinesBrush
        {
            get => (Brush)this.GetValue(GridLinesBrushProperty);
            set => this.SetValue(GridLinesBrushProperty, value);
        }

        //----------------------------
        public static readonly DependencyProperty MarkBrushProperty =
                DependencyProperty.Register(nameof(MarkBrush), typeof(Brush), typeof(TableView),
                    new FrameworkPropertyMetadata(Brushes.LightYellow));

        public Brush MarkBrush
        {
            get => (Brush)this.GetValue(MarkBrushProperty);
            set => this.SetValue(MarkBrushProperty, value);
        }

        //----------------------------
        public static readonly DependencyProperty HistogramAxisThicknessProperty =
      DependencyProperty.Register(nameof(HistogramAxisThickness), typeof(double), typeof(TableView),
          new FrameworkPropertyMetadata(1.0,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public double HistogramAxisThickness
        {
            get => (double)this.GetValue(HistogramAxisThicknessProperty);
            set => this.SetValue(HistogramAxisThicknessProperty, value);
        }

        public static readonly DependencyProperty HistogramAxisBrushProperty =
              DependencyProperty.Register(nameof(HistogramAxisBrush), typeof(SolidColorBrush), typeof(TableView),
                  new FrameworkPropertyMetadata(Brushes.Gray,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public SolidColorBrush HistogramAxisBrush
        {
            get => (SolidColorBrush)this.GetValue(HistogramAxisBrushProperty);
            set => this.SetValue(HistogramAxisBrushProperty, value);
        }

        public static readonly DependencyProperty HistogramBarBorderThicknessProperty =
              DependencyProperty.Register(nameof(HistogramBarBorderThickness), typeof(double), typeof(TableView),
                  new FrameworkPropertyMetadata(1.0,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public double HistogramBarBorderThickness
        {
            get => (double)this.GetValue(HistogramBarBorderThicknessProperty);
            set => this.SetValue(HistogramBarBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty HistogramBarBorderBrushProperty =
              DependencyProperty.Register(nameof(HistogramBarBorderBrush), typeof(SolidColorBrush), typeof(TableView),
                  new FrameworkPropertyMetadata(Brushes.Black,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public SolidColorBrush HistogramBarBorderBrush
        {
            get => (SolidColorBrush)this.GetValue(HistogramBarBorderBrushProperty);
            set => this.SetValue(HistogramBarBorderBrushProperty, value);
        }

        public static readonly DependencyProperty HistogramPositiveBarFillBrushProperty =
              DependencyProperty.Register(nameof(HistogramPositiveBarFillBrush), typeof(SolidColorBrush), typeof(TableView),
                  new FrameworkPropertyMetadata(Brushes.LightBlue,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public SolidColorBrush HistogramPositiveBarFillBrush
        {
            get => (SolidColorBrush)this.GetValue(HistogramPositiveBarFillBrushProperty);
            set => this.SetValue(HistogramPositiveBarFillBrushProperty, value);
        }

        public static readonly DependencyProperty HistogramNegativeBarFillBrushProperty =
              DependencyProperty.Register(nameof(HistogramNegativeBarFillBrush), typeof(SolidColorBrush), typeof(TableView),
                  new FrameworkPropertyMetadata(Brushes.LightCoral,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public SolidColorBrush HistogramNegativeBarFillBrush
        {
            get => (SolidColorBrush)this.GetValue(HistogramNegativeBarFillBrushProperty);
            set => this.SetValue(HistogramNegativeBarFillBrushProperty, value);
        }

        #endregion Dependency Properties
    }
}
