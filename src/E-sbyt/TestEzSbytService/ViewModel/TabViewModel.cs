using System;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
    using Shared;
    using Shared.Commands;

    public abstract class TabViewModel : ViewModelBase, IEzSbytServiceFunctionViewModel
    {
        #region Fields

        private ResultViewerViewModel _resultVM;
        private const string _defaultStatus = "Ожидание";
        private string _status;
        private bool _isSelected;

        protected Funcs.IFuncViewModel func;

        protected object _sync = new object();


        #endregion Fields

        #region Constructor

        protected TabViewModel()
        {
            CancelCommand = new DelegateCommand(() =>
            {
                IsCanceled = true;
                State = State.Canceling;
                Status = "Отмена операции ...";
                EzSbyt.EzSbytService.Instance.Cts.Cancel();
            },
            (param) => State == State.Busy,
            "Отменить");
        }

        #endregion Constructor

        #region Properties

        public ResultViewerViewModel Result
        {
            get { lock (_sync) { return _resultVM; } }
            set
            {
                lock (_sync)
                {
                    SetProperty(ref _resultVM, value);
                }
            }
        }

        public string Status
        {
            get
            {
                lock (_sync)
                {
                    if (String.IsNullOrEmpty(_status) == true) return _defaultStatus; else return _status;
                }
            }
            set
            {
                lock (_sync)
                {
                    SetProperty(ref _status, value);
                    Message = value;
                }
            }
        }
        public bool IsSelected
        {
            get
            {
                return _isSelected;                
            }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }
        #endregion Properties
    }
}