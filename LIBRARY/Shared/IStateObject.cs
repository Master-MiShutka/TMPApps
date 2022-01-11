namespace TMP.Shared
{
    using System.ComponentModel;

    public interface IStateObject : INotifyPropertyChanged
    {
        State State { get; set; }

        int Progress { get; set; }

        string Log { get; set; }
    }
}
