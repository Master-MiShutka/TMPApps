namespace TMP.UI.Controls.WPF.Reporting.MatrixGrid
{
    using System.Windows;

    /// <summary>
    /// Exposes two dependency properties which are bound to in
    /// order to know when the visual children of a MatrixGrid are
    /// given new values for the Grid.Row and Grid.Column properties.
    /// </summary>
    internal class MatrixGridChildMonitor : DependencyObject
    {
        #region GridRow

        public int GridRow
        {
            get => (int)this.GetValue(GridRowProperty);
            set => this.SetValue(GridRowProperty, value);
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
            get => (int)this.GetValue(GridColumnProperty);
            set => this.SetValue(GridColumnProperty, value);
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
            get => (int)this.GetValue(GridRowSpanProperty);
            set => this.SetValue(GridRowSpanProperty, value);
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
            get => (int)this.GetValue(GridColumnSpanProperty);
            set => this.SetValue(GridColumnSpanProperty, value);
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
