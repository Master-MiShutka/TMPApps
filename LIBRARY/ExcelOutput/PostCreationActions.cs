namespace TMP.ExcelOutput
{
    /// <summary>
    /// Enumerator of actions to perform after the Excel file has been generated.
    /// </summary>
    public enum PostCreationActions
    {
        /// <summary>
        /// Open Excel and display the generated table.
        /// </summary>
        Open,
        /// <summary>
        /// Save the file to the specified location, then open Excel and display the generated table.
        /// </summary>
        SaveAndView,
        /// <summary>
        /// Save the file to the specified location
        /// </summary>
        SaveAndClose
    }
}
