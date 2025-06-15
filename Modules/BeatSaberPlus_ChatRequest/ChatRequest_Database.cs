using Newtonsoft.Json.Linq;
using System.Text;

namespace BeatSaberPlus_ChatRequest
{
    /// <summary>
    /// Chat request database handler
    /// </summary>
    public partial class ChatRequest
    {
        /// <summary>
        /// DB File path
        /// </summary>
        private string m_DBFilePath = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus\\ChatRequest\\Database.json";
        /// <summary>
        /// Simple queue File path
        /// </summary>
        private string m_SimpleQueueFilePath = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus\\ChatRequest\\SimpleQueue.txt";
        /// <summary>
        /// Simple queue status File path
        /// </summary>
        private string m_SimpleQueueStatusFilePath = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus\\ChatRequest\\SimpleQueueStatus.txt";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load database
        /// </summary>
        private void LoadDatabase()
        {
            try
            {
                string l_FilePath = m_DBFilePath;
                if (!System.IO.File.Exists(l_FilePath))
                {
                    Logger.Instance.Error("File not found " + m_DBFilePath);
                    return;
                }

                var l_JSON = JObject.Parse(System.IO.File.ReadAllText(l_FilePath, UTF8Encoding.UTF8));
                if (l_JSON["queue"] != null && l_JSON["queue"].Type == JTokenType.Array)
                {
                    foreach (JObject l_Current in (JArray)l_JSON["queue"])
                    {
                        var l_Entry = Models.SongEntry.Deserialize(l_Current);
                        if (l_Entry == null)
                            continue;

                        SongQueue.Add(l_Entry);

                        /// Start populate
                        if (l_Entry.BeatSaver_Map.Partial)
                            l_Entry.BeatSaver_Map.Populate((x) => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                if (l_JSON["history"] != null && l_JSON["history"].Type == JTokenType.Array)
                {
                    foreach (JObject l_Current in (JArray)l_JSON["history"])
                    {
                        var l_Entry = Models.SongEntry.Deserialize(l_Current);
                        if (l_Entry == null)
                            continue;

                        SongHistory.Add(l_Entry);

                        /// Start populate
                        if (l_Entry.BeatSaver_Map.Partial)
                            l_Entry.BeatSaver_Map.Populate((x) => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                if (l_JSON["allowlist"] != null && l_JSON["allowlist"].Type == JTokenType.Array)
                {
                    foreach (var l_CurrentRaw in (JArray)l_JSON["allowlist"])
                    {
                        var l_Current = null as JObject;
                        if (l_CurrentRaw.Type == JTokenType.String)
                        {
                            l_Current = new JObject()
                            {
                                ["key"] = l_CurrentRaw.Value<string>() ?? "",
                                ["rqn"] = "$BS+Backport"
                            };
                        }
                        else
                            l_Current = (JObject)l_CurrentRaw;

                        var l_Entry = Models.SongEntry.Deserialize(l_Current);
                        if (l_Entry == null)
                            continue;

                        SongAllowlist.Add(l_Entry);

                        /// Start populate
                        if (l_Entry.BeatSaver_Map.Partial)
                            l_Entry.BeatSaver_Map.Populate((x) => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                /** LEGACY blacklist was renamed blocklist */
                if (l_JSON["blacklist"] != null && l_JSON["blacklist"].Type == JTokenType.Array)
                {
                    foreach (JObject l_Current in (JArray)l_JSON["blacklist"])
                    {
                        var l_Entry = Models.SongEntry.Deserialize(l_Current);
                        if (l_Entry == null)
                            continue;

                        SongBlocklist.Add(l_Entry);

                        /// Start populate
                        if (l_Entry.BeatSaver_Map.Partial)
                            l_Entry.BeatSaver_Map.Populate((x) => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                if (l_JSON["blocklist"] != null && l_JSON["blocklist"].Type == JTokenType.Array)
                {
                    foreach (JObject l_Current in (JArray)l_JSON["blocklist"])
                    {
                        var l_Entry = Models.SongEntry.Deserialize(l_Current);
                        if (l_Entry == null)
                            continue;

                        SongBlocklist.Add(l_Entry);

                        /// Start populate
                        if (l_Entry.BeatSaver_Map.Partial)
                            l_Entry.BeatSaver_Map.Populate((x) => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                if (l_JSON["bannedusers"] != null && l_JSON["bannedusers"].Type == JTokenType.Array)
                {
                    foreach (var l_Current in (JArray)l_JSON["bannedusers"])
                        BannedUsers.Add(l_Current.Value<string>() ?? "");
                }
                if (l_JSON["bannedmappers"] != null && l_JSON["bannedmappers"].Type == JTokenType.Array)
                {
                    foreach (var l_Current in (JArray)l_JSON["bannedmappers"])
                        BannedMappers.Add(l_Current.Value<string>() ?? "");
                }
                if (l_JSON["remaps"] != null && l_JSON["remaps"].Type == JTokenType.Array)
                {
                    foreach (JObject l_Current in (JArray)l_JSON["remaps"])
                    {
                        var l_Left  = (l_Current["l"]?.Value<string>() ?? "").ToLower();
                        var l_Right = (l_Current["r"]?.Value<string>() ?? "").ToLower();

                        if (string.IsNullOrEmpty(l_Left) || string.IsNullOrEmpty(l_Right) || Remaps.ContainsKey(l_Left))
                            continue;

                        Remaps.Add(l_Left, l_Right);
                    }
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("LoadDataBase");
                Logger.Instance.Error(p_Exception);
            }
        }
        /// <summary>
        /// Save database
        /// </summary>
        private void SaveDatabase()
        {
            lock (SongQueue) { lock (SongHistory) {  lock (SongAllowlist) { lock (SongBlocklist) { lock (BannedUsers) { lock (Remaps) {
                if (SongQueue.Count == 0 && SongHistory.Count == 0 && SongBlocklist.Count == 0)
                    return;

                try
                {
                    var l_Requests  = new JArray();
                    var l_History   = new JArray();
                    var l_Allowlist = new JArray();
                    var l_Blocklist = new JArray();

                    for (var l_I = 0; l_I < SongQueue.Count;     ++l_I) l_Requests.Add(Models.SongEntry.Serialize(SongQueue[l_I]));
                    for (var l_I = 0; l_I < SongHistory.Count;   ++l_I) l_History.Add(Models.SongEntry.Serialize(SongHistory[l_I]));
                    for (var l_I = 0; l_I < SongAllowlist.Count; ++l_I) l_Allowlist.Add(Models.SongEntry.Serialize(SongAllowlist[l_I]));
                    for (var l_I = 0; l_I < SongBlocklist.Count; ++l_I) l_Blocklist.Add(Models.SongEntry.Serialize(SongBlocklist[l_I]));

                    var l_Remaps = new JArray();
                    foreach (var l_KVP in Remaps)
                    {
                        l_Remaps.Add(new JObject()
                        {
                            ["l"] = l_KVP.Key,
                            ["r"] = l_KVP.Value
                        });
                    }

                    var l_JSON = new JObject
                    {
                        { "queue",          l_Requests                          },
                        { "history",        l_History                           },
                        { "allowlist",      l_Allowlist                         },
                        { "blocklist",      l_Blocklist                         },
                        { "bannedusers",    new JArray(BannedUsers.ToArray())   },
                        { "bannedmappers",  new JArray(BannedMappers.ToArray()) },
                        { "remaps",         l_Remaps                            },
                    };

                    string l_ResultJSON = l_JSON.ToString();
                    System.IO.File.WriteAllText(m_DBFilePath, l_ResultJSON, Encoding.UTF8);
                }
                catch (System.Exception p_Exception)
                {
                    Logger.Instance.Error("SaveDatabase");
                    Logger.Instance.Error(p_Exception);
                }
            } } } } } }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update simple queue file
        /// </summary>
        private void UpdateSimpleQueueFile()
        {
            string l_Content = "";
            string l_Format  = CRConfig.Instance.OverlayIntegration.SimpleQueueFileFormat;

            try
            {
                lock (SongQueue)
                {
                    int l_Added = 0;
                    for (int l_I = 0; l_I < SongQueue.Count && l_Added < CRConfig.Instance.OverlayIntegration.SimpleQueueFileCount; ++l_I)
                    {
                        if (SongQueue[l_I].BeatSaver_Map != null && SongQueue[l_I].BeatSaver_Map.Partial)
                            continue;

                        string l_Line = l_Format.Replace("%i", (l_I + 1).ToString())
                                                .Replace("%n", SongQueue[l_I].GetSongName())
                                                .Replace("%m", SongQueue[l_I].GetLevelAuthorName())
                                                .Replace("%r", SongQueue[l_I].RequesterName)
                                                .Replace("%k", SongQueue[l_I].BeatSaver_Map?.id ?? "----");

                        if (l_I > 0)
                            l_Content += "\n";
                        l_Content += l_Line;

                        ++l_Added;
                    }
                }

                using (var l_FileStream = new System.IO.FileStream(m_SimpleQueueFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
                {
                    using (var l_StreamWritter = new System.IO.StreamWriter(l_FileStream, Encoding.UTF8))
                    {
                        l_StreamWritter.WriteLine(l_Content);
                    }
                }

                using (var l_FileStream = new System.IO.FileStream(m_SimpleQueueStatusFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
                {
                    using (var l_StreamWritter = new System.IO.StreamWriter(l_FileStream, Encoding.UTF8))
                    {
                        l_StreamWritter.WriteLine(CRConfig.Instance.QueueOpen ? CRConfig.Instance.OverlayIntegration.SimpleQueueStatusOpen : CRConfig.Instance.OverlayIntegration.SimpleQueueStatusClosed);
                    }
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[ChatRequest] UpdateSimpleQueueFile failed");
                Logger.Instance.Error(p_Exception);
            }
        }
    }
}
