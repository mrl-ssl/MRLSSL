﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MRL.SSL.GameDefinitions.General_Settings {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    public sealed partial class AISettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static AISettings defaultInstance = ((AISettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new AISettings())));
        
        public static AISettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1011")]
        public int AiPort {
            get {
                return ((int)(this["AiPort"]));
            }
            set {
                this["AiPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1017")]
        public int VisPort {
            get {
                return ((int)(this["VisPort"]));
            }
            set {
                this["VisPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("224.5.23.1")]
        public string RefIP {
            get {
                return ((string)(this["RefIP"]));
            }
            set {
                this["RefIP"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10003")]
        public int RefPort {
            get {
                return ((int)(this["RefPort"]));
            }
            set {
                this["RefPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("224.5.23.2")]
        public string SSLVisionIP {
            get {
                return ((string)(this["SSLVisionIP"]));
            }
            set {
                this["SSLVisionIP"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10006")]
        public int SSLVisionPort {
            get {
                return ((int)(this["SSLVisionPort"]));
            }
            set {
                this["SSLVisionPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10016")]
        public int AnalayzerPort {
            get {
                return ((int)(this["AnalayzerPort"]));
            }
            set {
                this["AnalayzerPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10020")]
        public int SimulatorSendPort {
            get {
                return ((int)(this["SimulatorSendPort"]));
            }
            set {
                this["SimulatorSendPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20011")]
        public int SimulatorRecievePort {
            get {
                return ((int)(this["SimulatorRecievePort"]));
            }
            set {
                this["SimulatorRecievePort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int RecievePort {
            get {
                return ((int)(this["RecievePort"]));
            }
            set {
                this["RecievePort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MRL-SSL3-PC")]
        public string AiName {
            get {
                return ((string)(this["AiName"]));
            }
            set {
                this["AiName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MRL-SSL3-PC")]
        public string VisName {
            get {
                return ((string)(this["VisName"]));
            }
            set {
                this["VisName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MRL-SSL3-PC")]
        public string SimulatorName {
            get {
                return ((string)(this["SimulatorName"]));
            }
            set {
                this["SimulatorName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("MRL-SSL3-PC")]
        public string AnalayzerName {
            get {
                return ((string)(this["AnalayzerName"]));
            }
            set {
                this["AnalayzerName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6")]
        public int SerialPort {
            get {
                return ((int)(this["SerialPort"]));
            }
            set {
                this["SerialPort"] = value;
            }
        }
    }
}
