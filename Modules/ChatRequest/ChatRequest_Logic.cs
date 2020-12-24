using BeatSaberPlusChatCore.Interfaces;
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
            internal BeatSaverSharp.Beatmap BeatMap         = null;
            internal DateTime?              RequestTime     = null;
            internal string                 RequesterName   = "";
            internal string                 NamePrefix      = "";
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
        /// Is the queue open
        /// </summary>
        internal bool QueueOpen = false;

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
        /// When the provider for beat saver changed
        /// </summary>
        internal void OnBeatSaverProviderChanged()
        {
            SDK.Game.BeatSaver.UseAlternativeServer(m_BeatSaver, (Config.Online.Enabled && Config.Online.UseBSPCustomMapsServer) ? SDK.Game.BeatSaver.Server.BeatSaberPlus : SDK.Game.BeatSaver.Server.BeatSaver);

            lock (SongQueue)
            {
                SongQueue.ForEach(x =>
                {
                    x.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, x.BeatMap.Key);
                    x.BeatMap.Populate().ContinueWith(xx => OnBeatmapPopulated(xx, x));
                });
            }
            lock (SongHistory)
            {
                SongHistory.ForEach(y =>
                {
                    y.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, y.BeatMap.Key);
                    y.BeatMap.Populate().ContinueWith(yy => OnBeatmapPopulated(yy, y));
                });
            }
            lock (SongBlackList)
            {
                SongBlackList.ForEach(z =>
                {
                    z.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, z.BeatMap.Key);
                    z.BeatMap.Populate().ContinueWith(zz => OnBeatmapPopulated(zz, z));
                });
            }
        }
        /// <summary>
        /// When the queue is changed
        /// </summary>
        private void OnQueueChanged(bool p_UpdateSimpleQueueFile = true, bool p_UpdateSongList = true)
        {
            /// Update simple queue file
            if (p_UpdateSimpleQueueFile)
                UpdateSimpleQueueFile();

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
        private void OnBeatmapPopulated(Task p_Task, SongEntry p_Entry)
        {
            if (p_Task.Status != TaskStatus.RanToCompletion)
                return;

            if (p_Entry.BeatMap.Partial)
            {
                lock (SongQueue) { lock (SongHistory) { lock (SongBlackList) {
                    SongQueue.RemoveAll(    x => x == p_Entry);
                    SongHistory.RemoveAll(  y => y == p_Entry);
                    SongBlackList.RemoveAll(z => z == p_Entry);
                } } }
            }

            /// Update request manager
            OnQueueChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send message to chat
        /// </summary>
        /// <param name="p_Message">Messages to send</param>
        internal void SendChatMessage(string p_Message)
        {
            SDK.Chat.Service.BroadcastMessage("! " + p_Message);
        }
        /// <summary>
        /// Has privileges
        /// </summary>
        /// <param name="p_User">Source user</param>
        /// <returns></returns>
        private bool HasPower(IChatUser p_User)
        {
            if (p_User is BeatSaberPlusChatCore.Models.Twitch.TwitchUser)
            {
                var l_TwitchUser = p_User as BeatSaberPlusChatCore.Models.Twitch.TwitchUser;
                return l_TwitchUser.IsBroadcaster || (Config.ChatRequest.ModeratorPower && l_TwitchUser.IsModerator);
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
                Config.ChatRequest.QueueOpen = QueueOpen;

                if (UI.ManagerLeft.CanBeUpdated)
                    UI.ManagerLeft.Instance.UpdateQueueStatus();
            });
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
                    /// Remove from queue
                    if (SongQueue.Contains(p_Entry))
                        SongQueue.Remove(p_Entry);

                    /// Move at top of history
                    SongHistory.RemoveAll(x => x.BeatMap.Hash == p_Entry.BeatMap.Hash);
                    SongHistory.Insert(0, p_Entry);

                    /// Reduce history size
                    while (SongHistory.Count > Config.ChatRequest.HistorySize)
                        SongHistory.RemoveAt(SongHistory.Count - 1);
                }
            }

            /// Update request manager
            OnQueueChanged();

            if (p_ChatNotify)
            {
                float l_Vote = (float)Math.Round((double)p_Entry.BeatMap.Stats.Rating * 100f, 0);
                SendChatMessage($"{p_Entry.BeatMap.Metadata.SongName} / {p_Entry.BeatMap.Metadata.LevelAuthorName} {l_Vote}% (bsr {p_Entry.BeatMap.Key}) requested by @{p_Entry.RequesterName} is next!");
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
                SongHistory.RemoveAll(x => x.BeatMap.Hash == p_Entry.BeatMap.Hash);

                /// Add to blacklist
                SongBlackList.RemoveAll(x => x.BeatMap.Hash == p_Entry.BeatMap.Hash);
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
                SongHistory.RemoveAll(x => x.BeatMap.Hash == p_Entry.BeatMap.Hash);
                SongHistory.Insert(0, p_Entry);

                /// Reduce history size
                while (SongHistory.Count > Config.ChatRequest.HistorySize)
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
                    SongHistory.RemoveAll(x => x.BeatMap.Hash == l_BlacklistedSong.BeatMap.Hash);
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
        private bool FilterBeatMap(BeatSaverSharp.Beatmap p_BeatMap, string p_SenderName, out string p_Reply)
        {
            p_Reply = "";

            bool    l_FilterNPSMin  = Config.ChatRequest.NPSMin;
            bool    l_FilterNPSMax  = Config.ChatRequest.NPSMax;
            bool    l_FilterNJSMin  = Config.ChatRequest.NJSMin;
            bool    l_FilterNJSMax  = Config.ChatRequest.NJSMax;
            float   l_Vote          = (float)Math.Round((double)p_BeatMap.Stats.Rating * 100f, 0);

            if (Config.ChatRequest.NoBeatSage && p_BeatMap.Metadata.Automapper != null && p_BeatMap.Metadata.Automapper != "")
            {
                p_Reply = $"@{p_SenderName} BeatSage map are not allowed!";
                return false;
            }
            if (l_FilterNPSMin || l_FilterNPSMax)
            {
                int l_NPSMin = Config.ChatRequest.NPSMinV;
                int l_NPSMax = Config.ChatRequest.NPSMaxV;

                List<BeatSaverSharp.BeatmapCharacteristicDifficulty> l_Diffs = new List<BeatSaverSharp.BeatmapCharacteristicDifficulty>();
                foreach (var l_Chara in p_BeatMap.Metadata.Characteristics)
                    l_Diffs.AddRange(l_Chara.Difficulties.Where(x => x.Value.HasValue).Select(x => x.Value.Value));

                if (l_FilterNPSMin && !l_FilterNPSMax && l_Diffs.Count(x => ((float)x.Notes / (float)x.Length) >= l_NPSMin) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NPS of {l_NPSMin} minimum!";
                    return false;
                }
                if (!l_FilterNPSMin && l_FilterNPSMax && l_Diffs.Count(x => ((float)x.Notes / (float)x.Length) <= l_NPSMax) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NPS of {l_NPSMax} maximum!";
                    return false;
                }
                if (l_FilterNPSMin && l_FilterNPSMax && l_Diffs.Count(x => ((float)x.Notes / (float)x.Length) >= l_NPSMin && ((float)x.Notes / (float)x.Length) <= l_NPSMax) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NPS between {l_NPSMin} and {l_NPSMax}!";
                    return false;
                }
            }
            if (l_FilterNJSMin || l_FilterNJSMax)
            {
                int l_NJSMin = Config.ChatRequest.NJSMinV;
                int l_NJSMax = Config.ChatRequest.NJSMaxV;

                List<BeatSaverSharp.BeatmapCharacteristicDifficulty> l_Diffs = new List<BeatSaverSharp.BeatmapCharacteristicDifficulty>();
                foreach (var l_Chara in p_BeatMap.Metadata.Characteristics)
                    l_Diffs.AddRange(l_Chara.Difficulties.Where(x => x.Value.HasValue).Select(x => x.Value.Value));

                if (l_FilterNJSMin && !l_FilterNJSMax && l_Diffs.Count(x => x.NoteJumpSpeed >= l_NJSMin) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NJS of {l_NJSMin} minimum!";
                    return false;
                }
                if (!l_FilterNJSMin && l_FilterNJSMax && l_Diffs.Count(x => x.NoteJumpSpeed <= l_NJSMax) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NJS of {l_NJSMax} maximum!";
                    return false;
                }
                if (l_FilterNJSMin && l_FilterNJSMax && l_Diffs.Count(x => x.NoteJumpSpeed >= l_NJSMin && x.NoteJumpSpeed <= l_NJSMax) == 0)
                {
                    p_Reply = $"@{p_SenderName} this song has no difficulty with a NJS between {l_NJSMin} and {l_NJSMax}!";
                    return false;
                }
            }
            if (Config.ChatRequest.DurationMax && (int)p_BeatMap.Metadata.Duration > (Config.ChatRequest.DurationMaxV * 60))
            {
                p_Reply = $"@{p_SenderName} this song is too long ({Config.ChatRequest.DurationMaxV} minute(s) maximum)!";
                return false;
            }
            if (Config.ChatRequest.VoteMin && l_Vote < Config.ChatRequest.VoteMinV)
            {
                p_Reply = $"@{p_SenderName} this song rating is too low ({Config.ChatRequest.VoteMinV}% minimum)!";
                return false;
            }
            DateTime l_MinUploadDate = new DateTime(2018, 1, 1).AddMonths(Config.ChatRequest.DateMinV);
            if (Config.ChatRequest.DateMin && p_BeatMap.Uploaded < l_MinUploadDate)
            {
                string[] s_Months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                p_Reply = $"@{p_SenderName} this song is too old ({s_Months[l_MinUploadDate.Month - 1]} {l_MinUploadDate.Year} minimum)!";
                return false;
            }
            DateTime l_MaxUploadDate = new DateTime(2018, 1, 1).AddMonths(Config.ChatRequest.DateMaxV + 1);
            if (Config.ChatRequest.DateMax && p_BeatMap.Uploaded > l_MaxUploadDate)
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
