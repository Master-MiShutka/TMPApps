using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
    using TMP.Shared;
    using TMP.Shared.Commands;
    /// <summary>
    /// Модель представления для главного окна приложения
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        ObservableCollection<TabViewModel> _tabs;
        string _repositoryFileName = null;
        private RepositoryViewModel _repositoryViewModel;

        #endregion // Fields

        #region Constructor

        public MainWindowViewModel(string repositotyFileName)
        {
            _repositoryFileName = repositotyFileName;
            _repositoryViewModel = new RepositoryViewModel(_repositoryFileName);
            _repositoryViewModel.FuncSqlViewModelSelected += _repositoryViewModel_RequestSelected;

            base.DisplayName = "Тестирование сервиса jqsbyt";

            Tabs.Add(new SearchViewModel());
            Tabs.Add(new ShemaUchetViewModel());
            Tabs.Add(new ManualRequestViewModel() { DisplayName = "Ручной запрос" });
            SelectedItem = Tabs[0];

            CreateNewManualRequestTabCommand = new DelegateCommand(() =>
            {
                var list = Tabs.Where((t) => (t is ManualRequestViewModel) && (t.DisplayName.StartsWith("Запрос данных"))).ToList();
                ManualRequestViewModel mrvm = new ManualRequestViewModel();
                if (list != null)
                    mrvm.DisplayName += " (" + (list.Count + 1) + ")";
                Tabs.Add(mrvm);
            });
        }

        private void _repositoryViewModel_RequestSelected(object sender, SelectFuncSqlViewModelEventArgs e)
        {
            ManualRequestViewModel mrvm = new ManualRequestViewModel(e.ViewModel);
            mrvm.DisplayName = e.Name;

            // скрытие панели выбора функции сервиса - уже выбрана функция sql
            mrvm.IsServiceFunctionSelectorVisible = false;
            Tabs.Add(mrvm);
            SelectedItem = mrvm;            
        }

        private void MainWindowViewModelOnClose(object sender)
        {
            // сохранение репозитория при закрытии программы
            if (_repositoryViewModel != null)
            {
                _repositoryViewModel.Save();
            }
        }

        #endregion // Constructor

        #region Properties
        public override ICommand CloseCommand { get { return new RelayCommand((param) => { MainWindowViewModelOnClose(param); }); } }

        private object _selectedItem;
        public object SelectedItem {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        /// <summary>
        /// Возвращает колекцию доступных рабочих пространств для отображения.
        /// 'Рабочее пространство' это модель представления, которое может быть закрыто.
        /// </summary>
        public ObservableCollection<TabViewModel> Tabs
        {
            get
            {
                if (_tabs == null)
                {
                    _tabs = new ObservableCollection<TabViewModel>();
                    _tabs.CollectionChanged += this.OnTabsChanged;
                }
                return _tabs;
            }
        }

        public ICommand CreateNewManualRequestTabCommand { get; set; }

        public RepositoryViewModel Repository { get { return _repositoryViewModel; } }

        public string Status
        {
            get
            {
                switch (State)
                {
                    case State.Idle:
                        return "Готов";
                    case State.Busy:
                        if (Progress > 0d)
                            return String.Format("Выполнение задачи: статус {0}%", Progress);
                        else
                            return "Выполнение задачи";
                    default:
                        return "Готов";
                }
            }
        }

        #endregion

        #region Public methods

        void OnTabsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (TabViewModel workspace in e.NewItems)
                    workspace.RequestClose += this.OnTabRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (TabViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnTabRequestClose;
        }

        void OnTabRequestClose(object sender, EventArgs e)
        {
            TabViewModel tabvm = sender as TabViewModel;
            tabvm.Dispose();
            this.Tabs.Remove(tabvm);
        }

        #endregion

        #region Private Helpers


        #endregion // Private Helpers
    }
}
