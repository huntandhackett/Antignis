﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Antignis.Server.Properties {
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
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Antignis.Server.Properties.Resources", typeof(Resources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap Antignis {
            get {
                object obj = ResourceManager.GetObject("Antignis", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE TABLE &quot;Host&quot; (
        ///	&quot;Id&quot;	INTEGER NOT NULL UNIQUE,
        ///	&quot;IsServerOS&quot;	INTEGER,
        ///	&quot;OperatingSystem&quot;	TEXT,
        ///	&quot;DNSHostname&quot;	TEXT,
        ///	&quot;NetworkMask&quot;	TEXT,
        ///	&quot;IPAddress&quot;	TEXT,
        ///	&quot;WindowsFirewallSettingId&quot;	INTEGER,
        ///	PRIMARY KEY(&quot;Id&quot; AUTOINCREMENT)
        ///);
        ///
        ///CREATE TABLE &quot;FileShare&quot; (
        ///	&quot;Id&quot;	INTEGER NOT NULL UNIQUE,
        ///	&quot;Name&quot;	TEXT,
        ///	&quot;HostId&quot;	INTEGER,
        ///	FOREIGN KEY(&quot;HostId&quot;) REFERENCES &quot;Host&quot;(&quot;Id&quot;),
        ///	PRIMARY KEY(&quot;Id&quot; AUTOINCREMENT)
        ///);
        ///
        ///CREATE TABLE &quot;Port&quot; (
        ///	&quot;Id&quot;	INTEGER NOT NULL UNIQUE,
        ///	&quot;HostId&quot;	INTEGER,
        ///	&quot;Port [rest of string was truncated]&quot;;.
        /// </summary>
        public static string CreateStructure {
            get {
                return ResourceManager.GetString("CreateStructure", resourceCulture);
            }
        }
    }
}