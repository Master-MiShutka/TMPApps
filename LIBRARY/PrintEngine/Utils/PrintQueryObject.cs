namespace TMP.PrintEngine.Utils
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Printing;
    using TMP.PrintEngine.ViewModels;

    public class PrintQueryObject : INotifyPropertyChanged
    {
        private System.Drawing.Printing.PrinterSettings printerSettings;
        private PaperSize defaultPaperSize;

        public APrintControlViewModel ViewModel { set; get; }

        public string CurrentPrinterName { get; set; }

        private System.Drawing.Printing.PrinterSettings.PaperSizeCollection paperSizes;

        public IList<PaperSize> PaperSizes { get; set; }

        public PaperSize DefaultPaperSize { get; set; }

        public PrintQueryObject(string currentPrinterFullName, APrintControlViewModel viewModel)
        {
            this.CurrentPrinterName = currentPrinterFullName;
            this.ViewModel = viewModel;
        }

        public void FetchSetting()
        {
            this.SetPrinterSettings();
            this.PaperSizes = this.GetPaperSizes();
            this.DefaultPaperSize = this.GetDefaultPaperSize();
        }

        private IList<PaperSize> GetPaperSizes()
        {
            var Pss = new List<PaperSize>();
            var pss = this.printerSettings.PaperSizes;
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
            this.printerSettings = new System.Drawing.Printing.PrinterSettings { PrinterName = this.CurrentPrinterName };
        }

        private PaperSize GetDefaultPaperSize()
        {
            var pageSettings = this.printerSettings.DefaultPageSettings;
            return (PaperSize)pageSettings.PaperSize;
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}