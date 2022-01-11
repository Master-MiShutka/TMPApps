namespace TMP.Shared
{
    using System;
    using System.Windows.Input;

    public class WaitObject : IDisposable
    {
        private Cursor previousCursor;
        private IStateObject stateObject;

        public WaitObject(IStateObject sender)
        {
            this.stateObject = sender;
            this.previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
            this.stateObject.State = State.Busy;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = this.previousCursor;
            this.stateObject.State = State.Idle;
        }

        #endregion
    }
}