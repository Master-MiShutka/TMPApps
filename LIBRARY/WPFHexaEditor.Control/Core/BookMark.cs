using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WPFHexaEditor.Core.Bytes;

namespace WPFHexaEditor.Core
{
    /// <summary>
    /// BookMark class
    /// </summary>
    public class BookMark
    {
        ScrollMarker _marker = ScrollMarker.Nothing;
        long _bytePositionInFile = 0;


        public ScrollMarker Marker
        {
            get
            {
                return _marker;
            }

            set
            {
                _marker = value;
            }
        }

        public long BytePositionInFile
        {
            get
            {
                return _bytePositionInFile;
            }

            set
            {
                _bytePositionInFile = value;
            }
        }
    }
}
