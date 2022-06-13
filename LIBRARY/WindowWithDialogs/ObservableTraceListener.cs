namespace WindowWithDialogs
{
    using System;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// A TraceListener that raise an event each time a trace is written
    /// </summary>
    internal sealed class ObservableTraceListener : TraceListener
    {
        private StringBuilder buffer = new StringBuilder();

        public override void Write(string message)
        {
            this.buffer.Append(message);
        }

        [DebuggerStepThrough]
        public override void WriteLine(string message)
        {
            this.buffer.Append(message);
            string msg = this.buffer.ToString();

            Debug.WriteLine(msg, "Binding error >> ");

            if (this.TraceCatched != null)
            {
                this.TraceCatched(msg);
            }

            this.buffer.Clear();
        }

        public event Action<string> TraceCatched;
    }
}
