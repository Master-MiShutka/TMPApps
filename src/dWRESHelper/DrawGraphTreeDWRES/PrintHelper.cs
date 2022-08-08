using System.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TMP.DWRES.GUI
{
    internal static class PrintHelper
    {
        public static void ExportToImage(FrameworkElement surface, Uri path, ImageType itype, bool useZoomControlSurface = false)
        {
            BitmapSource bitmap;
            //Create a file stream for saving image
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                //Use png encoder for our data
                BitmapEncoder encoder;
                switch (itype)
                {
                    case ImageType.PNG: encoder = new PngBitmapEncoder();
                        bitmap = RenderTargetBitmap(surface, useZoomControlSurface);
                        break;
                    case ImageType.JPEG: encoder = new JpegBitmapEncoder() { QualityLevel = 100 };
                        bitmap = RenderTargetBitmap(surface, useZoomControlSurface);
                        break;
                    case ImageType.BMP: encoder = new BmpBitmapEncoder();
                        bitmap = RenderTargetBitmap(surface, useZoomControlSurface);
                        break;
                    case ImageType.GIF: encoder = new GifBitmapEncoder();
                        bitmap = RenderTargetBitmap(surface, useZoomControlSurface);
                        break;
                    case ImageType.TIFF: encoder = new TiffBitmapEncoder();
                        bitmap = RenderTargetBitmap(surface, useZoomControlSurface);
                        break;
                    default: throw new InvalidDataException("ExportToImage() -> Unknown output image format specified!");
                }

                //Push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                //Save the data to the stream
                encoder.Save(outStream);
            }
        }
        public static void ShowPrintPreview(FrameworkElement surface, string description = "")
        {
            try
            {
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(surface, description);
                }
            }
            catch
            {
                MessageBox.Show("Unexpected exception occured while trying to acces default printer. Please ensure that default printer is installed in your OS!");
            }
        }

        public static BitmapSource RenderTargetBitmap(FrameworkElement surface, bool useZoomControlSurface)
        {
            PresentationSource presentationSource = PresentationSource.FromVisual(surface);
            double dpiX = 96.0;
            double dpiY = 96.0;
            if (presentationSource != null)
            {
                dpiX *= presentationSource.CompositionTarget.TransformToDevice.M11;
                dpiY *= presentationSource.CompositionTarget.TransformToDevice.M22;
            }           

            Visual vis = surface;
            if (useZoomControlSurface)
            {
                if (surface.Parent != null && surface.Parent is WPFControls.Controls.IZoomControl)
                    vis = (surface.Parent as WPFControls.Controls.IZoomControl).PresenterVisual;
                else if (surface.Parent != null && surface.Parent is FrameworkElement &&
                         (surface.Parent as FrameworkElement).Parent is WPFControls.Controls.IZoomControl)
                    vis = ((surface.Parent as FrameworkElement).Parent as WPFControls.Controls.IZoomControl).PresenterVisual;
            }

            double width = surface.DesiredSize.Width;
            double height = surface.DesiredSize.Height;
            int w = (int)Math.Round(width);
            int h = (int)Math.Round(height);
            RenderTargetBitmap source = new RenderTargetBitmap(w, h, dpiX, dpiY, PixelFormats.Pbgra32);
            source.Render(vis);

            WriteableBitmap wbitmap = new WriteableBitmap(source);
            int wwidth = wbitmap.PixelWidth;
            int wheight = wbitmap.PixelHeight;
            int stride = wbitmap.BackBufferStride;
            byte[] imgdata = new byte[wwidth * wheight * 4];

            source.CopyPixels(imgdata, stride, 0);
            BitmapSource result = BitmapSource.Create(wwidth, wheight, dpiX, dpiY, PixelFormats.Bgra32, null, imgdata, stride);

            return result;
        }
    }

    public enum ImageType
    {
        PNG,
        JPEG,
        BMP,
        GIF,
        TIFF
    }
}
