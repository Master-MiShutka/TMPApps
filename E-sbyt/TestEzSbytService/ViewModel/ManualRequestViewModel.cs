using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
    using Shared;
    using Shared.Commands;
    using EzSbyt;

    public class ManualRequestViewModel : TabViewModel
    {
        #region Fields

        private FormatTypes _selectedFormat = FormatTypes.xml;
        private EzSbyt.EzSbytRequestFunctionType _selectedFunction = EzSbyt.EzSbytRequestFunctionType.sql;

        private Funcs.IFuncViewModel _functionViewModel;

        #endregion // Fields

        #region Constructor

        public ManualRequestViewModel()
        {
            DisplayName = "Запрос данных";
            CreateViewModel();
        }
        public ManualRequestViewModel(ViewModel.Funcs.IFuncViewModel functionViewModel)
        {
            _functionViewModel = functionViewModel;
            // подписка на события
            Subscribe();
            SelectedFunction = EzSbytRequestFunctionType.sql;
        }
        ~ManualRequestViewModel()
        {
            UnSubcribe();
        }
        #endregion
        #region Properties
        public override bool CanClose { get; } = true;
        /// <summary>
        /// Используемая функция сервиса
        /// </summary>
        public EzSbyt.EzSbytRequestFunctionType SelectedFunction
        {
            get { return _selectedFunction; }
            set
            {
                if (SetProperty(ref _selectedFunction, value) == false) return;

                // модель будет изменена, отписываемся
                UnSubcribe();
                // обнуление
                _functionViewModel = null;
                // уведомление об изменении модели представления
                OnPropertyChanged("FunctionViewModel");
            }
        }
        /// <summary>
        /// Выбранный вариант формата данных 
        /// </summary>
        public FormatTypes SelectedFormatType
        {
            get { return _selectedFormat; }
            set { SetProperty(ref _selectedFormat, value); }
        }
        /// <summary>
        /// Модель представления используемой функции сервиса
        /// </summary>
        public ViewModel.Funcs.IFuncViewModel FunctionViewModel
        {
            get
            {
                if (_functionViewModel == null)
                {
                    CreateViewModel();
                }
                return _functionViewModel;
            }
        }

        public ICommand SaveRequestAsCommand { get; set; }
        public ICommand UpdateRequestCommand { get; set; }

        /// <summary>
        /// Указывает необходимо ли отображение выбора функции сервиса
        /// </summary>
        public bool IsServiceFunctionSelectorVisible { get; set; } = true;

        #endregion
        #region Public methods
        #endregion
        #region Private Helpers

        private void CreateViewModel()
        {
            switch (SelectedFunction)
            {
                case EzSbyt.EzSbytRequestFunctionType.sql:
                    _functionViewModel = new Funcs.FuncSqlViewModel();
                    break;
                case EzSbyt.EzSbytRequestFunctionType.getobj:
                    _functionViewModel = new Funcs.FuncGetObjViewModel();
                    break;
                case EzSbyt.EzSbytRequestFunctionType.getpoint:
                    _functionViewModel = new Funcs.FuncGetPointViewModel();
                    break;
                case EzSbyt.EzSbytRequestFunctionType.meta:
                    _functionViewModel = new Funcs.FuncMetaViewModel();
                    break;
                case EzSbyt.EzSbytRequestFunctionType.schema:
                    _functionViewModel = new Funcs.FuncSchemaViewModel();
                    break;
                default:
                    throw new NotImplementedException("SelectedFunction");
            }
            // модель изменена, подписываемся
            Subscribe();
        }

        void OnFuncCommandExecuting(object sender, CancelCommandEventArgs args)
        {
            args.Parameter = this;
            Status = "Ожидание данных ...";
            State = State.Busy;
        }

        void OnFuncCommandExecuted(object sender, CommandEventArgs args)
        {
            State = State.Idle;
            Status = null;
            System.Media.SystemSounds.Asterisk.Play();
            if (Result != null) Result.UpdateUI();
        }

        void Subscribe()
        {
            if (_functionViewModel.GetCommand != null)
            {
                (_functionViewModel.GetCommand as AsynchronousDelegateCommand).Executing += OnFuncCommandExecuting;
                (_functionViewModel.GetCommand as AsynchronousDelegateCommand).Executed += OnFuncCommandExecuted;
            }
        }
        void UnSubcribe()
        {
            if (_functionViewModel != null && _functionViewModel.GetCommand != null)
            {
                (_functionViewModel.GetCommand as AsynchronousDelegateCommand).Executing -= OnFuncCommandExecuting;
                (_functionViewModel.GetCommand as AsynchronousDelegateCommand).Executed -= OnFuncCommandExecuted;
            }
        }
        #endregion
    }
}