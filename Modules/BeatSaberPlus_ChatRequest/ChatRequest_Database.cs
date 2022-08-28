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
        private string m_DBFilePathOld = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus_ChatRequestDB.json";
        /// <summary>
        /// Simple queue File path
        /// </summary>
        private string m_SimpleQueueFilePath = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus\\ChatRequest\\SimpleQueue.txt";
        private string m_SimpleQueueFilePathOld = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus_ChatRequest_SimpleQueue.txt";
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
                        var l_Key       = l_Current["key"]?.Value<string>()     ?? "";
                        var l_Time      = l_Current["rqt"]?.Value<long>()       ?? CP_SDK.Misc.Time.UnixTimeNow();
                        var l_Requester = l_Current["rqn"]?.Value<string>()     ?? "";
                        var l_Prefix    = l_Current["npr"]?.Value<string>()     ?? "";
                        var l_Message   = l_Current["msg"]?.Value<string>()     ?? "";

                        if (l_Key == "" && l_Current.ContainsKey("id"))
                            l_Key = l_Current["id"].Value<int>().ToString("x");

                        if (l_Key == "")
                            continue;

                        SongEntry l_Entry = new SongEntry()
                        {
                            BeatMap         = BeatSaberPlus.SDK.Game.BeatMapsClient.GetFromCacheByKey(l_Key) ?? BeatSaberPlus.SDK.Game.BeatMaps.MapDetail.PartialFromKey(l_Key),
                            RequestTime     = CP_SDK.Misc.Time.FromUnixTime(l_Time),
                            RequesterName   = l_Requester,
                            NamePrefix      = l_Prefix,
                            Message         = l_Message
                        };

                        SongQueue.Add(l_Entry);

                        /// Start populate
                        if (l_Entry.BeatMap.Partial)
                            l_Entry.BeatMap.Populate((x) => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                if (l_JSON["history"] != null && l_JSON["history"].Type == JTokenType.Array)
                {
                    foreach (JObject l_Current in (JArray)l_JSON["history"])
                    {
                        var l_Key       = l_Current["key"]?.Value<string>()     ?? "";
                        var l_Time      = l_Current["rqt"]?.Value<long>()       ?? CP_SDK.Misc.Time.UnixTimeNow();
                        var l_Requester = l_Current["rqn"]?.Value<string>()     ?? "";
                        var l_Prefix    = l_Current["npr"]?.Value<string>()     ?? "";
                        var l_Message   = l_Current["msg"]?.Value<string>()     ?? "";

                        if (l_Key == "" && l_Current.ContainsKey("id"))
                            l_Key = l_Current["id"].Value<int>().ToString("x");

                        if (l_Key == "")
                            continue;

                        SongEntry l_Entry = new SongEntry()
                        {
                            BeatMap         = BeatSaberPlus.SDK.Game.BeatMapsClient.GetFromCacheByKey(l_Key) ?? BeatSaberPlus.SDK.Game.BeatMaps.MapDetail.PartialFromKey(l_Key),
                            RequestTime     = CP_SDK.Misc.Time.FromUnixTime(l_Time),
                            RequesterName   = l_Requester,
                            NamePrefix      = l_Prefix,
                            Message         = l_Message
                        };

                        SongHistory.Add(l_Entry);

                        /// Start populate
                        if (l_Entry.BeatMap.Partial)
                            l_Entry.BeatMap.Populate((x) => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                if (l_JSON["blacklist"] != null && l_JSON["blacklist"].Type == JTokenType.Array)
                {
                    foreach (JObject l_Current in (JArray)l_JSON["blacklist"])
                    {
                        var l_Key       = l_Current["key"]?.Value<string>()     ?? "";
                        var l_Time      = l_Current["rqt"]?.Value<long>()       ?? CP_SDK.Misc.Time.UnixTimeNow();
                        var l_Requester = l_Current["rqn"]?.Value<string>()     ?? "";
                        var l_Prefix    = l_Current["npr"]?.Value<string>()     ?? "";
                        var l_Message   = l_Current["msg"]?.Value<string>()     ?? "";

                        if (l_Key == "" && l_Current.ContainsKey("id"))
                            l_Key = l_Current["id"].Value<int>().ToString("x");

                        if (l_Key == "")
                            continue;

                        SongEntry l_Entry = new SongEntry()
                        {
                            BeatMap         = BeatSaberPlus.SDK.Game.BeatMapsClient.GetFromCacheByKey(l_Key) ?? BeatSaberPlus.SDK.Game.BeatMaps.MapDetail.PartialFromKey(l_Key),
                            RequestTime     = CP_SDK.Misc.Time.FromUnixTime(l_Time),
                            RequesterName   = l_Requester,
                            NamePrefix      = l_Prefix,
                            Message         = l_Message
                        };

                        SongBlackList.Add(l_Entry);

                        /// Start populate
                        if (l_Entry.BeatMap.Partial)
                            l_Entry.BeatMap.Populate((x) => OnBeatmapPopulated(x, l_Entry));
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
                if (l_JSON["allowlist"] != null && l_JSON["allowlist"].Type == JTokenType.Array)
                {
                    foreach (var l_Current in (JArray)l_JSON["allowlist"])
                        AllowList.Add(l_Current.Value<string>() ?? "");
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
            lock (SongQueue) { lock (SongHistory) { lock (SongBlackList) { lock (BannedUsers) { lock (Remaps) { lock (AllowList) {
                if (SongQueue.Count == 0 && SongHistory.Count == 0 && SongBlackList.Count == 0)
                    return;

                try
                {
                    var l_Requests = new JArray();
                    foreach (var l_Current in SongQueue)
                    {
                        var l_Object = new JObject();
                        l_Object["key"]  = l_Current.BeatMap.id;
                        l_Object["rqt"] = l_Current.RequestTime.HasValue ? CP_SDK.Misc.Time.ToUnixTime(l_Current.RequestTime.Value) : CP_SDK.Misc.Time.UnixTimeNow();
                        l_Object["rqn"] = l_Current.RequesterName;
                        l_Object["npr"] = l_Current.NamePrefix;
                        l_Object["msg"] = l_Current.Message;
                        l_Requests.Add(l_Object);
                    }

                    var l_History = new JArray();
                    foreach (var l_Current in SongHistory)
                    {
                        var l_Object = new JObject();
                        l_Object["key"]  = l_Current.BeatMap.id;
                        l_Object["rqt"] = l_Current.RequestTime.HasValue ? CP_SDK.Misc.Time.ToUnixTime(l_Current.RequestTime.Value) : CP_SDK.Misc.Time.UnixTimeNow();
                        l_Object["rqn"] = l_Current.RequesterName;
                        l_Object["npr"] = l_Current.NamePrefix;
                        l_Object["msg"] = l_Current.Message;
                        l_History.Add(l_Object);
                    }

                    var l_BlackList = new JArray();
                    foreach (var l_Current in SongBlackList)
                    {
                        var l_Object = new JObject();
                        l_Object["key"]  = l_Current.BeatMap.id;
                        l_Object["rqt"] = l_Current.RequestTime.HasValue ? CP_SDK.Misc.Time.ToUnixTime(l_Current.RequestTime.Value) : CP_SDK.Misc.Time.UnixTimeNow();
                        l_Object["rqn"] = l_Current.RequesterName;
                        l_Object["npr"] = l_Current.NamePrefix;
                        l_Object["msg"] = l_Current.Message;
                        l_BlackList.Add(l_Object);
                    }

                    var l_Remaps = new JArray();
                    foreach (var l_KVP in Remaps)
                    {
                        l_Remaps.Add(new JObject()
                        {
                            ["l"] = l_KVP.Key,
                            ["r"] = l_KVP.Value
                        });
                    }

                    var l_JSON      = new JObject();
                    l_JSON.Add("queue",         l_Requests);
                    l_JSON.Add("history",       l_History);
                    l_JSON.Add("blacklist",     l_BlackList);
                    l_JSON.Add("bannedusers",   new JArray(BannedUsers.ToArray()));
                    l_JSON.Add("bannedmappers", new JArray(BannedMappers.ToArray()));
                    l_JSON.Add("remaps",        l_Remaps);
                    l_JSON.Add("allowlist",     new JArray(AllowList.ToArray()));

                    string l_ResultJSON = l_JSON.ToString();
                    System.IO.File.WriteAllText(m_DBFilePath, l_ResultJSON, UTF8Encoding.UTF8);
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
                        if (SongQueue[l_I].BeatMap == null || SongQueue[l_I].BeatMap.Partial)
                            continue;

                        string l_Line = l_Format.Replace("%i", (l_I + 1).ToString())
                                                .Replace("%n", SongQueue[l_I].BeatMap.name)
                                                .Replace("%m", SongQueue[l_I].BeatMap.metadata.levelAuthorName)
                                                .Replace("%r", SongQueue[l_I].RequesterName)
                                                .Replace("%k", SongQueue[l_I].BeatMap.id);

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
