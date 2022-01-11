using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TMP.Work.Emcos.Model
{
    public class HierarchicalEmcosPoint : EmcosPoint
    {
        public object Icon
        {
            get
            {
                // Fider 35 - 750
                if (TypeCode == "ELECTRICITY" && EcpName == "Линии" && ParentTypeCode == "VOLTAGE")
                    return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/fider35.png", UriKind.Absolute));
                // Transformer
                if (TypeCode == "ELECTRICITY" && EcpName == "POWERTRANSFORMER")
                    return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Transformer.png", UriKind.Absolute));
                // Auxiliary
                if (TypeCode == "ELECTRICITY" && EcpName == "Свои нужды")
                    return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/Transformer.png", UriKind.Absolute));
                // Fider 6-10
                if (TypeCode == "ELECTRICITY" && EcpName == "Линии")
                    return new BitmapImage(new Uri("pack://application:,,,/EmcosSiteWrapper;component/ImagesAndIcons/fider10.png", UriKind.Absolute));

                switch (TypeCode)
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
        public object Text
        {
            get { return Name; }
        }

        public object ToolTip
        {
            get
            {
                return string.Format("ИД точки в Emcos Corporate:{0},\nName:{1}, TypeCode:{2}", Id, Name, TypeCode);
            }
        }
        public override bool Equals(object obj)
        {
            EmcosPoint o = obj as EmcosPoint;
            if (o == null) return false;

            return this.Id == o.Id && this.Name == o.Name;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
