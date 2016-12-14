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
using System.ComponentModel;
using System.Runtime.CompilerServices;

using TMP.Extensions;

namespace TMP.Work.AmperM.TestApp.Controls.RepositoryControl
{
    using Model;
    using Shared.Commands;
    /// <summary>
    /// Interaction logic for AddEditItemWindow.xaml
    /// </summary>
    public partial class AddEditWindow : Window, INotifyPropertyChanged
    {
        private IRepositoryItem _repositoryItem;
        private string _windowTitle;
        private bool _addMode = false;
        public AddEditWindow()
        {
            this.Closing += Window_Closing;
            this.Loaded += Window_Loaded;

            InitializeComponent();
            DataContext = this;

            OKCommand = new DelegateCommand(
                () =>
                {
                    DialogResult = true;
                },
                (p) => true,
                () => AddMode ? "Добавить" : "Сохранить");
            CancelCommand = new DelegateCommand(
                () =>
                {
                    DialogResult = false;
                },
                "Отменить");
            ChangeItemTypeCommand = new DelegateCommand(
                (e) =>
                {
                    if (e == null) return;
                    string type = e.ToString();
                    if (type == "item")
                        Item = new RepositoryItem();
                    else
                        Item = new RepositoryGroup();
                });
        }

        public IRepositoryItem Show(IRepositoryItem item)
        {
            Owner = ApplicationExtention.ActiveWindow;
            if (Application.Current != null)
                Application.Current.DisableWindow();

            if (item == null)
            {
                AddMode = true;
                Item = new RepositoryItem();
                WindowTitle = "Добавление";
            }
            else
            {
                AddMode = false;
                Item = item;
                WindowTitle = "Редактирование";
            }            

            bool? show = ShowDialog();

            if (Application.Current != null)
                Application.Current.EnableWindow();

            if (show.HasValue && show.Value == true)
                return _repositoryItem;
            else
                return null;
        }

        #region Properties
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                OnPropertyChanged();
            }
        }
        public bool AddMode
        {
            get { return _addMode; }
            set
            {
                _addMode = value;
                OnPropertyChanged();
            }
        }
        public RepositoryItemType ItemType
        {
            get { return _repositoryItem.Type; }
        }

        public IRepositoryItem Item
        {
            get { return _repositoryItem; }
            set
            {
                _repositoryItem = value;
                OnPropertyChanged();
                OnPropertyChanged("ItemType");

                SetWindowSizeOnItemTypeChange();
            }
        }

        #region Commands

        public ICommand OKCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ChangeItemTypeCommand { get; set; }

        #endregion

        #endregion

        #region Private Methods
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == null)
                DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ClearValue(SizeToContentProperty);
            LayoutRoot.ClearValue(WidthProperty);
            LayoutRoot.ClearValue(HeightProperty);

            CorrectPositionToCenter();
        }

        private void SetWindowSizeOnItemTypeChange()
        {
            switch (ItemType)
            {
                case RepositoryItemType.Item:
                    SizeToContent = SizeToContent.Manual;
                    Width = 1200;
                    Height = 800;
                    break;
                case RepositoryItemType.Group:
                    SizeToContent = SizeToContent.WidthAndHeight;
                    ClearValue(WidthProperty);
                    ClearValue(HeightProperty);
                    break;
                default:
                    throw new NotImplementedException("ItemType");
            }
            CorrectPositionToCenter();
        }

        private void CorrectPositionToCenter()
        {
            Left = (SystemParameters.PrimaryScreenWidth - ActualWidth) / 2d;
            Top = (SystemParameters.PrimaryScreenHeight - ActualHeight) / 2d;
        }
        #endregion

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
