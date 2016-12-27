using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TMP.Work.AmperM.TestApp.ViewModel.Funcs
{
    using Shared;
    public class FuncSqlViewModel : BaseFuncViewModel
    {
        #region Fields
        private FuncParameter _ТекстЗапроса = new FuncParameter("q", null);
        private FuncParameter _ПараметрыЗапроса = new FuncParameter("p", null);
        private FuncParameter _ПоляСортировки = new FuncParameter("sort", null);
        private FuncParameter _SQLзапросДополнительнойОбработки = new FuncParameter("aq", null);

        #endregion

        #region Constructors

        public FuncSqlViewModel() : base() { FuncName = "sql"; OnPropertyChanged("ТекстЗапроса"); }

        #endregion

        #region Properties

        #region Требуемые параметры
        /// <summary>
        /// [q] Произвольный текст запроса, например: select * from Справочник.Договоры where номер=$прм_номер
        /// </summary>
        public FuncParameter ТекстЗапроса
        {
            get { return _ТекстЗапроса; }
            set { if (value.Name == null) value.Name = "q"; SetProperty(ref _ТекстЗапроса, value); }
        }
        #endregion
        #region Дополнительные параметры
        /// <summary>
        /// [p] Параметры запроса в формате JSON, например: {"прм_номер": 11}
        /// </summary>
        public FuncParameter ПараметрыЗапроса
        {
            get { return _ПараметрыЗапроса; }
            set { if (value.Name == null) value.Name = "p"; SetProperty(ref _ПараметрыЗапроса, value); }
        }
        /// <summary>
        /// [sort] Поля сортировки результата SQL запроса после объединения данных по филиалам, например: ФИЛИАЛ ВОЗР,НОМЕР УБЫВ
        /// </summary>
        public FuncParameter ПоляСортировки
        {
            get { return _ПоляСортировки; }
            set { if (value.Name == null) value.Name = "sort"; SetProperty(ref _ПоляСортировки, value); }
        }
        /// <summary>
        /// [aq] SQL запрос дополнительной обработки после объединения данных по филиалам, например: выбрать группа,филиал,найдено из т1 итоги сумма(найдено) по группа
        /// </summary>
        public FuncParameter SQLзапросДополнительнойОбработки
        {
            get { return _SQLзапросДополнительнойОбработки; }
            set { if (value.Name == null) value.Name = "aq"; SetProperty(ref _SQLзапросДополнительнойОбработки, value); }
        }
        #endregion

        protected override string[] BodyProperties() { return new string[]
            { nameof(ТекстЗапроса), nameof(ПараметрыЗапроса), nameof(ПоляСортировки), nameof(SQLзапросДополнительнойОбработки),
                nameof(МодификацияФормата), nameof(РазделительСтрок) }; }
        protected override string[] UrlParamsProperties() { return new string[] { nameof(Филиал), nameof(Формат), nameof(Отладка) }; }

        #endregion

        #region Private Helpers

        protected override bool CanExecute(object param)
        {
            return String.IsNullOrEmpty(Филиал.Value) == false && String.IsNullOrEmpty(_ТекстЗапроса.Value) == false;
        }

        protected override string GetCommandHeader()
        {
            return "Выполнить запрос";
        }

        #endregion
    }
}