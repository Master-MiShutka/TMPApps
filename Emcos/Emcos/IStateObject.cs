using System;
using System.ComponentModel;

namespace TMP.Work.Emcos
{
    public interface IStateObject : INotifyPropertyChanged
    {
        State State { get; set; }
        int Progress { get; set; }
        string Log { get; set; }
    }
}
