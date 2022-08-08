using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
    using TMP.Shared;
    using TMP.Shared.Commands;
    using EzSbyt;

    public class SearchViewModel : TabViewModel
    {
        #region Fields

        private string _textToSearch = null;
        private Funcs.FuncSqlViewModel  _func = new Funcs.FuncSqlViewModel();

        #endregion
        #region Constructor
        public SearchViewModel()
        {
            DisplayName = "Поиск данных";

            (_func.GetCommand as AsynchronousDelegateCommand).Executing += OnFuncCommandExecuting;
            (_func.GetCommand as AsynchronousDelegateCommand).Executed += OnFuncCommandExecuted;
        }
        ~SearchViewModel()
        {
            if (_func != null && _func.GetCommand != null)
            {
                (_func.GetCommand as AsynchronousDelegateCommand).Executing -= OnFuncCommandExecuting;
                (_func.GetCommand as AsynchronousDelegateCommand).Executed -= OnFuncCommandExecuted;
            }
        }
        #endregion
        #region Public properties
        public ICommand GetCommand { get { return _func.GetCommand; } }
        public override ICommand CloseCommand { get { return new RelayCommand((param) => { }); } }
        public string TextToSearch
        {
            get { return _textToSearch; }
            set
            {
                SetProperty(ref _textToSearch, value);
                SetRequestParams();
            }
        }
        public Funcs.FuncParameter Формат
        {
            get { return _func.Формат; }
            set { _func.Формат = value; OnPropertyChanged("Формат"); }
        }
        #endregion
        #region Public methods

        #endregion

        #region Private Helpers

        private void SetRequestParams()
        {
            _func.ТекстЗапроса = new Funcs.FuncParameter(_func.ТекстЗапроса, String.Format(
                @"выбрать _Филиал.наименование как филиал, ""абоненты"" как группа,количество(*) как найдено из справочник.договоры где не пометкаудаления и (абонент.наименование подобно ""%{0}%"" или абонент.адрес.наименование подобно ""%{0}%"" ) сгруппировать по _Филиал имеющие количество(*)>0" +
                @" объединить выбрать _Филиал.наименование как филиал,""расчетные точки"" как группа,количество(*) как найдено из справочник.расчетныеточки где не пометкаудаления и (наименование подобно ""%{0}%"" или объектучета.адрес.наименование подобно ""%{0}%"" ) сгруппировать по _Филиал имеющие количество(*)>0" +
                @" объединить выбрать _Филиал.наименование как филиал,""приборы учета установленные"" как группа,количество(*) как найдено из регистрсведений.счетчикиустановленные где счетчик.заводскойномер=""{0}"" сгруппировать по _Филиал имеющие количество(*)>0" +
                @" объединить выбрать _Филиал.наименование как филиал, ""приборы учета снятые"" как группа,количество(*) как найдено из регистрсведений.счетчикиснятые где счетчик.заводскойномер=""{0}"" сгруппировать по _Филиал имеющие количество(*)>0" +
                @" объединить выбрать _Филиал.наименование как филиал,""приборы учета (счётчики)"" как группа, количество(*) как найдено из справочник.счетчики где заводскойномер = ""{0}"" сгруппировать по _Филиал имеющие количество(*) > 0" +
                @" объединить выбрать _Филиал.наименование как филиал,""подключения"" как группа,количество(*) как найдено из справочник.подстанции где не пометкаудаления и наименование подобно ""%{0}%"" сгруппировать по _Филиал имеющие количество(*)>0",
                TextToSearch));
            //func.SQLзапросДополнительнойОбработки = new Funcs.FuncParameter(func.SQLзапросДополнительнойОбработки, "выбрать группа,филиал,найдено из т1 итоги сумма(найдено) по группа");
        }

        void OnFuncCommandExecuting(object sender, CancelCommandEventArgs args)
        {
            if (String.IsNullOrWhiteSpace(this.TextToSearch))
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