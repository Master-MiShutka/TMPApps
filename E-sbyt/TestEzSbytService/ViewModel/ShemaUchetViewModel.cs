using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
    using Shared;
    using Shared.Commands;

    public class ShemaUchetViewModel : TabViewModel
    {
        #region Fields

        private string _request = "схема_учета.расчетные_точки_1";

        private string _дг;
        private string _фнаим;
        private string _фсч;
        private string _фпс;
        private string _ффид;
        private string _фтп;

        private Funcs.FuncSqlViewModel _func = new Funcs.FuncSqlViewModel();

        #endregion

        #region Constructors

        public ShemaUchetViewModel() : base()
        {
            DisplayName = "Схема учёта";

            (_func.GetCommand as AsynchronousDelegateCommand).Executing += OnFuncCommandExecuting;
            (_func.GetCommand as AsynchronousDelegateCommand).Executed += OnFuncCommandExecuted;
        }

        ~ShemaUchetViewModel()
        {
            if (_func != null && _func.GetCommand != null)
            {
                (_func.GetCommand as AsynchronousDelegateCommand).Executing -= OnFuncCommandExecuting;
                (_func.GetCommand as AsynchronousDelegateCommand).Executed -= OnFuncCommandExecuted;
            }
        }

        #endregion

        #region Properties

        public ICommand GetCommand { get { return _func.GetCommand; } }
        public override ICommand CloseCommand { get { return new RelayCommand((param) => { }); } }

        public string Request
        {
            get { return _request; }
//            set { SetProperty(ref _request, value); }
        }

        public string дг
        {
            get { return _дг; }
            set { SetProperty(ref _дг, value); SetRequestParams(); }
        }
        public string фнаим
        {
            get { return _фнаим; }
            set { SetProperty(ref _фнаим, value); SetRequestParams(); }
        }
        public string фсч
        {
            get { return _фсч; }
            set { SetProperty(ref _фсч, value); SetRequestParams(); }
        }
        public string фпс
        {
            get { return _фпс; }
            set { SetProperty(ref _фпс, value); SetRequestParams(); }
        }
        public string ффид
        {
            get { return _ффид; }
            set { SetProperty(ref _ффид, value); SetRequestParams(); }
        }
        public string фтп
        {
            get { return _фтп; }
            set { SetProperty(ref _фтп, value); SetRequestParams(); }
        }
        public Funcs.FuncParameter Формат
        {
            get { return _func.Формат; }
            set { _func.Формат = value; OnPropertyChanged("Формат"); }
        }
        #endregion

        #region Public Methods


        #endregion

        #region Private Helpers

        private void SetRequestParams()
        {
            string parameters = String.Format(@"{{""дг"":""{0}"",""фнаим"":""{1}"",""фсч"":""{2}"",""фпс"":""{3}"",""ффид"":""{4}"",""фтп"":""{5}""}}",
              дг, фнаим, фсч, фпс, ффид, фтп);
            if (parameters.Length <= 57)
            {
              _func.ТекстЗапроса = new Funcs.FuncParameter(_func.ТекстЗапроса, string.Empty);
              return;
            }
            _func.ТекстЗапроса = new Funcs.FuncParameter(_func.ТекстЗапроса, Request);
            _func.ПараметрыЗапроса = new Funcs.FuncParameter(_func.ПараметрыЗапроса,
                parameters);
        }
        void OnFuncCommandExecuting(object sender, CancelCommandEventArgs args)
        {
            if (String.IsNullOrWhiteSpace(this.Request))
            {
                App.ShowWarning("Нечего искать!");
                args.Cancel = true;
                return;
            }
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

        #endregion
    }
}