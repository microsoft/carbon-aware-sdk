﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CarbonAware.CLI {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class CommandOptions {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal CommandOptions() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CarbonAware.CLI.CommandOptions", typeof(CommandOptions).Assembly);
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
        ///   Looks up a localized string similar to End time of the data query.
        /// </summary>
        internal static string endTimeDescription {
            get {
                return ResourceManager.GetString("endTimeDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to --endTime.
        /// </summary>
        internal static string endTimeName {
            get {
                return ResourceManager.GetString("endTimeName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to locations.
        /// </summary>
        internal static string locations {
            get {
                return ResourceManager.GetString("locations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A list of locations.
        /// </summary>
        internal static string locationsDescription {
            get {
                return ResourceManager.GetString("locationsDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Start time of the data query.
        /// </summary>
        internal static string startTimeDescription {
            get {
                return ResourceManager.GetString("startTimeDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to --startTime.
        /// </summary>
        internal static string startTimeName {
            get {
                return ResourceManager.GetString("startTimeName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to windowSize.
        /// </summary>
        internal static string windowSize {
            get {
                return ResourceManager.GetString("windowSize", resourceCulture);
            }
        }
    }
}
