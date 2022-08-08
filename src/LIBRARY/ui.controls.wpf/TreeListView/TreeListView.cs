namespace TMP.UI.WPF.Controls.TreeListView
{
    using System.Windows;
    using System.Windows.Controls;

    public class TreeListView : TreeView
    {
        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
        }

        public TreeListView() { }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem(this);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        public GridViewColumnCollection Columns
        {
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(GridViewColumnCollection), typeof(TreeListView), new PropertyMetadata(default));

        #region ColumnHeaderContainerStyle

        /// <summary>
        /// ColumnHeaderContainerStyleProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderContainerStyleProperty =
                DependencyProperty.Register(
                    nameof(ColumnHeaderContainerStyle),
                    typeof(Style),
                    typeof(TreeListView)
                );

        /// <summary>
        /// header container's style
        /// </summary>
        public Style ColumnHeaderContainerStyle
        {
            get { return (Style)GetValue(ColumnHeaderContainerStyleProperty); }
            set { SetValue(ColumnHeaderContainerStyleProperty, value); }
        }

        #endregion // ColumnHeaderContainerStyle

        #region ColumnHeaderTemplate

        /// <summary>
        /// ColumnHeaderTemplate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderTemplateProperty =
            DependencyProperty.Register(
                nameof(ColumnHeaderTemplate),
                typeof(DataTemplate),
                typeof(TreeListView),
                new FrameworkPropertyMetadata()
            );


        /// <summary>
        /// column header template
        /// </summary>
        public DataTemplate ColumnHeaderTemplate
        {
            get { return (DataTemplate)GetValue(ColumnHeaderTemplateProperty); }
            set { SetValue(ColumnHeaderTemplateProperty, value); }
        }

        #endregion  ColumnHeaderTemplate

        #region ColumnHeaderTemplateSelector

        /// <summary>
        /// ColumnHeaderTemplateSelector DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty =
            DependencyProperty.Register(
                nameof(ColumnHeaderTemplateSelector),
                typeof(DataTemplateSelector),
                typeof(TreeListView),
                new FrameworkPropertyMetadata()
            );


        /// <summary>
        /// header template selector
        /// </summary>
        /// <remarks>
        ///     This property is ignored if <seealso cref="ColumnHeaderTemplate"/> is set.
        /// </remarks>
        public DataTemplateSelector ColumnHeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ColumnHeaderTemplateSelectorProperty); }
            set { SetValue(ColumnHeaderTemplateSelectorProperty, value); }
        }

        #endregion ColumnHeaderTemplateSelector

        #region ColumnHeaderStringFormat

        /// <summary>
        /// ColumnHeaderStringFormat DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderStringFormatProperty =
            DependencyProperty.Register(
                nameof(ColumnHeaderStringFormat),
                typeof(string),
                typeof(TreeListView)
            );


        /// <summary>
        /// column header string format
        /// </summary>
        public string ColumnHeaderStringFormat
        {
            get { return (string)GetValue(ColumnHeaderStringFormatProperty); }
            set { SetValue(ColumnHeaderStringFormatProperty, value); }
        }

        #endregion  ColumnHeaderStringFormat

        #region AllowsColumnReorder

        /// <summary>
        /// AllowsColumnReorderProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty AllowsColumnReorderProperty =
                DependencyProperty.Register(
                    nameof(AllowsColumnReorder),
                    typeof(bool),
                    typeof(TreeListView),
                    new FrameworkPropertyMetadata(true)
                );

        /// <summary>
        /// AllowsColumnReorder
        /// </summary>
        public bool AllowsColumnReorder
        {
            get { return (bool)GetValue(AllowsColumnReorderProperty); }
            set { SetValue(AllowsColumnReorderProperty, value); }
        }

        #endregion AllowsColumnReorder

        #region ColumnHeaderContextMenu

        /// <summary>
        /// ColumnHeaderContextMenuProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderContextMenuProperty =
                DependencyProperty.Register(
                    nameof(ColumnHeaderContextMenu),
                    typeof(ContextMenu),
                    typeof(TreeListView)
                );

        /// <summary>
        /// ColumnHeaderContextMenu
        /// </summary>
        public ContextMenu ColumnHeaderContextMenu
        {
            get { return (ContextMenu)GetValue(ColumnHeaderContextMenuProperty); }
            set { SetValue(ColumnHeaderContextMenuProperty, value); }
        }

        #endregion ColumnHeaderContextMenu

        #region ColumnHeaderToolTip

        /// <summary>
        /// ColumnHeaderToolTipProperty DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderToolTipProperty =
                DependencyProperty.Register(
                    nameof(ColumnHeaderToolTip),
                    typeof(object),
                    typeof(TreeListView)
                );

        /// <summary>
        /// ColumnHeaderToolTip
        /// </summary>
        public object ColumnHeaderToolTip
        {
            get { return GetValue(ColumnHeaderToolTipProperty); }
            set { SetValue(ColumnHeaderToolTipProperty, value); }
        }

        #endregion ColumnHeaderToolTip
    }
}
