using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using TMP.Shared.Commands;

namespace TMP.Wpf.CommonControls
{


    /// <summary>
    /// Interaction logic for PagedDataGrid.xaml
    /// </summary>
    public partial class PagedDataGrid : UserControl
    {
        #region Fields

        private CollectionView _view;

        #endregion Fields

        public PagedDataGrid()
        {
            InitializeComponent();

            DataContext = this;
        }

        #region Properties

        public CollectionView View
        {
            get { return _view; }
            set
            {
                SetProperty(ref _view, value);
            }
        }

        #endregion Properties

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}