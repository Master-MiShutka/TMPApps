using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace TMP.WORK.AramisChetchiki.Model
{
    public class HierarchicalItem
    {
        private ICollection<object> _items = null;

        public HierarchicalItem Parent { get; set; }

        public string Name { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public string Description { get; set; }
        public object Tag { get; set; }
        public object DataContext { get; set; }

        public bool IsCheckable { get; private set; }

        public IEnumerable<object> Items
        {
            get { return _items; }
            set
            {
                if (value == null) return;
                _items = new List<object>();
                foreach (var item in value)
                {
                    if (item is HierarchicalItem)
                        (item as HierarchicalItem).Parent = this;
                    _items.Add(item);
                }
            }
        }

        public ControlTemplate Template { get; set; }

        public bool HasTemplate { get { return Template != null; } }
        public bool HasDataContext { get { return DataContext != null; } }

        public HierarchicalItem() { }

        public HierarchicalItem(string name, ICommand action, bool isCheckable = false)
        {
            Name = name;
            Command = action;
            IsCheckable = isCheckable;
        }
    }
}
