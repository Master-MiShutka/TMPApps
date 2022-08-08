using System.Windows;
using System.Text;
using System.Diagnostics;

namespace TMP.Work.AmperM.TestApp
{
    public class BindingErrorTraceListener : TraceListener
    {
        private readonly StringBuilder _messageBuilder = new StringBuilder();

        public override void Write(string message)
        {
            _messageBuilder.Append(message);
        }

        public override void WriteLine(string message)
        {
            Write(message);

            Debug.WriteLine(_messageBuilder.ToString(), "Binding error");
            _messageBuilder.Clear();
        }
    }
}