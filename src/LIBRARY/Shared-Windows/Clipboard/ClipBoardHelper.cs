namespace TMP.Shared.Windows
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using TMP.Shared.Common;

    /// <summary>
    // Format Identifier           Data Type           Description
    // CF_TEXT(1)                  ANSI text           Text.
    // CF_BITMAP (2)	              HBITMAP             Handle to a bitmap(GDI object).
    // CF_METAFILEPICT(3)          METAFILEPICT        Metafile picture.
    // CF_TIFF (6)	              Binary(TIFF)        TIFF image.
    // CF_ENHMETAFILE(14)          ENHMETAHEADER       Enhanced meta file.
    // CF_DSPTEXT(0x0081)          ANSI text           Text.
    // CF_DSPBITMAP (0x0082)	      HBITMAP             Handle to a bitmap(GDI object)
    // CF_DSPMETAFILEPICT(0x0083)  METAFILEPICT        Metafile picture.
    // CF_DSPENHMETAFILE (0x008E)  ENHMETAHEADER       Enhanced meta file.
    // text/html                                       HTML [W3C] file content. The encoding is defined by BOM and HTML headers.
    // text/csv                                        Comma-Separated Values (CSV) [IETF] file content.
    // CSV                                             See text/csv.
    // image/jpeg                                      JPEG [W3C] file content.
    // JPEG                                            See image/jpeg.
    // JPG                                             See image/jpeg.
    // image/png                                       PNG [W3C] file content.
    // PNG                                             See image/png.
    /// </summary>
    public static class ClipBoardHelper
    {
        public static BitmapSource RenderFrameworkElementToBitmapSource(FrameworkElement visual)
        {
            double width = visual.ActualWidth;
            double height = visual.ActualHeight;
            int w = (int)Math.Round(width);
            int h = (int)Math.Round(height);

            PresentationSource presentationSource = PresentationSource.FromVisual(visual);
            double dpiX = 96.0;
            double dpiY = 96.0;
            if (presentationSource != null)
            {
                dpiX *= presentationSource.CompositionTarget.TransformToDevice.M11;
                dpiY *= presentationSource.CompositionTarget.TransformToDevice.M22;
            }

            RenderTargetBitmap source = new RenderTargetBitmap(w, h, dpiX, dpiY, PixelFormats.Pbgra32);
            source.Render(visual);

            WriteableBitmap wbitmap = new WriteableBitmap(source);
            int wwidth = wbitmap.PixelWidth;
            int wheight = wbitmap.PixelHeight;
            int stride = wbitmap.BackBufferStride;
            byte[] imgdata = new byte[wwidth * wheight * 4];

            source.CopyPixels(imgdata, stride, 0);
            BitmapSource result = BitmapSource.Create(wwidth, wheight, dpiX, dpiY, PixelFormats.Bgra32, null, imgdata, stride);

            return result;
        }

        public static void PasteBitmapSourceToClipboardAsBitmapAndPng(BitmapSource bitmapSource)
        {
            // Create a DataObject to hold data
            // in different formats.
            IDataObject data_object = new DataObject();

            System.IO.MemoryStream ms;

            ms = new System.IO.MemoryStream();
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            pngEncoder.Save(ms);
            data_object.SetData("PNG", ms, false);

            ms = new System.IO.MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(ms);

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);
            data_object.SetData(DataFormats.Bitmap, bmp);

            // Place the data on the clipboard.
            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetDataObject(data_object, true);
        }

        /// <summary>
        /// Gets the clipboard data as a table.
        /// </summary>
        /// <returns>The parsed clipboard data as a table, or <c>null</c> if the clipboard is empty or does not contain normalized table data.</returns>
        /// <remarks>If no TEXT is present in the clipboard, CSV data is used.</remarks>
        public static IList<IList<string>>? GetClipboardDataAsTable()
        {
            var text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text))
            {
                return text.ParseTable(TableHelper.TextColumnSeparator);
            }

            var csv = Clipboard.GetData(DataFormats.CommaSeparatedValue) as string;
            if (!string.IsNullOrEmpty(csv))
            {
                return csv.ParseTable(TableHelper.CsvColumnSeparator);
            }

            return null;
        }

        /// <summary>
        /// Sets the clipboard data for the specified table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <remarks>
        /// This method sets the TEXT (tab delimited) and CSV data. Like in Excel the CSV delimiter is either comma or semicolon, depending on the current culture.
        /// </remarks>
        public static void SetClipboardData(this IList<IList<string>>? table)
        {
            if (table == null)
            {
                Clipboard.Clear();
                return;
            }

            var textString = table.ToTextString();
            var csvString = table.ToCsvString();

            var dataObject = new DataObject();

            dataObject.SetText(textString);
            dataObject.SetText(csvString, TextDataFormat.CommaSeparatedValue);

            Clipboard.SetDataObject(dataObject);
        }
    }
}
