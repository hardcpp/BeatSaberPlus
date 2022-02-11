using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models.Twitch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BeatSaberPlus.SDK.Chat.Services.Twitch
{
    /// <summary>
    /// IRC twitch message parser
    /// </summary>
    public class TwitchMessageParser : IChatMessageParser
    {
        /// <summary>
        /// Message META splitting regex
        /// </summary>
        private readonly Regex m_TwitchMessageRegex = new Regex(@"^(?:@(?<Tags>[^\r\n ]*) +|())(?::(?<HostName>[^\r\n ]+) +|())(?<MessageType>[^\r\n ]+)(?: +(?<ChannelName>[^:\r\n ]+[^\r\n ]*(?: +[^:\r\n ]+[^\r\n ]*)*)|())?(?: +:(?<Message>[^\r\n]*)| +())?[\r\n]*$", RegexOptions.Compiled | RegexOptions.Multiline);
        /// <summary>
        /// @ Message tags splitting regex
        /// </summary>
        private readonly Regex m_TagRegex = new Regex(@"(?<Tag>[^@^;^=]+)=(?<Value>[^;\s]+)", RegexOptions.Compiled | RegexOptions.Multiline);
        /// <summary>
        /// Twitch resource data provider
        /// </summary>
        private TwitchDataProvider m_TwitchDataProvider;
        /// <summary>
        /// Emoji parser
        /// </summary>
        private IEmojiParser m_EmojiParser;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public TwitchMessageParser( TwitchDataProvider twitchDataProvider,IEmojiParser emojiParser)
        {
            m_TwitchDataProvider = twitchDataProvider;
            m_EmojiParser = emojiParser;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Takes a raw Twitch message and parses it into an IChatMessage
        /// </summary>
        /// <param name="p_RawMessage">The raw message sent from Twitch</param>
        /// <param name="p_ParsedMessages">A list of chat messages that were parsed from the rawMessage</param>
        /// <returns>True if parsedMessages.Count > 0</returns>
        public bool ParseRawMessage(string p_RawMessage, ConcurrentDictionary<string, IChatChannel> l_ChannelInfo, IChatUser p_LoggedInUser, out IChatMessage[] p_ParsedMessages)
        {
#if DEBUG
            Stopwatch l_Stopwatch = Stopwatch.StartNew();
#endif
            p_ParsedMessages = null;

            var l_Matches = m_TwitchMessageRegex.Matches(p_RawMessage);
            if (l_Matches.Count == 0)
            {
                Logger.Instance.Info($"Unhandled message: {p_RawMessage}");
#if DEBUG
                l_Stopwatch.Stop();
#endif
                return false;
            }

            ///Logger.Instance.Information($"Parsing message {rawMessage}");
            var l_Messages = new List<IChatMessage>();
            foreach (Match l_Match in l_Matches)
            {
                if (!l_Match.Groups["MessageType"].Success)
                {
                    Logger.Instance.Info($"Failed to get messageType for message {l_Match.Value}");
                    continue;
                }

                ///Logger.Instance.Information($"Message: {match.Value}");

                string l_MessageType        = l_Match.Groups["MessageType"].Value;
                string l_MessageText        = l_Match.Groups["Message"].Success ? l_Match.Groups["Message"].Value : "";
                string l_MessageChannelName = l_Match.Groups["ChannelName"].Success ? l_Match.Groups["ChannelName"].Value.Trim(new char[] { '#' }) : "";
                string l_MessageRoomId      = "";

                if (l_ChannelInfo.TryGetValue(l_MessageChannelName, out var l_Channel))
                    l_MessageRoomId = l_Channel.AsTwitchChannel().Roomstate?.RoomId;

                try
                {
                    IChatBadge[]        l_UserBadges        = new IChatBadge[0];
                    List<IChatEmote>    l_MessageEmotes     = new List<IChatEmote>();
                    TwitchRoomstate     l_MessageRoomstate  = null;
                    HashSet<string>     l_FoundTwitchEmotes = new HashSet<string>();

                    bool l_IsActionMessage = false, l_IsHighlighted = false;
                    if (l_MessageText.StartsWith("\u0001ACTION"))
                    {
                        l_MessageText = l_MessageText.Remove(l_MessageText.Length - 1, 1).Remove(0, 8);
                        l_IsActionMessage = true;
                    }

                    var l_MessageMeta = new ReadOnlyDictionary<string, string>(m_TagRegex.Matches(l_Match.Value).Cast<Match>().Aggregate(new Dictionary<string, string>(), (l_Dictionnary, l_Meta) =>
                    {
                        l_Dictionnary[l_Meta.Groups["Tag"].Value] = l_Meta.Groups["Value"].Value;
                        return l_Dictionnary;
                    }));

                    int l_MessageBits = l_MessageMeta.TryGetValue("bits", out var l_BitsString) && int.TryParse(l_BitsString, out var bitsInt) ? bitsInt : 0;
                    if (l_MessageMeta.TryGetValue("badges", out var l_BadgeStr))
                    {
                        l_UserBadges = l_BadgeStr.Split(',').Aggregate(new List<IChatBadge>(), (l_List, l_Meta) =>
                        {
                            var l_BadgeId = l_Meta.Replace("/", "");
                            if (m_TwitchDataProvider.TryGetBadgeInfo(l_BadgeId, l_MessageRoomId, out var l_BadgeInfo))
                            {
                                l_List.Add(new TwitchBadge()
                                {
                                    Id      = $"{l_BadgeInfo.Type}_{l_BadgeId}",
                                    Name    = l_Meta.Split('/')[0],
                                    Uri     = l_BadgeInfo.Uri
                                });
                            }
                            return l_List;
                        }).ToArray();
                    }

                    if (l_MessageType == "PRIVMSG" || l_MessageType == "NOTIFY" || l_MessageType == "USERNOTICE")
                    {
                        if (l_MessageText.Length > 0)
                        {
                            if (SettingsConfig.Twitch.ParseTwitchEmotes && l_MessageMeta.TryGetValue("emotes", out var p_EmoteStr))
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

                                        if (l_StartIndex >= l_MessageText.Length)
                                            Logger.Instance.Warn($"Start index is greater than message length! RawMessage: {l_Match.Value}, InstanceString: {l_InstanceString}, EmoteStr: {p_EmoteStr}, StartIndex: {l_StartIndex}, MessageLength: {l_MessageText.Length}, IsActionMessage: {l_IsActionMessage}");

                                        string l_EmoteName = l_MessageText.Substring(l_StartIndex, l_EndIndex - l_StartIndex + 1);
                                        l_FoundTwitchEmotes.Add(l_EmoteName);

                                        l_EmoteList.Add(new TwitchEmote()
                                        {
                                            Id          = $"TwitchEmote_{l_EmoteParts[0]}",
                                            Name        = l_EmoteName,///endIndex >= messageText.Length ? messageText.Substring(startIndex) : ,
                                            Uri         = $"https://static-cdn.jtvnw.net/emoticons/v2/{l_EmoteParts[0]}/default/dark/3.0",
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
                            for (int l_I = 0; l_I <= l_MessageText.Length; l_I++)
                            {
                                if (l_I == l_MessageText.Length || char.IsWhiteSpace(l_MessageText[l_I]))
                                {
                                    if (l_CurrentWord.Length > 0)
                                    {
                                        var l_LastWord      = l_CurrentWord.ToString();
                                        int l_StartIndex    = l_I - l_LastWord.Length;
                                        int l_EndIndex      = l_I - 1;

                                        if (!l_FoundTwitchEmotes.Contains(l_LastWord))
                                        {
                                            /// Make sure we haven't already matched a Twitch emote with the same string, just incase the user has a BTTV/FFZ emote with the same name
                                            if (SettingsConfig.Twitch.ParseCheermotes && l_MessageBits > 0 && m_TwitchDataProvider.TryGetCheermote(l_LastWord, l_MessageRoomId, out var l_CheermoteData, out var l_NumBits) && l_NumBits > 0)
                                            {
                                                ///Logger.Instance.Information($"Got cheermote! Total message bits: {messageBits}");
                                                var l_Tier = l_CheermoteData.GetTier(l_NumBits);
                                                if (l_Tier != null)
                                                {
                                                    l_MessageEmotes.Add(new TwitchEmote()
                                                    {
                                                        Id          = $"TwitchCheermote_{l_CheermoteData.Prefix}{l_Tier.MinBits}",
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
                                            else if (m_TwitchDataProvider.TryGetThirdPartyEmote(l_LastWord, l_MessageChannelName, out var l_EmoteData))
                                            {
                                                if (   l_EmoteData.Type.StartsWith("BTTV") && SettingsConfig.Twitch.ParseBTTVEmotes
                                                    || l_EmoteData.Type.StartsWith("FFZ")  && SettingsConfig.Twitch.ParseFFZEmotes
                                                    || l_EmoteData.Type.StartsWith("7TV")  && SettingsConfig.Twitch.Parse7TVEmotes)
                                                {
                                                    l_MessageEmotes.Add(new TwitchEmote()
                                                    {
                                                        Id          = $"{l_EmoteData.Type}_{l_LastWord}",
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
                                    l_CurrentWord.Append(l_MessageText[l_I]);
                                }
                            }

                            ///if (TwitchConfig.ParseEmojis)
                            {
                                /// Parse all emojis
                                l_MessageEmotes.AddRange(m_EmojiParser.FindEmojis(l_MessageText));
                            }

                            /// Sort the emotes in descending order to make replacing them in the string later on easier
                            l_MessageEmotes.Sort((a, b) => b.StartIndex - a.StartIndex);
                        }
                    }
                    else if (l_MessageType == "ROOMSTATE")
                    {
                        TwitchRoomstate l_OldRoomState = null;

                        if (l_Channel is TwitchChannel)
                            l_OldRoomState = l_Channel.AsTwitchChannel().Roomstate;

                        l_MessageRoomstate = new TwitchRoomstate()
                        {
                            BroadcasterLang     = l_MessageMeta.TryGetValue("broadcaster-lang", out var l_Lang)                                                     ? l_Lang                  : (l_OldRoomState?.BroadcasterLang  ?? ""),
                            RoomId              = l_MessageMeta.TryGetValue("room-id",          out var l_RoomId)                                                   ? l_RoomId                : (l_OldRoomState?.RoomId           ?? ""),
                            EmoteOnly           = l_MessageMeta.TryGetValue("emote-only",       out var l_EmoteOnly)                                                ? l_EmoteOnly == "1"      : (l_OldRoomState?.EmoteOnly        ?? false),
                            FollowersOnly       = l_MessageMeta.TryGetValue("followers-only",   out var l_FollowersOnly)                                            ? l_FollowersOnly != "-1" : (l_OldRoomState?.FollowersOnly    ?? false),
                            MinFollowTime       = l_FollowersOnly != "-1" && int.TryParse(l_FollowersOnly, out var l_MinFollowTime)                                 ? l_MinFollowTime         : (l_OldRoomState?.MinFollowTime    ?? 0),
                            R9K                 = l_MessageMeta.TryGetValue("r9k",              out var l_R9K)                                                      ? l_R9K == "1"            : (l_OldRoomState?.R9K              ?? false),
                            SlowModeInterval    = l_MessageMeta.TryGetValue("slow",             out var l_Slow) && int.TryParse(l_Slow, out var l_SlowModeInterval) ? l_SlowModeInterval      : (l_OldRoomState?.SlowModeInterval ?? 0),
                            SubscribersOnly     = l_MessageMeta.TryGetValue("subs-only",        out var l_SubsOnly)                                                 ? l_SubsOnly == "1"       : (l_OldRoomState?.SubscribersOnly  ?? false)
                        };

                        if (l_Channel is TwitchChannel l_TwitchChannel)
                            l_TwitchChannel.Roomstate = l_MessageRoomstate;
                    }

                    string l_UserID       = l_MessageMeta.TryGetValue("user-id", out var l_UID) ? l_UID : "";
                    string l_UserName     = l_Match.Groups["HostName"].Success ? l_Match.Groups["HostName"].Value.Split('!')[0] : "";
                    string l_DisplayName  = l_MessageMeta.TryGetValue("display-name", out var l_Name) ? l_Name : l_UserName;

                    var l_NewMessage = new TwitchMessage()
                    {
                        Id      = l_MessageMeta.TryGetValue("id", out var l_MessageId) ? l_MessageId : "", /// TODO: default id of some sort?
                        Sender  = new TwitchUser()
                        {
                            Id              = l_UserID,
                            UserName        = l_UserName,
                            DisplayName     = l_DisplayName,
                            PaintedName     = m_TwitchDataProvider._7TVDataProvider.TryGetUserDisplayName(l_UserID, l_DisplayName),
                            Color           = l_MessageMeta.TryGetValue("color", out var l_Color) ? l_Color : ChatUtils.GetNameColor(l_UserName),
                            IsModerator     = l_BadgeStr != null && l_BadgeStr.Contains("moderator/"),
                            IsBroadcaster   = l_BadgeStr != null && l_BadgeStr.Contains("broadcaster/"),
                            IsSubscriber    = l_BadgeStr != null && (l_BadgeStr.Contains("subscriber/") || l_BadgeStr.Contains("founder/")),
                            IsTurbo         = l_BadgeStr != null && l_BadgeStr.Contains("turbo/"),
                            IsVip           = l_BadgeStr != null && l_BadgeStr.Contains("vip/"),
                            Badges          = l_UserBadges
                        },
                        Channel = l_Channel != null ? l_Channel : new TwitchChannel()
                        {
                            Id              = l_MessageChannelName,
                            Name            = l_MessageChannelName,
                            Roomstate       = l_MessageRoomstate
                        },
                        Emotes              = l_MessageEmotes.ToArray(),
                        Message             = l_MessageText,
                        IsActionMessage     = l_IsActionMessage,
                        IsSystemMessage     = l_MessageType == "NOTICE" || l_MessageType == "USERNOTICE",
                        IsHighlighted       = l_IsHighlighted,
                        IsPing              = !string.IsNullOrEmpty(l_MessageText) && p_LoggedInUser != null && l_MessageText.Contains($"@{p_LoggedInUser.DisplayName}", StringComparison.OrdinalIgnoreCase),
                        Bits                = l_MessageBits,
                        Metadata            = l_MessageMeta,
                        Type                = l_MessageType
                    };

                    if (l_MessageMeta.TryGetValue("msg-id", out var l_MsgIdValue))
                    {
                        TwitchMessage l_SystemMessage = null;

                        ///Logger.Instance.Information($"msg-id: {msgIdValue}");
                        ///Logger.Instance.Information($"Message: {match.Value}");
                        switch(l_MsgIdValue)
                        {
                            case "skip-subs-mode-message":
                                l_SystemMessage                     = (TwitchMessage)l_NewMessage.Clone();
                                l_SystemMessage.Message             = "Redeemed Send a Message In Sub-Only Mode";
                                l_SystemMessage.IsHighlighted       = false;
                                l_SystemMessage.IsSystemMessage     = true;
                                l_SystemMessage.Emotes              = new IChatEmote[0];

                                l_Messages.Add(l_SystemMessage);
                                break;

                            case "highlighted-message":
                                l_SystemMessage                     = (TwitchMessage)l_NewMessage.Clone();
                                l_SystemMessage.Message             = "Redeemed Highlight My Message";
                                l_SystemMessage.IsHighlighted       = true;
                                l_SystemMessage.IsSystemMessage     = true;
                                l_SystemMessage.Emotes              = new IChatEmote[0];

                                l_Messages.Add(l_SystemMessage);
                                break;

                            ///case "sub":
                            ///case "resub":
                            ///case "raid":
                            default:
                                Logger.Instance.Info($"Message: {l_Match.Value}");

                                if (l_MessageMeta.TryGetValue("system-msg", out var l_SystemMsgText))
                                {
                                    l_SystemMessage                 = (TwitchMessage)l_NewMessage.Clone();
                                    l_SystemMsgText                 = l_SystemMsgText.Replace(@"\s", " ");
                                    l_SystemMessage.IsHighlighted   = true;
                                    l_SystemMessage.IsSystemMessage = true;

                                    ///Logger.Instance.Information($"Message: {match.Value}");
                                    if (l_MessageMeta.TryGetValue("msg-param-sub-plan", out var l_SubPlanName))
                                    {
                                        if (l_SubPlanName == "Prime")
                                            l_SystemMessage.Message = $"👑  {l_SystemMsgText}";
                                        else
                                            l_SystemMessage.Message = $"⭐  {l_SystemMsgText}";

                                        l_SystemMessage.Emotes = m_EmojiParser.FindEmojis(l_SystemMessage.Message).ToArray();
                                    }
                                    else if (l_MessageMeta.TryGetValue("msg-param-profileImageURL", out var l_ProfileImage) && l_MessageMeta.TryGetValue("msg-param-login", out var l_LoginUser))
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

                                    l_Messages.Add(l_SystemMessage);
                                }
                                else
                                {
                                    /// If there's no system message, the message must be the actual message.
                                    /// In this case we wipe out the original message and skip it.
                                    l_SystemMessage = (TwitchMessage)l_NewMessage.Clone();
                                    l_SystemMessage.IsHighlighted   = true;
                                    l_SystemMessage.IsSystemMessage = true;
                                    l_NewMessage.Message            = "";

                                    l_Messages.Add(l_SystemMessage);
                                }

                                l_NewMessage.IsSystemMessage = false;

                                /// If there was no actual message, then we only need to queue up the system message
                                if (string.IsNullOrEmpty(l_NewMessage.Message))
                                    continue;

                                break;

                        }
                    }

                    ///Logger.Instance.Information($"RawMsg: {rawMessage}");
                    ///foreach(var kvp in newMessage.Metadata)
                    ///{
                    ///    Logger.Instance.Information($"Tag: {kvp.Key}, Value: {kvp.Value}");
                    ///}
                    l_Messages.Add(l_NewMessage);
                }
                catch (Exception l_Exception)
                {
                    Logger.Instance.Error($"Exception while parsing Twitch message {l_MessageText}");
                    Logger.Instance.Error(l_Exception);
                }
            }

            if (l_Messages.Count > 0)
            {
#if DEBUG
                l_Stopwatch.Stop();

                Logger.Instance.Debug($"Successfully parsed {l_Messages.Count} messages in {(decimal)l_Stopwatch.ElapsedTicks/TimeSpan.TicksPerMillisecond}ms");
#endif
                p_ParsedMessages = l_Messages.ToArray();

                return true;
            }

#if DEBUG
            Logger.Instance.Info("No messages were parsed successfully.");
#endif

            return false;
        }
    }
}
