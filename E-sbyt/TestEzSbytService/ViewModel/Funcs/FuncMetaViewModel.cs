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
    /// Формирует запрос для получения метаданных базы данных или запроса - список таблиц, полей базы данных филиала или все вместе.
    /// Основные параметры: f-филиал, q-вид метаданных и t-таблица если q="fields".
    /// </summary>
    public class FuncMetaViewModel : BaseFuncViewModel
    {
        #region Fields

        private FuncParameter _ВидМетаданных = new FuncParameter("q", null);
        private FuncParameter _ИмяТаблицы = new FuncParameter("t", null);
        private FuncParameter _ПараметрыЗапроса = new FuncParameter("p", null);
        private FuncParameter _ПереченьПолей = new FuncParameter("fields", null);
        private FuncParameter _Условие = new FuncParameter("filter", null);
        private FuncParameter _ПоляСортировки = new FuncParameter("sort", null);

        #endregion

        #region Constructors

        public FuncMetaViewModel() : base() { FuncName = "meta"; }

        #endregion

        #region Properties
        #region Требуемые параметры
        /// <summary>
        /// [q] Вид метаданных: tables | fields | keyfields | db | all | текст произвольного запроса
        /// </summary>
        public FuncParameter ВидМетаданных
        {
            get { return _ВидМетаданных; }
            set { if (value.Name == null) value.Name = "q"; SetProperty(ref _ВидМетаданных, value); }
        }
        /// <summary>
        /// [t] Имя таблицы, задается если q=fields или keyfields, например: РасчетныеТочки
        /// </summary>
        public FuncParameter ИмяТаблицы
        {
            get { return _ИмяТаблицы; }
            set { if (value.Name == null) value.Name = "t"; SetProperty(ref _ИмяТаблицы, value); }
        }
        #endregion

        #region Дополнительные параметры
        /// <summary>
        /// [p] Задается при необходимости если q=запрос, параметры запроса в формате JSON, например: {"прм_номер": 11}.
        /// </summary>
        public FuncParameter ПараметрыЗапроса
        {
            get { return _ПараметрыЗапроса; }
            set { if (value.Name == null) value.Name = "p"; SetProperty(ref _ПараметрыЗапроса, value); }
        }
        /// <summary>
        /// [fields] Перечень полей метаданных для выборки через запятую, доступные поля: "метатип,индекс,ид,идродителя,имяродителя,имя,тип,рутип,длина,кдз"
        /// </summary>
        public FuncParameter ПереченьПолей
        {
            get { return _ПереченьПолей; }
            set { if (value.Name == null) value.Name = "fields"; SetProperty(ref _ПереченьПолей, value); }
        }
        /// <summary>
        /// [filter] Условие (фильтр) выборки метаданных, например: тип="Reference"
        /// </summary>
        public FuncParameter Условие
        {
            get { return _Условие; }
            set { if (value.Name == null) value.Name = "filter"; SetProperty(ref _Условие, value); }
        }
        /// <summary>
        /// [sort] Поля сортировки результата, например: ТИП, ИМЯ
        /// </summary>
        public FuncParameter ПоляСортировки
        {
            get { return _ПоляСортировки; }
            set { if (value.Name == null) value.Name = "sort"; SetProperty(ref _ПоляСортировки, value); }
        }
        #endregion
        #endregion

        #region Private Helpers
        protected override string[] BodyProperties() { return new string[] { }; }
        protected override string[] UrlParamsProperties()
        { return new string[] { nameof(ВидМетаданных), nameof(ИмяТаблицы), nameof(ПараметрыЗапроса), nameof(ПереченьПолей), nameof(Условие), nameof(ПоляСортировки), nameof(Формат), nameof(Отладка) }; }
        protected override bool CanExecute(object param)
        {
            return String.IsNullOrEmpty(Филиал.Value) == false && String.IsNullOrEmpty(ВидМетаданных.Value) == false && String.IsNullOrEmpty(ИмяТаблицы.Value) == false;
        }

        protected override string GetCommandHeader()
        {
            return "Получить метаданные";
        }

        #endregion
    }
}