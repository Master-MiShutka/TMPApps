using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
    using TMP.Common.NetHelper;
    using TMP.Shared.Commands;
    using EzSbyt;
   

    public class ResultViewerViewModel : ViewModel.AbstractViewModel
    {
        #region Fields
        private IEzSbytServiceFunctionViewModel _parent;

        private ServiceResult _data;
        private FrameworkElement _resultViewerControl;
        private IList _source;

        private bool _showAsText = false;

        #endregion

        #region Constructors

        public ResultViewerViewModel()
        {
            ViewContentAsTextCommand = new DelegateCommand(
                () =>
                {
                    _showAsText = !_showAsText;
                    UpdateUI();
                    OnPropertyChanged("ViewContentAsTextCommand");
                },
                (param) =>
                {
                    return Data.HasData;
                },
                () =>
                {
                    return _showAsText ? (Data.HasData == false ? "<???>" : SelectedFormatType.ToString()) : "Как текст";
                }
                );

            ViewContentInNewWindowCommand = new DelegateCommand(
                () =>
                {
                    View.ContentWindow window = new View.ContentWindow()
                    {
                        Content = GetViewerControl(),
                        Owner = App.Current.MainWindow,
                        Title = "Просмотр результата запроса - размер: " + GetFileSizeHumanReadable(Data.ResultAsString.Length)
                    };
                    window.Show();

              /*if ((data.StartsWith("{") || data.StartsWith("[")) && (data.EndsWith("{") || data.EndsWith("[")))
              {
                  var gen = new JsonClassGenerator();
                  gen.Example = data;
                  gen.CodeWriter = new Common.JsonUtils.JsonClassGenerator.CodeWriters.CSharpCodeWriter();
                  gen.UseProperties = true;
                  gen.MainClass = "EzSbytServiceSqlFuncRequest";
                  gen.UsePascalCase = true;
                  gen.UseNestedClasses = true;
                  gen.SingleFile = true;
                  if (gen == null) return;
                  try
                  {
                      using (var sw = new System.IO.StringWriter())
                      {
                          gen.OutputStream = sw;
                          gen.GenerateClasses();
                          sw.Flush();
                          string result = sw.ToString();
                      }
                  }
                  catch (Exception ex)
                  {
                      App.LogException(ex);
                  }
              }*/
                },
                (param) => Data.HasData,
                "В новом окне");

            ViewContentAsTableCommand = new DelegateCommand(
                () =>
                {
                    View.ContentWindow tableWindow = new View.ContentWindow()
                    {
                        Content = GetDataGridViewer(),
                        Owner = App.Current.MainWindow,
                        Title = "Просмотр результата запроса в виде таблицы - размер: " + GetFileSizeHumanReadable(Data.ResultAsString.Length)
                    };
                    tableWindow.Show();
                },
                (param) => Data.HasData && Source != null,
                "Как таблицу");

            SaveContentCommand = new DelegateCommand(
                () =>
                {
                    TMP.Wpf.CommonControls.Viewers.IStringBasedViewer viewer = ResultViewerControl as TMP.Wpf.CommonControls.Viewers.IStringBasedViewer;
                    if (viewer != null)
                        viewer.SaveContent();
                },
                (param) => Data.HasData,
                "Сохранить в файл ...");

            ExportCommand = new AsynchronousDelegateCommand(
               () =>
               {
                   bool? success = false;
                   _parent.State = Shared.State.Busy;
                   try
                   {
                           success = DataAccess.Export.CreateExcelWorkBookFromCollection(Source, SourceItemType);
                           if (success != null && success == false)
                               App.UIAction(() => App.ShowWarning("Не удалось экспортировать результат запроса.", "Экспорт"));
                           _parent.State = Shared.State.Idle;
                   }
                   catch (Exception e)
                   {
                       _parent.State = Shared.State.Idle;
                       App.ToLogException(e);
                   }                   
               },
               (param) => Data.HasData && Source != null,
               "Экспорт в Excel"
               );

            this.PropertyChanged += ResultViewerViewModel_PropertyChanged;
        }
        public ResultViewerViewModel(IEzSbytServiceFunctionViewModel parent, ServiceResult data, FormatTypes formatType) : this()
        {
            if (parent == null)
                throw new ArgumentNullException("Parent must be not null!");
            _parent = parent;

            SelectedFormatType = formatType;
            _data = data;
            InitSource();
        }
        #endregion

        #region Properties

        public ServiceResult Data
        {
            get { return _data; }
            set
            {
                SetProperty(ref _data, value);
                InitSource();
            }
        }
        public FormatTypes SelectedFormatType { get; }

        public FrameworkElement ResultViewerControl
        {
            get
            {
                return _resultViewerControl;
            }
            set { SetProperty(ref _resultViewerControl, value); }
        }
        public string StatusCodeAsString
        {
            get
            {
                if (Data.StatusCode < 100)
                {
                    switch ((System.Net.WebExceptionStatus)Data.StatusCode)
                    {
                        case System.Net.WebExceptionStatus.ConnectFailure:
                            return "С точкой удаленной службы нельзя связаться на транспортном уровне.";
                        case System.Net.WebExceptionStatus.ConnectionClosed:
                            return "Подключение было преждевременно закрыто.";
                        case System.Net.WebExceptionStatus.MessageLengthLimitExceeded:
                            return "Принято сообщение о превышении заданного ограничения при передаче запроса или приеме ответа сервера.";
                        case System.Net.WebExceptionStatus.NameResolutionFailure:
                            return "Служба разрешения имен не может разрешить имя узла.";
                        case System.Net.WebExceptionStatus.ProtocolError:
                            return "Ответ, принятый от сервера, был завершен, но указал на ошибку на уровне протокола.";
                        case System.Net.WebExceptionStatus.ProxyNameResolutionFailure:
                            return "Служба разрешения имен не может распознать имя узла прокси-сервера.";
                        case System.Net.WebExceptionStatus.ReceiveFailure:
                            return "От удаленного сервера не был получен полный ответ.";
                        case System.Net.WebExceptionStatus.RequestCanceled:
                            return "Запрос был отменен, был вызван метод System.Net.WebRequest.Abort или возникла ошибка, не поддающаяся классификации.";
                        case System.Net.WebExceptionStatus.RequestProhibitedByProxy:
                            return "Этот запрос не разрешен прокси-сервером.";
                        case System.Net.WebExceptionStatus.SendFailure:
                            return "Полный запрос не был передан на удаленный сервер.";
                        case System.Net.WebExceptionStatus.ServerProtocolViolation:
                            return "Ответ сервера не являлся допустимым ответом HTTP.";
                        case System.Net.WebExceptionStatus.Success:
                            return "Ошибок не было.";
                        case System.Net.WebExceptionStatus.Timeout:
                            return "В течение времени ожидания запроса ответ получен не был.";
                        case System.Net.WebExceptionStatus.UnknownError:
                            return "Возникло исключение неизвестного типа.";
                        default:
                            return String.Format("Неизвестная ошибка, код: {0}", Data.StatusCode);
                    }
                }
                else
                    switch ((System.Net.HttpStatusCode)Data.StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                            return "OK - операция выполнена успешно";
                        case System.Net.HttpStatusCode.Found:
                            return "302 - перемещено временно";
                        case System.Net.HttpStatusCode.NotModified:
                            return "304 - не изменялось";
                        case System.Net.HttpStatusCode.BadRequest:
                            return "400 - плохой, неверный запрос";
                        case System.Net.HttpStatusCode.Unauthorized:
                            return "401 - не авторизован";
                        case System.Net.HttpStatusCode.Forbidden:
                            return "403 - запрещено";
                        case System.Net.HttpStatusCode.NotFound:
                            return "404 - не найдено";
                        case System.Net.HttpStatusCode.RequestTimeout:
                            return "408 - истекло время ожидания";
                        case System.Net.HttpStatusCode.LengthRequired:
                            return "411 - необходима длина";
                        case System.Net.HttpStatusCode.RequestEntityTooLarge:
                            return "413 - размер запроса слишком велик";
                        case System.Net.HttpStatusCode.RequestUriTooLong:
                            return "414 - запрашиваемый URI слишком длинный";
                        case System.Net.HttpStatusCode.InternalServerError:
                            return "500 - внутренняя ошибка сервера";
                        case System.Net.HttpStatusCode.NotImplemented:
                            return "501 - не реализовано";
                        case System.Net.HttpStatusCode.BadGateway:
                            return "502 - плохой, ошибочный шлюз";
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                            return "503 - сервис недоступен";
                        case System.Net.HttpStatusCode.GatewayTimeout:
                            return "504 - шлюз не отвечает";
                        default:
                            return String.Format("{0} - {1}", Data.StatusCode, System.Web.HttpWorkerRequest.GetStatusDescription((int)Data.StatusCode));
                    }
            }
        }

        public IList Source
        {
            get { return _source; }
            set
            {
                SetProperty(ref _source, value);
            }
        }

        public Type SourceItemType { get; private set; }

        public ICommand ViewContentAsTableCommand { get; private set; }
        public ICommand ViewContentInNewWindowCommand { get; private set; }
        public ICommand ViewContentAsTextCommand { get; private set; }
        public ICommand SaveContentCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }

        #endregion

        #region Public Methods

        public void UpdateUI()
        {
            ResultViewerControl = GetViewerControl();
        }

        #endregion

        #region Private Helpers
        private string GetFileSizeHumanReadable(long value)
        {
            Wpf.CommonControls.Converters.FileSizeToHumanReadableConverter conv = new Wpf.CommonControls.Converters.FileSizeToHumanReadableConverter();
            return (string)conv.Convert(value, null, null, null);
        }

        private string Datastring
        {
            get
            {
                if (String.IsNullOrEmpty(Data.ResultAsString))
                    return String.Empty;
                else
                    return Data.ResultAsString;
            }
        }

        private FrameworkElement GetTextViewer(string data)
        {
            TextBox textbox = new TextBox()
            {
                Text = data,
                IsReadOnly = true,
                IsReadOnlyCaretVisible = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible
            };
            return textbox;
        }

        private FrameworkElement GetDataGridViewer()
        {
            Controls.PagedDataGrid grid = new Controls.PagedDataGrid();
            grid.View = new PagingCollectionView(Source);
            return grid;
        }

        private FrameworkElement GetViewerControl()
        {
            if (Data.HasData == false)
            {
                return new Controls.ErrorView(Data.Error);
            }

            if (_showAsText)
            {
                return GetTextViewer(Data.ResultAsString);
            }
            else
                switch (SelectedFormatType)
                {
                    case FormatTypes.xml:
                        return new TMP.Wpf.CommonControls.Viewers.XmlViewer(GetPrintXML(Data.ResultAsString));
                    case FormatTypes.json:
                        var c = new TMP.Wpf.CommonControls.Viewers.JsonViewer();
                        c.Load(Data.ResultAsString);
                        return c;
                    case FormatTypes.text:
                        return GetTextViewer(Data.ResultAsString);
                    case FormatTypes.csv:
                        var csv = (TextBox)GetTextViewer(Data.ResultAsString);
                        csv.AcceptsTab = true;
                        csv.FontFamily = new System.Windows.Media.FontFamily("Consolas,Lucida Console,Courier New");
                        return csv;
                    case FormatTypes.native:
                        return GetDataGridViewer();
                    case FormatTypes.webview:
                        return GetDataGridViewer();
                    default:
                        return new TMP.Wpf.CommonControls.NoData();
                }
        }

        private void ResultViewerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Data":
                    if (String.IsNullOrEmpty(Data.ResultAsString) && String.IsNullOrEmpty(Data.Error))
                        ResultViewerControl = new TMP.Wpf.CommonControls.NoData();
                    else
                        if (String.IsNullOrEmpty(Data.ResultAsString) && String.IsNullOrEmpty(Data.Error) == false)
                        ResultViewerControl = new Controls.ErrorView(Data.Error);
                    else
                        UpdateUI();
                    //OnPropertyChanged("ViewContentInNewWindowCommand");
                    break;
                case "Status":
                    OnPropertyChanged("StatusCodeAsString");
                    break;
            }
        }

        private void InitSource()
        {
            try
            {
                // есть ошибка?
                if (Data.Error != null) return;

                if (SelectedFormatType == FormatTypes.json)
                {
                    if (String.IsNullOrEmpty(_data.ResultAsString)) return;

                    var list = EzSbyt.DataParser.CreateListOfObjectsFromJson(_data.ResultAsString);

                    SourceItemType = EzSbyt.DataParser.ObjectType;

                    Source = list;
                }
                if (SelectedFormatType == FormatTypes.text || SelectedFormatType == FormatTypes.native || SelectedFormatType == FormatTypes.webview)
                {
                    if (String.IsNullOrEmpty(_data.ResultAsString)) return;

                    var list = EzSbyt.DataParser.CreateListOfObjectsFromValueTable(_data.ResultAsString);

                    SourceItemType = EzSbyt.DataParser.ObjectType;

                    Source = list;
                }
            }
            catch (Exception ex)
            {
                App.ToLogException(ex);
            }
        }

        private String GetPrintXML(String xml)
        {
            if (String.IsNullOrEmpty(xml))
                return String.Empty;
            /*try
            {
                System.Xml.Linq.XDocument doc = System.Xml.Linq.XDocument.Parse(xml);
                return doc.ToString();
            }
            catch (Exception)
            {
                return xml;
            }*/

            String Result = String.Empty;

            using (System.IO.MemoryStream mStream = new System.IO.MemoryStream())
            {
                System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(mStream, Encoding.UTF8);
                System.Xml.XmlDocument document = new System.Xml.XmlDocument();

                try
                {
                    // Load the XmlDocument with the XML.
                    document.LoadXml(xml);

                    writer.Formatting = System.Xml.Formatting.Indented;

                    // Write the XML into a formatting XmlTextWriter
                    document.WriteContentTo(writer);
                    writer.Flush();
                    mStream.Flush();

                    // Have to rewind the MemoryStream in order to read
                    // its contents.
                    mStream.Position = 0;

                    // Read MemoryStream contents into a StreamReader.
                    System.IO.StreamReader sReader = new System.IO.StreamReader(mStream);

                    // Extract the text from the StreamReader.
                    String FormattedXML = sReader.ReadToEnd();

                    Result = FormattedXML;
                }
                catch (System.Xml.XmlException)
                {
                    return xml;
                }
                finally
                {
                    writer.Close();
                }
            }
            return Result;
        }
        #endregion
    }
}
