using CP_SDK.Chat.Interfaces;
using CP_SDK.Unity.Extensions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Chat floating panel view
    /// </summary>
    internal sealed class ChatFloatingPanelView : CP_SDK.UI.ViewController<ChatFloatingPanelView>
    {
        private GameObject  m_EnvironmentRotationRef;
        private Vector2     m_ChatSize;
        private float       m_FontSize;
        private bool        m_ReverseChatOrder;
        private bool        m_PlatformOriginColor;
        private Color       m_HighlightColor;
        private Color       m_TextColor;
        private Color       m_PingColor;
        private bool        m_FilterViewersCommands;
        private bool        m_FilterBroadcasterCommands;

        private Extensions.EnhancedFontInfo                             m_ChatFont                  = null;
        private CP_SDK.Pool.ObjectPool<Components.ChatMessageWidget>    m_MessagePool               = null;
        private List<Components.ChatMessageWidget>                      m_MessagePool_Allocated     = new List<Components.ChatMessageWidget>();
        private List<Components.ChatMessageWidget>                      m_Messages                  = new List<Components.ChatMessageWidget>();
        private bool                                                    m_UpdateMessagePositions    = false;
        private Components.ChatMessageWidget                            m_LastMessage               = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Update message position origin
            RTransform.pivot = new Vector2(0.5f, 0f);

            InitLogic();

            /// Make icons easier to click
            //m_SettingsIcon.gameObject.AddComponent<SphereCollider>().radius = 10f;
            //m_LockIcon.gameObject.AddComponent<SphereCollider>().radius = 10f;
        }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected override sealed void OnViewDestruction()
        {
            DestroyLogic();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update UI
        /// </summary>
        /// <param name="p_EnvironmentRotationRef">Flying hame HUD rotation</param>
        internal void UpdateUI(GameObject p_EnvironmentRotationRef)
        {
            m_ChatSize              = CConfig.Instance.ChatSize;
            m_FontSize              = CConfig.Instance.FontSize;
            m_ReverseChatOrder      = CConfig.Instance.ReverseChatOrder;
            m_PlatformOriginColor   = CConfig.Instance.PlatformOriginColor;
            m_HighlightColor        = CConfig.Instance.HighlightColor;
            m_TextColor             = CConfig.Instance.TextColor;
            m_PingColor             = CConfig.Instance.PingColor;

            m_FilterViewersCommands     = CConfig.Instance.FilterViewersCommands;
            m_FilterBroadcasterCommands = CConfig.Instance.FilterBroadcasterCommands;
            m_EnvironmentRotationRef    = p_EnvironmentRotationRef;

            UpdateMessagesStyleFull();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (CConfig.Instance.FollowEnvironementRotation && m_EnvironmentRotationRef != null && m_EnvironmentRotationRef)
                transform.parent.parent.rotation = m_EnvironmentRotationRef.transform.rotation;

            if (m_UpdateMessagePositions)
            {
                m_UpdateMessagePositions = false;
                float l_PositionY = m_ReverseChatOrder ? m_ChatSize.y : 0;

                for (int l_I = (m_Messages.Count - 1); l_I >= 0; --l_I)
                {
                    var l_CurrentMessage    = m_Messages[l_I];
                    var l_Height            = l_CurrentMessage.Height;

                    if (m_ReverseChatOrder)
                        l_PositionY -= l_Height;

                    l_CurrentMessage.SetPositionY(l_PositionY);

                    if (!m_ReverseChatOrder)
                        l_PositionY += l_Height;
                }

                for (int l_I = 0; l_I < m_Messages.Count;)
                {
                    var l_Current = m_Messages[l_I];
                    if ((m_ReverseChatOrder && l_Current.PositionY < -m_ChatSize.y) || l_Current.PositionY >= m_ChatSize.y)
                    {
                        m_Messages.Remove(l_Current);
                        m_MessagePool.Release(l_Current);
                        continue;
                    }

                    ++l_I;
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init logic
        /// </summary>
        private void InitLogic()
        {
            m_ChatFont = new Extensions.EnhancedFontInfo(CP_SDK.Unity.FontManager.GetChatFont());

            /// Clean reserved characters
            m_ChatFont.Font.characterTable.RemoveAll(x => x.glyphIndex > 0xE000 && x.glyphIndex <= 0xF8FF);
            m_ChatFont.Font.characterTable.RemoveAll(x => x.glyphIndex > 0xF0000);

            /// Setup message pool
            m_MessagePool = new CP_SDK.Pool.ObjectPool<Components.ChatMessageWidget>(
                createFunc: () =>
                {
                    var l_Message = null as Components.ChatMessageWidget;
                    l_Message = new GameObject().AddComponent<Components.ChatMessageWidget>();
                    l_Message.Text.FontInfo                 = m_ChatFont;
                    l_Message.Text.font                     = m_ChatFont.Font;
                    l_Message.Text.fontSize                 = m_FontSize;
                    l_Message.Text.color                    = m_TextColor;
                    l_Message.Text.text                     = ".";
                    l_Message.Text.SetAllDirty();

                    l_Message.SubText.FontInfo              = m_ChatFont;
                    l_Message.SubText.font                  = m_ChatFont.Font;
                    l_Message.SubText.fontSize              = m_FontSize;
                    l_Message.SubText.color                 = m_TextColor;
                    l_Message.SubText.text                  = ".";
                    l_Message.SubText.SetAllDirty();

                    l_Message.transform.SetParent(RTransform, false);
                    l_Message.transform.SetAsFirstSibling();
                    l_Message.SetWidth(m_ChatSize.x);

                    l_Message.transform.localScale = Vector3.zero;
                    l_Message.gameObject.ChangerLayerRecursive(CP_SDK.UI.UISystem.UILayer);

                    UpdateMessageStyle(l_Message);

                    l_Message.OnLatePreRenderRebuildComplete += OnMessageRenderRebuildComplete;

                    m_MessagePool_Allocated.Add(l_Message);

                    return l_Message;
                },
                actionOnGet: (p_Message) =>
                {
                    p_Message.EnableCallback = true;
                },
                actionOnRelease: (p_Message) =>
                {
                    try
                    {
                        p_Message.transform.localScale = Vector3.zero;

                        p_Message.HighlightEnabled      = false;
                        p_Message.AccentEnabled         = false;
                        p_Message.SubTextEnabled        = false;
                        p_Message.Text.ChatMessage      = null;
                        p_Message.SubText.ChatMessage   = null;

                        p_Message.EnableCallback = false;

                        p_Message.Text.ClearImages();
                        p_Message.SubText.ClearImages();
                    }
                    catch (System.Exception p_Exception)
                    {
                        Logger.Instance.Error("[ChatPlexMod_Chat.UI][ChatFloatingPanelView.InitLogic] An exception occurred while trying to free ChatMessageWidget object:");
                        Logger.Instance.Error(p_Exception);
                    }
                },
                actionOnDestroy: (p_Message) =>
                {
                    GameObject.Destroy(p_Message.gameObject);
                    m_MessagePool_Allocated.Remove(p_Message);
                },
                collectionCheck: false,
                defaultCapacity: 25
            );
        }
        /// <summary>
        /// Destroy logic
        /// </summary>
        private void DestroyLogic()
        {
            m_Messages.Clear();

            if (m_MessagePool != null)
            {
                m_MessagePool.Dispose();
                m_MessagePool = null;
            }

            if (m_ChatFont != null)
            {
                Destroy(m_ChatFont.Font);
                m_ChatFont = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a new message to the display
        /// </summary>
        /// <param name="p_NewMessage">Message to add</param>
        private void AddMessage(Components.ChatMessageWidget p_NewMessage)
        {
            p_NewMessage.SetPositionY(m_ReverseChatOrder ? m_ChatSize.y : 0);

            m_Messages.Add(p_NewMessage);
            UpdateMessageStyle(p_NewMessage);

            p_NewMessage.transform.localScale = Vector3.one;
        }
        /// <summary>
        /// Update all messages
        /// </summary>
        private void UpdateMessagesStyleFull()
        {
            for (int l_I = 0; l_I < m_MessagePool_Allocated.Count; ++l_I)
                UpdateMessageStyleFull(m_MessagePool_Allocated[l_I], true);

            m_UpdateMessagePositions = true;
        }
        /// <summary>
        /// Update message
        /// </summary>
        /// <param name="p_Message">Message to update</param>
        /// <param name="p_SetAllDirty">Should flag childs dirty</param>
        private void UpdateMessageStyleFull(Components.ChatMessageWidget p_Message, bool p_SetAllDirty = false)
        {
            p_Message.SetWidth(m_ChatSize.x);

            p_Message.Text.color    = m_TextColor;
            p_Message.Text.fontSize = m_FontSize;

            p_Message.SubText.color     = m_TextColor;
            p_Message.SubText.fontSize  = m_FontSize;

            UpdateMessageStyle(p_Message);

            if (p_SetAllDirty)
            {
                p_Message.Text.SetAllDirty();

                if (p_Message.SubTextEnabled)
                    p_Message.SubText.SetAllDirty();
            }
        }
        /// <summary>
        /// Update message
        /// </summary>
        /// <param name="p_Message">Message to update</param>
        /// <param name="p_SetAllDirty">Should flag childs dirty</param>
        private void UpdateMessageStyle(Components.ChatMessageWidget p_Message)
        {
            p_Message.AccentEnabled = m_PlatformOriginColor;
            p_Message.AccentColor   = p_Message.Service?.AccentColor ?? Color.gray;

            if (p_Message.Text.ChatMessage == null)
                return;

            p_Message.HighlightColor    = (p_Message.Text.ChatMessage.IsPing ? m_PingColor : m_HighlightColor);
            p_Message.HighlightEnabled  = p_Message.Text.ChatMessage.IsHighlighted || p_Message.Text.ChatMessage.IsPing;
        }
        /// <summary>
        /// Clear message
        /// </summary>
        /// <param name="p_Message">Message instance</param>
        private void ClearMessage(Components.ChatMessageWidget p_Message)
        {
            string BuildClearedMessage(Components.ChatMessageText p_MessageToClear)
            {
                var l_StringBuilder = new StringBuilder($"<color={p_MessageToClear.ChatMessage.Sender.Color}>{p_MessageToClear.ChatMessage.Sender.DisplayName}</color>");
                var l_BadgeEndIndex = p_MessageToClear.text.IndexOf("<color=");
                if (l_BadgeEndIndex != -1)
                    l_StringBuilder.Insert(0, p_MessageToClear.text.Substring(0, l_BadgeEndIndex));

                l_StringBuilder.Append(": <color=#bbbbbbbb><message deleted></color>");
                return l_StringBuilder.ToString();
            }

            /// Only clear non-system messages
            if (!p_Message.Text.ChatMessage.IsSystemMessage)
            {
                p_Message.Text.ReplaceContent(BuildClearedMessage(p_Message.Text));
                p_Message.SubTextEnabled = false;
            }

            if (p_Message.SubText.ChatMessage != null && !p_Message.SubText.ChatMessage.IsSystemMessage)
                p_Message.SubText.ReplaceContent(BuildClearedMessage(p_Message.SubText));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a message is rebuilt
        /// </summary>
        private void OnMessageRenderRebuildComplete()
            => m_UpdateMessagePositions = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On system message
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">System message</param>
        internal void OnSystemMessage(IChatService p_Service, string p_Message)
        {
            var l_MessageStr = $"<#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] {p_Message}";

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Get();
                l_NewMessage.Text.ReplaceContent(l_MessageStr);
                l_NewMessage.Service            = p_Service;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = ColorU.WithAlpha(Color.gray, 0.18f);

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On login
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        internal void OnLogin(IChatService p_Service)
        {
            var l_MessageStr = $"<#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] Success connecting to <b>{p_Service.DisplayName}";

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Get();
                l_NewMessage.Text.ReplaceContent(l_MessageStr);
                l_NewMessage.Service            = p_Service;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = ColorU.WithAlpha(Color.gray, 0.18f);

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
            var l_Prefix        = !string.IsNullOrEmpty(p_Channel.Prefix) ? $"<b><color=yellow>[{p_Channel.Prefix}]</color></b> " : string.Empty;
            var l_MessageStr    = $"<#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] Success joining {l_Prefix}<b>{p_Channel.Name}";

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Get();
                l_NewMessage.Text.ReplaceContent(l_MessageStr);
                l_NewMessage.Service            = p_Service;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = ColorU.WithAlpha(Color.gray, 0.18f);

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
            var l_Prefix        = !string.IsNullOrEmpty(p_Channel.Prefix) ? $"<b><color=yellow>[{p_Channel.Prefix}]</color></b> " : string.Empty;
            var l_MessageStr    = $"<#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] Success leaving {l_Prefix}<b>{p_Channel.Name}";

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Get();
                l_NewMessage.Text.ReplaceContent(l_MessageStr);
                l_NewMessage.Service            = p_Service;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = ColorU.WithAlpha(Color.gray, 0.18f);

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
            if (!CConfig.Instance.ShowFollowEvents)
                return;

            var l_Prefix        = !string.IsNullOrEmpty(p_Channel.Prefix) ? $"<b><color=yellow>[{p_Channel.Prefix}]</color></b> " : string.Empty;
            var l_MessageStr    = $"{l_Prefix}<#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] <b><color={p_User.Color}>@{p_User.PaintedName}</color></b> is now following <b><color={p_User.Color}>{p_Channel.Name}</color>";

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Get();
                l_NewMessage.Text.ReplaceContent(l_MessageStr);
                l_NewMessage.Service            = p_Service;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = ColorU.WithAlpha(Color.blue, 0.24f);

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
            if (!CConfig.Instance.ShowBitsCheeringEvents)
                return;

            var l_Prefix        = !string.IsNullOrEmpty(p_Channel.Prefix) ? $"<b><color=yellow>[{p_Channel.Prefix}]</color></b> " : string.Empty;
            var l_MessageStr    = $"{l_Prefix}<#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] <b><color={p_User.Color}>@{p_User.PaintedName}</color></b> cheered <b>{p_BitsUsed}</b> bits!";

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Get();
                l_NewMessage.Text.ReplaceContent(l_MessageStr);
                l_NewMessage.Service            = p_Service;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = ColorU.WithAlpha(Color.green, 0.24f);

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On channel points
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        internal void OnChannelPoints(IChatService p_Service, IChatChannel p_Channel, IChatUser p_User, IChatChannelPointEvent p_Event)
        {
            if (!CConfig.Instance.ShowChannelPointsEvent)
                return;

            if (!m_ChatFont.HasReplaceCharacter("TwitchChannelPoint_" + p_Event.Title))
            {
                TaskCompletionSource<CP_SDK.Unity.EnhancedImage> l_TaskCompletionSource = new TaskCompletionSource<CP_SDK.Unity.EnhancedImage>();

                CP_SDK.Chat.ChatImageProvider.TryCacheSingleImage(EChatResourceCategory.Badge, "TwitchChannelPoint_" + p_Event.Title, p_Event.Image, CP_SDK.Animation.EAnimationType.NONE, (l_Info) =>
                {
                    if (l_Info != null && !m_ChatFont.TryRegisterImageInfo(l_Info, out var l_Character))
                        Logger.Instance.Warning($"Failed to register emote \"{"TwitchChannelPoint_" + p_Event.Title}\" in font {m_ChatFont.Font.name}.");

                    l_TaskCompletionSource.SetResult(l_Info);
                });

                Task.WaitAll(new Task[] { l_TaskCompletionSource.Task }, 15000);
            }

            var l_ImagePart = "for";

            if (m_ChatFont.TryGetReplaceCharacter("TwitchChannelPoint_" + p_Event.Title, out uint p_Character))
                l_ImagePart = char.ConvertFromUtf32((int)p_Character);

            var l_Prefix        = !string.IsNullOrEmpty(p_Channel.Prefix) ? $"<b><color=yellow>[{p_Channel.Prefix}]</color></b> " : string.Empty;
            var l_MessageStr    = $"{l_Prefix}<#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] <color={p_User.Color}><b>@{p_User.PaintedName}</b></color> redeemed <color={p_User.Color}><b>{p_Event.Title}</b></color> {l_ImagePart} <color={p_User.Color}><b>{p_Event.Cost}</b></color>!";

            if (ColorU.TryToUnityColor(p_Event.BackgroundColor, out var l_HighlightColor))
                l_HighlightColor.a = 0.24f;
            else
                l_HighlightColor = ColorU.WithAlpha(Color.green, 0.24f);

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Get();
                l_NewMessage.Text.ReplaceContent(l_MessageStr);
                l_NewMessage.Service            = p_Service;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = l_HighlightColor;

                if (!string.IsNullOrEmpty(p_Event.UserInput))
                {
                    l_NewMessage.SubText.ReplaceContent(p_Event.UserInput);
                    l_NewMessage.SubTextEnabled = true;
                }

                AddMessage(l_NewMessage);
                m_LastMessage = l_NewMessage;
            });
        }
        /// <summary>
        /// On channel subscription
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        internal void OnChannelSubsciption(IChatService p_Service, IChatChannel p_Channel, IChatUser p_User, IChatSubscriptionEvent p_Event)
        {
            if (!CConfig.Instance.ShowSubscriptionEvents)
                return;

            var l_Prefix        = !string.IsNullOrEmpty(p_Channel.Prefix) ? $"<b><color=yellow>[{p_Channel.Prefix}]</color></b> " : string.Empty;
            var l_MessageStr    = $"{l_Prefix}<#FFFFFFBB>[<b>{p_Service.DisplayName}</b>] <color={p_User.Color}><b>@{p_User.PaintedName}</b>";
            if (p_Event.IsGift)
                l_MessageStr += $"gifted <color={p_User.Color}><b>{p_Event.PurchasedMonthCount}</b></color> month of <color={p_User.Color}><b>{p_Event.SubPlan}</b></color> to <color={p_User.Color}><b>@{p_Event.RecipientDisplayName}</b></color>!";
            else
                l_MessageStr += $"did get a <color={p_User.Color}><b>{p_Event.PurchasedMonthCount}</b></color> month of <color={p_User.Color}><b>{p_Event.SubPlan}</b></color>!";

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_NewMessage = m_MessagePool.Get();
                l_NewMessage.Text.ReplaceContent(l_MessageStr);
                l_NewMessage.Service            = p_Service;
                l_NewMessage.HighlightEnabled   = true;
                l_NewMessage.HighlightColor     = ColorU.WithAlpha(Color.green, 0.36f);

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
            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                foreach (var l_Current in m_Messages)
                {
                    if (l_Current.Text.ChatMessage == null)
                        continue;

                    if (p_UserID == null || l_Current.Text.ChatMessage.Sender.Id == p_UserID)
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

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
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
        internal async void OnTextMessageReceived(IChatService p_Service, IChatMessage p_Message)
        {
            /// Command filters
            if (m_FilterViewersCommands || m_FilterBroadcasterCommands)
            {
                bool l_IsBroadcaster = p_Message.Sender.IsBroadcaster;

                if (m_FilterViewersCommands && !l_IsBroadcaster && p_Message.Message.StartsWith("!"))
                    return;

                if (m_FilterBroadcasterCommands && l_IsBroadcaster && p_Message.Message.StartsWith("!"))
                    return;
            }

            var l_Prefix        = !string.IsNullOrEmpty(p_Message.Channel.Prefix) ? $"<b><color=yellow>[{p_Message.Channel.Prefix}]</color></b> " : string.Empty;
            var l_Message       = await Utils.ChatMessageBuilder.BuildMessage(p_Message, m_ChatFont).ConfigureAwait(false);
            var l_ParsedMessage = l_Prefix + l_Message;

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                if (m_LastMessage != null && !p_Message.IsSystemMessage && m_LastMessage.Text.ChatMessage != null && !string.IsNullOrEmpty(p_Message.Id) && m_LastMessage.Text.ChatMessage.Id == p_Message.Id)
                {
                    /// If the last message received had the same id and isn't a system message, then this was a sub-message of the original and may need to be highlighted along with the original message
                    m_LastMessage.Service               = p_Service;
                    m_LastMessage.SubText.ChatMessage   = p_Message;
                    m_LastMessage.SubText.ReplaceContent(l_ParsedMessage);
                    m_LastMessage.SubTextEnabled        = true;

                    UpdateMessageStyle(m_LastMessage);
                }
                else
                {
                    var l_NewMsg = m_MessagePool.Get();
                    l_NewMsg.Service            = p_Service;
                    l_NewMsg.Text.ChatMessage   = p_Message;
                    l_NewMsg.Text.ReplaceContent(l_ParsedMessage);

                    AddMessage(l_NewMsg);

                    m_LastMessage = l_NewMsg;
                }
            });
        }
    }
}
