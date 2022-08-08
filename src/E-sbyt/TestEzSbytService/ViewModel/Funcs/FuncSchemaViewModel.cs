using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TMP.Work.AmperM.TestApp.ViewModel.Funcs
{
    public class FuncSchemaViewModel : BaseFuncViewModel
    {
        #region Fields

        private FuncParameter _НомерДоговора = new FuncParameter("dogovor", null);
        private ImageSource _Схема;

        #endregion Fields

        #region Constructors

        public FuncSchemaViewModel() : base()
        {
            FuncName = "schema";
        }

        #endregion Constructors

        #region Properties

        #region Требуемые параметры
        /// <summary>
        /// [dogovor] № договора абонента, например: 4.
        /// </summary>
        public FuncParameter НомерДоговора
        {
            get { return _НомерДоговора; }
            set {
                if (value.Name == null)
                    value.Name = "dogovor";
                SetProperty(ref _НомерДоговора, value); }
        }

        #endregion Требуемые параметры

        public ImageSource Схема
        {
            get { return _Схема; }
            set { _Схема = value; OnPropertyChanged("Схема"); }
        }

        #endregion Properties

        #region Public Methods

        public override void Execute(object param)
        {
            IEzSbytServiceFunctionViewModel parent = param as IEzSbytServiceFunctionViewModel;
            if (parent == null)
                throw new ArgumentException("Param is not IResultViewerViewModel");

            TMP.Common.NetHelper.ServiceResult result = EzSbyt.EzSbytService.Instance.FuncRequest(this.FuncName, this.GetUrlParams(), this.GetEscapedBody());

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream(result.ResultBytes))
            {
                Схема = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }

        #endregion Public Methods

        #region Private Helpers

        protected override string[] BodyProperties()
        {
            return new string[] { };
        }

        protected override string[] UrlParamsProperties()
        {
            return new string[] { nameof(Филиал), nameof(НомерДоговора) };
        }

        protected override bool CanExecute(object param)
        {
            return String.IsNullOrEmpty(_НомерДоговора.Value) == false;
        }

        protected override string GetCommandHeader()
        {
            return "Получить схему";
        }

        #endregion Private Helpers
    }
}