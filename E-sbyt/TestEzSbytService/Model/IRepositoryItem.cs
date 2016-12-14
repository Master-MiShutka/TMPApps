using System;
using System.Collections.ObjectModel;

namespace TMP.Work.AmperM.TestApp.Model
{
    public interface IRepositoryItem : System.ComponentModel.INotifyPropertyChanged
    {
        RepositoryItemType Type { get; }
        DateTime AddedDate { get; }
        string Title { get; set; }
        ObservableCollection<IRepositoryItem> Items { get; set; }

        string Description { get; set; }

        IRepositoryItem Parent { get; set; }

        IRepositoryItem Clone();
        void Update(IRepositoryItem item);
    }
}
