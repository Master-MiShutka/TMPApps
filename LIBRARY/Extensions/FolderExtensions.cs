using System;
using System.IO;

namespace TMP.Extensions
{
    /// <summary>
    /// Static folder helper methods
    /// </summary>
    public static class FolderExtensions
    {
        /// <summary>
        /// Returns all files in a directory and its subdirectories as a string array
        /// </summary>
        /// <param name="rootPath">Path of the directory</param>
        /// <returns>String array containing all the files</returns>
        public static string[] GetFileList(string rootPath)
        {
            return FolderExtensions.GetFileList(rootPath, null);
        }
        /// <summary>
        /// Returns all files in a directory and its subdirectories as a string array matching to a pattern
        /// </summary>
        /// <param name="rootPath">Path of the directory</param>
        /// <param name="pattern">Pattern to match</param>
        /// <returns>String array containing all the files</returns>
        public static string[] GetFileList(string rootPath, string pattern)
        {
            if (rootPath.IsNullOrEmpty())
            {
                pattern = "*.*";
            }
            string[] result;
            try
            {
                result = Directory.GetFiles(rootPath, pattern, SearchOption.AllDirectories);
                return result;
            }
            catch
            {
            }
            result = null;
            return result;
        }
    }
}
