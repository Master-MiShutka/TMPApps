namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Collections.Generic;
    using System.Windows.Input;

    public class HierarchicalItem
    {
        private ICollection<object> items = null;

        public HierarchicalItem Parent { get; set; }

        public string Name { get; set; }

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        public string Description { get; set; }

        public object Tag { get; set; }

        // public object DataContext { get; set; }
        public bool IsCheckable { get; private set; }

        public IEnumerable<object> Items
        {
            get => this.items;
            set
            {
                if (value == null)
                {
                    return;
                }

                this.items = new List<object>();
                foreach (var item in value)
                {
                    if (item is HierarchicalItem)
                    {
                        (item as HierarchicalItem).Parent = this;
                    }

                    this.items.Add(item);
                }
            }
        }

        // public ControlTemplate Template { get; set; }

        // public bool HasTemplate => Template != null;
        //        public bool HasDataContext => DataContext != null;
        public HierarchicalItem()
        {
        }

        public HierarchicalItem(string name, ICommand action, bool isCheckable = false)
        {
            this.Name = name;
            this.Command = action;
            this.IsCheckable = isCheckable;
        }
    }
}
