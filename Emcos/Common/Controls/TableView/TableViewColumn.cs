using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;


namespace TMP.Wpf.Common.Controls.TableView
{
    public class TableViewColumn : ContentControl, INotifyPropertyChanged
    {
        public TableView ParentTableView { get; internal set; }
        public int ColumnIndex { get { return (ParentTableView == null) ? -1 : ParentTableView.Columns.IndexOf(this); } }

        private const string SHOW_HISTORAMM_TEXT = "Показать гистограмму";
        private const string HIDE_HISTORAMM_TEXT = "Скрыть гистограмму";

        #region Dependency Properties

        public enum ColumnSortDirection { None, Up, Down };

        public static readonly DependencyProperty SortDirectionProperty =
          DependencyProperty.Register("SortDirection", typeof(ColumnSortDirection), typeof(TableViewColumn), new FrameworkPropertyMetadata(ColumnSortDirection.None, new PropertyChangedCallback(OnSortDirectionPropertyChanged)));

        public ColumnSortDirection SortDirection
        {
            get { return (ColumnSortDirection)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }

        private static void OnSortDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TableViewColumn)d).NotifySortingChanged();
        }

        public static readonly DependencyProperty SortOrderProperty =
          DependencyProperty.Register("SortOrder", typeof(int), typeof(TableViewColumn), new FrameworkPropertyMetadata(0));

        public int SortOrder
        {
            get { return (int)GetValue(SortOrderProperty); }
            set { SetValue(SortOrderProperty, value); }
        }

        public static readonly DependencyProperty CellTemplateProperty =
          DependencyProperty.Register("CellTemplate", typeof(DataTemplate), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public DataTemplate CellTemplate
        {
            get { return (DataTemplate)GetValue(CellTemplateProperty); }
            set { SetValue(CellTemplateProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
          DependencyProperty.Register("Title", typeof(object), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleTemplateProperty =
          DependencyProperty.Register("TitleTemplate", typeof(DataTemplate), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public DataTemplate TitleTemplate
        {
            get { return (DataTemplate)GetValue(TitleTemplateProperty); }
            set { SetValue(TitleTemplateProperty, value); }
        }

        public static readonly DependencyProperty ContextBindingPathProperty =
          DependencyProperty.Register("ContextBindingPath", typeof(string), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public string ContextBindingPath
        {
            get { return (string)GetValue(ContextBindingPathProperty); }
            set { SetValue(ContextBindingPathProperty, value); }
        }

        public static readonly DependencyProperty TotalInfoProperty =
            DependencyProperty.Register("TotalInfo", typeof(TableViewColumnTotal), typeof(TableViewColumn), new FrameworkPropertyMetadata(null));

        public TableViewColumnTotal TotalInfo
        {
            get { return (TableViewColumnTotal)GetValue(TotalInfoProperty); }
            set { SetValue(TotalInfoProperty, value); }
        }

        //------------------------
        public static readonly DependencyProperty ShowHistogrammProperty =
            DependencyProperty.Register("ShowHistogramm", typeof(bool), typeof(TableView), new FrameworkPropertyMetadata(false));

        public bool ShowHistogramm
        {
            get { return (bool)GetValue(ShowHistogrammProperty); }
            set
            {
                SetValue(ShowHistogrammProperty, value);
                RaisePropertyChanged("ShowHistogramm");
            }
        }
        //------------------------
        public static readonly DependencyProperty UseHistogrammProperty =
            DependencyProperty.Register("UseHistogramm", typeof(bool), typeof(TableView), new FrameworkPropertyMetadata(false));

        public bool UseHistogramm
        {
            get { return (bool)GetValue(UseHistogrammProperty); }
            set
            {
                SetValue(UseHistogrammProperty, value);
                RaisePropertyChanged("UseHistogramm");
            }
        }

        #endregion Dependency Properties

        public Binding WidthBinding { get; private set; }
        public Binding ContextBinding { get; set; }

        internal void NotifySortingChanged()
        {
            if (ParentTableView != null)
                ParentTableView.NotifySortingChanged(this);
        }

        internal void AdjustWidth(double width)
        { 
            if (width < 0)
                width = 0;

            Width = width;  // adjust the width of this control

            if (ParentTableView != null)
                ParentTableView.NotifyColumnWidthChanged(this); // let the table view know that this has changed
        }

        public void GenerateCellContent(TableViewCell cell)
        {
            cell.ContentTemplate = CellTemplate;
            cell.HorizontalContentAlignment = HorizontalContentAlignment;

            if (ContextBinding == null && ContextBindingPath != null)
                ContextBinding = new Binding(ContextBindingPath);

            if (ContextBinding != null)
                BindingOperations.SetBinding(cell, DataContextProperty, ContextBinding);
        }

        public void FocusColumn()
        {
            if (ParentTableView != null)
                ParentTableView.FocusedColumnChanged(this);
        }

        public TableViewColumn()
          : base()
        {
            Width = 100;
            HorizontalContentAlignment = HorizontalAlignment.Stretch;

            WidthBinding = new Binding("Width");
            WidthBinding.Mode = BindingMode.OneWay;
            WidthBinding.Source = this;
        }

        public void InvalidateCells()
        {
            var e = PropertyChanged;
            if (e != null)
            {
                e(this, new PropertyChangedEventArgs("UI"));
            }
        }

        #region INotifyPropertyChanged Members

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            checkIfPropertyNameExists(propertyName);

            var e = PropertyChanged;
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
                RaisePropertyChanged(name);
                return true;
            }
            return false;
        }

        [Conditional("DEBUG")]
        private void checkIfPropertyNameExists(String propertyName)
        {
            Type type = this.GetType();
            Debug.Assert(
              type.GetProperty(propertyName) != null,
              propertyName + "property does not exist on object of type : " + type.FullName);
        }

        #endregion
    }
}