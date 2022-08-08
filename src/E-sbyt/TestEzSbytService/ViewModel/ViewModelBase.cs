using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;


namespace TMP.Work.AmperM.TestApp.ViewModel
{
    using Shared;
    using Shared.Commands;
    /// <summary>
    /// Базовый класс для всех моделей представления в приложении
    /// </summary>
    public class ViewModelBase : AbstractViewModelWaitable
    {
        #region Fields
        RelayCommand _closeCommand;

        #endregion

        #region Constructor

        public ViewModelBase()
        {
            DisplayName = "<нет названия>";
            State = State.Idle;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Возвращает команду, запуск которой удаляет вкладку из интерфейса
        /// </summary>
        public virtual ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(param => this.OnRequestClose());

                return _closeCommand;
            }
        }
        public virtual bool CanClose { get; } = false;

        private string _message = "Ожидание";
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        #endregion

        #region RequestClose [event]

        /// <summary>
        /// Будет вызвано при закрытии
        /// </summary>
        public event EventHandler RequestClose;

        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // RequestClose [event]

        #region Public methods

        #endregion
    }
}
