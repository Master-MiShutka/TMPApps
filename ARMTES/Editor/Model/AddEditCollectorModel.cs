using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using TMP.Shared;
using TMP.ARMTES.Model;

namespace TMP.ARMTES.Editor
{
    public class AddEditCollectorModel : PropertyChangedBase
    {
        private static AddEditCollectorModel instance;

        private RegistryCollector _collector = new RegistryCollector();

        private AddEditCollectorModel() { }
        static AddEditCollectorModel()
        {
            instance = new AddEditCollectorModel();
            instance.Collector = new RegistryCollector();
        }

        public static AddEditCollectorModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AddEditCollectorModel();
                }
                return instance;

            }
        }

        #region | Properties |

        public RegistryCollector Collector
        {
            get { return _collector; }
            set { SetProp<RegistryCollector>(ref _collector, value, "Collector"); }
        }

        #endregion

        #region | Commands |
        private RelayCommand _newCommand;
        private RelayCommand _editCommand;
        private RelayCommand _deleteCommand;

        private RelayCommand _okCommand;

        public ICommand NewCommand
        {
            get
            {
                if (_newCommand == null)
                {
                    _newCommand = new RelayCommand(param => NewItem());
                }
                return _newCommand;
            }
        }
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(param => EditItem(param), param => CanEditItem(param));
                }
                return _editCommand;
            }
        }
        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(param => DeleteItem(param), param => CanDeleteItem(param));
                }
                return _deleteCommand;
            }
        }
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(param => StoreChanges(param), param => CanStoreChanges(param));
                }
                return _okCommand;
            }
        }
        #endregion

        #region | CommandActions |
        private void NewItem()
        {
            ;
        }
        private bool CanEditItem(Object parameter)
        {
            if (parameter == null)
                return false;
            else
                return true;
        }
        private void EditItem(Object parameter)
        {
            RegistryCollector collector = parameter as RegistryCollector;
            if (collector == null)
                return;
            else
            {
                MessageBox.Show(collector.House);
            }
        }
        private bool CanDeleteItem(Object parameter)
        {
            if (parameter == null)
                return false;
            else
                return true;
        }
        private void DeleteItem(Object parameter)
        {
            ;
        }
        private bool CanStoreChanges(Object parameter)
        {
            return true;
        }
        private void StoreChanges(Object parameter)
        {
            ;
        }
        #endregion
    }
}
