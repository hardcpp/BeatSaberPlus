using BeatSaberPlus.SDK.Chat.Interfaces;
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

            ////////////////////////////////////////////////////////////////////////////

            if (l_LMessage.StartsWith("!link"))
                Command_Link(p_Service, p_Message);

            ////////////////////////////////////////////////////////////////////////////

            else if (l_LMessage.StartsWith("!bsr "))
                Command_BSR(p_Service, p_Message, p_Message.Message.Substring("!bsr ".Length).Trim(), false, false);
            else if (l_LMessage.StartsWith("!bsrhelp"))
                SendChatMessage($"@{p_Message.Sender.UserName} To request a song, go to https://beatsaver.com/search and find a song, Click on \"Copy !bsr\" button and paste this on the stream and I'll play it soon!.");
            else if (l_LMessage.StartsWith("!oops") || l_LMessage.StartsWith("!wrongsong") || l_LMessage.StartsWith("!wrong"))
                Command_Wrong(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!queuestatus"))
                Command_QueueStatus(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!queue"))
                Command_Queue(p_Service, p_Message);

            ////////////////////////////////////////////////////////////////////////////

            else if (l_LMessage.StartsWith("!modadd "))
                Command_BSR(p_Service, p_Message, p_Message.Message.Substring("!modadd ".Length).Trim(), true, false);
            else if (l_LMessage.StartsWith("!open"))
                Command_Open(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!close"))
                Command_Close(p_Service, p_Message);
            else if (l_LMessage.StartsWith("!sabotage "))
                Command_Sabotage(p_Service, p_Message, p_Message.Message.Substring("!sabotage ".Length).Trim());
            else if (l_LMessage.StartsWith("!songmsg "))
                Command_SongMessage(p_Service, p_Message, p_Message.Message.Substring("!songmsg ".Length).Trim());
            else if (l_LMessage.StartsWith("!mtt "))
                Command_MoveToTop(p_Service, p_Message, p_Message.Message.Substring("!mtt ".Length).Trim());
            else if (l_LMessage.StartsWith("!att "))
                Command_BSR(p_Service, p_Message, p_Message.Message.Substring("!att ".Length).Trim(), true, true);
            else if (l_LMessage.StartsWith("!remove"))
                Command_Remove(p_Service, p_Message, p_Message.Message.Substring("!remove ".Length).Trim());

            ////////////////////////////////////////////////////////////////////////////

            else if (l_LMessage.StartsWith("!bsrban "))
                Command_BanUser(p_Service, p_Message, p_Message.Message.Substring("!bsrban ".Length).Trim());
            else if (l_LMessage.StartsWith("!bsrunban "))
                Command_UnBanUser(p_Service, p_Message, p_Message.Message.Substring("!bsrunban ".Length).Trim());

            ////////////////////////////////////////////////////////////////////////////

            else if (l_LMessage.StartsWith("!bsrbanmapper "))
                Command_BanMapper(p_Service, p_Message, p_Message.Message.Substring("!bsrbanmapper ".Length).Trim());
            else if (l_LMessage.StartsWith("!bsrunbanmapper "))
                Command_UnBanMapper(p_Service, p_Message, p_Message.Message.Substring("!bsrunbanmapper ".Length).Trim());

            ////////////////////////////////////////////////////////////////////////////

            else if (l_LMessage.StartsWith("!remap "))
                Command_Remap(p_Service, p_Message, p_Message.Message.Substring("!remap ".Length).Trim());
            else if (l_LMessage.StartsWith("!allow "))
                Command_Allow(p_Service, p_Message, p_Message.Message.Substring("!allow ".Length).Trim());
            else if (l_LMessage.StartsWith("!block "))
                Command_Block(p_Service, p_Message, p_Message.Message.Substring("!block ".Length).Trim());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Command link
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void Command_Link(IChatService p_Service, IChatMessage p_Message)
        {
            if (SDK.Game.Logic.LevelData == null
                || SDK.Game.Logic.LevelData?.Data == null
                || SDK.Game.Logic.LevelData?.Data.difficultyBeatmap == null)
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
        /// Handle BSR command
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_ID">ID of the BSR</param>
        private void Command_BSR(IChatService p_Service, IChatMessage p_Message, string p_ID, bool p_ModeratorAddCommand, bool p_ModeratorAddToTop)
        {
            if (IsUserBanned(p_Message.Sender.UserName))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You are not allowed to make requests!");
                return;
            }

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
                _ = m_BeatSaver.Search(new BeatSaverSharp.SearchRequestOptions(l_Key)).ContinueWith((p_SearchTaskResult) =>
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

            /// Look for remaps
            lock (Remaps)
            {
                if (Remaps.TryGetValue(l_Key.ToLower(), out var l_RemapKey))
                    l_Key = l_RemapKey;
            }

            var l_UserRequestCount  = 0;

            /// Check if allow list or blacklisted
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
            if (p_Service is SDK.Chat.Services.Twitch.TwitchService)
            {
                var l_TwitchUser = p_Message.Sender as SDK.Chat.Models.Twitch.TwitchUser;

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

            var l_ForceAllow = false;
            lock (AllowList)
            {
                if (AllowList.Contains(l_Key))
                    l_ForceAllow = true;
            }

            /// Fetch beatmap
            _ = m_BeatSaver.Key(l_Key).ContinueWith(p_SongTaskResult =>
            {
                try
                {
                    string l_Reply = "@" + p_Message.Sender.UserName + " map " + l_Key + " not found.";
                    if (    p_SongTaskResult.Result != null
                        &&  (
                                    (l_IsModerator && p_ModeratorAddCommand)
                                ||  (l_ForceAllow  || FilterBeatMap(p_SongTaskResult.Result, p_Message.Sender.UserName, out l_Reply))
                            )
                       )
                    {
                        var     l_BeatMap           = p_SongTaskResult.Result;
                        float   l_Vote              = (float)Math.Round((double)l_BeatMap.Stats.Rating * 100f, 0);
                        var     l_IsMapperBanned    = false;

                        lock (BannedMappers)
                            l_IsMapperBanned = BannedMappers.Contains(l_BeatMap.Uploader.Username.ToLower());

                        if ((l_IsModerator && p_ModeratorAddCommand) || (!l_IsMapperBanned && !m_RequestedThisSession.Contains(l_Key)))
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

                            OnBeatmapPopulated(p_SongTaskResult, l_Entry);

                            /// Update request manager
                            OnQueueChanged();

                            l_Reply = $"(bsr {l_BeatMap.Key}) {l_BeatMap.Metadata.SongName} / {l_BeatMap.Metadata.LevelAuthorName} {l_Vote}% requested by @{p_Message.Sender.UserName} added to queue.";
                        }
                        else
                        {
                            if (l_IsMapperBanned)
                                l_Reply = $"@{p_Message.Sender.UserName} {l_BeatMap.Uploader.Username}'s maps are not allowed!";
                            else
                                l_Reply = $"@{p_Message.Sender.UserName} this song was already requested this session!";
                        }
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
                    var l_Minutes = QueueDuration / 60;
                    var l_Seconds = (QueueDuration - (l_Minutes * 60));

                    l_Reply = $"Song queue ({SongQueue.Count} songs {l_Minutes}m{l_Seconds}s), next : ";

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
                {
                    var l_Minutes = QueueDuration / 60;
                    var l_Seconds = (QueueDuration - (l_Minutes * 60));
                    l_Reply = $"@{p_Message.Sender.UserName} Song queue is " + (QueueOpen ? "open" : "closed") + $" ({SongQueue.Count} songs {l_Minutes}m{l_Seconds}s)";
                }
                else
                    l_Reply = $"@{p_Message.Sender.UserName} Song queue is " + (QueueOpen ? "open" : "closed") + ", no song queued!";
            }

            SendChatMessage(l_Reply);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
        /// Command sabotage (LIV streamer kit)
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_Parameter">Command parameter</param>
        private void Command_Sabotage(IChatService p_Service, IChatMessage p_Message, string p_Parameter)
        {
            /// Original implementation :
            /// https://github.com/angturil/SongRequestManager/blob/dev/EnhancedTwitchIntegration/Bot/ChatCommands.cs#L589

            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            p_Parameter = p_Parameter.ToLower();

            if (p_Parameter == "on" || p_Parameter == "off")
            {
                try
                {
                    System.Diagnostics.Process.Start($"liv-streamerkit://gamechanger/beat-saber-sabotage/" + (p_Parameter == "on" ? "enable" : "disable"));
                    SendChatMessage($"The !bomb command is now " + (p_Parameter == "on" ? "enabled!" : "disabled!"));
                }
                catch
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} command failed!");
                }
            }
            else
                SendChatMessage($"@{p_Message.Sender.UserName} syntax is : !sabotage on/off");
        }
        /// <summary>
        /// Command song message
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_Parameter">Command parameter</param>
        private void Command_SongMessage(IChatService p_Service, IChatMessage p_Message, string p_Parameter)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Parts = p_Parameter.Split(' ');
            if (l_Parts.Length < 2)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} invalid command, syntax is !songmsg #BSRKEY|#REQUESTERNAME #MESSAGE");
                return;
            }

            var l_Key       = l_Parts[0].ToLower();
            var l_Message   = string.Join(" ", l_Parts.Skip(1));

            SongEntry l_SongEntry = null;
            lock (SongQueue)
                l_SongEntry = SongQueue.Where(x => (l_Key.StartsWith("@") ? (l_Key.ToLower() == ("@" + x.RequesterName.ToLower())) : x.BeatMap.Key.ToLower() == l_Key)).FirstOrDefault();

            if (l_SongEntry != null)
            {
                l_SongEntry.Message = l_Message;

                /// Update request manager
                OnQueueChanged();

                SendChatMessage($"@{p_Message.Sender.UserName} message set!");
            }
            else
                SendChatMessage($"@{p_Message.Sender.UserName} No song in queue found with the key or username \"{l_Key}\"!");
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
                var l_SongEntry = SongQueue.Where(x => (l_Key.StartsWith("@") ? (l_Key.ToLower() == ("@" + x.RequesterName.ToLower())) : x.BeatMap.Key.ToLower() == l_Key)).FirstOrDefault();
                if (l_SongEntry != null)
                {
                    SongQueue.Remove(l_SongEntry);
                    SongQueue.Insert(0, l_SongEntry);

                    SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.Key}) {l_SongEntry.BeatMap.Metadata.SongName} / {l_SongEntry.BeatMap.Metadata.LevelAuthorName} requested by @{l_SongEntry.RequesterName} is now on top of queue!");

                    /// Update request manager
                    OnQueueChanged();

                    return;
                }

                SendChatMessage($"@{p_Message.Sender.UserName} No song in queue found with the key or username \"{l_Key}\"!");
            }
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
                l_SongEntry = SongQueue.Where(x => (l_Key.StartsWith("@") ? (l_Key.ToLower() == ("@" + x.RequesterName.ToLower())) : x.BeatMap.Key.ToLower() == l_Key)).FirstOrDefault();

                if (l_SongEntry != null)
                    SongQueue.Remove(l_SongEntry);
            }

            if (l_SongEntry != null)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.Key}) {l_SongEntry.BeatMap.Metadata.SongName} / {l_SongEntry.BeatMap.Metadata.LevelAuthorName} request by @{l_SongEntry.RequesterName} is removed from queue!");

                /// Update request manager
                OnQueueChanged();
            }
            else
                SendChatMessage($"@{p_Message.Sender.UserName} No song in queue found with the key or username \"{l_Key}\"!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ban user
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_Parameter">Command parameter</param>
        private void Command_BanUser(IChatService p_Service, IChatMessage p_Message, string p_Parameter)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_UserName = (p_Parameter.StartsWith("@") ? p_Parameter.Substring(1) : p_Parameter).ToLower();

            lock (BannedUsers)
            {
                if (BannedUsers.Count(x => x.ToLower() == l_UserName) != 0)
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} User \"{l_UserName}\" is already in requester ban list!");
                    return;
                }

                BannedUsers.Add(l_UserName);
                SendChatMessage($"@{p_Message.Sender.UserName} User \"{l_UserName}\" was add to the requester ban list!");
            }
        }
        /// <summary>
        /// Unban user
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_Parameter">Command parameter</param>
        private void Command_UnBanUser(IChatService p_Service, IChatMessage p_Message, string p_Parameter)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_UserName = (p_Parameter.StartsWith("@") ? p_Parameter.Substring(1) : p_Parameter).ToLower();

            lock (BannedUsers)
            {
                if (BannedUsers.Count(x => x.ToLower() == l_UserName) == 0)
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} User \"{l_UserName}\" is not in requester ban list!");
                    return;
                }

                BannedUsers.RemoveAll(x => x == l_UserName);
                SendChatMessage($"@{p_Message.Sender.UserName} User \"{l_UserName}\" was removed from the requester ban list!");
            }

            OnQueueChanged(false, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ban mapper
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_Parameter">Command parameter</param>
        private void Command_BanMapper(IChatService p_Service, IChatMessage p_Message, string p_Parameter)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_MapperName = (p_Parameter.StartsWith("@") ? p_Parameter.Substring(1) : p_Parameter).ToLower();

            lock (BannedMappers)
            {
                if (BannedMappers.Count(x => x.ToLower() == l_MapperName) != 0)
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} Mapper \"{l_MapperName}\" is already in the mapper ban list!");
                    return;
                }

                BannedMappers.Add(l_MapperName);
                SendChatMessage($"@{p_Message.Sender.UserName} Mapper \"{l_MapperName}\" was add to the mapper ban list!");
            }
        }
        /// <summary>
        /// Unban mapper
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_Parameter">Command parameter</param>
        private void Command_UnBanMapper(IChatService p_Service, IChatMessage p_Message, string p_Parameter)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_MapperName = (p_Parameter.StartsWith("@") ? p_Parameter.Substring(1) : p_Parameter).ToLower();

            lock (BannedMappers)
            {
                if (BannedMappers.Count(x => x.ToLower() == l_MapperName) == 0)
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} User \"{l_MapperName}\" is not in the mapper ban list!");
                    return;
                }

                BannedMappers.RemoveAll(x => x == l_MapperName);
                SendChatMessage($"@{p_Message.Sender.UserName} User \"{l_MapperName}\" was removed from the mapper ban list!");
            }

            OnQueueChanged(false, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Remap a song
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_Parameter">Command parameter</param>
        private void Command_Remap(IChatService p_Service, IChatMessage p_Message, string p_Parameter)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Parts = p_Parameter.Split(' ');

            if (l_Parts.Length != 2 || !OnlyHexInString(l_Parts[0]) || !OnlyHexInString(l_Parts[1]))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} Syntax is => !remap #BSR #BSR");
                return;
            }

            bool l_IsRemove = l_Parts[0] == l_Parts[1];

            lock (Remaps)
            {
                if (!l_IsRemove)
                {
                    if (Remaps.ContainsKey(l_Parts[0]))
                        Remaps[l_Parts[0]] = l_Parts[1];
                    else
                        Remaps.Add(l_Parts[0], l_Parts[1]);

                    SendChatMessage($"@{p_Message.Sender.UserName} All {l_Parts[0]} requests will remap to {l_Parts[1]}!");
                }
                else if (Remaps.ContainsKey(l_Parts[0]))
                {
                    Remaps.Remove(l_Parts[0]);
                    SendChatMessage($"@{p_Message.Sender.UserName} Remap for song {l_Parts[0]} removed!");
                }
                else
                    SendChatMessage($"@{p_Message.Sender.UserName} No remap found for {l_Parts[0]}!");
            }

            OnQueueChanged(false, false);
        }
        /// <summary>
        /// Allow a map
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        /// <param name="p_Parameter">Command parameter</param>
        private void Command_Allow(IChatService p_Service, IChatMessage p_Message, string p_Parameter)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            string l_Key = p_Parameter.ToLower();
            if (!OnlyHexInString(l_Key))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} Syntax is => !allow #BSR");
                return;
            }

            lock (AllowList)
            {
                if (!AllowList.Contains(l_Key))
                    AllowList.Add(l_Key);

                SendChatMessage($"@{p_Message.Sender.UserName} All {l_Key} requests will be allowed!");
            }

            OnQueueChanged(false, false);
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
    }
}
