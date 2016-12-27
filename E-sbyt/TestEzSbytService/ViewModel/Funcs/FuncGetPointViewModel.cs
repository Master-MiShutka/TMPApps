using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TMP.Work.AmperM.TestApp.ViewModel.Funcs
{
    using Shared;
    /// <summary>
    /// Формирует запрос для получения данных расчетной точки по выбранному идентификатору расчетной точки.
    /// Обязательный параметр: id (идентификатор расчетной точки), филиал определяется по id, поэтому параметр f задавать не нужно.
    /// </summary>
    public class FuncGetPointViewModel : BaseFuncViewModel
    {
        #region Fields

        private FuncParameter _Идентификатор = new FuncParameter("id", null);

        #endregion

        #region Constructors

        public FuncGetPointViewModel() : base() { FuncName = "getpoint"; }

        #endregion

        #region Properties
        #region Требуемые параметры
        /// <summary>
        /// [id] Идентификатор расчетной точки, например: 14310000001000002
        /// </summary>
        public FuncParameter Идентификатор
        {
            get { return _Идентификатор; }
            set { if (value.Name == null) value.Name = "id"; SetProperty(ref _Идентификатор, value); }
        }
        #endregion
        #endregion

        #region Private Helpers
        protected override string[] BodyProperties() { return new string[] { }; }
        protected override string[] UrlParamsProperties() { return new string[] { nameof(Идентификатор), nameof(Формат), nameof(Отладка) }; }

        protected override bool CanExecute(object param)
        {
            return String.IsNullOrEmpty(Идентификатор.Value) == false;
        }

        protected override string GetCommandHeader()
        {
            return "Получить данные точки";
        }

        #endregion
    }
}