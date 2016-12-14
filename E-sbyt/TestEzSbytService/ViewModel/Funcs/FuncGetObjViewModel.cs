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
    /// Формирует запрос для получения данных по объекту (записи) выбранной таблицы базы данных.
    /// Обязательные параметры: f, t, filter, параметр fields опциональный (по умолчанию выбираются все поля таблицы).
    /// </summary>
    public class FuncGetObjViewModel : BaseFuncViewModel
    {
        #region Fields

        private FuncParameter _Таблица = new FuncParameter("t", null);
        private FuncParameter _Условие = new FuncParameter("filter", null);
        private FuncParameter _ПереченьПолей = new FuncParameter("fields", null);

        #endregion

        #region Constructors

        public FuncGetObjViewModel() : base() { FuncName = "getobj";}

        #endregion

        #region Properties
        #region Требуемые параметры
        /// <summary>
        /// [t] Имя таблицы, например: "РасчетныеТочки"
        /// </summary>
        public FuncParameter Таблица
        {
            get { return _Таблица; }
            set { if (value.Name == null) value.Name = "t"; SetProperty(ref _Таблица, value); }
        }
        /// <summary>
        /// [filter] Условие (фильтр) выбора записи, например: "ид=14310000001000002"
        /// </summary>
        public FuncParameter Условие
        {
            get { return _Условие; }
            set { if (value.Name == null) value.Name = "filter"; SetProperty(ref _Условие, value); }
        }
        #endregion

        #region Дополнительные параметры
        /// <summary>
        /// [fields] Перечень полей таблицы для выборки через запятую, например: "ид,наименование,абонент:владелец.абонент.наименование"
        /// </summary>
        public FuncParameter ПереченьПолей
        {
            get { return _ПереченьПолей; }
            set { if (value.Name == null) value.Name = "fields"; SetProperty(ref _ПереченьПолей, value); }
        }
        #endregion
        #endregion

        #region Private Helpers
        protected override string[] BodyProperties() { return new string[] { }; }
        protected override string[] UrlParamsProperties()
        {
            return new string[] { nameof(Филиал), nameof(Таблица), nameof(Условие), nameof(ПереченьПолей),
            nameof(Формат), nameof(Отладка) };
        }
        protected override bool CanExecute(object param)
        {
            return String.IsNullOrEmpty(Филиал.Value) == false && String.IsNullOrEmpty(Таблица.Value) == false && String.IsNullOrEmpty(Условие.Value) == false;
        }

        protected override string GetCommandHeader()
        {
            return "Получить данные по объекту";
        }

        #endregion
    }
}