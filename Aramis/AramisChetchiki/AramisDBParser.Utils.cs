namespace TMP.WORK.AramisChetchiki
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using TMP.WORK.AramisChetchiki.Model;

    internal partial class AramisDBParser
    {
        private async Task<IList<T>> CheckAndLoadFromCacheAsync<T>(string fileName, Model.WorkTask workTask)
        {
            FileInfo fileInfo = new System.IO.FileInfo(fileName);
            List<T> data = null;
            string cachedFileName = this.GetDbTableNamePath(fileName);

            if (this.dataFilesInfo.ContainsKey(fileName))
            {
                workTask.UpdateStatus($"загрузка данных из кэша ...");
                workTask.IsIndeterminate = true;

                DataFileRecord info = this.dataFilesInfo[fileName];

                if (info.LastModified == fileInfo.LastWriteTime)
                {
                    T[] result = await this.DeserializeDataAsync<T>(cachedFileName);

                    if (result != null)
                    {
                        data = new List<T>(result);
                    }
                }
                else
                {
                    // расчет хэша файла и сравнение с ранее сохраненным
                    string hashAsString = this.CalculateSHA256(fileName);

                    if (string.Equals(info.Hash, hashAsString))
                    {
                        T[] result = await this.DeserializeDataAsync<T>(cachedFileName);
                        if (result != null)
                        {
                            data = new List<T>(result);
                        }
                    }
                    else
                    {
                        ;
                    }
                }
            }
            else
            {
                ;
            }

            return data;
        }

        private void StoreHashAndSaveData<T>(FileInfo fileInfo, WorkTask workTask, T[] data)
        {
            string fileName = fileInfo.FullName;

            string msg = "вычисление контрольной-суммы файла ...";
            workTask.UpdateStatus(msg);
            workTask.IsIndeterminate = true;

            string hashAsString = string.Empty;
            bool isOk = false;
            byte numberOfRetries = 1;
            do
            {
                try
                {
                    hashAsString = this.CalculateSHA256(fileName);
                    isOk = true;
                }
                catch (IOException ex)
                {
                    this.callBackAction(ex);
                    workTask.UpdateStatus(string.Format("{0}\nфайл используется другим процессом, попытка #{1}", msg, numberOfRetries));
                    Task.Delay(1_000);
                }

                numberOfRetries++;
            } while (isOk == false);

            DataFileRecord dataFileRecord = new DataFileRecord(fileName, hashAsString, fileInfo.LastWriteTime);
            if (this.dataFilesInfo.ContainsKey(fileName))
            {
                this.dataFilesInfo[fileName] = dataFileRecord;
            }
            else
            {
                this.dataFilesInfo.Add(fileName, dataFileRecord);
            }

            string cachedFileName = this.GetDbTableNamePath(fileName);
            _ = this.SerializeDataAsync<T>(data, cachedFileName);
        }

        private string GetDbTableNamePath(string fileName)
        {
            string s = Path.GetFileNameWithoutExtension(Path.GetRelativePath(this.aramisDbPath, fileName).Replace(Path.DirectorySeparatorChar, '-'));

            return Path.Combine(this.dataFilesPath, s + DATA_FILE_EXTENSION);
        }

        private async Task SerializeDataAsync<T>(T[] data, string fileName)
        {
            try
            {
                if (Directory.Exists(this.dataFilesPath) == false)
                {
                    Directory.CreateDirectory(this.dataFilesPath);
                }

                string fullFileName = this.GetDbTableNamePath(fileName);

                _ = await TMP.Common.RepositoryCommon.MessagePackSerializer.ToFileAsync<T[]>(data, fileName);
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
            }
        }

        private async Task<T[]> DeserializeDataAsync<T>(string fileName)
        {
            try
            {
                if (File.Exists(fileName) == false)
                {
                    return null;
                }

                T[] result = await TMP.Common.RepositoryCommon.MessagePackDeserializer.FromFileAsync<T[]>(fileName);

                return result;
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
                return null;
            }
        }

        private string CalculateSHA256(string fileName)
        {
            string hashAsString = string.Empty;

            // Not sure if BufferedStream should be wrapped in using block
            using (BufferedStream stream = new BufferedStream(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), bufferedStreamBufferSize))
            {
                using (System.Security.Cryptography.SHA256 mySHA256 = System.Security.Cryptography.SHA256.Create())
                {
                    // Compute the hash of the fileStream.
                    byte[] hashValue = mySHA256.ComputeHash(stream);

                    hashAsString = BitConverter.ToString(hashValue).Replace("-", string.Empty);
                }
            }

            return hashAsString;
        }

        private IList<T> SortData<T>(IEnumerable<T> source, WorkTask workTask = null)
            where T : IModelWithPersonalId
        {
            bool removeTaskAfterCompleted = false;

            if (workTask == null)
            {
                workTask = new("Обработка");
                this.workTasksProgressViewModel.WorkTasks.Add(workTask);
                workTask.StartProcessing();
                removeTaskAfterCompleted = true;
            }

            workTask.Status = "Сортировка данных";
            workTask.IsIndeterminate = true;

            List<T> result = source.OrderBy(i => i.Лицевой).ToList();

            workTask.IsCompleted = true;

            if (removeTaskAfterCompleted)
            {
                this.workTasksProgressViewModel.WorkTasks.Remove(workTask);
            }

            return result;
        }

        private TValue GetDictionaryValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (key is string keyAsString && string.IsNullOrWhiteSpace(keyAsString))
            {
                return default;
            }
            else
            {
                if (dictionary.ContainsKey(key))
                {
                    return dictionary[key];
                }
                else
                {
                    return default;
                }
            }
        }
    }
}
