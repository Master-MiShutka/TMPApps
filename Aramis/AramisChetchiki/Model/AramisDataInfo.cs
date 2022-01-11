namespace TMP.WORK.AramisChetchiki.Model
{
    using System.Runtime.Serialization;
    using MessagePack;
    using TMP.Common.RepositoryCommon;

    [KnownType(typeof(DatePeriod))]
    [MessagePackObject]
    [MessagePackFormatter(typeof(AramisDataInfoFormatter))]
    public class AramisDataInfo : TMP.Common.RepositoryCommon.DataFileInfo
    {
        [IgnoreMember]
        private string departamentName;
        [IgnoreMember]
        private string aramisDbPath = string.Empty;

        public AramisDataInfo()
        {
            this.period = new DatePeriod();
        }

        public AramisDataInfo(TMP.Common.RepositoryCommon.IDataFileInfo dataFileInfo) : base()
        {
            this.version = dataFileInfo?.Version;
            this.period = dataFileInfo?.Period;
        }

        /// <summary>
        /// Название подразделения
        /// </summary>
        [Key(2)]
        public string DepartamentName
        {
            get => this.departamentName;
            set => this.SetProperty(ref this.departamentName, value);
        }

        /// <summary>
        /// Путь к базе данных Арамис, откуда получены данные
        /// </summary>
        [Key(3)]
        public string AramisDbPath
        {
            get => this.aramisDbPath;
            set
            {
                if (this.SetProperty(ref this.aramisDbPath, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsLocal));
                }
            }
        }

        [IgnoreMember]
        public override bool IsLocal => string.IsNullOrWhiteSpace(this.aramisDbPath) == false && System.IO.Directory.Exists(this.aramisDbPath);
    }
}
