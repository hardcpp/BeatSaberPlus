using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BeatSaberPlus.Modules.ChatRequest
{
    /// <summary>
    /// Chat request database handler
    /// </summary>
    internal partial class ChatRequest
    {
        /// <summary>
        /// DB File path
        /// </summary>
        private string m_DBFilePath = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus_ChatRequestDB.json";
        /// <summary>
        /// Simple queue File path
        /// </summary>
        private string m_SimpleQueueFilePath = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus_ChatRequest_SimpleQueue.txt";

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
                if (l_JSON["queue"] != null)
                {
                    JArray l_JSONSongs = (JArray)l_JSON["queue"];
                    for (int l_SongIt = 0; l_SongIt < l_JSONSongs.Count; l_SongIt++)
                    {
                        SongEntry l_Entry = new SongEntry();
                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("key"))
                            l_Entry.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, (string)l_JSONSongs[l_SongIt]["key"]);
                        else
                        {
                            l_Entry.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, "", "", "");
                            JsonConvert.PopulateObject((string)l_JSONSongs[l_SongIt]["btm"], l_Entry.BeatMap);
                        }

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("rqt"))
                            l_Entry.RequestTime = SDK.Misc.Time.FromUnixTime(l_JSONSongs[l_SongIt]["rqt"].Value<long>());

                        l_Entry.RequesterName   = (string)l_JSONSongs[l_SongIt]["rqn"];

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("npr"))
                            l_Entry.NamePrefix = (string)l_JSONSongs[l_SongIt]["npr"];

                        SongQueue.Add(l_Entry);

                        /// Start populate
                        l_Entry.BeatMap.Populate().ContinueWith(x => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                if (l_JSON["history"] != null)
                {
                    JArray l_JSONSongs = (JArray)l_JSON["history"];
                    for (int l_SongIt = 0; l_SongIt < l_JSONSongs.Count; l_SongIt++)
                    {
                        SongEntry l_Entry = new SongEntry();
                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("key"))
                            l_Entry.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, (string)l_JSONSongs[l_SongIt]["key"]);
                        else
                        {
                            l_Entry.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, "", "", "");
                            JsonConvert.PopulateObject((string)l_JSONSongs[l_SongIt]["btm"], l_Entry.BeatMap);
                        }

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("rqt"))
                            l_Entry.RequestTime = SDK.Misc.Time.FromUnixTime(l_JSONSongs[l_SongIt]["rqt"].Value<long>());

                        l_Entry.RequesterName   = (string)l_JSONSongs[l_SongIt]["rqn"];

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("npr"))
                            l_Entry.NamePrefix = (string)l_JSONSongs[l_SongIt]["npr"];

                        SongHistory.Add(l_Entry);

                        /// Start populate
                        l_Entry.BeatMap.Populate().ContinueWith(x => OnBeatmapPopulated(x, l_Entry));
                    }
                }
                if (l_JSON["blacklist"] != null)
                {
                    JArray l_JSONSongs = (JArray)l_JSON["blacklist"];
                    for (int l_SongIt = 0; l_SongIt < l_JSONSongs.Count; l_SongIt++)
                    {
                        SongEntry l_Entry = new SongEntry();
                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("key"))
                            l_Entry.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, (string)l_JSONSongs[l_SongIt]["key"]);
                        else
                        {
                            l_Entry.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, "", "", "");
                            JsonConvert.PopulateObject((string)l_JSONSongs[l_SongIt]["btm"], l_Entry.BeatMap);
                        }

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("rqt"))
                            l_Entry.RequestTime = SDK.Misc.Time.FromUnixTime(l_JSONSongs[l_SongIt]["rqt"].Value<long>());

                        l_Entry.RequesterName   = (string)l_JSONSongs[l_SongIt]["rqn"];

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("npr"))
                            l_Entry.NamePrefix = (string)l_JSONSongs[l_SongIt]["npr"];

                        SongBlackList.Add(l_Entry);

                        /// Start populate
                        l_Entry.BeatMap.Populate().ContinueWith(x => OnBeatmapPopulated(x, l_Entry));
                    }
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Critical("LoadDataBase");
                Logger.Instance.Critical(p_Exception);
            }
        }
        /// <summary>
        /// Save database
        /// </summary>
        private void SaveDatabase()
        {
            lock (SongQueue) { lock (SongHistory) { lock (SongBlackList) {
                if (SongQueue.Count == 0 && SongHistory.Count == 0 && SongBlackList.Count == 0)
                    return;

                try
                {
                    var l_JSON      = new JObject();
                    var l_Requests  = new JArray();
                    var l_History   = new JArray();
                    var l_BlackList = new JArray();
                    foreach (var l_Current in SongQueue)
                    {
                        var l_Object = new JObject();
                        l_Object["key"] = l_Current.BeatMap.Key;
                        l_Object["rqt"] = l_Current.RequestTime.HasValue ? SDK.Misc.Time.ToUnixTime(l_Current.RequestTime.Value) : SDK.Misc.Time.UnixTimeNow();
                        l_Object["rqn"] = l_Current.RequesterName;
                        l_Object["npr"] = l_Current.NamePrefix;
                        l_Requests.Add(l_Object);
                    }
                    foreach (var l_Current in SongHistory)
                    {
                        var l_Object = new JObject();
                        l_Object["key"] = l_Current.BeatMap.Key;
                        l_Object["rqt"] = l_Current.RequestTime.HasValue ? SDK.Misc.Time.ToUnixTime(l_Current.RequestTime.Value) : SDK.Misc.Time.UnixTimeNow();
                        l_Object["rqn"] = l_Current.RequesterName;
                        l_Object["npr"] = l_Current.NamePrefix;
                        l_History.Add(l_Object);
                    }
                    foreach (var l_Current in SongBlackList)
                    {
                        var l_Object = new JObject();
                        l_Object["key"] = l_Current.BeatMap.Key;
                        l_Object["rqt"] = l_Current.RequestTime.HasValue ? SDK.Misc.Time.ToUnixTime(l_Current.RequestTime.Value) : SDK.Misc.Time.UnixTimeNow();
                        l_Object["rqn"] = l_Current.RequesterName;
                        l_Object["npr"] = l_Current.NamePrefix;
                        l_BlackList.Add(l_Object);
                    }

                    l_JSON.Add("queue", l_Requests);
                    l_JSON.Add("history", l_History);
                    l_JSON.Add("blacklist", l_BlackList);

                    string l_ResultJSON = l_JSON.ToString();
                    System.IO.File.WriteAllText(m_DBFilePath, l_ResultJSON, UTF8Encoding.UTF8);
                }
                catch (System.Exception p_Exception)
                {
                    Logger.Instance.Critical("SaveDatabase");
                    Logger.Instance.Critical(p_Exception);
                }
            } } }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update simple queue file
        /// </summary>
        private void UpdateSimpleQueueFile()
        {
            string l_Content = "";
            string l_Format  = Config.ChatRequest.SimpleQueueFileFormat;

            lock (SongQueue)
            {
                int l_Added = 0;
                for (int l_I = 0; l_I < SongQueue.Count && l_Added < Config.ChatRequest.SimpleQueueFileCount; ++l_I)
                {
                    if (SongQueue[l_I].BeatMap.Partial)
                        continue;

                    string l_Line = l_Format.Replace("%i", (l_I + 1).ToString())
                                            .Replace("%n", SongQueue[l_I].BeatMap.Name)
                                            .Replace("%m", SongQueue[l_I].BeatMap.Uploader.Username)
                                            .Replace("%r", SongQueue[l_I].RequesterName)
                                            .Replace("%k", SongQueue[l_I].BeatMap.Key);

                    if (l_I > 0)
                        l_Content += "\n";
                    l_Content += l_Line;

                    ++l_Added;
                }
            }

            try
            {
                using (var l_FileStream = new System.IO.FileStream(m_SimpleQueueFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite))
                {
                    using (var l_StreamWritter = new System.IO.StreamWriter(l_FileStream, Encoding.UTF8))
                    {
                        l_StreamWritter.WriteLine(l_Content);
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
