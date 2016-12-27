using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace TMP.Work.AmperM.TestApp.ViewModel
{
  using Shared;
  using Shared.Commands;
  /// <summary>
  /// Базовый абстрактный класс c реализацией интерфейса IWaitableObject
  /// </summary>
  public abstract class AbstractViewModelWaitable : IWaitableObject, INotifyPropertyChanged, IDisposable
  {
    #region Constructor

    protected AbstractViewModelWaitable()
    {
    }

    #endregion // Constructor
    #region DisplayName
    /// <summary>
    /// Возвращает название объекта
    /// </summary>
    public virtual string DisplayName { get; set; }
    #endregion // DisplayName

    #region INotifyPropertyChanged Members

    #region Debugging Aides
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
          throw new Exception(msg);
        else
          Debug.Fail(msg);
      }
    }
    protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

    #endregion // Debugging Aides

    public event PropertyChangedEventHandler PropertyChanged;
    public bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
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

    #endregion // INotifyPropertyChanged Members

    #region IDisposable Members
    public void Dispose()
    {
      this.OnDispose();
    }
    protected virtual void OnDispose()
    {
    }

#if DEBUG
    /// <summary>
    /// Useful for ensuring that ViewModel objects are properly garbage collected.
    /// </summary>
    ~AbstractViewModelWaitable()
    {
      string msg = string.Format("{0} ({1}) ({2}) Finalized", this.GetType().Name, this.DisplayName, this.GetHashCode());
      System.Diagnostics.Debug.WriteLine(msg);
    }
#endif

    #endregion // IDisposable Members

    #region | implementation ICancelable |
    public bool CanCanceled
    {
      get
      {
        return CancelCommand != null ? CancelCommand.CanExecute(null) : false;
      }
    }
    private ICommand _cancelCommand;
    public ICommand CancelCommand
    {
      get
      {
        return _cancelCommand;
      }

      set
      {
        SetProperty(ref _cancelCommand, value);
      }
    }

    private bool _isCanceled = false;
    public bool IsCanceled
    {
      get
      {
        return _isCanceled;
      }

      set
      {
        SetProperty(ref _isCanceled, value);
      }
    }
    #endregion

    #region | implementation IStateObject |
    private State _state = State.Idle;
    public State State
    {
      get
      {
        return _state;
      }

      set
      {
        SetProperty(ref _state, value);
      }
    }
    private int _progress = 0;
    public int Progress
    {
      get
      {
        return _progress;
      }

      set
      {
        SetProperty(ref _progress, value);
      }
    }
    private string _log = string.Empty;
    public string Log
    {
      get
      {
        return _log;
      }

      set
      {
        SetProperty(ref _log, value);
      }
    }
    #endregion
  }
}