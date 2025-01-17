﻿using CP_SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Cooldowns
        /// </summary>
        private Dictionary<string, Dictionary<string, long>> m_Cooldowns = new Dictionary<string, Dictionary<string, long>>();

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
            if (l_Config.BSRCommandEnabled)         RegisterCommand(l_Config.BSRCommand,            (x, y, z) => Command_BSR(x, y, z, false, false, CRConfig.Instance.Commands.BSRCommandPermissions));
            if (l_Config.BSRHelpCommandEnabled)     RegisterCommand(l_Config.BSRHelpCommand,        Command_BSRHelp);
            if (l_Config.LinkCommandEnabled)        RegisterCommand(l_Config.LinkCommand,           Command_Link);
            if (l_Config.QueueCommandEnabled)       RegisterCommand(l_Config.QueueCommand,          Command_Queue);
            if (l_Config.QueueStatusCommandEnabled) RegisterCommand(l_Config.QueueStatusCommand,    Command_QueueStatus);
            if (l_Config.WrongCommandEnabled)       RegisterCommand(l_Config.WrongCommand,          Command_Wrong);

            /// Moderator+
            if (l_Config.AddToTopCommandEnabled)      RegisterCommand(l_Config.AddToTopCommand,         (x, y, z) => Command_BSR(x, y, z, true, true, CRConfig.Instance.Commands.AddToTopCommandPermissions));
            if (l_Config.AllowCommandEnabled)         RegisterCommand(l_Config.AllowCommand,            Command_Allow);
            if (l_Config.BlockCommandEnabled)         RegisterCommand(l_Config.BlockCommand,            Command_Block);
            if (l_Config.BsrBanCommandEnabled)        RegisterCommand(l_Config.BsrBanCommand,           Command_BanUser);
            if (l_Config.BsrBanMapperCommandEnabled)  RegisterCommand(l_Config.BsrBanMapperCommand,     Command_BanMapper);
            if (l_Config.BsrUnbanCommandEnabled)      RegisterCommand(l_Config.BsrUnbanCommand,         Command_UnBanUser);
            if (l_Config.BsrUnbanMapperCommandEnabled)RegisterCommand(l_Config.BsrUnbanMapperCommand,   Command_UnBanMapper);
            if (l_Config.CloseCommandEnabled)         RegisterCommand(l_Config.CloseCommand,            Command_Close);
            if (l_Config.ModAddCommandEnabled)        RegisterCommand(l_Config.ModAddCommand,           (x, y, z) => Command_BSR(x, y, z, true, false, CRConfig.Instance.Commands.ModAddPermissions));
            if (l_Config.MoveToTopCommandEnabled)     RegisterCommand(l_Config.MoveToTopCommand,        Command_MoveToTop);
            if (l_Config.OpenCommandEnabled)          RegisterCommand(l_Config.OpenCommand,             Command_Open);
            if (l_Config.RemapCommandEnabled)         RegisterCommand(l_Config.RemapCommand,            Command_Remap);
            if (l_Config.RemoveCommandEnabled)        RegisterCommand(l_Config.RemoveCommand,           Command_Remove);
            if (l_Config.SabotageCommandEnabled)      RegisterCommand(l_Config.SabotageCommand,         Command_Sabotage);
            if (l_Config.SongMessageCommandEnabled)   RegisterCommand(l_Config.SongMessageCommand,      Command_SongMessage);
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
        /// <summary>
        /// Handle command cooldown
        /// </summary>
        /// <param name="p_Identifier">Command identifier</param>
        /// <param name="p_PerUser">Is it a per user based cooldown</param>
        /// <param name="p_CooldownSeconds">Cooldown in seconds</param>
        /// <param name="p_User">Context user</param>
        /// <returns>True if the cooldown is allowed</returns>
        private bool HandleCommandCooldown(string p_Identifier, bool p_PerUser, int p_CooldownSeconds, IChatUser p_User)
        {
            var l_Now = CP_SDK.Misc.Time.UnixTimeNow();
            var l_Key = p_PerUser ? p_User.Id : "$@_GLOBAL";

            lock (m_Cooldowns)
            {
                if (!m_Cooldowns.TryGetValue(p_Identifier, out var l_Container))
                {
                    l_Container = new Dictionary<string, long>();
                    m_Cooldowns[p_Identifier] = l_Container;
                }

                if (!l_Container.ContainsKey(l_Key))
                {
                    l_Container[l_Key] = l_Now;
                    return true;
                }

                if ((l_Now - l_Container[l_Key]) < p_CooldownSeconds)
                    return false;

                l_Container[l_Key] = l_Now;
            }

            return true;
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
            if (p_Message.Channel.IsTemp || p_Message.Message.Length < 2 || p_Message.Message[0] != '!')
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

        private void Command_BSR(IChatService p_Service, IChatMessage p_Message, string[] p_Params, bool p_ModAddCommand, bool p_AddToTopCommand, CRConfig._Commands.EPermission p_Permissions = CRConfig._Commands.EPermission.Viewers)
        {
            if (IsUserBanned(p_Message.Sender.UserName))
            {
                SendChatMessage(CRConfig.Instance.Commands.BSRCommand_UserBanned, p_Service, p_Message);
                return;
            }

            if (!HasPower(p_Message.Sender, p_Permissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (!QueueOpen && !p_ModAddCommand)
            {
                SendChatMessage(CRConfig.Instance.Commands.BSRCommand_QueueClosed, p_Service, p_Message);
                return;
            }

            if (!HandleCommandCooldown("Command_BSR", CRConfig.Instance.Commands.BSRCommandCooldownPerUser, CRConfig.Instance.Commands.BSRCommandCooldown, p_Message.Sender))
            {
                SendChatMessage(CRConfig.Instance.Messages.OnCooldown, p_Service, p_Message);
                return;
            }

            string l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";

            if (!OnlyHexInString(l_Key))
            {
                if (CRConfig.Instance.SafeMode2)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BSRCommand_SearchDisabled, p_Service, p_Message);
                    return;
                }

                CP_SDK_BS.Game.BeatMapsClient.GetOnlineBySearch(l_Key, (p_Valid, p_SearchTaskResult) =>
                {
                    if (!p_Valid || p_SearchTaskResult.Length == 0)
                    {
                        SendChatMessage(CRConfig.Instance.Commands.BSRCommand_Search0Result, p_Service, p_Message, null, ("$Search", l_Key));
                        return;
                    }

                    var l_Results   = "";

                    int l_I = 0;
                    for (; l_I < p_SearchTaskResult.Length && l_I < 4; ++l_I)
                    {
                        if (l_I != 0)
                            l_Results += ", ";

                        l_Results += " (!bsr " + p_SearchTaskResult[l_I].id + ") " + p_SearchTaskResult[l_I].name.Replace(".", " . ");
                    }

                    if (l_I < p_SearchTaskResult.Length)
                        l_Results += "...";

                    var l_Reply = CRConfig.Instance.Commands.BSRCommand_SearchResults
                                    .Replace("$Search",     l_Key)
                                    .Replace("$Count",      p_SearchTaskResult.Length.ToString())
                                    .Replace("$Results",    l_Results);

                    SendChatMessage(l_Reply, p_Service, p_Message);
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
            if (!p_ModAddCommand)
            {
                lock (SongBlackList)
                {
                    var l_BeatMap = SongBlackList.Where(x => x.BeatSaver_Map.id.ToLower() == l_Key).FirstOrDefault();
                    if (l_BeatMap != null)
                    {
                        SendChatMessage(CRConfig.Instance.Commands.BSRCommand_Blacklisted, p_Service, p_Message, l_BeatMap.BeatSaver_Map);
                        return;
                    }
                }
            }

            /// Check if already in queue
            lock (SongQueue)
            {
                var l_BeatMap = SongQueue.Where(x => x.BeatSaver_Map.id.ToLower() == l_Key).FirstOrDefault();
                if (l_BeatMap != null)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BSRCommand_AlreadyQueued, p_Service, p_Message, l_BeatMap.BeatSaver_Map);
                    return;
                }

                l_UserRequestCount = SongQueue.Where(x => x.RequesterName == p_Message.Sender.UserName).Count();
            }

            string l_NamePrefix = "";

            /// Handle request limits
            {
                (int, string) l_Limit = (CRConfig.Instance.UserMaxRequest, "Users");

                if (p_Message.Sender.IsVip          && !p_Message.Sender.IsSubscriber)  l_Limit = (l_Limit.Item1 + CRConfig.Instance.VIPBonusRequest,                                             "VIPs");
                if (p_Message.Sender.IsSubscriber   && !p_Message.Sender.IsVip)         l_Limit = (l_Limit.Item1 + CRConfig.Instance.SubscriberBonusRequest,                                      "Subscribers");
                if (p_Message.Sender.IsSubscriber   && p_Message.Sender.IsVip)          l_Limit = (l_Limit.Item1 + CRConfig.Instance.VIPBonusRequest + CRConfig.Instance.SubscriberBonusRequest,  "VIP Subscribers");
                if (p_Message.Sender.IsModerator    || p_Message.Sender.IsBroadcaster)  l_Limit = (1000,                                                                                          "Moderators");

                if (p_Message.Sender.IsModerator || p_Message.Sender.IsBroadcaster)
                    l_NamePrefix = "🗡";
                else if (p_Message.Sender.IsVip)
                    l_NamePrefix = "💎";
                else if (p_Message.Sender.IsSubscriber)
                    l_NamePrefix = "👑";

                if (l_UserRequestCount >= l_Limit.Item1)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BSRCommand_RequestLimit
                                    .Replace("$UserName",           p_Message.Sender.UserName)
                                    .Replace("$UserRequestCount",   l_UserRequestCount.ToString())
                                    .Replace("$UserTypeLimit",      l_Limit.Item1.ToString())/*Fix old variable*/.Replace("$UsersLimit", l_Limit.Item1.ToString())
                                    .Replace("$UserType",           l_Limit.Item2),
                                    p_Service, p_Message);
                    return;
                }
            }

            /// Check if already requested
            if (!p_ModAddCommand && m_RequestedThisSession.Contains(l_Key))
            {
                SendChatMessage(CRConfig.Instance.Commands.BSRCommand_AlreadyPlayed, p_Service, p_Message);
                return;
            }

            var l_ForceAllow = false;
            lock (AllowList)
            {
                if (AllowList.Contains(l_Key))
                    l_ForceAllow = true;
            }

            /// Fetch beatmap
            CP_SDK_BS.Game.BeatMapsClient.GetOnlineByKey(l_Key, (p_Valid, p_BeatMap) =>
            {
                try
                {
                    string l_Reply = CRConfig.Instance.Commands.BSRCommand_NotFound.Replace("$BSRKey", l_Key);

                    if (p_Valid && p_BeatMap != null
                        &&  (
                                    p_ModAddCommand
                                ||  (l_ForceAllow  || FilterBeatMap(p_BeatMap, p_Message.Sender.UserName, out l_Reply))
                            )
                       )
                    {
                        var l_IsMapperBanned = false;

                        lock (BannedMappers)
                            l_IsMapperBanned = BannedMappers.Contains(p_BeatMap.uploader.name.ToLower());

                        if (p_ModAddCommand || (!l_IsMapperBanned && !m_RequestedThisSession.Contains(l_Key)))
                        {
                            m_RequestedThisSession.Add(l_Key.ToLower());

                            var l_RequesterName = p_Message.Sender.UserName;
                            if (p_ModAddCommand && p_Params.Length == 2 && p_Params[1].Length > 3 && p_Params[1][0] == '@')
                            {
                                l_RequesterName = p_Params[1].Substring(1) + "\n(Added by " + l_NamePrefix + " " + p_Message.Sender.UserName + ")";
                                l_NamePrefix    = string.Empty;
                            }

                            var l_Entry = new Data.SongEntry()
                            {
                                BeatSaver_Map   = p_BeatMap,
                                RequesterName   = l_RequesterName,
                                RequestTime     = DateTime.Now,
                                TitlePrefix     = l_NamePrefix
                            };

                            lock (SongQueue)
                            {
                                if (p_AddToTopCommand)
                                    SongQueue.Insert(0, l_Entry);
                                else
                                    SongQueue.Add(l_Entry);
                            }

                            OnBeatmapPopulated(p_Valid, l_Entry);

                            /// Update request manager
                            OnQueueChanged();

                            l_Reply = CRConfig.Instance.Commands.BSRCommand_RequestOK;
                        }
                        else
                        {
                            if (l_IsMapperBanned)
                                l_Reply = CRConfig.Instance.Commands.BSRCommand_MapperBanned;
                            else
                                l_Reply = CRConfig.Instance.Commands.BSRCommand_AlreadyPlayed;
                        }
                    }

                    SendChatMessage(l_Reply, p_Service, p_Message, p_BeatMap);
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
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.LinkCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (p_Params?.Length > 0)
                SendChatMessage(CRConfig.Instance.Commands.BSRHelpCommand_Reply.Replace("$UserName", p_Params[0].Replace("@", "")), p_Service, p_Message);
            else
                SendChatMessage(CRConfig.Instance.Commands.BSRHelpCommand_Reply, p_Service, p_Message);
        }
        private void Command_Link(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.LinkCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            string l_Response = "";
            if (CP_SDK_BS.Game.Logic.LevelData == null
                || CP_SDK_BS.Game.Logic.LevelData?.Data == null
#if BEATSABER_1_35_0_OR_NEWER
                || CP_SDK_BS.Game.Logic.LevelData?.Data.beatmapLevel == null
#else
                || CP_SDK_BS.Game.Logic.LevelData?.Data.difficultyBeatmap == null
#endif
                )
            {
                if (m_LastPlayingLevelResponse == "")
                    l_Response = CRConfig.Instance.Commands.LinkCommand_NoSong;
                else
                    l_Response = CRConfig.Instance.Commands.LinkCommand_LastSong;
            }
            else
                l_Response = CRConfig.Instance.Commands.LinkCommand_CurrentSong;

            if (!string.IsNullOrEmpty(l_Response))
                SendChatMessage(l_Response, p_Service, p_Message, null, ("$SongInfo", m_LastPlayingLevelResponse), ("$SongLink", m_LastPlayingLevelResponseLink));
        }
        private void Command_Queue(IChatService p_Service, IChatMessage p_Message, string[] p_Param)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.QueueCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (!HandleCommandCooldown("Command_Queue", CRConfig.Instance.Commands.QueueCommandCooldownPerUser, CRConfig.Instance.Commands.QueueCommandCooldown, p_Message.Sender))
            {
                SendChatMessage(CRConfig.Instance.Messages.OnCooldown, p_Service, p_Message);
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
                    for (; l_I < SongQueue.Count && l_I < CRConfig.Instance.Commands.QueueCommandShowSize; ++l_I)
                    {
                        if (l_I != 0)
                            l_Reply += ", ";

                        l_Reply += " (bsr " + SongQueue[l_I].BeatSaver_Map.id.ToLower() + ") " + (CRConfig.Instance.SafeMode2 ? string.Empty : SongQueue[l_I].BeatSaver_Map.name);
                    }

                    if (l_I < SongQueue.Count)
                        l_Reply += "...";
                }
                else
                    l_Reply = CRConfig.Instance.Commands.QueueCommand_Empty;
            }

            SendChatMessage(l_Reply, p_Service, p_Message);
        }
        private void Command_QueueStatus(IChatService p_Service, IChatMessage p_Message, string[] p_Param)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.QueueStatusCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            string l_Reply = "";
            lock (SongQueue)
            {
                if (SongQueue.Count != 0)
                {
                    var l_Minutes = QueueDuration / 60;
                    var l_Seconds = (QueueDuration - (l_Minutes * 60));
                    l_Reply = $"@$UserName Song queue is " + (QueueOpen ? "open" : "closed") + $" ({SongQueue.Count} songs {l_Minutes}m{l_Seconds}s)";
                }
                else
                    l_Reply = $"@$UserName Song queue is " + (QueueOpen ? "open" : "closed") + ", no song queued!";
            }

            SendChatMessage(l_Reply, p_Service, p_Message);
        }
        private void Command_Wrong(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.WrongCommandCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_SongEntry = null as Data.SongEntry;

            if (p_Params.Length == 0)
            {
                lock (SongQueue)
                {
                    l_SongEntry = SongQueue.Where(x => x.RequesterName == p_Message.Sender.UserName).LastOrDefault();
                    if (l_SongEntry != null)
                        SongQueue.Remove(l_SongEntry);
                }

                if (l_SongEntry != null)
                {
                    SendChatMessage(CRConfig.Instance.Commands.WrongCommand_Removed, p_Service, p_Message, l_SongEntry.BeatSaver_Map);

                    /// Update request manager
                    OnQueueChanged();
                }
                else
                    SendChatMessage(CRConfig.Instance.Commands.WrongCommand_NoSong, p_Service, p_Message);
            }
            else
            {
                string l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
                if (!string.IsNullOrEmpty(l_Key) && OnlyHexInString(l_Key))
                {
                    l_Key = l_Key.TrimStart('0');
                    lock (SongQueue)
                    {
                        l_SongEntry = SongQueue.Where(x => x.RequesterName == p_Message.Sender.UserName && x.BeatSaver_Map.id == l_Key).LastOrDefault();
                        if (l_SongEntry != null)
                            SongQueue.Remove(l_SongEntry);
                    }
                }

                if (l_SongEntry != null)
                {
                    SendChatMessage(CRConfig.Instance.Commands.WrongCommand_Removed, p_Service, p_Message, l_SongEntry.BeatSaver_Map);

                    /// Update request manager
                    OnQueueChanged();
                }
                else
                    SendChatMessage(CRConfig.Instance.Commands.WrongCommand_NoSongFound, p_Service, p_Message);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_Open(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.OpenCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (QueueOpen)
            {
                SendChatMessage(CRConfig.Instance.Commands.OpenCommand_AlreadyOpen, p_Service, p_Message);
                return;
            }

            ToggleQueueStatus();
        }
        private void Command_Close(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.CloseCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (!QueueOpen)
            {
                SendChatMessage(CRConfig.Instance.Commands.CloseCommand_AlreadyClosed, p_Service, p_Message);
                return;
            }

            ToggleQueueStatus();
        }
        private void Command_Sabotage(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            /// Original implementation :
            /// https://github.com/angturil/SongRequestManager/blob/dev/EnhancedTwitchIntegration/Bot/ChatCommands.cs#L589

            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.SabotageCloseCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Parameter = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            if (l_Parameter == "on" || l_Parameter == "off")
            {
                try
                {
                    System.Diagnostics.Process.Start($"liv-streamerkit://gamechanger/beat-saber-sabotage/" + (l_Parameter == "on" ? "enable" : "disable"));
                    SendChatMessage("The !bomb command is now " + (l_Parameter == "on" ? "enabled!" : "disabled!"), p_Service, p_Message);
                }
                catch
                {
                    SendChatMessage(CRConfig.Instance.Messages.CommandFailed, p_Service, p_Message);
                }
            }
            else
                SendChatMessage(CRConfig.Instance.Messages.BadSyntax, p_Service, p_Message, null, ("$Syntax", "!sabotage on/off"));
        }
        private void Command_SongMessage(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.SongMessageCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (p_Params.Length < 2)
            {
                SendChatMessage(CRConfig.Instance.Messages.BadSyntax, p_Service, p_Message, null, ("$Syntax", "!songmsg #BSRKEY|#REQUESTERNAME #MESSAGE"));
                return;
            }

            var l_Param     = p_Params.Length > 0 ? p_Params[0].ToLower().Trim().TrimStart('@') : "";
            var l_IsKey     = OnlyHexInString(l_Param.Trim());
            var l_Key       = l_Param.TrimStart('0');
            var l_Message   = string.Join(" ", p_Params.Skip(1));

            var l_SongEntry = null as Data.SongEntry;
            lock (SongQueue)
                l_SongEntry = SongQueue.Where(x => l_IsKey ? (l_Key == x.BeatSaver_Map.id.ToLower()) : (l_Param == x.RequesterName.ToLower())).FirstOrDefault();

            if (l_SongEntry != null)
            {
                l_SongEntry.Message = l_Message;

                /// Update request manager
                OnQueueChanged();

                SendChatMessage(CRConfig.Instance.Commands.SongMessage_OK, p_Service, p_Message);
            }
            else
                SendChatMessage(CRConfig.Instance.Commands.SongMessage_NotFound, p_Service, p_Message, null, ("$Subject", l_IsKey ? l_Key : l_Param));
        }
        private void Command_MoveToTop(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.MoveToTopCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (p_Params.Length < 1)
            {
                SendChatMessage(CRConfig.Instance.Messages.BadSyntax, p_Service, p_Message, null, ("$Syntax", "!mtt #BSRKEY|#REQUESTERNAME"));
                return;
            }

            var l_Param = p_Params.Length > 0 ? p_Params[0].ToLower().Trim().TrimStart('@') : "";
            var l_IsKey = OnlyHexInString(l_Param.Trim());
            var l_Key   = l_Param.TrimStart('0');

            var l_SongEntry = null as Data.SongEntry;
            lock (SongQueue)
            {
                l_SongEntry = SongQueue.Where(x => l_IsKey ? (l_Key == x.BeatSaver_Map.id.ToLower()) : (l_Param == x.RequesterName.ToLower())).FirstOrDefault();

                if (l_SongEntry != null)
                {
                    SongQueue.Remove(l_SongEntry);
                    SongQueue.Insert(0, l_SongEntry);
                }
            }

            if (l_SongEntry != null)
            {
                SendChatMessage(CRConfig.Instance.Commands.MoveToTopCommand_OK, p_Service, p_Message, l_SongEntry.BeatSaver_Map, ("$$RequesterName", l_SongEntry.RequesterName));

                /// Update request manager
                OnQueueChanged();
            }
            else
                SendChatMessage(CRConfig.Instance.Commands.MoveToTopCommand_NotFound, p_Service, p_Message, null, ("$Subject", l_IsKey ? l_Key : l_Param));
        }
        private void Command_Remove(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.RemoveCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (p_Params.Length < 1)
            {
                SendChatMessage(CRConfig.Instance.Messages.BadSyntax, p_Service, p_Message, null, ("$Syntax", "!remove #BSRKEY|#REQUESTERNAME"));
                return;
            }

            var l_Param = p_Params.Length > 0 ? p_Params[0].ToLower().Trim().TrimStart('@') : "";
            var l_IsKey = OnlyHexInString(l_Param.Trim());
            var l_Key   = l_Param.TrimStart('0');

            var l_SongEntry = null as Data.SongEntry;
            lock (SongQueue)
            {
                l_SongEntry = SongQueue.Where(x => l_IsKey ? (l_Key == x.BeatSaver_Map.id.ToLower()) : (l_Param == x.RequesterName.ToLower())).FirstOrDefault();

                if (l_SongEntry != null)
                    SongQueue.Remove(l_SongEntry);
            }

            if (l_SongEntry != null)
            {
                SendChatMessage(CRConfig.Instance.Commands.RemoveCommand_OK, p_Service, p_Message, l_SongEntry.BeatSaver_Map, ("$RequesterName", l_SongEntry.RequesterName));

                /// Update request manager
                OnQueueChanged();
            }
            else
                SendChatMessage(CRConfig.Instance.Commands.RemoveCommand_NotFound, p_Service, p_Message, null, ("$Subject", l_IsKey ? l_Key : l_Param));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_BanUser(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.BsrBanCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Parameter         = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            var l_TargetUserName    = (l_Parameter.StartsWith("@") ? l_Parameter.Substring(1) : l_Parameter).ToLower();

            lock (BannedUsers)
            {
                if (BannedUsers.Count(x => x.ToLower() == l_TargetUserName) != 0)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BsrBanCommand_AlreadyIn, p_Service, p_Message, null, ("$l_TargetUserName", l_TargetUserName));
                    return;
                }

                BannedUsers.Add(l_TargetUserName);
                SendChatMessage(CRConfig.Instance.Commands.BsrBanCommand_OK, p_Service, p_Message, null, ("$l_TargetUserName", l_TargetUserName));
            }
        }
        private void Command_UnBanUser(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.BsrUnbanCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Parameter         = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            var l_TargetUserName    = (l_Parameter.StartsWith("@") ? l_Parameter.Substring(1) : l_Parameter).ToLower();

            lock (BannedUsers)
            {
                if (BannedUsers.Count(x => x.ToLower() == l_TargetUserName) == 0)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BsrUnbanCommand_NotFound, p_Service, p_Message, null, ("$TargetUserName", l_TargetUserName));
                    return;
                }

                BannedUsers.RemoveAll(x => x == l_TargetUserName);
                SendChatMessage(CRConfig.Instance.Commands.BsrUnbanCommand_OK, p_Service, p_Message, null, ("$TargetUserName", l_TargetUserName));
            }

            OnQueueChanged(false, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_BanMapper(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.BsrBanMapperCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Parameter = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            var l_MapperName = (l_Parameter.StartsWith("@") ? l_Parameter.Substring(1) : l_Parameter).ToLower();

            lock (BannedMappers)
            {
                if (BannedMappers.Count(x => x.ToLower() == l_MapperName) != 0)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BsrBanMapperCommand_AlreadyIn, p_Service, p_Message, null, ("$MapperName", l_MapperName));
                    return;
                }

                BannedMappers.Add(l_MapperName);
                SendChatMessage(CRConfig.Instance.Commands.BsrBanMapperCommand_OK, p_Service, p_Message, null, ("$MapperName", l_MapperName));
            }
        }
        private void Command_UnBanMapper(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.BsrUnbanMapperCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Parameter = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            var l_MapperName = (l_Parameter.StartsWith("@") ? l_Parameter.Substring(1) : l_Parameter).ToLower();

            lock (BannedMappers)
            {
                if (BannedMappers.Count(x => x.ToLower() == l_MapperName) == 0)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BsrUnbanMapperCommand_NotIn, p_Service, p_Message, null, ("$MapperName", l_MapperName));
                    return;
                }

                BannedMappers.RemoveAll(x => x == l_MapperName);
                SendChatMessage(CRConfig.Instance.Commands.BsrUnbanMapperCommand_OK, p_Service, p_Message, null, ("$MapperName", l_MapperName));
            }

            OnQueueChanged(false, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_Remap(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.RemapCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (p_Params.Length != 2 || !OnlyHexInString(p_Params[0]) || !OnlyHexInString(p_Params[1]))
            {
                SendChatMessage(CRConfig.Instance.Messages.BadSyntax, p_Service, p_Message, null, ("$Syntax", "!remap #BSR #BSR"));
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

                    SendChatMessage(CRConfig.Instance.Commands.RemapCommand_OK, p_Service, p_Message, null, ("$Source", p_Params[0]), ("$Target", p_Params[1]));
                }
                else if (Remaps.ContainsKey(p_Params[0]))
                {
                    Remaps.Remove(p_Params[0]);
                    SendChatMessage(CRConfig.Instance.Commands.RemapCommand_OKRemoved, p_Service, p_Message, null, ("$Source", p_Params[0]));
                }
                else
                    SendChatMessage(CRConfig.Instance.Commands.RemapCommand_NotFound, p_Service, p_Message, null, ("$Source", p_Params[0]));
            }

            OnQueueChanged(false, false);
        }
        private void Command_Allow(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.AllowCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            if (!OnlyHexInString(l_Key))
            {
                SendChatMessage(CRConfig.Instance.Messages.BadSyntax, p_Service, p_Message, null, ("$Syntax", "!allow #BSR"));
                return;
            }

            lock (AllowList)
            {
                if (!AllowList.Contains(l_Key))
                    AllowList.Add(l_Key);

                SendChatMessage(CRConfig.Instance.Commands.AllowCommand_OK, p_Service, p_Message, null, ("$BSRKey", l_Key));
            }

            OnQueueChanged(false, false);
        }
        private void Command_Block(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasPower(p_Message.Sender, CRConfig.Instance.Commands.BlockCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            if (!OnlyHexInString(l_Key))
            {
                SendChatMessage(CRConfig.Instance.Commands.BlockCommand_InvalidKey, p_Service, p_Message);
                return;
            }

            lock (SongBlackList)
            {
                var l_SongEntry = SongBlackList.Where(x => x.BeatSaver_Map.id.ToLower() == l_Key).FirstOrDefault();
                if (l_SongEntry != null)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BlockCommand_AlreadyBlacklisted, p_Service, p_Message, l_SongEntry.BeatSaver_Map);
                    return;
                }
            }

            /// Fetch beatmap
            CP_SDK_BS.Game.BeatMapsClient.GetOnlineByKey(l_Key, (p_Valid, p_BeatMap) =>
            {
                try
                {
                    string l_Reply = "@$UserName map " + l_Key + " not found.";
                    if (p_Valid && p_BeatMap != null)
                    {
                        l_Reply = "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is now blacklisted!";

                        lock (SongQueue) { lock (SongHistory) { lock (SongBlackList) {
                            SongQueue.RemoveAll(x => x.BeatSaver_Map.id   == p_BeatMap.id);
                            SongHistory.RemoveAll(x => x.BeatSaver_Map.id == p_BeatMap.id);

                            /// Add to blacklist
                            SongBlackList.RemoveAll(x => x.BeatSaver_Map.id == p_BeatMap.id);
                            SongBlackList.Insert(0, new Data.SongEntry() { BeatSaver_Map = p_BeatMap, TitlePrefix = "🗡", RequesterName = p_Message.Sender.DisplayName });
                        } } }

                        /// Update request manager
                        OnQueueChanged();
                    }

                    SendChatMessage(l_Reply, p_Service, p_Message, p_BeatMap);
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
