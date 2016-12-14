using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TMP.Work.AmperM.TestApp.ViewModel.Funcs
{
    using Shared.Commands;
    public abstract class BaseFuncViewModel : INotifyPropertyChanged, IFuncViewModel
    {
        #region Fields

        private FuncParameter _Филиал = new FuncParameter("f", null);

        private FuncParameter _Формат = new FuncParameter("format", "json");
        private FuncParameter _МодификацияФормата = new FuncParameter("mode", null);
        private FuncParameter _РазделительСтрок = new FuncParameter("rowDelimiter", null);
        private FuncParameter _РазделительКолонок = new FuncParameter("colDelimiter", null);
        private FuncParameter _callbackФункция = new FuncParameter("callback", null);
        private FuncParameter _Отладка = new FuncParameter("debug", null);

        #endregion

        #region Constructors

        public BaseFuncViewModel()
        {
            GetCommand = new AsynchronousDelegateCommand(
                (param) =>
                {
                    Execute(param);
                },
                (p) => CanExecute(p),
                GetCommandHeader);

            Отладка = new FuncParameter("debug", "1");

            Филиал = new FuncParameter("f", Properties.Settings.Default.Filial);

            Properties.Settings.Default.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Filial")
                Филиал = new FuncParameter("f", Properties.Settings.Default.Filial);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Название функции
        /// </summary>
        public string FuncName { get; set; }
        /// <summary>
        /// [f] Филиал, к базе данных которого выполняется запрос
        /// </summary>
        public FuncParameter Филиал
        {
            get { return _Филиал; }
            set { if (value.Name == null) value.Name = "f"; SetProperty(ref _Филиал, value); }
        }
        /// <summary>
        /// [format] Формат результата. Вы можете выбрать один из следующих: xml (default), json, csv, native
        /// </summary>
        public FuncParameter Формат
        {
            get { return _Формат; }
            set { if (value.Name == null) value.Name = "format"; SetProperty(ref _Формат, value); }
        }
        /// <summary>
        /// [mode] Вариант выбранного формата: default (default), elements, attributes, array, metaarray
        /// </summary>
        public FuncParameter МодификацияФормата
        {
            get { return _МодификацияФормата; }
            set { if (value.Name == null) value.Name = "mode"; SetProperty(ref _МодификацияФормата, value); }
        }
        /// <summary>
        /// [rowDelimiter] Разделитель строк, если задан csv формат (по умролчанию "\n")
        /// </summary>
        public FuncParameter РазделительСтрок
        {
            get { return _РазделительСтрок; }
            set { if (value.Name == null) value.Name = "rowDelimiter"; SetProperty(ref _РазделительСтрок, value); }
        }
        /// <summary>
        /// [colDelimiter] Разделитель колонок, если задан csv формат (по умролчанию "\t")
        /// </summary>
        public FuncParameter РазделительКолонок
        {
            get { return _РазделительКолонок; }
            set { if (value.Name == null) value.Name = "colDelimiter"; SetProperty(ref _РазделительКолонок, value); }
        }
        /// <summary>
        /// [callback] Дополнительный параметр, имеет смысл, если выбран json формат
        /// </summary>
        public FuncParameter CallbackФункция
        {
            get { return _callbackФункция; }
            set { if (value.Name == null) value.Name = "callback"; SetProperty(ref _callbackФункция, value); }
        }
        /// <summary>
        /// [debug] Если задан, - результат выводится в "читабельном" виде
        /// </summary>
        public FuncParameter Отладка
        {
            get { return _Отладка; }
            set { if (value.Name == null) value.Name = "debug"; SetProperty(ref _Отладка, value); }
        }
        /// <summary>
        /// Команда для отправки запроса
        /// </summary>
        public ICommand GetCommand { get; private set; }

        /// <summary>
        /// Указывает будет ли запрос передаваться методом POST
        /// </summary>
        public bool HasBody
        {
            get
            {
                string value = GetEscapedBody();
                return (String.IsNullOrEmpty(value) == false && value.Length > 1000);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Возвращает строку, состоящую из пар параметр=значение, разделенных "&"
        /// </summary>
        public override string ToString()
        {
            return GetString(false, false);
        }
        /// <summary>
        /// Возвращает строку, состоящую из пар параметр=значение, разделенных "&",
        /// где значение экранировано с помощью <see cref="EcapeString"/>
        /// </summary>
        public string ToEcapedString()
        {
            return GetString(true, false);
        }
        /// <summary>
        /// Возвращает строку, состоящую из пар параметр=значение, разделенных "&", без параметра Филиал
        /// </summary>
        public string ToStringWithOutFilial()
        {
            return GetString(false, true);
        }
        /// <summary>
        /// Возвращает строку, состоящую из пар параметр=значение, разделенных "&",
        /// где значение экранировано с помощью <see cref="EcapeString"/>, без параметра Филиал
        /// </summary>
        public string ToEcapedStringWithOutFilial()
        {
            return GetString(true, true);
        }
        /// <summary>
        /// Возвращает строку, состоящую из пар параметр=значение, разделенных "&",
        /// где значение экранировано с помощью <see cref="EcapeString"/>
        /// Список параметров задан <see cref="BodyProperties"/>
        /// </summary>
        public string GetEscapedBody()
        {
            if (BodyProperties() == null) return String.Empty;
            if (BodyProperties().Length == 0) return String.Empty;

            PropertyInfo[] propertyInfos = BodyProperties().Select((prop) => this.GetType().GetProperty(prop)).ToArray<PropertyInfo>();

            return GetString(propertyInfos, true, false);
        }
        /// <summary>
        /// Возвращает строку, состоящую из пар параметр=значение, разделенных "&",
        /// где значение экранировано с помощью <see cref="EcapeString"/>
        /// Список параметров задан <see cref="UrlParamsProperties"/>
        /// </summary>
        public string GetUrlParams()
        {
            PropertyInfo[] propertyInfos = UrlParamsProperties().Select((prop) => this.GetType().GetProperty(prop)).ToArray<PropertyInfo>();

            return GetString(propertyInfos, false, false);
        }
        public virtual void Execute(object param)
        {
            IEzSbytServiceFunctionViewModel parent = param as IEzSbytServiceFunctionViewModel;
            if (parent == null)
                throw new ArgumentException("Param is not IResultViewerViewModel");

            EzSbyt.FormatTypes format;
            Enum.TryParse<EzSbyt.FormatTypes>(_Формат.Value, out format);

            var result = EzSbyt.EzSbytService.Instance.FuncRequest(this.FuncName, this.GetUrlParams(), this.GetEscapedBody());
            
            parent.Result = new ResultViewerViewModel(result, format);
                            
        }

        #endregion

        #region Private Helpers
        /// <summary>
        /// Переопределяется в наследуемом классе.
        /// </summary>
        /// <param name="param">Параметр</param>
        /// <returns>Возвращает <c>true</c> если выполнение запроса разрешено, иначе <c>false</c>.</returns>
        protected abstract bool CanExecute(object param);
        /// <summary>
        /// Возвращает название команды для пользоватльского интерфейса
        /// </summary>
        protected abstract string GetCommandHeader();

        private PropertyInfo[] _propertyInfos = null;
        /// <summary>
        /// Список свойств класса, которые будут переданы в теле запроса методом POST
        /// Переопределяется в наследуемом классе.
        /// </summary>
        protected abstract string[] BodyProperties();
        /// <summary>
        /// Список свойств класса, которые будут переданы в запросе методом GET
        /// Переопределяется в наследуемом классе.
        /// </summary>
        protected abstract string[] UrlParamsProperties();

        protected virtual string EcapeString(string value)
        {
            return Uri.EscapeUriString(value);
        }

        private string GetString(PropertyInfo[] propertyInfos, bool escapeValues = false, bool withOutFilial = true)
        {
            StringBuilder sb = new StringBuilder();
            foreach (PropertyInfo info in propertyInfos)
            {
                string name;
                string value;
                if (info.PropertyType == typeof(FuncParameter))
                {
                    FuncParameter param = (FuncParameter)info.GetValue(this, null);
                    if (withOutFilial && param.Name == "f")
                        continue;
                    name = param.Name;
                    value = param.Value;
                }
                else
                {
                    name = info.Name;
                    value = info.GetValue(this, null).ToString();
                }

                if (value != null && String.IsNullOrEmpty(value) == false)
                    if (escapeValues)
                        sb.AppendFormat("&{0}={1}", name, EcapeString(value));
                    else
                        sb.AppendFormat("&{0}={1}", name, value);
            }
            string result = sb.ToString();
            if (result.StartsWith("&"))
                return result.Remove(0, 1);
            else
                return result;
        }

        private string GetString(bool escapeValues = false, bool withOutFilial = true)
        {
            if (_propertyInfos == null)
                _propertyInfos = this.GetType().GetProperties();

            return GetString(_propertyInfos, escapeValues, withOutFilial);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }
        public bool ThrowOnInvalidPropertyName { get; set; }

        #endregion
    }
}