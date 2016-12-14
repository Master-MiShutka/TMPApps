using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TMP.Shared;
using TMP.ARMTES.Model;

namespace TMP.ARMTES.Editor
{
    public class MainModel : PropertyChangedBase
    {
        private static MainModel instance;

        private RegistrySDSP sdsp = new RegistrySDSP();

        private MainModel() { }

        static MainModel()
        {
            instance = new MainModel();
        }

        public static MainModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainModel();
                }
                return instance;

            }
        }

        /// <summary>
        /// Реестр СДСП
        /// </summary>
        public RegistrySDSP SDSP
        {
            get { return sdsp; }
            set { SetProp<RegistrySDSP>(ref sdsp, value, "SDSP"); }
        }
    }
}
