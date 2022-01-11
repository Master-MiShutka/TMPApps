namespace TMP.ElectricMeterComm.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;

    public class ModbusOverTcpRequest : INotifyPropertyChanged
    {
        private string receivedAsHex;
        private string receivedAsText;
        private byte[] sended;
        private byte[] received;

        public void SetSendBytes(byte[] value)
        {
            this.sended = value;

            string text = string.Empty, hex = string.Empty;
            this.ParseBytes(this.sended, out text, out hex);
            this.SendedAsText = text;
            this.SendedAsHex = hex;

            this.OnPropertyChanged(nameof(this.SendedAsHex));
            this.OnPropertyChanged(nameof(this.SendedAsText));
        }

        public void SetRecivedBytes(byte[] value)
        {
            this.received = value;
            string text, hex;
            this.ParseBytes(this.received, out text, out hex);
            this.ReceivedAsText = text;
            this.ReceivedAsHex = hex;

            this.OnPropertyChanged(nameof(this.ReceivedAsHex));
            this.OnPropertyChanged(nameof(this.ReceivedAsText));
        }

        public string SendedAsHex { get; set; }

        public string SendedAsText { get; set; }

        public string ReceivedAsHex
        {
            get => this.receivedAsHex;

            set
            {
                this.receivedAsHex = value;
                this.OnPropertyChanged(nameof(this.ReceivedAsHex));
            }
        }

        public string ReceivedAsText
        {
            get => this.receivedAsText;

            set
            {
                this.receivedAsText = value;
                this.OnPropertyChanged(nameof(this.ReceivedAsText));
            }
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
