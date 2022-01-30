using System.Configuration;
namespace TMP.WORK.AramisChetchiki.Properties
{
    [System.Configuration.SettingsProvider(typeof(TMP.Shared.Settings.PortableSettingsProvider))]
    public sealed class Settings : System.Configuration.ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default => defaultInstance;

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("14")]
        public double FontSize
        {
            get => ((double)(this["FontSize"]));
            set => this["FontSize"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("d:\\aramis\\Disks\\OSHM\\aramis")]
        public string AramisDBPath
        {
            get => ((string)(this["AramisDBPath"]));
            set => this["AramisDBPath"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("")]
        public string DataFilesStorePath
        {
            get => ((string)(this["DataFilesStorePath"]));
            set => this["DataFilesStorePath"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public TMP.WORK.AramisChetchiki.ChangesOfMetersFieldsCollection ChangesOfMetersFields
        {
            get => ((TMP.WORK.AramisChetchiki.ChangesOfMetersFieldsCollection)(this["ChangesOfMetersFields"]));
            set => this["ChangesOfMetersFields"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public TMP.WORK.AramisChetchiki.SummaryInfoFieldsCollection SummaryInfoFields
        {
            get => ((TMP.WORK.AramisChetchiki.SummaryInfoFieldsCollection)(this["SummaryInfoFields"]));
            set => this["SummaryInfoFields"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("ViewAsDiagram")]
        public TMP.WORK.AramisChetchiki.Model.InfoViewType SelectedSummaryView
        {
            get => ((TMP.WORK.AramisChetchiki.Model.InfoViewType)(this["SelectedSummaryView"]));
            set => this["SelectedSummaryView"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public TMP.WORK.AramisChetchiki.MeterFieldsCollection BaseViewColumns
        {
            get => ((TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["BaseViewColumns"]));
            set => this["BaseViewColumns"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public TMP.WORK.AramisChetchiki.MeterFieldsCollection ShortViewColumns
        {
            get => ((TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["ShortViewColumns"]));
            set => this["ShortViewColumns"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public TMP.WORK.AramisChetchiki.MeterFieldsCollection DetailedViewColumns
        {
            get => ((TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["DetailedViewColumns"]));
            set => this["DetailedViewColumns"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public TMP.WORK.AramisChetchiki.MeterFieldsCollection ОплатаViewColumns
        {
            get => ((TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["ОплатаViewColumns"]));
            set => this["ОплатаViewColumns"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public TMP.WORK.AramisChetchiki.MeterFieldsCollection ПривязкаViewColumns
        {
            get => ((TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["ПривязкаViewColumns"]));
            set => this["ПривязкаViewColumns"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("BaseView")]
        public TMP.WORK.AramisChetchiki.Model.TableViewKinds SelectedTableViewKind
        {
            get => ((TMP.WORK.AramisChetchiki.Model.TableViewKinds)(this["SelectedTableViewKind"]));
            set => this["SelectedTableViewKind"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(System.Configuration.SettingsSerializeAs.Xml)]
        public TMPApplication.VisualTheme Theme
        {
            get => ((TMPApplication.VisualTheme)(this["Theme"]));
            set => this["Theme"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("")]
        public string LastUsedDataFileName
        {
            get => ((string)(this["LastUsedDataFileName"]));
            set => this["LastUsedDataFileName"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsCellSelectionEnabled
        {
            get => ((bool)(this["IsCellSelectionEnabled"]));
            set => this["IsCellSelectionEnabled"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.Configuration.DefaultSettingValueAttribute("16")]
        public uint NumberOfApartmentsInAnApartmentBuilding
        {
            get => ((uint)(this["NumberOfApartmentsInAnApartmentBuilding"]));
            set => this["NumberOfApartmentsInAnApartmentBuilding"] = value;
        }

        [System.Configuration.UserScopedSettingAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        public TMP.Shared.SerializableDictionary<string, string> ViewModelsTableColumns
        {
            get => ((TMP.Shared.SerializableDictionary<string, string>)(this["ViewModelsTableColumns"]));
            set => this["ViewModelsTableColumns"] = value;
        }
    }
}