using System;
using System.IO;
using TMP.Common.RepositoryCommon;

namespace TMP.Work.Emcos
{
    public partial class Repository
    {
        #region Public Methods

        /// <summary>
        /// Сохранение данных
        /// </summary>
        /// <returns>True если сохранение произошло успешно</returns>
        public bool Save()
        {
            bool result = SaveSessionData(null);
            Saved?.Invoke(null, EventArgs.Empty);
            return result;
        }
        /// <summary>
        /// Сохранение данных в указанный файл
        /// </summary>
        /// <param name="newFileName">Имя файла</param>
        /// <returns>True если сохранение произошло успешно</returns>
        public bool SaveAs(string newFileName)
        {
            bool result = SaveSessionData(newFileName);
            Saved?.Invoke(null, EventArgs.Empty);
            return result;
        }
        /// <summary>
        /// Создание резервной копии сессии
        /// </summary>
        public void SaveBackup()
        {
            if (ActiveSession == null || ActiveSession.Info.Period == null)
                return;
            SaveSessionData(BALANCE_SESSION_FILENAME + SESSION_FILE_EXTENSION + ".bak");
        }
        
        /// <summary>
        /// Сохранение конфигурации
        /// </summary>
        /// <returns>True если успешно</returns>
        public bool SaveConfiguration()
        {
            try
            {
                using (System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(CONFIGURATION_FILENAME, FileMode.Create, FileAccess.Write))
                {
                    // Коллекция элементов
                    byte[] bytes = JsonSerializer.JsonSerializeToBytes(ActiveSession.BalancePoints, _callBackAction);
                    SaveJsonDataToPart(bytes, package, PART_BalancePoints);
                    // Словарь пар код группы - формула расчёта баланса
                    bytes = JsonSerializer.JsonSerializeToBytes(BalanceGroupsFormulaById, _callBackAction);
                }
            }
            catch (Exception ex)
            {
                _callBackAction(ex);
                return false;
            }
            return true;
        }

        #endregion

        #region Private Methods



        /// <summary>
        /// Сохранение сессии
        /// </summary>
        /// <param name="fileName">Имя файла, если не указано, то загрузка из стандартного файла <see cref="BALANCE_SESSION_FILENAME"/></param>
        /// <returns>True если загрузка произошла успешно</returns>
        private bool SaveSessionData(string fileName = null)
        {
            if (ActiveSession == null)
                throw new InvalidOperationException();

            bool mustStoreLastSessionFileName = false;
            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = BALANCE_SESSION_FILENAME + SESSION_FILE_EXTENSION;
                try
                {
                    if (File.Exists(Path.Combine(SESSIONS_FOLDER, "lastsession")))
                        File.Delete(Path.Combine(SESSIONS_FOLDER, "lastsession"));
                }
                catch (IOException ex)
                {
                    _callBackAction(ex);
                }
            }
            else
            {
                if (fileName.EndsWith(".bak") == false)
                {
                    fileName = fileName + SESSION_FILE_EXTENSION;
                    mustStoreLastSessionFileName = true;
                }
            }

            ActiveSession.Info.FileName = fileName;

            using (System.IO.Packaging.Package package = System.IO.Packaging.Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                // Общая информация
                byte[] bytes = JsonSerializer.JsonSerializeToBytes(ActiveSession.Info, _callBackAction);
                SaveJsonDataToPart(bytes, package, PART_Info);
                // Коллекция элементов
                bytes = JsonSerializer.JsonSerializeToBytes(ActiveSession.BalancePoints, _callBackAction);
                SaveJsonDataToPart(bytes, package, PART_BalancePoints);

                SaveJsonDataToPart(bytes, package, PART_BalanceGroupsFormulaById);
                // Словарь пар код группы - баланс активной энергии
                bytes = JsonSerializer.JsonSerializeToBytes(ActiveSession.ActiveEnergyBalanceById, _callBackAction);
                SaveJsonDataToPart(bytes, package, PART_ActiveEnergyBalanceById);
                // Словарь пар код группы - баланс реактивной энергии
                bytes = JsonSerializer.JsonSerializeToBytes(ActiveSession.ReactiveEnergyBalanceById, _callBackAction);
                SaveJsonDataToPart(bytes, package, PART_ReactiveEnergyBalanceById);
                // Словарь пар код элемента - активная энергия
                bytes = JsonSerializer.JsonSerializeToBytes(ActiveSession.ActiveEnergyById, _callBackAction);
                SaveJsonDataToPart(bytes, package, PART_ActiveEnergyById);
                // Словарь пар код элемента - реактивная энергия
                bytes = JsonSerializer.JsonSerializeToBytes(ActiveSession.ReactiveEnergyById, _callBackAction);
                SaveJsonDataToPart(bytes, package, PART_ReactiveEnergyById);
                // Словарь пар код элемента - описание
                bytes = JsonSerializer.JsonSerializeToBytes(ActiveSession.DescriptionsById, _callBackAction);
                SaveJsonDataToPart(bytes, package, PART_DescriptionsById);
            }

            if (JsonSerializer.GzJsonSerialize(
                ActiveSession,
                Path.Combine(SESSIONS_FOLDER, fileName),
                _callBackAction) == false)
                    return false;

            // сохранение имени файла последней сессии
            if (mustStoreLastSessionFileName)
                File.WriteAllText(Path.Combine(SESSIONS_FOLDER, "lastsession"), fileName);
            return true;
        }

        private void SaveJsonDataToPart(byte[] bytes, System.IO.Packaging.Package package, string partName)
        {
            System.IO.Packaging.PackagePart part = package.CreatePart(System.IO.Packaging.PackUriHelper.CreatePartUri(new Uri(partName, UriKind.Relative)), System.Net.Mime.MediaTypeNames.Text.Plain);
            using (Stream outStream = part.GetStream())
            {
                outStream.Write(bytes, 0, bytes.Length);
            }
        }

        #endregion
    }
}
