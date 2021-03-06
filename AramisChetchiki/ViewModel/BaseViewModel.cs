﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

using TMP.UI.Controls.WPF;
using TMP.Work.Emcos;

namespace TMP.WORK.AramisChetchiki.ViewModel
{
    [DataContract]
    public abstract class BaseViewModel : PropertyChangedBase, IViewModel
    {
        private string _status = null;
        public String Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged("Status"); }
        }
        private string _detailedStatus = null;
        public String DetailedStatus
        {
            get { return _detailedStatus; }
            set { _detailedStatus = value; RaisePropertyChanged("DetailedStatus"); }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged("IsBusy"); }
        }

        private bool _isAnalizingData = false;
        public bool IsAnalizingData
        {
            get { return _isAnalizingData; }
            set { _isAnalizingData = value; RaisePropertyChanged("IsAnalizingData"); }
        }

        private bool _isDataLoaded = false;
        public bool IsDataLoaded
        {
            get { return _isDataLoaded; }
            set { _isDataLoaded = value; RaisePropertyChanged("IsDataLoaded"); }
        }

        public virtual ICommand CommandExport { get => throw new System.NotImplementedException(); }
        public virtual ICommand CommandPrint { get => throw new System.NotImplementedException(); }

        protected System.Windows.Window _window;

        public System.Windows.Input.ICommand CommandCloseWindow { get; }

        public BaseViewModel(System.Windows.Window window = null)
        {
            _window = window;
            CommandCloseWindow = new DelegateCommand(() =>
            {
                if (_window != null)
                    _window.Close();
            }, "Закрыть");
        }

    }

    [DataContract]
    public class BaseDataViewModel<T> : BaseViewModel, IDataViewModel<T> where T : Model.IModel
    {
        public virtual ICollection<T> Data
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
        public virtual ICollectionView View
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
    }
}
