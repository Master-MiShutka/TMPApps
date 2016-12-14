using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Windows;

namespace TMP.PrintEngine.Utils
{
    public class PrintUtility
    {
        public PrintUtility()
        {
            ;
        }

        public string GetSaveLocation(string printerFullName)
        {
            return String.Format("{0}//{1}_printTicket.xml", Constants.Print.SETTINGS_FOLDER, printerFullName.Replace("\\", "_"));
        }

        private PrinterSettings GetPrinterSettings(string currentPrinterName)
        {
            return new PrinterSettings { PrinterName = currentPrinterName };
        }

        public Thickness GetPageMargin(string currentPrinterName)
        {
            float hardMarginX;
            float hardMarginY;
            var printerSettings = GetPrinterSettings(currentPrinterName);
            try
            {
                hardMarginX = printerSettings.DefaultPageSettings.HardMarginX;
                hardMarginY = printerSettings.DefaultPageSettings.HardMarginY;
            }
            catch (Exception)
            {
                hardMarginX = 10;
                hardMarginY = 10;
            }
            var margin = new Thickness(hardMarginX + 5, hardMarginY + 5, printerSettings.DefaultPageSettings.Margins.Right, printerSettings.DefaultPageSettings.Margins.Bottom + 50);
            ////TempFileLogger.Log(String.Format("Paper margin = ({0}, {1}, {2}, {3})", margin.Left, margin.Top, margin.Right, margin.Bottom));
            return margin;
        }

        public IList<PaperSize> GetPaperSizes(string currentPrinterName)
        {
            var paperSizes = new List<PaperSize>();
            var sizes = GetPrinterSettings(currentPrinterName).PaperSizes;
            foreach (var ps in sizes)
            {
                if (((PaperSize)ps).PaperName != "Custom Size")
                {
                    paperSizes.Add((PaperSize)ps);
                }
            }
            ////TempFileLogger.Log("Paper sizes retrieved successfully.");
            return paperSizes;
        }

        public PrintQueueCollection GetPrinters()
        {
            try
            {
                var printServer = new PrintServer();                
                var printers = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Connections, EnumeratedPrintQueueTypes.Local });
                return printers;
            }
            catch
            {
                ////TempFileLogger.LogException(ex);
                throw;
            }
        }

        public PrintQueue GetDefaultPrintQueue(string printerName)
        {
            return LocalPrintServer.GetDefaultPrintQueue();
            ////var printQueue = new PrintServer().GetPrintQueues().Where(pq => pq.FullName == printerName).SingleOrDefault();
            ////return printQueue ?? LocalPrintServer.GetDefaultPrintQueue();
        }

        public PrintTicket GetUserPrintTicket(string printerFullName)
        {
            if (File.Exists(GetSaveLocation(printerFullName)))
            {
                var fileStream = new FileStream(GetSaveLocation(printerFullName), FileMode.Open);
                var userPrintTicket = new PrintTicket(fileStream);
                fileStream.Close();

                return userPrintTicket;
            }

            return null;
        }

        public void SaveUserPrintTicket(PrintQueue currentPrinter)
        {
            Stream outStream = new FileStream(GetSaveLocation(currentPrinter.FullName), FileMode.Create);
            currentPrinter.UserPrintTicket.SaveTo(outStream);
            outStream.Close();
        }
    }
}