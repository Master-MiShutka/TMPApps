﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TMP.WORK.AramisChetchiki.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.1.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("14")]
        public double FontSize {
            get {
                return ((double)(this["FontSize"]));
            }
            set {
                this["FontSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("d:\\aramis\\Disks\\OSHM\\aramis")]
        public string AramisDBPath {
            get {
                return ((string)(this["AramisDBPath"]));
            }
            set {
                this["AramisDBPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DataFilesStorePath {
            get {
                return ((string)(this["DataFilesStorePath"]));
            }
            set {
                this["DataFilesStorePath"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::TMP.WORK.AramisChetchiki.ChangesOfMetersFieldsCollection ChangesOfMetersFields {
            get {
                return ((global::TMP.WORK.AramisChetchiki.ChangesOfMetersFieldsCollection)(this["ChangesOfMetersFields"]));
            }
            set {
                this["ChangesOfMetersFields"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public SummaryInfoFieldsCollection SummaryInfoFields
        {
            get
            {
                return (SummaryInfoFieldsCollection)this["SummaryInfoFields"];
            }

            set
            {
                this["SummaryInfoFields"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public System.Collections.Generic.Dictionary<ViewModel.IViewModel, System.Collections.Generic.List<DataGridWpf.DataGridWpfColumnViewModel>> ViewModelsTableColumns
        {
            get
            {
                return (System.Collections.Generic.Dictionary<ViewModel.IViewModel, System.Collections.Generic.List<DataGridWpf.DataGridWpfColumnViewModel>>)this["ViewModelsTableColumns"];
            }

            set
            {
                this["ViewModelsTableColumns"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ViewAsDiagram")]
        public global::TMP.WORK.AramisChetchiki.Model.InfoViewType SelectedSummaryView {
            get {
                return ((global::TMP.WORK.AramisChetchiki.Model.InfoViewType)(this["SelectedSummaryView"]));
            }
            set {
                this["SelectedSummaryView"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::TMP.WORK.AramisChetchiki.MeterFieldsCollection BaseViewColumns {
            get {
                return ((global::TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["BaseViewColumns"]));
            }
            set {
                this["BaseViewColumns"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::TMP.WORK.AramisChetchiki.MeterFieldsCollection ShortViewColumns {
            get {
                return ((global::TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["ShortViewColumns"]));
            }
            set {
                this["ShortViewColumns"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::TMP.WORK.AramisChetchiki.MeterFieldsCollection DetailedViewColumns {
            get {
                return ((global::TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["DetailedViewColumns"]));
            }
            set {
                this["DetailedViewColumns"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::TMP.WORK.AramisChetchiki.MeterFieldsCollection ОплатаViewColumns {
            get {
                return ((global::TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["ОплатаViewColumns"]));
            }
            set {
                this["ОплатаViewColumns"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::TMP.WORK.AramisChetchiki.MeterFieldsCollection ПривязкаViewColumns {
            get {
                return ((global::TMP.WORK.AramisChetchiki.MeterFieldsCollection)(this["ПривязкаViewColumns"]));
            }
            set {
                this["ПривязкаViewColumns"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("BaseView")]
        public global::TMP.WORK.AramisChetchiki.Model.TableViewKinds SelectedTableViewKind {
            get {
                return ((global::TMP.WORK.AramisChetchiki.Model.TableViewKinds)(this["SelectedTableViewKind"]));
            }
            set {
                this["SelectedTableViewKind"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::TMPApplication.VisualTheme Theme {
            get {
                return ((global::TMPApplication.VisualTheme)(this["Theme"]));
            }
            set {
                this["Theme"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastUsedDataFileName {
            get {
                return ((string)(this["LastUsedDataFileName"]));
            }
            set {
                this["LastUsedDataFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsCellSelectionEnabled {
            get {
                return ((bool)(this["IsCellSelectionEnabled"]));
            }
            set {
                this["IsCellSelectionEnabled"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("16")]
        public uint NumberOfApartmentsInAnApartmentBuilding {
            get {
                return ((uint)(this["NumberOfApartmentsInAnApartmentBuilding"]));
            }
            set {
                this["NumberOfApartmentsInAnApartmentBuilding"] = value;
            }
        }
    }
}