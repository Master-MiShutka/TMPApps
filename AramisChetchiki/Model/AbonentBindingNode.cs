using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TMP.WORK.AramisChetchiki.Model
{
    public class AbonentBindingNode : ICSharpCode.TreeView.SharpTreeNode
    {
        public string Header { get; set; }
        public NodeType Type { get; set; }
        public List<Meter> Meters { get; set; }
        public int MetersCount { get; set; }
        public int NotBindingMetersCount { get; set; }
        public bool HasEmptyValue => System.String.IsNullOrWhiteSpace(Header);
        public AbonentBindingNode()
        {
            ;
        }
        public AbonentBindingNode(string header, List<Meter> meters, NodeType type)
        {
            Header = header;
            Meters = meters;
            Type = type;
            NotBindingMetersCount = MetersCount = meters.Count;
        }

        public void AddNotBindings(List<Meter> meters)
        {
            if (Meters == null)
                throw new System.InvalidOperationException();
            Meters.AddRange(meters);
            NotBindingMetersCount = MetersCount = Meters.Count;
        }

        public override object Icon
        {
            get
            {
                switch (Type)
                {
                    case NodeType.Substation:
                        return Application.Current.TryFindResource("IconSubstation");
                    case NodeType.Fider10:
                        return Application.Current.TryFindResource("IconFider10");
                    case NodeType.TP:
                        return Application.Current.TryFindResource("IconTp");
                    case NodeType.Fider04:
                        return Application.Current.TryFindResource("IconFider04");
                    case NodeType.Group:
                        return Application.Current.TryFindResource("IconGroup");
                    default:
                        return Application.Current.TryFindResource("IconDepartament");
                }
            }
        }

        public override object Text => Header;

        public override int ChildrenCount => MetersCount;

        public enum NodeType
        {
            Substation,
            Fider10,
            TP,
            Fider04,
            Group,
            Departament
        }
    }
}
