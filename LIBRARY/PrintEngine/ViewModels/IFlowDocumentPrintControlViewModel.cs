using System;
using System.Drawing.Printing;
using System.Printing;
using System.Windows.Documents;

namespace TMP.PrintEngine.ViewModels
{
    public interface IFlowDocumentPrintControlViewModel : IViewModel
    {
        PrintQueue CurrentPrinter { get; set; }
        string CurrentPrinterName { get; set; }
        void ReloadPreview(PageOrientation pageOrientation, PaperSize paperSize);
        void ReloadPreview();
        void InitializeProperties();
        Int32 NumberOfPages { get; set; }
        void ShowPrintPreview(FlowDocument flowDocument);
    }
}