using BeatSaberPlus.Utils;
using BeatSaberPlusChatCore.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.Plugins.ChatRequest
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
            internal Sprite                 Cover           = null;
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
            m_BeatSaver.UseBeatSaberPlusCustomMapsServer(Config.Online.Enabled && Config.Online.UseBSPCustomMapsServer);

            lock (SongQueue)
            {
                SongQueue.ForEach(x =>
                {
                    x.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, x.BeatMap.Key);
                    x.BeatMap.Populate().ContinueWith(y => OnBeatmapPopulated(y, x));
                });
            }
            lock (SongHistory)
            {
                SongHistory.ForEach(x =>
                {
                    x.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, x.BeatMap.Key);
                    x.BeatMap.Populate().ContinueWith(y => OnBeatmapPopulated(y, x));
                });
            }
            lock (SongBlackList)
            {
                SongBlackList.ForEach(x =>
                {
                    x.BeatMap = new BeatSaverSharp.Beatmap(m_BeatSaver, x.BeatMap.Key);
                    x.BeatMap.Populate().ContinueWith(y => OnBeatmapPopulated(y, x));
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send message to chat
        /// </summary>
        /// <param name="p_Message">Messages to send</param>
        internal void SendChatMessage(string p_Message)
        {
            Utils.ChatService.BroadcastMessage("! " + p_Message);
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

            HMMainThreadDispatcher.instance.Enqueue(() =>
            {
                Config.ChatRequest.QueueOpen = QueueOpen;

                if (Instance.m_ManagerViewFlowCoordinator != null && Instance.m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                    Instance.m_ManagerViewFlowCoordinator.LeftView.UpdateQueueStatus();
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

            UpdateSimpleQueueFile();

            /// Avoid saving during play
            if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
            {
                /// Update UI
                HMMainThreadDispatcher.instance.Enqueue(() => {
                    UpdateButton();
                });
            }

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

            /// Avoid saving during play
            if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
            {
                /// Update UI
                HMMainThreadDispatcher.instance.Enqueue(() => {
                    UpdateButton();

                    if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                        m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                    SaveDatabase();
                });
            }
        }
        /// <summary>
        /// Blacklist song
        /// </summary>
        /// <param name="p_Entry">Song to blacklist</param>
        internal void BlacklistSong(SongEntry p_Entry)
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

                        /// Remove from history
                        SongHistory.RemoveAll(x => x.BeatMap.Hash == p_Entry.BeatMap.Hash);

                        /// Add to blacklist
                        SongBlackList.RemoveAll(x => x.BeatMap.Hash == p_Entry.BeatMap.Hash);
                        SongBlackList.Insert(0, p_Entry);
                    }
                }
            }

            UpdateSimpleQueueFile();

            /// Avoid saving during play
            if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
            {
                /// Update UI
                HMMainThreadDispatcher.instance.Enqueue(() => {
                    UpdateButton();

                    if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                        m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                    /// Save database
                    SaveDatabase();
                });
            }
        }
        /// <summary>
        /// UnBlacklist song
        /// </summary>
        /// <param name="p_Entry">Song to blacklist</param>
        internal void UnBlacklistSong(SongEntry p_Entry)
        {
            if (p_Entry == null)
                return;

            lock (SongQueue)
            {
                lock (SongHistory)
                {
                    lock (SongBlackList)
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
                    }
                }
            }

            /// Avoid saving during play
            if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
            {
                /// Update UI
                HMMainThreadDispatcher.instance.Enqueue(() => {
                    if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                        m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                    /// Save database
                    SaveDatabase();
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">ID of the message</param>
        private void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_Service, IChatMessage p_Message)
        {
            if (p_Message.Message.Length < 2 || p_Message.Message[0] != '!')
                return;

            string l_LMessage = p_Message.Message.ToLower();

            if (l_LMessage.StartsWith("!bsr "))
                Command_BSR(p_Service, p_Message, p_Message.Message.Substring("!bsr ".Length).Trim(), false, false);
            else if(l_LMessage.StartsWith("!modadd "))
                Command_BSR(p_Service, p_Message, p_Message.Message.Substring("!modadd ".Length).Trim(), true, false);
            else if(l_LMessage.StartsWith("!att "))
                Command_BSR(p_Service, p_Message, p_Message.Message.Substring("!att ".Length).Trim(), true, true);
            else if (l_LMessage.StartsWith("!mtt "))
                Command_MoveToTop(p_Service, p_Message, p_Message.Message.Substring("!mtt ".Length).Trim());
            else if (l_LMessage.StartsWith("!block "))
                Command_Block(p_Service, p_Message, p_Message.Message.Substring("!block ".Length).Trim());
            else if (l_LMessage.StartsWith("!bsrhelp"))
                SendChatMessage($"@{p_Message.Sender.UserName} To request a song, go to https://beatsaver.com/search and find a song, Click on \"Copy !bsr\" button and paste this on the stream and I'll play it soon!.");
            else if (l_LMessage.StartsWith("!oops") || l_LMessage.StartsWith("!wrongsong") || l_LMessage.StartsWith("!wrong"))
                Command_Wrong(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!remove"))
                Command_Remove(p_Service, p_Message, p_Message.Message.Substring("!remove ".Length).Trim());
            else if (l_LMessage.StartsWith("!queue"))
                Command_Queue(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!queuestatus"))
                Command_QueueStatus(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!open"))
                Command_Open(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!close"))
                Command_Close(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!link"))
                Command_Link(p_Service, p_Message);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle BSR command
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_ID">ID of the BSR</param>
        private void Command_BSR(IChatService p_Service, IChatMessage p_Message, string p_ID, bool p_ModeratorAddCommand, bool p_ModeratorAddToTop)
        {
            bool l_IsModerator = HasPower(p_Message.Sender);

            if (p_ModeratorAddCommand && !l_IsModerator)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            if (!QueueOpen && !(l_IsModerator && p_ModeratorAddCommand))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} the queue is closed!");
                return;
            }

            string l_Key = p_ID.ToLower();

            if (!OnlyHexInString(l_Key))
            {
                _ = m_BeatSaver.Search(l_Key).ContinueWith((p_SearchTaskResult) =>
                {
                    if (p_SearchTaskResult.Result == null || p_SearchTaskResult.Result.Docs.Count == 0)
                    {
                        SendChatMessage($"@{p_Message.Sender.UserName} your search \"{l_Key}\" produced 0 results!");
                        return;
                    }

                    var l_Docs = p_SearchTaskResult.Result.Docs;
                    string l_Reply = $"@{p_Message.Sender.UserName} your search \"{l_Key}\" produced {l_Docs.Count} results : ";

                    int l_I = 0;
                    for (; l_I < l_Docs.Count && l_I < 4; ++l_I)
                    {
                        if (l_I != 0)
                            l_Reply += ", ";

                        l_Reply += " (!bsr " + l_Docs[l_I].Key + ") " + l_Docs[l_I].Name;
                    }

                    if (l_I < l_Docs.Count)
                        l_Reply += "...";

                    SendChatMessage(l_Reply);
                });

                return;
            }

            var l_UserRequestCount = 0;

            /// Check if blacklisted
            if (!(l_IsModerator && p_ModeratorAddCommand))
            {
                lock (SongBlackList)
                {
                    var l_BeatMap = SongBlackList.Where(x => x.BeatMap.Key.ToLower() == l_Key).FirstOrDefault();
                    if (l_BeatMap != null)
                    {
                        SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_BeatMap.BeatMap.Key}) {l_BeatMap.BeatMap.Metadata.SongName} / {l_BeatMap.BeatMap.Metadata.LevelAuthorName} is blacklisted!");
                        return;
                    }
                }
            }

            /// Check if already in queue
            lock (SongQueue)
            {
                var l_BeatMap = SongQueue.Where(x => x.BeatMap.Key.ToLower() == l_Key).FirstOrDefault();
                if (l_BeatMap != null)
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_BeatMap.BeatMap.Key}) {l_BeatMap.BeatMap.Metadata.SongName} / {l_BeatMap.BeatMap.Metadata.LevelAuthorName} is already in queue!");
                    return;
                }

                l_UserRequestCount = SongQueue.Where(x => x.RequesterName == p_Message.Sender.UserName).Count();
            }

            string l_NamePrefix = "";

            /// Handle request limits
            if (p_Service is BeatSaberPlusChatCore.Services.Twitch.TwitchService)
            {
                var l_TwitchUser = p_Message.Sender as BeatSaberPlusChatCore.Models.Twitch.TwitchUser;

                (int, string) l_Limit = (Config.ChatRequest.UserMaxRequest, "Users");

                if (l_TwitchUser.IsVip && !l_TwitchUser.IsSubscriber) l_Limit = (l_Limit.Item1 + Config.ChatRequest.VIPBonusRequest,                                             "VIPs");
                if (l_TwitchUser.IsSubscriber && !l_TwitchUser.IsVip) l_Limit = (l_Limit.Item1 + Config.ChatRequest.SubscriberBonusRequest,                                      "Subscribers");
                if (l_TwitchUser.IsSubscriber &&  l_TwitchUser.IsVip) l_Limit = (l_Limit.Item1 + Config.ChatRequest.VIPBonusRequest + Config.ChatRequest.SubscriberBonusRequest, "VIP Subscribers");
                if (l_TwitchUser.IsModerator)                         l_Limit = (1000,                                                                                           "Moderators");

                if (l_TwitchUser.IsModerator)
                    l_NamePrefix = "🗡";
                else if (l_TwitchUser.IsVip)
                    l_NamePrefix = "💎";
                else if (l_TwitchUser.IsSubscriber)
                    l_NamePrefix = "👑";

                if (l_UserRequestCount >= l_Limit.Item1)
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} You already have {l_UserRequestCount} on the queue. {l_Limit.Item2} are limited to {l_Limit.Item1} request(s).");
                    return;
                }
            }

            /// Check if already requested
            if (!(l_IsModerator && p_ModeratorAddCommand) && m_RequestedThisSession.Contains(l_Key))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} this song was already requested this session!");
                return;
            }

            /// Fetch beatmap
            _ = m_BeatSaver.Key(l_Key).ContinueWith(p_SongTaskResult =>
            {
                try
                {
                    string l_Reply = "@" + p_Message.Sender.UserName + " map " + l_Key + " not found.";
                    if (p_SongTaskResult.Result != null && ((l_IsModerator && p_ModeratorAddCommand) || FilterBeatMap(p_SongTaskResult.Result, p_Message.Sender.UserName, out l_Reply)))
                    {
                        var     l_BeatMap   = p_SongTaskResult.Result;
                        float   l_Vote      = (float)Math.Round((double)l_BeatMap.Stats.Rating * 100f, 0);

                        if ((l_IsModerator && p_ModeratorAddCommand) || !m_RequestedThisSession.Contains(l_Key))
                        {
                            m_RequestedThisSession.Add(l_Key.ToLower());

                            var l_Entry = new SongEntry()
                            {
                                BeatMap         = l_BeatMap,
                                RequesterName   = p_Message.Sender.UserName,
                                NamePrefix      = l_NamePrefix
                            };

                            lock (SongQueue)
                            {
                                if (l_IsModerator && p_ModeratorAddToTop)
                                    SongQueue.Insert(0, l_Entry);
                                else
                                    SongQueue.Add(l_Entry);
                            }

                            UpdateSimpleQueueFile();

                            /// Avoid saving during play
                            if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
                            {
                                HMMainThreadDispatcher.instance.Enqueue(() =>
                                {
                                    UpdateButton();

                                    if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                                        m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                                    SaveDatabase();
                                });
                            };

                            l_Reply = $"(bsr {l_BeatMap.Key}) {l_BeatMap.Metadata.SongName} / {l_BeatMap.Metadata.LevelAuthorName} {l_Vote}% requested by @{p_Message.Sender.UserName} added to queue.";
                        }
                        else
                            l_Reply = $"@{p_Message.Sender.UserName} this song was already requested this session!";
                    }

                    SendChatMessage(l_Reply);
                }
                catch (System.Exception p_Exception)
                {
                    Logger.Instance.Error("Command_BSR");
                    Logger.Instance.Error(p_Exception);
                }
            });
        }
        /// <summary>
        /// Move song to top
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_ID">ID of the BSR</param>
        private void Command_MoveToTop(IChatService p_Service, IChatMessage p_Message, string p_ID)
        {
            bool l_IsModerator = HasPower(p_Message.Sender);
            if (!l_IsModerator)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            string l_Key = p_ID.ToLower();

            lock (SongQueue)
            {
                var l_BeatMap = SongQueue.Where(x => x.BeatMap.Key.ToLower() == l_Key).FirstOrDefault();
                if (l_BeatMap != null)
                {
                    SongQueue.Remove(l_BeatMap);
                    SongQueue.Insert(0, l_BeatMap);

                    SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_BeatMap.BeatMap.Key}) {l_BeatMap.BeatMap.Metadata.SongName} / {l_BeatMap.BeatMap.Metadata.LevelAuthorName} is now on top of queue!");

                    /// Avoid saving during play
                    if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
                    {
                        HMMainThreadDispatcher.instance.Enqueue(() =>
                        {
                            UpdateButton();

                            if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                                m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                            SaveDatabase();
                        });
                    };

                    return;
                }

                SendChatMessage($"@{p_Message.Sender.UserName} No song in queue found with the key \"{l_Key}\"!");
            }

            UpdateSimpleQueueFile();
        }
        /// <summary>
        /// Block a song
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_ID">ID of the BSR</param>
        private void Command_Block(IChatService p_Service, IChatMessage p_Message, string p_ID)
        {
            bool l_IsModerator = HasPower(p_Message.Sender);
            if (!l_IsModerator)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            string l_Key = p_ID.ToLower();

            if (!OnlyHexInString(l_Key))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} Invalid key!");
                return;
            }

            lock (SongBlackList)
            {
                var l_BeatMap = SongBlackList.Where(x => x.BeatMap.Key.ToLower() == l_Key).FirstOrDefault();
                if (l_BeatMap != null)
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_BeatMap.BeatMap.Key}) {l_BeatMap.BeatMap.Metadata.SongName} / {l_BeatMap.BeatMap.Metadata.LevelAuthorName} is already blacklisted!");
                    return;
                }
            }

            /// Fetch beatmap
            _ = m_BeatSaver.Key(l_Key).ContinueWith(p_SongTaskResult =>
            {
                try
                {
                    string l_Reply = "@" + p_Message.Sender.UserName + " map " + l_Key + " not found.";
                    if (p_SongTaskResult.Result != null)
                    {
                        var l_BeatMap = p_SongTaskResult.Result;
                        l_Reply = $"@{p_Message.Sender.UserName} (bsr {l_BeatMap.Key}) {l_BeatMap.Metadata.SongName} / {l_BeatMap.Metadata.LevelAuthorName} is now blacklisted!";

                        lock (SongQueue) { lock (SongHistory) { lock (SongBlackList) {
                            SongQueue.RemoveAll(x => x.BeatMap.Key   == l_BeatMap.Key);
                            SongHistory.RemoveAll(x => x.BeatMap.Key == l_BeatMap.Key);

                            /// Add to blacklist
                            SongBlackList.RemoveAll(x => x.BeatMap.Hash == l_BeatMap.Hash);
                            SongBlackList.Insert(0, new SongEntry() { BeatMap = l_BeatMap, NamePrefix = "🗡", RequesterName = p_Message.Sender.DisplayName });
                        } } }

                        UpdateSimpleQueueFile();

                        /// Avoid saving during play
                        if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
                        {
                            /// Update UI
                            HMMainThreadDispatcher.instance.Enqueue(() => {
                                UpdateButton();

                                if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                                    m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                                /// Save database
                                SaveDatabase();
                            });
                        }
                    }

                    SendChatMessage(l_Reply);
                }
                catch (System.Exception p_Exception)
                {
                    Logger.Instance.Error("Command_Block");
                    Logger.Instance.Error(p_Exception);
                }
            });
        }
        /// <summary>
        /// Command wrong
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void Command_Wrong(IChatService p_Service, IChatMessage p_Message)
        {
            SongEntry l_SongEntry = null;
            lock (SongQueue)
            {
                l_SongEntry = SongQueue.Where(x => x.RequesterName == p_Message.Sender.UserName).LastOrDefault();
            }

            if (l_SongEntry != null)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.Key}) {l_SongEntry.BeatMap.Metadata.SongName} / {l_SongEntry.BeatMap.Metadata.LevelAuthorName} is removed from queue!");

                lock (SongQueue)
                {
                    SongQueue.Remove(l_SongEntry);
                }

                /// Avoid saving during play
                if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
                {
                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        UpdateButton();

                        if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                            m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                        SaveDatabase();
                    });
                };

                UpdateSimpleQueueFile();
            }
            else
                SendChatMessage($"@{p_Message.Sender.UserName} You have no song in queue!");
        }
        /// <summary>
        /// Command remove
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void Command_Remove(IChatService p_Service, IChatMessage p_Message, string p_ID)
        {
            bool l_IsModerator = HasPower(p_Message.Sender);
            if (!l_IsModerator)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            string l_Key = p_ID.ToLower();

            SongEntry l_SongEntry = null;
            lock (SongQueue)
            {
                l_SongEntry = SongQueue.Where(x => x.BeatMap.Key.ToLower() == l_Key).FirstOrDefault();
            }

            if (l_SongEntry != null)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.Key}) {l_SongEntry.BeatMap.Metadata.SongName} / {l_SongEntry.BeatMap.Metadata.LevelAuthorName} is removed from queue!");

                lock (SongQueue)
                {
                    SongQueue.Remove(l_SongEntry);
                }

                /// Avoid saving during play
                if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
                {
                    HMMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        UpdateButton();

                        if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                            m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                        SaveDatabase();
                    });
                };

                UpdateSimpleQueueFile();
            }
            else
                SendChatMessage($"@{p_Message.Sender.UserName} No song in queue found with the key \"{l_Key}\"!");
        }
        /// <summary>
        /// Command queue
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void Command_Queue(IChatService p_Service, IChatMessage p_Message)
        {
            var l_DiffTime = (DateTime.Now - m_LastQueueCommandTime).TotalSeconds;
            if (l_DiffTime < Config.ChatRequest.QueueCommandCooldown)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} queue command is on cooldown!");
                return;
            }

            m_LastQueueCommandTime = DateTime.Now;

            string l_Reply = "";
            lock (SongQueue)
            {
                if (SongQueue.Count != 0)
                {
                    l_Reply = $"Song queue ({SongQueue.Count} songs), next : ";

                    int l_I = 0;
                    for (; l_I < SongQueue.Count && l_I < Config.ChatRequest.QueueCommandShowSize; ++l_I)
                    {
                        if (l_I != 0)
                            l_Reply += ", ";

                        l_Reply += " (bsr " + SongQueue[l_I].BeatMap.Key + ") " + SongQueue[l_I].BeatMap.Name;
                    }

                    if (l_I < SongQueue.Count)
                        l_Reply += "...";
                }
                else
                    l_Reply = "Song queue is empty!";
            }

            SendChatMessage(l_Reply);
        }
        /// <summary>
        /// Command queue status
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void Command_QueueStatus(IChatService p_Service, IChatMessage p_Message)
        {
            string l_Reply = "";
            lock (SongQueue)
            {
                if (SongQueue.Count != 0)
                    l_Reply = $"{p_Message.Sender.UserName} Song queue ({SongQueue.Count} songs)";
                else
                    l_Reply = $"{p_Message.Sender.UserName} Song queue is empty!";
            }

            SendChatMessage(l_Reply);
        }
        /// <summary>
        /// Command open
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void Command_Open(IChatService p_Service, IChatMessage p_Message)
        {
            bool l_IsModerator = HasPower(p_Message.Sender);

            if (!l_IsModerator)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            if (QueueOpen)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} Queue is already open!");
                return;
            }

            ToggleQueueStatus();
        }
        /// <summary>
        /// Command open
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void Command_Close(IChatService p_Service, IChatMessage p_Message)
        {
            bool l_IsModerator = HasPower(p_Message.Sender);

            if (!l_IsModerator)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            if (!QueueOpen)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} Queue is already closed!");
                return;
            }

            ToggleQueueStatus();
        }
        /// <summary>
        /// Command link
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void Command_Link(IChatService p_Service, IChatMessage p_Message)
        {
            if (BS_Utils.Plugin.LevelData == null
                || BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData == null
                || BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap == null)
            {
                if (m_LastPlayingLevelResponse == "")
                    SendChatMessage($"@{p_Message.Sender.UserName} no song is being played right now!");
                else
                    SendChatMessage($"@{p_Message.Sender.UserName} last song : " + m_LastPlayingLevelResponse);
            }
            else
            {
                SendChatMessage($"@{p_Message.Sender.UserName} current song : " + m_LastPlayingLevelResponse);
            }
        }

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
                        l_Entry.RequesterName   = (string)l_JSONSongs[l_SongIt]["rqn"];

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("npr"))
                            l_Entry.NamePrefix = (string)l_JSONSongs[l_SongIt]["npr"];

                        /// Start populate
                        l_Entry.BeatMap.Populate().ContinueWith(x => OnBeatmapPopulated(x, l_Entry));

                        SongQueue.Add(l_Entry);
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
                        l_Entry.RequesterName   = (string)l_JSONSongs[l_SongIt]["rqn"];

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("npr"))
                            l_Entry.NamePrefix = (string)l_JSONSongs[l_SongIt]["npr"];

                        /// Start populate
                        l_Entry.BeatMap.Populate().ContinueWith(x => OnBeatmapPopulated(x, l_Entry));

                        SongHistory.Add(l_Entry);
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
                        l_Entry.RequesterName   = (string)l_JSONSongs[l_SongIt]["rqn"];

                        if ((l_JSONSongs[l_SongIt] as JObject).ContainsKey("npr"))
                            l_Entry.NamePrefix = (string)l_JSONSongs[l_SongIt]["npr"];

                        /// Start populate
                        l_Entry.BeatMap.Populate().ContinueWith(x => OnBeatmapPopulated(x, l_Entry));

                        SongBlackList.Add(l_Entry);
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
                        l_Object["rqn"] = l_Current.RequesterName;
                        l_Object["npr"] = l_Current.NamePrefix;
                        l_Requests.Add(l_Object);
                    }
                    foreach (var l_Current in SongHistory)
                    {
                        var l_Object = new JObject();
                        l_Object["key"] = l_Current.BeatMap.Key;
                        l_Object["rqn"] = l_Current.RequesterName;
                        l_Object["npr"] = l_Current.NamePrefix;
                        l_History.Add(l_Object);
                    }
                    foreach (var l_Current in SongBlackList)
                    {
                        var l_Object = new JObject();
                        l_Object["key"] = l_Current.BeatMap.Key;
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
                    SongQueue.RemoveAll(x => x.BeatMap.Key == p_Entry.BeatMap.Key);
                    SongHistory.RemoveAll(x => x.BeatMap.Key == p_Entry.BeatMap.Key);
                    SongBlackList.RemoveAll(x => x.BeatMap.Key == p_Entry.BeatMap.Key);
                } } }
            }

            UpdateSimpleQueueFile();

            /// Avoid saving during play
            if (Utils.Game.ActiveScene != Utils.Game.SceneType.Playing)
            {
                /// Update UI
                HMMainThreadDispatcher.instance.Enqueue(() => {
                    UpdateButton();

                    if (m_ManagerViewFlowCoordinator != null && m_ManagerViewFlowCoordinator.MainView.isInViewControllerHierarchy)
                        m_ManagerViewFlowCoordinator.MainView.RebuildSongList();

                    SaveDatabase();
                });
            }
        }
    }
}
