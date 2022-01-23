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
        private IList<T> CheckAndLoadFromCache<T>(string fileName, ref Model.WorkTask workTask)
        {
            var fileInfo = new System.IO.FileInfo(fileName);
            List<T> data = null;

            if (this.dataFilesInfo.ContainsKey(fileName))
            {
                workTask.UpdateStatus($"загрузка данных из кэша ...");
                workTask.IsIndeterminate = true;

                var info = this.dataFilesInfo[fileName];

                if (info.LastModified == fileInfo.LastWriteTime)
                {
                    T[] result = this.DeserializeData<T>(fileName);

                    if (result != null)
                        data = new List<T>(result);
                }
                else
                {
                    // расчет хэша файла и сравнение с ранее сохраненным
                    string hashAsString = this.CalculateSHA256(fileName);

                    if (string.Equals(info.Hash, hashAsString))
                    {
                        var result = this.DeserializeData<T>(fileName);
                        if (result != null)
                            data = new List<T>(result);
                    }
                }
            }

            return data;
        }

        private void StoreHashAndSaveData<T>(string fileName, ref WorkTask workTask, T[] data)
        {
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

            var fileInfo = new System.IO.FileInfo(fileName);
            DataFileRecord dataFileRecord = new DataFileRecord() { FileName = fileName, Hash = hashAsString, LastModified = fileInfo.LastWriteTime };
            if (this.dataFilesInfo.ContainsKey(fileName))
                this.dataFilesInfo[fileName] = dataFileRecord;
            else
                this.dataFilesInfo.Add(fileName, dataFileRecord);

            this.SerializeData<T>(data, fileName);
        }

        private string GetDbTableNamePath(string fileName)
        {
            string s = Path.GetFileNameWithoutExtension(Path.GetRelativePath(this.aramisDbPath, fileName).Replace(Path.DirectorySeparatorChar, '-'));

            return Path.Combine(this.dataFilesPath, s + DATA_FILE_EXTENSION);
        }

        private void SerializeData<T>(T[] data, string fileName)
        {
            try
            {
                if (Directory.Exists(this.dataFilesPath) == false)
                {
                    Directory.CreateDirectory(this.dataFilesPath);
                }

                string fullFileName = this.GetDbTableNamePath(fileName);

                using System.IO.FileStream fs = new System.IO.FileStream(fullFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
                MessagePack.MessagePackSerializer.Serialize<T[]>(fs, data, MessagePack.MessagePackSerializer.DefaultOptions);
            }
            catch (Exception ex)
            {
                this.callBackAction(ex);
            }
        }

        private T[] DeserializeData<T>(string fileName)
        {
            try
            {
                string fullFileName = this.GetDbTableNamePath(fileName);

                if (File.Exists(fullFileName) == false)
                {
                    return null;
                }

                using System.IO.FileStream fs = new System.IO.FileStream(fullFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                T[] result = MessagePack.MessagePackSerializer.Deserialize<T[]>(fs, MessagePack.MessagePackSerializer.DefaultOptions);

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
            using (var stream = new BufferedStream(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), bufferedStreamBufferSize))
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

            var result = source.OrderBy(i => i.Лицевой).ToList();

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
