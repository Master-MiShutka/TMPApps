using System;
using System.Diagnostics;
using System.Text;

namespace TMPApplication
{
    /// <summary>
    /// A TraceListener that raise an event each time a trace is written
    /// </summary>
    sealed class ObservableTraceListener : TraceListener
    {
        StringBuilder buffer = new StringBuilder();

        public override void Write(string message)
        {
            buffer.Append(message);
        }

        [DebuggerStepThrough]
        public override void WriteLine(string message)
        {
            buffer.Append(message);
            string msg = buffer.ToString();

            Debug.WriteLine(msg, "Binding error >> ");

            if (TraceCatched != null)
            {
                TraceCatched(msg);
            }

            buffer.Clear();
        }

        public event Action<string> TraceCatched;
    }
}
