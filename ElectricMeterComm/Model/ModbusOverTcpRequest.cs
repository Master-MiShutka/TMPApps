using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TMP.ElectricMeterComm.Model
{
    public class ModbusOverTcpRequest : INotifyPropertyChanged
    {
        private string _receivedAsHex, _receivedAsText;
        private byte[] sended, received;

        public void SetSendBytes(byte[] value)
        {
            sended = value;

            string text = string.Empty, hex = string.Empty;
            ParseBytes(sended, out text, out hex);
            SendedAsText = text;
            SendedAsHex = hex;

            OnPropertyChanged("SendedAsHex");
            OnPropertyChanged("SendedAsText");
        }
        public void SetRecivedBytes(byte[] value)
        {
            received = value;
            string text, hex;
            ParseBytes(received, out text, out hex);
            ReceivedAsText = text;
            ReceivedAsHex = hex;

            OnPropertyChanged("ReceivedAsHex");
            OnPropertyChanged("ReceivedAsText");
        }

        public string SendedAsHex { get; set; }
        public string SendedAsText { get; set; }
        public string ReceivedAsHex
        {
            get { return _receivedAsHex; }
            set { _receivedAsHex = value; OnPropertyChanged("ReceivedAsHex"); }
        }
        public string ReceivedAsText
        {
            get { return _receivedAsText; }
            set { _receivedAsText = value; OnPropertyChanged("ReceivedAsText"); }
        }
        private void ParseBytes(byte[] bytes, out string outText, out string outHex)
        {
            StringBuilder asText = new StringBuilder();
            StringBuilder asHex = new StringBuilder();
            int pos = 0;
            while (pos < bytes.Length)
            {
                asText.Append(Convert.ToChar(bytes[pos]));
                asHex.Append(bytes[pos].ToString("X2"));
                asHex.Append(" ");
                pos++;
                if (pos % 16 == 0)
                {
                    asText.Append(Environment.NewLine);
                    asHex.Append(Environment.NewLine);
                }
            }
            outText = asText.ToString();
            outHex = asHex.ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
    }
}
