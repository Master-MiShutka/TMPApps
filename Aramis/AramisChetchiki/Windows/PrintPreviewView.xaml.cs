namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.IO;
    using System.IO.Packaging;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Xps.Packaging;
    using System.Windows.Xps.Serialization;

    /// <summary>
    /// Interaction logic for PrintPreviewWindow.xaml
    /// </summary>
    public partial class PrintPreviewWindow : Window
    {
        private string packagePath = string.Format("pack://{0}.xps", System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetTempFileName()));

        public PrintPreviewWindow(FlowDocument source)
        {
            this.InitializeComponent();

            source.ColumnWidth = double.PositiveInfinity;

            // Create a package for the XPS document
            MemoryStream packageStream = new ();
            Package package = Package.Open(packageStream, FileMode.Create, FileAccess.ReadWrite);

            // Create a XPS document with the path "pack://temp.xps"
            PackageStore.AddPackage(new Uri(this.packagePath), package);
            XpsDocument xpsDocument = new (package, CompressionOption.SuperFast, this.packagePath);

            // Serialize the XPS document
            XpsSerializationManager serializer = new (new XpsPackagingPolicy(xpsDocument), false);
            DocumentPaginator paginator = ((IDocumentPaginatorSource)source).DocumentPaginator;
            serializer.SaveAsXaml(paginator);

            // Get the fixed document sequence
            FixedDocumentSequence documentSequence = xpsDocument.GetFixedDocumentSequence();
            this.documentViewer.Document = documentSequence;
            this.documentViewer.FitToHeight();
        }
    }
}
