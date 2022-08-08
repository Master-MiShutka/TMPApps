using System;
namespace TMP.Work.AmperM.TestApp.DataAccess
{
    using Model;
    public class RepositoryItemAddedEventArgs : EventArgs
    {
        public RepositoryItemAddedEventArgs(IRepositoryItem item)
        {
            this.NewRepositoryItem = item;
        }
        public IRepositoryItem NewRepositoryItem { get; private set; }
    }
}
