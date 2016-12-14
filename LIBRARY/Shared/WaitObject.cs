using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace TMP.Shared
{
    public class WaitObject : IDisposable
    {
        private Cursor _previousCursor;
        private IStateObject _stateObject;

        public WaitObject(IStateObject sender)
        {
            _stateObject = sender;
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
            _stateObject.State = State.Busy;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
            _stateObject.State = State.Idle;
        }

        #endregion
    }
}