using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BeatSaberPlus.Modules.ChatIntegrations
{
    /// <summary>
    /// ChatIntegrations database logic
    /// </summary>
    internal partial class ChatIntegrations
    {
        /// <summary>
        /// Load the database
        /// </summary>
        /// <returns></returns>
        private bool LoadDatabase()
        {
            /// Create folder if needed
            if (!Directory.Exists(s_DATABASE_FOLDER))
                Directory.CreateDirectory(s_DATABASE_FOLDER);

            if (!File.Exists(s_DATABASE_PATH))
                return false;

            string l_JSONContent = File.ReadAllText(s_DATABASE_PATH, Encoding.Unicode);
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

                try { File.Move(s_DATABASE_PATH, s_DATABASE_PATH + SDK.Misc.Time.UnixTimeNow()); }
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
            if (!Directory.Exists(s_DATABASE_FOLDER))
                Directory.CreateDirectory(s_DATABASE_FOLDER);

            string l_OldJSONContent = null;
            try
            {
                /// Backup in case of error
                if (File.Exists(s_DATABASE_PATH))
                    l_OldJSONContent = File.ReadAllText(s_DATABASE_PATH, Encoding.Unicode);

                /// Serialize everything
                var l_Object = new JObject()
                {
                    ["Events"] = JArray.FromObject(m_Events.Select(x => x.Serialize()).ToArray())
                };

                /// Save new database
                File.WriteAllText(s_DATABASE_PATH, l_Object.ToString(Newtonsoft.Json.Formatting.Indented), Encoding.Unicode);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error("[Modules.ChatIntegrations][ChatIntegrations.SaveDatabase] Failed");
                Logger.Instance?.Error(l_Exception);

                if (!string.IsNullOrEmpty(l_OldJSONContent))
                {
                    try { File.WriteAllText(s_DATABASE_PATH + SDK.Misc.Time.UnixTimeNow(), l_OldJSONContent, Encoding.Unicode); }
                    catch { }
                }
            }
        }
    }
}
