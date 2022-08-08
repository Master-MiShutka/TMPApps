using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;

namespace TMP.Work.Emcos.Controls
{
    using TMP.Shared.Commands;
    using TMP.Work.Emcos.Model;

    /// <summary>
    /// Interaction logic for SessionManager.xaml
    /// </summary>
    public partial class SessionManager : UserControl
    {
        private const string TITLE = "Менеджер сессий";
        private Repository _repository;

        public SessionManager()
        {
            InitializeComponent();

            LoadCommand = new DelegateCommand(async () =>
            {
                ;
            }, (o) => SelectedSessionInfo != null);
        }
        public SessionManager(Repository repository) : this()
        {
            System.Diagnostics.Debug.Assert(repository != null);
            _repository = repository;

            SessionsInfoList = _repository.SessionsInfoList
                .OrderByDescending(i => i.LastModifiedDate)
                .ThenBy(i => i.Title)
                .ToList();
            DataContext = this;
        }

        public string Message => _repository.SessionsInfoList == null
            ? string.Format("В папке '{0}' не обнаружены сессии (файлы с расширением '.{1}').\nСоздайте новую сессию.", _repository.SESSIONS_FOLDER, _repository.SESSION_FILE_EXTENSION)
            : "Обнаружены следующие сохранённые сессии.\nВыберите нужную или создайте новую.";

        public BalanceSessionInfo SelectedSessionInfo { get; set; }
        public IList<BalanceSessionInfo> SessionsInfoList { get; private set; }

        public ICommand LoadCommand { get; private set; }
    }
}
