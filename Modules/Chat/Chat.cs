using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberPlus.SDK.Chat.Interfaces;
using HMUI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.Chat
{
    /// <summary>
    /// Chat instance
    /// </summary>
    internal class Chat : SDK.ModuleBase<Chat>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override SDK.IModuleBaseType Type => SDK.IModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Chat";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Allow people to distract you while playing!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => true;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.Chat.Enabled; set => Config.Chat.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override SDK.IModuleBaseActivationType ActivationType => SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Root GameObject, kept alive between scenes transitions
        /// </summary>
        private GameObject m_RootGameObject = null;
        /// <summary>
        /// Settings view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Settings left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// Settings right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;
        /// <summary>
        /// Floating screen instance
        /// </summary>
        private FloatingScreen m_ChatFloatingScreen = null;
        /// <summary>
        /// View controller for floating screen
        /// </summary>
        private UI.FloatingWindow m_ChatFloatingScreenController = null;
        /// <summary>
        /// Mover handle material
        /// </summary>
        private Material m_ChatFloatingScreenHandleMaterial = null;
        /// <summary>
        /// Chat core instance
        /// </summary>
        private bool m_ChatCoreAcquired = false;
        /// <summary>
        /// Chat service action queue
        /// </summary>
        private ConcurrentQueue<Action> m_ActionQueue = new ConcurrentQueue<Action>();
        /// <summary>
        /// Is the action dequeue task running
        /// </summary>
        private bool m_ActionDequeueRun = true;
        /// <summary>
        /// Create button coroutine
        /// </summary>
        private Coroutine m_CreateButtonCoroutine = null;
        /// <summary>
        /// Moderation button
        /// </summary>
        private Button m_ModerationButton = null;
        /// <summary>
        /// Last chat users
        /// </summary>
        private SDK.Misc.RingBuffer<(IChatService, IChatUser)> m_LastChatUsers = null;
        /// <summary>
        /// View count owner
        /// </summary>
        private GameObject m_ViewerCountOwner = null;
        /// <summary>
        /// Viewer count floating screen
        /// </summary>
        private FloatingScreen m_ViewerCountFloatingScreen = null;
        /// <summary>
        /// Viewer count image
        /// </summary>
        private Image m_ViewerCountImage = null;
        /// <summary>
        /// Viewer count text
        /// </summary>
        private TextMeshProUGUI m_ViewerCountText = null;
        /// <summary>
        /// Video playback status
        /// </summary>
        private ConcurrentDictionary<string, (bool, int)> m_ChannelsVideoPlaybackStatus = new ConcurrentDictionary<string, (bool, int)>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Last chat users
        /// </summary>
        internal List<(IChatService, IChatUser)> LastChatUsers => m_LastChatUsers == null ? new List<(IChatService, IChatUser)>() : m_LastChatUsers.ToList();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Create ring buffer
            m_LastChatUsers = new SDK.Misc.RingBuffer<(IChatService, IChatUser)>(40);

            /// Clear video playback status
            m_ChannelsVideoPlaybackStatus.Clear();

            /// Bind events
            SDK.Game.Logic.OnMenuSceneLoaded += OnMenuSceneLoaded;
            SDK.Game.Logic.OnSceneChange     += OnSceneChange;

            /// If we are already in menu scene, activate
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                OnSceneChange(BeatSaberPlus.SDK.Game.Logic.ActiveScene);

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                SDK.Chat.Service.Acquire();

                /// Run all services
                SDK.Chat.Service.Multiplexer.OnLogin                    += ChatCoreMutiplixer_OnLogin;
                SDK.Chat.Service.Multiplexer.OnJoinChannel              += ChatCoreMutiplixer_OnJoinChannel;
                SDK.Chat.Service.Multiplexer.OnLeaveChannel             += ChatCoreMutiplixer_OnLeaveChannel;
                SDK.Chat.Service.Multiplexer.OnChannelFollow            += ChatCoreMutiplixer_OnChannelFollow;
                SDK.Chat.Service.Multiplexer.OnChannelBits              += ChatCoreMutiplixer_OnChannelBits;
                SDK.Chat.Service.Multiplexer.OnChannelPoints            += ChatCoreMutiplixer_OnChannelPoints;
                SDK.Chat.Service.Multiplexer.OnChannelSubscription      += ChatCoreMutiplixer_OnChannelSubscription;
                SDK.Chat.Service.Multiplexer.OnTextMessageReceived      += ChatCoreMutiplixer_OnTextMessageReceived;
                SDK.Chat.Service.Multiplexer.OnRoomStateUpdated         += ChatCoreMutiplixer_OnRoomStateUpdated;
                SDK.Chat.Service.Multiplexer.OnRoomVideoPlaybackUpdated += ChatCoreMutiplixer_OnRoomVideoPlaybackUpdated;
                SDK.Chat.Service.Multiplexer.OnChatCleared              += ChatCoreMutiplixer_OnChatCleared;
                SDK.Chat.Service.Multiplexer.OnMessageCleared           += ChatCoreMutiplixer_OnMessageCleared;

                /// Get back channels
                foreach (var l_Channel in SDK.Chat.Service.Multiplexer.Channels)
                    ChatCoreMutiplixer_OnJoinChannel(l_Channel.Item1, l_Channel.Item2);

                /// Enable dequeue system
                m_ActionDequeueRun = true;

                /// Start dequeue task
                Task.Run(ActionDequeueTask);
            }

            /// Add button
            if (m_CreateButtonCoroutine == null)
                m_CreateButtonCoroutine = SharedCoroutineStarter.instance.StartCoroutine(CreateButtonCoroutine());
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Un-init chat core
            if (m_ChatCoreAcquired)
            {
                /// Unbind services
                SDK.Chat.Service.Multiplexer.OnLogin                    -= ChatCoreMutiplixer_OnLogin;
                SDK.Chat.Service.Multiplexer.OnJoinChannel              -= ChatCoreMutiplixer_OnJoinChannel;
                SDK.Chat.Service.Multiplexer.OnLeaveChannel             -= ChatCoreMutiplixer_OnLeaveChannel;
                SDK.Chat.Service.Multiplexer.OnChannelFollow            -= ChatCoreMutiplixer_OnChannelFollow;
                SDK.Chat.Service.Multiplexer.OnChannelBits              -= ChatCoreMutiplixer_OnChannelBits;
                SDK.Chat.Service.Multiplexer.OnChannelPoints            -= ChatCoreMutiplixer_OnChannelPoints;
                SDK.Chat.Service.Multiplexer.OnChannelSubscription      -= ChatCoreMutiplixer_OnChannelSubscription;
                SDK.Chat.Service.Multiplexer.OnTextMessageReceived      -= ChatCoreMutiplixer_OnTextMessageReceived;
                SDK.Chat.Service.Multiplexer.OnRoomStateUpdated         -= ChatCoreMutiplixer_OnRoomStateUpdated;
                SDK.Chat.Service.Multiplexer.OnRoomVideoPlaybackUpdated -= ChatCoreMutiplixer_OnRoomVideoPlaybackUpdated;
                SDK.Chat.Service.Multiplexer.OnChatCleared              -= ChatCoreMutiplixer_OnChatCleared;
                SDK.Chat.Service.Multiplexer.OnMessageCleared           -= ChatCoreMutiplixer_OnMessageCleared;

                /// Stop all chat services
                SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;

                /// Stop dequeue task
                m_ActionDequeueRun = false;
            }

            /// Unbind events
            SDK.Game.Logic.OnSceneChange     -= OnSceneChange;
            SDK.Game.Logic.OnMenuSceneLoaded -= OnMenuSceneLoaded;

            /// Stop coroutine
            if (m_CreateButtonCoroutine != null)
            {
                SharedCoroutineStarter.instance.StopCoroutine(m_CreateButtonCoroutine);
                m_CreateButtonCoroutine = null;
            }

            /// Destroy moderation button
            if (m_ModerationButton != null)
            {
                GameObject.Destroy(m_ModerationButton.gameObject);
                m_ModerationButton = null;
            }

            /// Destroy
            DestroyFloatingWindow();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetSettingsUIImplementation()
        {
            /// Create view if needed
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<UI.Settings>();
            if (m_SettingsLeftView == null)
                m_SettingsLeftView = BeatSaberUI.CreateViewController<UI.SettingsLeft>();
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            return (m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the menu loaded
        /// </summary>
        private void OnMenuSceneLoaded()
        {
            if (m_ModerationButton == null || !m_ModerationButton )
            {
                /// Stop coroutine
                if (m_CreateButtonCoroutine != null)
                {
                    SharedCoroutineStarter.instance.StopCoroutine(m_CreateButtonCoroutine);
                    m_CreateButtonCoroutine = null;
                }

                /// Destroy moderation button
                if (m_ModerationButton != null)
                {
                    GameObject.Destroy(m_ModerationButton.gameObject);
                    m_ModerationButton = null;
                }

                /// Add button
                if (m_CreateButtonCoroutine == null)
                    m_CreateButtonCoroutine = SharedCoroutineStarter.instance.StartCoroutine(CreateButtonCoroutine());
            }
        }
        /// <summary>
        /// When the active scene is changed
        /// </summary>
        /// <param name="p_SceneType"></param>
        private void OnSceneChange(SDK.Game.Logic.SceneType p_SceneType)
        {
            if (p_SceneType == SDK.Game.Logic.SceneType.Menu)
                UpdateButton();

            if (m_ChatFloatingScreen == null)
                CreateFloatingWindow(p_SceneType);
            else
                UpdateFloatingWindow(p_SceneType, true);
        }
        /// <summary>
        /// When the floating window is moved
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Event">Event data</param>
        private void OnFloatingWindowMoved(object p_Sender, FloatingScreenHandleEventArgs p_Event)
        {
            /// Always parallel to the floor
            if (Config.Chat.AlignWithFloor)
                m_ChatFloatingScreen.transform.localEulerAngles = new Vector3(m_ChatFloatingScreen.transform.localEulerAngles.x, m_ChatFloatingScreen.transform.localEulerAngles.y, 0);

            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
            {
                Config.Chat.PlayingChatPositionX = m_ChatFloatingScreen.transform.localPosition.x;
                Config.Chat.PlayingChatPositionY = m_ChatFloatingScreen.transform.localPosition.y;
                Config.Chat.PlayingChatPositionZ = m_ChatFloatingScreen.transform.localPosition.z;
                Config.Chat.PlayingChatRotationX = m_ChatFloatingScreen.transform.localEulerAngles.x;
                Config.Chat.PlayingChatRotationY = m_ChatFloatingScreen.transform.localEulerAngles.y;
                Config.Chat.PlayingChatRotationZ = m_ChatFloatingScreen.transform.localEulerAngles.z;
            }
            else
            {
                Config.Chat.MenuChatPositionX = m_ChatFloatingScreen.transform.localPosition.x;
                Config.Chat.MenuChatPositionY = m_ChatFloatingScreen.transform.localPosition.y;
                Config.Chat.MenuChatPositionZ = m_ChatFloatingScreen.transform.localPosition.z;
                Config.Chat.MenuChatRotationX = m_ChatFloatingScreen.transform.localEulerAngles.x;
                Config.Chat.MenuChatRotationY = m_ChatFloatingScreen.transform.localEulerAngles.y;
                Config.Chat.MenuChatRotationZ = m_ChatFloatingScreen.transform.localEulerAngles.z;
            }

            m_ViewerCountOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
            m_ViewerCountOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create button coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateButtonCoroutine()
        {
            LevelSelectionNavigationController p_LevelSelectionNavigationController = null;

            while (true)
            {
                p_LevelSelectionNavigationController = Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().LastOrDefault();

                if (p_LevelSelectionNavigationController != null && p_LevelSelectionNavigationController.gameObject.transform.childCount >= 2)
                    break;

                yield return new WaitForSeconds(0.25f);
            }

            m_ModerationButton = SDK.UI.Button.Create(p_LevelSelectionNavigationController.transform, "Chat Moderation", () => UI.ModerationViewFlowCoordinator.Instance().Present(), null);
            m_ModerationButton.transform.localPosition      = new Vector3(32.50f, 38.50f, 2.6f);
            m_ModerationButton.transform.localScale         = new Vector3(1.0f, 0.8f, 1.0f);
            m_ModerationButton.transform.SetAsLastSibling();
            m_ModerationButton.gameObject.SetActive(true);

            UpdateButton();

            m_CreateButtonCoroutine = null;
        }
        /// <summary>
        /// Update button text
        /// </summary>
        internal void UpdateButton()
        {
            if (m_ModerationButton == null)
                return;

            m_ModerationButton.transform.localPosition = new Vector3(32.50f, 38.50f, 2.6f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On login
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        private void ChatCoreMutiplixer_OnLogin(IChatService p_ChatService)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnLogin(p_ChatService));
        }
        /// <summary>
        /// On channel join
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void ChatCoreMutiplixer_OnJoinChannel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnJoinChannel(p_ChatService, p_Channel));
        }
        /// <summary>
        /// On channel leave
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void ChatCoreMutiplixer_OnLeaveChannel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnLeaveChannel(p_ChatService, p_Channel));
        }
        /// <summary>
        /// On channel follow
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        private void ChatCoreMutiplixer_OnChannelFollow(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChannelFollow(p_ChatService, p_Channel, p_User));
        }
        /// <summary>
        /// On channel bits
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_BitsUsed">Used bits</param>
        private void ChatCoreMutiplixer_OnChannelBits(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, int p_BitsUsed)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChannelBits(p_ChatService, p_Channel, p_User, p_BitsUsed));
        }
        /// <summary>
        /// On channel points
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void ChatCoreMutiplixer_OnChannelPoints(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatChannelPointEvent p_Event)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChannelPoints(p_ChatService, p_Channel, p_User, p_Event));
        }
        /// <summary>
        /// On channel subscription
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void ChatCoreMutiplixer_OnChannelSubscription(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatSubscriptionEvent p_Event)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChannelSubsciption(p_ChatService, p_Channel, p_User, p_Event));
        }
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">ID of the message</param>
        private void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_ChatService, IChatMessage p_Message)
        {
            lock (m_LastChatUsers)
            {
                if (m_LastChatUsers.Count(x => x.Item1 == p_ChatService && x.Item2.UserName == p_Message.Sender.UserName) == 0)
                    m_LastChatUsers.Add((p_ChatService, p_Message.Sender));
            }

            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnTextMessageReceived(p_Message));
        }
        /// <summary>
        /// On room state changed
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void ChatCoreMutiplixer_OnRoomStateUpdated(IChatService p_ChatService, IChatChannel p_Channel)
        {
            if (UI.ModerationLeft.Instance != null)
                UI.ModerationLeft.Instance.UpdateRoomState();

        }
        /// <summary>
        /// On room video playback updated
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_StreamUP">Is the stream up</param>
        /// <param name="p_ViewerCount">Viewer count</param>
        private void ChatCoreMutiplixer_OnRoomVideoPlaybackUpdated(IChatService p_ChatService, IChatChannel p_Channel, bool p_StreamUP, int p_ViewerCount)
        {
            string l_Key = "[" + p_ChatService.DisplayName + "]_" + p_Channel.Name.ToLower();

            if (!m_ChannelsVideoPlaybackStatus.ContainsKey(l_Key))
                m_ChannelsVideoPlaybackStatus.TryAdd(l_Key, (p_StreamUP, p_ViewerCount));
            else
                m_ChannelsVideoPlaybackStatus[l_Key] = (p_StreamUP, p_ViewerCount);

            SDK.Unity.MainThreadInvoker.Enqueue(() => UpdateViewerCount());
        }
        /// <summary>
        /// On chat user cleared
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_UserID">ID of the user</param>
        private void ChatCoreMutiplixer_OnChatCleared(IChatService p_ChatService, string p_UserID)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChatCleared(p_UserID));
        }
        /// <summary>
        /// On message cleared
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_MessageID">ID of the message</param>
        private void ChatCoreMutiplixer_OnMessageCleared(IChatService p_ChatService, string p_MessageID)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnMessageCleared(p_MessageID));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queue or send an action
        /// </summary>
        /// <param name="p_Action">Action</param>
        private void QueueOrSendChatAction(Action p_Action)
        {
            if (m_ChatFloatingScreenController == null || !m_ChatFloatingScreenController.isActivated)
                m_ActionQueue.Enqueue(p_Action);
            else
                p_Action.Invoke();
        }
        /// <summary>
        /// Dequeue actions
        /// </summary>
        /// <returns></returns>
        private async Task ActionDequeueTask()
        {
            while (m_ActionDequeueRun)
            {
                if (m_ChatFloatingScreenController == null || !m_ChatFloatingScreenController.isActivated)
                {
                    await Task.Delay(1000);
                    continue;
                }

                if (m_ActionQueue.IsEmpty)
                {
                    await Task.Delay(1000);
                    continue;
                }

                /// Work through the queue of messages that has piled up one by one until they're all gone.
                while (m_ActionDequeueRun && m_ActionQueue.TryDequeue(out var l_Action))
                    l_Action.Invoke();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create the floating window
        /// </summary>
        /// <param name="p_SceneType">Scene type</param>
        private void CreateFloatingWindow(SDK.Game.Logic.SceneType p_SceneType)
        {
            if (m_RootGameObject != null)
                return;

            try
            {
                /// Prepare root game object
                m_RootGameObject = new GameObject("BeatSaberPlus_StreamChat");
                GameObject.DontDestroyOnLoad(m_RootGameObject);

                /// Prepare size, position, rotation
                Vector2 l_ChatSize      = new Vector2(Config.Chat.ChatWidth, Config.Chat.ChatHeight);
                Vector3 l_ChatPosition  = new Vector3(Config.Chat.MenuChatPositionX, Config.Chat.MenuChatPositionY, Config.Chat.MenuChatPositionZ);
                Vector3 l_ChatRotation  = new Vector3(Config.Chat.MenuChatRotationX, Config.Chat.MenuChatRotationY, Config.Chat.MenuChatRotationZ);

                if (p_SceneType == SDK.Game.Logic.SceneType.Playing)
                {
                    l_ChatPosition = new Vector3(Config.Chat.PlayingChatPositionX, Config.Chat.PlayingChatPositionY, Config.Chat.PlayingChatPositionZ);
                    l_ChatRotation = new Vector3(Config.Chat.PlayingChatRotationX, Config.Chat.PlayingChatRotationY, Config.Chat.PlayingChatRotationZ);
                }

                /// Create floating screen
                m_ChatFloatingScreen = FloatingScreen.CreateFloatingScreen(l_ChatSize, true, Vector3.zero, Quaternion.identity);
                m_ChatFloatingScreen.GetComponent<Canvas>().sortingOrder = 3;
                m_ChatFloatingScreen.GetComponent<CurvedCanvasSettings>().SetRadius(0);

                /// Update handle material
                m_ChatFloatingScreenHandleMaterial       = GameObject.Instantiate(SDK.Unity.Material.UINoGlowMaterial);
                m_ChatFloatingScreenHandleMaterial.color = Color.clear;
                m_ChatFloatingScreen.handle.gameObject.GetComponent<Renderer>().material = m_ChatFloatingScreenHandleMaterial;

                /// Create UI Controller
                m_ChatFloatingScreenController = BeatSaberUI.CreateViewController<UI.FloatingWindow>();
                m_ChatFloatingScreen.SetRootViewController(m_ChatFloatingScreenController, HMUI.ViewController.AnimationType.None);
                m_ChatFloatingScreenController.gameObject.SetActive(true);
                m_ChatFloatingScreen.GetComponentInChildren<Canvas>().sortingOrder = 4;

                /// Bind floating window to the root game object
                m_ChatFloatingScreen.transform.SetParent(m_RootGameObject.transform);

                /// Set position & rotation
                m_ChatFloatingScreen.transform.localPosition = l_ChatPosition;
                m_ChatFloatingScreen.transform.localRotation = Quaternion.Euler(l_ChatRotation);

                /// Bind event
                m_ChatFloatingScreen.HandleReleased += OnFloatingWindowMoved;

                /// Create viewer count owner
                m_ViewerCountOwner = new GameObject();
                m_ViewerCountOwner.transform.localPosition = l_ChatPosition;
                m_ViewerCountOwner.transform.localRotation = Quaternion.Euler(l_ChatRotation);
                m_ViewerCountOwner.transform.SetParent(m_RootGameObject.transform);

                /// Viewer count window
                m_ViewerCountFloatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(25, 8), false, Vector2.zero, Quaternion.identity, 0, false);
                m_ViewerCountFloatingScreen.transform.localPosition = new Vector3(  ((((float)Config.Chat.ChatWidth) / 2f) * 0.02f) + 0.2f, (((-(float)Config.Chat.ChatHeight) / 2f) * 0.02f) + 0.1f, 0f);
                m_ViewerCountFloatingScreen.transform.localRotation = Quaternion.identity;

                /// Horizontal layout
                var l_HorizontalTag = new BeatSaberMarkupLanguage.Tags.HorizontalLayoutTag();
                var l_Layout = l_HorizontalTag.CreateObject(m_ViewerCountFloatingScreen.transform);

                /// Viewer icon
                var l_ImageTag = new BeatSaberMarkupLanguage.Tags.ImageTag();
                var l_Image    = l_ImageTag.CreateObject(l_Layout.transform);
                l_Image.transform.localScale = Vector3.one * 0.5f;
                m_ViewerCountImage = l_Image.GetComponent<Image>();
                BeatSaberUI.SetImage(m_ViewerCountImage, "#PlayerIcon");

                /// Viewer text
                var l_TextTag = new BeatSaberMarkupLanguage.Tags.TextTag();
                var l_Text    = l_TextTag.CreateObject(l_Layout.transform);
                l_Text.GetComponent<TextMeshProUGUI>().margin   = new Vector4(-3, 3, 0, 0);
                l_Text.GetComponent<TextMeshProUGUI>().fontSize = 5;
                l_Text.GetComponent<TextMeshProUGUI>().text     = "0";

                m_ViewerCountText = l_Text.GetComponent<TextMeshProUGUI>();

                /// Bind floating window to the root game object
                m_ViewerCountFloatingScreen.transform.SetParent(m_ViewerCountOwner.transform, false);

                SDK.Unity.GameObject.ChangerLayerRecursive(m_ViewerCountFloatingScreen.gameObject, LayerMask.NameToLayer("UI"));

                UpdateViewerCount();

                UpdateFloatingWindow(p_SceneType, true);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[Chat] Failed to CreateFloatingWindow");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Destroy the floating window
        /// </summary>
        private void DestroyFloatingWindow()
        {
            if (m_RootGameObject == null)
                return;

            try
            {
                /// Dismiss controller
                m_ChatFloatingScreen.SetRootViewController(null, HMUI.ViewController.AnimationType.None);

                /// Destroy objects
                GameObject.Destroy(m_ChatFloatingScreenController);
                GameObject.Destroy(m_ChatFloatingScreen);
                GameObject.Destroy(m_ChatFloatingScreenHandleMaterial);
                GameObject.Destroy(m_ViewerCountFloatingScreen);
                GameObject.Destroy(m_RootGameObject);

                /// Reset variables
                m_ChatFloatingScreenController      = null;
                m_ChatFloatingScreen                = null;
                m_ChatFloatingScreenHandleMaterial  = null;
                m_ViewerCountFloatingScreen         = null;
                m_ViewerCountText                   = null;
                m_RootGameObject                    = null;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[Chat] Failed to DestroyFloatingPlayer");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Update floating window UI on scene change
        /// </summary>
        /// <param name="p_SceneType">New scene</param>
        /// <param name="p_OnSceneChange">Is on scene change</param>
        internal void UpdateFloatingWindow(SDK.Game.Logic.SceneType p_SceneType, bool p_OnSceneChange)
        {
            if (m_RootGameObject == null)
                return;

            try
            {
                Vector3 l_ChatPosition = new Vector3(Config.Chat.MenuChatPositionX, Config.Chat.MenuChatPositionY, Config.Chat.MenuChatPositionZ);
                Vector3 l_ChatRotation = new Vector3(Config.Chat.MenuChatRotationX, Config.Chat.MenuChatRotationY, Config.Chat.MenuChatRotationZ);

                if (p_SceneType == SDK.Game.Logic.SceneType.Playing)
                {
                    l_ChatPosition = new Vector3(Config.Chat.PlayingChatPositionX, Config.Chat.PlayingChatPositionY, Config.Chat.PlayingChatPositionZ);
                    l_ChatRotation = new Vector3(Config.Chat.PlayingChatRotationX, Config.Chat.PlayingChatRotationY, Config.Chat.PlayingChatRotationZ);
                }

                /// Prepare data for level with rotations
                var l_Is360Level            = SDK.Game.Logic.LevelData?.Data?.difficultyBeatmap?.beatmapData?.spawnRotationEventsCount > 0;
                var l_FlyingGameHUDRotation = l_Is360Level ? Resources.FindObjectsOfTypeAll<FlyingGameHUDRotation>().FirstOrDefault()?.gameObject : null as GameObject;

                /// Update chat messages display
                m_ChatFloatingScreen.ScreenSize                     = new Vector2(Config.Chat.ChatWidth, Config.Chat.ChatHeight);
                m_ChatFloatingScreen.handle.transform.localScale    = new Vector2(Config.Chat.ChatWidth, Config.Chat.ChatHeight);
                m_ChatFloatingScreen.handle.transform.localPosition = Vector3.zero;
                m_ChatFloatingScreen.handle.transform.localRotation = Quaternion.identity;
                m_ChatFloatingScreenController.UpdateUI(p_SceneType, p_OnSceneChange, l_Is360Level, l_FlyingGameHUDRotation);

                /// Update position & rotation
                m_ChatFloatingScreen.transform.localPosition = l_ChatPosition;
                m_ChatFloatingScreen.transform.localRotation = Quaternion.Euler(l_ChatRotation);

                /// Update viewer count
                m_ViewerCountFloatingScreen.transform.localPosition = new Vector3(((((float)Config.Chat.ChatWidth) / 2f) * 0.02f) + 0.2f, (((-(float)Config.Chat.ChatHeight) / 2f) * 0.02f) + 0.1f, 0f);
                m_ViewerCountOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
                m_ViewerCountOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;

                /// Update visibility
                m_ViewerCountImage.enabled  = Config.Chat.ShowViewerCount;
                m_ViewerCountText.enabled   = Config.Chat.ShowViewerCount;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[Chat] Failed to UpdateFloatingWindow");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Update viewer count
        /// </summary>
        private void UpdateViewerCount()
        {
            bool l_ShowUp = false;
            int l_SumViewers = 0;

            foreach (var l_KVP in m_ChannelsVideoPlaybackStatus)
            {
                if (!l_KVP.Value.Item1)
                    continue;

                l_ShowUp        = true;
                l_SumViewers   += l_KVP.Value.Item2;
            }

            if (l_ShowUp)
                m_ViewerCountText.text = l_SumViewers.ToString();
            else
                m_ViewerCountText.text = "<color=red>Offline";
        }
    }
}
