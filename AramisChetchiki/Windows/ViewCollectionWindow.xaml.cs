using System;
using System.Windows;

namespace TMP.WORK.AramisChetchiki
{
    using ViewModel;

    /// <summary>
    /// Interaction logic for ViewCollectionWindow.xaml
    /// </summary>
    public partial class ViewCollectionWindow : Window
    {
        private ViewCollectionViewModel _vm;

        #region Constructors

        public ViewCollectionWindow(ViewCollectionViewModel vm)
        {
            if (vm == null)
                throw new ArgumentNullException("ViewCollectionViewModel");
            _vm = vm;

            InitializeComponent();
            DataContext = _vm;
        }

        #endregion
    }
}
