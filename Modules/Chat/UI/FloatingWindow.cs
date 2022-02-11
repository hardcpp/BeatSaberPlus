using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberPlus.SDK.Chat.Interfaces;
using HMUI;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using VRUIControls;

namespace BeatSaberPlus_Chat.UI
{
    /// <summary>
    /// Floating window content
    /// </summary>
    internal partial class FloatingWindow : BeatSaberPlus.SDK.UI.ResourceViewController<FloatingWindow>
    {
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
        private BeatSaberPlus.SDK.Misc.ObjectPool<Extensions.EnhancedTextMeshProUGUIWithBackground> m_MessagePool;
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
                    var l_Pointer  = BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing ? l_Pointers.LastOrDefault() : l_Pointers.FirstOrDefault();

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

        private Vector2 m_ChatSize;
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
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Update background color
            GetComponentInChildren<ImageView>().color = CConfig.Instance.BackgroundColor;

            /// Update message position origin
            (transform.GetChild(0).transform as RectTransform).pivot = new Vector2(0.5f, 0f);

            /// Setup message pool
            m_MessagePool = new BeatSaberPlus.SDK.Misc.ObjectPool<Extensions.EnhancedTextMeshProUGUIWithBackground>(25,
                p_Constructor: () =>
                {
                    var l_GameObject = new GameObject();
                    DontDestroyOnLoad(l_GameObject);

                    var l_Message = l_GameObject.AddComponent<Extensions.EnhancedTextMeshProUGUIWithBackground>();

                    l_Message.Text.FontInfo                 = m_ChatFont;
                    l_Message.Text.font                     = m_ChatFont.Font;
                    l_Message.Text.fontStyle                = FontStyles.Normal;
                    l_Message.Text.fontSize                 = m_FontSize;
                    l_Message.Text.fontSizeMin              = 1f;
                    l_Message.Text.fontSizeMax              = m_FontSize;
                    l_Message.Text.enableAutoSizing         = false;
                    l_Message.Text.color                    = m_TextColor;
                    l_Message.Text.lineSpacing              = 1.5f;
                    l_Message.Text.enableWordWrapping       = true;
                    l_Message.Text.overflowMode             = TextOverflowModes.Overflow;
                    l_Message.Text.alignment                = TextAlignmentOptions.TopLeft;

                    l_Message.SubText.FontInfo              = m_ChatFont;
                    l_Message.SubText.font                  = m_ChatFont.Font;
                    l_Message.SubText.fontStyle             = FontStyles.Normal;
                    l_Message.SubText.fontSize              = m_FontSize;
                    l_Message.SubText.fontSizeMin           = 1f;
                    l_Message.SubText.fontSizeMax           = m_FontSize;
                    l_Message.SubText.enableAutoSizing      = false;
                    l_Message.SubText.color                 = m_TextColor;
                    l_Message.SubText.lineSpacing           = 1.5f;
                    l_Message.SubText.enableWordWrapping    = true;
                    l_Message.SubText.overflowMode          = TextOverflowModes.Overflow;
                    l_Message.SubText.alignment             = TextAlignmentOptions.TopLeft;

                    l_Message.RectTranform.pivot            = new Vector2(0.5f, 0);

                    l_Message.transform.SetParent(transform.GetChild(0).transform, false);
                    l_Message.transform.SetAsFirstSibling();
                    l_Message.SetWidth(m_ChatSize.x);
                    l_Message.gameObject.SetActive(false);

                    BeatSaberPlus.SDK.Unity.GameObject.ChangerLayerRecursive(l_Message.gameObject, LayerMask.NameToLayer("UI"));

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

            /// Hide/show the lock icon
            m_LockIcon.gameObject.SetActive(CConfig.Instance.ShowLockIcon);

            BeatSaberPlus.SDK.Unity.GameObject.ChangerLayerRecursive(gameObject, LayerMask.NameToLayer("UI"));
        }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected override sealed void OnViewDestruction()
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
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called every frames
        /// </summary>
        private void Update()
        {
            if (m_Is360Level && CConfig.Instance.FollowEnvironementRotation && m_FlyingGameHUDRotation != null && m_FlyingGameHUDRotation)
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
        internal void UpdateUI(BeatSaberPlus.SDK.Game.Logic.SceneType p_Scene, bool p_OnSceneChange, bool p_Is360Level, GameObject p_FlyingGameHUDRotation)
        {
            /// Disable settings in play mode
            m_SettingsIcon.gameObject.SetActive(p_Scene != BeatSaberPlus.SDK.Game.Logic.SceneType.Playing);

            /// On scene change, lock movement
            if (p_OnSceneChange)
                m_AllowMovement = false;

            /// Update background color
            GetComponentInChildren<ImageView>().color = CConfig.Instance.BackgroundColor;

            m_ChatSize          = CConfig.Instance.ChatSize;
            m_ReverseChatOrder  = CConfig.Instance.ReverseChatOrder;
            m_FontSize          = CConfig.Instance.FontSize;
            m_HighlightColor    = CConfig.Instance.HighlightColor;
            m_AccentColor       = CConfig.Instance.AccentColor;
            m_TextColor         = CConfig.Instance.TextColor;
            m_PingColor         = CConfig.Instance.PingColor;

            m_FilterViewersCommands     = CConfig.Instance.FilterViewersCommands;
            m_FilterBroadcasterCommands = CConfig.Instance.FilterBroadcasterCommands;

            m_Is360Level            = p_Is360Level;
            m_FlyingGameHUDRotation = p_FlyingGameHUDRotation;

            if (!m_Is360Level)
                transform.parent.parent.rotation = Quaternion.identity;

            UpdateMessages();

            /// Hide/show the lock icon
            m_LockIcon.gameObject.SetActive(CConfig.Instance.ShowLockIcon);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIAction("settings-pressed")]
        internal void OnSettingsPressed()
        {
            var l_Items = Chat.Instance.GetSettingsUI();
            BeatSaberPlus.UI.MainViewFlowCoordinator.Instance().ChangeView(l_Items.Item1, l_Items.Item2, l_Items.Item3);
        }
        [UIAction("lock-pressed")]
        internal void OnLockPressed()
        {
            m_AllowMovement = !m_AllowMovement;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a new message to the display
        /// </summary>
        /// <param name="p_NewMessage">Message to add</param>
        private void AddMessage(Extensions.EnhancedTextMeshProUGUIWithBackground p_NewMessage)
        {
            p_NewMessage.transform.localPosition = new Vector3(0, m_ReverseChatOrder ? m_ChatSize.y : 0);

            m_Messages.Add(p_NewMessage);
            UpdateMessage(p_NewMessage);

            for (int l_I = 0; l_I < m_Messages.Count; ++l_I)
            {
                var l_Current = m_Messages[l_I];
                if ((m_ReverseChatOrder && l_Current.transform.localPosition.y < 0 - (l_Current.transform as RectTransform).sizeDelta.y)
                    || l_Current.transform.localPosition.y >= m_ChatSize.y)
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
            p_Message.SetWidth(m_ChatSize.x);

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
            while (CanBeUpdated)
            {
                yield return m_WaitUntilMessagePositionsNeedUpdate;
                yield return m_WaitForEndOfFrame;

                float l_PositionY = m_ReverseChatOrder ? m_ChatSize.y : 0;

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
            string          l_FontName  = CConfig.Instance.SystemFontName;

            if (!FontManager.TryGetTMPFontByFamily(l_FontName, out l_Font))
            {
                Logger.Instance.Error($"Could not find font {l_FontName}! Falling back to Segoe UI");
                l_FontName = "Segoe UI";

                if (!FontManager.TryGetTMPFontByFamily(l_FontName, out l_Font))
                    Logger.Instance.Error($"Could not find font {l_FontName}!");
            }

            l_Font.material.shader  = BeatSaberPlus.SDK.Unity.Shader.TMPNoGlowFontShader;
            m_ChatFont              = new Extensions.EnhancedFontInfo(BeatSaberUI.CreateFixedUIFontClone(l_Font));

            /// Clean reserved characters
            m_ChatFont.Font.characterTable.RemoveAll(x => x.glyphIndex > 0xE000 && x.glyphIndex <= 0xF8FF);
            m_ChatFont.Font.characterTable.RemoveAll(x => x.glyphIndex > 0xF0000);

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
