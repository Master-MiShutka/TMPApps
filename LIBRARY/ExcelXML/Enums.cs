namespace TMP.ExcelXml
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    #region ParseArgumentType
    internal enum ParseArgumentType
    {
        None,
        Function,
        Range,
        AbsoluteRange,
    }
    #endregion

    #region ParameterType

    /// <summary>
    /// Formula parameter types
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// A unknown string parameter
        /// </summary>
        String,

        /// <summary>
        /// A Range
        /// </summary>
        Range,

        /// <summary>
        /// A Formula
        /// </summary>
        Formula,
    }
    #endregion

    #region Cell Content & Display Enums

    /// <summary>
    /// The default display format of the cell in excel
    /// </summary>
    public enum DisplayFormatType
    {
        /// <summary>
        /// General format
        /// </summary>
        None,

        /// <summary>
        /// Displays anything as text (i.e. Left aligned without formatting)
        /// </summary>
        Text,

        /// <summary>
        /// Displays numeric values with two fixed decimals
        /// </summary>
        Fixed,

        /// <summary>
        /// Displays numeric values with two fixed decimals and digit grouping
        /// </summary>
        Standard,

        /// <summary>
        /// Displays numeric values as percentage values
        /// </summary>
        Percent,

        /// <summary>
        /// Displays numeric values in scientific notation
        /// </summary>
        Scientific,

        /// <summary>
        /// Displays numeric or date values as formatted date values
        /// </summary>
        GeneralDate,

        /// <summary>
        /// Displays numeric or date values as short date format
        /// </summary>
        ShortDate,

        /// <summary>
        /// Displays numeric or date values as long date format
        /// </summary>
        LongDate,

        /// <summary>
        /// Displays numeric or date values in time format
        /// </summary>
        Time,

        /// <summary>
        /// Custom defined format
        /// </summary>
        Custom,
    }

    /// <summary>
    /// The cell content type
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// Cell does not contain anything
        /// </summary>
        None,

        /// <summary>
        /// Cell contains a string
        /// </summary>
        String,

        /// <summary>
        /// Cell contains a number
        /// </summary>
        Number,

        /// <summary>
        /// Cell contains a DateTime value
        /// </summary>
        DateTime,

        /// <summary>
        /// Cell contains a bool value
        /// </summary>
        Boolean,

        /// <summary>
        /// Cell contains a formula
        /// </summary>
        Formula,

        /// <summary>
        /// Cell contains a formula which cannot be resolved
        /// </summary>
        UnresolvedValue,
    }
    #endregion

    #region Cell Style Enums

    /// <summary>
    /// Cell's vertical alignment values
    /// </summary>
    public enum VerticalAlignment
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Top aligned
        /// </summary>
        Top,

        /// <summary>
        /// Centered
        /// </summary>
        Center,

        /// <summary>
        /// Bottom aligned
        /// </summary>
        Bottom,

        /// <summary>
        /// Justified
        /// </summary>
        Justify,

        /// <summary>
        /// Distributed
        /// </summary>
        Distributed,
    }

    /// <summary>
    /// Cell's horizontal alignment values
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Left aligned
        /// </summary>
        Left,

        /// <summary>
        /// Centered
        /// </summary>
        Center,

        /// <summary>
        /// Right aligned
        /// </summary>
        Right,

        /// <summary>
        /// Streched to fill
        /// </summary>
        Fill,

        /// <summary>
        /// Justified
        /// </summary>
        Justify,

        /// <summary>
        /// Distributed
        /// </summary>
        Distributed,
    }

    /// <summary>
    /// Different style of border lines
    /// </summary>
    public enum Borderline
    {
        /// <summary>
        /// Continuous line
        /// </summary>
        Continuous,

        /// <summary>
        /// Dash line
        /// </summary>
        Dash,

        /// <summary>
        /// DashDot line
        /// </summary>
        DashDot,

        /// <summary>
        /// DashDotDot line
        /// </summary>
        DashDotDot,

        /// <summary>
        /// Double line
        /// </summary>
        Double,

        /// <summary>
        /// Dot line
        /// </summary>
        Dot,

        /// <summary>
        /// SlantDashDot line
        /// </summary>
        SlantDashDot,
    }

    /// <summary>
    /// Different type of border sides.
    /// </summary>
    /// <remarks>Multiple values can be combined by an or (i.e. "|") operation.</remarks>
    [Flags]
    public enum BorderSides
    {
        /// <summary>
        /// No border
        /// </summary>
        None = 0,

        /// <summary>
        /// Cell has a top border
        /// </summary>
        Top = 1,

        /// <summary>
        /// Cell has a left border
        /// </summary>
        Left = 2,

        /// <summary>
        /// Cell has a botom border
        /// </summary>
        Bottom = 4,

        /// <summary>
        /// Cell has a right border
        /// </summary>
        Right = 8,

        /// <summary>
        /// Cell has full border on all sides
        /// </summary>
        All = 15,
    }

    #region Pattern

    /// <summary>
    /// Different types of cell background patterns
    /// </summary>
    public enum Pattern
    {
        /// <summary>
        /// Solid
        /// </summary>
        Solid,

        /// <summary>
        /// Gray25
        /// </summary>
        Gray25,

        /// <summary>
        /// Gray50
        /// </summary>
        Gray50,

        /// <summary>
        /// Gray75
        /// </summary>
        Gray75,

        /// <summary>
        /// Gray125
        /// </summary>
        Gray125,

        /// <summary>
        /// Gray0625
        /// </summary>
        Gray0625,

        /// <summary>
        /// HorzStripe
        /// </summary>
        HorzStripe,

        /// <summary>
        /// VertStripe
        /// </summary>
        VertStripe,

        /// <summary>
        /// ReverseDiagStripe
        /// </summary>
        ReverseDiagStripe,

        /// <summary>
        /// DiagStripe
        /// </summary>
        DiagStripe,

        /// <summary>
        /// DiagCross
        /// </summary>
        DiagCross,

        /// <summary>
        /// ThickDiagCross
        /// </summary>
        ThickDiagCross,

        /// <summary>
        /// ThinHorzStripe
        /// </summary>
        ThinHorzStripe,

        /// <summary>
        /// ThinVertStripe
        /// </summary>
        ThinVertStripe,

        /// <summary>
        /// ThinReverseDiagStripe
        /// </summary>
        ThinReverseDiagStripe,

        /// <summary>
        /// ThinDiagStripe
        /// </summary>
        ThinDiagStripe,

        /// <summary>
        /// ThinHorzCross
        /// </summary>
        ThinHorzCross,

        /// <summary>
        /// ThinDiagCross
        /// </summary>
        ThinDiagCross,
    }
    #endregion
    #endregion

    #region Page Properties

    /// <summary>
    /// Page layout
    /// </summary>
    public enum PageLayout
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Centers the page horizontally
        /// </summary>
        CenterHorizontal,

        /// <summary>
        /// Centers the page vertically
        /// </summary>
        CenterVertical,

        /// <summary>
        /// Centers the page vertically and horizontally
        /// </summary>
        CenterVerticalAndHorizontal,
    }

    /// <summary>
    /// Orientation mode
    /// </summary>
    public enum PageOrientation
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Landscape orientation
        /// </summary>
        Landscape,

        /// <summary>
        /// Portrait orientation
        /// </summary>
        Portrait,
    }

    /// <summary>
    /// Papersize
    /// </summary>
    public enum PaperSize
    {
        /// <summary>
        /// Letter paper (8.5 in. by 11 in.)
        /// </summary>
        Letter = 1,

        /// <summary>
        /// Letter small paper (8.5 in. by 11 in.)
        /// </summary>
        LetterSmall = 2,

        /// <summary>
        /// // Tabloid paper (11 in. by 17 in.)
        /// </summary>
        Tabloid = 3,

        /// <summary>
        /// Ledger paper (17 in. by 11 in.)
        /// </summary>
        Ledger = 4,

        /// <summary>
        /// Legal paper (8.5 in. by 14 in.)
        /// </summary>
        Legal = 5,

        /// <summary>
        /// Statement paper (5.5 in. by 8.5 in.)
        /// </summary>
        Statement = 6,

        /// <summary>
        /// Executive paper (7.25 in. by 10.5 in.)
        /// </summary>
        Executive = 7,

        /// <summary>
        /// A3 paper (297 mm by 420 mm)
        /// </summary>
        A3 = 8,

        /// <summary>
        /// A4 paper (210 mm by 297 mm)
        /// </summary>
        A4 = 9,

        /// <summary>
        /// A4 small paper (210 mm by 297 mm)
        /// </summary>
        A4Small = 10,

        /// <summary>
        /// A5 paper (148 mm by 210 mm)
        /// </summary>
        A5 = 11,

        /// <summary>
        /// B4 paper (250 mm by 353 mm)
        /// </summary>
        B4 = 12,

        /// <summary>
        /// B5 paper (176 mm by 250 mm)
        /// </summary>
        B5 = 13,

        /// <summary>
        /// Folio paper (8.5 in. by 13 in.)
        /// </summary>
        Folio = 14,

        /// <summary>
        /// Quarto paper (215 mm by 275 mm)
        /// </summary>
        Quarto = 15,

        /// <summary>
        /// Standard paper (10 in. by 14 in.)
        /// </summary>
        Standard10_14 = 16,

        /// <summary>
        /// Standard paper (11 in. by 17 in.)
        /// </summary>
        Standard11_17 = 17,

        /// <summary>
        /// Note paper (8.5 in. by 11 in.)
        /// </summary>
        Note = 18,

        /// <summary>
        /// #9 envelope (3.875 in. by 8.875 in.)
        /// </summary>
        Envelope9 = 19,

        /// <summary>
        /// #10 envelope (4.125 in. by 9.5 in.)
        /// </summary>
        Envelope10 = 20,

        /// <summary>
        /// #11 envelope (4.5 in. by 10.375 in.)
        /// </summary>
        Envelope11 = 21,

        /// <summary>
        /// #12 envelope (4.75 in. by 11 in.)
        /// </summary>
        Envelope12 = 22,

        /// <summary>
        /// #14 envelope (5 in. by 11.5 in.)
        /// </summary>
        Envelope14 = 23,

        /// <summary>
        /// C paper (17 in. by 22 in.)
        /// </summary>
        C = 24,

        /// <summary>
        /// D paper (22 in. by 34 in.)
        /// </summary>
        D = 25,

        /// <summary>
        /// E paper (34 in. by 44 in.)
        /// </summary>
        E = 26,

        /// <summary>
        /// DL envelope (110 mm by 220 mm)
        /// </summary>
        DLEnvelope = 27,

        /// <summary>
        /// C5 envelope (162 mm by 229 mm)
        /// </summary>
        C5Envelope = 28,

        /// <summary>
        /// C3 envelope (324 mm by 458 mm)
        /// </summary>
        C3Envelope = 29,

        /// <summary>
        /// C4 envelope (229 mm by 324 mm)
        /// </summary>
        C4Envelope = 30,

        /// <summary>
        /// C6 envelope (114 mm by 162 mm)
        /// </summary>
        C6Envelope = 31,

        /// <summary>
        /// C65 envelope (114 mm by 229 mm)
        /// </summary>
        C65Envelope = 32,

        /// <summary>
        /// B4 envelope (250 mm by 353 mm)
        /// </summary>
        B4Envelope = 33,

        /// <summary>
        /// B5 envelope (176 mm by 250 mm)
        /// </summary>
        B5Envelope = 34,

        /// <summary>
        /// B6 envelope (176 mm by 125 mm)
        /// </summary>
        B6Envelope = 35,

        /// <summary>
        /// Italy envelope (110 mm by 230 mm)
        /// </summary>
        ItalyEnvelope = 36,

        /// <summary>
        /// Monarch envelope (3.875 in. by 7.5 in.).
        /// </summary>
        MonarchEnvelope = 37,

        /// <summary>
        /// 6 3/4 envelope (3.625 in. by 6.5 in.)
        /// </summary>
        Six3_4Envelope = 38,

        /// <summary>
        /// US standard fanfold (14.875 in. by 11 in.)
        /// </summary>
        USStandard = 39,

        /// <summary>
        /// German standard fanfold (8.5 in. by 12 in.)
        /// </summary>
        GermanStandard = 40,

        /// <summary>
        /// German legal fanfold (8.5 in. by 13 in.)
        /// </summary>
        GermanLegal = 41,

        /// <summary>
        /// ISO B4 (250 mm by 353 mm)
        /// </summary>
        ISOB4 = 42,

        /// <summary>
        ///  Japanese double postcard (200 mm by 148 mm)
        /// </summary>
        JapaneseDoublePostcard = 43,

        /// <summary>
        /// Standard paper (9 in. by 11 in.)
        /// </summary>
        Standard9 = 44,

        /// <summary>
        /// Standard paper (10 in. by 11 in.)
        /// </summary>
        Standard10 = 45,

        /// <summary>
        /// Standard paper (15 in. by 11 in.)
        /// </summary>
        Standard15 = 46,

        /// <summary>
        /// Invite envelope (220 mm by 220 mm)
        /// </summary>
        InviteEnvelope = 47,

        /// <summary>
        /// Letter extra paper (9.275 in. by 12 in.)
        /// </summary>
        LetterExtra = 50,

        /// <summary>
        /// Legal extra paper (9.275 in. by 15 in.)
        /// </summary>
        LegalExtra = 51,

        /// <summary>
        /// Tabloid extra paper (11.69 in. by 18 in.)
        /// </summary>
        TabloidExtra = 52,

        /// <summary>
        /// A4 extra paper (236 mm by 322 mm)
        /// </summary>
        A4Extra = 53,

        /// <summary>
        /// Letter transverse paper (8.275 in. by 11 in.)
        /// </summary>
        LetterTransverse = 54,

        /// <summary>
        /// A4 transverse paper (210 mm by 297 mm)
        /// </summary>
        A4Transverse = 55,

        /// <summary>
        /// Letter extra transverse paper (9.275 in. by 12 in.)
        /// </summary>
        LetterExtraTransverse = 56,

        /// <summary>
        /// SuperA/SuperA/A4 paper (227 mm by 356 mm)
        /// </summary>
        SuperA = 57,

        /// <summary>
        /// SuperB/SuperB/A3 paper (305 mm by 487 mm)
        /// </summary>
        SuperB = 58,

        /// <summary>
        /// Letter plus paper (8.5 in. by 12.69 in.)
        /// </summary>
        LetterPlus = 59,

        /// <summary>
        /// A4 plus paper (210 mm by 330 mm)
        /// </summary>
        A4Plus = 60,

        /// <summary>
        /// A5 transverse paper (148 mm by 210 mm)
        /// </summary>
        A5Transverse = 61,

        /// <summary>
        /// JIS B5 transverse paper (182 mm by 257 mm)
        /// </summary>
        JISB5Transverse = 62,

        /// <summary>
        /// A3 extra paper (322 mm by 445 mm)
        /// </summary>
        A3Extra = 63,

        /// <summary>
        /// A5 extra paper (174 mm by 235 mm)
        /// </summary>
        A5Extra = 64,

        /// <summary>
        /// ISO B5 extra paper (201 mm by 276 mm)
        /// </summary>
        ISOB5 = 65,

        /// <summary>
        /// A2 paper (420 mm by 594 mm)
        /// </summary>
        A2 = 66,

        /// <summary>
        /// A3 transverse paper (297 mm by 420 mm)
        /// </summary>
        A3Transverse = 67,

        /// <summary>
        /// A3 extra transverse paper (322 mm by 445 mm*/
        /// </summary>
        A3ExtraTransverse = 68,
    }
    #endregion
}
