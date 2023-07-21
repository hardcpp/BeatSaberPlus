using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text;

namespace ChatPlexMod_ChatIntegrations
{
    /// <summary>
    /// ChatIntegrations database logic
    /// </summary>
    public partial class ChatIntegrations
    {
        /// <summary>
        /// Load the database
        /// </summary>
        /// <returns></returns>
        private bool LoadDatabase()
        {
            /// Create folder if needed
            if (!Directory.Exists(CIConfig.Instance.DataLocation))
                Directory.CreateDirectory(CIConfig.Instance.DataLocation);

            if (!File.Exists(s_DATABASE_FILE))
                return false;

            if (CIConfig.Instance.LastBackup != CP_SDK.ChatPlexSDK.ProductVersion)
            {
                try
                {
                    var l_DatabaseFolder    = Path.GetDirectoryName(s_DATABASE_FILE);
                    var l_BackupFolder      = Path.Combine(l_DatabaseFolder, "Backup");

                    if (!Directory.Exists(l_BackupFolder))
                        Directory.CreateDirectory(l_BackupFolder);

                    File.Copy(s_DATABASE_FILE, Path.Combine(l_BackupFolder, $"backup_pre_{CP_SDK.ChatPlexSDK.ProductVersion}.json"));
                }
                catch (System.Exception)
                {

                }

                CIConfig.Instance.LastBackup = CP_SDK.ChatPlexSDK.ProductVersion;
                CIConfig.Instance.Save();
            }

            string l_JSONContent = File.ReadAllText(s_DATABASE_FILE, Encoding.Unicode);
            try
            {
                var l_JSON = JObject.Parse(l_JSONContent);

                if (!l_JSON.ContainsKey("Events") || l_JSON["Events"].Type != JTokenType.Array)
                    return false;

                var l_Events = l_JSON["Events"] as JArray;
                foreach (JObject l_Event in l_Events)
                    AddEventFromSerialized(l_Event, false, false, out var _);

                return true;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error("[ChatPlexMod_ChatIntegrations][ChatIntegrations.LoadDatabase] Failed");
                Logger.Instance?.Error(l_Exception);

                try { File.Move(s_DATABASE_FILE, s_DATABASE_FILE + CP_SDK.Misc.Time.UnixTimeNow()); }
                catch { }
            }

            return false;
        }
        /// <summary>
        /// Save database
        /// </summary>
        internal void SaveDatabase()
        {
            /// Create folder if needed
            if (!Directory.Exists(CIConfig.Instance.DataLocation))
                Directory.CreateDirectory(CIConfig.Instance.DataLocation);

            string l_OldJSONContent = null;
            try
            {
                /// Backup in case of error
                if (File.Exists(s_DATABASE_FILE))
                    l_OldJSONContent = File.ReadAllText(s_DATABASE_FILE, Encoding.Unicode);

                /// Serialize everything
                var l_Object = new JObject()
                {
                    ["Events"] = JArray.FromObject(m_Events.Select(x => x.Serialize()).ToArray())
                };

                /// Save new database
                File.WriteAllText(s_DATABASE_FILE, l_Object.ToString(Newtonsoft.Json.Formatting.Indented), Encoding.Unicode);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error("[ChatPlexMod_ChatIntegrations][ChatIntegrations.SaveDatabase] Failed");
                Logger.Instance?.Error(l_Exception);

                if (!string.IsNullOrEmpty(l_OldJSONContent))
                {
                    try { File.WriteAllText(s_DATABASE_FILE + CP_SDK.Misc.Time.UnixTimeNow(), l_OldJSONContent, Encoding.Unicode); }
                    catch { }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get patched type name
        /// </summary>
        /// <param name="p_Input">Input value</param>
        /// <returns></returns>
        internal static string GetPatchedTypeName(string p_Input)
        {
            var l_Index = p_Input.LastIndexOf('.');
            if (l_Index == -1)
                return p_Input;

            return p_Input.Substring(l_Index + 1);
        }
    }
}
