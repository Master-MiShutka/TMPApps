using System;
using System.Configuration;
using TMP.Shared;

namespace TMP.Work.Emcos
{
    public class EmcosSettings : ApplicationSettingsBase
    {
        private static EmcosSettings defaultInstance = ((EmcosSettings)(ApplicationSettingsBase.Synchronized(new EmcosSettings())));
        public static EmcosSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSetting, DefaultSettingValue("sbyt")]
        public string UserName
        {
            get { return (string)this["UserName"]; }
            set { this["UserName"] = value; }
        }
        [UserScopedSetting, DefaultSettingValue("sbyt")]
        public string Password
        {
            get { return (string)this["Password"]; }
            set { this["Password"] = value; }
        }
        [UserScopedSetting, DefaultSettingValue("10.96.18.16")]
        public string ServerAddress
        {
            get { return (string)this["ServerAddress"]; }
            set { this["ServerAddress"] = value; }
        }
        [UserScopedSetting, DefaultSettingValue("testWebService/Service.asmx")]
        public string WebServiceName
        {
            get { return (string)this["WebServiceName"]; }
            set { this["WebServiceName"] = value; }
        }
        [UserScopedSetting, DefaultSettingValue("emcos")]
        public string SiteName
        {
            get { return (string)this["SiteName"]; }
            set { this["SiteName"] = value; }
        }
        [UserScopedSetting, DefaultSettingValue("1800")]
        public int NetTimeOutInSeconds
        {
            get { return (int)this["NetTimeOutInSeconds"]; }
            set { this["NetTimeOutInSeconds"] = value; }
        }
    }
}