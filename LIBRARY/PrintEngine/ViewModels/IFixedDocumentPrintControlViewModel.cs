namespace TMP.PrintEngine.ViewModels
{
    using System;
    using System.Drawing.Printing;
    using System.Printing;
    using System.Windows.Documents;
    using System.Windows.Forms;

    public interface IFixedDocumentPrintControlViewModel : IViewModel
    {
        PrintQueue CurrentPrinter { get; set; }

        string CurrentPrinterName { get; set; }

        void ReloadPreview(PageOrientation pageOrientation, PaperSize currentPaper);

        void ReloadPreview();

        void OnDialogResultEvent(DialogResult result);

        void InitializeProperties();

        int NumberOfPages { get; set; }

        void ShowPrintPreview(IDocumentPaginatorSource source);

        PaperSize CurrentPaper { get; set; }

        void ShowPrintPreview(IDocumentPaginatorSource paginator, double pageaccrosswith);
    }
}