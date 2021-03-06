﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TMP.Wpf.Common.Controls.TableView
{
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

        private Rect _fixedClipRect = Rect.Empty;

        internal Rect FixedClipRect
        {
            get
            {
                if (_fixedClipRect == Rect.Empty)
                {
                    double width = 0.0;
                    if (Columns.Count >= FixedColumnCount)
                        for (int i = 0; i < FixedColumnCount; ++i)
                            width += Columns[i].Width;

                    _fixedClipRect = new Rect(HorizontalScrollOffset, 0, width, 0);
                }
                return _fixedClipRect;
            }
        }

        internal void ResetFixedClipRect()
        {
            _fixedClipRect = Rect.Empty;
        }

        public double HeaderHeight { get { return HeaderRowPresenter == null ? 0 : HeaderRowPresenter.Height; } }

        public ItemCollection Items
        {
            get
            {
                if (this.RowsPresenter != null)
                    return this.RowsPresenter.Items;
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
            ResetFixedClipRect();

            Columns = new ObservableCollection<TableViewColumn>();
            //SnapsToDevicePixels = true;
            //UseLayoutRounding = true;
            //Columns.CollectionChanged += ColumnsChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            HeaderRowPresenter = GetTemplateChild("PART_HeaderPresenter") as TableViewHeaderPresenter;
            RowsPresenter = GetTemplateChild("PART_RowsPresenter") as TableViewRowsPresenter;
        }

        internal void NotifyColumnWidthChanged(TableViewColumn column)
        {
            if (ColumnWidthChanged != null)
                ColumnWidthChanged(this, new TableViewColumnEventArgs(column));
        }

        internal void NotifySortingChanged(TableViewColumn column)
        {
            if (SortingChanged != null)
                SortingChanged(this, new TableViewColumnEventArgs(column));
        }

        private void ColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (var item in e.NewItems)
                    (item as TableViewColumn).ParentTableView = this;

            /*foreach (var col in Columns)
              col.ParentTableView = this;*/

            if (HeaderRowPresenter != null)
            {
                ResetFixedClipRect();
                HeaderRowPresenter.HeaderInvalidateArrange();
            }

            if (RowsPresenter != null)
                RowsPresenter.ColumnsChanged();
        }

        private void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (HeaderRowPresenter != null)
            {
                ResetFixedClipRect();
                HeaderRowPresenter.HeaderInvalidateArrange();
            }

            if (RowsPresenter != null)
                RowsPresenter.RowsInvalidateArrange();
        }

        internal int IndexOfRow(TableViewCellsPresenter cp)
        {
            if (RowsPresenter != null)
                return RowsPresenter.ItemContainerGenerator.IndexFromContainer(cp);
            return -1;
        }

        internal void FocusedRowChanged(TableViewCellsPresenter cp)
        {
            FocusedRowIndex = IndexOfRow(cp);
            SelectedRowIndex = FocusedRowIndex;
        }

        internal void FocusedColumnChanged(TableViewColumn col)
        {
            FocusedColumnIndex = Columns.IndexOf(col);
            SelectedColumnIndex = FocusedColumnIndex;
        }

        public TableViewColumnHeader GetColumnHeaderAtLocation(Point loc)
        {
            if (HeaderRowPresenter != null)
                return HeaderRowPresenter.GetColumnHeaderAtLocation(loc);
            return null;
        }

        public TableViewColumn GetColumnAtLocation(Point loc)
        {
            var ch = GetColumnHeaderAtLocation(loc);
            if (ch != null)
                return Columns[ch.ColumnIndex];

            return null;
        }

        public object GetItemAtLocation(Point loc)
        {
            loc.Y -= HeaderRowPresenter.RenderSize.Height;
            if (RowsPresenter != null)
                return RowsPresenter.GetItemAtLocation(loc);
            return null;
        }

        public int GetCellIndexAtLocation(Point loc)
        {
            loc.Y -= HeaderRowPresenter.RenderSize.Height;
            if (RowsPresenter != null)
                return RowsPresenter.GetCellIndexAtLocation(loc);
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
                    if (c.ShowHistogramm) c.InvalidateCells();
            }
        }

        //------------------------
        public static readonly DependencyProperty CellNavigationProperty =
            DependencyProperty.Register("CellNavigation", typeof(bool), typeof(TableView), new PropertyMetadata(true));

        public bool CellNavigation
        {
            get { return (bool)GetValue(CellNavigationProperty); }
            set { SetValue(CellNavigationProperty, value); }
        }

        //------------------------
        public static readonly DependencyProperty ShowVerticalGridLinesProperty =
            DependencyProperty.Register("ShowVerticalGridLines", typeof(bool), typeof(TableView), new PropertyMetadata(true));

        public bool ShowVerticalGridLines
        {
            get { return (bool)GetValue(ShowVerticalGridLinesProperty); }
            set { SetValue(ShowVerticalGridLinesProperty, value); }
        }

        //------------------------
        public static readonly DependencyProperty ShowHorizontalGridLinesProperty =
            DependencyProperty.Register("ShowHorizontalGridLines", typeof(bool), typeof(TableView), new PropertyMetadata(false));

        public bool ShowHorizontalGridLines
        {
            get { return (bool)GetValue(ShowHorizontalGridLinesProperty); }
            set { SetValue(ShowHorizontalGridLinesProperty, value); }
        }

        //---------------
        public static readonly DependencyProperty ItemsSourceProperty =
           DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TableView), new FrameworkPropertyMetadata(null, OnItemsSourcePropertyChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tv = d as TableView;
            if (tv != null)
            {
                if (tv.RowsPresenter != null)
                    tv.RowsPresenter.ItemsSource = e.NewValue as IEnumerable;
            }
        }

        //--------------
        public static readonly DependencyProperty SelectedRowIndexProperty =
            DependencyProperty.Register("SelectedRowIndex", typeof(object), typeof(TableView), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int SelectedRowIndex
        {
            get { return (int)GetValue(SelectedRowIndexProperty); }
            set { SetValue(SelectedRowIndexProperty, value); }
        }

        //--------------
        public static readonly DependencyProperty SelectedColumnIndexProperty =
            DependencyProperty.Register("SelectedColumnIndex", typeof(int), typeof(TableView), new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int SelectedColumnIndex
        {
            get { return (int)GetValue(SelectedColumnIndexProperty); }
            set { SetValue(SelectedColumnIndexProperty, value); }
        }

        //--------------
        public static readonly DependencyProperty FocusedRowIndexProperty =
            DependencyProperty.Register("FocusedRowIndex", typeof(object), typeof(TableView), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int FocusedRowIndex
        {
            get { return (int)GetValue(FocusedRowIndexProperty); }
            set { SetValue(FocusedRowIndexProperty, value); }
        }

        //--------------
        public static readonly DependencyProperty FocusedColumnIndexProperty =
            DependencyProperty.Register("FocusedColumnIndex", typeof(int), typeof(TableView), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int FocusedColumnIndex
        {
            get { return (int)GetValue(FocusedColumnIndexProperty); }
            set { SetValue(FocusedColumnIndexProperty, value); }
        }

        //--------------
        public static readonly DependencyProperty FixedColumnCountProperty =
            DependencyProperty.Register("FixedColumnCount", typeof(int), typeof(TableView), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public int FixedColumnCount
        {
            get { return (int)GetValue(FixedColumnCountProperty); }
            set { SetValue(FixedColumnCountProperty, value); }
        }

        //--------------
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(ObservableCollection<TableViewColumn>), typeof(TableView), 
                new PropertyMetadata(null, new PropertyChangedCallback(TableView.OnColumnsPropertyChanged)));

        private static void OnColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tv = d as TableView;
            if (e.OldValue != null)
                ((ObservableCollection<TableViewColumn>)e.OldValue).CollectionChanged -= tv.ColumnsChanged;
            if (e.NewValue != null)
                ((ObservableCollection<TableViewColumn>)e.NewValue).CollectionChanged += tv.ColumnsChanged;
        }
        public ObservableCollection<TableViewColumn> Columns
        {
            get { return (ObservableCollection<TableViewColumn>)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
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
          DependencyProperty.Register("HorizontalScrollOffset", typeof(double), typeof(TableView), new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public double HorizontalScrollOffset
        {
            get { return (double)GetValue(HorizontalScrollOffsetProperty); }
            set { SetValue(HorizontalScrollOffsetProperty, value); }
        }

        //----------------------------
        public static readonly DependencyProperty CellsPanelStyleProperty =
          DependencyProperty.Register("CellsPanelStyle", typeof(Style), typeof(TableView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public Style CellsPanelStyle
        {
            get { return (Style)GetValue(CellsPanelStyleProperty); }
            set { SetValue(CellsPanelStyleProperty, value); }
        }

        //----------------------------
        public static readonly DependencyProperty RowsPanelStyleProperty =
          DependencyProperty.Register("RowsPanelStyle", typeof(Style), typeof(TableView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public Style RowsPanelStyle
        {
            get { return (Style)GetValue(RowsPanelStyleProperty); }
            set { SetValue(RowsPanelStyleProperty, value); }
        }

        //----------------------------
        public static readonly DependencyProperty HeaderPanelStyleProperty =
          DependencyProperty.Register("HeaderPanelStyle", typeof(Style), typeof(TableView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TableView.OnPropertyChanged)));

        public Style HeaderPanelStyle
        {
            get { return (Style)GetValue(HeaderPanelStyleProperty); }
            set { SetValue(HeaderPanelStyleProperty, value); }
        }

        //----------------------------
        public static readonly DependencyProperty GridLinesBrushProperty =
          DependencyProperty.Register("GridLinesBrush", typeof(Brush), typeof(TableView), new FrameworkPropertyMetadata(Brushes.DarkSlateGray));

        public Brush GridLinesBrush
        {
            get { return (Brush)GetValue(GridLinesBrushProperty); }
            set { SetValue(GridLinesBrushProperty, value); }
        }

        //----------------------------
        public static readonly DependencyProperty MarkBrushProperty =
                DependencyProperty.Register("MarkBrush", typeof(Brush), typeof(TableView), 
                    new FrameworkPropertyMetadata(Brushes.LightYellow));

        public Brush MarkBrush
        {
            get { return (Brush)GetValue(MarkBrushProperty); }
            set { SetValue(MarkBrushProperty, value); }
        }

        //----------------------------
        public static readonly DependencyProperty HistogramAxisThicknessProperty =
      DependencyProperty.Register("HistogramAxisThickness", typeof(double), typeof(TableView), 
          new FrameworkPropertyMetadata(1.0,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public double HistogramAxisThickness
        {
            get { return (double)GetValue(HistogramAxisThicknessProperty); }
            set { SetValue(HistogramAxisThicknessProperty, value); }
        }
        public static readonly DependencyProperty HistogramAxisBrushProperty =
              DependencyProperty.Register("HistogramAxisBrush", typeof(SolidColorBrush), typeof(TableView), 
                  new FrameworkPropertyMetadata(Brushes.Gray,
                      FrameworkPropertyMetadataOptions.AffectsRender,                      
                      TableView.OnUIPropertyChanged));

        public SolidColorBrush HistogramAxisBrush
        {
            get { return (SolidColorBrush)GetValue(HistogramAxisBrushProperty); }
            set { SetValue(HistogramAxisBrushProperty, value); }
        }
        public static readonly DependencyProperty HistogramBarBorderThicknessProperty =
              DependencyProperty.Register("HistogramBarBorderThickness", typeof(double), typeof(TableView), 
                  new FrameworkPropertyMetadata(1.0,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public double HistogramBarBorderThickness
        {
            get { return (double)GetValue(HistogramBarBorderThicknessProperty); }
            set { SetValue(HistogramBarBorderThicknessProperty, value); }
        }
        public static readonly DependencyProperty HistogramBarBorderBrushProperty =
              DependencyProperty.Register("HistogramBarBorderBrush", typeof(SolidColorBrush), typeof(TableView), 
                  new FrameworkPropertyMetadata(Brushes.Black,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public SolidColorBrush HistogramBarBorderBrush
        {
            get { return (SolidColorBrush)GetValue(HistogramBarBorderBrushProperty); }
            set { SetValue(HistogramBarBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty HistogramPositiveBarFillBrushProperty =
              DependencyProperty.Register("HistogramPositiveBarFillBrush", typeof(SolidColorBrush), typeof(TableView), 
                  new FrameworkPropertyMetadata(Brushes.LightBlue,
                      FrameworkPropertyMetadataOptions.AffectsRender,
                      TableView.OnUIPropertyChanged));

        public SolidColorBrush HistogramPositiveBarFillBrush
        {
            get { return (SolidColorBrush)GetValue(HistogramPositiveBarFillBrushProperty); }
            set { SetValue(HistogramPositiveBarFillBrushProperty, value); }
        }
        public static readonly DependencyProperty HistogramNegativeBarFillBrushProperty =
              DependencyProperty.Register("HistogramNegativeBarFillBrush", typeof(SolidColorBrush), typeof(TableView), 
                  new FrameworkPropertyMetadata(Brushes.LightCoral, 
                      FrameworkPropertyMetadataOptions.AffectsRender, 
                      TableView.OnUIPropertyChanged));

        public SolidColorBrush HistogramNegativeBarFillBrush
        {
            get { return (SolidColorBrush)GetValue(HistogramNegativeBarFillBrushProperty); }
            set { SetValue(HistogramNegativeBarFillBrushProperty, value); }
        }

        #endregion Dependency Properties
    }
}