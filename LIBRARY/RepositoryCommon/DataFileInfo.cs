namespace TMP.Common.RepositoryCommon
{
    using System;
    using System.Runtime.Serialization;

    [KnownType(typeof(DatePeriod))]
    [MessagePack.MessagePackObject(keyAsPropertyName: true)]
    public class DataFileInfo : Shared.PropertyChangedBase, IDataFileInfo
    {
        [MessagePack.IgnoreMember]
        protected Version version = new Version(1, 1);
        [MessagePack.IgnoreMember]
        protected DatePeriod period;
        [MessagePack.IgnoreMember]
        protected bool isLocal = true;

        [MessagePack.IgnoreMember]
        private string fileName;
        [MessagePack.IgnoreMember]
        private DateTime lastModifiedDate;
        [MessagePack.IgnoreMember]
        private long fileSize;
        [MessagePack.IgnoreMember]
        private bool isLoaded;
        [MessagePack.IgnoreMember]
        private bool isSelected;

        /// <summary>
        /// Версия данных
        /// </summary>
        public Version Version
        {
            get => this.version;
            set => this.SetProperty(ref this.version, value);
        }

        /// <summary>
        /// Имя файла
        /// </summary>
        [MessagePack.IgnoreMember]
        public string FileName
        {
            get => this.fileName;
            set => this.SetProperty(ref this.fileName, value);
        }

        /// <summary>
        /// Дата последнего изменения
        /// </summary>
        [MessagePack.IgnoreMember]
        public DateTime LastModifiedDate
        {
            get => this.lastModifiedDate;
            set => this.SetProperty(ref this.lastModifiedDate, value);
        }

        /// <summary>
        /// Размер файла
        /// </summary>
        [MessagePack.IgnoreMember]
        public long FileSize
        {
            get => this.fileSize;
            set => this.SetProperty(ref this.fileSize, value);
        }

        /// <summary>
        /// Признак загруженного файла
        /// </summary>
        [MessagePack.IgnoreMember]
        public bool IsLoaded
        {
            get => this.isLoaded;
            set => this.SetProperty(ref this.isLoaded, value);
        }

        /// <summary>
        /// Временной период хранящихся данных
        /// </summary>
        [DataMember]
        public DatePeriod Period
        {
            get => this.period;
            set => this.SetProperty(ref this.period, value);
        }

        /// <summary>
        /// Признак, указывающий, что файл выбран
        /// </summary>
        [MessagePack.IgnoreMember]
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        /// <summary>
        /// Указывает, что база данных, откуда получены данные, недоступна
        /// </summary>
        [MessagePack.IgnoreMember]
        public virtual bool IsLocal => this.isLocal;

        public override int GetHashCode()
        {
            return System.HashCode.Combine(this.Version, this.FileName, this.Period, this.IsSelected);
        }
    }
}
