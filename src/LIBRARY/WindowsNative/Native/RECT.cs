﻿namespace WindowsNative
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public static readonly RECT Empty = new RECT();

        public int Width => Math.Abs(this.right - this.left);

        public int Height => this.bottom - this.top;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public RECT(RECT rcSrc)
        {
            this.left = rcSrc.left;
            this.top = rcSrc.top;
            this.right = rcSrc.right;
            this.bottom = rcSrc.bottom;
        }

        public bool IsEmpty =>
                // BUGBUG : On Bidi OS (hebrew arabic) left > right
                this.left >= this.right || this.top >= this.bottom;

        public override string ToString()
        {
            if (this == Empty)
            {
                return "RECT {Empty}";
            }

            return "RECT { left : " + this.left + " / top : " + this.top + " / right : " + this.right + " / bottom : " + this.bottom + " }";
        }

        /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is Rect))
            {
                return false;
            }

            return this == (RECT)obj;
        }

        /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
        public override int GetHashCode()
        {
            return this.left.GetHashCode() + this.top.GetHashCode() + this.right.GetHashCode() + this.bottom.GetHashCode();
        }

        public static bool operator ==(RECT rect1, RECT rect2)
        {
            return rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom;
        }

        public static bool operator !=(RECT rect1, RECT rect2)
        {
            return !(rect1 == rect2);
        }
    }
}
