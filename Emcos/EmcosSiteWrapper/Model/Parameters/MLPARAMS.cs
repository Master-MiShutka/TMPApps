using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMP.Work.Emcos.Model
{
    /// <summary>
    /// Измерения
    /// </summary>
    public static class MLPARAMS
    {
        /// <summary>
        /// А + энергия за сутки
        /// </summary>
        public static Model.ML_Param A_PLUS_ENERGY_DAYS
        {
            get
            {
                return new Model.ML_Param
                {
                    Id = "385",
                    MD = new Model.MD_Param { Id = "5" },
                    AGGS = new Model.AGGS_Param { Id = "5" },
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
                    Id = "386",
                    MD = new Model.MD_Param { Id = "5" },
                    AGGS = new Model.AGGS_Param { Id = "5" },
                };
            }
        }
        /// <summary>
        /// А + энергия за месяц
        /// </summary>
        public static Model.ML_Param A_PLUS_ENERGY_MONTH
        {
            get
            {
                return new Model.ML_Param
                {
                    Id = "381",
                    MD = new Model.MD_Param { Id = "6" },
                    AGGS = new Model.AGGS_Param { Id = "6" },
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
                    Id = "382",
                    MD = new Model.MD_Param { Id = "6" },
                    AGGS = new Model.AGGS_Param { Id = "6" },
                };
            }
        }
    }
}
