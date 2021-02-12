using BeatSaberPlusChatCore.Interfaces;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.Modules.Chat.UI
{
    /// <summary>
    /// Floating window content
    /// </summary>
    internal partial class FloatingWindow
    {
        /// <summary>
        /// On login
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        internal void OnLogin(IChatService p_Service)
        {
            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Alloc();
                l_NewMessage.Text.text          = $"<color=#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] Success connecting to <b>{p_Service.DisplayName}</b></color>";
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = Color.gray.ColorWithAlpha(0.18f);

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On join channel
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Channel">Channel service</param>
        internal void OnJoinChannel(IChatService p_Service, IChatChannel p_Channel)
        {
            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Alloc();
                l_NewMessage.Text.text          = $"<color=#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] Success joining <b>{p_Channel.Name}</b></color>";
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = Color.gray.ColorWithAlpha(0.18f);

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On join leave
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Channel">Channel service</param>
        internal void OnLeaveChannel(IChatService p_Service, IChatChannel p_Channel)
        {
            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Alloc();
                l_NewMessage.Text.text          = $"<color=#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] Success leaving <b>{p_Channel.Name}</b></color>";
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = Color.gray.ColorWithAlpha(0.18f);

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On channel follow
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        internal void OnChannelFollow(IChatService p_Service, IChatChannel p_Channel, IChatUser p_User)
        {
            if (!Config.Chat.ShowFollowEvents)
                return;

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Alloc();
                l_NewMessage.Text.text          = $"<color=#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] <b><color={p_User.Color}>@{p_User.DisplayName}</color></b> is now following <b><color={p_User.Color}>{p_Channel.Name}</color></b></color>";
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = Color.blue.ColorWithAlpha(0.24f);

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On channel bits
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_BitsUsed">Bits used</param>
        internal void OnChannelBits(IChatService p_Service, IChatChannel p_Channel, IChatUser p_User, int p_BitsUsed)
        {
            if (!Config.Chat.ShowBitsCheeringEvents)
                return;

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Alloc();
                l_NewMessage.Text.text          = $"<color=#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] <b><color={p_User.Color}>@{p_User.DisplayName}</color></b> cheered <b>{p_BitsUsed}</b> bits!</color>";
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = Color.green.ColorWithAlpha(0.24f);

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On channel points
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        internal void OnChannelPoints(IChatService p_Service, IChatChannel p_Channel, IChatUser p_User, IChatChannelPointEvent p_Event)
        {
            if (!Config.Chat.ShowChannelPointsEvent)
                return;

            if (!m_ChatFont.HasReplaceCharacter("TwitchChannelPoint_" + p_Event.Title))
            {
                TaskCompletionSource<SDK.Unity.EnhancedImage> l_TaskCompletionSource = new TaskCompletionSource<SDK.Unity.EnhancedImage>();

                SDK.Chat.ImageProvider.TryCacheSingleImage("TwitchChannelPoint_" + p_Event.Title, p_Event.Image, false, (l_Info) =>
                {
                    if (l_Info != null && !m_ChatFont.TryRegisterImageInfo(l_Info, out var l_Character))
                        Logger.Instance.Warn($"Failed to register emote \"{"TwitchChannelPoint_" + p_Event.Title}\" in font {m_ChatFont.Font.name}.");

                    l_TaskCompletionSource.SetResult(l_Info);
                });

                Task.WaitAll(new Task[] { l_TaskCompletionSource.Task }, 15000);
            }

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                var l_ImagePart = "for";

                if (m_ChatFont.TryGetReplaceCharacter("TwitchChannelPoint_" + p_Event.Title, out uint p_Character))
                    l_ImagePart = char.ConvertFromUtf32((int)p_Character);

                var l_NewMessage = m_MessagePool.Alloc();
                l_NewMessage.Text.text          = $"<color=#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] <color={p_User.Color}><b>@{p_User.DisplayName}</b></color> redeemed <color={p_User.Color}><b>{p_Event.Title}</b></color> {l_ImagePart} <color={p_User.Color}><b>{p_Event.Cost}</b></color>!</color>";
                l_NewMessage.HighlightEnabled   = true;

                if (ColorUtility.TryParseHtmlString(p_Event.BackgroundColor + "FF", out var l_Color))
                {
                    l_Color.a                   = 0.24f;
                    l_NewMessage.HighlightColor = l_Color;
                }
                else
                    l_NewMessage.HighlightColor = Color.green.ColorWithAlpha(0.24f);

                if (!string.IsNullOrEmpty(p_Event.UserInput))
                {
                    l_NewMessage.SubText.text   = p_Event.UserInput;
                    l_NewMessage.SubTextEnabled = true;
                }

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On channel subscription
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        internal void OnChannelSubsciption(IChatService p_Service, IChatChannel p_Channel, IChatUser p_User, IChatSubscriptionEvent p_Event)
        {
            if (!Config.Chat.ShowSubscriptionEvents)
                return;

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                var l_MessageStr = $"<color=#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] <color={p_User.Color}><b>@{p_User.DisplayName}</b></color> ";
                if (p_Event.IsGift)
                    l_MessageStr += $"gifted <color={p_User.Color}><b>{p_Event.PurchasedMonthCount}</b></color> month of <color={p_User.Color}><b>{p_Event.SubPlan}</b></color> to <color={p_User.Color}><b>@{p_Event.RecipientDisplayName}</b></color>!";
                else
                    l_MessageStr += $"did get a <color={p_User.Color}><b>{p_Event.PurchasedMonthCount}</b></color> month of <color={p_User.Color}><b>{p_Event.SubPlan}</b></color>!";

                var l_NewMessage = m_MessagePool.Alloc();
                l_NewMessage.Text.text          = l_MessageStr;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = Color.green.ColorWithAlpha(0.36f);

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On chat user message cleared
        /// </summary>
        /// <param name="p_UserID">ID of the user</param>
        internal void OnChatCleared(string p_UserID)
        {
            if (p_UserID == null)
                return;

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                foreach (var l_Current in m_Messages)
                {
                    if (l_Current.Text.ChatMessage == null)
                        continue;

                    if (l_Current.Text.ChatMessage.Sender.Id == p_UserID)
                        ClearMessage(l_Current);
                }
            });
        }
        /// <summary>
        /// On message cleared
        /// </summary>
        /// <param name="p_MessageID">Message ID</param>
        internal void OnMessageCleared(string p_MessageID)
        {
            if (p_MessageID == null)
                return;

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                foreach (var l_Current in m_Messages)
                {
                    if (l_Current.Text.ChatMessage == null)
                        continue;

                    if (l_Current.Text.ChatMessage.Id == p_MessageID)
                        ClearMessage(l_Current);
                }
            });
        }
        /// <summary>
        /// When a message is received
        /// </summary>
        /// <param name="p_Message">Received message</param>
        internal async void OnTextMessageReceived(IChatMessage p_Message)
        {
            /// Command filters
            if (m_FilterViewersCommands || m_FilterBroadcasterCommands)
            {
                bool l_IsBroadcaster = false;
                if (p_Message.Sender is BeatSaberPlusChatCore.Models.Twitch.TwitchUser)
                    l_IsBroadcaster = (p_Message.Sender as BeatSaberPlusChatCore.Models.Twitch.TwitchUser).IsBroadcaster;

                if (m_FilterViewersCommands && !l_IsBroadcaster && p_Message.Message.StartsWith("!"))
                    return;

                if (m_FilterBroadcasterCommands && l_IsBroadcaster && p_Message.Message.StartsWith("!"))
                    return;
            }

            string l_ParsedMessage = await Utils.ChatMessageBuilder.BuildMessage(p_Message, m_ChatFont);

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                if (m_LastMessage != null && !p_Message.IsSystemMessage && m_LastMessage.Text.ChatMessage != null && !string.IsNullOrEmpty(p_Message.Id) && m_LastMessage.Text.ChatMessage.Id == p_Message.Id)
                {
                    /// If the last message received had the same id and isn't a system message, then this was a sub-message of the original and may need to be highlighted along with the original message
                    m_LastMessage.SubText.text          = l_ParsedMessage;
                    m_LastMessage.SubText.ChatMessage   = p_Message;
                    m_LastMessage.SubTextEnabled        = true;

                    UpdateMessage(m_LastMessage);
                }
                else
                {
                    var l_NewMsg = m_MessagePool.Alloc();
                    l_NewMsg.Text.ChatMessage   = p_Message;
                    l_NewMsg.Text.text          = l_ParsedMessage;

                    AddMessage(l_NewMsg);

                    m_LastMessage = l_NewMsg;
                }
            });
        }
    }
}
