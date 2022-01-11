namespace TMP.WORK.AramisChetchiki.Controls
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for DataGridControl.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "ToDo")]
    public partial class DataGridControl : UserControl
    {
        private const string ElementRowsItemsPresenterLabel = "PART_RowsPresenter";
        private ItemsPresenter rowsItemsPresenter;

        public DataGridControl()
        {
            this.InitializeComponent();
        }

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

            if (dataGridControl.filterDataGrid != null && dataGridControl.filterDataGrid.ItemsSource == null)
            {
                dataGridControl.filterDataGrid.ItemsSource = newValue;
            }
        }

        public Style DataRowStyle
        {
            get { return (Style)GetValue(DataRowStyleProperty); }
            set { SetValue(DataRowStyleProperty, value); }
        }

        public static readonly DependencyProperty DataRowStyleProperty =
            DependencyProperty.Register(nameof(DataRowStyle), typeof(Style), typeof(DataGridControl), new PropertyMetadata(default, new PropertyChangedCallback(OnRowStyleChanged)));

        private static void OnRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGridControl dataGridControl = (DataGridControl)d;
            Style oldValue = (Style)e.OldValue;
            Style newValue = (Style)e.NewValue;

            if (dataGridControl.filterDataGrid != null && dataGridControl.filterDataGrid.RowStyle == null)
            {
                dataGridControl.filterDataGrid.RowStyle = newValue;
            }
        }

        public string NoItemsMessage
        {
            get => (string)this.GetValue(NoItemsMessageProperty);
            set => this.SetValue(NoItemsMessageProperty, value);
        }

        // Using a DependencyProperty as the backing store for NoItemsDataTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoItemsMessageProperty =
            DependencyProperty.Register(nameof(NoItemsMessage), typeof(string), typeof(DataGridControl), new PropertyMetadata(default));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.rowsItemsPresenter = this.EnforceInstance<ItemsPresenter>(ElementRowsItemsPresenterLabel);

            ContextMenu contextMenu = (ContextMenu)this.TryFindResource("dataGridContextMenuKey");

            if (this.rowsItemsPresenter != null)
            {
                System.Collections.Generic.List<Control> menuItems = new ();
                this.GenerateRowsContextMenu(ref menuItems);
                menuItems.Add(new Separator());
                var selectColumnsMenuItem = new MenuItem()
                {
                    Header = "Отображаемые столбцы таблицы",
                    Tag = "selectColumnsMenuItem",

                    // IsEnabled = filterDataGrid.Columns.Count > 0 && filterDataGrid.ColumnsVisibilitySelectMenuItemList != null
                };
                selectColumnsMenuItem.Items.Add(new MenuItem() { Header = "пусто" });
                selectColumnsMenuItem.Loaded += this.SelectColumnsMenuItem_Loaded;
                menuItems.Add(selectColumnsMenuItem);

                System.Windows.Data.CompositeCollection cc = new ();

                if (this.rowsItemsPresenter.ContextMenu != null)
                {
                    System.Windows.Data.CollectionContainer c = new ();
                    c.SetCurrentValue(System.Windows.Data.CollectionContainer.CollectionProperty, this.rowsItemsPresenter.ContextMenu.Items);
                    cc.Add(c);
                }
                else
                {
                    this.rowsItemsPresenter.SetCurrentValue(ContextMenuProperty, new ContextMenu());
                }

                if (contextMenu != null)
                {
                    System.Windows.Data.CollectionContainer c = new ();
                    c.SetCurrentValue(System.Windows.Data.CollectionContainer.CollectionProperty, contextMenu.Items);
                    cc.Add(c);
                }

                System.Windows.Data.CollectionContainer newCollection = new ();
                menuItems.Add(new Separator());
                newCollection.SetCurrentValue(System.Windows.Data.CollectionContainer.CollectionProperty, menuItems);
                cc.Add(newCollection);

                this.rowsItemsPresenter.ContextMenu.SetCurrentValue(ItemsControl.ItemsSourceProperty, cc);
            }
        }

        private void SelectColumnsMenuItem_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ItemsPresenterContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ContextMenu contextMenu = (sender as FrameworkElement)?.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            MenuItem menuItem = null;
            foreach (var item in contextMenu.Items)
            {
                if (item is MenuItem menu && menu.Tag != null && Equals(menu.Tag, "selectColumnsMenuItem"))
                {
                    menuItem = menu;
                    break;
                }
            }

            if (menuItem == null)
            {
                return;
            }

            menuItem.Items.Clear();
            if (this.filterDataGrid.ColumnsVisibilitySelectMenuItemList != null)
            {
                foreach (var item in this.filterDataGrid.ColumnsVisibilitySelectMenuItemList)
                {
                    menuItem.Items.Add(item);
                }
            }
        }

        private void GenerateRowsContextMenu(ref System.Collections.Generic.List<Control> menuItems)
        {
            MenuItem menuItem1 = new ()
            {
                Header = "Режим копирования: включать заголовки столбцов",
                IsCheckable = true,
                IsChecked = true,
            };
            menuItem1.Click += this.GridCopyModeClick;
            MenuItem menuItem2 = new ()
            {
                Header = "Режим выделения",
            };
            MenuItem menuItem3 = new () { Header = "Строка" };
            menuItem3.Click += this.GridSelectModeRowClick;
            MenuItem menuItem4 = new () { Header = "Ячейка" };
            menuItem4.Click += this.GridSelectModeCellClick;
            menuItem2.Items.Add(menuItem3);
            menuItem2.Items.Add(menuItem4);

            menuItems.Add(menuItem1);
            menuItems.Add(menuItem2);
        }

        private void GridCopyModeClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            this.filterDataGrid.SetCurrentValue(DataGrid.ClipboardCopyModeProperty, menuItem.IsChecked ? DataGridClipboardCopyMode.IncludeHeader : DataGridClipboardCopyMode.ExcludeHeader);
        }

        private void GridSelectModeRowClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            this.filterDataGrid.SetCurrentValue(DataGrid.SelectionUnitProperty, DataGridSelectionUnit.FullRow);
            (menuItem.Parent as MenuItem).SetCurrentValue(HeaderedItemsControl.HeaderProperty, "Режим выделения: строка");
        }

        private void GridSelectModeCellClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            this.filterDataGrid.SetCurrentValue(DataGrid.SelectionUnitProperty, DataGridSelectionUnit.Cell);
            (menuItem.Parent as MenuItem).SetCurrentValue(HeaderedItemsControl.HeaderProperty, "Режим выделения: ячейка");
        }

        // Get element from name. If it exist then element instance return, if not, new will be created
        private T EnforceInstance<T>(string partName)
            where T : FrameworkElement, new()
        {
            T element = this.GetTemplateChild(partName) as T ?? new T();
            return element;
        }
    }
}
