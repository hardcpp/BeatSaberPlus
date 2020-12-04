using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace BeatSaberPlus.Utils
{
    /// <summary>
    /// Config file
    /// </summary>
    class Config
    {
        /// <summary>
        /// Boolean saving mode
        /// </summary>
        public enum BoolSavingMode
        {
            TrueFalse,
            OneZero,
            YesNo,
            EnabledDisabled,
            OnOff,
        };
        /// <summary>
        /// Yes alternatives for booleans
        /// </summary>
        private static List<string> s_YesAlts = new List<string>() { "True", "1", "Yes", "Enabled", "On" };
        /// <summary>
        /// No alternatives for booleans
        /// </summary>
        private static List<string> s_NoAlts = new List<string>() { "False", "0", "No", "Disabled", "Off" };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ini file helper
        /// </summary>
        internal class IniFile
        {
            /// <summary>
            /// Values
            /// </summary>
            private SortedDictionary<string, SortedDictionary<string, string>> m_Values = new SortedDictionary<string, SortedDictionary<string, string>>();
            /// <summary>
            /// Config file filepath
            /// </summary>
            private String m_IniFilePath;

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Opens the INI file at the given path and enumerates the values in the IniParser.
            /// </summary>
            /// <param name="p_FilePath">Full path to INI file.</param>
            public IniFile(string p_FilePath)
            {
                TextReader l_TextReader = null;

                SortedDictionary<string, string> l_CurrentRoot  = null;
                m_IniFilePath = p_FilePath;

                if (File.Exists(p_FilePath))
                {
                    try
                    {
                        l_TextReader    = new StreamReader(p_FilePath, Encoding.Unicode);
                        var l_StrLine   = l_TextReader.ReadLine();

                        while (l_StrLine != null)
                        {
                            l_StrLine = l_StrLine.Trim();
                            if (l_StrLine != "" && !l_StrLine.StartsWith("#"))
                            {
                                if (l_StrLine.StartsWith("[") && l_StrLine.EndsWith("]"))
                                {
                                    var l_SectionName = l_StrLine.Substring(1, l_StrLine.Length - 2).Trim();
                                    if (!m_Values.ContainsKey(l_SectionName))
                                        m_Values.Add(l_SectionName, new SortedDictionary<string, string>());

                                    l_CurrentRoot = m_Values[l_SectionName];
                                }
                                else if (l_CurrentRoot != null)
                                {
                                    var l_KeyPair   = l_StrLine.Split(new char[] { '=' }, 2);
                                    var l_Key       = (l_KeyPair.Length > 0) ? l_KeyPair[0].Trim() : "";
                                    var l_Val       = (l_KeyPair.Length > 1) ? l_KeyPair[1].Trim() : null;

                                    if (!l_CurrentRoot.ContainsKey(l_Key))
                                        l_CurrentRoot.Add(l_Key, l_Val);
                                    else
                                        l_CurrentRoot[l_Key] = l_Val;
                                }
                            }

                            l_StrLine = l_TextReader.ReadLine();
                        }
                    }
                    catch (Exception l_Exception)
                    {
                        Logger.Instance?.Error("IniFile failed to read configuration");
                        Logger.Instance?.Error(l_Exception);
                        throw l_Exception;
                    }
                    finally
                    {
                        if (l_TextReader != null)
                            l_TextReader.Close();
                    }
                }
            }

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Returns the value for the given section, key pair.
            /// </summary>
            /// <param name="p_Section">Section name.</param>
            /// <param name="p_Key">Key name.</param>
            public string GetSetting(string p_Section, string p_Key)
            {
                lock (m_Values)
                {
                    SortedDictionary<string, string> l_Section = null;
                    if (!m_Values.TryGetValue(p_Section, out l_Section))
                        return null;

                    if (l_Section.ContainsKey(p_Key))
                        return l_Section[p_Key];
                }

                return null;
            }
            /// <summary>
            /// Adds or replaces a setting to the table to be saved.
            /// </summary>
            /// <param name="p_Section">Section to add under.</param>
            /// <param name="p_Key">Key name to add.</param>
            /// <param name="p_Value">Value of key.</param>
            public void SetSetting(string p_Section, string p_Key, string p_Value)
            {
                lock (m_Values)
                {
                    SortedDictionary<string, string> l_Section = null;
                    if (!m_Values.TryGetValue(p_Section, out l_Section))
                    {
                        l_Section = new SortedDictionary<string, string>();
                        m_Values.Add(p_Section, l_Section);
                    }

                    if (l_Section.ContainsKey(p_Key))
                        l_Section[p_Key] = p_Value;
                    else
                        l_Section.Add(p_Key, p_Value);
                }

                /// Do save on main thread
                HMMainThreadDispatcher.instance.Enqueue(() =>
                {
                    try
                    {
                        SaveSettings();
                    }
                    catch (System.Exception l_Exception)
                    {
                        Logger.Instance?.Error("IniFile failed to save configuration");
                        Logger.Instance?.Error(l_Exception);
                    }
                });
            }

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Save settings to new file.
            /// </summary>
            public void SaveSettings()
            {
                lock (m_Values)
                {
                    string l_StrToSave = "";
                    foreach (var l_Section in m_Values)
                    {
                        l_StrToSave += ("[" + l_Section.Key + "]\r\n");
                        foreach (var l_Value in l_Section.Value)
                        {
                            if (l_Value.Value == null)
                                continue;

                            l_StrToSave += (l_Value.Key + " = " + l_Value.Value + "\r\n");
                        }
                        l_StrToSave += "\r\n";
                    }

                    try
                    {
                        TextWriter l_TextWritter = new StreamWriter(m_IniFilePath, false, Encoding.Unicode);
                        l_TextWritter.Write(l_StrToSave);
                        l_TextWritter.Close();
                    }
                    catch (Exception l_Exception)
                    {
                        throw l_Exception;
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ini file instance
        /// </summary>
        private IniFile m_Instance;
        /// <summary>
        /// Cache
        /// </summary>
        private Dictionary<(string, string), string>    m_StringCache   = new Dictionary<(string, string), string>();
        private Dictionary<(string, string), int>       m_IntCache      = new Dictionary<(string, string), int>();
        private Dictionary<(string, string), float>     m_FloatCache    = new Dictionary<(string, string), float>();
        private Dictionary<(string, string), bool>      m_BoolCache     = new Dictionary<(string, string), bool>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_ConfigName"></param>
        public Config(string p_ConfigName)
        {
            m_Instance = new IniFile(Path.Combine(Environment.CurrentDirectory, $"UserData/{p_ConfigName}.ini"));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a string from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public string GetString(string p_Section, string p_Name, string p_DefaultValue = "", bool p_AutoSave = false)
        {
            if (m_StringCache.TryGetValue((p_Section, p_Name), out var l_Cache))
                return l_Cache;

            string l_Value = m_Instance.GetSetting(p_Section, p_Name);
            if (l_Value != null)
                return l_Value;
            else if (p_AutoSave)
                SetString(p_Section, p_Name, p_DefaultValue);

            return p_DefaultValue;
        }
        /// <summary>
        /// Sets a string in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Value">Value that should be written.</param>
        public void SetString(string p_Section, string p_Name, string p_Value)
        {
            m_Instance.SetSetting(p_Section, p_Name, p_Value);

            if (m_StringCache.ContainsKey((p_Section, p_Name)))
                m_StringCache[(p_Section, p_Name)] = p_Value;
            else
                m_StringCache.Add((p_Section, p_Name), p_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets an int from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public int GetInt(string p_Section, string p_Name, int p_DefaultValue = 0, bool p_AutoSave = false)
        {
            if (m_IntCache.TryGetValue((p_Section, p_Name), out var l_Cache))
                return l_Cache;

            var l_RawValue = m_Instance.GetSetting(p_Section, p_Name);
            if (l_RawValue != null && int.TryParse(l_RawValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int l_Value))
                return l_Value;
            else if (p_AutoSave)
                SetInt(p_Section, p_Name, p_DefaultValue);

            return p_DefaultValue;
        }
        /// <summary>
        /// Sets an int in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Value">Value that should be written.</param>
        public void SetInt(string p_Section, string p_Name, int p_Value)
        {
            m_Instance.SetSetting(p_Section, p_Name, p_Value.ToString(CultureInfo.InvariantCulture));

            if (m_IntCache.ContainsKey((p_Section, p_Name)))
                m_IntCache[(p_Section, p_Name)] = p_Value;
            else
                m_IntCache.Add((p_Section, p_Name), p_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a float from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public float GetFloat(string p_Section, string p_Name, float p_DefaultValue = 0f, bool p_AutoSave = false)
        {
            if (m_FloatCache.TryGetValue((p_Section, p_Name), out var l_Cache))
                return l_Cache;

            var l_RawValue = m_Instance.GetSetting(p_Section, p_Name);
            if (l_RawValue != null && float.TryParse(l_RawValue, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out float l_Value))
                return l_Value;
            else if (p_AutoSave)
                SetFloat(p_Section, p_Name, p_DefaultValue);

            return p_DefaultValue;
        }
        /// <summary>
        /// Sets a float in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Value">Value that should be written.</param>
        public void SetFloat(string p_Section, string p_Name, float p_Value)
        {
            m_Instance.SetSetting(p_Section, p_Name, p_Value.ToString(CultureInfo.InvariantCulture));

            if (m_FloatCache.ContainsKey((p_Section, p_Name)))
                m_FloatCache[(p_Section, p_Name)] = p_Value;
            else
                m_FloatCache.Add((p_Section, p_Name), p_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a bool from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public bool GetBool(string p_Section, string p_Name, bool p_DefaultValue = false, bool p_AutoSave = false)
        {
            return GetBool(p_Section, p_Name, BoolSavingMode.TrueFalse, p_DefaultValue, p_AutoSave);
        }
        /// <summary>
        /// Gets a bool from the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Mode">Yes/No alternative we should look for.</param>
        /// <param name="p_DefaultValue">Value that should be used when no value is found.</param>
        /// <param name="p_AutoSave">Whether or not the default value should be written if no value is found.</param>
        /// <returns></returns>
        public bool GetBool(string p_Section, string p_Name, BoolSavingMode p_Mode, bool p_DefaultValue = false, bool p_AutoSave = false)
        {
            if (m_BoolCache.TryGetValue((p_Section, p_Name), out var l_Cache))
                return l_Cache;

            string l_StringVal = GetString(p_Section, p_Name);
            if (l_StringVal != null)
            {
                try
                {
                    int l_YesIndex  = s_YesAlts.IndexOf(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(l_StringVal));
                    int l_NoIndex   = s_NoAlts.IndexOf(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(l_StringVal));

                    if (l_YesIndex != -1 && l_YesIndex == (int)p_Mode)
                        return true;
                    else if (l_NoIndex != -1 && l_NoIndex == (int)p_Mode)
                        return false;
                    else if (p_AutoSave)
                        SetBool(p_Section, p_Name, p_DefaultValue);
                }
                catch
                {
                    SetBool(p_Section, p_Name, p_DefaultValue);
                }
            }
            else
                SetBool(p_Section, p_Name, p_DefaultValue);

            return p_DefaultValue;
        }
        /// <summary>
        /// Sets a bool in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Value">Value that should be written.</param>
        public void SetBool(string p_Section, string p_Name, bool p_Value)
        {
            SetBool(p_Section, p_Name, BoolSavingMode.TrueFalse, p_Value);
        }
        /// <summary>
        /// Sets a bool in the ini.
        /// </summary>
        /// <param name="p_Section">Section of the key.</param>
        /// <param name="p_Name">Name of the key.</param>
        /// <param name="p_Mode">What common yes/no alternative should we use.</param>
        /// <param name="p_Value">Value that should be written.</param>
        public void SetBool(string p_Section, string p_Name, BoolSavingMode p_Mode, bool p_Value)
        {
            m_Instance.SetSetting(p_Section, p_Name, p_Value ? s_YesAlts[(int)p_Mode] : s_NoAlts[(int)p_Mode]);

            if (m_BoolCache.ContainsKey((p_Section, p_Name)))
                m_BoolCache[(p_Section, p_Name)] = p_Value;
            else
                m_BoolCache.Add((p_Section, p_Name), p_Value);
        }
    }
}
