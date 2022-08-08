namespace TMP.PrintEngine.ViewModels
{
    using System;
    using System.Drawing.Printing;
    using System.Printing;
    using System.Windows.Documents;

    public interface IFlowDocumentPrintControlViewModel : IViewModel
    {
        PrintQueue CurrentPrinter { get; set; }

        string CurrentPrinterName { get; set; }

        void ReloadPreview(PageOrientation pageOrientation, PaperSize paperSize);

        void ReloadPreview();

        void InitializeProperties();

        int NumberOfPages { get; set; }

        void ShowPrintPreview(FlowDocument flowDocument);
    }
}