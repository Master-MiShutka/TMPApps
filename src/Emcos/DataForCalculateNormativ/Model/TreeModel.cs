using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using TMP.Work.Emcos.Model;

namespace TMP.Work.Emcos.DataForCalculateNormativ
{
    [Serializable]
    [DataContract]
    public class TreeModel : ICSharpCode.TreeView.SharpTreeNode
    {
        private ListPoint _point;

        public TreeModel()
        {
            Point = new ListPoint();
        }
        public TreeModel(ListPoint point) : this()
        {
            Init(point);
            if (point.Items != null)
                foreach (var item in point.Items)
                {
                    Children.Add(new TreeModel(item));
                }
        }
        public ListPoint Point
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
        public override bool IsCheckable
        {
            get
            {
                if (_point.TypeCode == "FES" | _point.TypeCode == "RES" | _point.TypeCode == "SUBSTATION" | _point.TypeCode == "VOLTAGE")
                    return true;
                return false;
            }
        }
        public override object Icon
        {
            get
            {
                // Fider 35 - 750
                if (Point.TypeCode == "ELECTRICITY" && Point.EсpName == "Линии" && Point.ParentTypeCode == "VOLTAGE")
                    return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/fider35.png", UriKind.Absolute));
                // Transformer
                if (Point.TypeCode == "ELECTRICITY" && Point.EсpName == "Трансформаторы")
                    return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Transformer.png", UriKind.Absolute));
                // Auxiliary
                if (Point.TypeCode == "ELECTRICITY" && Point.EсpName == "Свои нужды")
                    return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Transformer.png", UriKind.Absolute));
                // Fider 6-10
                if (Point.TypeCode == "ELECTRICITY" && Point.EсpName == "Линии")
                    return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/fider10.png", UriKind.Absolute));

                switch (Point.TypeCode)
                {
                    case "REGION": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/area.png", UriKind.Absolute));
                    case "FES": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Departament.png", UriKind.Absolute));
                    case "RES": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Departament.png", UriKind.Absolute));
                    case "SUBSTATION": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/substation.png", UriKind.Absolute));
                    case "VOLTAGE": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Voltage.png", UriKind.Absolute));
                    case "AUXILIARY": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Auxiliary.png", UriKind.Absolute));
                    case "SECTIONBUS": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/section.png", UriKind.Absolute));
                    case "TRANSFORMER": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Group.png", UriKind.Absolute));
                    case "ENTERPRISE": return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Prom.png", UriKind.Absolute));                    
                   default:
                        return new BitmapImage(new Uri("pack://application:,,,/DataForCalculateNormativ;component/Resources/Group.png", UriKind.Absolute));
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
            ListPoint o = obj as ListPoint;
            if (o == null) return false;

            return this.Point.Id == o.Id && this.Point.Name == o.Name;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private void Init(ListPoint point)
        {
            this.Point = point;
            IsChecked = point.IsChecked;
        }
    }
}