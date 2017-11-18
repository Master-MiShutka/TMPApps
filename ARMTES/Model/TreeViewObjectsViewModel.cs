using System.Collections.Generic;

namespace TMP.ARMTES.Model
{
    public class TreeViewObjectsViewModel
    {
        public List<TreeViewItem> TreeViewItems { get; set; }

        public TreeViewObjectsViewModel()
        {
            TreeViewItems = new List<TreeViewItem>();
        }
    }
    public class TreeViewItem
    {
        public string label { get; set; }
        public int parentid { get; set; }
        public string value { get; set; }
        public List<TreeViewItem> items { get; set; }
    }
}