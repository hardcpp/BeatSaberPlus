using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberPlusChatCore.Interfaces;
using HMUI;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VRUIControls;

namespace BeatSaberPlus.Plugins.Chat.UI
{
    /// <summary>
    /// Floating window content
    /// </summary>
    internal class FloatingWindow : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Backup message queue, keep a track of the messages if the game reload
        /// </summary>
        private static ConcurrentQueue<IChatMessage> m_BackupMessageQueue = new ConcurrentQueue<IChatMessage>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Chat font
        /// </summary>
        private Extensions.EnhancedFontInfo m_ChatFont;
        /// <summary>
        /// Message pool
        /// </summary>
        private BeatSaberPlus.Utils.ObjectPool<Extensions.EnhancedTextMeshProUGUIWithBackground> m_MessagePool;
        /// <summary>
        /// Visible message queue
        /// </summary>
        private List<Extensions.EnhancedTextMeshProUGUIWithBackground> m_Messages = new List<Extensions.EnhancedTextMeshProUGUIWithBackground>();
        /// <summary>
        /// Should update message positions
        /// </summary>
        private bool m_UpdateMessagePositions = false;
        /// <summary>
        ///  Should update message positions waiter
        /// </summary>
        private WaitUntil m_WaitUntilMessagePositionsNeedUpdate;
        /// <summary>
        /// Frame ending waiter
        /// </summary>
        private WaitForEndOfFrame m_WaitForEndOfFrame;
        /// <summary>
        /// Last message added
        /// </summary>
        private Extensions.EnhancedTextMeshProUGUIWithBackground m_LastMessage;
        /// <summary>
        /// Is movement allowed
        /// </summary>
        private bool m__AllowMovement = false;
        /// <summary>
        /// Is movement allowed
        /// </summary>
        private bool m_AllowMovement
        {
            get => m__AllowMovement;
            set {
                m__AllowMovement = value;
                ColorUtility.TryParseHtmlString(value ? "#FFFFFFFF" : "#FFFFFF11", out var l_ColH);
                ColorUtility.TryParseHtmlString(value ? "#FFFFFF11" : "#FFFFFFFF", out var l_ColD);
                m_LockIcon.HighlightColor   = l_ColH;
                m_LockIcon.DefaultColor     = l_ColD;

                var l_FloatingScreen = transform.parent.GetComponent<FloatingScreen>();
                l_FloatingScreen.ShowHandle = value;

                if (value)
                {
                    /// Refresh VR pointer due to bug
                    var l_Pointers = Resources.FindObjectsOfTypeAll<VRPointer>();
                    var l_Pointer  = BeatSaberPlus.Utils.Game.ActiveScene == BeatSaberPlus.Utils.Game.SceneType.Playing ? l_Pointers.LastOrDefault() : l_Pointers.FirstOrDefault();

                    if (l_Pointer != null)
                    {
                        if (l_FloatingScreen.screenMover)
                            Destroy(l_FloatingScreen.screenMover);

                        l_FloatingScreen.screenMover = l_Pointer.gameObject.AddComponent<FloatingScreenMoverPointer>();
                        l_FloatingScreen.screenMover.Init(l_FloatingScreen);
                    }
                    else
                    {
                        Logger.Instance.Warn("Failed to get VRPointer!");
                    }
                }
            }
        }
        /// <summary>
        /// Is a 360 level
        /// </summary>
        private bool m_Is360Level;
        /// <summary>
        /// FlyingGameHUDRotation instance
        /// </summary>
        private GameObject m_FlyingGameHUDRotation;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private int m_ChatWidth;
        private int m_ChatHeight;
        private bool m_ReverseChatOrder;
        private float m_FontSize;
        private Color m_HighlightColor;
        private Color m_AccentColor;
        private Color m_TextColor;
        private Color m_PingColor;
        private bool m_FilterViewersCommands;
        private bool m_FilterBroadcasterCommands;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Lock icon
        /// </summary>
        [UIComponent("SettingsIcon")]
        private ClickableImage m_SettingsIcon = null;
        /// <summary>
        /// Lock icon
        /// </summary>
        [UIComponent("LockIcon")]
        private ClickableImage m_LockIcon = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On GameObject awake
        /// </summary>
        private void Awake()
        {
            CreateChatFont();

            /// Prepare waiters
            m_WaitForEndOfFrame                     = new WaitForEndOfFrame();
            m_WaitUntilMessagePositionsNeedUpdate   = new WaitUntil(() => m_UpdateMessagePositions == true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            if (p_FirstActivation)
            {
                /// Update background color
                var l_Color = GetComponentInChildren<ImageView>().color;
                l_Color.a = Config.Chat.BackgroundA;
                l_Color.r = Config.Chat.BackgroundR;
                l_Color.g = Config.Chat.BackgroundG;
                l_Color.b = Config.Chat.BackgroundB;
                GetComponentInChildren<ImageView>().color = l_Color;

                /// Update message position origin
                (transform.GetChild(0).transform as RectTransform).pivot = new Vector2(0.5f, 0f);

                /// Setup message pool
                m_MessagePool = new BeatSaberPlus.Utils.ObjectPool<Extensions.EnhancedTextMeshProUGUIWithBackground>(25,
                    p_Constructor: () =>
                    {
                        var l_GameObject = new GameObject();
                        DontDestroyOnLoad(l_GameObject);

                        var l_Message = l_GameObject.AddComponent<Extensions.EnhancedTextMeshProUGUIWithBackground>();
                        l_Message.Text.enableWordWrapping       = true;
                        l_Message.Text.FontInfo                 = m_ChatFont;
                        l_Message.Text.font                     = m_ChatFont.Font;
                        l_Message.Text.overflowMode             = TextOverflowModes.Overflow;
                        l_Message.Text.alignment                = TextAlignmentOptions.BottomLeft;
                        l_Message.Text.lineSpacing              = 1.5f;
                        l_Message.Text.color                    = m_TextColor;
                        l_Message.Text.fontSize                 = m_FontSize;

                        l_Message.SubText.enableWordWrapping    = true;
                        l_Message.SubText.FontInfo              = m_ChatFont;
                        l_Message.SubText.font                  = m_ChatFont.Font;
                        l_Message.SubText.overflowMode          = TextOverflowModes.Overflow;
                        l_Message.SubText.alignment             = TextAlignmentOptions.BottomLeft;
                        l_Message.SubText.color                 = m_TextColor;
                        l_Message.SubText.fontSize              = m_FontSize;
                        l_Message.SubText.lineSpacing           = 1.5f;

                        l_Message.RectTranform.pivot            = new Vector2(0.5f, 0);

                        l_Message.transform.SetParent(transform.GetChild(0).transform, false);
                        l_Message.transform.SetAsFirstSibling();
                        l_Message.gameObject.SetActive(false);

                        UpdateMessage(l_Message);

                        return l_Message;
                    },
                    p_OnFree: (p_Message) =>
                    {
                        try
                        {
                            p_Message.HighlightEnabled      = false;
                            p_Message.AccentEnabled         = false;
                            p_Message.SubTextEnabled        = false;
                            p_Message.Text.text             = "";
                            p_Message.Text.ChatMessage      = null;
                            p_Message.SubText.text          = "";
                            p_Message.SubText.ChatMessage   = null;

                            p_Message.OnLatePreRenderRebuildComplete -= OnMessageRenderRebuildComplete;

                            p_Message.gameObject.SetActive(false);

                            p_Message.Text.ClearImages();
                            p_Message.SubText.ClearImages();
                        }
                        catch (System.Exception p_Exception)
                        {
                            Logger.Instance.Error("An exception occurred while trying to free CustomText object");
                            Logger.Instance.Error(p_Exception);
                        }
                    }
                );

                /// Start update coroutine
                StartCoroutine(UpdateMessagePositions());

                /// Update lock state
                m_AllowMovement = false;
            }
        }
        /// <summary>
        /// On destroy
        /// </summary>
        protected override void OnDestroy()
        {
            StopCoroutine(UpdateMessagePositions());

            /// Backup messages
            foreach (var l_Current in m_Messages)
            {
                l_Current.OnLatePreRenderRebuildComplete -= OnMessageRenderRebuildComplete;

                if (l_Current.Text.ChatMessage != null)
                    m_BackupMessageQueue.Enqueue(l_Current.Text.ChatMessage);

                if (l_Current.SubText.ChatMessage != null)
                    m_BackupMessageQueue.Enqueue(l_Current.SubText.ChatMessage);

                Destroy(l_Current);
            }
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

            base.OnDestroy();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called every frames
        /// </summary>
        private void Update()
        {
            if (m_Is360Level && Config.SongChartVisualizer.FollowEnvironementRotation && m_FlyingGameHUDRotation != null && m_FlyingGameHUDRotation)
                transform.parent.parent.rotation = m_FlyingGameHUDRotation.transform.rotation;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update UI
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        /// <param name="p_OnSceneChange">Is the scene changed ?</param>
        /// <param name="p_Is360Level">Is a 360 level</param>
        /// <param name="p_FlyingGameHUDRotation">Flying hame HUD rotation</param>
        internal void UpdateUI(BeatSaberPlus.Utils.Game.SceneType p_Scene, bool p_OnSceneChange, bool p_Is360Level, GameObject p_FlyingGameHUDRotation)
        {
            /// Disable settings in play mode
            m_SettingsIcon.gameObject.SetActive(p_Scene != BeatSaberPlus.Utils.Game.SceneType.Playing);

            /// On scene change, lock movement
            if (p_OnSceneChange)
                m_AllowMovement = false;

            /// Update background color
            GetComponentInChildren<ImageView>().color = Config.Chat.BackgroundColor;

            m_ChatWidth         = Config.Chat.ChatWidth;
            m_ChatHeight        = Config.Chat.ChatHeight;
            m_ReverseChatOrder  = Config.Chat.ReverseChatOrder;
            m_FontSize          = Config.Chat.FontSize;
            m_HighlightColor    = Config.Chat.HighlightColor;
            m_AccentColor       = Config.Chat.AccentColor;
            m_TextColor         = Config.Chat.TextColor;
            m_PingColor         = Config.Chat.PingColor;

            m_FilterViewersCommands     = Config.Chat.FilterViewersCommands;
            m_FilterBroadcasterCommands = Config.Chat.FilterBroadcasterCommands;

            m_Is360Level            = p_Is360Level;
            m_FlyingGameHUDRotation = p_FlyingGameHUDRotation;

            if (!m_Is360Level)
                transform.parent.parent.rotation = Quaternion.identity;

            UpdateMessages();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIAction("settings-pressed")]
        internal void OnSettingsPressed()
        {
            Plugin.Instance.Plugins.Where(x => x is Plugins.Chat.Chat).SingleOrDefault().ShowUI();
        }
        [UIAction("lock-pressed")]
        internal void OnLockPressed()
        {
            m_AllowMovement = !m_AllowMovement;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On login
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        internal void OnLogin(IChatService p_Service)
        {
            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
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
            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
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
            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
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

            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
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

            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
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

            if (!m_ChatFont.CharacterLookupTable.ContainsKey("TwitchChannelPoint_" + p_Event.Title))
            {
                TaskCompletionSource<BeatSaberPlus.Utils.EnhancedImageInfo> l_TaskCompletionSource = new TaskCompletionSource<BeatSaberPlus.Utils.EnhancedImageInfo>();

                SharedCoroutineStarter.instance.StartCoroutine(BeatSaberPlus.Utils.ChatImageProvider.instance.TryCacheSingleImage("TwitchChannelPoint_" + p_Event.Title, p_Event.Image, false, (l_Info) => {
                    if (l_Info != null && !m_ChatFont.TryRegisterImageInfo(l_Info, out var l_Character))
                        Logger.Instance.Warn($"Failed to register emote \"{"TwitchChannelPoint_" + p_Event.Title}\" in font {m_ChatFont.Font.name}.");

                    l_TaskCompletionSource.SetResult(l_Info);
                }, p_ForcedHeight: 110));

                Task.WaitAll(new Task[] { l_TaskCompletionSource.Task }, 15000);
            }

            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
            {
                var l_ImagePart = "for";

                if (m_ChatFont.TryGetCharacter("TwitchChannelPoint_" + p_Event.Title, out uint p_Character))
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

            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
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

            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
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
        public void OnMessageCleared(string p_MessageID)
        {
            if (p_MessageID == null)
                return;

            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
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
                if (Config.ChatRequest.ModeratorPower && p_Message.Sender is BeatSaberPlusChatCore.Models.Twitch.TwitchUser)
                    l_IsBroadcaster = (p_Message.Sender as BeatSaberPlusChatCore.Models.Twitch.TwitchUser).IsBroadcaster;

                if (m_FilterViewersCommands && !l_IsBroadcaster && p_Message.Message.StartsWith("!"))
                    return;

                if (m_FilterBroadcasterCommands && l_IsBroadcaster && p_Message.Message.StartsWith("!"))
                    return;
            }

            string l_ParsedMessage = await Utils.ChatMessageBuilder.BuildMessage(p_Message, m_ChatFont);

            BeatSaberPlus.Utils.MainThreadInvoker.Invoke(() =>
            {
                if (m_LastMessage != null && !p_Message.IsSystemMessage && m_LastMessage.Text.ChatMessage != null && m_LastMessage.Text.ChatMessage.Id == p_Message.Id)
                {
                    /// If the last message received had the same id and isn't a system message, then this was a sub-message of the original and may need to be highlighted along with the original message
                    m_LastMessage.SubText.text          = l_ParsedMessage;
                    m_LastMessage.SubText.ChatMessage   = p_Message;
                    m_LastMessage.SubTextEnabled = true;

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a new message to the display
        /// </summary>
        /// <param name="p_NewMessage">Message to add</param>
        private void AddMessage(Extensions.EnhancedTextMeshProUGUIWithBackground p_NewMessage)
        {
            p_NewMessage.transform.localPosition = new Vector3(0, m_ReverseChatOrder ? m_ChatHeight : 0);

            m_Messages.Add(p_NewMessage);
            UpdateMessage(p_NewMessage);

            for (int l_I = 0; l_I < m_Messages.Count; ++l_I)
            {
                var l_Current = m_Messages[l_I];
                if ((m_ReverseChatOrder && l_Current.transform.localPosition.y < 0 - (l_Current.transform as RectTransform).sizeDelta.y)
                    || l_Current.transform.localPosition.y >= m_ChatHeight)
                {
                    m_Messages.Remove(l_Current);
                    m_MessagePool.Free(l_Current);
                    continue;
                }

                ++l_I;
            }

            p_NewMessage.OnLatePreRenderRebuildComplete += OnMessageRenderRebuildComplete;
            p_NewMessage.gameObject.SetActive(true);
        }
        /// <summary>
        /// Update all messages
        /// </summary>
        private void UpdateMessages()
        {
            foreach (var l_Current in m_Messages)
                UpdateMessage(l_Current, true);

            m_UpdateMessagePositions = true;
        }
        /// <summary>
        /// Update message
        /// </summary>
        /// <param name="p_Message">Message to update</param>
        /// <param name="p_SetAllDirty">Should flag childs dirty</param>
        private void UpdateMessage(Extensions.EnhancedTextMeshProUGUIWithBackground p_Message, bool p_SetAllDirty = false)
        {
            var l_Size = (p_Message.transform as RectTransform).sizeDelta;
            l_Size.x = m_ChatWidth;
            (p_Message.transform as RectTransform).sizeDelta = l_Size;

            p_Message.Text.color        = m_TextColor;
            p_Message.Text.fontSize     = m_FontSize;

            p_Message.SubText.color         = m_TextColor;
            p_Message.SubText.fontSize      = m_FontSize;

            if (p_Message.Text.ChatMessage != null)
            {
                p_Message.HighlightColor    = p_Message.Text.ChatMessage.IsPing ? m_PingColor : m_HighlightColor;
                p_Message.AccentColor       = m_AccentColor;
                p_Message.HighlightEnabled  = p_Message.Text.ChatMessage.IsHighlighted || p_Message.Text.ChatMessage.IsPing;
                p_Message.AccentEnabled     = !p_Message.Text.ChatMessage.IsPing && (p_Message.HighlightEnabled || p_Message.SubText.ChatMessage != null);
            }

            if (p_SetAllDirty)
            {
                p_Message.Text.SetAllDirty();

                if (p_Message.SubTextEnabled)
                    p_Message.SubText.SetAllDirty();
            }
        }
        /// <summary>
        /// Clear message
        /// </summary>
        /// <param name="p_Message">Message instance</param>
        private void ClearMessage(Extensions.EnhancedTextMeshProUGUIWithBackground p_Message)
        {
            string BuildClearedMessage(Extensions.EnhancedTextMeshProUGUI p_MessageToClear)
            {
                StringBuilder l_StringBuilder = new StringBuilder($"<color={p_MessageToClear.ChatMessage.Sender.Color}>{p_MessageToClear.ChatMessage.Sender.DisplayName}</color>");
                var l_BadgeEndIndex = p_MessageToClear.text.IndexOf("<color=");
                if (l_BadgeEndIndex != -1)
                    l_StringBuilder.Insert(0, p_MessageToClear.text.Substring(0, l_BadgeEndIndex));

                l_StringBuilder.Append(": <color=#bbbbbbbb><message deleted></color>");
                return l_StringBuilder.ToString();
            }

            /// Only clear non-system messages
            if (!p_Message.Text.ChatMessage.IsSystemMessage)
            {
                p_Message.Text.text      = BuildClearedMessage(p_Message.Text);
                p_Message.SubTextEnabled = false;
            }

            if (p_Message.SubText.ChatMessage != null && !p_Message.SubText.ChatMessage.IsSystemMessage)
                p_Message.SubText.text = BuildClearedMessage(p_Message.SubText);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update messages position coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateMessagePositions()
        {
            while (this.isInViewControllerHierarchy)
            {
                yield return m_WaitUntilMessagePositionsNeedUpdate;
                yield return m_WaitForEndOfFrame;

                float l_PositionY = m_ReverseChatOrder ? m_ChatHeight : 0;

                for (int l_I = (m_Messages.Count - 1); l_I >= 0; --l_I)
                {
                    var l_CurrentMessage = m_Messages[l_I];
                    var l_Height         = (l_CurrentMessage.transform as RectTransform).sizeDelta.y;

                    if (m_ReverseChatOrder)
                        l_PositionY -= l_Height;

                    l_CurrentMessage.transform.localPosition = new Vector3(0, l_PositionY);

                    if (!m_ReverseChatOrder)
                        l_PositionY += l_Height;
                }

                m_UpdateMessagePositions = false;
            }
        }
        /// <summary>
        /// When a message is rebuilt
        /// </summary>
        private void OnMessageRenderRebuildComplete()
        {
            m_UpdateMessagePositions = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create chat font
        /// </summary>
        private void CreateChatFont()
        {
            if (m_ChatFont != null)
                return;

            if (!FontManager.IsInitialized)
            {
                var l_Task = FontManager.AsyncLoadSystemFonts();
                if (!l_Task.IsCompleted)
                    l_Task.Wait();
            }

            TMP_FontAsset   l_Font      = null;
            string          l_FontName  = Config.Chat.SystemFontName;

            if (!FontManager.TryGetTMPFontByFamily(l_FontName, out l_Font))
            {
                Logger.Instance.Error($"Could not find font {l_FontName}! Falling back to Segoe UI");
                l_FontName = "Segoe UI";
            }

            l_Font.material.shader = BeatSaberPlus.Utils.UnityShader.TMPNoGlowFontShader;
            m_ChatFont = new Extensions.EnhancedFontInfo(l_Font);

            foreach (var l_Current in m_Messages)
            {
                l_Current.Text.SetAllDirty();

                if (l_Current.SubTextEnabled)
                    l_Current.SubText.SetAllDirty();
            }

            while (m_BackupMessageQueue.TryDequeue(out var l_Current))
                OnTextMessageReceived(l_Current);
        }
    }
}
