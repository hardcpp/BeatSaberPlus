using CP_SDK.Extensions;
using CP_SDK.Chat.Interfaces;
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
            if (l_Config.AllowlistCommandEnabled)     RegisterCommand(l_Config.AllowlistCommand,        Command_AllowList);
            if (l_Config.BlocklistCommandEnabled)     RegisterCommand(l_Config.BlocklistCommand,        Command_Blocklist);
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
        /// <param name="name">Name or names</param>
        /// <param name="callback">Callback method</param>
        private void RegisterCommand(string name, Action<IChatService, IChatMessage, string[]> callback)
        {
            foreach (var l_Current in name.Split(','))
            {
                var l_Name = l_Current.ToLower().Trim().Replace("!", "");
                if (!m_CommandTable.ContainsKey(l_Name))
                    m_CommandTable.Add(l_Name, callback);
            }
        }
        /// <summary>
        /// Handle command cooldown
        /// </summary>
        /// <param name="identifier">Command identifier</param>
        /// <param name="perUser">Is it a per user based cooldown</param>
        /// <param name="cooldownSeconds">Cooldown in seconds</param>
        /// <param name="user">Context user</param>
        /// <returns>True if the cooldown is allowed</returns>
        private bool HandleCommandCooldown(string identifier, bool perUser, int cooldownSeconds, IChatUser user)
        {
            var l_Now = CP_SDK.Misc.Time.UnixTimeNow();
            var l_Key = perUser ? user.Id : "$@_GLOBAL";

            lock (m_Cooldowns)
            {
                if (!m_Cooldowns.TryGetValue(identifier, out var l_Container))
                {
                    l_Container = new Dictionary<string, long>();
                    m_Cooldowns[identifier] = l_Container;
                }

                if (!l_Container.ContainsKey(l_Key))
                {
                    l_Container[l_Key] = l_Now;
                    return true;
                }

                if ((l_Now - l_Container[l_Key]) < cooldownSeconds)
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
        /// <param name="service">Chat service</param>
        /// <param name="message">ID of the message</param>
        private void ChatCoreMutiplixer_OnTextMessageReceived(IChatService service, IChatMessage message)
        {
            if (message.Channel.IsTemp || message.Message.Length < 2 || message.Message[0] != '!')
                return;

            var l_Parts         = Regex.Split(message.Message, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            var l_Command       = l_Parts[0]?.ToLower().Remove(0, 1) ?? "";
            var l_Parameters    = new List<string>(l_Parts);

            if (l_Parameters.Count != 0)
                l_Parameters.RemoveAt(0);

            if (m_CommandTable.TryGetValue(l_Command, out var l_CommandBlock))
                l_CommandBlock?.Invoke(service, message, l_Parameters.ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_BSR(
            IChatService                    service,
            IChatMessage                    message,
            string[]                        @params,
            bool                            modAddCommand,
            bool                            addToTopCommand,
            CRConfig._Commands.EPermission  permissions     = CRConfig._Commands.EPermission.Viewers)
        {
            if (!HasRequesterPermission(message.Sender, permissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, service, message);
                return;
            }

            if (!HandleCommandCooldown("Command_BSR", CRConfig.Instance.Commands.BSRCommandCooldownPerUser, CRConfig.Instance.Commands.BSRCommandCooldown, message.Sender))
            {
                SendChatMessage(CRConfig.Instance.Messages.OnCooldown, service, message);
                return;
            }

            string l_Key = @params.Length > 0 ? @params[0].ToLower() : "";

            if (!l_Key.IsOnlyHexSymbols())
            {
                if (CRConfig.Instance.SafeMode2)
                {
                    SendChatMessage(CRConfig.Instance.Commands.BSRCommand_SearchDisabled, service, message);
                    return;
                }

                CP_SDK_BS.Game.BeatMapsClient.GetOnlineBySearch(l_Key, (p_Valid, p_SearchTaskResult) =>
                {
                    if (!p_Valid || p_SearchTaskResult.Length == 0)
                    {
                        SendChatMessage(CRConfig.Instance.Commands.BSRCommand_Search0Result, service, message, null, ("$Search", l_Key));
                        return;
                    }

                    var l_Results = "";

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
                                    .Replace("$Search", l_Key)
                                    .Replace("$Count", p_SearchTaskResult.Length.ToString())
                                    .Replace("$Results", l_Results);

                    SendChatMessage(l_Reply, service, message);
                });

                return;
            }
            else
                l_Key = l_Key.TrimStart('0');

            var l_OnBehalfOf = string.Empty;
            if (modAddCommand && @params.Length == 2 && @params[1].Length > 3 && @params[1][0] == '@')
                l_OnBehalfOf = @params[1].Substring(1);

            AddToQueueFromBSRKey(
                bsrKey:         l_Key,
                requester:      message.Sender,
                onBehalfOf:     l_OnBehalfOf,
                forceNamePrefix: string.Empty,
                asModAdd:       modAddCommand,
                addToTop:       addToTopCommand,
                callback:       (p_Result) =>
                {
                    switch (p_Result.Result)
                    {
                        case Models.EAddToQueueResult.OK:/*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_RequestOK,
                                service,
                                message,
                                p_Result.FoundOrCreatedSongEntry
                            );
                            break;

                        case Models.EAddToQueueResult.QueueClosed: /*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_QueueClosed,
                                service,
                                message
                            );
                            break;

                        case Models.EAddToQueueResult.MapBanned:/*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_Blocklisted,
                                service,
                                message,
                                p_Result.FoundOrCreatedSongEntry
                            );
                            break;

                        case Models.EAddToQueueResult.RequesterBanned: /*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_UserBanned,
                                service,
                                message
                            );
                            break;

                        case Models.EAddToQueueResult.MapperBanned: /*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_MapperBanned,
                                service,
                                message,
                                p_Result.FoundOrCreatedSongEntry
                            );
                            break;

                        case Models.EAddToQueueResult.AlreadyRequestedThisSession:/*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_AlreadyPlayed,
                                service,
                                message
                            );
                            break;

                        case Models.EAddToQueueResult.AlreadyInQueue:/*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_AlreadyQueued,
                                service,
                                message,
                                p_Result.FoundOrCreatedSongEntry
                            );
                            break;

                        case Models.EAddToQueueResult.RequestLimit:  /*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_RequestLimit,
                                service,
                                message,
                                null,
                                ("$UserName",           message.Sender.UserName),
                                ("$UserRequestCount",   p_Result.RateLimit.CurrentRequestCount.ToString()),
                                ("$UserTypeLimit",      p_Result.RateLimit.RequestMaxCount.ToString()),
                                ("$UsersLimit",         p_Result.RateLimit.RequestMaxCount.ToString()), /*Fix old variable*/
                                ("$UserType",           p_Result.RateLimit.RequestLimitType)
                            );
                            break;

                        case Models.EAddToQueueResult.NotFound: /*TESTED*/
                            SendChatMessage(
                                CRConfig.Instance.Commands.BSRCommand_NotFound,
                                service,
                                message,
                                null,
                                ("$BSRKey", l_Key)
                            );
                            break;

                        case Models.EAddToQueueResult.FilterError: /*TESTED*/
                            SendChatMessage(
                                p_Result.FilterError,
                                service,
                                message,
                                p_Result.FoundOrCreatedSongEntry
                            );
                            break;

                    }
                }
            );
        }
        private void Command_BSRHelp(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.LinkCommandPermissions))
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.LinkCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            string l_Response = "";
            if (CP_SDK_BS.Game.Logic.LevelData == null
                || CP_SDK_BS.Game.Logic.LevelData?.Data == null
                || CP_SDK_BS.Game.Logic.LevelData?.Data.beatmapLevel == null
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.QueueCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (!HandleCommandCooldown("Command_Queue", CRConfig.Instance.Commands.QueueCommandCooldownPerUser, CRConfig.Instance.Commands.QueueCommandCooldown, p_Message.Sender))
            {
                SendChatMessage(CRConfig.Instance.Messages.OnCooldown, p_Service, p_Message);
                return;
            }

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

                        l_Reply += " (bsr " + (SongQueue[l_I].BeatSaver_Map?.id.ToLower() ?? "<unk>") + ") " + (CRConfig.Instance.SafeMode2 ? string.Empty : (SongQueue[l_I].GetSongName()));
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.QueueStatusCommandPermissions))
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.WrongCommandCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_SongEntry = null as Models.SongEntry;

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
                    SendChatMessage(CRConfig.Instance.Commands.WrongCommand_Removed, p_Service, p_Message, l_SongEntry);

                    /// Update request manager
                    OnQueueChanged();
                }
                else
                    SendChatMessage(CRConfig.Instance.Commands.WrongCommand_NoSong, p_Service, p_Message);
            }
            else
            {
                string l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
                if (!string.IsNullOrEmpty(l_Key) && l_Key.IsOnlyHexSymbols())
                {
                    l_Key = l_Key.TrimStart('0');
                    lock (SongQueue)
                    {
                        l_SongEntry = SongQueue.LastOrDefault(x => x.RequesterName == p_Message.Sender.UserName && x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(l_Key, StringComparison.OrdinalIgnoreCase));
                        if (l_SongEntry != null)
                            SongQueue.Remove(l_SongEntry);
                    }
                }

                if (l_SongEntry != null)
                {
                    SendChatMessage(CRConfig.Instance.Commands.WrongCommand_Removed, p_Service, p_Message, l_SongEntry);

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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.OpenCommandPermissions))
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.CloseCommandPermissions))
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

            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.SabotageCloseCommandPermissions))
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.SongMessageCommandPermissions))
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
            var l_IsKey     = l_Param.Trim().IsOnlyHexSymbols();
            var l_Key       = l_Param.TrimStart('0');
            var l_Message   = string.Join(" ", p_Params.Skip(1));

            var l_SongEntry = null as Models.SongEntry;
            lock (SongQueue)
                l_SongEntry = SongQueue.FirstOrDefault(x => l_IsKey ? (x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(l_Key, StringComparison.OrdinalIgnoreCase)) : (x.RequesterName.Equals(l_Param, StringComparison.OrdinalIgnoreCase)));

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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.MoveToTopCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (p_Params.Length < 1)
            {
                SendChatMessage(
                    CRConfig.Instance.Messages.BadSyntax,
                    p_Service,
                    p_Message,
                    null,
                    ("$Syntax", "!mtt #BSRKEY|#REQUESTERNAME")
                );
                return;
            }

            var l_Param = p_Params.Length > 0 ? p_Params[0].ToLower().Trim().TrimStart('@') : "";
            var l_IsKey = l_Param.Trim().IsOnlyHexSymbols();
            var l_Key   = l_Param.TrimStart('0');

            var l_SongEntry = null as Models.SongEntry;
            lock (SongQueue)
            {
                l_SongEntry = SongQueue.FirstOrDefault(x => l_IsKey ? (x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(l_Key, StringComparison.OrdinalIgnoreCase)) : (x.RequesterName.Equals(l_Param, StringComparison.OrdinalIgnoreCase)));

                if (l_SongEntry != null)
                {
                    SongQueue.Remove(l_SongEntry);
                    SongQueue.Insert(0, l_SongEntry);
                }
            }

            if (l_SongEntry != null)
            {
                SendChatMessage(
                    CRConfig.Instance.Commands.MoveToTopCommand_OK,
                    p_Service,
                    p_Message,
                    l_SongEntry
                );

                /// Update request manager
                OnQueueChanged();
            }
            else
                SendChatMessage(CRConfig.Instance.Commands.MoveToTopCommand_NotFound, p_Service, p_Message, null, ("$Subject", l_IsKey ? l_Key : l_Param));
        }
        private void Command_Remove(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.RemoveCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (p_Params.Length < 1)
            {
                SendChatMessage(
                    CRConfig.Instance.Messages.BadSyntax,
                    p_Service,
                    p_Message,
                    null,
                    ("$Syntax", "!remove #BSRKEY|#REQUESTERNAME")
                );
                return;
            }

            var l_Param = p_Params.Length > 0 ? p_Params[0].ToLower().Trim().TrimStart('@') : "";
            var l_IsKey = l_Param.Trim().IsOnlyHexSymbols();
            var l_Key   = l_Param.TrimStart('0');

            var l_SongEntry = null as Models.SongEntry;
            lock (SongQueue)
            {
                l_SongEntry = SongQueue.FirstOrDefault(x => l_IsKey ? (x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(l_Key, StringComparison.OrdinalIgnoreCase)) : (x.RequesterName.Equals(l_Param, StringComparison.OrdinalIgnoreCase)));

                if (l_SongEntry != null)
                    SongQueue.Remove(l_SongEntry);
            }

            if (l_SongEntry != null)
            {
                SendChatMessage(
                    CRConfig.Instance.Commands.RemoveCommand_OK,
                    p_Service,
                    p_Message,
                    l_SongEntry,
                    ("$RequesterName", l_SongEntry.RequesterName)
                );

                /// Update request manager
                OnQueueChanged();
            }
            else
            {
                SendChatMessage(
                    CRConfig.Instance.Commands.RemoveCommand_NotFound,
                    p_Service,
                    p_Message,
                    null,
                    ("$Subject", l_IsKey ? l_Key : l_Param)
                );
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Command_BanUser(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.BsrBanCommandPermissions))
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.BsrUnbanCommandPermissions))
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.BsrBanMapperCommandPermissions))
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.BsrUnbanMapperCommandPermissions))
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
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.RemapCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            if (p_Params.Length != 2 || !p_Params[0].IsOnlyHexSymbols() || !p_Params[1].IsOnlyHexSymbols())
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
        private void Command_AllowList(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.AllowlistCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            if (!l_Key.IsOnlyHexSymbols())
            {
                SendChatMessage(CRConfig.Instance.Messages.BadSyntax, p_Service, p_Message, null, ("$Syntax", "!allow #BSR"));
                return;
            }

            lock (SongAllowlist)
            {
                var l_SongEntry = SongAllowlist.FirstOrDefault(x => x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(l_Key, StringComparison.OrdinalIgnoreCase));
                if (l_SongEntry != null)
                {
                    SendChatMessage(
                        CRConfig.Instance.Commands.AllowlistCommand_AlreadyAllowlisted,
                        p_Service,
                        p_Message,
                        l_SongEntry
                    );
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
                        var l_Entry = new Models.SongEntry()
                        {
                            BeatSaver_Map   = p_BeatMap,
                            TitlePrefix     = "🗡",
                            RequesterName   = p_Message.Sender.DisplayName
                        };

                        AddSongEntryToAllowlist(l_Entry);

                        SendChatMessage(
                            "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is now in the allowlist!",
                            p_Service,
                            p_Message,
                            l_Entry
                        );
                    }
                    else
                        SendChatMessage("@$UserName map $BSRKey not found.", p_Service, p_Message, null, ("$BSRKey", l_Key));
                }
                catch (System.Exception p_Exception)
                {
                    Logger.Instance.Error("Command_Block");
                    Logger.Instance.Error(p_Exception);
                }
            });
        }
        private void Command_Blocklist(IChatService p_Service, IChatMessage p_Message, string[] p_Params)
        {
            if (!HasRequesterPermission(p_Message.Sender, CRConfig.Instance.Commands.BlocklistCommandPermissions))
            {
                SendChatMessage(CRConfig.Instance.Messages.NoPermissions, p_Service, p_Message);
                return;
            }

            var l_Key = p_Params.Length > 0 ? p_Params[0].ToLower() : "";
            if (!l_Key.IsOnlyHexSymbols())
            {
                SendChatMessage(CRConfig.Instance.Messages.BadSyntax, p_Service, p_Message, null, ("$Syntax", "!allow #BSR"));
                return;
            }

            lock (SongBlocklist)
            {
                var l_SongEntry = SongBlocklist.FirstOrDefault(x => x.BeatSaver_Map != null && x.BeatSaver_Map.id.Equals(l_Key, StringComparison.OrdinalIgnoreCase));
                if (l_SongEntry != null)
                {
                    SendChatMessage(
                        CRConfig.Instance.Commands.BlocklistCommand_AlreadyBlocklisted,
                        p_Service,
                        p_Message,
                        l_SongEntry
                    );
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
                        var l_Entry = new Models.SongEntry()
                        {
                            BeatSaver_Map   = p_BeatMap,
                            TitlePrefix     = "🗡",
                            RequesterName   = p_Message.Sender.DisplayName
                        };

                        AddSongEntryToBlocklist(l_Entry);

                        SendChatMessage(
                            "@$UserName (bsr $BSRKey) $SongName / $LevelAuthorName is now in the blocklist!",
                            p_Service,
                            p_Message,
                            l_Entry
                        );
                    }
                    else
                        SendChatMessage("@$UserName map $BSRKey not found.", p_Service, p_Message, null, ("$BSRKey", l_Key));
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
