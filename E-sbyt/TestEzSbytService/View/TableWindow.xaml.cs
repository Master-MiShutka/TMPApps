using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Data;


namespace TMP.Work.AmperM.TestApp.View
{
    using EzSbyt;
    using TMP.Shared.Commands;
    /// <summary>
    /// Interaction logic for TableWindow.xaml
    /// </summary>
    public partial class TableWindow : Window
    {
        #region Fields

        private System.Data.DataView _source;

        #endregion
        public TableWindow()
        {           
            InitializeComponent();

            ExportContentCommand = new DelegateCommand(
               () =>
               {
                   bool success = true;
                   try
                   {
                       success = DataAccess.Export.ToExcelFromDataView(Source);
                   }
                   catch (Exception e)
                   {
                       App.LogException(e);                       
                   }
                   if (success == false)
                       MessageBox.Show("Не удалось экспортировать результат запроса.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Warning);
               },
               (param) =>
               {
                   return Source != null;
               });

            menuitemExport.Command = ExportContentCommand;

           this.Loaded += TableWindow_Loaded;
        }

        #region Properties

        public System.Data.DataView Source
        {
            get { return _source; }
            set
            {
                SetProperty(ref _source, value);
                datagrid.ItemsSource = _source;
            }
        }
        //public ICommand SaveContentCommand { get; private set; }
        public ICommand ExportContentCommand { get; private set; }
        #endregion

        #region Private Helpers        

        private void TableWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ;
        }            

        #endregion

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

    #endregion // INotifyPropertyChanged Members

  }
}
