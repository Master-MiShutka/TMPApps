﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TMP.Work.Emcos {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TMP.Work.Emcos.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ВСЕ.
        /// </summary>
        internal static string AllReses {
            get {
                return ResourceManager.GetString("AllReses", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to период не задан.
        /// </summary>
        internal static string DatePeriodNotDefined {
            get {
                return ResourceManager.GetString("DatePeriodNotDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to пустая сессия.
        /// </summary>
        internal static string EmptySession {
            get {
                return ResourceManager.GetString("EmptySession", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Данные за период: {0}.
        /// </summary>
        internal static string FormatDataByPeriod {
            get {
                return ResourceManager.GetString("FormatDataByPeriod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Баланс подстанций.
        /// </summary>
        internal static string MainTitle {
            get {
                return ResourceManager.GetString("MainTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Нет данных для отображения.
        /// </summary>
        internal static string NoDataForDisplay {
            get {
                return ResourceManager.GetString("NoDataForDisplay", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Выберите период и получите данные.
        /// </summary>
        internal static string SelectPeriodAndGetData {
            get {
                return ResourceManager.GetString("SelectPeriodAndGetData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Сессия не загружена. Выберите или создайте новую..
        /// </summary>
        internal static string SessionNotLoadedSelectOrCreateNew {
            get {
                return ResourceManager.GetString("SessionNotLoadedSelectOrCreateNew", resourceCulture);
            }
        }
    }
}
