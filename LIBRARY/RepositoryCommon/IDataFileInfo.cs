namespace TMP.Common.RepositoryCommon
{
    using System;
    using MessagePack;
    using MessagePack.Formatters;
    using MsgPack.Serialization;

    [MessagePack.Union(0, typeof(DataFileInfo))]
    public interface IDataFileInfo
    {
        /// <summary>
        /// Версия данных
        /// </summary>
        Version Version { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Дата последнего изменения
        /// </summary>
        DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// Размер файла
        /// </summary>
        long FileSize { get; set; }

        /// <summary>
        /// Признак загруженного файла
        /// </summary>
        bool IsLoaded { get; set; }

        /// <summary>
        /// Временной период хранящихся данных
        /// </summary>
        DatePeriod Period { get; set; }

        /// <summary>
        /// Указывает, что используются локальные данные
        /// </summary>
        bool IsLocal { get; }

        /// <summary>
        /// Признак, указывающий, что файл выбран
        /// </summary>
        bool IsSelected { get; set; }
    }
}
