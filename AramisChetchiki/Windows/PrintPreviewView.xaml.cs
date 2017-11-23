using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

namespace TMP.WORK.AramisChetchiki
{
    /// <summary>
    /// Interaction logic for PrintPreviewWindow.xaml
    /// </summary>
    public partial class PrintPreviewWindow : Window
    {
        private string _packagePath = String.Format("pack://{0}.xps", System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()));

        public PrintPreviewWindow(FlowDocument source)
        {
            InitializeComponent();

            source.ColumnWidth = double.PositiveInfinity;

            // Create a package for the XPS document
            MemoryStream packageStream = new MemoryStream();
            Package package = Package.Open(packageStream, FileMode.Create, FileAccess.ReadWrite);

            // Create a XPS document with the path "pack://temp.xps"
            PackageStore.AddPackage(new Uri(_packagePath), package);
            XpsDocument xpsDocument = new XpsDocument(package, CompressionOption.SuperFast, _packagePath);
            
            // Serialize the XPS document
            XpsSerializationManager serializer = new XpsSerializationManager(new XpsPackagingPolicy(xpsDocument), false);
            DocumentPaginator paginator = ((IDocumentPaginatorSource)source).DocumentPaginator;
            serializer.SaveAsXaml(paginator);

            // Get the fixed document sequence
            FixedDocumentSequence documentSequence = xpsDocument.GetFixedDocumentSequence();
            documentViewer.Document = documentSequence;
            documentViewer.FitToHeight();
        }
    }
}
