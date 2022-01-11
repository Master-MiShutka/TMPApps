namespace TMP.ElectricMeterComm
{
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
    using Modbus.Device;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<Model.ModbusOverTcpRequest> modbusOverTcpRequests = null;

        private bool isConnected = false;
        private bool _isReady = true;

        private TcpClient _tcpClient;

        private byte[] _buffer;

        private const int BUFFER_SIZE = 250;

        private string _request;

        private Model.ModbusOverTcpRequest _lastSend;

        #endregion

        public MainWindow()
        {
            this.InitializeComponent();

            // hexEditor.Stream = new MemoryStream(new byte[] { 0 });
            this.ModbusOverTcpRequests = new ObservableCollection<Model.ModbusOverTcpRequest>();

            this.DataContext = this;

            this._tcpClient = new TcpClient();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (this._tcpClient != null)
            {
                this._tcpClient.Close();
            }
        }

        #region Private methods

        private void ResetBuffer()
        {
            this._buffer = new byte[BUFFER_SIZE];
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            try
            {
                this._tcpClient.Client.EndDisconnect(ar);
                this._tcpClient.Close();

                this.UIAction(() =>
                {
                    MessageBox.Show("Соединение разорвано.", string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                    this.btnConnectDisconnect.Content = "Подключить";
                });
                this.IsConnected = false;
            }
            catch (Exception ex)
            {
                this.UIAction(() => MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        private void UIAction(Action action)
        {
            if (this.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                this.Dispatcher.Invoke(action);
            }
        }

        private void SendAndReceive(object param)
        {
            this.IsReady = false;
            try
            {
                var text = string.Empty;
                this.UIAction(() => text = this.Request);
                if (string.IsNullOrEmpty(text))
                {
                    this.UIAction(() => MessageBox.Show("Текст запроса не должен быть пустым", "ERROR", MessageBoxButton.OK, MessageBoxImage.Warning));
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
                    this.UIAction(() => MessageBox.Show("Текст запроса должен быть в шестнадцатиричном формате!\n" + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Warning));
                    return;
                }

                this._lastSend = new Model.ModbusOverTcpRequest();
                this._lastSend.SetSendBytes(sendBytesList.ToArray());
                this.UIAction(() => this.ModbusOverTcpRequests.Add(this._lastSend));

                byte[] buffer = new byte[BUFFER_SIZE];
                IModbusMaster mmaster = ModbusSerialMaster.CreateRtu(this._tcpClient);

                SocketError Error;
                this._tcpClient.Client.Send(sendBytesList.ToArray(), 0, sendBytesList.Count, SocketFlags.None, out Error);
                if (Error > SocketError.Success)
                {
                    if (Error == SocketError.NotConnected || Error == SocketError.ConnectionReset || Error == SocketError.ConnectionAborted || Error == SocketError.ConnectionRefused)
                    {
                        this.UIAction(() => MessageBox.Show("Ошибка соединения", string.Empty, MessageBoxButton.OK, MessageBoxImage.Information));
                    }
                }

                Action<string> setError = (msg) =>
                {
                    this._lastSend.ReceivedAsHex = msg;
                    this._lastSend.ReceivedAsText = string.Empty;
                };

                this.ResetBuffer();
                int timeout = this._tcpClient.Client.ReceiveTimeout;
                int received = 0;
                int needToReceive = 16;
                int delay = 100;
                while (timeout >= 0)
                {
                    System.Threading.Thread.Sleep(delay);
                    if (this._tcpClient.Available == 0 && timeout <= 0)
                    {
                        setError("истекло время ожидания");
                        break;
                    }
                    else
                    {
                        int bytesRead = this._tcpClient.Client.Receive(this._buffer, received, this._tcpClient.Client.Available, SocketFlags.None, out Error);
                        if (bytesRead > 0 && Error == SocketError.Success)
                        {
                            received += bytesRead;
                            if (received >= needToReceive)
                            {
                                break;
                            }
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
                    this._buffer.ToList<byte>().CopyTo(0, tmparray, 0, received);
                    this._lastSend.SetRecivedBytes(tmparray);
                }
            }
            catch (Exception ex)
            {
                this.UIAction(() => MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error));
            }
            finally
            {
                this.IsReady = true;
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
            {
                sb.AppendFormat("{0:X2} ", s);
            }

            this.Request = sb.ToString().Trim();
        }

        #endregion

        #region Events

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            this.IsConnected = false;
            if (this._tcpClient.Connected)
            {
                this._tcpClient.Client.BeginDisconnect(false, new AsyncCallback(this.DisconnectCallback), this._tcpClient);
            }
            else
            {
                this._tcpClient.ReceiveTimeout = Convert.ToInt32(this.tbTimeOut.Text) * 1000;
                this._tcpClient.SendTimeout = Convert.ToInt32(this.tbTimeOut.Text) * 1000;
                this._tcpClient.ReceiveBufferSize = 1024;
                (sender as Button).Content = "Отключить";

                try
                {
                    (sender as Button).IsEnabled = false;
                    await this._tcpClient.ConnectAsync(this.tbAddress.Text, Convert.ToInt32(this.tbPort.Text));
                    (sender as Button).IsEnabled = true;
                    this.IsConnected = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                    (sender as Button).IsEnabled = true;
                    this.IsConnected = false;
                    (sender as Button).Content = "Подключить";
                }
            }
        }

        private void GetCounterType_Click(object sender, RoutedEventArgs e)
        {
            this.SetCommandBytes(new byte[6] { Convert.ToByte(this.tbCounterAddr.Text), 3, 17, 0, 0, 0 });
        }

        private void GetCounterNumber_Click(object sender, RoutedEventArgs e)
        {
            this.SetCommandBytes(new byte[6] { Convert.ToByte(this.tbCounterAddr.Text), 3, 18, 0, 0, 0 });
        }

        private void AnyCounter_Click(object sender, RoutedEventArgs e)
        {
            // SetCommandBytes(new byte[4] { 0xf, 0, 0, 0 });
            List<byte> req = new List<byte>();
            req.AddRange(new byte[7] { 0xf, 3, 0, 0, 0, 0xD8, 0x41 });

            StringBuilder sb = new StringBuilder();
            foreach (var s in req)
            {
                sb.AppendFormat("{0:X2} ", s);
            }

            this.Request = sb.ToString().Trim();
        }

        private void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.SendAndReceive));
        }

        private void ClearRequest_Click(object sender, RoutedEventArgs e)
        {
            this.Request = string.Empty;
        }
        #endregion

        #region Properties

        public ObservableCollection<Model.ModbusOverTcpRequest> ModbusOverTcpRequests
        {
            get => this.modbusOverTcpRequests;
            private set => this.SetProperty(ref this.modbusOverTcpRequests, value, nameof(this.ModbusOverTcpRequests));
        }

        public bool IsConnected
        {
            get => this.isConnected;

            private set
            {
                this.SetProperty(ref this.isConnected, value, nameof(this.IsConnected));
                this.OnPropertyChanged(nameof(this.TcpParamPanelEnabled));
            }
        }

        public bool TcpParamPanelEnabled => !this.isConnected;

        public string Request
        {
            get => this._request;
            set => this.SetProperty(ref this._request, value, nameof(this.Request));
        }

        public bool IsReady
        {
            get => this._isReady;
            set => this.SetProperty(ref this._isReady, value, nameof(this.IsReady));
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
                {
                    throw new Exception(msg);
                }
                else
                {
                    Debug.Fail(msg);
                }
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
