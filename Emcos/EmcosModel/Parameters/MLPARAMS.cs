namespace TMP.Work.Emcos.Model
{
    /// <summary>
    /// Измерения
    /// </summary>
    public static class MLPARAMS
    {
        #region За сутки

        /// <summary>
        /// А + энергия за сутки
        /// </summary>
        public static Model.ML_Param A_PLUS_ENERGY_DAYS
        {
            get
            {
                return new Model.ML_Param
                {
                    Name = "A+ энергия за сутки",
                    Id = "385",
                    MD = new Model.MD_Param { Id = "5", Code = "DAY", Per_Id = "4", Name = "Месячные", Per_Code = "DD", Per_Name = "Сутки" },
                    MSF = new MSF_Param() { Id = "14", Code = "A" },
                    DIR = new DIR_Param() { Id = "1", Code = "+" },
                    EU = new EU_Param() { Id = "2", Code = "kWh", Name = "Киловаттчасы" },
                    MS = new MS_Param() { Code = "P", Name = "Эл. активная мощность/энергия", Id = "1" },
                    AGGS = new Model.AGGS_Param { Id = "5", Value = "1", Per_Id = "4", Per_Code = "DD", Per_Name = "Сутки" }
                };
            }
        }
        /// <summary>
        /// А - энергия за сутки
        /// </summary>
        public static Model.ML_Param A_MINUS_ENERGY_DAYS
        {
            get
            {
                return new Model.ML_Param
                {
                    Name = "A- энергия за сутки",
                    Id = "386",
                    MD = new Model.MD_Param { Id = "5", Code = "DAY", Per_Id = "4", Name = "Месячные", Per_Code = "DD", Per_Name = "Сутки" },
                    MSF = new MSF_Param() { Id = "14", Code = "A" },
                    DIR = new DIR_Param() { Id = "2", Code = "-" },
                    EU = new EU_Param() { Id = "2", Code = "kWh", Name = "Киловаттчасы" },
                    MS = new MS_Param() { Code = "P", Name = "Эл. активная мощность/энергия", Id = "1" },
                    AGGS = new Model.AGGS_Param { Id = "5", Value = "1", Per_Id = "4", Per_Code = "DD", Per_Name = "Сутки" }
                };
            }
        }

        /// <summary>
        /// R + энергия за сутки
        /// </summary>
        public static Model.ML_Param R_PLUS_ENERGY_DAYS
        {
            get
            {
                return new Model.ML_Param
                {
                    Name = "R+ энергия за сутки",
                    Id = "387",
                    MD = new Model.MD_Param { Id = "5", Code = "DAY", Per_Id = "4", Name = "Месячные", Per_Code = "DD", Per_Name = "Сутки" },
                    MSF = new MSF_Param() { Id = "14", Code = "R" },
                    DIR = new DIR_Param() { Id = "1", Code = "+" },
                    EU = new EU_Param() { Id = "4", Code = "kVarh", Name = "Киловарчасы" },
                    MS = new MS_Param() { Code = "Q", Name = "Эл. реактивная мощность/энергия", Id = "3" },
                    AGGS = new Model.AGGS_Param { Id = "5", Value = "1", Per_Id = "4", Per_Code = "DD", Per_Name = "Сутки" }
                };
            }
        }
        /// <summary>
        /// R - энергия за сутки
        /// </summary>
        public static Model.ML_Param R_MINUS_ENERGY_DAYS
        {
            get
            {
                return new Model.ML_Param
                {
                    Name = "R- энергия за сутки",
                    Id = "388",
                    MD = new Model.MD_Param { Id = "5", Code = "DAY", Per_Id = "4", Name = "Месячные", Per_Code = "DD", Per_Name = "Сутки" },
                    MSF = new MSF_Param() { Id = "14", Code = "R" },
                    DIR = new DIR_Param() { Id = "2", Code = "-" },
                    EU = new EU_Param() { Id = "4", Code = "kVarh", Name = "Киловарчасы" },
                    MS = new MS_Param() { Code = "Q", Name = "Эл. реактивная мощность/энергия", Id = "3" },
                    AGGS = new Model.AGGS_Param { Id = "5", Value = "1", Per_Id = "4", Per_Code = "DD", Per_Name = "Сутки" }
                };
            }
        }

        #endregion

        #region За месяц

        /// <summary>
        /// А + энергия за месяц
        /// </summary>
        public static Model.ML_Param A_PLUS_ENERGY_MONTH
        {
            get
            {
                return new Model.ML_Param
                {
                    Name = "A+ энергия за месяц",
                    Id = "381",
                    MD = new Model.MD_Param { Id = "6", Code = "MONTH", Per_Id = "5", Name = "Месячные", Per_Code = "MM", Per_Name = "Месяц" },
                    MSF = new MSF_Param() { Id = "14", Code = "A" },
                    DIR = new DIR_Param() { Id = "1", Code = "+" },
                    EU = new EU_Param() { Id = "2", Code = "kWh", Name = "Киловаттчасы" },
                    MS = new MS_Param() { Code = "P", Name = "Эл. активная мощность/энергия", Id = "1" },
                    AGGS = new Model.AGGS_Param { Id = "6", Value = "1", Per_Id = "5", Per_Code = "MM", Per_Name = "Месяц" },
                };
            }
        }
        /// <summary>
        /// А - энергия за месяц
        /// </summary>
        public static Model.ML_Param A_MINUS_ENERGY_MONTH
        {
            get
            {
                return new Model.ML_Param
                {
                    Name = "A- энергия за месяц",
                    Id = "382",
                    MD = new Model.MD_Param { Id = "6", Code = "MONTH", Per_Id = "5", Name = "Месячные", Per_Code = "MM", Per_Name = "Месяц" },
                    MSF = new MSF_Param() { Id = "14", Code = "A" },
                    DIR = new DIR_Param() {  Id="2", Code = "-"},
                    EU = new EU_Param() { Id="2", Code = "kWh", Name = "Киловаттчасы" },
                    MS = new MS_Param() { Code = "P", Name = "Эл. активная мощность/энергия", Id = "1" },
                    AGGS = new Model.AGGS_Param { Id = "6", Value = "1", Per_Id = "5", Per_Code = "MM", Per_Name = "Месяц" },
                    };
            }
        }

        /// <summary>
        /// R + энергия за месяц
        /// </summary>
        public static Model.ML_Param R_PLUS_ENERGY_MONTH
        {
            get
            {
                return new Model.ML_Param
                {
                    Name = "R+ энергия за месяц",
                    Id = "383",
                    MD = new Model.MD_Param { Id = "6", Code = "MONTH", Per_Id = "5", Name = "Месячные", Per_Code = "MM", Per_Name = "Месяц" },
                    MSF = new MSF_Param() { Id = "14", Code = "R"},
                    DIR = new DIR_Param() { Id = "1", Code = "+" },
                    EU = new EU_Param() { Id = "4", Code = "kVarh", Name = "Киловарчасы" },
                    MS = new MS_Param() { Code = "Q", Name = "Эл. реактивная мощность/энергия", Id = "3" },
                    AGGS = new Model.AGGS_Param { Id = "6", Value = "1", Per_Id = "5", Per_Code = "MM", Per_Name = "Месяц" },
                };
            }
        }
        /// <summary>
        /// R - энергия за месяц
        /// </summary>
        public static Model.ML_Param R_MINUS_ENERGY_MONTH
        {
            get
            {
                return new Model.ML_Param
                {
                    Name = "R- энергия за месяц",
                    Id = "384",
                    MD = new Model.MD_Param { Id = "6", Code = "MONTH", Per_Id = "5", Name = "Месячные", Per_Code = "MM", Per_Name = "Месяц" },
                    MSF = new MSF_Param() { Id = "14", Code = "R" },
                    DIR = new DIR_Param() { Id = "2", Code = "-" },
                    EU = new EU_Param() { Id = "4", Code = "kVarh", Name = "Киловарчасы" },
                    MS = new MS_Param() { Code = "Q", Name = "Эл. реактивная мощность/энергия", Id = "3" },
                    AGGS = new Model.AGGS_Param { Id = "6", Value = "1", Per_Id = "5", Per_Code = "MM", Per_Name = "Месяц" },
                };
            }
        }

        #endregion
    }
}
