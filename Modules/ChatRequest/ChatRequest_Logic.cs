using BeatSaberPlus.SDK.Chat.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSaberPlus.Modules.ChatRequest
{
    /// <summary>
    /// Chat request logic handler
    /// </summary>
    internal partial class ChatRequest
    {
        /// <summary>
        /// Song entry
        /// </summary>
        internal class SongEntry
        {
            internal SDK.Game.BeatMaps.MapDetail  BeatMap         = null;
            internal DateTime?                  RequestTime     = null;
            internal string                     RequesterName   = "";
            internal string                     NamePrefix      = "";
            internal string                     Message         = "";
        }

        /// <summary>
        /// Queue
        /// </summary>
        internal List<SongEntry> SongQueue      = new List<SongEntry>();
        /// <summary>
        /// History
        /// </summary>
        internal List<SongEntry> SongHistory    = new List<SongEntry>();
        /// <summary>
        /// Blacklist
        /// </summary>
        internal List<SongEntry> SongBlackList  = new List<SongEntry>();
        /// <summary>
        /// Banned user list
        /// </summary>
        internal List<string> BannedUsers = new List<string>();
        /// <summary>
        /// Banned mapper list
        /// </summary>
        internal List<string> BannedMappers = new List<string>();
        /// <summary>
        /// BSR codes remap lookup dictionary
        /// </summary>
        internal Dictionary<string, string> Remaps = new Dictionary<string, string>();
        /// <summary>
        /// Map allow list
        /// </summary>
        internal List<string> AllowList = new List<string>();
        /// <summary>
        /// Is the queue open
        /// </summary>
        internal bool QueueOpen = false;
        /// <summary>
        /// Total queue duration
        /// </summary>
        internal int QueueDuration { get; private set; } = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Requested this session songs
        /// </summary>
        private ConcurrentBag<string> m_RequestedThisSession = new ConcurrentBag<string>();
        /// <summary>
        /// Last queue command time
        /// </summary>
        private DateTime m_LastQueueCommandTime = DateTime.Now;
        /// <summary>
        /// Link command cache
        /// </summary>
        private IBeatmapLevel m_LastPlayingLevel = null;
        /// <summary>
        /// Link command cache
        /// </summary>
        private string m_LastPlayingLevelResponse = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the queue is changed
        /// </summary>
        /// <param name="p_UpdateSimpleQueueFile">Update simple queue list for OBS integration</param>
        /// <param name="p_UpdateSongList">Update queue and total queue duration</param>
        private void OnQueueChanged(bool p_UpdateSimpleQueueFile = true, bool p_UpdateSongList = true)
        {
            /// Update simple queue file
            if (p_UpdateSimpleQueueFile)
                UpdateSimpleQueueFile();

            if (p_UpdateSongList)
            {
                QueueDuration = 0;
                try
                {
                    lock (SongQueue)
                    {
                        SongQueue.ForEach(x =>
                        {
                            if (!x.BeatMap.Partial)
                                QueueDuration += (int)x.BeatMap.metadata.duration;
                        });
                    }
                }
                catch
                {

                }
            }

            /// Avoid saving during play
            if (SDK.Game.Logic.ActiveScene != SDK.Game.Logic.SceneType.Playing)
            {
                SDK.Unity.MainThreadInvoker.Enqueue(() =>
                {
                    UpdateButton();

                    if (p_UpdateSongList && UI.ManagerMain.CanBeUpdated)
                        UI.ManagerMain.Instance.RebuildSongList();

                    SaveDatabase();
                });
            };
        }
        /// <summary>
        /// When a beatmap get fully loaded
        /// </summary>
        /// <param name="p_Task">Task instance</param>
        private void OnBeatmapPopulated(bool p_Valid, SongEntry p_Entry)
        {
            if (!p_Valid)
            {
                lock (SongQueue) { lock (SongHistory) { lock (SongBlackList) {
                    SongQueue.RemoveAll(    x => x == p_Entry);
                    SongHistory.RemoveAll(  y => y == p_Entry);
                    SongBlackList.RemoveAll(z => z == p_Entry);
                } } }

                SDK.Game.BeatMapsClient.ClearCache(p_Entry.BeatMap.id);

                SDK.Unity.MainThreadInvoker.Enqueue(() =>
                {
                    UpdateButton();

                    if (UI.ManagerMain.CanBeUpdated)
                        UI.ManagerMain.Instance.RebuildSongList();

                    SaveDatabase();
                });
            }

            if (!p_Valid)
                return;

            SDK.Game.BeatMapsClient.Cache(p_Entry.BeatMap);

            /// Update request manager
            OnQueueChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send message to chat
        /// </summary>
        /// <param name="p_Message">Messages to send</param>
        /// <param name="p_ContextUser">Context user</param>
        /// <param name="p_ContextMap">Context map</param>
        internal void SendChatMessage(string p_Message, IChatUser p_ContextUser = null, SDK.Game.BeatMaps.MapDetail p_ContextMap = null)
        {
            if (p_ContextUser != null)
                p_Message = p_Message.Replace("$UserName", p_ContextUser.DisplayName);

            if (p_ContextMap != null)
            {
                p_Message = p_Message.Replace("$BSRKey",            p_ContextMap.id);
                p_Message = p_Message.Replace("$SongName",          p_ContextMap.metadata.songName);
                p_Message = p_Message.Replace("$LevelAuthorName",   p_ContextMap.metadata.levelAuthorName);
            }

            SDK.Chat.Service.BroadcastMessage("! " + p_Message);
        }
        /// <summary>
        /// Is user banned
        /// </summary>
        /// <param name="p_UserName">User name to check</param>
        /// <returns></returns>
        private bool IsUserBanned(string p_UserName)
        {
            lock (BannedUsers)
                return BannedUsers.Where(x => x.ToLower() == p_UserName.ToLower()).Count() != 0;
        }
        /// <summary>
        /// Has privileges
        /// </summary>
        /// <param name="p_User">Source user</param>
        /// <returns></returns>
        private bool HasPower(IChatUser p_User)
        {
            if (p_User is SDK.Chat.Models.Twitch.TwitchUser)
            {
                var l_TwitchUser = p_User as SDK.Chat.Models.Twitch.TwitchUser;
                return l_TwitchUser.IsBroadcaster || (CRConfig.Instance.ModeratorPower && l_TwitchUser.IsModerator);
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Toggle queue status
        /// </summary>
        internal void ToggleQueueStatus()
        {
            if (QueueOpen)
                SendChatMessage("Queue is now closed!");
            else
                SendChatMessage("Queue is now open!");

            QueueOpen = !QueueOpen;

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                CRConfig.Instance.QueueOpen = QueueOpen;
                CRConfig.Instance.Save();

                UpdateSimpleQueueFile();

                if (UI.ManagerLeft.CanBeUpdated)
                    UI.ManagerLeft.Instance.UpdateQueueStatus();
            });
        }
        /// <summary>
        /// Re-Enqueue a song by play or skip
        /// </summary>
        /// <param name="p_Entry">Song to dequeue</param>
        internal void ReEnqueueSong(SongEntry p_Entry)
        {
            if (p_Entry == null)
                return;

            lock (SongQueue)
            {
                lock (SongHistory)
                {
                    /// Remove from history
                    if (SongHistory.Contains(p_Entry))
                        SongHistory.Remove(p_Entry);

                    /// Move at top of queue
                    SongQueue.RemoveAll(x => x.BeatMap.id == p_Entry.BeatMap.id);
                    SongQueue.Insert(0, p_Entry);
                }
            }

            /// Update request manager
            OnQueueChanged();
        }
        /// <summary>
        /// Dequeue a song by play or skip
        /// </summary>
        /// <param name="p_Entry">Song to dequeue</param>
        internal void DequeueSong(SongEntry p_Entry, bool p_ChatNotify)
        {
            if (p_Entry == null)
                return;

            lock (SongQueue)
            {
                lock (SongHistory)
                {
                    lock (SongBlackList)
                    {
                        /// Remove from queue
                        if (SongQueue.Contains(p_Entry))
                            SongQueue.Remove(p_Entry);

                        /// Move at top of history
                        SongHistory.RemoveAll(x => x.BeatMap.id == p_Entry.BeatMap.id);
                        SongHistory.Insert(0, p_Entry);

                        /// Reduce history size
                        while (SongHistory.Count > CRConfig.Instance.HistorySize)
                        {
                            var l_ToRemove = SongHistory.ElementAt(SongHistory.Count - 1);

                            /// Clear cache
                            if (SongBlackList.Count(x => x.BeatMap.id == l_ToRemove.BeatMap.id) == 0)
                                SDK.Game.BeatMapsClient.ClearCache(l_ToRemove.BeatMap.id);

                            SongHistory.RemoveAt(SongHistory.Count - 1);
                        }
                    }
                }
            }

            /// Update request manager
            OnQueueChanged();

            if (p_ChatNotify)
            {
                float l_Vote = (float)Math.Round((double)p_Entry.BeatMap.stats.score * 100f, 0);
                SendChatMessage($"{p_Entry.BeatMap.metadata.songName} / {p_Entry.BeatMap.metadata.levelAuthorName} {l_Vote}% (bsr {p_Entry.BeatMap.id}) requested by @{p_Entry.RequesterName} is next!");
            }
        }
        /// <summary>
        /// Clear queue
        /// </summary>
        internal void ClearQueue()
        {
            lock (SongQueue)
            {
                SongQueue.Clear();
            }

            /// Update request manager
            OnQueueChanged();
        }
        /// <summary>
        /// Blacklist song
        /// </summary>
        /// <param name="p_Entry">Song to blacklist</param>
        internal void BlacklistSong(SongEntry p_Entry)
        {
            if (p_Entry == null)
                return;

            lock (SongQueue) { lock (SongHistory) { lock (SongBlackList)
            {
                /// Remove from queue
                if (SongQueue.Contains(p_Entry))
                    SongQueue.Remove(p_Entry);

                /// Remove from history
                SongHistory.RemoveAll(x => x.BeatMap.id == p_Entry.BeatMap.id);

                /// Add to blacklist
                SongBlackList.RemoveAll(x => x.BeatMap.id == p_Entry.BeatMap.id);
                SongBlackList.Insert(0, p_Entry);
            } } }

            /// Update request manager
            OnQueueChanged(false);
        }
        /// <summary>
        /// UnBlacklist song
        /// </summary>
        /// <param name="p_Entry">Song to blacklist</param>
        internal void UnBlacklistSong(SongEntry p_Entry)
        {
            if (p_Entry == null)
                return;

            lock (SongQueue) { lock (SongHistory) { lock (SongBlackList)
            {
                /// Remove from blacklist
                if (SongBlackList.Contains(p_Entry))
                    SongBlackList.Remove(p_Entry);

                /// Move at top of history
                SongHistory.RemoveAll(x => x.BeatMap.id == p_Entry.BeatMap.id);
                SongHistory.Insert(0, p_Entry);

                /// Reduce history size
                while (SongHistory.Count > CRConfig.Instance.HistorySize)
                    SongHistory.RemoveAt(SongHistory.Count - 1);
            } } }

            /// Update request manager
            OnQueueChanged(false);
        }
        /// <summary>
        /// Reset blacklist
        /// </summary>
        internal void ResetBlacklist()
        {
            lock (SongHistory) { lock (SongBlackList)
            {
                /// Add all blacklisted songs to history
                SongBlackList.ForEach(l_BlacklistedSong =>
                {
                    /// Move at top of history
                    SongHistory.RemoveAll(x => x.BeatMap.id == l_BlacklistedSong.BeatMap.id);
                    SongHistory.Insert(0, l_BlacklistedSong);
                });

                /// Remove all blacklisted songs
                SongBlackList.Clear();
            } }

            /// Update request manager
            OnQueueChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Filter a beatmap
        /// </summary>
        /// <param name="p_BeatMap">Beatmap to filter</param>
        /// <param name="p_SenderName">Requester name</param>
        /// <param name="p_Reply">Output reply</param>
        /// <returns></returns>
        private bool FilterBeatMap(SDK.Game.BeatMaps.MapDetail p_BeatMap, string p_SenderName, out string p_Reply)
        {
            p_Reply = "";

            bool    l_FilterNPSMin  = CRConfig.Instance.Filters.NPSMin;
            bool    l_FilterNPSMax  = CRConfig.Instance.Filters.NPSMax;
            bool    l_FilterNJSMin  = CRConfig.Instance.Filters.NJSMin;
            bool    l_FilterNJSMax  = CRConfig.Instance.Filters.NJSMax;
            float   l_Vote          = (float)Math.Round((double)p_BeatMap.stats.score * 100f, 0);

            /// Thanks beatsaver, fix filters for maps without votes
            if ((p_BeatMap.stats.downvotes + p_BeatMap.stats.upvotes) == 0)
                l_Vote = 50f;

            if (CRConfig.Instance.Filters.NoBeatSage && p_BeatMap.automapper)
            {
                p_Reply = $"@{p_SenderName} BeatSage map are not allowed!";
                return false;
            }
            if (l_FilterNPSMin || l_FilterNPSMax)
            {
                int l_NPSMin = CRConfig.Instance.Filters.NPSMinV;
                int l_NPSMax = CRConfig.Instance.Filters.NPSMaxV;

                var l_Diffs = p_BeatMap.SelectMapVersion().diffs;

                if (l_FilterNPSMin && !l_FilterNPSMax && l_Diffs.Count(x => x.nps >= l_NPSMin) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NPS of {l_NPSMin} minimum!";
                    return false;
                }
                if (!l_FilterNPSMin && l_FilterNPSMax && l_Diffs.Count(x => x.nps <= l_NPSMax) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NPS of {l_NPSMax} maximum!";
                    return false;
                }
                if (l_FilterNPSMin && l_FilterNPSMax && l_Diffs.Count(x => x.nps >= l_NPSMin && x.nps <= l_NPSMax) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NPS between {l_NPSMin} and {l_NPSMax}!";
                    return false;
                }
            }
            if (l_FilterNJSMin || l_FilterNJSMax)
            {
                int l_NJSMin = CRConfig.Instance.Filters.NJSMinV;
                int l_NJSMax = CRConfig.Instance.Filters.NJSMaxV;

                var l_Diffs = p_BeatMap.SelectMapVersion().diffs;

                if (l_FilterNJSMin && !l_FilterNJSMax && l_Diffs.Count(x => x.njs >= l_NJSMin) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NJS of {l_NJSMin} minimum!";
                    return false;
                }
                if (!l_FilterNJSMin && l_FilterNJSMax && l_Diffs.Count(x => x.njs <= l_NJSMax) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NJS of {l_NJSMax} maximum!";
                    return false;
                }
                if (l_FilterNJSMin && l_FilterNJSMax && l_Diffs.Count(x => x.njs >= l_NJSMin && x.njs <= l_NJSMax) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NJS between {l_NJSMin} and {l_NJSMax}!";
                    return false;
                }
            }
            if (CRConfig.Instance.Filters.DurationMax && (int)p_BeatMap.metadata.duration > (CRConfig.Instance.Filters.DurationMaxV * 60))
            {
                p_Reply = $"@{p_SenderName} this song is too long ({CRConfig.Instance.Filters.DurationMaxV} minute(s) maximum)!";
                return false;
            }
            if (CRConfig.Instance.Filters.VoteMin && l_Vote < (float)Math.Round((double)CRConfig.Instance.Filters.VoteMinV * 100f, 0))
            {
                p_Reply = $"@{p_SenderName} this song rating is too low ({(float)Math.Round((double)CRConfig.Instance.Filters.VoteMinV * 100f, 0)}% minimum)!";
                return false;
            }

            var l_Date = p_BeatMap.GetUploadTime();

            DateTime l_MinUploadDate = new DateTime(2018, 1, 1).AddMonths(CRConfig.Instance.Filters.DateMinV);
            if (CRConfig.Instance.Filters.DateMin && l_Date < l_MinUploadDate)
            {
                string[] s_Months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                p_Reply = $"@{p_SenderName} this song is too old ({s_Months[l_MinUploadDate.Month - 1]} {l_MinUploadDate.Year} minimum)!";
                return false;
            }
            DateTime l_MaxUploadDate = new DateTime(2018, 1, 1).AddMonths(CRConfig.Instance.Filters.DateMaxV + 1);
            if (CRConfig.Instance.Filters.DateMax && l_Date > l_MaxUploadDate)
            {
                string[] s_Months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                p_Reply = $"@{p_SenderName} this song is too recent ({s_Months[l_MinUploadDate.Month - 1]} {l_MinUploadDate.Year} maximum)!";
                return false;
            }

            return true;
        }
        /// <summary>
        /// Is hex only string
        /// </summary>
        /// <param name="p_Value">Value to test</param>
        /// <returns></returns>
        private bool OnlyHexInString(string p_Value)
        {
            // For C-style hex notation (0xFF) you can use @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z"
            return System.Text.RegularExpressions.Regex.IsMatch(p_Value, @"\A\b[0-9a-fA-F]+\b\Z");
        }
    }
}
