namespace TMP.Extensions
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Static file helper methods
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Upload a file to FTP
        /// </summary>
        /// <param name="filePath">File to upload</param>
        /// <param name="remotePath">Remote path</param>
        /// <param name="logOn">User logOn</param>
        /// <param name="password">User password</param>
        public static void UploadToFTP(string filePath, string remotePath, string logOn, string password)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                string uriString = Path.Combine(remotePath, Path.GetFileName(filePath));
                Uri requestUri = new Uri(uriString);
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(requestUri);
                ftpWebRequest.Credentials = new NetworkCredential(logOn, password);
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.Method = "STOR";
                ftpWebRequest.UseBinary = true;
                ftpWebRequest.ContentLength = fileStream.Length;
                ftpWebRequest.Proxy = null;
                fileStream.Position = 0L;
                int num = 2048;
                byte[] buffer = new byte[num];
                using (Stream requestStream = ftpWebRequest.GetRequestStream())
                {
                    for (int num2 = fileStream.Read(buffer, 0, num); num2 != 0; num2 = fileStream.Read(buffer, 0, num))
                    {
                        requestStream.Write(buffer, 0, num2);
                    }
                }
            }
        }

        /// <summary>
        /// Merge two files into single file
        /// </summary>
        /// <param name="firstFile">First file</param>
        /// <param name="secondFile">Second file</param>
        /// <param name="mergedFile">Path to save the merged file to</param>
        /// <returns></returns>
        public static bool MergeFiles(string firstFile, string secondFile, string mergedFile)
        {
            bool result;
            try
            {
                byte[] buffer = File.ReadAllBytes(firstFile);
                byte[] buffer2 = File.ReadAllBytes(secondFile);
                BinaryWriter binaryWriter = new BinaryWriter(File.Open(mergedFile, FileMode.Create, FileAccess.Write));
                binaryWriter.Write(buffer);
                binaryWriter.Write(buffer2);
                binaryWriter.Close();
            }
            catch
            {
                result = false;
                return result;
            }

            result = true;
            return result;
        }

        /// <summary>
        /// Count the number of lines in a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Number of lines in a file or -1 on error</returns>
        public static int NumberOfLines(string fileName)
        {
            int result;
            try
            {
                TextReader textReader = new StreamReader(fileName);
                string text = textReader.ReadToEnd();
                int length = text.Length;
                int num = length - text.Replace("\n", string.Empty).Length + 1;
                textReader.Close();
                result = num;
            }
            catch
            {
                result = -1;
            }

            return result;
        }

        /// <summary>
        /// Compare the last modified date stamp of two files
        /// </summary>
        /// <param name="firstFile">First file</param>
        /// <param name="secondFile">Second file</param>
        /// <returns>-1 if firstFile is newer, 0 if files are same and 1 if secondFile is newer.</returns>
        public static int CompareFile(string firstFile, string secondFile)
        {
            bool flag = File.Exists(firstFile);
            bool flag2 = File.Exists(secondFile);
            int result;
            if (!flag && !flag2)
            {
                result = 0;
            }
            else
            {
                if (!flag && flag2)
                {
                    result = 1;
                }
                else
                {
                    if (flag && !flag2)
                    {
                        result = -1;
                    }
                    else
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(firstFile);
                        DateTime lastWriteTime2 = File.GetLastWriteTime(secondFile);
                        if (lastWriteTime == lastWriteTime2)
                        {
                            result = 0;
                        }
                        else
                        {
                            if (lastWriteTime < lastWriteTime2)
                            {
                                result = 1;
                            }
                            else
                            {
                                result = -1;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Convert a path to relative path
        /// </summary>
        /// <param name="fromDirectory">Convert from path</param>
        /// <param name="toPath">To relative path</param>
        /// <returns>Relative path of the conversion path given</returns>
        public static string GetRelativePath(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
            {
                throw new ArgumentNullException(nameof(fromDirectory));
            }

            if (toPath == null)
            {
                throw new ArgumentNullException(nameof(toPath));
            }

            bool flag = Path.IsPathRooted(fromDirectory) && Path.IsPathRooted(toPath);
            string result;
            if (flag)
            {
                bool flag2 = string.Compare(Path.GetPathRoot(fromDirectory), Path.GetPathRoot(toPath), true, CultureInfo.CurrentCulture) != 0;
                if (flag2)
                {
                    result = toPath;
                    return result;
                }
            }

            StringCollection stringCollection = new StringCollection();
            string[] array = fromDirectory.Split(new char[]
            {
                Path.DirectorySeparatorChar,
            });
            string[] array2 = toPath.Split(new char[]
            {
                Path.DirectorySeparatorChar,
            });
            int num = Math.Min(array.Length, array2.Length);
            int num2 = -1;
            for (int i = 0; i < num; i++)
            {
                if (string.Compare(array[i], array2[i], true, CultureInfo.CurrentCulture) != 0)
                {
                    break;
                }

                num2 = i;
            }

            if (num2 == -1)
            {
                result = toPath;
            }
            else
            {
                for (int i = num2 + 1; i < array.Length; i++)
                {
                    if (array[i].Length > 0)
                    {
                        stringCollection.Add("..");
                    }
                }

                for (int i = num2 + 1; i < array2.Length; i++)
                {
                    stringCollection.Add(array2[i]);
                }

                string[] array3 = new string[stringCollection.Count];
                stringCollection.CopyTo(array3, 0);
                string text = string.Join(Path.DirectorySeparatorChar.ToString(), array3);
                result = text;
            }

            return result;
        }
    }
}
