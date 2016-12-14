using System.Windows.Input;

namespace TMP.Work.AmperM.TestApp.ViewModel.Funcs
{
    public interface IFuncViewModel
    {
        /// <summary>
        /// Имя функции
        /// </summary>
        string FuncName { get; }
        /// <summary>
        /// Возвращает содержимое запроса, причем экранированы только значения параметров
        /// </summary>
        /// <returns></returns>
        string GetEscapedBody();
        /// <summary>
        /// Возвращает список параметр=значение для запроса, причем экранированы только значения параметров
        /// </summary>
        /// <returns></returns>
        string GetUrlParams();
        /// <summary>
        /// Указывает будет ли запрос передаваться методом POST
        /// </summary>
        bool HasBody { get; }
        /// <summary>
        /// Команда для отправки запроса
        /// </summary>
        ICommand GetCommand { get; }
    }
}
