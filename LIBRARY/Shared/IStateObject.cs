using System.ComponentModel;

namespace TMP.Shared
{
    public interface IStateObject : INotifyPropertyChanged
    {
        State State { get; set; }
        int Progress { get; set; }
        string Log { get; set; }
    }
}
