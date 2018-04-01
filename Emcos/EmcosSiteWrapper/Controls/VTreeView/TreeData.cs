using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;

namespace TMP.Work.Emcos.Controls.VTreeView
{
    public class TreeData : INotifyPropertyChanged
    {
        private AsyncObservableCollection<ITreeNode> items = new AsyncObservableCollection<ITreeNode>();
        public AsyncObservableCollection<ITreeNode> Items { get { return items; } }

        public void AddRootItem(ITreeNode item)
        {
            item.Level = 0;
            // чтобы отловить изменение IsExpanded
            item.PropertyChanged += new PropertyChangedEventHandler(TN_PropertyChanged);
            this.items.Add(item);

            if (item.IsExpanded)
            {
                PopulateChildren(item);
            }
        }
        public void AddRootItems(IEnumerable<ITreeNode> nodes)
        {
            var myEnumerator = nodes.GetEnumerator();

            while (myEnumerator.MoveNext())
            {
                AddRootItem(myEnumerator.Current);
            }
        }

        public void ClearAll()
        {
            this.items.Clear();
        }

        private void PopulateChildren(ITreeNode item)
        {
            if (items.Contains(item))
            {
                int index = this.items.IndexOf(item);
                // если это последний в списке или уровень следующего не больше
                if (index == this.items.Count - 1 || this.items[index + 1].Level <= item.Level)
                {
                    IEnumerable children = item.Children;
                    if (children == null)
                        return;
                    int offset = 0;
                    foreach (ITreeNode child in children)
                    {
                        child.PropertyChanged += new PropertyChangedEventHandler(TN_PropertyChanged);
                        child.Level = item.Level + 1;
                        this.items.Insert(index + offset + 1, child);

                        offset++;
                    }

                    foreach (ITreeNode child in children)
                    {
                        if ((child.IsExpanded) && (item.HasChildren))
                        {
                            PopulateChildren(child);
                        }
                    }
                }
            }
        }

        void TN_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = (ITreeNode)sender;
            if (e.PropertyName == "IsExpanded" && node.State == TreeNodeState.Ready)
            {                
                OnExpanding(node);
                if (node.IsExpanded)
                    this.PopulateChildren(node);
                else
                    this.ClearChildren(node);
                OnExpandet(node);
            }
            if (e.PropertyName == "State")
            {
                if (node.State == TreeNodeState.ChildrenPrepared)
                {
                    if (node.IsExpanded)
                        this.PopulateChildren(node);
                    else
                        this.ClearChildren(node);
                    node.State = TreeNodeState.Ready;
                }
            }
        }

        private void ClearChildren(ITreeNode TN)
        {
            if (items.Contains(TN))
            {
                int indexToRemove = this.items.IndexOf(TN) + 1;
                while ((indexToRemove < this.items.Count) && (this.items[indexToRemove].Level > TN.Level))
                {
                    items[indexToRemove].PropertyChanged -= new PropertyChangedEventHandler(TN_PropertyChanged);
                    items.RemoveAt(indexToRemove);
                }
            }
        }
        
        public event NodeExpandEventHandler BeforeNodeExpanded;
        public event NodeExpandEventHandler AfterNodeExpanded;

        protected void OnExpanding(ITreeNode TN)
        {
            var before_handler = BeforeNodeExpanded;
            if (before_handler != null)
                before_handler(this, new NodeExpandEventArgs(TN));
        }
        protected void OnExpandet(ITreeNode TN)
        {
            var after_handler = AfterNodeExpanded;
            if (after_handler != null)
                after_handler(this, new NodeExpandEventArgs(TN));
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
