using QuickGraph;
using System.ComponentModel;
using System.Diagnostics;

namespace TMP.DWRES.Graph
{
    [DebuggerDisplay("{Source.ID} -> {Target.ID}")]
    public class FiderGraphEdge : Edge<FiderGraphVertex>, INotifyPropertyChanged
    {
        private int id;
        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("ID");
            }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public FiderGraphEdge(int id, FiderGraphVertex source, FiderGraphVertex target)
            : base(source, target)
        {
            ID = id;
            Name = string.Format("{0}-{1}", source.ID, target.ID);
        }

        public override string ToString()
        {
            return Name;
        }
        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
