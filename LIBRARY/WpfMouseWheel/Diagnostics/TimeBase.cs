namespace WpfMouseWheel.Diagnostics
{
    using System;
    using System.Diagnostics;

    public class TimeBase
    {
        #region Constants
        public static readonly TimeBase Current = new TimeBase();
        #endregion

        #region Fields
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        #endregion

        #region Queries
        public TimeSpan Elapsed => this._stopwatch.Elapsed;
        #endregion

        #region Methods
        public void Start() { }
        #endregion
    }
}
