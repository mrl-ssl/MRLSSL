﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MRL.SSL.CommonClasses.MathLibrary;
namespace MRL.SSL.GameDefinitions.General_Settings {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    public sealed partial class TuneVariables : global::System.Configuration.ApplicationSettingsBase
    {

        private static TuneVariables defaultInstance = ((TuneVariables)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new TuneVariables())));

        public static TuneVariables Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, double> Doubles
        {
            get
            {
                return ((global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, double>)(this["Doubles"]));
            }
            set
            {
                this["Doubles"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, bool> Booleans
        {
            get
            {
                return ((global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, bool>)(this["Booleans"]));
            }
            set
            {
                this["Booleans"] = value;
            }
        }


        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, Position2D> Position2Ds
        {
            get
            {
                return ((global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, Position2D>)(this["Position2Ds"]));
            }
            set
            {
                this["Position2Ds"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, int> Integers
        {
            get
            {
                return ((global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<string, int>)(this["Integers"]));
            }
            set
            {
                this["Integers"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<int, double> AngleError
        {
            get
            {
                return ((global::MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<int, double>)(this["AngleError"]));
            }
            set
            {
                this["AngleError"] = value;
            }
        }
    }
}
