namespace System.Windows.Media.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public struct IntRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width
        {
            get => this.Right - this.Left + 1;
            set => this.Right = this.Left + value - 1;
        }

        public int Height
        {
            get => this.Bottom - this.Top + 1;
            set => this.Bottom = this.Top + value - 1;
        }

        public IntPoint TopLeft
        {
            get => new IntPoint(this.Left, this.Top);

            set
            {
                this.Left = value.X;
                this.Top = value.Y;
            }
        }

        public IntPoint BottomRight
        {
            get => new IntPoint(this.Right, this.Bottom);

            set
            {
                this.Right = value.X;
                this.Bottom = value.Y;
            }
        }

        public IntSize Size
        {
            get => new IntSize(this.Width, this.Height);

            set
            {
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }

        public IntRect(IntPoint topLeft, IntSize size)
        {
            this.Left = topLeft.X;
            this.Top = topLeft.Y;
            this.Right = topLeft.X + size.Width - 1;
            this.Bottom = topLeft.Y + size.Height - 1;
        }

        public IntRect(IntPoint topLeft, IntPoint bottomRight)
        {
            this.Left = topLeft.X;
            this.Top = topLeft.Y;
            this.Right = bottomRight.X;
            this.Bottom = bottomRight.Y;
        }

        public IntRect GrowSymmetrical(int dWidth2, int dHeight2)
        {
            return new IntRect(
                new IntPoint(this.Left - dWidth2, this.Top - dHeight2),
                new IntSize(this.Width + (2 * dWidth2), this.Height + (2 * dHeight2)));
        }

        public Rect ToRect()
        {
            return new Rect(this.Left, this.Top, this.Width, this.Height);
        }

        public IntRect Translate(IntPoint pt)
        {
            return new IntRect(this.TopLeft.Translate(pt), this.Size);
        }

        public bool Contains(Point pt)
        {
            return pt.X >= this.Left && pt.X <= this.Right && pt.Y >= this.Top && pt.Y <= this.Bottom;
        }
    }

    public struct IntPoint
    {
        public int X;
        public int Y;

        public IntPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public IntPoint Translate(IntPoint pt)
        {
            return new IntPoint(this.X + pt.X, this.Y + pt.Y);
        }
    }

    public struct IntSize
    {
        public int Width;
        public int Height;

        public IntSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }

    static partial class WriteableBitmapExtensions
    {
        public static void FillRectangle(this WriteableBitmap bmp, IntRect rect, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillRectangle(rect.Left, rect.Top, rect.Right + 1, rect.Bottom + 1, col);
        }

        public static void DrawRectangle(this WriteableBitmap bmp, IntRect rect, Color color)
        {
            var col = ConvertColor(color);
            bmp.DrawRectangle(rect.Left, rect.Top, rect.Right, rect.Bottom, col);
        }
    }
}
