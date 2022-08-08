namespace TMP.PrintEngine.Utils
{
    using System;
    using System.IO;
    using System.Windows;

    public class Constants
    {
        public class CsBook
        {
            public const int ColumnHeaderMargin = 10;
            public const int MaxColumnWidth = 400;
            public const int SSTGridFontSize = 11;
            public const int EntityChartPageHeaderSize = 50;
            public const int PageNumberTextLength = 75;
        }

        public class Print
        {
            public static readonly Size PaperSizeA4Portrait = new Size(827 * 96 / 100, 1169 * 96 / 100);
            public static readonly Size PaperSizeA4Landscape = new Size(1169 * 96 / 100, 827 * 96 / 100);

            public static readonly Size Letter = new Size(850, 1100);
            public static readonly Size Legal = new Size(850, 1400);
            public static readonly Size A3 = new Size(1169, 1654);
            public static readonly Size A4 = new Size(827, 1169);
            public static readonly Size A5 = new Size(583, 827);

            public static string SETTINGS_FOLDER
            {
                get
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"TMP\PRINTENGINE\SETTINGS");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    return path;
                }
            }
        }
    }
}