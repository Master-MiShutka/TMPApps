namespace WPFHexaEditor.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using WPFHexaEditor.Core.Bytes;

    /// <summary>
    /// BookMark class
    /// </summary>
    public class BookMark
    {
        private ScrollMarker marker = ScrollMarker.Nothing;
        private long bytePositionInFile = 0;

        public ScrollMarker Marker
        {
            get => this.marker;

            set => this.marker = value;
        }

        public long BytePositionInFile
        {
            get => this.bytePositionInFile;

            set => this.bytePositionInFile = value;
        }
    }
}
