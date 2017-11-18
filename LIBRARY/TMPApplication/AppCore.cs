using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMPApplication
{
    using System;
    using System.Globalization;
    using System.Reflection;
    public class AppCore
    {
        #region Fields

        protected static TMP.Common.Logger.ILoggerFacade Logger;

        #endregion

        #region Constructor

        static AppCore()
        {
            Logger = ServiceInjector.Instance.GetService<TMP.Common.Logger.ILoggerFacade>();
        }

        #endregion

        #region Properties

        public static string AssemblyTitle
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Name;
            }
        }

        public static string AssemblyEntryLocation
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }
        public static string AppDataFolder
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                                 System.IO.Path.DirectorySeparatorChar +
                                                 AppCore.Company;
            }
        }
        public static string AppDataSettingFileName
        {
            get
            {
                return System.IO.Path.Combine(AppCore.AppDataFolder,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.settings", AppCore.AssemblyTitle));
            }
        }
        public static string AppSessionFileName
        {
            get
            {
                return System.IO.Path.Combine(AppCore.AppDataFolder,
                                              string.Format(CultureInfo.InvariantCulture, "{0}.App.session", AppCore.AssemblyTitle));
            }
        }
        public static string MyDocumentsFolder
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }


        public static string Company
        {
            get
            {
                return "TMPApps";
            }
        }

        #endregion

        #region Methods

        public static bool CreateAppDataFolder()
        {
            try
            {
                if (System.IO.Directory.Exists(AppCore.AppDataFolder) == false)
                    System.IO.Directory.CreateDirectory(AppCore.AppDataFolder);
            }
            catch (Exception exp)
            {
                if (Logger != null)
                    Logger.LogException(exp);
                return false;
            }

            return true;
        }

        #endregion
    }
}