﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1434
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WixToolset.Tools {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class MeltStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal MeltStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("WixToolset.Tools.MeltStrings", typeof(MeltStrings).Assembly);
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
        ///   Looks up a localized string similar to  usage: melt.exe [-?] [-nologo] database.msm source.wxs [@responseFile]
        ///
        ///   -ext &lt;extension&gt;  extension assembly or &quot;class, assembly&quot;
        ///   -id        friendly identifier to use instead of module id
        ///   -nologo    skip printing melt logo information
        ///   -notidy    do not delete temporary files (useful for debugging)
        ///   -o[ut]     specify output file (default: write to current directory)
        ///   -sw[N]     suppress all warnings or a specific message ID
        ///              (example: -sw1059 -sw1067)
        ///   -swall     su [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string HelpMessage {
            get {
                return ResourceManager.GetString("HelpMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Temporary directory located at &apos;{0}&apos;..
        /// </summary>
        internal static string INF_TempDirLocatedAt {
            get {
                return ResourceManager.GetString("INF_TempDirLocatedAt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning, failed to delete temporary directory: {0}.
        /// </summary>
        internal static string WAR_FailedToDeleteTempDir {
            get {
                return ResourceManager.GetString("WAR_FailedToDeleteTempDir", resourceCulture);
            }
        }
    }
}
