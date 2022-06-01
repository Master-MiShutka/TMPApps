using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TMP.WORK.AramisChetchiki.Controls
{
    /// <summary>
    /// Interaction logic for DataGridControl.xaml
    /// </summary>
    public partial class DataGridControl : UserControl
    {
        public DataGridControl()
        {
            InitializeComponent();
        }

        public string NoItemsMessage
        {
            get => (string)this.GetValue(NoItemsMessageProperty);
            set => this.SetValue(NoItemsMessageProperty, value);
        }

        public static readonly DependencyProperty NoItemsMessageProperty =
            DependencyProperty.Register(nameof(NoItemsMessage), typeof(string), typeof(DataGridControl), new PropertyMetadata(default));

        public Style RowStyle
        {
            get { return (Style)this.GetValue(RowStyleProperty); }
            set { this.SetValue(RowStyleProperty, value); }
        }

        /// <summary>Identifies the <see cref="RowStyle"/> dependency property.</summary>
        public static readonly DependencyProperty RowStyleProperty =
            DependencyProperty.Register(nameof(RowStyle), typeof(Style), typeof(DataGridControl), new PropertyMetadata(default));

        public bool AutoGenerateColumns
        {
            get { return (bool)this.GetValue(AutoGenerateColumnsProperty); }
            set { this.SetValue(AutoGenerateColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoGenerateColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            DependencyProperty.Register(nameof(AutoGenerateColumns), typeof(bool), typeof(DataGridControl), new PropertyMetadata(false));

        [Bindable(true)]
        public System.Collections.IEnumerable ItemsSource
        {
            get => (System.Collections.IEnumerable)this.GetValue(ItemsSourceProperty);
            set
            {
                if (value == null)
                {
                    this.ClearValue(ItemsSourceProperty);
                }
                else
                {
                    this.SetValue(ItemsSourceProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(System.Collections.IEnumerable),
                typeof(DataGridControl),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged)));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridControl dataGridControl = (DataGridControl)d;
            System.Collections.IEnumerable oldValue = (System.Collections.IEnumerable)e.OldValue;
            System.Collections.IEnumerable newValue = (System.Collections.IEnumerable)e.NewValue;
            if (dataGridControl.filterDataGrid != null)
            {
                dataGridControl.filterDataGrid.ItemsSource = newValue;
            }
        }
    }
}
