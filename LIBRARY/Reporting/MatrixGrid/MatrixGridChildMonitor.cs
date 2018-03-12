using System.Windows;

namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    /// <summary>
    /// Exposes two dependency properties which are bound to in
    /// order to know when the visual children of a MatrixGrid are
    /// given new values for the Grid.Row and Grid.Column properties.
    /// </summary>
    class MatrixGridChildMonitor : DependencyObject
    {
        #region GridRow

        public int GridRow
        {
            get { return (int)GetValue(GridRowProperty); }
            set { SetValue(GridRowProperty, value); }
        }

        public static readonly DependencyProperty GridRowProperty =
            DependencyProperty.Register(
            "GridRow",
            typeof(int),
            typeof(MatrixGridChildMonitor),
            new UIPropertyMetadata(0));

        #endregion // GridRow

        #region GridColumn

        public int GridColumn
        {
            get { return (int)GetValue(GridColumnProperty); }
            set { SetValue(GridColumnProperty, value); }
        }

        public static readonly DependencyProperty GridColumnProperty =
            DependencyProperty.Register(
            "GridColumn",
            typeof(int),
            typeof(MatrixGridChildMonitor),
            new UIPropertyMetadata(0));

        #endregion // GridColumn

        #region GridRowSpan

        public int GridRowSpan
        {
            get { return (int)GetValue(GridRowSpanProperty); }
            set { SetValue(GridRowSpanProperty, value); }
        }

        public static readonly DependencyProperty GridRowSpanProperty =
            DependencyProperty.Register(
            "GridRowSpan",
            typeof(int),
            typeof(MatrixGridChildMonitor),
            new UIPropertyMetadata(1));

        #endregion // GridRowSpan

        #region GridColumnSpan

        public int GridColumnSpan
        {
            get { return (int)GetValue(GridColumnSpanProperty); }
            set { SetValue(GridColumnSpanProperty, value); }
        }

        public static readonly DependencyProperty GridColumnSpanProperty =
            DependencyProperty.Register(
            "GridColumnSpan",
            typeof(int),
            typeof(MatrixGridChildMonitor),
            new UIPropertyMetadata(1));

        #endregion // GridColumnSpan
    }
}
