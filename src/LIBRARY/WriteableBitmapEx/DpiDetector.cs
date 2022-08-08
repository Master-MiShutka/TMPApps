namespace System.Windows.Media.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    public static class DpiDetector
    {
        private static double? dpiXKoef;

        public static double DpiXKoef
        {
            get
            {
                if (dpiXKoef == null)
                {
                    using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        dpiXKoef = graphics.DpiX / 96.0;
                    }
                }

                return dpiXKoef ?? 1;
            }
        }

        private static double? dpiYKoef;

        public static double DpiYKoef
        {
            get
            {
                if (dpiYKoef == null)
                {
                    using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        dpiYKoef = graphics.DpiY / 96.0;
                    }
                }

                return dpiYKoef ?? 1;

                // return Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.WorkArea.Width;
                // WantGlobalTransformMatrix();
                // if (_globalTransformPatrix.HasValue) return _globalTransformPatrix.Value.M22;
                // return 1;
            }
        }

        // private static void WantGlobalTransformMatrix()
        // {
        //    if (_globalTransformPatrix != null) return;
        //    try
        //    {
        //        _globalTransformPatrix =
        //            PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice;
        //    }
        //    catch
        //    {
        //        _globalTransformPatrix = null;
        //    }
        // }
    }
}
