using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TMP.ElectricMeterComm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<Model.ModbusOverTcpRequest> _modbusOverTcpRequests = null;

        private bool _isConnected = false;
        private bool _isReady = true;

        private TcpClient _tcpClient;

        private byte[] _buffer;

        private const int BUFFER_SIZE = 250;

        private string _request;

        private Model.ModbusOverTcpRequest _lastSend;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //hexEditor.Stream = new MemoryStream(new byte[] { 0 });


            ModbusOverTcpRequests = new ObservableCollection<Model.ModbusOverTcpRequest>();

            DataContext = this;

            this._tcpClient = new TcpClient();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_tcpClient != null)
                _tcpClient.Close();
        }

        #region Private methods

        private void ResetBuffer()
        {
            _buffer = new Byte[BUFFER_SIZE];
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            try
            {
                _tcpClient.Client.EndDisconnect(ar);
                _tcpClient.Close();

                UIAction(() =>
                {
                    MessageBox.Show("Соединение разорвано.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    btnConnectDisconnect.Content = "Подключить";
                });
                IsConnected = false;
            }
            catch (Exception ex)
            {
                UIAction(() => MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        private void UIAction(Action action)
        {
            if (this.Dispatcher.CheckAccess())
                action();
            else
                this.Dispatcher.Invoke(action);
        }

        private void SendAndReceive(object param)
        {
            IsReady = false;
            try
            {
                var text = string.Empty;
                UIAction(() => text = Request);
                if (String.IsNullOrEmpty(text))
                {
                    UIAction(() => MessageBox.Show("Текст запроса не должен быть пустым", "ERROR", MessageBoxButton.OK, MessageBoxImage.Warning));
                    return;
                }

                string[] strbuffer = text.Split(new char[] { ' ' });
                List<byte> sendBytesList = new List<byte>();
                sendBytesList.Add(9); // признак режима
                sendBytesList.Add(3); // скорость обмена: 0 - настройки не изменяются и следующие 2 байта игнорируются, 1 - 1200, 3 - 9600, 4 - 19200, 7 - 115200
                sendBytesList.Add(0); // длина посылки: 0 - 8 бит, 1 - 7 бит
                sendBytesList.Add(1); // младшая тетрада - контроль паритета(0,1,2): 0 - нет контроля, 1 - even, 2 - odd; старшая тетрада: 0 - 1 стоп бит, 1 - 2 стоп бита
                try
                {
                    for (int i = 0; i < strbuffer.Length; i++)
                    {
                        sendBytesList.Add(Convert.ToByte(strbuffer[i], 16));
                    }
                }
                catch (Exception ex)
                {
                    UIAction(() => MessageBox.Show("Текст запроса должен быть в шестнадцатиричном формате!\n" + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Warning));
                    return;
                }

                _lastSend = new Model.ModbusOverTcpRequest();
                _lastSend.SetSendBytes(sendBytesList.ToArray());
                UIAction(() => ModbusOverTcpRequests.Add(_lastSend));

                byte[] buffer = new byte[BUFFER_SIZE];
                IModbusMaster mmaster = ModbusSerialMaster.CreateRtu(this._tcpClient);

                SocketError Error;
                _tcpClient.Client.Send(sendBytesList.ToArray(), 0, sendBytesList.Count, SocketFlags.None, out Error);
                if (Error > SocketError.Success)
                {
                    if (Error == SocketError.NotConnected || Error == SocketError.ConnectionReset || Error == SocketError.ConnectionAborted || Error == SocketError.ConnectionRefused)
                    {
                        UIAction(() => MessageBox.Show("Ошибка соединения", "", MessageBoxButton.OK, MessageBoxImage.Information));
                    }
                }

                Action<string> setError = (msg) =>
                {
                    _lastSend.ReceivedAsHex = msg;
                    _lastSend.ReceivedAsText = String.Empty;
                };

                ResetBuffer();
                int timeout = _tcpClient.Client.ReceiveTimeout;
                int received = 0;
                int needToReceive = 16;
                int delay = 100;
                while (timeout >= 0)
                {
                    System.Threading.Thread.Sleep(delay);
                    if (_tcpClient.Available == 0 && timeout <= 0)
                    {
                        setError("истекло время ожидания");
                        break;
                    }
                    else
                    {
                        int bytesRead = _tcpClient.Client.Receive(_buffer, received, _tcpClient.Client.Available, SocketFlags.None, out Error);
                        if (bytesRead > 0 && Error == SocketError.Success)
                        {
                            received += bytesRead;
                            if (received >= needToReceive)
                                break;
                        }
                        else
                            if (Error == SocketError.TimedOut)
                        {
                            setError("истекло время ожидания");
                            break;
                        }
                        else
                        {
                            setError("ошибка: " + Error.ToString());
                            break;
                        }
                    }
                    timeout -= delay;
                }
                if (received > 0)
                {
                    byte[] tmparray = new byte[received];
                    _buffer.ToList<byte>().CopyTo(0, tmparray, 0, received);
                    _lastSend.SetRecivedBytes(tmparray);
                }
            }
            catch (Exception ex)
            {
                UIAction(() => MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error));
            }
            finally
            {
                IsReady = true;
            }
        }

        private void SetCommandBytes(byte[] data)
        {
            byte[] crc = CRCHelper.GetCRC16(data);

            List<byte> req = new List<byte>();
            req.AddRange(data);
            req.AddRange(crc);

            StringBuilder sb = new StringBuilder();
            foreach (var s in req)
                sb.AppendFormat("{0:X2} ", s);

            Request = sb.ToString().Trim();
        }

        #endregion

        #region Events

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            IsConnected = false;
            if (_tcpClient.Connected)
            {
                _tcpClient.Client.BeginDisconnect(false, new AsyncCallback(this.DisconnectCallback), _tcpClient);
            }
            else
            {
                _tcpClient.ReceiveTimeout = Convert.ToInt32(this.tbTimeOut.Text) * 1000;
                _tcpClient.SendTimeout = Convert.ToInt32(this.tbTimeOut.Text) * 1000;
                _tcpClient.ReceiveBufferSize = 1024;
                (sender as Button).Content = "Отключить";

                try
                {
                    (sender as Button).IsEnabled = false;
                    await _tcpClient.ConnectAsync(this.tbAddress.Text, Convert.ToInt32(this.tbPort.Text));
                    (sender as Button).IsEnabled = true;
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    (sender as Button).IsEnabled = true;
                    IsConnected = false;
                    (sender as Button).Content = "Подключить";
                }
            }
        }

        private void GetCounterType_Click(object sender, RoutedEventArgs e)
        {
            SetCommandBytes(new byte[6] { Convert.ToByte(tbCounterAddr.Text), 3, 17, 0, 0, 0 });
        }

        private void GetCounterNumber_Click(object sender, RoutedEventArgs e)
        {
            SetCommandBytes(new byte[6] { Convert.ToByte(tbCounterAddr.Text), 3, 18, 0, 0, 0 });
        }

        private void AnyCounter_Click(object sender, RoutedEventArgs e)
        {
            //SetCommandBytes(new byte[4] { 0xf, 0, 0, 0 });

            List<byte> req = new List<byte>();
            req.AddRange(new byte[7] { 0xf, 3, 0, 0, 0, 0xD8, 0x41 });

            StringBuilder sb = new StringBuilder();
            foreach (var s in req)
                sb.AppendFormat("{0:X2} ", s);

            Request = sb.ToString().Trim();
        }

        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(SendAndReceive));
        }

        private void ClearRequest_Click(object sender, RoutedEventArgs e)
        {
            Request = string.Empty;
        }
        #endregion

        #region Properties

        public ObservableCollection<Model.ModbusOverTcpRequest> ModbusOverTcpRequests
        {
            get { return _modbusOverTcpRequests; }
            private set { SetProperty(ref _modbusOverTcpRequests, value, "ModbusOverTcpRequests"); }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            private set { SetProperty(ref _isConnected, value, "IsConnected"); OnPropertyChanged("TcpParamPanelEnabled"); }
        }
        public bool TcpParamPanelEnabled
        {
            get { return !_isConnected; }
        }

        public string Request
        {
            get { return _request; }
            set { SetProperty(ref _request, value, "Request"); }
        }

        public bool IsReady
        {
            get { return _isReady; }
            set { SetProperty(ref _isReady, value, "IsReady"); }
        }

        #endregion

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

        public bool SetProperty<T>(ref T storage, T value, string propertyName)
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
