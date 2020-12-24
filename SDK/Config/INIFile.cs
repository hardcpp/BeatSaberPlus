using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeatSaberPlus.SDK.Config
{
    /// <summary>
    /// Ini file reader/writter
    /// </summary>
    public class INIFile
    {
        /// <summary>
        /// Values
        /// </summary>
        private SortedDictionary<string, SortedDictionary<string, string>> m_Values = new SortedDictionary<string, SortedDictionary<string, string>>();
        /// <summary>
        /// Config file filepath
        /// </summary>
        private string m_IniFilePath;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="p_FilePath">Full path to INI file.</param>
        public INIFile(string p_FilePath)
        {
            TextReader l_TextReader = null;

            SortedDictionary<string, string> l_CurrentRoot = null;
            m_IniFilePath = p_FilePath;

            if (File.Exists(p_FilePath))
            {
                try
                {
                    l_TextReader = new StreamReader(p_FilePath, Encoding.Unicode);
                    var l_StrLine = l_TextReader.ReadLine();

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
                                var l_KeyPair = l_StrLine.Split(new char[] { '=' }, 2);
                                var l_Key = (l_KeyPair.Length > 0) ? l_KeyPair[0].Trim() : "";
                                var l_Val = (l_KeyPair.Length > 1) ? l_KeyPair[1].Trim() : null;

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
                    Logger.Instance?.Error("[SDK.Config][IniFile] Failed to read configuration");
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
            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                try
                {
                    SaveSettings();
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance?.Error("[SDK.Config][IniFile.SetSetting] failed to save configuration");
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
}
