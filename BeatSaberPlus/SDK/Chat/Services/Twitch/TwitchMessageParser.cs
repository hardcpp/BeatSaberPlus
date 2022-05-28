using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models.Twitch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BeatSaberPlus.SDK.Chat.Services.Twitch
{
    /// <summary>
    /// IRC twitch message parser
    /// </summary>
    public class TwitchMessageParser
    {
        private TwitchService m_Service;
        /// <summary>
        /// Twitch resource data provider
        /// </summary>
        private TwitchDataProvider m_TwitchDataProvider;
        /// <summary>
        /// Emoji parser
        /// </summary>
        private IEmojiParser m_EmojiParser;
        /// <summary>
        /// Message split token
        /// </summary>
        private string[] m_SplitToken = new string[] { "\r\n" };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public TwitchMessageParser(TwitchService p_Service, TwitchDataProvider p_TwitchDataProvider, IEmojiParser p_EmojiParser)
        {
            m_Service               = p_Service;
            m_TwitchDataProvider    = p_TwitchDataProvider;
            m_EmojiParser           = p_EmojiParser;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Takes a raw Twitch message and parses it into an IChatMessage
        /// </summary>
        /// <param name="p_RawMessage">The raw message sent from Twitch</param>
        /// <param name="p_ParsedMessages">A list of chat messages that were parsed from the rawMessage</param>
        /// <returns>True if parsedMessages.Count > 0</returns>
        public bool ParseRawMessage(string p_RawMessages, ConcurrentDictionary<string, TwitchChannel> p_ChannelInfo, TwitchUser p_LoggedInUser, List<TwitchMessage> p_ParsedMessages, string p_LoggedInUsername)
        {
#if DEBUG
            Stopwatch l_Stopwatch = Stopwatch.StartNew();
#endif

            var l_Tags          = Pool.MTDictionaryPool<string, string>.Get();
            var l_RawMessages   = p_RawMessages.Split(m_SplitToken, StringSplitOptions.RemoveEmptyEntries);
            var l_Row           = 0;

            do
            {
                var l_RawMessage    = l_RawMessages[l_Row++];
                var l_Position      = 0;
                var l_Prefix        = string.Empty;
                var l_MessageType   = string.Empty;
                var l_ChannelName   = string.Empty;
                var l_MessageText   = string.Empty;
                var l_SkipFinalAdd  = false;

                l_Tags.Clear();

                /// Tags
                if (l_RawMessage[l_Position] == '@')
                {
                    var l_NextBlock = l_RawMessage.IndexOf(" ", l_Position);
                    if (l_NextBlock == -1)
                        continue;

                    for (var l_I = l_Position + 1; l_I < l_NextBlock; ++l_I)
                    {
                        var l_Split = l_RawMessage.IndexOf('=', l_I);
                        var l_End   = l_RawMessage.IndexOf(';', l_Split);

                        if (l_End == -1)
                            l_End = l_NextBlock;

                        var l_Key = l_RawMessage.Substring(l_I,         l_Split - l_I);
                        var l_Val = l_RawMessage.Substring(l_Split + 1, (l_End - l_Split) - 1);

                        if (!string.IsNullOrEmpty(l_Val))
                            l_Tags.Add(l_Key, l_Val);

                        l_I = l_End;
                    }

                    l_Position = l_NextBlock + 1;
                }

                /// Prefix
                if (l_RawMessage[l_Position] == ':')
                {
                    var l_NextBlock = l_RawMessage.IndexOf(" ", l_Position);
                    if (l_NextBlock == -1)
                        continue;

                    l_Prefix = l_RawMessage.Substring(l_Position + 1, (l_NextBlock - l_Position) - 1);

                    l_Position = l_NextBlock + 1;
                }

                /// Message type
                if (l_Position < l_RawMessage.Length)
                {
                    var l_NextBlock = l_RawMessage.IndexOf(" ", l_Position);
                    if (l_NextBlock == -1)
                    {
                        l_MessageType   = l_RawMessage.Substring(l_Position);
                        l_Position      = l_RawMessage.Length;
                    }
                    else
                    {
                        l_MessageType   = l_RawMessage.Substring(l_Position, l_NextBlock - l_Position);
                        l_Position      = l_NextBlock + 1;
                    }
                }

                /// Channel
                if (l_Position < l_RawMessage.Length)
                {
                    var l_NextBlock = l_RawMessage.IndexOf(" ", l_Position);
                    if (l_NextBlock == -1)
                    {
                        l_ChannelName   = l_RawMessage.Substring(l_Position + 1);
                        l_Position      = l_RawMessage.Length;
                    }
                    else
                    {
                        var l_Offset    = l_RawMessage[l_Position] == '#' ? 1 : 0;
                        l_ChannelName   = l_RawMessage.Substring(l_Position + l_Offset, (l_NextBlock - l_Position) - l_Offset);
                        l_Position      = l_NextBlock + 2;
                    }
                }

                /// Content
                if (l_Position < l_RawMessage.Length)
                {
                    l_MessageText   = l_RawMessage.Substring(l_Position);
                    l_Position      = l_RawMessage.Length;
                }

                var l_MessageRoomId = "";
                var l_Channel = null as TwitchChannel;
                if (!string.IsNullOrEmpty(l_ChannelName) && !p_ChannelInfo.TryGetValue(l_ChannelName, out l_Channel))
                {
                    l_Channel = new TwitchChannel()
                    {
                        Id   = l_ChannelName,
                        Name = l_ChannelName,
                    };
                    l_MessageRoomId = l_Channel.Roomstate.RoomId;
                }
                else if (l_Channel != null)
                    l_MessageRoomId = l_Channel.Roomstate.RoomId;
                else
                {
                    if (l_MessageType == "GLOBALUSERSTATE" && l_Tags.TryGetValue("user-id", out var l_UserID))
                    {
                        l_MessageRoomId = l_UserID;
                        l_Channel       = p_ChannelInfo.Values.FirstOrDefault(x => x.Roomstate.RoomId == l_MessageRoomId);
                    }
                }

                try
                {
                    var l_IsActionMessage = false;
                    if (l_MessageText.StartsWith("\u0001ACTION"))
                    {
                        l_MessageText       = l_MessageText.Remove(l_MessageText.Length - 1, 1).Remove(0, 8);
                        l_IsActionMessage   = true;
                    }

                    int l_MessageBits = l_Tags.TryGetValue("bits", out var l_BitsString) && int.TryParse(l_BitsString, out var l_ParsedBits) ? l_ParsedBits : 0;

                    var l_Emotes = null as IChatEmote[];
                    if (l_MessageType == "PRIVMSG" || l_MessageType == "NOTIFY" || l_MessageType == "USERNOTICE")
                        l_Emotes = GetEmotes(l_Tags, l_MessageText, l_MessageRoomId, l_ChannelName, l_MessageBits);
                    else if (l_MessageType == "ROOMSTATE")
                    {
                        var l_RoomState = l_Channel.Roomstate;
                        if (l_Tags.TryGetValue("broadcaster-lang",  out var l_Lang))        l_RoomState.BroadcasterLang = l_Lang;
                        if (l_Tags.TryGetValue("room-id",           out var l_RoomId))      l_RoomState.RoomId          = l_RoomId;
                        if (l_Tags.TryGetValue("emote-only",        out var l_EmoteOnly))   l_RoomState.EmoteOnly       = l_EmoteOnly   == "1";
                        if (l_Tags.TryGetValue("r9k",               out var l_R9K))         l_RoomState.R9K             = l_R9K         == "1";
                        if (l_Tags.TryGetValue("subs-only",         out var l_SubsOnly))    l_RoomState.SubscribersOnly = l_SubsOnly    == "1";
                        if (l_Tags.TryGetValue("followers-only",    out var l_FollowersOnly))
                        {
                            l_RoomState.FollowersOnly = l_FollowersOnly != "-1";
                            l_RoomState.MinFollowTime = l_FollowersOnly != "-1" && int.TryParse(l_FollowersOnly, out var l_MinFollowTime) ? l_MinFollowTime : 0;
                        }
                        if (l_Tags.TryGetValue("slow",              out var l_Slow) && int.TryParse(l_Slow, out var l_SlowModeInterval))
                            l_RoomState.SlowModeInterval = l_SlowModeInterval;

                        l_Channel.Roomstate = l_RoomState;
                    }

                    var l_NewMessage = new TwitchMessage()
                    {
                        Id                  = l_Tags.TryGetValue("id", out var l_MessageId) ? l_MessageId : "", /// TODO: default id of some sort?
                        Sender              = GetAndUpdateUser(l_Tags, l_Prefix, l_MessageRoomId, (l_MessageType == "PRIVMSG" || l_MessageType == "USERSTATE") ? p_LoggedInUsername : string.Empty),
                        Channel             = l_Channel,
                        Emotes              = l_Emotes,
                        Message             = l_MessageText,
                        IsActionMessage     = l_IsActionMessage,
                        IsSystemMessage     = l_MessageType == "NOTICE" || l_MessageType == "USERNOTICE",
                        IsHighlighted       = false,
                        IsPing              = !string.IsNullOrEmpty(l_MessageText) && p_LoggedInUser != null && l_MessageText.Contains("@" + p_LoggedInUser.DisplayName, StringComparison.OrdinalIgnoreCase),
                        Bits                = l_MessageBits,
                        TargetUserId        = l_Tags.ContainsKey("target-user-id") ? l_Tags["target-user-id"] : string.Empty,
                        TargetMsgId         = l_Tags.ContainsKey("target-msg-id") ? l_Tags["target-msg-id"] : string.Empty,
                        Type                = l_MessageType
                    };

                    if (l_Tags.TryGetValue("msg-id", out var l_MsgIdValue))
                    {
                        TwitchMessage l_SystemMessage = null;

                        switch(l_MsgIdValue)
                        {
                            case "skip-subs-mode-message":
                                l_SystemMessage                     = (TwitchMessage)l_NewMessage.Clone();
                                l_SystemMessage.Message             = "Redeemed Send a Message In Sub-Only Mode";
                                l_SystemMessage.IsHighlighted       = false;
                                l_SystemMessage.IsSystemMessage     = true;
                                l_SystemMessage.Emotes              = null;

                                p_ParsedMessages.Add(l_SystemMessage);
                                break;

                            case "highlighted-message":
                                l_SystemMessage                     = (TwitchMessage)l_NewMessage.Clone();
                                l_SystemMessage.Message             = "Redeemed Highlight My Message";
                                l_SystemMessage.IsHighlighted       = true;
                                l_SystemMessage.IsSystemMessage     = true;
                                l_SystemMessage.Emotes              = null;

                                p_ParsedMessages.Add(l_SystemMessage);
                                break;

                            ///case "sub":
                            ///case "resub":
                            ///case "raid":
                            default:
                                if (l_Tags.TryGetValue("system-msg", out var l_SystemMsgText))
                                {
                                    l_SystemMessage                 = (TwitchMessage)l_NewMessage.Clone();
                                    l_SystemMsgText                 = l_SystemMsgText.Replace(@"\s", " ");
                                    l_SystemMessage.IsHighlighted   = true;
                                    l_SystemMessage.IsSystemMessage = true;

                                    if (l_MsgIdValue == "raid")
                                    {
                                        l_SystemMessage.IsRaid = true;

                                        if (l_Tags.TryGetValue("msg-param-viewerCount", out var l_RaidViewerCountStr)
                                            && int.TryParse(l_RaidViewerCountStr, out var l_RaidViewerCount))
                                            l_SystemMessage.RaidViewerCount = l_RaidViewerCount;
                                        else
                                            l_SystemMessage.RaidViewerCount = 0;
                                    }

                                    ///Logger.Instance.Information($"Message: {match.Value}");
                                    if (l_Tags.TryGetValue("msg-param-sub-plan", out var l_SubPlanName))
                                    {
                                        if (l_SubPlanName == "Prime")
                                            l_SystemMessage.Message = $"👑  {l_SystemMsgText}";
                                        else
                                            l_SystemMessage.Message = $"⭐  {l_SystemMsgText}";

                                        var l_NewEmotes = new List<IChatEmote>();
                                        m_EmojiParser.FindEmojis(l_SystemMessage.Message, l_NewEmotes);
                                        l_SystemMessage.Emotes = l_NewEmotes.ToArray();
                                    }
                                    else if (l_Tags.TryGetValue("msg-param-profileImageURL", out var l_ProfileImage) && l_Tags.TryGetValue("msg-param-login", out var l_LoginUser))
                                    {
                                        var l_EmoteId = $"ProfileImage_{l_LoginUser}";
                                        l_SystemMessage.Emotes = new IChatEmote[]
                                        {
                                            new TwitchEmote()
                                            {
                                                Id              = l_EmoteId,
                                                Name            = $"[{l_EmoteId}]",
                                                Uri             = l_ProfileImage,
                                                StartIndex      = 0,
                                                EndIndex        = l_EmoteId.Length + 1,
                                                Animation       = Animation.AnimationType.NONE,
                                                Bits            = 0,
                                                Color           = ""
                                            }
                                        };
                                        l_SystemMessage.Message = $"{l_SystemMessage.Emotes[0].Name}  {l_SystemMsgText}";
                                    }

                                    p_ParsedMessages.Add(l_SystemMessage);
                                }
                                else
                                {
                                    /// If there's no system message, the message must be the actual message.
                                    /// In this case we wipe out the original message and skip it.
                                    l_SystemMessage = (TwitchMessage)l_NewMessage.Clone();
                                    l_SystemMessage.IsHighlighted   = true;
                                    l_SystemMessage.IsSystemMessage = true;
                                    l_NewMessage.Message            = "";

                                    p_ParsedMessages.Add(l_SystemMessage);
                                }

                                l_NewMessage.IsSystemMessage = false;

                                /// If there was no actual message, then we only need to queue up the system message
                                if (string.IsNullOrEmpty(l_NewMessage.Message))
                                    l_SkipFinalAdd = true;

                                break;

                        }
                    }

                    if (!l_SkipFinalAdd)
                        p_ParsedMessages.Add(l_NewMessage);
                }
                catch (Exception l_Exception)
                {
                    Logger.Instance.Error($"Exception while parsing Twitch message {l_MessageText}");
                    Logger.Instance.Error(l_Exception);
                }
            } while (l_Row < l_RawMessages.Length);

            Pool.MTDictionaryPool<string, string>.Release(l_Tags);

#if DEBUG
            l_Stopwatch.Stop();
            Logger.Instance.Debug($"Successfully parsed {p_ParsedMessages.Count} messages in {(decimal)l_Stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond}ms");
#endif

            return p_ParsedMessages.Count > 0;
        }


        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get and update twitch user instance
        /// </summary>
        /// <param name="p_Tags">IRC tags</param>
        /// <param name="p_Prefix">Message prefix</param>
        /// <param name="p_RoomID">Channel ID</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TwitchUser GetAndUpdateUser(Dictionary<string, string> p_Tags, string p_Prefix, string p_RoomID, string p_OverrideUserName)
        {
            var l_UserName  = p_Prefix.Contains('!') ? p_Prefix.Substring(0, p_Prefix.IndexOf('!')) : p_OverrideUserName;
            var l_HasColor  = p_Tags.TryGetValue("color", out var l_Color);
            var l_User      = m_Service.GetTwitchUser(null, l_UserName, null, l_HasColor ? l_Color : null);

            if (p_Tags.TryGetValue("display-name", out var l_Name))
                l_User.DisplayName = l_Name;

            if ((l_User.Id == null || l_User.Id.Length == 0) && p_Tags.TryGetValue("user-id", out var l_UID))
                l_User.Id = l_UID;

            if (m_TwitchDataProvider.IsReady && !l_User._FancyNameReady && !string.IsNullOrEmpty(l_User.Id))
            {
                l_User.PaintedName      = m_TwitchDataProvider._7TVDataProvider.TryGetUserDisplayName(l_User.Id, l_User.DisplayName);
                l_User._FancyNameReady  = true;
            }

            if (l_HasColor)
                l_User.Color = l_Color;

            if (m_TwitchDataProvider.IsReady && p_Tags.TryGetValue("badges", out var l_BadgeStr) && l_BadgeStr.Length != l_User._BadgesCache)
            {
                l_User.IsModerator      = l_BadgeStr.Contains("moderator/");
                l_User.IsBroadcaster    = l_BadgeStr.Contains("broadcaster/");
                l_User.IsSubscriber     = l_BadgeStr.Contains("subscriber/") || l_BadgeStr.Contains("founder/");
                l_User.IsTurbo          = l_BadgeStr.Contains("turbo/");
                l_User.IsVip            = l_BadgeStr.Contains("vip/");

                var l_Parts     = l_BadgeStr.Split(',');
                var l_Badges    = Pool.MTListPool<IChatBadge>.Get();
                var l_Failed    = false;

                for (int l_I = 0; l_I < l_Parts.Length; ++l_I)
                {
                    var l_BadgeId = l_Parts[l_I].Replace("/", "");
                    if (m_TwitchDataProvider.TryGetBadgeInfo(l_BadgeId, p_RoomID, out var l_BadgeInfo))
                    {
                        l_Badges.Add(new TwitchBadge()
                        {
                            Id      = $"{l_BadgeInfo.Type}_{l_BadgeId}",
                            Name    = l_Parts[l_I].Split('/')[0],
                            Type    = EBadgeType.Image,
                            Content     = l_BadgeInfo.Uri
                        });
                    }
                    else
                        l_Failed = true;
                }

                if (l_Badges.Count > 0)
                    l_User.Badges = l_Badges.ToArray();

                Pool.MTListPool<IChatBadge>.Release(l_Badges);

                if (!l_Failed)
                    l_User._BadgesCache = l_BadgeStr.Length;
            }

            return l_User;
        }
        /// <summary>
        /// Extract emotes
        /// </summary>
        /// <param name="p_Tags">IRC tags</param>
        /// <param name="p_MessageText">Raw message content</param>
        /// <param name="p_RoomID">Channel ID</param>
        /// <param name="p_RoomName">Channel name</param>
        /// <param name="p_MessageBits">Message bits</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IChatEmote[] GetEmotes(Dictionary<string, string> p_Tags, string p_MessageText, string p_RoomID, string p_RoomName, int p_MessageBits)
        {
            if (p_MessageText.Length == 0)
                return null;

            var l_MessageEmotes     = Pool.MTListPool<IChatEmote>.Get();
            var l_FoundTwitchEmotes = Pool.MTHashSetPool<string>.Get();

            l_MessageEmotes.Clear();
            l_FoundTwitchEmotes.Clear();

            if (BSPConfig.Instance.Twitch.ParseTwitchEmotes && p_Tags.TryGetValue("emotes", out var p_EmoteStr))
            {
                /// Parse all the normal Twitch emotes
                l_MessageEmotes = p_EmoteStr.Split('/').Aggregate(new List<IChatEmote>(), (l_EmoteList, p_EmoteInstanceString) =>
                {
                    var l_EmoteParts = p_EmoteInstanceString.Split(':');
                    foreach (var l_InstanceString in l_EmoteParts[1].Split(','))
                    {
                        var l_InstanceParts = l_InstanceString.Split('-');
                        int l_StartIndex    = int.Parse(l_InstanceParts[0]);
                        int l_EndIndex      = int.Parse(l_InstanceParts[1]);

                        if (l_StartIndex >= p_MessageText.Length)
                            Logger.Instance.Warn($"Start index is greater than message length! InstanceString: {l_InstanceString}, EmoteStr: {p_EmoteStr}, StartIndex: {l_StartIndex}, MessageLength: {p_MessageText.Length}");

                        string l_EmoteName = p_MessageText.Substring(l_StartIndex, l_EndIndex - l_StartIndex + 1);
                        l_FoundTwitchEmotes.Add(l_EmoteName);

                        l_EmoteList.Add(new TwitchEmote()
                        {
                            Id          = "TwitchEmote_" + l_EmoteParts[0],
                            Name        = l_EmoteName,
                            Uri         = "https://static-cdn.jtvnw.net/emoticons/v2/" + l_EmoteParts[0] + "/default/dark/3.0",
                            StartIndex  = l_StartIndex,
                            EndIndex    = l_EndIndex,
                            Animation   = Animation.AnimationType.MAYBE_GIF,
                            Bits        = 0,
                            Color       = ""
                        });
                    }

                    return l_EmoteList;
                });
            }

            /// Parse all the third party (BTTV, FFZ, etc) emotes
            StringBuilder l_CurrentWord = new StringBuilder();
            for (int l_I = 0; l_I <= p_MessageText.Length; l_I++)
            {
                if (l_I == p_MessageText.Length || char.IsWhiteSpace(p_MessageText[l_I]))
                {
                    if (l_CurrentWord.Length > 0)
                    {
                        var l_LastWord      = l_CurrentWord.ToString();
                        int l_StartIndex    = l_I - l_LastWord.Length;
                        int l_EndIndex      = l_I - 1;

                        if (!l_FoundTwitchEmotes.Contains(l_LastWord))
                        {
                            /// Make sure we haven't already matched a Twitch emote with the same string, just incase the user has a BTTV/FFZ emote with the same name
                            if (BSPConfig.Instance.Twitch.ParseCheermotes && p_MessageBits > 0 && m_TwitchDataProvider.TryGetCheermote(l_LastWord, p_RoomID, out var l_CheermoteData, out var l_NumBits) && l_NumBits > 0)
                            {
                                ///Logger.Instance.Error($"Got cheermote! Total message bits: {l_NumBits} {l_CheermoteData.Prefix}");
                                var l_Tier = l_CheermoteData.GetTier(l_NumBits);
                                if (l_Tier != null)
                                {
                                    l_MessageEmotes.Add(new TwitchEmote()
                                    {
                                        Id          = $"TwitchCheermote_" + l_CheermoteData.Prefix + l_Tier.MinBits,
                                        Name        = l_LastWord,
                                        Uri         = l_Tier.Uri,
                                        StartIndex  = l_StartIndex,
                                        EndIndex    = l_EndIndex,
                                        Animation   = Animation.AnimationType.GIF,
                                        Bits        = l_NumBits,
                                        Color       = l_Tier.Color
                                    });
                                }
                            }
                            else if (m_TwitchDataProvider.TryGetThirdPartyEmote(l_LastWord, p_RoomName, out var l_EmoteData))
                            {
                                if (   l_EmoteData.Type.StartsWith("BTTV") && BSPConfig.Instance.Twitch.ParseBTTVEmotes
                                    || l_EmoteData.Type.StartsWith("FFZ")  && BSPConfig.Instance.Twitch.ParseFFZEmotes
                                    || l_EmoteData.Type.StartsWith("7TV")  && BSPConfig.Instance.Twitch.Parse7TVEmotes)
                                {
                                    l_MessageEmotes.Add(new TwitchEmote()
                                    {
                                        Id          = l_EmoteData.Type + "_" +  l_LastWord,
                                        Name        = l_LastWord,
                                        Uri         = l_EmoteData.Uri,
                                        StartIndex  = l_StartIndex,
                                        EndIndex    = l_EndIndex,
                                        Animation   = l_EmoteData.Animation,
                                        Bits        = 0,
                                        Color       = ""
                                    });
                                }
                            }
                        }

                        l_CurrentWord.Clear();
                    }
                }
                else
                {
                    l_CurrentWord.Append(p_MessageText[l_I]);
                }
            }

            /// Parse all emojis
            m_EmojiParser.FindEmojis(p_MessageText, l_MessageEmotes);
            /// Sort the emotes in descending order to make replacing them in the string later on easier
            l_MessageEmotes.Sort((a, b) => b.StartIndex - a.StartIndex);

            var l_Result = l_MessageEmotes.ToArray();

            Pool.MTListPool<IChatEmote>.Release(l_MessageEmotes);
            Pool.MTHashSetPool<string>.Release(l_FoundTwitchEmotes);

            return l_Result;
        }
    }
}
