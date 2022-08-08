namespace TMPApplication
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Raises an event each time a WPF Binding error occurs.
    /// </summary>
    public sealed class BindingErrorListener : IDisposable
    {
        private readonly ObservableTraceListener traceListener;

        static BindingErrorListener()
        {
            // PresentationTraceSources.Refresh();
        }

        public BindingErrorListener()
        {
            this.traceListener = new ObservableTraceListener();
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            PresentationTraceSources.DataBindingSource.Listeners.Add(this.traceListener);
        }

        public void Dispose()
        {
            PresentationTraceSources.DataBindingSource.Listeners.Remove(this.traceListener);
            this.traceListener.Dispose();
        }

        /// <summary>
        /// Event raised each time a WPF binding error occurs
        /// </summary>
        public event Action<string> ErrorCatched
        {
            add { this.traceListener.TraceCatched += value; }
            remove { this.traceListener.TraceCatched -= value; }
        }
    }
}
