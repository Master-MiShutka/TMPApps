namespace WPFHexaEditor.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// ByteAction used for ByteModified class
    /// </summary>
    public enum ByteAction
    {
        Nothing,
        Added,
        Deleted,
        Modified,

        /// <summary>
        /// Used in ByteProvider for get list
        /// </summary>
        All,
    }

    /// <summary>
    /// Used for coloring mode of selection
    /// </summary>
    public enum FirstColor
    {
        HexByteData,
        StringByteData,
    }

    /// <summary>
    /// Mode of Copy/Paste
    /// </summary>
    public enum CopyPasteMode
    {
        Byte,
        HexaString,
        ASCIIString,
    }

    /// <summary>
    /// Used for check label are selected et next label to select...
    /// </summary>
    public enum KeyDownLabel
    {
        FirstChar,
        SecondChar,
        NextPosition,
    }

    public enum ByteToString
    {
        /// <summary>
        /// Build-in convertion mode. (recommended)
        /// </summary>
        ByteToCharProcess,

        /// <summary>
        /// System.Text.Encoding.ASCII string encoder
        /// </summary>
        ASCIIEncoding,
    }

    /// <summary>
    /// Scrollbar marker
    /// </summary>
    public enum ScrollMarker
    {
        Nothing,
        SearchHighLight,
        Bookmark,
        SelectionStart,
        ByteModified,
        ByteDeleted,
    }

    /// <summary>
    /// Type are opened in byteprovider
    /// </summary>
    public enum ByteProviderStreamType
    {
        File,
        MemoryStream,
        Nothing,
    }

    /// <summary>
    /// Type of character are used
    /// </summary>
    public enum CharacterTable
    {
        ASCII,
        TBLFile,
    }
}
