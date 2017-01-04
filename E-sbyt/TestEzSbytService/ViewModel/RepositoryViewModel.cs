using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMP.Extensions;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
    using DataAccess;
    using Model;
    using Controls.RepositoryControl;
    using Shared.Commands;

    public class RepositoryViewModel : AbstractViewModel
    {
        #region Fields
        private RequestsRepository _repository;
        private IRepositoryItem _selectedRepositoryItem;
        private ViewModel.Funcs.FuncSqlViewModel _sqlViewModel;
        #endregion
        #region Constructor
        public RepositoryViewModel(string fileName = null)
        {
            _repository = new RequestsRepository(fileName);
            InitCommands();
        }
        #endregion
        #region Properties

        public ICommand AddCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public bool IsEmptyList
        {
            get { return RepositoryItems == null || RepositoryItems.Count == 0; }
        }

        public IRepositoryItem SelectedItem
        {
            get { return _selectedRepositoryItem; }
            set { SetProperty(ref _selectedRepositoryItem, value); }
        }
        public ObservableCollection<IRepositoryItem> RepositoryItems
        {
            get { return _repository.RepositoryItems; }
            set
            {
                _repository.RepositoryItems = value;
                OnPropertyChanged("RepositoryItems");
                OnPropertyChanged("IsEmptyList");
                _repository.RepositoryItems.CollectionChanged += _repositoryItems_CollectionChanged;
            }
        }
        public ViewModel.Funcs.FuncSqlViewModel SqlViewModel
        {
            get { return _sqlViewModel; }
            set { SetProperty(ref _sqlViewModel, value.CloneJson()); }
        }

        public event EventHandler<SelectFuncSqlViewModelEventArgs> FuncSqlViewModelSelected;

        #endregion
        #region Public methods

        public void Save()
        {
            if (_repository != null)
                _repository.Save();
        }

        #endregion
        #region Private helpers

        private int GetRequestsCountInList(IList<IRepositoryItem> list)
        {
            int result = 0;
            if (list == null) return result;
            foreach (IRepositoryItem item in list)
            {
                if (item.Type == RepositoryItemType.Item)
                    result++;
                if (item.Items != null && item.Items.Count > 0)
                    result += GetRequestsCountInList(item.Items);
            }
            return result;
        }

        private void InitCommands()
        {
            AddCommand = new DelegateCommand(() =>
             {
                 AddEditWindow aer = new AddEditWindow();
                 IRepositoryItem result = aer.Show(null);

                 if (result != null)
                 {
                     if (SelectedItem != null)
                     {
                         result.Parent = SelectedItem;
                         SelectedItem.Items.Add(result);
                     }
                     else
                     {
                         result.Parent = null;
                         RepositoryItems.Add(result);
                     }
                     App.Log.Log(String.Format("В репозиторий добавлен элемент <{0}>", result));
                     OnPropertyChanged("IsEmptyList");
                     Save();
                 }
             });

            RemoveCommand = new DelegateCommand(() =>
            {
                if (SelectedItem == null) return;

                if (App.ShowQuestion("Вы действительно хотите удалить запись с именем '" + SelectedItem.Title + "'?") == System.Windows.MessageBoxResult.Yes)
                {
                    IRepositoryItem parent = SelectedItem.Parent;
                    if (parent != null)
                    {
                        parent.Items.Remove(SelectedItem);
                    }
                    else
                        RepositoryItems.Remove(SelectedItem);
                    App.Log.Log(String.Format("Из репозитория удалён элемент <{0}>", SelectedItem));
                    Save();
                }
            }, (param) =>
            {
                return (SelectedItem != null);
            });

            EditCommand = new DelegateCommand(() =>
             {
                 if (SelectedItem == null) throw new InvalidOperationException("EditCommand - SelectedItem == null");
                 App.Log.Log(String.Format("Попытка изменения элемента репозитория: <{0}>", SelectedItem));
                 AddEditWindow aer = new AddEditWindow();
                 aer.Owner = App.Current.MainWindow;
                 IRepositoryItem result = aer.Show(SelectedItem.Clone());

                 if (result != null)
                 {
                     IRepositoryItem parent = SelectedItem.Parent;
                     if (parent != null)
                     {
                         int index = parent.Items.IndexOf(SelectedItem);
                         if (index > 0)
                             parent.Items[index] = result;
                         SelectedItem = result;
                     }
                     else
                     {
                         SelectedItem = result;
                     }
                     App.Log.Log(String.Format("Изменённый элемент репозитория: <{0}>", result));
                     Save();
                 }
             }, (param) =>
             {
                 return SelectedItem != null;
             });

            LoadCommand = new DelegateCommand(() =>
            {
                if (SelectedItem == null) throw new InvalidOperationException("EditCommand - SelectedItem == null");

                if (SelectedItem.Type == RepositoryItemType.Group)
                    throw new ArgumentException("Выбрана не запись, а группа!");

                SqlViewModel = (SelectedItem as RepositoryItem).SqlViewModel;

                FuncSqlViewModelSelected?.Invoke(this, new SelectFuncSqlViewModelEventArgs(SelectedItem.Title, SqlViewModel));

                App.Log.Log(String.Format("Выбран элемент репозитория: <{0}>", SelectedItem));

            }, (param) =>
            {
                return SelectedItem is RepositoryItem;
            });

            ImportCommand = new DelegateCommand(() =>
            {
                if (RepositoryItems != null && RepositoryItems.Count > 0)
                    if (App.ShowQuestion("Очистить список перед импортом'?") == System.Windows.MessageBoxResult.Yes)
                        RepositoryItems.Clear();
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.AddExtension = true;
                ofd.CheckFileExists = true;
                ofd.Filter = String.Format("Файл списка запросов (*{0})|*{0}", RequestsRepository.REPOSITORY_FILE_EXTENSION);
                ofd.FilterIndex = 0;
                bool? show = ofd.ShowDialog(Application.Current.MainWindow);
                if (show != null && show.HasValue && show.Value == true)
                {
                    App.Log.Log("Попытка импорта списка запросов из файла '" + ofd.FileName + "'");
                    bool success = true;
                    IList<IRepositoryItem> list = Common.RepositoryCommon.BaseRepository<List<IRepositoryItem>>.GzJsonDeSerialize(
                        ofd.FileName,
                        (e) =>
                        {
                              success = false;
                              App.ToLogException(e);
                              App.ShowError(String.Format("Не удалось импортировать список запросов.\n\t{0}", e.Message), "ОШИБКА");
                          });
                    if (list != null)
                    {
                        var notImported = new List<IRepositoryItem>();
                        foreach (var item in list)
                            if (_repository.AddRepositoryItem(item) == false)
                                notImported.Add(item);
                        int toImportCount = GetRequestsCountInList(list);
                        int notImportedCount = GetRequestsCountInList(notImported);
                        int allCount = GetRequestsCountInList(RepositoryItems);
                        string message = String.Format("Завершён. Импортировано {0} запросов из списка. Не удалось импортировать {1} запросов. Всего сейчас в списке {2} запросов.",
                                toImportCount - notImportedCount, notImportedCount, allCount);
                        App.Log.Log(message);
                        App.ShowInfo(message, "ИМПОРТ");
                    }
                    else if (success)
                    {
                        string message = "Ничего не импортировано. Файл пуст!";
                        App.Log.Log(message);
                        App.ShowInfo(message, "ИМПОРТ");
                    }
                }

            });
            ExportCommand = new DelegateCommand(() =>
            {
                Microsoft.Win32.SaveFileDialog ofd = new Microsoft.Win32.SaveFileDialog();
                ofd.AddExtension = true;
                ofd.Filter = String.Format("Файл списка запросов (*{0})|*{0}", RequestsRepository.REPOSITORY_FILE_EXTENSION);
                ofd.FilterIndex = 0;
                bool? show = ofd.ShowDialog(Application.Current.MainWindow);
                if (show != null && show.HasValue && show.Value == true)
                {
                    App.Log.Log("Попытка экспорта списка запросов в файл '" + ofd.FileName + "'");
                    bool success = true;
                    Common.RepositoryCommon.BaseRepository<List<IRepositoryItem>>.GzJsonSerialize(
                        RepositoryItems.ToList(),
                        ofd.FileName,
                        (e) =>
                        {
                              success = false;
                              App.ToLogException(e);
                              App.ShowError(String.Format("Не удалось экспортировать список запросов.\n\t{0}", e.Message), "ОШИБКА");
                          });
                    if (success)
                    {
                        string message = String.Format("Экспортировано {0} запросов.", RepositoryItems.Count);
                        App.Log.Log(message);
                        App.ShowInfo(message, "ЭКСПОРТ");
                    }
                }
            }, (param) =>
            {
                if (RepositoryItems == null)
                    return false;
                else
                    return RepositoryItems.Count != 0;
            });

        }

        private void _repositoryItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("IsEmptyList");
        }

        #endregion
    }
    public class SelectFuncSqlViewModelEventArgs : EventArgs
    {
        public SelectFuncSqlViewModelEventArgs(string name, ViewModel.Funcs.FuncSqlViewModel viewModel)
        {
            Name = name;
            ViewModel = viewModel;
        }

        public string Name { get; set; }
        public ViewModel.Funcs.FuncSqlViewModel ViewModel { get; set; }
    }
}
