﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TMP.Work.Emcos.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.6.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("DataModel.json.gz")]
        public string BalancePointsFileName {
            get {
                return ((string)(this["BalancePointsFileName"]));
            }
            set {
                this["BalancePointsFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("session")]
        public string SessionFileName {
            get {
                return ((string)(this["SessionFileName"]));
            }
            set {
                this["SessionFileName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double MainWindowTop {
            get {
                return ((double)(this["MainWindowTop"]));
            }
            set {
                this["MainWindowTop"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public double MainWindowLeft {
            get {
                return ((double)(this["MainWindowLeft"]));
            }
            set {
                this["MainWindowLeft"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("600")]
        public double MainWindowHeight {
            get {
                return ((double)(this["MainWindowHeight"]));
            }
            set {
                this["MainWindowHeight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1500")]
        public double MainWindowWidth {
            get {
                return ((double)(this["MainWindowWidth"]));
            }
            set {
                this["MainWindowWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Normal")]
        public global::System.Windows.WindowState MainWindowState {
            get {
                return ((global::System.Windows.WindowState)(this["MainWindowState"]));
            }
            set {
                this["MainWindowState"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("d:\\Документы ОС\\«БАЛАНСЫ, ПЕРЕТОКИ, ПОТЕРИ»\\ОДС\\Расчёт баланса 6-10кВ по ПС 35-11" +
            "0кВ")]
        public string ReportBalancePSFolder {
            get {
                return ((string)(this["ReportBalancePSFolder"]));
            }
            set {
                this["ReportBalancePSFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Расчёт баланса 6-10 за {0:MM - yyyy}")]
        public string ReportBalancePSFileNameTemplate {
            get {
                return ((string)(this["ReportBalancePSFileNameTemplate"]));
            }
            set {
                this["ReportBalancePSFileNameTemplate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("143")]
        public int RootEmcosPointCode {
            get {
                return ((int)(this["RootEmcosPointCode"]));
            }
            set {
                this["RootEmcosPointCode"] = value;
            }
        }
    }
}