using CP_SDK.Chat.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BeatSaberPlus_ChatRequest
{
    /// <summary>
    /// Chat request logic handler
    /// </summary>
    public partial class ChatRequest
    {
        public bool QueueOpen { get; private set; } = false;
        public int QueueDuration { get; private set; } = 0;
        public int SongQueueCount { get
            {
                lock (SongQueue)
                    return SongQueue.Count;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal List<Models.SongEntry> SongQueue      = new List<Models.SongEntry>();
        internal List<Models.SongEntry> SongHistory    = new List<Models.SongEntry>();
        internal List<Models.SongEntry> SongAllowlist  = new List<Models.SongEntry>();
        internal List<Models.SongEntry> SongBlocklist  = new List<Models.SongEntry>();

        internal List<string> BannedUsers   = new List<string>();
        internal List<string> BannedMappers = new List<string>();

        internal Dictionary<string, string> Remaps = new Dictionary<string, string>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private ConcurrentBag<string>   m_RequestedThisSessionID        = new ConcurrentBag<string>();
        private ConcurrentBag<string>   m_RequestedThisSessionHash      = new ConcurrentBag<string>();
        private BeatmapLevel            m_LastPlayingLevel              = null;
        private string                  m_LastPlayingLevelResponse      = "";
        private string                  m_LastPlayingLevelResponseLink  = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add to queue from a BSR key
        /// </summary>
        /// <param name="bsrKey">BSR Key in hex</param>
        /// <param name="requester">Optional requester</param>
        /// <param name="onBehalfOf">Optional on behalf of (Someone with privileges requesting for someone else)</param>
        /// <param name="forceNamePrefix">Optional name prefix override</param>
        /// <param name="asModAdd">As a moderator add</param>
        /// <param name="addToTop">Should add to the top of the queue?</param>
        /// <param name="callback">Result callback</param>
        public void AddToQueueFromBSRKey(
            string                          bsrKey,
            IChatUser                       requester,
            string                          onBehalfOf,
            string                          forceNamePrefix,
            bool                            asModAdd,
            bool                            addToTop,
            Action<Models.AddToQueueResult> callback)
        {
            if (!QueueOpen && !asModAdd)
            {
                callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.QueueClosed, bsrKey));
                return;
            }

            if (requester != null && IsRequesterBanned(requester))
            {
                callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.RequesterBanned, bsrKey));
                return;
            }

            /// Look for remaps
            lock (Remaps)
            {
                if (Remaps.TryGetValue(bsrKey.ToLower(), out var l_RemapKey))
                    bsrKey = l_RemapKey;
            }

            var l_ForceAllow = false;
            lock (SongAllowlist)
            {
                var l_BeatMap = SongAllowlist.FirstOrDefault(x => x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(bsrKey, StringComparison.OrdinalIgnoreCase));
                if (l_BeatMap != null)
                    l_ForceAllow = true;
            }

            /// Check if already requested
            if (!asModAdd && m_RequestedThisSessionID.Contains(bsrKey.ToLower()))
            {
                callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.AlreadyRequestedThisSession, bsrKey));
                return;
            }

            /// Check if allow listed or blocklisted
            if (!asModAdd && !l_ForceAllow)
            {
                lock (SongBlocklist)
                {
                    var l_BeatMap = SongBlocklist.FirstOrDefault(x => x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(bsrKey, StringComparison.OrdinalIgnoreCase));
                    if (l_BeatMap != null)
                    {
                        callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.MapBanned, bsrKey, l_BeatMap));
                        return;
                    }
                }
            }

            var l_RateLimit     = new Models.RequesterRateLimit(CRConfig.Instance.UserMaxRequest, "Users");

            /// Check if already in queue
            lock (SongQueue)
            {
                var l_BeatMap = SongQueue.FirstOrDefault(x => x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(bsrKey, StringComparison.OrdinalIgnoreCase));
                if (l_BeatMap != null)
                {
                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.AlreadyInQueue, bsrKey, l_BeatMap));
                    return;
                }

                if (requester != null)
                    l_RateLimit.CurrentRequestCount = SongQueue.Where(x => x.RequesterName == requester.UserName).Count();
            }

            /// Handle limits and title prefix
            if (requester != null)
            {
                l_RateLimit.UpdateRateLimitFromRequester(requester);

                if (l_RateLimit.CurrentRequestCount >= l_RateLimit.RequestMaxCount)
                {
                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.RequestLimit, bsrKey, l_RateLimit));
                    return;
                }
            }

            /// Fetch beatmap
            CP_SDK_BS.Game.BeatMapsClient.GetOnlineByKey(bsrKey, (_, p_BeatMap) =>
            {
                AddToQueueFinal(
                    requestTerm:    bsrKey,
                    requester:      requester,
                    onBehalfOf:     onBehalfOf,
                    forceNamePrefix:forceNamePrefix,
                    asModAdd:       asModAdd,
                    addToTop:       addToTop,
                    forceAllow:     l_ForceAllow,
                    beatmapLevel:   null,
                    mapDetail:      p_BeatMap,
                    callback:       callback
                );
            });
        }
        /// <summary>
        /// Add to queue from a BeatmapLevel
        /// </summary>
        /// <param name="beatmapLevel">Local level</param>
        /// <param name="requester">Optional requester</param>
        /// <param name="onBehalfOf">Optional on behalf of (Someone with privileges requesting for someone else)</param>
        /// <param name="forceNamePrefix">Optional name prefix override</param>
        /// <param name="asModAdd">As a moderator add</param>
        /// <param name="addToTop">Should add to the top of the queue?</param>
        /// <param name="callback">Result callback</param>
        public void AddToQueueFromBeatmapLevel(
            BeatmapLevel                    beatmapLevel,
            IChatUser                       requester,
            string                          onBehalfOf,
            string                          forceNamePrefix,
            bool                            asModAdd,
            bool                            addToTop,
            Action<Models.AddToQueueResult> callback)
        {
            var l_LevelHash = string.Empty;
            if (beatmapLevel == null || !CP_SDK_BS.Game.Levels.TryGetHashFromLevelID(beatmapLevel.levelID, out l_LevelHash))
            {
                callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.NotFound, l_LevelHash));
                return;
            }

            if (!QueueOpen && !asModAdd)
            {
                callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.QueueClosed, l_LevelHash));
                return;
            }

            if (requester != null && IsRequesterBanned(requester))
            {
                callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.RequesterBanned, l_LevelHash));
                return;
            }

            var l_ForceAllow = false;
            lock (SongAllowlist)
            {
                var l_BeatMap = SongAllowlist.FirstOrDefault(x => x.GetLevelHash() == l_LevelHash);
                if (l_BeatMap != null)
                    l_ForceAllow = true;
            }

            /// Check if already requested
            if (!asModAdd && m_RequestedThisSessionHash.Contains(l_LevelHash))
            {
                callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.AlreadyRequestedThisSession, l_LevelHash));
                return;
            }

            /// Check if allow listed or blocklisted
            if (!asModAdd && !l_ForceAllow)
            {
                lock (SongBlocklist)
                {
                    var l_BeatMap = SongBlocklist.FirstOrDefault(x => x.GetLevelHash().Equals(l_LevelHash, StringComparison.OrdinalIgnoreCase));
                    if (l_BeatMap != null)
                    {
                        callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.MapBanned, l_LevelHash, l_BeatMap));
                        return;
                    }
                }
            }

            var l_RateLimit = new Models.RequesterRateLimit(CRConfig.Instance.UserMaxRequest, "Users");

            /// Check if already in queue
            lock (SongQueue)
            {
                var l_BeatMap = SongQueue.FirstOrDefault(x => x.GetLevelHash().Equals(l_LevelHash, StringComparison.OrdinalIgnoreCase));
                if (l_BeatMap != null)
                {
                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.AlreadyInQueue, l_LevelHash, l_BeatMap));
                    return;
                }

                if (requester != null)
                    l_RateLimit.CurrentRequestCount = SongQueue.Where(x => x.RequesterName == requester.UserName).Count();
            }

            /// Handle limits and title prefix
            if (requester != null)
            {
                l_RateLimit.UpdateRateLimitFromRequester(requester);

                if (l_RateLimit.CurrentRequestCount >= l_RateLimit.RequestMaxCount)
                {
                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.RequestLimit, l_LevelHash, l_RateLimit));
                    return;
                }
            }

            /// Fetch beatmap
            CP_SDK_BS.Game.BeatMapsClient.GetOnlineByHash(l_LevelHash, (_, p_BeatMap) =>
            {
                AddToQueueFinal(
                    requestTerm:    l_LevelHash,
                    requester:      requester,
                    onBehalfOf:     onBehalfOf,
                    forceNamePrefix:forceNamePrefix,
                    asModAdd:       asModAdd,
                    addToTop:       addToTop,
                    forceAllow:     l_ForceAllow,
                    beatmapLevel:   beatmapLevel,
                    mapDetail:      p_BeatMap,
                    callback:       callback
                );
            });
        }
        /// <summary>
        /// Add to queue final
        /// </summary>
        /// <param name="requestTerm"></param>
        /// <param name="requester">Optional requester</param>
        /// <param name="onBehalfOf">Optional on behalf of (Someone with privileges requesting for someone else)</param>
        /// <param name="forceNamePrefix">Optional name prefix override</param>
        /// <param name="asModAdd">As a moderator add</param>
        /// <param name="addToTop">Should add to the top of the queue?</param>
        /// <param name="forceAllow">Force allow adding to queue</param>
        /// <param name="beatmapLevel">Local level</param>
        /// <param name="mapDetail">Found map details from BeatMaps</param>
        /// <param name="callback">Result callback</param>
        private void AddToQueueFinal(
            string                              requestTerm,
            IChatUser                           requester,
            string                              onBehalfOf,
            string                              forceNamePrefix,
            bool                                asModAdd,
            bool                                addToTop,
            bool                                forceAllow,
            BeatmapLevel                        beatmapLevel,
            CP_SDK_BS.Game.BeatMaps.MapDetail   mapDetail,
            Action<Models.AddToQueueResult>     callback)
        {
            try
            {
                if (mapDetail == null)
                {
                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.NotFound, requestTerm));
                    return;
                }

                /// Check if already requested (A second time because of delay)
                if (!asModAdd && m_RequestedThisSessionID.Contains(mapDetail.id.ToLower()))
                {
                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.AlreadyRequestedThisSession, requestTerm));
                    return;
                }

                var l_RequesterName = requester?.UserName ?? "<unk>";
                if (requester == null && !string.IsNullOrEmpty(onBehalfOf))
                {
                    l_RequesterName = onBehalfOf;
                    onBehalfOf = string.Empty;
                }

                var l_NamePrefix    = GetTitlePrefixFromRequester(requester);
                var l_Entry         = new Models.SongEntry()
                {
                    BeatSaver_Map   = mapDetail,
                    BeatmapLevel    = beatmapLevel,
                    RequesterName   = l_RequesterName,
                    RequestTime     = DateTime.Now,
                    TitlePrefix     = !string.IsNullOrEmpty(forceNamePrefix) ? forceNamePrefix : l_NamePrefix
                };

                var l_IsMapperBanned = false;
                lock (BannedMappers)
                    l_IsMapperBanned = BannedMappers.Contains(mapDetail.uploader.name.ToLower());

                if (l_IsMapperBanned)
                {
                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.MapperBanned, requestTerm, l_Entry));
                    return;
                }

                var l_FilterError = string.Empty;
                if (asModAdd || forceAllow || FilterBeatMap(mapDetail, l_RequesterName, out l_FilterError))
                {
                    m_RequestedThisSessionID.Add(mapDetail.id.ToLower());
                    m_RequestedThisSessionHash.Add(l_Entry.GetLevelHash());

                    if (asModAdd && !string.IsNullOrEmpty(onBehalfOf))
                    {
                        l_RequesterName = onBehalfOf + "\n(Added by " + l_NamePrefix + " " + l_RequesterName + ")";
                        l_NamePrefix    = string.Empty;
                    }

                    lock (SongQueue)
                    {
                        if (addToTop)
                            SongQueue.Insert(0, l_Entry);
                        else
                            SongQueue.Add(l_Entry);
                    }

                    OnBeatmapPopulated(true, l_Entry);

                    /// Update request manager
                    OnQueueChanged();

                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.OK, requestTerm, l_Entry));
                }
                else if (!string.IsNullOrEmpty(l_FilterError))
                {
                    callback?.Invoke(new Models.AddToQueueResult(Models.EAddToQueueResult.FilterError, requestTerm, l_Entry, l_FilterError));
                }
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("AddToQueueFinal");
                Logger.Instance.Error(p_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the queue is changed
        /// </summary>
        /// <param name="updateSimpleQueueFile">Update simple queue list for OBS integration</param>
        /// <param name="updateSongList">Update queue and total queue duration</param>
        private void OnQueueChanged(bool updateSimpleQueueFile = true, bool updateSongList = true)
        {
            /// Update simple queue file
            if (updateSimpleQueueFile)
                UpdateSimpleQueueFile();

            if (updateSongList)
            {
                QueueDuration = 0;
                try
                {
                    lock (SongQueue)
                    {
                        SongQueue.ForEach(x => QueueDuration += (int)x.GetSongDuration());
                    }
                }
                catch
                {

                }
            }

            /// Avoid saving during play
            if (CP_SDK_BS.Game.Logic.ActiveScene != CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                {
                    UpdateButton();

                    if (updateSongList && UI.ManagerMainView.CanBeUpdated)
                        UI.ManagerMainView.Instance.RebuildSongList();

                    SaveDatabase();
                });
            };
        }
        /// <summary>
        /// When a beatmap get fully loaded
        /// </summary>
        /// <param name="valid">Is the beatmap valid</param>
        /// <param name="songEntry">Context song entry</param>
        private void OnBeatmapPopulated(bool valid, Models.SongEntry songEntry)
        {
            if (!valid)
            {
                lock (SongQueue) { lock (SongHistory) { lock (SongBlocklist) {
                    SongQueue.RemoveAll(    x => x == songEntry);
                    SongHistory.RemoveAll(  y => y == songEntry);
                    SongBlocklist.RemoveAll(z => z == songEntry);
                } } }

                CP_SDK_BS.Game.BeatMapsClient.ClearCache(songEntry.BeatSaver_Map?.id ?? null);

                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                {
                    UpdateButton();

                    if (UI.ManagerMainView.CanBeUpdated)
                        UI.ManagerMainView.Instance.RebuildSongList();

                    SaveDatabase();
                });
            }

            if (!valid)
                return;

            CP_SDK_BS.Game.BeatMapsClient.Cache(songEntry.BeatSaver_Map);

            /// Update request manager
            OnQueueChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send message to chat
        /// </summary>
        /// <param name="message">Messages to send</param>
        /// <param name="service">Source channel</param>
        /// <param name="sourceMessage">Context message</param>
        /// <param name="songEntry">Context SongEntry</param>
        /// <param name="toReplace">Additional replace keys</param>
        internal void SendChatMessage(
                string                      message,
                IChatService                service,
                IChatMessage                sourceMessage,
                Models.SongEntry            songEntry = null,
                params (string, string)[]   toReplace
            )
        {
            if (sourceMessage != null && sourceMessage.Sender != null)
                message = message.Replace("$UserName", sourceMessage.Sender.DisplayName);

            if (songEntry != null)
            {
                var l_LevelID           = songEntry.BeatSaver_Map?.id ?? "----";
                var l_SongName          = songEntry.GetSongName();
                var l_SongAuthorName    = songEntry.GetSongAuthorName();
                var l_LevelAuthorName   = songEntry.GetLevelAuthorName();
                var l_LevelUploaderName = songEntry.GetLevelUploaderName();

                message = message.Replace("$RequesterName",     songEntry.RequesterName);
                message = message.Replace("$BSRKey",            l_LevelID);
                message = message.Replace("$SongName",          CRConfig.Instance.SafeMode2 ? l_LevelID : l_SongName.Replace(".", " . "));
                message = message.Replace("$SongAuthorName",    CRConfig.Instance.SafeMode2 ? l_LevelID : l_SongAuthorName.Replace(".", " . "));
                message = message.Replace("$LevelAuthorName",   CRConfig.Instance.SafeMode2 ? l_LevelID : l_LevelAuthorName.Replace(".", " . "));
                message = message.Replace("$UploaderName",      CRConfig.Instance.SafeMode2 ? l_LevelID : l_LevelUploaderName.Replace(".", " . "));

                if (songEntry.BeatSaver_Map != null)
                    message = message.Replace("$Vote",  Math.Round((double)songEntry.BeatSaver_Map.stats.score * 100f, 0).ToString());
                else
                    message = message.Replace("$Vote",  "--");
            }

            if (toReplace != null)
            {
                foreach (var l_CurrentKey in toReplace)
                    message = message.Replace(
                        l_CurrentKey.Item1,
                        l_CurrentKey.Item1 == "$SongLink" ? l_CurrentKey.Item2 : l_CurrentKey.Item2.Replace(".", " . ")
                    );
            }

            if (service == null && sourceMessage == null)
                CP_SDK.Chat.Service.BroadcastMessage("! " + message);
            else
                service.SendTextMessage(sourceMessage.Channel, "! " + message);
        }
        /// <summary>
        /// Is a requester banned
        /// </summary>
        /// <param name="requester">Requester to check</param>
        /// <returns></returns>
        private bool IsRequesterBanned(IChatUser requester)
        {
            if (requester == null)
                return false;

            lock (BannedUsers)
                return BannedUsers.Any(x => x.Equals(requester.UserName, StringComparison.OrdinalIgnoreCase));
        }
        /// <summary>
        /// Has requester permission
        /// </summary>
        /// <param name="requester">Requester</param>
        /// <param name="permission">Checking permission</param>
        /// <returns></returns>
        private bool HasRequesterPermission(IChatUser requester, CRConfig._Commands.EPermission permission)
        {
            if (requester.IsBroadcaster)
                return true;

            if ((permission & CRConfig._Commands.EPermission.Viewers) != 0)
                return true;

            if ((permission & CRConfig._Commands.EPermission.Subscribers) != 0 && requester.IsSubscriber)
                return true;

            if ((permission & CRConfig._Commands.EPermission.VIPs) != 0 && requester.IsVip)
                return true;

            if ((permission & CRConfig._Commands.EPermission.Moderators) != 0 && requester.IsModerator)
                return true;

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Toggle queue status
        /// </summary>
        public void ToggleQueueStatus()
        {
            if (QueueOpen)
                SendChatMessage(CRConfig.Instance.Commands.CloseCommand_OK, null, null);
            else
                SendChatMessage(CRConfig.Instance.Commands.OpenCommand_OK, null, null);

            QueueOpen = !QueueOpen;

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                CRConfig.Instance.QueueOpen = QueueOpen;
                CRConfig.Instance.Save();

                UpdateSimpleQueueFile();

                if (UI.ManagerLeftView.CanBeUpdated)
                    UI.ManagerLeftView.Instance.UpdateQueueStatus();
            });
        }
        /// <summary>
        /// Re-Enqueue a song by play or skip
        /// </summary>
        /// <param name="songEntry">Song to re-enqueue</param>
        internal void ReEnqueueSongEntry(Models.SongEntry songEntry)
        {
            if (songEntry == null)
                return;

            lock (SongQueue)
            {
                lock (SongHistory)
                {
                    /// Remove from history
                    if (SongHistory.Contains(songEntry))
                        SongHistory.Remove(songEntry);

                    /// Move at top of queue
                    SongQueue.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());
                    SongQueue.Insert(0, songEntry);
                }
            }

            /// Update request manager
            OnQueueChanged();
        }
        /// <summary>
        /// Dequeue a song by play or skip
        /// </summary>
        /// <param name="songEntry">Song to dequeue</param>
        /// <param name="notifyChat">Should notify chat?</param>
        internal void DequeueSongEntry(Models.SongEntry songEntry, bool notifyChat)
        {
            if (songEntry == null)
                return;

            lock (SongQueue) { lock (SongHistory) { lock (SongBlocklist)
            {
                /// Remove from queue
                if (SongQueue.Contains(songEntry))
                    SongQueue.Remove(songEntry);

                /// Move at top of history
                SongHistory.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());
                SongHistory.Insert(0, songEntry);

                /// Reduce history size
                while (SongHistory.Count > CRConfig.Instance.HistorySize)
                {
                    var l_ToRemove = SongHistory.ElementAt(SongHistory.Count - 1);

                    /// Clear cache
                    if (SongBlocklist.Count(x => x.GetLevelHash() == l_ToRemove.GetLevelHash()) == 0)
                        CP_SDK_BS.Game.BeatMapsClient.ClearCache(l_ToRemove.BeatSaver_Map.id);

                    SongHistory.RemoveAt(SongHistory.Count - 1);
                }
            } } }

            /// Update request manager
            OnQueueChanged();

            if (notifyChat)
                SendChatMessage(CRConfig.Instance.Messages.NextSong, null, null, songEntry);
        }
        /// <summary>
        /// Clear queue
        /// </summary>
        public void ClearSongEntryQueue()
        {
            lock (SongQueue)
            {
                SongQueue.Clear();
            }

            /// Update request manager
            OnQueueChanged();
        }
        /// <summary>
        /// Add a song entry to the allowlist
        /// </summary>
        /// <param name="songEntry">Song entry to allow</param>
        internal void AddSongEntryToAllowlist(Models.SongEntry songEntry)
        {
            if (songEntry == null)
                return;

            lock (SongAllowlist) { lock (SongBlocklist)
            {
                /// Remove from blocklist
                SongBlocklist.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());

                /// Add to allowlist
                SongAllowlist.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());
                SongAllowlist.Insert(0, songEntry);
            } }

            /// Update request manager
            OnQueueChanged(false);
        }
        /// <summary>
        /// Remove a song entry from the allowlist
        /// </summary>
        /// <param name="songEntry">Song entry to remove from the allowlist</param>
        internal void RemoveSongEntryFromAllowlist(Models.SongEntry songEntry)
        {
            if (songEntry == null)
                return;

            lock (SongHistory) { lock (SongAllowlist)
            {
                /// Remove from allowlist
                SongAllowlist.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());
                /// Move at top of history
                SongHistory.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());
                SongHistory.Insert(0, songEntry);

                /// Reduce history size
                while (SongHistory.Count > CRConfig.Instance.HistorySize)
                    SongHistory.RemoveAt(SongHistory.Count - 1);
            } }

            /// Update request manager
            OnQueueChanged(false);
        }
        /// <summary>
        /// Add a song entry to the blocklist
        /// </summary>
        /// <param name="songEntry">Song entry to block</param>
        internal void AddSongEntryToBlocklist(Models.SongEntry songEntry)
        {
            if (songEntry == null)
                return;

            lock (SongQueue) { lock (SongHistory) { lock (SongBlocklist)
            {
                /// Remove from queue
                SongQueue.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());

                /// Remove from history
                SongHistory.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());

                /// Add to blocklist
                SongBlocklist.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());
                SongBlocklist.Insert(0, songEntry);
            } } }

            /// Update request manager
            OnQueueChanged(false);
        }
        /// <summary>
        /// Remove a song entry from the blocklist
        /// </summary>
        /// <param name="songEntry">Song entry to remove from the blocklist</param>
        internal void RemoveSongEntryFromBlocklist(Models.SongEntry songEntry)
        {
            if (songEntry == null)
                return;

            lock (SongQueue) { lock (SongHistory) { lock (SongBlocklist)
            {
                /// Remove from blocklist
                SongBlocklist.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());

                /// Move at top of history
                SongHistory.RemoveAll(x => x.GetLevelHash() == songEntry.GetLevelHash());
                SongHistory.Insert(0, songEntry);

                /// Reduce history size
                while (SongHistory.Count > CRConfig.Instance.HistorySize)
                    SongHistory.RemoveAt(SongHistory.Count - 1);
            } } }

            /// Update request manager
            OnQueueChanged(false);
        }
        /// <summary>
        /// Reset the song entry blocklist
        /// </summary>
        internal void ResetSongEntryBlocklist()
        {
            lock (SongHistory) { lock (SongBlocklist)
            {
                /// Add all blocked songs to history
                SongBlocklist.ForEach(l_BlocklistedSong =>
                {
                    /// Move at top of history
                    SongHistory.RemoveAll(x => x.GetLevelHash() == l_BlocklistedSong.GetLevelHash());
                    SongHistory.Insert(0, l_BlocklistedSong);
                });

                /// Remove all blocked songs
                SongBlocklist.Clear();
            } }

            /// Update request manager
            OnQueueChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Filter a beatmap
        /// </summary>
        /// <param name="beatMap">Beatmap to filter</param>
        /// <param name="requesterName">Requester name</param>
        /// <param name="filterError">Output reply</param>
        /// <returns></returns>
        private static bool FilterBeatMap(CP_SDK_BS.Game.BeatMaps.MapDetail beatMap, string requesterName, out string filterError)
        {
            filterError = "";

            bool    l_FilterNPSMin  = CRConfig.Instance.Filters.NPSMin;
            bool    l_FilterNPSMax  = CRConfig.Instance.Filters.NPSMax;
            bool    l_FilterNJSMin  = CRConfig.Instance.Filters.NJSMin;
            bool    l_FilterNJSMax  = CRConfig.Instance.Filters.NJSMax;
            float   l_Vote          = (float)Math.Round((double)beatMap.stats.score * 100f, 0);

            /// Thanks beatsaver, fix filters for maps without votes
            if ((beatMap.stats.downvotes + beatMap.stats.upvotes) == 0)
                l_Vote = 50f;

            if (CRConfig.Instance.Filters.NoBeatSage && beatMap.automapper)
            {
                filterError = $"@{requesterName} BeatSage maps are not allowed!";
                return false;
            }
            if (CRConfig.Instance.Filters.NoRanked && beatMap.ranked)
            {
                filterError = $"@{requesterName} Ranked maps are not allowed!";
                return false;
            }
            if (l_FilterNPSMin || l_FilterNPSMax)
            {
                int l_NPSMin = CRConfig.Instance.Filters.NPSMinV;
                int l_NPSMax = CRConfig.Instance.Filters.NPSMaxV;

                var l_Diffs = beatMap.SelectMapVersion().diffs;

                if (l_FilterNPSMin && !l_FilterNPSMax && l_Diffs.Count(x => x.nps >= l_NPSMin) == 0)
                {
                    filterError = $"@{requesterName} this song has no difficulty with a NPS of {l_NPSMin} minimum!";
                    return false;
                }
                if (!l_FilterNPSMin && l_FilterNPSMax && l_Diffs.Count(x => x.nps <= l_NPSMax) == 0)
                {
                    filterError = $"@{requesterName} this song has no difficulty with a NPS of {l_NPSMax} maximum!";
                    return false;
                }
                if (l_FilterNPSMin && l_FilterNPSMax && l_Diffs.Count(x => x.nps >= l_NPSMin && x.nps <= l_NPSMax) == 0)
                {
                    filterError = $"@{requesterName} this song has no difficulty with a NPS between {l_NPSMin} and {l_NPSMax}!";
                    return false;
                }
            }
            if (l_FilterNJSMin || l_FilterNJSMax)
            {
                int l_NJSMin = CRConfig.Instance.Filters.NJSMinV;
                int l_NJSMax = CRConfig.Instance.Filters.NJSMaxV;

                var l_Diffs = beatMap.SelectMapVersion().diffs;

                if (l_FilterNJSMin && !l_FilterNJSMax && l_Diffs.Count(x => x.njs >= l_NJSMin) == 0)
                {
                    filterError = $"@{requesterName} this song has no difficulty with a NJS of {l_NJSMin} minimum!";
                    return false;
                }
                if (!l_FilterNJSMin && l_FilterNJSMax && l_Diffs.Count(x => x.njs <= l_NJSMax) == 0)
                {
                    filterError = $"@{requesterName} this song has no difficulty with a NJS of {l_NJSMax} maximum!";
                    return false;
                }
                if (l_FilterNJSMin && l_FilterNJSMax && l_Diffs.Count(x => x.njs >= l_NJSMin && x.njs <= l_NJSMax) == 0)
                {
                    filterError = $"@{requesterName} this song has no difficulty with a NJS between {l_NJSMin} and {l_NJSMax}!";
                    return false;
                }
            }
            if (CRConfig.Instance.Filters.DurationMin && (int)beatMap.metadata.duration < (CRConfig.Instance.Filters.DurationMinV * 60))
            {
                filterError = $"@{requesterName} this song is too short ({CRConfig.Instance.Filters.DurationMaxV} minute(s) minimum)!";
                return false;
            }
            if (CRConfig.Instance.Filters.DurationMax && (int)beatMap.metadata.duration > (CRConfig.Instance.Filters.DurationMaxV * 60))
            {
                filterError = $"@{requesterName} this song is too long ({CRConfig.Instance.Filters.DurationMaxV} minute(s) maximum)!";
                return false;
            }

            var l_Date = beatMap.GetUploadTime();

            if (CRConfig.Instance.Filters.VoteMin && l_Vote < (float)Math.Round((double)CRConfig.Instance.Filters.VoteMinV * 100f, 0))
            {
                var l_AgeInDays = (DateTime.Now - l_Date).Days;
                if (!CRConfig.Instance.Filters.IgnoreMinVoteBelow || l_AgeInDays > CRConfig.Instance.Filters.IgnoreMinVoteBelowV)
                {
                    filterError = $"@{requesterName} this song rating is too low ({(float)Math.Round((double)CRConfig.Instance.Filters.VoteMinV * 100f, 0)}% minimum)!";
                    return false;
                }
            }

            DateTime l_MinUploadDate = new DateTime(2018, 1, 1).AddMonths(CRConfig.Instance.Filters.DateMinV);
            if (CRConfig.Instance.Filters.DateMin && l_Date < l_MinUploadDate)
            {
                filterError = $"@{requesterName} this song is too old ({CP_SDK.Misc.Time.MonthNames[l_MinUploadDate.Month - 1]} {l_MinUploadDate.Year} minimum)!";
                return false;
            }
            DateTime l_MaxUploadDate = new DateTime(2018, 1, 1).AddMonths(CRConfig.Instance.Filters.DateMaxV + 1);
            if (CRConfig.Instance.Filters.DateMax && l_Date > l_MaxUploadDate)
            {
                filterError = $"@{requesterName} this song is too recent ({CP_SDK.Misc.Time.MonthNames[l_MinUploadDate.Month - 1]} {l_MinUploadDate.Year} maximum)!";
                return false;
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get title prefix from the requester
        /// </summary>
        /// <param name="requester">Requester</param>
        /// <returns></returns>
        private static string GetTitlePrefixFromRequester(IChatUser requester)
        {
            if (requester == null)
                return string.Empty;

            if (requester.IsModerator || requester.IsBroadcaster)
                return "🗡";
            else if (requester.IsVip)
                return "💎";
            else if (requester.IsSubscriber)
                return "👑";

            return string.Empty;
        }
    }
}
