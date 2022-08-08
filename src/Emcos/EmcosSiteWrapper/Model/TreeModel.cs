using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;

namespace TMP.Work.Emcos.Model
{
    /// <summary>
    /// Обертка над <see cref="IHierarchicalEmcosPoint"/> для описания иерархической структуры
    /// </summary>
    [Serializable]
    [DataContract]
    public class TreeModel : ICSharpCode.TreeView.SharpTreeNode
    {
        private IHierarchicalEmcosPoint _point;

        public TreeModel()
        {
            Point = new EmcosPoint();
        }
        public TreeModel(IHierarchicalEmcosPoint point) : this()
        {
            Init(point);
            if (point.Children != null)
                foreach (var item in point.Children)
                {
                    Children.Add(new TreeModel(item));
                }
        }
        public IHierarchicalEmcosPoint Point
        {
            get { return _point; }
            set
            {
                _point = value;
                RaisePropertyChanged("Point");
                RaisePropertyChanged("IsCheckable");
                RaisePropertyChanged("Icon");
            }
        }
        /*public override bool IsCheckable
        {
            get
            {
                if (_point.TypeCode == "FES" | _point.TypeCode == "RES" | _point.TypeCode == "SUBSTATION" | _point.TypeCode == "VOLTAGE")
                    return true;
                return false;
            }
        }*/

        public override bool ShowIcon => true;

        public override object Icon
        {
            get
            {
                // Fider 35 - 750
                if (Point.TypeCode == "ELECTRICITY" && Point.EcpName == "Линии" && Point.ParentTypeCode == "VOLTAGE")
                    return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/fider35.png", UriKind.Absolute));
                // Transformer
                if (Point.TypeCode == "ELECTRICITY" && Point.EcpName == "POWERTRANSFORMER")
                    return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Transformer.png", UriKind.Absolute));
                // Auxiliary
                if (Point.TypeCode == "ELECTRICITY" && Point.EcpName == "Свои нужды")
                    return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Transformer.png", UriKind.Absolute));
                // Fider 6-10
                if (Point.TypeCode == "ELECTRICITY" && Point.EcpName == "Линии")
                    return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/fider10.png", UriKind.Absolute));

                switch (Point.TypeCode)
                {
                    case "REGION": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/area.png", UriKind.Absolute));
                    case "FES": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Departament.png", UriKind.Absolute));
                    case "RES": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Departament.png", UriKind.Absolute));
                    case "SUBSTATION": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/substation.png", UriKind.Absolute));
                    case "VOLTAGE": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Voltage.png", UriKind.Absolute));
                    case "AUXILIARY": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Auxiliary.png", UriKind.Absolute));
                    case "SECTIONS":
                    case "SECTIONBUS": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/section.png", UriKind.Absolute));
                    case "TRANSFORMER": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Group.png", UriKind.Absolute));
                    case "ENTERPRISE": return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Prom.png", UriKind.Absolute));
                    default:
                        return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Group.png", UriKind.Absolute));
                };
            }
        }
        public override object Text
        {
            get { return Point.Name; }
        }

        public override object ToolTip
        {
            get
            {
                return string.Format("ИД точки в Emcos Corporate:{0},\nName:{1}, TypeCode:{2}", Point.Id, Point.Name, Point.TypeCode);
            }
        }
        public override bool Equals(object obj)
        {
            EmcosPoint o = obj as EmcosPoint;
            if (o == null) return false;

            return this.Point.Id == o.Id && this.Point.Name == o.Name;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private void Init(IHierarchicalEmcosPoint point)
        {
            this.Point = point;
        }
    }
}
