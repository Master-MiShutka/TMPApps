namespace TMP.UI.Controls.WPF.TableView
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;

    public class TableViewColumn : ContentControl, INotifyPropertyChanged
    {
        public TableView ParentTableView { get; internal set; }

        public int ColumnIndex => (this.ParentTableView == null) ? -1 : this.ParentTableView.Columns.IndexOf(this);

        private const string SHOW_HISTORAMM_TEXT = "Показать гистограмму";
        private const string HIDE_HISTORAMM_TEXT = "Скрыть гистограмму";

        #region Dependency Properties

        public enum ColumnSortDirection
        {
            None, Up, Down,
        }

        public static readonly DependencyProperty SortDirectionProperty =
          DependencyProperty.Register(nameof(SortDirection), typeof(ColumnSortDirection), typeof(TableViewColumn), new FrameworkPropertyMetadata(ColumnSortDirection.None, new PropertyChangedCallback(OnSortDirectionChanged)));

        public ColumnSortDirection SortDirection
        {
            get => (ColumnSortDirection)this.GetValue(SortDirectionProperty);
            set => this.SetValue(SortDirectionProperty, value);
        }

        private static void OnSortDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TableViewColumn)d).NotifySortingChanged();
        }

        public static readonly DependencyProperty SortOrderProperty =
          DependencyProperty.Register(nameof(SortOrder), typeof(int), typeof(TableViewColumn), new FrameworkPropertyMetadata(0));

        public int SortOrder
        {
            get => (int)this.GetValue(SortOrderProperty);
            set => this.SetValue(SortOrderProperty, value);
        }

        public static readonly DependencyProperty CellTemplateProperty =
          DependencyProperty.Register(nameof(CellTemplate), typeof(DataTemplate), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public DataTemplate CellTemplate
        {
            get => (DataTemplate)this.GetValue(CellTemplateProperty);
            set => this.SetValue(CellTemplateProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
          DependencyProperty.Register(nameof(Title), typeof(object), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public object Title
        {
            get => this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleTemplateProperty =
          DependencyProperty.Register(nameof(TitleTemplate), typeof(DataTemplate), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public DataTemplate TitleTemplate
        {
            get => (DataTemplate)this.GetValue(TitleTemplateProperty);
            set => this.SetValue(TitleTemplateProperty, value);
        }

        public static readonly DependencyProperty ContextBindingPathProperty =
          DependencyProperty.Register(nameof(ContextBindingPath), typeof(string), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public string ContextBindingPath
        {
            get => (string)this.GetValue(ContextBindingPathProperty);
            set => this.SetValue(ContextBindingPathProperty, value);
        }

        public static readonly DependencyProperty TotalInfoProperty =
            DependencyProperty.Register(nameof(TotalInfo), typeof(TableViewColumnTotal), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public TableViewColumnTotal TotalInfo
        {
            get => (TableViewColumnTotal)this.GetValue(TotalInfoProperty);
            set => this.SetValue(TotalInfoProperty, value);
        }

        //------------------------
        public static readonly DependencyProperty ShowHistogrammProperty =
            DependencyProperty.Register(nameof(ShowHistogramm), typeof(bool), typeof(TableViewColumn), new FrameworkPropertyMetadata(false));

        public bool ShowHistogramm
        {
            get => (bool)this.GetValue(ShowHistogrammProperty);
            set => this.SetValue(ShowHistogrammProperty, value);
        }

        //------------------------
        public static readonly DependencyProperty UseHistogrammProperty =
            DependencyProperty.Register(nameof(UseHistogramm), typeof(bool), typeof(TableViewColumn), new FrameworkPropertyMetadata(false));

        public bool UseHistogramm
        {
            get => (bool)this.GetValue(UseHistogrammProperty);
            set => this.SetValue(UseHistogrammProperty, value);
        }

        #endregion Dependency Properties

        public Binding WidthBinding { get; private set; }

        public Binding ContextBinding { get; set; }

        internal void NotifySortingChanged()
        {
            if (this.ParentTableView != null)
            {
                this.ParentTableView.NotifySortingChanged(this);
            }
        }

        internal void AdjustWidth(double width)
        {
            if (width < 0)
            {
                width = 0;
            }

            this.Width = width;  // adjust the width of this control

            if (this.ParentTableView != null)
            {
                this.ParentTableView.NotifyColumnWidthChanged(this); // let the table view know that this has changed
            }
        }

        public void GenerateCellContent(TableViewCell cell)
        {
            cell.ContentTemplate = this.CellTemplate;
            cell.HorizontalContentAlignment = this.HorizontalContentAlignment;

            if (this.ContextBinding == null && this.ContextBindingPath != null)
            {
                this.ContextBinding = new Binding(this.ContextBindingPath);
            }

            if (this.ContextBinding != null)
            {
                BindingOperations.SetBinding(cell, DataContextProperty, this.ContextBinding);
            }
        }

        public void FocusColumn()
        {
            if (this.ParentTableView != null)
            {
                this.ParentTableView.FocusedColumnChanged(this);
            }
        }

        public TableViewColumn()
          : base()
        {
            this.Width = 100;
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;

            this.WidthBinding = new Binding("Width");
            this.WidthBinding.Mode = BindingMode.OneWay;
            this.WidthBinding.Source = this;
        }

        public void InvalidateCells()
        {
            var e = this.PropertyChanged;
            if (e != null)
            {
                e(this, new PropertyChangedEventArgs("UI"));
            }
        }

        #region INotifyPropertyChanged Members

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            this.checkIfPropertyNameExists(propertyName);

            var e = this.PropertyChanged;
            if (e != null)
            {
                e(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProp<T>(ref T variable, T value, string name)
        {
            if (!EqualityComparer<T>.Default.Equals(variable, value))
            {
                variable = value;
                this.RaisePropertyChanged(name);
                return true;
            }

            return false;
        }

        [Conditional("DEBUG")]
        private void checkIfPropertyNameExists(string propertyName)
        {
            Type type = this.GetType();
            Debug.Assert(
              type.GetProperty(propertyName) != null,
              propertyName + "property does not exist on object of type : " + type.FullName);
        }

        #endregion
    }
}