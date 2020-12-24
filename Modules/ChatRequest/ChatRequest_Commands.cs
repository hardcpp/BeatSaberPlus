using BeatSaberPlusChatCore.Interfaces;
using System;
using System.Linq;

namespace BeatSaberPlus.Modules.ChatRequest
{
    /// <summary>
    /// Chat request command handler
    /// </summary>
    internal partial class ChatRequest
    {
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
            else if (l_LMessage.StartsWith("!queuestatus"))
                Command_QueueStatus(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!queue"))
                Command_Queue(p_Service, p_Message);
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

                if (l_TwitchUser.IsVip && !l_TwitchUser.IsSubscriber)       l_Limit = (l_Limit.Item1 + Config.ChatRequest.VIPBonusRequest,                                             "VIPs");
                if (l_TwitchUser.IsSubscriber && !l_TwitchUser.IsVip)       l_Limit = (l_Limit.Item1 + Config.ChatRequest.SubscriberBonusRequest,                                      "Subscribers");
                if (l_TwitchUser.IsSubscriber &&  l_TwitchUser.IsVip)       l_Limit = (l_Limit.Item1 + Config.ChatRequest.VIPBonusRequest + Config.ChatRequest.SubscriberBonusRequest, "VIP Subscribers");
                if (l_TwitchUser.IsModerator || l_TwitchUser.IsBroadcaster) l_Limit = (1000,                                                                                           "Moderators");

                if (l_TwitchUser.IsModerator || l_TwitchUser.IsBroadcaster)
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
                                RequestTime     = DateTime.Now,
                                NamePrefix      = l_NamePrefix
                            };

                            lock (SongQueue)
                            {
                                if (l_IsModerator && p_ModeratorAddToTop)
                                    SongQueue.Insert(0, l_Entry);
                                else
                                    SongQueue.Add(l_Entry);
                            }

                            /// Update request manager
                            OnQueueChanged();

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
            if (!HasPower(p_Message.Sender))
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

                    /// Update request manager
                    OnQueueChanged();

                    return;
                }

                SendChatMessage($"@{p_Message.Sender.UserName} No song in queue found with the key \"{l_Key}\"!");
            }
        }
        /// <summary>
        /// Block a song
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_ID">ID of the BSR</param>
        private void Command_Block(IChatService p_Service, IChatMessage p_Message, string p_ID)
        {
            if (!HasPower(p_Message.Sender))
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

                        /// Update request manager
                        OnQueueChanged();
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
                if (l_SongEntry != null)
                    SongQueue.Remove(l_SongEntry);
            }

            if (l_SongEntry != null)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.Key}) {l_SongEntry.BeatMap.Metadata.SongName} / {l_SongEntry.BeatMap.Metadata.LevelAuthorName} is removed from queue!");

                /// Update request manager
                OnQueueChanged();
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
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            string l_Key = p_ID.ToLower();

            SongEntry l_SongEntry = null;
            lock (SongQueue)
            {
                l_SongEntry = SongQueue.Where(x => x.BeatMap.Key.ToLower() == l_Key).FirstOrDefault();
                if (l_SongEntry != null)
                    SongQueue.Remove(l_SongEntry);
            }

            if (l_SongEntry != null)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.Key}) {l_SongEntry.BeatMap.Metadata.SongName} / {l_SongEntry.BeatMap.Metadata.LevelAuthorName} is removed from queue!");

                /// Update request manager
                OnQueueChanged();
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
                    l_Reply = $"@{p_Message.Sender.UserName} Song queue is " + (QueueOpen ? "open" : "closed") + $" ({SongQueue.Count} songs)";
                else
                    l_Reply = $"@{p_Message.Sender.UserName} Song queue is " + (QueueOpen ? "open" : "closed") + ", no song queued!";
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
            if (!HasPower(p_Message.Sender))
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
            if (!HasPower(p_Message.Sender))
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
    }
}
