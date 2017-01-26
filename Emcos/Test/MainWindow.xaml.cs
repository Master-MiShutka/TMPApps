using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace Test
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using TMP.Wpf.Common.Controls.TableView;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Task _task;
        public List<TestClass> Items { get; set; }
        private System.Data.DataView _items2;

        private string _responseStatus;
        private Test.EmcosServiceReference.ServiceSoapClient client;

        public MainWindow()
        {
            client = new EmcosServiceReference.ServiceSoapClient("ServiceSoap", new System.ServiceModel.EndpointAddress("http://10.96.18.16/testWebService/Service.asmx"));

            InitializeComponent();

            Items = new List<TestClass>();
            Random rnd = new Random();
            for (int i = 0; i < 100000; i++)
                Items.Add(new TestClass()
                {
                    Date = DateTime.Now.AddDays(rnd.NextDouble()*10d),
                    VvodaIn = rnd.NextDouble()* rnd.NextDouble() + rnd.NextDouble()*100d
                });

            var table = client.GetPointInfo("PSDTU_SERVER", "");
            if (table != null && table.DefaultView != null)
                Items2 = table.DefaultView;

            DataContext = this;
        }

        public System.Data.DataView Items2
        {
            get { return _items2; }
            private set { SetProperty(ref _items2, value); }
        }

        private void ButtonGet_Click(object sender, RoutedEventArgs e)
        {
            string[] _pointCodes = txtPointIds.Text.Split(new char[] { '|' });

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (var point in _pointCodes)
                sb.AppendFormat("{{id:{0}}},", point);
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");

            var table = client.GetSTPLData("PSDTU_SERVER", "[{id:3644},{id:3646},{id:3648},{id:3650}]", "[{id:385},{id:386}]", "01.12.2016 00:00:00", "01.01.2017 00:00:00");
            Items2 = table.DefaultView;

            /*Stopwatch s = new Stopwatch();
            s.Start();
            System.Data.DataTable table = ExecuteGetSTPLDataFunction(sb.ToString(), new int[] { 385, 386 }, new DateTime(2016, 12, 1), new DateTime(2017, 1, 1));
            s.Stop();
            MessageBox.Show(s.ElapsedTicks.ToString());

            Items2 = table.DefaultView;*/
        }

        private System.Data.DataTable ExecuteGetSTPLDataFunction(string ppointIds, int[] mlIds, DateTime start, DateTime end)
        {
            System.Data.DataTable table = new System.Data.DataTable();

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (var point in mlIds)
                sb.AppendFormat("{{id:{0}}},", point);
            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");

            string body = String.Format(@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">" +
                @"<GetSTPLData xmlns=""http://ksmlab.ru/""><UserName>PSDTU_SERVER</UserName>" +
                @"<PPOINT_CODE_ID>{0}</PPOINT_CODE_ID><PML_ID>{1}</PML_ID>" +
                @"<PBT>{2}</PBT><PET>{3}</PET></GetSTPLData></s:Body></s:Envelope>",
                ppointIds,
                sb.ToString(),
                start.ToString("dd.MM.yyyy hh:mm:ss"),
                end.ToString("dd.MM.yyyy hh:mm:ss"));

            HttpWebResponse response;
            string responseText;
            if (SendEmcosWebServiceRequest(body, out response))
            {
                //Success, possibly use response.
                responseText = ReadResponse(response);
                response.Close();

                if (String.IsNullOrEmpty(responseText))
                {
                    MessageBox.Show("Произошла ошибка:\n" + responseText, "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                XmlDocument xd = new XmlDocument();
                xd.LoadXml(responseText);
                XmlNode xn = xd.DocumentElement;
                XmlNode result = xn.SelectSingleNode("ROWDATA");

                System.Data.DataSet ds = new System.Data.DataSet();
                ds.ReadXml(result.InnerXml);

                table = ds.Tables[0];
                MessageBox.Show(_responseStatus, "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                //Failure, cannot use response.
                MessageBox.Show("Произошла ошибка:\n" + _responseStatus, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return table;
        }

        private bool SendEmcosWebServiceRequest(string postData, out HttpWebResponse response)
        {
            response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://10.96.18.16/testWebService/Service.asmx");
                request.Method = "POST";

                request.ContentType = "text/xml; charset=utf-8";
                request.Headers.Add("SOAPAction", @"""http://ksmlab.ru/GetSTPLData""");
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate");

                // Disable 'Expect: 100-continue' behavior. More info: http://haacked.com/archive/2004/05/15/http-web-request-expect-100-continue.aspx
                request.ServicePoint.Expect100Continue = false;

                //Set request body.
                byte[] postBytes = System.Text.Encoding.UTF8.GetBytes(postData);
                request.ContentLength = postBytes.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(postBytes, 0, postBytes.Length);
                stream.Close();

                //Get response to request.
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                //ProtocolError indicates a valid HTTP response, but with a non-200 status code (e.g. 304 Not Modified, 404 Not Found)
                if (e.Status == WebExceptionStatus.ProtocolError)
                    response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception ex)
            {
                if (response != null) response.Close();
                _responseStatus = ex.Message;
                return false;
            }
            return true;
        }
        private string ReadResponse(HttpWebResponse response)
        {
            _responseStatus = String.Format("Статус: {0}, получено байт: {1}\r\n", ((HttpWebResponse)response).StatusDescription,
                                                 response.ContentLength);
            try
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    Stream streamToRead = responseStream;
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                    }

                    using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                _responseStatus = ex.Message;
                return string.Empty;
            }
        }



        private void Create_tableColumns()
        {
            table.Columns.Clear();

            DataTemplate datecell = this.TryFindResource("dateTableViewCellTemplate") as DataTemplate;
            if (datecell == null)
                throw new NullReferenceException("Не найден шаблон: dateTableViewCellTemplate");
            DataTemplate textCell = this.TryFindResource("textTableViewCellTemplate") as DataTemplate;
            if (textCell == null)
                throw new NullReferenceException("Не найден шаблон: textTableViewCellTemplate");
            DataTemplate columnTitleTemplate = this.TryFindResource("tableViewColumnHeaderDataTemplate") as DataTemplate;
            if (columnTitleTemplate == null)
                throw new NullReferenceException("Не найден шаблон: tableViewColumnHeaderDataTemplate");

            System.Collections.ObjectModel.ObservableCollection<TableViewColumn> columns = new System.Collections.ObjectModel.ObservableCollection<TableViewColumn>();

            columns.Add(new TableViewColumn()
            {
                Title = "Data",
                ContextBinding = new Binding("Date"),
                CellTemplate = datecell
            });
            columns.Add(new TableViewColumn()
            {
                Title = "Header 1",
                ContextBinding = new Binding("VvodaIn"),
                Width = 110,
                CellTemplate = textCell,
                UseHistogramm = true,
                ShowHistogramm = true
            });
            foreach (TableViewColumn column in columns)
                column.TitleTemplate = columnTitleTemplate;
            table.Columns = columns;
        }

        public class TestClass
        {
            public DateTime Date { get; set; }
            public double VvodaIn { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            table.BeginInit();
            _task = Task.Factory.StartNew(() =>
            {
                //System.Threading.Thread.Sleep(5000);
                table.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                    new Action(delegate ()
                    {
                        Create_tableColumns();
                        table.EndInit();
                        Cursor = Cursors.Arrow;
                    }));
            });

            _task.ContinueWith((t) =>
            {
                MessageBox.Show("Произошла ошибка.\n" + t.Exception.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ggtfecgfe", Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
        }

        #region INotifyPropertyChanged Members

        #region Debugging Aides

        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

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
        #endregion Debugging Aides

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

        #endregion INotifyPropertyChanged Members
    }
}
