using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMP.Work.Emcos.Controls.VTreeView
{
    /// <summary>
    /// Interaction logic for VTreeView.xaml
    /// </summary>
    public partial class VTreeView : ListBox
    {
        public VTreeView()
        {
            InitializeComponent();

            this.MouseDoubleClick += new MouseButtonEventHandler(VTreeView_MouseDoubleClick);
            _data = new TreeData();
            this.ItemsSource = _data.Items;
            SelectionMode = SelectionMode.Extended;
        }

        static VTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VTreeView), new FrameworkPropertyMetadata(typeof(VTreeView)));
        }

        private TreeData _data;

        public TreeData Data
        {
            get { return _data; }
        }

        private bool doubleClickExpand = true;
        public bool DoubleClickExpand
        {
            get { return doubleClickExpand; }
            set { doubleClickExpand = value; }
        }


        void VTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClickExpand)
            {
                var fe = e.OriginalSource as FrameworkElement;
                if (fe != null)
                {
                    var TN = (TreeNode)fe.DataContext;
                    if (TN != null)
                    {
                        TN.IsExpanded = !TN.IsExpanded;
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
