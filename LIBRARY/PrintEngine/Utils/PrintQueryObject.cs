using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using TMP.PrintEngine.ViewModels;

namespace TMP.PrintEngine.Utils
{
    public class PrintQueryObject : INotifyPropertyChanged
    {
        private System.Drawing.Printing.PrinterSettings _printerSettings;
        private PaperSize _defaultPaperSize;
        public APrintControlViewModel ViewModel { set; get; }
        public string CurrentPrinterName { get; set; }
        private System.Drawing.Printing.PrinterSettings.PaperSizeCollection _paperSizes;
        public IList<PaperSize> PaperSizes { get; set; }
        public PaperSize DefaultPaperSize { get; set; }

        public PrintQueryObject(string currentPrinterFullName, APrintControlViewModel viewModel)
        {
            CurrentPrinterName = currentPrinterFullName;
            ViewModel = viewModel;
        }

        public void FetchSetting()
        {
            SetPrinterSettings();
            PaperSizes = GetPaperSizes(); 
            DefaultPaperSize = GetDefaultPaperSize();
        }

        private IList<PaperSize> GetPaperSizes()
        {
            var Pss = new List<PaperSize>();
            var pss = _printerSettings.PaperSizes;
            foreach (var ps in pss)
            {
                if (((PaperSize)ps).PaperName != "Custom")
                {
                    Pss.Add((PaperSize)ps);
                }

            }
            return Pss;
        }

        protected void SetPrinterSettings()
        {
            _printerSettings = new System.Drawing.Printing.PrinterSettings { PrinterName = CurrentPrinterName };
        }

        private PaperSize GetDefaultPaperSize()
        {
            var pageSettings = _printerSettings.DefaultPageSettings;
            return (PaperSize)pageSettings.PaperSize;
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}