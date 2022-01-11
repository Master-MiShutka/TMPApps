namespace System.Windows.Media.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PortableFontDesc
    {
        public readonly string FontName;
        public readonly int EmSize;
        public readonly bool IsBold;
        public readonly bool IsItalic;
        public readonly bool IsClearType;

        public PortableFontDesc(string name = "Arial", int emsize = 12, bool isbold = false, bool isitalic = false, bool cleartype = false)
        {
            this.FontName = name;
            this.EmSize = emsize;
            this.IsBold = isbold;
            this.IsItalic = isitalic;
            this.IsClearType = cleartype;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return this.FontName.GetHashCode() ^ this.EmSize.GetHashCode() ^ this.IsBold.GetHashCode() ^ this.IsItalic.GetHashCode() ^ this.IsClearType.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as PortableFontDesc;
            if (other == null)
            {
                return false;
            }

            return this.FontName == other.FontName && this.EmSize == other.EmSize && this.IsBold == other.IsBold && this.IsItalic == other.IsItalic && this.IsClearType == other.IsClearType;
        }
    }
}
