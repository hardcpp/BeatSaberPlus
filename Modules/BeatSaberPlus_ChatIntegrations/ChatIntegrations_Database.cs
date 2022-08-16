using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BeatSaberPlus_ChatIntegrations
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
            /// Migrate old database if any
            try
            {
                if (File.Exists(s_OLD_DATABASE_FILE))
                {
                    if (!File.Exists(s_DATABASE_FILE))
                        File.Move(s_OLD_DATABASE_FILE, s_DATABASE_FILE);
                    else
                        File.Move(s_OLD_DATABASE_FILE, s_DATABASE_FILE + CP_SDK.Misc.Time.UnixTimeNow());
                }
            }
            catch
            {

            }

            /// Create folder if needed
            if (!Directory.Exists(CIConfig.Instance.DataLocation))
                Directory.CreateDirectory(CIConfig.Instance.DataLocation);

            if (!File.Exists(s_DATABASE_FILE))
                return false;

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
                Logger.Instance?.Error("[Modules.ChatIntegrations][ChatIntegrations.LoadDatabase] Failed");
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
                Logger.Instance?.Error("[Modules.ChatIntegrations][ChatIntegrations.SaveDatabase] Failed");
                Logger.Instance?.Error(l_Exception);

                if (!string.IsNullOrEmpty(l_OldJSONContent))
                {
                    try { File.WriteAllText(s_DATABASE_FILE + CP_SDK.Misc.Time.UnixTimeNow(), l_OldJSONContent, Encoding.Unicode); }
                    catch { }
                }
            }
        }
    }
}
