using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using TMP.Shared.Commands;

namespace TMP.Work.AmperM.TestApp.Controls
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

            ExportContentCommand = new AsynchronousDelegateCommand(
               () =>
               {
                   bool? success = true;
                   try
                   {
                       //success = DataAccess.Export.CreateExcelWorkBookFromCollection(View.SourceCollection.Cast<dynamic>());
                   }
                   catch (Exception e)
                   {
                       App.ToLogException(e);
                   }
                   if (success != null && success == false)
                       App.ShowWarning("Не удалось экспортировать результат запроса.", "Экспорт");
               },
               (param) =>
               {
                   return View != null;
               });

            (ExportContentCommand as AsynchronousDelegateCommand).Executing += OnExportContentCommandExecuting;
            (ExportContentCommand as AsynchronousDelegateCommand).Executed += OnExportContentCommandExecuted;

            DataContext = this;
        }

        private void OnExportContentCommandExecuted(object sender, CommandEventArgs args)
        {
            Shared.IWaitableObject parent = null;
            Control control = this;
            while (control != null && control.DataContext != null && !(control.DataContext is Shared.IWaitableObject))
            {
                parent = control.DataContext as Shared.IWaitableObject;
                control = control.Parent as Control;
            }
            if (control != null)
                parent = control.DataContext as Shared.IWaitableObject;
            else
                return;
            if (parent != null)
                parent.State = Shared.State.Idle;
        }

        private void OnExportContentCommandExecuting(object sender, CancelCommandEventArgs args)
        {
            Shared.IWaitableObject parent = null;
            Control control = this;
            while (control != null && control.DataContext != null && !(control.DataContext is Shared.IWaitableObject))
            {
                parent = control.DataContext as Shared.IWaitableObject;
                control = control.Parent as Control;
            }
            if (control != null)
                parent = control.DataContext as Shared.IWaitableObject;
            else
                return;
            if (parent != null)
                parent.State = Shared.State.Busy;
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

        public ICommand ExportContentCommand { get; private set; }

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