using BeatSaberPlus.SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BeatSaberPlus_ChatRequest
{
    /// <summary>
    /// Chat request command handler
    /// </summary>
    public partial class ChatRequest
    {
        /// <summary>
        /// Command table
        /// </summary>
        private Dictionary<string, Action<IChatService, IChatMessage, string[]>>
             m_CommandTable = new Dictionary<string, Action<IChatService, IChatMessage, string[]>>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build command table
        /// </summary>
        private void BuildCommandTable()
        {
            m_CommandTable.Clear();

            var l_Config = CRConfig.Instance.Commands;

            /// User
            if (l_Config.BSRCommandEnabled)         RegisterCommand(l_Config.BSRCommand,            (x, y, z) => Command_BSR(x, y, z, false, false));
            if (l_Config.BSRHelpCommandEnabled)     RegisterCommand(l_Config.BSRHelpCommand,        Command_BSRHelp);
            if (l_Config.LinkCommandEnabled)        RegisterCommand(l_Config.LinkCommand,           Command_Link);
            if (l_Config.QueueCommandEnabled)       RegisterCommand(l_Config.QueueCommand,          Command_Queue);
            if (l_Config.QueueStatusCommandEnabled) RegisterCommand(l_Config.QueueStatusCommand,    Command_QueueStatus);
            if (l_Config.WrongCommandEnabled)       RegisterCommand(l_Config.WrongCommand,          Command_Wrong);

            /// Moderator+
            if (l_Config.Moderator_AddToTopCommandEnabled)      RegisterCommand(l_Config.Moderator_AddToTopCommand,         (x, y, z) => Command_BSR(x, y, z, true, true));
            if (l_Config.Moderator_AllowCommandEnabled)         RegisterCommand(l_Config.Moderator_AllowCommand,            Command_Allow);
            if (l_Config.Moderator_BlockCommandEnabled)         RegisterCommand(l_Config.Moderator_BlockCommand,            Command_Block);
            if (l_Config.Moderator_BsrBanCommandEnabled)        RegisterCommand(l_Config.Moderator_BsrBanCommand,           Command_BanUser);
            if (l_Config.Moderator_BsrBanMapperCommandEnabled)  RegisterCommand(l_Config.Moderator_BsrBanMapperCommand,     Command_BanMapper);
            if (l_Config.Moderator_BsrUnbanCommandEnabled)      RegisterCommand(l_Config.Moderator_BsrUnbanCommand,         Command_UnBanUser);
            if (l_Config.Moderator_BsrUnbanMapperCommandEnabled)RegisterCommand(l_Config.Moderator_BsrUnbanMapperCommand,   Command_UnBanMapper);
            if (l_Config.Moderator_CloseCommandEnabled)         RegisterCommand(l_Config.Moderator_CloseCommand,            Command_Close);
            if (l_Config.Moderator_ModAddCommandEnabled)        RegisterCommand(l_Config.Moderator_ModAddCommand,           (x, y, z) => Command_BSR(x, y, z, true, false));
            if (l_Config.Moderator_MoveToTopCommandEnabled)     RegisterCommand(l_Config.Moderator_MoveToTopCommand,        Command_MoveToTop);
            if (l_Config.Moderator_OpenCommandEnabled)          RegisterCommand(l_Config.Moderator_OpenCommand,             Command_Open);
            if (l_Config.Moderator_RemapCommandEnabled)         RegisterCommand(l_Config.Moderator_RemapCommand,            Command_Remap);
            if (l_Config.Moderator_RemoveCommandEnabled)        RegisterCommand(l_Config.Moderator_RemoveCommand,           Command_Remove);
            if (l_Config.Moderator_SabotageCommandEnabled)      RegisterCommand(l_Config.Moderator_SabotageCommand,         Command_Sabotage);
            if (l_Config.Moderator_SongMessageCommandEnabled)   RegisterCommand(l_Config.Moderator_SongMessageCommand,      Command_SongMessage);
        }
        /// <summary>
        /// Register a command
        /// </summary>
        /// <param name="p_Name">Name or names</param>
        /// <param name="p_Callback">Callback method</param>
        private void RegisterCommand(string p_Name, Action<IChatService, IChatMessage, string[]> p_Callback)
        {
            foreach (var l_Current in p_Name.Split(','))
            {
                var l_Name = l_Current.ToLower().Trim().Replace("!", "");
                if (!m_CommandTable.ContainsKey(l_Name))
                    m_CommandTable.Add(l_Name, p_Callback);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">ID of the message</param>
        private void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_Service, IChatMessage p_Message)
        {
            if (p_Message.Message.Length < 2 || p_Message.Message[0] != '!')
                return;

            var l_Parts         = Regex.Split(p_Message.Message, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            var l_Command       = l_Parts[0]?.ToLower().Remove(0, 1) ?? "";
            var l_Parameters    = new List<string>(l_Parts);

            if (l_Parameters.Count != 0)
                l_Parameters.RemoveAt(0);

            if (m_CommandTable.TryGetValue(l_Command, out var l_CommandBlock))
                l_CommandBlock?.Invoke(p_Service, p_Message, l_Parameters.ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_BSR(IChatService p_Service, IChatMessage p_Message, string[] p_Params, bool p_ModeratorAddCommand, bool p_ModeratorAddToTop)
        {
            if (IsUserBanned(p_Message.Sender.UserName))
            {
                SendChatMessage(CRConfig.Instance.Commands.BSRCommand_UserBanned.Replace("$UserName", p_Message.Sender.UserName));
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
                SendChatMessage(CRConfig.Instance.Commands.BSRCommand_QueueClosed, p_Message.Sender);
                return;
            }

            string l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";

            if (!OnlyHexInString(l_Key))
            {
                BeatSaberPlus.SDK.Game.BeatMapsClient.GetOnlineBySearch(l_Key, (p_Valid, p_SearchTaskResult) =>
                {
                    if (!p_Valid || p_SearchTaskResult.Length == 0)
                    {
                        SendChatMessage(CRConfig.Instance.Commands.BSRCommand_Search0Result.Replace("$Search", l_Key), p_Message.Sender);
                        return;
                    }

                    var l_Results   = "";

                    int l_I = 0;
                    for (; l_I < p_SearchTaskResult.Length && l_I < 4; ++l_I)
                    {
                        if (l_I != 0)
                            l_Results += ", ";

                        l_Results += " (!bsr " + p_SearchTaskResult[l_I].id + ") " + p_SearchTaskResult[l_I].name;
                    }

                    if (l_I < p_SearchTaskResult.Length)
                        l_Results += "...";

                    var l_Reply = CRConfig.Instance.Commands.BSRCommand_SearchResults
                                    .Replace("$Search",     l_Key)
                                    .Replace("$Count",      p_SearchTaskResult.Length.ToString())
                                    .Replace("$Results",    l_Results);

                    SendChatMessage(l_Reply, p_Message.Sender);
                });

                return;
            }
            else
                l_Key = l_Key.TrimStart('0');

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
                    var l_BeatMap = SongBlackList.Where(x => x.BeatMap.id.ToLower() == l_Key).FirstOrDefault();
                    if (l_BeatMap != null)
                    {
                        SendChatMessage(CRConfig.Instance.Commands.BSRCommand_Blacklisted, p_Message.Sender, l_BeatMap.BeatMap);
                        return;
                    }
                }
            }

            /// Check if already in queue
            lock (SongQueue)
            {
                var l_BeatMap = SongQueue.Where(x => x.BeatMap.id.ToLower() == l_Key).FirstOrDefault();
                if (l_BeatMap != null)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BSRCommand_AlreadyQueued, p_Message.Sender, l_BeatMap.BeatMap);
                    return;
                }

                l_UserRequestCount = SongQueue.Where(x => x.RequesterName == p_Message.Sender.UserName).Count();
            }

            string l_NamePrefix = "";

            /// Handle request limits
            if (p_Service is BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService)
            {
                var l_TwitchUser = p_Message.Sender as BeatSaberPlus.SDK.Chat.Models.Twitch.TwitchUser;

                (int, string) l_Limit = (CRConfig.Instance.UserMaxRequest, "Users");

                if (l_TwitchUser.IsVip && !l_TwitchUser.IsSubscriber)       l_Limit = (l_Limit.Item1 + CRConfig.Instance.VIPBonusRequest,                                             "VIPs");
                if (l_TwitchUser.IsSubscriber && !l_TwitchUser.IsVip)       l_Limit = (l_Limit.Item1 + CRConfig.Instance.SubscriberBonusRequest,                                      "Subscribers");
                if (l_TwitchUser.IsSubscriber &&  l_TwitchUser.IsVip)       l_Limit = (l_Limit.Item1 + CRConfig.Instance.VIPBonusRequest + CRConfig.Instance.SubscriberBonusRequest,  "VIP Subscribers");
                if (l_TwitchUser.IsModerator || l_TwitchUser.IsBroadcaster) l_Limit = (1000,                                                                                          "Moderators");

                if (l_TwitchUser.IsModerator || l_TwitchUser.IsBroadcaster)
                    l_NamePrefix = "🗡";
                else if (l_TwitchUser.IsVip)
                    l_NamePrefix = "💎";
                else if (l_TwitchUser.IsSubscriber)
                    l_NamePrefix = "👑";

                if (l_UserRequestCount >= l_Limit.Item1)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BSRCommand_RequestLimit
                                    .Replace("$UserName",           p_Message.Sender.UserName)
                                    .Replace("$UserRequestCount",   l_UserRequestCount.ToString())
                                    .Replace("$UserType",           l_Limit.Item2)
                                    .Replace("$UserTypeLimit",      l_Limit.Item1.ToString()));
                    return;
                }
            }

            /// Check if already requested
            if (!(l_IsModerator && p_ModeratorAddCommand) && m_RequestedThisSession.Contains(l_Key))
            {
                SendChatMessage(CRConfig.Instance.Commands.BSRCommand_AlreadyPlayed, p_Message.Sender);
                return;
            }

            var l_ForceAllow = false;
            lock (AllowList)
            {
                if (AllowList.Contains(l_Key))
                    l_ForceAllow = true;
            }

            /// Fetch beatmap
            BeatSaberPlus.SDK.Game.BeatMapsClient.GetOnlineByKey(l_Key, (p_Valid, p_BeatMap) =>
            {
                try
                {
                    string l_Reply = CRConfig.Instance.Commands.BSRCommand_NotFound.Replace("$BSRKey", l_Key);

                    if (p_Valid
                        &&  (
                                    (l_IsModerator && p_ModeratorAddCommand)
                                ||  (l_ForceAllow  || FilterBeatMap(p_BeatMap, p_Message.Sender.UserName, out l_Reply))
                            )
                       )
                    {
                        float   l_Vote              = (float)Math.Round((double)p_BeatMap.stats.score * 100f, 0);
                        var     l_IsMapperBanned    = false;

                        lock (BannedMappers)
                            l_IsMapperBanned = BannedMappers.Contains(p_BeatMap.uploader.name.ToLower());

                        if ((l_IsModerator && p_ModeratorAddCommand) || (!l_IsMapperBanned && !m_RequestedThisSession.Contains(l_Key)))
                        {
                            m_RequestedThisSession.Add(l_Key.ToLower());

                            var l_Entry = new SongEntry()
                            {
                                BeatMap         = p_BeatMap,
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

                            OnBeatmapPopulated(p_Valid, l_Entry);

                            /// Update request manager
                            OnQueueChanged();

                            l_Reply = CRConfig.Instance.Commands.BSRCommand_RequestOK
                                        .Replace("$BSRKey",             p_BeatMap.id.ToLower())
                                        .Replace("$SongName",           p_BeatMap.metadata.songName)
                                        .Replace("$LevelAuthorName",    p_BeatMap.metadata.levelAuthorName)
                                        .Replace("$Vote",               l_Vote.ToString());
                        }
                        else
                        {
                            if (l_IsMapperBanned)
                                l_Reply = CRConfig.Instance.Commands.BSRCommand_MapperBanned
                                        .Replace("$UploaderName", p_BeatMap.uploader.name);
                            else
                                l_Reply = CRConfig.Instance.Commands.BSRCommand_AlreadyPlayed;
                        }
                    }

                    SendChatMessage(l_Reply, p_Message.Sender);
                }
                catch (System.Exception p_Exception)
                {
                    Logger.Instance.Error("Command_BSR");
                    Logger.Instance.Error(p_Exception);
                }
            });
        }
        private void Command_BSRHelp(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            SendChatMessage(CRConfig.Instance.Commands.BSRHelpCommand_Reply, p_Message.Sender);
        }
        private void Command_Link(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            string l_Response = "";
            if (BeatSaberPlus.SDK.Game.Logic.LevelData == null
                || BeatSaberPlus.SDK.Game.Logic.LevelData?.Data == null
                || BeatSaberPlus.SDK.Game.Logic.LevelData?.Data.difficultyBeatmap == null)
            {
                if (m_LastPlayingLevelResponse == "")
                    l_Response = CRConfig.Instance.Commands.LinkCommand_NoSong;
                else
                    l_Response = CRConfig.Instance.Commands.LinkCommand_LastSong;
            }
            else
                l_Response = CRConfig.Instance.Commands.LinkCommand_CurrentSong;

            if (!string.IsNullOrEmpty(l_Response))
                SendChatMessage(l_Response.Replace("$SongInfo", m_LastPlayingLevelResponse), p_Message.Sender);
        }
        private void Command_Queue(IChatService p_Service, IChatMessage p_Message, string[] p_Param)
        {
            var l_DiffTime = (DateTime.Now - m_LastQueueCommandTime).TotalSeconds;
            if (l_DiffTime < CRConfig.Instance.QueueCommandCooldown)
            {
                SendChatMessage(CRConfig.Instance.Commands.QueueCommand_Cooldown, p_Message.Sender);
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
                    for (; l_I < SongQueue.Count && l_I < CRConfig.Instance.QueueCommandShowSize; ++l_I)
                    {
                        if (l_I != 0)
                            l_Reply += ", ";

                        l_Reply += " (bsr " + SongQueue[l_I].BeatMap.id.ToLower() + ") " + SongQueue[l_I].BeatMap.name;
                    }

                    if (l_I < SongQueue.Count)
                        l_Reply += "...";
                }
                else
                    l_Reply = CRConfig.Instance.Commands.QueueCommand_Empty;
            }

            SendChatMessage(l_Reply, p_Message.Sender);
        }
        private void Command_QueueStatus(IChatService p_Service, IChatMessage p_Message, string[] p_Param)
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

            SendChatMessage(l_Reply, p_Message.Sender);
        }
        private void Command_Wrong(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
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
                SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.id}) {l_SongEntry.BeatMap.metadata.songName} / {l_SongEntry.BeatMap.metadata.levelAuthorName} is removed from queue!");

                /// Update request manager
                OnQueueChanged();
            }
            else
                SendChatMessage(CRConfig.Instance.Commands.WrongCommand_NoSong, p_Message.Sender);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_Open(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
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
        private void Command_Close(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
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
        private void Command_Sabotage(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            /// Original implementation :
            /// https://github.com/angturil/SongRequestManager/blob/dev/EnhancedTwitchIntegration/Bot/ChatCommands.cs#L589

            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Parameter = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            if (l_Parameter == "on" || l_Parameter == "off")
            {
                try
                {
                    System.Diagnostics.Process.Start($"liv-streamerkit://gamechanger/beat-saber-sabotage/" + (l_Parameter == "on" ? "enable" : "disable"));
                    SendChatMessage($"The !bomb command is now " + (l_Parameter == "on" ? "enabled!" : "disabled!"));
                }
                catch
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} command failed!");
                }
            }
            else
                SendChatMessage($"@{p_Message.Sender.UserName} syntax is : !sabotage on/off");
        }
        private void Command_SongMessage(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            if (p_Params.Length < 2)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} invalid command, syntax is !songmsg #BSRKEY|#REQUESTERNAME #MESSAGE");
                return;
            }

            var l_Key       = p_Params[0].ToLower();
            var l_Message   = string.Join(" ", p_Params.Skip(1));

            SongEntry l_SongEntry = null;
            lock (SongQueue)
                l_SongEntry = SongQueue.Where(x => (l_Key.StartsWith("@") ? (l_Key.ToLower() == ("@" + x.RequesterName.ToLower())) : x.BeatMap.id.ToLower() == l_Key)).FirstOrDefault();

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
        private void Command_MoveToTop(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            string l_Key = p_Params.Length > 0 ? p_Params[0].ToLower().Trim() : "";

            lock (SongQueue)
            {
                var l_SongEntry = SongQueue.Where(x => (l_Key.StartsWith("@") ? (l_Key.ToLower() == ("@" + x.RequesterName.ToLower())) : x.BeatMap.id.ToLower() == l_Key)).FirstOrDefault();
                if (l_SongEntry != null)
                {
                    SongQueue.Remove(l_SongEntry);
                    SongQueue.Insert(0, l_SongEntry);

                    SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.id.ToLower()}) {l_SongEntry.BeatMap.metadata.songName} / {l_SongEntry.BeatMap.metadata.levelAuthorName} requested by @{l_SongEntry.RequesterName} is now on top of queue!");

                    /// Update request manager
                    OnQueueChanged();

                    return;
                }

                SendChatMessage($"@{p_Message.Sender.UserName} No song in queue found with the key or username \"{l_Key}\"!");
            }
        }
        private void Command_Remove(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            string l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";

            SongEntry l_SongEntry = null;
            lock (SongQueue)
            {
                l_SongEntry = SongQueue.Where(x => (l_Key.StartsWith("@") ? (l_Key.ToLower() == ("@" + x.RequesterName.ToLower())) : x.BeatMap.id.ToLower() == l_Key)).FirstOrDefault();

                if (l_SongEntry != null)
                    SongQueue.Remove(l_SongEntry);
            }

            if (l_SongEntry != null)
            {
                SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_SongEntry.BeatMap.id}) {l_SongEntry.BeatMap.metadata.songName} / {l_SongEntry.BeatMap.metadata.levelAuthorName} request by @{l_SongEntry.RequesterName} is removed from queue!");

                /// Update request manager
                OnQueueChanged();
            }
            else
                SendChatMessage($"@{p_Message.Sender.UserName} No song in queue found with the key or username \"{l_Key}\"!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_BanUser(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Parameter = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            var l_UserName = (l_Parameter.StartsWith("@") ? l_Parameter.Substring(1) : l_Parameter).ToLower();

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
        private void Command_UnBanUser(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Parameter = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            var l_UserName = (l_Parameter.StartsWith("@") ? l_Parameter.Substring(1) : l_Parameter).ToLower();

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

        private void Command_BanMapper(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Parameter = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            var l_MapperName = (l_Parameter.StartsWith("@") ? l_Parameter.Substring(1) : l_Parameter).ToLower();

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
        private void Command_UnBanMapper(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Parameter = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            var l_MapperName = (l_Parameter.StartsWith("@") ? l_Parameter.Substring(1) : l_Parameter).ToLower();

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

        private void Command_Remap(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            if (p_Params.Length != 2 || !OnlyHexInString(p_Params[0]) || !OnlyHexInString(p_Params[1]))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} Syntax is => !remap #BSR #BSR");
                return;
            }

            bool l_IsRemove = p_Params[0] == p_Params[1];

            lock (Remaps)
            {
                if (!l_IsRemove)
                {
                    if (Remaps.ContainsKey(p_Params[0]))
                        Remaps[p_Params[0]] = p_Params[1];
                    else
                        Remaps.Add(p_Params[0], p_Params[1]);

                    SendChatMessage($"@{p_Message.Sender.UserName} All {p_Params[0]} requests will remap to {p_Params[1]}!");
                }
                else if (Remaps.ContainsKey(p_Params[0]))
                {
                    Remaps.Remove(p_Params[0]);
                    SendChatMessage($"@{p_Message.Sender.UserName} Remap for song {p_Params[0]} removed!");
                }
                else
                    SendChatMessage($"@{p_Message.Sender.UserName} No remap found for {p_Params[0]}!");
            }

            OnQueueChanged(false, false);
        }
        private void Command_Allow(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
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
        private void Command_Block(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                return;
            }

            var l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";

            if (!OnlyHexInString(l_Key))
            {
                SendChatMessage($"@{p_Message.Sender.UserName} Invalid key!");
                return;
            }

            lock (SongBlackList)
            {
                var l_BeatMap = SongBlackList.Where(x => x.BeatMap.id.ToLower() == l_Key).FirstOrDefault();
                if (l_BeatMap != null)
                {
                    SendChatMessage($"@{p_Message.Sender.UserName} (bsr {l_BeatMap.BeatMap.id}) {l_BeatMap.BeatMap.metadata.songName} / {l_BeatMap.BeatMap.metadata.levelAuthorName} is already blacklisted!");
                    return;
                }
            }

            /// Fetch beatmap
            BeatSaberPlus.SDK.Game.BeatMapsClient.GetOnlineByKey(l_Key, (p_Valid, p_BeatMap) =>
            {
                try
                {
                    string l_Reply = "@" + p_Message.Sender.UserName + " map " + l_Key + " not found.";
                    if (p_Valid)
                    {
                        l_Reply = $"@{p_Message.Sender.UserName} (bsr {p_BeatMap.id.ToLower()}) {p_BeatMap.metadata.songName} / {p_BeatMap.metadata.levelAuthorName} is now blacklisted!";

                        lock (SongQueue) { lock (SongHistory) { lock (SongBlackList) {
                            SongQueue.RemoveAll(x => x.BeatMap.id   == p_BeatMap.id);
                            SongHistory.RemoveAll(x => x.BeatMap.id == p_BeatMap.id);

                            /// Add to blacklist
                            SongBlackList.RemoveAll(x => x.BeatMap.id == p_BeatMap.id);
                            SongBlackList.Insert(0, new SongEntry() { BeatMap = p_BeatMap, NamePrefix = "🗡", RequesterName = p_Message.Sender.DisplayName });
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
