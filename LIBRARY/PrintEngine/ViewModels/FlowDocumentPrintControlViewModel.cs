namespace TMP.PrintEngine.ViewModels
{
    using System;
    using System.Drawing.Printing;
    using System.IO;
    using System.IO.Packaging;
    using System.Printing;
    using System.Windows.Documents;
    using System.Windows.Xps.Packaging;
    using TMP.PrintEngine.Extensions;
    using TMP.PrintEngine.Views;

    public sealed class FlowDocumentPrintControlViewModel : APrintControlViewModel, IFlowDocumentPrintControlViewModel
    {
        public FlowDocument FlowDocument { get; set; }

        public FlowDocumentPrintControlViewModel(PrintControlView view)
            : base(view)
        {
        }

        public override void ReloadPreview()
        {
            if (this.CurrentPaper != null)
            {
                this.ReloadPreview(this.PageOrientation, this.CurrentPaper);
            }
        }

        private MemoryStream ms;
        private Package pkg;
        private XpsDocument xpsDocument;

        public void ReloadPreview(PageOrientation pageOrientation, PaperSize currentPaper)
        {
            this.ReloadingPreview = true;
            if (this.FullScreenPrintWindow != null)
            {
                this.WaitScreen.Show(TMP.PrintEngine.Resources.Strings.WaitMessage);
            }

            if (this.PageOrientation == PageOrientation.Portrait)
            {
                this.FlowDocument.PageHeight = currentPaper.Height;
                this.FlowDocument.PageWidth = currentPaper.Width;
            }
            else
            {
                this.FlowDocument.PageHeight = currentPaper.Width;
                this.FlowDocument.PageWidth = currentPaper.Height;
            }

            this.ms = new MemoryStream();
            this.pkg = Package.Open(this.ms, FileMode.Create, FileAccess.ReadWrite);
            const string pack = "pack://temp.xps";
            var oldPackage = PackageStore.GetPackage(new Uri(pack));
            if (oldPackage == null)
            {
                PackageStore.AddPackage(new Uri(pack), this.pkg);
            }
            else
            {
                PackageStore.RemovePackage(new Uri(pack));
                PackageStore.AddPackage(new Uri(pack), this.pkg);
            }

            this.xpsDocument = new XpsDocument(this.pkg, CompressionOption.SuperFast, pack);
            var xpsWriter = XpsDocument.CreateXpsDocumentWriter(this.xpsDocument);

            var documentPaginator = ((IDocumentPaginatorSource)this.FlowDocument).DocumentPaginator;
            xpsWriter.Write(documentPaginator);
            this.Paginator = documentPaginator;
            this.MaxCopies = this.NumberOfPages = this.ApproaxNumberOfPages = this.Paginator.PageCount;
            this.PagesAcross = 2;
            this.DisplayPagePreviewsAll(documentPaginator);
            this.WaitScreen.Hide();
            this.ReloadingPreview = false;
        }

        public void ShowPrintPreview(FlowDocument flowDocument)
        {
            this.FlowDocument = flowDocument;
            if (this.FullScreenPrintWindow == null)
            {
                this.CreatePrintPreviewWindow();
            }

            this.Loading = true;
            if (this.FullScreenPrintWindow != null)
            {
                this.FullScreenPrintWindow.ShowDialog();
            }

            ApplicationExtention.MainWindow = null;
        }

        public override void FullScreenPrintWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.xpsDocument != null)
            {
                this.xpsDocument.Close();
            }

            if (this.pkg != null)
            {
                this.pkg.Close();
            }

            if (this.ms != null)
            {
                this.ms.Close();
            }

            base.FullScreenPrintWindowClosing(sender, e);
        }
    }
}